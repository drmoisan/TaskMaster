# Issue 995 csharp-tests-coverage

Timestamp: 2026-10-17T22-57
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-995-baseline.cobertura.xml; preserve complete report/results under SCRATCH; calculate per-file executable coverage
EXIT_CODE: 0
Output Summary: Repository line=86.0212%; branch=80.1898%; covered/valid=56897/66143; tests excluded by canonical collector and first-party processing.
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs: 30/30 = 100.0000%
UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs: 99/103 = 96.1165%
QuickFiler/Controllers/BreadcrumbBridgeRouter.cs: 237/239 = 99.1632%
QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs: 140/143 = 97.9021%
QuickFiler/Controllers/EfcFormController.Breadcrumb.cs: 62/97 = 63.9175%
QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs: 47/123 = 38.2114%
BreadcrumbDocumentAssets.cs: 0/0 executable lines, percentage N/A. Source contains only compile-time const strings; absent from instrumentation report. Renderer regression is required after the fix.
Coverage output: .\coverage\issue-995-baseline.cobertura.xml
dotnet-coverage v18.10.0.0 [win-x64 - .NET 10.0.12]
  Passed WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption [< 1 ms]
Results File: .\coverage\test-results\mstest-coverage-run.trx
Test Run Successful.
Total tests: 7450
     Passed: 7450
Code coverage results: .\coverage\issue-995-baseline.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 56897/66143 (86.02%), branches 13771/17173 (80.19%)
Coverage projection: .\coverage\issue-995-baseline.cobertura.jacoco.xml
Test-result summary: .\coverage\test-results\mstest-coverage-run.summary.txt
Done. Coverage artifact: .\coverage\issue-995-baseline.cobertura.xml
