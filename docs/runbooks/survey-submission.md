# Runbook: Survey Submission Failures

**Related Alerts**: Fast burn SLO, Slow burn SLO

## 1. Detection
- Alert triggers when error budget burn rate exceeds thresholds.
- Validate the alert via the observability dashboard (survey submission SLI graph) and `/healthz` endpoint.

## 2. Initial Triage
- Check latest deployments or feature flags that touched the API or database.
- Inspect OTLP traces filtered by `/api/survey` with `outcome=failure` to identify dominant failure codes.
- Review recent logs (correlation IDs in alert payload) for stack traces or database error signatures.

## 3. Mitigation Steps
1. **Application regression**
   - Roll back to the last known-good deployment.
   - Disable newly introduced feature flags affecting submission logic.
2. **Database connectivity**
   - Confirm database availability via cloud portal.
   - Fail over to secondary database (if configured) or restart connection pool.
3. **Validation spike**
   - Verify request payloads; if external misuse, adjust rate limits or block offending sources.

## 4. Verification
- Confirm `/api/survey` 2xx rate recovers and burn rate drops below 1x.
- Ensure `/readyz` reports `Healthy` for all entries.
- Close or downgrade the alert once success metrics stabilize for 30 minutes.

## 5. Post-Incident
- Create an incident summary with timeline, impact, and remediation tasks.
- File issues for preventive fixes (tests, guards, documentation updates).
