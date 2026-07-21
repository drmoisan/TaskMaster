# Remediation Inputs — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Timestamp: 2026-07-20T22-30
- Base: main @ cd6362f0 | Head: bug/breadcrumb-suggestions-upgrade-race-398 @ 1cb031f6
- Source artifacts:
  - policy-audit: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/policy-audit.2026-07-20T22-30.md
  - code-review: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/code-review.2026-07-20T22-30.md
  - feature-audit: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/feature-audit.2026-07-20T22-30.md

Remediation is required because the policy audit contains FAIL findings and the feature audit grades
AC-5 PARTIAL. The production bug fix itself has no identified logic defect; the items below are
structural/procedural.

## Remediation-Required Findings

### R1 — Test files exceed the 500-line limit (Major, policy FAIL)

- Files:
  - UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs — 536 lines (baseline 474).
  - UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs — 545 lines (baseline 426).
- Rule: General Code Change Policy §4 / `.claude/rules/general-code-change.md` File Size Limit
  (500 lines; test code is not exempt).
- Required change: split each over-limit test file into cohesive, scenario-grouped files, each < 500
  lines (for example, separate the in-flight-upgrade invariant tests from the ReplaceRows seam tests).
  Wire every new test `.cs` file into its `.csproj` with an explicit `<Compile Include="..." />`.
- Acceptance: both files (and any resulting split files) are < 500 lines; the full MSTest suite still
  passes 5061/5061; CSharpier/analyzer/nullable builds remain green.

### R2 — Canonical HEAD C# coverage artifact absent (procedural FAIL; AC-5 coverage sub-clause)

- Context: the file previously at `artifacts/csharp/coverage.xml` was a stale, untracked leftover
  (mtime 2026-07-20 14:34, predating the 22:08 source changes; repo-wide 16.26% line / 13.61% branch;
  touched class `BreadcrumbStateModel` at 43% with no `ReplaceRows` coverage; assemblies Tags /
  TaskVisualization / ToDoModel at 0%). It did not reflect HEAD and was removed. No valid HEAD coverage
  artifact remains.
- Substantive evidence (executor narrative) indicates the target is met: instrumented scope (UtilitiesCS
  + QuickFiler) line 86.54% / branch 80.26% with no regression; new/changed-code line coverage 100%.
- Required change: regenerate `artifacts/csharp/coverage.xml` at HEAD scoped to first-party instrumented
  production packages (Cobertura -> JaCoCo conversion per the repo convention so the gate hook can parse
  `//counter[@type="LINE"]` / `//counter[@type="BRANCH"]`), OR record the PR CI coverage run URL once a
  branch/PR run exists. Do not cherry-pick only instrumented assemblies to force a number.
- Acceptance: a HEAD-reflecting canonical artifact exists and the C# repo-wide line coverage is >= 85%
  and branch coverage >= 75% on the first-party denominator (or the PR CI coverage run is cited); AC-5
  coverage sub-clause is confirmed.

## Non-Blocking Observation (no remediation required)

- BreadcrumbStateModel `_rows` is published by plain reference swap across the UI thread and thread-pool
  continuations without a memory barrier. This is functionally correct for the fix's contract (selection
  reconciled before publish; readers never see a torn/empty list). Consider documenting the memory-model
  assumption. Code review "Minor" finding.

## Handoff

Route to `atomic-planner` (per `remediation-handoff-atomic-planner`) to generate a phased remediation
plan targeting R1 and R2. R1 is a mechanical test-file split; R2 is coverage-artifact regeneration /
CI-run citation. The production source fix does not require changes.
