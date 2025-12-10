# Runbook: Dashboard Availability Drops

**Related Alerts**: Dashboard availability alert

## 1. Detection
- Alert fires when browser telemetry reports availability below 97% in a 30-minute window.
- Validate by opening the real-user monitoring (RUM) dashboard and reviewing error samples.

## 2. Initial Triage
- Identify the dominant error types (JavaScript exceptions, failed network calls, asset loads).
- Check CDN or static asset hosting status pages for outages.
- Verify backend health (`/healthz`, `/readyz`) to rule out API failures.

## 3. Mitigation Steps
1. **Frontend regression**
   - Roll back the latest frontend build in the hosting platform.
   - Purge CDN caches to remove broken assets.
2. **Backend dependency**
   - If API calls fail, follow the relevant backend runbook before returning here.
3. **Third-party outage**
   - Communicate impact to stakeholders and enable customer-facing incident banner if available.

## 4. Verification
- Confirm RUM availability climbs above 99% and error rate stabilizes.
- Perform manual smoke tests covering dashboard load, chart rendering, and API calls.
- Clear incident banner once stability holds for 30 minutes.

## 5. Post-Incident
- Capture root cause analysis including failing stack traces and timeline.
- Add automated synthetic monitoring if gaps were identified.
