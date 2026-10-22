# Post-change MSBuild analyzer rebuild (P6-T3)

Timestamp: 2026-09-02T23-35

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Tool resolution used the Block K prelude.

EXIT_CODE: 0

## MSBuild summary lines

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:17.33
```

## Diagnostic-count comparison against the P0-T9 baseline

| Measure | Baseline (P0-T9) | Post-change (P6-T3) | Verdict |
|---|---|---|---|
| Errors | 0 | 0 | no higher |
| Warnings | 5 | 5 | no higher |
| Warnings carrying a diagnostic identifier | 0 | 0 | no higher |

The diagnostic count is no higher than the count recorded by P0-T9. It is equal on all three
measures.

The 5 warnings are the same non-diagnostic-ID message the baseline recorded, emitted once per
packages.config project by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:
the project contains a `packages.config` file, which `System.Reactive` v7.0 and later does not
support. A pattern search of the captured log for a warning carrying a diagnostic identifier of
the form `warning <LETTERS><DIGITS>` returns 0 matches, unchanged from baseline.

## AC4 evidence — no CS0123 regression at the method-group conversion

A fixed-string search of the captured log returns:

- `CS0123` — 0 occurrences. The method-group conversion
  `_delay = delay ?? NonBlockingDelay.WaitAsync;` at
  `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` line 102 still binds to
  `Func<TimeSpan, Task>`, which is what D1's overload-pair-not-optional-parameter decision
  protects.
- `CS8632` — 0 occurrences.

## Non-vacuity check

`/t:Rebuild` was used rather than `/t:Build`. The captured log contains 75 `CoreCompile:` task
executions, so compilation and therefore analyzer execution actually occurred rather than being
skipped by MSBuild's incremental up-to-date check. The baseline recorded 64; the reason for the
difference was not investigated, because the non-vacuity property this check establishes needs
only that the count is greater than zero.

Output Summary: The analyzer gate passes with exit code 0, 0 errors, and 5 warnings, none carrying
a diagnostic identifier. The diagnostic count is no higher than the P0-T9 baseline on every
measure. Zero `CS0123` and zero `CS8632`.
