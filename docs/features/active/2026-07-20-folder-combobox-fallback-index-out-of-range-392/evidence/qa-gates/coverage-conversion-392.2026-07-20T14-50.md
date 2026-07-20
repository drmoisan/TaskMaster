Timestamp: 2026-07-20T14-50
Command: Python conversion script (inline, scratchpad-only, not committed to the repo): parse
`final-coverage.cobertura.xml` (P2-T4's `dotnet-coverage collect -f cobertura` output), dedup lines
per `(sourcefile, line-number)` within each class, scope to first-party assemblies only
`{QuickFiler, UtilitiesCS, TaskMaster, TaskVisualization, ToDoModel, Tags, SVGControl}` (test
assemblies `QuickFiler.Test`/`UtilitiesCS.Test`/etc. and third-party/vendored libraries
`FluentAssertions`, `log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`,
`System.Linq.Async`, `System.Interactive` excluded), emit JaCoCo-format `<report><package><class>`
`counter` elements (`type="LINE"` and `type="BRANCH"`, aggregated bottom-up at class/package/report
level) to `artifacts/csharp/coverage.xml`, mirroring the established conversion pattern recorded in
`docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/evidence/qa-gates/coverage-conversion.2026-07-18T10-55.md`.
EXIT_CODE: 0
Output Summary:
- `artifacts/csharp/coverage.xml` written in JaCoCo format (report/package/class `counter`
  elements), regenerated from the P2-T4 final Cobertura output.
- Report-level LINE counter totals: covered=27,063, missed=139,470 (total 166,533) => 16.25%
  first-party line coverage under the single-suite (`QuickFiler.Test` only) measurement scope.
- Report-level BRANCH counter totals: covered=5,604, missed=35,604 (total 41,208) => 13.60% branch
  coverage. Unlike the referenced prior-feature conversion (which reported 0/0 BRANCH counters
  because that dotnet-coverage run carried no `condition-coverage` attributes), this run's Cobertura
  XML does carry `branch="True"`/`condition-coverage="X% (covered/total)"` attributes on branch
  lines, so BRANCH counters here are populated from real condition-coverage data, not left at 0/0.
- Verification: the hook `.claude/hooks/validate-feature-review-coverage.ps1`'s own
  `Get-JacocoRepoCoverage`/`Get-JacocoBranchCoverage` XPath logic
  (`//counter[@type="LINE"]` / `//counter[@type="BRANCH"]`, summed across all matching nodes) was
  independently re-run against the generated `artifacts/csharp/coverage.xml` and reproduced the
  identical 16.25% / 13.6% figures, confirming the multi-level (class + package + report) counter
  nesting sums to a ratio-preserving result (a constant multiplicative factor cancels in the
  covered/total ratio) and that the hook will read this file correctly.
- Scope note (matches the referenced prior-feature pattern): the 16.25% figure under-represents the
  repository floor because first-party assemblies not exercised by the single `QuickFiler.Test` suite
  run here (`TaskVisualization`, `ToDoModel`, `Tags` — each 0% under this scope, each covered by its
  own suite in PR CI) and the large `UtilitiesCS` assembly (7.48% under this scope, exercised
  primarily by `UtilitiesCS.Test`, not run here) drag the aggregate down. Per-package figures for the
  directly exercised assembly are `QuickFiler` 73.68% line / 64.62% branch (matching P2-T4's
  per-package Cobertura figures exactly). Repo-wide aggregation across all first-party test suites is
  deferred to PR CI per established repo practice (policy-audit §5.4); no assembly was cherry-picked
  out of the first-party scope to inflate this figure.
