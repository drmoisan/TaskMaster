# Feature Audit — quickfiler-high-confidence-prefilter (Issue #171)

- Date: 2026-06-02T11-06
- Reviewer: feature-reviewer agent
- Review type: RE-AUDIT following remediation (supersedes `feature-audit.2026-06-02T10-36.md`)
- Work Mode: full-bug

## Scope and Baseline

- Resolved base branch: `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9` (merge base).
- Head: `bug/quickfiler-high-confidence-prefilter-171` @ `9ddaa32e750be3ef29c9103cb8b7852b8ea6a9e7`.
- Diff range audited: `5e944344041b10becb98c56d358176fc9e7b8ee9..9ddaa32e750be3ef29c9103cb8b7852b8ea6a9e7` (full branch diff vs base; no narrowing applied).
- AC sources: `spec.md` Definition of Done (authoritative for full-bug) and `user-story.md` AC1-AC8 (named in the workflow input). Both were evaluated; AC1-AC8 are mirrored between the two files.
- Evidence basis: source reading of the diff and changed files, feature-folder evidence under `evidence/`, and the reviewer's direct parse of the now-present canonical coverage artifact `artifacts/csharp/coverage.xml`.

## Acceptance Criteria Inventory

From `user-story.md` / `spec.md` Definition of Done (AC1-AC8):

1. AC1 — Pre-UI scoring and filtering in `QfcHomeController.RunAsync` before any UI item controller is constructed (seam-verifiable; not post-hoc).
2. AC2 — Below-threshold exclusion (score below `(long)Math.Round(threshold*1000,0)`).
3. AC3 — No-suggestion exclusion (zero score).
4. AC4 — Predetermined folder carried and preselected (not index-based).
5. AC5 — Inclusive boundary (score == cutoff retained).
6. AC6 — No transient render of below-threshold emails (carrier path does not invoke the post-UI removal pass).
7. AC7 — Mode disabled => standard `IList<MailItem>` flow unchanged; no pre-pass.
8. AC8 — Design constraints and toolchain (new-file isolation, scoring reuse, off-UI-thread, DI seams, MSTest+Moq+FluentAssertions coverage, full toolchain passes with zero regressions).

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `QfcHomeController.RunAsync` adds an HC branch between `InitEmailQueueAsync` and `LoadItemsAsync` that runs `HighConfidencePreFilterLoader` inside `Task.Run` and then calls the carrier `LoadItemsAsync`. Tests `RunAsync_HighConfidenceEnabled_InvokesPreFilterBeforeCarrierLoad` (carrier overload Times.Once, plain overload Times.Never) and `RunAsync_HighConfidence_PreFilterPrecedesUiConstruction`. |
| AC2 | PASS | `QfcHighConfidencePreFilter.FilterAsync` retains `result.score >= cutoff && result.score > 0`; `cutoff = (long)Math.Round(threshold*1000,0)`. Test `FilterAsync_ExcludesItemsBelowCutoff`. |
| AC3 | PASS | `&& result.score > 0` excludes zero-score items. Test `FilterAsync_ExcludesZeroScoreNoSuggestion`. |
| AC4 | PASS | `QfcPreScoredItem` carries `PredeterminedFolder`; threaded through `EncapsulateItemGroup` -> `QfcItemGroup.PredeterminedFolder` -> `QfcItemController` ctor -> `PopulateAndSelectFolder`, which selects the predetermined folder's index when present. Tests `FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `CarrierLoad_SetsPredeterminedFolderOnItemGroup`, `AssignFolderComboBox_WithPredeterminedFolder_SelectsThatFolderNotIndexOne`. |
| AC5 | PASS | Comparison is `>= cutoff` (inclusive). Test `FilterAsync_RetainsItemExactlyAtCutoff` (score 900 at threshold 0.90 survives). |
| AC6 | PASS | Carrier `LoadItemsAsync` constructs UI only for survivors and does not call `ApplyHighConfidenceFilterAsync`; it calls `LoadSecondaryAsync` (load-only, no removal — `QfcCollectionController.LoadSecondaryAsync` has no `RemoveBelowThresholdAsync` call). Test `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` (RemoveBelowThresholdAsync Times.Never). |
| AC7 | PASS | `RunAsync` `else` branch uses the plain `IList<MailItem>` overload when `HighConfidenceModeEnabled == false`. Tests `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload` and `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly` (plain Times.Once, carrier Times.Never, pre-filter not invoked). |
| AC8 | PASS | Design/seam/reuse/threading sub-claims: new logic isolated in `QfcHighConfidencePreFilter.cs`; scoring reuses `FolderPredictor`/`FolderScorer` via `FolderScoringService`; off-UI-thread (`Task.Run`); DI seams (`IFolderScoringService`, `HighConfidencePreFilterLoader`). Coverage/toolchain sub-claim now verifiable from the canonical artifact: reviewer-parsed `artifacts/csharp/coverage.xml` shows `QfcHighConfidencePreFilter.cs` at 100.00% (>= 90% new-file gate) and no changed-line regression across the modified files; format/analyzers/nullable clean for Issue #171 files; Issue #171 tests pass. The prior-round gap (absent canonical artifact) that held AC8 at PARTIAL is resolved. |

## Summary

All eight acceptance criteria PASS. AC1-AC7 are verified by source reading of the implementation and the corresponding deterministic unit tests. AC8 — which was PARTIAL in the prior round solely because the mandatory canonical C# coverage artifact was absent — is now PASS: the artifact `artifacts/csharp/coverage.xml` exists, parses, and the reviewer independently confirmed the >= 90% new-file gate (100%) and no changed-line regression. The COM/WinForms-bound modified controllers retain their pre-existing low coverage with no regression, which is within the General Unit Test Policy change-scope gates and is a documented pre-existing condition (see `policy-audit.2026-06-02T11-06.md` §5, note A).

Feature-behavior verdict: delivered (AC1-AC8 PASS). Process verdict: PASS. No remediation required.

## Acceptance Criteria Check-off

All eight AC items were already marked `- [x]` in both `user-story.md` and `spec.md`. This re-audit evaluated each item independently against the full branch diff and the now-present canonical coverage artifact:

- AC1-AC7: confirmed PASS; `[x]` retained (consistent with evidence).
- AC8: re-evaluated as PASS (previously PARTIAL pending the canonical coverage artifact, which now exists and was verified). `[x]` retained and now fully substantiated.

The reviewer confirms the existing `[x]` marks in `user-story.md` and `spec.md` are accurate as of this re-audit; no source AC text was authored or removed by the reviewer.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/user-story.md` and `.../spec.md`
- Total AC items: 8
- Checked off (delivered and verified): 8 (AC1-AC8)
- Remaining (unchecked): 0
- Items remaining (gap): none
