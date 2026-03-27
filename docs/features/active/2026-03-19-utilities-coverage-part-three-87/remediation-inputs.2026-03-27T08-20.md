# Remediation Inputs — utilities-coverage-part-three-87 (2026-03-27T08-20)

## Required fixes

1. **Raise `UtilitiesCS` coverage to the documented threshold.**
   - **Files / locations:** `coverage/coverage.cobertura.xml` currently reports `UtilitiesCS` package line-rate `0.6978814543390189`; representative remaining low files include:
     - `UtilitiesCS/Dialogs/InputBox.cs`
     - `UtilitiesCS/Dialogs/MyBox.cs`
     - `UtilitiesCS/Dialogs/NotImplementedDialog.cs`
     - `UtilitiesCS/Dialogs/YesNoToAll.cs`
     - `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`
     - `UtilitiesCS/Extensions/AsyncSerialization.cs`
     - `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNew.cs`
     - `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs`
     - `UtilitiesCS/HelperClasses/ToolTips/TipsController.cs`
   - **Expected behavior:** The next coverage-enabled MSTest run must place the `UtilitiesCS` package and the remaining non-skip production files at or above the documented `80%` threshold.
   - **Acceptance criteria:** AC1 from `v2/user-story.md` and `v2/spec.md`.
   - **Verification commands:**
     1. `dotnet tool run csharpier format .`
     2. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
     3. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
     4. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

2. **Remove residual non-`#87` diff content before PR review.**
   - **Files / locations:**
     - `VBFunctions.Test/ComputerInfo_Test.cs`
     - `VBFunctions.Test/VBFunctions.Test.csproj`
     - `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/audit-2026-03-26T09-40/**`
   - **Expected behavior:** The branch diff relative to `development` should contain only issue `#87` implementation, test, and current review artifacts.
   - **Acceptance criteria:** PR-review readiness requirement derived from the refreshed baseline diff and review verdict.
   - **Verification commands:**
     1. `git diff --shortstat development...HEAD`
     2. `git diff --name-status development...HEAD`

3. **Keep feature documents aligned with actual completion state.**
   - **Files / locations:**
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/plan.2026-03-22T21-00.md`
   - **Expected behavior:** Do not mark the feature complete or check off the main coverage criterion until the refreshed coverage run proves the threshold is met.
   - **Acceptance criteria:** AC1 and documentation-completion consistency.
   - **Verification commands:**
     1. Re-run the full C# QA loop.
     2. Confirm the main coverage checkbox remains unchecked until the threshold is satisfied.

## Do not do

- Do not weaken the repo coverage requirement or change the coverage tooling configuration to force a pass.
- Do not broaden skip-candidate scope without new documented rationale.
- Do not keep unrelated issue `#96`, `VBFunctions.Test`, or stale audit content in the final PR branch.
- Do not mark acceptance criteria complete based on historical evidence alone.
- Do not skip the required format → analyzer → nullable → test sequence.

## Acceptance criteria not yet met

1. **Every `.cs` file compiled by `UtilitiesCS.csproj` has `>= 80%` line coverage as reported by Cobertura.**
   - **Minimum change required:** Add or refine tests for the remaining low-coverage `UtilitiesCS` production files until the refreshed Cobertura result clears the threshold.

## Review-time baseline used for remediation

- Refreshed diff: `279 files changed, 25608 insertions(+), 1387 deletions(-)` relative to `development`
- Review-time tests: `3415` total, `3413` passed, `2` skipped
- Review-time `UtilitiesCS` package coverage: `69.79%`
