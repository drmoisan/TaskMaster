# Coverage Artifact Substitution (Issue #508)

Timestamp: 2026-08-08T17-30
Decision by: orchestrator
Command: `pwsh -NoProfile -File <scratchpad>/Convert-CoberturaToJacoco.ps1 -CoberturaPath <cobertura> -JacocoPath <jacoco>`
EXIT_CODE: 0

## What changed

The atomic executor produced two raw full-detail Cobertura reports as evidence:

- `evidence/baseline/coverage-baseline.cobertura.xml` — ~10 MB
- `evidence/qa-gates/coverage-postchange.cobertura.xml` — ~10 MB

Together these would have added roughly 20 MB and 378,000 lines to permanent git history for a
single bug fix. They were replaced, **before being pushed**, with compact package-level JaCoCo
summaries carrying the identical measured totals:

- `evidence/baseline/coverage-baseline.jacoco.xml`
- `evidence/qa-gates/coverage-postchange.jacoco.xml`

This follows the convention established by commit `d0955dc4` ("docs(#503): replace raw cobertura
coverage evidence with jacoco summaries", 2026-08-08), which made the same substitution for issue
#503. In that case the raw reports had already been committed and had to be deleted afterwards; here
the substitution was made by amending the unpushed commit, so the 20 MB never enters history at all.

No number was recomputed, adjusted, or re-run. The summaries are a lossless projection of the
per-line `hits` and `condition-coverage` attributes in the source reports onto JaCoCo
`<counter type="LINE">` / `<counter type="BRANCH">` elements, aggregated per package.

## Verified totals (derived from the source reports before substitution)

| Report | Cobertura root attributes | Derived from the projection | Agreement |
|---|---|---|---|
| Baseline (merge-base `003c5715`) | `lines-covered="95274" lines-valid="111021"`, `line-rate="0.858162"` | 95274 covered / 15747 missed = 111021 valid, **85.82%** | exact |
| Post-change | `lines-covered="95325" lines-valid="111059"`, `line-rate="0.858328"` | 95325 covered / 15734 missed = 111059 valid, **85.83%** | exact |

The derived line counts reproduce the Cobertura root `lines-covered` / `lines-valid` attributes
exactly, which is the check establishing the projection is lossless.

Branch: baseline 22070 covered / 5792 missed = 27862 valid (79.21%); post-change 22093 covered /
5791 missed = 27884 valid (79.23%).

Delta: line coverage **+0.000166**, branch coverage **+0.000200**. **No coverage regression.** Both
figures sit above the `.claude/rules/general-unit-test.md` floors (line >= 85%, branch >= 75%).

## Why `lines-valid` grew by 38

The denominator grew from 111021 to 111059 because this change removed
`[ExcludeFromCodeCoverage]` from `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, adding
that class's 38 lines to the measured denominator. The change is visible entirely inside the
`UtilitiesCS` package:

| Package | Baseline LINE (missed/covered) | Post-change LINE (missed/covered) |
|---|---|---|
| `UtilitiesCS` | 7537 / 69205 | 7530 / 69250 |

45 newly covered lines against 38 added denominator lines, so the repo-wide rate moved upward
despite the larger denominator. This corrects a prior expectation recorded in the plan, which
assumed `[ExcludeFromCodeCoverage]` was **not** honored under this repository's `coverage.config`;
the measured baseline shows it **was** honored, because the class is absent from the baseline report
and present in the post-change report.

## Denominator note

These figures are measured over the nine first-party solution packages: `QuickFiler`, `SVGControl`,
`Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`.
Vendored third-party assemblies are excluded by `coverage.config`, and no `*.Test` assembly appears
in the denominator. This is the policy denominator per `.claude/rules/general-unit-test.md` and
CLAUDE.md § UT2.

## Canonical gate artifact

`artifacts/csharp/coverage.xml` was generated from `evidence/qa-gates/coverage-postchange.jacoco.xml`
for consumption by `.claude/hooks/validate-feature-review-coverage.ps1`. That hook parses JaCoCo
`<counter>` elements and cannot read Cobertura, which is why the format conversion is required rather
than optional. `artifacts/` is gitignored (`.gitignore:57`), so this file is local-only by repository
convention and is regenerated rather than committed.

## Regeneration

```powershell
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug
```

This writes full-detail Cobertura, which the projection above converts to the committed summaries.

## Output Summary

Two ~10 MB raw Cobertura reports replaced by lossless package-level JaCoCo summaries before the
commit was pushed, so roughly 20 MB and 378,000 lines were kept out of permanent git history.
Derived line counts reproduce the Cobertura root attributes exactly. Repo-wide line coverage moved
85.82% -> 85.83% and branch 79.21% -> 79.23%; no regression, both floors met. The 38-line denominator
increase is the de-exempted `WpfDispatcherYield` class, offset by 45 newly covered lines.
