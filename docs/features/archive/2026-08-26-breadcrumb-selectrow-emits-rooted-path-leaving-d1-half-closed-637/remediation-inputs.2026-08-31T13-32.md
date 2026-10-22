# Remediation Inputs: Issue #637 post-merge feature review

Timestamp: 2026-08-31T13-32

## Authoritative Finding

The post-merge policy audit is FAIL because `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is modified in this feature diff and has 694 lines. The repository general code-change policy limits production code, test code, and reusable scripts to 500 lines. The specification's AC25 no-growth statement is not an approved policy exception.

## Required Fixes

1. Split `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` into cohesive MSTest source files so every resulting modified or new test file is at or below 500 lines.
   - Preserve all existing test method names, test behavior, assertions, and setup/cleanup semantics.
   - Keep the Issue #637-related provider lookup assertion and the AC21 archive-relative selected-value correction intact.
   - Do not change the production normalization behavior.
2. Update `QuickFiler.Test/QuickFiler.Test.csproj` compile includes for every new test source file created by the split.
3. Re-measure the affected test-file line counts and record evidence under the active feature's `evidence/qa-gates/` directory.
4. Rerun the full C# toolchain in order after the split:
   - `dotnet tool run csharpier format .`
   - `dotnet tool run csharpier check .`
   - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   - `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-remediation.cobertura.xml`
5. Refresh PR context after the remediation commit and repeat feature review against `main`.

## Do Not Do

- Do not weaken, delete, skip, or rename test methods merely to reduce line count.
- Do not alter `spec.md` AC21 wording or reclassify its deliberate specification correction as a weakened test.
- Do not modify production behavior outside the fixture split and necessary project compile includes.
- Do not weaken repository policy or introduce an exception for the 500-line limit.
- Do not bypass the required formatter, analyzer, nullable, test, coverage, PR-context, or review gates.

## Required Context Package

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `policy-audit.2026-08-31T13-32.md`
- `code-review.2026-08-31T13-32.md`
- `feature-audit.2026-08-31T13-32.md`
- `plan.2026-08-29T12-20.md`
- `spec.md`
