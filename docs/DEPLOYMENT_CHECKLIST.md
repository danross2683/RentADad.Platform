# Deployment Checklist (Staging vs Production)

Use this checklist for each deploy. Keep staging and production steps distinct.

## Staging

- [ ] Confirm staging config values and secrets are set.
- [ ] Run database migrations.
- [ ] Deploy artefact.
- [ ] Run smoke tests.
- [ ] Verify OpenTelemetry exports.
- [ ] Check logs for errors.

## Production

- [ ] Confirm production config values and secrets are set.
- [ ] Ensure backup is current.
- [ ] Run database migrations (approved window).
- [ ] Deploy artefact.
- [ ] Run smoke tests.
- [ ] Verify health endpoints.
- [ ] Monitor error rate and latency for 30 minutes.
- [ ] Announce release status.

