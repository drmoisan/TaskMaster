# 2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select (Spec)

- **Issue:** #796
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-06
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug

> Work-mode note: this feature runs in full-bug mode. This spec is the single authoritative acceptance-criteria source. No user-story.md exists for this feature and its absence is by design, not a gap.

> Path convention in this document: a backticked path is a write claim consumed by downstream blast-radius derivation. Every backticked path in this document appears in the `## Write Set` section. All other file references — citations, prior-art references, runbooks, evidence directories, and files explicitly excluded from the diff — are written without backticks on purpose.

## Context

In the QuickFiler item view, opening the folder drop-down makes the list flash open and immediately close, whether opened by clicking the arrow or by pressing Down in the search box. Typing letters in the search box does expand the list and keep it open, but clicking an item in the expanded list closes it without selecting that item; only the Up and Down keys change the selection. The list should stay open until it is closed explicitly, an item is selected, or a different QfcItem is selected, and a click on an item should select it.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in. The drop-down is not a ComboBox: it is a WebView2 breadcrumb page hosted by ItemViewer, plus a ToolStripDropDown popup hosting a second WebView2 owned by BreadcrumbDropDownHost.
- Build under test: debug build from TaskMaster/bin/Debug, HEAD c431dc32 (2026-09-06).
- Entry point: Outlook ribbon -> QuickFiler (ordinary and High Confidence).
- Data source: live mailbox, Inbox view.

Severity: High. Mouse selection of a filing folder is not possible in the item view; the user must type a search string and navigate with the keyboard.

Primary technical source: the verified research artifact at docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/research/2026-09-06T21-30-quickfiler-folder-dropdown-close-ordering-research.md. Its line counts, its AutoClose finding, and its non-SDK-style project finding were independently re-verified.

## Repro & Evidence

Steps to reproduce:
1. Launch QuickFiler. On any item, click the drop-down arrow in the folder field. Observe: the list opens and closes within a fraction of a second.
2. Put the caret in the search box and press Down. Observe: the same flash.
3. Type two or three letters in the search box. Observe: the list expands with search results and stays open.
4. Click an item in the expanded list. Observe: the list closes and the folder field still shows the previous selection.
5. Use Up/Down keys instead. Observe: the selection changes as expected.

Reproduces on every item; timing after item load does not matter (maintainer confirmed "it always occurs").

Expected:
- Opening the list by mouse or keyboard keeps it open until the user closes it (Escape, Left arrow, clicking the arrow again), an item is selected, or a different QfcItem is selected.
- Clicking an item in the open list selects that item and closes the list.
- A refresh of the row set while the list is open (search results, late suggestion decoration) does not close it. This is already guaranteed by #438 AC-3 and must remain so.

Actual:
- Open-by-arrow and open-by-Down both close immediately.
- Click on a row closes the list and discards the selection.
- Keyboard selection works.

Log evidence:
- Research section 8 examined TaskMaster/bin/Debug/logs/debug_2026-09-06.log (read only, outside the worktree). Zero log lines of any level are emitted by the QuickFiler item-view drop-down pipeline. In the fourth session the last load-time line is at 20:02:28,953 and the next line in the entire file is at 20:15:31,320, a thirteen-minute window of live interaction containing no log lines at all.
- The recurring WebView2BreadcrumbHost initialization errors in that log come from EfcFormController.InitializeBreadcrumbHostAsync, a different component (EmailFilerControl), and are tracked separately. They are not evidence for this defect.
- Conclusion: the close is a normal, non-exceptional code path that produces no diagnostic. Runtime instrumentation is the only way to establish the ordering, which is why AC6 exists.

## Scope & Non-Goals

In scope:
- The OPEN and CLOSE lifecycle of the folder drop-down popup, on both the mouse (arrow-click) and keyboard (Down) gesture paths.
- The selection-commit ordering for a row click relative to any auto-close cancel.
- The self-inflicted-versus-genuine form-deactivation distinction consumed by the #677 cancel-on-deactivate handler.
- The #680 search-leave handoff latch, extended to the mouse open path.
- Debug instrumentation at the two sites named by AC6.

