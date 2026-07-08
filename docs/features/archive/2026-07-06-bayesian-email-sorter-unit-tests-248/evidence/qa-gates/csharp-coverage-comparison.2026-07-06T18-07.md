Timestamp: 2026-07-06T18:46:00-04:00
Issue: #248
Command: coverage comparison from converted coverage XML and recorded coverage evidence
EXIT_CODE: 0

Input Evidence:
- Baseline evidence: docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T18-07.md
- Baseline coverage XML: docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T18-07.coveragexml
- Targeted test evidence: docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T18-07.md
- Targeted coverage XML: docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T18-07.coveragexml
- Final coverage evidence: docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T18-07.md
- Final coverage XML: docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T18-07.coveragexml

Output Summary:
- Baseline full-suite tests: 472 total, 472 passed, 0 failed.
- Targeted issue #248 tests: 14 total, 14 passed, 0 failed.
- Final full-suite tests: 486 total, 486 passed, 0 failed.
- Baseline coverage headline from recorded evidence: overall line coverage 18.54%; overall block coverage 18.66%.
- Targeted-test coverage headline from recorded evidence: overall line coverage 3.52%; overall block coverage 3.60%.
- Final coverage headline from recorded evidence: overall line coverage 20.21%; overall block coverage 19.48%.
- Coverage delta from baseline evidence to final evidence: +1.67 percentage points line coverage; +0.82 percentage points block coverage.
- Repository-wide final line coverage remains below the repository 80% floor: final recorded line coverage 20.21%.
- No production files were changed for issue #248, so changed-line production coverage regression is not applicable.
- Changed issue #248 test-file line coverage from final XML: 293 covered-or-partial lines out of 296 instrumented lines, 98.99%.
- Target production controller file line coverage from final XML: 111 covered-or-partial lines out of 146 instrumented lines, 76.03%.

Issue #248 Source-File Coverage From Final XML:
- QuickFiler/Controllers/EmailSorter.cs: 47 covered-or-partial lines out of 49 instrumented lines, 95.92%.
- QuickFiler/Controllers/BayesianPerformanceController.cs: 64 covered-or-partial lines out of 97 instrumented lines, 65.98%.
- QuickFiler.Test/Controllers/EmailSorterTests.cs: 30 covered-or-partial lines out of 30 instrumented lines, 100.00%.
- QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs: 185 covered-or-partial lines out of 185 instrumented lines, 100.00%.
- QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs: 78 covered-or-partial lines out of 81 instrumented lines, 96.30%.

Changed-File Scope:
- Production files changed: none.
- Test files changed:
  - QuickFiler.Test/Controllers/EmailSorterTests.cs
  - QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs
  - QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs
  - QuickFiler.Test/QuickFiler.Test.csproj

Coverage Disposition:
- PASS: issue #248 targeted tests passed.
- PASS: final full-suite tests passed with coverage enabled.
- PASS: issue #248 changed test files have high line coverage.
- PASS: no production changed-line coverage regression exists because the target production files were not changed.
- PARTIAL: repository-wide final line coverage is below the 80% policy floor, although it improved relative to the baseline evidence.
