# JackLimited Alert Policy

This document maps service-level objectives to actionable alerts. Alerts should direct responders to the relevant runbook (see `docs/runbooks/`).

## Alert Streams

1. **Fast burn SLO alert**
   - **Signal**: 5-minute rolling error budget burn rate > 14x (approx. 2 hours of budget consumed in 10 minutes)
   - **Scope**: Survey submission success SLO
   - **Routing**: Pager duty (primary on-call)
   - **Runbook**: `docs/runbooks/survey-submission.md`

2. **Slow burn SLO alert**
   - **Signal**: 1-hour rolling burn rate > 2x for 60 consecutive minutes
   - **Scope**: Survey submission success SLO
   - **Routing**: Pager duty (primary on-call); downgrade to ticket if within business hours
   - **Runbook**: `docs/runbooks/survey-submission.md`

3. **Analytics latency alert**
   - **Signal**: 15-minute 95th percentile latency > 400 ms on `/api/survey/nps|average|distribution`
   - **Routing**: Slack #oncall + ticket
   - **Runbook**: `docs/runbooks/analytics-latency.md`

4. **Dashboard availability alert**
   - **Signal**: Browser RUM availability < 97% over 30 minutes
   - **Routing**: Slack #oncall (no page)
   - **Runbook**: `docs/runbooks/dashboard-availability.md`

5. **Database connectivity alert**
   - **Signal**: `/readyz` failures for 3 consecutive probes (default 15-second interval)
   - **Routing**: Pager duty (primary on-call)
   - **Runbook**: `docs/runbooks/database-connectivity.md`

## Alert Configuration Guidance

- Use App Insights or Prometheus alert rules to compute burn rates based on the published SLO error budgets.
- Route alerts through the centralized incident management platform (PagerDuty or equivalent) with escalation policies for off-hours coverage.
- Attach the latest dashboard link and log query in the alert payload for rapid context.
- Silence windows longer than 15 minutes require approval from the incident commander.
- Ensure alerts auto-resolve when signal drops below threshold to avoid manual cleanup.
