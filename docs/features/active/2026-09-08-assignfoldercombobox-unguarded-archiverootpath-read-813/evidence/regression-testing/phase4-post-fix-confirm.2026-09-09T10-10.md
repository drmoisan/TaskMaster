Timestamp: 2026-09-09T10-10
Command: "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing" /InIsolation /Logger:trx /ResultsDirectory:docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing
EXIT_CODE: 0
Output Summary: Total tests: 1. Passed: 1. Failed: 0. Test Run Successful. TRX file:
DanMoisan_MEGALODON4_2026-09-09_10_10_13_net481.trx.

Preconditions: QuickFiler.Test.csproj was rebuilt via
`msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`
immediately before this run (EXIT_CODE 0, Build succeeded, 0 Error(s)). Verbose build log confirmed
this Rebuild target performed a genuine Clean+Build of the full dependency graph, including
QuickFiler.csproj (which contains the Phase 3 fix in
QuickFiler/Controllers/QfcItemController.FolderHandling.cs), not an incremental up-to-date skip.
This confirms the test now passes against the fixed production code, closing the regression proven
failing in P2-T2.
