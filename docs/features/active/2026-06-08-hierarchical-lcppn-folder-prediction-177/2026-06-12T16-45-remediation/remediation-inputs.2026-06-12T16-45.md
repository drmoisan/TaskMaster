# Remediation Inputs (Cycle 2): hierarchical-lcppn-folder-prediction (#177)

**Cycle:** 2
**Entry timestamp:** 2026-06-12T16-45 (UTC)
**Authored by:** orchestrator
**Base:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Head:** `TaskMaster-wt-2026-06-08-12-06` (`e159bead` + cycle-1 exit audits commit)

## Trigger

The cycle-1 end-of-cycle reaudit (artifacts dated 2026-06-12T16-35) confirmed both
cycle-1 objectives (F1 flag-on reachability, F2 strict coverage) were met, but
surfaced one NEW FAIL with `blocking_count = 1`. Per the scope-change rule, this new
finding opens cycle 2.

## Single in-scope finding for cycle 2

### F3 [FAIL / AC20, REQUIRED] New test file over the 500-line cap

- File: `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` — **554 lines** (was 418 at `d06f5c00`; the cycle-1 F2 coverage tests added ~136 lines, pushing it over the 500-line cap).
- Policy: General Code Change Policy file-size limit — no test file may exceed 500 lines. AC20 explicitly forbids a new test file over the cap. This is not a pre-existing overage (the file is new to this feature), so no grandfather exception applies.
- Required outcome: split `LcppnFolderPredictor_Tests.cs` into two or more cohesive test files, each <= 500 lines, preserving ALL existing test cases and the strict coverage achieved in cycle 1 (`LcppnFolderPredictor` >= 90% strict; current 97.71%). Group the split by behavior (for example: construction/config/training in one file; classify/beam-descent/abstention/serialization-adjacent in another) so each file has a clear purpose.
- Non-SDK registration: every resulting test file (the trimmed original plus any new file) MUST have an explicit `<Compile Include="...">` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. The original file is already registered; register any newly created file.
- Verification: after the split, all previously passing `LcppnFolderPredictor` tests still pass; strict line coverage of `LcppnFolderPredictor` remains >= 90%; every resulting test file is <= 500 lines; full C# toolchain green in a single final pass.

## Out-of-scope for cycle 2 (recorded, not remediated here)

- Pre-existing over-cap files `BayesianClassifierGroup.cs` (515; was 513 pre-feature),
  `FolderScorer.cs` (608), `SortEmail.cs` (1406): pre-existing overages, separate
  refactors outside #177's scope. Not remediated here.
- `FolderHierarchyNode.cs` strict coverage (auto-generated record members; inclusive
  100%): accepted; no action.
- Pre-existing flaky `IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue...`
  (`ci-flaky-test-isolation-176`): out of scope; passes in isolation.

## Exit condition for cycle 2

End-of-cycle reaudit (three reaudit artifacts) must show `blocking_count == 0`: the
F3 over-cap file split into files each <= 500 lines, `LcppnFolderPredictor` strict
coverage still >= 90%, no test loss, containment still held (ManagerAsyncLazy.cs and
the four out-of-scope subsystems zero diff), and the full C# toolchain green.
