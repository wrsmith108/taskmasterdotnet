# Debug Mode - Deployment & Runtime Gotchas

## Database Auto-Migration Gotcha
The application runs `Database.Migrate()` automatically on startup in [`TaskMaster/Program.cs`](TaskMaster/Program.cs). This means:
- Schema changes apply immediately when the app starts
- Multiple concurrent instances could conflict during migrations
- No manual migration step is required (or possible) in deployment pipelines

## SQLite File Location Differences
- **Development**: Database is at `tasks.db` in the project root
- **Production**: Database is at `/data/tasks.db`
- If debugging production issues, check the `/data` volume mount - wrong mount path means fresh database on every restart

## Health Check Endpoint
The `/health` endpoint is implemented for Fly.io deployment monitoring. Use this for debugging deployment issues or confirming the app is responsive.

## Production Port Configuration
Application runs on port 8080 in production (configured in [`TaskMaster/appsettings.Production.json`](TaskMaster/appsettings.Production.json)), not the default development port. Check this when debugging connectivity issues.

## Volume Mount Requirement
Production database requires `/data` volume mount configured in [`fly.toml`](fly.toml). Without this mount, the database will not persist between deployments and restarts - data loss will occur.