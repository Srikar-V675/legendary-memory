# Multi-stage build Dockerfile for WebApiTemplate (.NET8 Web API)
#1. Restore & build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj separately for efficient layer caching
COPY WebApiTemplate/WebApiTemplate.csproj WebApiTemplate/
RUN dotnet restore WebApiTemplate/WebApiTemplate.csproj

# Copy the remaining source
COPY . .
WORKDIR /src/WebApiTemplate

# Publish (no self-contained app host for smaller image)
RUN dotnet publish WebApiTemplate.csproj -c Release -o /app/publish /p:UseAppHost=false

#2. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Optionally configure ASP.NET Core URLs (container listens on8080 internally)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Copy published output
COPY --from=build /app/publish .

# Entry point

ENTRYPOINT ["dotnet", "WebApiTemplate.dll"]
