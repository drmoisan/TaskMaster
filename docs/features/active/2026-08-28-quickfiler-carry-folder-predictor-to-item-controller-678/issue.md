# quickfiler-carry-folder-predictor-to-item-controller (Issue #678)

- Date captured: 2026-08-28 (originally identified 2026-08-24; promoted from a stranded worktree during cleanup)
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-carry-folder-predictor-to-item-controller/ (Issue #678)
- Found during: preparation of epic child `quickfiler-queue-datamodel-defects` (primary issue #446)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #678
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/678
- Last Updated: 2026-08-28
- Work Mode: minor-audit

## Summary

Issue #427 reports that every accepted QuickFiler mail item is scored twice in high-confidence mode.
Preparation research for the `quickfiler-queue-datamodel-defects` feature established that the fix
proposed in the original #427 potential document does not actually remove the second scoring pass,
so #427 cannot be fully resolved by carrying the top-folder string alone. This entry records the
remaining consumer-side work.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework 4.8.1 VSTO add-in
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox

## Steps to Reproduce

1. Enable High Confidence mode and launch QuickFiler.
2. Enable debug logging and inspect the `Probability debug` entries for a single accepted item.
3. Observe one entry from the pre-UI scan and a second, independent classification after the form is shown.

## Expected Behavior

An item accepted by the confidence gate carries its already-initialised folder predictor forward, so
the item controller populates the folder combo, the suggestion list and the folder array from that
result instead of recomputing them.

## Actual Behavior

The initialised predictor is discarded and the full `FolderPredictor.InitAsync(InitOptions.FromField)`
sequence runs a second time per accepted item after `Show()`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: two `Probability debug` lines per accepted item, as recorded in the original #427 potential document.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: wasted work, not incorrect behavior. It occurs after `Show()`. The user-visible effect is slower
folder-combo population and redundant Outlook COM traffic proportional to the number of items on screen.

## Acceptance Criteria

Derived from the Expected Behavior and Actual Behavior sections above, from the two guard sites named
in Suspected Cause / Notes, and from the preparation research at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/research/2026-08-31T21-15-quickfiler-carry-folder-predictor-research.md`. Scope is limited to
carrying the already-initialised folder predictor forward from the confidence gate to the item
controller and removing the resulting redundant second initialisation, on both reachable display
paths. Nothing else in QuickFiler is in scope.

Three premises in the sections above were corrected by that research and the criteria below follow the
corrected reading: the live producer is the dequeue gate rather than the dormant
`QfcHighConfidencePreFilter.FilterAsync`; there are two re-scoring legs rather than one; and the
`Times.Never` assertions at `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246` and `:277` are
disabled-mode assertions that must be preserved rather than rewritten.

### Carrier and producer

- [ ] AC1. `QfcPreScoredItem` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`) carries the already-initialised `IFolderSearchHandler` in addition to its existing `MailItem` and `PredeterminedFolder` members. The two existing members keep their current names, types and non-null contracts. The carried type is `IFolderSearchHandler`, not the concrete `FolderPredictor`.
- [ ] AC2. `IFolderScoringService.ScoreAsync` and its `FolderScoringService` implementation publish the handler they initialise instead of discarding it. `FolderScoringService` retains its `[ExcludeFromCodeCoverage]` attribute and its justification comment.
- [ ] AC3. The handler reaches the datamodel boundary. The `scoreLoader` delegate of `QfcStreamingDequeueConfidenceGate`, its acceptance projection, and `QfcDatamodel.QueueProcessing.ScoreRemainingQueueMailItemAsync` all forward the handler so it is present on `QfcGateBatch.Accepted` and on `QfcDequeueBatch.PreScored`. Every production construction site of `QfcPreScoredItem` populates the new member; the executor re-derives the complete set of those sites against the branch base and records it.

### Consumer, leg A (first page)

- [ ] AC4. `QfcHomeController.RunAsync` in high-confidence-enabled mode obtains the carriers from the outcome-returning dequeue and selects the `IList<QfcPreScoredItem>` overload of `IQfcFormController.LoadItemsAsync`, so the carried handler reaches `QfcCollectionController`, `QfcItemGroup` and `QfcItemController`. In high-confidence-disabled mode `RunAsync` continues to select the `IList<MailItem>` overload.
- [ ] AC5. `QfcItemGroup` carries the handler alongside `PredeterminedFolder`, and `QfcCollectionController.EncapsulateItemGroup` and the `QfcPreScoredItem` overload of `LoadControlsAndHandlers_01Async` thread it through to the `QfcItemController` constructor, which stores it.

### Consumer, leg B (every subsequent page)

- [ ] AC6. `QfcHomeController.IterateQueueAsync` forwards `batch.PreScored` into `QfcQueue`, and `QfcQueue` carries the handler through `EnqueueAsync` to the `QfcItemController` instances it constructs, so items displayed after the first page also arrive with a carried handler. If a seam is required to make this assertable, it is the injectable-delegate seam (form 2 of `.claude/rules/csharp.md`), mirroring the existing `_folderPredictorFactory` and `ScoringServiceFactory` patterns in the same assembly; no new interface is introduced.

### Adoption and the single-initialisation invariant

- [ ] AC7. `QfcItemController.LoadFolderHandlerAsync` adopts a carried handler inside its `varList is null` branch only. For an item that arrives with a carried handler, neither `_folderPredictorFactory` nor `FolderPredictor.InitAsync` is invoked by that method.
- [ ] AC8. When no carried handler is present, `QfcItemController.LoadFolderHandlerAsync` behaves exactly as it does today: it builds a predictor through `_folderPredictorFactory` and initialises it with `FolderPredictor.InitOptions.FromField`. The existing test that pins the un-carried path passes unmodified.
- [ ] AC9. The `FolderPredictor.InitOptions.FromArrayOrString` branches of both `LoadFolderHandler` and `LoadFolderHandlerAsync` are unchanged, and a carried handler is never adopted on a `FromArrayOrString` call. A negative test proves the carried handler is ignored when `varList` is non-null.
- [ ] AC10. The carried handler is released in `QfcItemController` cleanup alongside `_folderHandler`, so it does not outlive the row.

### Preserved behaviour

- [ ] AC11. The folder entry preselected by `AssignFolderComboBox` is identical to the entry the pre-change code preselects, for both the predetermined-folder case and the index fallback cases. `FolderArray`, `Suggestions` and `FolderRowArray` are populated from the carried result with the same values the recomputed result produced.
- [ ] AC12. The raw-versus-projected path mismatch identified in the research is resolved deliberately and the resolution is stated in the change description: the carried `PredeterminedFolder` and the `FolderArray` entries use the same normalisation so `_itemViewer.FolderContains` matches for archive-rooted suggestions. A test covers an archive-rooted suggestion and fails against the unnormalised form.
- [ ] AC13. `QfcHighConfidencePreFilter.FilterAsync` remains dormant and `HighConfidencePreFilterLoader` remains uninvoked. The `Times.Never` assertions at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:246` and `:277` and the `preFilterInvoked` assertions in the same file and in `QfcHomeControllerIssue218Tests.cs` are preserved verbatim.
- [ ] AC14. The `QfcDequeueStop` handling in `IterateQueueAsync` and the empty-batch early-return behaviour are unchanged. The carrier overload of `LoadItemsAsync` returns early on the same condition as the `IList<MailItem>` overload (null, not empty).
- [ ] AC15. The accepted behavioural delta is stated in the change description: reusing the scan-time suggestion set freezes conversation-derived (`CtfMap`) suggestions at scan time rather than re-deriving them at display time, for both legs.

### Tests

- [ ] AC16. A new MSTest test asserts the single-initialisation invariant directly: for an item carrying an initialised handler, `LoadFolderHandlerAsync` invokes the predictor-construction seam exactly zero times, verified with a Moq `Times` assertion. The test fails against the pre-change code and passes after the change.
- [ ] AC17. The two verifications that constrain the `IList<QfcPreScoredItem>` overload in high-confidence-enabled tests (`QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs:178` and `:256`) are rewritten rather than deleted, so they assert the carrier overload is now selected. No test is weakened or removed to accommodate the change; every changed test carries a recorded reason.
- [ ] AC18. All new and modified tests use MSTest, Moq and FluentAssertions, create no temporary files, and require no live Outlook COM, per the repository unit-test policy.

### Gates and footprint

- [ ] AC19. The full C# toolchain passes in order on the final pass: `dotnet tool run csharpier check .`, the analyzer build, the nullable build, and the MSTest run, each with its own evidence artifact under the feature folder recording `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`.
- [ ] AC20. Coverage does not regress on the changed lines and every new or modified member reaches at least 90% line coverage. Baseline and post-change coverage figures are recorded numerically. No `[ExcludeFromCodeCoverage]` attribute is added or removed anywhere in the change.
- [ ] AC21. No source file exceeds the 500-line limit as a result of the change. Additions to files already at or over the limit go into new partial parts rather than extending the existing file.
- [ ] AC22. The items the research places out of scope are not changed: the synchronous `LoadFolderHandler` predictor-initialisation defect, de-exempting any coverage-exempt class, splitting oversized files, adding `InitAsync` to `IFolderSearchHandler`, deleting the dormant post-display filter, and consolidating the duplicated `MailItemHelper.FromMailItemAsync` calls. Any of these that the executor confirms is a real defect is reported for separate promotion rather than fixed here.
- [ ] AC23. The change is confined to the `QuickFiler` and `QuickFiler.Test` projects plus this feature folder. No change to `.claude/rules/`, `CLAUDE.md`, any policy document, or any file under `UtilitiesCS`.

## Suspected Cause / Notes

Verified at `988e819b` during preparation research for issue #446. Full analysis was recorded at
`docs/features/active/quickfiler-queue-datamodel-defects-446/research/2026-08-24T09-50-quickfiler-queue-datamodel-defects-research.md`
§ 4.5 in the worktree that captured it; that worktree's copy of the feature folder is a superseded
pre-execution draft and was not carried into the merged feature (the merged version does not include
this consumer-side follow-up).

The original #427 potential document proposed activating the dormant
`QfcFormController.LoadItemsAsync(IList<QfcPreScoredItem>)` overload so the predetermined folder is
carried forward. That premise is incorrect:

- `_predeterminedFolder` is consumed only for combo-box *selection* inside `AssignFolderComboBox`
  (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:193-199`).
