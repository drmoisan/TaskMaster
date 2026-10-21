# Phase 0 — Nullable / TreatWarningsAsErrors Build Baseline (P0-T12)

Timestamp: 2026-08-27T23-27
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 1

BaselineNullableWarningCount: 0

## ACCEPTANCE PARTIALLY MET — this task is recorded but NOT checked off

P0-T12's acceptance has two conjuncts:

1. `EXIT_CODE: 0` — **NOT met.** The observed exit code is `1`.
2. The recorded command line contains neither `/p:Nullable=enable` nor `/t:Build` — **met.** The
   command recorded above is character-for-character the command in `.github/workflows/ci.yml`
   step "Build with nullable warnings treated as errors". It uses `/t:Rebuild`, and no `Nullable`
   property is passed on the command line. Nullable enforcement in this repository is per-file opt-in
   via `#nullable enable`; adding `/p:Nullable=enable` would conscript every file that never adopted
   the pragma and would diverge from CI.

Because the first conjunct fails, the plan checkbox for `[P0-T12]` is left unchecked.

## What the build reported

- `Build FAILED.` with `0 Warning(s)` and `10 Error(s)`.
- Every error is `CS0006`. A code-frequency count over the console output returns `20 error CS0006`
  and no other code at all — 20 rather than 10 because MSBuild prints each error once in the
  per-project stream and once again in the end-of-build summary.
- Occurrences of any `CS86` diagnostic (the nullable-flow family this gate exists to enforce): **0**.

## Root cause — the same inherited analyzer version skew recorded under P0-T11

This is not a second, independent failure. It is the identical `CS0006` metadata-file failure from
`VBFunctions/VBFunctions.csproj` and `UtilitiesCS/UtilitiesCS.csproj`: the Analyzer Include HintPaths
name Meziantou.Analyzer 3.0.156 and Roslynator.Analyzers 4.16.0, while packages.config and `packages/`
carry 3.0.174 and 4.16.1. The full evidence, the proof that the skew is inherited rather than
introduced by this feature, and the reason no remedy was applied are recorded in the P0-T11 artifact
`phase0-analyzer-build.2026-08-27T23-26.md` and are not repeated here.

The skew breaks this gate as well as the analyzer gate because both invoke `csc` through
`CoreCompile`, and the missing analyzer assemblies are passed to `csc` regardless of which
`/p:` properties the command carries.

## BaselineNullableWarningCount interpretation

`BaselineNullableWarningCount:` is recorded as `0` because that is the integer the build reports
(`0 Warning(s)`). As with the analyzer count, it is **not** a usable baseline. The compile aborted at
`CoreCompile` on the two root projects and no downstream project — including `QuickFiler` and
`QuickFiler.Test` — was compiled, so no `#nullable enable` file in this feature's surface was
type-checked. The zero is the absence of a measurement, not a clean result. It must be re-baselined
once the analyzer reference skew is repaired.

Output Summary: The nullable build **FAILED** with `EXIT_CODE: 1`, `0 Warning(s)` and `10 Error(s)`,
all `CS0006`, and **zero** `CS86xx` nullable diagnostics — the same inherited analyzer version skew
recorded under P0-T11, not a nullable defect. The recorded command line correctly contains neither
`/p:Nullable=enable` nor `/t:Build`, satisfying the second half of the acceptance condition, but
`EXIT_CODE: 0` is not met, so the task is recorded and left unchecked. The recorded warning count of
`0` is the absence of a measurement rather than a clean result, because no project in this feature's
surface was compiled.
