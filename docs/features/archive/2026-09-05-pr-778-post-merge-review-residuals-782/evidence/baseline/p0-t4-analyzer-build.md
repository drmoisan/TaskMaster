# Baseline — Analyzer Build (P0-T4, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-56

## Why the earlier record is superseded, and what the re-measurement changed

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the record had to be
re-taken at the re-anchored base whether or not its figures moved.

**This is the one gate whose figures the re-measurement left unchanged.** The re-measurement
reproduced the superseded figures exactly: exit 0, `    0 Warning(s)`, `    0 Error(s)`, and 18
distinct project build-output lines. A reader must not read the absence of a numeric change as a
failure to re-measure. The gate was re-run at `736c2cf2` and returned the same figures, which is the
expected outcome: the main advance added no project and removed none, and `TaskMaster.sln` declares
18 projects at both commits.

## Measurement method and measuring party

This gate was measured by the **orchestrator, not the executor**, at the re-anchored base commit
`736c2cf2`, by the temporary-restore method: the orchestrator restored the six Write Set source
files Phase 1 has changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

The executor did **not** re-run the analyzer build for this task, and this artifact does not present
the figure as an executor run.

Command (the orchestrator's command):

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

That is the exit code the **orchestrator** observed, not an exit code the executor observed.

BASELINE_PROJECT_COUNT: 18

Output Summary:

The summary warning and error lines, verbatim, as the orchestrator observed them:

```text
    0 Warning(s)
    0 Error(s)
```

Both hard conditions hold: `    0 Warning(s)` and `    0 Error(s)` are recorded exactly.

**Project build-output line count: 18.** The count was taken over lines of the arrow form
`<ProjectName> -> <output assembly path>` in the build log. Both the total count and the distinct
count are 18, so the figure is not an artifact of de-duplication. The eighteen projects are:

```text
QuickFiler -> ...\QuickFiler\bin\Debug\QuickFiler.dll
QuickFiler.Test -> ...\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl -> ...\SVGControl\bin\Debug\SVGControl.dll
SVGControl.Test -> ...\SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags -> ...\Tags\bin\Debug\Tags.dll
Tags.Test -> ...\Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster -> ...\TaskMaster\bin\Debug\TaskMaster.dll
TaskMaster.Test -> ...\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree -> ...\TaskTree\bin\Debug\TaskTree.dll
TaskTree.Test -> ...\TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization -> ...\TaskVisualization\bin\Debug\TaskVisualization.dll
TaskVisualization.Test -> ...\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel -> ...\ToDoModel\bin\Debug\ToDoModel.dll
ToDoModel.Test -> ...\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS -> ...\UtilitiesCS\bin\Debug\UtilitiesCS.dll
UtilitiesCS.Test -> ...\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions -> ...\VBFunctions\bin\Debug\VBFunctions.dll
VBFunctions.Test -> ...\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

The absolute output paths are elided above to keep the host account and machine name out of this
artifact; the project name and the relative output path are the load-bearing parts.

The set is nine production projects and their nine sibling test projects. The plan's Environment
Facts item 3 states that the analyzer packages are wired into 16 first-party project files, which is
a count of projects carrying `<Analyzer Include>` items and is a different population from the count
of projects that emit a build-output line. Both counts remain correct for their own populations.

P7-T3 derives its expected value from the `BASELINE_PROJECT_COUNT:` line above rather than from any
figure tabled in the plan. This delivery adds no project and removes none, so the Phase 7 count is
expected to be identical to 18 rather than merely close to it.
