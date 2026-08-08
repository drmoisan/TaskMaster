# 2026-08-07-quickfiler-search-keystroke-focus-steal (Spec)

- **Issue:** #438
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T11-00
- **Status:** Ready for planning
- **Version:** 1.0

## Context

Typing into the QuickFiler folder-search textbox (`TxtboxSearch`) loses keyboard focus after roughly one to two characters. Each `TextChanged` event opens the breadcrumb folder drop-down, and opening the drop-down moves focus to the popup surface, so the remainder of the typed search string is not delivered to the textbox and the view jumps to the folder that was selected in the partially-typed result set.

Research (`research/2026-08-08T10-30-quickfiler-search-keystroke-focus-steal-research.md`) confirmed the issue's code-read and identified a second, independent focus-steal mechanism on the close side of the pipeline, plus a related committed-selection defect in the same handler. All three are in scope for #438.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI path: QuickFiler `ItemViewer` folder-search textbox (`TxtboxSearch`) and the WebView2 breadcrumb folder selector
- Data source or fixture: `FolderPredictor` / `IFolderSearchHandler.FindFolder` results for a wildcard search string

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Folder search is the primary way to file an item to a folder that is not among the suggestions. Because focus is lost after one to two characters, the search textbox is effectively unusable for any multi-character query, and the user must retype or click back into the textbox repeatedly.

## Repro & Evidence

Steps to Reproduce:
1. Open QuickFiler on an item so an `ItemViewer` folder-search textbox is available.
2. Place focus in the folder-search textbox.
3. Type a multi-character folder search string at normal typing speed (for example `invoices`).
4. Observe where the caret and keyboard focus are after the first one to two characters, and observe which folder the selector shows.

Expected:
- Typing in the search textbox opens (or keeps open) the folder drop-down and refreshes its filtered contents on every keystroke.
- Keyboard focus and the caret stay in the search textbox for the whole time the user is typing. No keystroke is redirected to the drop-down surface.
- The drop-down updates its rows and its highlighted row without taking focus and without committing a folder selection.
- Focus moves to the drop-down only on an explicit user gesture: Down arrow from the textbox (existing `TextBoxSearch_KeyDown` contract), a click on the drop-down arrow, or a click on a row.

Actual:
- After approximately one to two characters, keyboard focus leaves the search textbox and the selector surface receives focus, so subsequent characters are not appended to the search text.
- The selector highlights and shows the folder that matched the truncated search string, which reads as the view "jumping to the open folder" mid-typing.

Logs / Screenshots:
- [x] Code-read evidence with `file:line` citations is recorded in the research artifact; no runtime log capture is required.

## Scope & Non-Goals

- In scope:
  - Removing both focus-steal mechanisms from the search-text path: the open-side focus transfer (`_focusPending` via `FocusCurrentSurface` and the re-issued-open `Schedule(_focusPending)`) and the close-side focus transfer (`_focusAnchor` via `FinishClose`, triggered by the per-keystroke `ClearFolderItems()` session cancel).
  - Removing the per-keystroke committed-selection mutation: `SetFolderSelectedIndex(1)` on the search path currently changes the committed model selection, raises `SelectionChanged`, and leaves a stale controller-cached `_selectedFolder` after Escape.
  - One additive `IItemViewer` presentation member for the search path (research proposes `PresentFolderSearchResults(string[])`).
  - One additive `IBreadcrumbDropDownHost.OpenAsync` overload carrying an explicit `takeFocus` intent; the existing 3-parameter overload defaults to `takeFocus: true`.
  - A session-preserving row-replacement transition so a search refresh does not close and reopen the popup.
  - A pending-only highlight transition that never commits a selection while the search session is open.
  - MSTest regression coverage at the controller, coordinator, host, and router/session seams.
