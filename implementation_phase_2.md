# TaskMaster - Phase 2: Fly.io Production Deployment

**Phase:** 2 of 3  
**Goal:** Deploy monolithic application to production on Fly.io  
**Architecture:** Monolithic Razor Pages with SQLite + Persistent Volume  
**Total Estimated Effort:** 75 minutes (1.25 hours)  
**Last Updated:** October 28, 2025

---

## 📋 Navigation

- 📊 [Progress Tracker](./progress_tracker.md) - Overall project status
- 📝 [Phase 1 Complete](./TaskMaster/PHASE1_COMPLETE.md) - Phase 1 completion report
- 📐 [Software Architecture Plan](./TaskMaster-Software-Architecture-Plan.md) - Complete architecture details
- 📋 [Implementation Phase 1](./implementation_phase_1.md) - Local build documentation

---

## 🎯 Phase 2 Overview

This phase focuses on deploying the completed Phase 1 application to production on Fly.io. Upon completion, the application will:

- Be accessible via a public Fly.io URL
- Run in a production environment with proper configuration
- Persist data across deployments using Fly.io volumes
- Include production-grade security and configuration
- Have monitoring and logging capabilities
- Support CI/CD for future updates

**Success Criteria:**

- ✅ Application deployed to Fly.io
- ✅ Accessible via public URL (https://\*.fly.dev)
- ✅ Database persists across deployments and restarts
- ✅ All CRUD operations work in production
- ✅ Environment configuration properly set
- ✅ Health checks configured
- ✅ Logs accessible via Fly.io CLI

---

## 📊 Effort Estimation Guide

| Size   | Token Budget | Typical Tasks                  | Guidelines                          |
| ------ | ------------ | ------------------------------ | ----------------------------------- |
| **XS** | ~50k tokens  | Config change, simple setup    | Single file, clear scope            |
| **S**  | ~100k tokens | Service setup, configuration   | 2-3 files, straightforward          |
| **M**  | ~200k tokens | Complex configuration, testing | Multiple steps, integration work    |
| **L**  | ~400k tokens | Full deployment workflow       | Major component, requires breakdown |

---

## 📦 Task Breakdown with Effort Estimates

### 2.1 Production Configuration & Security

**Total Estimated Time:** 20 minutes

#### Task 2.1.1: Update Application for Production

**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**

- [ ] Production connection string configuration updated
- [ ] Environment-specific appsettings created
- [ ] HTTPS redirection configured
- [ ] HSTS headers configured
- [ ] Security headers added
- [ ] Production logging level configured

**Files to Create/Modify:**

**appsettings.Production.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/data/tasks.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Program.cs** (add security middleware):

```csharp
// After builder.Build()
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
```

**Verification:**

```bash
dotnet build -c Release
```

---

#### Task 2.1.2: Add Health Check Endpoint

**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**

- [ ] Health check services registered
- [ ] Database health check configured
- [ ] `/health` endpoint exposed
- [ ] Health check responds with 200 OK when healthy

**Program.cs modifications:**

```csharp
// Add after AddDbContext
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// Add before app.Run()
app.MapHealthChecks("/health");
```

**Verification:**

```bash
dotnet run
# In another terminal:
curl http://localhost:5165/health
# Should return: Healthy
```

---

#### Task 2.1.3: Optimize Dockerfile for Production

**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**

- [ ] Dockerfile updated with volume mount point
- [ ] Production environment variables set
- [ ] Non-root user configured
- [ ] Database directory configured
- [ ] Optimized layer caching

**TaskMaster/Dockerfile** (update final stage):

```dockerfile
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Stage 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskMaster.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build "TaskMaster.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "TaskMaster.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Create data directory for SQLite database
RUN mkdir -p /data && chmod 777 /data

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TaskMaster.dll"]
```

**Verification:**

```bash
cd TaskMaster
docker build -t taskmaster:production .
docker run -d -p 8080:8080 -v taskmaster_data:/data --name taskmaster-prod taskmaster:production
# Test application
curl http://localhost:8080/health
# Cleanup
docker stop taskmaster-prod && docker rm taskmaster-prod
```

---

### 2.2 Fly.io Setup & Configuration

**Total Estimated Time:** 20 minutes

#### Task 2.2.1: Install Fly.io CLI and Authenticate

**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**

- [ ] Fly.io CLI installed
- [ ] Successfully authenticated with Fly.io account
- [ ] Account verified with `flyctl auth whoami`
- [ ] Organization selected (if applicable)

**Commands:**

```bash
# Install Fly.io CLI (Linux/macOS)
curl -L https://fly.io/install.sh | sh

# Or using package manager
# macOS: brew install flyctl
# Linux: See https://fly.io/docs/hands-on/install-flyctl/

# Add to PATH if needed
export FLYCTL_INSTALL="/home/codespace/.fly"
export PATH="$FLYCTL_INSTALL/bin:$PATH"

# Authenticate
flyctl auth login

# Verify authentication
flyctl auth whoami
```

**Verification:**

- [ ] `flyctl version` shows installed version
- [ ] `flyctl auth whoami` shows your email

**Documentation:**

- [Fly.io CLI Installation](https://fly.io/docs/hands-on/install-flyctl/)
- [Fly.io Sign Up](https://fly.io/app/sign-up)

---

#### Task 2.2.2: Initialize Fly.io Application

**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**

- [ ] Fly.io app initialized with `fly launch`
- [ ] fly.toml configuration file created
- [ ] App name configured
- [ ] Region selected (closest to target users)
- [ ] Volume configuration planned
- [ ] Internal port set to 8080

**Commands:**

```bash
cd /workspaces/taskmasterdotnet/TaskMaster

# Initialize Fly.io app (will prompt for configuration)
flyctl launch --no-deploy

# Follow prompts:
# - Choose app name (e.g., taskmaster-dotnet-[yourname])
# - Choose region (e.g., iad - Virginia for US East)
# - Don't deploy yet (we need to configure volume first)
```

**fly.toml** (review and modify if needed):

```toml
app = "taskmaster-dotnet-app"
primary_region = "iad"

[build]

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = true
  auto_start_machines = true
  min_machines_running = 0
  processes = ["app"]

[[vm]]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 256
```

**Verification:**

- [ ] fly.toml exists in TaskMaster directory
- [ ] App name is unique and available
- [ ] Configuration matches application requirements

---

#### Task 2.2.3: Create Persistent Volume for Database

**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**

- [ ] Fly.io volume created for SQLite database
- [ ] Volume size appropriate (1GB minimum)
- [ ] Volume in same region as app
- [ ] Volume mount path configured in fly.toml

**Commands:**

```bash
# Create a 1GB volume for the database
flyctl volumes create taskmaster_data --size 1 --region iad

# List volumes to verify
flyctl volumes list
```

**fly.toml** (add mounts section):

```toml
[mounts]
  source = "taskmaster_data"
  destination = "/data"
```

**Complete fly.toml example:**

```toml
app = "taskmaster-dotnet-app"
primary_region = "iad"

[build]

[env]
  ASPNETCORE_ENVIRONMENT = "Production"

[http_service]
  internal_port = 8080
  force_https = true
  auto_stop_machines = true
  auto_start_machines = true
  min_machines_running = 0
  processes = ["app"]

[mounts]
  source = "taskmaster_data"
  destination = "/data"

[[vm]]
  cpu_kind = "shared"
  cpus = 1
  memory_mb = 256

[checks]
  [checks.health]
    grace_period = "10s"
    interval = "30s"
    method = "GET"
    path = "/health"
    port = 8080
    timeout = "5s"
    type = "http"
```

**Verification:**

```bash
flyctl volumes list
# Should show taskmaster_data volume
```

---

### 2.3 Initial Deployment

**Total Estimated Time:** 15 minutes

#### Task 2.3.1: Deploy Application to Fly.io

**Effort:** M (~200k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**

- [ ] Application successfully deployed
- [ ] Docker image built and pushed to Fly.io registry
- [ ] Machine(s) started successfully
- [ ] Health checks passing
- [ ] Application accessible via Fly.io URL
- [ ] Deployment logs show no errors

**Commands:**

```bash
cd /workspaces/taskmasterdotnet/TaskMaster

# Deploy the application
flyctl deploy

# Monitor deployment
flyctl logs

# Check application status
flyctl status

# Open application in browser
flyctl open
```

**Deployment Process:**

1. Fly.io builds Docker image from Dockerfile
2. Pushes image to Fly.io registry
3. Creates machine with volume mounted
4. Starts application
5. Runs health checks
6. Routes traffic once healthy

**Verification:**

```bash
# Check deployment status
flyctl status

# View recent logs
flyctl logs

# Test health endpoint
curl https://[your-app-name].fly.dev/health

# Test application in browser
flyctl open
```

**Expected Output:**

- Status shows "deployed" and "running"
- Logs show application started successfully
- Health endpoint returns 200 OK
- Application UI loads in browser

---

#### Task 2.3.2: Verify Production Functionality

**Effort:** XS (~50k tokens) | **Time:** 5 minutes

**Acceptance Criteria:**

- [ ] Application accessible via public URL
- [ ] Homepage loads correctly
- [ ] Can create new task
- [ ] Can view all tasks
- [ ] Can toggle task completion
- [ ] Can delete task
- [ ] Filters work correctly
- [ ] Styling renders correctly

**Manual Testing Checklist:**

```
1. Open application URL: https://[your-app-name].fly.dev
2. Verify homepage loads with empty state
3. Create task: "Test deployment" (Work category)
4. Verify task appears in list
5. Toggle task to completed
6. Verify strikethrough and opacity change
7. Filter by "Done" - verify task shows
8. Filter by "Active" - verify task hidden
9. Filter by "All" - verify task shows
10. Delete task
11. Verify task removed
12. Create 3 new tasks to test persistence
```

**Commands for Testing:**

```bash
# Get application URL
flyctl info

# Monitor logs during testing
flyctl logs --follow

# Check health status
curl https://[your-app-name].fly.dev/health
```

---

### 2.4 Database Persistence & Restart Testing

**Total Estimated Time:** 10 minutes

#### Task 2.4.1: Test Data Persistence Across Deployments

**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**

- [ ] Tasks created before deployment persist after redeploy
- [ ] Database file persists in volume
- [ ] No data loss during application restart
- [ ] Migrations run successfully on cold start
- [ ] Multiple deploy cycles preserve all data

**Testing Procedure:**

```bash
# 1. Create test data
# - Open application in browser
# - Create 5 tasks with different categories
# - Mark 2 as completed
# - Note task IDs or titles

# 2. Trigger a redeployment
flyctl deploy

# 3. Wait for deployment to complete
flyctl status

# 4. Open application again
flyctl open

# 5. Verify all 5 tasks still exist
# 6. Verify completion status preserved
# 7. Create new task to test write functionality

# 6. Test machine restart
flyctl machine restart [machine-id]

# 7. Wait for restart
sleep 30

# 8. Verify data still intact
flyctl open
```

**Verification Commands:**

```bash
# Check volume status
flyctl volumes list

# SSH into machine to inspect database
flyctl ssh console
ls -lh /data/
# Should show tasks.db file
exit

# Check logs for migration messages
flyctl logs | grep -i migration
```

**Success Indicators:**

- All tasks persist across redeployment
- Database file size increases with new tasks
- No migration errors in logs
- Application starts successfully after restart

---

### 2.5 Production Monitoring & Logging

**Total Estimated Time:** 10 minutes

#### Task 2.5.1: Configure Logging and Monitoring

**Effort:** S (~100k tokens) | **Time:** 10 minutes

**Acceptance Criteria:**

- [ ] Application logs accessible via Fly.io CLI
- [ ] Log levels appropriate for production
- [ ] Health check monitoring configured
- [ ] Machine metrics visible in Fly.io dashboard
- [ ] Alert configuration documented
- [ ] Log retention understood

**Configure Enhanced Logging:**

**appsettings.Production.json** (update if needed):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.AspNetCore.Hosting": "Information",
      "Microsoft.AspNetCore.Routing": "Information"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "SingleLine": true,
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss ",
        "UseUtcTimestamp": true
      }
    }
  }
}
```

**Program.cs** (add request logging):

```csharp
// Add after builder creation
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode;
});

