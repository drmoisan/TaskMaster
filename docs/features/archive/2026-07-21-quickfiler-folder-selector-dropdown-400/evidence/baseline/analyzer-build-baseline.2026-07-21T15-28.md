Timestamp: 2026-07-21T15-28Z

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 1

WarningCount: 4

ErrorCount: 31

Baseline Diagnostics:

- `MSB3245` in `SVGControl/SVGControl.csproj`: unresolved `ExCSS` reference.
- `MSB3245` in `SVGControl/SVGControl.csproj`: unresolved `Fizzler` reference.
- `MSB3245` in `SVGControl/SVGControl.csproj`: unresolved `log4net` reference.
- `MSB3245` in `SVGControl/SVGControl.csproj`: unresolved `Svg` reference.
- Missing NuGet import error in `Tags/Tags.csproj`: `Meziantou.Analyzer.3.0.123` props.
- Missing NuGet import error in `ToDoModel/ToDoModel.csproj`: `System.ValueTuple.4.6.2` targets.
- Missing NuGet import error in `ToDoModel.Test/ToDoModel.Test.csproj`: `System.ValueTuple.4.6.2` targets.
- Missing NuGet import error in `TaskVisualization/TaskVisualization.csproj`: `Meziantou.Analyzer.3.0.123` props.
- Missing NuGet import error in `UtilitiesCS/UtilitiesCS.csproj`: `NETStandard.Library.2.0.3` targets.
- Missing NuGet import error in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: `System.ValueTuple.4.6.2` targets.
- Missing NuGet import error in `QuickFiler/QuickFiler.csproj`: `NETStandard.Library.2.0.3` targets.
- Missing NuGet import error in `QuickFiler.Test/QuickFiler.Test.csproj`: `System.ValueTuple.4.6.2` targets.
- Missing NuGet import error in `TaskVisualization.Test/TaskVisualization.Test.csproj`: `System.ValueTuple.4.6.2` targets.
- Missing NuGet import error in `TaskTree/TaskTree.csproj`: `Meziantou.Analyzer.3.0.123` props.
- Missing NuGet import error in `TaskMaster/TaskMaster.csproj`: `NETStandard.Library.2.0.3` targets.
- Missing NuGet import error in `TaskTree.Test/TaskTree.Test.csproj`: `Microsoft.Testing.Platform.2.3.2` props.
- Missing NuGet import error in `VBFunctions/VBFunctions.csproj`: `Meziantou.Analyzer.3.0.123` props.
- Missing NuGet import error in `VBFunctions.Test/VBFunctions.Test.csproj`: `System.ValueTuple.4.6.2` targets.
- Missing NuGet import error in `TaskMaster.Test/TaskMaster.Test.csproj`: `System.ValueTuple.4.6.2` targets.
- `CS0246` in `SVGControl/PictureBoxSVG.cs:15`: `Fizzler` namespace unavailable.
- `CS0246` in `SVGControl/PictureBoxSVG.cs:16`: `Svg` namespace unavailable.
- `CS0246` in `SVGControl/SvgImageSelector.cs:13`: `Svg` namespace unavailable.
- `CS0246` in `SVGControl/SvgImageSelector.cs:27`: `log4net` namespace unavailable.
- `CS0246` in `SVGControl/SVGParser.cs:10`: `Svg` namespace unavailable.
- `CS0246` in `SVGControl/SVGParser.cs:67`: `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SVGParser.cs:73`: `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SVGParser.cs:85` (return type): `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SVGParser.cs:85` (parameter): `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:15`: `Svg` namespace unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:21`: `log4net` namespace unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:149`: `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:158`: `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:179`: `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:223`: `SvgDocument` unavailable.
- `CS0246` in `SVGControl/SvgRenderer.cs:330`: `SvgDocument` unavailable.

Output Summary: The analyzer baseline failed before compilation because this new worktree had no restored legacy `packages` directory. The resulting unresolved-package warnings and 31 dependent errors are recorded as pre-existing environment debt; no issue #400 source or test diagnostic was produced.

## Effective post-restore baseline

Timestamp: 2026-07-21T16-02Z

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

WarningCount: 5

ErrorCount: 0

Baseline Diagnostics:

- `System.Reactive.PackagesConfigCheck.targets` warning x5 in `UtilitiesCS/UtilitiesCS.csproj`, `ToDoModel/ToDoModel.csproj`, `QuickFiler/QuickFiler.csproj`, `TaskMaster/TaskMaster.csproj`, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: System.Reactive 7 does not support `packages.config`.

Output Summary: After the repository restore and installation of the exact legacy analyzer package versions referenced by the project files, analyzer compilation completed. The five System.Reactive package-management warnings are the effective permitted analyzer baseline; no analyzer error was present.
