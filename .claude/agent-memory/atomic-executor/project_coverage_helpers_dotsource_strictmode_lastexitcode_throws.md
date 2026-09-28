---
name: coverage-helpers-dotsource-strictmode-lastexitcode-throws
description: Dot-sourcing scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 (the P0-T9/P6-T6 case-(2) manual post-processing fallback) turns on Set-StrictMode, so a trailing `$LASTEXITCODE` read in the same pwsh invocation throws InvalidOperation and flips the exit to 1 even though the post-processing succeeded
metadata:
  type: project
---

The manual Koverage post-processing fallback (`. .\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1; ... ConvertTo-KoverageCoberturaXml ...; Write-Output "POSTPROCESSED-MANUALLY"`) succeeds, but any diagnostic appended after it that reads `$LASTEXITCODE` throws `The variable '$LASTEXITCODE' cannot be retrieved because it has not been set` and the pwsh invocation exits 1.

**Why:** the helpers file (and its four dot-sourced siblings) set `Set-StrictMode`, which propagates into the caller's scope when dot-sourced; `$LASTEXITCODE` is only defined after a native executable runs, and the fallback runs none. Observed on issue 743 P6-T6 (2026-09-13). The plan's command body itself is fine; the recorded `EXIT_CODE` for the plan command is 0 and the artifact should say so, with the diagnostic failure noted separately.

**How to apply:** when wrapping that fallback, verify success by `POSTPROCESSED-MANUALLY` plus a follow-up extraction that matches backslash filenames (`classNodes=1`), not by `$LASTEXITCODE`. If a numeric exit is wanted, use `$?` or run the fallback in its own invocation with nothing appended. Same run also confirmed: runner case (2) (`MSTest with coverage failed with exit code 1`) was triggered by the known-intermittent `QfcInitEmailQueueZeroBatchTests` Deedle/netstandard-2.1 binding failure under the runner's PARALLEL regime while the SERIAL P6-T5 run a minute earlier was 1400/1400; the fallback plus extraction gave valid per-file figures without a re-run. Related: [[fresh-worktree-quickfiler-test-red-netstandard21-deedle]], [[recursive-delete-idioms-blocked-use-dotnet-api]] (P6-T18's `Remove-Item -Recurse -Force` was hook-blocked again; `[System.IO.Directory]::Delete($p, $true)` substituted).