Out of scope / non-goals:
- Row TEXT projection and the breadcrumb bridge router's render projection classes. A concurrent sibling item owns those. See the sibling-contention note in `## Write Set`.
- The EmailFilerControl breadcrumb (WebView2BreadcrumbHost / EfcFormController) initialization failures.
- Any redesign of the breadcrumb selection session model in UtilitiesCS. The AC5 regression guard is satisfied by leaving the session-preserving replacement path untouched, so the UtilitiesCS router and session files must stay out of the diff.

Explicitly excluded systems and paths: no edits under the .claude tree, the .codex tree, or the .agents tree; no edits to the published JSON files under the config directory; no edits to any GitHub workflow file; no edits to the solution file or the repository-root build property files.

## Root Cause Analysis

All statements below were re-derived against the current worktree in research sections 1 through 3. Where a claim is inferred rather than verified, it is labelled.

### Verified structure

- The arrow is the `#dropDownButton` element in the breadcrumb page, which posts a `selectorToggle` message. The open pipeline is: bridge coordinator selector-message handling -> router `OpenSelector` -> session `OpenSelector` -> `Open` -> `SelectorOpenStateChanged` -> `HandleSelectorOpenStateChanged` -> host `OpenAsync` -> open-lifetime `OpenCoreAsync` -> `FocusCurrentSurface` -> `FocusPending`.
- The mouse toggle and the programmatic (keyboard) open share one request path. An existing test, SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle in QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs, pins that equivalence. This is consistent with mouse and keyboard failing identically.
- `AutoClose` is not constant. The constructor sets it true, `FinishClose` restores it to true, and every show overwrites it with the gesture's focus intent: `ShowPopup` assigns `DropDown.AutoClose = takeFocus`. Gesture opens run with `takeFocus == true`; a search-driven open latches no-focus, so it runs with `takeFocus == false` and both the framework auto-dismiss and `FocusPending` are disabled. That asymmetry maps one-to-one onto the reported symptom asymmetry.
- Asynchronous suggestion decoration is not the cause. The session-preserving row replacement path preserves `IsOpen` and emits no `OpenStateChanged`.
- `FinishClose` is the single close completion point. Under reason `Uncommitted` it calls the cancel delegate unconditionally, while the focus restoration step is gated by a may-take-focus predicate. That asymmetry is already documented in an in-source comment.
- A deactivation-driven cancel closes the host with reason `ExplicitCommit`, so `FinishClose` does not cancel a second time; the session was already cancelled upstream.

### The three candidate close paths

1. Form deactivation. `ParkFocusAndCancelSelectors` runs synchronously inside the WinForms Form.Deactivate event, parks the active control back into the form, then cancels every item controller's selector. There is no latch distinguishing a self-inflicted deactivation from a genuine one. Opening the popup calls Focus on the popup's own top-level window. Downstream code is CONFIRMED; that focusing the popup actually deactivates the QuickFiler form is Win32/WinForms runtime behavior and is INFERRED, not confirmed.
2. Native ToolStripDropDown auto-close, reaching `OnDropDownClosed` -> `FinishClose(Uncommitted)` -> cancel. The code path is CONFIRMED. Its first-cause status is INFERRED for gesture opens. For the search path in repro step 4 it is LIKELY REFUTED, because a search-driven open shows the popup with `AutoClose == false`, which disables the framework auto-dismiss. The `ToolStripDropDownClosedEventArgs.CloseReason` value is currently discarded by the handler and is the single most discriminating value available.
3. Search-box leave. The leave handler closes the drop-down; its #680 handoff latch is set only on the Down-arrow branch. The code path is CONFIRMED but it is LIKELY NOT REACHED on either reproduction path: on the arrow-click path the search textbox never holds focus, and on the search path the active leaf is a TextBox so the WebView2-focused precondition for focus parking is false. The AC4 gap is real and must still be closed, but this candidate is not the first cause.

### Why the row click does not select

