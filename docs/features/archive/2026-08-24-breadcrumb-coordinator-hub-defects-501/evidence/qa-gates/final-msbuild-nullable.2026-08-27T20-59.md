# Final QA — Step 3, MSBuild Nullable / Type-Check Gate (P7-T4, AC-31)

Timestamp: 2026-08-27T20-59

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary:

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:21.71
```

| Metric | Value |
| --- | ---: |
| Errors | **0** |
| Warnings | 5 |
| `CS86xx` nullable diagnostics anywhere in the log | **0** |

The 5 warnings are the same pre-existing `System.Reactive` `packages.config` advisory recorded at
baseline (P0-T13 reported the identical figure). They carry no diagnostic ID, which is why
`/p:TreatWarningsAsErrors=true` does not promote them to errors.

Zero `CS86xx` diagnostics is the substantive result: the three production files this feature edited all
carry `#nullable enable`, so they participate in nullable analysis, and
`/p:TreatWarningsAsErrors=true` would have promoted any nullable-flow warning in them to a build error.
The new production partial part `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` opens
with `#nullable enable`, so the relocated members remained under nullable analysis across the SR-1 split
rather than silently opting out of it.

## Command-shape verification (mandatory)

- The `Command:` line above **contains `/t:Rebuild`**. Confirmed.
- The `Command:` line above **does not contain `/p:Nullable=enable`**. Confirmed.

Both properties are deliberate and must not be "restored". `/p:Nullable=enable` is a solution-wide
opt-in that would conscript every file which has never adopted the pragma; no project in this repository
carries a `<Nullable>` element and there is no `Directory.Build.props`, and CI omits the property. Using
`/t:Build` instead of `/t:Rebuild` would let MSBuild's up-to-date check skip `CoreCompile` and return
exit 0 without type-checking anything.

## Non-vacuity verification (mandatory)

Count of lines matching `Skipping target "CoreCompile"`: **0**.

Corroborating positive evidence: **54** `CoreCompile:` target headers in this run's log, so compilation
and nullable-flow analysis genuinely executed. A count other than zero would mean the gate compiled
nothing and this artifact would have to record FAIL.

Acceptance: `EXIT_CODE: 0`, an error count of 0, `Command:` contains `/t:Rebuild` and does not contain
`/p:Nullable=enable`. PASS (AC-31).
