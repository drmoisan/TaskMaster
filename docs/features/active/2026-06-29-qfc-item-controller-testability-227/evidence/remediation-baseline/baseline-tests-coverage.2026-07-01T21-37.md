# Baseline — Tests + Code Coverage (Cycle-2 Remediation, toolchain step 4)

Timestamp: 2026-07-01T21-37
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage
EXIT_CODE: 0

## Test result (regression baseline)

- Total tests: 233
- Passed: 233
- Failed: 0
- Skipped: 0

This 233-passing count is the regression baseline that every later phase must preserve
(each phase adds new tests on top; the pre-existing 233 must continue to pass).

## Coverage measurement mechanism (phase-consistent)

The plan's canonical command `vstest.console.exe ... /EnableCodeCoverage` passes 233/233 and emits a
binary `.coverage`. That binary is not offline-convertible by dotnet-coverage v18.5 or
Microsoft.CodeCoverage.Console v18.7 in this environment (both convert to empty packages). Numeric
per-class coverage is therefore obtained by re-running the identical vstest environment with a
Cobertura-format runsettings that carries the standard attribute exclusions (including
`System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute`). That run also passes 233/233,
so the coverage figures reflect the same passing test set. The runsettings is preserved for reuse
across all phases; `[ExcludeFromCodeCoverage]` members are excluded from the denominator, matching
the cycle-1 "affected non-exempt denominator" methodology.

Note: running tests under `dotnet-coverage collect` was rejected as a mechanism because the profiler
attach fails 135/233 tests in this environment (unrepresentative). The Cobertura-runsettings path
preserves 233/233 and is authoritative.

## Numeric coverage (baseline)

- Repo-wide line coverage (Cobertura root, exemptions honored): 8353/61046 = 13.68%.
  The repo-wide floor is satisfied-with-documented-exception under the #223 authority-scoped
  precedent; residual repo-wide uplift is tracked under #197 (see plan P8-T5).
- QfcItemController affected NON-EXEMPT denominator (all partials, exempt members excluded):
  **226/239 = 94.56%**.

Per-partial non-exempt (baseline; fully-exempt partials contribute 0 non-exempt lines and are absent):

| Partial | non-exempt covered/total | % |
|---|---:|---:|
| QfcItemController.cs (Properties/INotify) | 62/65 | 95.38% |
| QfcItemController.Conversation.cs | 11/11 | 100.00% |
| QfcItemController.EventWiring.cs | 115/121 | 95.04% |
| QfcItemController.FolderHandling.cs | 26/29 | 89.66% |
| QfcItemController.MailActions.cs | 12/12 | 100.00% |
| QfcItemController.ViewerSetup.cs | 0/1 | 0.00% |
| AGGREGATE | 226/239 | 94.56% |

Carried cycle-1 reference (different line-granularity, same non-exempt concept):
484/585 = 82.74% (from `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`).

Output Summary: 233/233 tests pass (regression baseline). Baseline QfcItemController non-exempt
denominator 226/239 = 94.56% (small denominator because 103 members are still exempt). As Phases 5-6
remove exemptions and add covering tests, the denominator grows toward the whole class and must stay
>= 80% (AC5). Repo-wide 13.68% under documented authority-scoped exception (#197).
