# Feature Audit: QuickFiler Folder Selector Drop-Down (#400)

**Audit Date:** 2026-07-21
**Feature Folder:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`
**Base Branch:** `origin/main`
**Head Branch:** `bug/quickfiler-folder-selector-dropdown-400` plus current remediation worktree
**Work Mode:** full-bug
**Audit Type:** Post-remediation acceptance verification

## Scope and Baseline

- **Base branch:** `origin/main` at `fd9fb5ee1ca0c044b8dd0e02a81a22f58c6f3f68`
- **Head branch/commit:** `bug/quickfiler-folder-selector-dropdown-400` at `b38a87751669f3522928dd01ac0f4f97b82572ed`, plus tracked and untracked remediation changes
- **Merge base:** `df5ad49c909f6b739edef45d0336151f44e827a6`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`
  - Secondary baseline diff: `git diff --no-color df5ad49c909f6b739edef45d0336151f44e827a6 --`
  - Feature evidence: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence`
  - Review evidence: `policy-audit.2026-07-21T21-27.md` and `code-review.2026-07-21T21-27.md`
- **Feature folder used:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`
- **Requirements source:** `spec.md`
- **Work mode resolution note:** `issue.md` explicitly records `full-bug`; `spec.md` is therefore the authoritative checkbox source.
- **Scope note:** The review covers the merge-base range and all current remediation changes. `origin/main` advanced after the branch was created through an unrelated archive merge; the feature merge base remains `df5ad49`. Incoming base-only archive moves were excluded from the feature scope.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**

- `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md` — only authoritative acceptance-criteria source

### Acceptance criteria

