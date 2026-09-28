# Phase 0 — Halt Gate

Timestamp: 2026-09-13T15-07
Task: [P0-T14]

PHASE0_GATE: GREEN

## Gate Table

Each row records the `EXIT_CODE:` value transcribed into the named artifact by the task that produced
it, read back from that artifact rather than from memory of the run.

| Task | Artifact | Recorded EXIT_CODE | Zero? |
|---|---|---|---|
| P0-T5 | evidence/baseline/csharpier-check.md | 0 | yes |
| P0-T6 | evidence/baseline/build-analyzers.md | 0 | yes |
| P0-T7 | evidence/baseline/build-nullable.md | 0 | yes |
| P0-T8 | evidence/baseline/tests-utilitiescs.md | 0 | yes |
| P0-T9 | evidence/baseline/tests-quickfiler.md | 0 | yes |
| P0-T10 | evidence/baseline/coverage-baseline.md | 0 | yes |

Every row is zero, so the gate records `PHASE0_GATE: GREEN` and Phase 1 may proceed. No row is
non-zero, so there is no failing row to name and no BLOCKED condition arising from this gate.

## Why This Gate Exists

An admitted red baseline would make every Phase 2 exit-zero demand unmeetable by any work this plan
performs. The divergence would then surface as a false Phase 2 failure rather than as a baseline
problem, so it must be resolved by the caller before implementation starts.

## Re-Run Note, Per D15

This artifact overwrites a superseded capture. Every row above was re-measured against the post-merge
tree by the re-executed task named in that row; no value is carried forward from the pre-merge run.
The row that previously blocked this plan is the P0-T10 row: on the pre-merge tree the coverage runner
produced three failures in the QuickFiler zero-batch email-queue test class under the parallelism the
runner's internal MSTest runsettings file imposes, as D14 records. The fix for issue #877, merged as
pull request #880, resolved it; the re-run passed 7222 of 7222 tests and exited 0.

## Baseline Figures Carried Into Later Phases

| Figure | Value | Superseded value |
|---|---|---|
| UtilitiesCS executed tests (AC11 baseline) | 4903 | 4903 |
| QuickFiler executed tests (AC11 baseline) | 1395 | 1394 |
| ProgressPackage.cs covered / total lines (AC12 baseline) | 52 / 52 | 52 / 52 |
| ProgressPackage.cs class elements | 1 | 6 |
| UtilitiesCS.csproj Compile items | 492 | 492 |
| UtilitiesCS.Test.csproj Compile items | 478 | 477 |
| Repository first-party line coverage | 85.71 percent | not recorded on the halted run |
| Repository first-party branch coverage | 79.88 percent | not recorded on the halted run |

The QuickFiler baseline moved by one because the merge brought in one added test method, named
Init_CreatesTokenSourceBeforeAnyLoaderObservesIt, a regression test for issue #839. The
class-element count moved because the superseded figure was derived from a raw unprocessed Cobertura
document left by the failing run, while the re-derivation reads the post-processed document; the
covered and total line figures and the ratio are unchanged. The test-project Compile-item count moved
by one because the #877 fix added the Compile item naming the shared assembly resolver source to that
project alone.

## Test Failures Observed

None. No test failed in P0-T8, in P0-T9 or in the P0-T10 coverage run, so there is no failure to
report as a possible consequence of the assembly-resolution change that pull request 880 made.
