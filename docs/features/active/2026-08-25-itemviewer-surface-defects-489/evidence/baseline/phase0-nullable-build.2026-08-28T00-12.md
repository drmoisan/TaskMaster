# Phase 0 — Nullable / TreatWarningsAsErrors Build Baseline (P0-T12) — re-run, supersedes 2026-08-27T23-27

Timestamp: 2026-08-28T00-12
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
ExpectedExitCode: 0

BaselineNullableWarningCount: 5
BaselineNullableErrorCount: 0

## Supersession

This artifact supersedes `evidence/baseline/phase0-nullable-build.2026-08-27T23-27.md`, which
recorded `EXIT_CODE: 1` with ten `CS0006` errors. That failure was the same inherited analyzer
version skew recorded under P0-T11, now cleared for this worktree without changing any tracked
file. The superseded artifact is retained as the audit record of the blocked first attempt.

## Both acceptance conjuncts are met

1. `EXIT_CODE: 0` — **met.** `Build succeeded.` with `5 Warning(s)` and `0 Error(s)`.
2. The recorded command line contains neither `/p:Nullable=enable` nor `/t:Build` — **met.** The
   command above is character-for-character the command in `.github/workflows/ci.yml` step
   "Build with nullable warnings treated as errors". It uses `/t:Rebuild`, and no `Nullable`
   property is passed. Nullable enforcement in this repository is per-file opt-in via
   `#nullable enable`; adding `/p:Nullable=enable` would conscript every file that never adopted
   the pragma and would diverge from CI.

## Warning composition

`BaselineNullableWarningCount: 5` is the build's total warning count. Its composition:

- Occurrences of any `CS86` diagnostic — the nullable-flow family this gate exists to enforce:
  **0**.
- Occurrences of any `CS` diagnostic at all: **0**.
- All five warnings are the identical pre-existing `System.Reactive` `packages.config` advisory
  raised by `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets`,
  one each for `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `QuickFiler` and `TaskMaster`.
  It is emitted by an MSBuild targets file and carries no compiler warning code, which is why
  `/p:TreatWarningsAsErrors=true` — a `csc` property — does not promote it and the build still
  exits `0`.

The nullable warning count attributable to compiled C# is therefore **0**, and the count of 5
recorded above is the whole-build figure that Phase 11 compares against under the same command.

## Non-vacuity

`/t:Rebuild` is used, not `/t:Build`. The companion P0-T11 run of the same target over the same
tree recorded **0** occurrences of the literal `Skipping target "CoreCompile"` in its `/v:normal`
file log, so `CoreCompile` genuinely ran on every project under the Rebuild target.

Output Summary: The nullable gate **passes**. `msbuild TaskMaster.sln /t:Rebuild /m
/p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` exited `0` with
`Build succeeded.`, `5 Warning(s)`, `0 Error(s)`. `BaselineNullableWarningCount: 5` and
`BaselineNullableErrorCount: 0`. Zero `CS86xx` diagnostics and zero `CS` diagnostics of any kind;
all five warnings are the pre-existing `System.Reactive` `packages.config` advisory, which carries
no compiler warning code and is therefore not promoted by `TreatWarningsAsErrors`. The command
contains neither `/p:Nullable=enable` nor `/t:Build`. This run supersedes the 2026-08-27T23-27
artifact, which failed `CS0006` on the inherited analyzer version skew now cleared.
