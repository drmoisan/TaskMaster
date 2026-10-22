# AC5 (issue #809) — runtime apartment measurement

Timestamp: 2026-09-13T23-29

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t5-pass-after.trx" /ResultsDirectory:coverage\trx\p2-t5 "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

## The two recorded values

```
MTA_GUARD_APARTMENT: MTA
MTA_INITIALIZE_OUTCOME: COMPLETED
```

**Guard line:** `MTA_GUARD_APARTMENT: MTA`. The value is **MTA**, so the probe measured what it was
built to measure and the run is not void.

**Settling line:** `MTA_INITIALIZE_OUTCOME: COMPLETED`. The apartment thread runner returned null,
meaning the delegate completed normally: constructing `QuickFiler.Viewers.SyncContextForm`, setting
`ShowInTaskbar` to false and `WindowState` to minimized, and calling `Show()` on a thread whose
apartment is MTA did **not** throw on this host. There is no `THREW` branch to report and therefore
no exception type and no exception message.

## Source of the two values

Read from the **standard-output section of the measurement test in the run document written by
P2-T5**, under the gitignored coverage directory. That section carried output, so the fallback
re-run described in P3-T1 (with the log file name `p3-t1-measurement.trx`, the results directory
`coverage\trx\p3-t1`, a detailed console logger, and the test-case filter replaced by a
class-scoped one) was not needed and was not performed. The test is recorded **Passed** in that
same run.

## How the apartment was obtained

The value was **read on the executing thread at runtime**, by the statement

```csharp
measuredApartment = Thread.CurrentThread.GetApartmentState();
```

which is the first statement inside the delegate that
`UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome`
hands to the internal apartment thread runner with `ApartmentState.MTA` requested. The runner
creates a dedicated background thread, calls `SetApartmentState` on it before starting it, and joins
it before returning.

The apartment was **not** derived from a settings file, **not** derived from the assembly-level
parallelization attribute, and **not** derived from documented framework behaviour for the STA
test-class attribute. That is the precise defect of the earlier probe on issue #809, whose
conclusion was withdrawn because it inferred the apartment of its own thread from a research premise
that the same delivery falsified. The value above rests on nothing but the read performed on the
thread that ran the delegate.

The test asserts the guard value and records the settling value without asserting it, because
clause (i) of issue #809's AC5 is a measurement obligation rather than a gate.

## Scope note

This measurement records what the production capture form does when shown on an MTA thread. It is
not a statement about `UiThread.Init()`, which rejects a non-STA caller before `Initialize()` runs;
the measurement deliberately constructs the form directly so that no revert of the threading source
is required to take it.
