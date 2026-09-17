# Phase 4 — Fresh Whole-Solution Rebuild of the FIXED Tree (Issue #895)

Timestamp: 2026-09-17T01-23
Task: [P4-T1]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: acquired before this task (`ACQUIRED 895`, exit 0) and held across `[P4-T2]` to
`[P4-T4]`, which read the output tree this build produced.

Tree state: fixed. All six `FSharp.Core` HintPaths select the netstandard2.0 flavour, the two new
test files are present and registered, and the `<remarks>` correction is applied.

OUTLOOK-PROCESS-COUNT=0
BUILD-START-UTC=2026-09-17T05:23:10.8820050Z

Command (inside a WT-PREAMBLE payload with MSBUILD-RESOLVE):

```
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "coverage/logs/p4-t1-build.txt"
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p4-t1-build.txt` (git-ignored, not committed).

## Output Summary:

```
ZERO_ERRORS_LINES=1
SKIPPED_CORECOMPILE=0
QuickFiler CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
QuickFiler.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
ToDoModel CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
UtilitiesCS CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
UtilitiesCS.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
ToDoModel.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
Tags CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
Tags.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
VBFunctions.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskTree CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskTree.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskVisualization CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskVisualization.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskMaster CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskMaster.Test CSC_OUT=2 CSC_INVOCATIONS=1 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
```

## Acceptance

- `OUTLOOK-PROCESS-COUNT=0`: yes.
- `EXIT_CODE: 0`: yes.
- `ZERO_ERRORS_LINES` at least 1: 1.
- `SKIPPED_CORECOMPILE=0`: yes. This is a real whole-solution rebuild, so the Shape B run that
  follows reads output this build produced rather than stale output.
- `OWN_DLL_FRESH=True` for all fifteen: yes.
- `FSHARPCORE_PRESENT=True` for all fifteen: yes.
- `CSC_OUT=1` for all fifteen: NOT AS WRITTEN, exactly as recorded at `[P1-T5]`. The measured value
  is 2 for every project because two log lines carry the `/out:` token for one compilation: the
  echoed `csc.exe` command line and a `BuildResponseFile = '...'` property echo. The disambiguating
  measurement is included above and reads `CSC_INVOCATIONS=1` for all fifteen, so each project was
  compiled exactly once. The deviation is identical in cause and consequence to the one recorded at
  `[P1-T5]`, and is escalated in the executor's completion report.
