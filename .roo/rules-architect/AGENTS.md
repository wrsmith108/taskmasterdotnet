# Architect Mode - Design Decisions & Constraints

## Auto-Migration Risk in Multi-Instance Deployments
[`TaskMaster/Program.cs`](TaskMaster/Program.cs) runs `Database.Migrate()` on startup. This architectural decision means:
- Each application instance attempts migrations on boot
- Concurrent migrations across instances can cause race conditions
- Consider migration locking mechanism or separate migration job for production scale-out
- Current architecture is single-instance friendly but limits horizontal scaling

## Fly.io Volume Mount Architecture Constraint
Database persistence relies on `/data` volume mount specified in [`fly.toml`](fly.toml). This creates:
- Single-region database storage (volumes are region-specific)
- Potential single point of failure
- Limits multi-region deployment strategies
- Alternative architectures would require external database service

## Custom Docker User Requirements
Dockerfiles create a specific user with UID/GID 1001:1001. This is not arbitrary:
- Ensures consistent file permissions across environments
- Required for `/data` volume mount write access in Fly.io
- Changing this requires coordinated updates to volume permissions

## Manual Security Headers vs Standard Packages
Custom middleware implements security headers instead of using packages like NWebsec. Architectural rationale:
- Fine-grained control over header values
- Reduced dependency footprint
- Trade-off: manual maintenance and updates required
- Consider standard packages if headers need frequent updates

## Filter State Preservation Architectural Pattern
POST-Redirect-GET pattern in [`TaskMaster/Pages/Index.cshtml.cs`](TaskMaster/Pages/Index.cshtml.cs:57) preserves filter state through query parameters. This stateless approach:
- Enables bookmarkable filtered views
- Avoids server-side session storage
- Makes filter state visible in URL (transparency vs privacy trade-off)
- Supports browser back/forward navigation correctly