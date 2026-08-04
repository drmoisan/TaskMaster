# P8-T48 configured JunkCertain fail-before

Timestamp: 2026-07-27T03-11Z

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.EmailIntelligence.SpamBayesActionsRegressionTests.GetDestinationFolder_WhenSpamTrueAndJunkCertainExists_ReturnsConfiguredJunkCertain /Logger:console;verbosity=detailed`

EXIT_CODE: 1

Output Summary: Exactly one test was discovered; zero passed, the intended configured-JunkCertain assertion failed, and zero tests were skipped. The failure observed the mail item's current parent rather than the configured JunkCertain instance.

## Intended assertion failure

`Expected result to refer to Mock<Folder:1>.Object, but found Mock<Folder:2>.Object.`

The test failed at `SpamBayesActionsRegressionTests.cs:24`, where it asserts the configured JunkCertain object. No unrelated test, crash, timeout, source change, or coverage command occurred.

The distinct P8-T44 four-failure baseline remains preserved at `p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-59.md` and its canonical TRX.
