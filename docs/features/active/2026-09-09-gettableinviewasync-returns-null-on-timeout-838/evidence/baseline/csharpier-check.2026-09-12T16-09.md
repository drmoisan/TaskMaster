# P0-T16 — Formatter baseline in read-only check mode

Timestamp: 2026-09-13T02-24

Canonical CLAUDE.md command text: `dotnet tool run csharpier check .`

Command: `pwsh -NoProfile -Command '& ".\.dotnet-sdk\dotnet.exe" tool run csharpier check .; exit $LASTEXITCODE'`

EXIT_CODE: 0

Output Summary: the read-only check reported `Checked 1624 files in 5123ms.` and exited 0, so the worktree carries no pre-existing format drift in any file the formatter's scope reaches. No path was reported, so neither halt condition fired: there is no `PHASE-0 HALT: PRE-EXISTING FORMAT DRIFT` and no `PHASE-0 HALT: FORMATTER SCOPE INCLUDES BOOTSTRAP TREES`. The bootstrap trees created by P0-T9 and P0-T10 did not enter the formatter's scope, so the repository-wide write-mode pass P4-T1 performs cannot rewrite a file this change does not own, which is the property this baseline exists to establish. The check command is read-only and exits non-zero on drift, so its exit code alone is a falsifiable signal; no processed-file count is asserted as a gate.
