# [P7-T6] Toolchain Loop Closure

Timestamp: 2026-09-08T10-24

LOOP: CLEAN PASS

## Step-by-step record

| Step | Artifact | Recorded EXIT_CODE | Declared expectation |
| --- | --- | --- | --- |
| [P7-T1] | `evidence/qa-gates/p7-t1-csharpier-format.md` | 0 | `EXIT_CODE: 0` |
| [P7-T2] | `evidence/qa-gates/p7-t2-csharpier-check.md` | 0 | `EXIT_CODE: 0` |
| [P7-T3] | `evidence/qa-gates/p7-t3-msbuild-analyzers.md` | 0 | `EXIT_CODE: 0` |
| [P7-T4] | `evidence/qa-gates/p7-t4-msbuild-nullable.md` | 0 | `EXIT_CODE: 0` |
| [P7-T5] | `evidence/qa-gates/p7-t5-tests-coverage.md` | 0 | `ExpectedExitCode: 0`, keyed to `BASELINE-FAILED-TESTS: 0` from [P0-T12] |

EXPECTATION-MET: [P7-T1] YES
EXPECTATION-MET: [P7-T2] YES
EXPECTATION-MET: [P7-T3] YES
EXPECTATION-MET: [P7-T4] YES
EXPECTATION-MET: [P7-T5] YES

## The rule applied

`LOOP: CLEAN PASS` means every step met its own declared expectation, not that every step returned 0. [P7-T1] through [P7-T4] each declare `EXIT_CODE: 0` and each returned 0. [P7-T5] declares an `ExpectedExitCode:` keyed to `BASELINE-FAILED-TESTS` from [P0-T12]; that baseline value is 0, so its declared expectation is 0, its observed exit code is 0, and its `NEWLY-FAILING: NONE` line holds. On this tree the two readings coincide, because every step returned 0 on its own terms.

All five `EXPECTATION-MET:` lines read `YES`, so `LOOP: CLEAN PASS` is written. Had any read `NO`, this artifact would record `LOOP: NOT CLEAN` and the loop would restart from [P7-T1] rather than proceeding to [P7-T7].

## Order and the no-intervening-change requirement

The five steps ran in the order [P7-T1], [P7-T2], [P7-T3], [P7-T4], [P7-T5], and no file changed after the final [P7-T1] pass.

[P7-T1] itself required two passes. Its first pass rewrote `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`, which under D4 is a loop restart, so the formatter was run again rather than proceeding; the second pass changed nothing and reported both of its observation spans identical. Every subsequent step of this loop ran against the tree as that final pass left it. That ordering is what makes the four gates below it meaningful: had [P7-T2] through [P7-T5] been run against the pre-rewrite tree and the rewrite applied afterwards, none of their results would describe the tree that is committed.

The independent confirmation that nothing changed after the final format pass is [P7-T2] itself. `dotnet tool run csharpier check .` is read-only and exits non-zero on any drift; it ran after the final [P7-T1] pass, exited 0, and reported no drifting file.
