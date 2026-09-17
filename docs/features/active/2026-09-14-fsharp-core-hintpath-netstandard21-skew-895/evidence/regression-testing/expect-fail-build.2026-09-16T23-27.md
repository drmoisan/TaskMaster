# Phase 1 — Fresh Whole-Solution Rebuild of the UNFIXED Tree (Issue #895)

Timestamp: 2026-09-17T01-20
Task: [P1-T5]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: acquired before this task (`ACQUIRED 895`, exit 0) and held across `[P1-T6]` and
`[P1-T7]`, which read the output tree this build produced; released after `[P1-T7]`.

Tree state: unfixed (all three netstandard2.1 HintPaths still in place), with the two new test files
created and registered. This is the build AC2's pre-fix observation reads. A warm or partial build
would let Shape B pass vacuously against stale output, which is why the rebuild is whole-solution.

OUTLOOK-PROCESS-COUNT=0
BUILD-START-UTC=2026-09-17T05:19:31.6181202Z

Command (inside a WT-PREAMBLE payload with MSBUILD-RESOLVE):

```
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" *> "coverage/logs/p1-t5-build.txt"
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p1-t5-build.txt` (git-ignored, not committed).

## Output Summary:

```
ZERO_ERRORS_LINES=1
SKIPPED_CORECOMPILE=0
QuickFiler CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
QuickFiler.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
ToDoModel CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
UtilitiesCS CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
UtilitiesCS.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
ToDoModel.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
Tags CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
Tags.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
VBFunctions.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskTree CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskTree.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskVisualization CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskVisualization.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskMaster CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
TaskMaster.Test CSC_OUT=2 OWN_DLL_FRESH=True FSHARPCORE_PRESENT=True
```

## PLAN-LITERAL DEVIATION: CSC_OUT reads 2, not the expected 1

The task's acceptance names the literal `CSC_OUT=1` for each of the fifteen lines. The measured
value is uniformly 2. The cause was identified by reading the matching lines rather than inferred,
and the plan's underlying claim is unaffected.

At this MSBuild version's normal verbosity, two distinct log lines carry the `/out:obj\Debug\<name>.dll`
token for a single compilation of one project:

1. the echoed `csc.exe` command line, which begins with the full path of `csc.exe` followed by
   `/noconfig /nowarn:1701,1702 ...`; and
2. a `BuildResponseFile = '...'` property echo, which repeats the same switch list.

Both are emitted by one compilation. The expectation of exactly 1 is therefore not reachable by any
correct build in this environment, and a value of 2 is not evidence of a second compilation.

The same phenomenon is visible in the Phase 0 baselines: `[P0-T5]` and `[P0-T6]` each recorded
`CSC_OUT_LINES=36` across 18 projects, which is two per project. Those tasks gate on a lower bound
(`at least 15`), so the deviation did not surface there.

A disambiguating measurement was run over the same log to recover the value the clause intends —
how many times the compiler was actually invoked for each project:

```
QuickFiler CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
QuickFiler.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
ToDoModel CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
UtilitiesCS CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
UtilitiesCS.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
ToDoModel.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
Tags CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
Tags.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
VBFunctions.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
TaskTree CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
TaskTree.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
TaskVisualization CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
TaskVisualization.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
TaskMaster CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
TaskMaster.Test CSC_OUT=2 CSC_INVOCATIONS=1 RESPONSEFILE_ECHOES=1
```

`CSC_INVOCATIONS=1` for all fifteen projects, with the second hit accounted for in every case as the
response-file echo. Each project was compiled exactly once by this build.

The plan is not edited. The deviation is recorded here and escalated in the executor's completion
report.

## Acceptance

- `OUTLOOK-PROCESS-COUNT=0`: yes.
- `EXIT_CODE: 0`: yes.
- `ZERO_ERRORS_LINES` at least 1: 1.
- `SKIPPED_CORECOMPILE=0`: yes. Every project ran `CoreCompile`; no project was skipped by MSBuild
  incrementality, so this is a real whole-solution rebuild.
- `OWN_DLL_FRESH=True` for all fifteen: yes. Every project's own compiled assembly was written after
  `BUILD-START-UTC`. Freshness is asserted on the compiled assembly rather than on
  `FSharp.Core.dll`, because a copied reference keeps its source timestamp.
- `FSHARPCORE_PRESENT=True` for all fifteen: yes. Every one of the fifteen enumerated output
  directories received a copy, so no Shape B row can pass because no file was there to inspect.
- `CSC_OUT=1` for all fifteen: NOT AS WRITTEN. Measured 2 uniformly, with `CSC_INVOCATIONS=1`
  uniformly. See the deviation section above. The clause's purpose — one real compilation per
  project in this build — is met and is evidenced by `CSC_INVOCATIONS` together with
  `SKIPPED_CORECOMPILE=0` and `OWN_DLL_FRESH=True`.
