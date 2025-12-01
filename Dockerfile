# syntax=docker/dockerfile:1

# Build the Vue frontend assets
FROM node:22-alpine AS frontend-builder
WORKDIR /app/frontend
RUN apk --no-cache update && apk --no-cache upgrade && apk --no-cache add --virtual .build-deps && apk del .build-deps
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build
RUN npm prune --omit=dev && npm cache clean --force

# Publish the ASP.NET Core backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-builder
WORKDIR /src
COPY JackLimited.sln ./
COPY src/backend ./src/backend
RUN dotnet restore JackLimited.sln
RUN dotnet publish src/backend/JackLimited.Api/JackLimited.Api.csproj -c Release -o /app/publish /p:ASPNETCORE_ENVIRONMENT=Production

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN apt-get update && \
    apt-get install --only-upgrade zlib1g && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=backend-builder /app/publish .
COPY --from=frontend-builder /app/frontend/dist ./wwwroot
EXPOSE 8080
ENTRYPOINT ["dotnet", "JackLimited.Api.dll"]
