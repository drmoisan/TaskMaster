# Remediation Inputs — TaskMaster Ribbon Tab (Issue #185)

**Entry Timestamp:** 2026-06-12T11-37
**Cycle:** Remediation cycle 2 entry (authored at cycle-1 exit re-audit)
**Feature Folder:** `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185`
**Base Branch:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head:** `2fcd1581e26f360ae54aa6cd79f14ca0d1326db5`

## Source Audit Artifacts

The findings below were produced by the cycle-1 exit re-audit:

- `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/policy-audit.2026-06-12T11-37.md`
- `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/code-review.2026-06-12T11-37.md`
- `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/feature-audit.2026-06-12T11-37.md`

## Blocking Findings (must be resolved before the loop can exit)

### R1 (BLOCKING) — Repository-wide C# line coverage below the >= 80% threshold

- **Finding:** The canonical Cobertura artifact `artifacts/csharp/coverage.xml` now exists (prior cycle's R1 absence is resolved), and the now-evaluable repository-wide C# line coverage is **58.94%** (root `line-rate="0.5893769565947007"`, lines-covered 101852 / lines-valid 172813). First-party-only line coverage from the same artifact is 77.61% (including test assemblies) and 60.49% (first-party production assemblies only). All three are below the mandatory >= 80% threshold defined in `.claude/rules/csharp.md` ("Repository-wide line coverage must remain >= 80%") and the feature-review coverage contract.
- **Cause:** Pre-existing repository condition. Under-covered first-party production assemblies include `TaskVisualization` (0.37%), `ToDoModel` (10.8%), `QuickFiler` (25.2%), `TaskMaster` (25.8%), and `Tags` (31.4%). The figure is also depressed by bundled third-party DLLs (Deedle, System.Linq.Async, FSharp.Core, log4net, FluentAssertions, Swordfish, Mono.Reflection, System.Interactive) and vendored projects (SVGControl). This shortfall is NOT caused by the #185 change; the #185 in-scope changed lines show no regression (new test class line-rate 1.00; the production change is a non-instrumentable XML resource).
- **Affected artifact path(s):** `artifacts/csharp/coverage.xml`; evidence `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/qa-gates/repo-wide-coverage.md`.
- **Expected resolution (choose one):**
  1. Raise repository-wide C# line coverage to >= 80% by adding MSTest/Moq/FluentAssertions tests to under-covered first-party production assemblies, then regenerate the canonical Cobertura artifact and confirm the repo-wide `line-rate` >= 0.80. This is a repository-scale effort beyond the #185 change.
  2. Record an explicit, authority-sourced policy exception that scopes the >= 80% C# coverage gate to changed/new code for this feature (citing the pre-existing nature of the shortfall and the fact that the #185 change introduces no new production IL). The exception must be authored by an appropriate authority, not by a worker or reviewer, and must be referenced in the next-cycle policy audit.
- **Verification commands:**
  - Repo-wide run: `vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /EnableCodeCoverage /InIsolation /ResultsDirectory:coverage-out`
  - Convert: `dotnet-coverage merge -f cobertura -o artifacts/csharp/coverage.xml coverage-out/<guid>/<run>.coverage`
  - Gate check: confirm the root `<coverage line-rate=...>` is >= 0.80 (or that an authority-recorded exception applies).

## Non-Blocking Findings (address opportunistically; do not block the loop)

### R2 (MINOR) — PR-context summary misclassifies the C# scope

- **Finding:** `artifacts/pr_context.summary.txt` "Changed files overview" reports "Core logic changes: 0 files" and omits the two changed C# files, although the appendix lists both correctly.
- **Expected resolution:** Regenerate the PR context summary so the changed-files overview lists `TaskMaster/Ribbon/RibbonExplorer.xml (+1/-1)` and `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs (+64/-0)`.
- **Verification command:** `grep -n "RibbonExplorer" artifacts/pr_context.summary.txt` should show both files in the "Changed files overview" block.

### R3 (INFO) — Pre-existing vendored nullable errors

- **Finding:** The nullable build exits non-zero with 84 errors confined to vendored projects `SVGControl` (34) and `UtilitiesSwordfish.NET.General` (50), excluded per `.claude/rules/csharp.md`. Identical to baseline; not attributable to #185.
- **Expected resolution:** No action this cycle. Documented baseline.

### R4 (INFO) — Out-of-scope flaky WinForms dispatcher test

- **Finding:** `UtilitiesCS.Test...AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` failed once on the P2 re-run and passes in isolation and in the P1-T1 repo-wide run.
- **Expected resolution:** Track as a separate flaky-test issue; not part of #185.

## Do Not Do

- Do not weaken, reword, or skip the >= 80% C# coverage threshold. If an exception is the chosen path, it must be an explicit authority-recorded exception, not a silent reinterpretation.
- Do not modify `RibbonExplorer.xml` group/control content; AC4 verbatim preservation must remain intact.
- Do not edit policy documents under `.claude/rules/` or `.github/instructions/`.
- Do not widen scope beyond #185. Adding repository-wide coverage tests is a large effort; if pursued, it must be scoped and tracked explicitly, not folded silently into the #185 branch.
- Do not write evidence to non-canonical paths; all evidence goes under `docs/features/active/2026-06-12-taskmaster-ribbon-tab-185/evidence/<kind>/`.
- Do not mask the flaky dispatcher test with sleeps, retries, or timing hacks.

## Handoff

Per `remediation-handoff-atomic-planner`, the orchestrator authors this `remediation-inputs.2026-06-12T11-37.md`, then delegates to `atomic-planner` to author `remediation-plan.2026-06-12T11-37.md` (validated via `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"`), preflighted and executed by `atomic-executor`, then reaudited by `feature-review` at a fresh exit timestamp. The decision between resolution option 1 (raise coverage) and option 2 (authority exception) is an orchestrator/authority decision, not a worker decision.
