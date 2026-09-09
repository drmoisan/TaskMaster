# Baseline — NuGet Restore

Timestamp: 2026-09-09T16-33

Command: pwsh -File scripts/vscode/Invoke-Restore.ps1

EXIT_CODE: 0

PackagesDirectoryPresent: true

Output Summary: The script invoked the vswhere-resolved MSBuild against TaskMaster.sln with
/t:Restore /p:RestorePackagesConfig=true. MSBuild reported "Build succeeded", 0 Warning(s),
0 Error(s), and NuGet reported "Installed: 172 package(s) to packages.config projects". Elapsed time
was 00:00:03.65. After the run,
packages/Microsoft.Bcl.TimeProvider.10.0.11/lib/net462/Microsoft.Bcl.TimeProvider.dll exists on
disk; it did not exist before this task. That path is the HintPath named at UtilitiesCS/UtilitiesCS.csproj
line 97 and the version is pinned at UtilitiesCS/packages.config line 28. Its presence removes the
unresolved-reference confounder that P1-T2 must exclude before any CS1061 reading can be treated as
a refutation.
