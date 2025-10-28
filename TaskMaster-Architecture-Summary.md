# TaskMaster Microservices Architecture - Executive Summary

**Date:** October 28, 2025  
**Architecture Type:** 4-Service Microservices  
**Deployment Target:** Fly.io + Local Docker Compose

---

## Quick Overview

TaskMaster has been designed as a **4-service microservices architecture** optimized for rapid deployment and pragmatic trade-offs suitable for a single-user MVP.

### The 4 Services

1. **Gateway Service** (Port 8080)
   - YARP reverse proxy
   - Single external entry point
   - Correlation ID injection
   - Routes all traffic

2. **UI Service** (Port 8081)
   - Razor Pages frontend
   - HttpClient with Polly resilience
   - Service orchestration
   - Error handling

3. **Task Service** (Port 8082)
   - Web API (Minimal API)
   - EF Core + SQLite
   - Task CRUD operations
   - Owns database schema

4. **Category Service** (Port 8083)
   - Web API (Minimal API)
   - Enum-based categories
   - No database dependency
   - Reference data only

---

## Key Architectural Decisions

### ✅ Pragmatic Choices for MVP

| Decision | Rationale |
|----------|-----------|
| **Shared SQLite DB with WAL Mode** | Simpler than service-to-service data calls; acceptable for single-user |
| **Manual HttpClient (No Refit)** | Reduces external dependencies; HttpClient + Polly is sufficient |
| **Correlation IDs (No OpenTelemetry)** | Adequate tracing for MVP without complexity overhead |
| **All Communication via Gateway** | Clean separation; centralized routing and correlation ID injection |
| **Simple Enum Categories** | Fast to implement; extensible to database-backed if needed later |

### 🔒 Non-Negotiable Patterns

- ✅ Each service has its own Dockerfile and can be deployed independently
- ✅ Polly retry policies (3 retries with exponential backoff) for resilience
- ✅ Health check endpoints on all services (`/health`)
- ✅ Serilog structured logging with correlation ID context
- ✅ Gateway is the **only** external entry point (ports 8081-8083 are internal)

---

## Communication Patterns

### User Views Tasks
```
Browser → Gateway → UI Service → Gateway → Task Service → SQLite → Response Chain
```

### User Creates Task
```
Browser → Gateway → UI Service → Gateway → Category Service (validate)
                                 ↓
                              Gateway → Task Service → SQLite → Response
```

### Error Handling
```
Task Service fails → Polly retries (2s, 4s, 8s) → Still fails → UI shows friendly error
```

---

## Data Strategy

### Shared SQLite Database

- **Location:** `/data/tasks.db` (Fly.io volume mounted to all services)
- **Mode:** Write-Ahead Logging (WAL) for concurrent reads
- **Ownership:** Task Service owns schema migrations
- **Access:** Only Task Service has DbContext; others call via API

### Why This Works for MVP

✅ Single-user means minimal write contention  
✅ No need for distributed transactions  
✅ Simpler than maintaining multiple databases  
✅ Can migrate to Postgres later without changing service contracts

---

## Technology Stack Summary

### Gateway Service
- YARP.ReverseProxy 2.1.0
- Serilog 8.0.0
- Custom correlation ID middleware

### UI Service
- Razor Pages (ASP.NET Core 8)
- Microsoft.Extensions.Http.Polly 8.0.0
- Custom HttpClient typed clients

### Task Service
- Minimal API (ASP.NET Core 8)
- EF Core Sqlite 8.0.0
- Swashbuckle (Swagger) 6.5.0
- AspNetCore.HealthChecks.Sqlite 8.0.1

### Category Service
- Minimal API (ASP.NET Core 8)
- No database dependencies
- Enum-based reference data

---

## Deployment Strategy

### Local Development
```bash
docker-compose up --build
# Access: http://localhost:8080
```

**Services run on:**
- Gateway: 8080 (external)
- UI: 8081 (internal)
- Task: 8082 (internal)
- Category: 8083 (internal)

