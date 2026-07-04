Timestamp: 2026-07-04T11-57-04:00
Command: dotnet-coverage collect --output docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-11-30-vstest.cobertura.xml --output-format cobertura --settings coverage.config -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation; post-process with scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1.
EXIT_CODE: 0
Output Summary:
- dotnet-coverage version: 18.5.2.
- VSTest version: 18.7.0 (x64).
- Test run status: successful.
- Total tests: 385.
- Passed: 385.
- Generated Cobertura: docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\remediation-11-30-vstest.cobertura.xml.
- Generated Cobertura raw summary after repository helper post-processing: 9960/87965 = 11.32%.
- New/changed non-COM-bound gate coverage: PASS for QuickFiler\Controllers\QfcStreamingDequeueConfidenceGate.cs at 57/60 = 95.00%.
- Corrected repository-wide coverage interpretation from approved exception input: 76.2%.
- Repository-wide 80% floor status: FAIL; 76.2% remains below 80%.
- No-regression status: PASS by approved exception input assertion.
- AC10 status: PASS by one-time approved exception disposition plus final toolchain, new-code coverage, and no-regression evidence.

Numeric Coverage Values:
- Corrected repository-wide coverage: 76.2%.
- Repository-wide threshold: 80%.
- Repository-wide threshold result: FAIL.
- Generated Cobertura raw lines: 9960/87965 = 11.32%.
- QuickFiler\Controllers\QfcStreamingDequeueConfidenceGate.cs: 57/60 = 95.00%.
- QuickFiler\Controllers\QfcHomeController.cs: 165/244 = 67.62% by line count; Cobertura line-rate reported 71.00%.
- QuickFiler\Controllers\QfcHomeController.Iteration.cs: 45/56 = 80.36% by line count; Cobertura line-rate reported 86.00%.
- QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs: not reported as a distinct Cobertura class/file entry.

AC10 Disposition:
- CSharpier gate: PASS after scoped CSharpier 1.2.6 command-syntax correction.
- Analyzer build: PASS with 0 warnings and 0 errors.
- Nullable/warnings-as-errors build: PASS with 0 warnings and 0 errors.
- MSTest with coverage: PASS with 385/385 tests passed.
- New code coverage: PASS at 95.00%, above the 90% target.
- Repository-wide floor: FAIL because 76.2% is below 80%.
- No regression: PASS by approved exception input assertion.
- Overall AC10: PASS only because the approved one-time exception authorizes disposition of the pre-existing repository-wide below-threshold condition.

Exception Evidence:
- docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/remediation-11-30-ac10-approved-exception.md.
- This comparison does not record repository-wide coverage as passing the 80% floor.
