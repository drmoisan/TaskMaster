# P0-T7 — Analyzer-Build Baseline

Timestamp: 2026-09-01T08-07

## MSBuild Resolution

Resolved with:

```text
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
```

Resolved absolute path:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
(Visual Studio 18 Community; MSBuild `18.9.1.35102`, the same installation `nuget restore`
auto-detected in P0-T4.) This is the resolved path used by P0-T8, P1-T5, P2-T3, P3-T3, and P3-T4.

## Command

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Invoked through the resolved absolute path above. The argument vector actually passed, recorded by
the runner:

```text
TaskMaster.sln | /t:Rebuild | /m | /p:Configuration=Debug | /p:Platform=Any CPU | /p:EnableNETAnalyzers=true | /p:EnforceCodeStyleInBuild=true
```

`/t:Rebuild` was used, not `/t:Build`.

EXIT_CODE: 0

## Output Summary

MSBuild's trailing summary:

```text
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.40
```

**BASELINE ANALYZER WARNING COUNT: 5** (integer). **Baseline error count: 0.**

Phase 3's P3-T3 compares its recorded warning count against this integer and requires it to be less
than or equal to 5.

### Gate was not vacuous

`/t:Rebuild` forces Clean before Build, so `CoreCompile` cannot be skipped by MSBuild's incremental
up-to-date check. Verified against the captured log: the string `CoreCompile:` appears 66 times and
`csc.exe` appears 36 times, so the compiler and its analyzers genuinely ran. The 13.4-second elapsed
time is the result of `/m` parallel project execution, not a skipped compile.

### The 5 warnings, enumerated

All 5 are the same diagnostic, emitted once per affected project. It carries no diagnostic ID; the
text is:

```text
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)
```

It is a build-targets warning originating in a restored NuGet package, is unrelated to this change,
and is pre-existing at the merge base. Two of the five instances are attributed to
`TaskMaster/TaskMaster.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`; the remaining three
are the same warning attributed to the other `packages.config` projects in the solution.

### No baseline diagnostic names either changed file

The string `TimeOutTask` occurs 4 times in the captured log. All 4 occurrences are on `csc.exe`
command lines and `BuildResponseFile` echo lines — that is, the compiler's source-file argument list
— and none is a diagnostic. **Zero analyzer or compiler diagnostics name
`UtilitiesCS\Threading\TimeOutTask.cs` or
`UtilitiesCS.Test\Threading\TimeOutTask_OverloadCoverageTests.cs` at baseline.** P3-T3's
corresponding assertion is therefore a real delta against a clean starting point.

Acceptance: met. `EXIT_CODE: 0`, and the `Output Summary:` records the exact baseline warning count
as the integer 5.
