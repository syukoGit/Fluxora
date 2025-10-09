# Fluxora Environment Scripts

This folder contains PowerShell scripts to manage the Fluxora development environment.

## 📜 Available Scripts

### 🚀 init-dev.ps1

**Development environment initialization script**

This script automates the complete environment setup:
- Checks prerequisites (Docker, Docker Compose)
- Starts PostgreSQL, Keycloak and pgAdmin with Docker
- Waits for services to become operational
- Creates and applies database migrations
- Displays connection information

**Usage:**
```powershell
# From project root
.\scripts\init-dev.ps1
```

**When to use:**
- ✅ **First installation** of the project
- ✅ After removing Docker containers
- ✅ After machine reboot
- ⚠️ Asks for confirmation if migrations already exist

### 🛑 stop-dev.ps1

**Docker services shutdown script**

Properly stops all Docker services (PostgreSQL, Keycloak, pgAdmin).

**Basic usage:**
```powershell
# Stop services (preserves data)
.\scripts\stop-dev.ps1
```

**Stop with volume removal:**
```powershell
# Stop and remove volumes (⚠️ DATA LOSS)
.\scripts\stop-dev.ps1 -RemoveVolumes
```

**Options:**
- No option: Stops containers but preserves data
- `-RemoveVolumes`: Also removes volumes (database erased)

## 🔄 Typical Workflow

### First use
```powershell
# 1. Initialize environment
.\scripts\init-dev.ps1

# 2. Configure Keycloak (follow KEYCLOAK_SETUP.md)
# Create realm, client, roles, etc.

# 3. Start API
cd services/api/Api
dotnet run
```

### Daily use
```powershell
# Start existing services
docker-compose start

# Or use the full script
.\scripts\init-dev.ps1

# Start API
cd services/api/Api
dotnet run

# Stop at end of day
docker-compose stop
# or
.\scripts\stop-dev.ps1
```

### Complete reset
```powershell
# Delete everything and start from scratch
.\scripts\stop-dev.ps1 -RemoveVolumes
.\scripts\init-dev.ps1
```

## 🔍 Technical Details

### init-dev.ps1

**Execution steps:**

1. **Prerequisites check**
   - Checks for Docker presence
   - Checks Docker Compose availability

2. **Docker startup**
   - `docker-compose up -d` to start in background

3. **PostgreSQL check**
   - Max 10 attempts with 5 seconds interval
   - Command: `docker exec fluxora-postgres pg_isready`

4. **Keycloak check**
   - Max 12 attempts with 10 seconds interval
   - Endpoint: `http://localhost:8080/health/ready`
   - Continues even if Keycloak is not completely ready

5. **Migrations management**
   - If no migration: Creates `InitialCreate` and applies it
   - If existing migrations: Asks for confirmation before applying

6. **Information display**
   - URLs and credentials for all services
   - Next steps to follow

**Important variables:**
- `$maxRetries`: Number of attempts for PostgreSQL (10)
- `$maxRetries`: Number of attempts for Keycloak (12)
- Keycloak timeout: 5 seconds per request

### stop-dev.ps1

**Behaviors:**

- **Without parameter**: `docker-compose down`
  - Stops containers
  - Preserves volumes (data)
  - Preserves Docker networks

- **With `-RemoveVolumes`**: `docker-compose down -v`
  - Stops containers
  - Removes volumes (⚠️ data loss)
  - Removes Docker networks
  - Asks for confirmation before deletion

## 📊 Managed Docker Services

| Service | Port | Description |
|---------|------|-------------|
| PostgreSQL | 5432 | Main database |
| Keycloak | 8080 | Authentication server |
| pgAdmin | 5050 | PostgreSQL administration interface |

## ⚙️ Docker Configuration

Services are defined in `docker-compose.yml` at project root.

**Created volumes:**
- `postgres_data`: Persistent PostgreSQL data

**Networks:**
- `fluxora-network`: Bridge network for inter-service communication

## 🆘 Troubleshooting

### Docker is not installed
```
❌ Docker is not installed. Please install Docker Desktop.
```
**Solution:** Install [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### PostgreSQL doesn't start
```
❌ PostgreSQL did not start correctly
```
**Solutions:**
- Check that port 5432 is not already in use
- Check logs: `docker-compose logs postgres`
- Restart Docker Desktop

### Keycloak takes too long
```
⚠️ Keycloak is taking longer than expected, but continuing...
```
**This is normal**: Keycloak can take 2-3 minutes on first startup
**Solutions:**
- Wait a few more minutes
- Check: `docker-compose logs -f keycloak`
- Check manually: http://localhost:8080

### Migration error
```
⚠️ Error applying migrations
```
**Solutions:**
- Check that PostgreSQL is accessible
- Check connection string in `appsettings.Development.json`
- Delete `Migrations` folder and re-run
- Check logs: `docker-compose logs postgres`

### Port already in use
**Error:** `Bind for 0.0.0.0:5432 failed: port is already allocated`

**Solutions:**
- Stop local PostgreSQL instance
- Change ports in `docker-compose.yml`
- Use `docker ps` to see active containers

## 📝 Important Notes

1. **First run**: Can take 5-10 minutes (Docker image downloads)
2. **Keycloak**: Takes the longest to start (1-2 minutes)
3. **Data**: Preserved between restarts (unless `-RemoveVolumes`)
4. **Migrations**: Saved in `services/api/Api/Migrations/`

## 🔗 Useful Links

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Entity Framework Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)

## 💡 Tips

1. **Daily development**: Use `docker-compose start/stop` rather than scripts
2. **Debugging**: Check logs with `docker-compose logs -f [service]`
3. **Performance**: Allocate enough RAM to Docker (minimum 4GB recommended)
4. **Backup**: Before reset with `-RemoveVolumes`, export your data if necessary
