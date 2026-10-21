# P1-T5 — Leg A, the first page (AC4, AC5)

Timestamp: 2026-09-01T22-52

## What was implemented

### AC4 — `RunAsync` selects the outcome-returning dequeue and the carrier overload

`QuickFiler/Controllers/QfcHomeController.cs`, inside `RunAsync`:

- The high-confidence-enabled branch now calls `_datamodel.DequeueNextItemGroupWithOutcomeAsync`,
  declared at `QuickFiler/Interfaces/IQfcDatamodel.cs:113`, in place of the four-argument
  `DequeueNextItemGroupAsync`. The four arguments are unchanged
  (`itemsPerIteration`, `200`, `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`,
  `scanProgress.Report`), so the issue #424 deadline bound and the 0-to-30 progress band are
  preserved exactly. The outcome member is the only one that surfaces `QfcDequeueBatch.PreScored`.
- `listEmail` is taken from `batch.Items` and a new local `preScored` from `batch.PreScored`.
- The load call, previously the unconditional `await _formController.LoadItemsAsync(listEmail);` at
  `QuickFiler/Controllers/QfcHomeController.cs:307`, is now a two-branch selection: enabled mode
  awaits `LoadItemsAsync(preScored)` (the `IList<QfcPreScoredItem>` overload), disabled mode awaits
  `LoadItemsAsync(listEmail)` (the `IList<MailItem>` overload).

**The disabled branch still selects the `IList<MailItem>` overload.** It is the `else` arm of the
same `highConfidenceModeEnabled` test that guards the dequeue, so in disabled mode `preScored` stays
null, no dequeue call is made, and the plain overload is the only one reachable. This is a
structural property of the code, not an inference from a test.

Citation note: the plan cites `QuickFiler/Controllers/QfcHomeController.cs:307` as the unconditional
call site, and that is correct. The `issue.md` "Proposed Fix / Validation Ideas" section calls
`:310` "the sole overload-selection call site"; at the base ref `:310` is a blank line and `:307`
is the call. The plan's citation is the accurate one and was the one followed.

### AC5 — the handler is threaded to the `QfcItemController` constructor

- `QuickFiler/Controllers/QfcItemGroup.cs` carries
  `internal IFolderSearchHandler CarriedFolderHandler { get; set; }` alongside
  `PredeterminedFolder` at `:50`. (Landed in P1-T4 because the relocated
  `CarrierLoad_SetsPredeterminedFolderOnItemGroup` test would not compile without it; recorded in
  `carrier-chain.md`.)
- `EncapsulateItemGroup` gained a seventh parameter `IFolderSearchHandler carriedFolderHandler = null`,
  assigns it to the new group property in the object initialiser, and passes `grp.CarriedFolderHandler`
  as the tenth argument to the `QfcItemController` constructor.
- The `QfcPreScoredItem` overload of `LoadControlsAndHandlers_01Async` passes `scored.FolderHandler`
  as the seventh argument to `EncapsulateItemGroup`.
- `QfcItemController` stores it in `_carriedFolderHandler` (landed by P1-T2).

Both new parameters default to `null`, so the standard non-high-confidence path through
`EncapsulateItemGroup` is unchanged.

## Acceptance conditions

### 1. The analyzer build exits 0

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0, `5 Warning(s)`, `0 Error(s)`, no coded warning, `CoreCompile:` ran 57 times.
The nullable build was also run: EXIT_CODE 0, `0 Error(s)`, no `CS86` diagnostic.

### 2. The high-confidence-disabled branch still selects the `IList<MailItem>` overload

Stated and justified structurally above.

### 3. New members land in a new partial part, `partial` added at `:22`, `<Compile Include>` added,
and both relocated methods keep the base file at or below its `BASELINE_SIZE_CENSUS` value

- New part: `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs`, 156 lines.
- `partial` was added to the class declaration at `QuickFiler/Controllers/QfcCollectionController.cs:22`.
  `:21` is the `[ExcludeFromCodeCoverage]` attribute and `:22` is
  `public class QfcCollectionController : IQfcCollectionController`; the plan's `:22` citation is
  correct and was verified against numbered output before editing.
- `<Compile Include="Controllers\QfcCollectionController.CarrierLoad.cs" />` added to
  `QuickFiler/QuickFiler.csproj` immediately after the base part's entry.
- Both methods were moved **in full**, as the plan directs, because each gains a parameter on its own
  line under CSharpier and the base file is already far over the cap:
  `EncapsulateItemGroup` (was at `:646`) and the `QfcPreScoredItem` overload of
  `LoadControlsAndHandlers_01Async` (was at `:487`).