When the session is open, activation commits and ends the session with `ExplicitCommit`, so no cancel runs. When the session is already closed, activation still updates the committed identity. The reported symptom is that the selection does not change at all, which is most parsimoniously explained by the activation message never being produced: the row listener registers `click`, which fires on mouseup, and if the popup is dismissed on mousedown no mouseup and therefore no click reaches the page. This is a hypothesis about browser and window behavior, not a code-verified fact. AC6's instrumentation is what settles it.

### Correction to a citation carried from the issue

The issue attributes the click-without-select symptom to candidate 2. For the reproduction as written (step 3 types, step 4 clicks) that attribution is very likely wrong, because the search-driven open sets `AutoClose` false. This spec records the attribution as unsettled and defers it to the AC6 evidence.

## Proposed Fix

### Invariant

A close of the folder drop-down cancels the pending selection if and only if the close was not caused by this add-in's own activation or focus movement and no selection commit is in flight. Every other close either commits or leaves the committed selection untouched.

### Ordering constraint imposed by AC6 (mandatory)

The FIRST implementation step is instrumentation, not a fix. It adds debug log lines to `ParkFocusAndCancelSelectors` and to `OnDropDownClosed`. The reproduction is then run and the ordering is read off the log. The choice among the three candidate close paths is made FROM that evidence.

No fix that presupposes the Win32 activation ordering may be written before that evidence exists. The premise that focusing the popup's WebView2 deactivates the QuickFiler form is INFERRED. This spec does not assert it as confirmed and does not specify a fix that depends on it being true.

Log-line requirements, so the evidence is actually discriminating:
- Both sites run on the single Outlook UI thread and log4net appends in call order, so FILE ORDER is the ordering. Millisecond timestamps are not sufficient: the entire flash occurs inside a fraction of a second and several steps can share a millisecond. Any analysis must rely on file order.
- `ParkFocusAndCancelSelectors` entry line: method name, WebView2Focused, whether the active form is null or is this form, group count; and per item, the item number and whether that item's selector was open.
- `OnDropDownClosed` entry line: method name, CloseReason (from the event args, which are presently discarded), programmatic-close flag, open state, AutoClose, disposed flag, pending-close flag. Log at entry, BEFORE the guard returns, so a suppressed close is still visible.
- A third temporary line at the search-leave handler is recommended although AC6 does not require it; without it candidate 3 can only be excluded indirectly.
- Logging shape must match the existing repository conventions: an interpolated string with a sentence prefix followed by Key=Value pairs. The field name differs by neighbourhood: controllers use `logger`, viewers use `log`.

Discriminators, stated in advance so the evidence read is not post-hoc:
- Candidate 1 is confirmed if a `ParkFocusAndCancelSelectors` line precedes any `OnDropDownClosed` line and the subsequent close reports CloseReason=CloseCalled with the programmatic-close flag set. It is refuted if that method is never entered during the flash, or reports zero groups or zero cancels, or is entered strictly after the close.
- Candidate 2 is confirmed if an `OnDropDownClosed` line appears first with CloseReason of AppFocusChange or AppClicked, programmatic-close false, open state true, AutoClose true. It is refuted by CloseReason=CloseCalled, by the line not appearing first, or by AutoClose false at that moment.
- Candidate 3 is confirmed by a leave line reporting handoff-pending false with the drop-down open immediately preceding the close, and refuted by the absence of that line or by handoff-pending true.

An additional low-cost discriminator: log whether the active form is null at deactivation entry. A ToolStripDropDown is not a Form, so a null active form is evidence of a self-inflicted deactivation and a non-null active form naming a foreign window is evidence of a genuine one. That framework behavior is asserted from background knowledge and is not verified in this repository, so it is corroborating rather than decisive.

### Design summary — what changes where

Contingent on the AC6 evidence. The shape below is the planned response; the branch actually taken is selected from the log.