// Add after app.Build(), before other middleware
if (app.Environment.IsProduction())
{
    app.UseHttpLogging();
}
```

**Monitoring Commands:**

```bash
# View real-time logs
flyctl logs --follow

# View logs for specific time period
flyctl logs --since 1h

# View machine metrics
flyctl machine list

# View machine details
flyctl machine status [machine-id]

# View app metrics in dashboard
flyctl dashboard
```

**Set Up Log Searching:**

```bash
# Search logs for errors
flyctl logs | grep -i error

# Search logs for specific endpoints
flyctl logs | grep -i "POST /Create"

# Search logs for health checks
flyctl logs | grep -i "/health"
```

**Verification:**

- [ ] Can view logs in real-time
- [ ] Logs show INFO level messages
- [ ] Health check logs visible
- [ ] Request/response logs visible
- [ ] No ERROR or WARNING logs during normal operation

---

## 📊 Phase 2 Summary

### Total Effort by Category

| Category            | Tasks  | Estimated Time | Token Budget     |
| ------------------- | ------ | -------------- | ---------------- |
| Production Config   | 3      | 20 min         | ~150k tokens     |
| Fly.io Setup        | 3      | 20 min         | ~150k tokens     |
| Deployment          | 2      | 15 min         | ~250k tokens     |
| Persistence Testing | 1      | 10 min         | ~100k tokens     |
| Monitoring          | 1      | 10 min         | ~100k tokens     |
| **TOTAL**           | **10** | **75 min**     | **~750k tokens** |

### Task Size Distribution

- **XS Tasks (50k):** 6 tasks → 300k tokens
- **S Tasks (100k):** 3 tasks → 300k tokens
- **M Tasks (200k):** 1 task → 200k tokens

### Critical Path

1. Production Configuration (Security and optimization)
2. Fly.io Setup (Account, app, volume)
3. Initial Deployment (Build and deploy)
4. Persistence Verification (Data integrity)
5. Monitoring Setup (Observability)

---

## 🚀 Getting Started with Phase 2

**Prerequisites:**

- ✅ Phase 1 completed and tested locally
- ✅ Docker image builds successfully
- ✅ Fly.io account created (free tier available)
- ✅ Credit card on file (required by Fly.io, but free tier exists)
- ✅ Git repository with latest code

**Start Here:**

```bash
# 1. Ensure Phase 1 is complete
cd /workspaces/taskmasterdotnet/TaskMaster
dotnet build -c Release

