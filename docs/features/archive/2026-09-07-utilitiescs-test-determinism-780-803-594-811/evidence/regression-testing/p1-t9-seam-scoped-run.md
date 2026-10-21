# P1-T9 — Scoped run of every class the seams touch

Timestamp: 2026-09-08T09-38
Task: [P1-T9]
Command: <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t9.trx" /ResultsDirectory:coverage/trx/p1-t9 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:FullyQualifiedName~DfDeedle_COM_Tests|FullyQualifiedName~DfDeedleQfcColumnTimeoutTests|FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~TimeOutTask_Tests"
EXIT_CODE: 0

The filter uses `FullyQualifiedName~<Class>` joined with `|` rather than the full-run filter,
because a run scoped to `UtilitiesCS.Test.dll` alone cannot reach the repository's only
`[TestCategory("LiveOutlook")]` test, which lives in `TaskMaster.Test`.

## TRX counters

```
total=179 executed=179 passed=179 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

No result carried an outcome other than `Passed`.

## Named outcomes

| Class | Method | Outcome |
|---|---|---|
| `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests` | `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` | `Passed` |
| `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests` | `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` | `Passed` |

The second outcome is the load-bearing one for P1-T6: it proves the `ArmingBarrierTimeProvider`
still drives the #798 deadline tests correctly after being moved out of
`DfDeedleQfcColumnTimeoutTests.cs` into the shared
`UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs`. The move was a pure extraction with
no member-body change, and `NewSignal` still resolves from `ColumnAdderProbe` at the source file's
former lines 95 and 103 through the added `using UtilitiesCS.Test.TestHelpers;`.

`TimeOutTask_Tests` is included in the filter because
`UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs:197,210` still call the inert `(int, int)`
`TimeoutAfter` overloads, which this change deliberately leaves in place (D9, spec non-goal). The
class passing proves those untouched overloads still bind after the call sites in
`OlTableExtensions.Etl.cs` and `DfDeedle.cs` moved to the `(int, TimeProvider?)` overload.

## Blame hang guard

SEQUENCE_FILES: 0

No `Sequence` file was written under `coverage/trx/p1-t9`, so the 4-minute blame hang timeout did
not fire on any test.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- Counters recorded. PASS
- `failed` = 0. PASS
- `Read-TrxOutcome` returned `Passed` for both named tests. Neither returned `ABSENT`, so both
  were discovered and executed. PASS

## Output Summary

179 tests across the four seam-affected classes, all passed, exit 0. Both named sentinel tests
passed. The Phase 1 seam work introduced no behaviour change detectable by the existing suite,
which is the intended outcome: every new parameter defaults to the value that reproduces the
previous behaviour.
