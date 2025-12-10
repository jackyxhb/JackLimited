# JackLimited Service Level Objectives

## Scope and Critical User Journeys

JackLimited enables customer success teams to capture Net Promoter Score (NPS) feedback and view analytics. The following journeys must remain reliable:

1. Submit a survey response via `POST /api/survey`.
2. Retrieve analytics for dashboards via `GET /api/survey/nps`, `.../average`, and `.../distribution`.
3. Load the customer success dashboard SPA served by the backend.

## Service Level Indicators (SLIs)

| Journey | Indicator | Source | Notes |
|---------|-----------|--------|-------|
| Survey submission | Percentage of successful 2xx responses within 250 ms | Backend HTTP telemetry | Includes validation failures. 4xx due to user error do not count as failures. |
| Analytics retrieval | Percentage of successful 2xx responses within 250 ms | Backend HTTP telemetry | Combine all analytics endpoints into a single stream. |
| Dashboard availability | Percentage of page loads completing without JS fatal error within 3 s | Frontend RUM telemetry | Includes CDN/asset delivery and API bootstrap calls. |

## Service Level Objectives (SLOs)

| Journey | Target | Window | Error Budget |
|---------|--------|--------|--------------|
| Survey submission success | 99.5% | 30 days | 216 minutes of failed requests per 30 days |
| Survey submission latency | 95% < 250 ms | 30 days | 5% of requests may exceed 250 ms |
| Analytics success | 99.0% | 30 days | 432 minutes of failed requests per 30 days |
| Analytics latency | 95% < 250 ms | 30 days | 5% of requests may exceed 250 ms |
| Dashboard availability | 99.0% | 30 days | 432 minutes of failed loads per 30 days |

## Data Collection and Storage

- Backend SLIs collected via OpenTelemetry metrics exporter (histograms and counters) emitted from JackLimited.Api.
- Frontend SLIs collected via Application Insights (or equivalent) browser SDK, reporting page load outcome and total duration.
- Aggregations stored in centralized monitoring platform (App Insights, Prometheus + Grafana, or Datadog) with 90-day retention.

## Review Cadence

- Weekly review of error budget burn during engineering sync.
- Post-incident review includes SLI graphs and burn-rate analysis.
- SLOs revisited quarterly or when product commitments change.

## Out of Scope

- Internal admin tooling and deterministic testing endpoints are excluded from customer-facing SLO calculations.
- Batch exports or background jobs will require separate SLIs/SLOs when introduced.