1. AC-1: With a scored folder selected, the collapsed selector renders exactly one data row, that row is the committed selected folder, and its displayed `percentText` equals the existing `PercentageFormatter` output for the supplied normalized `FolderRow.Score`; the selector performs no probability recomputation or renormalization.
2. AC-2: The collapsed page exposes no vertical scrollbar, spinner, or scroll arrows and contains exactly one drop-down button with an accessible name, `aria-haspopup="listbox"`, and `aria-expanded` matching the host open state.
3. AC-3: Button activation and `SetFolderDroppedDown(true)` open a native `ToolStripDropDown`/`ToolStripControlHost` popup over `ItemViewer` sibling controls; while open it remains owned by and above that `ItemViewer` and is never configured as a global/system-wide topmost window.
4. AC-4: Placement uses the anchor's active monitor working area: the full desired height opens below when it fits; otherwise a full-height popup opens above when it fits; when neither fits, the side with more available space is chosen, an equal-space tie opens below, and size/location are clamped horizontally and vertically for primary, non-primary, and negative-coordinate monitor rectangles.
5. AC-5: While closed, Up and Down immediately commit the previous or next selectable folder, skip non-selectable rows, stop without wrapping at the first/last selectable row, publish at most one selection change, and never scroll the page.
6. AC-6: Opening snapshots the committed identity as `original` and initializes `pending` to it. While open, Up and Down change only `pending`, skip/clamp using the same selectable-row rules, keep the active option visible, and do not change the committed selection before commit.
7. AC-7: Enter and accessible/mouse row activation commit the pending row, publish the selection exactly once, close the popup, render the committed row in the collapsed control, and return focus to the collapsed selector or owning `ItemViewer` focus target.
8. AC-8: Escape, outside click, lost activation, and every other uncommitted automatic close restore the identity selected when the popup opened, publish no pending selection as committed, close cleanly, and return focus. A close after an explicit commit does not roll back that commit.
9. AC-9: Left and Right preserve the existing breadcrumb expand, collapse, and unhandled-key behavior in both view modes and do not mutate the committed/original/pending selector session.
10. AC-10: Immediate synchronous render, successful hierarchy resolution, unresolved key, empty resolved chain, and hierarchy-provider failure all retain the supplied score, stable row identity, and selection; only genuinely non-scored rows display no percentage.
11. AC-11: Issue #398 guarantees remain intact: no transient cleared or partially rebuilt model is observable, row replacement is atomic, readback remains pre-upgrade consistent while an upgrade is in flight, a host selection made after upgrade start survives replacement, and a stale completion cannot overwrite newer state.
12. AC-12: Both closed and popup WebView surfaces receive the same logical selector state with their respective view modes; each state update renders once per attached surface, and each event from either surface is routed once with no duplicate selection, open/close, or breadcrumb transition.
13. AC-13: Automated asset and host-seam tests prove light and dark theme state reaches both surfaces, the expanded list exposes listbox/option selection semantics, focus enters the pending option on open, and focus returns predictably on commit, cancellation, and initialization failure.
14. AC-14: The popup WebView is created lazily with the existing `CoreWebView2Environment`, reused rather than recreated for each open, and disposed/reset with `ItemViewer`; repeated pooled viewer reuse leaves one live subscription per surface, no orphan popup, and no callback after disposal.
15. AC-15: Empty/no-selectable state, selection `-1`, invalid selector messages, unknown keys, popup initialization failure, zero available placement space, repeated open/close, and provider failure are deterministic, preserve the last valid committed selection and any supplied scores, and do not throw or leak resources at the selector boundary.
16. AC-16: Deterministic failure-first MSTest evidence exists for the pre-fix defects and covers selection sessions, probability fallbacks, issue #398 concurrency, bridge serialization/routing, placement geometry, HTML/accessibility/theme contracts, popup ownership/focus, and lifecycle/reuse; each regression fails for the intended reason before implementation and passes afterward without sleeps, temporary files, external services, screenshots, or user interaction.
17. AC-17: Every added production and test `.cs` file is explicitly included in the applicable legacy `.csproj`; no new or modified production/test source file exceeds 500 lines; no hand-written runtime behavior is added to the already oversized generated `ItemViewer.Designer.cs`; no new external package or persisted configuration is introduced.
18. AC-18: One final uninterrupted C# toolchain pass succeeds in this exact order: `csharpier format .`; analyzer-enabled `msbuild`; nullable warnings-as-errors `msbuild`; and coverage-enabled `vstest.console.exe` for `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`. Repository-wide line coverage is at least 80%, every measurable new or changed selector type and member reaches at least 90%, and changed-line coverage does not regress, with numeric baseline/post-change/delta evidence. Only direct WebView2/WinForms adapter calls and unavoidable navigation-readiness coordination and cleanup may be classified as bounded nonnumeric surfaces, and every such surface must be enumerated and verified through deterministic injected seams; no numeric threshold, filter, or exclusion is waived or widened.
19. AC-19: All existing breadcrumb, QuickFiler controller, UtilitiesCS, and issue #398 regression tests pass, and the full specified semantic contract is verified through automated host-neutral, bridge, asset-contract, and integration-seam tests. Pixel-identical cross-environment rendering is not required and is not treated as a blocker.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|---|
| 1 | Collapsed committed scored row and probability | FAIL | Duplicate suggestion/recent paths share one identity; collapsed identity selection chooses the first match. The collapsed surface also replays before document readiness. | `git diff --no-color df5ad49 -- FolderPredictor.cs FolderBreadcrumbBridgeRouter.cs BreadcrumbSelectionSession.cs FolderBreadcrumb.html ItemViewer.Breadcrumb.cs` | Existing unique-row probability tests pass but do not cover the failing runtime cases. |
| 2 | Closed overflow and accessible toggle | PASS | `FolderBreadcrumbAssetContractTests` verifies hidden overflow/scroll controls, exactly one accessible toggle, and `aria-expanded`. | Final coverage wrapper; asset source inspection | 12/12 asset contracts pass. |
| 3 | ItemViewer-owned native popup | PASS | Host uses `ToolStripDropDown`/`ToolStripControlHost`, anchor-relative show, and no global topmost mode. | Host/integration test inspection | Ownership requirement is met. |
| 4 | Monitor-aware placement | PASS | Geometry tests cover below, above, more-space, equal tie, clamp, primary/non-primary, negative coordinates, and zero space. | Final coverage wrapper; `BreadcrumbPopupPlacementTests` | Placement is 44/44 measured lines. |
| 5 | Closed Up/Down commit behavior | FAIL | Duplicate identities make navigation repeatedly resolve from the first matching row and can prevent progression/clamping. | Source trace through session `IndexOfSelectable`; no duplicate-identity test exists | Unique-identity scenarios pass only. |
| 6 | Open pending navigation | FAIL | Duplicate identities mark multiple options active and make pending movement ambiguous; session mutation also races async row replacement. | Source trace across session/router/coordinator/asset | Pending state is not reliable for all valid folder sets. |
| 7 | Enter/mouse activation commit exactly once | FAIL | Activating a duplicate recent row commits the first duplicate; subfolder click raises selection without closing/committing the selector session. | Source trace across asset/coordinator/session | The notified selection can later be replaced or rolled back. |
| 8 | Uncommitted close rollback/focus | FAIL | `Close` ignores a pending `_openTask`; readiness can later show/focus the popup before ItemViewer dismisses it. | Source trace through host `OpenAsync`/`Close` and ItemViewer open pipeline | Open-state close paths pass, pending-state close does not. |
| 9 | Left/Right compatibility | PASS | Existing router/coordinator/asset tests preserve breadcrumb transitions without selector-session mutation. | Final coverage wrapper; test/source inspection | No contrary path was found. |
| 10 | Score, identity, and selection across hierarchy outcomes | FAIL | Probability preservation passes, but duplicate path identities can move selection from a recent row to the first scored duplicate during identity reconciliation. Async posts can also violate WebView thread affinity. | Source trace and final probability evidence | Score calculation is correct; stable selection/identity is not complete. |
| 11 | Issue #398 atomicity and in-flight host selection | FAIL | New selector-session/direct item mutations bypass the router lock; an upgrade can restore its captured old identity after a user move. | Source interleaving; inspect `FolderBreadcrumbBridgeRouterInFlightTests` | Existing test covers only locked `router.SelectRow`. |
| 12 | Same state/exactly-once two-surface delivery | FAIL | Collapsed replay is not ready-gated; stale coordinator upgrades always post current state and can duplicate a newer update; async posts can occur off the UI thread. | Source trace through hub/coordinator/ItemViewer | Popup ready replay alone does not satisfy both surfaces. |
| 13 | Theme, accessibility, and focus proof | FAIL | Initial collapsed theme can be lost before readiness; duplicate identities can produce multiple active/`aria-selected` options. | Asset and persistent-surface source inspection | Static unique-row contracts do not prove these runtime cases. |
| 14 | Lazy reuse/reset/disposal/no callbacks | FAIL | Popup host reset/disposal races pass, but coordinator upgrades are not canceled or suppressed and can post after pooled reset/disposal. | Source trace through coordinator/ItemViewer/hub | The no-callback-after-disposal requirement is not met. |
| 15 | Deterministic edge/failure/repeated lifecycle | FAIL | Duplicate rows, close while open is pending, off-thread post, and late upgrade completion are uncovered and not deterministic at the boundary. | Full changed-test inventory and source inspection | Existing listed edge cases do not cover these compositions. |
| 16 | Failure-first comprehensive regression evidence | FAIL | No fail-before/pass-after evidence exists for the seven Major findings. | Inspect `evidence/regression-testing` and 19 changed test classes | Existing evidence covers earlier findings only. |
| 17 | Project wiring, file sizes, dependency/config scope | PASS | Every added source has one legacy-project include; host 484, helper 118, largest test 500; no Designer/package/persisted-config change. | `evidence/qa-gates/file-size-and-project-includes.2026-07-21T20-19.md`; source inspection | Structural requirements pass. |
| 18 | Ordered final QA and numeric coverage | PASS | One uninterrupted run passed; repository 84.1647%; modified hunks 100%; changed/new measurable 99.8250%; every measurable selector type/member exceeds 90%; bounded adapters enumerated. | Final format/analyzer/nullable/test artifacts and `coverage-delta.2026-07-21T21-18.md` | The quality-gate criterion passes even though behavioral review finds defects. |
| 19 | Full regression suite and semantic contract | FAIL | All 5,849 tests pass, but seven material selector/readiness/lifecycle semantics are absent from automated verification and fail source review. | Final coverage wrapper; code review findings | Passing tests do not establish the full contract. |

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**

