# P0-T14 — Identifier-Leak Baseline (counts only)

Timestamp: 2026-09-13T05-03
Task: [P0-T14]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; $account = Split-Path -Leaf $env:USERPROFILE; $hostToken = $env:COMPUTERNAME; foreach ($f in $files) { ([regex]::Matches((Get-Content -Raw -LiteralPath $f), [regex]::Escape($account), "IgnoreCase")).Count ... }'
EXIT_CODE: 0

Both tokens were derived at run time and neither is reproduced anywhere in this artifact: the account
token is the leaf name of the current user profile directory and the host token is the computer name.
Matching is case-insensitive and fixed-string, with the token escaped before matching. Each recorded
figure is the total number of occurrences in the file, not the number of matching lines.

## Per-file counts, fourteen integers

```
COUNT| TaskMaster/TaskMaster.csproj | account=1 | host=0
COUNT| .vscode/settings.json | account=1 | host=0
COUNT| .claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md | account=1 | host=0
COUNT| .claude/agent-memory/feature-review/project_464-review-residuals.md | account=1 | host=0
COUNT| .claude/agent-memory/feature-review/project_488-review-residuals.md | account=0 | host=1
COUNT| .claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md | account=2 | host=0
COUNT| .claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md | account=1 | host=0
```

SUM_OF_FOURTEEN_PER_FILE_TOKEN_COUNTS: 8

## Fifteenth integer — the single-word literal in the project file

Command: case-insensitive occurrence count of the single-word literal `OneDrive` in
`TaskMaster/TaskMaster.csproj`.

ONEDRIVE_COUNT_IN_PROJECT_FILE: 1

## Output Summary

Fifteen bare integers are recorded: fourteen per-file token counts (1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 2,
0, 1, 0) and the project file's `OneDrive` count of 1.

The sum of the fourteen per-file token counts is 8, which is greater than zero, so the after-state
check in Phase 5 is falsifiable. The recorded count of the single-word literal `OneDrive` in the
project file is 1, which is greater than zero, so that after-state check is falsifiable too and this
task does not halt.

Distribution facts the Phase 5 corrections must satisfy, recorded so each file's target is
unambiguous: six of the seven files carry the account token and one carries the host token instead;
the file `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md` carries the
account token twice on one line, which is the case-sensitivity contrast the spec's Identifier
Corrections item 3 says must be preserved by a rewrite rather than by a token swap.

This artifact contains no account token, no host token and no organization name anywhere in its text.

EXIT_CODE: 0
