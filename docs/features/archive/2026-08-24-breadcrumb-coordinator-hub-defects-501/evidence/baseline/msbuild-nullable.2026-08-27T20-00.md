# Baseline — MSBuild Nullable / Type-Check Gate (P0-T13)

Timestamp: 2026-08-27T20-00

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

`$msbuild` is the path recorded by P0-T4.

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.24
```

- Error count: **0**
- Warning count: 5, all the same pre-existing `System.Reactive.PackagesConfigCheck.targets`
  `packages.config` advisory recorded in the P0-T12 artifact. It is emitted as a bare `warning` with no
  diagnostic ID, which is why `/p:TreatWarningsAsErrors=true` does not promote it.

## Command-shape verification (mandatory)

- The `Command:` line above contains `/t:Rebuild`. Confirmed.
- The `Command:` line above does NOT contain `/p:Nullable=enable`. Confirmed. Adding it is prohibited:
  no project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`,
  so the property would conscript every file that has never adopted the `#nullable enable` pragma. CI
  omits it deliberately.

## Non-vacuity verification (mandatory)

Count of lines matching `Skipping target "CoreCompile"` in the build output: **0**.

Corroborating positive evidence: 62 `CoreCompile:` target headers appear in this run's log, so
compilation and nullable-flow analysis genuinely executed. A count other than zero would mean the gate
compiled nothing and this artifact would have to record FAIL.

Acceptance: `EXIT_CODE: 0`, error count 0, `Command:` contains `/t:Rebuild` and does not contain
`/p:Nullable=enable`. PASS.
