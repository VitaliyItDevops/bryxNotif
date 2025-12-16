# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["BryxBot/BryxBot.csproj", "BryxBot/"]
RUN dotnet restore "BryxBot/BryxBot.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/BryxBot"
RUN dotnet publish "BryxBot.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "BryxBot.dll"]
