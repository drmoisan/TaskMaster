# Fail-Before Exception Dossier

Timestamp: 2026-06-13T00-31

## WhyFailingRunImpossible

The defect (#191) is an intermittent, wall-clock/thread-pool timing failure. `RunWithTimeout_FuncT1TResult_ShouldReturnResult` schedules a trivially-completing function against a 200 ms timeout. The failure only manifests under thread-pool starvation (class-level parallelism `Workers=0`, aggravated by code-coverage instrumentation overhead) on a busy machine. Because the failure is non-deterministic and load-dependent, a single deterministic "fails-before" regression run cannot be reliably produced; running the test in isolation passes. Per evidence-and-timestamp-conventions, the fail-before requirement is satisfied by this exception dossier plus the alternative-proof citation below.

## Alternative Proof — Reported Failure (Issue #191)

Source: issue.md Summary and Actual Behavior.

- Summary: "`TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnResult` is flaky under parallel and/or coverage execution. It runs a trivial synchronous function but asserts completion within a 200 ms wall-clock window; under thread-pool starvation (24-worker class-level parallelism, aggravated by coverage instrumentation overhead) the work can miss the window and the test fails with a `TimeoutException` in strict mode."
- Actual Behavior: "The trivial function is scheduled on the thread pool and raced against a 200 ms timeout. Under thread-pool starvation the work does not complete within 200 ms, the timeout wins, and strict mode throws `TimeoutException`, failing the assertion."
- Logs/Screenshots: "`TimeoutException` from `RunWithTimeout` (strict mode) on a function that returns immediately."

This documents the pre-fix failure mode. Phase 2 (P2-T5) demonstrates post-fix stability across repeated parallel/coverage runs (AC5) in lieu of a single fail-then-pass transition.
