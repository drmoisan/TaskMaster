# Phase 6 — Toolchain Loop Closure

Timestamp: 2026-08-26T11-30
Task: [P6-T9]
Command: aggregation of the [P6-T1] through [P6-T5] artifacts for the final pass
EXIT_CODE: 0

## Acceptance status

**The loop did NOT close in a clean pass.** Four of the five conditions hold; the fifth does not.

| Condition | Required | Observed | Holds |
| --- | --- | --- | --- |
| Order is format, check, analyzers, nullable, coverage-enabled test | yes | yes | **yes** |
| [P6-T2] exit code | 0 | 0 | **yes** |
| [P6-T3] exit code | 0 | 0 | **yes** |
| [P6-T4] exit code | 0 | 0 | **yes** |
| Formatter rewrote zero files in the final pass | 0 | 0 | **yes** |
| [P6-T5] recorded zero failed tests | 0 | **1** | **no** |

## Final-pass record, in order

| Order | Task | Step | Timestamp | EXIT_CODE |
| --- | --- | --- | --- | --- |
| 1 | [P6-T1] | CSharpier format, scoped to the seven owned files | 2026-08-26T11-27 | **0** |
| 2 | [P6-T2] | CSharpier check, repository-wide, read-only | 2026-08-26T11-27 | **0** |
| 3 | [P6-T3] | msbuild analyzer gate, `/t:Rebuild` | 2026-08-26T11-27 | **0** |
| 4 | [P6-T4] | msbuild nullable / type-check gate, `/t:Rebuild` | 2026-08-26T11-27 | **0** |
| 5 | [P6-T5] | coverage-enabled full test suite | 2026-08-26T11-30 | **1** |

**Files the formatter rewrote in the final pass: 0.**

Verified by SHA-256 comparison of each of the seven owned files before and after the command, not by
the tool's processed-file count. All seven hashes were byte-identical to their pass-1 results.

## Pass history

**Pass 1.** [P6-T1] rewrote four of the seven owned files: `QfcHomeController.cs`,
`QfcHomeController.Metrics.cs`, `EfcHomeController.Metrics.cs`, and
`EfcHomeController.ExecuteMoves.cs`. The phase preamble requires restarting from [P6-T1] when the
formatter modifies any file, so the remaining pass-1 steps were not run and the phase restarted
immediately.

**Pass 2, the final pass.** [P6-T1] rewrote zero files, reaching a formatter fixed point.
[P6-T2] through [P6-T4] each exited 0. [P6-T5] exited 1 with one failing test.

## Why the phase was not restarted again

Restarting Phase 6 from [P6-T1] is the prescribed response to a failing step. It was not done,
because it cannot change the outcome and would consume roughly forty minutes per iteration without
converging.

The single [P6-T5] failure is not a flake, not an ordering artifact, and not a stale-output
artifact. It is a deterministic type mismatch:
`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64` injects a `System.Boolean` by reflection
into `_isExecuting`, which [P3-T5] changed to `private int` as AC-14 requires, and
`FieldInfo.SetValue` rejects that conversion on every run. Re-running the format, lint, and
type-check steps cannot affect it, and the test file that would have to change is on this plan's
forbidden-to-write list and is gated by [P7-T6].

The full diagnosis, the one-line delta that resolves it, and the reasoning for not applying that
delta unilaterally are recorded in `evidence/qa-gates/mstest-coverage.2026-08-26T11-30.md`. The
condition is escalated rather than looped on.

## Consequence for acceptance criteria

- AC-23 (full toolchain pass with zero errors and no file modified by the formatter) is **not
  satisfied**, because step 4 of the toolchain did not complete with zero failures.
- The format, lint, and type-check obligations of AC-23 are individually satisfied and are recorded
  in their own artifacts.
