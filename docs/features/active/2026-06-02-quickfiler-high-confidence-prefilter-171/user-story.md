# `2026-06-02-quickfiler-high-confidence-prefilter` — User Story

- Issue: #171
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-06-02T13-45

## Story Statement

- As a QuickFiler user launching the "QuickFiler — High Confidence" entry point, I want the email batch scored and filtered before the window opens, so that I only ever see emails whose top suggested folder meets the confidence threshold, with no flicker of low-confidence emails appearing and then disappearing.
- As a QuickFiler user in high-confidence mode, I want each shown email to already have its predetermined high-confidence folder selected, so that I can file it with a single confirmation instead of choosing the folder myself.

## Problem / Why

The Issue #169 high-confidence mode filters after the fact: the full batch is materialized, rendered into UI item controllers, and shown, and only then are below-threshold items scored and removed from the populated view. This produces a transient display of below-threshold emails and couples scoring to the UI item controllers, so the folder choice is not pre-selected from the prediction. Users expect the filter to take effect before anything is shown: the candidate email list should be scored first, below-threshold and no-suggestion emails should never reach the UI, and surviving emails should arrive with their high-confidence folder already chosen. This is a redesign of the Issue #169 behavior, moving scoring and filtering ahead of UI construction.

## Personas & Scenarios

- Persona: High-volume QuickFiler user
  - who the user is: An Outlook user who regularly processes large inboxes with QuickFiler and relies on Bayesian folder suggestions to file email.
  - what they care about: Speed and accuracy when clearing email that can be filed with high certainty, and a stable view with no flicker.
  - their constraints: Limited time per session; does not want to change the familiar QuickFiler flow for ambiguous items.
  - their goals and frustrations: Wants to confirm-and-file confidently classified email immediately; frustrated by low-confidence items briefly appearing and by having to pick the folder when the classifier is already confident.
  - their context and motivations: Works through the initial batch QuickFiler loads and wants only the high-confidence subset surfaced, pre-selected.

- Scenario: Filing the confident subset of a batch, pre-filtered
  - who is acting? The high-volume QuickFiler user.
  - what triggered the action? The user wants to clear easily classified email before handling ambiguous items.
  - what steps do they take? The user clicks "QuickFiler — High Confidence". QuickFiler scores the candidate email list off the UI thread before constructing any UI item controller. Emails whose top suggested folder probability is below the configured threshold (default 0.90), including any email with no qualifying suggestion, are eliminated from the list. The window then opens showing only the surviving above-threshold emails, each with its predetermined top-suggestion folder already selected. The user confirms and files them.
  - what obstacles or decisions occur? If the entire batch is below threshold, the view is empty and the existing zero-item advance behavior applies. The user may also choose the standard "QuickFiler" entry point for an unfiltered pass.
  - what outcome do they expect? Only above-threshold emails ever appear, each pre-selected to its high-confidence folder; no below-threshold email is ever rendered; the standard entry point continues to show all emails exactly as before.

## Acceptance Criteria

1. [x] **Pre-UI scoring and filtering.** When high-confidence mode is enabled, the candidate email list is scored and filtered in `QfcHomeController.RunAsync` between `InitEmailQueueAsync` (line 257) and `LoadItemsAsync` (line 262), before any UI item controller is constructed. Verifiable via the injected pre-filter delegate seam on `QfcHomeController` (mock the delegate; assert it is invoked before the carrier-list `LoadItemsAsync` overload and that no `IList<MailItem>` UI-load occurs). This is not a post-hoc removal. — Met. `QfcHomeController.RunAsync` HC branch + `HighConfidencePreFilterLoader` seam. Tests: `RunAsync_HighConfidenceEnabled_InvokesPreFilterBeforeCarrierLoad`, `RunAsync_HighConfidence_PreFilterPrecedesUiConstruction`. Evidence: `evidence/qa/qa-final-171.2026-06-02T10-26.md`.

2. [x] **Below-threshold exclusion.** Emails whose top suggested folder score is below the cutoff (`(long)Math.Round(threshold * 1000, 0)`) are excluded from the candidate list before UI load. Verifiable by unit-testing the pre-filter (`QfcHighConfidencePreFilter.FilterAsync`) with mixed scores and asserting only `TopScore() >= cutoff` items survive. — Met. `FilterAsync` uses `score >= cutoff && score > 0`. Test: `FilterAsync_ExcludesItemsBelowCutoff`. Evidence: `evidence/coverage/prefilter-coverage-171.2026-06-02T10-04.txt`.

