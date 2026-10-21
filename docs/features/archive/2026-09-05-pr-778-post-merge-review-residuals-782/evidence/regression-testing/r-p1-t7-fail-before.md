# [P1-T7] [expect-fail] Falsification — the constant-reference assertion fails on a restored tail

Timestamp: 2026-09-06T01-39

Command:

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1

& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-r1-p1t7' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:FullyQualifiedName~YieldAsync_WithoutDispatcher_RemainsStrict|FullyQualifiedName~Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize'
```

This is the [P1-T4] command with a different results directory, run with the [P1-T5] mutation in
place.

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary: one of the two tests failed, and it is the one whose throw site the mutation touched.
The counts below are read from the TRX `ResultSummary/Counters` element in
`TestResults\782-r1-p1t7`, which contains exactly one `.trx` file and records `outcome="Failed"` with
`error="0"`, `timeout="0"`, `aborted="0"`, `inconclusive="0"`, and `notExecuted="0"`.

```text
Total tests: 2
Passed: 1
Failed: 1
```

- **Failed:** `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`
- **Passed:** `UtilitiesCS.Test.Threading.UiThread_Dispatcher_Tests.Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`

## The FluentAssertions failure message, verbatim

```text
Expected the exception message to match the equivalent of

 "The UI dispatcher has not been captured. Call UiThread.Init() on the UI (STA) thread during host startup before reading UiThread.Dispatcher.",

but

 "The UI dispatcher has not been captured. Call UiThread.Init() on the UI (STA) thread during host startup before reading UiThread.Dispatcher. before yielding folder tree work"

does not.
```

The failure was raised from
`FluentAssertions.Specialized.ExceptionAssertions<T>.WithMessage`, reached from
`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:132`, which is the first line of
the chained assertion whose trailing call [P1-T2] rewrote. Absolute host paths from the stack trace
are not reproduced here; the frame is given as its repository-relative path.

## What this observation establishes

The expected value in the failure message is the shared constant's whole value, and the actual value
is that same text with the mutation's tail appended. The assertion compared the pattern against the
entire message and rejected it. **A caller-specific tail appended at the `WpfDispatcherYield` throw
site therefore fails this assertion.** That is the property AC10 now claims, observed rather than
derived.

The sibling assertion in `UtilitiesCS.Test/Threading/UiThread_Tests.cs` passed in the same run. That
is the expected outcome and it is what bounds the claim: the C20 test injects two null providers, so
it reaches the `WpfDispatcherYield` throw only, and a tail appended at the `UiThread.Dispatcher`
throw site would fail the sibling assertion instead of this one. Neither assertion covers the other
site.

## The leg that is derived and not observed

**No run of this mutation against the previous wildcard assertion was performed.** By derivation it
would not have failed: the mutated message is the constant plus a suffix, so it still contains the
substring `UiThread.Init()`, and the wildcard pattern `"*UiThread.Init()*"` matches any message
containing that substring. The pre-782 message recorded in
`evidence/remediation-baseline/r-p0-t4-pre782-message.md` contains the same substring, which is the
same reason the wildcard could not distinguish the delivered message from the pre-782 one. That is
the R3 defect.

This artifact states that leg as derived. It is not presented as an observed run.

## Constant declaration cited above

`UtilitiesCS/Threading/UiThread.cs:135-136` declares:

```csharp
        internal const string DispatcherNotInitializedMessage =
            "The UI dispatcher has not been captured. Call UiThread.Init() on the UI (STA) thread during host startup before reading UiThread.Dispatcher.";
```

The value contains no `*` and no `?`, so FluentAssertions compares it against the entire message
rather than treating any part of it as a wildcard.
