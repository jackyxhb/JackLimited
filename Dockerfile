# syntax=docker/dockerfile:1

# Build the Vue frontend assets
FROM node:22-alpine AS frontend-builder
WORKDIR /app/frontend
ENV NODE_OPTIONS="--max-old-space-size=8192"
RUN apk --no-cache update && apk --no-cache upgrade && apk --no-cache add --virtual .build-deps && apk del .build-deps
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build-only
RUN npm prune --omit=dev && npm cache clean --force
RUN if [ -f dist/index.html ]; then echo "index.html exists"; else echo "index.html missing"; fi
RUN ls -la dist/

# Publish the ASP.NET Core backend
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS backend-builder
WORKDIR /src
COPY JackLimited.sln ./
COPY src/backend ./src/backend
RUN dotnet restore JackLimited.sln
RUN dotnet publish src/backend/JackLimited.Api/JackLimited.Api.csproj -c Release -o /app/publish /p:ASPNETCORE_ENVIRONMENT=Production

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS final
WORKDIR /app
RUN apt-get update && \
    apt-get install --only-upgrade zlib1g && \
    apt-get install -y postgresql-client && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=backend-builder /app/publish .
COPY --from=frontend-builder /app/frontend/dist ./wwwroot
RUN if [ -f wwwroot/index.html ]; then echo "wwwroot/index.html exists"; else echo "wwwroot/index.html missing"; fi
RUN ls -la wwwroot/
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh
ENTRYPOINT ["./entrypoint.sh"]