# 2. Begin with Task 2.1.1
# Update production configuration files

# 3. Install Fly.io CLI (Task 2.2.1)
curl -L https://fly.io/install.sh | sh

# 4. Follow tasks in order
# 5. Test thoroughly after deployment
```

**Success Tips:**

- ✅ Test locally with Release configuration before deploying
- ✅ Start with smallest machine size (256MB) to minimize costs
- ✅ Monitor logs during and after deployment
- ✅ Test data persistence early and often
- ✅ Keep deployment commands handy for quick rollbacks
- ✅ Document your application URL and credentials

---

## 🔒 Security Considerations

### Production Security Checklist

- [ ] HTTPS enforced (Fly.io provides automatic SSL)
- [ ] HSTS headers configured
- [ ] Security headers added (X-Content-Type-Options, X-Frame-Options, etc.)
- [ ] Database file permissions restricted
- [ ] No sensitive data in logs
- [ ] Connection strings use environment variables
- [ ] Error pages don't expose stack traces
- [ ] Health check doesn't expose sensitive information

### Fly.io Security Features

- **Automatic SSL/TLS**: Fly.io provides free SSL certificates
- **Private networking**: Machines can communicate internally
- **Secrets management**: Use `flyctl secrets set` for sensitive data
- **Volume encryption**: Data at rest is encrypted

### Setting Secrets (if needed):

```bash
# Example: Set a secret
flyctl secrets set MY_SECRET_KEY="value"

