# Ask Mode - Project Structure & Configuration

## Dual Dockerfile Pattern
The project has two Dockerfiles with different build contexts:
- **Root [`Dockerfile`](Dockerfile)**: Builds from repository root, references `TaskMaster/TaskMaster.csproj`
- **Nested [`TaskMaster/Dockerfile`](TaskMaster/Dockerfile)**: Builds from TaskMaster directory, references `TaskMaster.csproj` directly
This allows flexibility in CI/CD pipelines and local builds depending on working directory.

## Fly.io Configuration Details
[`fly.toml`](fly.toml) contains deployment-specific configuration including:
- Port 8080 internal HTTP service
- `/data` volume mount specification for database persistence
- Health check configuration pointing to `/health` endpoint
- Without the volume mount, database data is ephemeral

## Custom Roo Modes
[`.roomodes`](.roomodes) defines project-specific agent modes beyond the standard set. These modes are configured specifically for this TaskMaster project's workflow.

## Inline CSS Location
The application's styling is not in standard CSS files. Instead, 349 lines of CSS are embedded in [`TaskMaster/Pages/Shared/_Layout.cshtml`](TaskMaster/Pages/Shared/_Layout.cshtml:7-350) within a `<style>` tag. This includes all custom task management UI styling.

## Database Path Convention Reasoning
Development uses relative path (`tasks.db`) for easy local development without directory setup. Production uses absolute path (`/data/tasks.db`) to ensure database persistence via Fly.io volume mounts, preventing data loss during deployments.