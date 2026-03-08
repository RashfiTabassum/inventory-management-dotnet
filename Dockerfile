# Stage 1: Build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (for better Docker layer caching)
COPY InventoryApp.Core/InventoryApp.Core.csproj InventoryApp.Core/
COPY InventoryApp.Data/InventoryApp.Data.csproj InventoryApp.Data/
COPY InventoryApp.Web/InventoryApp.Web.csproj InventoryApp.Web/
RUN dotnet restore InventoryApp.Web/InventoryApp.Web.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish InventoryApp.Web/InventoryApp.Web.csproj -c Release -o /app/publish

# Stage 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render sets the PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "InventoryApp.Web.dll"]