# List secrets (values hidden)
flyctl secrets list

# Access in application via environment variables
# Configuration["MY_SECRET_KEY"]
```

---

## 💰 Cost Estimation

### Fly.io Free Tier (as of 2025)

- **Included Free:**
  - Up to 3 shared-cpu-1x machines with 256MB RAM
  - 3GB persistent volume storage
  - 160GB outbound data transfer per month

### Expected Monthly Cost (Minimal Usage)

- **Free Tier**: $0/month for this application
- **Volume**: 1GB used of 3GB free = $0
- **Compute**: 1 machine @ 256MB = $0 (within free tier)
- **Bandwidth**: Typical usage < 160GB = $0

### Cost Optimization Tips

- Use `auto_stop_machines = true` to stop when idle
- Use `auto_start_machines = true` to start on request
- Set `min_machines_running = 0` to allow full shutdown when idle
- Monitor usage in Fly.io dashboard

---

## 🔄 CI/CD Considerations (Future Enhancement)

### GitHub Actions Setup (Optional)

For automated deployments, create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Fly.io

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Set up Flyctl
        uses: superfly/flyctl-actions/setup-flyctl@master

      - name: Deploy to Fly.io
        run: flyctl deploy --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
        working-directory: ./TaskMaster
```

**Setup Steps:**

