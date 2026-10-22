# Phase 4 — Shape A Against the FIXED Tree (Issue #895)

Timestamp: 2026-09-17T01-24
Task: [P4-T2]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P4-T1]` (`ACQUIRED 895`, exit 0).

This is the measured pass-after run for AC1. The paired pre-fix observation is
`evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md`.

Commands (inside a WT-PREAMBLE payload, with a pre-run removal of any earlier TRX in this task's own
results directory, then VSTEST-RESOLVE, then SCOPED-RUN):

```
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~FSharpCoreHintPathAlignmentTests" "/Logger:trx;LogFileName=p4-t2.trx" /ResultsDirectory:TestResults/p4-t2
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

Console summary:

```
Test Parallelization enabled for ...\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed SolutionHasExactlySixFSharpCoreHintPaths [184 ms]
  Passed EveryFSharpCoreHintPath_SelectsNetstandard20 [119 ms]
Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.3462 Seconds
```

An all-green run prints no `Failed:` and no `Skipped:` line, which is why the gated counts are read
from the TRX rather than from the console text.

TRX-READ output:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
COUNTERS_TOTAL=2 EXECUTED=2 PASSED=2 FAILED=0
EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Passed
SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed
```

## Acceptance

- `PRERUN_TRX_COUNT=0`: yes.
- `TRX_MATCH_COUNT=1`: yes.
- `COUNTERS_TOTAL=2 EXECUTED=2 PASSED=2 FAILED=0`: yes.
- Both tests `OUTCOME=Passed`: yes.
- `EXIT_CODE: 0`: yes.

The same two tests were observed failing and passing respectively on the unfixed tree at `[P1-T6]`,
so the flavour assertion is known to be able to fail and this pass is attributable to the fix rather
than to an assertion that cannot fail.
