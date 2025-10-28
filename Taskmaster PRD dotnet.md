# **TaskMaster PRD \- .NET Focused**

**v3.0 | Oct 25, 2025 | Single-Project Razor Pages**

## **Problem & Solution**

**Problem:** Demonstrate C\#/.NET agentic engineering principles for an MVP Task Manager product with minimal deployment complexity.

**Solution:** Single-user task app using Razor Pages, EF Core, SQLite. All C\#/.NET \- no separate frontend, no external services.

**Success:** Working app deployable to fly.io in \< 180 minutes

---

## **Scope**

**In:** View tasks, Create task (title/category/due date), Toggle complete, Delete, Filter (All/Active/Done)

**Out:** Edit, descriptions, search, sort, multiple categories, responsive breakpoints, auth, priorities, recurring

**Why Lean:** Focus on .NET features (Razor Pages, EF Core, Tag Helpers, Model Binding), not feature complexity.

---

## **Stack**

**Single Project:** ASP.NET Core 8 Razor Pages  
**Database:** SQLite \+ EF Core (file-based, no external service)  
**Frontend:** Server-rendered HTML \+ HTMX (no build step)  
**Deployment:** fly.io (single container)

Browser → Razor Pages (C\#) → EF Core → SQLite (tasks.db)

**No JavaScript framework, no npm, no React, no external DB.**

---

## **Data Model**

public class TaskItem

{

    public int Id { get; set; }

    \[Required(ErrorMessage \= "Title is required")\]

    \[StringLength(200, ErrorMessage \= "Title must be under 200 characters")\]

    public string Title { get; set; }

    \[Required\]

    public Category Category { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } \= DateTime.UtcNow;

}

public enum Category

{

    Work,

    Personal,

    Shopping,

    Health

}

**Database:** SQLite file `tasks.db` in app directory, created automatically by EF Core migrations.

---

## **Database Setup**

**DbContext:**

public class AppDbContext : DbContext

{

    public AppDbContext(DbContextOptions\<AppDbContext\> options) : base(options) { }

    public DbSet\<TaskItem\> Tasks { get; set; }

}

**Program.cs Configuration:**

builder.Services.AddDbContext\<AppDbContext\>(options \=\>

    options.UseSqlite("Data Source=tasks.db"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();

**Migration Commands:**

dotnet ef migrations add InitialCreate

dotnet ef database update

**Deploy:** SQLite file deploys with app, persists in fly.io volume.

---

## **.NET Features Demonstrated**

### **1\. Razor Pages**

* **PageModel pattern** \- C\# code-behind for each page  
* **Handler methods** \- OnGet, OnPost, OnPostToggle, OnPostDelete  
* **Model binding** \- `[BindProperty]` automatic form → C\# object  
* **Query string binding** \- `[BindProperty(SupportsGet = true)]` for filters

### **2\. Tag Helpers**

* **Forms:** `<form method="post">` with anti-forgery tokens  
* **Inputs:** `<input asp-for="Task.Title" />` with validation  
* **Validation:** `<span asp-validation-for="Task.Title" />`  
* **Links:** `<a asp-page="/Create">Add Task</a>`  
* **Select:** `<select asp-for="Task.Category" asp-items="Html.GetEnumSelectList<Category>()"/>`

### **3\. Entity Framework Core**

* **DbContext** \- Database abstraction  
* **Migrations** \- Schema versioning  
* **LINQ** \- Type-safe queries  
* **Change tracking** \- Automatic updates

### **4\. Model Validation**

* **Data Annotations** \- \[Required\], \[StringLength\]  
* **ModelState.IsValid** \- Server-side validation  
* **asp-validation-for** \- Client-side validation  
* **Enum validation** \- Category dropdown

### **5\. Dependency Injection**

* **DbContext injection** \- Constructor injection in PageModel  
* **Service lifetime** \- Scoped DbContext per request  
* **Configuration** \- Settings from appsettings.json

---

## **File Structure**

TaskMaster/

├── Pages/

│   ├── Shared/

│   │   └── \_Layout.cshtml          \# Layout with CSS

│   ├── Index.cshtml                \# List view

│   ├── Index.cshtml.cs             \# List logic

│   ├── Create.cshtml               \# Create form

│   └── Create.cshtml.cs            \# Create logic

├── Data/

│   ├── AppDbContext.cs             \# EF Core context

│   └── TaskItem.cs                 \# Model

├── Migrations/                     \# EF Core migrations

├── wwwroot/                        \# Static files (minimal)

├── Program.cs                      \# App configuration

├── appsettings.json                \# Configuration

├── TaskMaster.csproj               \# Project file

└── tasks.db                        \# SQLite database (created at runtime)

---

## **Deployment to fly.io**

**Dockerfile:**

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY \["TaskMaster.csproj", "."\]

RUN dotnet restore

COPY . .

RUN dotnet build \-c Release \-o /app/build

FROM build AS publish

RUN dotnet publish \-c Release \-o /app/publish

FROM base AS final

WORKDIR /app

COPY \--from=publish /app/publish .

ENTRYPOINT \["dotnet", "TaskMaster.dll"\]

---

## **Testing Checklist**

**Functionality:**

* \[ \] View all tasks  
* \[ \] Filter by All/Active/Done  
* \[ \] Create task with title only  
* \[ \] Create task with category  
* \[ \] Create task with due date  
* \[ \] Toggle task completion  
* \[ \] Delete task  
* \[ \] Validation: Empty title shows error  
* \[ \] Validation: Title \>200 chars shows error

**Deployment:**

* \[ \] Runs locally with `dotnet run`  
* \[ \] Database persists after restart  
* \[ \] Deploys to fly.io  
* \[ \] Database persists in fly.io volume  
* \[ \] App accessible via fly.io URL

---

## **Summary**

**Pure .NET Stack:**

* Razor Pages (server-side C\#)  
* EF Core (ORM)  
* SQLite (embedded DB)  
* Tag Helpers (form generation)  
* Model Binding (automatic)  
* Validation (data annotations)

**No External Dependencies:**

* No JavaScript framework  
* No build tools  
* No external database service  
* No API contracts

**Single Command Deploy:**

fly deploy

**Purpose:** Demonstrate .NET web development principles with minimal complexity.