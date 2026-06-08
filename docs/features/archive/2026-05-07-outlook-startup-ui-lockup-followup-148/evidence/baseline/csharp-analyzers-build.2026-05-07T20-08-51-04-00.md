# Baseline C# Analyzer Build Evidence

Timestamp: 2026-05-07T20:08:51.5306361-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 1
Output Summary: The analyzer-enabled baseline build failed. Final log summary reported `Build FAILED.`, `4 Warning(s)`, and `26 Error(s)`. Representative failures included missing NuGet-package restore errors across multiple projects and missing namespace/type errors such as `log4net`, `Svg`, `SvgDocument`, and `Fizzler` in `SVGControl` sources. The script ended with `MSBuild failed with exit code 1`.
