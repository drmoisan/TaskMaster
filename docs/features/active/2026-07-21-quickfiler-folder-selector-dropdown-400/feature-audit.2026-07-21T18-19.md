# Feature Audit: QuickFiler Folder Selector Drop-Down (#400)

**Audit Date:** 2026-07-21
**Feature Folder:** docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400
**Base Branch:** main
**Head Branch:** bug/quickfiler-folder-selector-dropdown-400
**Work Mode:** full-bug
**Audit Type:** Initial acceptance review

## Scope and Baseline

- **Base branch:** main at df5ad49c909f6b739edef45d0336151f44e827a6
- **Head branch/commit:** bug/quickfiler-folder-selector-dropdown-400 at b38a87751669f3522928dd01ac0f4f97b82572ed
- **Merge base:** df5ad49c909f6b739edef45d0336151f44e827a6
- **Evidence sources:**
  - Primary: artifacts/pr_context.summary.txt and artifacts/pr_context.appendix.txt
  - Secondary baseline diff: git diff df5ad49c909f6b739edef45d0336151f44e827a6...HEAD
  - Feature evidence: docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence
  - Review evidence: policy-audit.2026-07-21T18-19.md and code-review.2026-07-21T18-19.md
- **Feature folder used:** docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400
- **Requirements source:** spec.md
- **Work mode resolution note:** issue.md explicitly records full-bug; spec.md is therefore the authoritative checkbox source.
- **Scope note:** The review covers the complete merge-base diff and the exact HEAD. The PR context is fresh for the reviewed base/head pair.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**

- docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md — only authoritative acceptance-criteria source

### Acceptance criteria

