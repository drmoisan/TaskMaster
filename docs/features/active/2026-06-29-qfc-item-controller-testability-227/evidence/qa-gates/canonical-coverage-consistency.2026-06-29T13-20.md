# Phase 2 — Canonical Coverage Consistency (P2-T2)

Timestamp: 2026-06-29T13-20

## Generation command

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
dotnet-coverage merge <.coverage> -f cobertura -o artifacts/csharp/coverage.xml
```

EXIT_CODE: 0 (both the collection and the conversion exited 0)

## Resulting Cobertura line-rate

- Root (whole-process, single-assembly): line-rate 0.13954594080589564 → 10566/75717 = 13.95%.
- QuickFiler package line-rate: 0.41350 (41.35%).

## Consistency checks against existing feature evidence

Reference evidence: `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`,
`evidence/qa-gates/p8-tests-coverage.2026-06-29T12-40.md`.

1. **Tests**: this cycle's collection produced 233/233 passing (0 failed) — matches the existing
   233/233 evidence. No test removed or weakened (G3).
2. **Whole-process single-assembly figure**: produced root = 10566/75717 = 13.95%, which exactly
   reproduces the documented post-change single-assembly figure "10566/75717 = 13.95%" in
   `coverage-delta.2026-06-29T12-50.md`.
3. **Affected testable non-exempt numerator**: the per-cluster covered counts parsed from the
   produced XML (124, 70, 186, 52, 24, 28, 0) sum to 484 and match the prior evidence per cluster
   exactly. Applying the documented brace-matched `[ExcludeFromCodeCoverage]` exempt-range
   exclusion to the same covered set yields the gate metric 484/585 = 82.74%, identical to the
   existing evidence.
4. **Denominator difference is expected, not an inconsistency**: the raw Cobertura cluster
   denominator (929) exceeds 585 because the VS `.coverage` collector does not honor
   `[ExcludeFromCodeCoverage]` on async state machines — this behavior is explicitly documented in
   `p8-tests-coverage.2026-06-29T12-40.md`. The 82.74% gate metric is the non-exempt-adjusted value
   derived from the identical covered numerator.

## Determination

CONSISTENT: YES

The produced canonical artifact reproduces the existing evidence: 233/233 tests, the exact
whole-process figure 10566/75717 = 13.95%, and the exact affected testable non-exempt numerator
(484) yielding 484/585 = 82.74% (>= 80% MET). No discrepancy was found.

## Output Summary

CONSISTENT: YES. Generated `artifacts/csharp/coverage.xml` (EXIT_CODE 0) reproduces the prior-cycle
233/233 test result, the 13.95% whole-process line-rate, and the 484/585 = 82.74% affected testable
non-exempt denominator. R1 numeric-consistency sub-claim resolved as PASS.