- Out of scope / non-goals:
  - The EfcViewer search path (`EfcFormController.SearchText_TextChanged`). Research §7 confirmed it is not the same defect: it re-renders a persistently visible WebView2 with no popup, no open intent, and no managed focus call in the path. It must not be modified under #438. If focus loss is ever reproduced there, it is a separate defect to be promoted as its own issue.
  - The suggestions path `AssignFolderComboBox` and its `SetFolderSelectedIndex(1)` call (`QfcItemController.FolderHandling.cs:202`), which is unchanged.
  - Any behavior change to explicit-gesture opens: `TextBoxSearch_KeyDown` (Down arrow), `JumpToFolderDropDown`/`Async`, and the mouse toggle keep their exact current semantics, including focus-on-open.
  - Any rework of the #400 open/commit/cancel/placement/lifecycle contract beyond the single, explicitly sanctioned qualification of #400 AC-13 recorded below.
- Explicitly excluded systems, integrations, or datasets:
  - Outlook folder scoring and predictor algorithms; `IFolderSearchHandler.FindFolder` result production is consumed as-is.
  - Non-QuickFiler controls and unrelated `ItemViewer` interactions.

## Root Cause Analysis

The defect is a composition of three verified behaviors (research §1–§2, all `[VERIFIED]` with `file:line` citations):

1. **Per-keystroke handler composition.** `QfcItemController.TextBoxSearch_TextChanged` (`QuickFiler/Controllers/QfcItemController.EventHandlers.cs:164-178`) runs on every keystroke and unconditionally issues `ClearFolderItems()`, `SetFolderItems(folders)`, `SetFolderSelectedIndex(1)` (when two or more rows matched), and `SetFolderDroppedDown(true)`. No branch leaves focus with the sender.
2. **Open-side focus steal.** In the production configuration, the open pipeline focuses the popup by design: a fresh open ends in `BreadcrumbDropDownOpenLifetime.FocusCurrentSurface` → `_host.FocusPending()` (`BreadcrumbDropDownOpenLifetime.cs:287-305`), and a re-issued open on an already-open popup executes `_openLifetime.Schedule(_focusPending)` (`BreadcrumbDropDownHost.cs:228-242`). The bare-viewer and no-open-coordinator fallback branches focus directly (`ItemViewer.Breadcrumb.cs:223-235`; `BreadcrumbItemViewerLifecycleCoordinator.cs:176-189`).
3. **Close-side focus steal (not recorded in the original issue).** On every keystroke after the drop-down is open, the leading `ClearFolderItems()` cancels the open selector session (`BreadcrumbSelectionSession.ClearSelector`), which drives `CloseCore` → `BreadcrumbDropDownHost.FinishClose` (`BreadcrumbDropDownHost.cs:385-399, 427-437`), and `FinishClose` always invokes `_focusAnchor` — focusing the collapsed breadcrumb surface. A fix that suppresses only the open-side focus still loses focus through this path, because the search path closes and reopens the session on every keystroke.

Related defect in the same handler (research §2): the per-keystroke `SetFolderSelectedIndex(1)` mutates the **committed** model selection — what the collapsed surface renders and what `GetSelectedFolder()` returns — and always raises `SelectionChanged`, so `CboFolders_SelectedIndexChanged` caches a mid-search `_selectedFolder` on every keystroke. Because `CancelSelector` raises no `SelectionChanged`, an Escape/uncommitted close restores the model but leaves the controller's cached `_selectedFolder` holding the partial-search row-1 value.

Net behavior: the first keystroke opens the popup and focuses it (matching "after roughly one to two characters"); each subsequent keystroke while open closes the popup (focus → anchor) and reopens it (focus → popup), with visible popup churn. The fix must therefore (a) stop the per-keystroke close/reopen cycle, (b) make the search-driven open non-focusing, and (c) make the search highlight pending-only.

## Proposed Fix

The approved design is research §3 **Option 3** (accepted by the orchestrator; the option choice is settled). The paragraphs below state the specified behavior; the planner owns sequencing and may refine file-level placement provided every contract below is preserved.

