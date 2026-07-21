# Feature Audit — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Timestamp: 2026-07-20T23-28
- Reviewer: feature-review
- Work Mode: minor-audit (issue.md marker)
- AC Source: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/issue.md, section `## Acceptance Criteria` (AC-1..AC-5)
- Cycle: remediation cycle 1 re-audit (R4)

## Summary

All five acceptance criteria (AC-1..AC-5) are verified PASS against the branch head 4412d2da. The bug
fix eliminates the transient empty window in `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` via an
atomic `BreadcrumbStateModel.ReplaceRows` swap, backed by deterministic MSTest regression tests. The two
remediation findings from the 2026-07-20T22-30 cycle (test-file size limit, canonical coverage artifact)
are resolved. No AC remains PARTIAL, FAIL, or UNVERIFIED.

## Scope and Baseline

- Baseline (base of comparison): main @ cd6362f0264217d9ed94487f44c193df96eb1fa6 (merge-base).
- Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 4412d2dabb0b3b32a47215d07a780e0e0decf913.
- Changed production surface: `BreadcrumbStateModel.cs` (new `ReplaceRows` seam; `_rows` field made
  swappable), `FolderBreadcrumbBridgeRouter.cs` (`SetSuggestionsAsync` local-build + atomic swap).
- Changed test surface: coordinator-level regression test, router-level in-flight invariant tests, and
  R1 scenario splits of the two model/router test files.
- Baseline toolchain/coverage recorded in `evidence/baseline/*.2026-07-20T21-41.md`.

## Acceptance Criteria Inventory

| AC | Text (abbreviated) |
|---|---|
| AC-1 | Deterministic MSTest regression test reproduces the defect (`TCS`-gated fake provider, no sleeps); `SelectRow(1)` throws before the fix, succeeds after. |
| AC-2 | `SetSuggestionsAsync` exposes no transient cleared/partial model; rows built locally and swapped atomically; observable row count never drops below pre-upgrade count. |
| AC-3 | Readback contract (`FolderContains`, `GetSelectedFolder`, `GetFolderItems`, `SelectRow`) stays pre-upgrade-consistent in flight; host-selected index survives the swap. |
| AC-4 | Completed-upgrade behavior unchanged (ancestor chains, probabilities, plain-row fallbacks); all existing router/coordinator/controller tests pass. |
| AC-5 | Full C# toolchain passes in order with zero regressions vs Phase 0 baseline; new/changed code meets >= 90% coverage. |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC-1 | PASS | `evidence/regression-testing/fail-before.2026-07-20T21-41.md` (EXIT 1, `ArgumentOutOfRangeException` "Row selection requires -1 or an index in [0, 0]", actual 1); `pass-after.2026-07-20T21-41.md` (EXIT 0). Test `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection` uses a `TaskCompletionSource`-gated fake `IFolderHierarchyProvider`, no timing waits. |
| AC-2 | PASS | Production diff: `SetSuggestionsAsync` builds into a local `List<BreadcrumbStateRow>` and calls `_model.ReplaceRows(built)` once; the up-front `_model.Clear()` is removed. Router in-flight test `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount` asserts the invariant. |
| AC-3 | PASS | `ReplaceRows` reconciles `_selectedIndex` against the new count before publishing the new backing list; test `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives` verifies readback consistency and selection survival. |
| AC-4 | PASS | Full suite 5061/5061 passing (`evidence/qa-gates/tests-coverage.2026-07-20T22-30.md`, EXIT 0). Scored/unresolvable-chain fallback and plain-row verbatim paths preserved in the refactor. |
| AC-5 | PASS | Toolchain all EXIT 0 (CSharpier, analyzer build, nullable build, MSTest); canonical HEAD coverage artifact `artifacts/csharp/coverage.xml` line 86.54% / branch 80.85% (both above floor); new/changed-code line coverage 100% (>= 90%). No regression on changed lines. See policy-audit Section 5. |

## Acceptance Criteria Check-off

All AC items in `issue.md` were already marked `- [x]` by the executor and are confirmed by this review;
no check-off state change was required. No phantom criteria were added.

### Acceptance Criteria Status
- Source: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/issue.md
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none
