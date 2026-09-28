# P2-T4 — Compile gate after the hardening

Timestamp: 2026-09-13T23-26

Command:

```
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\logs\p2-t4-analyzers.log;Verbosity=detailed"
```

EXIT_CODE: 0

Output Summary:

- Error count, read from `coverage\logs\p2-t4-analyzers.log` by the pattern
  `(^|[^0-9])[0-9]+ Error\(s\)`: `    0 Error(s)` — **0**
- `(Select-String -Path coverage\logs\p2-t4-analyzers.log -Pattern 'Task \x22Csc\x22').Count`: **36**

The error count is zero and the compile-task count is greater than zero. Recorded for context only:
the warning count line reads `    0 Warning(s)`.

This build is the first that includes the hardened condition in
`UtilitiesCS/Threading/UiThread.cs` together with the two tightened assertions in
`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`. It gates the pass-after run in P2-T5.

## Interim formatting verification recorded here

Before this build, `dotnet tool run csharpier check .` was run twice as a verification step, so that
the Phase 4 formatting gate would not be entered with known drift.

- First run, exit code 1, one reported path:
  `Error .\UtilitiesCS.Test\Threading\UiThreadApartmentMeasurement_Tests.cs - Was not formatted.`
  with the detail `The file contained different line endings than formatting it would result in.`
  The new file had been written with LF line endings while every other C# file in the repository,
  including the two this delivery edits, uses CRLF. No structural formatting difference was
  reported in any file, so the hardened condition and the new test bodies already matched the
  formatter's output.
- The new file was converted to CRLF, with no BOM, matching the two sibling files in the same
  directory (`UiThread_Tests.cs` and `UiThreadInitContract_Tests.cs`), both of which are CRLF and
  BOM-free.
- Second run, exit code 0, `Checked 1634 files in 5262ms.` with zero lines matching
  `Was not formatted`.

The authoritative formatting gate remains P4-T2 and P4-T3; this interim verification is recorded so
that the equal before-and-after hashes P4-T2 will record are attributable to a tree that was
already formatted rather than to a formatter that failed to run.
