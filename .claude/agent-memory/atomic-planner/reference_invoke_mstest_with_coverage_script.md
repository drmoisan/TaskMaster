---
name: reference-invoke-mstest-with-coverage-script
description: repo-canonical full-suite MSTest+coverage runner at scripts/vscode/Invoke-MSTestWithCoverage.ps1 — use for baseline/final-QC coverage capture tasks
metadata:
  type: reference
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` discovers every `*.Test.dll` under `-SearchRoot` (filtered to `bin\<Configuration>\`, excluding `obj\`/`ref\`) and drives them through one `dotnet-coverage collect` invocation wrapping `vstest.console.exe` (`/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`), emitting a Cobertura-format XML at `-CoverageOutput` (default `coverage\coverage.cobertura.xml`, relative to repo root). It requires `dotnet-coverage` (global tool) and VS Test Platform components (resolved via `vswhere.exe`) and reads `coverage.config` at repo root for instrumentation excludes plus `scripts/vscode/TaskMaster.cli.runsettings` for MSTest parallelization.

This is the correct command to cite in atomic plans needing a full first-party-assembly baseline/final-QC coverage figure (numeric `line-rate`/`branch-rate` from the emitted Cobertura XML root `<coverage>` element), satisfying the CUT3 `vstest.console.exe ... /EnableCodeCoverage` toolchain requirement without inventing new tooling. `-CoverageOutput` can be pointed at `<FEATURE>/evidence/<baseline|qa-gates>/coverage-<stage>.cobertura.xml` to keep the artifact in the canonical evidence location — see [evidence-path-normalization](evidence-path-normalization.md).

**Correction (verified 2026-09-05, #781 planning):** the `\.claude\` caveat this note previously carried is now stale. Line 301 of the current script filters discovery with `([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'`, so stale `.claude/worktrees/agent-*` builds are already excluded. Keep asserting the discovered-assembly list is free of `\.claude\` as cheap corroboration, but do not plan a workaround for it. Also unlike `Invoke-MSTest.ps1`, this script wraps discovery in `@(...)` at line 296, so a `-SearchRoot` matching exactly one assembly does **not** trip the StrictMode `PropertyNotFoundException` described in [reference_invoke_mstest_single_searchroot_defect](reference_invoke_mstest_single_searchroot_defect.md).

Two live constraints, both verified 2026-09-05:

- **`Get-DotnetCoverageArgumentList` hardcodes `/TestCaseFilter:TestCategory!=LiveOutlook` at line 76** with no extension point. A repo-wide run on a workstation that needs an extra `FullyQualifiedName!~` exclusion (for example the shell-icon hang classes) therefore cannot go through the wrapper. Issue `dotnet-coverage collect` directly with the same argument shape, and dot-source the script for `ConvertTo-DerivedCoverageSettingsXml` plus `Invoke-MSTestWithCoverage.Helpers.ps1` for `ConvertTo-KoverageCoberturaXml`. Dot-sourcing is safe: line 348 guards the entry point with `if ($MyInvocation.InvocationName -ne '.')`.
- **`Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Threshold.ps1`) throws unless the post-processed root `line-rate` is >= 0.80**, and it runs at line 344, *after* the post-processed XML is written at line 342. So the Cobertura artifact exists even when the assert throws — but the script's exit is non-zero. A `-SearchRoot <SingleProject>` run narrows the denominator and can breach that floor on an otherwise healthy tree; prefer the repo-wide denominator when the number is going to be read as evidence.

The script requires **PowerShell 7** (`pwsh`), not Windows PowerShell 5.1: line 301 calls `System.IO.Path.GetRelativePath`, which does not exist on the .NET Framework runtime 5.1 uses. Cite `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 ...` in plan tasks.

When a task only needs pass/fail identity (not coverage), prefer invoking `vstest.console.exe` against an explicitly named assembly path plus `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, which bypasses globbing entirely.

Note: this Cobertura-format output is a different artifact/format from the JaCoCo-format `artifacts/csharp/coverage.xml` expected by `validate-feature-review-coverage.ps1` (see [project_csharp_coverage_gate_jacoco_format](project_csharp_coverage_gate_jacoco_format.md)) — do not conflate the two when a plan needs to satisfy the feature-review coverage gate specifically.
