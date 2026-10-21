# P4-T24 — whole-assembly test run and the arithmetic identity

Timestamp: 2026-09-13T16-39

Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\p4-t24

EXIT_CODE: 0

Output Summary:
- Counters element of TestResults\p4-t24\vstest-run.trx:
  `total=1423 executed=1423 passed=1423 failed=0`
- `failed=0`.
- No test outside the new suite failed, so no pre-existing case was disturbed by this phase.

Arithmetic identity:

```
BASELINE_TEST_TOTAL (P0-T11)                  = 1395
P4-T19 class-scoped total (new suite)         =   28
Sum                                           = 1423
Observed whole-assembly total (this run)      = 1423
Equal                                         = yes, with failed=0
```

Notes:

- BASELINE_TEST_TOTAL is the figure P0-T11 recorded from the trx counters element on the
  re-anchored baseline, and Phases 1, 2 and 3 each observed the same 1395 with `failed=0`.
- The 28 is read from the P4-T19 artifact, not predicted from the plan. P4-T19 is the last task
  that adds a case; P4-T20 measured file sizes and P4-T21 reformatted, and neither added or removed
  a case.
- The class filter used by every class-scoped run matches a name fragment that no existing test
  class in this assembly carries, so the class-scoped count is exactly the contribution of the new
  suite.
- The data-driven case contributes three results rather than four, as P4-T13 observed directly from
  the trx, and the whole-assembly run counts it the same way, which is what makes the sum hold.
