# Phase 4 — Nullable Type-Check Gate (P4-T6)

Timestamp: 2026-09-03T03-12
Task: [P4-T6]
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

MSBuild version 18.9.1+a81b43525 for .NET Framework, resolved through vswhere.

`/p:Nullable=enable` is deliberately NOT passed. This repository opts into nullable analysis per file
with `#nullable enable`; the solution-wide property conscripts every file that has never adopted the
pragma and produces hundreds of errors that CI does not see. `/t:Rebuild` is required because
MSBuild's up-to-date check does not invalidate on a command-line property change; the 14.96-second
elapsed time confirms a full rebuild ran.

## Trailing counts

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.96
```

## Comparison against the P0-T7 baseline

| Count | Baseline (P0-T7) | This gate | No worse |
|---|---|---|---|
| Warnings | 5 | **5** | Yes — equal |
| Errors | 0 | **0** | Yes — equal |

The five warnings are the System.Reactive `packages.config` advisory, one per consuming project, as
at baseline. No `CS86xx` nullable-flow diagnostic is present.

This matters for two constructs this change introduced. The lazily built gate property in the
Intelligence partial uses the null-forgiving operator on two accessors (`() => Globals?.AF!` and
`() => Globals?.Engines!`), matching the established precedent on the sibling engine-commands
partial; and the extracted cache's `TryGetActive` uses an `out bool` that is assigned on every path.
Neither raises a diagnostic under warnings-as-errors.

Output Summary: Nullable type-check gate passed with EXIT_CODE 0, 5 warnings and 0 errors —
identical to the P0-T7 baseline, so no worse on either count. No CS86xx nullable diagnostic was
introduced.
