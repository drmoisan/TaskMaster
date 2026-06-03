# 2026-06-02-quickfiler-high-confidence-prefilter (Spec)

- **Issue:** #171
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-02T13-45
- **Status:** Draft
- **Version:** 0.2

## Context
QuickFiler high-confidence mode (Issue #169) applies its confidence filter only after every email has been fully materialized and loaded into the UI item controllers. The filter must instead run before the emails are loaded into UI objects: the email list must be scored and filtered first, and only emails whose top suggested folder meets or exceeds the configured confidence threshold (default 90%) may ever be passed to the UI.

This is a redesign of the Issue #169 behavior, not a tweak. Issue #169 shipped a post-UI removal pass (`QfcFormController.ApplyHighConfidenceFilterAsync` -> `QfcCollectionController.RemoveBelowThresholdAsync`) that runs after the window is shown and after UI item controllers exist. Issue #171 moves scoring and filtering ahead of UI construction so below-threshold emails never reach the UI at all.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Python version: N/A (C# / VSTO Outlook add-in)
- Command/flags used: Ribbon entry point "QuickFiler — High Confidence"
- Data source or fixture: Live Outlook mailbox with a Bayesian folder classifier trained

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Launch QuickFiler via the "QuickFiler — High Confidence" ribbon entry point.
2. Observe the initial batch load.
3. All emails in the batch are materialized and rendered into UI item controllers first.
4. Per-item Bayesian scoring (`LoadSecondaryAsync`) then runs, and only afterward are below-threshold groups removed from the already-populated view (`ApplyHighConfidenceFilterAsync` -> `RemoveBelowThresholdAsync`).

Expected:
- The email list is filtered before being loaded into the UI item controllers.
- Folder scoring runs on the candidate email list first; any email that cannot be resolved to a suggested folder at or above the threshold (default 90%) is eliminated from the list entirely.
- The UI receives only emails at or above the threshold, each fed in with its predetermined high-confidence folder choice already selected (because all surviving items are above the threshold).
- The UI never receives an email below the threshold.

Actual:
- The full batch is loaded into UI item controllers and the window is shown before any filtering occurs.
- Scoring and below-threshold removal happen after the UI objects exist, so the UI transiently receives and renders emails below the threshold, then removes them.
- The folder choice is not pre-selected from the high-confidence prediction.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: N/A


## Scope & Non-Goals
- In scope:
  - A pre-UI scoring and filtering pass over the candidate `IList<MailItem>` produced by `InitEmailQueueAsync`, executed before `QfcFormController.LoadItemsAsync` constructs any UI item controller.
  - Elimination, from the candidate list, of every email whose top suggested folder score is below the configured threshold (default 0.90 -> 900 on the 0-1000 scale), including emails with no suggestion at all.
  - Carrying each surviving email's predetermined top-suggestion folder through to UI loading so the item controller preselects that folder instead of selecting by index.
  - A new, separate file for the pre-filter logic to avoid further inflating the oversized controller files.
  - DI seams (injectable factory delegate and/or interface) so the pre-filter and its scoring step are unit-testable without live Outlook COM.
  - MSTest + Moq + FluentAssertions coverage for the new and changed logic.
- Out of scope / non-goals:
  - Re-application of the pre-filter across later background batches delivered through `IterateQueueAsync` / `QfcQueue.EnqueueAsync`. Scope is the initial batch only, consistent with the Issue #169 batch-1 scope.
  - The standard (non-high-confidence) "QuickFiler" entry point and its behavior, which must remain exactly unchanged.
  - The synchronous `QfcHomeController.Run()` path (lines 231-243); only the async `RunAsync` path is changed.
  - The settings model, ribbon entry points, and threshold input control delivered in Issue #169, which are reused as-is.
- Explicitly excluded systems, integrations, or datasets:
  - No new external services, network calls, or persisted artifacts beyond the existing Issue #169 settings.
  - No temporary files in tests; no live Outlook COM in tests.

## Root Cause Analysis
- `QuickFiler/Controllers/QfcFormController.cs` `LoadItemsAsync` builds UI controllers (`LoadControlsAndHandlers_01Async`), shows the window, then calls `LoadSecondaryAsync` and `ApplyHighConfidenceFilterAsync`.
- `QuickFiler/Controllers/QfcCollectionController.cs` `LoadSecondaryAsync` performs per-item scoring against `_itemGroups` (UI controllers), and `RemoveBelowThresholdAsync` removes below-threshold groups post-hoc.
- `QuickFiler/Controllers/QfcHomeController.cs` `RunAsync` builds the email list via `InitEmailQueueAsync` then hands it to `LoadItemsAsync`.
- The Issue #169 design intentionally placed the filter after `LoadSecondaryAsync` because folder probabilities are computed lazily per item by the UI item controllers. The defect in Issue #171 is structural: scoring is coupled to the UI item controllers, so the only place the score is available is after the controllers exist and the window is shown. The redesign decouples scoring from the UI item controllers so scoring can run on the raw `IList<MailItem>` before any UI object is created.
- Standard (non-high-confidence) QuickFiler behavior must remain unchanged.


## Proposed Fix

### Design summary (what changes where):
- Add a pre-UI scoring and filtering pass in `QfcHomeController.RunAsync`, between the `InitEmailQueueAsync` call and the `LoadItemsAsync` call (the seam between lines 257 and 262; see research section D). The pass runs only when `globals.QfSettings.HighConfidenceModeEnabled` is `true`.
- Place the scoring and filtering logic in a new file `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`. This file holds the per-item scoring helper, the batch filter, and a small carrier type that pairs a surviving `MailItem` with its predetermined top-suggestion folder path.
- Reuse the existing scoring path (`FolderPredictor` + `FolderScorer`) rather than duplicating it. Extract the `FromField` scoring sequence currently inlined in `QfcItemController.LoadFolderHandlerAsync` into a shared helper that both the pre-filter and the existing item-controller path call (DRY).
- Carry the predetermined folder through UI loading. The surviving items become a list of `(MailItem, predeterminedFolder)` pairs that flow into a new overload of `LoadItemsAsync` -> `LoadControlsAndHandlers_01Async` -> `EncapsulateItemGroup`. Each item group records its `PredeterminedFolder`, and `AssignFolderComboBox` preselects that folder instead of selecting index 1.
- In high-confidence mode, the pre-filter has already removed below-threshold items, so the post-UI removal pass is no longer invoked from the standard load sequence for that mode.

### Boundaries and invariants to preserve:
- The UI must never render a below-threshold email. There is no transient render-then-remove in high-confidence mode.
- Boundary is inclusive: an email whose top score exactly equals the cutoff (`(long)Math.Round(threshold * 1000, 0)`) is retained. Comparison is "score < cutoff removes," consistent with the existing 0-1000 cutoff math in `RemoveBelowThresholdAsync`.
- When high-confidence mode is disabled (the default and the standard "QuickFiler" entry point), no scoring or filtering pre-pass runs and behavior is exactly unchanged.
- Scope is the initial batch only.

### Dependencies or blocked work:
- Depends on Issue #169 settings and entry points already in place: `IAppQuickFilerSettings.HighConfidenceModeEnabled`, `IAppQuickFilerSettings.HighConfidenceThreshold` (default 0.90), and the high-confidence ribbon entry point. No changes to those are required.
- No external blocked work.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- NEW `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` — per-item scoring helper, batch filter, and `QfcPreScoredItem` carrier type.
- `QuickFiler/Controllers/QfcHomeController.cs` — add the conditional pre-filter call between `InitEmailQueueAsync` and `LoadItemsAsync` in `RunAsync`; add an injectable factory delegate for the pre-filter so the controller stays unit-testable.
- `QuickFiler/Controllers/QfcFormController.cs` — add a `LoadItemsAsync` overload accepting the pre-scored carrier list; in high-confidence mode, do not invoke the post-UI removal pass.
- `QuickFiler/Interfaces/IQfcFormController.cs` — add the pre-scored `LoadItemsAsync` overload to the interface.
- `QuickFiler/Controllers/QfcCollectionController.cs` — add a `LoadControlsAndHandlers_01Async` overload accepting the pre-scored carrier list; pass `predeterminedFolder` through `EncapsulateItemGroup`.
- `QuickFiler/Interfaces/IQfcCollectionController.cs` — add the pre-scored `LoadControlsAndHandlers_01Async` overload to the interface.
- `QuickFiler/Controllers/QfcItemController.cs` — make `AssignFolderComboBox` honor the predetermined folder (preselect that folder instead of index 1) when one is supplied.
- `QuickFiler/Helper_Classes/QfcItemGroup.cs` (or wherever `QfcItemGroup` lives) — add a `string? PredeterminedFolder` property carried from `EncapsulateItemGroup`.
- Tests: `QuickFiler.Test` controller test suites plus new tests for the pre-filter; `UtilitiesCS.Test` `FolderScorer` test if a `TopFolderPath` accessor is added.

The exact extraction and sequencing are the responsibility of the atomic planner. The constraint is that oversized files must not be made materially worse and new logic goes in the new file.

#### Functions/classes/CLI commands impacted:
- New: `QfcHighConfidencePreFilter` (scoring helper + `FilterAsync`), `QfcPreScoredItem` carrier type.
- Changed: `QfcHomeController.RunAsync`; `QfcFormController.LoadItemsAsync` (new overload); `QfcCollectionController.LoadControlsAndHandlers_01Async` (new overload) and `EncapsulateItemGroup`; `QfcItemController.AssignFolderComboBox`.
- Reused (no behavior change to callers): `FolderPredictor.InitAsync` (`FromField`), `FolderScorer.TopScore()`, the score scale `(long)Math.Round(prediction.Probability * 1000, 0)`.
- Retained but not invoked in the high-confidence load sequence: `QfcFormController.ApplyHighConfidenceFilterAsync`, `QfcCollectionController.RemoveBelowThresholdAsync` (kept so existing tests pass and as a defensive seam).

#### Data flow and validation changes:
- Before: `InitEmailQueueAsync` -> `LoadItemsAsync(IList<MailItem>)` -> UI controllers -> show window -> `LoadSecondaryAsync` -> post-UI removal.
- After (high-confidence mode): `InitEmailQueueAsync` -> pre-filter (`FilterAsync`) producing `IList<QfcPreScoredItem>` (each surviving item carries its predetermined folder) -> `LoadItemsAsync(IList<QfcPreScoredItem>)` -> UI controllers built only for surviving items, each preselecting its predetermined folder -> show window.
- After (standard mode): unchanged `IList<MailItem>` path; no pre-filter, no carrier list.
- Validation: an item with no suggestion (`FolderScorer.Count == 0`, `TopScore() == 0`) is excluded. An item with `TopScore() < cutoff` is excluded. An item with `TopScore() >= cutoff` is retained and assigned its top-suggestion folder path.

#### Error handling and logging updates:
- Cancellation: the existing `CancellationToken` on `QfcHomeController` is threaded through the pre-filter chain.
- Scoring is per-item; an item that fails to score (e.g., classifier returns no predictions) is treated as below threshold and excluded, not surfaced as a fatal error, consistent with the existing "no suggestion -> excluded" behavior.
- Use the existing project logging pattern if diagnostic logging of filtered counts is warranted; no new telemetry sink is introduced.

#### Rollback/feature-flag considerations (if applicable):
- The behavior is gated by `HighConfidenceModeEnabled`, which defaults to `false`. With the flag off, the pre-filter code path is not exercised and the standard flow is the fallback. The high-confidence ribbon entry point remains the only way to enable the mode for a launch.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Pre-filter input: `IList<MailItem>` (initial batch from `InitEmailQueueAsync`), `IApplicationGlobals`, `double threshold` in [0.0, 1.0], `CancellationToken`.
- Pre-filter output: `IList<QfcPreScoredItem>` where each element pairs a surviving `MailItem` with its predetermined top-suggestion folder path (`string`).
- Per-item score: `FolderScorer.TopScore()` returns the maximum value in `_folderNameScores` as a `long`, or `0` if empty. Cutoff is `(long)Math.Round(threshold * 1000, 0)`.
- Top folder path: derived from the populated scorer (top-ranked folder key). If a `TopFolderPath` accessor is added to `FolderScorer`, it returns the key with the highest score; otherwise the planner may inline `_folderNameScores.OrderByDescending(x => x.Value).First().Key` within the helper.

#### Required configuration keys and defaults:
- `HighConfidenceModeEnabled` : `bool`, default `false` (reused from Issue #169).
- `HighConfidenceThreshold` : `double`, default `0.90` (900 on the internal 0-1000 score scale; reused from Issue #169).

#### Backward-compatibility expectations:
- Standard "QuickFiler" entry point behavior is identical to the current release.
- New interface members are additive overloads; existing overloads and callers are unchanged.
- Issue #169 settings and ribbon controls are unchanged.

#### Performance constraints (latency/throughput/memory):
- Pre-scoring runs before the window is shown, so it adds startup latency proportional to the initial batch size (typically 5-12 items). Per item is an in-memory Bayesian classification plus COM-backed tokenization reads.
- Scoring runs off the UI thread in a `Task.Run` context, parallel across items (e.g., `Task.WhenAll`), consistent with the existing `LoadSecondaryAsync` pattern, to keep wall-clock startup latency comparable to the current post-UI scoring time.
- No new persistent memory beyond the transient carrier list.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - A trained Bayesian folder classifier is available via `globals.AF.Manager["Folder"]`, as in the existing path.
  - `MailItem` COM property reads (Subject, Body, sender) succeed on a thread-pool thread, as already relied upon by `QfcItemController.LoadFolderHandlerAsync`.
- Constraints (budget, performance, compatibility):
  - Scoring/filter logic must live in the new file and must not worsen the pre-existing >500-line violations in `QfcCollectionController.cs`, `QfcItemController.cs`, `QfcFormController.cs`, or `QfcHomeController.cs`.
  - Scoring of `MailItem`s runs off the UI thread via the existing `Task.Run` pattern; UI construction stays on the UI thread.
  - DI seams (injectable factory delegate / interface) are required so the pre-filter is unit-testable without live Outlook COM.
  - Reuse scoring logic rather than duplicating between the existing `LoadSecondaryAsync`/`LoadFolderHandlerAsync` path and the new pre-UI path (DRY).
  - No temporary files in tests; no external services.
- External dependencies (services, libraries, releases): none beyond approved libraries already in the project.

## Data / API / Config Impact
- User-facing or API changes:
  - In high-confidence mode, the QuickFiler window opens already showing only above-threshold emails, each with its predetermined folder preselected. No transient display of below-threshold emails.
  - Standard mode: no user-facing change.
- Data or migration considerations: none. No new persisted data; the transient carrier list is in-memory only.
- Logging/telemetry updates (if any): optional diagnostic logging of filtered counts using the existing logging pattern; no new telemetry sink.
- Compatibility notes (CLI flags, config schemas, versioning): new interface overloads are additive; Issue #169 settings/ribbon are reused unchanged.

## Test Strategy
Seeded from issue:

- [ ] Unit coverage: pre-UI scoring/filter selects only >= threshold items; below-threshold and no-suggestion items are excluded; predetermined folder choice is applied; mode disabled leaves standard flow unchanged.
- [ ] Integration scenario: high-confidence launch shows only above-threshold items with a folder pre-selected; standard launch unaffected.
- [ ] Manual verification notes: confirm UI never renders a below-threshold email during high-confidence launch.

- Regression tests to add or update:
  - `QuickFiler.Test` `QfcHomeController` tests: pre-filter delegate is invoked when mode enabled and not invoked when disabled; the carrier-list `LoadItemsAsync` overload is called in high-confidence mode; the plain `IList<MailItem>` overload is called in standard mode.
  - `QuickFiler.Test` `QfcFormController` tests: the pre-scored `LoadItemsAsync` overload constructs item groups without invoking the post-UI removal pass.
  - `QuickFiler.Test` `QfcCollectionController` tests: the pre-scored `LoadControlsAndHandlers_01Async` overload carries `PredeterminedFolder` into each item group; `AssignFolderComboBox` preselects the predetermined folder rather than index 1.
  - `UtilitiesCS.Test` `FolderScorer` tests: `TopScore()` behavior at boundary and empty-scorer; `TopFolderPath` accessor if added.
- Unit tests (MSTest) for the fixed behavior and boundaries:
  - Pre-filter selects only items with `TopScore() >= cutoff`.
  - Below-threshold and no-suggestion (`TopScore() == 0`) items excluded before UI load.
  - Surviving items carry the correct predetermined top-suggestion folder.
  - Boundary: an item whose top score equals the cutoff is retained (inclusive).
  - Mode disabled: no pre-pass; plain `IList<MailItem>` path used.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - All items below threshold -> empty surviving list passed to UI (and the existing zero-item behavior is exercised).
  - Mixed batch (some above, some below, some no-suggestion).
  - Exact-boundary item retained.
  - Null/empty candidate list guard.
- Error handling and logging verification:
  - Per-item scoring failure -> item excluded, no fatal error.
  - Cancellation token honored through the pre-filter chain.
- Coverage impact and targets for changed lines/modules:
  - New `QfcHighConfidencePreFilter` and changed methods target >= 90% coverage on changed lines; repository-wide coverage stays >= 80%.
- Toolchain commands to run (format -> lint -> type-check -> test):
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (if required):
  - Launch via the high-confidence ribbon entry point and confirm the window opens already showing only above-threshold items with the predetermined folder preselected and no transient render-then-remove. Launch via the standard entry point and confirm unchanged behavior.


## Acceptance Criteria
The authoritative, numbered acceptance criteria (AC1..AC8) are defined in `user-story.md` and are mirrored by the Definition of Done below. In summary:

- [x] Pre-UI scoring/filtering occurs before UI item controllers are constructed, verified via the seam (not post-hoc removal).
- [x] Below-threshold and no-suggestion emails are excluded from the candidate list before UI load.
- [x] Surviving items carry and preselect their predetermined top-suggestion folder.
- [x] Boundary: an email whose top score exactly equals the cutoff is retained (inclusive).
- [x] Mode disabled => standard flow unchanged; the standard entry point never pre-filters.
- [x] No unintended behavior changes outside the defined scope.
- [x] New/changed logic covered by MSTest + Moq + FluentAssertions.
- [x] Full toolchain pass completed (CSharpier -> .NET analyzers -> nullable analysis -> MSTest) with zero regressions.

## Definition of Done
- [x] AC1: Pre-UI scoring and filtering run in `QfcHomeController.RunAsync` between `InitEmailQueueAsync` and `LoadItemsAsync`, before any UI item controller is constructed. Verified via the injected pre-filter delegate seam, not via post-hoc removal. — Tests `RunAsync_HighConfidenceEnabled_InvokesPreFilterBeforeCarrierLoad`, `RunAsync_HighConfidence_PreFilterPrecedesUiConstruction`.
- [x] AC2: Below-threshold emails (`TopScore() < cutoff`) and no-suggestion emails (`TopScore() == 0`) are excluded from the candidate list before UI load. — Tests `FilterAsync_ExcludesItemsBelowCutoff`, `FilterAsync_ExcludesZeroScoreNoSuggestion`, `FilterAsync_AllBelowThreshold_ReturnsEmpty`.
- [x] AC3: Each surviving email carries its predetermined top-suggestion folder, and the item controller preselects that folder instead of selecting by index. — Tests `FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `CarrierLoad_SetsPredeterminedFolderOnItemGroup`, `AssignFolderComboBox_WithPredeterminedFolder_SelectsThatFolderNotIndexOne`.
- [x] AC4: An email whose top score exactly equals the cutoff (`(long)Math.Round(threshold * 1000, 0)`) is retained (inclusive boundary). — Test `FilterAsync_RetainsItemExactlyAtCutoff`.
- [x] AC5: With high-confidence mode disabled (default, standard entry point), no scoring/filter pre-pass runs and behavior is exactly unchanged. — Tests `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`, `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`; `evidence/regression/regression-171.2026-06-02T10-26.md`.
- [x] AC6: The UI never renders a below-threshold email; there is no transient render-then-remove in high-confidence mode. — Carrier path omits `ApplyHighConfidenceFilterAsync`; test `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval`.
- [x] AC7: Scoring/filter logic lives in the new `QfcHighConfidencePreFilter.cs`; scoring is reused (not duplicated) from the existing path; oversized controller files are not made materially worse; scoring runs off the UI thread and UI construction stays on the UI thread. — `evidence/qa/file-size-check-171.2026-06-02T10-26.md`; `FolderScoringService` reuses `FolderPredictor`/`FolderScorer`; `RunAsync` scores inside `Task.Run`.
- [x] AC8: New and changed logic is covered by MSTest + Moq + FluentAssertions; the full C# toolchain (CSharpier, .NET analyzers, nullable analysis, MSTest) passes with zero regressions. Evidence is written under `docs/features/active/2026-06-02-quickfiler-high-confidence-prefilter-171/evidence/qa/`. — `evidence/qa/qa-final-171.2026-06-02T10-26.md`, `evidence/coverage/coverage-comparison-171.2026-06-02T10-26.md` (new file 100%), `evidence/regression/regression-171.2026-06-02T10-26.md`.

## Risks & Mitigations
- Technical or operational risks:
  - Startup latency: pre-scoring blocks the window from showing until the batch is scored. Mitigation: score in parallel via `Task.WhenAll` off the UI thread, matching the existing `LoadSecondaryAsync` pattern; scope to the initial batch only.
  - Empty surviving list: if all items in the batch are below threshold, `LoadItemsAsync` receives an empty list. Mitigation: align with the existing zero-item behavior (`RemoveSpecificControlGroup` -> `ActionOkAsync` -> `MoveAndIterate`); cover with a test.
  - Lost predetermined selection: a later `AssignFolderComboBox()` call could reset the selection to index 1. Mitigation: carry `PredeterminedFolder` on `QfcItemGroup` so `AssignFolderComboBox` preselects it; cover with a test.
  - COM/STA threading during parallel scoring. Mitigation: use the same `Task.Run` pattern already in production in `LoadFolderHandlerAsync`.
  - Worsening oversized files. Mitigation: new logic goes in the new file; any inlined additions to existing files are minimal.
- Mitigations and rollbacks:
  - The `HighConfidenceModeEnabled` flag (default `false`) keeps the feature inert; the standard entry point is the fallback path.

## Rollout & Follow-up
- Release/rollout steps:
  - Merge after the full toolchain passes with zero regressions. The feature is gated by the existing high-confidence ribbon entry point and the default-off setting.
- Post-fix monitoring or clean-up tasks:
  - Consider extending the pre-filter to later background batches (`IterateQueueAsync` / `QfcQueue`) in a follow-up; out of scope here.
  - Consider removing the now-unused post-UI removal seam (`ApplyHighConfidenceFilterAsync` / `RemoveBelowThresholdAsync`) once confidence in the pre-filter is established; retained for now for backward compatibility and existing test coverage.
- Links: issue #171 (https://github.com/drmoisan/TaskMaster/issues/171); research `artifacts/research/quickfiler-high-confidence-prefilter-171.2026-06-02T13-45.md`; superseded design Issue #169 (`docs/features/active/quickfiler-high-confidence-filter-169/`).
