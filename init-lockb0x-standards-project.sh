#!/bin/bash
# Lockb0x Standards Submission Project setup (new GitHub Projects API)
# Requires: gh CLI with project scope, jq
# If you see a scopes error, run: gh auth refresh -s project,read:project

set -euo pipefail

OWNER="lockb0x-llc"
PROJECT_NAME="Standards Submission Roadmap"

need() {
  command -v "$1" >/dev/null 2>&1 || { echo "❌ '$1' is required but not installed."; exit 1; }
}
need gh
need jq

echo "📦 Creating project: $PROJECT_NAME..."

# Create project and capture node ID
PROJECT_JSON=$(gh project create --owner "$OWNER" --title "$PROJECT_NAME" --format json || true)
if [[ -z "$PROJECT_JSON" || "$PROJECT_JSON" == *"missing required scopes"* ]]; then
  echo "❌ Missing scopes. Run: gh auth refresh -s project,read:project"
  exit 1
fi

PROJECT_ID=$(echo "$PROJECT_JSON" | jq -r '.id // empty')
if [[ -z "$PROJECT_ID" ]]; then
  echo "❌ Failed to create project. CLI output:"
  echo "$PROJECT_JSON"
  exit 1
fi
echo "✅ Project created with ID: $PROJECT_ID"

# Get the numeric project number (for the nice URL)
PROJECT_NUMBER=$(gh project list --owner "$OWNER" --format json \
  | jq -r ".[] | select(.title==\"$PROJECT_NAME\") | .number")

if [[ -z "$PROJECT_NUMBER" ]]; then
  echo "⚠️ Could not determine project number for URL. It will still exist in org projects."
fi

# Create a Status field (SINGLE_SELECT)
STATUS_JSON=$(gh project field-create "$PROJECT_ID" \
  --name "Status" \
  --data-type SINGLE_SELECT \
  --single-select-options "Backlog,Drafting,Review,Submit,Post-Submission" \
  --format json || true)

if [[ -z "$STATUS_JSON" || "$STATUS_JSON" == *"unknown flag"* ]]; then
  echo "❌ Field creation failed. Your gh version may be old. Check with: gh --version"
  echo "CLI output:"
  echo "$STATUS_JSON"
  exit 1
fi

STATUS_FIELD_ID=$(echo "$STATUS_JSON" | jq -r '.id // empty')
if [[ -z "$STATUS_FIELD_ID" ]]; then
  echo "❌ Failed to capture Status field id."
  echo "$STATUS_JSON"
  exit 1
fi
echo "✅ Status field created with ID: $STATUS_FIELD_ID"

# Helper to add item and set Status
add_item () {
  local title="$1"
  local stage="$2"

  local item_json
  item_json=$(gh project item-create "$PROJECT_ID" --title "$title" --format json)
  local item_id
  item_id=$(echo "$item_json" | jq -r '.id // empty')
  if [[ -z "$item_id" ]]; then
    echo "❌ Failed to create item: $title"
    echo "$item_json"
    exit 1
  fi

  # Assign Status by option name
  gh project item-edit --id "$item_id" --field-id "$STATUS_FIELD_ID" --single-select-option-name "$stage" >/dev/null
  echo "➕ Added: $title [$stage]"
}

# Add starter roadmap items
add_item "Research IETF working groups (SEC, ART, or new WG)" "Backlog"
add_item "Convert Lockb0x v1.0.0 spec into IETF Internet-Draft format" "Drafting"
add_item "Internal review of IETF draft (clarity, RFC2119 keywords, references)" "Review"
add_item "Submit Internet-Draft to IETF Datatracker" "Submit"
add_item "Collect community feedback on Internet-Draft" "Post-Submission"

if [[ -n "${PROJECT_NUMBER:-}" ]]; then
  echo "🎉 All done! View at: https://github.com/orgs/$OWNER/projects/$PROJECT_NUMBER"
else
  echo "🎉 All done! View your org projects: https://github.com/orgs/$OWNER/projects"
fi