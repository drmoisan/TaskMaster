# Baseline — Analyzer Build (P0-T4)

Timestamp: 2026-09-05T19-23

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

BASELINE_PROJECT_COUNT: 18

Output Summary:

The summary warning and error lines, verbatim:

```text
    0 Warning(s)
    0 Error(s)
```

Both hard conditions hold: `    0 Warning(s)` and `    0 Error(s)` are recorded exactly as the
task requires.

**Project build-output line count: observed 18, expected 16.** The recorded value differs from the
tabled expectation, so the task's record-and-continue escape is invoked and
`BASELINE_PROJECT_COUNT: 18` is recorded above. P7-T3 derives its expected value from that recorded
observation rather than from the tabled 16.

The count was taken over lines of the arrow form `<ProjectName> -> <output assembly path>` in the
build log. Both the total count and the distinct count are 18, so the figure is not an artifact of
de-duplication. The eighteen lines are:

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
of projects that emit a build-output line. That is the likely origin of the tabled 16, but this
artifact records only the measurement, not an inference about its cause.
