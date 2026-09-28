# P5-T8 — Final QC loop result

Timestamp: 2026-09-13T16-55
Command: none; this task records the outcome of the seven command tasks P5-T1 through P5-T7
EXIT_CODE: 0

## The pass that completed

CompletedLoopPass: 1

Pass 1 through P5-T1 to P5-T7 completed with every step exiting as required and with P5-T1 rewriting no
file. The loop therefore did not restart. No step in this phase was skipped and none was recorded as
skipped.

The formatter-rewrote-nothing condition is the load-bearing one, because a pass in which the formatter
rewrote a file is not a completed loop under this plan and under the standing instructions file's
restart rule. It is established here by the two porcelain captures P5-T1 recorded immediately before and
immediately after the format command, which are byte-identical, and corroborated by the read-only check
in P5-T2, which inspected 1632 files and named none. Neither observation rests on the formatter's exit
code, which is 0 whether it rewrote a file or not.

## The seven artifacts of that pass

| Step | Task | Command class | Artifact | Result |
|---|---|---|---|---|
| 1 | P5-T1 | repository-wide format | p5-t1-format.2026-09-12T10-25.md | exit 0, rewrote no file |
| 2 | P5-T2 | repository-wide format check | p5-t2-check.2026-09-12T10-25.md | exit 0, named no file |
| 3 | P5-T3 | analyzer rebuild | p5-t3-analyze.2026-09-12T10-25.md | exit 0, 0 errors, 0 warnings |
| 4 | P5-T4 | nullable rebuild | p5-t4-nullable.2026-09-12T10-25.md | exit 0, 0 errors, 0 warnings |
| 5 | P5-T5 | coverage-mode test run | p5-t5-coverage-postchange.2026-09-12T10-25.md | 1423 of 1423 passed, exit 1 against declared ExpectedExitCode 1 |
| 6 | P5-T6 | post-format line counts | p5-t6-line-counts-final.2026-09-12T10-25.md | all seven counts under 500 |
| 7 | P5-T7 | whole-assembly test run | p5-t7-tests-final.2026-09-12T10-25.md | exit 0, 1423 of 1423 passed, failed=0 |

All seven artifacts are written under the qa-gates evidence directory of this feature folder. The
Cobertura document P5-T5 produced sits alongside its Markdown interpretation in the same directory as
coverage-postchange.2026-09-12T10-25.cobertura.xml.

## Why the P5-T5 exit code does not restart the loop

P5-T5 observed exit code 1. That exit code originates in the coverage runner's own document-level 80
percent assertion, which is a repository-wide gate evaluated over a denominator spanning all six
first-party packages while this run's numerator comes from the single test assembly the plan scopes it
to. The plan states that this assertion is not the task's gate either way, the artifact declares
`ExpectedExitCode: 1`, and the underlying test outcome inside that run was 1423 of 1423 passed with
`failed=0`. The toolchain step this task represents — testing — therefore passed; the non-zero code
reports a repository-wide coverage figure, which P6-T4 handles by projection. Nothing in the run
rewrote a file, so there is no restart trigger of either kind.

## Order the loop was run in

Format, then format check, then analyzers, then nullable, then tests in coverage mode, then the
post-format size measurement, then the whole-assembly test run. This is the order the standing
instructions file mandates for C# work, with the coverage-mode run supplying the numeric coverage
evidence the coverage evidence contract requires and the plain run supplying the counters the
no-regression gate reads.

Output Summary: Loop pass 1 completed. P5-T1 rewrote no file, proved by two byte-identical porcelain
captures and corroborated by P5-T2 naming no drifting file across 1632 inspected. Analyzers and nullable
both exited 0 at 0 errors and 0 warnings. Tests ran twice, under coverage and plain, at 1423 of 1423
passed with `failed=0` both times. All seven post-format line counts are under 500. The loop did not
restart and no step was skipped. Acceptance met.
