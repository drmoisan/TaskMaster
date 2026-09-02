# Feature Audit — issue #678, carry the folder predictor to the item controller

- Timestamp: 2026-09-01T23-35
- Head: `d1f51e3a99cc5a98f622663df27abac7c8043f11`
- Base: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Work mode: `minor-audit`
- AC source: `issue.md`, section `## Acceptance Criteria` only (AC1 through AC23)

## AC source resolution

The work-mode marker at `issue.md:13` reads `- Work Mode: minor-audit`. Under that mode the sole
authoritative acceptance-criteria source is the explicit `## Acceptance Criteria` section of
`issue.md`, which is present at `issue.md:62` and contains exactly 23 checkbox items numbered AC1
through AC23. No criterion was inferred from any other section of `issue.md`, from the plan, or from
the research document.

`spec.md` and `user-story.md` are absent. That is correct for `minor-audit` and is not a finding;
their presence would have been one. Confirmed by directory listing: the feature folder contains
`issue.md`, `plan.2026-08-31T21-12.md`, `research/` and `evidence/` and no other requirement
document.

The other checkbox items in `issue.md` — under `## Logs / Screenshots` (`:49`),
`## Impact / Severity` (`:54-57`), `## Proposed Fix / Validation Ideas` (`:176-178`) and
`## Next Step` (`:185-186`) — are deliberately excluded from this evaluation and were not altered.

## Per-criterion evaluation

