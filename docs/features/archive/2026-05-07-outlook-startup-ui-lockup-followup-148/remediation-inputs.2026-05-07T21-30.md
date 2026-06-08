# Remediation Inputs — outlook-startup-ui-lockup-followup (Issue #148)

Timestamp: 2026-05-07T21-30
Feature Folder: `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148`
Base Branch: `development`
Trigger: Post-implementation feature review found blocking coverage, validation, scope-control, and structural policy gaps.
Authoritative Review Artifacts:
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/policy-audit.2026-05-07T21-30.md`
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/code-review.2026-05-07T21-30.md`
- `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/feature-audit.2026-05-07T21-30.md`

## Enumerated Fix List

1. **Close the C# coverage gate for the approved issue `#148` primary scope.**
   - **Files:** `TaskMaster/AppGlobals/AppEvents.cs`, `QuickFiler/Controllers/EfcHomeController.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`, and the mapped MSTest homes under `TaskMaster.Test/`, `QuickFiler.Test/`, and `UtilitiesCS.Test/`
   - **Expected behavior:** The refreshed branch must produce `Coverage Conclusion: PASS`, with repository coverage meeting the repo gate or a valid no-regression baseline and changed/new-code coverage `>= 90%`.
   - **Verification commands:**
     - `dotnet tool run csharpier format .`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
     - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

2. **Replace brittle source-text regressions with behavioral seam tests for the issue `#148` fix boundaries.**
   - **Files:** `TaskMaster.Test/AppGlobals/AppEventsTests.cs`, `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, `QuickFiler.Test/Controllers/EfcDataModelTests.cs`, `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`, `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs`, `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`
   - **Expected behavior:** The refreshed regression suite should verify observable stage boundaries, batching, snapshot handoff, cancellation, and publication cadence through runtime behavior or stable test seams rather than source-string matching.
   - **Verification commands:**
     - Focused MSTest runs for the affected project test assemblies
     - Full coverage-enabled MSTest run listed in item 1

3. **Reconcile actual branch scope with the declared feature scope before the final QA loop.**
   - **Files:** current unstaged changes recorded in `artifacts/pr_context.appendix.txt`, including unrelated QuickFiler test files and unstaged `.csproj` edits such as `QuickFiler/QuickFiler.csproj`, `SVGControl/SVGControl.csproj`, `Tags/Tags.csproj`, `TaskMaster/TaskMaster.csproj`, `TaskTree/TaskTree.csproj`, `TaskVisualization/TaskVisualization.csproj`, `ToDoModel/ToDoModel.csproj`, `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesSwordfish/UtilitiesSwordfish.NET.General.csproj`, plus any additional file not explicitly justified by issue `#148`
   - **Expected behavior:** Either remove the unrelated changes from this branch or explicitly promote them into scope with updated scope evidence before another final review.
   - **Verification commands:**
     - `git diff --name-status development...HEAD`
     - `git status --short`
     - Compare against `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/other/implementation-scope.2026-05-07T20-09-49-04-00.md`

4. **Bring changed production files back into structural compliance with the 500-line repository rule.**
   - **Files:** `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`
   - **Expected behavior:** The remediated branch should either extract focused helpers to reduce touched-file size below `500` lines or revert/split the affected changes so the branch no longer carries oversized modified files.
   - **Verification commands:**
     - `git diff --name-only development...HEAD`
     - `(Get-Content <file> | Measure-Object -Line).Lines` for each affected production file
     - full C# toolchain commands listed in item 1

5. **Complete the blocked manual-validation and end-state path after coverage passes.**
   - **Files:** `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/outlook-manual-validation.*.md`, `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/full-bug-end-state.*.md`
   - **Expected behavior:** Manual Outlook validation must run on the live repro path and confirm that Outlook repaints and accepts input during startup overlap and first-email interaction. The refreshed end-state artifact must then record `Ready For Validator: true`.
   - **Verification commands:**
     - Use the latest passing coverage summary as the prerequisite gate
     - Perform the manual Outlook validation steps documented in `spec.md`

## Do Not Do

- Do not weaken the repository coverage thresholds or bypass the saved `Coverage Conclusion: PASS` requirement.
- Do not mark the feature merge-ready while the live responsiveness criterion remains unverified.
- Do not keep unrelated `.csproj` or test-file churn in this branch without explicit scope evidence.
- Do not rely only on source-text assertions to claim runtime behavior is protected.
- Do not widen production scope beyond what is necessary to resolve the review findings.