### Design summary (what changes where):

- **One controller-facing presentation intent.** `TextBoxSearch_TextChanged` reduces to `FindFolder` plus a single call to a new additive `IItemViewer` member (research proposes `void PresentFolderSearchResults(string[] items)`), replacing the `ClearFolderItems` + `SetFolderItems` + `SetFolderSelectedIndex` + `SetFolderDroppedDown` composition. The open/highlight sequencing moves into the coordinator layer that owns the posted-operation queue.
- **Session-preserving row replacement.** The coordinator composite performs, in order: (a) a new router transition that replaces plain rows while preserving the open selector session (reusing the existing `ReconcileRowsReplaced` reconciliation; the existing unreachable `SetItems` is not reused as-is); (b) `OpenSelector()` only if the session is closed; (c) a pending-only highlight of the first selectable row. A refresh while open replaces rows without any native close/reopen.
- **Pending-only highlight.** A new session transition (research proposes `HighlightRow`) requires an open session, sets only `PendingIdentity`, and emits `Handled | RenderRequired` — no `SelectionChanged`, no `OpenStateChanged`, no committed-model mutation. Ordering (a)→(b)→(c) guarantees the highlight applies to an open session, so it can never commit; Escape restores the pre-search committed identity through the existing `Cancel` semantics.
- **Explicit `takeFocus` intent through the open pipeline.** `BreadcrumbDropDownOpenCoordinator` latches "next native open takes no focus" for search-originated opens (deterministic via the FIFO `BreadcrumbPopupUiOperations` queue). The flag crosses the host boundary as an additive `IBreadcrumbDropDownHost.OpenAsync(anchor, workingArea, size, bool takeFocus)` overload. `BreadcrumbDropDownHost.OpenAsync` skips `Schedule(_focusPending)` in the already-open branch when `takeFocus` is false and passes the flag to `BreadcrumbDropDownOpenLifetime`, which makes the `_host.FocusPending()` call inside `FocusCurrentSurface` conditional. The open-result contract and the `LastInitializationException = null` step are unchanged.
- **Fallback branches.** The non-focusing variant of `SetBreadcrumbDropDownState` skips `FocusBreadcrumb()` / `Focus(focus)` in the bare-viewer and no-open-coordinator branches.

### Boundaries and invariants to preserve:

