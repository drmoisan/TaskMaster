# Fail-Before Exception Dossier — Consume Test (Cycle 4, Issue #181)

Timestamp: 2026-06-08T21-23

Test: `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`
Source: `UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Orchestration_Tests.cs:144`

## WhyFailingRunImpossible

A deterministic fail-before run cannot be reliably produced for this test because its failure mode is non-determinism, not a fixed defect. The production `Consume<T>` reports per-item progress only through a wall-clock `System.Threading.Timer` firing every 500ms (plus one eager `progress.Report(0, ...)`). The test asserts `tracker.Reports.Count >= 2` within a 1-second `SpinWait`. Whether a second report arrives depends on whether the timer fires inside that window during the run, which varies with machine load and scheduling. In the baseline run on this machine the timer happened to fire, so the test PASSED; on slower/CLI/CI hosts it intermittently fails. Forcing a single guaranteed fail-before run would require introducing artificial timing manipulation, which guardrail G3 prohibits.

## Alternative Proof (Defect-by-Construction)

The defect is structural and verifiable by reading the production source `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` `Consume<T>` (lines 47-69):

- The only deterministic `progress.Report` call is the single eager `progress.Report(0, ...)` at line 51 (one report).
- The per-item callback `(x) => completed = x` (line 66) updates a local `int` only and never calls `progress.Report`, so it contributes zero reports.
- The second-and-subsequent reports come solely from the `System.Threading.Timer` callback (lines 53-64), whose firing within the test's 1-second window is wall-clock dependent.

Therefore, in the absence of timer firing, `tracker.Reports.Count == 1`, which fails the `>= 2` assertion. The "reports at least twice" intent is satisfied only opportunistically at baseline. The cycle-4 fix (P2-T2) makes the second report deterministic by driving per-item reporting through the injected `progress` tracker, removing the timing dependency without any sleep/retry/timing slack.

## Search / Evidence References

- Baseline 4-test run recorded in `evidence/baseline/baseline-target-tests.2026-06-08T21-23.md` (this test PASSED that run, confirming non-deterministic pass-at-baseline).
- Post-fix deterministic pass evidence will be recorded in `evidence/regression-testing/consume-after-fix.2026-06-08T21-23.md` (P2-T4).
