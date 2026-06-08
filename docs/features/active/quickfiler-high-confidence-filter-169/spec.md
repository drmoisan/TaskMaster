# quickfiler-high-confidence-filter — Spec

- **Issue:** #169
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-01T12-29
- **Status:** Implemented (pending review)
- **Version:** 0.2

## Overview

Brief summary of the behavior and scope.

This feature adds a high-confidence filtering mode to QuickFiler. It is exposed as a separate ribbon entry point ("QuickFiler — High Confidence"), not as an in-window toggle. When invoked, the entry point launches QuickFiler in high-confidence mode. After per-item Bayesian folder scoring completes (`LoadSecondaryAsync`), the feature removes from the view any email whose top suggested folder score is below a configured threshold. Emails with no suggestion at or above the threshold (including emails with no suggestion at all) are excluded. When high-confidence mode is disabled (the default), QuickFiler behaves exactly as it does today.

- Target users/personas and primary use cases: QuickFiler users who process large batches of email and want to review only the items the classifier is confident about, so they can clear high-certainty items quickly and leave ambiguous items for a normal QuickFiler pass.
- Success metrics or expected impact: Reduced manual review time for confidently classified email; the high-confidence view contains only items whose top folder suggestion meets or exceeds the configured threshold; no behavioral change to the existing (non-filtered) QuickFiler flow.

## Behavior

Describe how the feature should behave end-to-end.

- Main user flow (happy path):
  1. The user clicks the new "QuickFiler — High Confidence" ribbon button.
  2. `RibbonViewer` invokes `RibbonController.LoadQuickFilerHighConfidenceAsync()`, which launches QuickFiler with high-confidence mode active (the `HighConfidenceModeEnabled` setting is honored / set for this launch path).
  3. QuickFiler loads and shows the window using the existing pipeline: `QfcHomeController.LaunchAsync` → `InitAsync` → `RunAsync` → `QfcFormController.LoadItemsAsync`. The initial batch of emails is materialized and rendered exactly as in the standard flow.
  4. `QfcCollectionController.LoadSecondaryAsync` runs per-item Bayesian folder scoring; for each item group, `FolderPredictor`/`FolderScorer` populates folder-path scores.
  5. After `LoadSecondaryAsync` completes and the mode is active, `QfcFormController.LoadItemsAsync` calls `QfcCollectionController.RemoveBelowThresholdAsync(threshold)`. This pass inspects each item group's top score via `FolderScorer.TopScore()` and removes from the view every group whose top score is below `(long)Math.Round(threshold * 1000, 0)`.
  6. The user sees only emails whose top suggested folder probability meets or exceeds the threshold and files them as usual.

- Alternate/edge flows:
  - High-confidence mode disabled (default): the standard "QuickFiler" entry point is used; `RemoveBelowThresholdAsync` is not called; all emails are shown exactly as today.
  - Email with no folder suggestion at or above the threshold, including emails with no suggestion at all (`FolderScorer.Count == 0`, `TopScore() == 0`): excluded from the view when the mode is active.
  - Threshold exactly at the boundary: an email whose top score equals the threshold (in 0–1000 score units) is retained; comparison is "below threshold removes," so the threshold is inclusive of the boundary value.
  - Runtime threshold change: the user adjusts the threshold percentage via the ribbon input control. The validated value is persisted and applied on the next high-confidence launch.

- Error handling and recovery behavior:
  - Invalid threshold input from the ribbon control is rejected by validation and the persisted value is left unchanged. Valid input is the percentage range [0, 100], stored as a `double` in [0.0, 1.0].
  - The removal pass runs only after `LoadSecondaryAsync` has awaited to completion, so scores are fully populated before they are read; reading `FolderScorer.TopScore()` is in-memory and does not invoke the classifier a second time.
  - Item-group removal and the corresponding UI changes are performed on the WinForms UI thread, consistent with the existing `QfcCollectionController` pattern. A removed item's move monitor is unhooked to avoid retaining unnecessary COM references.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars):
  - Ribbon action: the "QuickFiler — High Confidence" button.
  - Ribbon input control: threshold percentage entered at runtime (validated).
  - Persisted user settings: `HighConfidenceModeEnabled` (bool) and `HighConfidenceThreshold` (double).
- Outputs (artifacts, logs, telemetry):
  - A QuickFiler window whose initial batch shows only emails meeting or exceeding the threshold when the mode is active.
  - No new persisted artifacts beyond the two user settings.
- Config keys and defaults:
  - `HighConfidenceModeEnabled` : `bool`, default `false`.
  - `HighConfidenceThreshold` : `double`, default `0.90` (900 in the internal 0–1000 score scale).
  - Settings are user-scoped (`Scope="User"`) and persist across sessions.
