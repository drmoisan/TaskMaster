# quickfiler-search-box-loses-focus-on-dropdown-expand (Spec)

- **Issue:** #680
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-28T12-56
- **Status:** Draft
- **Version:** 0.1

## Context
In QuickFiler's folder search box, typing a character correctly auto-opens/expands the search results drop-down, but the search box then loses keyboard focus, so no further characters can be typed until the user manually closes the drop-down and refocuses the search box — making auto-open effectively unusable for multi-character searches.

Environment:
- OS/version: Windows (VSTO add-in host, Outlook desktop)
- Component: QuickFiler folder search box (`QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`)
- Data source or fixture: N/A (manual interactive repro)

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Launch Outlook with the TaskMaster VSTO add-in loaded and run QuickFiler against a mail item.
2. Click into (or navigate to) a QuickFiler folder search box.
3. Type a single character. The search results drop-down auto-opens/expands as expected.
4. Attempt to type a second character to continue/narrow the search.

Expected:
Typing should be able to continue uninterrupted while the search drop-down is open, letting the user type a full multi-character search term and see the results narrow live.

Actual:
After the first character opens the drop-down, the search box loses keyboard focus. Additional keystrokes are not received by the search box. The user must close the drop-down and click back into the search box to type again, which reopens the drop-down after only one more character — making it effectively impossible to enter a multi-character search term through normal typing.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: N/A — behavioral repro, no exception/log signature identified yet.


## Scope & Non-Goals
- In scope:
  - The `AutoClose` toggle in `BreadcrumbDropDownHost`, keyed off the existing `takeFocus` intent: set `AutoClose = false` before show for non-focusing search-driven opens (suppressing WinForms menu-mode entry), and restore `AutoClose = true` on gesture/focusing opens and on close completion.
  - The new managed dismissal ownership that replaces what `AutoClose = true` previously provided during a non-capturing open: close the search popup when the search textbox loses focus to anything other than the popup surface, and route Escape-while-typing to the existing cancel path (`CancelSelector`).
- Out of scope / non-goals:
  - No change to #438's managed focus pipeline (latch, `takeFocus` threading, `FocusPending` skip). It is already correct, and its regression suites must remain green unmodified.
  - No reflection-based `ToolStripManager.ModalMenuFilter.ExitMenuMode` workaround (internal framework API; brittle).
  - No rewrite of the popup off `ToolStripDropDown` to a borderless `Form`. The research notes this as a viable long-term follow-up only, not part of this fix (see Rollout & Follow-up).
- Explicitly excluded systems, integrations, or datasets:
  - Gesture-driven opens (Down arrow, mouse toggle, row click): their focus-on-open and auto-close-on-outside-click semantics are unchanged.
  - Any QuickFiler subsystem that does not touch the search-box open/dismiss lifecycle.

## Root Cause Analysis

Full findings: `research/2026-08-28T11-00-quickfiler-search-box-focus-loss-680-research.md`.

**Root cause: WinForms `ModalMenuFilter` menu-mode keyboard capture, engaged as a side effect of showing the breadcrumb results `ToolStripDropDown` popup — not a managed focus call, not a WebView2 focus grab, and not a regression of #438's fix.**

The managed keystroke-to-open pipeline is fully non-focusing and #438's fix is intact and verified end-to-end (`QfcItemController.EventHandlers.cs:173-182` → `BreadcrumbItemViewerLifecycleCoordinator.Search.cs:34-43` latch → `BreadcrumbBridgeCoordinator.Search.cs:47-100` → `BreadcrumbDropDownOpenCoordinator.cs` `RequestOpen`/`BeginOpenCore` → `BreadcrumbDropDownHost.Open.cs:52-72` with `takeFocus: false` → `BreadcrumbDropDownOpenLifetime.Focus.cs:32-51` skips `FocusPending`). No managed code moves Win32 focus off the search textbox.

