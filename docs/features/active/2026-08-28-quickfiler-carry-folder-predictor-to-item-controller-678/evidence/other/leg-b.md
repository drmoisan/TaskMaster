# P1-T6 — Leg B, every subsequent page (AC6)

Timestamp: 2026-09-01T22-58

## What was implemented

`QfcHomeController.IterateQueueAsync` (`QuickFiler/Controllers/QfcHomeController.Iteration.cs`) read
only `batch.Items` at `:28` and called `EnqueueAsync` at `:33`. It now also forwards
`batch.PreScored` as the third argument, so the carriers the dequeue-time gate produced reach the
background queue instead of being dropped at that hop.

`IQfcQueue.EnqueueAsync` and `QfcQueue.EnqueueAsync` gained a third parameter
`IList<QfcPreScoredItem> preScored`. `QfcQueue.LoadControllersViewersAsync` gained the same
parameter and, for each row, resolves the carried handler, stores it on the `QfcItemGroup`, and
passes it into the item-controller construction.

The parameter is **required rather than optional** on both the interface and the implementation.
That is deliberate and is documented in the interface: C# forbids omitting an optional argument
inside an expression tree (CS0854), so an optional parameter could not be named in the existing Moq
`Setup` and `Verify` expressions and the collateral edit would have been unavoidable anyway, without
the compiler pointing at every site. Callers outside high-confidence mode pass the value they have,
which is an empty list.

## The seam introduced

