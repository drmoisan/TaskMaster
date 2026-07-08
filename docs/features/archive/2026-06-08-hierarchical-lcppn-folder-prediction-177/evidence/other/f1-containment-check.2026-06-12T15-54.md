# F1 Containment Check — HARD Constraints (#177 Cycle 1)

- Timestamp: 2026-06-12T17-22 (UTC)
- Task: [P3-T4]
- Command: `git diff --stat` (filtered)

## Prohibited files — ZERO diff (verified)

`git diff --stat -- "*ManagerAsyncLazy.cs" "*Triage.cs" "*SpamBayes.cs" "*CategoryClassifierGroup.cs" "*MulticlassEngine.cs"`
returned no output: none of the prohibited files were modified.

- `ManagerAsyncLazy.cs` — zero diff.
- `Triage.cs` — zero diff.
- `SpamBayes.cs` — zero diff.
- `CategoryClassifierGroup.cs` — zero diff.
- `MulticlassEngine.cs` — zero diff.

## Shared Manager value type — unchanged

The `Globals.AF.Manager` value type
`ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` (declared via
`ManagerAsyncLazy : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`) was
NOT retyped. `ManagerAsyncLazy.cs` line 27-28 still declares the original base type (grep confirmed the
exact generic token present once, file untouched).

## Touched files (exactly the expected F1/F2 set)

| File | Lines | Purpose |
|---|---|---|
| `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` | +9 | F1: declare `IFolderPredictor FolderPredictor { get; set; }` holder |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | +8 | F1: implement the `FolderPredictor` auto-property |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` | +/-24/12 | F1: route accessor/build/SetLcppnPredictor through `Globals.AF.FolderPredictor`; remove `_lcppnPredictor` field |
| `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` | +63 | F1: shared-holder mock setup + 2 regression tests |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/FolderHierarchyTree_Tests.cs` | +112 | F2: targeted coverage tests |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` | +136 | F2: targeted coverage tests |

No `.csproj` files changed (both F2 test files were already registered; no new file was created).

## Result

All HARD constraints held: prohibited files have zero diff, the shared `Manager` value type is
unchanged, and only the four expected production/interface files and the three test files were modified.
