# Feature Audit — ribbon-engine-toggle-state-guards (#505, #506, #518)

- **Artifact:** `feature-audit.2026-08-08T21-59.md`
- **Reviewer:** feature-review agent
- **Date:** 2026-08-08

## Summary

Work mode is `full-bug` (marker verified in `issue.md`), so `spec.md` is the sole authoritative acceptance-criteria source: AC-1 through AC-23. Twenty-two criteria evaluate **PASS**; **AC-22** is **PENDING-MANUAL** by design (MANUAL-ONLY live-Outlook verification, deliberately unchecked with a maintainer checklist committed). No criterion evaluates FAIL or blocking-PARTIAL. **Blocking findings in this artifact: 0.**

## Scope and Baseline

- Branch: `bug/ribbon-engine-toggle-state-guards-505` at `96650d6e`; base: `origin/main` at `f910ff2f` (merge base). Full diff reviewed: 79 files, +6856/-55.
- Baseline facts independently confirmed: 11 `Engines.` references in `RibbonViewer.EngineCommands.cs` at merge base with 1 pre-gated site; `TestSpam_Click` byte-identical at head; `RibbonController.Intelligence.cs` zero-line diff with the #507 `Globals?.Engines` guard intact at line 204; merge-base coverage baseline committed before implementation (`evidence/baseline/`, commit `c18fd2ea` precedes fix commit `d0f3a13e`).

## Acceptance Criteria Inventory

