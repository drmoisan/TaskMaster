# quickfiler-keyboard-hook-leaks-to-outlook (Spec)

- **Issue:** #677
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-28
- **Status:** Draft
- **Version:** 0.3

## Context
Running QuickFiler causes keyboard input to native Outlook windows (Explorer/Inspector) to stop working after the user clicks out of the QuickFiler window; QuickFiler's own keyboard navigation continues to work correctly while its window has focus.

Environment:
- OS/version: Windows (VSTO add-in host, Outlook desktop)
- Component: QuickFiler (WinForms UserControl/Form hosted inside the Outlook VSTO process)
- Data source or fixture: N/A (manual interactive repro)

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Launch Outlook with the TaskMaster VSTO add-in loaded.
2. Run QuickFiler against a mail item so its filing window opens and keyboard navigation is active.
3. Click out of the QuickFiler window into a native Outlook window (Explorer list, an open Inspector, the search box, etc.) without closing QuickFiler.
4. Attempt to type characters in the native Outlook window.

Expected:
Keyboard input scoping should be limited to the QuickFiler window. Once focus moves to a native Outlook window, that window should receive keystrokes normally, exactly as if QuickFiler were not running.

Actual:
Keystrokes typed into native Outlook windows are blocked/suppressed while QuickFiler is open, even though focus is no longer on the QuickFiler window. Keyboard navigation inside QuickFiler itself works correctly. Keyboard input outside the Outlook process (other applications) is unaffected.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: N/A — behavioral repro, no exception/log signature identified yet.


## Scope & Non-Goals
- In scope:
  - The two-part fix described under Proposed Fix:
    1. An activation-guarded focus-restoration predicate on `BreadcrumbDropDownHost` (injectable `Func<bool>` "may take focus", evaluated at execution time) gating the `_focusAnchor` call in `FinishClose` and the late-arriving `_focusPending` completion.
    2. A `Form.Deactivate`-driven focus-parking and selector-cancel handler routed through `QfcFormViewer`/`QfcFormController` (Seam-B style) that moves focus off any focused WebView2 control and cancels an open breadcrumb selector via `BreadcrumbCoordinator.CancelSelector()`.
  - Unit tests for both parts through the existing injectable seams, plus manual live-Outlook verification of the end-to-end behavior.
- Out of scope / non-goals:
  - No changes to `KeyboardHandler` — its wiring is already correctly scoped to the QuickFiler control tree (see Root Cause Analysis).
  - No adoption of WebView2 controller-level focus APIs (`CoreWebView2Controller.MoveFocus` or related); no such surface exists in the repository today and introducing it is a larger change than the fix requires.
  - No rewrite of the breadcrumb popup off `ToolStripDropDown`.
  - No upstream fix of WebView2Feedback #951; this change works around the runtime defect within QuickFiler.
- Explicitly excluded systems, integrations, or datasets:
  - Any QuickFiler subsystem that does not touch focus or activation (item loading, filing/move actions, search and filtering logic, breadcrumb data pipeline, UI layout, dark/expanded viewer variants beyond the shared deactivate wiring).
  - All non-QuickFiler projects and the `QuickFiler/Legacy/` tree (dead code for this bug).

## Root Cause Analysis

Full findings: `research/2026-08-28T09-15-quickfiler-outlook-keyboard-suppression-677-research.md`.

