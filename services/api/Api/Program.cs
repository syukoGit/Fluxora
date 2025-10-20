using System.Security.Claims;
using Api.Configuration;
using Api.Data;
using Api.Infrastructure;
using Api.Services.JwtTokenValidation;
using Api.Services.Keycloak;
using Api.Mapping;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration Binding =====
var keycloakSettings = new KeycloakSettings();
builder.Configuration.GetSection("Keycloak").Bind(keycloakSettings);
builder.Services.AddSingleton(keycloakSettings);

// ===== PostgreSQL Database Configuration =====
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
                                                        builder.Configuration
                                                               .GetConnectionString("DefaultConnection")));

// Identity storage is delegated to Keycloak; no ASP.NET Identity registration

// ===== Authentication Configuration with Keycloak (JWT) =====
string? keycloakAuthority = builder.Configuration["Keycloak:Authority"];
string? keycloakAudience = builder.Configuration["Keycloak:Audience"];
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
               NameClaimType = ClaimTypes.NameIdentifier,
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
               OnMessageReceived = _ => Task.CompletedTask,
               OnChallenge = _ => Task.CompletedTask,
           };
       });

// ===== Authorization Configuration =====
builder.Services.AddAuthorization();

// ===== CORS Configuration =====
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// OpenAPI/Swagger
builder.Services.AddOpenApi();
// Register Swashbuckle (Swagger) generator so we can serve the Swagger UI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Fluxora API", Version = "v1" });

    // Configuration for JWT authentication in Swagger
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

// Add controllers and configure JSON options to serialize enums as strings
builder.Services.AddControllers()
       .AddNewtonsoftJson()
       .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

// ===== Application Services Registration =====
// HttpClient for Keycloak with base configuration
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();

// JWT user validation and provisioning service
builder.Services.AddScoped<IJwtTokenValidationService, JwtTokenValidationService>();

// ===== Keycloak Claims Transformation Registration =====
builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

// ===== AutoMapper Registration =====
builder.Services.AddAutoMapper(typeof(MappingProfile));

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

    app.MapGet("/", () => Results.Redirect("/swagger", permanent: false)).ExcludeFromDescription();
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

// No local role seeding; roles are managed in Keycloak

app.Run();
