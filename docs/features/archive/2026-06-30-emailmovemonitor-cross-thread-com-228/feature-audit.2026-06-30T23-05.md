# Feature Audit: EmailMoveMonitor cross-thread COM fix (#228)

**Audit Date:** 2026-06-30
**Feature Folder:** `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-06-30-17-46` (commit `174b2650a6ce52bd41cc38ac75a556a38d9ad8fd`)
**Work Mode:** `full-bug`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-30-17-46` (commit `174b2650a6ce52bd41cc38ac75a556a38d9ad8fd`)
- **Merge base:** `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/evidence/**`
  - Additional evidence: direct `git diff 4611fd60..174b2650` inspection of the changed `.cs` and `.csproj` files
- **Feature folder used:** `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228`
- **Requirements source:** `spec.md` (AC1–AC9)
- **Work mode resolution note:** `issue.md` records `- Work Mode: full-bug`. Per the work-mode contract, `full-bug` resolves the acceptance-criteria source to `spec.md` only.
- **Scope note:** The audit scope is the full branch diff against the merge-base. The PR-context summary overview classified the change as "Core logic changes: 0 files" / "Docs/templates/agents/tooling: 16 files"; this is a known summary-overview misclassification of C# changes. Direct `git diff` inspection confirms 5 modified and 2 new `.cs` files plus 2 `.csproj` edits, so C# is the in-scope changed language and was audited accordingly. No caller-supplied scope narrowing was applied.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/spec.md` — only source (work mode `full-bug`)

### Acceptance criteria

1. AC1: No Outlook COM member access (`mail.Parent`, `Folder.EntryID`, `BeforeItemMove +=/-=`) in `EmailMoveMonitor` executes on a ThreadPool/background thread; all such access is marshaled to the captured Outlook STA thread.
2. AC2: The redundant `Task.Run` wrapper around the unhook loop in `QfcDatamodel.DequeueNextItemGroupAsync` is removed; the method's returned-node behavior is unchanged.
3. AC3: `EmailMoveMonitor` is consumed through `IEmailMoveMonitor` with an injectable marshal-to-STA delegate that defaults to the existing `UiThread` seam; tests substitute a deterministic pass-through.
4. AC4: Regression/unit tests added and passing.
5. AC5: Changed/new `EmailMoveMonitor` bookkeeping code reaches >=90% line coverage; repo-wide coverage no-regression on changed lines (testable denominator); COM-host-bound exemption documented and scoped.
6. AC6: No banned-API regressions; existing `TimeProvider.Delay` preserved.
7. AC7: No unintended behavior changes outside the defined scope; existing log4net logging in `TryUnhookOrReplace` preserved.
8. AC8: Full toolchain pass completed in order with no failures in the final pass.
9. AC9: Spec/issue references updated to reflect the implemented behavior.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | All EmailMoveMonitor COM access marshaled to STA thread | PASS | `EmailMoveMonitor.cs` — `HookItem` (50-60), `UnhookItem` (72-87), `UnhookAll` (189-199) all wrap COM access in `_marshalToSta(...)`. Test `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread` proves the COM-access body runs on the marshal-target thread, not the invoking ThreadPool thread. | `git diff` of EmailMoveMonitor.cs; `vstest.console.exe ... /EnableCodeCoverage` | The BeforeItemMove handler body stays STA-bound by Outlook contract (intentionally not re-marshaled). |
| 2 | Redundant `Task.Run` unhook wrapper removed; returned-node behavior unchanged | PASS | `QfcDatamodel.QueueProcessing.cs` diff — `await Task.Run(...)` removed; the `for` loop calling `TryUnhookOrReplace` runs directly inside the preserved try/catch; `return nodes;` unchanged. | `git diff` of QfcDatamodel.QueueProcessing.cs | Commented-out `UnhookItemAsync` path remains commented out (not re-activated). |
| 3 | Consumed via `IEmailMoveMonitor` with injectable marshal delegate defaulting to `UiThread`; tests pass-through | PASS | `IEmailMoveMonitor.cs` (new); `EmailMoveMonitor(Action<System.Action> marshalToSta = null)` defaults to `action => UiThread.Dispatcher.Invoke(action)`; `_moveMonitor` fields in `QfcDatamodel.cs`, `QfcQueue.cs`, `QfcCollectionController.cs` typed `IEmailMoveMonitor`; tests use `a => a()` pass-through. | `git diff` of the four consumer files + interface | Smallest seam per `.claude/rules/csharp.md` DI-seam ordering. |
| 4 | Regression/unit tests added and passing | PASS | `EmailMoveMonitorTests.cs` — 8 tests; suite 209/209 passed, EXIT 0. | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` | MSTest + Moq + FluentAssertions; deterministic, no temp files. |
| 5 | Changed/new bookkeeping >=90%; no changed-line regression (testable denominator); COM-host-bound exemption documented | PASS | Changed/new bookkeeping = 96.92% (63/65). QuickFiler first-party package 32.94% -> 33.74% (no changed-line regression). Exempt/non-exempt boundary documented in `evidence/qa-gates/coverage-delta.2026-06-30T18-10.md`. | inspect `evidence/qa-gates/coverage-delta.2026-06-30T18-10.md` | Repo-wide testable-denominator floor (<80%) is a pre-existing, maintainer-ratified, authority-scoped condition under `feature/csharp-coverage-uplift`, not introduced by #228. Canonical `artifacts/csharp/coverage.xml` is absent (Minor/Info gap, see policy-audit Section 8); does not change the PASS because numeric coverage is documented and traceable. |
| 6 | No banned-API regressions; `TimeProvider.Delay` preserved | PASS | `evidence/qa-gates/qa-analyzers.2026-06-30T18-10.md` — BannedApiAnalyzers no diagnostics for changed files; no `DateTime.Now/UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay` introduced; `TimeProvider.Delay` in `WaitForQueue` unchanged. | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Test file uses `Thread`/`Task.Run` which are NOT in the banned set (only `Thread.Sleep`/`Task.Delay`). |
| 7 | No unintended behavior changes outside scope; log4net logging in `TryUnhookOrReplace` preserved | PASS | `TryUnhookOrReplace` body unchanged; `DequeueNextItemGroupAsync` try/catch logging ("Error unhooking items from move monitor") preserved; dormant async members not re-wired. | `git diff` of QfcDatamodel.QueueProcessing.cs | Field-type-only edits in the three consumers; no behavioral change. |
| 8 | Full toolchain pass in order, no failures in final pass | PASS | qa-csharpier EXIT 0; qa-analyzers EXIT 0; qa-nullable EXIT 0; qa-tests-coverage EXIT 0 (209/209). | `csharpier check .`; two `msbuild` builds; `vstest.console.exe` | Nullable: first-party clean; 50 vendored `UtilitiesSwordfish.NET.General` errors are a pre-existing baseline in an excluded vendored project per `.claude/rules/csharp.md`. |
| 9 | Spec/issue references updated to reflect implemented behavior | PASS | `spec.md` Status -> "Implemented (pending review/merge)", AC1–AC9 checked; issue-update mirror at `evidence/issue-updates/issue-228.2026-06-30T18-10.md`. | inspect `spec.md`, `evidence/issue-updates/issue-228.2026-06-30T18-10.md` | — |

---

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 9 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None. All nine acceptance criteria are satisfied with verified evidence.

**Recommended follow-up verification steps:**

1. Emit the canonical machine-readable C# coverage artifact (`artifacts/csharp/coverage.xml`, or commit the cobertura XML under `evidence/qa-gates/`) on the next run for traceability. Non-blocking.
2. Continue the repo-wide testable-denominator coverage uplift under `feature/csharp-coverage-uplift`. Out of scope for #228.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if represented as markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

All nine criteria in `spec.md` are already represented as `- [x]` checkboxes (checked off by the executor). This audit independently verified each as PASS; no checkbox state change was required. No new criteria were added.

### AC Status Summary

- Source: `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/spec.md`
- Total AC items: 9
- Checked off (delivered): 9
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `docs/features/active/2026-06-30-emailmovemonitor-cross-thread-com-228/spec.md` | 9 | 9 | 0 | Checkbox-backed; all already checked, all independently verified PASS |