### Fly.io Production
```bash
# 4 separate Fly apps with internal DNS
fly deploy # in each service directory
```

**Apps:**
- `taskmaster-gateway` (public URL)
- `taskmaster-ui.internal`
- `taskmaster-tasks.internal` (mounts volume)
- `taskmaster-categories.internal`

---

## Service Boundaries

### What Each Service Owns

| Service | Owns | Does NOT Own |
|---------|------|--------------|
| **Gateway** | Routing rules, correlation IDs | Business logic, data |
| **UI** | HTML rendering, form validation, service orchestration | Direct DB access, business rules |
| **Task** | Task entities, business logic, DB schema | UI rendering, routing |
| **Category** | Category enum, validation | Database, task logic |

---

## Resilience & Error Handling

### Polly Policies (UI Service)

**Retry Policy:**
- 3 attempts
- Exponential backoff: 2s, 4s, 8s
- Logs each retry attempt

**Circuit Breaker:**
- Opens after 5 consecutive failures
- Stays open for 30 seconds
- Prevents cascading failures

**Timeout:**
- 30-second HTTP request timeout
- Prevents hanging requests

### User Experience

When services fail:
1. Polly retries automatically (transparent to user)
2. If all retries fail: Friendly error message displayed
3. User can click "Try Again" to refresh
4. Correlation ID logged for debugging

---

## What's NOT in MVP (But Architected For)

🚫 **Skip for Now:**
- Distributed tracing (OpenTelemetry)
- Response caching
- Health check UI dashboard
- API versioning
- Authentication/Authorization
- Event-driven communication

✅ **But Architecture Supports:**
- Adding OpenTelemetry later without major refactoring
- Migrating categories to database-backed if needed
- Switching SQLite to Postgres without changing service contracts
- Adding authentication middleware to Gateway
- Implementing CQRS if read/write patterns diverge

---

## Implementation Order

### Phase 1: Foundation (Build Simplest First)
1. Category Service (no dependencies)
2. Task Service (only DB dependency)
3. Gateway Service (routes to Task + Category)
4. UI Service (depends on all via Gateway)

### Phase 2: Integration
5. Docker Compose configuration
6. Local testing with all services running
7. End-to-end workflow verification

### Phase 3: Deployment
8. Dockerfiles for each service
9. Fly.io deployment scripts
10. Volume configuration for SQLite persistence

---

## Success Metrics

✅ **Architecture is successful if:**
- All services can be built and deployed independently
- UI service never calls Task/Category directly (only via Gateway)
- Correlation IDs appear in all service logs for a single request
- SQLite database persists across container restarts
- Application works locally via `docker-compose up`
- Application deploys to Fly.io with 4 separate apps
- Service failures are gracefully handled with user-friendly errors

---

## Risk Mitigation

### Risk: SQLite Write Contention
**Mitigation:** Single-user app means minimal writes; WAL mode handles concurrent reads

### Risk: Gateway Single Point of Failure
**Mitigation:** Fly.io auto-restarts failed containers; health checks detect issues quickly

### Risk: Service Discovery Complexity
**Mitigation:** Fly.io internal DNS resolves service names automatically

### Risk: Schema Evolution Coordination
**Mitigation:** Task Service owns migrations; API contracts versioned if breaking changes needed

---

## Next Steps

1. **Review this architecture** with stakeholders
2. **Validate technical decisions** against project constraints
3. **Switch to Code mode** to begin implementation
4. **Start with Category Service** (simplest, no dependencies)
5. **Build incrementally** and test each service independently

---

## Questions Before Implementation?

- Does the shared SQLite approach align with your scalability expectations?
- Should we add any additional monitoring/observability for the MVP?
- Are the Polly retry settings (3 attempts, exponential backoff) appropriate?
- Do you want Swagger UI enabled in production or only development?

---

**Ready to implement? Switch to Code mode and start with the solution structure setup.**
