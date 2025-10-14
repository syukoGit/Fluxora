# Fluxora

Full-stack application with .NET API and Next.js frontend.

## Quick Start

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS version)
- [pnpm](https://pnpm.io/) package manager

### Development

1. **Install dependencies:**
   ```bash
   pnpm install
   ```

2. **Start Docker services (PostgreSQL & Keycloak):**
   ```bash
   docker compose up -d
   ```

3. **Run both API and Web app:**
   ```bash
   pnpm run dev
   ```

   This will start:
   - API server with hot reload (dotnet watch)
   - Next.js web app on http://localhost:3000

### Individual Services

You can also run services individually:

- **Web app only:** `pnpm run dev:web`
- **API only:** `pnpm run dev:api`
- **Worker only:** `pnpm run dev:worker`

### Initial Setup

For first-time setup, run the initialization script:

```powershell
# Windows PowerShell
.\scripts\init-dev.ps1
```

See [scripts/README.md](./scripts/README.md) for more details.

## Project Structure

- `apps/web` - Next.js frontend application
- `services/api` - .NET Web API
- `scripts/` - Development scripts

## License

MIT License - see [LICENSE](./LICENSE) for details.