- Contract changes are **additive only**: exactly one new `IItemViewer` member and exactly one new `IBreadcrumbDropDownHost.OpenAsync` overload. The existing 3-parameter `OpenAsync` delegates with `takeFocus: true`, so every existing caller keeps its exact semantics. No existing signature is removed or altered.
- Explicitly unchanged behavior: `TextBoxSearch_KeyDown` (Down arrow) keeps issuing both `SetFolderDroppedDown(true)` and `FocusFolderDropDown()`; `JumpToFolderDropDown`/`Async` unchanged; mouse toggle unchanged; the suggestions path `AssignFolderComboBox`'s `SetFolderSelectedIndex(1)` unchanged and out of scope.
- The #400 acceptance criteria remain in force per the reconciliation below, with one sanctioned qualification (AC-13).
- One render per surface per state update and one transition per inbound event (#400 AC-12) apply to the new row-replacement transition.
- Commit, cancel, focus-return-on-close, placement, lazy creation, reuse, and disposal semantics from #400 are untouched.

### Issue #400 reconciliation (required):

Research §4 mapped every constraining #400 acceptance criterion. All are preserved except AC-13, which receives a deliberate, documented, gesture-scoped qualification.

| #400 AC | Constraint | Effect under #438 |
|---|---|---|
| AC-3 | Button activation and `SetFolderDroppedDown(true)` open the native popup | Preserved. `SetFolderDroppedDown(bool)` is unchanged (signature pinned by `ItemViewerBreadcrumbDropDownContractTests.cs:77-100`); the search path uses a new additive intent. |
| AC-5 | Closed Up/Down commit immediately | Preserved. Session `Move` semantics untouched. |
| AC-6 | Open snapshots `original`; open Up/Down change only `pending` | Preserved and reused. The search highlight uses the same pending-only mechanism, so it composes with open-arrow navigation. |
| AC-7 | Enter/mouse activation commit once, close, return focus | Preserved. Commit paths untouched; focus-return-on-close (`FinishClose`) untouched. |
| AC-8 | Escape/uncommitted close restores the opening identity, returns focus | Preserved. The search path stops triggering uncommitted closes per keystroke; the close behavior itself is unchanged. With the fix, Escape during search restores the identity committed before the search session opened. |
| AC-9 | Left/Right breadcrumb behavior | Preserved. No arrow routing touched. |
| AC-12 | One render per surface per state update; one transition per inbound event | Preserved and pinned for the new transition: the session-preserving row replacement must emit exactly one render per surface per keystroke (the current Clear+AddItems emits at least two). |
| AC-13 | "Focus enters the pending option on open" | **Sanctioned, gesture-scoped qualification.** Search-driven opens are non-focusing; explicit-gesture opens (mouse toggle, Down arrow, `JumpToFolderDropDown`) keep focus-on-open because plain `SetFolderDroppedDown(true)` and the 3-parameter `OpenAsync` default to `takeFocus: true`. Rationale: an open initiated as a side effect of typing must not move the caret away from the textbox the user is typing in; #400 AC-13 was authored against explicit open gestures, and this refinement scopes it to those gestures. This is a deliberate, documented refinement of the #400 contract, not a regression and not a silent deviation. |
| AC-14 | Lazy popup creation, reuse, disposal | Preserved and improved: the popup is no longer closed and recreated-shown per keystroke. |
| AC-15 | Deterministic edge behavior, no throw/leak | Preserved. Empty result sets and single-row (banner-only) results remain deterministic no-ops for the highlight step; `OpenSelector` already refuses to open with no selectable rows. |

### Dependencies or blocked work:

- No new package, service, runtime, persisted setting, or migration. Existing .NET Framework 4.8.1 WinForms + WebView2 only.
- The research artifact is sufficient; there is no research blocker. A verification runbook is being authored concurrently at `runbooks/verify-search-focus-retention.runbook.md` (referenced under Rollout & Follow-up).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

Estimated 12 production files (research §3 Option 3): `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`; `QuickFiler/Viewers/IItemViewer.cs`; `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`; `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`; `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`; `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`; `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs`; `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`; `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs`; `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`; `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`; `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`. Test projects `QuickFiler.Test` and `UtilitiesCS.Test` are legacy non-SDK projects; every new `.cs` file requires an explicit `<Compile Include>` entry.

#### Functions/classes/CLI commands impacted:

- `QfcItemController.TextBoxSearch_TextChanged`: reduced to `FindFolder` + the single presentation intent.
- `IItemViewer`: one additive member (research proposes `PresentFolderSearchResults(string[])`).
- `BreadcrumbBridgeCoordinator`: one composite member implementing replace → open-if-closed → highlight.
- `FolderBreadcrumbBridgeRouter`: one new session-preserving plain-row replacement transition (`RenderRequired` only, no `OpenStateChanged`).
- `BreadcrumbSelectionSession`: one new pending-only transition (`HighlightRow(index)`: requires `IsOpen`; sets `PendingIdentity` to the first selectable row at/after the index; effects `Handled | RenderRequired`).
- `IBreadcrumbDropDownHost` / `BreadcrumbDropDownHost` / `BreadcrumbDropDownOpenLifetime` / `BreadcrumbDropDownOpenCoordinator`: additive `takeFocus` intent as described above.
- `ItemViewer.Breadcrumb` / `BreadcrumbItemViewerLifecycleCoordinator`: non-focusing fallback branches.

#### Data flow and validation changes:

Per keystroke (fixed behavior): closed → `[replace rows] → [open session] → [pending = first selectable row] → [native open, no focus]`; open → `[replace rows, session preserved] → [pending = first selectable row] → no native churn`. Explicit gestures unchanged: Down/toggle → focusing open; Enter/click → commit + close + focus return; Escape/outside → cancel + close + focus return. Router mutations execute synchronously in keystroke order; native-open work executes through the posted FIFO queue. An already-open popup keeps its height until reopened when a later keystroke changes the row count (accepted; note for the plan).

#### Error handling and logging updates:

- No new logging surface. Existing bridge parse-failure and popup initialization-failure logging boundaries are unchanged.
- The new session and router transitions are deterministic no-ops for empty and single-row (banner-only) result sets and must not throw.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag and no persisted state. Reverting the change restores the prior handler composition; all contract additions are additive, so revert requires no caller migration.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- New `IItemViewer` member: accepts the `FindFolder` result array (folder path strings) and performs presentation only; no return value.
- New `IBreadcrumbDropDownHost.OpenAsync(anchor, workingArea, size, bool takeFocus)` overload: same open-result contract as the 3-parameter overload; `takeFocus: false` suppresses both the fresh-open `FocusPending` step and the already-open `Schedule(_focusPending)` step.

#### Required configuration keys and defaults:

- None. `takeFocus` defaults to `true` via the delegating 3-parameter overload; `false` is used only by search-originated opens.

#### Backward-compatibility expectations:

- All existing `IItemViewer` and `IBreadcrumbDropDownHost` signatures are preserved. All in-repo implementations are updated; Moq interface mocks regenerate automatically.
- All existing gesture-path, integration, contract, lifecycle, and router/session tests remain green unmodified, with the single justified exception listed in the Test Strategy.

#### Performance constraints (latency/throughput/memory):

- Row replacement and highlight are synchronous host-neutral operations with no Outlook, network, or filesystem calls.
- The per-keystroke native popup close/reopen cycle is removed; no new allocation per keystroke beyond the replacement row set.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): the FIFO `BreadcrumbPopupUiOperations` queue provides deterministic ordering for the latched focus intent; `ReconcileRowsReplaced` reconciles committed/original/pending identities across an atomic row swap, as it does for the suggestions path.
- Constraints (budget, performance, compatibility): .NET Framework 4.8.1 — no `init` setters, no `record`, no `record struct`; all new and modified production and test files remain under 500 lines (`BreadcrumbDropDownOpenLifetime.cs` is at 477 lines and needs watching); MSTest + Moq + FluentAssertions; temporary files in tests are prohibited.
- External dependencies (services, libraries, releases): none beyond existing references.

