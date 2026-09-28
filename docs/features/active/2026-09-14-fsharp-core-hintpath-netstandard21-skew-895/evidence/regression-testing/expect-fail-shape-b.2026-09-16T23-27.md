# Phase 1 — [expect-fail] Shape B Against the UNFIXED, Freshly Rebuilt Tree (Issue #895)

Timestamp: 2026-09-17T01-21
Task: [P1-T7] [expect-fail]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P1-T5]` (`ACQUIRED 895`, exit 0); released after this task.

Tree state: unfixed, and freshly rebuilt by `[P1-T5]`, whose log recorded
`SKIPPED_CORECOMPILE=0` and a fresh own-assembly timestamp for all fifteen projects. This is AC2's
required pre-fix observation. A failing run is the expected outcome of this task.

Commands (inside a WT-PREAMBLE payload, with a pre-run removal of any earlier TRX in this task's own
results directory, then VSTEST-RESOLVE, then SCOPED-RUN):

```
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~FSharpCoreDeployedIdentityTests" "/Logger:trx;LogFileName=p1-t7.trx" /ResultsDirectory:TestResults/p1-t7
$LASTEXITCODE
```

EXIT_CODE: 1
ExpectedExitCode: 1

Raw console log: `coverage/logs/p1-t7-console.txt` (git-ignored, not committed).

## Output Summary:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
COUNTERS_TOTAL=16 EXECUTED=16 PASSED=13 FAILED=3
DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster.Test] OUTCOME=Passed
Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskTree.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler] OUTCOME=Failed
DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler.Test] OUTCOME=Failed
DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [VBFunctions.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskTree] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [Tags.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [Tags] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel] OUTCOME=Failed
DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS.Test] OUTCOME=Passed
```

Failed-row identity, matched with ordinal `Contains` so `[QuickFiler]` cannot match the
`[QuickFiler.Test]` row:

```
FAILED_ROW_COUNT=3
FAILED_MESSAGE[DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler]] CONTAINS_2_1_0_0=True
FAILED_MESSAGE[DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler.Test]] CONTAINS_2_1_0_0=True
FAILED_MESSAGE[DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel]] CONTAINS_2_1_0_0=True
REQUIRED_FAILED_ROW [QuickFiler] PRESENT=True
REQUIRED_FAILED_ROW [QuickFiler.Test] PRESENT=True
REQUIRED_FAILED_ROW [ToDoModel] PRESENT=True
```

Representative failure message:

```
Expected netstandardVersions[0] to be equal to 2.0.0.0 because only netstandard 2.0.0.0 resolves on .NET Framework, so the copy deployed into QuickFiler must reference that version, but found 2.1.0.0.
```

## Additional Failing Rows:

NONE

Exactly the three deterministic rows failed. The six flip-capable directories identified by the
research (`TaskTree`, `TaskVisualization`, `TaskTree.Test`, `TaskVisualization.Test`, `TaskMaster`,
`TaskMaster.Test`) all received the netstandard2.0 flavour on this build. That is an observed
build-order outcome for this run, recorded rather than gated: it is exactly the last-writer-wins
nondeterminism the defect statement describes, and it is why the per-directory criterion covers all
fifteen rather than a sample.

## Acceptance

- `PRERUN_TRX_COUNT=0`: yes.
- `TRX_MATCH_COUNT=1`: yes.
- `COUNTERS_TOTAL=16 EXECUTED=16`: yes. Discovery found all fifteen data rows plus the control, so
  the `[DataRow]` wiring is live.
- `FAILED` at least 3 and at most 15: 3.
- The three `OUTCOME=Failed` rows are `[QuickFiler]`, `[QuickFiler.Test]` and `[ToDoModel]`: yes,
  confirmed by ordinal `Contains`.
- `Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed`: yes. The positive control reads
  the package's own netstandard2.1 binary and reports 2.1.0.0, so the reader can see a 2.1.0.0
  reference and a uniformly passing result could not be produced by a reader that finds nothing.
- Every `FAILED_MESSAGE[` line for a failed row contains the token `2.1.0.0`: yes, all three.
- `EXIT_CODE: 1` with `ExpectedExitCode: 1`: yes.

Neither blocking branch was taken: the positive control passed, and `TOTAL` is 16 as expected.