- AC2, self-inflicted deactivation seam. Add a boolean intent member to the form-viewer interface, implemented in the concrete form viewer as the only site that reads non-injectable activation state, and consumed by the deactivate handler to gate the cancel loop. Polarity: false means genuine (not self-inflicted). See the test-strategy section for why that polarity is load-bearing. An alternative with a smaller interface footprint is a predicate property assigned by the same wiring that assigns the existing may-take-focus predicate; the interface-member form is preferred because the deactivate suite already mocks the interface and would need no new construction seam.
- AC3, commit-before-cancel ordering. Add a pending-commit latch consulted by `FinishClose` so an uncommitted-reason close does not cancel while a commit is in flight. If the row-activation message is shown by the evidence never to be produced, the additional change is to move the row activation listener from `click` (mouseup) to a pointer-down event so the activation message is produced before dismissal.
- AC4, mouse-path latch. Extend the #680 handoff latch so it is also set on the mouse open path, not only the Down-arrow branch.
- AC6, instrumentation. Host-side instrumentation goes into a new partial part, for the file-size reason recorded below.
- AC1 and AC5 are outcomes of the above plus the untouched session-preserving replacement path; neither introduces its own production change beyond what AC2, AC3 and AC4 deliver.

### Boundaries and invariants to preserve

- The #677 contract: a genuine deactivation of the QuickFiler form still cancels every item controller's selector.
- The #438 AC-3 contract: a row-set refresh while open does not close the list. This is preserved by leaving the session-preserving replacement path in UtilitiesCS out of the diff entirely.
- The #680 contract: the search-leave handoff continues to work on the Down-arrow path.
- Cancel suppression must be SCOPED. A close with no commit in flight and no self-inflicted activation must still cancel. See the test-strategy section, where two existing assertions are kept unchanged specifically to prove the suppression is scoped rather than global.
- The reflection-based constructor binding on the drop-down host must not be disturbed. Any new host state is a settable internal property assigned after construction, matching the existing may-take-focus precedent, not a constructor-arity change.
- The may-take-focus predicate default must not be changed.

### File-size constraints (independently verified)

- QuickFiler/Viewers/BreadcrumbDropDownHost.cs is 498 lines against the repository's 500-line ceiling and declares no logger of any kind. AC6's host-side instrumentation cannot be added to it. It requires the new partial part `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs`, following the existing partial-split precedent set by `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`. Any net line growth in the main part must be offset by moving `OnDropDownClosed`, or by having it delegate to a diagnostics helper declared in the new part.
- QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs is 499 lines and QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs is 500 lines. Neither can absorb new host-level tests, so a new test file is required.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is 456 lines. Growth there is constrained; keep the AC2 wiring change to the minimum.
- QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs is 477 lines, leaving 23 lines of headroom. That is not enough for an Arrange-Act-Assert pair with doc comments, so the AC4 tests go in a new file.

### Correlation identifier for the AC6 evidence

No identifier exists today that both instrumentation sites can carry. The form-controller side can log the item number, which already exists on the item-controller interface. The host side knows only its anchor and its own instance, and the anchor name is a Designer constant identical for every item viewer, so it does not discriminate between items. If per-item correlation proves necessary when the log is read, use an ordinal surrogate logged at both ends plus a tying line, or a settable internal owner-descriptor property on the host assigned right after construction. Do not change the host constructor arity.

### Error handling and logging updates

- Instrumentation is Debug level and must not change control flow. The `OnDropDownClosed` line is emitted at entry, before any guard return.
- The existing per-item boundary catch with error logging in the deactivate handler is preserved.
- No new exception is introduced by the instrumentation step.

### Rollback considerations

The instrumentation step is log-only and independently revertable. The AC2, AC3 and AC4 changes are each independently revertable and each carries its own regression test.

## Assumptions, Constraints, Dependencies