| AC | Verdict | Basis, and the evidence artifact it rests on |
|---|---|---|
| AC1 | **PASS** | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:139-147` declares `public IFolderSearchHandler FolderHandler { get; }`. The type is the narrow seam, not the concrete `FolderPredictor`. `MailItem` and `PredeterminedFolder` keep their names, types and non-null coercion (`:153` retains `predeterminedFolder ?? string.Empty`). Reviewer read the diff directly; corroborated by `evidence/other/carrier-chain.md`. |
| AC2 | **PASS** | `IFolderScoringService.ScoreAsync` widened to `Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>` at `QfcHighConfidencePreFilter.cs:196-200`; `FolderScoringService.ScoreAsync` returns `(score, topFolder, predictor)` at `:219`. The `[ExcludeFromCodeCoverage]` attribute and its justification remain at `:198`. Reviewer confirmed a zero net change on that attribute token across the whole three-dot diff. Evidence: `evidence/other/carrier-chain.md`, `evidence/qa-gates/exclude-attribute-invariant.md`. |
| AC3 | **PASS** | `QfcStreamingDequeueConfidenceGate` widened its `scoreLoader` on both constructors and its acceptance projection now builds `new QfcPreScoredItem(mailItem, topFolder, handler)` at `:212`. `QfcDatamodel.QueueProcessing.ScoreRemainingQueueMailItemAsync` returns the third element at `:278`. Reviewer re-derived the complete production construction-site set independently: `grep -rn "new QfcPreScoredItem(" QuickFiler/` returns exactly two sites, `QfcHighConfidencePreFilter.cs:90` and `QfcStreamingDequeueConfidenceGate.cs:212`, and both populate the member. Evidence: `evidence/baseline/carrier-construction-sites.md`. |
| AC4 | **PASS** | `QfcHomeController.cs:299-306` calls `DequeueNextItemGroupWithOutcomeAsync` in enabled mode and assigns `preScored = batch.PreScored`; `:313-323` selects `LoadItemsAsync(preScored)` when enabled and `LoadItemsAsync(listEmail)` when disabled. Pinned by the rewritten verifications in `QfcHomeControllerIssue218Tests.cs:179-190` and `:283-287`. Evidence: `evidence/other/leg-a.md`. See NB-1 in the code review for a divergence risk this criterion's wording does not address. |
| AC5 | **PASS** | `QfcItemGroup.cs:52-60` adds `internal IFolderSearchHandler CarriedFolderHandler { get; set; }`. `QfcCollectionController.CarrierLoad.cs:126-138` passes `scored.FolderHandler` into `EncapsulateItemGroup`, which sets it on the group at `:191` and forwards `grp.CarriedFolderHandler` into the `QfcItemController` constructor at `:206`. The constructor stores it at `QfcItemController.Initialization.cs:116`. Evidence: `evidence/other/leg-a.md`. |
| AC6 | **PASS** | `QfcHomeController.Iteration.cs:35` forwards `batch.PreScored` into `EnqueueAsync`. `QfcQueue.Enqueue.cs` carries it to `LoadControllersViewersAsync`, which resolves it per row via `ResolveCarriedHandler` and passes it into the `ItemControllerFactory` seam. The seam is the injectable-delegate form and introduces no new interface, as the criterion permits. Pinned by `QfcHomeControllerIterationTests.Part2.cs:60-97` (forwarding), `QfcQueuePurePathsTests.cs:280-350` (resolution and factory default). Evidence: `evidence/other/leg-b.md`. |
| AC7 | **PASS** | `QfcItemController.FolderHandling.cs:68-77` places the adoption inside the `if (varList is null)` block and returns immediately, so neither `_folderPredictorFactory` nor `FolderPredictor.InitAsync` is reached. Reviewer read the whole method (`:57-148`) and confirmed the early return skips only logging, since the method body ends at the branch. Pinned by `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` with `Times.Never()`. Evidence: `evidence/regression-testing/ac16-green.md`. |
| AC8 | **PASS** | The un-carried route at `:79-122` is byte-identical to the base ref text; the only change inside the branch is the inserted adoption block above it. `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` has exactly one changed line in the whole diff — `class` to `partial class` at `:19` — so every existing test in it, including the un-carried pin, passes unmodified. Evidence: `evidence/regression-testing/ac16-green.md`, corroborated by reviewer diff inspection. |
| AC9 | **PASS** | The `else` branch at `:124-147` is unchanged. `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory` (`QfcItemController.FolderHandlingTests.Part2.cs:116-145`) supplies both a carried handler and a non-null `varList` and asserts the sentinel-throwing factory is invoked `Times.Once()`. The synchronous `LoadFolderHandler` is untouched. Evidence: `evidence/regression-testing/ac9-negative-guard.md`. |
| AC10 | **PASS** | `QfcItemController.ViewerSetup.cs:466` adds `_carriedFolderHandler = null;` directly beside the existing `_folderHandler = null;` in cleanup. Line measured at 1/1 covered. Evidence: `evidence/other/carrier-chain.md`. |
| AC11 | **PASS**, scoped by AC12 | `FolderArray`, `Suggestions` and `FolderRowArray` are read from the carried handler, which is the same object the scorer initialised with the same `FromField` sequence, so those three produce the same values. For preselection, the projection at `FolderHandling.cs:228-231` is the identity whenever the archive root is null or empty, which preserves pre-change behaviour for the standard path and for every existing test that supplies no globals. For an archive-rooted suggestion the preselected entry deliberately changes, which is exactly what AC12 mandates; the two criteria are in tension as authored and this is recorded as NB-8 rather than counted against either. Evidence: `evidence/regression-testing/ac12-path-normalisation.md`. |
| AC12 | **PASS** | `ProjectPredeterminedFolder` at `FolderHandling.cs:257-271` normalises the carried value before both the `FolderContains` probe and `SetFolderSelectedItem`. `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` covers an archive-rooted suggestion and asserts `SetFolderSelectedIndex` is never called. The resolution, and why the consumer side rather than the producer side was normalised, is stated in `evidence/other/change-description.md:15-51`. NB-2 in the code review records an edge case where the mirror is imperfect; it does not defeat the criterion. Evidence: `evidence/regression-testing/ac12-path-normalisation.md`. |
| AC13 | **PASS** | Reviewer verified independently that baseline lines 246 and 277 of `QfcHomeControllerRunAsyncHighConfidenceTests.cs` fall between diff hunks and are therefore untouched; both are disabled-mode `Times.Never` assertions on the carrier overload. The `preFilterInvoked` assertions survive at `QfcHomeControllerIssue218Tests.cs:167-176` and `RunAsyncHighConfidenceTests.cs:276-287` as diff context. `HighConfidencePreFilterLoader` has no production invocation; `FilterAsync`'s only edits are the tuple widening required to compile. Evidence: `evidence/other/test-reconciliation.md`. |
| AC14 | **PASS** | Reviewer read `QfcHomeController.Iteration.cs:12-64` in full: the `batch.Stop == QfcDequeueStop.SourceExhausted` guard, the `listObjects.Count > 0` test and the `CompleteAddingAsync` call are unchanged; the only edit is the third argument to `EnqueueAsync`. The carrier overload of `LoadItemsAsync` (`QfcFormController.Actions.cs:120-134`) returns early on `preScored is null`, the same null-not-empty condition as the `IList<MailItem>` overload at `:67-79`. Evidence: `evidence/other/carrier-chain.md`. |
| AC15 | **PASS** | `evidence/other/change-description.md:53-78` states the delta explicitly, distinguishes the bounded leg-A interval from the unbounded leg-B one, and separates what is frozen (the scores computed during the scan) from what is not (the array construction, ordering and recents section, which are still materialised lazily at display time). Evidence: `evidence/other/change-description.md`. |
| AC16 | **PASS** | The test exists at `QfcItemController.FolderHandlingTests.Part2.cs:78-106`, uses a Moq delegate mock and a `Times.Never()` assertion, and additionally asserts the carried instance was adopted. RED evidence records a scoped single-test run at exit 1 with `Total tests: 1, Failed: 1`, the sentinel `InvalidOperationException` identified by message, a stack frame through `QfcItemController.LoadFolderHandlerAsync`, and a preceding exit-0 build ruling out a stale assembly. Evidence: `evidence/regression-testing/ac16-red.md` and `ac16-green.md`. |
| AC17 | **PASS** | Both verifications are rewritten in place rather than deleted: `QfcHomeControllerIssue218Tests.cs:179-190` inverts to assert the carrier overload is selected `Times.Once`, and `:283-287` inverts to assert the `IList<MailItem>` overload is `Times.Never`. Each carries an updated reason string naming issue #678. Reviewer checked the whole test diff for weakening: the two Issue #424 tests removed from `RunAsyncHighConfidenceTests.cs` were relocated verbatim into `...Part2.cs` with their setups retargeted to the outcome-returning member, not deleted; the pointer comment at `:326-329` records the move. Evidence: `evidence/other/test-reconciliation.md`. |
| AC18 | **PASS** | Reviewer inspected all five new or modified test files. Every added test uses `[TestMethod]`, `Mock<T>` and `.Should()`. No `Path.GetTempFileName`, `Path.GetTempPath` or file creation appears. `MailItem` is a Moq double in every case; the one concrete `QfcQueue` construction passes a null home controller and mocked globals. Evidence: `evidence/other/test-reconciliation.md`. |
| AC19 | **PASS** | Four gates, each with its own artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. The reviewer re-ran gate 1 independently: `dotnet tool run csharpier check .` returned `Checked 1574 files in 4737ms.` at exit 0, matching the recorded result. Gates 2 and 3 use the policy commands verbatim with `/t:Rebuild` and report `CoreCompile` counts of 63 and 71, proving neither was vacuous. Gate 4 reports 6946 of 6946 passing and produced the Cobertura document the reviewer parsed. Evidence: `evidence/qa-gates/final-toolchain-pass.md`. |
| AC20 | **FAIL** | Three of four clauses hold and one fails. **Holds:** no regression on changed lines — repository-wide line 85.3973 to 85.4119 and branch 79.4239 to 79.4494, every non-exempt file's added executable lines 100 % covered except the relocation target, combined `QfcQueue` surface 41.47 % to 44.90 %. **Holds:** figures recorded numerically on both sides. **Holds:** zero exclusion attributes added or removed, reproduced by the reviewer over the three-dot diff. **Fails:** `QfcQueue.EnqueueAsync` at 0/46 and `QfcQueue.LoadControllersViewersAsync` at 0/24 do not reach 90 %, and both are modified members, each having gained a parameter. Both figures reproduced independently by the reviewer from `coverage/coverage.cobertura.xml`. Left unchecked in `issue.md`, correctly. Evidence: `evidence/qa-gates/coverage-delta.md`. |
| AC21 | **PASS** | Reviewer measured every changed `.cs` file on both sides. No file crossed the 500-line limit as a result of this change. The three files that remain over it were over it at the base ref and are all smaller now: `QfcCollectionController.cs` 2446 to 2336, `QfcFormControllerTests.cs` 827 to 792, `QfcQueue.cs` 610 to 505. Additions went into four new production and test partial parts rather than extending the oversized files, which is what the criterion requires. Evidence: `evidence/qa-gates/file-size-audit.md`. |
| AC22 | **PASS** | All six named items carry a verdict with a file and line: four `CONFIRMED-DEFECT`, two `NOT-CONFIRMED`. Reviewer spot-checked the two most consequential: the synchronous `LoadFolderHandler` at `FolderHandling.cs:27-55` is untouched by any diff hunk and still omits `InitAsync` in both branches, and `QfcItemController.ViewerSetup.cs:387` is unchanged. Each confirmed defect names the same referral route, a single consolidated follow-up issue filed from a separate branch after merge. No promotion tool was run and no issue was opened from this branch. Evidence: `evidence/other/out-of-scope-register.md`. |
| AC23 | **PASS** | Reviewer re-derived the footprint from `git diff --numstat 807fb0bb...HEAD`: 16 paths under `QuickFiler/`, 19 under `QuickFiler.Test/`, 43 under this feature folder, and zero outside those three prefixes. Nothing under `.claude/`, nothing named `CLAUDE.md`, nothing under `UtilitiesCS/`, no policy document. Evidence: `evidence/qa-gates/scope-confinement.md`. |

Rows: 23. No more, no fewer.

## Verdict distribution

| Verdict | Count | Criteria |
|---|---:|---|
| PASS | 22 | AC1-AC19, AC21-AC23 |
| PARTIAL | 0 | — |
| FAIL | 1 | AC20 |
| Not evaluated for lack of evidence | 0 | — |

## Check-off actions taken by this reviewer

None. All 22 criteria this reviewer evaluated as PASS were already checked `- [x]` in `issue.md`, and
the one criterion evaluated as FAIL, AC20, was already left `- [ ]`. The checkbox state in `issue.md`
therefore already matches this audit exactly and required no edit. No criterion text was altered.

## AC20 adjudication

The caller asked for an independent determination on three points. Each is answered from evidence the
reviewer gathered directly.

**(a) Were the two members genuinely at 0 % before the change?** Yes, and this is established
without relying on the executor's subtraction arithmetic. At the base ref `807fb0bb`, `git grep`
over `QuickFiler.Test/` finds three references to `EnqueueAsync`, all of them Moq setups or
verifications on a `Mock<IQfcQueue>` (`QfcHomeControllerIterationTests.cs:133`, `:175`, `:282`). The
three test classes that construct a concrete `QfcQueue` — `QfcQueueCoverageExpansionTests`,
`QfcQueuePurePathsTests` and `QfcQueueTests` — never call `EnqueueAsync`.
`LoadControllersViewersAsync` was `private` and has no reference of any kind in the test project.
Neither member was reachable from any test, so neither could have carried a covered line. The
executor's independent arithmetic agrees: 69 executable lines left `QfcQueue.cs` and exactly one
covered line left with them. **This is not a coverage regression on changed lines.**

**(b) Does ">= 90 % for new or modified members" bind a member that was merely relocated?** The
members were not merely relocated. `EnqueueAsync` gained a third parameter and a new argument to its
inner call; `LoadControllersViewersAsync` gained an optional parameter and two body statements. Both
are modified by any reading. The criterion says "new **or modified**", so it binds them, and the
lenient reading — that a relocation resets the obligation — is not available. The executor reached
the same conclusion and did not take the lenient path. **AC20's fourth clause applies and fails.**

It is worth stating that AC20 as authored is unsatisfiable for these two members. Reaching 90 % needs
a headless seam over `AddAsync` and the UI-idle marshal, which no criterion authorises and which
would be a far wider change than the fix; the only other route is an exclusion attribute, which the
same criterion forbids. A criterion that forbids both available remedies cannot be met. That is a
defect in the criterion, not in the delivery.

**(c) Blocking or non-blocking?** **Non-blocking.** The distinction that matters is between an
acceptance criterion and a repository policy floor. AC20 fails. No policy floor does:

- `.claude/rules/general-unit-test.md` and `quality-tiers.md` require line >= 85 % and branch >= 75 %
  repository-wide. Measured: 85.4119 % and 79.4494 %. Both cleared.
- `CLAUDE.md` UT2 requires repository-wide >= 80 % and >= 90 % for "new modules, classes, or methods
  **added**". These two methods were not added; they existed at the base ref. The 90 % rule in the
  policy text does not reach them, and the 80 % repository floor is cleared.
- Both policy texts require no reduction in coverage for changed lines. Established under (a).
- The genuinely new code in the relocation target — the `ItemControllerFactory` production default
  and `ResolveCarriedHandler` — measures 25 of 25 lines, that is 100 %.

Recommendation: merge with AC20 recorded as an accepted, maintainer-visible exception, and fold the
criterion's unsatisfiability into the consolidated follow-up issue alongside the finding that
`QuickFiler/Controllers/QfcQueue.cs` is five lines over the file-size limit. Closing that overage
would relocate a third member and is the natural place to also introduce a headless seam if the
coverage of `EnqueueAsync` is ever to be raised.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md`, section `## Acceptance Criteria`
- Total AC items: 23
- Checked off (delivered): 22
- Remaining (unchecked): 1
- Items remaining: AC20 — "Coverage does not regress on the changed lines and every new or modified member reaches at least 90% line coverage. Baseline and post-change coverage figures are recorded numerically. No `[ExcludeFromCodeCoverage]` attribute is added or removed anywhere in the change."

## Merge readiness

**Ready to merge**, subject to the maintainer accepting the AC20 exception recorded above. Zero
blocking findings. Eight non-blocking findings, all enumerated in `code-review.2026-09-01T23-35.md`;
NB-1 is the one with behavioural weight and belongs first in the consolidated follow-up issue.
