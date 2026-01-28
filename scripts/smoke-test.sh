#!/usr/bin/env sh
set -eu

BASE_URL="${BASE_URL:-http://localhost:8080}"
TIMEOUT_SECS="${TIMEOUT_SECS:-10}"

echo "Running smoke tests against ${BASE_URL}"

check() {
  url="$1"
  code="$(curl -sS -o /dev/null -w "%{http_code}" --max-time "${TIMEOUT_SECS}" "${url}")"
  if [ "${code}" -ne 200 ]; then
    echo "Smoke test failed for ${url} (status ${code})."
    exit 1
  fi
}

check "${BASE_URL}/health/live"
check "${BASE_URL}/health/ready"
check "${BASE_URL}/api/v1/jobs"

echo "Smoke tests passed."
