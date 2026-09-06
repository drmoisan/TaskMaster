# [P2-T15] Whole `QuickFiler.Test` assembly after the fix

Timestamp: 2026-09-06T14-59

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p2-t15b' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
This is the same command and the same switches as the [P0-T10] baseline, differing only in the
results directory.

EXIT_CODE: 0

POST-QFT-TOTAL: 1362
POST-QFT-PASSED: 1362
POST-QFT-FAILED: 0

NEWLY-FAILING: NONE

Output Summary: `Test Run Successful. Total tests: 1362, Passed: 1362, Total time: 12.1509 Seconds.`

## Comparison against the [P0-T10] baseline

| Measure | Baseline [P0-T10] | This run | Relation |
|---|---|---|---|
| Total | 1339 | 1362 | +23 |
| Passed | 1339 | 1362 | +23 |
| Failed | 0 | 0 | `POST-QFT-FAILED <= BASELINE-QFT-FAILED` holds (0 <= 0) |

The +23 is exactly the tests this plan added: seven in
`QfcStreamingDequeueConfidenceGateTests.Part4.cs` ([P1-T6]), eight in
`QfcFormControllerCancelTeardownTests.cs` ([P1-T12]), two in `QfcHomeControllerCleanupTests.cs`
([P1-T13]), five in `QfcDatamodelTeardownTests.cs` ([P1-T14]) and one added to
`QfcHomeControllerIterationTests.cs` ([P1-T11]). The retargeted tests were renamed rather than
added, so they do not change the total.

`NEWLY-FAILING: NONE` is a substantive determination, not a vacuous one: the baseline failure set was
empty, so any failure in this run would be newly failing by construction, and the first execution of
this task did surface one (below).

## The one newly-failing test surfaced by the first execution, and its repair

The first execution of this command exited 1 with `Total tests: 1362, Passed: 1361, Failed: 1`. The
single failure was a pre-existing architecture pin, not one of this plan's tests:

`QuickFiler.Controllers.Tests.QfcMoveMonitorTopologyTests.NoTypeDeclaresMoreThanOneEmailMoveMonitorField`
— `Expected declaringTypes to contain 3 item(s) because issue #731 finding 1 pins the three-owner
topology ... but found 4: {"QuickFiler.Controllers.QfcCollectionController",
"QuickFiler.Controllers.QfcDatamodel", "QuickFiler.Controllers.QfcQueue",
"QuickFiler.Controllers.QfcDatamodel+<TryQueueRemainingMailItemAsync>d__58"}`.

Cause: that test reflects over every type in the QuickFiler assembly and counts types declaring an
`IEmailMoveMonitor` field. [P2-T5] introduced an `IEmailMoveMonitor moveMonitor` local inside the
`async` `TryQueueRemainingMailItemAsync`, and the C# compiler hoists locals in an `async` method into
the generated state-machine type as fields. The state machine
`QfcDatamodel+<TryQueueRemainingMailItemAsync>d__58` therefore became a fourth declaring type. The
production topology was never actually changed — no new monitor instance exists — but the pin reads
declared fields, not instances, so it reported a real violation of what it pins.

Repair, applied as a micro-action inside [P2-T5]: the snapshot and the guard were moved into a new
private **synchronous** helper `TryCreateRemainingQueueAdmission(CancellationToken)`, which returns
the constructed `QfcRemainingQueueAdmission` or `null`. `TryQueueRemainingMailItemAsync` calls it,
returns `false` on null, and otherwise awaits `TryQueueAsync` as before. A synchronous method has no
state machine, so the `IEmailMoveMonitor` local is a stack slot and declares no field. The guard
semantics, the snapshot semantics and the three-delegate
`QfcRemainingQueueAdmission` constructor shape are all unchanged; only where the two locals live
changed.

`EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer`, the source-text sibling pin in the same
class, passed in both executions.

After the repair the assembly was rebuilt (`Build succeeded. 0 Warning(s) 0 Error(s)`) and this
command was re-run from the start, producing the 1362/1362 result recorded above. [P2-T14] was also
re-run verbatim against the repaired build and reproduced its 76/76 result.
