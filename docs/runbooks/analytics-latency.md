# Runbook: Analytics Latency Degradation

**Related Alerts**: Analytics latency alert

## 1. Detection
- Alert triggers when the 95th percentile latency for analytics endpoints exceeds 400 ms for 15 minutes.
- Validate via dashboards showing `/api/survey/nps`, `/average`, and `/distribution` latency histograms.

## 2. Initial Triage
- Review recent traces highlighting slow spans; note downstream dependencies (database queries).
- Inspect database performance metrics (locks, slow queries) for the `Surveys` table.
- Check if background jobs or reporting exports are competing for database resources.

## 3. Mitigation Steps
1. **Database contention**
   - Kill runaway queries identified via PostgreSQL monitoring.
   - Temporarily increase database compute tier if saturation is confirmed.
2. **Application regression**
   - Compare latest deployment traces against previous releases; roll back if a code change introduced latency.
3. **Traffic surge**
   - Enable autoscaling or increase pod/container replicas.
   - Tune caching layer if available (consider short-term caching in API if hotspots persist).

## 4. Verification
- Confirm latency metrics return below 250 ms (95th percentile) and stay stable for 30 minutes.
- Ensure trace sampling shows normalized span durations.
- Close the alert after verifying user-facing dashboards load within the three-second target.

## 5. Post-Incident
- Document bottlenecks and longer-term fixes (indexes, caching strategies).
- Update capacity planning assumptions if traffic surges are recurring.
