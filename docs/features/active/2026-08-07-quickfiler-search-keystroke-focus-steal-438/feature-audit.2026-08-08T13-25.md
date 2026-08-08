# Feature Audit — QuickFiler Search Keystroke Focus Steal (Issue #438)

- **Date:** 2026-08-08T13-25
- **Reviewer:** feature-review agent

## Scope and Baseline

- **Base branch:** `main` (resolved per `pr-base-branch-merge-base`; caller-supplied and independently recomputed via `git merge-base HEAD origin/main` — both yield `003c5715055d7d1933db68a742531332756e30b2`)
- **Branch head:** `bug/quickfiler-search-keystroke-focus-steal-438` @ `ff9d14ab32d7e6d25c1c1c5b9011ccf9ae6286f5` (matches the refreshed PR-context artifacts' recorded head)
- **Diff scope:** 76 files (30 `.cs`, 4 `.csproj`, 42 docs/evidence/agent-memory). Only C# has changed files.
- **Work mode:** `full-bug` (persisted `- Work Mode: full-bug` marker in `issue.md`). AC source: `spec.md` only — AC-1..AC-14 gating, HV-1 a documented non-gating human-verification exception. `user-story.md` is intentionally absent for this mode.
- **Primary evidence:** refreshed `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt`; executor evidence tree under `evidence/`; independent reviewer verification (diff inspection, Cobertura parsing, line counts, merge-base attribution checks) recorded in `policy-audit.2026-08-08T13-25.md`.

## Acceptance Criteria Inventory

| ID | Summary | Gating |
|---|---|---|
| AC-1 | Controller seam: presentation intent exactly once; no `SetFolderDroppedDown` / `FocusFolderDropDown` / `SetFolderSelectedIndex`; fails before, passes after | Yes |
| AC-2 | Search-driven open: zero `FocusPending` and zero `FocusAnchor` invocations; default (gesture) open: `FocusPending` exactly once | Yes |
| AC-3 | Two consecutive search refreshes: exactly one host `OpenAsync`, zero host `Close` | Yes |
| AC-4 | Highlight changes only `PendingIdentity`; no `SelectionChanged`; committed selection untouched | Yes |
| AC-5 | Escape during search restores pre-search committed identity; controller `_selectedFolder` cache not stale | Yes |
| AC-6 | Multi-character string through the viewer seam delivers the full string; row set reflects the complete query | Yes |
| AC-7 | Explicit-gesture behavior unchanged and pinned; named existing tests pass unmodified | Yes |
| AC-8 | Session-preserving row replacement emits exactly one render per surface per state update (#400 AC-12 preserved) | Yes |
| AC-9 | Empty and banner-only result sets are deterministic no-ops for the highlight step | Yes |
| AC-10 | Contract changes additive only: one `IItemViewer` member, one 4-parameter `OpenAsync` overload; contract tests pass unmodified | Yes |
| AC-11 | #400 reconciliation holds; single sanctioned test-method rewrite; enumerated structural edits only | Yes |
| AC-12 | Final uninterrupted toolchain pass; new/changed members >= 90%; changed-line coverage no regression; repo figure >= baseline; figures recorded in evidence | Yes |
| AC-13 | EfcViewer search path has zero diff | Yes |
| AC-14 | `<Compile Include>` wiring for every added `.cs`; no file over 500 lines; no new package/config | Yes |
| HV-1 | Live-Outlook eight-character typing check per runbook | No (documented human-verification exception; not a merge gate) |

## Acceptance Criteria Evaluation

| ID | Verdict | Independent verification |
|---|---|---|
| AC-1 | PASS | `QfcItemController.SearchFocusRegressionTests.cs` (8 tests) asserts intent-once and the three `Times.Never()` negatives; fail-before evidence shows 4-of-5 failing pre-fix with Moq `MockException` (EXIT 1); pass-after 180/180. Handler diff confirms the composition is reduced to `FindFolder` + `PresentFolderSearchResults(folders)`. |
| AC-2 | PASS | `BreadcrumbDropDownHostTests.Part2.cs`: `OpenAsync_FreshOpenWithoutFocus_InvokesNeitherFocusDelegate`, `OpenAsync_ReissuedWithoutFocusWhileOpen_DoesNotScheduleFocusPending`, `OpenAsync_FreshOpenWithFocus_InvokesFocusPendingExactlyOnce`, `OpenAsync_ThreeParameterOverload_StillFocusesPendingExactlyOnce` on the real-host delegate-count harness. Production guards verified in `BreadcrumbDropDownHost.Open.cs` and `BreadcrumbDropDownOpenLifetime.Focus.cs`. |
| AC-3 | PASS | `PresentFolderSearchResults_TwoConsecutiveRefreshes_OpenOnceAndNeverClose` (integration) and `LatchedOpen_ReachesTheHostOnceWithoutFocus` (coordinator); router-level `ReplaceItemsPreservingSession_TwoConsecutiveRefreshes_LeaveTheSessionOpen`. Root cause removed: the per-keystroke `ClearFolderItems()` session cancel is gone from the search path. |
| AC-4 | PASS | `HighlightRow_OpenSession_ChangesOnlyPendingIdentity` and `..._PublishesNoSelectionOrOpenStateChange` (session); `PresentFolderSearchResults_PublishesNoSelectionChangeAndKeepsCommittedFolder` (integration). `HighlightRow` implementation mutates only `PendingIdentity` and returns `Handled \| RenderRequired`. |
| AC-5 | PASS | `Cancel_AfterHighlight_RestoresThePreSearchCommittedIdentity` (session) and `SearchThenCancel_LeavesTheCachedFolderAtThePreSearchCommittedValue` (controller cache). |
| AC-6 | PASS | `EightCharacterQueryTypedThroughTheSeam_DeliversTheFullTextAndCompleteRowSet` (integration) and `TextBoxSearch_TextChanged_PerKeystroke_QueriesTheCompleteSearchTextEachTime` (controller). |
| AC-7 | PASS | The named existing test files (`QfcItemController.NavigationTests.cs`, `QfcItemController.SeamDispatcherTests.cs`) are absent from the branch diff; the Down-arrow tests in `QfcItemController.EventHandlersTests.cs:355-388` are outside the single rewritten method's hunk; `BreadcrumbSelectorOpenRetryTests` edits are the sanctioned additive fake member only. All pass in the final 6348/6348 run. `SetFolderDroppedDownTrue_StillUsesTheFocusingThreeParameterOverload` additionally pins the gesture path. |
| AC-8 | PASS | `PresentFolderSearchResults_RefreshWhileOpen_EmitsOneRenderPerSurface` (integration) and `ReplaceItemsPreservingSession_ReportsRenderRequiredOnly` (router); `PublishSearchPresentation` posts exactly one render and one selector-state message per composite call. |
| AC-9 | PASS | Empty/banner-only no-op tests at all three seams (controller `EmptyResult`, integration `EmptyResultSet`/`BannerOnlyResultSet`, session `EmptyRowSet`/`BannerOnlyRowSet`/`IndexBeyondTheLastRow`/`NegativeIndex`). |
| AC-10 | PASS | Interface diffs verified purely additive (+10/−0 and +26/−0; no existing member touched); 3-parameter `OpenAsync` delegates with `takeFocus: true`; `ItemViewerBreadcrumbDropDownContractTests.cs` is absent from the diff and passes in the final run. |
| AC-11 | PASS | Reconciliation table present in spec with the sanctioned gesture-scoped AC-13 qualification (assessed a documented refinement — see code review, Info finding); exactly one test method rewritten (verified by full test-diff review; assertions strengthened); structural edits limited to the three enumerated fakes plus two one-token `partial` keywords; all #400 suites green in the final run. |
| AC-12 | PASS | Final uninterrupted pass: csharpier EXIT 0 → analyzer msbuild EXIT 0 → nullable-as-errors msbuild EXIT 0 → coverage-enabled vstest 6348/6348 EXIT 0. Member minimum 95.24% (>= 90 gate); the single uncovered `BeginOpenCore` line independently proven pre-existing at baseline (line 187 baseline = line 221 post-offset); repo line 0.858261 → 0.858665 and branch 0.792082 → 0.792502, both not lower than baseline; baseline and post-change figures recorded under `evidence/`. Note: the separate repository per-file floor findings (R1/R2 in the policy audit) are policy-gate findings, not AC-12 failures — AC-12's member-level and no-regression clauses are satisfied. |
| AC-13 | PASS | `git diff --name-only 003c5715..HEAD` contains no `EfcFormController.cs` and no `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`; zero diff on the EfcViewer search path. |
| AC-14 | PASS | All 13 added `.cs` files matched 1:1 to new `<Compile Include>` entries in the four csproj diffs; reviewer line counts: maximum changed-file length 499; no new package reference, no configuration or persisted-state change. |
| HV-1 | UNVERIFIED (non-gating by spec) | Live-Outlook manual check deliberately outstanding; runbook exists at `runbooks/verify-search-focus-retention.runbook.md`; spec records that a negative outcome is promoted as a new issue rather than blocking #438. |

## Summary

All 14 gating acceptance criteria evaluate **PASS** on independent verification against the branch diff, the executor evidence tree, and reviewer-computed coverage figures. HV-1 remains deliberately outstanding as a documented, non-gating human-verification exception with a runbook. The defect's three verified mechanisms (open-side focus steal, close-side focus steal, committed-selection mutation) are each removed and each pinned by failing-first or new-seam regression tests.

Feature completeness is not the gating concern for PR readiness; the policy audit's R1 finding (new-file branch floor, blocking) is. See `policy-audit.2026-08-08T13-25.md` §8 and `remediation-inputs.2026-08-08T13-25.md`.

## Acceptance Criteria Check-off

All 14 gating criteria were already checked `[x]` in `spec.md` by the executor. Per the reviewer check-off protocol, each was independently re-verified this session and each verifies PASS, so all existing check-offs stand; no source-file edits were required and none were made. HV-1 remains `[ ]` (correctly unchecked; non-gating). No phantom criteria were added.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md`
- Total AC items: 15 (14 gating + HV-1 non-gating)
- Checked off (delivered): 14
- Remaining (unchecked): 1
- Items remaining: HV-1 (documented human-verification exception — not a merge gate; execute post-fix per `runbooks/verify-search-focus-retention.runbook.md`)