`QfcQueue.ItemControllerFactory`, declared in the new part
`QuickFiler/Controllers/QfcQueue.Enqueue.cs`. It is the **injectable-delegate seam**, form 2 of
`.claude/rules/csharp.md:52`, mirroring the existing `ScoringServiceFactory` pattern at
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:260-261`. **No new interface is introduced**,
as AC6 requires: the seam is a `Func<>` property.

**The seam has a production default that preserves the current construction expression.** The
default lambda reproduces the previous `new QfcItemController(...)` call argument for argument, in
the same order, with the same named-argument spellings, and appends only
`carriedFolderHandler: carriedHandler`. A queue that no test configures therefore constructs rows
exactly as it did before the seam existed. This is asserted, not merely stated:
`ItemControllerFactory_OnAFreshQueue_HasANonNullProductionDefault` in
`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` constructs a fresh `QfcQueue` and asserts the
property is non-null, so a regression that left the default null would fail rather than silently turn
the seam into a behaviour change.

A second helper, `QfcQueue.ResolveCarriedHandler`, was added as an `internal static` pure function.
It matches a carrier to its mail item by `EntryID` rather than by position, because
`UnhookDequeuedNodes` can replace an element of the item list in place and positional matching would
then pair a row with another row's handler. Being pure and static, it is directly unit-testable with
no WinForms or COM.

## The tests that drive the seam

| Test | File | What it pins |
|---|---|---|
| `IterateQueueAsync_WhenBatchCarriesPreScoredItems_ForwardsCarriersToEnqueue` | QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.Part2.cs | The leg-B hop itself: `IterateQueueAsync` forwards `batch.PreScored` to `IQfcQueue.EnqueueAsync` intact — same count, same handler instance paired with the same mail item. This is the assertion that fails if the forwarding is removed. |
| `ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler` | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | The resolver returns the handler belonging to the matching item, using a carrier list ordered so a positional implementation would return the wrong handler. |
| `ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull` | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | Five negative cases (null list, empty list, null mail item, empty `EntryID`, absent item) all yield null, which is the pre-change behaviour for every row. |
| `ItemControllerFactory_OnAFreshQueue_HasANonNullProductionDefault` | QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | The seam's production default exists. |

### What remains unpinned by a test, stated plainly

The single statement inside `LoadControllersViewersAsync` that passes
`x.grp.CarriedFolderHandler` into `ItemControllerFactory` is not covered by a test. Reaching it
requires executing `AddAsync`, which dequeues a live WinForms `ItemViewer`, and the repository unit
test policy prohibits a test that requires a real window. The two ends of that statement are pinned
instead — the resolver that produces the value, and the constructor that stores it
(`PredeterminedFolderConstructor_StoresPredeterminedFolder`, extended by P1-T5) — but the joining
statement itself is not. This is recorded rather than papered over.

## Acceptance conditions

### 1. The analyzer build exits 0

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0, `5 Warning(s)`, `0 Error(s)`, no coded warning, `CoreCompile:` ran 59 times.
The nullable build was also run: EXIT_CODE 0, `0 Error(s)`, no `CS86` diagnostic.

### 2. `QuickFiler/Controllers/QfcQueue.cs` is at or below its `BASELINE_SIZE_CENSUS` count of 610

| File | Baseline | After | Verdict |
|---|---:|---:|---|
| QuickFiler/Controllers/QfcQueue.cs | 610 | **505** | Below the census value **and** below 500 is not required for it, but it is in fact now below 500 as well. |
| QuickFiler/Controllers/QfcQueue.Enqueue.cs | new | 214 | Under 500. |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 95 | 98 | Under 500. |
| QuickFiler/Controllers/IQfcQueue.cs | 42 | 53 | Under 500. |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 497 | 477 | Under 500. |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.Part2.cs | new | 101 | Under 500. |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 262 | 361 | Under 500. |

Achieved as the plan directs: `EnqueueAsync` (was `:211`) and `LoadControllersViewersAsync` (was
`:380`, the member whose body contains the `new QfcItemController(` construction at `:405`) were both
moved **in full** into the new part. Each gains a parameter or argument on its own line under
CSharpier, a widened signature cannot be split across parts, and the construction at `:405` sits
inside a lambda in that member's body so it is not a relocatable unit on its own.

`partial` was added to the declaration at `QuickFiler/Controllers/QfcQueue.cs:20`, which is
`public class QfcQueue(` and carries a primary constructor. **The primary constructor's parameter
list remains on that part alone**; the new part declares `public partial class QfcQueue` with no
parameter list, which is the only legal form. `<Compile Include="Controllers\QfcQueue.Enqueue.cs" />`
was added to `QuickFiler/QuickFiler.csproj`.

### 3. The seam has a production default that preserves the current construction expression

Stated and asserted above.

### 4. Every named site in `QfcHomeControllerIterationTests.cs` is recorded as unchanged or rewritten, and no test in that file is left failing

| Baseline site | Disposition | Reason |
|---|---|---|
| `IQfcQueue.EnqueueAsync` setup at `:133` | **Rewritten** | Gained `It.IsAny<IList<QfcPreScoredItem>>()` as the third matcher. Required by the signature change; an omitted optional argument is illegal in an expression tree (CS0854). Behaviour of the setup is unchanged: it still matches any invocation. |
| `IQfcQueue.EnqueueAsync` verification at `:175` (inside `VerifyEnqueue`) | **Rewritten** | Same third matcher added, same reason. The helper still verifies the unconstrained invocation count, so no assertion was narrowed or widened. |
| `IQfcQueue.EnqueueAsync` verification at `:282` (inside `IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems`) | **Rewritten and relocated** | Same third matcher added. The whole test moved to `QfcHomeControllerIterationTests.Part2.cs` because the base file stood at 497 lines with three lines of headroom and this task adds both a widening and a new test. Its two existing constraints, the exact item sequence and the exact collection controller, are byte-identical after the move. |
| `DequeueNextItemGroupWithOutcomeAsync` setup at `:118` | **Unchanged** | The dequeue member and its four arguments are untouched by leg B. |
| `DequeueNextItemGroupWithOutcomeAsync` verification at `:194` | **Unchanged** | Same. |
| `DequeueNextItemGroupWithOutcomeAsync` verification at `:221` | **Unchanged** | Same. |
| `DequeueNextItemGroupWithOutcomeAsync` verification at `:253` | **Unchanged** | Same. |

The four `DequeueNextItemGroupWithOutcomeAsync` sites are still present at `:118`, `:196`, `:223`
and `:255` after the file shrank by the relocation; the shift is the relocation, not an edit to
them.

**No test in that file is left failing.** Scoped Derivation D7 run:

```
/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~QfcHomeControllerIterationTests
/ResultsDirectory:TestResults\p1-t6-iteration
```

EXIT_CODE: 0. `Total tests: 14`, `Passed: 14`, `Test Run Successful.` The 14 include both relocated
and new tests: `IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems` and
`IterateQueueAsync_WhenBatchCarriesPreScoredItems_ForwardsCarriersToEnqueue`. The preceding
`msbuild /t:Build` exited 0, so the run read a current assembly.

A second scoped run covered the queue-level tests:

```
/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~QfcQueuePurePathsTests
/ResultsDirectory:TestResults\p1-t6-queue
```

EXIT_CODE: 0. `Total tests: 10`, `Passed: 10`, `Test Run Successful.`, including all three new
issue #678 tests.

Both TRX files were written under `TestResults\`, which is git-ignored (`.gitignore:39`), and are
referenced by results directory only; no absolute host path, account name or machine name is
recorded here.

## New `<Compile Include>` entries

- `QuickFiler/QuickFiler.csproj`: `Controllers\QfcQueue.Enqueue.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`: `Controllers\QfcHomeControllerIterationTests.Part2.cs`
