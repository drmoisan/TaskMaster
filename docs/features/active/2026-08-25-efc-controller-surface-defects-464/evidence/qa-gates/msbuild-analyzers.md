# [P10-T4] Final analyzer build

Timestamp: 2026-08-28T01-57
Task: [P10-T4]
Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:n` under `pwsh -NoProfile` from the worktree root, output redirected to a log file under the system temp directory
EXIT_CODE: 0

Run start (UTC): `2026-08-28T01-56-28`
Run end (UTC): `2026-08-28T01-56-48`
`Time Elapsed 00:00:12.74`

The MSBuild path is the one `[P0-T4]` resolved via `vswhere`. `/t:Rebuild` was used, never `/t:Build`.

## Result

```
    5 Warning(s)
    0 Error(s)
```

| Metric | `[P0-T10]` baseline | This run | Verdict |
|---|---|---|---|
| Exit code | 0 | **0** | equal |
| Errors | `BASELINE_ANALYZER_ERRORS` = 0 | **0** | does not exceed baseline |
| Warnings | `BASELINE_ANALYZER_WARNINGS` = 5 | **5** | does not exceed baseline |

The acceptance condition's first branch applies: `EXIT_CODE: 0`. The authorised-exception branch is not
needed.

## Delivered diagnostic-identifier set

A search of the build log for the pattern `(warning|error) <letters><digits>` — the shape every Roslyn,
analyzer and MSBuild diagnostic identifier takes — returns **0 matching lines**.

```
DELIVERED_ANALYZER_IDS: (none)
```

Cardinality: **0**.

| Set | Cardinality |
|---|---|
| `BASELINE_ANALYZER_IDS` from `[P0-T10]` | 0 |
| `DELIVERED_ANALYZER_IDS` from this run | **0** |

The delivered set is a **subset** of the baseline set: the empty set is a subset of the empty set.
Identifiers in the delivered set and not in the baseline set, enumerated verbatim: **none**.

This is the comparison the criterion beginning "The analyzer build introduces" actually requires. A count
comparison alone would be necessary but not sufficient, because a new diagnostic that displaced a
baseline diagnostic would keep the counts equal; the set comparison closes that gap and also returns
empty.

All five warnings are the identifier-less `System.Reactive` advisory emitted by the
`System.Reactive.PackagesConfigCheck` MSBuild target, one per project, from the same five projects as at
baseline: `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`,
`UtilitiesCS.Test.csproj`. Each reads:

> The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
> Please migrate to PackageReference.

None carries a diagnostic code, which is why the identifier set is empty rather than containing five
entries. This is a pre-existing repository condition unrelated to this feature.

## Non-vacuity proof

| Measure | Value |
|---|---|
| Lines matching the literal `Skipping target "CoreCompile"` | **0** |
| `csc.exe` invocations in the log | **36** |

Zero `CoreCompile` skips with 36 compiler invocations proves the build genuinely compiled every project
and the analyzers genuinely ran. A warm `/t:Build` would have exited 0 with `CoreCompile` skipped
everywhere and this gate would have been vacuous; `/t:Rebuild` prevents that.

## Loop position

Stage 2 (linting) of the first Phase 10 pass. The command wrote no source file — `git status --porcelain`
remained limited to this feature's documentation folder — so no loop restart is triggered. Execution
proceeds to `[P10-T5]`.

Output Summary: PASS. The analyzer rebuild exits 0 with 0 errors and 5 warnings, both at their `[P0-T10]`
baseline figures. `DELIVERED_ANALYZER_IDS` is the empty set and is therefore a subset of the empty
`BASELINE_ANALYZER_IDS`; no identifier appears in the delivered set that is absent from the baseline set.
Non-vacuity is proved by zero `Skipping target "CoreCompile"` lines against 36 `csc.exe` invocations.
