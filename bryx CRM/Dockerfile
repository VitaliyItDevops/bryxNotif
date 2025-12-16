# Railway Dockerfile for bryx CRM with PostgreSQL client tools
# This Dockerfile is executed from repository root by Railway
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file (relative to repo root where Railway executes)
COPY ["bryx CRM/bryx CRM/bryx CRM.csproj", "bryx CRM/bryx CRM/"]
RUN dotnet restore "bryx CRM/bryx CRM/bryx CRM.csproj"

# Copy all source code from bryx CRM directory
COPY ["bryx CRM/", "bryx CRM/"]

# Build and publish
WORKDIR "/src/bryx CRM/bryx CRM"
RUN dotnet publish "bryx CRM.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install PostgreSQL 17 client tools (pg_dump, pg_restore)
# Match server version 17.7 to avoid version mismatch
RUN apt-get update && \
    apt-get install -y wget gnupg && \
    echo "deb http://apt.postgresql.org/pub/repos/apt bookworm-pgdg main" > /etc/apt/sources.list.d/pgdg.list && \
    wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add - && \
    apt-get update && \
    apt-get install -y postgresql-client-17 && \
    rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Expose port (Railway uses PORT env variable)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start application
ENTRYPOINT ["dotnet", "bryx_CRM.dll"]
