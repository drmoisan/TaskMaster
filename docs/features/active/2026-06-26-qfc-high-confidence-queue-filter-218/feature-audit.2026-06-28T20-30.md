# Feature Audit (Cycle 2 Exit Reaudit): qfc-high-confidence-queue-filter (Issue #218)

---

**Audit Date:** 2026-06-28
**Feature Folder:** `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218`
**Base Branch:** `main`
**Head Branch:** `bug/qfc-high-confidence-queue-filter-218` at `27ca7717e7bf020ab5d2b5788fbdad6c1a1d0943`
**Work Mode:** `minor-audit`
**Audit Type:** Remediation cycle 2 exit reaudit

---

## Scope and Baseline

- **Base branch:** `main`
- **Head branch/commit:** `bug/qfc-high-confidence-queue-filter-218` at `27ca7717e7bf020ab5d2b5788fbdad6c1a1d0943`
- **Merge base:** `1b8536b6e5fb0778aba528caa39853590185bcb7`
- **Branch commits (main..HEAD):** `eac99432`, `b99f0e03`, `2637e4c1` (maintainer production split), `27ca7717` (cycle-2 test split)
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/**`
  - Additional evidence: reviewer commands for on-disk line counts, CSharpier check, `git show`/`git grep` provenance checks, and Cobertura inspection
- **Feature folder used:** `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218`
- **Requirements source:** `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`
- **Work mode resolution note:** `issue.md` contains `- Work Mode: minor-audit`; therefore only the explicit `## Acceptance Criteria` section in `issue.md` is authoritative.
- **Scope note:** The audit scope is the full branch diff against `main` (merge base `1b8536b6`), including the maintainer production split and the cycle-2 test split. No caller-supplied narrowing was applied.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md` - only source (minor-audit mode)

### Acceptance criteria

1. When high-confidence mode is enabled, `QfcDatamodel.LoadRemainingEmailsToQueueAsync` scores each remaining `MailItem` before adding it to `_masterQueue`.
2. When a remaining item score is greater than or equal to `Globals.QfSettings.HighConfidenceThreshold`, the item is added to `_masterQueue` and hooked with `_moveMonitor.HookItem`.
3. When a remaining item score is below the configured threshold, the item is not added to `_masterQueue` and is not hooked with `_moveMonitor.HookItem`.
4. When high-confidence mode is disabled, remaining `MailItem` queue loading keeps the existing add and hook behavior.
5. The GUI initial load path no longer owns the high-confidence filtering decision for only the first visible batch.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | When high-confidence mode is enabled, `QfcDatamodel.LoadRemainingEmailsToQueueAsync` scores each remaining `MailItem` before adding it to `_masterQueue`. | PASS | `QfcRemainingQueueAdmission.TryQueueAsync` (33/33 covered); `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_ScoresBeforeQueueAdmission`; `focused-pass-after-cycle2-218.md`. | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcDatamodelTests|FullyQualifiedName~RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch|FullyQualifiedName~RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter"` | Behavior preserved through the cycle-2 test split; the admission seam scores before add/hook. |
| 2 | When a remaining item score is >= `Globals.QfSettings.HighConfidenceThreshold`, the item is added to `_masterQueue` and hooked with `_moveMonitor.HookItem`. | PASS | `TryQueueRemainingMailItemAsync_ScoreEqualsThreshold_AddsAndHooksMailItem`. | Same focused issue #218 VSTest command. | Inclusive equality boundary covered. |
| 3 | When a remaining item score is below the configured threshold, the item is not added to `_masterQueue` and is not hooked with `_moveMonitor.HookItem`. | PASS | `TryQueueRemainingMailItemAsync_ScoreBelowThreshold_DoesNotAddOrHookMailItem`. | Same focused issue #218 VSTest command. | Below-threshold path returns false before queue add/hook. |
| 4 | When high-confidence mode is disabled, remaining `MailItem` queue loading keeps the existing add and hook behavior. | PASS | `TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring`. | Same focused issue #218 VSTest command. | Disabled mode test fails if scoring is invoked. |
| 5 | The GUI initial load path no longer owns the high-confidence filtering decision for only the first visible batch. | PASS | `QfcHomeController.cs` initial-load line (1/1 covered); `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch`; `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` (now in `QfcHomeControllerIssue218Tests.cs`). | Same focused issue #218 VSTest command. | `RunAsync` loads the initial `IList<MailItem>` path and does not invoke the prefilter delegate. |

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 5 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

All five acceptance criteria pass and the issue #218 behavior is preserved through the maintainer production split and the cycle-2 test split. The focused issue #218 suite passes 7/7 (including the new null-mailItem admission test), and the full MSTest suite passes 4270/4270. The behavior is unchanged from the cycle-1 acceptance; the cycle-2 work was mechanical (file decomposition and test split) with no behavior change.

**Top gaps preventing PASS:**

1. None for acceptance criteria.

**Non-acceptance follow-ups (tracked in `policy-audit.2026-06-28T20-30.md`, non-blocking):**

1. Repository-wide C# coverage (62.12%) remains below 80% raw - authority-scoped exception requiring maintainer ratification under `feature/csharp-coverage-uplift`.
2. Aggregate changed-line coverage (41.91%) is in pre-existing relocated code; the `EmailSorter` exemption rationale should be corrected to "relocated pre-existing untested code."
3. Eight deferred pre-existing banned-API sites (`DateTime.Now`, `Task.Delay`) - follow-up time-seam migration.

**Recommended follow-up verification steps:**

1. Obtain maintainer ratification of the repo-wide coverage exception under `feature/csharp-coverage-uplift`.
2. Keep the focused issue #218 VSTest command in any future remediation that touches production code.

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.
- The five authoritative criteria in `issue.md` were already checked (lines 78-82) before this reaudit and remain correctly checked; no source-file change was required.

### AC Status Summary

- Source: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md` | 5 | 5 | 0 | Checkbox-backed minor-audit source; all five remain checked and pass after the cycle-2 split. |
