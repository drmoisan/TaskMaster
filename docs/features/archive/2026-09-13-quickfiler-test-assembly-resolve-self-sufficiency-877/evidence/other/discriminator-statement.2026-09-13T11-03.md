# Discriminator and non-probative-status declaration — issue #877

Timestamp: 2026-09-13T11-03
Command: read-only declaration derived from the committed run artifacts, no command executed
EXIT_CODE: 0
Output Summary: M3 is the only discriminator for this fix and passed 3 of 3 on three separate post-fix runs. The M2 suite run and the `UtilitiesCS.Test` suite run are regression checks only and are explicitly non-probative. M2 has been observed to disagree with itself across two runs of identical shape. M6's two observations have different totals and are not a same-command flip.

## 1. M3 is the only discriminator

The M3 run shape — `QuickFiler.Test.dll` filtered to `FullyQualifiedName~QfcInitEmailQueueZeroBatchTests`, `/InIsolation`, with NO runsettings file — is the only discriminator for this fix. It selects a single class, so no other class in the assembly has the opportunity to install a process-global `AssemblyResolve` handler before the Deedle bind is attempted. It is strictly stricter than any suite run, because a suite run lets any earlier class rescue the bind invisibly.

## 2. The two suite runs are non-probative

The M2 run at [P2-T11] and the `UtilitiesCS.Test` run at [P2-T12] are REGRESSION CHECKS ONLY and are explicitly NON-PROBATIVE. Neither can confirm the fix and neither can refute it. A passing M2 result is not offered as evidence that the fix works, and a passing `UtilitiesCS.Test` result is not offered as evidence that the fix works. They are recorded to show that the change introduced no regression in either assembly, and for nothing else.

## 3. M3 was executed 3 times after the fix

| Run | `/ResultsDirectory:` leaf | Total | Passed | Failed | Exit code |
|---|---|---|---|---|---|
| 1 | `m3-post-1` | 3 | 3 | 0 | 0 |
| 2 | `m3-post-2` | 3 | 3 | 0 | 0 |
| 3 | `m3-post-3` | 3 | 3 | 0 | 0 |

Each run used a distinct `/ResultsDirectory:` and a distinct `LogFileName=`. The fail-before half of the pair is `evidence/regression-testing/m3-fail-before.2026-09-13T09-14.md`, recording Total 3, Failed 3, exit 1, which is cited rather than re-run.

## 4. Why M2 cannot serve as the gate, and what M6 does not claim

M2 was observed to disagree with itself across two runs of identical shape. Both runs selected the identical total of 1395 tests; one reported 1395 passed with a zero exit and the other reported 1392 passed with 3 FAILED and a non-zero exit. Because the run shape was identical and only the outcome differed, M2 is nondeterministic across runs and therefore cannot serve as the gate for this fix. That single disagreement is on its own sufficient to establish the non-probative status of M2 and of any full-suite run.

M6's two recorded observations have different totals, 9 against 13. They did not select the same set of tests, so they are NOT a same-command flip and are not offered as one. M6's value is as a single-run demonstration that the zero-batch class fails when the runner schedules it first, even with an SVG-bearing class present later in the same sequential run.
