# Feature Audit — quickfiler-high-confidence-prefilter (Issue #171)

- Date: 2026-06-02T10-36
- Reviewer: feature-reviewer agent
- Work Mode: full-bug

## Scope and Baseline

- Resolved base branch: `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9` (merge base).
- Head: `bug/quickfiler-high-confidence-prefilter-171` @ `ae7eb670ee7738640cab2b41bc7226255224f7ca`.
- Diff range audited: `5e944344041b10becb98c56d358176fc9e7b8ee9..ae7eb670ee7738640cab2b41bc7226255224f7ca` (full branch diff vs base; no narrowing applied).
- AC sources: `spec.md` Definition of Done (authoritative for full-bug) and `user-story.md` AC1-AC8 (named in the workflow input). Both were evaluated; AC1-AC8 are mirrored between the two files.
- Evidence basis: source reading of the diff and changed files, plus feature-folder evidence under `evidence/`. Coverage was verified for artifact presence (not re-executed).

## Acceptance Criteria Inventory

From `user-story.md` / `spec.md` Definition of Done (AC1-AC8):

1. AC1 — Pre-UI scoring and filtering in `QfcHomeController.RunAsync` before any UI item controller is constructed (seam-verifiable; not post-hoc).
2. AC2 — Below-threshold exclusion (score below `(long)Math.Round(threshold*1000,0)`).
3. AC3 — No-suggestion exclusion (zero score).
4. AC4 — Predetermined folder carried and preselected (not index-based).
5. AC5 — Inclusive boundary (score == cutoff retained).
6. AC6 — No transient render of below-threshold emails (carrier path does not invoke the post-UI removal pass).
7. AC7 — Mode disabled => standard `IList<MailItem>` flow unchanged; no pre-pass.
8. AC8 — Design constraints and toolchain (new-file isolation, scoring reuse, off-UI-thread, DI seams, MSTest+Moq+FluentAssertions coverage, full toolchain passes).

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `QfcHomeController.RunAsync` adds an HC branch between `InitEmailQueueAsync` and `LoadItemsAsync` that runs `HighConfidencePreFilterLoader` inside `Task.Run` and then calls the carrier `LoadItemsAsync`. Tests `RunAsync_HighConfidenceEnabled_InvokesPreFilterBeforeCarrierLoad` (carrier overload Times.Once, plain overload Times.Never) and `RunAsync_HighConfidence_PreFilterPrecedesUiConstruction`. |
| AC2 | PASS | `QfcHighConfidencePreFilter.FilterAsync` retains `result.score >= cutoff && result.score > 0`; `cutoff = (long)Math.Round(threshold*1000,0)`. Test `FilterAsync_ExcludesItemsBelowCutoff` (899 excluded, 901/1000 retained at cutoff 900). |
| AC3 | PASS | `&& result.score > 0` excludes zero-score items. Test `FilterAsync_ExcludesZeroScoreNoSuggestion`. |
| AC4 | PASS | `QfcPreScoredItem` carries `PredeterminedFolder`; threaded through `EncapsulateItemGroup` -> `QfcItemGroup.PredeterminedFolder` -> `QfcItemController` ctor -> `PopulateAndSelectFolder`, which selects the predetermined folder's index when present. Tests `FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `CarrierLoad_SetsPredeterminedFolderOnItemGroup`, `AssignFolderComboBox_WithPredeterminedFolder_SelectsThatFolderNotIndexOne` (asserts SelectedIndex 3, not 1). |
| AC5 | PASS | Comparison is `>= cutoff` (inclusive). Test `FilterAsync_RetainsItemExactlyAtCutoff` (score 900 at threshold 0.90 survives). |
| AC6 | PASS | Carrier `LoadItemsAsync` constructs UI only for survivors and intentionally does not call `ApplyHighConfidenceFilterAsync`; it calls `LoadSecondaryAsync` (load-only, no removal — verified by reading `QfcCollectionController.LoadSecondaryAsync`, which has no `RemoveBelowThresholdAsync` call). Test `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` (RemoveBelowThresholdAsync Times.Never). |
| AC7 | PASS | `RunAsync` `else` branch uses the plain `IList<MailItem>` overload when `HighConfidenceModeEnabled == false`. Tests `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload` and `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly` (plain Times.Once, carrier Times.Never, pre-filter not invoked). |
| AC8 | PARTIAL | New logic isolated in `QfcHighConfidencePreFilter.cs`; scoring reuses `FolderPredictor`/`FolderScorer` via `FolderScoringService`; off-UI-thread (`Task.Run`); DI seams present (`IFolderScoringService`, `HighConfidencePreFilterLoader`); MSTest+Moq+FluentAssertions coverage present. Toolchain evidence reports format/analyzers/nullable clean for Issue #171 files and tests passing. However, the AC8 coverage/toolchain claim cannot be fully verified: the mandatory canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent, so the ">= 90% new-file / no-regression" coverage portion of AC8 is unverifiable from the canonical source. The design/seam/reuse/off-thread sub-claims PASS; the coverage-gate sub-claim is UNVERIFIED, yielding an overall PARTIAL. |

## Summary

AC1-AC7 PASS based on source verification of the implementation and the corresponding deterministic unit tests. AC8 is PARTIAL: every design, seam, reuse, threading, and test-presence sub-claim is satisfied, but its coverage/toolchain sub-claim cannot be verified because the mandatory canonical C# coverage artifact is absent. The feature behavior is delivered as specified; the gap is in coverage-evidence form, not in feature implementation.

Feature-behavior verdict: delivered (AC1-AC7 PASS, AC8 design portion PASS). Process verdict: PARTIAL pending the canonical coverage artifact. Remediation is triggered for the coverage-artifact gap (see `remediation-inputs.2026-06-02T10-36.md`).

## Acceptance Criteria Check-off

All eight AC items were already marked `- [x]` in both `user-story.md` and `spec.md` by the executor. The reviewer evaluated each item independently:

- AC1-AC7: confirmed PASS; `[x]` retained (consistent with evidence).
- AC8: evaluated PARTIAL. Per `acceptance-criteria-tracking`, a PARTIAL item should not be marked `[x]`. The criterion text was authored by the planning agent and is already `[x]` in the source files; the reviewer does not author or remove AC items, and per the tracking protocol the reviewer leaves the source text unmodified but records the gap here. The AC8 `[x]` in the source files overstates the coverage/toolchain sub-claim until the canonical C# coverage artifact exists; this is documented as the outstanding item rather than silently accepted.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/user-story.md` and `.../spec.md`
- Total AC items: 8
- Checked off (delivered): 7 fully verified (AC1-AC7); AC8 marked `[x]` in source but evaluated PARTIAL by the reviewer
- Remaining (unchecked): 0 unchecked in source; 1 overstated (AC8 coverage/toolchain sub-claim)
- Items remaining (gap): AC8 — coverage/toolchain sub-claim unverifiable until `artifacts/csharp/coverage.xml` is produced