- The surrounding code still requires a fully-initialised predictor: `FolderArray`, `Suggestions`
  and `FolderRowArray` all come from `_folderHandler` (`IFolderSearchHandler`, declared
  `QuickFiler/Controllers/QfcItemController.cs:41`), which is produced only by
  `LoadFolderHandler`/`LoadFolderHandlerAsync`.
- So even on the carrier path the item controller must still run
  `FolderPredictor.InitAsync(FromField)`. Carrying only the top-folder string changes which entry is
  preselected, a behavior the code already implements, and saves no scoring work.

Removing the second scoring pass requires carrying the initialised `FolderPredictor` /
`IFolderSearchHandler` from `FolderScoringService.ScoreAsync`
(`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:184`, where it is discarded) through to
`_folderHandler`.

Line numbers above were verified against commit `988e819b` (2026-08-24) and should be re-checked
against current `main` before planning, since the referenced files may have moved since.

## Proposed Fix / Validation Ideas

Files that must change, none of which were owned by the `quickfiler-queue-datamodel-defects` feature:

- `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` — widen `IFolderScoringService.ScoreAsync` to surface the predictor
- `QuickFiler/Controllers/QfcItemGroup.cs:50` — new carried member
- `QuickFiler/Controllers/QfcCollectionController.cs:428-471`, `:616`
- `QuickFiler/Controllers/QfcItemController.cs:41`, `:83-89`
- `QuickFiler/Controllers/QfcItemController.Initialization.cs:63-64`, `:108`, `:398-400`
- `QuickFiler/Controllers/QfcHomeController.cs:310` — the sole overload-selection call site

