# P1-T7 — Compile gate for the new tests, before any production change

Timestamp: 2026-09-13T23-19

Command:

```
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\logs\p1-t7-analyzers.log;Verbosity=detailed"
```

EXIT_CODE: 0

Output Summary:

- Error count, read from `coverage\logs\p1-t7-analyzers.log` by the pattern
  `(^|[^0-9])[0-9]+ Error\(s\)`: `    0 Error(s)` — **0**
- `(Select-String -Path coverage\logs\p1-t7-analyzers.log -Pattern 'Task \x22Csc\x22').Count`: **36**

The error count is zero and the compile-task count is greater than zero, so neither FAIL condition
is met. Recorded for context only: the warning count line reads `    0 Warning(s)`.

This gate exists so that the fail-before run in P1-T8 means something. The three new tests must
compile clean into `UtilitiesCS.Test.dll` before a Failed outcome can be read as the predicate
rejecting them rather than as a build or discovery problem.

No production source has changed at this point. The only files modified since the P0 baseline are
the three test-side files this phase writes:
`UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` (new),
`UtilitiesCS.Test/Threading/UiThread_Tests.cs` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
