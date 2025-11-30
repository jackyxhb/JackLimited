# Security Configuration Guide

This document outlines the security measures implemented in the JackLimited API and how to configure them properly.

## Rate Limiting

The API implements IP-based rate limiting using AspNetCoreRateLimit to prevent abuse and DoS attacks.

### Configuration

Rate limiting is configured in `appsettings.json` and environment-specific files:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "RealIpHeader": "X-Forwarded-For",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "POST:/api/survey",
        "Period": "1h",
        "Limit": 20
      }
    ]
  }
}
```

### Rate Limits

- **General endpoints**: 100 requests per minute
- **Survey submission**: 20 requests per hour (to prevent spam)
- **Analytics endpoints**: 200 requests per minute

## Security Headers

The following security headers are automatically added to all responses:

### Content Security Policy (CSP)
```
default-src 'self';
script-src 'self' 'unsafe-inline' 'unsafe-eval';
style-src 'self' 'unsafe-inline';
img-src 'self' data: https:;
font-src 'self';
connect-src 'self';
frame-ancestors 'none';
```

### Other Headers
- `X-Frame-Options: DENY` - Prevents clickjacking
- `X-Content-Type-Options: nosniff` - Prevents MIME type sniffing
- `X-XSS-Protection: 1; mode=block` - Enables XSS protection
- `Referrer-Policy: strict-origin-when-cross-origin` - Controls referrer information
- `Permissions-Policy: geolocation=(), microphone=(), camera=()` - Restricts browser features
- `Strict-Transport-Security` - Enforces HTTPS (production only)

## Secrets Management

### Development Environment

Use .NET User Secrets for sensitive configuration:

1. Initialize user secrets:
   ```bash
   dotnet user-secrets init
   ```

2. Set secrets:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=JackLimited;Username=devuser;Password=devpassword"
   dotnet user-secrets set "ApiKey" "your-development-api-key"
   ```

### Production Environment

Use Azure Key Vault or environment variables:

#### Azure App Service
Set connection strings and secrets in the Azure portal under "Configuration > Connection strings" and "Configuration > Application settings".

#### Environment Variables
```bash
export ConnectionStrings__DefaultConnection="Host=prod-server.postgres.database.azure.com;Database=JackLimited;Username=produser;Password=prodpassword"
export ApiKey="your-production-api-key"
```

### Required Secrets

- `ConnectionStrings:DefaultConnection` - PostgreSQL connection string
- `ApiKey` - API key for external integrations (if needed)

## HTTPS Enforcement

- HSTS is enabled in production with default settings
- Consider using Azure Front Door or Application Gateway for additional SSL/TLS termination

## CORS Configuration

CORS is configured to allow requests from:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:5174` (Alternative dev port)
- `https://jacklimited-portal.azurewebsites.net` (Production)

Update the origins in `Program.cs` for different environments.

## Monitoring and Logging

Security events are logged at Warning level or higher. Monitor for:
- Rate limit violations (HTTP 429 responses)
- Failed authentication attempts
- Unusual request patterns

## Deployment Checklist

- [ ] Configure production connection string in Azure App Service
- [ ] Set environment to "Production"
- [ ] Enable HTTPS-only in Azure App Service
- [ ] Configure custom domain SSL certificate
- [ ] Set up Azure Key Vault for secrets (optional)
- [ ] Configure Azure Front Door for additional security (optional)
- [ ] Enable Azure Monitor and Application Insights