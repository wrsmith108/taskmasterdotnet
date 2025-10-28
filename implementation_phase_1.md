# TaskMaster - Phase 1: Local Build Implementation

**Phase:** 1 of 3  
**Goal:** Complete working local application with all core features  
**Architecture:** Monolithic Razor Pages with SQLite  
**Total Estimated Effort:** 165 minutes (2.75 hours)  
**Last Updated:** October 28, 2025

---

## 📋 Navigation

- 📊 [Progress Tracker](./progress_tracker.md) - Overall project status
- 📐 [Software Architecture Plan](./TaskMaster-Software-Architecture-Plan.md) - Complete architecture details
- 📝 [Product Requirements](./Taskmaster%20PRD%20dotnet.md) - Feature specifications

---

## 🎯 Phase 1 Overview

This phase focuses on building a complete, working local application following the monolithic Razor Pages architecture. Upon completion, the application will:

- Run locally with `dotnet run`
- Support full CRUD operations for tasks
- Include filtering (All/Active/Done)
- Persist data in SQLite database
- Be containerized with Docker
- Be ready for Fly.io deployment (Phase 2)

**Success Criteria:**
- ✅ All features from PRD working locally
- ✅ Data persists across restarts
- ✅ Validation prevents invalid data
- ✅ Docker image builds and runs successfully
- ✅ Zero external dependencies beyond .NET ecosystem

---

## 📊 Effort Estimation Guide

| Size | Token Budget | Typical Tasks | Guidelines |
|------|--------------|---------------|------------|
| **XS** | ~50k tokens | Bug fix, config change, simple implementation | Single file, clear scope |
| **S** | ~100k tokens | Feature implementation, small test suite | 2-3 files, straightforward logic |
| **M** | ~200k tokens | Large test suite, complex feature | Multiple files, integration work |
| **L** | ~400k tokens | Full module implementation | Major component, requires breakdown |
| **XL** | >400k tokens | **MUST BREAK DOWN** | Split into M or smaller tasks |

---

## 📦 Task Breakdown with Effort Estimates

### 1.1 Project Foundation & Setup
**Total Estimated Time:** 15 minutes

#### Task 1.1.1: Create Project Structure
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] New Razor Pages project created with `dotnet new razor -n TaskMaster`
- [ ] Solution structure verified
- [ ] Default template files present
- [ ] Project compiles without errors

**Commands:**
```bash
dotnet new razor -n TaskMaster
cd TaskMaster
dotnet build
```

**Deliverables:**
- TaskMaster.csproj
- Program.cs (template)
- Pages/ directory with default pages

---

#### Task 1.1.2: Add NuGet Packages
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] `Microsoft.EntityFrameworkCore.Sqlite` (8.0.0) installed
- [ ] `Microsoft.EntityFrameworkCore.Design` (8.0.0) installed
- [ ] `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` (8.0.0) installed
- [ ] Packages restored successfully
- [ ] No package conflicts

**Commands:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore --version 8.0.0
dotnet restore
```

**Verification:**
```bash
dotnet list package
```

---

#### Task 1.1.3: Install EF Core Tools & Initialize Git
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] `dotnet-ef` CLI tool installed globally
- [ ] Git repository initialized
- [ ] .gitignore configured for .NET projects
- [ ] Initial commit created

**Commands:**
```bash
dotnet tool install --global dotnet-ef
git init
dotnet new gitignore
git add .
git commit -m "Initial project setup"
```

**Verification:**
```bash
dotnet ef --version
git status
```

---

### 1.2 Data Layer Implementation
**Total Estimated Time:** 20 minutes

#### Task 1.2.1: Create Domain Models
**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**
- [ ] `Data/Category.cs` enum created with Work, Personal, Shopping, Health
- [ ] `Data/TaskItem.cs` entity created with all properties
- [ ] Data annotations applied ([Required], [StringLength])
- [ ] Properties correctly typed (int, string, DateTime?, bool)
- [ ] Default values set (CreatedAt, IsCompleted)

**Files to Create:**

**Data/Category.cs:**
```csharp
namespace TaskMaster.Data
{
    public enum Category
    {
        Work,
        Personal,
        Shopping,
        Health
    }
}
```

**Data/TaskItem.cs:**
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

**Verification:**
- Compile project: `dotnet build`
- No build errors

---

#### Task 1.2.2: Create DbContext
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] `Data/AppDbContext.cs` created
- [ ] Inherits from DbContext
- [ ] Constructor accepts DbContextOptions<AppDbContext>
- [ ] Tasks DbSet property defined
- [ ] Project compiles

**File to Create:**

**Data/AppDbContext.cs:**
```csharp
using Microsoft.EntityFrameworkCore;

