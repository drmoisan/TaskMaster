# Baseline — MSBuild Analyzer Gate (P0-T12)

Timestamp: 2026-08-27T19-59

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

`$msbuild` is the path recorded by P0-T4:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.79
```

- Error count: **0**
- Warning count: **5** (the MSBuild summary figure). All five are the same pre-existing
  `System.Reactive.PackagesConfigCheck.targets` advisory that `packages.config` is unsupported by
  System.Reactive v7.0 or later, emitted once per project that references it. None is an analyzer
  diagnostic and none is attributable to this feature.

## Non-vacuity verification (mandatory)

Count of lines matching `Skipping target "CoreCompile"` in the build output: **0**.

A count other than zero would mean the gate compiled nothing and this artifact would have to record
FAIL. The count is zero, so the gate is non-vacuous. Corroborating positive evidence from the same log:

- 50 `Rebuild target` entries and 36 `CoreCompile` target headers appear in the 4795-line log.
- Every `CoreCompile` block invokes `csc.exe` with the full analyzer set on the command line, including
  `/analyzer:..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`
  and the four `..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\` DLLs, which
  confirms the P0-T8 back-fill resolved the version skew: no `error CS0006` was emitted.
- `QuickFiler/bin/Debug/QuickFiler.dll` and `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` were both
  produced by this run.

The command contains `/t:Rebuild` and does not contain `/t:Build`.

Acceptance: `EXIT_CODE: 0` and an error count of 0. PASS.
