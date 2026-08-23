# [P0-T10] C# build baseline (test-assembly production)

Timestamp: 2026-08-11T00-26
Command (as written in the plan): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
Actual invoked form (this environment; `msbuild` is not on PATH and the Bash tool is git-bash, which
mangles MSBuild-style `/switch` arguments into filesystem paths):
`pwsh -NoProfile -Command '& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"'`
The single-quoted-outer / double-quoted-inner quoting discipline mandated by the plan's Conventions
section is preserved verbatim.
EXIT_CODE: 0

Runs after `[P0-T9]` restored packages, per the plan's stated ordering.

`/p:Nullable=enable` is deliberately absent per issue #522 and the plan's scope prohibitions. This
step exists only to produce `*.Test.dll` assemblies for coverage collection; it is not a type-check
gate.

## Result

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.42
```

- Errors: **0**
- Warnings: **5**

All five warnings are the same non-blocking advisory emitted once per `packages.config` project:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
Please migrate to PackageReference.
```

No C# source file was modified by this task.

## `*.Test.dll` outputs under `bin\Debug\`

Enumerated with `find . -name "*.Test.dll" -path "*/bin/Debug/*" -not -path "*/obj/*" -not -path "*/ref/*"`,
matching `Invoke-MSTestWithCoverageMain`'s own discovery filter:

```
./QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
./SVGControl.Test/bin/Debug/SVGControl.Test.dll
./Tags.Test/bin/Debug/Tags.Test.dll
./TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
./TaskTree.Test/bin/Debug/TaskTree.Test.dll
./TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
./ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
./UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
./VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
```

Count: **9** test assemblies.

## Output Summary

`MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exited 0 with 0
errors and 5 non-blocking `System.Reactive` packages.config advisories, in 19.42 seconds. Nine
`*.Test.dll` outputs exist under `bin\Debug\`. `[P0-T11]`'s precondition is satisfied.
