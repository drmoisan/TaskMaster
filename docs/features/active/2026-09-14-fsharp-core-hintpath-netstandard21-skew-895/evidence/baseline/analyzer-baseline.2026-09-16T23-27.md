# Phase 0 — Analyzer Baseline (Issue #895)

Timestamp: 2026-09-17T01-13
Task: [P0-T5]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: acquired (`ACQUIRED 895`, exit 0) and released (`RELEASED by 895`, exit 0).
Outlook gate re-checked in the same payload: `OUTLOOK-PROCESS-COUNT=0`.

Command (inside a WT-PREAMBLE payload, after `New-Item -ItemType Directory -Force -Path
"coverage/logs"` and MSBUILD-RESOLVE):

```
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true *> "coverage/logs/p0-t5-analyzer.txt"
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p0-t5-analyzer.txt` (git-ignored, not committed).

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

`CSC_OUT_LINES` is the positive control: 36 echoed compiler command lines were written to the log,
so `SKIPPED_CORECOMPILE=0` is an observation over a log that recorded real compilation rather than
an artefact of an empty log. `ERROR_SUMMARY_LINES` equals `ZERO_ERRORS_LINES`, so the only
`N Error(s)` summary line in the log is the zero one; the anchored pattern rules out a `10 Error(s)`
substring match.

## Acceptance

- `EXIT_CODE: 0`: yes.
- `ZERO_ERRORS_LINES` at least 1: 1.
- `ERROR_SUMMARY_LINES` equal to `ZERO_ERRORS_LINES`: 1 = 1.
- `SKIPPED_CORECOMPILE=0`: yes.
- `CSC_OUT_LINES` at least 15: 36.
