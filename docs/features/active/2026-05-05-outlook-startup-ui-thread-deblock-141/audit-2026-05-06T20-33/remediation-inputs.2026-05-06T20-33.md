# Remediation Inputs — outlook-startup-ui-thread-deblock (Issue #141)

Timestamp: 2026-05-06T20-33
Feature Folder: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141`
Base Branch: `development`
Trigger: Post-implementation feature review found blocking coverage, scope, and PowerShell-policy gaps.
Authoritative Review Artifacts:
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/policy-audit.2026-05-06T20-33.md`
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/code-review.2026-05-06T20-33.md`
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/feature-audit.2026-05-06T20-33.md`

## Enumerated Fix List

1. **Close the C# coverage gate for the approved startup-fix scope.**
	 - **Files:** `TaskMaster/AppGlobals/ApplicationGlobals.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `TaskMaster/AppGlobals/AppToDoObjects.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, associated MSTest files under `TaskMaster.Test/AppGlobals/`, `TaskMaster.Test/OutlookObjects/Store/`, and `UtilitiesCS.Test/OutlookObjects/Store/`
	 - **Expected behavior:** The updated branch must produce `Coverage Conclusion: PASS`, with changed/new-code coverage `>=90%` and no repo-wide regression attributable to the branch.
	 - **Verification commands:**
		 - `dotnet tool run csharpier format .`
		 - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
		 - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
		 - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

2. **Reconcile the retained `async void` deserialization hook in `StoresWrapper`.**
	 - **Files:** `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, related MSTest coverage files
	 - **Expected behavior:** The final implementation must have a single clearly supported store-rewire completion contract, or a documented/tested reason why both the legacy deserialization callback and the explicit await path are required.
	 - **Verification commands:**
		 - Focused MSTest execution for the store-rewire tests recorded in `targeted-regression.2026-05-06T14-37-21.md`
		 - Full C# toolchain commands listed in item 1

3. **Reconcile branch scope to match the approved implementation budget.**
	 - **Files:** `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`, `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-VSBuild.ps1`, `scripts/vscode/TestProcessCleanup.ps1`, and the project/config churn recorded in `artifacts/pr_context.appendix.txt`
	 - **Expected behavior:** Either remove/split unrelated changes from this branch, or update scope-control artifacts to promote them explicitly and rerun review gates for the expanded scope.
	 - **Verification commands:**
		 - `git diff --name-status development...HEAD`
		 - Review against `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/other/implementation-scope.2026-05-05T09-23-00.md`

4. **If PowerShell tooling changes remain in scope, satisfy the repository PowerShell toolchain.**
	 - **Files:** `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-VSBuild.ps1`, `scripts/vscode/TestProcessCleanup.ps1`
	 - **Expected behavior:** The changed scripts must pass repo-required formatting, analysis, and test validation, with evidence captured for the feature folder or an equivalent agreed remediation evidence location.
	 - **Verification commands:**
		 - `mcp_drmcopilotext_run_poshqc_format`
		 - `mcp_drmcopilotext_run_poshqc_analyze`
		 - `mcp_drmcopilotext_run_poshqc_test`

5. **Complete the blocked manual-validation and end-state path after coverage passes.**
	 - **Files:** `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/outlook-manual-validation.*.md`, `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/full-bug-end-state.*.md`
	 - **Expected behavior:** Manual Outlook startup validation must run on a representative multi-store profile and confirm responsiveness, startup timing evidence, and no regression of the COM-safety fixes from issues `#124`, `#126`, and `#128`.
	 - **Verification commands:**
		 - Use the latest passing Phase 6 QA evidence as the prerequisite gate
		 - Perform the manual Outlook validation steps documented in `spec.md` and record the PASS-path artifact contents required by `plan.2026-05-05T08-43.md`

## Do Not Do

- Do not widen production scope beyond what is required to close the documented review findings.
- Do not weaken the coverage threshold, manual-validation sequencing, or PowerShell policy requirements.
- Do not silently keep out-of-scope changes in this branch without explicit scope evidence.
- Do not replace deterministic regression tests with broader but less diagnostic suite-only coverage.
- Do not mark the feature ready to merge until the latest coverage summary is `PASS` and the manual Outlook validation PASS artifact exists.
