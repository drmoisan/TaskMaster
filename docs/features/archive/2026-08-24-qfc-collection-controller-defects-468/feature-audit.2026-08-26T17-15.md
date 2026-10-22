# Feature Audit — qfc-collection-controller-defects-468 (issue #468 family)

- **Date:** 2026-08-26T17-15
- **Reviewer:** feature-review agent
- **Branch:** `bug/qfc-collection-controller-defects-468` @ `91943050`
- **Base:** `origin/epic/quickfiler-bug-family-integration` @ `141efcb8`
- **Work mode:** `full-bug` — `spec.md` is the sole acceptance-criteria source. `issue.md` checkboxes are pointers only and were not treated as acceptance criteria.
- **AC source:** `docs/features/active/qfc-collection-controller-defects-468/spec.md` §`## Acceptance Criteria` (29 items)
- **Plan of record:** `plan.2026-08-24T09-39.md` — 180 of 180 tasks checked.

Verification method: each criterion was evaluated against the branch diff (`origin/epic/quickfiler-bug-family-integration...HEAD`, merge base equal to the integration tip), direct source inspection at review time, and the committed evidence tree. "Re-verified" below means the reviewer independently reproduced the check in this session rather than relying on the executor's record alone.

---

## AC Evaluation Table

Evidence paths are relative to the feature folder.