Prerequisite already landed by the `quickfiler-queue-datamodel-defects` feature (Scope 427-A): the
producer side no longer discards the scoring result, and the datamodel boundary exposes
`QfcPreScoredItem` carriers on its dequeue batch. Nothing consumes them yet; this entry is that
consumer work.

Pinned tests that must be deliberately rewritten, not deleted, because they encode the landed
decision of issue #233 that high-confidence enforcement moved from post-display filtering to
dequeue-time gating:

- `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs:137-259`
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`, `:277`

The `Times.Never` assertion on `HighConfidencePreFilterLoader` should stay: the pre-filter class
remains dormant, and only the carrier overload would become live.

- [ ] Unit coverage areas: predictor carry-through, `QfcItemController` folder-handler population, overload selection
- [ ] Integration scenario to retest: high-confidence launch, confirming one scoring pass per accepted item and an unchanged folder-combo selection
- [ ] Manual verification notes: compare `Probability debug` log output before and after; confirm the preselected folder matches the previous behavior

Tests must use MSTest with Moq and FluentAssertions, no live Outlook COM and no temporary files, per
repository unit-test policy.

## Next Step

- [ ] Promote to GitHub issue (bug-report template), or attach as a scoped follow-up to issue #427
- [ ] Coordinate with the epic children that own the six files listed above
