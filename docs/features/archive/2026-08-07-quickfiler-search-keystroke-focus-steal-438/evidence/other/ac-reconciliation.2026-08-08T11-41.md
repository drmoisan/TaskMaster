# [P7-T1…P7-T15] Acceptance Criteria Reconciliation

- **Issue:** #438
- **Tasks:** [P7-T1] through [P7-T15]
- **Timestamp:** 2026-08-08T11-41
- **AC source:** `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md` § Acceptance Criteria (work mode `full-bug`, so `spec.md` only)

## Command

Verification of each criterion against the evidence artifacts on disk, followed by a single checkbox flip per criterion in `spec.md`.

- **EXIT_CODE:** 0

## Per-criterion verification

### [P7-T1] AC-1 — controller seam issues the intent once, no focus transfer, no committed-selection change

- **Evidence:** `evidence/regression-testing/fail-before.2026-08-08T11-41.md`, `fail-before-controller.2026-08-08T11-41.md`, `pass-after.2026-08-08T11-41.md`
- **Verified:** fail-before at P1-T2 (EXIT 1, 4 of 5 failing with `SetFolderDroppedDown` / `SetFolderSelectedIndex` each invoked once on a byte-clean production tree); second fail-before at P5-T2 (EXIT 1, 6 failing, including the positive `PresentFolderSearchResults` intent assertion at 0 invocations); pass-after at P5-T4 (EXIT 0, 180/180). `TextBoxSearch_TextChanged` now reduces to `FindFolder` plus exactly one `_itemViewer.PresentFolderSearchResults(folders)` call.
- **Status: PASS — checked off.**

### [P7-T2] AC-2 — search-driven open takes no focus; gesture open still focuses once

- **Evidence:** `evidence/other/p3-gate.2026-08-08T11-41.md` (P3-T6 host run), `evidence/other/p4-gate.2026-08-08T11-41.md` (P4-T4 integration run), `evidence/qa-gates/test-coverage-final.2026-08-08T11-41.md` (full suite)
- **Verified:** in the real-host delegate-count harness, `OpenAsync_FreshOpenWithoutFocus_InvokesNeitherFocusDelegate` asserts `FocusPendingCount == 0` **and** `FocusAnchorCount == 0`; `OpenAsync_ThreeParameterOverload_StillFocusesPendingExactlyOnce` and `OpenAsync_FreshOpenWithFocus_InvokesFocusPendingExactlyOnce` assert `FocusPendingCount == 1` for a default open. At the integration seam, `PresentFolderSearchResults_OnAClosedSelector_OpensOnceWithoutFocus` asserts one 4-parameter `OpenAsync(..., false)` and zero 3-parameter opens.
- **Status: PASS — checked off.**

### [P7-T3] AC-3 — two consecutive refreshes produce one `OpenAsync` and zero `Close`

- **Evidence:** `evidence/other/p4-gate.2026-08-08T11-41.md`, `evidence/regression-testing/fail-before-exception.2026-08-08T11-41.md`
- **Verified:** `PresentFolderSearchResults_TwoConsecutiveRefreshes_OpenOnceAndNeverClose` asserts `OpenAsync` `Times.Once()` and `Close` `Times.Never()`; `PresentFolderSearchResults_KeystrokeByKeystroke_OpensOnceAndTracksEveryRefresh` extends this to four keystrokes. The dossier records why a pre-fix failing run is impossible (the members did not exist) and supplies the alternative proof.
- **Status: PASS — checked off.**

### [P7-T4] AC-4 — highlight changes only `PendingIdentity`, no `SelectionChanged`, committed model untouched

- **Evidence:** `evidence/other/p2-gate.2026-08-08T11-41.md` (P2-T3 run), `evidence/other/p4-gate.2026-08-08T11-41.md` (P4-T4 run)
- **Verified:** `HighlightRow_OpenSession_ChangesOnlyPendingIdentity` asserts effects are exactly `Handled | RenderRequired`, `CommittedIdentity` and `OriginalIdentity` unchanged, and `model.SelectedIndex` unmoved; `HighlightRow_OpenSession_PublishesNoSelectionOrOpenStateChange` asserts neither flag is set. Through the viewer seam, `PresentFolderSearchResults_PublishesNoSelectionChangeAndKeepsCommittedFolder` counts zero `SelectionChanged` events across two refreshes.
- **Status: PASS — checked off.**

### [P7-T5] AC-5 — Escape restores the pre-search identity; controller cache holds no mid-search value

