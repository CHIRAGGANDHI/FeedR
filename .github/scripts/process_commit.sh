#!/usr/bin/env bash
set -euo pipefail

# process_commit.sh
# Reads GitHub Actions event JSON (from $GITHUB_EVENT_PATH) and
# extracts commit details for both `workflow_dispatch` and
# `repository_dispatch` events.

EVENT_NAME="${GITHUB_EVENT_NAME:-}"
EVENT_PATH="${GITHUB_EVENT_PATH:-}"

if [ -z "$EVENT_PATH" ] || [ ! -f "$EVENT_PATH" ]; then
  echo "GITHUB_EVENT_PATH is missing or invalid: $EVENT_PATH"
  exit 1
fi

jq_get() { jq -r --exit-status "$1" "$EVENT_PATH" 2>/dev/null || echo ""; }

if [ "$EVENT_NAME" = "workflow_dispatch" ]; then
  COMMIT_SHA=$(jq_get '.inputs.commit_sha')
  COMMIT_MESSAGE=$(jq_get '.inputs.commit_message')
  COMMIT_AUTHOR=$(jq_get '.inputs.commit_author')
  COMMIT_EMAIL=$(jq_get '.inputs.commit_email')
  COMMIT_URL=$(jq_get '.inputs.commit_url')
  SOURCE_REPO=$(jq_get '.inputs.source_repo')
  SOURCE_BRANCH=$(jq_get '.inputs.source_branch')
elif [ "$EVENT_NAME" = "repository_dispatch" ]; then
  COMMIT_SHA=$(jq_get '.client_payload.commit_sha')
  COMMIT_MESSAGE=$(jq_get '.client_payload.commit_message')
  COMMIT_AUTHOR=$(jq_get '.client_payload.commit_author')
  COMMIT_EMAIL=$(jq_get '.client_payload.commit_email')
  COMMIT_URL=$(jq_get '.client_payload.commit_url')
  SOURCE_REPO=$(jq_get '.client_payload.source_repo')
  SOURCE_BRANCH=$(jq_get '.client_payload.source_branch')
else
  echo "Unsupported event: $EVENT_NAME"
  exit 1
fi

echo "Received commit from: ${SOURCE_REPO:-unknown} (${SOURCE_BRANCH:-unknown})"
echo "SHA: ${COMMIT_SHA:-unknown}"
echo "Author: ${COMMIT_AUTHOR:-unknown} <${COMMIT_EMAIL:-}>"
echo "Message: ${COMMIT_MESSAGE:-}""
echo "URL: ${COMMIT_URL:-}""

# write a small log
mkdir -p logs
printf '%s\n' \
  "Timestamp: $(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
  "Source Repo: ${SOURCE_REPO:-}" \
  "Source Branch: ${SOURCE_BRANCH:-}" \
  "Commit SHA: ${COMMIT_SHA:-}" > logs/commits.log

echo "Wrote logs/commits.log"

exit 0