- Versioning or backward-compatibility constraints:
  - With the mode disabled (default), behavior is identical to the current release. The existing "QuickFiler" entry point is unchanged.
  - The two new settings are additive; existing settings and their defaults are unaffected.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

- Example invocations with expected outputs (concise):
  - User clicks "QuickFiler — High Confidence" with `HighConfidenceThreshold = 0.90`. QuickFiler opens; after scoring, only emails whose top folder probability is >= 0.90 remain in the view.
  - User clicks the standard "QuickFiler" button (mode disabled). QuickFiler opens with all emails in the initial batch.
  - User sets the ribbon threshold control to 75. The value is validated, stored as `HighConfidenceThreshold = 0.75`, and applied on the next high-confidence launch.
- Contracts and validation rules:
  - `FolderScorer.TopScore()` returns the highest score in `_folderNameScores` as a `long`, or `0` if the scorer is empty. Pure in-memory computation; callable on any thread once scores are populated.
  - `IQfcCollectionController.RemoveBelowThresholdAsync(double threshold)`: removes item groups whose `FolderScorer.TopScore()` is below `(long)Math.Round(threshold * 1000, 0)`. `threshold` is a `double` in [0.0, 1.0].
  - `IAppQuickFilerSettings`: read-only `bool HighConfidenceModeEnabled { get; }` and `double HighConfidenceThreshold { get; }`. Writes occur through the concrete `AppQuickFilerSettings` (internal setter saving to `Settings.Default`).
  - Threshold input validation: accepted as a percentage in [0, 100], stored as a `double` in [0.0, 1.0]; out-of-range or non-numeric input is rejected and the persisted value is unchanged.

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants:
  - Folder probabilities are computed lazily per email after the window is shown, inside `LoadSecondaryAsync` (`QfcCollectionController.cs:430`), via `FolderPredictor` → `FolderScorer.AddBayesianSuggestionsAsync` (`FolderScorer.cs:151`). There is intentionally no datamodel-stage filtering; the Deedle DataFrame built in `InitDfAsync` (`QfcDatamodel.cs:411`) contains no probability column.
  - The filter is a post-scoring removal pass over the already-populated `_itemGroups`. It does not re-run the classifier. Score units are `(long)Math.Round(prediction.Probability * 1000, 0)` (`FolderScorer.cs:175`); the threshold is converted to the same scale for comparison.
- Caching or persistence details:
  - `HighConfidenceModeEnabled` and `HighConfidenceThreshold` are user-scoped settings persisted via `Settings.Default` (`TaskMaster/Properties/Settings.settings` and `Settings.Designer.cs`). They persist across sessions.
- Migration or backfill requirements (if any):
  - None. The two settings are additive with defaults; no data migration is required.

## Constraints & Risks

Performance, compatibility, security, rollout constraints.

- Limits (latency/throughput/memory) and acceptable trade-offs:
  - The filter applies to the initially loaded batch only, consistent with the current batch-1 scope of `InitEmailQueueAsync`. Re-application across later background batches is out of scope for v1.
  - The removal pass adds a single in-memory iteration over the initial batch's item groups after scoring completes; it does not add classifier calls.
  - Item-group removal and UI updates must run on the WinForms UI thread; scores are read only after `LoadSecondaryAsync` has awaited to completion, avoiding read-before-populate races.
- Security/privacy considerations:
  - No new external I/O, network calls, or stored personal data beyond the existing classifier inputs and the two numeric/boolean user settings.
- Operational/rollout risks and mitigations:
  - Risk: users may expect the filter to persist as later background batches load. Mitigation: document the v1 batch-1 limitation explicitly in user-facing notes.
  - Risk: a too-high threshold could hide all emails. Mitigation: the threshold is user-configurable and validated; the default (0.90) is conservative; the standard entry point remains available with no filtering.
  - Risk: divergence from existing QuickFiler behavior. Mitigation: with the mode disabled (default), no filtering code path is exercised and behavior is identical to today.

## Implementation Strategy

- Implementation scope (what changes, not sequencing):
  - Add a top-score accessor to the classifier scorer.
  - Add two persisted user settings and their interface/implementation plumbing.
  - Add a post-scoring removal pass to the collection controller and a conditional call to it from the form controller.
  - Add a second ribbon entry point and a runtime threshold input control.
  - No changes to `QfcDatamodel.cs`, `QfcHomeController.cs`, `QfcItemController.cs`, or `FolderPredictor.cs` core logic.

