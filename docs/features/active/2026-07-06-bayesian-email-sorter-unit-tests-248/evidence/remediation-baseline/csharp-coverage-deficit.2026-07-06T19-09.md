Timestamp: 2026-07-06T19:16:32-04:00
Command: coverage deficit analysis from recorded evidence
EXIT_CODE: 0

Input Evidence:
- docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T18-07.md
- docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T18-07.coveragexml
- artifacts/pr_context.summary.txt
- git diff --name-only main...HEAD

Output Summary:
- Final recorded full-suite C# line coverage: 20.21%.
- Required repository-wide C# line coverage floor: 80.00%.
- Numeric gap to policy floor: 59.79 percentage points.
- Baseline full-suite C# line coverage from recorded evidence: 18.54%.
- Recorded final delta from baseline: +1.67 percentage points.
- Final full-suite test evidence: 486 total, 486 passed, 0 failed.
- Issue #248 changed production files: none.
- Issue #248 changed C# test files:
  - QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs
  - QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs
  - QuickFiler.Test/Controllers/EmailSorterTests.cs
  - QuickFiler.Test/QuickFiler.Test.csproj
- Coverage finding: COV-1 remains unresolved by recorded final evidence because repository-wide line coverage is below the 80% floor.