## Data / API / Config Impact

- User-facing or API changes: typing in the folder-search textbox now retains caret and keyboard focus; the drop-down stays open and tracks each keystroke; the collapsed surface no longer jumps to a mid-search folder. Two additive interface changes as specified; no signature removed or altered.
- Data or migration considerations: none. No persisted state.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy

Test seams (all existing patterns; research §3/§5):
- `Mock<IItemViewer>` + `Mock<IFolderSearchHandler>` at the controller (`QfcItemController.EventHandlersTests.cs:314-350` arrangement).
- Headless `ItemViewer` + `Mock<IBreadcrumbDropDownHost>` integration harness (`ItemViewerDropDownHarness`, `BreadcrumbDropDownIntegrationTests.cs`).
- Real-host delegate-count harness (`BreadcrumbDropDownHostTests.cs:394-443`, `FocusPendingCount`/`FocusAnchorCount`).
- Host-neutral router/session tests in `UtilitiesCS.Test/OutlookObjects/Folder/`.

Existing tests that are part of the spec and must keep passing unmodified: the Down-arrow and non-Down `TextBoxSearch_KeyDown` tests (`QfcItemController.EventHandlersTests.cs:355-388`); `JumpToFolderDropDown` navigation and async-dispatcher tests; the suggestions-path tests (`QfcItemController.FolderSuggestionsTests.cs`, `QfcItemController.FolderHandlingTests.cs`); all `BreadcrumbDropDownIntegrationTests` cases; the default-open `FocusPendingCount` host tests; `BreadcrumbSelectorOpenRetryTests`; `ItemViewerBreadcrumbDropDownContractTests`; the open-coordinator/lifecycle/hub/placement suites; and the existing `UtilitiesCS.Test` session/router/map suites.