namespace TaskMaster.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<TaskItem> Tasks { get; set; }
    }
}
```

**Verification:**
```bash
dotnet build
```

---

#### Task 1.2.3: Configure Services & Connection String
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] DbContext registered in Program.cs with SQLite
- [ ] Connection string added to appsettings.json
- [ ] Database developer page exception filter added
- [ ] Services configured correctly

**Files to Modify:**

**Program.cs** (add after builder creation):
```csharp
using TaskMaster.Data;
using Microsoft.EntityFrameworkCore;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
```

**appsettings.json** (add ConnectionStrings section):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tasks.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Verification:**
```bash
dotnet build
```

---

### 1.3 Database Migration
**Total Estimated Time:** 10 minutes

#### Task 1.3.1: Generate & Apply Initial Migration
**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**
- [ ] Initial migration generated successfully
- [ ] Migration files created in Migrations/ folder
- [ ] Migration applied to database
- [ ] tasks.db file created in project root
- [ ] Database schema verified
- [ ] Auto-migration code added to Program.cs for development

**Commands:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Program.cs Addition** (add before app.Run()):
```csharp
// Auto-apply migrations in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}
```

**Verification:**
- Check Migrations/ folder contains migration files
- Check tasks.db exists in project root
- Query database: `dotnet ef database update --verbose`

**Deliverables:**
- Migrations/[timestamp]_InitialCreate.cs
- Migrations/AppDbContextModelSnapshot.cs
- tasks.db

---

### 1.4 Index Page - Task List View
**Total Estimated Time:** 35 minutes

#### Task 1.4.1: Create Index PageModel Backend
**Effort:** M (~200k tokens) | **Time:** 15 minutes

**Acceptance Criteria:**
- [ ] IndexModel class created in Pages/Index.cshtml.cs
- [ ] DbContext injected via constructor
- [ ] Tasks property for binding data
- [ ] Filter property with query string binding
- [ ] OnGetAsync implemented with filtering logic
- [ ] OnPostToggleAsync handler implemented
- [ ] OnPostDeleteAsync handler implemented
- [ ] All handlers return correct IActionResult

**File to Modify:**

**Pages/Index.cshtml.cs:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TaskMaster.Data;

namespace TaskMaster.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }
        
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        
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
            return RedirectToPage(new { Filter });
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { Filter });
        }
    }
}
```

**Verification:**
```bash
dotnet build
```

---

#### Task 1.4.2: Create Index View Frontend
**Effort:** M (~200k tokens) | **Time:** 15 minutes

**Acceptance Criteria:**
- [ ] Page directive and model binding configured
- [ ] Filter navigation links created (All/Active/Done)
- [ ] Task list rendered with foreach loop
- [ ] Task properties displayed (title, category, due date, status)
- [ ] Toggle form added for each task
- [ ] Delete form added for each task
- [ ] "Create New Task" link added
- [ ] Conditional CSS classes for completed tasks

**File to Modify:**

