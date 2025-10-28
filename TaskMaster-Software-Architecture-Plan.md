# TaskMaster Software Architecture Plan

**Version:** 1.0  
**Date:** October 28, 2025  
**Project:** TaskMaster MVP - Single-User Task Management Application  
**Target Deployment Time:** < 180 minutes

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Architecture Overview](#2-architecture-overview)
3. [Technology Stack Details](#3-technology-stack-details)
4. [Component Design](#4-component-design)
5. [Data Architecture](#5-data-architecture)
6. [NuGet Packages & Dependencies](#6-nuget-packages--dependencies)
7. [Application Configuration](#7-application-configuration)
8. [Deployment Architecture](#8-deployment-architecture)
9. [Development Workflow](#9-development-workflow)
10. [Security Considerations](#10-security-considerations)
11. [Performance Considerations](#11-performance-considerations)
12. [Future Extensibility](#12-future-extensibility)
13. [Risks & Mitigation](#13-risks--mitigation)
14. [Development Estimates](#14-development-estimates)

---

## 1. Executive Summary

### 1.1 Project Overview

**TaskMaster** is a single-user MVP task management application designed to demonstrate C#/.NET engineering principles with minimal deployment complexity. The application allows users to create, view, toggle completion status, delete, and filter tasks with categories and optional due dates.

**Primary Goals:**
- Demonstrate ASP.NET Core 8 best practices
- Achieve rapid deployment to Fly.io (< 180 minutes)
- Showcase Razor Pages, Entity Framework Core, and SQLite integration
- Maintain zero external dependencies beyond the .NET ecosystem

### 1.2 Architecture Approach Rationale

**Monolithic Architecture** has been selected for this MVP for the following strategic reasons:

1. **Simplicity:** Single deployment unit, single codebase, minimal operational overhead
2. **Rapid Development:** No need for API contracts, service orchestration, or inter-service communication
3. **Cost Efficiency:** Single container deployment, minimal infrastructure requirements
4. **Development Velocity:** Direct method calls instead of HTTP/RPC overhead
5. **Debugging Efficiency:** Single process to debug, trace, and monitor
6. **Deployment Speed:** One build, one container, one deployment command

For a single-user MVP with limited scope, a monolithic architecture provides the optimal balance of simplicity, performance, and maintainability.

### 1.3 Key Technology Decisions

| Decision | Technology | Rationale |
|----------|-----------|-----------|
| **Framework** | ASP.NET Core 8 | Latest LTS, high performance, built-in DI, comprehensive tooling |
| **UI Pattern** | Razor Pages | Server-side rendering, no build step, simpler than MVC for page-centric apps |
| **Database** | SQLite | File-based, zero configuration, no external service, perfect for single-user |
| **ORM** | Entity Framework Core | Type-safe queries, migration support, change tracking, LINQ integration |
| **Frontend** | Server-rendered HTML | No JavaScript framework, no npm, minimal complexity, fast initial load |
| **Deployment** | Fly.io | Single container, volume persistence, simple CLI deployment |

---

## 2. Architecture Overview

### 2.1 High-Level System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Browser                              │
│                    (HTML Renderer)                           │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP Request/Response
                     │ (Server-rendered HTML)
                     ▼
┌─────────────────────────────────────────────────────────────┐
│              ASP.NET Core 8 Application                      │
│          (Single Monolithic Container)                       │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Presentation Layer                           │   │
│  │  - Razor Pages (.cshtml)                            │   │
│  │  - PageModels (.cshtml.cs)                          │   │
│  │  - Tag Helpers                                      │   │
│  │  - Model Binding & Validation                       │   │
│  └──────────────────┬──────────────────────────────────┘   │
│                     │                                        │
│  ┌─────────────────▼──────────────────────────────────┐   │
│  │         Domain Layer                                │   │
│  │  - TaskItem Entity                                  │   │
│  │  - Category Enum                                    │   │
│  │  - Data Annotations                                 │   │
│  └──────────────────┬──────────────────────────────────┘   │
│                     │                                        │
│  ┌─────────────────▼──────────────────────────────────┐   │
│  │         Data Access Layer                           │   │
│  │  - AppDbContext                                     │   │
│  │  - EF Core                                          │   │
│  │  - Migrations                                       │   │
│  └──────────────────┬──────────────────────────────────┘   │
│                     │                                        │
└─────────────────────┼────────────────────────────────────────┘
                      │ EF Core (ORM)
                      ▼
         ┌────────────────────────────┐
         │   SQLite Database          │
         │   (tasks.db)               │
         │   Persisted in Fly.io      │
         │   Volume                   │
         └────────────────────────────┘
```

### 2.2 Monolithic Architecture Explanation

The TaskMaster application follows a **layered monolithic architecture** where all components reside within a single ASP.NET Core project. This approach provides:

- **Unified Deployment:** All code deploys as a single unit
- **Direct Integration:** Components communicate via direct method calls (in-process)
- **Shared Context:** Single DbContext, single dependency injection container
- **Simplified Testing:** All code accessible in same process for integration tests

### 2.3 Layered Architecture Within the Monolith

#### **Presentation Layer (Razor Pages)**
- Handles HTTP requests/responses
- Renders server-side HTML
- Performs model binding and validation
- Contains user interface logic

#### **Domain Layer (Entities)**
- Defines business entities (TaskItem, Category)
- Encapsulates business rules via data annotations
- Provides domain models used across layers

#### **Data Access Layer (EF Core)**
- Manages database context and connections
- Handles CRUD operations via Entity Framework Core
- Executes migrations for schema management
- Provides data persistence abstraction

**Flow:** Browser → Razor Page → PageModel → DbContext → SQLite → DbContext → PageModel → Razor Page → Browser

---

## 3. Technology Stack Details

### 3.1 ASP.NET Core 8 Specifics

**ASP.NET Core 8** (LTS release, supported until November 2026) provides:

- **High Performance:** Kestrel web server with HTTP/2 and HTTP/3 support
- **Minimal APIs:** Simplified Program.cs with top-level statements
- **Native AOT Support:** Faster startup and smaller memory footprint (optional)
- **Improved Dependency Injection:** Enhanced lifetime management and validation
- **Built-in Observability:** Logging, metrics, and distributed tracing

**Key Features Utilized:**
- Razor Pages framework for page-centric UI
- Entity Framework Core 8 integration
- Built-in model validation with data annotations
- Tag Helpers for HTML generation
- Anti-forgery token generation for form security

### 3.2 Razor Pages Architecture

**Razor Pages** is ASP.NET Core's page-based programming model optimized for server-side rendered applications.

**Why Razor Pages over MVC:**
1. **Page-Centric:** Each page is self-contained with its own model (PageModel)
2. **Simpler Organization:** Co-located .cshtml and .cshtml.cs files
3. **Less Boilerplate:** No need for separate Controllers and Views folders
4. **Clearer Intent:** Page-specific handlers (OnGet, OnPost, OnPostDelete)
5. **Better for CRUD:** Ideal for form-based applications like TaskMaster

**Architecture Components:**
- **`.cshtml` files:** Razor markup with HTML and C# expressions
- **`.cshtml.cs` files:** PageModel classes with handler methods
- **Tag Helpers:** Server-side components that generate HTML (asp-for, asp-page, etc.)
- **Model Binding:** Automatic mapping of form data to C# properties via `[BindProperty]`
- **Handler Methods:** OnGet (read), OnPost (create/update), OnPostDelete, OnPostToggle

### 3.3 Entity Framework Core with SQLite

**Entity Framework Core 8** is a modern object-relational mapper (ORM) that provides:

- **Code-First Approach:** Define models in C#, generate database schema
- **LINQ Queries:** Type-safe, compile-time checked database queries
- **Change Tracking:** Automatic detection of entity modifications
- **Migration System:** Version-controlled schema changes
- **Database Abstraction:** Switch databases with minimal code changes

**SQLite Integration:**
```csharp
options.UseSqlite("Data Source=tasks.db");
```

**Why SQLite:**
1. **Zero Configuration:** No server installation or setup
2. **File-Based:** Single database file (tasks.db)
3. **Embedded:** Runs in-process with the application
4. **Reliable:** ACID-compliant with proven track record
5. **Perfect for Single-User:** No concurrent write contention issues
6. **Portable:** Database file moves with application

**SQLite Limitations (Acceptable for This Use Case):**
- Limited concurrent write access (not needed for single-user)
- No built-in replication (not needed for MVP)
- Maximum database size ~140TB (far exceeds task app needs)

### 3.4 Technology Choice Justifications

| Technology | Alternative Considered | Why Chosen |
|------------|----------------------|------------|
| **Razor Pages** | MVC, Blazor Server | Simpler for page-centric apps, no WebSocket overhead |
| **SQLite** | PostgreSQL, SQL Server | No external service, zero config, file-based persistence |
| **Server Rendering** | SPA (React/Vue) | No build step, faster initial load, simpler deployment |
| **Fly.io** | Azure, AWS, Heroku | Simple CLI, affordable, volume support, .NET friendly |

---

## 4. Component Design

### 4.1 Presentation Layer

#### **Razor Pages Structure**

```
Pages/
├── Shared/
│   └── _Layout.cshtml          # Master layout with CSS
├── Index.cshtml                # Task list view
├── Index.cshtml.cs             # Task list logic (PageModel)
├── Create.cshtml               # Task creation form
└── Create.cshtml.cs            # Task creation logic (PageModel)
```

#### **Index.cshtml - Task List Page**

**Responsibilities:**
- Display all tasks with title, category, due date, completion status
- Provide filter buttons (All, Active, Done)
- Render Toggle Complete and Delete buttons for each task
- Show "Add New Task" link

**Key Razor Features:**
```razor
@page
@model IndexModel

<!-- Filter links with query string binding -->
<a asp-page="/Index" asp-route-filter="All">All</a>

<!-- Display tasks -->
@foreach (var task in Model.Tasks)
{
    <div class="task-item @(task.IsCompleted ? "completed" : "")">
        <h3>@task.Title</h3>
        <span>@task.Category</span>
        
        <!-- Toggle completion form -->
        <form method="post" asp-page-handler="Toggle" asp-route-id="@task.Id">
            <button type="submit">Toggle</button>
        </form>
        
        <!-- Delete form -->
        <form method="post" asp-page-handler="Delete" asp-route-id="@task.Id">
            <button type="submit">Delete</button>
        </form>
    </div>
}
```

#### **Index.cshtml.cs - PageModel**

**Responsibilities:**
- Load tasks from database with filtering
- Handle toggle completion status
- Handle delete task
- Manage filter state via query string

**Key Implementation:**
```csharp
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    
    public IndexModel(AppDbContext context)
    {
        _context = context;
    }
    
    public List<TaskItem> Tasks { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "All";
    
    public async Task OnGetAsync()
    {
        var query = _context.Tasks.AsQueryable();
        
        query = Filter switch
        {
            "Active" => query.Where(t => !t.IsCompleted),
            "Done" => query.Where(t => t.IsCompleted),
            _ => query
        };
        
        Tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }
    
    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
```

#### **Create.cshtml - Task Creation Form**

**Responsibilities:**
- Render form with Title, Category, and DueDate inputs
- Display validation messages
- Handle form submission

**Key Razor Features:**
```razor
@page
@model CreateModel

<form method="post">
    <div asp-validation-summary="All"></div>
    
    <label asp-for="Task.Title"></label>
    <input asp-for="Task.Title" />
    <span asp-validation-for="Task.Title"></span>
    
    <label asp-for="Task.Category"></label>
    <select asp-for="Task.Category" asp-items="Html.GetEnumSelectList<Category>()">
        <option value="">-- Select Category --</option>
    </select>
    <span asp-validation-for="Task.Category"></span>
    
    <label asp-for="Task.DueDate"></label>
    <input asp-for="Task.DueDate" type="date" />
    <span asp-validation-for="Task.DueDate"></span>
    
    <button type="submit">Create Task</button>
</form>
```

#### **Create.cshtml.cs - PageModel**

**Responsibilities:**
- Bind form data to TaskItem model
- Validate input using ModelState
- Save new task to database
- Redirect to Index on success

**Key Implementation:**
```csharp
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    
    public CreateModel(AppDbContext context)
    {
        _context = context;
    }
    
    [BindProperty]
    public TaskItem Task { get; set; }
    
    public void OnGet()
    {
        // Display empty form
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        Task.CreatedAt = DateTime.UtcNow;
        _context.Tasks.Add(Task);
        await _context.SaveChangesAsync();
        
        return RedirectToPage("/Index");
    }
}
```

#### **_Layout.cshtml - Shared Layout**

**Responsibilities:**
- Provide consistent page structure
- Include CSS styling
- Define navigation elements
- Render validation scripts

**Key Features:**
```razor
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - TaskMaster</title>
    <style>
        /* Embedded CSS for zero external dependencies */
        body { font-family: system-ui; max-width: 800px; margin: 0 auto; padding: 20px; }
        .task-item { border: 1px solid #ddd; padding: 15px; margin: 10px 0; }
        .completed { opacity: 0.6; text-decoration: line-through; }
        /* Additional styling... */
    </style>
</head>
<body>
    <header>
        <h1>TaskMaster</h1>
    </header>
    <main>
        @RenderBody()
    </main>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### 4.2 Domain Layer

#### **TaskItem Entity**

**Purpose:** Represents a single task in the system

**Implementation:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskMaster.Data
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title must be under 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public Category Category { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

**Property Details:**
- **Id:** Primary key, auto-generated by database
- **Title:** Required, max 200 characters, user-visible task name
- **Category:** Required enum, determines task classification
- **DueDate:** Nullable, optional deadline for task completion
- **IsCompleted:** Boolean flag, tracks completion status
- **CreatedAt:** Timestamp, set automatically on creation

**Validation Attributes:**
- `[Required]`: Ensures non-null value
- `[StringLength]`: Enforces maximum character limit
- Data annotations work with both client-side (Tag Helpers) and server-side (ModelState) validation

#### **Category Enum**

**Purpose:** Defines valid task categories

**Implementation:**
```csharp
namespace TaskMaster.Data
{
    public enum Category
    {
        Work = 0,
        Personal = 1,
        Shopping = 2,
        Health = 3
    }
}
```

**Design Considerations:**
- Stored as integer in database (efficient storage)
- Type-safe selection (compile-time validation)
- Easy to render as dropdown via `Html.GetEnumSelectList<Category>()`
- Easily extensible by adding new enum values

### 4.3 Data Access Layer

#### **AppDbContext**

**Purpose:** Entity Framework Core database context for TaskMaster

**Implementation:**
```csharp
using Microsoft.EntityFrameworkCore;

namespace TaskMaster.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<TaskItem> Tasks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Optional: Configure entity relationships, indexes, constraints
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.IsCompleted); // Index for filtering
                entity.HasIndex(e => e.CreatedAt);   // Index for sorting
            });
        }
    }
}
```

**Responsibilities:**
- Define DbSet properties for each entity type
- Configure database connection via dependency injection
- Manage entity change tracking
- Execute queries and persist changes
- Apply migrations to database schema

**DbSet Operations:**
- **Add:** `_context.Tasks.Add(task)` - Insert new task
- **Find:** `await _context.Tasks.FindAsync(id)` - Find by primary key
- **Remove:** `_context.Tasks.Remove(task)` - Delete task
- **Query:** `_context.Tasks.Where(t => !t.IsCompleted)` - LINQ queries
- **SaveChanges:** `await _context.SaveChangesAsync()` - Commit changes to database

---

## 5. Data Architecture

### 5.1 SQLite Database Schema

**Database File:** `tasks.db` (SQLite format 3)

**Table: Tasks**

| Column Name | Data Type | Constraints | Description |
|-------------|-----------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY AUTOINCREMENT | Unique task identifier |
| Title | TEXT | NOT NULL, MAX 200 chars | Task title |
| Category | INTEGER | NOT NULL | Category enum value (0-3) |
| DueDate | TEXT | NULL | ISO 8601 date string (nullable) |
| IsCompleted | INTEGER | NOT NULL DEFAULT 0 | Boolean flag (0 or 1) |
| CreatedAt | TEXT | NOT NULL | ISO 8601 datetime string (UTC) |

**Indexes:**
```sql
CREATE INDEX IX_Tasks_IsCompleted ON Tasks(IsCompleted);
CREATE INDEX IX_Tasks_CreatedAt ON Tasks(CreatedAt DESC);
```

**SQLite Data Type Mapping:**
- .NET `int` → SQLite `INTEGER`
- .NET `string` → SQLite `TEXT`
- .NET `DateTime` → SQLite `TEXT` (ISO 8601 format: "yyyy-MM-dd HH:mm:ss")
- .NET `bool` → SQLite `INTEGER` (0 = false, 1 = true)
- .NET `Category` enum → SQLite `INTEGER`

### 5.2 Entity Relationships

**Current State:** Single entity (TaskItem) with no relationships

**Future Considerations:**
- **Tags:** Many-to-many relationship (TaskItem ↔ Tag)
- **Users:** One-to-many relationship (User → TaskItem) if multi-user support added
- **Comments:** One-to-many relationship (TaskItem → Comment)
- **Attachments:** One-to-many relationship (TaskItem → Attachment)

For the MVP, keeping a single entity simplifies development and meets all functional requirements.

### 5.3 Migration Strategy

**EF Core Migrations** provide version-controlled schema changes.

**Initial Migration:**
```bash
dotnet ef migrations add InitialCreate
```

This generates:
```
Migrations/
├── 20251028_InitialCreate.cs           # Migration code
├── 20251028_InitialCreate.Designer.cs  # Metadata
└── AppDbContextModelSnapshot.cs        # Current model state
```

**Apply Migration:**
```bash
dotnet ef database update
```

**Automatic Migration on Startup (Production):**
```csharp
// In Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate(); // Apply pending migrations automatically
}
```

**Benefits:**
- Version control for database schema
- Rollback capability (`dotnet ef database update <PreviousMigration>`)
- Automatic schema synchronization on deployment
- Team collaboration with consistent schema

### 5.4 Data Persistence on Fly.io

**Challenge:** SQLite database file needs to persist across container restarts

**Solution:** Fly.io persistent volumes

**fly.toml Configuration:**
```toml
[mounts]
  source = "taskmaster_data"
  destination = "/app/data"
```

**Connection String:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/tasks.db"
  }
}
```

**Volume Creation:**
```bash
fly volumes create taskmaster_data --region sea --size 1
```

**Persistence Strategy:**
1. Volume mounted at `/app/data`
2. SQLite database stored at `/app/data/tasks.db`
3. Volume persists independently of container lifecycle
4. Automatic reattachment on container restart
5. Backups via `fly volumes snapshot`

**Data Backup Strategy:**
1. Automated Fly.io volume snapshots (daily)
2. Manual export: `fly ssh console` → copy tasks.db
3. Application-level export feature (future enhancement)

---

## 6. NuGet Packages & Dependencies

### 6.1 Required NuGet Packages

**For ASP.NET Core 8 with Razor Pages and EF Core SQLite:**

| Package Name | Version | Purpose | Required |
|--------------|---------|---------|----------|
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.0 | SQLite database provider for EF Core | ✅ Yes |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Design-time tools for migrations | ✅ Yes |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 8.0.0 | Developer exception page for EF Core errors | ✅ Yes |

**Package Justifications:**

#### **Microsoft.EntityFrameworkCore.Sqlite (8.0.0)**
- Provides SQLite database provider for Entity Framework Core
- Enables `UseSqlite()` extension method
- Includes SQLite native libraries for all platforms (Windows, Linux, macOS)
- Required for database operations

#### **Microsoft.EntityFrameworkCore.Design (8.0.0)**
- Required for EF Core CLI tools (`dotnet ef`)
- Enables migration generation and application
- Contains design-time components for scaffolding
- Only needed during development but harmless in production

#### **Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore (8.0.0)**
- Provides helpful error pages for database-related issues
- Suggests pending migrations when database is out of sync
- Shows detailed exception information in development
- Improves developer experience during development

### 6.2 Implicit Package References

**ASP.NET Core 8 SDK includes these packages automatically:**

- `Microsoft.AspNetCore.App` (metapackage)
  - Razor Pages framework
  - Tag Helpers
  - Model binding and validation
  - Dependency injection
  - Logging infrastructure
  - Configuration system

- `Microsoft.EntityFrameworkCore` (included via .Sqlite package)
  - Core EF functionality
  - LINQ provider
  - Change tracking
  - DbContext base class

### 6.3 TaskMaster.csproj Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="8.0.0" />
  </ItemGroup>

</Project>
```

**Property Explanations:**
- `TargetFramework`: net8.0 (ASP.NET Core 8)
- `Nullable`: enable (use nullable reference types for better null safety)
- `ImplicitUsings`: enable (automatically include common namespaces)

### 6.4 Version Management

**Versioning Strategy:**
- Use specific versions (8.0.0) for stability and reproducibility
- All EF Core packages should use the same version to avoid compatibility issues
- Update all packages together when upgrading .NET versions

**Package Restore:**
```bash
dotnet restore
```

**Package Update:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.1
```

---

## 7. Application Configuration

### 7.1 Program.cs Setup

**Modern ASP.NET Core 8 with Minimal Hosting Model:**

```csharp
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
```

**Key Configuration Sections:**

#### **Service Registration**
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```
- Registers AppDbContext with dependency injection container
- Configures SQLite as the database provider
- Sets scoped lifetime (one instance per HTTP request)
- Reads connection string from configuration

#### **Development Tools**
```csharp
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
```
- Displays helpful error pages for database issues
- Shows pending migrations and suggestions
- Only active in development environment

#### **Automatic Migrations**
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```
- Applies pending migrations on application startup
- Ensures database schema is always up-to-date
- Creates database file if it doesn't exist
- Critical for Fly.io deployment (no manual migration step)

#### **Middleware Pipeline**
```csharp
app.UseStaticFiles();      // Serve files from wwwroot
app.UseRouting();           // Enable endpoint routing
app.UseAuthorization();     // Enable authorization (even if not used, required for future)
app.MapRazorPages();        // Map Razor Pages endpoints
```

### 7.2 appsettings.json Structure

**Development Configuration:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tasks.db"
  }
}
```

**Configuration Sections:**

#### **Logging**
- `Default`: Information - log all informational messages and above
- `Microsoft.AspNetCore`: Warning - reduce ASP.NET Core noise
- `Microsoft.EntityFrameworkCore.Database.Command`: Information - log SQL queries (helpful for debugging)

#### **Connection Strings**
- `DefaultConnection`: SQLite connection string
- `Data Source=tasks.db` - database file in application root (development)

### 7.3 appsettings.Production.json

**Production-Specific Configuration:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/tasks.db"
  }
}
```

**Production Differences:**
- **Reduced Logging:** Less verbose output to improve performance
- **Volume Path:** Database stored in persistent volume (`/app/data/tasks.db`)
- **Error-Only Logging:** Only log errors and warnings in production

### 7.4 Environment Configuration

**Environment Detection:**
```csharp
if (!app.Environment.IsDevelopment())
{
    // Production middleware
}
else
{
    // Development middleware
}
```

**Environment Variables (Fly.io):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

**Setting Environment in Fly.io:**
```toml
# fly.toml
[env]
  ASPNETCORE_ENVIRONMENT = "Production"
```

### 7.5 Development vs Production Considerations

| Aspect | Development | Production |
|--------|-------------|------------|
| **Error Pages** | Developer exception page with stack traces | Generic error page |
| **Database Path** | `tasks.db` (project root) | `/app/data/tasks.db` (volume) |
| **Logging Level** | Information | Warning/Error |
| **SQL Logging** | Enabled (Information) | Disabled (Warning) |
| **HTTPS** | Optional | Recommended (Fly.io handles) |
| **Migrations** | Manual via CLI | Automatic on startup |

---

## 8. Deployment Architecture

### 8.1 Fly.io Deployment Strategy

**Fly.io** is a platform-as-a-service that runs applications as Docker containers close to users.

**Why Fly.io:**
- Simple CLI-based deployment
- Built-in volume support for database persistence
- Global CDN and edge deployment
- Automatic HTTPS certificates
- Generous free tier
- .NET-friendly (excellent Docker support)

**Deployment Flow:**
```
Local Development → Docker Build → Fly.io Registry → Container Deployment → Volume Attachment → Application Running
```

### 8.2 Dockerfile Design

**Multi-Stage Dockerfile for .NET 8:**

```dockerfile
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["TaskMaster.csproj", "./"]
RUN dotnet restore "TaskMaster.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "TaskMaster.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "TaskMaster.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskMaster.dll"]
```

**Dockerfile Explanation:**

#### **Stage 1: Base (Runtime)**
- Uses ASP.NET Core runtime image (smaller than SDK)
- Sets working directory to `/app`
- Exposes port 8080 (Fly.io default)

#### **Stage 2: Build**
- Uses full .NET SDK for compilation
- Copies `.csproj` file first (layer caching optimization)
- Restores NuGet packages
- Copies remaining source code
- Compiles application in Release mode

#### **Stage 3: Publish**
- Creates deployment-ready output
- Removes unnecessary files
- Optimizes for production runtime

#### **Stage 4: Final**
- Copies only published files from publish stage
- Results in minimal final image size
- Sets entry point to run the application

**Multi-Stage Build Benefits:**
- **Smaller Final Image:** Only runtime + compiled app (no SDK, no source code)
- **Faster Deployments:** Less data to transfer
- **Better Security:** Minimal attack surface
- **Layer Caching:** Faster rebuilds when only source code changes

### 8.3 fly.toml Configuration

**Fly.io Application Configuration:**

```toml
app = "taskmaster"
primary_region = "sea"

[build]
  dockerfile = "Dockerfile"

[env]
  ASPNETCORE_ENVIRONMENT = "Production"
  ASPNETCORE_URLS = "http://+:8080"

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = true
  auto_start_machines = true
  min_machines_running = 1

[mounts]
  source = "taskmaster_data"
  destination = "/app/data"

[[vm]]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 256
```

**Configuration Sections:**

#### **[build]**
- `dockerfile`: Specifies Dockerfile location
- Fly.io builds image using this Dockerfile

#### **[env]**
- `ASPNETCORE_ENVIRONMENT`: Sets environment to Production
- `ASPNETCORE_URLS`: Configures Kestrel to listen on port 8080

#### **[http_service]**
- `internal_port`: Port application listens on inside container
- `force_https`: Redirect HTTP to HTTPS
- `auto_stop_machines`: Stop when idle to save resources
- `auto_start_machines`: Start on incoming requests
- `min_machines_running`: Keep at least 1 instance running

#### **[mounts]**
- `source`: Volume name (created separately)
- `destination`: Mount path inside container (`/app/data`)
- Ensures `tasks.db` persists across deployments

#### **[[vm]]**
- `cpu_kind`: shared (cost-effective)
- `cpus`: 1 CPU
- `memory_mb`: 256MB (sufficient for single-user SQLite app)

### 8.4 Volume Configuration for SQLite Persistence

**Volume Creation:**
```bash
fly volumes create taskmaster_data --region sea --size 1
```

**Volume Details:**
- **Name:** `taskmaster_data`
- **Region:** `sea` (Seattle - match primary_region in fly.toml)
- **Size:** 1GB (more than enough for task database)
- **Persistence:** Survives deployments, restarts, and container replacements

**Database Path:**
```
/app/data/tasks.db
```

**Backup Strategy:**
```bash
# Create snapshot
fly volumes snapshots create taskmaster_data

# List snapshots
fly volumes snapshots list taskmaster_data

# Restore from snapshot
fly volumes restore <snapshot-id>
```

### 8.5 Port Exposure and Environment Configuration

**Port Configuration:**
```dockerfile
EXPOSE 8080
```

```csharp
// Program.cs
app.Run(); // Listens on ports specified in ASPNETCORE_URLS
```

**Environment Variables:**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

**Fly.io Networking:**
- External HTTPS (port 443) → Fly.io Proxy → Internal HTTP (port 8080)
- Automatic SSL certificate management
- HTTP to HTTPS redirect via `force_https = true`

### 8.6 Deployment Commands

**Initial Setup:**
```bash
# Install Fly.io CLI
curl -L https://fly.io/install.sh | sh

# Login to Fly.io
fly auth login

# Initialize Fly.io app
fly launch

# Create persistent volume
fly volumes create taskmaster_data --region sea --size 1

# Deploy application
fly deploy

# Open application
fly open
```

**Subsequent Deployments:**
```bash
# Deploy changes
fly deploy

# View logs
fly logs

# Check status
fly status

# SSH into container
fly ssh console
```

**Rollback Strategy:**
```bash
# List releases
fly releases

# Rollback to previous version
fly releases rollback <version>
```

---

## 9. Development Workflow

### 9.1 Project Setup Steps

**Step 1: Create New Razor Pages Project**
```bash
dotnet new razor -n TaskMaster
cd TaskMaster
```

**Step 2: Add NuGet Packages**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore --version 8.0.0
```

**Step 3: Install EF Core Tools**
```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

**Step 4: Create Project Structure**
```bash
mkdir Data
mkdir Migrations
```

**Step 5: Create Domain Models**
- Create `Data/TaskItem.cs`
- Create `Data/Category.cs` (enum)
- Create `Data/AppDbContext.cs`

**Step 6: Configure Database**
- Update `Program.cs` with DbContext registration
- Update `appsettings.json` with connection string

**Step 7: Create Initial Migration**
```bash
dotnet ef migrations add InitialCreate
```

**Step 8: Apply Migration**
```bash
dotnet ef database update
```

**Step 9: Create Razor Pages**
- Create `Pages/Index.cshtml` and `Pages/Index.cshtml.cs`
- Create `Pages/Create.cshtml` and `Pages/Create.cshtml.cs`
- Update `Pages/Shared/_Layout.cshtml`

**Step 10: Test Locally**
```bash
dotnet run
# Navigate to https://localhost:5001
```

### 9.2 Database Migration Workflow

**Creating New Migration:**
```bash
# After modifying entities
dotnet ef migrations add <MigrationName>
```

**Applying Migrations:**
```bash
# Update local database
dotnet ef database update

# Apply specific migration
dotnet ef database update <MigrationName>
```

**Removing Last Migration:**
```bash
# If not yet applied
dotnet ef migrations remove
```

**Viewing Migration SQL:**
```bash
# Generate SQL script
dotnet ef migrations script
```

**Production Migration:**
- Migrations apply automatically on app startup via `context.Database.Migrate()`
- No manual intervention required on Fly.io deployment

### 9.3 Local Development with dotnet run

**Development Server:**
```bash
dotnet run
```

**Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Hot Reload (File Watch Mode):**
```bash
dotnet watch run
```
- Automatically recompiles and restarts on file changes
- Ideal for rapid development iteration

**Development URLs:**
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

**Database Location (Development):**
- File path: `./tasks.db` (project root)
- Visible in VS Code Explorer
- Can open with SQLite extension for inspection

### 9.4 Testing Checklist from PRD

#### **Functionality Tests**

- [ ] **View all tasks**
  - Navigate to `/Index`
  - Verify tasks are displayed in a list
  - Confirm task details (title, category, due date, status) are visible

- [ ] **Filter by All/Active/Done**
  - Click "All" filter → verify all tasks shown
  - Click "Active" filter → verify only incomplete tasks shown
  - Click "Done" filter → verify only completed tasks shown

- [ ] **Create task with title only**
  - Navigate to `/Create`
  - Enter title, leave category and due date empty
  - Submit form → verify validation requires category
  - Select category → submit → verify task created

- [ ] **Create task with category**
  - Navigate to `/Create`
  - Enter title and select category
  - Submit → verify task appears in list with correct category

- [ ] **Create task with due date**
  - Navigate to `/Create`
  - Enter title, category, and due date
  - Submit → verify task shows due date

- [ ] **Toggle task completion**
  - Find task in list
  - Click "Toggle" button
  - Verify task shows as completed (styling change)
  - Click again → verify task returns to active state

- [ ] **Delete task**
  - Find task in list
  - Click "Delete" button
  - Verify task is removed from list

- [ ] **Validation: Empty title shows error**
  - Navigate to `/Create`
  - Leave title empty, submit form
  - Verify "Title is required" error message displays

- [ ] **Validation: Title >200 chars shows error**
  - Navigate to `/Create`
  - Enter title with 201 characters
  - Submit → verify "Title must be under 200 characters" error displays

#### **Deployment Tests**

- [ ] **Runs locally with `dotnet run`**
  - Execute `dotnet run`
  - Verify application starts without errors
  - Access `https://localhost:5001` successfully

- [ ] **Database persists after restart**
  - Create several tasks
  - Stop application (Ctrl+C)
  - Start application (`dotnet run`)
  - Verify tasks still exist

- [ ] **Deploys to Fly.io**
  - Execute `fly deploy`
  - Verify deployment completes successfully
  - No build errors or deployment failures

- [ ] **Database persists in Fly.io volume**
  - Create tasks on deployed app
  - Redeploy application (`fly deploy`)
  - Verify tasks still exist after redeployment

- [ ] **App accessible via Fly.io URL**
  - Access `https://<app-name>.fly.dev`
  - Verify application loads
  - Test all functionality on production instance

---

## 10. Security Considerations

### 10.1 Anti-Forgery Token Usage in Forms

**Why Needed:** Prevent Cross-Site Request Forgery (CSRF) attacks

**Implementation:**
```razor
<form method="post">
    <!-- Anti-forgery token automatically included -->
    @* Token rendered automatically by Razor Pages *@
</form>
```

**Automatic Protection:**
- Razor Pages automatically generate and validate anti-forgery tokens
- Token embedded in hidden form field
- Validated on POST request
- Invalid/missing token → HTTP 400 Bad Request

**Manual Configuration (if needed):**
```csharp
// Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});
```

### 10.2 Input Validation and ModelState

**Server-Side Validation:**
```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page(); // Return form with validation errors
    }
    
    // Proceed with validated data
    _context.Tasks.Add(Task);
    await _context.SaveChangesAsync();
    return RedirectToPage("/Index");
}
```

**Data Annotation Validation:**
```csharp
[Required(ErrorMessage = "Title is required")]
[StringLength(200, ErrorMessage = "Title must be under 200 characters")]
public string Title { get; set; }
```

**Validation Benefits:**
- Prevents invalid data from entering database
- Automatic client-side validation via Tag Helpers
- Server-side validation as final defense
- User-friendly error messages

**Tag Helper Validation:**
```razor
<span asp-validation-for="Task.Title" class="text-danger"></span>
```
- Automatically displays validation errors
- Works with data annotations
- Renders client-side validation scripts

### 10.3 SQL Injection Prevention via EF Core

**EF Core Parameterization:**
```csharp
// Safe: EF Core automatically parameterizes queries
var tasks = await _context.Tasks
    .Where(t => t.Title.Contains(searchTerm))
    .ToListAsync();
```

**Generated SQL:**
```sql
SELECT * FROM Tasks WHERE Title LIKE @p0
-- Parameter: @p0 = '%searchTerm%'
```

**Why Safe:**
- All LINQ queries are parameterized
- User input never concatenated into SQL strings
- EF Core handles escaping and sanitization
- No risk of SQL injection

**Unsafe Alternative (DON'T DO THIS):**
```csharp
// DANGEROUS: Raw SQL with string concatenation
var tasks = _context.Tasks
    .FromSqlRaw($"SELECT * FROM Tasks WHERE Title LIKE '%{searchTerm}%'")
    .ToList();
```

**Best Practice:** Always use LINQ queries or parameterized raw SQL if raw SQL is necessary.

### 10.4 Additional Security Measures

#### **HTTPS Enforcement**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // HTTP Strict Transport Security
}
```

#### **Content Security Policy (Future Enhancement)**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; style-src 'self' 'unsafe-inline'");
    await next();
});
```

#### **XSS Prevention**
- Razor automatically HTML-encodes output
- Use `@` syntax: `<h3>@task.Title</h3>` (automatically escaped)
- Avoid `@Html.Raw()` unless absolutely necessary

#### **File Upload Restrictions (if added in future)**
- Validate file types
- Limit file sizes
- Store files outside web root
- Scan for malware

---

## 11. Performance Considerations

### 11.1 SQLite Performance Characteristics

**Strengths:**
- **Fast Read Performance:** In-process database, no network latency
- **Low Memory Footprint:** Minimal overhead compared to client-server databases
- **Zero Configuration:** No server process, no tuning required
- **Efficient for Small Datasets:** Perfect for single-user task management

**Limitations:**
- **Concurrent Writes:** Only one write transaction at a time (not an issue for single-user)
- **No Built-in Replication:** Database file is single point of truth
- **File Locking:** Entire database locked during writes (negligible for small databases)

**Performance Benchmarks (Typical):**
- Read: 10,000+ operations/second
- Write: 1,000+ operations/second
- Database size: Up to 140TB (far exceeds task app needs)

**Optimization Tips:**
- Use indexes on frequently queried columns (IsCompleted, CreatedAt)
- Keep database file on SSD storage
- Use `PRAGMA` statements for tuning (if needed)

### 11.2 EF Core Query Optimization

**Efficient Queries:**
```csharp
// Good: Single database query with filtering
var tasks = await _context.Tasks
    .Where(t => !t.IsCompleted)
    .OrderByDescending(t => t.CreatedAt)
    .ToListAsync();
```

**Avoid N+1 Queries:**
```csharp
// Bad: N+1 query problem (if relationships existed)
foreach (var task in tasks)
{
    // Each iteration hits database
    var relatedData = _context.RelatedData.Where(r => r.TaskId == task.Id).ToList();
}

// Good: Single query with eager loading
var tasks = await _context.Tasks
    .Include(t => t.RelatedData)
    .ToListAsync();
```

**Asynchronous Operations:**
```csharp
// Use async methods for I/O operations
await _context.Tasks.ToListAsync();  // Good
_context.Tasks.ToList();             // Avoid (blocks thread)
```

**Tracking vs. No-Tracking:**
```csharp
// Read-only queries: disable change tracking for better performance
var tasks = await _context.Tasks
    .AsNoTracking()
    .ToListAsync();
```

**Indexes for Performance:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<TaskItem>(entity =>
    {
        entity.HasIndex(e => e.IsCompleted); // Filter queries
        entity.HasIndex(e => e.CreatedAt);   // Sorting queries
    });
}
```

### 11.3 Page Load Optimization with Server-Side Rendering

**Benefits of Server-Side Rendering:**
- **Faster Initial Load:** HTML generated on server, immediate display
- **No JavaScript Parsing:** Browser displays content immediately
- **Better SEO:** Search engines see complete HTML
- **Reduced Client Processing:** No client-side framework overhead

**Optimization Techniques:**

#### **Minimize Database Queries**
```csharp
// Single query per page load
public async Task OnGetAsync()
{
    Tasks = await _context.Tasks
        .AsNoTracking() // No change tracking needed for display
        .Where(/* filter */)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
}
```

#### **Response Caching (if needed)**
```csharp
[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
public class IndexModel : PageModel
{
    // Page cached for 60 seconds
}
```

#### **Static File Caching**
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});
```

#### **Minimize CSS/HTML Size**
- Embed critical CSS in `<style>` tag in _Layout.cshtml
- Minify CSS (if using external files)
- Use system fonts to avoid web font loading

**Expected Performance:**
- Page load time: < 100ms (local)
- Page load time: < 500ms (Fly.io)
- Database query time: < 10ms (SQLite in-process)

---

## 12. Future Extensibility

### 12.1 How the Monolithic Architecture Could Evolve

**Phase 1: MVP (Current)**
- Single-user, monolithic application
- All code in one project
- SQLite database

**Phase 2: Enhanced MVP**
- Add user authentication (ASP.NET Core Identity)
- Multi-tenant single database (UserId column)
- Same monolithic architecture

**Phase 3: Modular Monolith**
- Separate projects within same solution:
  - TaskMaster.Core (domain models)
  - TaskMaster.Data (data access)
  - TaskMaster.Web (Razor Pages)
- Still deploys as single unit
- Better code organization

**Phase 4: Distributed Monolith**
- Extract background jobs to separate process
- Add Redis for caching
- Still monolithic web app
- External services for specific features

**Phase 5: Microservices (if needed)**
- Extract task management to separate service
- API Gateway (BFF pattern)
- Separate databases per service
- Independent scaling

**Key Principle:** Don't prematurely split. Monoliths are easier to develop, test, and deploy. Only split when clear benefits emerge (team scaling, independent deployment cycles, etc.).

### 12.2 Potential Migration Paths if Multi-User Needed

**Option 1: Multi-Tenant Single Database**
```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string UserId { get; set; } // Add user identifier
    public string Title { get; set; }
    // ... other properties
}

// Filter queries by user
var tasks = await _context.Tasks
    .Where(t => t.UserId == currentUserId)
    .ToListAsync();
```

**Steps:**
1. Add ASP.NET Core Identity for authentication
2. Add UserId foreign key to TaskItem
3. Update queries to filter by UserId
4. Add authentication middleware
5. Migrate SQLite → PostgreSQL (better concurrent access)

**Option 2: Database per Tenant**
- Each user gets their own SQLite file
- `tasks_{userId}.db`
- DbContext configured dynamically based on authenticated user
- Better data isolation

**Option 3: Separate Services**
- User Management Service (authentication, profiles)
- Task Management Service (tasks CRUD)
- API Gateway (BFF for frontend)
- Each service has its own database

**Recommended Path:** Start with Option 1 (single database multi-tenant) unless user count exceeds 1,000+ or strict data isolation required.

### 12.3 Clean Architecture Principles Applied

**Current Architecture Already Follows:**

#### **Dependency Inversion**
```csharp
public class IndexModel : PageModel
{
    private readonly AppDbContext _context; // Depends on abstraction (DbContext)
    
    public IndexModel(AppDbContext context)
    {
        _context = context; // Injected by DI container
    }
}
```

#### **Separation of Concerns**
- **Presentation:** Razor Pages handle HTTP concerns
- **Business Logic:** Minimal (in PageModels, could be extracted)
- **Data Access:** EF Core DbContext

#### **Single Responsibility**
- Each PageModel handles one page's logic
- Each entity represents one domain concept
- DbContext manages database operations

**Future Refactoring Opportunities:**

#### **Extract Business Logic to Services**
```csharp
// Before: Logic in PageModel
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();
    Task.CreatedAt = DateTime.UtcNow;
    _context.Tasks.Add(Task);
    await _context.SaveChangesAsync();
    return RedirectToPage("/Index");
}

// After: Logic in service
public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    
    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }
}

// PageModel becomes thin controller
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();
    await _taskService.CreateTaskAsync(Task);
    return RedirectToPage("/Index");
}
```

#### **Repository Pattern (Optional)**
```csharp
public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task CreateAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task DeleteAsync(int id);
}

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    
    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.Tasks.ToListAsync();
    }
    
    // ... other methods
}
```

**When to Refactor:**
- Business logic becomes complex
- Need to swap data source (e.g., API instead of database)
- Unit testing becomes difficult
- Multiple PageModels duplicate logic

**Principle:** Start simple, refactor when complexity demands it. Don't over-engineer the MVP.

---

## 13. Risks & Mitigation

### 13.1 SQLite Limitations for Concurrent Access

**Risk:** SQLite locks entire database during write operations

**Mitigation for Single-User:**
- ✅ Non-issue: Only one user means no concurrent writes
- ✅ Read operations don't block each other
- ✅ Typical write latency < 10ms (imperceptible to user)

**Future Multi-User Mitigation:**
- Migrate to PostgreSQL or SQL Server
- Use connection pooling
- Implement optimistic concurrency with row versioning

**SQLite Write Locking:**
```
User Action → Write Lock → Database Update → Unlock
Duration: ~5-10ms (negligible for single user)
```

### 13.2 Data Backup Strategy on Fly.io

**Risk:** Volume failure or accidental data deletion

**Mitigation Strategy:**

#### **1. Automated Volume Snapshots**
```bash
# Daily automated snapshots
fly volumes snapshots create taskmaster_data --schedule daily
```

#### **2. Manual Backups**
```bash
# SSH into container
fly ssh console

# Copy database file
cat /app/data/tasks.db > /tmp/backup.db

# Download backup
fly ssh sftp get /tmp/backup.db ./backup-$(date +%Y%m%d).db
```

#### **3. Application-Level Export (Future Feature)**
```csharp
// Export all tasks to JSON
public async Task<IActionResult> OnGetExportAsync()
{
    var tasks = await _context.Tasks.ToListAsync();
    var json = JsonSerializer.Serialize(tasks);
    return File(Encoding.UTF8.GetBytes(json), "application/json", "tasks-export.json");
}
```

#### **4. Database Replication (Advanced)**
- Use Litestream for SQLite replication to S3
- Continuous backup stream
- Point-in-time recovery

**Recommended Approach:**
- Start with automated daily snapshots
- Implement manual export feature after MVP
- Consider Litestream for production critical data

### 13.3 Deployment Risks and Rollback Strategy

**Risk: Failed Deployment**

**Causes:**
- Build errors
- Migration failures
- Configuration issues
- Network problems

**Mitigation:**

#### **1. Pre-Deployment Validation**
```bash
# Local build test
dotnet build -c Release

# Local migration test
dotnet ef database update

# Docker build test
docker build -t taskmaster:test .
docker run -p 8080:8080 taskmaster:test
```

#### **2. Fly.io Deployment with Health Checks**
```toml
# fly.toml
[http_service]
  [[http_service.checks]]
    interval = "10s"
    timeout = "2s"
    grace_period = "5s"
    method = "get"
    path = "/"
```

If health check fails, Fly.io automatically rolls back.

#### **3. Manual Rollback**
```bash
# List releases
fly releases

# Output:
# v3  complete  deploy  user@example.com  2025-10-28T10:00:00Z
# v2  complete  deploy  user@example.com  2025-10-27T09:00:00Z
# v1  complete  deploy  user@example.com  2025-10-26T08:00:00Z

# Rollback to previous version
fly releases rollback v2
```

#### **4. Blue-Green Deployment (Advanced)**
```bash
# Deploy to new machine
fly scale count 2

# Shift traffic gradually
fly deploy --strategy rolling

# Monitor for errors
fly logs

# If issues, rollback
fly releases rollback
```

**Risk: Database Migration Failure**

**Mitigation:**
```csharp
// Graceful migration handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
}
catch (Exception ex)
{
    // Log error but don't crash app
    app.Logger.LogError(ex, "Migration failed");
    // App can still run with existing schema if migration is backward-compatible
}
```

**Best Practices:**
- Test migrations locally before deploying
- Make migrations backward-compatible when possible
- Keep database backups before major schema changes
- Use feature flags for new features requiring schema changes

---

## 14. Development Estimates

### 14.1 Breakdown Aligned with 180-Minute Target

**Total Time Budget:** 180 minutes (3 hours)

| Phase | Tasks | Time | Cumulative |
|-------|-------|------|------------|
| **1. Project Setup** | Create project, add NuGet packages, configure tools | 15 min | 15 min |
| **2. Data Layer** | Create entities, DbContext, configure EF Core | 20 min | 35 min |
| **3. Database Migration** | Create and apply initial migration | 10 min | 45 min |
| **4. Index Page** | List view with filtering and delete/toggle | 35 min | 80 min |
| **5. Create Page** | Form with validation and submission | 25 min | 105 min |
| **6. Layout & Styling** | Shared layout, CSS styling | 20 min | 125 min |
| **7. Testing & Fixes** | Manual testing, bug fixes | 20 min | 145 min |
| **8. Dockerfile** | Create multi-stage Dockerfile | 10 min | 155 min |
| **9. Fly.io Setup** | Configure fly.toml, create volume | 10 min | 165 min |
| **10. Deployment** | Deploy and verify production | 15 min | 180 min |

**Contingency:** Built into each phase (~10% buffer)

### 14.2 Phase-by-Phase Implementation Plan

#### **Phase 1: Project Setup (15 minutes)**

**Actions:**
1. Create new Razor Pages project: `dotnet new razor -n TaskMaster`
2. Add NuGet packages (EF Core SQLite, Design, Diagnostics)
3. Install `dotnet ef` CLI tool
4. Create folder structure (Data, Migrations)
5. Initialize Git repository

**Deliverable:** Empty ASP.NET Core project with required dependencies

---

#### **Phase 2: Data Layer (20 minutes)**

**Actions:**
1. Create `Data/Category.cs` enum (Work, Personal, Shopping, Health)
2. Create `Data/TaskItem.cs` entity with properties and data annotations
3. Create `Data/AppDbContext.cs` with Tasks DbSet
4. Update `Program.cs` with DbContext registration
5. Update `appsettings.json` with SQLite connection string

**Deliverable:** Domain models and EF Core configuration complete

---

#### **Phase 3: Database Migration (10 minutes)**

**Actions:**
1. Run `dotnet ef migrations add InitialCreate`
2. Review generated migration files
3. Run `dotnet ef database update`
4. Verify `tasks.db` file created
5. Add auto-migration code to `Program.cs`

**Deliverable:** Database schema created and migration infrastructure working

---

#### **Phase 4: Index Page (35 minutes)**

**Actions:**
1. Create `Pages/Index.cshtml.cs` PageModel
   - Constructor with DbContext injection
   - OnGetAsync with filter logic
   - OnPostToggleAsync handler
   - OnPostDeleteAsync handler
2. Create `Pages/Index.cshtml` view
   - Filter links (All/Active/Done)
   - Task list with foreach loop
   - Toggle and Delete forms for each task
   - "Add New Task" link
3. Test filtering, toggling, deleting

**Deliverable:** Functional task list page with all interactions

---

#### **Phase 5: Create Page (25 minutes)**

**Actions:**
1. Create `Pages/Create.cshtml.cs` PageModel
   - BindProperty for TaskItem
   - OnGet handler (empty)
   - OnPostAsync with validation
2. Create `Pages/Create.cshtml` view
   - Form with Title input
   - Category dropdown with enum
   - DueDate date picker
   - Validation message spans
   - Submit button
3. Test validation (empty title, long title)
4. Test successful creation

**Deliverable:** Functional task creation page with validation

---

#### **Phase 6: Layout & Styling (20 minutes)**

**Actions:**
1. Update `Pages/Shared/_Layout.cshtml`
   - Add header with app title
   - Embed CSS in `<style>` tag
   - Create task-item styling
   - Add completed task styling
   - Style forms and buttons
2. Apply consistent styling across pages
3. Test responsive behavior (basic)

**Deliverable:** Visually appealing UI with embedded CSS

---

#### **Phase 7: Testing & Fixes (20 minutes)**

**Actions:**
1. Run through complete testing checklist
   - Create tasks with various inputs
   - Test all filters
   - Toggle completion status
   - Delete tasks
   - Test validation errors
2. Fix any bugs discovered
3. Verify database persistence (restart app)
4. Clean up code (remove unused using statements, comments)

**Deliverable:** Fully tested, bug-free local application

---

#### **Phase 8: Dockerfile (10 minutes)**

**Actions:**
1. Create `Dockerfile` with multi-stage build
2. Test Docker build locally: `docker build -t taskmaster .`
3. Test Docker run locally: `docker run -p 8080:8080 taskmaster`
4. Verify app works in container
5. Create `.dockerignore` file

**Deliverable:** Working Dockerfile producing deployable image

---

#### **Phase 9: Fly.io Setup (10 minutes)**

**Actions:**
1. Install Fly.io CLI (if not already installed)
2. Run `fly auth login`
3. Run `fly launch` (generate fly.toml)
4. Update `fly.toml` with volume mount configuration
5. Update `appsettings.Production.json` with volume path
6. Create persistent volume: `fly volumes create taskmaster_data --region sea --size 1`

**Deliverable:** Fly.io app configured and ready to deploy

---

#### **Phase 10: Deployment (15 minutes)**

**Actions:**
1. Run `fly deploy`
2. Monitor deployment logs
3. Verify successful deployment
4. Run `fly open` to access app
5. Test all functionality on production
   - Create tasks
   - Filter tasks
   - Toggle completion
   - Delete tasks
6. Verify database persistence (redeploy and check data)

**Deliverable:** Live application on Fly.io with persistent data

---

### 14.3 Risk Buffers

**Time Buffers Built Into Plan:**
- Complex phases (Index Page, Create Page) have +5 min buffer
- Simple phases (Dockerfile, Fly.io Setup) are tight but achievable
- Testing phase can absorb small overruns from earlier phases

**If Running Behind:**
- Simplify CSS (use minimal styling)
- Skip date picker styling
- Use basic filter buttons instead of styled tabs
- Deploy with minimal testing (test in production)

**If Running Ahead:**
- Add task editing feature
- Improve CSS styling
- Add task count indicators
- Implement better error pages

---

## Summary & Key Recommendations

### **Architecture Decisions**

✅ **Monolithic architecture** is the right choice for this MVP
- Simplifies development and deployment
- Reduces operational complexity
- Achieves rapid time-to-market

✅ **Razor Pages over MVC** provides optimal simplicity
- Page-centric model fits CRUD operations
- Less boilerplate than MVC
- Easier for single-developer projects

✅ **SQLite over client-server database** minimizes infrastructure
- Zero configuration
- Perfect for single-user scenarios
- File-based persistence on Fly.io volume

### **Key Technical Highlights**

🔹 **ASP.NET Core 8** provides modern, high-performance foundation
🔹 **Entity Framework Core** enables type-safe, LINQ-based queries
🔹 **Built-in DI and validation** reduce custom code requirements
🔹 **Multi-stage Dockerfile** optimizes image size and build time
🔹 **Fly.io deployment** achieves single-command deployment with persistence

### **Success Factors**

1. **Simplicity:** Every technology choice prioritizes simplicity over flexibility
2. **Speed:** Entire stack optimized for rapid development (< 180 min target)
3. **Maintainability:** Clean separation of concerns enables future enhancements
4. **Deployability:** Single container, single volume, single command deployment

### **Future Considerations**

- Authentication can be added via ASP.NET Core Identity
- Multi-user support via UserId foreign key and query filtering
- PostgreSQL migration if concurrent access needed
- Service layer extraction if business logic grows
- API addition if mobile/SPA frontend needed

### **Recommended Next Steps**

1. **Review this architecture plan** with stakeholders
2. **Proceed with implementation** following phase-by-phase plan
3. **Track time per phase** to validate estimates
4. **Deploy early and often** to catch integration issues
5. **Gather user feedback** immediately after deployment

---

**This architecture plan provides a comprehensive blueprint for building TaskMaster as a production-ready MVP within the 180-minute target timeframe.**