# Baseline — NuGet restore bootstrap ([P0-T6])

Timestamp: 2026-08-10T22-42
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Invoke-Restore.ps1`
EXIT_CODE: 0

## Console output (head)

```
Using MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
MSBuild version 18.8.2+ce25c0108 for .NET Framework
Build started 8/10/2026 10:35:24 PM.

     1>Project "...\TaskMaster.sln" on node 1 (Restore target(s)).
     1>ValidateSolutionConfiguration:
         Building solution configuration "Debug|Any CPU".
       _GetAllRestoreProjectPathItems:
         Determining projects to restore...
       Restore:
```

## Console output (tail)

```
         Installed:
             171 package(s) to packages.config projects
     1>Done Building Project "...\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.07
```

## Project-count verification

The `Restore` target ran against `TaskMaster.sln`, which carries **18** `*.csproj` members
(`grep -o 'csproj"' TaskMaster.sln | wc -l` -> `18`; the 19th `Project(` line is a solution folder).
Each of the 18 is a legacy `packages.config` project:

```
./QuickFiler.Test/packages.config        ./TaskTree/packages.config
./QuickFiler/packages.config             ./TaskVisualization.Test/packages.config
./SVGControl.Test/packages.config        ./TaskVisualization/packages.config
./SVGControl/packages.config             ./ToDoModel.Test/packages.config
./Tags.Test/packages.config              ./ToDoModel/packages.config
./Tags/packages.config                   ./UtilitiesCS.Test/packages.config
./TaskMaster.Test/packages.config        ./UtilitiesCS/packages.config
./TaskMaster/packages.config             ./VBFunctions.Test/packages.config
./TaskTree.Test/packages.config          ./VBFunctions/packages.config
```

MSBuild's `Restore` target does not emit a per-project line for `packages.config` projects; it
emits the aggregate `Installed: 171 package(s) to packages.config projects`. The per-project
enumeration above is the count verification.

## Output Summary

NuGet restore completed with `EXIT_CODE: 0`, `0 Warning(s)`, `0 Error(s)` in 4.07 s. **All 18
projects in `TaskMaster.sln` restored**, receiving 171 packages in total into the repo-local
`packages/` folder. The tree is now bootstrapped for the cold MSBuild measurement in [P0-T9].