| AC | Subject | Verdict | Evidence / reviewer verification |
|---|---|---|---|
| AC-1 | #286 counter restored on exceptional exit | **PASS** | `Interlocked.Decrement` re-verified inside a `finally` block in `RemoveSpecificControlGroupAsync`; two named tests (`...ThrowAtFirstStatement...`, `...ThrowLaterInBody...`) present with red-then-green TRX pairs (`p3-t2`, `p3-t3` fail-before; `p3-t5` pass-after). |
| AC-2 | #468 twelve dead members + field + commented reference absent | **PASS** | Re-verified: all 13 identifiers return 0 hits in `QfcCollectionController.cs` at review time. Executor sweep with per-identifier baseline contrast at `evidence/qa-gates/p1-t3-dead-identifier-sweep.2026-08-26T08-45.md`. |
| AC-3 | #468 five live members unmodified | **PASS** | Re-verified: all five identifiers present. Non-regression record at `evidence/qa-gates/p1-t4-live-member-nonregression.2026-08-26T08-45.md`. |
| AC-4 | #469-1 null-`ItemController` branch reachable | **PASS** | Re-verified in source: null test dominates every dereference; `"To Unknown,Sender Unknown,Email,Folder Unknown"` literal present in the reachable branch. Test `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` red-then-green (`p6-t3` / `p6-t5`). |
| AC-5 | #469-1/2 array length equals count, no null element | **PASS** | Re-verified: allocation is `new string[_itemGroupsToMove.Count]` with loop bound `Count`. One-group and three-group tests red-then-green (`p6-t1`, `p6-t2` / `p6-t5`). |
| AC-6 | #469-3 ordered contract + explicit bounds check | **PASS** | Re-verified: field declared `IReadOnlyList<QfcItemGroup>`; `TryGetItemGroupByIndex` performs explicit null-and-bounds check, no broad catch. Structural test red-then-green (`p4-t3` / `p4-t7`); behavioural ordering test justified as permanent-green (dossier item 1). |
| AC-7 | #469-4 `stackMovedItems` documented and consumed | **PASS** | Re-verified: XML doc block on interface and implementation states the push-path contract and source-compatibility retention; body carries the explicit `_ = stackMovedItems;` discard. Test `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` present. `QfcFormController.EventHandlers.cs` untouched (diff verified). |
| AC-8 | #470-1 `-1` index handled in `PromoteFirstChild` / `ChangeConversationSilently` | **PASS** | Re-verified: `-1` sentinel returned with one `Warn` log, no subscript. Both named tests present; red-then-green pairs (`p8-t1`, `p8-t2` / `p8-t4`). |
| AC-9 | #470-2 single-source insertion count + reconciliation log + `-1` guard | **PASS** | Re-verified: `ResolveConversationInsertions` resolved exactly once before `MakeSpaceForItems`; `ReconcileInsertionCount` receives all six spec-named values with a `Warn` sink; `baseEmailIndex == -1` guarded with state restore; loop not clamped. Above/equal/below tests plus direct pure-helper test present. |
| AC-10 | #470-3 `SetVisualDigits` skips null controller/viewer | **PASS** | Test `SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing` red-then-green (`p9-t1` / `p9-t3`). |
| AC-11 | #471 `EliminateSpaceForItems` shrinks `MinimumSize` and `Size`; round-trip neutrality | **PASS** | Re-verified: both properties shrunk via the pure `ShrinkByRows` helper; inversion removed in exactly one place (`MakeSpaceForItems` still grows via negative count). MTA `ShrinkByRows` tests plus two `[STATestClass]` call-site tests present; red-then-green (`p10-t6` / `p10-t9`); the `MakeSpaceForItems` `Size` asymmetry recorded in P10 evidence as the spec requires. |
| AC-12 | #473-1 late-added background task still awaited; drain logic in one place | **PASS** | Re-verified: single `DrainBackgroundLoadingTasksAsync` definition with `Interlocked.Exchange` bag swap, called from both former drain sites; test uses two `TaskCompletionSource` instances with `ExecuteSynchronously`, no timing waits. Red-then-green (`p11-t4` / `p11-t6`). |
| AC-13 | #473-2 cancellation propagates; single log per root failure | **PASS** | Re-verified: `OperationCanceledException` clause precedes the broad catch and rethrows; failure path logs once and returns without re-reading `Subject`. Both tests present, `VerifyGet(x => x.Subject, Times.Never())` assertion re-verified in source; red-then-green (`p5-t1`, `p5-t2` / `p5-t5`). |
| AC-14 | #474-1 `_parent` and constructor parameter 5 typed `IQfcFormController`; no downcast | **PASS** | Re-verified: field and parameter both `IQfcFormController`; `(QfcFormController)_parent` returns 0 hits. Reflection test present; red-then-green (`p2-t6` / `p2-t10`). `EfcFormController.cs` and construction sites untouched. |
| AC-15 | #474-2 `TryGetMoveReadiness` + delegate-preserved dialog | **PASS** | Re-verified: readiness evaluation presents nothing; `NotifyNotReady` defaults to the exact prior `MessageBox.Show` call; three header sentinel strings present in the evaluation. Recording-delegate tests present (permanent-green, dossier item 7). |
| AC-16 | #468 residual-risk search (build-input files + reflective calls) | **PASS** | `evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`: 0 hits across 398 build-input files; 42 `GetMethod(` hits and 0 `InvokeMember(` hits enumerated, none naming a removed identifier. The deliberate non-repository-wide scope is the spec's own stated construction. Residual risk promoted as issue #635. |
| AC-17 | Fix order followed; dead-code removal isolated | **PASS** | `evidence/qa-gates/p14-t7-fix-order-audit.2026-08-26T16-34.md`: 18 feature commits match the D1 order exactly; removal commit `63eebd47` carries exactly one `.cs` path. |
| AC-18 | Bugfix workflow: failing test committed and demonstrated first | **PASS** | Fifteen red-then-green TRX pairs indexed at `evidence/qa-gates/p14-t8-fail-before-index.2026-08-26T16-30.md`; every fail-before failed count >= 1, every pass-after failed count = 0. |
| AC-19 | Fail-before exception dossier for the four no-red items | **PASS** | `evidence/regression-testing/fail-before-exception.2026-08-26T16-24.md` records seven items with per-item impossibility reasons and alternative proofs — a superset that includes all four the criterion enumerates. |
| AC-20 | Three seams behavior-preserving | **PASS** | `evidence/qa-gates/p14-t9-seam-audit.2026-08-26T16-35.md`: each seam in its own single-file commit; identical suite pass counts before/at each seam (958/958, 962/962, 964/964). |
| AC-21 | Owned-file discipline | **PASS** | Re-verified via `git diff --name-status`: the only changed code files are the owned set; `KbdActions.cs`, `QfcFormController.EventHandlers.cs`, `EfcFormController.cs` untouched. Non-code collateral in the diff (feature docs/evidence, promoted potential documents, one orchestrator agent-memory note) is process-artifact output of the delivery lifecycle, not feature code, and does not breach the owned-file clause. |
| AC-22 | Test-file constraints (no new method in the capped file; new files < 500; csproj insertion point) | **PASS** | Re-verified: `QfcCollectionControllerTests.cs` diff is exactly the 3+/3- injection-type change at :63-71 with no new method; new files at 154/494/497/432/183 lines; all five `Compile Include` entries sit between the `DarkModeTests` and `QfcDatamodelTests` entries. |
| AC-23 | Test policy (frameworks, no temp files, no UI, no banned APIs, STA hygiene) | **PASS** | Re-verified: 0 executable hits for all four banned literals (the 4 raw hits are `///` doc lines stating non-use, two mandated by plan D9); STA file `[STATestClass]`, panel disposed per test, no `Show`/`ShowDialog` call. Full audit at `evidence/qa-gates/p14-t12-test-policy-audit.2026-08-26T16-39.md`. |
| AC-24 | Single clean toolchain pass in order | **PASS** | P15-T1/T2/T3/T4/T5 all EXIT 0 in one pass; 18 projects executed `CoreCompile` on both Rebuild gates; 6581/6581 tests green. Loop record at `evidence/qa-gates/p15-t6-loop-record.2026-08-26T16-48.md`. |
| AC-25 | No scope creep (no partial split; exclusion attribute retained; no NuGet addition; parameter retained) | **PASS** | Re-verified: no `partial class` in the controller; the measurement-exclusion attribute remains at :21; the csproj diff adds only `Compile Include` entries and no `packages.config` changed; `stackMovedItems` retained. Each item recorded in `## Follow-up Candidates` and promoted (AC-29). Audit at `evidence/qa-gates/p14-t13-scope-creep-audit.2026-08-26T16-40.md`. |
| AC-26 | Downstream handoff for #444 recorded | **PASS** | Spec §`## Downstream Notes for Sibling Issues` records all three required items: `WireUpKeyboardHandler` removal side effect, the `KbdActions(IEnumerable<UClass>)` missing duplicate check, and the dead `conversationCount` parameter (now made live). Handoff evidence at `evidence/other/downstream-handoff-444.2026-08-26T16-26.md`. |
| AC-27 | PR accuracy constraints honored in the PR body | **DEFERRED — NOT YET EVALUABLE (NON-BLOCKING)** | The PR body does not exist yet; the orchestrator authors it after this review. The binding constraints are recorded at `evidence/other/pr-accuracy-constraints.2026-08-26T16-27.md` and must be applied at PR-authoring time. Deliberately left unchecked in spec.md; not a defect of the implementation. |
| AC-28 | Seven issues closed by the merge | **DEFERRED — NOT SATISFIED AT THIS MERGE (NON-BLOCKING)** | This branch merges into the epic integration branch, not the default branch; GitHub registers closing references only for PRs targeting the default branch, so none of #286/#468/#469/#470/#471/#473/#474 closes at this merge. Closure is deferred to the epic integration-to-default merge. Closure set recorded at `evidence/other/issue-closure-set.2026-08-26T16-28.md`. Deliberately left unchecked in spec.md; not a defect of the implementation. |
| AC-29 | Follow-up candidates promoted with issue numbers recorded | **PASS** | `evidence/other/followup-promotion-resolution.2026-08-26T21-05.md`: all nine candidates map to real issues (#623, #629-#635, plus pre-existing #444); seven promoted with MCP receipts and destination paths under `docs/features/potential/promoted/` (files present in the diff); zero deferred rows remain. |

---

## Check-off Actions

Per the acceptance-criteria-tracking protocol: AC-1 through AC-26 and AC-29 were already checked in `spec.md` by the executor and are confirmed correct — no reviewer check-off was required. AC-27 and AC-28 remain unchecked by design (deferred, evaluable only at PR-authoring and default-branch-merge time respectively) and were not flipped.

## Deferred Items — Ownership

| Item | Owner | When |
|---|---|---|
| AC-27 verification | orchestrator / PR author | at `pr-author` time, against `evidence/other/pr-accuracy-constraints.2026-08-26T16-27.md` |
| AC-28 verification | epic orchestrator | at the epic integration-to-default merge |
| Issue #623 line-count refresh (2,349 -> 2,437) | orchestrator or maintainer | at or after the integration merge |

### Acceptance Criteria Status
- Source: docs/features/active/qfc-collection-controller-defects-468/spec.md
- Total AC items: 29
- Checked off (delivered): 27
- Remaining (unchecked): 2
- Items remaining: AC-27 (PR accuracy — evaluable only when the PR body is authored, after this review), AC-28 (issue closure — deferred to the epic integration-to-default merge; cannot occur at the integration merge)

---

## Verdict

27 of 29 acceptance criteria PASS with evidence; the remaining 2 are deferred by design to later lifecycle stages and are NON-BLOCKING. Zero blocking findings. The feature delivers every in-scope defect fix in the seven-issue family with red-then-green proof or a justified exception for each.