**Pages/Index.cshtml:**
```razor
@page
@model IndexModel
@{
    ViewData["Title"] = "My Tasks";
}

<div class="header">
    <h1>TaskMaster</h1>
    <a asp-page="/Create" class="btn btn-primary">+ Add New Task</a>
</div>

<div class="filters">
    <a asp-page="/Index" asp-route-filter="All" class="filter-link @(Model.Filter == "All" ? "active" : "")">All</a>
    <a asp-page="/Index" asp-route-filter="Active" class="filter-link @(Model.Filter == "Active" ? "active" : "")">Active</a>
    <a asp-page="/Index" asp-route-filter="Done" class="filter-link @(Model.Filter == "Done" ? "active" : "")">Done</a>
</div>

@if (!Model.Tasks.Any())
{
    <div class="empty-state">
        <p>No tasks found. <a asp-page="/Create">Create your first task</a></p>
    </div>
}
else
{
    <div class="task-list">
        @foreach (var task in Model.Tasks)
        {
            <div class="task-item @(task.IsCompleted ? "completed" : "")">
                <div class="task-content">
                    <h3>@task.Title</h3>
                    <div class="task-meta">
                        <span class="category">@task.Category</span>
                        @if (task.DueDate.HasValue)
                        {
                            <span class="due-date">Due: @task.DueDate.Value.ToString("MMM dd, yyyy")</span>
                        }
                    </div>
                </div>
                <div class="task-actions">
                    <form method="post" asp-page-handler="Toggle" asp-route-id="@task.Id" style="display: inline;">
                        <button type="submit" class="btn btn-secondary">
                            @(task.IsCompleted ? "Undo" : "Complete")
                        </button>
                    </form>
                    <form method="post" asp-page-handler="Delete" asp-route-id="@task.Id" style="display: inline;">
                        <button type="submit" class="btn btn-danger" onclick="return confirm('Delete this task?');">
                            Delete
                        </button>
                    </form>
                </div>
            </div>
        }
    </div>
}
```

**Verification:**
- Run app: `dotnet run`
- Navigate to http://localhost:5000
- Page renders without errors (may be empty)

---

#### Task 1.4.3: Test Index Page Functionality
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] Page displays with no tasks (empty state)
- [ ] Filter links are clickable and maintain state
- [ ] No console errors
- [ ] Page styling renders correctly

**Manual Testing Steps:**
1. Run `dotnet run`
2. Open http://localhost:5000
3. Verify empty state message
4. Click filter links (All/Active/Done)
5. Verify URL parameter changes (?filter=Active)

---

### 1.5 Create Page - Task Creation
**Total Estimated Time:** 25 minutes

#### Task 1.5.1: Create PageModel Backend
**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**
- [ ] CreateModel class created in Pages/Create.cshtml.cs
- [ ] DbContext injected via constructor
- [ ] TaskItem property with BindProperty attribute
- [ ] OnGet handler implemented
- [ ] OnPostAsync with validation implemented
- [ ] CreatedAt timestamp set on save
- [ ] Redirect to Index on success

**File to Create:**

**Pages/Create.cshtml.cs:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskMaster.Data;

namespace TaskMaster.Pages
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        
        public CreateModel(AppDbContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public TaskItem Task { get; set; } = new TaskItem();
        
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
}
```

**Verification:**
```bash
dotnet build
```

---

#### Task 1.5.2: Create Form View Frontend
**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**
- [ ] Page directive and model binding configured
- [ ] Form with method="post" created
- [ ] Title input with validation
- [ ] Category dropdown with enum binding
- [ ] DueDate input (type="date")
- [ ] Validation spans for each field
- [ ] Validation summary added
- [ ] Submit and Cancel buttons

**File to Create:**

**Pages/Create.cshtml:**
```razor
@page
@model CreateModel
@{
    ViewData["Title"] = "Create Task";
}

<div class="header">
    <h1>Create New Task</h1>
    <a asp-page="/Index" class="btn btn-secondary">← Back to Tasks</a>
</div>

<div class="form-container">
    <form method="post">
        <div asp-validation-summary="All" class="validation-summary"></div>
        
        <div class="form-group">
            <label asp-for="Task.Title"></label>
            <input asp-for="Task.Title" class="form-control" placeholder="Enter task title" />
            <span asp-validation-for="Task.Title" class="validation-error"></span>
        </div>
        
        <div class="form-group">
            <label asp-for="Task.Category"></label>
            <select asp-for="Task.Category" asp-items="Html.GetEnumSelectList<Category>()" class="form-control">
                <option value="">-- Select Category --</option>
            </select>
            <span asp-validation-for="Task.Category" class="validation-error"></span>
        </div>
        
        <div class="form-group">
            <label asp-for="Task.DueDate"></label>
            <input asp-for="Task.DueDate" type="date" class="form-control" />
            <span asp-validation-for="Task.DueDate" class="validation-error"></span>
        </div>
        
        <div class="form-actions">
            <button type="submit" class="btn btn-primary">Create Task</button>
            <a asp-page="/Index" class="btn btn-secondary">Cancel</a>
        </div>
    </form>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

**Verification:**
- Run app: `dotnet run`
- Navigate to http://localhost:5000/Create
- Form renders without errors

---

