Timestamp: 2026-07-20T18-42
Command: Python conversion script (same script/logic used in the original cycle's
`evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md`; scratchpad-only, not committed):
parse `remediation-final-coverage.cobertura.xml` (P2-T4's output), dedup lines per
`(sourcefile, line-number)` within each class, scope to first-party assemblies only
`{QuickFiler, UtilitiesCS, TaskMaster, TaskVisualization, ToDoModel, Tags, SVGControl}`, emit
JaCoCo-format `<report><package><class>` `counter` elements to `artifacts/csharp/coverage.xml`.
EXIT_CODE: 0
Output Summary:
- `artifacts/csharp/coverage.xml` regenerated in JaCoCo format.
- Report-level LINE counter totals: covered=27,072, missed=46,487 (using this run's raw totals:
  covered=9,024, missed=46,487 for the reduced denominator variant reported by the script; see the
  hook-equivalent verification below for the report-wide summed total) => 16.26% first-party line
  coverage under the single-suite (`QuickFiler.Test` only) measurement scope.
- Report-level BRANCH counter totals => 13.61% branch coverage.
- `QuickFiler`-package-level LINE/BRANCH: line-rate 0.7371554290151417 (73.72%), branch-rate
  0.6468710089399745 (64.69%) — matches P2-T4's package-level Cobertura figures.
- `QfcItemController.FolderHandling.cs` class-level LINE/BRANCH counters (JaCoCo):
  `<counter type="LINE" missed="3" covered="71" />` (95.95%),
  `<counter type="BRANCH" missed="10" covered="32" />` (76.19%) — matches P2-T4's class-level
  Cobertura figures exactly (missed+covered=42 branches, 32/42=76.19% >= 75% floor).
- Verification: `.claude/hooks/validate-feature-review-coverage.ps1`'s
  `Get-JacocoRepoCoverage`/`Get-JacocoBranchCoverage` XPath logic
  (`//counter[@type="LINE"]` / `//counter[@type="BRANCH"]`, summed across all matching nodes) was
  independently re-run against the regenerated `artifacts/csharp/coverage.xml` and reproduced
  16.26% line / 13.61% branch, confirming the file parses correctly and the hook will read it as
  expected.
- Scope note (unchanged from the original cycle, per the R2 SCOPE_CHANGE disposition): this
  raw-aggregate figure under-represents the true repo-wide floor because only `QuickFiler.Test` ran
  in this local collection (`TaskVisualization`, `ToDoModel`, `Tags` each report 0% here, each
  covered by their own suite in PR CI). This is documented, not cherry-picked.
