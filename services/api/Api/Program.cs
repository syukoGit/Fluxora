using System.Security.Claims;
using Api.Configuration;
using Api.Data;
using Api.Infrastructure;
using Api.Models;
using Api.Services.JwtTokenValidation;
using Api.Services.Keycloak;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration Binding =====
var keycloakSettings = new KeycloakSettings();
builder.Configuration.GetSection("Keycloak").Bind(keycloakSettings);
builder.Services.AddSingleton(keycloakSettings);

// ===== PostgreSQL Database Configuration =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== ASP.NET Identity Configuration =====
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password configuration
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User configuration
    options.User.RequireUniqueEmail = true;

    // Account lockout configuration
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Sign-in configuration
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===== Authentication Configuration with Keycloak (JWT) =====
var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
var keycloakAudience = builder.Configuration["Keycloak:Audience"];
var requireHttpsMetadata = builder.Configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

// Configure JWT as default scheme
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakAuthority;
    options.RequireHttpsMetadata = requireHttpsMetadata;
    options.MetadataAddress = builder.Configuration["Keycloak:MetadataAddress"]
        ?? $"{keycloakAuthority}/.well-known/openid-configuration";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = builder.Configuration.GetValue<bool>("Jwt:ValidateIssuer"),
        ValidateAudience = builder.Configuration.GetValue<bool>("Jwt:ValidateAudience"),
        ValidateLifetime = builder.Configuration.GetValue<bool>("Jwt:ValidateLifetime"),
        ValidateIssuerSigningKey = builder.Configuration.GetValue<bool>("Jwt:ValidateIssuerSigningKey"),
        ValidIssuer = keycloakAuthority,
        ValidAudience = keycloakAudience,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var tokenValidationService = context.HttpContext.RequestServices
                .GetRequiredService<IJwtTokenValidationService>();
            await tokenValidationService.HandleTokenValidationAsync(context);
        },
        OnMessageReceived = context => Task.CompletedTask,
        OnChallenge = context => Task.CompletedTask,
    };
});

// ===== Authorization Configuration =====
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"))
    .AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Administrator"));

// ===== CORS Configuration =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// OpenAPI/Swagger
builder.Services.AddOpenApi();
// Register Swashbuckle (Swagger) generator so we can serve the Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Fluxora API", Version = "v1" });

    // Configuration for JWT authentication in Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add controllers
builder.Services.AddControllers();

// ===== Application Services Registration =====
// HttpClient for Keycloak with base configuration
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();

// JWT user validation and provisioning service
builder.Services.AddScoped<IJwtTokenValidationService, JwtTokenValidationService>();

// ===== Keycloak Claims Transformation Registration =====
builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Minimal helper from Microsoft that maps OpenAPI metadata
    app.MapOpenApi();

    // Enable Swashbuckle middleware for interactive documentation
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger"; // UI at /swagger
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fluxora API V1");
    });

    app.MapGet("/", () => Results.Redirect("/swagger", permanent: false))
        .ExcludeFromDescription();
}

// Enable CORS
app.UseCors("AllowAll");

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Disabled in development to avoid issues with tokens
// app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

// ========= Seed base roles =========
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        string[] roles = ["User", "Administrator"];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during role seeding");
    }
}

app.Run();
