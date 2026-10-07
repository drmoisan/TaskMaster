# Remediation Inputs: Build Triage Classifier (#979)

Timestamp: 2026-10-06T23-01
Review-Verdict: REMEDIATION_REQUIRED
Base merge commit: `c76e830c18976221b5730f84b8d88aebbfc4f04b`
Reviewed head: `3a355e14a57109f5470fcf3b7d747351bade5804`

## Blocking Finding

### CR-979-2 / PA-979-2: Issue-specific tests widen files at or above the repository limit

Remediability: autonomous

The repository caps production code, tests, and reusable scripts at 500 physical lines. The reviewed branch adds issue-specific tests to three test files at or above that limit:

| File | Merge-base lines | Branch additions/removals | Reviewed-head lines | Required disposition |
|---|---:|---:|---:|---|
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 496 | +35/-0 | 531 | Move the new menu test to a focused file and keep the original file at or below 500 lines. |
| `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs` | 1,732 | +244/-0 | 1,976 | Move the complete appended `TriageClassifierRebuild_Tests` class to a dedicated file and restore the aggregate file to its merge-base content. |
| `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs` | 609 | +2/-0 | 611 | Remove the two issue-specific lines and retain equivalent Triage mapping verification in a compliant focused file. |

The baseline counts are derived from current physical line counts minus the no-deletion `git diff --numstat` additions. The two files already over 500 lines at the merge base are preexisting violations. This remediation must remove only issue #979 additions from them; broad restructuring of unrelated baseline tests is outside scope. New files must remain under 500 lines.

## Required Changes

1. Move `TriageClassifierRebuild_Tests` intact from `ClassifierGroups_Tests.cs` to a dedicated test file under `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/`, preserving all six test methods and the `ConfigurableManagerAsyncLazy` helper.
2. Move `RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu` into a focused ribbon XML test file. A partial `RibbonExplorerXmlTests` companion may reuse the existing private resource loader; if used, change the original declaration to `partial` and keep the original file at or below 500 lines.
3. Remove the two Triage additions from `EmailDataMiner_Tests.cs`. Preserve an explicit `EmailDataMiner.ToMinedMail` Triage assertion in a new focused partial test file or another compliant focused file. The existing partial-class test support may be reused.
4. Add every new test file to the applicable legacy `.csproj` compile list.
5. Preserve test names, behavior, MSTest/Moq/FluentAssertions conventions, and all issue #979 acceptance behavior.

## Expected Changed Files

- `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`
- New focused file under `TaskMaster.Test/Ribbon/` for the Build Triage Classifier XML test
- `TaskMaster.Test/TaskMaster.Test.csproj`
- `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs`
- New focused `TriageClassifierRebuild_Tests.cs` under `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/`
- `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs`
- New focused EmailDataMiner Triage-mapping test file under `UtilitiesCS.Test/EmailIntelligence/`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

No production file should require a behavior change.

## Acceptance and Verification

- `RibbonExplorerXmlTests.cs` is at or below 500 physical lines.
- Both preexisting oversized UtilitiesCS aggregate test files contain no issue #979 additions relative to the merge base.
- Every new test file is below 500 physical lines and is explicitly compiled by its legacy test project.
- The six classifier-rebuild tests, ribbon XML test, and EmailDataMiner Triage mapping test remain discoverable and pass.
- `dotnet tool run csharpier format .` completes; if it changes files, restart the ordered gate.
- `dotnet tool run csharpier check .` passes.
- The full analyzer/code-style rebuild passes with zero new diagnostics.
- The full warnings-as-errors compiler/nullable rebuild passes with zero new diagnostics.
- UtilitiesCS and TaskMaster standard-QC VSTest suites pass; TaskMaster continues to exclude `TestCategory=LiveOutlook` under its existing standard-QC contract.
- `git diff --check` passes against merge base `c76e830c18976221b5730f84b8d88aebbfc4f04b`.
- A final line-count/diff comparison records the three remediated source files and all new test files.

The user's one-time exception applies to coverage requirements only for issue #979. Record coverage if the canonical test command produces it, but coverage thresholds and missing numeric PowerShell coverage do not block this remediation. Functional tests and every non-coverage gate remain mandatory.

## Do Not Widen Scope

- Do not change production behavior.
- Do not restructure unrelated tests in the preexisting 1,732-line or 609-line aggregate files.
- Do not weaken or edit repository policy documents.
- Do not expand the issue-979 coverage exception to functional, formatting, analysis, compiler, test, or file-size requirements.
- Do not create temporary files from unit tests.
- Do not drop assertions or test scenarios during extraction.

## Context Package

- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Policy audit: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-00-audit/policy-audit.2026-10-06T23-00.md`
- Code review: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-00-audit/code-review.2026-10-06T23-00.md`
- Feature audit: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-00-audit/feature-audit.2026-10-06T23-00.md`
- Authoritative acceptance criteria: `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md`
- Original feature plan: `docs/features/active/2026-10-06-build-triage-classifier-979/plan.2026-10-06T19-29.md`
- Earlier remediation plan: `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T21-49-remediation/remediation-plan.2026-10-06T21-49.md`
- Latest C# QA evidence: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p2-t5-vstest-coverage.2026-10-06T22-51.md`
- Coverage authorization record: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/other/coverage-exception.2026-10-06T21-37.md`

## Planner Target

Write the executor-ready atomic plan to:

`docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-01-remediation/remediation-plan.2026-10-06T23-01.md`
