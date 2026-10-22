# P5-T5 — Iteration 1, Failed: Environmental File Contention

Timestamp: 2026-09-17T02-32

This record documents the first iteration of the P5-T1 through P5-T7 toolchain loop, which did not
complete. It is written so the loop's iteration count is auditable. The canonical
`p5-t5-mstest-coverage` artifact records the iteration that completed; this file carries a distinct
name so that the artifact glob for that task still matches exactly one file.

Command: `CMD-COVERAGE` with `STAGE` `final`.

EXIT_CODE: 1

ExpectedExitCode: 0

CHANNEL: COMMAND

The declared expectation for this step is exit 0, because the acceptance condition requires
`NEWLY-FAILING: NONE`. The observed exit code is 1 and the expectation was not met, so this
iteration failed and the loop restarts from P5-T1.

## What happened

    Total tests: 7288
         Passed: 7287
         Failed: 1
    COLLECT_EXIT_CODE: 1
    ASSEMBLY_COUNT: 9
    COUNTERS total=7288 executed=7288 passed=7287 failed=1

FINAL-FAILING-SET (iteration 1):

    UtilitiesCS.Test.HelperClasses.FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile

Failure message, verbatim except that the absolute path is redacted:

    Test method UtilitiesCS.Test.HelperClasses.FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile threw exception:
    System.IO.IOException: The process cannot access the file '<repo-root>\TaskMaster.sln' because it is being used by another process.

NEWLY-FAILING (iteration 1): `OpenRead_ShouldReturnReadableStreamForWrappedFile`

P0-T10's `BASELINE-FAILING-SET:` recorded no failing test, so this name is not in it and
`NEWLY-FAILING:` is not `NONE`. The acceptance condition therefore fails and no `ExpectedExitCode: 1`
waiver is written: the plan's rule is that a non-`NONE` newly-failing set fails the task rather than
being normalized.

Both in-scope tests passed in this run. The failure is in `UtilitiesCS.Test` and is unrelated to the
change under this item, which touches one file in `QuickFiler.Test` and no production code.

## Diagnosis

The failing test opens the repository's own `TaskMaster.sln` through a filesystem-wrapper adapter
and asserts it gets a readable stream. It failed because another process held that file at the
moment it ran.

Process inventory taken immediately after the run identified seventeen `MSBuild.exe` processes, each
with the command line
`MSBuild.exe /noautoresponse /nologo /nodemode:1 /nodeReuse:true /low:false` and each created at
`2026-09-17T02-12-26`. These are MSBuild node-reuse worker processes: MSBuild's `/m` switch leaves
worker nodes resident after a build so the next build can reuse them, and the plan's mandated
analyzer and nullable gates both pass `/m`. Their creation timestamp falls inside this run's session
and inside a window in which this item held the shared build lock, so they descend from this run's
own `msbuild` invocations rather than from a sibling worktree. Their command lines carry no project
path, which is normal for node-reuse workers and is why ownership was established from the creation
time and the lock window rather than from the command line.

The contention is therefore self-inflicted by the toolchain rather than caused by a sibling: the
plan's own P5-T3 and P5-T4 solution rebuilds, which completed about thirty seconds before the
coverage run started, left resident nodes holding the solution file.

It is a race rather than a certainty. The P0-T10 baseline run followed the same two rebuilds with a
comparable gap and reported 7288 of 7288 passing, with the same node processes resident.

## Action taken

1. The shared build lock was released, as the protocol requires even when the command fails.
2. The seventeen `MSBuild.exe` node-reuse workers created during this session were terminated, so
   the machine is quiescent before the loop is retried. The selection predicate was name
   `MSBuild.exe`, command line containing `/nodemode:1`, and creation time at or after this
   session's start. Node-reuse workers are disposable by design: a subsequent build starts new ones,
   and terminating an idle one cannot corrupt build state. Verified afterwards:
   `MSBUILD_REMAINING: 0`.
3. Two `vstest.console.exe` processes created on 2026-09-15, which predate this session, were
   identified and deliberately left running. Nothing belonging to another session was touched.
4. The loop restarts from P5-T1, which is the plan's own rule when any step of the toolchain loop
   fails.

No test was serialised, no `[DoNotParallelize]` was added, no retry or timing tolerance was
introduced, no test was modified, no run settings were changed, and no plan command was altered. The
only action taken was to restore machine quiescence between two plan tasks and to re-run the loop as
the plan directs.

## Iteration 1, steps P5-T1 through P5-T4, for the record

These four steps all met their expectations in iteration 1 before P5-T5 failed. Their per-step
artifacts are rewritten for iteration 2, so their iteration 1 values are preserved here:

| Step | Iteration 1 result |
| --- | --- |
| P5-T1 repository-wide format | `EXIT_CODE: 0`; `Formatted 1641 files in 5445ms.`; `FORMAT_CHANGED_TREE: False`; one changed path under the source pathspecs, the Write Set file |
| P5-T2 repository-wide check | `EXIT_CODE: 0`; `Checked 1641 files in 5367ms.`; `CHECKED-DELTA: 0` |
| P5-T3 analyzer gate | `EXIT_CODE: 0`; `WARNINGS: 0`; `ERRORS: 0`; `CSC_OUT_LINES: 2`; `FILE-DIAGNOSTIC-LINES: 0` |
| P5-T4 nullable gate | `EXIT_CODE: 0`; `WARNINGS: 0`; `ERRORS: 0`; `CSC_OUT_LINES: 2`; `FILE-DIAGNOSTIC-LINES: 0`; `TEST-DLL-EXISTS: True` |

## Latent defect, out of scope

`UtilitiesCS.Test.HelperClasses.FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile`
depends on a real repository file, `TaskMaster.sln`, which other processes on the machine legitimately
open. That makes the test's outcome a function of ambient machine state rather than of the unit under
test, which the repository's unit-test policy prohibits under environment stability. It is not fixed
here and is reported to the orchestrator in the executor's final message for promotion.