1. Generate Fly.io deploy token: `flyctl tokens create deploy`
2. Add token to GitHub Secrets as `FLY_API_TOKEN`
3. Push to main branch to trigger deployment

---

## 🐛 Troubleshooting Guide

### Common Issues and Solutions

#### Issue: Deployment fails during build

**Solution:**

```bash
# Check Docker builds locally first
cd TaskMaster
docker build -t taskmaster:test .

# View detailed deployment logs
flyctl logs

# Check for .dockerignore issues
cat .dockerignore
```

#### Issue: Database not persisting

**Solution:**

```bash
# Verify volume is created
flyctl volumes list

# Check volume mount in fly.toml
cat fly.toml | grep -A2 mounts

# SSH into machine to check
flyctl ssh console
ls -la /data/
```

#### Issue: Application won't start

**Solution:**

```bash
# Check machine status
flyctl status

# View recent logs
flyctl logs --since 10m

# Check health endpoint
flyctl machine status [machine-id]

# Restart machine
flyctl machine restart [machine-id]
```

#### Issue: Health checks failing

**Solution:**

```bash
# Test health endpoint locally
curl http://localhost:8080/health

# Check health check configuration in fly.toml
cat fly.toml | grep -A8 checks

# Verify internal_port matches ASPNETCORE_URLS
# Should be 8080 for both
```

#### Issue: Out of memory errors

**Solution:**

```bash
# Scale up machine memory
flyctl scale memory 512

# Or edit fly.toml
# Change memory_mb = 256 to memory_mb = 512

# Redeploy
flyctl deploy
```

---

## 📚 Useful Fly.io Commands Reference

### Deployment Commands

```bash
flyctl deploy                    # Deploy application
flyctl deploy --no-cache         # Deploy with fresh build
flyctl deploy --strategy immediate  # Deploy without health checks
flyctl releases                  # View deployment history
flyctl releases rollback         # Rollback to previous version
```

### Machine Management

```bash
flyctl machine list              # List all machines
flyctl machine status [id]       # Check machine status
flyctl machine restart [id]      # Restart machine
flyctl machine stop [id]         # Stop machine
flyctl machine start [id]        # Start machine
flyctl machine destroy [id]      # Delete machine
```

### Volume Management

