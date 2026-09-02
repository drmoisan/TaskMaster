# MSBuild Analyzer Gate — Baseline (P0-T8)

Timestamp: 2026-09-01T15-43

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

The `$msbuild` prelude resolved `vswhere` at
`${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe` and
resolved MSBuild to the absolute path recorded in the `Command:` field above.
The `if (-not $msbuild) { throw ... }` guard did not fire.

Output Summary:

MSBuild summary block, transcribed with the worktree root replaced by
`<repo-root>` per the artifact-hygiene rule:

```
     1>Done Building Project "<repo-root>\TaskMaster.sln" (Rebuild target(s)).

Build succeeded.

       "<repo-root>\TaskMaster.sln" (Rebuild target) (1) ->
       "<repo-root>\UtilitiesCS\UtilitiesCS.csproj" (Rebuild target) (19) ->
       (_RxCheckPackagesConfig target) ->
         <repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [<repo-root>\UtilitiesCS\UtilitiesCS.csproj]

       "<repo-root>\TaskMaster.sln" (Rebuild target) (1) ->
       "<repo-root>\ToDoModel\ToDoModel.csproj" (Rebuild target) (6) ->
         <repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (...) [<repo-root>\ToDoModel\ToDoModel.csproj]

       "<repo-root>\TaskMaster.sln" (Rebuild target) (1) ->
       "<repo-root>\QuickFiler\QuickFiler.csproj" (Rebuild target) (12) ->
         <repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (...) [<repo-root>\QuickFiler\QuickFiler.csproj]

       "<repo-root>\TaskMaster.sln" (Rebuild target) (1) ->
       "<repo-root>\TaskMaster\TaskMaster.csproj" (Rebuild target) (8) ->
         <repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (...) [<repo-root>\TaskMaster\TaskMaster.csproj]

       "<repo-root>\TaskMaster.sln" (Rebuild target) (1) ->
       "<repo-root>\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Rebuild target) (18) ->
         <repo-root>\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (...) [<repo-root>\UtilitiesCS.Test\UtilitiesCS.Test.csproj]

    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.68
```

Build-result line: `Build succeeded.`
Warning count: 5. Error count: 0.

All five warnings are the same diagnostic emitted once per project by
`System.Reactive.PackagesConfigCheck.targets`, reporting that the project uses
`packages.config` rather than `PackageReference`. It is a pre-existing
repository condition, is not an analyzer diagnostic, and is unrelated to this
change. It is recorded here so the Phase 2 analyzer gate can be compared
against the same figure.

`CoreCompile` appears 57 times in the full transcript, which confirms that
`/t:Rebuild` actually recompiled rather than being short-circuited by MSBuild's
incremental up-to-date check. `/t:Build` was not used, per CLAUDE.md.
`/p:Nullable=enable` was not added.