- Assumption (INFERRED, to be settled by AC6): focusing the popup's WebView2 deactivates the QuickFiler form on this thread, and the active form is null at that moment.
- Assumption (INFERRED, to be settled by AC6): a row click may be dismissed on mousedown so no click event and therefore no activation message reaches the page.
- Constraint: `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are non-SDK-style. Both declare `<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">` (verified). Every added .cs file requires an explicit `<Compile Include>` entry or it is silently not compiled.
- Constraint: 500-line ceiling per file. See the file-size section.
- Constraint: tests are MSTest with Moq and FluentAssertions. No temporary files, no live Outlook process, no shown window.
- Dependency: the AC6 evidence must exist before the fix branch is chosen.
- Dependency: the manual-verification runbook at docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/runbooks/confirm-dropdown-close-ordering.runbook.md is authored in parallel with this spec.

## Data / API / Config Impact

- User-facing behavior change: the folder drop-down stays open on gesture opens and a row click selects. No API, schema, config key, or CLI surface changes.
- Interface change: one added member on the form-viewer interface (AC2). This is an internal add-in interface with in-repo implementations and mocks only; all implementors are updated in the same diff.
- Logging: new Debug-level lines at two production sites (AC6). No telemetry, no persisted data, no new log destination. The existing log4net configuration is unchanged.
- Backward compatibility: no persisted state, no serialized contract, and no cross-assembly public API is affected.

## Write Set

Every path below is a concrete file the fix's diff will create or modify. Paths are repository-relative, use forward slashes, and contain no spaces. Sixteen paths.

Preflight round 1 proposed a seventeenth path, QuickFiler/Interfaces/IQfcItemController.cs, to carry the per-item selector-open state the AC6 diagnostic reports. That path is written here without backticks on purpose, because it is not a write claim. That proposal was adopted and then withdrawn after the planner established that adding a member to that interface breaks a compiled hand-written implementor outside the write set, on a target framework with no default interface members. The adopted resolution reaches the same value through an internal member on the concrete item controller, whose file is already in the write set, so the write set stays at sixteen paths and no interface changes.

### Production — modify