User hypothesis (QuickFiler's `KeyboardHandler` is hooked/scoped to the whole Outlook process) is **refuted in its literal form, confirmed in spirit**. `KeyboardHandler` (`QuickFiler/Controllers/KeyboardHandler.cs`) is ordinary WinForms `PreviewKeyDown`/`KeyDown` wiring confined strictly to QuickFiler's own control tree (`QfcFormController.SetupDisposal.cs:149-175`, `QfcItemController.EventWiring.cs:40-91`), is instantiated per QuickFiler launch, and is never static/shared (`QfcHomeController.cs:93,181-186,350-359`). It cannot receive events from native Outlook windows.

The actual mechanism is focus routing, not key interception:

1. **Primary root cause.** Every visible `ItemViewer` row hosts two WebView2 instances on Outlook's shared UI thread (`ItemViewer.Designer.cs:46,49`), plus a third lazily-created popup WebView2 for the breadcrumb selector (`BreadcrumbPopupUiOperations.cs:376`). QuickFiler's breadcrumb pipeline routinely places Win32 keyboard focus on these WebView2 surfaces during normal keyboard filing (`ItemViewer.Breadcrumb.cs:252-262`, `QfcItemController.Navigation.cs:33-46`). Critically, `BreadcrumbDropDownHost.FinishClose` unconditionally re-focuses the anchor WebView2 on every close, including the `OnDropDownClosed` event fired when the user clicks out of QuickFiler into Outlook (`BreadcrumbDropDownHost.cs:397-420`); this re-focus runs asynchronously, landing after the user's click into Outlook has already occurred. Once a WebView2 holds thread-wide keyboard focus in a VSTO WinForms host, the WebView2 runtime is known not to reliably release it back to the host on click-back (MicrosoftEdge/WebView2Feedback issue #951, open). Because Outlook and QuickFiler share one UI thread/input queue, every keystroke typed "into" Outlook is instead delivered to the WebView2 until QuickFiler is closed (which disposes the WebView2s and releases focus) — matching every observed symptom (mouse works, keyboard doesn't; other processes unaffected; recovery on close).
2. **Secondary contributor (only while the breadcrumb popup is open).** `BreadcrumbDropDownHost` shows a `ToolStripDropDown` with `AutoClose = true` (`BreadcrumbDropDownHost.cs:165-170`). While open in a hosted (non-`Application.MessageLoop`) process, WinForms modal menu mode redirects thread keyboard messages to the dropdown — a real Outlook-thread-wide keyboard redirection that QuickFiler indirectly creates, invisible to hook-API greps because the .NET Framework installs it, not first-party code. This is transient (dismissed by the click-out itself) and is asserted from framework background knowledge, not verified live this session; it is addressed by ensuring the popup cannot outlive form deactivation.

Rejected mechanisms (all with negative evidence in the research artifact): global keyboard hooks, `Application.AddMessageFilter`, `Form.KeyPreview`, Win32 window disabling/`ShowDialog` ownership, the dead `QuickFiler/Legacy/` accelerator tree, timer/Idle-driven activation stealing.

## Proposed Fix

### Design summary (what changes where):
Activation-guarded focus restoration plus deactivate-time focus parking. No changes to `KeyboardHandler` — it is already correctly scoped.

1. Guard `BreadcrumbDropDownHost.FinishClose`'s unconditional `_focusAnchor` call (and the late-arriving `_focusPending` completion) behind an injectable `Func<bool>` "may take focus" predicate, evaluated at execution time, so a close/open that completes after the user has already clicked out of QuickFiler does not steal focus back into a WebView2.
2. Add a `Form.Deactivate`-driven handler (routed through `QfcFormViewer`/`QfcFormController` the same way existing Seam B events are routed) that, when the currently focused control is a WebView2, parks focus on a benign non-WebView2 control and closes any open breadcrumb selector (`BreadcrumbCoordinator.CancelSelector()`), so no menu-mode-holding popup can outlive deactivation.

