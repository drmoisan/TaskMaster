Timestamp: 2026-08-28T18-22
Command: MSBuild.exe TaskMaster.sln -t:Rebuild -m -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded. 5 Warning(s), 0 Error(s). BASELINE_ANALYZER_WARNINGS = 5, all identical
System.Reactive.PackagesConfigCheck warnings (packages.config not supported by System.Reactive v7.0+),
one per project referencing System.Reactive: QuickFiler.csproj, TaskMaster.csproj, ToDoModel.csproj,
UtilitiesCS.Test.csproj, UtilitiesCS.csproj. No analyzer-rule diagnostics present.
