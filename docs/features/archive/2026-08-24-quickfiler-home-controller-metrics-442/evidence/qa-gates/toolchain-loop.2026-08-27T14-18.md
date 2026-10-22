# Phase 6 — Toolchain Loop Closure

Timestamp: 2026-08-27T14-18
Task: [P6-T9]
Command: reconciliation of the [P6-T1] through [P6-T5] records for the final pass
EXIT_CODE: 0

## Output Summary

**The loop closed in a single clean pass.** All four acceptance conditions hold:

1. the recorded order is format, then check, then analyzers, then nullable, then coverage-enabled test;
2. every recorded exit code for [P6-T2], [P6-T3] and [P6-T4] is zero;
3. the formatter rewrote **zero** files in the final pass;
4. [P6-T5] recorded **zero** failed tests.

## Final pass (pass of record)

| Step | Task | Start (UTC) | End (UTC) | Exit code | Key result |
| --- | --- | --- | --- | --- | --- |
| 1 Format | [P6-T1] | 2026-08-27T14:17:20Z | 2026-08-27T14:17:23Z | **0** | 0 of 7 owned files rewritten (SHA-256 before/after) |
| 2 Check | [P6-T2] | 2026-08-27T14:17:23Z | 2026-08-27T14:17:28Z | **0** | 1540 files checked, none unformatted |
| 3 Analyzers | [P6-T3] | 2026-08-27T14:17:28Z | 2026-08-27T14:17:50Z | **0** | 0 errors, 5 pre-existing warnings |
| 4 Nullable | [P6-T4] | 2026-08-27T14:17:50Z | 2026-08-27T14:18:12Z | **0** | 0 errors, 0 `CS86xx` |
| 5 Coverage test | [P6-T5] | 2026-08-27T14:18:12Z | 2026-08-27T14:19:36Z | **0** | 6701 tests, 6701 passed, 0 failed |

Files the formatter rewrote in the final pass: **0**.

### Non-vacuity of the two build gates

Both build gates used `/t:Rebuild`, and both are demonstrably non-vacuous:

| Gate | `Skipping target "CoreCompile"` occurrences | `CoreCompile:` executions |
| --- | --- | --- |
| [P6-T3] analyzers | **0** | 51 |
| [P6-T4] nullable | **0** | 54 |

A warm `/t:Build` would have returned exit 0 with `CoreCompile` skipped on every project, running no
analyzer and producing no nullable diagnostic; neither gate did that.

## The two aborted attempts, recorded in full

The Phase 6 restart rule ("if any step fails, or if the formatter modifies any file, restart this
phase from [P6-T1]") fired once in this session. Both attempts are recorded so the transcript is
complete.

### Attempt A — 2026-08-27T13:54, aborted at step 3

| Step | Start (UTC) | Exit code | Result |
| --- | --- | --- | --- |
| 1 Format | 13:54:43Z | 0 | 0 of 7 files rewritten |
| 2 Check | 13:54:52Z | 0 | 1540 files checked |
| 3 Analyzers | 13:54:58Z | **1** | 28 errors, all `MSB3021`/`MSB3027` file-copy contention |
| 4 Nullable | — | not reached | |
| 5 Coverage test | — | not reached | |

Every one of the 28 errors read `The file is locked by: "testhost (84376)"` or the equivalent
`The process cannot access the file ... because it is being used by another process`, against
`bin\Debug` outputs of `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`,
`TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test` and
`VBFunctions.Test`. **Zero compiler diagnostics were emitted.** The failure was contention, not a
defect in the change under test.

### Cause, and how it was resolved

A coverage-enabled vstest run was already live in this worktree when the session resumed. It was
launched at 2026-08-27T13:46:27Z, before the resume, and held the build outputs that attempt A tried
to replace. Process identities at the time of observation: `dotnet-coverage` pid 1444,
`vstest.console` pid 24748, `testhost` pid 84376, all naming this worktree in their command lines.

That run was then confirmed **hung**, not merely slow:

- 28.7 seconds of CPU accumulated in `testhost` across 30 minutes of wall time;
- no `.trx` written into `TestResults/` at any point during those 30 minutes, the most recent one
  dating from 2026-08-26T11:26;
- no coverage output written, the `coverage/coverage.cobertura.xml` on disk still dating from
  2026-08-26T11:30.

Its result would in any case have been invalid: the `/t:Rebuild` in attempt A had already deleted
and replaced the assemblies it had loaded, so it was measuring a tree that no longer existed. It was
terminated, the worktree was confirmed free of any contending test process, and the phase was
restarted from [P6-T1]. This is recorded rather than omitted because the terminated run was not
started by this session and its termination is a deliberate, reasoned action rather than routine
cleanup.

### Attempt B — the final pass

Recorded in the table at the top of this artifact. Clean at every step.

## Bearing on AC-23

AC-23 requires that the four commands in the spec's Test Strategy ran in order, that the final pass
completed with zero errors, and that no file was modified by the formatter, with the transcript
recorded under `evidence/qa-gates/`. The final-pass table above is that transcript, and its
per-step artifacts are:

- `evidence/qa-gates/csharpier-format.2026-08-27T14-18.md`
- `evidence/qa-gates/csharpier-check.2026-08-27T14-18.md`
- `evidence/qa-gates/msbuild-analyzers.2026-08-27T14-18.md`
- `evidence/qa-gates/msbuild-nullable.2026-08-27T14-18.md`
- `evidence/qa-gates/mstest-coverage.2026-08-27T14-19.md`