One existing test encodes the defective behavior and must change (research §5.2): `QfcItemController.EventHandlersTests.cs:313-350` (`TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder`) pins the exact defective sequence. Its durable protections (wildcard `FindFolder` query from `SearchText`, drop-down population, row-1 highlight, drop-down shown) are re-asserted against the new presentation intent, with added negative assertions (`SetFolderDroppedDown` never, `FocusFolderDropDown` never, `SetFolderSelectedIndex` never). No other existing test pins the search-path composition.

- Regression tests to add or update (research §6):
  - Controller seam (primary, fails before / passes after): `TextBoxSearch_TextChanged` issues the presentation intent exactly once and never issues `SetFolderDroppedDown`, `FocusFolderDropDown`, or `SetFolderSelectedIndex`.
  - Host/pipeline seam: a search-driven open invokes `FocusPending` zero times and `FocusAnchor` zero times; a default open still invokes `FocusPending` exactly once.
  - Open-coordinator seam: two consecutive search refreshes produce exactly one `OpenAsync` and zero `Close` calls on the host (fails before: the second refresh closes and reopens).
  - Session/router seam: the highlight transition on an open session changes only `PendingIdentity` and publishes no `SelectionChanged`; `Cancel` restores the pre-search committed identity; row replacement while open preserves the session via `ReconcileRowsReplaced`; the replacement transition emits exactly one render per surface.
  - Controller cache: after Escape-cancel, the controller's cached `_selectedFolder` does not retain a mid-search highlight.
  - Integration seam: a multi-character search string typed through the viewer seam delivers the full string to `SearchText` and the row set reflects the complete query.
- Unit tests (MSTest) for the fixed behavior and boundaries: MSTest + Moq + FluentAssertions; Arrange–Act–Assert; no network, external process, temporary file, wall-clock sleep, or user interaction.
- Edge cases and negative scenarios: empty result set; single-row (banner-only) result set; refresh while an open is in flight; Escape during an active search session; keystroke ordering across the posted queue.
- Error handling and logging verification: the new transitions are no-ops (no throw, no state corruption) for empty/single-row inputs; initialization-failure behavior is unchanged and remains covered by the existing #400 suites.
- Coverage impact and targets for changed lines/modules: new/changed members target >= 90%; changed-line coverage does not regress; router/session/coordinator changes are host-neutral and measurable; `ItemViewer` partials remain `[ExcludeFromCodeCoverage]` thin forwarding per the ratified exemption; the controller handler is covered via the seam tests. Baseline and post-change repository figures are recorded in `evidence/` per AC-12.
- Toolchain commands to run (format → lint → type-check → test), restarted from formatting after any failure or file change:
  1. `dotnet tool run csharpier .` (or `csharpier .`)
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
- Manual validation steps (if required): one documented post-fix human verification step exists and is not a merge gate; see "Automation feasibility and residual human verification" below.

## Acceptance Criteria

