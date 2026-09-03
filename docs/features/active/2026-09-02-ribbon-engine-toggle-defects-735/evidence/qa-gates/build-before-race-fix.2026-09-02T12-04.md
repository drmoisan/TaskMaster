# Finding 3 — Build Before the Race Fix (P3-T4)

Timestamp: 2026-09-03T02-32
Task: [P3-T4]
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Trailing counts

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.46
```

Five warnings, unchanged from the P0-T6 and P0-T7 baselines. Zero errors.

## The race test file is registered and compiled

Search: `Ribbon\EngineToggleStateCoordinatorTests.Race.cs` (literal, case-sensitive) over the
normal-verbosity MSBuild file log.
Hits: **2**, both on the `csc.exe` command line recorded for `TaskMaster.Test.csproj`.

The added line in `TaskMaster.Test/TaskMaster.Test.csproj` is:

```
    <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.Race.cs" />
```

## Why a green build here is load-bearing

The six new tests reference only members that already exist on the coordinator and on the existing
private harness, so they compile against the pre-fix tree once the `partial` keyword has been added
by P3-T1. That keyword had to land first: two files declaring the same class without it is a compile
error that would redden the whole test assembly and destroy the fail-before evidence, because the
run would report compile failures rather than genuine assertion failures.

A green build is what makes P3-T5's three failures genuine assertion failures on real defective
behavior rather than build breakage.

Output Summary: Build succeeded with EXIT_CODE 0, 5 warnings and 0 errors, with the new race test
file present on the `csc.exe` command line for the test project. The tree is green immediately
before the fail-before run.
