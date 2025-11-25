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

# Fluxora AI Coding Instructions

## Project Architecture

**Fluxora** is a full-stack application with a **microservices architecture** using:

- **Monorepo structure** managed by `pnpm` workspaces and `turbo` build system
- **Frontend**: Next.js 15 App Router (`apps/web/`) with shadcn/ui components
- **Backend**: .NET 9 Web API (`services/api/Api/`) with Entity Framework Core
- **Authentication**: Keycloak integration (OAuth2/JWT) - **not ASP.NET Identity**
- **Database**: PostgreSQL with Docker Compose orchestration
- **Worker Service**: .NET background service (`services/worker/Worker/`)

### Monorepo Structure

- `apps/`: Client applications (web, mobile, desktop)
- `services/`: Backend services (.NET APIs and workers)
- `infra/`: Infrastructure configuration (Docker, K8s)
- `scripts/`: PowerShell automation scripts

## Essential Development Workflow

### Environment Setup (CRITICAL - Always Use Scripts)

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
- Database migrations (via EF Core)
- Service health checks with retry logic
- Connection verification

### Development Commands

```powershell
# Frontend development
pnpm dev:web                    # Next.js at localhost:3000
pnpm build                      # Build all apps with Turbo

# Backend development
pnpm dev:api                    # .NET API with hot reload (dotnet watch)
pnpm dev:worker                 # Worker service with hot reload

# Alternative: Manual API start
cd services/api/Api && dotnet watch

# Build API (VS Code task available)
# Use: Ctrl+Shift+P -> "Tasks: Run Task" -> "build-api"
```

### Service URLs

- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000 (Swagger at /swagger)
- **Keycloak**: http://localhost:8080 (admin/admin)
- **pgAdmin**: http://localhost:5050 (admin@fluxora.dev/admin)
- **PostgreSQL**: localhost:5432 (postgres/postgres)

## Authentication Architecture (Keycloak-Centric)

**Key Pattern**: All identity management is delegated to Keycloak - **no local user storage in application database**.

### Backend Authentication Flow

- **JWT validation**: Bearer tokens from Keycloak validated via OIDC discovery (`Program.cs` lines 25-75)
- **Claims transformation**: `KeycloakRolesClaimsTransformation` extracts `realm_access.roles` from JWT
- **Token validation service**: `IJwtTokenValidationService` handles custom validation logic
- **Controller pattern**: `AuthController` proxies to `IKeycloakService` (no direct Keycloak coupling in controllers)
- **No ASP.NET Identity**: User table (`UserAccountLink`) only stores Keycloak reference + app metadata

### Frontend Authentication Flow

- **State management**: `AuthContext.tsx` provides React context with user state
- **Token persistence**: `tokenService` manages access/refresh tokens in localStorage
- **Automatic refresh**: `apiClient.fetch()` intercepts 401s and refreshes tokens transparently
- **Middleware protection**: `middleware.ts` blocks unauthenticated access at the Edge runtime level
- **Route configuration**: Role-based access control defined in `route-config.ts`

### Authentication Configuration

```typescript
// Frontend (.env.local required)
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000

// Backend (appsettings.Development.json)
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/fluxora",
    "Audience": "fluxora-api",
    "AuthClientId": "fluxora-api",
    "AuthClientSecret": "YOUR_CLIENT_SECRET"
  }
}
```

**Environment variable pattern**: Frontend uses `NEXT_PUBLIC_*` for client-side access; API uses `appsettings.{Environment}.json`.

### Token Refresh Pattern

```typescript
// Frontend implements retry-once pattern
if (response.status === 401 && !isRetry) {
  await apiClient.refreshAccessToken();
  return this.fetch(endpoint, { ...options, isRetry: true });
}
```

Backend sets `Token-Expired: true` header on JWT expiration for client-side handling.

## Code Patterns & Conventions

### .NET API Patterns

- **Primary constructors** for dependency injection (C# 12):
  ```csharp
  public class AuthController(IKeycloakService keycloak) : ControllerBase
  ```
- **Service registration**: Scoped services in `Program.cs` (lines 131-138) - follow existing pattern
- **DTO namespace convention**: All API models in `DTOs/Auth/`, `DTOs/Users/`, etc.
- **Controller responsibility**: Controllers delegate to services, never call Keycloak directly
- **Configuration binding**: Use strongly-typed settings classes (`KeycloakSettings`)
- **No Entity Framework navigation properties**: Avoid lazy loading; explicit includes only

### Next.js Frontend Patterns

- **Route groups**: `(app)` for authenticated layouts, `(auth)` for public login pages
- **Server/Client split**: Use `'use client'` only when necessary (hooks, events, state)
- **Context nesting order**: `ThemeProvider` → `AuthProvider` in root `layout.tsx`
- **Custom hooks pattern**: `useAuth()` wraps `AuthContext`, throws if used outside provider
- **Middleware edge runtime**: Token validation uses edge-compatible JWT library (`jose`)
- **Client-side routing**: Use `<Link>` from `next/link`, not `<a>` tags

### Database Migration Pattern

**Never run EF migrations manually** - they're executed by `init-dev.ps1` automatically:

```powershell
# Script handles this internally
dotnet ef database update --project services/api/Api
```

When creating new migrations:

```powershell
cd services/api/Api
dotnet ef migrations add MigrationName
# Commit migration files, then run init-dev.ps1 to apply
```

## Integration Points

### Frontend ↔ API Communication

- **Base URL**: Must set `NEXT_PUBLIC_API_BASE_URL` in `.env.local` (not committed)
- **API client pattern**: All requests through `apiClient.fetch()` for automatic token handling
- **Token refresh flow**: Client detects 401 → refreshes token → retries once → clears auth on second failure
- **Auth state sync**: `window.dispatchEvent(new Event('auth:unauthorized'))` triggers global logout
- **Error boundary**: Frontend shows user-friendly errors, logs details to console in dev mode

### API ↔ Keycloak Communication

- **Service pattern**: `KeycloakService` implements `IKeycloakService` using `HttpClient`
- **Admin operations**: User creation/management via Keycloak Admin API (port 8080)
- **Token endpoint**: `/realms/fluxora/protocol/openid-connect/token` for login/refresh
- **OIDC discovery**: `.well-known/openid-configuration` auto-configures JWT validation
- **Client credentials**: Separate clients for API (`fluxora-api`) and admin (`fluxora-user-manager`)

### Docker Service Dependencies

**Startup order** (enforced via health checks):

```
PostgreSQL (healthy) → Keycloak (healthy) → API (manual start)
```

- **Network**: All services on `fluxora-network` bridge
- **Database init**: `init-postgres.sh` creates both `fluxora_dev` and `keycloak` databases
- **Volume persistence**: PostgreSQL data survives `docker-compose stop`, removed with `-RemoveVolumes`
- **Health check retry**: `init-dev.ps1` polls Keycloak for 90 seconds before failing

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