- **Evidence:** `evidence/other/p2-gate.2026-08-08T11-41.md` (session half), P5-T5 run recorded in `evidence/qa-gates/test-coverage-final.2026-08-08T11-41.md` (controller cache)
- **Verified:** `Cancel_AfterHighlight_RestoresThePreSearchCommittedIdentity` asserts the committed identity returns to the pre-search value with no `SelectionChanged`; `SearchThenCancel_LeavesTheCachedFolderAtThePreSearchCommittedValue` reads the controller's private `_selectedFolder` after three keystrokes and confirms it still equals the pre-search committed folder, with `SetFolderSelectedIndex` never invoked.
- **Status: PASS — checked off.**

### [P7-T6] AC-6 — a multi-character query reaches `SearchText` in full; rows reflect the complete query

- **Evidence:** P5-T5 run; `EightCharacterQueryTypedThroughTheSeam_DeliversTheFullTextAndCompleteRowSet` and `TextBoxSearch_TextChanged_PerKeystroke_QueriesTheCompleteSearchTextEachTime`
- **Verified:** the eight-character string "invoices" is delivered one character at a time through the real controller-to-viewer seam; all eight queries are observed (`*i*` … `*invoices*`), the final query carries the complete text, and `GetFolderItems()` equals the result set for the complete query. No truncation at one to two characters.
- **Status: PASS — checked off.**

### [P7-T7] AC-7 — explicit-gesture behavior unchanged and pinned

- **Evidence:** `evidence/regression-testing/pass-after.2026-08-08T11-41.md`, `evidence/other/scope-guard.2026-08-08T11-41.md`, `evidence/qa-gates/test-coverage-final.2026-08-08T11-41.md`
- **Verified:** `TextBoxSearch_KeyDown` proven **byte-identical** to HEAD by ordinal string comparison (338 characters); `QfcItemController.FolderHandling.cs` byte-unmodified; the pinned suites (`EventHandlersTests` Down-arrow pair, `NavigationTests`, `SeamDispatcherTests`, `FolderSuggestionsTests`, `FolderHandlingTests`, `BreadcrumbSelectorOpenRetryTests`) all pass in the 180-test P5-T4 run and the 6348-test P6-T5 run.
- **Status: PASS — checked off.**

### [P7-T8] AC-8 — one render per surface per state update

- **Evidence:** `evidence/other/p2-gate.2026-08-08T11-41.md` (P2-T4 run), `evidence/other/p4-gate.2026-08-08T11-41.md` (P4-T4 run)
- **Verified:** `ReplaceItemsPreservingSession_ReportsRenderRequiredOnly` asserts a single handled transition with `RenderRequired` and neither `SelectionChanged` nor `OpenStateChanged`. End to end, `PresentFolderSearchResults_RefreshWhileOpen_EmitsOneRenderPerSurface` counts the `"type":"render"` messages on both the collapsed and popup messengers and asserts each increases by exactly one per refresh. The composite publishes once even though it performs three router mutations.
- **Status: PASS — checked off.**

### [P7-T9] AC-9 — empty and banner-only result sets are deterministic no-ops

- **Evidence:** `evidence/other/p2-gate.2026-08-08T11-41.md` (P2-T3 and P2-T4 runs), `evidence/other/p4-gate.2026-08-08T11-41.md` (P4-T4 run)
- **Verified:** session level — `HighlightRow_EmptyRowSet_IsANoOpAndDoesNotThrow`, `HighlightRow_BannerOnlyRowSet_IsANoOpAndDoesNotThrow`, `HighlightRow_ClosedSession_IsADeterministicNoOp`, `HighlightRow_IndexBeyondTheLastRow_IsANoOp`. Router level — `ReplaceItemsPreservingSession_EmptyInput_IsDeterministicAndDoesNotThrow`, `ReplaceItemsPreservingSession_BannerOnlyInput_LeavesNothingSelectable`. Viewer level — `PresentFolderSearchResults_EmptyResultSet_DoesNotThrowOpenOrMutateSelection`, `PresentFolderSearchResults_BannerOnlyResultSet_DoesNotOpenOrHighlight` (both assert `OpenAsync` `Times.Never()`). No throw, no selection mutation, no open of a selector with no selectable rows.
- **Status: PASS — checked off.**

### [P7-T10] AC-10 — contract changes are additive only

- **Evidence:** `evidence/other/scope-guard.2026-08-08T11-41.md`, `evidence/other/p4-gate.2026-08-08T11-41.md`
- **Verified:** `git diff -U0` of `IItemViewer.cs` and `IBreadcrumbDropDownHost.cs` filtered for removed lines yields **zero**; exactly one new `IItemViewer` member and exactly one new `OpenAsync` overload, with the 3-parameter member delegating `takeFocus: true`. `ItemViewerBreadcrumbDropDownContractTests` passes unmodified. Deviation D11 (explicit interface implementation on the concrete host) changes no interface contract.
- **Status: PASS — checked off.**

### [P7-T11] AC-11 — #400 reconciliation holds; one sanctioned test-method modification

