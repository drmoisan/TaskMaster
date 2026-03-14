# Change Plan

## Objective
Restore solution build after the recent NuGet package upgrades by identifying and fixing the current compile errors with minimal code changes.

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
- [ ] Fixes applied
- [ ] Solution build passes
- [ ] Relevant tests run

## Current Findings
- Current workspace repro after unloading projects shows the active build failures are limited to MSTest v4 removing `ExpectedException` in `UtilitiesCS.Test`.
- The affected files are `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` and `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs`.
