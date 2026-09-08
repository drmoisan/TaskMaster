# P2-T4 — AC2 fail-before evidence [expect-fail]

Timestamp: 2026-09-08T09-45
Task: [P2-T4]
Command: <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t4.trx" /ResultsDirectory:coverage/trx/p2-t4 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:FullyQualifiedName~DfDeedleEtlTimeoutTests"
EXIT_CODE: 1
ExpectedExitCode: 1

A failing test is the expected and required outcome of this task. The paired pass-after record is
`p3-t2-ac2-pass-after.md`, produced after P3-T1 inserts the guard.

## TRX counters

`coverage/trx/p2-t4/p2-t4.trx` exists.

```
total=2 executed=2 passed=1 failed=1 error=0 timeout=0 aborted=0 notExecuted=0
```

Expected: `total`=2, `passed`=1, `failed`=1. Observed exactly that.

## Named outcomes

| Method | Expected | Observed |
|---|---|---|
| `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` | `Failed` | `Failed` |
| `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` | `Passed` | `Passed` |

Neither returned `ABSENT`, so both tests were discovered and executed: the new file is registered
in the project file and compiled into the assembly.

## Failure message, verbatim after redaction

The worktree path is redacted to `<worktree>` and the account name to `<user>`; nothing else is
altered.

```
Expected a <System.InvalidOperationException> to be thrown, but found <System.NullReferenceException>:
System.NullReferenceException: Object reference not set to an instance of an object.
   at UtilitiesCS.DfDeedle.<GetEmailDataInViewAsync>d__8.MoveNext() in <worktree>\UtilitiesCS\Extensions\DfDeedle.cs:line 179
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at FluentAssertions.Specialized.AsyncFunctionAssertions`2.<InvokeWithInterceptionAsync>d__15.MoveNext() in /_/Src/FluentAssertions/Specialized/AsyncFunctionAssertions.cs:line 373.
```

The TRX text contains `System.NullReferenceException` inside that result's `ErrorInfo/Message`;
FluentAssertions reports the found exception type, which is exactly the discriminating signal the
acceptance condition asks for. A test that merely asserted `Should().Throw<Exception>()` would
have passed here and proved nothing.

## The reported line is the defect site

`DfDeedle.cs:179` in the current tree is the opening line of the `LogDfTiming(` invocation whose
dereference `tableSnapshot.Item1.GetLength(0)` sits on line 181. This is the same statement the CI
stack trace reported as `DfDeedle.cs:186` in the pre-change tree; the site moved from 186 to 179
because P1-T3 deleted 14 lines of static-seam declarations above it and P1-T2 added 4 lines below
that. The .NET sequence point for a multi-line invocation is the line the statement opens on,
which is why both traces name the opening line rather than the dereference line.

The mechanism is the one the research artifact derived: the 250 ms ETL hop deadline expires on the
injected clock, `EtlByRowAsync` throws `TimeoutException`, `EtlAsync` swallows it at its
`catch (TimeoutException)` block, `data` stays null, and `return (data!, columnDictionary)` forces
that null through a null-forgiving suppression into a non-nullable tuple element. The guard P3-T1
adds goes between lines 178 and 179, ahead of the whole call.

## Blame hang guard

SEQUENCE_FILES: 0

No `Sequence` file was written under `coverage/trx/p2-t4`, so the 4-minute blame hang timeout did
not fire. The test reached its assertion and failed on the assertion, rather than hanging: this is
what confirms the D5 arming order held and that the clock was advanced only after the 250 ms timer
existed.

## Acceptance evaluation

- `EXIT_CODE: 1` with `ExpectedExitCode: 1`. PASS
- Counters `total`=2, `passed`=1, `failed`=1. PASS
- `Read-TrxOutcome` for the expiry test is `Failed` and its `ErrorInfo/Message` contains
  `System.NullReferenceException`. PASS
- `Read-TrxOutcome` for the green test is `Passed`. PASS
- No `Sequence` file under the results directory, so the blame hang timeout did not fire. PASS
- The failure message is quoted verbatim after redaction. PASS

## Output Summary

RED established for AC2. The new deadline-expiry test fails with `NullReferenceException` where it
asserts `InvalidOperationException`, at the exact production site the issue reports. The companion
green-path test passes on the same un-advanced clock, which shows the failure is caused by the
expired deadline and not by the test arrangement.
