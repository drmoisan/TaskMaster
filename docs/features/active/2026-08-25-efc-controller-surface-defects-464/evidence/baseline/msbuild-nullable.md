# Phase 0 — baseline nullable / type-check build

Timestamp: 2026-08-27T23-22
Task: [P0-T11]
Command: `& "<resolved MSBuild.exe>" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:n` under `pwsh -NoProfile` from the worktree root, with output redirected to a log file
EXIT_CODE: 0

`/p:Nullable=enable` was **not** added. This command is character-for-character CI's nullable step apart
from the logging switches. `/t:Rebuild` was used, never `/t:Build`.

## Result

```
    5 Warning(s)
    0 Error(s)
```

BASELINE_NULLABLE_ERRORS: 0

## BASELINE_NULLABLE_IDS

```
BASELINE_NULLABLE_IDS: (none)
```

Cardinality: **0**.

No identifier-bearing diagnostic was emitted. The five warnings are the same identifier-less
`System.Reactive` `packages.config` advisories recorded in `[P0-T10]`; they carry no code and, being
emitted by an MSBuild target rather than by the compiler, are not promoted to errors by
`/p:TreatWarningsAsErrors=true`. Zero `CS86xx` nullable diagnostics were produced, which is the expected
state for a repository whose nullable enforcement is per-file opt-in through `#nullable enable`.

## Non-vacuity proof

Count of `Skipping target "CoreCompile"` lines in the build log: **0**, with 36 `csc.exe` invocations. The
compiler and its nullable-flow analysis genuinely ran.

## How `[P10-T5]` consumes this

`[P10-T5]` compares as a **set**: any error identifier in the final nullable build that is absent from
`BASELINE_NULLABLE_IDS` is a new error, which the cross-cutting criterion beginning "The nullable/type-check
build" forbids. Because the baseline set is empty and the baseline error count is 0, the final build must
exit 0 with 0 errors.

Output Summary: Baseline nullable build exits 0 with 0 errors and the same 5 identifier-less
System.Reactive advisories. BASELINE_NULLABLE_ERRORS is 0 and BASELINE_NULLABLE_IDS is the empty set.
Non-vacuity confirmed by zero `Skipping target "CoreCompile"` lines.
