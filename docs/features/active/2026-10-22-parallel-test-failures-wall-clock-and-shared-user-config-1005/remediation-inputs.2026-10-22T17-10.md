# Remediation Inputs: Issue #1005 (review 2026-10-22T17-10)

Review-Verdict: REMEDIATION_REQUIRED

Remediation-Mode: proportionate

- Feature folder: `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005`
- Branch head reviewed: `e399de597`; merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72`
- Source audit artifacts:
  - `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/policy-audit.2026-10-22T17-10.md`
  - `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/code-review.2026-10-22T17-10.md`
  - `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/feature-audit.2026-10-22T17-10.md`
- Next link: `atomic-planner` authors the remediation plan per `remediation-handoff-atomic-planner`; this review does not author the plan.
- Mode rationale: one test-only addition plus a correction to one evidence summary; no production code, acceptance criterion or design decision changes.

## Remediation-Required Findings

### B-1 `IDList.MaxLengthOfID` lost all test coverage; `IDList.cs` branch coverage regressed

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the fix is a test-only addition to an existing test file plus a regenerated evidence summary; the getter's behaviour is fully determined by the source and needs no maintainer input or external system.
- Files: `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` (235 lines at head); `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/evidence/qa-gates/coverage-comparison.2026-10-22T16-36.md` (superseded by a new timestamped comparison).
- Problem: the merge-base helper `CreateListWithCachedMaxLength` executed `_ = list.MaxLengthOfID;` and was the only test that ran the getter (`ToDoModel/Data Model/ID/IDList.cs` lines 104 to 114). Deleting it, as AC2 requires, dropped the getter from 7/7 lines and 4/4 branches covered (baseline Cobertura, method `get_MaxLengthOfID`) to 0/7 and 0/4 (`artifacts/csharp/coverage.xml`). The file's branch rate fell from 20.69% to 19.35% while the file is below the 80% / 75% floor. The committed coverage comparison reports only line rates for `IDList.cs` and attributes its uncovered residue to the unchanged constructors.
- Expected state after the fix: `get_MaxLengthOfID` has every line and both branch lines covered, so `IDList.cs` line and branch rates are not below the merge-base rates (24.36% line, 20.69% branch), and a new coverage comparison reports both rates for `IDList.cs` and names the getter.
- Fix:
  - In `IDListGetNextToDoIDTests.cs`, add C# 7.3-compatible MSTest tests with an injected `Mock<ISettingsStore>`:
    - cold cache: `new IDList(new List<string> { "02", "0100" }, store.Object).MaxLengthOfID` returns 4 (exercises the `_maxIDLength == 0` true branch and the `Max` lambda). Line 110 carries a second compiler-generated branch (the cached-lambda initialization), which the merge-base suite covered through four separate cold-cache reads; read a cold cache on at least two distinct lists so both arms execute in one test run;
    - warm cache: on a store-injected list, call `GetNextToDoID("ZZ")` (caches 4), then read `MaxLengthOfID` and assert 4 without recomputation (exercises the false branch); assert `store.Verify(s => s.Save(), Times.Once())` so the read does not persist.
  - Use a non-empty list in every new test: on an empty list with a zero cache the getter throws `InvalidOperationException` (pre-existing; out of scope).
  - Keep the file at or under 500 lines; do not reintroduce the cache-priming helper.
  - Run the full C# toolchain in CLAUDE.md order (CSharpier format and check, analyzer `/t:Rebuild`, warnings-as-errors `/t:Rebuild`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`), restarting from format if any step changes files or fails.
  - Write a new timestamped coverage comparison under `evidence/qa-gates/` that reports `IDList.cs` line and branch rates at baseline and head and the `get_MaxLengthOfID` member figures; leave the earlier file in place.
- Verification commands:
  - Grep `artifacts/csharp/coverage.xml` for `name="get_MaxLengthOfID"` and confirm `line-rate="1" branch-rate="1"`.
  - Grep `artifacts/csharp/coverage.xml` for `filename="ToDoModel\Data Model\ID\IDList.cs"` and confirm `branch-rate` at or above 0.206897 and `line-rate` at or above 0.24359.
  - `git diff 346651242a2bd6ed25d703a4b1ecb5a055c6ae72..HEAD --stat -- ToDoModel/ UtilitiesCS/ TaskMaster/` shows no new production change from the remediation commit.

## Non-Blocking Items

- N-1: `TaskMaster/AppGlobals/AppAutoFileObjects.cs` per-file line rate fell from 45.66% to 40.81% only because covered scalar-property lines moved into `AppAutoFileObjects.ScalarSettings.cs`; no action required.
- O-1: `artifacts/pr_context.summary.txt` and `.appendix.txt` describe the #1002 branch; regenerate them before the PR is authored.
- O-2: the TaskMaster CI checks have not run (no PR yet); they remain a PR-time gate.
- O-3 to O-8: informational observations recorded in the policy audit (section 8) and code review (findings table); no action required.

## Do Not Do

- Do not change production code; B-1 is test-only.
- Do not add `[ExcludeFromCodeCoverage]`, `coverage.config` entries, `[DoNotParallelize]`, sleeps, delays, retries or temporary files.
- Do not reintroduce `CreateListWithCachedMaxLength` or any test that reaches `ApplicationSettingsBase.Save()` on a real `Properties.Settings.Default`.
- Do not edit, uncheck or add acceptance criteria in `spec.md`.
- Do not address the in-memory `Settings.Default` mutations tracked by issue #1006.
- Do not commit raw `.trx`, `.coverage` or Cobertura documents; evidence stays as Markdown summaries and package-level JaCoCo projections under `evidence/`.
- Do not edit policy documents under `.claude/rules/` or `.github/instructions/`.