- [x] AC-1: At the controller seam (`Mock<IFolderSearchHandler>` + `Mock<IItemViewer>`, arrangement of `QfcItemController.EventHandlersTests.cs:314-343`), `TextBoxSearch_TextChanged` issues the new presentation intent exactly once per invocation and issues no focus-transfer intent and no committed-selection change: `SetFolderDroppedDown(It.IsAny<bool>())` never, `FocusFolderDropDown()` never, `SetFolderSelectedIndex(It.IsAny<int>())` never. This regression fails before the fix (the handler calls `SetFolderDroppedDown(true)` at `EventHandlers.cs:177`) and passes after.
- [x] AC-2: In the real-host delegate-count harness (`FocusPendingCount`/`FocusAnchorCount`), a search-driven open invokes the focus-pending delegate zero times and the focus-anchor delegate zero times, while a default (gesture) open still invokes the focus-pending delegate exactly once.
- [x] AC-3: Two consecutive search refreshes produce exactly one host `OpenAsync` call and zero host `Close` calls (fails before the fix: the second refresh closes and reopens the popup).
- [x] AC-4: The search highlight changes only the session's `PendingIdentity`, publishes no `SelectionChanged` event, and leaves the committed model selection (collapsed surface and `GetSelectedFolder()`) untouched while the search session is open.
- [x] AC-5: Escape (uncommitted close) during an active search session restores the identity committed before the search session opened, and the controller's cached `_selectedFolder` does not retain a mid-search highlight value after the cancel.
- [x] AC-6: A multi-character search string typed through the viewer seam delivers the complete string to `SearchText`, and the presented row set reflects the complete query (no truncation at one to two characters).
- [x] AC-7: Explicit-gesture behavior is unchanged and pinned: `TextBoxSearch_KeyDown` (Down arrow) still issues both `SetFolderDroppedDown(true)` and `FocusFolderDropDown()`; `JumpToFolderDropDown`/`Async` and the mouse toggle keep their current semantics; the existing tests at `QfcItemController.EventHandlersTests.cs:355-388`, `QfcItemController.NavigationTests.cs:159-181`, `QfcItemController.SeamDispatcherTests.cs:94-95`, and `BreadcrumbSelectorOpenRetryTests.cs:55-76` pass unmodified.
- [x] AC-8: The new session-preserving row-replacement transition emits exactly one render per surface per state update, preserving #400 AC-12 for the search-refresh path.
- [x] AC-9: Empty result sets and single-row (banner-only) result sets are deterministic no-ops for the highlight step: no throw, no selection mutation, no open of a selector with no selectable rows.
- [x] AC-10: Contract changes are additive only: exactly one new `IItemViewer` member and exactly one new `IBreadcrumbDropDownHost.OpenAsync(anchor, workingArea, size, bool takeFocus)` overload, with the existing 3-parameter overload delegating with `takeFocus: true`; no existing signature is removed or altered; `ItemViewerBreadcrumbDropDownContractTests` passes unmodified.
- [x] AC-11: The #400 reconciliation holds as specified: all mapped #400 acceptance criteria are preserved, the gesture-scoped qualification of #400 AC-13 is recorded in this spec, and all existing #400 suites (`BreadcrumbDropDownIntegrationTests`, `BreadcrumbDropDownHostTests` default-open cases, open-coordinator/lifecycle/session/router suites) pass, with the `TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder` method (`QfcItemController.EventHandlersTests.cs:313-350`) as the single justified **test-method** modification. Sanctioned structural, non-test edits to existing test files are limited to: purely additive 4-parameter `OpenAsync` members on the private test fakes `ControlledHost` (`BreadcrumbDropDownOpenCoordinatorTests.cs`), `RecordingHost` (`BreadcrumbItemViewerLifecycleCoordinatorTests.cs`), and `RecordingDropDownHost` (`BreadcrumbSelectorOpenRetryTests.cs`), plus one-token `partial` keywords on test-class declarations required for partial-file extensions; these edits must add, remove, weaken, or alter no test method, and no other test method may be added, removed, weakened, or altered beyond the one sanctioned rewrite.
- [x] AC-12: One final uninterrupted toolchain pass succeeds in order (csharpier → analyzer msbuild → nullable warnings-as-errors msbuild → coverage-enabled vstest for `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`). Every measurable new or changed member reaches >= 90% coverage; changed-line coverage does not regress. Baseline and post-change repository-wide coverage figures (testable denominator per CLAUDE.md § UT2) are recorded under `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/`, and the post-change figure is not lower than the captured baseline.
- [x] AC-13: The EfcViewer search path is unmodified: no diff under `EfcFormController.SearchText_TextChanged`, `BindFolderRows`/`BindBreadcrumbRowsAsync`, or `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is delivered under #438.
- [x] AC-14: Every added production and test `.cs` file has an explicit `<Compile Include>` entry in the applicable legacy `.csproj`; no new or modified production or test source file exceeds 500 lines; no new external package or persisted configuration is introduced.

### Automation feasibility and residual human verification

AC-1 through AC-14 are discharged entirely by automated MSTest evidence at the seams named in the Test Strategy; they constitute the merge gate. Every focus transfer in the defective pipeline is a managed delegate invocation injected through constructor seams, so all of the above are deterministically assertable (research §8).

Exactly one residual criterion is not automatable — the issue's manual note: *"type an eight-character folder name at normal speed and confirm the caret never leaves the textbox."* Research §8 established this is observable only in a live Outlook session, because (a) CoreWebView2 popup-surface creation may grab Win32 focus independently of managed code, and (b) `ToolStripDropDown.AutoClose` behavior while the user keeps typing is not unit-observable.

- [ ] HV-1 (documented human-verification exception — **not a merge gate**): post-fix, in a live Outlook session, type an eight-character folder name at normal speed into `TxtboxSearch` and confirm the caret never leaves the textbox while the drop-down contents track each keystroke. Execute per the runbook at `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/runbooks/verify-search-focus-retention.runbook.md`. This mirrors the #400 precedent (spec lines 60, 84, 235: user-operated validation is not required delivery evidence). The automated seam-level assertions (AC-1 through AC-14) are the merge gate; HV-1 is a post-fix verification step whose outcome, if negative, is promoted as a new issue rather than blocking #438 delivery.

## Risks & Mitigations

- Technical or operational risks:
  - `ToolStripDropDown.AutoClose = true` is expected to keep the non-activated popup open while the user types in the same form; this is inferred WinForms behavior, not provable in a unit test. If runtime behavior differs, the popup would auto-close per keystroke via `OnDropDownClosed`.
  - CoreWebView2 popup-surface creation may move Win32 focus on the first search-driven open independently of managed code (unverified).
  - Keystroke ordering: a later keystroke's replacement supersedes an in-flight open's row count; an already-open popup keeps its height until reopened.
  - `BreadcrumbDropDownOpenLifetime.cs` is at 477 lines; the conditional focus step risks crossing the 500-line limit.
- Mitigations and rollbacks:
  - Both native-behavior risks are exactly what HV-1 observes; the runbook covers them, and a negative outcome is promoted as its own issue.
  - The FIFO posted-operation queue guarantees deterministic latch ordering; the height note is recorded for the plan as accepted behavior.
  - If the lifetime file approaches the limit, the planner extracts a cohesive type rather than exceeding it.
  - Rollback is a plain revert; all contract changes are additive with no persisted state.

## Rollout & Follow-up

- Release/rollout steps: deliver through the normal TaskMaster build path after AC-1 through AC-14, feature review, and CI gates pass. No migration, flag, or manual bootstrap.
- Post-fix monitoring or clean-up tasks: execute HV-1 per the runbook at `runbooks/verify-search-focus-retention.runbook.md`; if focus loss is reproduced in the EfcViewer search box at any point, promote it as a separate issue (research §7) rather than reopening #438.
- Links: issue #438 (https://github.com/drmoisan/TaskMaster/issues/438); constraining prior work issue #400 (`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`); research artifact `research/2026-08-08T10-30-quickfiler-search-keystroke-focus-steal-research.md`; implementation PR and audit links to be recorded by the orchestration workflow.
