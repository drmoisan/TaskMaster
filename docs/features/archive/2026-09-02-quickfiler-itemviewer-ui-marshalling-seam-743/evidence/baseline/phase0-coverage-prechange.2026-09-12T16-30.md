# Phase 0 — PRE-CHANGE coverage measurement, PARALLEL regime (P0-T9)

Task: [P0-T9]
Timestamp: 2026-09-13T02-26
ExpectedExitCode: 1

## Command 1 — coverage runner

Command: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\743-prechange.cobertura.xml` Run from the item worktree root via Set-Location inside one pwsh invocation, with all output streams redirected to the ignored path `coverage\p0-t9-runner.log`. Run while holding the shared machine build lock for item 743.
EXIT_CODE: 1
Output Summary:
- `Discovered 1 test assemblies.` (QuickFiler.Test\bin\Debug\QuickFiler.Test.dll)
- `Test Run Successful.` / `Total tests: 1394` / `Passed: 1394` / `Total time: 14.2463 Seconds`
- `Post-processing coverage XML for Koverage compatibility...` was printed, so the post-processed document was written at runner line 342 before the assert ran.
- Non-zero exit is case (1) of the task text, the threshold assert at runner line 344; the quoted runner output line is: `Cobertura line coverage 24.1706% is below the required 80% threshold.` (thrown from the Threshold helper file at its line 54). The figure is the single-assembly run's whole-repository rate and is expected to sit below 80% because only QuickFiler.Test was executed. The per-file figures below are therefore valid and the case-(2) manual post-processing fallback was not needed.
- No test failed; no failed test names to transcribe.
- REGIME: PARALLEL (the runner appends /Settings: resolved to the CLI runsettings file under the scripts directory, which declares Workers 0 and Scope ClassLevel).

## Command 2 — per-file Cobertura extraction (Command Reference)

Command: the per-file extraction span from the plan's Command Reference, verbatim, with `coverage\743-prechange.cobertura.xml` as the input path, plus one leading `Write-Output` of the root `line-rate`, `lines-valid` and `lines-covered` attributes. Run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical).
EXIT_CODE: 0
Output Summary:
- (a) Root attributes: `line-rate=0.241706`, `lines-valid=61852`, `lines-covered=14950`.
- (b) Per-file figures printed by the extraction:
```
QuickFiler\Controllers\QfcItemController.ViewerSetup.cs classNodes=1 linesValid=210 linesCovered=190 rate=0.904762
QuickFiler\Controllers\QfcItemController.Initialization.cs classNodes=1 linesValid=262 linesCovered=249 rate=0.950382
```
- (c) REGIME: PARALLEL (the runner appends /Settings: resolved to the CLI runsettings file under the scripts directory, which declares Workers 0 and Scope ClassLevel).
- Both named controller partials record `linesValid` greater than zero and a `rate`, satisfying the acceptance condition. The raw Cobertura document remains under the ignored `coverage` directory and is not committed (D1).
