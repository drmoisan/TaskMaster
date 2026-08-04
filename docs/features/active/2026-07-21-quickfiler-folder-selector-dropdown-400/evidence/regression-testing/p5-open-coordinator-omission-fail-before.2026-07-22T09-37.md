# P5 Open Coordinator Omission Failure-First Test

Timestamp: 2026-07-22T09:37:26.8293014Z

Command: `& $vstest $assembly /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests" '/Logger:console;Verbosity=detailed'`, where `$vstest` resolved through the latest Visual Studio installation and `$assembly` resolved to `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.

EXIT_CODE: `1` (expected failure)

Output Summary: VALID EXPECTED FAILURE. VSTest discovered exactly five cases. The four pre-existing contract cases passed. Only `HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator` failed. Its aggregated FluentAssertions message identified both intended omissions: `QuickFiler.Viewers.BreadcrumbDropDownOpenCoordinator` was absent and `ItemViewer.OpenBreadcrumbDropDownAsync` was still declared. There was no compilation failure, zero-test result, skipped case, or unrelated failure.

Inventory: `5 total; 4 passed; 1 failed; 0 skipped`.