### Boundaries and invariants to preserve:
- In-form behavior is unchanged: Escape/commit inside QuickFiler still returns caret focus to the breadcrumb anchor (issue #438/#400 acceptance criteria).
- `KeyboardHandler` wiring and scope are unchanged.
- The predicate defaults to `() => true` so existing tests and callers that don't need the guard are unaffected.

### Dependencies or blocked work:
None. No new third-party dependency; no `CoreWebView2Controller`-level focus API is introduced (not present in the repo today and a larger change than required).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` (or its Open/FinishClose partials) — add the focus-permission predicate and consult it before `_focusAnchor`/`_focusPending`.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` — supply the predicate at `ConfigureBreadcrumbDropDown`, implemented against `FindForm()` activation/containment.
- `QuickFiler/Viewers/QfcFormViewer.cs` + `QuickFiler/Controllers/QfcFormController*.cs` — add a deactivate-routed event/handler that parks focus off any focused WebView2 and cancels an open selector.

#### Functions/classes/CLI commands impacted:
`BreadcrumbDropDownHost.FinishClose`, `BreadcrumbDropDownOpenLifetime` focus completion, `ItemViewer.ConfigureBreadcrumbDropDown`, `QfcFormViewer` (new `Deactivate` wiring), `QfcFormController` (new handler).

#### Data flow and validation changes:
None (behavioral/focus-routing change only; no data model impact).

#### Error handling and logging updates:
None required beyond existing patterns; no new failure modes introduced (the predicate is a pure function of form state).

#### Rollback/feature-flag considerations (if applicable):
No feature flag; the predicate's `() => true` default is the pre-fix behavior, so a revert is a single-commit revert.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
New internal settable property on `BreadcrumbDropDownHost`: `internal Func<bool> MayTakeFocus { get; set; } = () => true;`, assigned by `ItemViewer` immediately after constructing the host (and assignable by tests via the existing `InternalsVisibleTo("QuickFiler.Test")`). No constructor signature changes — the six-constructor chain keeps its baseline arities so existing reflection-bound test harnesses (arity/shape constructor matching) remain valid. No public signature changes.

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
Additive internal property with a `() => true` default and unchanged constructor signatures; no breaking change to existing callers or tests (including tests that bind to the constructor set by reflection).

#### Performance constraints (latency/throughput/memory):
None beyond existing behavior; the predicate check is O(1).

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - Outlook/VSTO single-UI-thread hosting model: all Outlook windows and the QuickFiler form share one UI thread, one Win32 input queue, and one focus window. The fix's correctness reasoning depends on this model.
  - MicrosoftEdge/WebView2Feedback issue #951 (WebView2 holds keyboard focus in VSTO WinForms hosts after click-back into the host) is the underlying runtime defect. It remains open upstream; this change works around it by never re-acquiring focus after deactivation, it does not fix it.
  - A live Outlook session with the add-in loaded is available for manual end-to-end verification.
- Constraints (budget, performance, compatibility):
  - Repository C# toolchain and policies per CLAUDE.md: MSTest + Moq + FluentAssertions for tests; CSharpier formatting via `dotnet tool run`; .NET analyzer and nullable gates via the approved `msbuild /t:Rebuild` commands; no temporary files in tests.
  - Coverage policy per CLAUDE.md: repository-wide line coverage >= 80% (with the ratified COM/VSTO/WinForms exemptions); new or changed code targets >= 90%; no coverage reduction on changed lines.
  - Backward compatibility: the `BreadcrumbDropDownHost` predicate seam must not change any constructor signature — existing tests bind to the constructor set by reflection with arity/shape matching — so the predicate is an internal settable property defaulting to `() => true`, leaving existing callers and tests unaffected.
- External dependencies (services, libraries, releases): none new. No new NuGet packages; no WebView2 SDK surface beyond what is already referenced.

## Data / API / Config Impact
- User-facing or API changes: none user-facing. The only code-surface change is the additive internal settable property on `BreadcrumbDropDownHost` (`Func<bool>` focus-permission predicate `MayTakeFocus`, default `() => true`; constructor signatures unchanged), plus the internal Seam-B style deactivate event/handler on `QfcFormViewer`/`QfcFormController`.
- Data or migration considerations: none. Behavioral focus-routing change only; no data model, storage, or schema impact.
- Logging/telemetry updates (if any): none required beyond existing patterns.
- Compatibility notes (CLI flags, config schemas, versioning): no configuration keys, flags, or schema changes. The property addition is non-breaking for existing callers and tests; no constructor signature changes.

## Test Strategy
Seeded from issue (disposition after root-cause analysis):

- Unit coverage areas: the seeded `KeyboardHandler`/`KbdActive` coverage area is superseded — root-cause analysis confirmed `KeyboardHandler` is correctly scoped and is not changed by this fix. The window-activation/focus-transfer logic is the correct coverage area and is expanded below.
- Integration scenario to retest (manual): open QuickFiler, click into a native Outlook Explorer/Inspector window, confirm normal typing; return to QuickFiler and confirm its own keyboard navigation still functions before closing.
- Manual verification notes: verify no regression to QuickFiler's own keyboard-driven filing workflow (arrow keys, character actions, string filter actions) after the fix.

- Regression tests to add or update (MSTest + Moq + FluentAssertions, headless, via the existing injectable seams — `Mock<IBreadcrumbDropDownHost>`, the injectable `showPopup`/`closePopup` delegates, and the constructor-injected focus delegates):
  - Focus-permission predicate gating `FinishClose`: when the predicate returns `false`, `_focusAnchor` is not invoked on close (including the `OnDropDownClosed` path, coverable by raising `DropDown.Closed` on the real host with stub delegates, per the existing `BreadcrumbDropDownHostTests` FocusPendingCount pattern); when the predicate returns `true`, `_focusAnchor` is invoked as before.
  - Focus-permission predicate gating the late-arriving `_focusPending` completion: an open that completes after the predicate flips to `false` does not invoke the pending focus delegate; with the predicate `true`, the pending focus completes as before.
  - Deactivate-parking handler (controller-level, through the viewer interface event, no window shown): when the currently focused control hosts a WebView2, the handler parks focus on a benign non-WebView2 control and invokes selector cancellation (`BreadcrumbCoordinator.CancelSelector()`); when no WebView2 holds focus, it does not disturb focus.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Click-out while the popup is open (the `FinishClose` steal path).
  - Click-out while a popup open is in flight (late `_focusPending`).
  - Click-out during search typing (per-keystroke close/reopen churn from #438).
  - Deactivate while a WebView2 (anchor, body, or popup) holds focus.
  - Predicate left unset (property default `() => true`): behavior is identical to pre-fix; all existing `BreadcrumbDropDownHost` tests remain green unmodified.
- Error handling and logging verification: no new failure modes — the predicate is a pure function of form state; verify no exceptions escape the deactivate handler path.
- Coverage impact and targets for changed lines/modules: new/changed code targets >= 90% line coverage; no coverage reduction on changed lines; repository-wide floor (>= 80%, with ratified COM/VSTO/WinForms exemptions) maintained.
- Toolchain commands to run (format → lint → type-check → test), per CLAUDE.md:
  1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (required): live WebView2-runtime focus retention and actual Outlook keystroke delivery cannot be unit-tested (per the research artifact's Automation Feasibility section — the failure exists only in the composition of Outlook's native message pump, real WebView2 runtime child windows, and real Win32 activation transitions). Verify manually in a live Outlook session: type into native Outlook windows with QuickFiler open in each internal state (navigation on/off, popup open/closed, mid-search); confirm click-back restores QuickFiler navigation; confirm Escape/commit still returns the caret to the breadcrumb anchor.


## Acceptance Criteria
- [ ] With QuickFiler open in any internal state (keyboard navigation on or off, breadcrumb popup open or closed, mid-search), typing into a native Outlook window (Explorer, Inspector, search box) operates Outlook normally — verified manually in a live Outlook session per the repro steps. (pending manual live-Outlook verification — see evidence/other/manual-verification-pending.md)
- [ ] Returning to QuickFiler by click restores QuickFiler's own keyboard navigation (arrow keys, character actions, string filter actions), verified in the same manual session. (pending manual live-Outlook verification — see evidence/other/manual-verification-pending.md)
- [ ] Escape/commit inside QuickFiler still returns the caret to the breadcrumb anchor (issue #438/#400 behavior preserved), verified by existing tests remaining green and by manual check. (pending manual live-Outlook verification — see evidence/other/manual-verification-pending.md) The automated half is already satisfied: the whole `QuickFiler.Test` assembly ran 1218/1218 green with the pre-existing breadcrumb tests byte-unmodified (`evidence/regression-testing/p4-t3-summary.md`), and `FinishClose_PredicateTrue_FocusAnchorInvoked` plus `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked` assert the in-form focus return directly. Only the manual half is outstanding.
- [x] Regression unit tests added and passing (list file paths and test names) covering: (a) `FinishClose` does not invoke `_focusAnchor` when the focus-permission predicate is false and does when true; (b) the late `_focusPending` completion is gated by the same predicate; (c) the deactivate handler parks focus off a WebView2-hosting control and invokes selector cancellation.
  - Seventeen regression tests added across three files, all passing.
  - `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` (8): `FinishClose_DropDownClosedPath_PredicateFalse_DoesNotFocusAnchor`, `FinishClose_ProgrammaticClose_PredicateFalse_DoesNotFocusAnchor`, `FinishClose_PredicateTrue_FocusAnchorInvoked`, `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor`, `AlreadyOpenRefocus_PredicateFalse_DoesNotFocusPending`, `AlreadyOpenRefocus_PredicateTrue_FocusPendingInvoked`, `FreshOpenFocus_PredicateFalse_DoesNotFocusPending`, `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked`. Covers (a) and (b).
  - `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` (7): `RegisterFormEventHandlers_SubscribesFormDeactivated`, `UnregisterFormEventHandlers_UnsubscribesFormDeactivated`, `FormDeactivated_WebView2Focused_ParksFocusOnce`, `FormDeactivated_NoWebView2Focus_DoesNotPark`, `FormDeactivated_CancelsSelectorOnEveryItemController`, `FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow`, `FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues`. Covers (c).
  - `QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` (2): `CancelBreadcrumbSelector_ForwardsToViewer`, `CancelBreadcrumbSelector_NullViewer_DoesNotThrow`. Covers the (c) fan-out hop.
  - Evidence: `evidence/regression-testing/p4-t1/` (29/29 passed, TRX `p4-t1-breadcrumbdropdownhosttests.trx`) and `evidence/regression-testing/p4-t2/` (9/9 passed, TRX `p4-t2-controller-deactivate-and-cancel.trx`).
- [x] The focus-permission predicate is evaluated at execution time of the scheduled focus action, not at scheduling time (asserted by test).
  - Asserted by `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor` (`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`): the close work is queued while the predicate still returns true, the test asserts `context.PendingCount` is greater than zero to prove the work was queued rather than run inline, the predicate then flips to false, and only then is the queue drained — after which `FocusAnchorCount` is 0. Passing run recorded in `evidence/regression-testing/p4-t1/p4-t1-breadcrumbdropdownhosttests.trx`.
- [x] `KeyboardHandler` is unchanged, and no behavior changes occur outside the focus/activation scope defined in Scope & Non-Goals.
  - Evidence: `evidence/qa-gates/keyboardhandler-unchanged.md`. Both `git status --porcelain -- QuickFiler/Controllers/KeyboardHandler.cs` and `git diff 361a49b884a4e3fe192bf04bae05151c598398fa -- QuickFiler/Controllers/KeyboardHandler.cs` produced empty output. The complete production change set is the eleven files enumerated in `evidence/baseline/scope-lock.md`, all of which are focus-permission, deactivation-routing, or selector-cancellation surface.
- [x] All existing `BreadcrumbDropDownHost` and breadcrumb pipeline tests pass without modification (predicate default `() => true` preserves pre-fix behavior).
  - Evidence: `evidence/regression-testing/p4-t3-summary.md` and TRX `evidence/regression-testing/p4-t3/p4-t3-quickfiler-test-whole-assembly.trx`. The whole `QuickFiler.Test` assembly ran 1218 tests with 1218 passed and 0 failed, exactly `QFT_BASELINE_TOTAL` (1201) plus the 17 new tests, so no pre-existing test was dropped, filtered away, or altered.
  - Unmodified-files assertion: `git status --porcelain -- QuickFiler.Test/` lists only the three new test files, `QuickFiler.Test.csproj`, and `QfcThemeHelperTests.cs`. `BreadcrumbDropDownHostTests.cs` and `BreadcrumbDropDownHostTests.Part2.cs` are byte-unmodified, so the reflection-bound constructor harnesses were never edited — which is why the fix uses a settable property rather than a constructor parameter.
  - Sanctioned structural-enabler exception: `FakeQfcItemController` in `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` gained the interface-completing member `public void CancelBreadcrumbSelector() { }`. That is the file's entire diff; no `[TestMethod]`, assertion, or test body was changed.
- [x] Coverage: new/changed code >= 90% line coverage; no coverage reduction on changed lines.
  - Evidence: `evidence/qa-gates/coverage-delta.md`. Changed-line coverage is **100.00%** on all five coverage-bearing files: `BreadcrumbDropDownHost.cs` (11 changed executable lines), `BreadcrumbDropDownHost.Open.cs` (1), `QfcFormController.Deactivate.cs` (24), `QfcItemController.FolderHandling.cs` (1), `QfcFormController.SetupDisposal.cs` (2). Zero uncovered changed lines and zero regressed lines.
  - Repo-wide line-rate rose from **0.852721** at baseline to **0.852804** at final (delta **+0.000083**), above the gate minimum of 0.851721 and above both the CLAUDE.md >= 80% floor and the >= 85% uniform threshold. Branch-rate 0.792300, above the 75% uniform threshold.
- [x] Full toolchain pass completed (CSharpier format → analyzers rebuild → nullable rebuild → vstest with coverage) using the commands listed in Test Strategy.
  - All four gates passed in the same single clean pass (the loop never restarted; no step failed and no step rewrote a file):
    - `evidence/qa-gates/final-format-check.md` — `csharpier check .`, EXIT_CODE 0, 1558 files, zero violations.
    - `evidence/qa-gates/final-analyzer-build.md` — `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, EXIT_CODE 0, 0 errors, 5 warnings (all the pre-existing packages.config advisory; zero delta versus baseline).
    - `evidence/qa-gates/final-nullable-build.md` — `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`, EXIT_CODE 0, 0 errors, zero `CS86xx` diagnostics.
    - `evidence/qa-gates/coverage-final.md` — full-suite vstest with coverage, EXIT_CODE 0, 6838/6838 passed, 0 failed.
- [x] Feature-folder docs updated to match the delivered behavior.
  - `spec.md` — AC-4 through AC-9 checked off with evidence citations; AC-1, AC-2 and AC-3 annotated as pending manual live-Outlook verification; Rollout & Follow-up extended with the live-session checklist owner and the two follow-up items.
  - `issue.md` — "Proposed Fix / Validation Ideas" records the corrected coverage area (window-activation / focus-transfer, not `KeyboardHandler`) as delivered and leaves the two manual items open; "Next Step" records fix-implemented and manual-verification-pending.
  - `evidence/issue-updates/issue-677.md` — mirror of the `issue.md` update text.
  - `evidence/other/manual-verification-pending.md` — the outstanding manual checklist.
  - `pr-notes.md` — PR summary, root-cause synopsis, risks, validation performed, and rollback note.
  - `plan.2026-08-28T08-45.md` — every task checked off against its evidence artifact.

## Risks & Mitigations
- Technical or operational risks:
  - **Predicate evaluated at the wrong time.** The steal arises precisely from the gap between scheduling a focus action and executing it (`FinishClose` schedules `_focusAnchor` asynchronously, which lands after the user's click-out). If the guard predicate is captured or evaluated at scheduling time, a stale `true` reintroduces the steal.
  - **Over-broad focus parking.** If the deactivate-parking handler (or the predicate) suppresses focus restoration on in-form paths, it breaks the Escape/commit-returns-to-breadcrumb behavior delivered under issues #438/#400.
- Mitigations and rollbacks:
  - Evaluate the "may take focus" predicate at execution time of the scheduled focus action only, per the research artifact's "Ordering rule"; encode this in a dedicated unit test (see Acceptance Criteria).
  - Scope focus parking strictly to the `Form.Deactivate` path; never invoke it on the in-form Escape/commit path. Existing breadcrumb tests plus the preserved #438/#400 acceptance behavior guard against regression.
  - Rollback: the predicate's `() => true` default is the pre-fix behavior, so the change is a single-commit revert with no data or config cleanup.

## Rollout & Follow-up
- Release/rollout steps: single PR; no phased rollout, feature flag, or configuration change required. Ships with the normal add-in build.
- Post-fix monitoring or clean-up tasks:
  - During live verification, reconfirm or rule out the secondary WinForms modal-menu-mode contributor (`ToolStripManager.ModalMenuFilter` / hosted-message-hook behavior). It is asserted from .NET Framework reference-source background knowledge, not verified this session — e.g., check whether keyboard loss ever occurs in a session where no breadcrumb popup has been opened.
  - If the symptom recurs after the fix, log the focused HWND at click-out time to disambiguate which focus path dominates (gesture-navigation residue vs. `FinishClose` steal vs. late `_focusPending`), per the research artifact's provenance notes.
- Links: issue #677 (https://github.com/drmoisan/TaskMaster/issues/677); research: `research/2026-08-28T09-15-quickfiler-outlook-keyboard-suppression-677-research.md`; upstream runtime defect: MicrosoftEdge/WebView2Feedback issue #951; related prior work: issues #438/#400 (breadcrumb focus-return behavior).

### Post-implementation follow-up (added at delivery)

1. **Live-verification session — owner: project maintainer (drmoisan).** One session covers two
   things. First, the manual acceptance items AC-1, AC-2 and the manual half of AC-3, whose full
   checklist is in `evidence/other/manual-verification-pending.md`; those three checkboxes stay
   unchecked until that session runs. Second, in the same session, the **secondary-contributor
   reconfirmation**: test whether keyboard loss ever occurs in a session where no breadcrumb popup
   has been opened. If it does, the WinForms modal-menu-mode contributor
   (`ToolStripManager.ModalMenuFilter` / `HostedWindowsFormsMessageHook`) is ruled out as a
   necessary condition; if keyboard loss occurs only after a popup has been opened, it is
   confirmed as a contributor. That mechanism is asserted from .NET Framework reference-source
   background knowledge and was never verified live, so this is the measurement that settles it.
2. **If the secondary contributor is confirmed as an independent live defect, it must be promoted
   through the MCP promotion lifecycle into a new GitHub issue.** It must not be left as prose in
   this feature folder, because feature-folder prose disappears at merge. The promotion carries its
   own spec and plan; it is out of scope for issue #677, whose Part 2 already prevents a popup from
   outliving deactivation.
3. **Focused-HWND logging fallback.** If the symptom recurs after this fix, log the focused HWND at
   click-out time to disambiguate which focus path dominates — gesture-navigation residue versus the
   `FinishClose` steal versus the late `_focusPending` completion. Per the research artifact's
   provenance notes, which path dominates in the reporter's specific repro is recorded as
   **not verified by any means**, so recurrence is exactly the case where that measurement is
   needed rather than a further code change.
