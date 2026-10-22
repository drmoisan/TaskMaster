# P1-T4 — Producer and carrier chain (AC1, AC2, AC3)

Timestamp: 2026-09-01T22-40

## What was implemented

### AC1 — the carrier

`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`. `QfcPreScoredItem` gained a third
constructor parameter `IFolderSearchHandler folderHandler = null` and a get-only property
`FolderHandler`. The two existing members are unchanged in name, type and contract:

- `MailItem MailItem { get; }` — unchanged, still assigned directly.
- `string PredeterminedFolder { get; }` — unchanged, still `predeterminedFolder ?? string.Empty`,
  so the non-null contract still holds.

The carried type is the narrow `UtilitiesCS.IFolderSearchHandler` seam, **not** the concrete
`FolderPredictor`, as AC1 requires.

The parameter is optional and the member is nullable. That is deliberate and is stated in the
member's own documentation: the carrier is constructed on paths that have no handler to publish, and
the item controller falls back to its existing behaviour when the value is null. It is not a
weakening of AC1, which requires the member to exist and be carried, not to be non-null.

### AC2 — the producer publishes rather than discards

`IFolderScoringService.ScoreAsync` now returns
`Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>`. `FolderScoringService.ScoreAsync`
returns `(score, topFolder, predictor)`, publishing the very predictor its own
`await predictor.InitAsync(helper, FolderPredictor.InitOptions.FromField)` call produced. Before this
change only the two scalars escaped and the initialised predictor fell out of scope, which is the
defect issue #678 records.

`FolderScoringService` **retains** its `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`
attribute and the full `<remarks>` justification block above it. Verified by diff: a
`git diff -- QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` filtered for lines touching
`ExcludeFromCodeCoverage` or the remark block returns **no output**, so neither the attribute nor its
justification appears as an added or removed line. The attribute now sits at `:198` rather than
`:166` purely because 32 lines were inserted above it.

### AC3 — the handler reaches the datamodel boundary

Three forwarding points, in order along the path:

1. `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` — the `_scoreLoader` field type and
   both constructor overloads widened to the three-element tuple; the deconstruction in
   `DequeueAsync` now binds `handler`; and the acceptance projection constructs
   `new QfcPreScoredItem(mailItem, topFolder, handler)`. The handler is therefore present on every
   element of `QfcGateBatch.Accepted`.
2. `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` —
   `ScoreRemainingQueueMailItemAsync` widened to the same three-element tuple and returns
   `(score.Score, score.TopFolder, score.Handler)`.
3. `DequeueWithHighConfidenceGateAsync` (same file) already passes `batch.Accepted` straight into
   `new QfcDequeueBatch(UnhookDequeuedNodes(nodes), accepted, batch.Stop)`, so no edit was needed
   there: the widened carriers flow through unchanged and are present on
   `QfcDequeueBatch.PreScored`.

## Post-change construction-site inventory

Re-derived by the same ordinal scan P0-T13 used, over every `.cs` file under `QuickFiler/` and
`QuickFiler.Test/` excluding `bin/` and `obj/`:

| # | File | Line | Populates the new member? |
|---:|---|---:|---|
| 1 | QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 90 | **Yes** — `new QfcPreScoredItem(result.item, result.topFolder, result.handler)`, where `result.handler` is the third element the widened `service.ScoreAsync` now returns. |
| 2 | QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 212 | **Yes** — `new QfcPreScoredItem(mailItem, topFolder, handler)`. |
| 3 | QuickFiler.Test/Controllers/QfcCollectionControllerTests.Part2.cs | 39 | **Yes** — `new QfcPreScoredItem(mail, @"\\Archive\Projects\Active", handler)` with a `Mock<IFolderSearchHandler>` object. |
| 4 | QuickFiler.Test/Controllers/QfcFormControllerTests.Part2.cs | 53 | **Yes** — three-argument form with a `Mock<IFolderSearchHandler>` object. |

**COUNT: 4.**

