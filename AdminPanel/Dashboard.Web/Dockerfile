# Dashboard Web (Admin Panel) Dockerfile
# Multi-stage build for optimized image size

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ECommerce.slnx ./
COPY src/Core/ECommerce.Domain/ECommerce.Domain.csproj src/Core/ECommerce.Domain/
COPY src/Core/ECommerce.Application/ECommerce.Application.csproj src/Core/ECommerce.Application/
COPY src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/Infrastructure/ECommerce.Infrastructure/
COPY AdminPanel/Dashboard.Web/Dashboard.Web.csproj AdminPanel/Dashboard.Web/

# Restore dependencies
RUN dotnet restore AdminPanel/Dashboard.Web/Dashboard.Web.csproj

# Copy all source code
COPY src/ src/
COPY AdminPanel/ AdminPanel/

# Build and publish
WORKDIR /src/AdminPanel/Dashboard.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ApiSettings__BaseUrl=http://api:5000/api

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:5001/health || exit 1

EXPOSE 5001

ENTRYPOINT ["dotnet", "Dashboard.Web.dll"]
