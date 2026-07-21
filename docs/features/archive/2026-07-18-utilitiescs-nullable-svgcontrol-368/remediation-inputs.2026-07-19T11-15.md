# Remediation Inputs — utilitiescs-nullable-svgcontrol (Issue #368)

- Timestamp: 2026-07-19T11-15
- Source review artifacts:
  - `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/policy-audit.2026-07-19T11-15.md`
  - `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/code-review.2026-07-19T11-15.md`
  - `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/feature-audit.2026-07-19T11-15.md`

## Context

All 6 acceptance criteria (AC1–AC6) are independently verified PASS, and no Blocking
code-correctness or behavior-preservation defect was found in the 12 remediated `SVGControl/`
files. The two remediation-required findings below are procedural/systemic (coverage-artifact
tooling gap) and process-compliance (missing regression test for an unrelated one-line PowerShell
fix), not defects in the feature's `SVGControl/` nullable-annotation work itself.

## Remediation-Required Finding 1 — Coverage artifact absence (C# and PowerShell)

- **Severity:** Blocking (per the mandatory coverage-verification procedure); systemic, not
  introduced by this feature.
- **Languages affected:** C# (12 changed `.cs` files in `SVGControl/`), PowerShell (1 changed
  `.ps1` file: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`).
- **Finding:** Neither `artifacts/csharp/coverage.xml` nor `artifacts/pester/powershell-coverage.xml`
  exists in this worktree. The mandatory coverage-verification procedure requires an explicit
  PASS/FAIL verdict backed by a canonical artifact for every language with changed files; absence
  of the artifact is itself a FAIL condition, independent of the underlying code's actual test
  coverage.
- **Supporting evidence already available (non-canonical, supplementary):** this feature's own
  project-scoped Cobertura captures at `evidence/qa-gates/final-coverage.cobertura.xml` and
  `evidence/baseline/baseline-coverage.cobertura.xml` show no regression on any previously-covered
  line (`SVGControl` package `lines-covered` unchanged at 870; `RelativePath.cs` byte-identical
  56.75%/54.35% before and after). These do not substitute for the canonical repo-wide artifacts.
- **Recommended remediation:** Generate `artifacts/csharp/coverage.xml` and
  `artifacts/pester/powershell-coverage.xml` via the repo's canonical CI coverage pipeline (this
  has been independently confirmed in prior epic-sibling reviews to be blocked in local
  environments by an unrelated Moq binding-redirect issue for full-solution C# coverage runs), or
  obtain an explicit maintainer decision to treat this specific gap as a tracked, ratified
  exemption consistent with how prior epic-sibling PRs (#309, #354) handled the identical
  systemic condition.
- **Not required:** No change to any of the 12 `SVGControl/` production files is required to
  address this finding; it is a tooling/CI-pipeline gap, not a code defect.

## Remediation-Required Finding 2 — Missing regression test for the `Invoke-MSTestWithCoverage.ps1` bugfix

- **Severity:** Partial (process-compliance gap; the fix itself is independently verified
  functionally correct).
- **File:** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, line 133
  (`$testAssemblies = @(Get-ChildItem ... | Select-Object -ExpandProperty FullName)`).
- **Finding:** This is a genuine defect fix (a `Set-StrictMode`-triggered scalar/array coercion bug
  that throws `The property 'Count' cannot be found on this object.` when exactly one test
  assembly matches the filter). The General Code Change Policy's Bugfix Workflow requires a
  failing regression test be added before the fix. No test was added (confirmed via `git diff`:
  only the production script changed).
- **Recommended remediation:** Extract the `Get-ChildItem | Where-Object | Select-Object`
  test-assembly-discovery pipeline into a named, testable function in
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (following the existing pattern of
  `Get-DotnetCoverageArgumentList`/`ConvertTo-KoverageCoberturaXml` in that same file), then add a
  Pester test in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` asserting that
  the function returns an array (not a scalar) when exactly one match is supplied, and that
  `.Count` succeeds under `Set-StrictMode -Version Latest`.
- **Not required:** No change to the fix's correctness is required; the `@(...)` wrapping is the
  minimal, correct fix and was independently re-verified in this review by rebuilding
  `SVGControl.Test.csproj` and confirming the coverage-wrapped test-discovery step no longer throws
  when exactly one `*.Test.dll` matches (the actual condition in this worktree, since
  `UtilitiesCS`/`VBFunctions`-dependent test projects fail to build for the unrelated,
  pre-existing analyzer-package-pin reason documented in `policy-audit`).

## Not Remediation-Required (Recorded for Completeness)

- The two flagged maintainer-judgment deviations (`ISvgResource` interface nullability;
  additional-member annotations beyond the plan's literal task text) were evaluated in
  `feature-audit` and judged legitimate, in-scope consequences of the stated architecture. No
  remediation action is recommended for either.
- The `SvgImageSelector.ImagePath` judgment call was evaluated and judged correctly,
  conservatively resolved. No remediation action is recommended.
- The CLAUDE.md-vs-`.claude/rules/general-unit-test.md` coverage-threshold conflict (80%/90% vs.
  85%/75% uniform) is a pre-existing, repository-wide condition explicitly flagged (not silently
  resolved) by the executing plan. It is out of scope for this feature to resolve and is not
  carried forward as a remediation trigger specific to this feature.
