# P1-T1 — Implementation handoff packet

Timestamp: 2026-09-01T22-15

## Delegation status — DELEGATION UNAVAILABLE

P1-T1 directs the executor to delegate implementation to the C# implementation engineer. **No
Agent or delegation tool exists in this session**, so no subagent could be spawned and the delegation
could not be performed. Per the delegating orchestrator's explicit instruction, the packet is written
in full exactly as specified and **the executor performed the implementation directly**. This
deviation from the plan's delegated-block model is recorded here and is reported in the executor's
completion report. No plan task was skipped and no acceptance condition was relaxed as a result: the
acceptance conditions of P1-T2 through P1-T13 are unchanged and are evaluated identically whoever
performed the edit.

## Completion criteria

The implementation is complete when acceptance criteria **AC1 through AC18** and **AC21 through
AC23** of
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md`
are satisfied. Named in full:

- AC1 — `QfcPreScoredItem` carries the already-initialised `IFolderSearchHandler` in addition to its
  existing `MailItem` and `PredeterminedFolder` members; the two existing members keep their current
  names, types and non-null contracts; the carried type is `IFolderSearchHandler`, not the concrete
  `FolderPredictor`.
- AC2 — `IFolderScoringService.ScoreAsync` and `FolderScoringService` publish the handler they
  initialise instead of discarding it; `FolderScoringService` retains its `[ExcludeFromCodeCoverage]`
  attribute and its justification comment.
- AC3 — the handler reaches the datamodel boundary through the `scoreLoader` delegate of
  `QfcStreamingDequeueConfidenceGate`, its acceptance projection, and
  `QfcDatamodel.QueueProcessing.ScoreRemainingQueueMailItemAsync`, so it is present on
  `QfcGateBatch.Accepted` and on `QfcDequeueBatch.PreScored`; every production construction site of
  `QfcPreScoredItem` populates the new member.
- AC4 — `QfcHomeController.RunAsync` in high-confidence-enabled mode obtains the carriers from the
  outcome-returning dequeue and selects the `IList<QfcPreScoredItem>` overload of
  `IQfcFormController.LoadItemsAsync`; disabled mode continues to select the `IList<MailItem>`
  overload.
- AC5 — `QfcItemGroup` carries the handler alongside `PredeterminedFolder`, and
  `QfcCollectionController.EncapsulateItemGroup` and the `QfcPreScoredItem` overload of
  `LoadControlsAndHandlers_01Async` thread it through to the `QfcItemController` constructor, which
  stores it.
- AC6 — `QfcHomeController.IterateQueueAsync` forwards `batch.PreScored` into `QfcQueue`, and
  `QfcQueue` carries the handler through `EnqueueAsync` to the `QfcItemController` instances it
  constructs. Any seam required to make this assertable is the injectable-delegate seam, form 2 of
  `.claude/rules/csharp.md` (line 52), mirroring the existing `_folderPredictorFactory` and
  `ScoringServiceFactory` patterns; **no new interface is introduced**.
- AC7 — `QfcItemController.LoadFolderHandlerAsync` adopts a carried handler inside its
  `varList is null` branch only; for an item arriving with a carried handler, neither
  `_folderPredictorFactory` nor `FolderPredictor.InitAsync` is invoked by that method.
- AC8 — with no carried handler, `LoadFolderHandlerAsync` behaves exactly as today; the existing
  test that pins the un-carried path passes unmodified.
- AC9 — the `FromArrayOrString` branches of both `LoadFolderHandler` and `LoadFolderHandlerAsync` are
  unchanged and a carried handler is never adopted on a `FromArrayOrString` call; a negative test
  proves it.
- AC10 — the carried handler is released in `QfcItemController` cleanup alongside `_folderHandler`.
- AC11 — the folder entry preselected by `AssignFolderComboBox` is identical to the pre-change
  entry, for the predetermined-folder case and the index fallback cases; `FolderArray`,
  `Suggestions` and `FolderRowArray` are populated from the carried result with the same values.
- AC12 — the raw-versus-projected path mismatch is resolved deliberately and stated in the change
  description; a test covers an archive-rooted suggestion and fails against the unnormalised form.
- AC13 — `QfcHighConfidencePreFilter.FilterAsync` remains dormant and `HighConfidencePreFilterLoader`
  remains uninvoked; the `Times.Never` assertions at
  `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:246` and `:277` and
  the `preFilterInvoked` assertions in that file and in `QfcHomeControllerIssue218Tests.cs` are
  preserved verbatim.
- AC14 — `QfcDequeueStop` handling in `IterateQueueAsync` and the empty-batch early return are
  unchanged; the carrier overload of `LoadItemsAsync` returns early on the same condition as the
  `IList<MailItem>` overload (null, not empty).
- AC15 — the accepted behavioural delta is stated in the change description.
- AC16 — a new MSTest test asserts the single-initialisation invariant with a Moq `Times` assertion;
  it fails against the pre-change code and passes after.
- AC17 — the two verifications constraining the `IList<QfcPreScoredItem>` overload in
  high-confidence-enabled tests are rewritten rather than deleted; no test is weakened or removed,
  and every changed test carries a recorded reason.
- AC18 — all new and modified tests use MSTest, Moq and FluentAssertions, create no temporary files
  and require no live Outlook COM.
- AC21 — no source file exceeds the 500-line limit as a result of the change; additions to files
  already at or over the limit go into new partial parts.
- AC22 — the out-of-scope items are not changed; any confirmed defect is reported for separate
  promotion.
- AC23 — the change is confined to `QuickFiler`, `QuickFiler.Test` and this feature folder.

AC19 and AC20 are **not** implementation-engineer criteria: they are the Phase 2 gate and coverage
criteria and are owned by P2-T1 through P2-T9.

## Out-of-scope list (AC22), reproduced item by item

1. The synchronous `QfcItemController.LoadFolderHandler` predictor-initialisation defect
   (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:27-55`).
