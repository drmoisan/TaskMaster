# Code Review: Folder-tree dispatcher thread affinity (#420)

**Review Date:** 2026-08-06
**Reviewer:** feature-review agent
**Feature Folder:** `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420`
**Feature Folder Selection Rule:** `issue.md` identifies #420 as `full-bug`; the canonical PR context identifies this active folder.
**Base Branch:** `main` / `origin/main` at `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
**Head Branch:** `bug/folder-tree-dispatcher-thread-affinity-420` at `369e974e7396f0321c6b51f5ae96c0794fa12460`
**Review Type:** Post-remediation final review of the committed range.

## Executive Summary

The reviewed range moves live folder-tree composition, builds, refreshes, cleanup, and post-yield traversal through the captured Outlook STA dispatcher while retaining the public snapshot contract. It also changes FilterOlFolders cold initialization to an awaited path and adds focused lifecycle/coverage regression surfaces. The review used `artifacts/pr_context.summary.txt` as primary scope, `artifacts/pr_context.appendix.txt` as exact diff evidence, revision-7 plan/evidence, and the base-to-head code diff.

Current deterministic evidence addresses the prior lifecycle, refresh, ownership, fault-observability, and coverage blockers. Independent CSharpier and whitespace checks passed. AC8 remains unchecked in the source despite complete documentation; this assignment does not authorize source requirement edits.

**What changed:** Dispatcher-owned service composition and builds; dispatcher-preserving live traversal; queued cleanup/terminal isolation; awaited FilterOlFolders initialization and ribbon fault boundary; dedicated test seams and capacity-approved test partials; cycle-4 coverage reconciliation.

**Top 3 risks:**

1. Live Outlook/VSTO host confirmation remains an operational follow-up; automated tests intentionally do not use Outlook.
2. An untracked post-commit receipt exists in the worktree and was excluded from this committed-range review.
3. AC8's checkbox requires owning-workflow reconciliation after this review.

**PR readiness recommendation:** **Go** — the committed issue #420 implementation passes the reviewed policy, test, coverage, formatting, and whitespace gates.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Info | `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md` | AC8 | The final-documentation checkbox remains unchecked although the specification records final design, evidence paths, coverage, and no approved deviation. | The owning workflow should check AC8 after recording this review; do not alter it in this audit-only assignment. | Keeps requirement-source tracking aligned with the completed review. | `spec.md`; revision-7 plan P7-T1/P7-T5; feature audit. |

No Blocker, Major, Minor, or Nit findings were identified.

## Implementation Audit

### C# implementation audit

#### What changed well

- `AppOlObjects.FolderTreeService` uses a single completion state for worker-first composition and propagates dispatcher/composition terminal outcomes without worker fallback.
- `OutlookFolderTreeService` owns dispatcher execution for cold builds and refreshes, preserves publication/coalescing state, and isolates cancellation and cleanup failures.
- `FolderTreeSnapshotBuilder` and `OutlookFolderHierarchyReader` retain dispatcher context through live traversal yields; no production `Task.Yield` fallback was introduced.
- FilterOlFolders and ribbon handling await cold initialization and define error reporting at the UI-event boundary.

#### Type safety and API notes

Nullable compilation passed. `IOutlookFolderTreeService.GetSnapshotAsync` and `IUiDispatcher.BeginInvoke(Action)` remain compatible. The generic dispatcher overload is documented and covered by dedicated STA tests.

#### Error handling and logging

Composition, scheduled-refresh, observer, cancellation, and cleanup paths retain a primary terminal outcome and report contained observer/cleanup failures through logging. Cycle-4 tests cover failure identity, retry, disposal, and terminal isolation.

## Test Quality Audit

### Reviewed test and QA artifacts

- `evidence/regression-testing/remediation-cycle4-acceptance-criteria-mapping.2026-08-06T18-20.md` — AC/CR mapping and deterministic constraints.
- `evidence/regression-testing/remediation-cycle4-predecessor-reconciliation.2026-08-06T16-14.md` — prior lifecycle findings reconciled by a 90/90 two-assembly run.
- `evidence/qa-gates/remediation-cycle4-mstest-coverage.2026-08-06T18-35.md` — 6,166/6,166 tests and repository metrics.
- `evidence/qa-gates/remediation-cycle4-coverage-and-quality-delta.2026-08-06T18-36.md` — changed-line and target-method coverage reconciliation.

- **Determinism:** Fakes, mocks, controlled tasks, and dedicated STA hosts; no live Outlook, network, temporary files, timers, sleeps, polling, or retries.
- **Isolation:** Test classes split ownership, dispatcher, traversal, and view-lifecycle boundaries into focused cases.
- **Speed:** The recorded full run passed; no timing-based retry behavior is used.
- **Diagnostics:** Fail-before and green artifacts identify test names, commands, and lifecycle conditions.

## Security / Correctness Checks

| Check | Status | Evidence |
| --- | --- | --- |
| No secrets in code | PASS | Reviewed C# diff and appendix inventory; no credential/config additions. |
| No unsafe subprocess or command construction | PASS | No changed runtime process-invocation path. |
| Input validation at boundaries | PASS | Constructors preserve null guards; invalid lifecycle states throw explicit exceptions. |
| Error handling remains explicit | PASS | Dispatcher, composition, cleanup, and observer terminal paths are covered by cycle-4 regressions. |
| Configuration / path handling is safe | PASS | No runtime configuration or path handling behavior was added. |

## Research Log

No external research was required. Repository research, feature evidence, and canonical PR-context artifacts supplied the technical and scope evidence.

## Verdict

The committed review range is ready for normal PR flow. All prior blocking code-review requirements are covered by current cycle-4 regressions and final QA. The remaining AC8 checkmark is a source-tracking follow-up outside this audit's write authorization and does not change the implementation or validation result.
