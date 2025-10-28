# TaskMaster Microservices - NuGet Package Research

**Version:** 1.0  
**Date:** October 28, 2025  
**Architecture:** Microservices with REST APIs, YARP Gateway, Shared SQLite  
**Target:** .NET 8 LTS, Single-User MVP, Fly.io Deployment

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Microservices Communication Packages](#2-microservices-communication-packages)
3. [API Gateway Packages (YARP)](#3-api-gateway-packages-yarp)
4. [Razor Pages Frontend Packages](#4-razor-pages-frontend-packages)
5. [Data Persistence Packages](#5-data-persistence-packages)
6. [Health Checks & Monitoring](#6-health-checks--monitoring)
7. [Logging & Diagnostics](#7-logging--diagnostics)
8. [Containerization & Docker](#8-containerization--docker)
9. [Resilience Patterns](#9-resilience-patterns)
10. [Configuration Management](#10-configuration-management)
11. [Fly.io Deployment Considerations](#11-flyio-deployment-considerations)
12. [Complete Package Matrix](#12-complete-package-matrix)
13. [Pending Decisions](#13-pending-decisions)
14. [Trade-offs & Concerns](#14-trade-offs--concerns)

---

## 1. Architecture Overview

### 1.1 Proposed Microservices Structure

```
┌─────────────────────────────────────────────────────────────┐
│                         Browser                              │
└──────────────────┬──────────────────────────────────────────┘
                   │ HTTP
                   ▼
┌─────────────────────────────────────────────────────────────┐
│              YARP API Gateway                                │
│         (Reverse Proxy + Routing)                           │
└──────┬──────────────────┬──────────────────┬───────────────┘
       │                  │                  │
       │ REST             │ REST             │ REST
       ▼                  ▼                  ▼
┌─────────────┐   ┌─────────────┐   ┌─────────────┐
│  UI Service │   │ Task Service│   │ Category    │
│ (Razor Pages)   │ (Web API)   │   │ Service     │
└──────┬──────┘   └──────┬──────┘   └──────┬──────┘
       │                  │                  │
       │                  │                  │
       └──────────────────┴──────────────────┘
                          │
                          ▼
                 ┌────────────────┐
                 │ Shared SQLite  │
                 │   (tasks.db)   │
                 └────────────────┘
```

### 1.2 Service Responsibilities

| Service | Responsibility | Technology |
|---------|---------------|------------|
| **UI Service** | Server-side rendering, user interaction | Razor Pages + HttpClient |
| **Task Service** | Task CRUD operations, business logic | ASP.NET Core Web API |
| **Category Service** | Category management (enum/reference data) | ASP.NET Core Web API |
| **API Gateway** | Request routing, aggregation | YARP (Yet Another Reverse Proxy) |

---

## 2. Microservices Communication Packages

### 2.1 Core HTTP Communication

#### **System.Net.Http.Json (Built-in with .NET 8)**
- **Version:** Included in .NET 8 SDK
- **Purpose:** JSON serialization/deserialization for HTTP requests
- **Usage:** `HttpClient.GetFromJsonAsync<T>()`, `PostAsJsonAsync<T>()`
- **Justification:** Modern, efficient, built-in alternative to manual JSON handling

**Configuration:**
```csharp
// In Program.cs of UI Service
builder.Services.AddHttpClient("TaskService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TaskService:Url"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

#### **Microsoft.Extensions.Http (Built-in with ASP.NET Core)**
- **Version:** Included in Microsoft.AspNetCore.App
- **Purpose:** HttpClientFactory for managed HttpClient instances
- **Justification:** Prevents socket exhaustion, manages lifecycle, supports named/typed clients
- **Features:** Automatic connection pooling, DNS refresh, request/response logging

### 2.2 Typed HTTP Client (Optional Enhancement)

#### **Refit (7.1.2)** - OPTIONAL
- **Version:** 7.1.2
- **Purpose:** Type-safe REST API client generation
- **NuGet:** `Refit` + `Refit.HttpClientFactory`
- **Justification:** Reduces boilerplate, compile-time safety, automatic serialization

**Example:**
```csharp
// Define interface
public interface ITaskServiceClient
{
    [Get("/api/tasks")]
    Task<List<TaskDto>> GetTasksAsync();
    
    [Post("/api/tasks")]
    Task<TaskDto> CreateTaskAsync([Body] CreateTaskRequest request);
}

// Register in Program.cs
builder.Services.AddRefitClient<ITaskServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://task-service:8080"));
```

**⚠️ DECISION REQUIRED:** Refit adds external dependency. Alternative: Manual HttpClient calls.

### 2.3 Service-to-Service Communication Best Practices

**Packages Already Available:**
- `System.Text.Json` (built-in .NET 8) - High performance JSON serialization
- `Microsoft.Extensions.Http.Polly` (see Resilience section) - Retry/circuit breaker

---

## 3. API Gateway Packages (YARP)

### 3.1 YARP (Yet Another Reverse Proxy)

#### **Yarp.ReverseProxy (2.1.0)**
- **Version:** 2.1.0
- **Purpose:** Microsoft-maintained reverse proxy for .NET
- **NuGet:** `Yarp.ReverseProxy`
- **Justification:** 
  - Official Microsoft project
  - High performance (built on ASP.NET Core)
  - Dynamic configuration
  - Simple for single-user scenarios
  - No external dependencies

**Installation:**
```bash
dotnet add package Yarp.ReverseProxy --version 2.1.0
```

**Program.cs Configuration:**
```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
```

**appsettings.json Configuration:**
```json
{
  "ReverseProxy": {
    "Routes": {
      "task-route": {
        "ClusterId": "task-cluster",
        "Match": {
          "Path": "/api/tasks/{**catch-all}"
        }
      },
      "category-route": {
        "ClusterId": "category-cluster",
        "Match": {
          "Path": "/api/categories/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "task-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://task-service:8080"
          }
        }
      },
      "category-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://category-service:8081"
          }
        }
      }
    }
  }
}
```

### 3.2 Alternative to YARP (Not Recommended)

#### **Ocelot (23.2.0)** - NOT RECOMMENDED
- **Reason:** More complex, additional configuration, not officially Microsoft-maintained
- **YARP is preferred** for simplicity and Microsoft support

---

## 4. Razor Pages Frontend Packages

### 4.1 Required Packages

#### **Microsoft.AspNetCore.App (Metapackage - Implicit)**
- **Version:** Included in .NET 8 SDK
- **Includes:**
  - Razor Pages framework
  - Tag Helpers
  - Model binding and validation
  - Antiforgery tokens
  - Session/Cookie support

**No additional packages needed** for basic Razor Pages functionality.

### 4.2 HTTP Client for Service Communication

#### **Configuration in UI Service:**
```csharp
// Program.cs
builder.Services.AddHttpClient<ITaskServiceClient, TaskServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TaskService:Url"]);
});

builder.Services.AddRazorPages();
```

#### **Typed Client Pattern (Recommended):**
```csharp
public class TaskServiceClient : ITaskServiceClient
{
    private readonly HttpClient _httpClient;
    
    public TaskServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<TaskDto>> GetTasksAsync(string filter)
    {
        return await _httpClient.GetFromJsonAsync<List<TaskDto>>($"/api/tasks?filter={filter}");
    }
}
```

### 4.3 Response Caching (Optional)

#### **Microsoft.AspNetCore.ResponseCaching (Built-in)**
- **Purpose:** Cache API responses to reduce service calls
- **Configuration:**
```csharp
builder.Services.AddResponseCaching();
app.UseResponseCaching();
```

---

## 5. Data Persistence Packages

### 5.1 Shared SQLite Database Approach

**Challenge:** Multiple services accessing same SQLite file requires careful configuration.

#### **Microsoft.EntityFrameworkCore.Sqlite (8.0.0)**
- **Version:** 8.0.0
- **Purpose:** SQLite provider for EF Core
- **Usage:** All services (Task, Category) will reference this
- **Configuration:**

**Task Service DbContext:**
```csharp
public class TaskDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=/data/tasks.db");
    }
}
```

**Category Service DbContext:**
```csharp
public class CategoryDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=/data/tasks.db"); // Same database
    }
}
```

#### **Microsoft.EntityFrameworkCore.Design (8.0.0)**
- **Version:** 8.0.0
- **Purpose:** EF Core tooling for migrations
- **Required For:** `dotnet ef migrations add/update`

**⚠️ IMPORTANT:** Shared SQLite database requires:
1. **Shared volume** in Fly.io (all services mount `/data`)
2. **WAL mode** for concurrent reads: `PRAGMA journal_mode=WAL;`
3. **Read-heavy workload** (SQLite handles concurrent reads well, writes are serialized)

### 5.2 Connection String Configuration

**Shared Volume Path:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/data/tasks.db;Mode=ReadWriteCreate;Cache=Shared;Journal Mode=WAL;"
  }
}
```

### 5.3 Database per Service (Alternative - NOT CHOSEN)

**If using separate databases:**
- Task Service: `/data/tasks.db`
- Category Service: `/data/categories.db`
- Requires data synchronization logic
- More complex for single-user scenario

---

## 6. Health Checks & Monitoring

### 6.1 Health Check Packages

#### **Microsoft.Extensions.Diagnostics.HealthChecks (Built-in)**
- **Version:** Included in .NET 8
- **Purpose:** Service health monitoring
- **Usage:** Health check endpoints for each service

#### **AspNetCore.HealthChecks.Sqlite (8.0.1)**
- **Version:** 8.0.1
- **NuGet:** `AspNetCore.HealthChecks.Sqlite`
- **Purpose:** SQLite database health checks
- **Justification:** Verify database connectivity from each service

**Configuration:**
```csharp
builder.Services.AddHealthChecks()
    .AddSqlite(
        connectionString: "Data Source=/data/tasks.db",
        name: "sqlite-db",
        tags: new[] { "db", "sqlite" }
    );

app.MapHealthChecks("/health");
```

#### **AspNetCore.HealthChecks.UI (8.0.1)** - OPTIONAL
- **Version:** 8.0.1
- **Purpose:** Web UI for aggregated health status
- **Justification:** Visual dashboard for all services
- **⚠️ DECISION REQUIRED:** Adds complexity, may not be needed for MVP

### 6.2 Service Health Check Configuration

**Each service should expose:**
- `/health` - Basic liveness probe
- `/health/ready` - Readiness probe (includes DB checks)

**Fly.io health check configuration (fly.toml):**
```toml
[http_service]
  internal_port = 8080
  
  [[http_service.checks]]
    grace_period = "10s"
    interval = "30s"
    method = "GET"
    timeout = "5s"
    path = "/health"
```

---

## 7. Logging & Diagnostics

### 7.1 Structured Logging

#### **Serilog.AspNetCore (8.0.0)**
- **Version:** 8.0.0
- **NuGet:** `Serilog.AspNetCore`
- **Purpose:** Structured logging with rich context
- **Justification:** Better than default ILogger for microservices debugging

#### **Serilog.Sinks.Console (5.0.1)**
- **Version:** 5.0.1
- **Purpose:** Console output for Fly.io logs
- **Justification:** Fly.io captures stdout/stderr

#### **Serilog.Sinks.File (5.0.0)**
- **Version:** 5.0.0
- **Purpose:** File-based logging (optional backup)
- **Path:** `/data/logs/service-name.log`

**Configuration:**
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "TaskService")
    .WriteTo.Console()
    .WriteTo.File("/data/logs/task-service.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### 7.2 Distributed Tracing (Advanced - OPTIONAL)

#### **OpenTelemetry.Extensions.Hosting (1.7.0)** - OPTIONAL
- **Version:** 1.7.0
- **Purpose:** Distributed tracing across services
- **Additional Packages:**
  - `OpenTelemetry.Instrumentation.AspNetCore` (1.7.0)
  - `OpenTelemetry.Instrumentation.Http` (1.7.0)
  - `OpenTelemetry.Exporter.Console` (1.7.0)

**⚠️ DECISION REQUIRED:** Adds significant complexity for single-user MVP. Consider only if debugging distributed flows is critical.

**Simpler Alternative:** Use correlation IDs in logs
```csharp
builder.Services.AddHttpContextAccessor();
// Add correlation ID to all log entries and HTTP requests
```

### 7.3 Application Insights (Cloud Alternative - NOT RECOMMENDED)

**Reason:** Requires Azure subscription, external dependency, overkill for single-user app.

---

## 8. Containerization & Docker

### 8.1 .NET SDK/Runtime Images (No NuGet Packages)

**Base Images:**
- `mcr.microsoft.com/dotnet/aspnet:8.0` - Runtime image
- `mcr.microsoft.com/dotnet/sdk:8.0` - Build image

**No NuGet packages needed** - Docker configuration only.

### 8.2 Multi-Stage Dockerfile Pattern

Each service needs a Dockerfile:

```dockerfile
# Stage 1: Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskService/TaskService.csproj", "TaskService/"]
RUN dotnet restore "TaskService/TaskService.csproj"
COPY . .
WORKDIR "/src/TaskService"
RUN dotnet build "TaskService.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "TaskService.csproj" -c Release -o /app/publish

# Stage 4: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskService.dll"]
```

---

## 9. Resilience Patterns

### 9.1 Polly for Resilience

#### **Microsoft.Extensions.Http.Polly (8.0.0)**
- **Version:** 8.0.0
- **NuGet:** `Microsoft.Extensions.Http.Polly`
- **Purpose:** Retry, circuit breaker, timeout policies for HttpClient
- **Justification:** Critical for inter-service communication reliability

**Configuration:**
```csharp
builder.Services.AddHttpClient("TaskService")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

#### **Polly (8.2.0)** - Included via Microsoft.Extensions.Http.Polly
- **Core resilience library**
- **Features:** Retry, circuit breaker, timeout, bulkhead, fallback

**⚠️ IMPORTANT:** Essential for microservices to handle transient failures.

---

## 10. Configuration Management

### 10.1 Built-in Configuration Providers

#### **Microsoft.Extensions.Configuration (Built-in)**
- **Included in .NET 8 SDK**
- **Providers:**
  - JSON files (`appsettings.json`, `appsettings.Production.json`)
  - Environment variables
  - Command-line arguments

**No additional packages needed.**

### 10.2 Service Discovery via Configuration

**Static configuration approach (as decided):**

**appsettings.json (UI Service):**
```json
{
  "Services": {
    "TaskService": {
      "Url": "http://task-service:8080"
    },
    "CategoryService": {
      "Url": "http://category-service:8081"
    }
  }
}
```

**Fly.io environment variables (fly.toml):**
```toml
[env]
  ASPNETCORE_ENVIRONMENT = "Production"
  Services__TaskService__Url = "http://task-service.internal:8080"
  Services__CategoryService__Url = "http://category-service.internal:8081"
```

### 10.3 Secrets Management

#### **User Secrets (Development Only)**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=tasks.db"
```

#### **Fly.io Secrets (Production)**
```bash
fly secrets set DATABASE_PATH="/data/tasks.db"
```

**No additional packages needed** - built-in configuration binding.

---

## 11. Fly.io Deployment Considerations

### 11.1 Multi-Container Deployment

**Fly.io Apps Configuration:**
- `taskmaster-gateway` (YARP)
- `taskmaster-ui` (Razor Pages)
- `taskmaster-task-service` (Web API)
- `taskmaster-category-service` (Web API)

**fly.toml for each service:**
```toml
app = "taskmaster-task-service"

[build]
  dockerfile = "Dockerfile.TaskService"

[env]
  ASPNETCORE_URLS = "http://0.0.0.0:8080"

[[mounts]]
  source = "taskmaster_data"
  destination = "/data"

[[vm]]
  size = "shared-cpu-1x"
  memory = "256mb"
```

### 11.2 Shared Volume for SQLite

**Create volume:**
```bash
fly volumes create taskmaster_data --region ord --size 1
```

**Mount in all services:**
```toml
[[mounts]]
  source = "taskmaster_data"
  destination = "/data"
```

**⚠️ LIMITATION:** Fly.io volumes are region-specific and single-instance. Multiple replicas would conflict with SQLite writes.

### 11.3 Internal Networking

**Fly.io .internal DNS:**
- Services communicate via `http://<app-name>.internal:8080`
- Automatic service discovery within organization
- No additional packages needed

---

## 12. Complete Package Matrix

### 12.1 Gateway Service (YARP)

| Package | Version | Required | Purpose |
|---------|---------|----------|---------|
| `Yarp.ReverseProxy` | 2.1.0 | ✅ Yes | API Gateway routing |
| `Serilog.AspNetCore` | 8.0.0 | ⚠️ Recommended | Structured logging |
| `Serilog.Sinks.Console` | 5.0.1 | ⚠️ Recommended | Console output |

**.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="Yarp.ReverseProxy" Version="2.1.0" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
</ItemGroup>
```

### 12.2 UI Service (Razor Pages)

| Package | Version | Required | Purpose |
|---------|---------|----------|---------|
| `Microsoft.AspNetCore.App` | (Implicit) | ✅ Yes | Razor Pages framework |
| `Microsoft.Extensions.Http.Polly` | 8.0.0 | ✅ Yes | Resilient HTTP calls |
| `Serilog.AspNetCore` | 8.0.0 | ⚠️ Recommended | Structured logging |
| `Refit` | 7.1.2 | ❓ Optional | Typed HTTP clients |

**.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
  <!-- Optional: <PackageReference Include="Refit.HttpClientFactory" Version="7.1.2" /> -->
</ItemGroup>
```

### 12.3 Task Service (Web API)

| Package | Version | Required | Purpose |
|---------|---------|----------|---------|
| `Microsoft.AspNetCore.App` | (Implicit) | ✅ Yes | ASP.NET Core Web API |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.0 | ✅ Yes | SQLite database access |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | ✅ Yes | Migrations tooling |
| `AspNetCore.HealthChecks.Sqlite` | 8.0.1 | ⚠️ Recommended | Database health checks |
| `Serilog.AspNetCore` | 8.0.0 | ⚠️ Recommended | Structured logging |

**.csproj:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  <PackageReference Include="AspNetCore.HealthChecks.Sqlite" Version="8.0.1" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
</ItemGroup>
```

### 12.4 Category Service (Web API)

**Same as Task Service** - identical package dependencies.

---

## 13. Pending Decisions

### 13.1 Critical Architectural Decisions

| # | Decision | Options | Recommendation | Impact |
|---|----------|---------|----------------|--------|
| 1 | **Typed HTTP Clients** | Refit vs. Manual HttpClient | Manual for MVP simplicity | External dependency vs. boilerplate |
| 2 | **Distributed Tracing** | OpenTelemetry vs. Correlation IDs | Correlation IDs | Complexity vs. observability |
| 3 | **Health Check UI** | AspNetCore.HealthChecks.UI vs. None | None for MVP | Visual dashboard vs. simplicity |
| 4 | **Shared Database** | Single SQLite vs. DB per service | Single SQLite (as decided) | Simplicity vs. service isolation |
| 5 | **Response Caching** | Enable vs. Skip | Skip for MVP | Performance vs. stale data risk |

### 13.2 Package Version Strategy

**Question:** Should we use latest patch versions (e.g., 8.0.1, 8.0.2) or pin to specific versions?

**Recommendation:** Pin to specific minor versions but allow patch updates:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
```

---

## 14. Trade-offs & Concerns

### 14.1 Microservices Complexity vs. MVP Goals

**⚠️ MAJOR CONCERN:** The PRD emphasizes "minimal deployment complexity" and "< 180 minutes" deployment, but microservices introduce:

1. **Multiple Dockerfiles** (4 services vs. 1)
2. **Multiple Fly.io apps** (4 deployments vs. 1)
3. **Inter-service networking** configuration
4. **Distributed debugging** challenges
5. **Shared database coordination** (SQLite concurrent access)

**Trade-off Analysis:**

| Aspect | Monolithic | Microservices |
|--------|-----------|---------------|
| **Deployment Time** | 15 min | 60-90 min |
| **Operational Complexity** | Low | High |
| **Debugging** | Single process | Distributed logs |
| **Cost (Fly.io)** | 1 VM | 4 VMs |
| **Database Access** | Direct | Network + latency |
| **Learning Curve** | Low | High |

**Recommendation:** If the goal is truly MVP and < 180 minutes, **reconsider microservices** or accept that deployment target will increase to **< 4-6 hours**.

### 14.2 Zero External Dependencies Constraint

**Packages that violate "zero external dependencies":**
- ✅ `Yarp.ReverseProxy` - Microsoft-maintained, not truly "external"
- ✅ `Serilog` - Industry standard, widely used
- ✅ `AspNetCore.HealthChecks.Sqlite` - Community package, optional
- ❌ `Refit` - External, but optional

**Verdict:** Only Refit is truly external and optional. Others are Microsoft or widely adopted.

### 14.3 SQLite Shared Database Concerns

**Potential Issues:**
1. **Write Contention:** Only one writer at a time (acceptable for single-user)
2. **File Locking:** WAL mode helps but not perfect
3. **Volume Mount:** All services must mount same volume (Fly.io limitation)
4. **Backup Complexity:** Only one service should handle backups

**Mitigation:**
- Use `PRAGMA journal_mode=WAL;`
- Set `PRAGMA busy_timeout=5000;` for write retries
- Implement retry logic in services
- Designate Task Service as "primary" for migrations

### 14.4 Fly.io Multi-Service Deployment

**Challenges:**
1. **No Docker Compose Support:** Each service is separate Fly.io app
2. **Manual Coordination:** Must deploy services in correct order
3. **Volume Sharing:** Requires all services in same region
4. **Internal DNS:** Services use `.internal` domain for discovery

**Deployment Order:**
1. Create shared volume
2. Deploy Task Service (runs migrations)
3. Deploy Category Service
4. Deploy UI Service
5. Deploy Gateway (last, routes to others)

---

## 15. Alternative Recommendations

### 15.1 Modular Monolith (Recommended Alternative)

Instead of full microservices, consider a **modular monolith**:

**Structure:**
```
TaskMaster/
├── TaskMaster.UI/           # Razor Pages
├── TaskMaster.Api/          # Single Web API
│   ├── Features/
│   │   ├── Tasks/           # Task endpoints
│   │   └── Categories/      # Category endpoints
├── TaskMaster.Domain/       # Shared models
└── TaskMaster.Data/         # Shared DbContext
```

**Benefits:**
- ✅ Clear module boundaries (easy to extract later)
- ✅ Single deployment (< 180 min target achievable)
- ✅ Shared database (no coordination needed)
- ✅ Can still use YARP if needed (Gateway + API + UI)
- ✅ Easier debugging and development

**Packages Needed:**
- Same EF Core packages
- Same Razor Pages packages
- Optional YARP if gateway desired
- Simpler overall

### 15.2 Three-Service Compromise

If microservices are required, simplify to **3 services** instead of 4:

1. **Gateway + UI** (Combined - Razor Pages with YARP routing)
2. **Task Service** (Web API)
3. **Category Service** (Web API - or just use enum in Task Service)

**Justification:** Category is just an enum - doesn't need a separate service.

---

## 16. Final Package Summary

### 16.1 Required Packages (All Services)

| Package | Version | Services | Purpose |
|---------|---------|----------|---------|
| `Yarp.ReverseProxy` | 2.1.0 | Gateway | Reverse proxy routing |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.0 | Task, Category | SQLite access |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Task, Category | Migrations |
| `Microsoft.Extensions.Http.Polly` | 8.0.0 | UI | Resilient HTTP calls |

### 16.2 Recommended Packages

| Package | Version | Services | Purpose |
|---------|---------|----------|---------|
| `Serilog.AspNetCore` | 8.0.0 | All | Structured logging |
| `Serilog.Sinks.Console` | 5.0.1 | All | Console output |
| `AspNetCore.HealthChecks.Sqlite` | 8.0.1 | Task, Category | Health monitoring |

### 16.3 Optional Packages

| Package | Version | Services | Purpose | Decision |
|---------|---------|----------|---------|----------|
| `Refit.HttpClientFactory` | 7.1.2 | UI | Typed HTTP clients | Skip for MVP |
| `OpenTelemetry.Extensions.Hosting` | 1.7.0 | All | Distributed tracing | Skip for MVP |
| `AspNetCore.HealthChecks.UI` | 8.0.1 | Gateway | Health dashboard | Skip for MVP |

---

## 17. Implementation Checklist

- [ ] Create solution with 4 projects (Gateway, UI, TaskService, CategoryService)
- [ ] Add NuGet packages to each project as per matrix
- [ ] Configure YARP routing in Gateway service
- [ ] Implement typed HttpClient in UI service with Polly policies
- [ ] Configure shared SQLite connection string with WAL mode
- [ ] Set up Serilog with console sink for all services
- [ ] Add health check endpoints to API services
- [ ] Create Dockerfiles for each service
- [ ] Configure fly.toml for each service
- [ ] Create shared Fly.io volume for database
- [ ] Deploy services in correct order (Task → Category → UI → Gateway)
- [ ] Test inter-service communication
- [ ] Verify database access from all services
- [ ] Test health endpoints
- [ ] Review logs in Fly.io dashboard

---

## 18. Conclusion

This package research provides a comprehensive foundation for implementing TaskMaster as a microservices architecture on .NET 8. However, **critical trade-offs** must be considered:

1. **Complexity vs. MVP Goals:** Microservices significantly increase complexity
2. **Deployment Time:** Exceeds < 180 minute target
3. **Operational Overhead:** 4 services vs. 1 monolith
4. **SQLite Limitations:** Shared database creates coordination challenges

**Recommendation:** Re-evaluate architecture choice or accept extended timeline. Consider modular monolith as alternative that preserves learning goals with lower complexity.

**Next Steps:** Review with stakeholders and decide on architecture before implementation.
