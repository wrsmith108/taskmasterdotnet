# Code Mode - Project-Specific Patterns

## Filter State Preservation
Task operations (complete, delete, toggle) preserve filter state through redirect parameters in [`TaskMaster/Pages/Index.cshtml.cs`](TaskMaster/Pages/Index.cshtml.cs:57). After POST operations, users are redirected back with their original filter settings intact via query string parameters.

## UTC Time Storage with Local Display
All `DateTime` values are stored in UTC in the database. Display conversion uses `.ToLocalTime()` throughout Razor pages. No timezone library (e.g., NodaTime) is used - this is a manual pattern applied consistently across the codebase.

## Mixed Entity Framework Configuration
The project uses both Fluent API (in `AppDbContext.OnModelCreating`) and Data Annotations (on model classes) for EF configuration. This hybrid approach means configuration is split between [`TaskMaster/Data/AppDbContext.cs`](TaskMaster/Data/AppDbContext.cs) and model files in [`TaskMaster/Models/`](TaskMaster/Models/).

## Custom Security Headers Middleware
Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy) are added via custom middleware in [`TaskMaster/Program.cs`](TaskMaster/Program.cs) using `app.Use()`. This manual implementation was chosen over standard packages like NWebsec.

## Inline CSS Architecture
The application's primary styling is embedded directly in [`TaskMaster/Pages/Shared/_Layout.cshtml`](TaskMaster/Pages/Shared/_Layout.cshtml:7-350) as a 349-line `<style>` block. This is not in external CSS files - modifications require editing the layout file directly.