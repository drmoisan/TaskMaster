# [P0-T6] Nullable / TreatWarningsAsErrors Build Baseline — Baseline Evidence

- **Issue:** #424
- **Task:** [P0-T6]
- **Toolchain step:** 3 of 4 (type check)

Timestamp: 2026-08-06T22-23

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

**Error count: 0** (expected 0). No `CS86xx` nullable-flow diagnostic and no warning promoted to an error anywhere in the solution.

## Non-vacuity verification

A `/t:Build` invocation can return exit code 0 without compiling anything when MSBuild's timestamp-based up-to-date check short-circuits `CoreCompile`, which would make this baseline meaningless. That was checked explicitly:

| Measure | Nullable run | Analyzer run ([P0-T5]) |
|---|---|---|
| `CoreCompile:` target executions | 18 | 18 |
| `skipping target "CoreCompile"` (up-to-date short-circuits) | 0 | 0 |

All 18 solution projects genuinely recompiled under both property sets — changing `/p:Nullable` and `/p:TreatWarningsAsErrors` alters the compile property set and correctly forces a full `CoreCompile`. **Neither baseline is vacuous.**

## Warning inventory

The same 5 pre-existing `System.Reactive` packages.config warnings recorded in `[P0-T5]` (projects `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `QuickFiler`, `TaskMaster`). These originate from a NuGet `.targets` file, not from C# source, so `TreatWarningsAsErrors=true` does not promote them to errors. Unrelated to issue #424 and not touched by this plan.

**Baseline conclusion:** the nullable/type-check gate passes cleanly at exit code 0 with 0 errors across a genuine full compile of all 18 projects. Any `CS86xx` or promoted-warning error appearing in `[P6-T3]` is attributable to changes made by this plan.