The popup itself is a `ToolStripDropDown` constructed with `AutoClose = true` (`BreadcrumbDropDownHost.cs:165-171`) and shown via `DropDown.Show(anchor, point)` (`BreadcrumbPopupUiOperations.cs:101-105`). WinForms' `ToolStripDropDown.SetVisibleCore(true)` unconditionally calls `ToolStripManager.ModalMenuFilter.SetActiveToolStrip(this)` for a top-level dropdown, entering menu mode unless `AutoClose` is `false` (verified verbatim against `dotnet/winforms` source). Menu mode installs a message filter whose keyboard handling contains: `if (!activeToolStrip.ContainsFocus) { m.HWnd = activeToolStrip.Handle; }` — the popup shows `SW_SHOWNOACTIVATE` so Win32 focus genuinely stays in the textbox, which means `ContainsFocus` is false, which means **every subsequent keystroke is retargeted to the dropdown's window handle and never reaches the textbox**. The #438 fix's very success (focus stays in the textbox) is what activates the retargeting branch. When the user clicks back into the textbox, the click dismisses the `AutoClose = true` popup and exits menu mode; the next character reopens the popup and re-enters menu mode, producing the observed one-character-per-cycle loop.

**Relationship to #438 (archived):** not a regression. #438's spec explicitly predicted and deferred this exact gap as a documented, non-merge-gating residual risk (its Risks section: `AutoClose = true` "is expected to keep the non-activated popup open while the user types... not provable in a unit test"; its HV-1 manual-verification item: "type an eight-character folder name... confirm the caret never leaves the textbox," explicitly scoped as "a negative outcome is promoted as its own issue rather than blocking #438 delivery"). **#680 is that HV-1 negative outcome, now confirmed with a source-level mechanism.** #438's regression suites (`QfcItemController.SearchFocusRegressionTests.cs` and related) pin the managed seam below which this defect lives and remain valid; they are not expected to change.

**Relationship to #677:** same problem family (keyboard delivery disrupted around the same `BreadcrumbDropDownHost`/popup machinery), distinct mechanism. #677's `MayTakeFocus` predicate gates *managed focus-taking calls*; menu-mode entry is not a focus call and is engaged inside the WinForms framework before any host code can intervene, so widening #677's guard cannot fix #680. #680 is the WinForms modal-menu-mode contributor that #677's own spec flagged under Rollout & Follow-up as "asserted, not verified" — this research verifies it at framework-source level and #680's fix discharges that follow-up item. **Planning/implementation note:** #677's fix (PR #684, not yet merged into this branch's base at research time) touches `BreadcrumbDropDownHost.cs`/`BreadcrumbDropDownHost.Open.cs`/`FinishClose`/`CompleteClose` in the same file region this fix will touch. The #680 implementation must compose with #677's `MayTakeFocus` guard rather than conflict with or bypass it — the atomic-planner must re-derive current line citations against whatever base the #680 branch is planned/executed on, not assume the citations below are still accurate if #677 has since merged into main.

Rejected mechanisms (with evidence in the research artifact): explicit coordinator-level focus moves, a first-open WebView2 focus grab as the primary/recurring cause (the surface is created once and reused, so it cannot explain the *recurring* per-reopen loop).

## Proposed Fix

