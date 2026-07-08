# Remediation Inputs: Coverage Gaps Test Seams (#236)

Timestamp: 2026-07-04T17:29:43.5923638-04:00
ReviewStatus: REMEDIATION_REQUIRED
PrimaryFinding: AC8 repository-wide coverage remains below the required 80.00% threshold.

## Required Fix List

1. File paths: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/spec.md`, `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/user-story.md`, and coverage evidence under `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/`.
   Expected behavior: AC8 is checked only when repository-wide line coverage is at least 80.00%, issue #236 changed/new non-exempt coverage is at least 90.00%, per-file changed/new coverage is at least 90.00%, target coverage remains adequate, and no coverage exemptions are added.
   Verification commands: `dotnet tool run csharpier format .`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <feature evidence path>`.

2. File paths: C# production/test files required to raise repository-wide coverage.
   Expected behavior: Add deterministic tests and seams as needed without broad behavior changes, live Outlook dependencies, temporary files, or coverage exemptions.
   Verification commands: focused MSTest runs for touched areas followed by the full C# toolchain.

## Do Not Do

- Do not add `[ExcludeFromCodeCoverage]` to issue #236 targets or other newly changed production code.
- Do not weaken `coverage.config`, `TaskMaster.runsettings`, or `scripts/vscode/TaskMaster.cli.runsettings`.
- Do not check AC8 until the threshold artifact proves every AC8 gate.
- Do not broaden the feature beyond coverage remediation except where necessary to satisfy AC8.
- Do not skip CSharpier, analyzer build, nullable build, or full MSTest coverage.

## Required Context

- Policy audit: `docs\features\active\2026-07-04-coverage-gaps-test-seams-236\policy-audit.2026-07-04T17-29.md`
- Code review: `docs\features\active\2026-07-04-coverage-gaps-test-seams-236\code-review.2026-07-04T17-29.md`
- Feature audit: `docs\features\active\2026-07-04-coverage-gaps-test-seams-236\feature-audit.2026-07-04T17-29.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Current plan: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/plan.2026-07-04T13-15.md`
