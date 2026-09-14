# P2-T6 — AC2, AC3, AC4 and AC5 pass-after projection

Timestamp: 2026-09-13T23-28

Command, verbatim:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t5-pass-after.trx" /ResultsDirectory:coverage\trx\p2-t5 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

## The seven tests, by fully qualified name, with outcome

| Fully qualified test | Outcome | Criterion |
|---|---|---|
| `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse` | Passed | AC2 pass-after |
| `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse` | Passed | AC3 |
| `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` | Passed | AC4 |
| `UtilitiesCS.Test.Threading.SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` | Passed | The in-repository guard against a bare owning-thread-identity predicate |
| `UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` | Passed | AC5 |
| `UtilitiesCS.Test.Threading.UiThreadInitRetryContract_Tests.Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` | Passed | AC5 |
| `UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome` | Passed | AC6, AC12 |

## The invariance control

`IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` is
recorded **Passed** in the P1-T8 fail-before run against the unmodified predicate and **Passed**
again in this run against the modified one. That pair is the invariance control: it shows the
change did not alter behaviour on the leg declared out of scope, the leg on which a caller is
genuinely standing on the thread that owns the captured dispatcher. Had the hardening broken that
leg, the two exits of the accessor would have disagreed in the opposite direction.

The same two runs record the two negative tests **Failed** then **Passed**, which is the red-green
pair AC2 and AC3 require.

## The message comparison AC5 requires

The assertion tightened by P2-T3 adds `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`.
The constraint is load-bearing only if the two sources of `InvalidOperationException` reachable from
`UiThread.Init()` carry different message text. They do.

**Source 1 — the non-STA rejection.** The message is produced by the private helper at lines
233-234 of the pre-change `UtilitiesCS/Threading/UiThread.cs`:

```csharp
private static string NonStaInitMessage(ApartmentState observed) =>
    NonStaInitMessagePrefix + observed;
```

It is the constant declared at lines 230-231 of that pre-change file — the declaration on line 230
and its string literal on line 231, line 232 being a blank separator — concatenated with the
observed apartment state. The constant is:

```csharp
internal const string NonStaInitMessagePrefix =
    "UiThread.Init() must be called on the UI (STA) thread during host startup. Observed apartment state: ";
```

Its text **begins** with the sentence stating that the initializer must be called on the UI STA
thread during host startup, and **ends** with the phrase reporting the observed apartment state.
The split exists so a test can assert the stable prefix without pinning how the `ApartmentState`
enum renders.

**Source 2 — the capture failure the fake produces.** Declared at lines 25-26 of
`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`:

```csharp
internal const string CaptureFailureMessage =
    "FakeUiCaptureSource was configured to fail during CaptureUiVariables().";
```

**The two texts are not equal.** They share no leading substring, so the added constraint
distinguishes the two sources of that exception type. Before P2-T3 the assertion was the
unconstrained `failing.Should().Throw<InvalidOperationException>();`, which could not tell a test
running under MTA — where the non-STA rejection fires before `Initialize()` ever runs — from a test
running under STA in which the fake's capture failure fires as intended. P2-T2 additionally makes
that apartment premise explicit by asserting
`Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA)` as the first Arrange
step of the same test.

The identical unconstrained statement in
`AutoScaleFactor_ReadFromMtaThreadAfterAFailedInit_ThrowsAndDoesNotReEnterTheFactory` was
deliberately left untouched; that test's assertion is out of scope for this delivery.