```bash
flyctl volumes list              # List volumes
flyctl volumes show [id]         # Show volume details
flyctl volumes extend [id] --size 2  # Increase volume size
flyctl volumes destroy [id]      # Delete volume (DANGEROUS!)
```

### Monitoring & Debugging

```bash
flyctl logs                      # View logs
flyctl logs --follow            # Stream logs real-time
flyctl logs --since 1h          # Logs from last hour
flyctl ssh console              # SSH into machine
flyctl ssh sftp                 # SFTP access to machine
flyctl dashboard                # Open web dashboard
```

### Configuration

```bash
flyctl info                      # App information
flyctl status                    # Current status
flyctl config display           # Show current config
flyctl secrets list             # List secrets
flyctl secrets set KEY=value    # Set secret
flyctl scale show               # Show scaling config
```

---

## ✅ Phase 2 Completion Checklist

### Pre-Deployment

- [ ] Phase 1 completed and tested
- [ ] Production configuration files created
- [ ] Health check endpoint implemented
- [ ] Dockerfile optimized for production
- [ ] Security headers configured

### Fly.io Setup

- [ ] Fly.io CLI installed
- [ ] Authenticated with Fly.io account
- [ ] Application initialized with `fly launch`
- [ ] fly.toml configured correctly
- [ ] Persistent volume created

### Deployment

- [ ] Application deployed successfully
- [ ] Health checks passing
- [ ] Application accessible via public URL
- [ ] All CRUD operations working in production

### Verification

- [ ] Data persists across deployments
- [ ] Application restarts without data loss
- [ ] Filters work correctly
- [ ] Validation functions properly
- [ ] No errors in production logs

### Monitoring

- [ ] Can view logs via CLI
- [ ] Health checks configured and working
- [ ] Dashboard accessible
- [ ] Alert strategy documented

### Documentation

- [ ] Application URL documented
- [ ] Deployment commands documented
- [ ] Troubleshooting procedures documented
- [ ] Progress tracker updated

---

## 🎯 Success Metrics

**Phase 2 will be considered successful when:**

- ✅ Application is live at https://[app-name].fly.dev
- ✅ All Phase 1 functionality works in production
- ✅ Data persists across multiple deployments
- ✅ Health checks report healthy status
- ✅ Application handles restarts gracefully
- ✅ Logs are accessible and meaningful
- ✅ Zero-downtime deployments possible
- ✅ Application runs within free tier limits

---

## 🔗 Resources

### Fly.io Documentation

- [Fly.io Getting Started](https://fly.io/docs/getting-started/)
- [Fly.io .NET Guide](https://fly.io/docs/languages-and-frameworks/dotnet/)
- [Fly.io Volumes](https://fly.io/docs/reference/volumes/)
- [Fly.io Secrets](https://fly.io/docs/reference/secrets/)
- [Fly.io Autoscaling](https://fly.io/docs/reference/autoscaling/)

### .NET Production Resources

- [ASP.NET Core Production Best Practices](https://docs.microsoft.com/aspnet/core/fundamentals/best-practices)
- [EF Core Connection Strings](https://docs.microsoft.com/ef/core/miscellaneous/connection-strings)
- [Health Checks in .NET](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks)

### SQLite Resources

- [SQLite in Production](https://www.sqlite.org/whentouse.html)
- [SQLite Performance Tuning](https://www.sqlite.org/pragma.html)

---

## 🚀 Next Steps After Phase 2

Once Phase 2 is complete, consider:

1. **Custom Domain**: Map a custom domain to your Fly.io app
2. **CI/CD**: Set up GitHub Actions for automated deployments
3. **Monitoring**: Integrate external monitoring (e.g., UptimeRobot)
4. **Scaling**: Add more machines in different regions
5. **Phase 3**: Begin microservices evolution (see architecture docs)

---

**Ready to deploy?** Start with [Task 2.1.1: Update Application for Production](#task-211-update-application-for-production)

**Previous Phase:** [Implementation Phase 1](./implementation_phase_1.md)  
**Progress Tracking:** [Progress Tracker](./progress_tracker.md)
