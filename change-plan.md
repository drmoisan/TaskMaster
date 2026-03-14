# Change Plan

## Objective
Restore solution build after the recent NuGet package upgrades by identifying and fixing the current compile errors with minimal code changes.

## Current Task
Redirect legacy `OlJunkPotential` setting usage to `JunkPotential` so the add-in loads and saves the same folder path setting.

## Assumptions
- The current solution build failures are caused by API or behavioral changes introduced by upgraded packages.
- The goal is to get the solution building again without unrelated refactoring.

## Plan
1. Run a full solution build and capture the current compiler errors.
2. Group failures by root cause and inspect the affected files.
3. Apply minimal code changes to restore compatibility with the upgraded packages.
4. Rebuild and iterate until the solution builds cleanly.
5. Run relevant tests for the touched projects where practical, then update this plan with results.

## Status
- [x] Plan created
- [x] Build errors captured
- [x] Fixes applied
- [x] Solution build passes
- [x] Relevant tests run

## Current Findings
- Current workspace repro after unloading projects shows the active build failures are limited to MSTest v4 removing `ExpectedException` in `UtilitiesCS.Test`.
- The affected files are `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` and `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs`.
- `TaskMaster/AppGlobals/AppOlObjects.cs` loads `JunkPotential` but persists the manually selected folder to legacy `OlJunkPotential`, which can cause the prompt to reappear on next startup.
- `TaskMaster/Properties/Settings.settings` and `TaskMaster/app.config` no longer define `OlJunkPotential`; the only remaining source hit is the generated `TaskMaster/Properties/Settings.Designer.cs` accessor, which requires regeneration of the settings designer rather than a manual edit.
- Validation results for this task: both solution build commands passed and the focused `AppOlObjects` regression tests passed. The repo-required `dotnet format TaskMaster.sln` command still fails in this environment during MSBuild workspace initialization before formatting begins.