| File | Baseline (census) | After | Verdict |
|---|---:|---:|---|
| QuickFiler/Controllers/QfcCollectionController.cs | 2446 | **2336** | At or below census. The base file did **not** rise. |
| QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs | new | 156 | Under 500. |
| QuickFiler/Controllers/QfcHomeController.cs | 449 | 465 | Under 500. |
| QuickFiler/Controllers/QfcItemGroup.cs | 52 | 61 | Under 500. |
| QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs | 261 | 271 | Under 500. |

Counts are post-format; `dotnet tool run csharpier format .` was run and reports no remaining drift.

### 4. This artifact records the changed file list with per-file counts from Derivation D8

Table above. Derivation D8 is `(Get-Content -LiteralPath '<path>').Count`.

## The one-public-constructor pin is preserved

`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` asserts
`typeof(QfcCollectionController).GetConstructors()` contains exactly one entry. The new partial part
declares **no constructor of any kind**; a scan for `public QfcCollectionController(` across both
parts returns exactly one hit, at `QuickFiler/Controllers/QfcCollectionController.cs:30`. The pin
still holds.

## Which behaviour of the exempt `QfcCollectionController` is left unpinned, and why

`QfcCollectionController` carries `[ExcludeFromCodeCoverage]` at
`QuickFiler/Controllers/QfcCollectionController.cs:21`. The attribute is class-level, so it covers
the new partial part too and no attribute was added or removed by the move. Every line this task
added to that class is therefore outside the coverage denominator and **cannot be pinned by a
coverage figure**.

It is also not pinned by an existing behavioural test. `CarrierLoad_SetsPredeterminedFolderOnItemGroup`
(now at `QuickFiler.Test/Controllers/QfcCollectionControllerTests.Part2.cs`, relocated from
`QfcCollectionControllerTests.cs:302-326`) **replicates** the group-level carry rather than invoking
`EncapsulateItemGroup`, exactly as its own comment states: the real method dequeues a WinForms
`ItemViewer` and constructs a live `QfcItemController`, both of which require WinForms and Outlook
COM that the unit-test policy prohibits. That test therefore exercises no `QfcCollectionController`
member at all, before or after this change.

**The behaviour left unpinned by any test is:** that `EncapsulateItemGroup` propagates
`scored.FolderHandler` from the carrier, through `QfcItemGroup.CarriedFolderHandler`, into the tenth
constructor argument of `QfcItemController`; and that the carrier overload of
`LoadControlsAndHandlers_01Async` passes `scored.FolderHandler` into `EncapsulateItemGroup`. Both
are single argument-passing steps inside COM-bound method bodies.

**The only structural pin that survives the change** is the constructor-contract assertion in
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` at `:110`. It is structural,
using reflection over `GetConstructors()` and `ParameterInfo`, so it runs without touching WinForms
or COM, and it is what constrains this task not to add a second public constructor when introducing
the new part. It pins the constructor shape, not the argument propagation.

Citation note: the plan cites that test as `:110-131`. The `[TestMethod]` attribute is at `:109` and
the method body runs to `:150`; `:131` is the `parameters[4].ParameterType.FullName` read and the
cited span omits the two `Should().Be(...)` assertions at `:134-149`. The substantive claim the plan
makes about the test is accurate.

The compensating measures actually available are recorded here rather than left implicit: the two
ends of the propagation chain are pinned by non-exempt tests on either side of the exempt middle.
`CarrierLoad_SetsPredeterminedFolderOnItemGroup` pins that the carrier's `FolderHandler` reaches a
`QfcItemGroup`, and `PredeterminedFolderConstructor_StoresPredeterminedFolder` in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` was extended by this task to
pin that the constructor argument reaches `_carriedFolderHandler`. Neither pins the exempt method
that joins them.

## The reflection constructor pin extended rather than rewritten

`PredeterminedFolderConstructor_StoresPredeterminedFolder` gained one arranged value
(`carriedFolderHandler`), one named constructor argument, and one additional assertion that
`_carriedFolderHandler` holds that same instance. **No existing arrange step, argument or assertion
was changed or removed.** The test would have compiled and passed unmodified, because the new
constructor parameter is optional; it was extended deliberately so the new argument is covered.

Citation note: the plan locates this test at
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:91-123`. Its true location at
the base ref is `:142-175` (`[TestMethod]` at `:142`). This is a stale plan citation; the task was
executed against the true location.
