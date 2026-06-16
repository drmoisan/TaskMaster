# Remediation Inputs (Cycle 2): hierarchical-lcppn-folder-prediction (#177)

**Cycle:** 2 (proposed)
**Entry timestamp:** 2026-06-12T16-35 (UTC)
**Authored by:** feature-reviewer (cycle-1 exit reaudit)
**Base:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head:** `TaskMaster-wt-2026-06-08-12-06` (`e159bead`)

## Trigger

The cycle-1 end-of-cycle reaudit (artifacts dated 2026-06-12T16-35) confirms the two in-scope cycle-1 findings (F1 Major, F2 Minor) are resolved, but surfaces one new FAIL-level policy finding introduced by the cycle-1 F2 coverage work. `blocking_count == 1` (one FAIL). The exit gate is not clean.

## Source audit artifacts

- `policy-audit.2026-06-12T16-35.md` (Section 2.3 — file-size cap, NEW FAIL)
- `code-review.2026-06-12T16-35.md` (Findings Table — Major)
- `feature-audit.2026-06-12T16-35.md` (AC20 — FAIL)

## In-scope findings for cycle 2

### F3 [Major/FAIL, REQUIRED] New test file exceeds the 500-line cap

- File: `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs`.
- Problem: the file is 554 lines (verified `awk END{NR}` = 554, `wc -l` = 554). It was 418 lines at the pre-cycle-1 head `d06f5c00` and crossed the 500-line cap in cycle-1 commit `e159bead` when plan task P2-T2 added 136 lines of F2 branch-coverage tests. The file is NEW in the branch (absent at the merge-base), so this is not a pre-existing overage; the cycle-1 remediation created the violation. The General Code Change Policy applies the 500-line cap to test code with exceptions only for throwaway scripts and raw text fixtures, neither of which applies. AC20 explicitly forbids any new test file over 500 lines.
- Required outcome: split `LcppnFolderPredictor_Tests.cs` into two (or more) cohesive MSTest files, each under 500 lines (e.g., one for descent/beam/abstention behavior and one for construction/serialization/branch-coverage). Add a matching `<Compile Include>` entry for every new test file in `UtilitiesCS.Test.csproj`. Do not weaken or remove any test; preserve the F2 strict coverage (`FolderHierarchyTree` 100.00%, `LcppnFolderPredictor` 97.71%) and all current assertions. Keep tests deterministic, in-memory, COM-free, no temp files.
- Verification: each resulting test file is < 500 lines; the full C# toolchain passes in a single final pass (CSharpier → analyzers → nullable → vstest); F2 strict per-type coverage is re-confirmed >= 90% from the canonical `artifacts/csharp/coverage.xml`.
- HARD constraint (carries over): do not retype the shared `Globals.AF.Manager` value type, do not modify `ManagerAsyncLazy.cs`, and do not touch the out-of-scope classifier subsystems (`Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`).

## Out-of-scope for cycle 2 (recorded, not remediated here)

- `BayesianClassifierGroup.cs` 515 lines, `FolderScorer.cs` 608, `SortEmail.cs` 1406 (pre-existing over-cap modified files; separate refactor). Accepted; no action.
- `FolderHierarchyNode.cs` 60.0% strict / 100.0% inclusive (auto-generated record members). Accepted; no action.

## Exit condition for cycle 2

End-of-cycle feature-review (three reaudit artifacts) must show `blocking_count == 0` AND every test file in the branch diff under the 500-line cap AND F2 strict per-type coverage preserved (>= 90%) with repo-wide UtilitiesCS.dll >= 80% and no regression, with the full C# toolchain green in a single final pass.