- Source: `spec.md` § Acceptance Criteria — 23 items (AC-1..AC-23), with the issue-tag coverage map to `issue.md` iAC1-iAC17.
- Checkbox state at review start: 22 checked, AC-22 unchecked (by design).

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC-1 | PASS | `RibbonViewer.EngineCommands.cs:188` — `public bool SpamBayesEnabled_GetPressed(Office.IRibbonControl control)`; R1 pin `ToggleGetPressedCallbacks_MatchOfficeCheckBoxGetPressedSignature` (red pre-fix: `fail-before-505.2026-08-08T20-52.md`). |
| AC-2 | PASS | `RibbonViewer.EngineCommands.cs:294` — the Triage mirror; same R1 pin (name resolved from the ribbon XML, both checkboxes iterated). |
| AC-3 | PASS | Read path is `_controller?.IsEngineToggleActive(...)` -> `EngineToggles.GetPressed` (dictionary read; source-verified never awaits/blocks/throws). Independent grep of the full branch diff: zero `.Result` / `.Wait()` / `GetAwaiter().GetResult()` on added lines. Each B2 semantic (never-primed false; at-most-one prime; completion updates cache + invalidates mapped control; fault logged, still false) has a named test in `EngineToggleStateCoordinatorTests`. |
| AC-4 | PASS | `RibbonViewerEngineCallbackShapeTests` pins both `getPressed` and both `onAction` signatures by reflection with `FullName` comparison; demonstrated red pre-fix, so a regression fails the build. |
| AC-5 | PASS | `SpamBayesEnabled_Click` is `async void`, awaits `Controller.HandleEngineToggleClickAsync(SpamBayes.GroupName)` (L169-170); R5 `AsyncStateMachineAttribute` pin, red pre-fix. No discarded `Task`. |
| AC-6 | PASS | `TriageEnabled_Click` mirror (L279-280); same R5 pin. |
| AC-7 | PASS | Single boundary `catch` in `HandleToggleClickAsync` routes to `logError` (production `logger.Error(message, exception)`), no rethrow, no invalidate; `ExecuteToggleAsync` contains no `catch` (source-verified). Tests: `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate`, `ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged` (asserts the same exception instance). |
| AC-8 | PASS | Both toggle handlers and both `ShowSaveInfo` handlers are `async void` + single awaited expression matching the sibling shape; R5 pins cover all four (`ShowSaveInfoHandlers_AreAsyncVoidAwaitedShape` red pre-fix). |
| AC-9 | PASS | `ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder` records the sequence and probes the cache from inside the invalidation sink (`CacheVisible:True` precedes `Invalidate:SpamBayesEnabledToggle`). Note: the invariant holds per path; the cross-path prime/toggle interleaving is code-review CR-1 (Major, non-blocking), outside this criterion's text. |
| AC-10 | PASS | Source inspection of all ten rewritten sites: 4 via the coordinator (never `RunEngineCommandAsync`), 6 via `RunEngineCommandAsync` lambdas; 6 new catalog entries exactly as enumerated; 6 `getEnabled="EngineCommand_GetEnabled"` XML attributes; R3/R4 red-then-green evidence. |
| AC-11 | PASS | `TestSpam_Click` byte-identical to merge base (independently compared). Count re-derived independently: 10 newly guarded + 1 pre-existing gated = 11; 0 unguarded production dereferences at head. Recorded in `evidence/qa-gates/guarded-site-audit.2026-08-08T21-09.md` and re-verified here. |
| AC-12 | PASS | R2 (`GetPressedCallbacks_BeforeSetGlobals_ReturnFalseWithoutThrowing`, red pre-fix with the NRE attributed to line 123) plus `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing` and `GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing`; command sites blocked by the closed readiness gate (pre-existing #503 tests). |
| AC-13 | PASS | `RibbonController.Intelligence.cs:204` reads exactly `internal IAppItemEngines Engines => Globals?.Engines;`; zero-line diff on the file; `RibbonControllerTests.Engines.cs` untouched and passing in the final run. #507 not reverted. |
| AC-14 | PASS | Independent grep of both new files: zero `Microsoft.Office.*`/Interop usings, zero `MessageBox`, zero WinForms types, zero `[ExcludeFromCodeCoverage]`. No existing exemption removed or widened anywhere in the diff. |
| AC-15 | PASS | Red-first evidence: `fail-before-505.2026-08-08T20-52.md` (exit 1, 11 failures each attributed to its pre-fix cause, R1/R2/R3/R5), `fail-before-r4-xml.2026-08-08T21-04.md` (R4), `pass-after-505.2026-08-08T21-06.md`. |
| AC-16 | PASS | `toolchain-clean-pass.2026-08-08T21-40.md`: five steps back-to-back, all exit 0, identical SHA-256 tree fingerprint before/after, format rewrote 0 files, 18 `csc.exe` invocations (non-vacuous), 6 pre-existing warnings byte-identical to base, type-check per CI's command with the documented #522 deviation, 6435 tests passed with stale `\.claude\worktrees\` assemblies excluded. Independently re-verified by the orchestrator against the committed tree. |
| AC-17 | PASS | Scope held (full-diff inspection; no out-of-scope production change). Promotion receipts: item 1 already #504 (tracker-verified), item 3 resolved during authoring, item 2 promoted as **#524** — re-verified in this review via `gh issue view 524` (OPEN). One Minor documentation staleness in `issue.md` (code-review CR-4) does not affect the criterion's substance. |
| AC-18 | PASS | All eight Test Strategy scenario groups have named tests (verified by reading the file); coordinator line coverage 0.991489 >= 0.90 (`new-type-coverage.2026-08-08T21-38.md`). The 2 uncovered lines are the defensive direct-caller guard (code-review CR-3, Minor). |
| AC-19 | PASS | Baseline captured pre-implementation (commit order verified); comparison artifact shows zero changed-line regression and repo-wide 0.859190 line / 0.793602 branch, slightly above baseline; record-and-report obligation satisfied; the exemption-flatness expectation confirmed empirically (both exempt types absent from the Cobertura document). |
| AC-20 | PASS | Independent grep of added test lines: zero temp files, `Thread.Sleep`/`Task.Delay`, wall-clock reads, `Form`/`MessageBox`/`BackgroundWorker`, message pump, or live COM; no test drives `NotifyEngineCommandNotReady` (notification sink is an injected delegate); corroborated by `test-determinism-audit.2026-08-08T21-14.md` and the green AC-16 run. |
| AC-21 | PASS | Independent line counts at head: every `.cs` file in the diff <= 500 (max 459). `RibbonExplorer.xml` 545 retains its pre-existing #503-recorded overage as the criterion itself provides; `TaskMaster.csproj` 583 is likewise pre-existing and is an MSBuild project file. |
| AC-22 | PENDING-MANUAL (by design; not a gap) | MANUAL-ONLY. Deliberately unchecked in `spec.md`; maintainer checklist committed at `evidence/manual-verification/ac22-checklist.2026-08-08T21-44.md` with the guard rationale at `manual-only-unchecked.2026-08-08T21-45.md`. Must be executed against a live Outlook profile and recorded before merge per `spec.md` § Rollout. Not counted as blocking: this is the intended disposition for a criterion that unit tests must never check off. |
| AC-23 | PASS | `spec.md` carries `## Delivery Notes and Deviations` covering all deviations (the #522 command, B7 UX change, no file split, continuation-based prime observation, empirical exemption probe, size overages, the #511 phase restart, and the then-deferred promotion); `issue.md` iAC1-iAC17 are all checked with an accurate reviewer-facing delivery note. Minor: `issue.md` point 3 predates the #524 promotion and should be refreshed (code-review CR-4); the committed receipt is accurate, so the delivered outcome is reflected. |

## Acceptance Criteria Check-off

All 22 PASS criteria were already checked `[x]` in `spec.md` by the executor with evidence; no check-off action was required from this review. AC-22 correctly remains `- [ ]` and must not be checked until the manual checklist is executed and recorded. No phantom criteria were added; no criterion text was modified.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/spec.md`
- Total AC items: 23
- Checked off (delivered): 22
- Remaining (unchecked): 1
- Items remaining: AC-22 (iAC9) — MANUAL-ONLY live-Outlook verification (callback binding with "Show add-in user interface errors" enabled; toggle state after click and across menu reopen; pre-initialization invocation of the ten callbacks without `NullReferenceException`).

## Verdict

**PASS — 0 blocking findings.** All automated acceptance criteria are delivered and verified against evidence plus independent re-derivation. Pre-merge actions for the maintainer/orchestrator: (1) execute and record the AC-22 manual checklist; (2) refresh the stale `issue.md` promotion bullet to cite #524; (3) consider promoting code-review CR-1 (prime/toggle last-writer race) so the analysis survives as a tracked issue.
