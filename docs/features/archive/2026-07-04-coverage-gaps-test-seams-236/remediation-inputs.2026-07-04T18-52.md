# Remediation Inputs: Coverage Gaps Test Seams (#236) Cycle 2

Timestamp: 2026-07-04T18:52:53.7597616-04:00
ReviewStatus: REMEDIATION_REQUIRED
PrimaryFinding: AC8 repository-wide coverage remains 46.15%, below the required 80.00% threshold.

## Required Fix List

1. File paths: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/spec.md`, `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/user-story.md`, and coverage evidence under `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/`.
   Expected behavior: AC8 is checked only when repository-wide line coverage is at least 80.00%, issue #236 changed/new non-exempt coverage is at least 90.00%, per-file changed/new coverage is at least 90.00%, target coverage remains adequate, and no coverage exemptions are added.
   Verification commands: `dotnet tool run csharpier format .`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`; `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <feature evidence path>`.

2. File paths: repository C# production/test areas with high uncovered line counts.
   Expected behavior: Add deterministic tests and seams only where needed. The plan must prioritize areas with enough uncovered executable lines to materially move repository-wide coverage. The previous remediation raised coverage from 45.59% to 46.15%, so the next plan must identify larger coverage opportunities before implementation.
   Verification commands: focused MSTest runs for touched areas followed by the full C# toolchain.

3. File paths: `coverage.config`, `TaskMaster.runsettings`, `scripts/vscode/TaskMaster.cli.runsettings`.
   Expected behavior: No weakening or target exclusions. Final no-exemption evidence must be produced in the next cycle even if AC8 remains failed.
   Verification commands: search and diff commands from the previous remediation plan P4-T7.

## Do Not Do

- Do not add `[ExcludeFromCodeCoverage]` to issue #236 targets or other newly changed production code.
- Do not weaken coverage configuration or runsettings.
- Do not check AC8 until the threshold artifact proves every AC8 gate.
- Do not continue adding small scattered tests without first producing a coverage-opportunity report that identifies enough uncovered lines to materially affect repository-wide coverage.
- Do not skip final no-exemption, diff-check, or file-size evidence in the next cycle.

## Required Context

- Policy audit: `docs\features\active\2026-07-04-coverage-gaps-test-seams-236\policy-audit.2026-07-04T18-52.md`
- Code review: `docs\features\active\2026-07-04-coverage-gaps-test-seams-236\code-review.2026-07-04T18-52.md`
- Feature audit: `docs\features\active\2026-07-04-coverage-gaps-test-seams-236\feature-audit.2026-07-04T18-52.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Prior remediation threshold artifact: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage-thresholds.2026-07-04T17-29.md`
- Prior remediation coverage targets: `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage-targets.2026-07-04T17-29.md`
