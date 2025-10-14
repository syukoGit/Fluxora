# Fluxora AI Coding Instructions

## Project Architecture

**Fluxora** is a full-stack application with a **microservices architecture** using:

- **Monorepo structure** managed by `pnpm` and `turbo`
- **Frontend**: Next.js 15 App Router (`apps/web/`) with shadcn/ui components
- **Backend**: .NET 9 Web API (`services/api/Api/`) with Entity Framework Core
- **Authentication**: Keycloak integration (OAuth2/JWT) - **not ASP.NET Identity**
- **Database**: PostgreSQL with Docker Compose orchestration
- **Worker Service**: .NET background service (`services/worker/Worker/`)

## Essential Development Workflow

### Environment Setup (CRITICAL - Use Scripts)

```powershell
# Initialize full development environment (first time or after reset)
.\scripts\init-dev.ps1

# Stop services (preserves data)
.\scripts\stop-dev.ps1

# Nuclear option: stop and delete all data
.\scripts\stop-dev.ps1 -RemoveVolumes
```

**Never start services manually** - always use `init-dev.ps1` which handles:

- Docker service orchestration (PostgreSQL, Keycloak, pgAdmin)
- Database migrations
- Service health checks
- Connection verification

### Development Commands

```powershell
# Frontend development
pnpm dev:web                    # Next.js at localhost:3000

# Backend development
pnpm dev:api                    # .NET API with hot reload
# or manually:
cd services/api/Api && dotnet watch

# Build API (VS Code task available)
# Use: Ctrl+Shift+P -> "Tasks: Run Task" -> "build-api"
```

## Authentication Architecture (Keycloak-Centric)

**Key Pattern**: All identity management is delegated to Keycloak - **no local user storage**.

### Backend Authentication Flow

- JWT Bearer tokens from Keycloak (`Program.cs` lines 25-65)
- Claims transformation via `KeycloakRolesClaimsTransformation`
- Token validation service: `IJwtTokenValidationService`
- Controllers proxy to Keycloak: `AuthController` → `IKeycloakService`

### Frontend Authentication Flow

- Context: `AuthContext.tsx` with React patterns
- Token management: `tokenService` with automatic refresh
- API client: `apiClient` with interceptor-like token refresh
- Route protection: App Router layout patterns

### Configuration Dependencies

- Keycloak realm: `fluxora` (localhost:8080)
- API audience: `fluxora-api`
- Client credentials in `appsettings.Development.json`

## Code Patterns & Conventions

### .NET API Patterns

- **Primary constructors** for dependency injection: `public class AuthController(IKeycloakService keycloak)`
- **Service registration** in `Program.cs` with interface abstractions
- **DTO pattern**: All API models in `DTOs/` namespace
- **Controller pattern**: Proxy pattern hiding Keycloak implementation

### Next.js Frontend Patterns

- **App Router**: Route groups `(app)` and `(auth)` for layout organization
- **Server/Client components**: Explicit `'use client'` directives
- **Context providers**: Nested `ThemeProvider` → `AuthProvider` in root layout
- **Custom hooks**: `useAuth()` for authentication state management

### Database Patterns

- **Entity Framework migrations**: Always run through `init-dev.ps1`
- **PostgreSQL**: Multi-database setup (app + keycloak databases)
- **Connection strings**: Environment-based configuration

## Integration Points

### Frontend ↔ API Communication

- Base URL: `NEXT_PUBLIC_API_BASE_URL` environment variable
- Automatic token refresh on 401 responses
- Event-driven auth state: `window.dispatchEvent(new Event('auth:unauthorized'))`

### API ↔ Keycloak Communication

- HttpClient-based service: `KeycloakService`
- Admin API calls for user management
- Token validation via OIDC discovery

### Docker Service Dependencies

- PostgreSQL → Keycloak → API (dependency chain)
- Health checks enforce startup order
- Shared network: `fluxora-network`

## Critical File Locations

- **Environment setup**: `scripts/init-dev.ps1` (PowerShell)
- **API configuration**: `services/api/Api/Program.cs`
- **Authentication logic**: `services/api/Api/Services/Keycloak/`
- **Frontend auth**: `apps/web/src/contexts/AuthContext.tsx`
- **Token management**: `apps/web/src/lib/auth/token.ts`
- **Docker orchestration**: `docker-compose.yml`

## Common Debugging Scenarios

1. **Authentication failures**: Check Keycloak realm configuration and API client secrets
2. **Database connection issues**: Verify `init-dev.ps1` completed successfully
3. **CORS errors**: Development uses `AllowAll` policy in API
4. **Token refresh failures**: Check refresh token expiry and Keycloak client configuration

## VS Code Integration

- **Tasks**: Use `build-api` task instead of manual dotnet commands
- **Debugging**: Launch configurations for API and worker services
- **Extensions**: Recommended for C# TypeScript and Docker development

When working with this codebase, always consider the Keycloak-first authentication model and use the provided development scripts for consistent environment setup.
