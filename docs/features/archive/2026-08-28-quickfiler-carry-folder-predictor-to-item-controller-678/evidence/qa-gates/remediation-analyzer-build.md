# P2-T3 — Analyzer build, remediation cycle 1

Timestamp: 2026-09-02T01-33

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

## Output Summary

MSBuild summary lines, verbatim:

```
    5 Warning(s)
    0 Error(s)
```

`CoreCompile:` occurrences in the build log: **57**. Build log length: 12037 lines.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `EXIT_CODE: 0` with a zero error count in the MSBuild summary | PASS — exit 0, `0 Error(s)` |
| 2 | warning count at or below the `R_BASELINE_ANALYZER_SUMMARY` count, any new warning named individually | PASS — 5, equal to the baseline 5; no new warning |
| 3 | `CoreCompile:` occurrences recorded and greater than zero | PASS — **57** |

Clause 2 detail. `R_BASELINE_ANALYZER_SUMMARY` from P0-T6 is `5 warnings, 0 errors`. The
post-change count is also 5, so it is at the baseline rather than above it, and the list of
warnings is unchanged: all five are the same pre-existing System.Reactive
`packages.config` migration notice, emitted once each by `QuickFiler/QuickFiler.csproj`,
`TaskMaster/TaskMaster.csproj`, `ToDoModel/ToDoModel.csproj`, `UtilitiesCS/UtilitiesCS.csproj`
and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. A search of the build log for
`System.Reactive.PackagesConfigCheck` returns 10 lines, which is those 5 warnings each
appearing twice — once inline during the build and once in MSBuild's end-of-run warning
rollup. **No warning is new, so the "named individually" sub-clause has an empty list.**
No analyzer rule diagnostic and no C# compiler diagnostic was reported.

Clause 3 detail. 57 is greater than zero, so compilation actually ran and the analyzers ran
with it. `/t:Rebuild` is what guarantees this: MSBuild's up-to-date check does not invalidate
on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 having skipped
`CoreCompile` on every project, and the gate could not fail.