#### Task 1.5.3: Test Create Page Functionality
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] Form displays correctly
- [ ] Required field validation triggers
- [ ] Category dropdown populates
- [ ] Date picker works
- [ ] Successful submission redirects to Index
- [ ] Created task appears in task list

**Manual Testing Steps:**
1. Navigate to Create page
2. Try submitting empty form (should show validation)
3. Enter title longer than 200 chars (should show validation)
4. Fill valid data and submit
5. Verify redirect to Index
6. Verify new task appears in list

---

### 1.6 Layout & Styling
**Total Estimated Time:** 20 minutes

#### Task 1.6.1: Create Layout and Embed CSS
**Effort:** M (~200k tokens) | **Time:** 20 minutes

**Acceptance Criteria:**
- [ ] _Layout.cshtml updated with app structure
- [ ] Complete CSS embedded in <style> tag
- [ ] Task list styling implemented
- [ ] Completed task styling (strikethrough, opacity)
- [ ] Form styling implemented
- [ ] Button styling (primary, secondary, danger)
- [ ] Filter navigation styling
- [ ] Basic responsive styles
- [ ] Consistent styling across all pages

**File to Modify:**

**Pages/Shared/_Layout.cshtml:**
```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - TaskMaster</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            line-height: 1.6;
            color: #333;
            background: #f5f5f5;
            padding: 20px;
        }
        
        .container {
            max-width: 800px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 2px solid #e0e0e0;
        }
        
        h1 {
            color: #2c3e50;
            font-size: 2em;
        }
        
        .filters {
            display: flex;
            gap: 10px;
            margin-bottom: 30px;
        }
        
        .filter-link {
            padding: 8px 16px;
            text-decoration: none;
            color: #666;
            border: 1px solid #ddd;
            border-radius: 4px;
            transition: all 0.3s;
        }
        
        .filter-link:hover {
            background: #f0f0f0;
        }
        
        .filter-link.active {
            background: #3498db;
            color: white;
            border-color: #3498db;
        }
        
        .task-list {
            display: flex;
            flex-direction: column;
            gap: 15px;
        }
        
        .task-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 20px;
            border: 1px solid #e0e0e0;
            border-radius: 6px;
            transition: all 0.3s;
        }
        
        .task-item:hover {
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        
        .task-item.completed {
            opacity: 0.6;
            background: #f9f9f9;
        }
        
        .task-item.completed h3 {
            text-decoration: line-through;
        }
        
        .task-content {
            flex: 1;
        }
        
        .task-content h3 {
            margin-bottom: 8px;
            color: #2c3e50;
        }
        
        .task-meta {
            display: flex;
            gap: 15px;
            font-size: 0.9em;
            color: #666;
        }
        
        .category {
            background: #3498db;
            color: white;
            padding: 3px 10px;
            border-radius: 3px;
            font-size: 0.85em;
        }
        
        .due-date {
            color: #e74c3c;
        }
        
        .task-actions {
            display: flex;
            gap: 10px;
        }
        
        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            text-decoration: none;
            font-size: 0.9em;
            transition: all 0.3s;
            display: inline-block;
        }
        
        .btn-primary {
            background: #3498db;
            color: white;
        }
        
        .btn-primary:hover {
            background: #2980b9;
        }
        
        .btn-secondary {
            background: #95a5a6;
            color: white;
        }
        
        .btn-secondary:hover {
            background: #7f8c8d;
        }
        
        .btn-danger {
            background: #e74c3c;
            color: white;
        }
        
        .btn-danger:hover {
            background: #c0392b;
        }
        
        .empty-state {
            text-align: center;
            padding: 60px 20px;
            color: #999;
        }
        
        .empty-state p {
            font-size: 1.2em;
            margin-bottom: 20px;
        }
        
        .form-container {
            max-width: 600px;
            margin: 0 auto;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 500;
            color: #2c3e50;
        }
        
        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 1em;
        }
        
        .form-control:focus {
            outline: none;
            border-color: #3498db;
        }
        
        .validation-error {
            display: block;
            color: #e74c3c;
            font-size: 0.9em;
            margin-top: 5px;
        }
        
        .validation-summary {
            background: #fee;
            border: 1px solid #e74c3c;
            padding: 15px;
            border-radius: 4px;
            margin-bottom: 20px;
            color: #c0392b;
        }
        
        .form-actions {
            display: flex;
            gap: 10px;
            margin-top: 30px;
        }
        
        @media (max-width: 600px) {
            body {
                padding: 10px;
            }
            
            .container {
                padding: 20px;
            }
            
            .header {
                flex-direction: column;
                align-items: flex-start;
                gap: 15px;
            }
            
            .task-item {
                flex-direction: column;
                align-items: flex-start;
            }
            
            .task-actions {
                width: 100%;
                margin-top: 15px;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        @RenderBody()
    </div>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

**Verification:**
- Run app: `dotnet run`
- Check both Index and Create pages
- Verify styling consistency
- Test responsive behavior (resize browser)

---

### 1.7 Testing & Bug Fixes
**Total Estimated Time:** 20 minutes

#### Task 1.7.1: Complete Functionality Testing
**Effort:** M (~200k tokens) | **Time:** 15 minutes

**Acceptance Criteria - All Must Pass:**

**Task Creation:**
- [ ] Create task with title only
- [ ] Create task with title and category
- [ ] Create task with title, category, and due date
- [ ] Create task with all fields
- [ ] Empty title shows validation error
- [ ] Title >200 characters shows validation error
- [ ] Missing category shows validation error

**Task Display:**
- [ ] All tasks display correctly
- [ ] Task title renders correctly
- [ ] Category displays correctly
- [ ] Due date formats correctly (MMM dd, yyyy)
- [ ] Tasks without due date don't show date field

**Filtering:**
- [ ] "All" filter shows all tasks
- [ ] "Active" filter shows only incomplete tasks
- [ ] "Done" filter shows only completed tasks
- [ ] Filter state persists after actions
- [ ] Active filter link highlights correctly

**Task Operations:**
- [ ] Toggle completion (Active → Done)
- [ ] Toggle completion (Done → Active)
- [ ] Completed tasks show strikethrough
- [ ] Completed tasks have reduced opacity
- [ ] Delete task removes from database
- [ ] Delete confirmation dialog appears

**Data Persistence:**
- [ ] Create multiple tasks
- [ ] Stop application (Ctrl+C)
- [ ] Restart application
- [ ] Verify all tasks still exist
- [ ] Verify completion status persisted

**Manual Testing Checklist:**
```
1. Start fresh: Delete tasks.db, run `dotnet run`
2. Create task: "Buy groceries" (Shopping, no date)
3. Create task: "Team meeting" (Work, tomorrow's date)
4. Create task: "Gym workout" (Health, today's date)
5. Create task: "Pay bills" (Personal, next week)
6. Verify all 4 tasks display
7. Toggle "Buy groceries" to complete
8. Filter by "Active" → should see 3 tasks
9. Filter by "Done" → should see 1 task
10. Filter by "All" → should see 4 tasks
11. Delete "Team meeting"
12. Verify only 3 tasks remain
13. Stop and restart application
14. Verify 3 tasks still exist with correct states
```

---

#### Task 1.7.2: Fix Bugs & Polish Code
**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**
- [ ] All discovered bugs fixed
- [ ] Unused using statements removed
- [ ] Code formatted consistently
- [ ] Comments added where needed
- [ ] No compiler warnings

**Common Issues to Check:**
- Null reference exceptions
- DateTime formatting issues
- Form validation not triggering
- Redirect not preserving filter
- CSS class naming conflicts

**Commands:**
```bash
dotnet clean
dotnet build
# Check for warnings
```

---

### 1.8 Local Docker Build
**Total Estimated Time:** 10 minutes

#### Task 1.8.1: Create Dockerfile and Build Image
**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**
- [ ] Dockerfile created with multi-stage build
- [ ] .dockerignore file created
- [ ] Docker image builds successfully
- [ ] Image size is reasonable (<200MB)
- [ ] Container runs successfully
- [ ] Application accessible in container
- [ ] Database persists in container

**File to Create:**

**Dockerfile:**
```dockerfile
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskMaster.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build "TaskMaster.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "TaskMaster.csproj" -c Release -o /app/publish

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskMaster.dll"]
```

**.dockerignore:**
```
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.vs
**/.vscode
**/*.*proj.user
**/bin
**/obj
**/out
**/tasks.db
**/Dockerfile*
**/docker-compose*
README.md
```

**Commands:**
```bash
# Build image
docker build -t taskmaster:local .

# Run container
docker run -d -p 8080:8080 --name taskmaster-test taskmaster:local

# Check logs
docker logs taskmaster-test

# Test application
curl http://localhost:8080

# Stop and remove
docker stop taskmaster-test
docker rm taskmaster-test
```

**Verification:**
- [ ] Build completes without errors
- [ ] Container starts successfully
- [ ] Can access http://localhost:8080
- [ ] Can create and view tasks
- [ ] Database persists while container runs

---

### 1.9 Documentation & Handoff
**Total Estimated Time:** 10 minutes

#### Task 1.9.1: Update Documentation & Prepare for Phase 2
**Effort:** XS (~50k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**
- [ ] progress_tracker.md updated with actual times
- [ ] All Phase 1 checkboxes marked complete
- [ ] Technical debt documented
- [ ] Phase 2 prerequisites verified
- [ ] Handoff notes created

**Files to Update:**
- progress_tracker.md - Mark Phase 1 tasks complete
- Create PHASE1_COMPLETE.md with handoff notes

**PHASE1_COMPLETE.md Template:**
```markdown
# Phase 1 Completion Report

**Completion Date:** [Date]
**Total Time:** [Actual] minutes (Estimated: 165 minutes)

## Deliverables Completed
- [x] Functional Razor Pages application
- [x] SQLite database with EF Core
- [x] Full CRUD operations
- [x] Filtering (All/Active/Done)
- [x] Data validation
- [x] Docker image

## Deviations from Plan
- [List any changes]

## Technical Debt
- [List items to address later]

## Known Issues
- [List any known issues]

## Phase 2 Prerequisites Met
- [x] Application runs locally
- [x] Docker image builds
- [x] All tests passing
- [x] Database persists

## Phase 2 Readiness
Status: READY / NOT READY

## Notes for Phase 2
- [Any important notes for deployment]
```

---

## 📊 Phase 1 Summary

### Total Effort by Category

| Category | Tasks | Estimated Time | Token Budget |
|----------|-------|----------------|--------------|
| Project Setup | 3 | 15 min | ~150k tokens |
| Data Layer | 3 | 20 min | ~150k tokens |
| Database | 1 | 10 min | ~100k tokens |
| Index Page | 3 | 35 min | ~450k tokens |
| Create Page | 3 | 25 min | ~250k tokens |
| Styling | 1 | 20 min | ~200k tokens |
| Testing | 2 | 20 min | ~250k tokens |
| Docker | 1 | 10 min | ~100k tokens |
| Documentation | 1 | 10 min | ~50k tokens |
| **TOTAL** | **18** | **165 min** | **~1.7M tokens** |

### Task Size Distribution

- **XS Tasks (50k):** 8 tasks → 400k tokens
- **S Tasks (100k):** 6 tasks → 600k tokens
- **M Tasks (200k):** 4 tasks → 800k tokens
- **L Tasks (400k):** 0 tasks (all broken down)

### Critical Path
1. Project Setup → Data Layer → Database Migration (Foundation)
2. Index Page (Core functionality)
3. Create Page (User input)
4. Testing (Quality assurance)
5. Docker (Deployment preparation)

---

## 🚀 Getting Started with Phase 1

**Prerequisites:**
- .NET 8 SDK installed
- Docker Desktop installed (for Task 1.8)
- Git installed
- Code editor (VS Code, Visual Studio, or Rider)

**Start Here:**
```bash
# 1. Begin with Task 1.1.1
dotnet new razor -n TaskMaster
cd TaskMaster

# 2. Follow tasks in order
# 3. Update progress_tracker.md after each major task
# 4. Test incrementally, don't wait until the end
```

**Success Tips:**
- ✅ Build and test after each task
- ✅ Commit code after completing each section
- ✅ Take breaks between major sections
- ✅ Document issues as you encounter them
- ✅ Track actual time vs estimated time

---

## 🔗 Quick Links

- [Back to Progress Tracker](./progress_tracker.md)
- [Architecture Plan](./TaskMaster-Software-Architecture-Plan.md)
- [Product Requirements](./Taskmaster%20PRD%20dotnet.md)

---

**Ready to begin?** Start with [Task 1.1.1: Create Project Structure](#task-111-create-project-structure)