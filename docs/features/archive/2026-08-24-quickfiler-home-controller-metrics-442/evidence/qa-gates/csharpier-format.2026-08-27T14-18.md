# Phase 6 — CSharpier Format, Owned Files (final pass)

Timestamp: 2026-08-27T14-18
Task: [P6-T1]
Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier format "QuickFiler\Controllers\QfcHomeController.cs" "QuickFiler\Controllers\QfcHomeController.Metrics.cs" "QuickFiler\Controllers\EfcHomeController.cs" "QuickFiler\Controllers\EfcHomeController.Metrics.cs" "QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs" "QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs" "QuickFiler.Test\Controllers\EfcHomeControllerMetricsTests.cs"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

Started 2026-08-27T14:17:20Z, ended 2026-08-27T14:17:23Z. Tool output: `Formatted 7 files in 3082ms.`

## Output Summary

**Zero of the seven owned files were rewritten.** That is determined by comparing each file's
SHA-256 digest immediately before and immediately after the command, not by reading the tool's
processed-file count. CSharpier reports `Formatted 7 files` for every file it *examined*, whether or
not it changed any bytes, so the tool's own count cannot answer this question.

| # | Owned file | Rewritten |
| --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | no |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | no |
| 3 | `QuickFiler/Controllers/EfcHomeController.cs` | no |
| 4 | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | no |
| 5 | `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | no |
| 6 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | no |
| 7 | `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | no |

The mutating pass is scoped to the seven owned files so it cannot rewrite an unowned file and break
the Phase 7 ownership gates. Because it rewrote nothing, the Phase 6 restart condition ("restart from
[P6-T1] if the formatter modifies any file") did not fire.

This is the final pass. An earlier attempt on the same tree at 2026-08-27T13:54:43Z also recorded
`EXIT_CODE: 0` with zero rewrites, but that attempt aborted at [P6-T3] on build-output file
contention from a concurrent test run and is therefore not the pass of record. See
`evidence/qa-gates/toolchain-loop.2026-08-27T14-18.md` for the full sequence of both attempts.
