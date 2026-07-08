# QA Gate 05 — Coverage Delta and Verdict (P8-T5)

Timestamp: 2026-07-07T23-35

## Methodology note (important)

The Phase-0 baseline run (`coverage/baseline.cobertura.xml`, 47.16%, denominator 180,246) was a
one-off dotnet-coverage double-count anomaly: it recorded implausibly inflated per-package line
counts (e.g. UtilitiesCS 141,188 valid lines). dotnet-coverage instruments all runtime-loaded
modules and its merge across the 7 test assemblies is order-sensitive under Workers=0 parallelism.
To obtain a trustworthy apples-to-apples comparison, the pre-change baseline was RE-MEASURED under
the same de-duplicated methodology by git-stashing all F1 code changes, rebuilding, and re-running
the coverage suite. That clean baseline is the authoritative comparison point below.

## Values

- Baseline coverage (clean re-measure, `coverage/cleanbaseline.cobertura.xml`):
  81.02% (79,345 / 97,933 lines), 4995 tests.
- Post-change coverage (`coverage/postchange.cobertura.xml`, reproduced by `verify.cobertura.xml`):
  81.08% (79,667 / 98,254 lines) / 81.07% (79,656 / 98,254), 5032 tests.
- New-code coverage:
  - StoreIdentity.cs: 100.00% (50/50)
  - StoreDisableService.cs: 97.92% (188/192)
  - DisabledStoreEntry (IStoreDisableService.cs): 100.00% (8/8)
  - StoreFilterAttribution.cs (touched): 100.00% (96/96)
  - StoresWrapper.cs (touched): 98.60% (424/430)

## Verdicts

1. No regression on previously-covered lines: PASS.
   - Overall coverage moved +0.06pp (81.02% -> 81.08%); +322 covered lines, +321 valid lines.
   - Every touched production file is 98.6%-100% covered; the pre-existing StoresWrapper and
     StoreFilterAttribution tests all still pass (4995 -> 5032, zero failures, zero removals).
2. New-code coverage >= 90%: PASS (StoreIdentity 100%, StoreDisableService 97.92%; filter/attribution
   deltas 98.6%-100%).
3. Repository line coverage >= 80% (testable denominator): PASS (81.08%, reproduced 81.07%).

Overall verdict: PASS on all three checks.
