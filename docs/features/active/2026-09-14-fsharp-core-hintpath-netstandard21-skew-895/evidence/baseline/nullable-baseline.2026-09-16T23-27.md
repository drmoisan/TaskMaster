# Phase 0 — Nullable Baseline (Issue #895)

Timestamp: 2026-09-17T01-14
Task: [P0-T6]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: acquired (`ACQUIRED 895`, exit 0); held across `[P0-T7]` and released after it.
Outlook gate re-checked in the same payload: `OUTLOOK-PROCESS-COUNT=0`.

Command (inside a WT-PREAMBLE payload with MSBUILD-RESOLVE):

```
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true *> "coverage/logs/p0-t6-nullable.txt"
$LASTEXITCODE
```

`/p:Nullable=enable` is deliberately absent, per the toolchain invariant. `/t:Rebuild` is used, never
`/t:Build`.

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p0-t6-nullable.txt` (git-ignored, not committed).

## Output Summary:

MSBuild summary values read from the log:

```
0 Warning(s)
0 Error(s)
```

Measured counts:

```
ZERO_ERRORS_LINES=1
ERROR_SUMMARY_LINES=1
SKIPPED_CORECOMPILE=0
CSC_OUT_LINES=36
```

`CSC_OUT_LINES=36` is the positive control paired with the `SKIPPED_CORECOMPILE=0` zero-count
assertion, read from the same log.

This build also produced the `bin\Debug` output tree that `[P0-T7]` runs against.

## Acceptance

- `EXIT_CODE: 0`: yes.
- `ZERO_ERRORS_LINES` at least 1: 1.
- `ERROR_SUMMARY_LINES` equal to `ZERO_ERRORS_LINES`: 1 = 1.
- `SKIPPED_CORECOMPILE=0`: yes.
- `CSC_OUT_LINES` at least 15: 36.
