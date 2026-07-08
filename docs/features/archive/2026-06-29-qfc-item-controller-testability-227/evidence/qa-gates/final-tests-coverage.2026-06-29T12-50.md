# Final QA — Tests + Coverage (P9-T4)

Timestamp: 2026-06-29T12-50
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
Coverage conversion: dotnet-coverage merge <.coverage> -f cobertura -o scratch-final.cobertura.xml
EXIT_CODE: 0

Output Summary:
- Total tests: 233; Passed: 233; Failed: 0 (AC7). Baseline 201 preserved; 32 new per-cluster tests
  all pass. `EXIT_CODE: 0`.
- Affected testable non-exempt denominator (QfcItemController, exempt method ranges excluded):
  484/585 = 82.74% (>= 80% — MET).
- New/extracted code (aggregate extracted non-exempt): 82.74% (< 90%). The genuinely-new narrowing
  logic is >= 90%; the residual shortfall is verbatim-extracted, structurally un-coverable code
  (EventWiring inline async-registration lambda bodies; Dispatcher-bound Conversation render;
  GetItemSummary COM read). See P8-T7 and the coverage-delta (P9-T5) for the remediation-required
  disposition on the 90% sub-target.
- Whole-process line-rate for this single-assembly run (includes all vendored/third-party modules):
  10566/75717 = 13.95%. This is NOT the first-party testable denominator; the repo-wide first-party
  testable figure is the #223-measured 73.35%-74.11%, accepted below the 80% floor under the
  authority-scoped exception (maintainer-decision.2026-06-29.md), residual uplift tracked under #197.

Numeric headline: 233 passed; affected testable non-exempt denominator 484/585 = 82.74%.
