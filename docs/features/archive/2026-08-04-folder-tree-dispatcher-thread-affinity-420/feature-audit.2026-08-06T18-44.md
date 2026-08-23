# Feature Audit: Folder-tree dispatcher thread affinity (#420)

**Audit Date:** 2026-08-06
**Feature Folder:** `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420`
**Base Branch:** `main` / `origin/main` at `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
**Head Branch:** `bug/folder-tree-dispatcher-thread-affinity-420` at `369e974e7396f0321c6b51f5ae96c0794fa12460`
**Work Mode:** `full-bug`
**Audit Type:** Post-remediation acceptance verification.

## Scope and Baseline

- **Merge base:** `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`.
- **Primary evidence:** current `artifacts/pr_context.summary.txt`.
- **Secondary exact-diff evidence:** `artifacts/pr_context.appendix.txt`.
- **Feature evidence:** revision-7 plan; cycle-4 P5 mapping, P6 QA, inventory, and precommit-validation artifacts.
- **Requirements source:** `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md` only.
- **Work mode resolution:** `issue.md` explicitly declares `full-bug`; `spec.md` is the sole authoritative AC source.
- **Scope note:** An untracked post-commit receipt is excluded; this audit evaluates only the stated committed range.

## Acceptance Criteria Inventory

**Authoritative AC source file:** `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md`.

1. A dispatcher-free worker can initiate a cold folder-tree request without `WpfDispatcherYield` throwing `InvalidOperationException`.
2. Service composition, notification-sink construction, every live hierarchy adapter access, and every post-yield continuation for a cold build or refresh execute on the captured Outlook STA dispatcher.
3. The production live traversal path uses `WpfDispatcherYield` on the captured dispatcher and does not select `Task.Yield`, a worker-local dispatcher, or caller-specific yield fallback logic.
4. The folder-tree service retains one session-scoped instance, coalesces concurrent cold requests, and preserves cancellation, stale/current, invalidation, publication, and disposal behavior.
5. FilterOlFolders cold initialization awaits the snapshot without synchronously blocking the UI dispatcher, and the viewer is wired only after snapshot acquisition.
6. Deterministic MSTest coverage proves worker-started cold build affinity, continuation affinity after a forced yield, service-composition and notification-sink affinity, and nonblocking cold filter initialization without Outlook, network, temporary files, sleeps, or retry loops.
7. The final C# toolchain passes in one uninterrupted final pass: CSharpier, analyzer build, nullable build, and MSTest with code coverage; changed behavior meets the repository coverage requirements.
8. The feature documentation records the final implementation decisions, validation evidence, and any approved deviation from this scope.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
| --- | --- | --- | --- | --- | --- |
| 1 | Worker cold request avoids dispatcher-free yield exception | PASS | Cycle-4 P5 mapping and worker/cold-build evidence | Recorded focused/full MSTest coverage command | Dispatcher-owned build is current and covered. |
| 2 | Composition, access, and continuation stay on captured STA | PASS | Cycle-4 AppOl, Outlook/WPF, predecessor-reconciliation, and P5 mapping | Recorded targeted and full MSTest commands | Covers composition, notification lifecycle, refresh, forced yield, and cleanup. |
| 3 | Strict WPF yield with no fallback | PASS | Diff inspection; strict-yield regression; P5 no-fallback evidence | `git diff ce0c91e686bf7e060aaab6f185ee6883269e4fd4..369e974e7396f0321c6b51f5ae96c0794fa12460 -- '*.cs'` | No fallback found. |
| 4 | Session, coalescing, state, cancellation, invalidation, publication, disposal | PASS | P5 CR-003/CR-004 mapping and coverage fixtures | Recorded full MSTest coverage command | Covers terminal isolation and publication lifecycle. |
| 5 | Nonblocking FilterOlFolders initialization and delayed wiring | PASS | Controller fixture and ribbon regressions | Recorded focused/full MSTest coverage command | Covers ownership, close races, factory failures, and await behavior. |
| 6 | Deterministic coverage without external dependencies | PASS | P5 mapping and final inventory | Artifact inspection plus full MSTest evidence | No prohibited external/timing dependency found. |
| 7 | Final C# toolchain and coverage requirements | PASS | Cycle-4 P6: 6,166/6,166; 84.8015% repository; 99.7730% changed production | Recorded P6 commands; independent CSharpier check | All policy thresholds are met. |
| 8 | Final implementation, validation, and deviation documentation | PASS | `spec.md` design/evidence, inventory, and P6 evidence | Artifact inspection | Checkbox update is outside audit-only authority. |

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**

- **PASS:** 8 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:** None for the committed implementation. Outlook VSTO host exercise remains an operational follow-up, not a blocking criterion.

**Recommended follow-up verification steps:**

1. Record the normal Outlook/VSTO smoke check when a suitable host is available.
2. The owning workflow should update AC8's checkbox after recording this review.

## Acceptance Criteria Check-off

All eight criteria evaluate as PASS. The source's first seven criteria are already checked. AC8 remains unchecked in `spec.md`; the parent assignment authorizes audit-artifact writes only, so no source checkbox was changed by this reviewer.

### AC Status Summary

- Source: `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md`
- Total AC items: 8
- Checked off (delivered): 7
- Remaining (unchecked): 1
- Items remaining: The feature documentation records the final implementation decisions, validation evidence, and any approved deviation from this scope.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
| --- | ---: | ---: | ---: | --- |
| `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/spec.md` | 8 | 7 | 1 | AC8 evaluates PASS; checkbox update is outside audit-only authority. |
