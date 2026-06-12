# Phase 0 — Repository Baseline

Timestamp: 2026-06-12T19-22

Command: `git branch --show-current && git rev-parse --short HEAD && git status --porcelain`

EXIT_CODE: 0

Output Summary:
- Current branch: `bug/vscode-test-runner-parity-188`
- HEAD short SHA: `aa63315b`
- `git status --porcelain` output:

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M scripts/vscode/Invoke-MSTest.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
?? .claude/agent-memory/atomic-executor/project_runsettings_datacollector_default_enabled.md
?? docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/
?? docs/features/active/2026-06-12-vscode-test-runner-parity-188/
?? tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
```

Confirmation of the three #188 working-tree changes present BEFORE this plan's edits:
- `scripts/vscode/Invoke-MSTest.ps1` — present (` M`, tracked modification).
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — present (` M`, tracked modification).
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — present (`??`, new untracked test file authored under #188).

All three #188 files are present as working-tree changes (two tracked modifications, one new untracked test file).
`TaskMaster.runsettings` is NOT listed in `git status`, confirming it is currently at its committed baseline
(no working-tree edit; the aborted v1.0 run left no residual change to it).
