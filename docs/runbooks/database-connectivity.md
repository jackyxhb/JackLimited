# Runbook: Database Connectivity Failures

**Related Alerts**: Database connectivity alert, `/readyz` failures

## 1. Detection
- Alert triggers after three consecutive `/readyz` failures.
- Confirm via infrastructure monitoring (database CPU, connections) and application logs referencing connection errors.

## 2. Initial Triage
- Verify database instance status in the cloud portal (availability zone health, maintenance windows).
- Check recent configuration changes (connection strings, firewall rules, TLS certificates).
- Review connection pool metrics to detect exhaustion or authentication failures.

## 3. Mitigation Steps
1. **Database outage**
   - Initiate failover to the secondary instance (if configured) or request provider intervention.
   - Scale up compute or storage if resource saturation is detected.
2. **Networking/firewall**
   - Restore firewall rules or VNet routing changes rolled out recently.
   - Reapply managed identity or credentials if secrets rotated unexpectedly.
3. **Application pool exhaustion**
   - Restart API pods/instances to clear stuck connections.
   - Temporarily reduce traffic using rate limiting if saturation persists.

## 4. Verification
- Ensure `/readyz` and `/healthz` report `Healthy` after remediation.
- Validate API endpoints (survey submission and analytics) respond successfully.
- Monitor database error metrics for 30 minutes to confirm stability.

## 5. Post-Incident
- Document timeline, root cause, and corrective actions.
- Schedule follow-up tasks (e.g., connection resiliency improvements, alert tuning).
