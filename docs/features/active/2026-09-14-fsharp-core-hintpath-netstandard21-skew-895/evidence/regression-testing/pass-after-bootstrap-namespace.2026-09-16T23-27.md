# Phase 4 — Whole TaskMaster.Test.Bootstrap Namespace Against the FIXED Tree (Issue #895)

Timestamp: 2026-09-17T01-24
Task: [P4-T4]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P4-T1]` (`ACQUIRED 895`, exit 0) and released after this task
(`RELEASED by 895`, exit 0).

Commands (inside a WT-PREAMBLE payload, with a pre-run removal of any earlier TRX in this task's own
results directory, then VSTEST-RESOLVE, then SCOPED-RUN):

```
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" "/Logger:trx;LogFileName=p4-t4.trx" /ResultsDirectory:TestResults/p4-t4
$LASTEXITCODE
```

EXIT_CODE: 0
ExpectedExitCode: 0

Raw console log: `coverage/logs/p4-t4-console.txt` (git-ignored, not committed).

## Output Summary:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
COUNTERS_TOTAL=29 EXECUTED=29 PASSED=29 FAILED=0
AppConfig_DeclaresNetstandardRedirect OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS.Test] OUTCOME=Passed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskTree.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [ToDoModel.Test] OUTCOME=Passed
ThisAddIn_HasExplicitStaticConstructor OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [UtilitiesCS] OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [Tags] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskTree] OUTCOME=Passed
ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed
SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed
NegativeControl_Netstandard20Observation_IsRecorded OUTCOME=Passed
AfterInstall_BothNetstandardVersionsBind OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskVisualization] OUTCOME=Passed
EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Passed
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [VBFunctions.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler] OUTCOME=Passed
AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed
Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [QuickFiler.Test] OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [TaskMaster] OUTCOME=Passed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
DeployedFSharpCore_ReferencesNetstandard20 [Tags.Test] OUTCOME=Passed
```

The 29 results are the 9 `NetstandardBindChildDomainTests` tests (pinned `[DoNotParallelize]` by
issue #879 and unchanged here), the 2 `AddInEagerInstallShapeTests` tests, and the 18 results of the
two new classes.

## Record, Not Fix

AfterInstall_DeedleTypeInitializerSucceeds passes with or without the #879 installer now that
QuickFiler.Test deploys the netstandard2.0 flavour; its discriminating power has moved to the
display-name tests, whose negative control is unaffected. Recorded, not fixed.

## Acceptance

- `PRERUN_TRX_COUNT=0`: yes.
- `TRX_MATCH_COUNT=1`: yes.
- `COUNTERS_TOTAL=29 EXECUTED=29 PASSED=29 FAILED=0`: yes.
- `NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed`: yes. The #879 isolation
  invariant holds: the display-name bind still throws without the installer, because that test binds
  the `netstandard 2.1.0.0` display name directly rather than through `FSharp.Core`, so this fix
  does not weaken it.
- `EXIT_CODE: 0`: yes.
- The recorded sentence is present, under `## Record, Not Fix` above.
