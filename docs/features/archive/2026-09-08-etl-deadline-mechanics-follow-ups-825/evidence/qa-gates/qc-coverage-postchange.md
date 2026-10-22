# QC Step 5 — Post-Change Repository-Wide Coverage

Timestamp: 2026-09-09T17-20

Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/qa-gates/coverage-postchange.cobertura.xml

EXIT_CODE: 0

LineRate: 0.856686
LinesCovered: 56029
LinesValid: 65402
BranchRate: 0.798366
BranchesCovered: 13486
BranchesValid: 16892
TestsPassed: 7213
TestsFailed: 0 (omitted category, transcribed per D10)
TestsSkipped: 0 (omitted category, transcribed per D10)

Output Summary: The run used the identical command shape as the P0-T8 baseline, with -SearchRoot at
the repository root, so the two figures are comparable. vstest printed "Total tests: 7213",
"Passed: 7213" and "Total time: 30.0594 Seconds". Per D10 a fully green run emits no `Failed:` and
no `Skipped:` line, so both counters are transcribed as 0. The run was green, so D9's re-run ladder
was not entered and the processed Cobertura XML was produced; a raw unprocessed Cobertura would not
have been an acceptable substitute. The runner's own post-processing line reported "First-party
coverage: lines 56029/65402 (85.67%), branches 13486/16892 (79.84%)", which reconciles with the six
root-element values above.

The total test count moved from 7212 at baseline to 7213. That is the net of this feature's test
changes: four tests added in
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs, and three deleted
(EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData,
TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult and
TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully), for a net gain of one.

D6's non-termination hazard did not occur; the run completed in about half a minute and no search
root was narrowed.
