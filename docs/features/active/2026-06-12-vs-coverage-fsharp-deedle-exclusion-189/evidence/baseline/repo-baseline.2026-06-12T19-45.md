# Phase 0 — Repo Baseline (P0-T2)

Timestamp: 2026-06-12T19-45

Command: `git branch --show-current && git rev-parse --short HEAD && git status --porcelain`

EXIT_CODE: 0

Output Summary:
- Current branch: `bug/vscode-test-runner-parity-188`
- HEAD short SHA: `aa63315b`
- `git status --porcelain` at baseline:
  ```
   M scripts/vscode/Invoke-MSTest.ps1
   M scripts/vscode/Invoke-MSTestWithCoverage.ps1
  ?? docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/
  ?? docs/features/active/2026-06-12-vscode-test-runner-parity-188/
  ?? tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
  ```
- Confirmation: `TaskMaster.runsettings` is UNMODIFIED at baseline (does not appear in `git status`).
- The only staged/modified production changes are the issue #188 files: `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and the new test `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`. These must remain untouched per scope lock.
- The two untracked `docs/features/active/...` folders are the #188 and #189 feature folders (documentation/evidence only).
