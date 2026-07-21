# Feature / Acceptance-Criteria Audit — Issue #398

- Timestamp: 2026-07-20T22-30
- Work Mode: minor-audit
- AC source: `issue.md` `## Acceptance Criteria` (AC-1..AC-5)

## Scope and Baseline

- Base branch (resolved): main @ cd6362f0264217d9ed94487f44c193df96eb1fa6 (merge-base; equals origin/main).
- Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 1cb031f6ea2a7dfba2d035433208042a01f32fe6.
- Diff scope audited: full branch cd6362f0..1cb031f6 (5 C# files, 15 Markdown files).
- Defect: `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` cleared the breadcrumb model synchronously
  before its first `await`, exposing a transient empty/partial window that let a concurrent host
  `SelectRow(1)` throw `ArgumentOutOfRangeException` when `FolderArray.Length > 1`.

## Acceptance Criteria Inventory

| ID | Criterion (abbreviated) |
|---|---|
| AC-1 | Deterministic MSTest regression test (TCS-gated fake provider, no sleeps): `SelectRow(1)` throws before fix, succeeds after. |
| AC-2 | `SetSuggestionsAsync` no longer exposes a transient cleared/partial model; rows built locally and swapped atomically; observable count never drops below pre-upgrade. |
| AC-3 | Readback contract (`FolderContains`, `GetSelectedFolder`, `GetFolderItems`, `SelectRow`) stays pre-upgrade-consistent during an in-flight upgrade; host-selected index survives the swap. |
| AC-4 | Completed-upgrade behavior unchanged (chains/probabilities, unresolvable->plain fallback, non-scored verbatim); all existing router/coordinator/controller tests pass. |
| AC-5 | Full C# toolchain passes in order with zero regressions vs Phase 0 baseline; new/changed code >= 90% coverage. |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence |
|---|---|---|
| AC-1 | PASS | `evidence/regression-testing/fail-before.2026-07-20T21-41.md` records the pre-fix `ArgumentOutOfRangeException` ("Row selection requires -1 or an index in [0, 0]", actual 1); `pass-after` records the green post-fix run. Test `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection` present in `BreadcrumbBridgeCoordinatorTests.cs` (csproj-wired). TCS-gated fake provider, no timing sleeps. |
| AC-2 | PASS | Diff of `FolderBreadcrumbBridgeRouter.cs`: `_model.Clear()` removed; rows built into local `List<BreadcrumbStateRow> built`; single `_model.ReplaceRows(built)` at the end. `ReplaceRows` (BreadcrumbStateModel.cs) swaps `_rows` by reference and reconciles selection before publishing. Router test `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount` asserts the invariant. |
| AC-3 | PASS | `ReplaceRows` preserves the selected index when still valid and resets subfolder selection before publish. Router test `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives` asserts readback consistency and selection survival across the swap. |
| AC-4 | PASS | Scored/unresolvable-fallback (plain row carrying the score's folder path) and non-scored verbatim paths preserved in the diff. Full suite 5061/5061 pass (baseline 5054 + 7 new), EXIT_CODE 0 — `evidence/qa-gates/tests-coverage.2026-07-20T21-41.md`. |
| AC-5 | PARTIAL | Toolchain PASS: CSharpier, analyzer build, nullable build, MSTest all EXIT_CODE 0 in order (`evidence/qa-gates/*.2026-07-20T21-41.md`); no regression vs baseline. Coverage clause: new/changed-code line coverage documented at 100% and instrumented-scope shows no regression (`coverage-delta.2026-07-20T21-41.md`), but this cannot be independently confirmed from a valid HEAD canonical coverage artifact — the on-disk `artifacts/csharp/coverage.xml` was a stale, unrelated leftover (removed). See policy audit §5.1. Toolchain sub-clause met; coverage sub-clause supported by evidence but not artifact-verifiable at HEAD. |

## Summary

Four of five acceptance criteria pass on the merits with corroborating evidence in the diff and the
executor qa-gate/regression artifacts. AC-5 is graded PARTIAL: its toolchain sub-clause is verified PASS,
but its coverage sub-clause depends on a valid HEAD canonical coverage artifact that is absent (the file
at the canonical path was a stale leftover predating the changes and was removed). The documented
new/changed-code coverage (100%) and no-regression result appear to satisfy the coverage target, but
independent artifact verification is pending. Two remediation-required policy findings (two test files
over the 500-line limit; canonical coverage artifact regeneration) are carried in the policy audit and
remediation inputs.

Go/no-go: not ready for unqualified merge. Address the two remediation items (split over-limit test
files; regenerate/confirm the canonical C# coverage artifact at HEAD or cite the PR CI coverage run),
then the fix is mergeable.

## Acceptance Criteria Check-off

- AC-1: PASS — remains `[x]` in issue.md.
- AC-2: PASS — remains `[x]` in issue.md.
- AC-3: PASS — remains `[x]` in issue.md.
- AC-4: PASS — remains `[x]` in issue.md.
- AC-5: PARTIAL — the executor set this to `[x]` based on its coverage evidence. The reviewer did not
  clear the box because the coverage sub-clause is not independently verifiable from a valid HEAD
  canonical artifact; issue.md was not mutated to avoid altering the AC source on a substantively-met
  clause. Confirm via canonical-artifact regeneration or the PR CI coverage run.

### Acceptance Criteria Status
- Source: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/issue.md
- Total AC items: 5
- Checked off (delivered): 4 fully verified (AC-1..AC-4); AC-5 marked delivered by executor, reviewer verdict PARTIAL
- Remaining (unchecked by reviewer): 0 fully open; 1 PARTIAL (AC-5 coverage sub-clause pending independent artifact confirmation)
- Items remaining: AC-5 coverage sub-clause (independent HEAD coverage-artifact confirmation)