- **Evidence:** `evidence/qa-gates/test-coverage-final.2026-08-08T11-41.md`, `evidence/other/scope-guard.2026-08-08T11-41.md`, `evidence/other/p3-gate.2026-08-08T11-41.md`
- **Verified:** all #400 suites pass in the 6348-test run (`BreadcrumbDropDownIntegrationTests`, the default-open `BreadcrumbDropDownHostTests` cases, open-coordinator, lifecycle, session, router). Exactly one test method changed — every diff hunk in `QfcItemController.EventHandlersTests.cs` lies inside the original `:311-350` range. Sanctioned structural edits are limited to the three additive fake-implementer members plus one-token `partial` keywords; a targeted diff audit of those four files returns **0** changed test-assertion lines. The gesture-scoped qualification of #400 AC-13 is recorded in `spec.md`.
- **Status: PASS — checked off.**

### [P7-T12] AC-12 — one uninterrupted toolchain pass; coverage thresholds

- **Evidence:** `evidence/baseline/test-coverage-baseline.2026-08-08T11-41.md`, `evidence/qa-gates/final-format.2026-08-08T11-41.md`, `final-analyze.2026-08-08T11-41.md`, `final-nullable.2026-08-08T11-41.md`, `test-coverage-final.2026-08-08T11-41.md`, `coverage-delta.2026-08-08T11-41.md`
- **Verified:** the final pass ran in order — csharpier format/check EXIT 0, analyzer msbuild EXIT 0 with 0 errors, nullable warnings-as-errors msbuild EXIT 0 with 0 errors, coverage-enabled vstest EXIT 0 with 6348/6348 passing. Every measurable new/changed member reaches at least 95.24% line coverage (twelve of fourteen at 100%); changed-line coverage improved (`BeginOpenCore` 93.75% -> 95.24%, its single uncovered line proven pre-existing); repository-wide line coverage rose 0.858261 -> 0.858665 and branch 0.792082 -> 0.792502. Baseline and post-change figures are recorded under `<FEATURE>/evidence/`.
- **Status: PASS — checked off.**

### [P7-T13] AC-13 — EfcViewer search path unmodified

- **Evidence:** `evidence/other/scope-guard.2026-08-08T11-41.md`
- **Verified:** `git diff -- QuickFiler/Controllers/EfcFormController.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` produced **empty output**, and neither file appears in `git status --porcelain`. Re-verified after formatting in P6-T1. `SearchText_TextChanged`, `BindFolderRows`, and `BindBreadcrumbRowsAsync` are byte-unmodified.
- **Status: PASS — checked off.**

### [P7-T14] AC-14 — `<Compile Include>` wiring, 500-line ceiling, no new package or config

- **Evidence:** `evidence/qa-gates/wiring-audit.2026-08-08T11-41.md`, `evidence/qa-gates/file-size-audit.2026-08-08T11-41.md`
- **Verified:** all 13 new `.cs` files have an exact matching `<Compile Include>` entry (zero missing), each verified by exact-string match against its owning legacy `.csproj`. The authoritative post-format size audit enumerates 30 added or modified `.cs` files, **all at or under 500 lines** (largest 499); the one violation found (502 lines) was remediated by extracting a partial and the loop restarted. No new external package and no persisted configuration: `packages.config`, `app.config`, and `.runsettings` are unmodified.
- **Status: PASS — checked off.**

## [P7-T15] Reconciliation

`spec.md` § Acceptance Criteria state after check-off:

```
- [x] AC-1   - [x] AC-2   - [x] AC-3   - [x] AC-4   - [x] AC-5
- [x] AC-6   - [x] AC-7   - [x] AC-8   - [x] AC-9   - [x] AC-10
- [x] AC-11  - [x] AC-12  - [x] AC-13  - [x] AC-14
- [ ] HV-1  (unchecked by design)
```

- **AC-1…AC-14: all 14 are `[x]`**, each with at least one evidence artifact resolving on disk (enumerated above).
- **HV-1 remains `[ ]` by design.** It is the documented human-verification exception and explicitly **not a merge gate** (`spec.md` § Automation feasibility; plan § HV-1). It is discharged post-fix per `runbooks/verify-search-focus-retention.runbook.md`; a negative outcome is promoted as a new issue rather than reopening #438.

### Spec integrity after edit

Only the 14 `- [ ]` -> `- [x]` markers changed. Criterion text is untouched: the AC-12 line still contains its three `→` characters, the file retains its original encoding (no BOM introduced), and no AC item was added or removed.

## Result

- **Output Summary:** All 14 gating acceptance criteria (AC-1 through AC-14) were individually verified against evidence artifacts on disk and checked off in `spec.md`. HV-1 remains unchecked by design as a non-gating post-fix human verification. No criterion text was altered and no phantom criterion was added. Accept criteria met.
