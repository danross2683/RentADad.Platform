#!/usr/bin/env sh
set -eu

BASE_URL="${BASE_URL:-http://localhost:8080}"
REQUESTS="${REQUESTS:-200}"
CONCURRENCY="${CONCURRENCY:-10}"

echo "Load test: ${REQUESTS} requests at concurrency ${CONCURRENCY}"
echo "Target: ${BASE_URL}/api/v1/jobs"

seq 1 "${REQUESTS}" | xargs -n1 -P "${CONCURRENCY}" -I{} sh -c \
  "curl -sS -o /dev/null -w '%{http_code}\n' '${BASE_URL}/api/v1/jobs'" \
  | awk '
    { counts[$1]++ }
    END {
      for (code in counts) {
        printf(\"%s %d\n\", code, counts[code])
      }
    }'