- **PASS:** 6 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 13 criteria

**Top gaps preventing PASS:**

1. Row identity is not unique across suggestions/recents, breaking navigation, activation, collapsed selection, and accessibility.
2. WebView message delivery does not consistently honor UI-thread and navigation-readiness requirements.
3. Router/session/coordinator lifecycle ownership is incomplete across async replacement, reset/disposal, pending close, and subfolder activation.

**Recommended follow-up verification steps:**

1. Execute the generated remediation plan with failure-first tests for all seven findings.
2. Rerun the full ordered C# toolchain and numeric coverage comparison, reconcile `spec.md`, and repeat independent review.

## Acceptance Criteria Check-off

This review was explicitly read-only for production code, tests, `spec.md` checkboxes, the executed plan, and prior 18-19 audit artifacts. No acceptance-criteria checkbox was modified.

The current source has all 19 boxes checked, but this review supports only AC-2, AC-3, AC-4, AC-9, AC-17, and AC-18. The 13 FAIL criteria must be reconciled to unchecked by the downstream remediation workflow before execution and may be checked again only after verified delivery.

### AC Status Summary

- Source: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`
- Total AC items: 19
- Current source checkboxes: 19 checked, 0 unchecked
- Review-supported delivered items: 6
- Review-supported remaining items: 13
- Items remaining: AC-1, AC-5, AC-6, AC-7, AC-8, AC-10, AC-11, AC-12, AC-13, AC-14, AC-15, AC-16, AC-19

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 19 | 6 | 13 | Review-supported state. The file currently shows 19 checked; no edit was made because the review directive prohibited source checkbox changes. |
