# [P1-T14] Deduplicate the Iteration Test File Before It Absorbs New Tests

Timestamp: 2026-08-26T09-26

Task: [P1-T14]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` — extracted one shared
`ArrangeIterate(...)` helper from the five duplicated arrangements the plan names (the four
`IterateQueueAsync_*` setups formerly at `:84`, `:130`, `:201` and `:268`, and the `Iterate` setup
formerly at `:372`). The helper builds the `IQfcDatamodel`, `IQfcQueue`, `IQfcFormController` and
`IQfcCollectionController` mocks, wires them into `_controller` (including the `_formController`
private-field reflection assignment that appeared five times), and returns all four mocks as a
named tuple so each test keeps its own assertions.

No `Part2` partial file was added and `QuickFiler.Test/QuickFiler.Test.csproj` was not edited.
`using System.Linq.Expressions;` was added.

### Matchers are parameters, and the pins survive

`ArrangeIterate` takes the quantity and timeout matchers as
`Expression<Func<int, bool>>` parameters and applies them with `It.Is(quantity)` /
`It.Is(timeOut)`. Passing `It.IsAny<int>()` as an ordinary argument would evaluate to `0` before
the setup expression was built, so the expression form is what keeps a pinned call site pinned.

Post-change call sites, read directly from the post-change test bodies:

| Test (formerly at) | Post-change arguments | Pin preserved |
| --- | --- | --- |
| `IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems` (`:268`) | `quantity: q => q == 8, timeOut: t => t == 2000` | yes — `8` and `2000` are reproduced concretely, not widened to `It.IsAny<int>()` |
| `Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch` (`:372`) | `quantity: q => q == itemsPerIteration, timeOut: t => true, itemsPerIteration: itemsPerIteration` | yes — the `itemsPerIteration` argument is reproduced; only the timeout, which was `It.IsAny<int>()` at base, is unconstrained |
| `IterateQueueAsync_DataModelComplete` (`:84`) | `quantity: q => true, timeOut: t => true, complete: true` | n/a — was `It.IsAny<int>()` at base |
| `IterateQueueAsync_QueueEmpty` (`:130`) | `quantity: q => true, timeOut: t => true` | n/a — was `It.IsAny<int>()` at base |
| `IterateQueueAsync_Queue2` (`:201`) | `quantity: q => true, timeOut: t => true, dequeued: mailItems` | n/a — was `It.IsAny<int>()` at base |

`ArrangeIterate` currently arranges `DequeueNextItemGroupAsync` only, because
`DequeueNextItemGroupWithOutcomeAsync` does not exist until `[P1-T15]` declares it. `[P1-T15]`
adds the second arrangement to this same helper, which is what keeps
`Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch` on the synchronous-path member
while the four `IterateQueueAsync_*` tests move to the outcome-bearing member.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcHomeControllerIterationTests" "/Logger:trx;LogFileName=p1-t14.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t14"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t14/p1-t14.trx`

Total tests 8, Passed 8, **Failed 0**.

Command: `wc -l "QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs"`
EXIT_CODE: 0

| File | Recorded by `[P0-T14]` | Post-change | Condition | Result |
| --- | --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 464 | **428** | strictly less than 464 | PASS (36 lines freed) |

`git diff --stat` for the file: 97 insertions, 133 deletions.

## Output Summary

`ArrangeIterate` extracted and shared across all five arrangements the plan names. File drops from
464 to 428 lines, freeing 36 lines of headroom under the 500-line cap before `[P1-T16]`,
`[P1-T17]` and `[P1-T19]` add five more tests. Format EXIT_CODE 0, compile EXIT_CODE 0, scoped run
EXIT_CODE 0 with 8 passed and 0 failed. Both pinned call sites reproduce their concrete arguments.
