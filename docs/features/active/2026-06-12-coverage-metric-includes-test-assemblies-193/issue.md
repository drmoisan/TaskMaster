# coverage-metric-includes-test-assemblies (Issue #193)

- Date captured: 2026-06-12
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-metric-includes-test-assemblies/ (Issue #193)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #193
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/193
- Last Updated: 2026-06-13
- Work Mode: minor-audit

## Summary

The Koverage coverage-metric pipeline includes `.Test` assemblies in both the numerator and denominator, inflating the reported line-coverage rate and violating the General Unit Test Policy requirement that coverage metrics exclude test files.

## Environment

- OS/version: Windows, PowerShell 7+
- Python version: N/A
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (Koverage post-processing via `ConvertTo-KoverageCoberturaXml` / `Get-KoverageProjectAllowlist`)
- Data source or fixture: Cobertura output from `dotnet-coverage collect`

## Steps to Reproduce

1. Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug`.
2. Inspect the post-processed Cobertura report's `<packages>` and headline `line-rate`.
3. Observe that `.Test` packages (for example `UtilitiesCS.Test`, `QuickFiler.Test`) remain in the report and are counted in the aggregate `lines-covered` / `lines-valid`.

## Expected Behavior

Test assemblies (`*.Test`) are excluded from both the numerator (lines-covered) and the denominator (lines-valid), so the reported metric reflects production application code only, consistent with the General Unit Test Policy ("Configure coverage tooling to exclude test files… so metrics reflect the application code, not the tests themselves").

## Actual Behavior

`Get-KoverageProjectAllowlist` builds the allowlist from every `*.csproj/*.vbproj/*.fsproj` in the repo, including test projects, so `ConvertTo-KoverageCoberturaXml` retains `.Test` packages. With test assemblies included the reported rate is approximately 76.4%; the policy-correct production-only rate is 58.95% (38,767 / 65,768 deduped lines).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see `artifacts/csharp/coverage-firstparty.cobertura.xml` and `artifacts/csharp/coverage-rerun.log`; per-package evidence recorded in `artifacts/research/csharp-coverage-roadmap.2026-06-12.md` §0.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Reported coverage drives the policy gate; including test assemblies overstates application-code coverage by roughly 17 percentage points and could mask regressions.

## Suspected Cause / Notes

`Get-KoverageProjectAllowlist` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` does not filter out test projects. All current test projects use the `.Test` assembly-name suffix (QuickFiler.Test, SVGControl.Test, Swordfish.NET.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test).

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: exclude test projects from `Get-KoverageProjectAllowlist` so `ConvertTo-KoverageCoberturaXml` strips `.Test` packages from numerator and denominator.
- [x] Integration scenario to retest: regenerate coverage and confirm `.Test` packages are absent and the rate reflects production-only lines.
- [x] Manual verification notes: existing Pester tests live at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`; extend them to assert test-project exclusion.

## Acceptance Criteria

- [x] AC1: `Get-KoverageProjectAllowlist` excludes projects that resolve to a test assembly (assembly name matching `.Test`), so test projects are not added to the allowlist.
- [x] AC2: `ConvertTo-KoverageCoberturaXml` output contains no `<package>` whose name corresponds to a `.Test` assembly; both their covered and valid lines are removed from the aggregate `lines-covered` and `lines-valid`.
- [x] AC3: A failing-first Pester regression in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` asserts test-project exclusion (allowlist excludes `.Test`; post-processed report strips `.Test` packages from numerator and denominator).
- [x] AC4: Non-test first-party and vendored production packages (UtilitiesCS, QuickFiler, TaskMaster, ToDoModel, Tags, TaskVisualization, VBFunctions, SVGControl, Swordfish.NET.General) remain in the report unchanged.
- [x] AC5: PowerShell toolchain passes in order for the change scope — PoshQC format clean; PSScriptAnalyzer zero new findings (the single `PSUseSingularNouns` on `Get-CoberturaLineConditionCoverageParts` pre-exists on HEAD and is outside the changed function); Pester for `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` 6/6 pass. Note: the folder-level Pester run has one pre-existing, unrelated failure (`Install-RepoDotNetSdk.Tests.ps1` expects SDK `8.0.205` but committed `global.json` pins `10.0.200`); tracked separately, not part of #193.
- [x] AC6: No production file exceeds 500 lines; change scope limited to the helper module and its test file.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
- [x] Implement fix via powershell-typed-engineer; verify (orchestrator ran PoshQC format/analyze/Pester gates)
- [ ] Minor-audit review (feature-review) + PR