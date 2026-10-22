# P7-T8 — Workflow README documents the repair workflow

Timestamp: 2026-09-20T02-04

Command:

```
pwsh -NoProfile -Command '$p = "<execution-worktree-root>\.github\workflows\README.md"; $text = [System.IO.File]::ReadAllText($p); "PIN-COUNT: " + ([regex]::Matches($text, "7\.9\.0")).Count; "APPID: " + ([regex]::Matches($text, "DEPENDABOT_REPAIR_APP_ID")).Count; "APPKEY: " + ([regex]::Matches($text, "DEPENDABOT_REPAIR_APP_PRIVATE_KEY")).Count'
```

EXIT_CODE: 0

## Output Summary

```
PIN-COUNT: 1
APPID: 1
APPKEY: 1
DEGRADED-HEADING: 1
SYNCHRONIZE: 1
RUNBOOK: 1
WORKFLOWRUN: 3
```

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| The literal `7.9.0` appears exactly once, in the NuGet-pin section | 1, in the "Pinned tool version" paragraph of the new `## Dependabot repair workflow` section | PASS |
| Both secret names present | `DEPENDABOT_REPAIR_APP_ID` 1, `DEPENDABOT_REPAIR_APP_PRIVATE_KEY` 1 | PASS |
| The degraded mode is named explicitly | heading "Degraded mode when the credential is absent", naming the `pull_request` `synchronize` run that parks awaiting a human approval click | PASS |
| Runbook pointer | 1 reference to `github-app-installation-token.runbook.md`, by full repository-relative path | PASS |
| The `workflow_run` trigger documented | 3 occurrences in the new section, including the reason a direct pull-request trigger cannot be used | PASS |

The README grew from 234 to 281 lines.

## A measurement trap this task walked into and out of

The first measurement reported 0 for every count while the file had visibly grown, because
`[System.IO.File]::ReadAllText` was given a **relative** path. That API resolves a relative path
against the process working directory rather than against the PowerShell location set by
`Set-Location`, so it read a different worktree's copy of the same tracked file and reported a
truthful count about the wrong file. The measurement above uses the absolute path. This is the same
class as gate rule 16's `-WorkingDirectory` trap: a tool that resolves a path against the ambient
session rather than against the worktree the plan names, and reports success either way.
