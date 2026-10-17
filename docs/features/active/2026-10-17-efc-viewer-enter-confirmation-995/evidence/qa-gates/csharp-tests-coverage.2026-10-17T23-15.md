# Issue 995 csharp-tests-coverage

Timestamp: 2026-10-17T23-15
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-995-final.cobertura.xml; preserve complete report/results under SCRATCH; calculate per-file executable coverage
EXIT_CODE: 0
Output Summary: Repository line=86.0331%; branch=80.2210%; covered/valid=56935/66178; tests excluded by canonical collector and first-party processing.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: 30/30 = 100.0000%
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: 107/107 = 100.0000%
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: 239/241 = 99.1701%
QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs: 150/153 = 98.0392%
QuickFiler/Controllers/EfcFormController.Breadcrumb.cs: 78/115 = 67.8261%
QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs: 48/124 = 38.7097%
BreadcrumbDocumentAssets.cs: 0/0 executable lines, percentage N/A. Source contains only compile-time const strings; absent from instrumentation report. Renderer regression is required after the fix.
Coverage output: .\coverage\issue-995-final.cobertura.xml
dotnet-coverage v18.10.0.0 [win-x64 - .NET 10.0.12]
  Passed WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption [< 1 ms]
WARNING: Overwriting results file: .\coverage\test-results\mstest-coverage-run.trx
Results File: .\coverage\test-results\mstest-coverage-run.trx
Test Run Successful.
Total tests: 7471
     Passed: 7471
Code coverage results: .\coverage\issue-995-final.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 56935/66178 (86.03%), branches 13794/17195 (80.22%)
Coverage projection: .\coverage\issue-995-final.cobertura.jacoco.xml
Test-result summary: .\coverage\test-results\mstest-coverage-run.summary.txt
Done. Coverage artifact: .\coverage\issue-995-final.cobertura.xml
