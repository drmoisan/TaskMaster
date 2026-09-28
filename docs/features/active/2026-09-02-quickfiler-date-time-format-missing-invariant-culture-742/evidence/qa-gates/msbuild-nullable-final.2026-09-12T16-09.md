# Final QA Gate 3 — Nullable / Warnings As Errors (issue #742, [P5-T3])

Timestamp: 2026-09-14T02-23

Command: `pwsh -NoProfile -Command '$out = msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1; $exit = $LASTEXITCODE; $out | Select-String -Pattern "^\s*\d+ Error\(s\)\s*$" | ForEach-Object { $_.Line.Trim() }; Write-Output "EXITCODE=$exit"'`

EXIT_CODE: 0

Output Summary: the transcribed summary line, trimmed, is exactly `0 Error(s)`, and `EXITCODE` is 0.
The `Select-String` pattern is anchored to the whole trimmed line, so a two-digit error count could
not satisfy this condition.

Acceptance: satisfied.

`/p:Nullable=enable` was not added, per `CLAUDE.md` and `.claude/rules/csharp.md`. Nullable
enforcement in this repository is per-file opt-in via `#nullable enable`, and the solution-wide
property would conscript every file that has never adopted the pragma. `/t:Rebuild` was used rather
than `/t:Build` for the same incrementality reason recorded for [P5-T2].

This command is character-for-character the one in `.github/workflows/_build-nullable.yml`, aside
from the PowerShell wrapper that captures the summary line and the exit code.

This run followed [P5-T2] with no intervening file rewrite, so no restart of the Phase 5 sequence
was required.
