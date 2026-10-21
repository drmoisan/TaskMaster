# Finding 2 — Build After the Gate Test Fixture (P2-T7)

Timestamp: 2026-09-03T02-10
Task: [P2-T7]
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Trailing counts

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.56
```

Five warnings, unchanged from the P0-T6 and P0-T7 baselines. Zero errors. The short elapsed time
reflects MSBuild incrementality: only `TaskMaster.Test.csproj` had a changed input, so only that
project recompiled.

## The test-file compile-item registration took effect

Search: `Ribbon\SpamManagerResetGateTests.cs` (literal, case-sensitive) over the normal-verbosity
MSBuild file log.
Hits: **2**.

The hits are in the `CoreCompile` step for `TaskMaster.Test.csproj`, where the file appears in the
source list on the recorded `csc.exe` command line. A logged `csc.exe` command line is itself proof
that `CoreCompile` ran rather than being skipped as up to date.

The added line in `TaskMaster.Test/TaskMaster.Test.csproj` is:

```
    <Compile Include="Ribbon\SpamManagerResetGateTests.cs" />
```

placed in the existing ribbon compile-item group. That group is not alphabetically ordered — the
XML-consistency fixture entry already sits after the try-functionality entry — so the new entry is
appended to the group rather than sorted into it, and no ordering is asserted.

Independent confirmation: P2-T8 discovered and executed all nine tests from the built assembly,
which is only possible if the file was compiled into it.

Output Summary: Build succeeded with EXIT_CODE 0, 5 warnings and 0 errors. The new gate test file
appears twice in the build log as an input on the `csc.exe` command line for
`TaskMaster.Test.csproj`.
