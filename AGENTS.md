# AGENTS.md

This file provides guidance to agents when working with code in this repository.

## Database Auto-Migration on Startup
Database migrations run automatically on application startup via `Database.Migrate()` in [`TaskMaster/Program.cs`](TaskMaster/Program.cs). This is non-standard for ASP.NET Core and poses risks in multi-instance deployments where concurrent migrations could conflict.

## SQLite Database Path Conventions
- **Development**: `tasks.db` in project root (relative path)
- **Production**: `/data/tasks.db` (absolute path)
- Path switching is environment-based in [`TaskMaster/appsettings.Production.json`](TaskMaster/appsettings.Production.json)

## Fly.io Deployment Requirements
- **Volume Mount**: Requires `/data` volume mount specified in [`fly.toml`](fly.toml) for database persistence
- **Port**: Application runs on port 8080 in production
- **Health Check**: `/health` endpoint required for deployment monitoring
- Database will be lost without proper volume configuration

## Time Handling Convention
All timestamps are stored in UTC in the database but displayed in local time using `.ToLocalTime()`. No timezone library is used - conversion is manual throughout the codebase.

## Custom Roo Modes
This project defines custom agent modes in [`.roomodes`](.roomodes) file. These are project-specific configurations for different development workflows.

## Custom Security Headers
Security headers are manually implemented via custom middleware in [`TaskMaster/Program.cs`](TaskMaster/Program.cs) rather than using standard ASP.NET packages. This approach provides fine-grained control but requires manual updates.