2. De-exempting any `[ExcludeFromCodeCoverage]` class.
3. Splitting oversized files.
4. Adding `InitAsync` to `IFolderSearchHandler`.
5. Deleting the dormant post-display filter.
6. Consolidating the duplicated `MailItemHelper.FromMailItemAsync` calls.

Each is confirmed or not confirmed by the executor and, when confirmed a real defect, is REPORTED
for separate promotion and left unchanged in this branch. No change to any file under `UtilitiesCS`,
to `.claude/rules/`, to `CLAUDE.md`, or to any policy document.

## The three corrected premises

The issue body's Suspected Cause / Notes section was corrected by the preparation research; the
acceptance criteria are written against the corrected reading, and where the two disagree the
research governs:

1. **The live producer is the dequeue-time confidence gate.** `QfcHighConfidencePreFilter.FilterAsync`
   is dormant and must remain dormant (AC13).
2. **There are two re-scoring legs, not one.** Leg A is the first page, through `RunAsync`; leg B is
   every subsequent page, through `IterateQueueAsync` into `QfcQueue`. Both are in scope (AC4, AC5,
   AC6).
3. **`QfcHomeControllerRunAsyncHighConfidenceTests.cs:246` and `:277` are inside
   high-confidence-DISABLED tests** and are preserved verbatim. The enabled-mode sites requiring
   rewrite are enumerated in full by P1-T10, and that enumeration is the authoritative list; it is
   wider than the three sites the research named, because the P1-T5 overload switch also invalidates
   shared arrange steps that no verification line cites.

## File-size budget — BASELINE_SIZE_CENSUS (P0-T12)

Production paths, lines and headroom to 500:

| Path | Lines | Headroom |
|---|---:|---:|
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 191 | 309 |
| QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 245 | 255 |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 288 | 212 |
| QuickFiler/Controllers/QfcHomeController.cs | 449 | 51 |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 95 | 405 |
| QuickFiler/Controllers/QfcItemGroup.cs | 52 | 448 |
| QuickFiler/Controllers/QfcCollectionController.cs | 2446 | -1946 |
| QuickFiler/Controllers/QfcQueue.cs | 610 | -110 |
| QuickFiler/Controllers/QfcItemController.cs | 323 | 177 |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | 489 | 11 |
| QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 239 | 261 |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 499 | 1 |

Test paths, lines and headroom to 500:

| Path | Lines | Headroom |
|---|---:|---:|
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | 498 | 2 |
| QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs | 261 | 239 |
| QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | 261 | 239 |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs | 473 | 27 |
| QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 359 | 141 |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 391 | 109 |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 262 | 238 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | 468 | 32 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs | 460 | 40 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs | 270 | 230 |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 827 | -327 |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 499 | 1 |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 497 | 3 |

`QfcFormControllerTests.cs` is already over the cap at 827 and must not grow at all: its post-change
count is measured against 827, not 500. `QfcCollectionController.cs` (2446) and `QfcQueue.cs` (610)
are likewise measured against their census values.

## `QfcItemController.FolderHandlingTests.cs` has insufficient headroom for new tests

`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` is at 498 lines with 2 lines
of headroom to the 500-line cap. It **cannot** hold the three new tests this change adds. Every new
test goes into a new partial part,
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`, with:

- `partial` added to the class declaration at `QfcItemController.FolderHandlingTests.cs:19`,
- **no second `[TestClass]` attribute** on the new part, mirroring
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:30`,
- a matching `<Compile Include>` entry in `QuickFiler.Test/QuickFiler.Test.csproj`, because that
  project uses an explicit compile item list.

## Acceptance-criterion editing and check-off

The implementation engineer **edits no acceptance criterion text in `issue.md` and performs no
check-off**. Check-off is performed by the executor, per the `acceptance-criteria-tracking` skill,
one criterion at a time, only after that criterion's supporting evidence artifact exists and
verifies. The only permitted edit to the `## Acceptance Criteria` section is the checkbox transition
`- [ ]` to `- [x]`; no criterion text is reworded, added or removed.
