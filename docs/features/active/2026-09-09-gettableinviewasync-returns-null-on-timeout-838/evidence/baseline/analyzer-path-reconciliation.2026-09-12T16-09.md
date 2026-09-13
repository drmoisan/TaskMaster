# P0-T13 — Analyzer include-path existence measurement (post-restore, pre-containment)

Timestamp: 2026-09-13T02-19

Command: a single pwsh payload that enumerates every tracked project file returned by `git -C . ls-files -- "*.csproj"`, reads each as XML, extracts the `Include` attribute of every `Analyzer` element, resolves each include against **that project file's own directory** (an include resolves against the declaring project, not the repository root), tests each resolved path for existence, and prints `PROJECT_COUNT=`, `ANALYZER_ITEM_TOTAL=`, `UNRESOLVED_COUNT=` and one `UNRESOLVED:` line per unresolved include carrying the declaring project, the include verbatim, and the resolved repository-relative path.

EXIT_CODE: 0

PROJECT_COUNT=18
ANALYZER_ITEM_TOTAL=162
UNRESOLVED_COUNT=15

## Unresolved includes, verbatim as they appear in the project file

Every one of the fifteen carries the identical include text:

```
..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
```

resolving in every case to the repository-relative path:

```
packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
```

The fifteen declaring project files are:

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/QuickFiler.csproj
Tags.Test/Tags.Test.csproj
Tags/Tags.csproj
TaskMaster.Test/TaskMaster.Test.csproj
TaskTree.Test/TaskTree.Test.csproj
TaskTree/TaskTree.csproj
TaskVisualization.Test/TaskVisualization.Test.csproj
TaskVisualization/TaskVisualization.csproj
ToDoModel.Test/ToDoModel.Test.csproj
ToDoModel/ToDoModel.csproj
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/UtilitiesCS.csproj
VBFunctions.Test/VBFunctions.Test.csproj
VBFunctions/VBFunctions.csproj
```

Output Summary: eighteen tracked project files were enumerated and 162 analyzer items examined. Fifteen resolved paths do not exist on disk after the P0-T10 restore, and all fifteen carry the identical include naming the package version directory `Meziantou.Analyzer.3.0.203`. This reproduces the planner's measurement exactly, including the fifteen declaring project files and the count. `TaskMaster/TaskMaster.csproj` is not among them, consistent with the plan's statement that it names 3.0.235 in all three places; the two SVGControl projects carry no analyzer item. Every remaining analyzer include resolves. This task is a measurement and gates nothing; the condition selects Branch B for P0-T14, because the count is above zero and every unresolved include contains the case-sensitive fixed literal `Meziantou.Analyzer.3.0.203`.
