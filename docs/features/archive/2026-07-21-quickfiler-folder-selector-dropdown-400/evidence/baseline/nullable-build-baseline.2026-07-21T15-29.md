Timestamp: 2026-07-21T15-29Z

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`

EXIT_CODE: 1

WarningCount: 4

ErrorCount: 31

Baseline Diagnostic Identities and Files:

- `MSB3245` x4 — `SVGControl/SVGControl.csproj` — unresolved ExCSS, Fizzler, log4net, and Svg references.
- Missing NuGet import error x15 — `Tags/Tags.csproj`, `ToDoModel/ToDoModel.csproj`, `ToDoModel.Test/ToDoModel.Test.csproj`, `TaskVisualization/TaskVisualization.csproj`, `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`, `TaskVisualization.Test/TaskVisualization.Test.csproj`, `TaskTree/TaskTree.csproj`, `TaskMaster/TaskMaster.csproj`, `TaskTree.Test/TaskTree.Test.csproj`, `VBFunctions/VBFunctions.csproj`, `VBFunctions.Test/VBFunctions.Test.csproj`, and `TaskMaster.Test/TaskMaster.Test.csproj`.
- `CS0246` x2 — `SVGControl/PictureBoxSVG.cs:15-16` — unavailable Fizzler/Svg namespaces.
- `CS0246` x2 — `SVGControl/SvgImageSelector.cs:13,27` — unavailable Svg/log4net namespaces.
- `CS0246` x5 — `SVGControl/SVGParser.cs:10,67,73,85,85` — unavailable Svg/SvgDocument types.
- `CS0246` x7 — `SVGControl/SvgRenderer.cs:15,21,149,158,179,223,330` — unavailable Svg/log4net/SvgDocument types.

Output Summary: The nullable warnings-as-errors baseline produced the same missing-package diagnostic set as P0-T7 before nullable analysis could execute: 4 warnings and 31 errors. No issue #400 source or test diagnostic was produced.

## Effective post-restore baseline

Timestamp: 2026-07-21T16-02Z

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

WarningCount: 5

ErrorCount: 0

Baseline Diagnostics:

- `System.Reactive.PackagesConfigCheck.targets` warning x5 in `UtilitiesCS/UtilitiesCS.csproj`, `ToDoModel/ToDoModel.csproj`, `QuickFiler/QuickFiler.csproj`, `TaskMaster/TaskMaster.csproj`, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: System.Reactive 7 does not support `packages.config`.

Output Summary: After the repository restore and installation of the exact legacy analyzer package versions referenced by the project files, nullable warnings-as-errors compilation completed. The five package-management warnings are outside nullable analysis and form the effective permitted nullable baseline; no nullable/compiler error was present.
