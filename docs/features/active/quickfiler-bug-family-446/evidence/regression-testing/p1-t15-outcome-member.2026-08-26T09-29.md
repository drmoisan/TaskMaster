# [P1-T15] Outcome-Bearing Interface Member with a Stubbed Stop Reason

Timestamp: 2026-08-26T09-29

Task: [P1-T15]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

### Interface

`QuickFiler/Interfaces/IQfcDatamodel.cs` — declared

```csharp
Task<QfcDequeueBatch> DequeueNextItemGroupWithOutcomeAsync(
    int quantity,
    int timeOut,
    TimeSpan firstBatchDeadline,
    Action<int, int, int> progress
);
```

The three pre-existing members (`DequeueNextItemGroupAsync(int, int)`, the four-argument
`DequeueNextItemGroupAsync` overload and `DequeueNextItemGroup(int)`) are byte-unchanged; the new
member and its doc comment are inserted between the four-argument overload and
`DequeueNextItemGroup`.

### Implementation

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` — implemented by delegating to the
existing four-argument path for the items and returning a `QfcDequeueBatch` whose `Stop` is
hard-coded to `QfcDequeueStop.QuantitySatisfied`, per D-Plan-1. `using QuickFiler.Interfaces;`
added.

### Caller

`QuickFiler/Controllers/QfcHomeController.Iteration.cs` — `IterateQueueAsync` now calls the new
member and reads `batch.Items`:

```csharp
QfcDequeueBatch batch = await _datamodel.DequeueNextItemGroupWithOutcomeAsync(
    _formController.ItemsPerIteration,
    2000,
    QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
    null
);
IList<MailItem> listObjects = batch.Items;
```

The `else` branch (`await QfcQueue.CompleteAddingAsync(Token, 10000);`) is left unconditional, as
this task requires; `[P2-T7]` adds the `SourceExhausted` guard. `using QuickFiler.Interfaces;`
added.

### Tests

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`:

- `ArrangeIterate` (added by `[P1-T14]` and shared with
  `Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch`) now arranges **both** members.
  It keeps its `DequeueNextItemGroupAsync(It.Is(quantity), It.Is(timeOut))` setup — which is what
  `Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch` consumes, since that test drives
  the synchronous `Iterate()` path this plan does not change — and adds a
  `DequeueNextItemGroupWithOutcomeAsync` setup returning
  `new QfcDequeueBatch(batch, null, QfcDequeueStop.QuantitySatisfied)`.
- The three `mockDataModel.Verify` expressions, located by the `mockDataModel.Verify(` call opening
  `m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` rather than by line, were
  retargeted to the new member with their `Times` arguments preserved:

| Test | `Times` at base | `Times` post-change | Member named |
| --- | --- | --- | --- |
| `IterateQueueAsync_DataModelComplete` | `Times.Never` | `Times.Never` | `DequeueNextItemGroupWithOutcomeAsync` |
| `IterateQueueAsync_QueueEmpty` | `Times.Once` | `Times.Once` | `DequeueNextItemGroupWithOutcomeAsync` |
| `IterateQueueAsync_Queue2` | `Times.Once` | `Times.Once` | `DequeueNextItemGroupWithOutcomeAsync` |

- The two synchronous `DequeueNextItemGroup` verifications (base `:343` and `:392`, now `:316` and
  `:357`) are untouched.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Interfaces/IQfcDatamodel.cs" "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs" "QuickFiler/Controllers/QfcHomeController.Iteration.cs" "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerIterationTests" "/Logger:trx;LogFileName=p1-t15.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t15"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t15/p1-t15.trx`

Total tests 8, Passed 8, **Failed 0**.
`Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch` Passed.

Post-change line counts (all under the 500-line cap):
`QfcHomeControllerIterationTests.cs` 456, `QfcDatamodel.QueueProcessing.cs` 230,
`IQfcDatamodel.cs` 133, `QfcHomeController.Iteration.cs` 90.

## Output Summary

The outcome-bearing member exists end to end (interface, datamodel, `IterateQueueAsync` caller) with
its stop reason stubbed to `QuantitySatisfied`. Format EXIT_CODE 0, compile EXIT_CODE 0, scoped run
EXIT_CODE 0 with 8 passed and 0 failed. `ArrangeIterate` arranges both dequeue members, so the
high-confidence synchronous `Iterate` test keeps its `DequeueNextItemGroupAsync` arrangement, and all
three retargeted verifications name `DequeueNextItemGroupWithOutcomeAsync` with `Times.Never`,
`Times.Once` and `Times.Once` preserved.