3. [x] **No-suggestion exclusion.** Emails with no folder suggestion at all (`FolderScorer.Count == 0`, `TopScore() == 0`) are excluded before UI load. Verifiable via a pre-filter test with a zero-score item asserting it is not in the surviving list. — Met. Test: `FilterAsync_ExcludesZeroScoreNoSuggestion`.

4. [x] **Predetermined folder carried and preselected.** Each surviving email carries its predetermined top-suggestion folder path through to UI loading, and the item controller preselects that folder rather than selecting by index. Verifiable via the `LoadControlsAndHandlers_01Async` carrier-list overload test (assert `QfcItemGroup.PredeterminedFolder` is set) and the `AssignFolderComboBox` test (assert the predetermined folder is selected, not index 1). — Met. Tests: `FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `CarrierLoad_SetsPredeterminedFolderOnItemGroup`, `AssignFolderComboBox_WithPredeterminedFolder_SelectsThatFolderNotIndexOne` (+ `_WithoutPredeterminedFolder_SelectsIndexOne` fallback).

5. [x] **Inclusive boundary.** An email whose top score exactly equals the cutoff is retained. Verifiable via a pre-filter test with an item scoring exactly `cutoff` asserting it survives (comparison is "score < cutoff removes"). — Met. Test: `FilterAsync_RetainsItemExactlyAtCutoff` (score 900 at threshold 0.90).

6. [x] **No transient render of below-threshold emails.** In high-confidence mode the UI is constructed only for surviving items; no below-threshold email is ever rendered and then removed. Verifiable via the seam in AC1 (only the carrier list reaches `LoadItemsAsync`) and by confirming the post-UI removal pass (`ApplyHighConfidenceFilterAsync` / `RemoveBelowThresholdAsync`) is not invoked in the high-confidence load sequence. — Met. The carrier `LoadItemsAsync`/`LoadControlsAndHandlers_01Async` path does not call `ApplyHighConfidenceFilterAsync`. Test: `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` (Verify `RemoveBelowThresholdAsync` Times.Never).

7. [x] **Mode disabled => standard flow unchanged.** With high-confidence mode disabled (the default and the standard "QuickFiler" entry point), no scoring/filter pre-pass runs and the plain `IList<MailItem>` `LoadItemsAsync` path is used unchanged. Verifiable via a `QfcHomeController` test asserting the pre-filter delegate is not invoked and the `IList<MailItem>` overload is called when `HighConfidenceModeEnabled == false`. — Met. Tests: `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`, `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`. Evidence: `evidence/regression/regression-171.2026-06-02T10-26.md`.

8. [x] **Design constraints and toolchain.** Scoring/filter logic lives in the new `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`; the scoring sequence is reused (not duplicated) from the existing `FolderPredictor`/`FolderScorer` path; oversized controller files are not made materially worse; scoring runs off the UI thread (`Task.Run`) and UI construction stays on the UI thread; the pre-filter is unit-testable without live Outlook COM via DI seams. New and changed logic is covered by MSTest + Moq + FluentAssertions, and the full C# toolchain (CSharpier, .NET analyzers, nullable analysis, MSTest) passes with zero regressions. — Met. New logic in `QfcHighConfidencePreFilter.cs` (182 lines, 100% testable-surface coverage); scoring reuses `FolderPredictor`/`FolderScorer` via `FolderScoringService`; controllers grew by glue only (+35/+60/+67/+91); scoring off UI thread (`Task.Run` in RunAsync); DI seams (`HighConfidencePreFilterLoader`, `IFolderScoringService`). Evidence: `evidence/qa/qa-final-171.2026-06-02T10-26.md`, `evidence/qa/file-size-check-171.2026-06-02T10-26.md`, `evidence/coverage/coverage-comparison-171.2026-06-02T10-26.md`.

## Non-Goals

- No re-application of the pre-filter across later background batches; scope is the initial batch only, consistent with the Issue #169 batch-1 scope.
- No change to the standard "QuickFiler" entry point or to its default behavior when high-confidence mode is disabled.
- No changes to the Issue #169 settings model, ribbon entry points, or threshold input control; they are reused as-is.
- No change to the synchronous `QfcHomeController.Run()` path; only the async `RunAsync` path is changed.
- No new external dependencies, network I/O, or telemetry sinks; no temporary files or live Outlook COM in tests.
- Removal of the now-unused post-UI removal seam (`ApplyHighConfidenceFilterAsync` / `RemoveBelowThresholdAsync`) is out of scope; it is retained for backward compatibility and existing test coverage.