- New classes/functions/commands to add or update:
  - `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`: add `public long TopScore()` returning the highest value in `_folderNameScores`, or `0` if empty.
  - `UtilitiesCS/Interfaces/IGlobals/IAppQuickFilerSettings.cs`: add read-only `bool HighConfidenceModeEnabled { get; }` and `double HighConfidenceThreshold { get; }`.
  - `TaskMaster/AppGlobals/AppQuickFilerSettings.cs`: add implementations reading from `Settings.Default` with internal setters that persist via `Settings.Default.Save()`.
  - `TaskMaster/Properties/Settings.settings`: add `HighConfidenceModeEnabled` (System.Boolean, Scope="User", default `False`) and `HighConfidenceThreshold` (System.Double, Scope="User", default `0.9`).
  - `TaskMaster/Properties/Settings.Designer.cs`: add the matching generated property pair following the existing pattern (lines 506–552).
  - `QuickFiler/Interfaces/IQfcCollectionController.cs`: add `Task RemoveBelowThresholdAsync(double threshold)`.
  - `QuickFiler/Controllers/QfcCollectionController.cs`: implement `RemoveBelowThresholdAsync`, iterating `_itemGroups`, comparing `FolderScorer.TopScore()` against `(long)Math.Round(threshold * 1000, 0)`, removing below-threshold groups on the UI thread, and unhooking the move monitor for removed items.
  - `QuickFiler/Controllers/QfcFormController.cs`: after `await _groups.LoadSecondaryAsync()` in `LoadItemsAsync` (around `QfcFormController.cs:935`), add a conditional call to `RemoveBelowThresholdAsync(_globals.QfSettings.HighConfidenceThreshold)` when `_globals.QfSettings.HighConfidenceModeEnabled` is `true`.
  - `TaskMaster/Ribbon/RibbonController.cs`: add `LoadQuickFilerHighConfidenceAsync()` (mirrors `LoadQuickFilerAsync` and activates high-confidence mode) plus settings accessor/toggle helpers and a validated threshold setter.
  - `TaskMaster/Ribbon/RibbonViewer.cs`: add ribbon callbacks for the new button and the threshold input control following the existing callback pattern.
  - `TaskMaster/Ribbon/RibbonExplorer.xml`: add the "QuickFiler — High Confidence" button entry and the threshold input control.

- Dependency changes (new/removed packages) and rationale: none. All work uses existing APIs and approved libraries.

- Logging/telemetry additions and locations: use the existing project logging pattern within the new removal pass and ribbon handlers if diagnostic logging is warranted; no new telemetry sink is introduced.

- Rollout plan (feature flags, staged deploys, fallback path):
  - The `HighConfidenceModeEnabled` setting defaults to `false`, so the feature is inert until the high-confidence entry point is used. The standard "QuickFiler" entry point provides the unchanged fallback path.

## Definition of Done

- [x] Acceptance criteria (AC1–AC7 in user-story.md) documented and mapped to tests or demos: AC1 (new ribbon entry point) → RibbonController/RibbonViewer/RibbonExplorer.xml; AC2/AC3 (below-threshold and no-suggestion exclusion) → `QfcCollectionController.RemoveBelowThresholdAsync` + `FolderScorer.TopScore`; AC4 (default 0.90 persisted) → settings plumbing; AC5 (runtime validated, persisted threshold) → ribbon input control + `AppQuickFilerSettings`; AC6 (disabled = unchanged) → `QfcFormController` conditional call; AC7 (test coverage + toolchain) → MSTest suites and toolchain pass.
- [x] Behavior matches acceptance criteria in all documented environments
- [x] Tests updated/added: `FolderScorerTests` (`UtilitiesCS.Test`), `AppQuickFilerSettings` tests (`TaskMaster.Test`), `QfcCollectionControllerTests` and `QfcFormControllerTests` (`QuickFiler.Test`), `RibbonController` tests (`TaskMaster.Test`), using MSTest + Moq + FluentAssertions.
- [x] Edge cases and error handling covered by tests: empty scorer (`TopScore() == 0`); all-above, all-below, and mixed batches; exact boundary value; mode disabled (no removal call); invalid threshold input rejected by validation; null `_itemGroups`/null-settings guards.
- [x] Docs updated (README, docs/features/active/quickfiler-high-confidence-filter-169/ links): spec.md and user-story.md DoD/AC updated; plan task notes record the two test seams.
- [x] Telemetry/logging added or updated (if applicable): not applicable; no new telemetry sink introduced.
- [x] Toolchain pass completed (CSharpier → .NET analyzers → nullable analysis → MSTest) with zero regressions: see `evidence/qa/final-toolchain.2026-06-01T17-12-39Z.md`. The only failing tests are pre-existing flaky timing/concurrency tests (verified deterministic on re-run), not regressions from this work.

## v1 limitation note

v1 filters only the initially loaded batch (consistent with the current `InitEmailQueueAsync` batch-1 scope). Re-application of the high-confidence filter across later background batches is out of scope for v1 (see Constraints & Risks above).
