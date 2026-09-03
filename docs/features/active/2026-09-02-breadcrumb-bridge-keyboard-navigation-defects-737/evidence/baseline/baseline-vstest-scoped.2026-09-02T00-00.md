Timestamp: 2026-09-03T01-27

Command: vstest.console.exe (vswhere-resolved from
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe")
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings
/InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbHtmlRendererTests"
"/Logger:trx;LogFileName=baseline-scoped.trx"
/ResultsDirectory:docs\features\active\2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737\evidence\baseline\testresults

EXIT_CODE: 0

Output Summary: "Test Run Successful." followed by "Total tests: 40" and "Passed: 40".
This matches the plan's stated expected baseline of Total: 40 (14 in
FolderBreadcrumbBridgeRouterTests.cs, 12 in the sibling partial
FolderBreadcrumbBridgeRouterInFlightTests.cs, 14 in BreadcrumbHtmlRendererTests.cs),
Failed: 0. TRX written to
evidence\baseline\testresults\baseline-scoped.trx.