### Design summary (what changes where):
Extend the existing `takeFocus: false` intent (already threaded through the #438 fix) to a "non-capturing open" that suppresses WinForms menu-mode entry via the framework's own opt-out: `ToolStripDropDown.AutoClose = false` skips menu mode entirely. No new framework-internal APIs, no popup architecture rewrite.

1. In `BreadcrumbDropDownHost`, for a `takeFocus: false` open, set `DropDown.AutoClose = false` before `ShowPopup` runs (menu mode is entered inside `SetVisibleCore(true)`, so the property must be set pre-`Show`).
2. Restore `AutoClose = true` at the two transitions where standard popup semantics must resume: (a) a focusing/gesture open (e.g. Down-arrow) or the already-open `takeFocus: true` branch; (b) close completion (`CompleteClose`/`FinishClose`), so the next lifecycle starts from the default.
3. Own the dismissal paths that `AutoClose = true` previously provided while the search popup is in the non-capturing state: close the search popup when the search textbox loses focus to anything other than the popup surface, and/or via the `Form.Deactivate` handler #677 introduces; route Escape-while-typing to the existing cancel path, since Escape no longer reaches the popup through menu mode.

### Boundaries and invariants to preserve:
- Explicit gestures (Down arrow, mouse toggle, row click) keep their current focus-on-open, auto-close-on-outside-click semantics (#400/#438).
- #438's regression suites (`SearchFocusRegressionTests`, `BreadcrumbDropDownSearchIntegrationTests`, `BreadcrumbDropDownOpenCoordinatorTests.Part3`, `BreadcrumbItemViewerLifecycleCoordinatorTests`) pass unmodified — this fix operates strictly below the managed seams they pin.
- Must not reintroduce an unconditional focus-taking path that #677's `MayTakeFocus` guard was added to prevent.

### Dependencies or blocked work:
Depends on the current state of `BreadcrumbDropDownHost.cs`/`.Open.cs` at implementation time; if #677 (PR #684) has merged into this branch's base by then, the fix must be written against and compose with its `MayTakeFocus` property and `Form.Deactivate` handler rather than the pre-#677 shape.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` / `BreadcrumbDropDownHost.Open.cs` — `AutoClose` toggle keyed off the `takeFocus` intent, restored on gesture/close-completion transitions.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` (or the search-box viewer's leave/Escape seam) — new managed dismissal ownership (textbox-leave close, Escape-to-cancel) to replace what `AutoClose = true` previously provided during a non-capturing open.

#### Functions/classes/CLI commands impacted:
`BreadcrumbDropDownHost.ShowPopup`/open path, `CompleteClose`/`FinishClose`, the Down-arrow gesture handler (`TextBoxSearch_KeyDown`), and whichever textbox-leave/Escape seam is chosen for dismissal ownership.

#### Data flow and validation changes:
None (focus/keyboard-routing behavior only; no data model impact).

#### Error handling and logging updates:
None required beyond existing patterns.

#### Rollback/feature-flag considerations (if applicable):
No feature flag; `AutoClose` defaults to its current `true` behavior outside the `takeFocus: false` branch, so a revert is a single-commit revert.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
No new public API surface expected; the `AutoClose` toggle is an internal implementation detail of `BreadcrumbDropDownHost`'s existing open/close lifecycle.

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
No breaking change; gesture-driven opens keep today's `AutoClose = true` behavior unchanged.

#### Performance constraints (latency/throughput/memory):
None beyond existing behavior.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The WinForms `ModalMenuFilter` menu-mode/`AutoClose` behavior verified against `dotnet/winforms` source is assumed to match this application's .NET Framework 4.8.1 runtime. Per the research artifact's net48-parity caveat (§7), this is asserted, not byte-verified; it is confirmed by the live-Outlook HV step in Test Strategy.
- Constraints (budget, performance, compatibility):
  - This repository's C# toolchain and coverage policy per CLAUDE.md: MSTest/Moq/FluentAssertions; CSharpier formatting; .NET analyzer diagnostics; nullable gate; >= 80% line coverage on the testable denominator per § UT2, with >= 90% for new modules/classes/methods.
  - #438's regression suites must remain green unmodified; this fix operates strictly below the managed seams they pin.
- External dependencies (services, libraries, releases):
  - The implementation must be planned and executed against the state of `BreadcrumbDropDownHost.cs`/`BreadcrumbDropDownHost.Open.cs` current at that time. If issue #677 (PR #684) has merged by then, the fix must compose with its `MayTakeFocus` property and `Form.Deactivate` handler rather than the pre-#677 code shape cited in the research artifact; line citations must be re-derived against the actual base.

## Data / API / Config Impact
- User-facing or API changes: none. No new public API surface — the `AutoClose` toggle is an internal implementation detail of `BreadcrumbDropDownHost`'s existing open/close lifecycle. Any new host member introduced for the toggle or dismissal ownership is additive and internal.
- Data or migration considerations: none (focus/keyboard-routing behavior only; no data model impact).
- Logging/telemetry updates (if any): none required beyond existing patterns.
- Compatibility notes (CLI flags, config schemas, versioning): none. No configuration keys, no feature flag; a revert is a single-commit revert.

## Test Strategy

The seeded issue items (unit coverage of search-box keystroke handling while the drop-down is open/auto-opening; multi-character continuous-typing integration retest; manual confirmation of live narrowing and unaffected Escape/commit/selection behavior) are discharged by the concrete strategy below, derived from the research artifact's Testing implications (§9). All tests use MSTest, Moq, and FluentAssertions per CLAUDE.md.

- Regression tests to add (deterministic, fail-before):
  - **Host seam (fail-before unit test).** Using the existing injected `_showPopup` host seam (internal constructor of `BreadcrumbDropDownHost`; the real-`ToolStripDropDown`-no-show harness pattern already used by `BreadcrumbDropDownHostTests`), assert `DropDown.AutoClose` is `false` at the moment the injected show delegate runs for a `takeFocus: false` open (**fails today: it is `true`**); is `true` for the 3-parameter (gesture) open; and is restored to `true` after `Close(...)` completes and after a `takeFocus: true` open on an already-open popup.
  - **Coordinator seam.** The non-capturing intent rides the existing latch: two consecutive search refreshes still produce one `OpenAsync` and zero `Close`, preserving #438 AC-3 unmodified.
  - **Controller seam.** New dismissal ownership: search-box leave and Escape each route exactly one close-or-cancel intent through the existing cancel path (`CancelSelector`); existing `SearchFocusRegressionTests` assertions remain green unmodified.
  - **Contract test.** Any new host member is additive; `ItemViewerBreadcrumbDropDownContractTests` passes unmodified (mirroring #438's AC-10 discipline).
- Edge cases and negative scenarios: leave/Escape with the popup already closed produces no spurious close/cancel intent; a gesture open immediately following a non-capturing open restores `AutoClose = true` before its show; close completion always restores `AutoClose = true` so the next lifecycle starts from the default.
- Error handling and logging verification: none beyond existing patterns (no new error paths or log statements are introduced).
- Coverage impact and targets: new/changed members target >= 90% line coverage; no coverage reduction on changed lines; repo-wide figure assessed against the testable denominator per CLAUDE.md § UT2.
- Suites that must pass unmodified: `QfcItemController.SearchFocusRegressionTests`, `BreadcrumbDropDownSearchIntegrationTests(.Part2)`, `BreadcrumbDropDownOpenCoordinatorTests.Part3`, `BreadcrumbItemViewerLifecycleCoordinatorTests`, `ItemViewerBreadcrumbDropDownContractTests`, and all #438/#400 suites named in the #438 spec's Test Strategy — this fix operates strictly below the seams they pin.
- Toolchain commands to run (in order, per CLAUDE.md):
  1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation (required): menu-mode engagement itself and live keyboard-message retargeting cannot be unit-tested (research artifact §8, Automation Feasibility — they require a real message pump, a real popup window, and a live WebView2, none of which are permitted or reliable in unit tests). **HV runbook item (mirroring #438's HV-1):** in a live Outlook session, type an eight-plus-character folder name at normal speed; confirm every character lands in the search box and the drop-down tracks each keystroke; then verify the gesture paths — Down-arrow handoff, outside-click dismissal, Escape — still behave per #400/#438.


## Acceptance Criteria

> This spec is the sole authoritative acceptance-criteria source for issue #680 (`- Work Mode: full-bug` in `issue.md`).

- [ ] AC-1: While typing in the search box, every keystroke is delivered to the textbox and the drop-down auto-opens/refreshes without capturing the keyboard, with caret and Win32 focus remaining in the textbox continuously — verified manually per the HV runbook item in Test Strategy (eight-plus-character folder name at normal speed in a live Outlook session), with the outcome recorded in the feature folder.
- [ ] AC-2: Gesture paths retain their current behavior unchanged: Down-arrow handoff, mouse toggle, row click, outside-click dismissal, and Escape behave per #400/#438 (verified in the same HV session).
- [x] AC-3: The fail-before host-seam regression test is added and passing: `DropDown.AutoClose` is `false` when the injected show delegate runs for a `takeFocus: false` open, `true` for a gesture (3-parameter) open, and restored to `true` after close completion and after a `takeFocus: true` open on an already-open popup (file path and test names listed in the delivery report). (Fail-before: `evidence/regression-testing/p2-t3-red-run-host.2026-08-28T15-30.md`; pass-after: `evidence/regression-testing/p3-t6-green-run-host.2026-08-28T15-57.md`.)
- [x] AC-4: Coordinator-, controller-, and contract-seam tests from Test Strategy are added and passing, including the dismissal-ownership edge cases (exactly one close-or-cancel intent per leave/Escape; no spurious intent when already closed; no state in which the popup is left un-dismissable). (Fail-before: `evidence/regression-testing/p2-t10-red-run-dismissal.2026-08-28T15-47.md`; pass-after: `evidence/regression-testing/p3-t9-green-run-dismissal.2026-08-28T16-02.md`; wiring tests from plan task P2-T7; additive-contract tests from plan task P2-T8.)
- [x] AC-5: #438's and #400's existing regression suites pass unmodified (`SearchFocusRegressionTests`, `BreadcrumbDropDownSearchIntegrationTests(.Part2)`, `BreadcrumbDropDownOpenCoordinatorTests.Part3`, `BreadcrumbItemViewerLifecycleCoordinatorTests`, `ItemViewerBreadcrumbDropDownContractTests`). (Unmodified proven by `evidence/qa-gates/p4-t1-pinned-diff.2026-08-28T16-05.md`; green run in `evidence/qa-gates/p4-t2-pinned-suites.2026-08-28T16-07.md` — 75 tests, 0 failures.)
- [x] AC-6: No unintended behavior changes outside the search-box open/dismiss lifecycle defined in Scope & Non-Goals. (Pinned files untouched per `evidence/qa-gates/p4-t1-pinned-diff.2026-08-28T16-05.md`. The complete diff footprint is twelve files, all inside the Scope & Non-Goals boundary: production — `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs`, `QuickFiler/Viewers/IItemViewer.cs`, `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, `QuickFiler/Controllers/QfcItemController.EventWiring.cs`; tests — `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs`, `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` (new), `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs`, `QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs` (new); build — `QuickFiler.Test/QuickFiler.Test.csproj` (two `Compile Include` entries). No gesture path, no #438 managed focus pipeline, and no QuickFiler subsystem outside the search-box open/dismiss lifecycle is modified.)
- [x] AC-7: Coverage: new/changed members reach >= 90% line coverage; no coverage reduction on changed lines; the repo-wide figure is recorded and assessed against the testable denominator per CLAUDE.md § UT2 (no baseline evidence was captured at spec time — if the raw figure is below the floor, record the pre-existing shortfall and demonstrate this change does not lower it). (Discharged by `evidence/qa-gates/p6-t5-coverage-delta.2026-08-28T16-20.md` and `evidence/qa-gates/p6-t4-coverage-final.2026-08-28T16-20.md`: all six changed members at 1.0000 line coverage against the 0.90 floor (6/6 found); all five changed measured files show final covered-line count >= baseline (5/5 found); repo-wide `line-rate` moved from 0.85269 to 0.85279, above the 80% floor, so there is no pre-existing shortfall and this change does not lower coverage.)
- [x] AC-8: Full toolchain pass completed in order (CSharpier format → analyzer rebuild → nullable rebuild → vstest with coverage) using the CLAUDE.md commands, with all steps passing in the final pass. (All four passed in a single restart-free pass — the second loop pass: `evidence/qa-gates/p6-t1-format.2026-08-28T16-20.md` (`PRE_FORMAT_CHECK_EXIT: 0`, post-check exit 0, porcelain identical), `evidence/qa-gates/p6-t2-analyzers.2026-08-28T16-20.md` (exit 0), `evidence/qa-gates/p6-t3-nullable.2026-08-28T16-20.md` (exit 0), `evidence/qa-gates/p6-t4-coverage-final.2026-08-28T16-20.md` (exit 0, 6839/6839 passed). The first pass restarted the loop because the formatter rewrote three files this plan had just authored; that pass is recorded in the P6-T1 artifact and does not count as final.)
- [x] AC-9: Docs updated to match the new behavior, including recording the discharge of #677's "WinForms modal-menu-mode contributor" follow-up item (see Rollout & Follow-up). No config references require updates (see Data / API / Config Impact).

### Acceptance Criteria Status — 2026-08-28

Recorded at the close of the #680 implementation plan
(`plan.2026-08-28T12-56.md`, v1.4). Source: this file, `spec.md` (Work Mode `full-bug`).

- Total AC items: **9**
- Checked off (delivered): **7** — AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9
- Remaining (unchecked): **2** — AC-1, AC-2

| AC | State | Basis |
|---|---|---|
| AC-1 | **[ ] unchecked — pending live-Outlook HV** | Not dischargeable by any automated test: menu-mode engagement and live keyboard-message retargeting need a real message pump, a real popup window, and a live WebView2. Runbook: `evidence/other/hv-runbook-680.2026-08-28T16-12.md` (items HV-1, HV-2). |
| AC-2 | **[ ] unchecked — pending live-Outlook HV** | Same runbook, items HV-3 through HV-9 (including the DR-8 Risk 1 post-handoff outside-click and DR-8 Risk 2 row-click cases). The automated half of AC-2 is pinned by `TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown` and the gesture-open host tests, which are green. |
| AC-3 | **[x] delivered** | Fail-before `evidence/regression-testing/p2-t3-red-run-host.2026-08-28T15-30.md`; pass-after `evidence/regression-testing/p3-t6-green-run-host.2026-08-28T15-57.md`. |
| AC-4 | **[x] delivered** | Fail-before `evidence/regression-testing/p2-t10-red-run-dismissal.2026-08-28T15-47.md`; pass-after `evidence/regression-testing/p3-t9-green-run-dismissal.2026-08-28T16-02.md`; plus the P2-T7 wiring and P2-T8 contract tests. |
| AC-5 | **[x] delivered** | `evidence/qa-gates/p4-t1-pinned-diff.2026-08-28T16-05.md` (nine pinned files byte-unmodified) and `evidence/qa-gates/p4-t2-pinned-suites.2026-08-28T16-07.md` (75 tests, 0 failures). |
| AC-6 | **[x] delivered** | `evidence/qa-gates/p4-t1-pinned-diff.2026-08-28T16-05.md` plus the twelve-file diff footprint enumerated on the AC-6 line. |
| AC-7 | **[x] delivered** | `evidence/qa-gates/p6-t5-coverage-delta.2026-08-28T16-20.md` — 6/6 members at 1.0000 against the 0.90 floor, 5/5 files non-regressing, repo-wide line-rate 0.85269 to 0.85279. |
| AC-8 | **[x] delivered** | Four final-pass artifacts under `evidence/qa-gates/`: `p6-t1-format`, `p6-t2-analyzers`, `p6-t3-nullable`, `p6-t4-coverage-final`, all exit 0 in one restart-free pass. |
| AC-9 | **[x] delivered** | This document's Rollout & Follow-up discharge record for issue #677's "WinForms modal-menu-mode contributor" item, plus the re-confirmed "no config updates" statement in Data / API / Config Impact. |

AC-1 and AC-2 are intentionally left unchecked. They may be checked only after the HV runbook is
executed in a live Outlook session and its outcome is recorded under `evidence/other/`.

## Risks & Mitigations
- Technical or operational risks:
  1. **Lifecycle-timing error in the `AutoClose` toggle.** Toggling at the wrong point in the open/close lifecycle could either fail to suppress menu mode (the property must be set before `Show`, because menu mode is entered inside `SetVisibleCore(true)`) or leave the popup un-dismissable in some transition.
  2. **Incomplete dismissal coverage.** Removing `AutoClose = true`'s automatic dismissal requires new managed dismissal paths (textbox-leave, Escape) that could miss an edge case and leave the popup stuck open.
  3. **net48 framework divergence.** The .NET Framework 4.8.1 `ModalMenuFilter` behavior could diverge from the `dotnet/winforms` source used for verification (the research artifact's stated caveat: parity asserted, not byte-verified).
- Mitigations and rollbacks:
  1. The fail-before host-seam unit test (Test Strategy) pins the exact `AutoClose` state at each transition: false when the show delegate runs for a non-focusing open, true for gesture opens, restored to true after close/gesture-handoff.
  2. Route dismissal through the existing, already-tested cancel path (`CancelSelector`) rather than inventing new dismissal logic; edge-case tests assert exactly one close-or-cancel intent and no stuck-open state.
  3. The live-Outlook HV step is the final confirmation, exactly as #438's HV-1 and #677 required (AC-1/AC-2).
  - Rollback: no feature flag needed; `AutoClose` defaults to today's `true` behavior outside the `takeFocus: false` branch, so a revert is a single-commit revert.

## Rollout & Follow-up
- Release/rollout steps: no phased rollout needed — a single PR delivers the fix, tests, and documentation.
- Post-fix monitoring or clean-up tasks:
  - This fix discharges the "WinForms modal-menu-mode contributor" follow-up item from issue #677's spec Rollout & Follow-up section (per the research artifact's Relationship to #677 finding: that contributor was "asserted, not verified" in #677 and is now verified at framework-source level and fixed here). Record the discharge in that item's tracking location when this fix merges (AC-9).
    Discharged by #680 on 2026-08-28 — see delivery-report
  - Cross-issue record location: no `docs/features/**/*677*` tracking folder exists in this worktree (verified by a `find` over `docs/features` at execution time, which returned no match). The discharge record is therefore carried by the rollout notes written in this feature folder and by the PR body for this change, not by an in-repo #677 folder.
  - Data / API / Config Impact re-confirmed at implementation time: the section's "no config updates" statement still holds. The delivered change introduces no configuration key, no feature flag, and no new public API surface — `SearchLeave` and `IsFolderDropDownOpen` are additive internal-to-the-add-in `IItemViewer` members, and `ShowPopup`'s new `takeFocus` parameter is on an `internal` method.
  - Follow-up note: the borderless-`Form` popup rewrite (replacing `ToolStripDropDown` entirely, so no menu filter is involved) was rejected as non-minimal for this fix but remains a viable long-term option if `AutoClose` toggling proves fragile in live verification. If the HV step surfaces fragility, promote that rewrite through the promotion lifecycle as its own issue.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/680
  - Research: `research/2026-08-28T11-00-quickfiler-search-box-focus-loss-680-research.md`
  - Related: issue #677 (PR #684, same host file region — composition dependency); archived #438 (`docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/`) whose HV-1 negative outcome this issue is.
