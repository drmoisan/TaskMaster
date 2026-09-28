# Phase 4 — Toolchain Step 3: Nullable (Issue #895)

Timestamp: 2026-09-17T01-26
Task: [P4-T8]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P4-T5]` (`ACQUIRED 895`, exit 0).
Outlook gate re-checked in the same payload: `OUTLOOK-PROCESS-COUNT=0`.

Command (inside a WT-PREAMBLE payload with MSBUILD-RESOLVE):

```
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true *> "coverage/logs/p4-t8-nullable.txt"
$LASTEXITCODE
```

`/p:Nullable=enable` is deliberately absent. `/t:Rebuild` is used, never `/t:Build`.

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p4-t8-nullable.txt` (git-ignored, not committed).

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

## Acceptance

- `EXIT_CODE: 0`: yes.
- `ZERO_ERRORS_LINES` at least 1: 1.
- `ERROR_SUMMARY_LINES` equal to `ZERO_ERRORS_LINES`: 1 = 1.
- `SKIPPED_CORECOMPILE=0`: yes, so compiler and nullable-flow diagnostics actually ran.
- `CSC_OUT_LINES` at least 15: 36.

The gate passed, so the loop does not restart from `[P4-T5]`. The figures are identical to the
`[P0-T6]` baseline. Neither new test file carries a `#nullable` directive, matching the convention
of the two existing files in `TaskMaster.Test/Bootstrap/`, so no `CS86xx` diagnostic in the new code
is subject to promotion under `/p:TreatWarningsAsErrors=true`.
