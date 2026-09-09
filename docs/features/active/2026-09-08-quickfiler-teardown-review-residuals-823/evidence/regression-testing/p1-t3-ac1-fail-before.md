# Phase 1 — AC1 fail-before (expect-fail)

Timestamp: 2026-09-09T14-04

Task: [P1-T3] [expect-fail]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p1-t3' `
  '/TestCaseFilter:FullyQualifiedName~PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce'
```

EXIT_CODE: 1
ExpectedExitCode: 1

BASE-SHA: d636b0f28f548181685260d929de6d7d2940d1da

The SHA above is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md`. This run was executed on the tree with only the AC1
regression test added and no production change, which is the state that base ref plus the [P1-T1]
edit describes. It ran before any Phase 2 task, because a deliberately-failing test executed after
its fix has landed cannot produce the failure it is written to record.

TOTAL: 1
FAILED: 1

FAILURE-REASON: Store B's `ExchangeUser.PrimarySmtpAddress` getter was observed `Times.Never()`,
because the single `bool` field at `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:105`
had already been consumed by store A on the first `PopulateWithCurrent()` call, leaving the third
conjunct of the retry gate false when store B was displayed. The verbatim Moq mismatch message the
run printed was:

```
Test method UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperController_Tests.PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: x => x.PrimarySmtpAddress

Performed invocations:

   Mock<ExchangeUser:2> (x):
   No invocations performed.
```

`Mock<ExchangeUser:2>` is store B's mock, the second `ExchangeUser` the test constructs; store A's
mock recorded its one invocation and its own `VerifyGet` therefore passed. The failing frame is the
`exchangeUserB.VerifyGet` call in
`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`. The absolute paths
that appeared in the printed stack trace are not reproduced here (D14).

Output Summary: `Test Run Failed.` 1 test total, 1 failed, in 1.69 seconds. Exit code 1, matching
the declared expectation. The failure is the intended per-store defect: store B's SMTP lookup never
ran because store A had already consumed the single controller-scoped retry flag.