1. AC-1: With a scored folder selected, the collapsed selector renders exactly one data row, that row is the committed selected folder, and its displayed percentText equals the existing PercentageFormatter output for the supplied normalized FolderRow.Score; the selector performs no probability recomputation or renormalization.
2. AC-2: The collapsed page exposes no vertical scrollbar, spinner, or scroll arrows and contains exactly one drop-down button with an accessible name, aria-haspopup="listbox", and aria-expanded matching the host open state.
3. AC-3: Button activation and SetFolderDroppedDown(true) open a native ToolStripDropDown/ToolStripControlHost popup over ItemViewer sibling controls; while open it remains owned by and above that ItemViewer and is never configured as a global/system-wide topmost window.
4. AC-4: Placement uses the anchor's active monitor working area: the full desired height opens below when it fits; otherwise a full-height popup opens above when it fits; when neither fits, the side with more available space is chosen, an equal-space tie opens below, and size/location are clamped horizontally and vertically for primary, non-primary, and negative-coordinate monitor rectangles.
5. AC-5: While closed, Up and Down immediately commit the previous or next selectable folder, skip non-selectable rows, stop without wrapping at the first/last selectable row, publish at most one selection change, and never scroll the page.
6. AC-6: Opening snapshots the committed identity as original and initializes pending to it. While open, Up and Down change only pending, skip/clamp using the same selectable-row rules, keep the active option visible, and do not change the committed selection before commit.
7. AC-7: Enter and accessible/mouse row activation commit the pending row, publish the selection exactly once, close the popup, render the committed row in the collapsed control, and return focus to the collapsed selector or owning ItemViewer focus target.
8. AC-8: Escape, outside click, lost activation, and every other uncommitted automatic close restore the identity selected when the popup opened, publish no pending selection as committed, close cleanly, and return focus. A close after an explicit commit does not roll back that commit.
9. AC-9: Left and Right preserve the existing breadcrumb expand, collapse, and unhandled-key behavior in both view modes and do not mutate the committed/original/pending selector session.
10. AC-10: Immediate synchronous render, successful hierarchy resolution, unresolved key, empty resolved chain, and hierarchy-provider failure all retain the supplied score, stable row identity, and selection; only genuinely non-scored rows display no percentage.
11. AC-11: Issue #398 guarantees remain intact: no transient cleared or partially rebuilt model is observable, row replacement is atomic, readback remains pre-upgrade consistent while an upgrade is in flight, a host selection made after upgrade start survives replacement, and a stale completion cannot overwrite newer state.
12. AC-12: Both closed and popup WebView surfaces receive the same logical selector state with their respective view modes; each state update renders once per attached surface, and each event from either surface is routed once with no duplicate selection, open/close, or breadcrumb transition.
13. AC-13: Automated asset and host-seam tests prove light and dark theme state reaches both surfaces, the expanded list exposes listbox/option selection semantics, focus enters the pending option on open, and focus returns predictably on commit, cancellation, and initialization failure.
14. AC-14: The popup WebView is created lazily with the existing CoreWebView2Environment, reused rather than recreated for each open, and disposed/reset with ItemViewer; repeated pooled viewer reuse leaves one live subscription per surface, no orphan popup, and no callback after disposal.
15. AC-15: Empty/no-selectable state, selection -1, invalid selector messages, unknown keys, popup initialization failure, zero available placement space, repeated open/close, and provider failure are deterministic, preserve the last valid committed selection and any supplied scores, and do not throw or leak resources at the selector boundary.
16. AC-16: Deterministic failure-first MSTest evidence exists for the pre-fix defects and covers selection sessions, probability fallbacks, issue #398 concurrency, bridge serialization/routing, placement geometry, HTML/accessibility/theme contracts, popup ownership/focus, and lifecycle/reuse; each regression fails for the intended reason before implementation and passes afterward without sleeps, temporary files, external services, screenshots, or user interaction.
17. AC-17: Every added production and test .cs file is explicitly included in the applicable legacy .csproj; no new or modified production/test source file exceeds 500 lines; no hand-written runtime behavior is added to the already oversized generated ItemViewer.Designer.cs; no new external package or persisted configuration is introduced.
18. AC-18: One final uninterrupted C# toolchain pass succeeds in this exact order: csharpier format .; analyzer-enabled msbuild; nullable warnings-as-errors msbuild; and coverage-enabled vstest.console.exe for UtilitiesCS.Test.dll and QuickFiler.Test.dll. Repository-wide line coverage is at least 80%, every new class/method and new/changed selector type reaches at least 90%, and changed-line coverage does not regress, with numeric baseline/post-change/delta evidence.
19. AC-19: All existing breadcrumb, QuickFiler controller, UtilitiesCS, and issue #398 regression tests pass, and the full specified semantic contract is verified through automated host-neutral, bridge, asset-contract, and integration-seam tests. Pixel-identical cross-environment rendering is not required and is not treated as a blocker.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|---|
| 1 | Collapsed selected row and probability | PASS | BreadcrumbRenderProjectionSelectorTests and probability/router tests; measurable code is fully covered. | Final MSTest/coverage evidence | No recomputation was found. |
| 2 | Closed asset overflow and accessible arrow | PASS | FolderBreadcrumb.html and FolderBreadcrumbAssetContractTests. | Source and test inspection | Static asset contract is present. |
| 3 | Owned native popup | PASS | BreadcrumbDropDownHost and injected ownership/show seams. | Source inspection; issue-400 integrated suite | The native adapter is direct and not global topmost. |
| 4 | Active-monitor placement | PASS | BreadcrumbPopupPlacement and its geometry tests. | Issue-specific test evidence | Required fit, tie, clamp, and negative coordinates are covered. |
| 5 | Closed Up/Down commits | PASS | BreadcrumbSelectionSession and coordinator tests. | Issue-specific test evidence | Selectable-row clamping is covered. |
| 6 | Open pending navigation | PARTIAL | Host-neutral session behavior passes, but first-popup state/active-option delivery can be lost before page readiness; open-state Up composition is incomplete. | Source and test inspection | Runtime delivery is not reliable. |
| 7 | Commit and focus return | PARTIAL | Commit/callback seams pass, but a first popup can lack pending state and DOM focus because messages are sent before listener readiness. | Source and test inspection | Host callbacks do not prove page focus/state. |
| 8 | Uncommitted rollback | PASS | SelectionSession cancel and host native-close callbacks implement rollback. | Issue-specific test evidence | Add a composed pending-move/outside-close regression under remediation, but no contrary implementation was found. |
| 9 | Left/Right compatibility | PASS | Existing and added router/coordinator tests pass. | Issue-specific and full-suite evidence | No selector-session mutation was found. |
| 10 | Score-preserving fallback paths | PASS | Projection, coordinator probability, and router edge/in-flight tests. | Coverage and regression evidence | Immediate and upgrade/failure paths retain score and identity. |
| 11 | Issue #398 atomicity | PASS | Named predecessor regressions and router generation logic. | evidence/regression-testing/issue-398-regression.2026-07-21T17-08.md | Five named regressions pass within the full suite. |
| 12 | Consistent exactly-once multi-surface state | FAIL | Hub replays cached state on attachment before popup document readiness. | Host/hub/HTML source inspection | Initial state can be lost, so both surfaces are not guaranteed the same logical state. |
| 13 | Theme, accessibility, and focus proof | FAIL | Asset tests are source-only and popup theme/selector/focus messages can precede the page listener. | Test and source inspection | Automated proof is incomplete and production delivery is racy. |
| 14 | Lazy reuse, reset/disposal, no callbacks | FAIL | EnsureSurfaceAsync lacks in-flight serialization and lifecycle generation checks. | Host and lifecycle-test inspection | Reset/disposal during a pending factory can attach a stale surface. |
| 15 | Deterministic repeated lifecycle and failures | FAIL | Concurrent/repeated open while initialization is pending is unguarded. | Host and lifecycle-test inspection | Duplicate surface creation and post-disposal callbacks are possible. |
| 16 | Comprehensive deterministic failure-first evidence | FAIL | No page-ready or pending-factory concurrency regression; HTML tests do not execute page behavior. | Full changed-test inspection | Several specified composition scenarios are missing. |
| 17 | Project wiring, size, dependencies | PASS | All added .cs files are in legacy projects; maximum changed test file is 499 lines; maximum production file is 456 lines. | Project/diff/line-count inspection | No new package, persistence, or Designer runtime edit. |
| 18 | Final toolchain and numeric coverage | PARTIAL | Ordered final pass succeeds; repository is 84.1610%; measurable changed/new lines are 100%. Two new methods are nonnumeric. | Final QA and coverage-delta evidence | The recorded scope change is bounded but does not satisfy the literal every-new-method numeric clause. |
| 19 | Full semantic contract | FAIL | All tests pass, but the page-readiness and pending-initialization defects remain and required regressions are absent. | Full review | Passing tests do not establish the full contract. |

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**

- **PASS:** 10 criteria
- **PARTIAL:** 3 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 6 criteria

**Top gaps preventing PASS:**

1. Popup state and focus are replayed before the WebView document is confirmed ready.
2. Surface initialization is not serialized or invalidated across concurrent open, reset, and disposal.
3. Deterministic tests do not cover these timing boundaries, and two new methods remain outside literal numeric coverage.

**Recommended follow-up verification steps:**

1. Add failure-first tests for page readiness and incomplete-factory lifecycle transitions, then implement the minimal fixes.
2. Add composed rollback and open-Up regressions, run the full ordered C# toolchain, regenerate coverage evidence, and repeat feature review.

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, PASS criteria remain checked. PARTIAL and FAIL criteria have been changed to unchecked in spec.md without altering their text.

### AC Status Summary

- Source: docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md
- Total AC items: 19
- Checked off (delivered): 10
- Remaining (unchecked): 9
- Items remaining: AC-6, AC-7, AC-12, AC-13, AC-14, AC-15, AC-16, AC-18, AC-19

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|---|---:|---:|---:|---|
| spec.md | 19 | 10 | 9 | Checkbox-backed authoritative source |
