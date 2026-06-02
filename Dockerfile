# ── Stage 1: Build Angular SPA ────────────────────────────────────────────────
FROM node:24-alpine AS angular-build

WORKDIR /angular

# Install dependencies first (layer cache)
COPY src/SoberNetwork.Web/package*.json ./
RUN npm ci

# Build Angular in production mode
COPY src/SoberNetwork.Web/ ./
RUN npx ng build --configuration production

# ── Stage 2: Publish .NET API ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build

WORKDIR /src

# Restore (layer cache — only re-runs when .csproj files change)
COPY SoberNetwork.sln ./
COPY src/SoberNetwork.Api/SoberNetwork.Api.csproj           src/SoberNetwork.Api/
COPY src/SoberNetwork.Core/SoberNetwork.Core.csproj         src/SoberNetwork.Core/
COPY src/SoberNetwork.Infrastructure/SoberNetwork.Infrastructure.csproj src/SoberNetwork.Infrastructure/
RUN dotnet restore

# Build & publish
COPY src/SoberNetwork.Api/           src/SoberNetwork.Api/
COPY src/SoberNetwork.Core/          src/SoberNetwork.Core/
COPY src/SoberNetwork.Infrastructure/ src/SoberNetwork.Infrastructure/
RUN dotnet publish src/SoberNetwork.Api/SoberNetwork.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /publish

# ── Stage 3: Runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy published API
COPY --from=dotnet-build /publish ./

# Copy Angular SPA into wwwroot so ASP.NET Core can serve it as static files.
# Angular 19 @angular/build:application outputs browser assets under
#   dist/<project-name>/browser/
COPY --from=angular-build /angular/dist/sober-network-web/browser ./wwwroot/

# Fly.io terminates TLS at the edge and forwards HTTP on port 8080.
# The internal listener must match the internal_port in fly.toml.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SoberNetwork.Api.dll"]