**The post-change member set equals the P0-T13 list.** The same four construction sites exist, in
the same four logical locations; two of them moved file because the enclosing test method was
relocated into a new partial part for the file-size reasons recorded below. Concretely:
`QfcCollectionControllerTests.cs:307` became `QfcCollectionControllerTests.Part2.cs:39`, and
`QfcFormControllerTests.cs:814` became `QfcFormControllerTests.Part2.cs:53`. No construction site was
added and none was removed.

All four populate the new member. The two production sites, which are the ones AC3 constrains, both
populate it with a real handler rather than a null placeholder.

The plan's P1-T4 prose names `:98-122`, `:143-147`, `:170-189` and `:184` in
`QfcHighConfidencePreFilter.cs` but does **not** name the construction site at `:86` in
`FilterAsync`. P0-T13 recorded that omission, and this task covered the site: it is production code,
its constructor signature widened, and AC3 requires every production construction site to populate
the new member. `QfcHighConfidencePreFilter.FilterAsync` remains dormant (AC13); dormancy does not
exempt it from compiling correctly or from populating the member.

## Collateral test edits this task owns

Per the P1-T10 assignment clause, the following belong to P1-T4 and are recorded here:

| File | Site | Reason |
|---|---|---|
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | `MockBehavior.Strict` `IFolderScoringService` setup (was `:337`), result shape (was `:352`), reflection invoker return type (was `:370`, `:385`) | The widened seam changed the `ReturnsAsync` tuple arity and the reflected return type. The test was **extended**, not weakened: it now additionally asserts `result.Handler` is the same instance the mock published, which is the same discard defect one element to the right. |
| QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | `BuildScoringMock` lambda (was `:86`, `:88`) | Scripted double now returns the three-element tuple with a null handler. This double exercises cutoff and ordering behaviour and publishes no handler; the null is tolerated by the carrier. |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | `ReturnsAsync` (was `:161`) and the `FakeTimeProvider` lambda (was `:233`) | Same arity change. Assertions unchanged. |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | `CreateGate` `scoreLoader` parameter type (was `:28`) and the exact-type constructor lookup entry (was `:54`) | The reflection lookup names the delegate type by exact `typeof`, so it must be widened with the production signature. It fails **closed** by design, as its own comment records, so leaving it unwidened would have failed every test in the partial class rather than degrading quietly. |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs, `.Part2.cs`, `.Part3.cs` | every inline two-value `scoreLoader` lambda, and the `TaskCompletionSource<(long, string)>` in `.Part2.cs` | Arity change. A single `Scored(long score, string topFolder = "", IFolderSearchHandler handler = null)` helper was added to `.Part3.cs` and the inline lambdas now call it, so the widening is spelled out once instead of at roughly twenty call sites. This **shrinks** the affected lines rather than growing them, which matters because the base part had 32 lines of headroom. No assertion changed. |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | `CarrierLoad_SetsPredeterminedFolderOnItemGroup` relocated to `.Part2.cs`, class marked `partial` at `:24` | The file stood at 499 lines with one line of headroom; the widened construction reflows across several lines under CSharpier. The test was moved verbatim and then **extended** with the handler-carry assertion. |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` relocated to `.Part2.cs`, class marked `partial` at `:20` | The file stood at 827 lines, already over the cap, so it must not grow at all. The test was moved verbatim, with only the construction site widened. |

No `[TestMethod]` was deleted, and no assertion was removed or weakened by any of the above.

## Acceptance conditions

1. **Analyzer build exits 0.** Command:
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   EXIT_CODE: 0, `5 Warning(s)`, `0 Error(s)`, no coded warning, `CoreCompile:` ran 57 times.
   (The nullable build was also run and exited 0 with `0 Error(s)` and no `CS86` diagnostic.)
2. **The `[ExcludeFromCodeCoverage]` attribute and its justification remark block are unchanged.**
   Confirmed by an empty filtered diff, as described under AC2 above.
3. **Every construction site enumerated in P0-T13 populates the new member.** Table above; 4 of 4.
4. **This artifact records the post-change construction-site list and states that its member set
   equals the P0-T13 list.** Stated above.

## File sizes after this task, post-format

| Path | Baseline | After | Budget |
|---|---:|---:|---|
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 191 | 228 | 500 |
| QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 245 | 262 | 500 |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 288 | 292 | 500 |
| QuickFiler/Controllers/QfcItemGroup.cs | 52 | 61 | 500 |
| QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 359 | 363 | 500 |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 391 | 401 | 500 |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 262 | 262 | 500 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | 468 | 477 | 500 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs | 460 | 465 | 500 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs | 270 | 280 | 500 |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 827 | 792 | 827 (census) |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 499 | 464 | 500 |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.Part2.cs | new | 73 | 500 |
| QuickFiler.Test/Controllers/QfcFormControllerTests.Part2.cs | new | 68 | 500 |

Counts are post-format: `dotnet tool run csharpier check .` reports `Checked 1570 files` with no
file listed as needing formatting.

`QfcItemGroup` also gained `internal IFolderSearchHandler CarriedFolderHandler { get; set; }` in this
task rather than in P1-T5, because the relocated `CarrierLoad_SetsPredeterminedFolderOnItemGroup`
test asserts the group-level carry and would not compile without it. AC5's remaining obligations,
threading it through `EncapsulateItemGroup` and `LoadControlsAndHandlers_01Async`, are P1-T5's.

## New `<Compile Include>` entries added to `QuickFiler.Test/QuickFiler.Test.csproj`

- `Controllers\QfcItemController.FolderHandlingTests.Part2.cs` (added by P1-T3)
- `Controllers\QfcCollectionControllerTests.Part2.cs`
- `Controllers\QfcFormControllerTests.Part2.cs`

---

## Appendix: end of the carrier chain (AC10 and AC14)

Appended 2026-09-01T23-05, after P1-T7 landed the adoption and release. The acceptance-criterion
index names this artifact as the primary evidence for AC10 and AC14, so the chain's two terminal
properties are recorded here alongside the chain itself.

### AC10 — the carried handler is released in cleanup

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, inside `Cleanup`:
`_carriedFolderHandler = null;` sits immediately after the **first** of the two pre-existing
`_folderHandler = null;` statements, which was at `:465` at the base ref. The carried reference is
therefore released on the same pass and at the same point as the handler it feeds, and cannot
outlive the row.

The duplicate `_folderHandler = null;` two lines further down is pre-existing. It was left in place
deliberately: removing it is not required by any acceptance criterion and would be an opportunistic
edit outside the change's scope.

The file is now exactly **500** lines, at the cap and not over it, which is the outcome the plan's
file-size section predicted for a single statement added inside an existing method body that cannot
be relocated to another part.

### AC14 — `QfcDequeueStop` handling and the early-return condition are unchanged

- **`QfcDequeueStop` handling in `IterateQueueAsync`** is unchanged. The
  `else if (batch.Stop == QfcDequeueStop.SourceExhausted)` arm and its
  `await QfcQueue.CompleteAddingAsync(Token, 10000);` call are untouched, as is the comment
  recording why only genuine source exhaustion may close the queue. The **only** edit inside that
  method is the third argument added to the `EnqueueAsync` call, which sits inside the existing
  `if (listObjects.Count > 0)` guard and therefore cannot change which arm is taken.
- **The empty-batch early-return behaviour** is unchanged for the same reason: the guard condition
  `listObjects.Count > 0` is byte-identical.
- **The carrier overload of `LoadItemsAsync` returns early on the same condition as the
  `IList<MailItem>` overload (null, not empty).**
  `QuickFiler/Controllers/QfcFormController.Actions.cs` was **not edited by this change at all**.
  Its guard at `:125-135` still reads `if (preScored is null || _globals is null || _formViewer is
  null || _parent is null || _tokenSource is null || _states is null) { return; }`. The condition is
  `is null`, not a count test, so an empty carrier list proceeds rather than returning early.

  This is load-bearing for leg A after P1-T5's overload switch: in high-confidence-enabled mode
  `RunAsync` now always calls the carrier overload, including when the gate returns an empty batch,
  and the empty case must still construct the collection controller rather than return. The
  behaviour is pinned by the pre-existing test
  `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration` in
  `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`, rewritten onto the
  carrier overload by P1-T10 and recorded in `test-reconciliation.md`.
