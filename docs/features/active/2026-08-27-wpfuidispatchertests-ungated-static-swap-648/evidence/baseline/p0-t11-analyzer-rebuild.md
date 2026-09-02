# P0-T11 — Analyzer-Gate Baseline

Timestamp: 2026-09-01T13-38

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```
MSBuild was re-resolved through `vswhere.exe` as in P0-T9 and the command was issued through `pwsh`.
It was run from the checkout root.

EXIT_CODE: 0

Output Summary:

Summary error count and warning count, taken verbatim from the MSBuild summary block
(log lines 5203, 5230 and 5231):

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

The error count is zero and the observed exit code is zero, so no `BASELINE_GATE_RED:` line is
recorded and execution continues to P0-T12.

Supporting observations confirming this was a genuine compile rather than an incrementality no-op:

- The log contains 63 `CoreCompile:` target entries and explicit `csc.exe` command lines carrying the
  `/analyzer:` operands for the six wired analyzer packages, so analyzer diagnostics were produced.
- `Build started 9/1/2026 1:24:24 PM.` with `Time Elapsed 00:00:12.90`.

All five warnings are the same diagnostic, emitted once per project that carries a `packages.config`
alongside the System.Reactive 7.0.0 package. The text, with the checkout-root prefix elided:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference.
```

The five owning projects are `UtilitiesCS\UtilitiesCS.csproj`, `ToDoModel\ToDoModel.csproj`,
`QuickFiler\QuickFiler.csproj`, `TaskMaster\TaskMaster.csproj`, and
`UtilitiesCS.Test\UtilitiesCS.Test.csproj`. None is a C# compiler or analyzer diagnostic, and none
names `Controllers\WpfUiDispatcherTests.cs`. The baseline warning count that P2-T3 compares against
is therefore 5.
