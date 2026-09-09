Timestamp: 2026-09-09T10-06
Command: <vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing" /InIsolation /Logger:trx /ResultsDirectory:docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing
(vstest.console.exe resolved path: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe, per phase0-toolchain-paths artifact)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Total tests: 1. Passed: 0. Failed: 1. The failure is an unhandled
System.InvalidOperationException ("Operation is not valid due to the current state of the object")
thrown from the Moq stub on Ol.ArchiveRootPath and propagated out of
QuickFiler.Controllers.QfcItemController.AssignFolderComboBox() at
QuickFiler\Controllers\QfcItemController.FolderHandling.cs:line 231 (the unguarded read this plan
fixes), through the test's Act delegate, causing the FluentAssertions
`act.Should().NotThrow<InvalidOperationException>()` assertion to fail. TRX results file:
p2-t2-expect-fail.trx in the same evidence/regression-testing directory (renamed from vstest's
default account/host-identifier-bearing filename and sanitized in place per the repository's
no-absolute-host-paths rule; verified zero residual identifier matches and confirmed the file still
parses as well-formed XML with the expected single UnitTestResult entry). A vstest MSTest
`Deploy_<account> <timestamp>_<pid>` deployment directory was also produced by this run (expected
only on a failing run) and has been deleted; it carried no evidence this plan requires beyond the
TRX and this summary.

Acceptance: EXIT_CODE (1) equals the declared ExpectedExitCode (1) — the test fails pre-fix,
confirming the regression is real and reproducible.
