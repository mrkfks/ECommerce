# ECommerce REST API Dockerfile
# Multi-stage build for optimized image size

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ECommerce.slnx ./
COPY src/Core/ECommerce.Domain/ECommerce.Domain.csproj src/Core/ECommerce.Domain/
COPY src/Core/ECommerce.Application/ECommerce.Application.csproj src/Core/ECommerce.Application/
COPY src/Infrastructure/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj src/Infrastructure/ECommerce.Infrastructure/
COPY src/Presentation/ECommerce.RestApi/ECommerce.RestApi.csproj src/Presentation/ECommerce.RestApi/

# Restore dependencies
RUN dotnet restore src/Presentation/ECommerce.RestApi/ECommerce.RestApi.csproj

# Copy all source code
COPY src/ src/

# Build and publish
WORKDIR /src/src/Presentation/ECommerce.RestApi
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published files
COPY --from=build /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/ECommerce.db"

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:5000/health || exit 1

EXPOSE 5000

ENTRYPOINT ["dotnet", "ECommerce.RestApi.dll"]
