# GitHub Workflow - Cross-Repository Commit Trigger

This setup contains two workflows that work together to trigger a workflow in a different repository and send commit details.

## Files

1. **trigger-external-workflow.yml** - Runs in the source repository (FeedR)
2. **receive-commit-details.yml** - Runs in the target repository

## Setup Instructions

### 1. In the Source Repository (FeedR)

1. Copy `trigger-external-workflow.yml` to `.github/workflows/`
2. Create a personal access token (PAT) with `workflow` scope:
   - Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Click "Generate new token (classic)"
   - Select scopes: `workflow`, `repo`, `read:org`
   - Copy the token

3. Add the token as a repository secret:
   - Go to your repository settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `EXTERNAL_REPO_TOKEN`
   - Value: Paste your PAT

4. Update the workflow inputs:
   - Edit `.github/workflows/trigger-external-workflow.yml`
   - Change `your-org/target-repo` to your actual target repository
   - Change `receive-commit-details.yml` to match the target workflow filename

### 2. In the Target Repository

1. Copy `receive-commit-details.yml` to `.github/workflows/`
2. The workflow will be triggered via `workflow_dispatch` event with commit details as inputs

## How It Works

### Method 1: Workflow Dispatch (Primary)

```
Source Repo (Push Event)
    ↓
trigger-external-workflow.yml
    ↓
Collects commit details (SHA, message, author, email, URL, branch)
    ↓
Calls GitHub API: actions.createWorkflowDispatch()
    ↓
Target Repo receives event
    ↓
receive-commit-details.yml runs with inputs containing commit details
```

### Method 2: Repository Dispatch (Alternative)

```
Source Repo (Push Event)
    ↓
trigger-external-workflow.yml
    ↓
Calls GitHub API: repos.createDispatchEvent()
    ↓
Target Repo receives repository_dispatch event
    ↓
receive-commit-details.yml runs with payload containing commit details
```

## Commit Details Passed

- **commit_sha** - Full commit hash
- **commit_message** - Commit message body
- **commit_author** - Author name
- **commit_email** - Author email
- **commit_url** - Full GitHub URL to the commit
- **source_repo** - Source repository (owner/repo)
- **source_branch** - Branch name where commit was made

## Triggering Manually

You can manually trigger the source workflow:

```bash
gh workflow run trigger-external-workflow.yml \
  -f target_repo="your-org/target-repo" \
  -f target_workflow="receive-commit-details.yml"
```

## Token Permissions

The `EXTERNAL_REPO_TOKEN` needs these permissions:

- `workflow` - To trigger workflows in the target repository
- `repo` - To access repository data
- `read:org` - To read organization data (if needed)

## Environment Variables (Optional)

You can set these in the workflow file instead of using inputs:

```yaml
env:
  TARGET_REPO: "your-org/target-repo"
  TARGET_WORKFLOW: "receive-commit-details.yml"
```

## Customization

### Add more commit details

Edit the "Get commit details" step in `trigger-external-workflow.yml` to extract additional information:

```bash
COMMIT_TIMESTAMP=$(git log -1 --pretty=%aI)
echo "timestamp=$COMMIT_TIMESTAMP" >> $GITHUB_OUTPUT
```

### Filter triggers

Only trigger on specific branches or tags:

```yaml
on:
  push:
    branches:
      - main
    tags:
      - "v*"
```

### Add validation

Validate commit details before triggering:

```yaml
- name: Validate commit
  run: |
    if [[ "${{ steps.commit.outputs.message }}" == *"skip-trigger"* ]]; then
      echo "Skipping trigger based on commit message"
      exit 0
    fi
```

## Troubleshooting

### Workflow not triggering

- ✓ Verify `EXTERNAL_REPO_TOKEN` is set correctly
- ✓ Check token has `workflow` scope
- ✓ Verify target repository name is correct
- ✓ Check target repository has receive workflow enabled
- ✓ Review GitHub Actions logs for errors

### Token expired

- Generate a new PAT and update the secret

### "Permission denied" error

- Ensure token has sufficient scopes
- Consider using a GitHub App or organization token for better control

## Security Considerations

- Store tokens as secrets (✓ Already done)
- Use fine-grained personal access tokens for better security
- Consider using GitHub Apps for better audit trails
- Restrict workflows to specific branches/tags
- Review commit details before taking action in target workflow
