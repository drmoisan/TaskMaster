# Fail-Before Exception Dossier — Finding C (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Test: `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`
(`UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Orchestration_Tests.cs` lines 143-163)

WhyFailingRunImpossible:
The test is flaky-by-construction, not deterministically failing. Its `tracker.Reports.Count >= 2` assertion depends on a wall-clock `System.Threading.Timer` firing every 500ms inside `SubjectMapSco.Consume<T>` (`SubjectMapSco.Orchestration.cs` lines 53-64). Under isolated or light load the timer fires within the test's 1-second `SpinWait`, so the test passes; under full-suite CPU contention the timer can be starved and the test fails. A single targeted CLI run therefore cannot be made to reliably FAIL on demand without introducing artificial load, which would itself be a timing hack prohibited by guardrail G3. In the two cycle-5 baseline runs of the four-test set, this test PASSED both times (`baseline-target-tests.2026-06-08T21-53.md`), confirming it does not fail deterministically in isolation.

SearchScope:
- docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/
- docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/
SearchPatterns: baseline-target-tests.*.md, fail-before-exception.*.md
SearchResult: baseline-target-tests.2026-06-08T21-53.md (Consume PASSED both runs in isolation); this dossier.

## Alternative Proof — Timing-Dependence (Source Analysis)

The flakiness is structural and verifiable by reading the production source rather than by observing a failing run:

1. `SubjectMapSco.Consume<T>` (`SubjectMapSco.Orchestration.cs` lines 47-69):
   - Line 51: `progress.Report(0, $"Consuming {0:N0} of {count:N0}")` — exactly ONE eager report.
   - Lines 53-64: a `System.Threading.Timer` that calls `progress.Report(...)` every 500ms.
   - Line 66: `enumerable.WithProgressReporting(count, (x) => completed = x).ToList()` — the per-item callback `(x) => completed = x` only mutates a LOCAL `int`; it never calls `progress.Report`.

2. Therefore the ONLY source of a second-or-later `Report` is the wall-clock timer. With the test's three `Thread.Sleep(20)` elements, enumeration takes roughly 60ms — well under the timer's 500ms period — so whether a second report arrives within the test's 1-second `SpinWait` is entirely a function of timer scheduling under load.

3. `RecordingProgressTracker.Report(double, string)` (test, lines 458-459) appends to `_reports`, so `tracker.Reports.Count` reflects exactly the number of `progress.Report` calls observed. With only the eager report guaranteed, `Reports.Count >= 2` is non-deterministic.

The Finding C fix (P3-T2) makes the per-item callback also call `progress.Report(...)`, guaranteeing at least the eager report plus one report per consumed item (>= 4 total for a 3-element sequence), independent of wall-clock timing. This converts the flaky assertion into a deterministic pass and is the no-timing-hack seam authorized by the plan.

This dossier satisfies the fail-before requirement for Finding C per evidence-and-timestamp-conventions (failing run impossible to produce reliably; structural proof of the defect supplied).
