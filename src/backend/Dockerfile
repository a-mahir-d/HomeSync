FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy .csproj files and restore dependencies
COPY HomeSync.WebAPI/*.csproj ./HomeSync.WebAPI/

# Restore dependencies
RUN dotnet restore HomeSync.WebAPI/HomeSync.WebAPI.csproj

# Copy all source files
COPY . .

# Build and publish the application
WORKDIR /src/HomeSync.WebAPI
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false


# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

# Environment configuration
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "HomeSync.WebAPI.dll"]
