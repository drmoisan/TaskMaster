# Determinism — Repeated Runs (AC5)

Timestamp: 2026-06-13T00-41

Both mitigations applied (shipped state):
1. Class-level `[DoNotParallelize]` on the `TimeOutTask_Tests` partial-class declaration carrying `[TestClass]` (TimeOutTask_Tests.cs).
2. Widened success-path timeout `milliseconds: 5000` in `RunWithTimeout_FuncT1TResult_ShouldReturnResult` (TimeOutTask_AdditionalTests.cs).

Command (per run): vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:RunWithTimeout_FuncT1TResult_ShouldReturnResult /InIsolation
Coverage run: same with /EnableCodeCoverage appended.
(Class-level parallelism is active by default — the affected class is governed by [DoNotParallelize], but the runner still enables ClassLevel parallelism across the rest of the assembly; the test targets a single test by name.)

## Per-run results (12 consecutive parallel runs)
| Run | EXIT_CODE | TimeoutException count | Result |
|-----|-----------|------------------------|--------|
| 1  | 0 | 0 | Passed [47 ms] |
| 2  | 0 | 0 | Passed [48 ms] |
| 3  | 0 | 0 | Passed [48 ms] |
| 4  | 0 | 0 | Passed [48 ms] |
| 5  | 0 | 0 | Passed [49 ms] |
| 6  | 0 | 0 | Passed [47 ms] |
| 7  | 0 | 0 | Passed [47 ms] |
| 8  | 0 | 0 | Passed [46 ms] |
| 9  | 0 | 0 | Passed [49 ms] |
| 10 | 0 | 0 | Passed [47 ms] |
| 11 | 0 | 0 | Passed [46 ms] |
| 12 | 0 | 0 | Passed [49 ms] |

## Coverage-instrumented run (1)
| Run | EXIT_CODE | TimeoutException count | Result |
|-----|-----------|------------------------|--------|
| coverage | 0 | 0 | Passed [98 ms] |

Output Summary:
- 12/12 parallel runs PASSED; 1/1 coverage run PASSED. 13/13 total passes.
- Zero `TimeoutException` across all 13 runs (against the shipped state with both mitigations applied).
- Demonstrates deterministic pass under class-level parallelism and under coverage instrumentation (AC5). No other test was driven by these targeted runs.