- `QuickFiler/Controllers/QfcFormController.Deactivate.cs` — modify — AC6 instrumentation of `ParkFocusAndCancelSelectors`, and the AC2 self-inflicted guard around the cancel loop.
- `QuickFiler/Interfaces/IQfcFormViewer.cs` — modify — AC2: declare the self-inflicted-deactivation seam.
- `QuickFiler/Viewers/QfcFormViewer.cs` — modify — AC2: production implementation of the seam; the only form-side site that reads non-injectable activation state.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — modify — AC3: commit-before-cancel ordering in `FinishClose`; move or delegate `OnDropDownClosed` into the new diagnostics part to stay under the 500-line ceiling (currently 498).
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` — modify — AC1 and AC3: the `AutoClose = takeFocus` policy in `ShowPopup`, and any pending-commit latch set at open time.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` — modify — AC2: assign the new popup-owns-activation state alongside the existing may-take-focus assignment.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — modify — AC4: extend the #680 leave-handoff latch to the mouse open path. Also AC6: expose this item's selector-open state to the per-item deactivation diagnostic as an internal get-only member forwarding to the item viewer's existing `IsFolderDropDownOpen`. The expression `_itemViewer.IsFolderDropDownOpen` is already used in this file at lines 200 and 225, so no new dependency is introduced.
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` — modify — AC3: only if the commit-before-cancel ordering must be enforced at the coordinator rather than at the host; the AC6 evidence decides.
- `QuickFiler/Resources/FolderBreadcrumb.html` — modify — AC3: move the row activation listener at lines 289-291 from `click` (which fires on mouseup) to a pointer-down event so the activation message is produced before dismissal. SIBLING CONTENTION: a concurrent sibling item edits this same page for the row TEXT projection. This item owns only the row activation listener and touches no projection logic. The contention is recorded here rather than avoided by dropping the file.

### Production — create

- `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs` — create — AC6 host-side instrumentation of `OnDropDownClosed` plus the log4net field. Required because the main part is 498/500 lines and declares no logger; follows the existing `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` partial-split precedent.

### Test — modify

- `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` — modify — AC2: deliberate update of the deactivate contract plus the new self-inflicted negative test. 248 lines, ample room.
- `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` — modify — AC1 and AC3: keep all five existing tests, add the pending-commit-versus-native-close guard. 380 lines.

### Test — create

- `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs` — create — AC3: commit-before-cancel ordering at the host seam. The two adjacent host-level suites are at 499 and 500 lines and have no room.
- `QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs` — create — AC4: mouse-path leave latch. The existing event-handler suite has only 23 lines of headroom.

### Compile entries — modify

- `QuickFiler/QuickFiler.csproj` — modify — non-SDK-style project (verified `<Project ToolsVersion="15.0" ...>`); add the `<Compile Include>` entry for the new diagnostics part or it is silently not compiled.
- `QuickFiler.Test/QuickFiler.Test.csproj` — modify — non-SDK-style project (verified); add the `<Compile Include>` entries for both new test files.

### Explicitly not in the write set

These files were considered and no change is expected, so they are named without backticks on purpose: QuickFiler/Interfaces/IQfcItemController.cs, QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs, QuickFiler/Viewers/IItemViewer.cs, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs, UtilitiesCS/UtilitiesCS.csproj, UtilitiesCS.Test/UtilitiesCS.Test.csproj, QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs, QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs, QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs, QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs, QuickFiler/Viewers/IBreadcrumbDropDownHost.cs. The AC5 regression guard is satisfied by leaving the session-preserving replacement path untouched, which is why the UtilitiesCS router and session files must stay out of the diff.

## Test Strategy

Framework: MSTest, with Moq for mocking and FluentAssertions for assertions. No temporary files, no external processes, no live Outlook, no shown window.

### Existing tests that pin the current contract

Both files below are deliberately UPDATED, not weakened and not deleted.

**QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs — method `FormDeactivated_CancelsSelectorOnEveryItemController` at line 172.**

This method currently pins an unconditional cancel-on-deactivate for every item controller, asserting `Times.Once()` on two injected controllers. Under AC2 the cancel becomes conditional. The deliberate update is:

1. Keep the method at its current name and its current `Times.Once()` assertions on both controllers, so the #677 contract remains visibly pinned. Add one Arrange line setting the new seam to report a GENUINE deactivation, and amend the doc comment.
2. Polarity choice, load-bearing: false means genuine (not self-inflicted). Moq's default `bool` return is false, so this keeps the existing Arrange block valid without modification and holds the diff on this method to a doc-comment amendment plus one explicit line. Do not invert the polarity.
3. Add a sibling test, for example `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector`, that sets the seam to report self-inflicted and asserts `Times.Never()` on both controllers. This is the AC2 fail-before test.
4. `FormDeactivated_WebView2Focused_ParksFocusOnce` at line 134 stays unchanged only if the fix keeps focus parking unconditional. If parking is also suppressed for a self-inflicted deactivation, that test needs the same explicit genuine-case Arrange line plus a paired negative. Decide this explicitly; do not let it change by default.

The other four methods in the file are unaffected. At 248 lines the file absorbs both additions under the ceiling.

**QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs.**

The two tests the issue cites at lines 124 and 143 assert close IDEMPOTENCY — that exactly one `Close` of the expected reason reaches a mocked host when a close intent arrives while the open task is unresolved. They do not encode cancel-versus-commit precedence. Under AC1 they remain correct as written and the deliberate update for them is NO CHANGE.

The assertions that actually encode cancel precedence are the literal `CancelCount.Should().Be(1)` assertions at lines 48 and 79, in `CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent` and `CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus`. In both tests no commit is in flight, so the correct post-fix value remains 1. Both assertions are KEPT UNCHANGED and serve as the guard proving that any commit-time cancel suppression is SCOPED rather than global. A fix that drives either of them to zero is a design signal that the suppression is too broad; it is not a test to update.

The `FocusAnchorCount` assertions at lines 49, 80 and 114 count the focus-anchor delegate and stay at 1 because the harness leaves the may-take-focus predicate at its permissive default. Do not change that default.

The added test is the AC3 fail-before guard: a native-reason close arriving while an activation commit is pending must not cancel. At 380 lines the file has room for one or two added tests; a third harness would require a new file.

### New tests by acceptance criterion

- AC1: at the managed seam, assert the open task resolves true, the host reports open, no `Close` reaches the mocked host, and the session reports the selector open, across the gesture open path. New assertions in `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs`.
- AC2: both branches of the new seam, in `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`, using the existing mocked interface, `Mock.Raise` for the deactivation event, and the existing reflection-injected group fan-out.
- AC3: in `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs`, drive the host with delegate counters in the style of the existing pending-open harness and assert the cancel delegate is not invoked while a commit latch is set and IS invoked when it is not. If the pointer-down change is taken, add a coordinator-level assertion that a pointer-down-sourced activation commits.
- AC4: in `QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs`, with a mocked item viewer, drive the mouse open path, raise the search-box leave, and assert the drop-down is not closed.
- AC5: assert `Close` is never invoked on a mocked host across a row-set refresh while open. The underlying replacement path already emits no open-state change and existing router suites pin that.
- AC6: pin structurally that a log statement exists at each of the two named sites, in the manner the repository already uses for declaration-only seams.

### Automation feasibility, carried from research

Every acceptance criterion retains at least one automatable managed-seam assertion. No criterion is left with manual verification as its only evidence.

| AC | Automatable at a managed seam | The part that is not automatable |
|---|---|---|
| AC1 | Partially. Open task resolves true, host stays open, no `Close` reaches the mocked host, session reports open. | That no FRAMEWORK close occurs, because no framework dropdown is shown. |
| AC2 | Yes, fully. Both branches of the new seam are drivable with the existing mock and reflection-injected groups. | Whether the popup taking focus actually raises Form.Deactivate on this thread. |
| AC3 | Partially. The branching is testable with delegate counters: cancel suppressed while the commit latch is set, invoked when it is not. | Producing a genuine framework closed-event argument with a framework-chosen CloseReason. A test can only hand the handler a constructed value, which proves the branch, not the framework's choice. |
| AC4 | Yes, fully. Mocked item viewer; drive the mouse open path and raise leave. | None material. |
| AC5 | Yes, fully, and largely already covered by existing router suites. | None material. |
| AC6 | Partially. That the log statements exist can be pinned structurally. | That the emitted ORDERING is what the fix assumes; that requires the live host. |

### Manual verification (permitted exception, not a merge gate)

Confirming the runtime ordering requires a live Outlook process, a real WebView2 surface, a real ToolStripDropDown, and human mouse and keyboard gestures. None of that is automatable in this repository: the test policy forbids external processes and no window may be shown. The orchestrator has recorded this as a permitted human-interaction exception, following the precedent set by #400 and #438.

Runbook: docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/runbooks/confirm-dropdown-close-ordering.runbook.md

Manual steps to perform and record:
1. Run the AC6-instrumented build and reproduce the arrow-click flash; capture the log excerpt showing file order across the two instrumented sites.
2. Reproduce the Down-arrow flash; capture the same.
3. Reproduce the type-then-click case; capture the same, and specifically capture whether an activation message is produced.
4. Refresh the row set while open and confirm the list does not close.
5. Select a different QfcItem while the list is open and confirm the list closes.

The manual result is captured as an evidence artifact under the feature's canonical evidence tree at docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/. This manual step is NOT a merge gate. It converts the INFERRED premise into an observation. It does not weaken AC6 and does not substitute for the automated assertions listed above.

### Coverage

The repository line-coverage floor applies to the changed production files. New tests must cover the changed lines in the deactivate handler, the host close path, and the item-controller leave latch. The instrumentation partial part contains logging only; its coverage contribution is incidental and must not be used to inflate the figure for the behavioral changes.

### Toolchain (run in this exact order; restart from step 1 on any failure or auto-fix)

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Use the Rebuild target, never Build: MSBuild's up-to-date check does not invalidate on a command-line property change, so a warm Build returns exit 0 with compilation skipped and no analyzer or nullable diagnostic is produced. Do NOT add the solution-wide `/p:Nullable=enable` property; it is deliberately absent from CI and produces hundreds of errors against files that never opted in.

## Acceptance Criteria

- [ ] AC1: Opening the list by arrow click or by Down in the search box leaves it open until Escape, Left, a second arrow click, an item selection, or selection of a different QfcItem.
- [ ] AC2: A deactivation of the QuickFiler form caused by the popup taking focus does not cancel the selector session; a deactivation caused by any other window still does (the #677 contract is preserved for genuine deactivation).
- [ ] AC3: A mouse click on a row in the open list selects that row and closes the list; the selection is committed before any auto-close cancel runs.
- [ ] AC4: The #680 leave-handoff latch covers the mouse open path as well as the Down-arrow path.
- [ ] AC5: Row-set refreshes while open (search, late decoration) continue not to close the list (#438 AC-3 regression guard).
- [ ] AC6: The first implementation step instruments `ParkFocusAndCancelSelectors` and `OnDropDownClosed` with debug log lines so the runtime ordering is confirmed before the fix is chosen.

## Verification Conditions

These are supporting conditions, not acceptance criteria. They are tracked here so nothing is added to the acceptance-criteria section above.

- The full toolchain completes in a single clean pass in the order given, with the Rebuild target and without the solution-wide nullable property.
- Both new .cs files have explicit `<Compile Include>` entries in their non-SDK-style project files; verified by confirming the new tests actually execute rather than silently not compiling.
- No file in the write set exceeds 500 lines after the change, with particular attention to the host main part (498 before), the breadcrumb item-viewer part (456 before), and the pending-open test suite (380 before).
- The two `CancelCount` assertions at lines 48 and 79 of the pending-open suite still pass unchanged.
- The AC6 log excerpt is captured as an evidence artifact under the feature's evidence tree before the fix branch is chosen, and the spec's candidate status table is updated from INFERRED to OBSERVED or REFUTED accordingly.
- No file under the .claude, .codex or .agents trees, no published config JSON, no workflow file, no solution file, and no repository-root build property file appears in the diff.

## Risks & Mitigations

- Risk: the AC6 evidence contradicts the candidate ranking and the planned AC2 or AC3 change is not the first cause. Mitigation: the ordering constraint makes the instrumentation the first step precisely so the fix is chosen from evidence; the spec commits to no fix that presupposes the inferred Win32 ordering.
- Risk: a commit-time cancel suppression is written too broadly and silently disables the legitimate cancel path. Mitigation: the two `CancelCount` assertions at lines 48 and 79 of the pending-open suite are kept unchanged as the scoping guard; either dropping to zero is a design signal.
- Risk: shared-file contention on the breadcrumb HTML page with the concurrent sibling item. Mitigation: the file is declared in the write set with a contention note so the scheduler serializes rather than silently interleaves; this item's change is confined to the row activation listener.
- Risk: a new .cs file is added without a project compile entry and its tests silently do not exist. Mitigation: both project files are in the write set and the verification conditions require confirming the new tests actually execute.
- Risk: instrumentation exceeds the 500-line ceiling on the host. Mitigation: the new partial diagnostics part, following the established partial-split precedent.
- Risk: the manual verification is treated as a merge gate that no automated suite can satisfy. Mitigation: it is explicitly recorded as a permitted exception with a runbook, and every acceptance criterion retains an automatable managed-seam assertion.

## Rollout & Follow-up

- Rollout: single branch `bug/quickfiler-folder-dropdown-closes-on-open-796`, one pull request against main. No configuration change, no migration, no feature flag.
- The instrumentation added for AC6 is retained at Debug level after the fix lands; it is the only diagnostic in this pipeline and its absence is what made this defect unobservable.
- Post-fix follow-up: if the AC6 evidence refutes candidate 3 as expected but the mouse-path latch gap remains, AC4 is still delivered in this change; no separate issue is required.
- Links: issue #796 at https://github.com/drmoisan/TaskMaster/issues/796; the research artifact and the runbook cited above; related prior work #438, #677, #680.
