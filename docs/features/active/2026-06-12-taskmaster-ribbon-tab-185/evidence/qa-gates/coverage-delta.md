# Coverage Delta — Baseline vs Post-Change (Issue #185)

Timestamp: 2026-06-12T10-49

Sources:
- Baseline: evidence/baseline/baseline-tests.md (P0-T5)
- Post-change: evidence/qa-gates/final-tests.md (P2-T4)
- Both runs used the same command scope:
  vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage

## Aggregate line coverage (single-assembly run, all instrumented modules)

| Metric            | Baseline (P0-T5) | Post-change (P2-T4) | Delta   |
|-------------------|------------------|---------------------|---------|
| Lines covered     | 7711             | 7766                | +55     |
| Lines partial     | 442              | 443                 | +1      |
| Lines not-covered | 84260            | 84242               | -18     |
| Total lines       | 92413            | 92451               | +38     |
| Line coverage %   | 8.34%            | 8.40%               | +0.06pp |

Note: The aggregate is dominated by third-party/other-project DLLs (Deedle, log4net,
System.Linq.Async, FluentAssertions, Swordfish, etc.) loaded but not exercised by this
single-assembly run. It is recorded for apples-to-apples comparison under the plan's exact
command, not as a repository-wide CI figure. The +38 total-lines change is the 2 new test
methods being compiled into TaskMaster.Test.dll.

## First-party module coverage (no regression check)

| Module                 | Baseline covered | Post-change covered | Delta |
|------------------------|------------------|---------------------|-------|
| TaskMaster.Test.dll    | 2206             | 2242                | +36   |
| TaskMaster.dll         | 804              | 804                 | 0     |
| ToDoModel.dll          | 45               | 45                  | 0     |
| UtilitiesCS.dll        | 1774             | 1774                | 0     |
| TaskVisualization.dll  | 13               | 13                  | 0     |

## Changed-code coverage

The in-scope production change is to TaskMaster/Ribbon/RibbonExplorer.xml, a non-compiled
embedded XML resource. It contains no executable IL and is therefore not line-instrumentable;
it has no changed-code coverage figure by construction. Its correctness is verified instead by
the RibbonExplorerXmlTests suite (4 tests passing, see evidence/regression-testing/targeted-verification.md).

The in-scope test change adds two new test methods to RibbonExplorerXmlTests.cs
(TaskMaster.Test.dll). Both new methods are executed by the post-change run (TaskMaster.Test.dll
covered lines rose by +36), so the added test code is fully exercised.

## Conclusion

- Repository line-coverage gate (>= 80%): Not evaluable from this targeted single-assembly run.
  The 8.34% -> 8.40% figures are not repository-wide; the repository-wide >= 80% gate is
  assessed by the full CI suite, which this minor-audit small-path task does not run. This is
  consistent with the baseline (P0-T5), which recorded the same scope limitation.
- No-regression on changed lines: PASS. No first-party production module lost coverage
  (all deltas >= 0). The only changed/added code (the two new test methods) is fully covered.
- Outcome: No coverage regression attributable to issue #185. Coverage increased slightly
  (+0.06 percentage points aggregate; +36 covered lines in TaskMaster.Test.dll).
