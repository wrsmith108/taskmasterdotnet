# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies (layer caching optimization)
COPY TaskMaster.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 taskmaster && \
    adduser --system --uid 1001 --ingroup taskmaster taskmaster

# Create data directory for SQLite database with proper permissions
RUN mkdir -p /data && \
    chown -R taskmaster:taskmaster /data && \
    chmod -R 755 /data

# Copy published app from build stage
COPY --from=build /app/publish .

# Ensure app directory has correct permissions
RUN chown -R taskmaster:taskmaster /app

# Switch to non-root user
USER taskmaster

# Expose port (Fly.io requirement)
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Run the application
ENTRYPOINT ["dotnet", "TaskMaster.dll"]