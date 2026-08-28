# [P10-T5] Final nullable / type-check build

Timestamp: 2026-08-28T01-58
Task: [P10-T5]
Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:n` under `pwsh -NoProfile` from the worktree root, output redirected to a log file under the system temp directory
EXIT_CODE: 0

Run start (UTC): `2026-08-28T01-57-30`
Run end (UTC): `2026-08-28T01-57-42`
`Time Elapsed 00:00:11.75`

`/p:Nullable=enable` was **not** added and `/t:Build` was **not** substituted, per the task text and
repository policy. Apart from the two logging switches this is character-for-character CI's nullable
step.

## Result

```
    5 Warning(s)
    0 Error(s)
```

| Metric | `[P0-T11]` baseline | This run | Verdict |
|---|---|---|---|
| Exit code | 0 | **0** | equal |
| Errors | `BASELINE_NULLABLE_ERRORS` = 0 | **0** | does not exceed baseline |

The acceptance condition's first branch applies: `EXIT_CODE: 0`. The authorised-exception branch is not
needed.

## Delivered diagnostic-identifier set

A search of the build log for `(warning|error) <letters><digits>` returns an empty match set.

```
DELIVERED_NULLABLE_IDS: (none)
```

Cardinality: **0**.

| Set | Cardinality |
|---|---|
| `BASELINE_NULLABLE_IDS` from `[P0-T11]` | 0 |
| `DELIVERED_NULLABLE_IDS` from this run | **0** |

The delivered set is a subset of the baseline set. Identifiers present in the delivered set and absent
from the baseline set, enumerated verbatim: **none**. In particular there is **no `CS86xx` nullable
diagnostic**, which is the expected state for a repository whose nullable enforcement is per-file opt-in
through `#nullable enable`.

The five warnings are the same identifier-less `System.Reactive` `packages.config` advisories recorded in
`[P10-T4]` and at baseline. They are emitted by an MSBuild target rather than by the compiler, so
`/p:TreatWarningsAsErrors=true` does not promote them to errors.

## Non-vacuity proof

| Measure | Value |
|---|---|
| Lines matching the literal `Skipping target "CoreCompile"` | **0** |
| `csc.exe` invocations in the log | **36** |

The compiler and its nullable-flow analysis genuinely ran across all 36 projects.

## Loop position

Stage 3 (type checking) of the first Phase 10 pass. No source file was written, so no loop restart is
triggered. Execution proceeds to `[P10-T6]`.

Output Summary: PASS. The nullable rebuild exits 0 with 0 errors, equal to `BASELINE_NULLABLE_ERRORS`.
`DELIVERED_NULLABLE_IDS` is the empty set and is a subset of the empty `BASELINE_NULLABLE_IDS`; no new
error identifier appears, and no `CS86xx` diagnostic was produced. Non-vacuity is proved by zero
`Skipping target "CoreCompile"` lines against 36 `csc.exe` invocations.
