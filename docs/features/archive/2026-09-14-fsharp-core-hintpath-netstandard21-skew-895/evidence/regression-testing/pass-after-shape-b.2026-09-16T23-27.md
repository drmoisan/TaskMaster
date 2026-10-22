# Phase 4 — Shape B Against the FIXED, Freshly Rebuilt Tree (Issue #895)

Timestamp: 2026-09-17T01-24
Task: [P4-T3]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P4-T1]` (`ACQUIRED 895`, exit 0).

This is the measured pass-after run for AC2. It reads the output tree produced by the `[P4-T1]`
whole-solution `/t:Rebuild`, whose log recorded `SKIPPED_CORECOMPILE=0` and a fresh own-assembly
timestamp for all fifteen projects, so no row can pass against stale output. The paired pre-fix
observation is `evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md`.

Commands (inside a WT-PREAMBLE payload, with a pre-run removal of any earlier TRX in this task's own
results directory, then VSTEST-RESOLVE, then SCOPED-RUN):

```
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~FSharpCoreDeployedIdentityTests" "/Logger:trx;LogFileName=p4-t3.trx" /ResultsDirectory:TestResults/p4-t3
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p4-t3-console.txt` (git-ignored, not committed).

## Output Summary:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
COUNTERS_TOTAL=16 EXECUTED=16 PASSED=16 FAILED=0
DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler.Test] OUTCOME=Passed
Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [VBFunctions.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskTree.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [Tags] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskTree] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [Tags.Test] OUTCOME=Passed
```

## Acceptance

- `PRERUN_TRX_COUNT=0`: yes.
- `TRX_MATCH_COUNT=1`: yes.
- `COUNTERS_TOTAL=16 EXECUTED=16 PASSED=16 FAILED=0`: yes.
- All fifteen bracketed rows read `OUTCOME=Passed`: yes, including the three that were observed
  failing at `[P1-T7]` (`[QuickFiler]`, `[QuickFiler.Test]`, `[ToDoModel]`).
- `Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed`: yes. The positive control still
  reads 2.1.0.0 from the package's own netstandard2.1 binary after the fix, so the uniformly passing
  result above is not produced by a reader that has stopped being able to see a 2.1.0.0 reference.
  This is the clause that makes the fifteen passes meaningful.
- `EXIT_CODE: 0`: yes.

Every one of the fifteen enumerated output directories now deploys a copy whose own
assembly-reference table names netstandard at 2.0.0.0 and carries no 2.1.0.0 reference. The six
flip-capable directories are no longer subject to build-order choice, because every parent directory
a transitive copy can be resolved from now holds the same flavour.
