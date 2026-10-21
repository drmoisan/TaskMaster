# P7-T6 — The repair workflow

Timestamp: 2026-09-20T01-48

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $lines = Get-Content -LiteralPath ".github/workflows/dependabot-repair.yml"; "PRT: " + @($lines | Select-String -SimpleMatch "pull_request_target").Count; "WFRUN: " + @($lines | Select-String -SimpleMatch "  workflow_run:").Count; "PREFIX: " + @($lines | Select-String -SimpleMatch "startsWith(github.event.workflow_run.head_branch").Count; "CW: " + @($lines | Select-String -SimpleMatch "contents: write").Count; "PW: " + @($lines | Select-String -SimpleMatch "pull-requests: write").Count'
```

EXIT_CODE: 0

## Output Summary

```
LINES: 123
PRT: 0
WFRUN: 1
PREFIX: 1
CW: 1
PW: 1
```

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| `.github/workflows/dependabot-repair.yml` exists | yes, 123 lines | PASS |
| Occurrences of the base-context pull-request trigger name | 0 | PASS |
| `workflow_run` triggers | 1 | PASS |
| Branch-prefix restriction expressions | 1, `startsWith(github.event.workflow_run.head_branch, 'dependabot/')` | PASS |
| `contents: write` declared | 1 | PASS |
| `pull-requests: write` declared | 1 | PASS |

The three positive counts guard the zero: a file that declared no trigger and no restriction would
also report zero occurrences of the prohibited trigger.

The prohibited trigger name was present once when the file was first written, in the header comment
explaining why the mechanism was rejected. That is the zero-hit-gate-meets-documentation class: the
count is over the file, not over its executable part. The comment was rewritten to describe the
mechanism without naming it and now states in terms that the name appears nowhere in the file.

## What the workflow does

| Requirement | Where |
|---|---|
| Triggered by `workflow_run` on completion of the CI workflow | `on.workflow_run` with `workflows: [CI]` and `types: [completed]` |
| Restricted to Dependabot head branches | job-level `if:`, also requiring the originating run's event to be `pull_request` |
| Write permissions | top-level `permissions:` block |
| Installation token | `actions/create-github-app-token@v3` with `secrets.DEPENDABOT_REPAIR_APP_ID` and `secrets.DEPENDABOT_REPAIR_APP_PRIVATE_KEY` |
| Checkout with that token | `actions/checkout@v4` with `ref` = the head branch and `token` = the minted token |
| MSBuild and NuGet, pinned | `microsoft/setup-msbuild@v2`; `nuget/setup-nuget@v2` with `nuget-version: '7.9.0'` |
| Restore | `nuget restore $env:SOLUTION_PATH` |
| Run the repair | `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, failing the job on a failure result |
| Commit and push onto the Dependabot branch | guarded by a non-zero repair count, pushed with the same token |
| Pull-request body disclosure | appends the script's own `Body`, which carries `## Repairs applied` and, only when a skip was recorded, `## Packages skipped` |
| `deps:autofixed` label | applied only when the run repaired something outside the analyzer-item and binding-redirect classes |

`run-actionlint` returns `EXIT_CODE: 0` on the changed workflow set; the static-validity evidence is
recorded at P7-T7 together with the independent workflow-file enumeration.
