# P4-T14 — AC7 measurement-residue projection

Timestamp: 2026-09-13T23-50

## 1. The structural no-Form guard passed in every repetition

`UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`, named
by its fully qualified form because more than one test assembly declares a test of that short name,
is recorded **Passed** in each of the three repetitions:

| Repetition | Task | Outcome |
|---|---|---|
| 1 | P4-T6 | Passed |
| 2 | P4-T7 | Passed |
| 3 | P4-T8 | Passed |

That guard reflects over the types compiled into the test assembly, so its passing proves no
`Form`-derived type was compiled into `UtilitiesCS.Test.dll` despite the measurement constructing
one. The form the measurement constructs, `QuickFiler.Viewers.SyncContextForm`, is declared in the
production assembly `UtilitiesCS`, which is why the guard is not violated.

## 2. The form is disposed in a `finally`, with both properties set before `Show`

Quoted lines of the measurement method
`UtilitiesCS.Test.Threading.UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome`,
which occupies lines 123-162 of `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`:

```csharp
                    var form = new SyncContextForm();
                    try
                    {
                        form.ShowInTaskbar = false;
                        form.WindowState = FormWindowState.Minimized;
                        form.Show();
                    }
                    finally
                    {
                        form.Dispose();
                    }
```

`form.Dispose();` is inside the `finally` block, so the form is disposed whether `Show()` returns
or throws. `form.ShowInTaskbar = false;` and `form.WindowState = FormWindowState.Minimized;` both
precede `form.Show();`, mirroring what production initialization does at lines 73-75 of
`UtilitiesCS/Threading/UiThread.cs`, so a non-throwing call does not display a real window in an
unattended run.

## 3. The measurement creates no dispatcher and starts no message loop

This is a negative claim, so it is supported by a token count over the method's line range rather
than by a quoted line, because no quoted line can establish an absence.

- Line range measured: **123-162** of `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`.
- Count of lines in that range containing the token `Dispatcher`: **0**.
- Count of lines in that range containing the token `SharedStaDispatcherHost`: **0**.

The method therefore neither constructs a dispatcher host, nor reads `Dispatcher.CurrentDispatcher`
(which would create one), nor calls `Dispatcher.Run` (which would start a message loop). The two
negative predicate tests in the same file do use a dispatcher host, but they are different methods
outside this range.

## 4. The thread the measurement creates is joined before the test returns

The measurement method delegates thread creation and joining to the internal apartment thread
runner and contains no join of its own. The join is the line

```csharp
            thread.Join();
```

at **line 102** of `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, inside
`ApartmentThreadRunner.RunOnThread`. That method starts the thread, joins it, and only then returns
the captured exception, so `RunOnThread` cannot return before the thread has terminated. The
measurement method's next statement after the `RunOnThread` call is its first assertion, so no
thread it created is still alive when the test returns. The runner also sets `IsBackground = true`
before starting, so no non-background thread is ever created.

(A second `_thread.Join();` exists at line 160 of the same file, inside
`SharedStaDispatcherHost.Dispose`. It belongs to the dispatcher host used by the two negative
predicate tests, not to the measurement.)

## 5. No repetition stalled

Each of the three results directories under the gitignored coverage directory contains **no**
`Sequence_*.xml` file:

| Results directory | `Sequence_*.xml` count |
|---|---|
| `coverage\trx\p4-t6` | 0 |
| `coverage\trx\p4-t7` | 0 |
| `coverage\trx\p4-t8` | 0 |

The blame sequence document is what a stall produces, so its absence is an observation that can
fail. The absence of a *dump* file would not be: the blame collector is configured with
`HangDumpType=None`, so it writes no dump under any outcome and a stalled run would be equally
dump-free.

Repetition 1 additionally recorded the console line
`Data collector 'Blame' message: All tests finished running, Sequence file will not be generated.`
in the P1-T8 run of the same collector configuration, which is the collector's own statement that
it saw the run complete.

## 6. No live Outlook process was required

Evidenced by `evidence/qa-gates/p4-t1-outlook-precondition.md`, which records that
`Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue` returned no process object before the
Phase 4 rebuilds and runs. The whole delivery, including the measurement, ran with Outlook closed.

## FAIL determination

The run left no visible window (both display properties are set before `Show`), no live form (the
form is disposed in a `finally`), no un-shut dispatcher (the method creates none), and no
non-background thread alive (the runner sets `IsBackground` and joins). No FAIL condition of AC7 is
met.
