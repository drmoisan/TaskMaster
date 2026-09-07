# P4-T5 — Solution analyzer rebuild after the #487 deletions

Timestamp: 2026-08-28T00-45
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `5 Warning(s)` and `0 Error(s)` across 18 projects in 11.42
seconds. This is exactly the Phase 0 analyzer baseline: exit 0, `5 Warning(s)`, `0 Error(s)`,
18 projects.

This is a **solution**-level build and therefore keeps the spaced platform spelling
`"/p:Platform=Any CPU"` verbatim. The single-token substitution recorded at P1-T5 and P3-T2 applies
only to project-level invocations; MSBuild normalises the spaced form to `AnyCPU` when it is driving
a `.sln`, so no correction was needed or made here.

`/v:normal` is appended to the plan's printed command so the log carries per-target lines and the
non-vacuity count below can be taken. It changes verbosity only, not build semantics: no `/p:`
property was added or removed, `/t:Rebuild` is unchanged, and `/p:Nullable=enable` was not
introduced.

## The gate is non-vacuous

SkippingCoreCompileCount: 0
CoreCompileInvocations: 55
CscInvocations: 36
ProjectsBuilt: 18

The literal `Skipping target "CoreCompile"` occurs **0** times in the `/v:normal` log, which is what
proves the analyzers actually ran. `/t:Rebuild` is load-bearing here: MSBuild's incremental
up-to-date check compares timestamps without invalidating on a command-line `/p:` change, so a warm
`/t:Build` would return exit 0 with `CoreCompile` skipped on every project and would run no
analyzers at all. The 55 `CoreCompile` invocations and 36 `csc.exe` invocations are recorded as
corroboration, not as the gate.

## The five warnings are the pre-existing advisory, and there is no Roslyn diagnostic

All five warnings are the identical `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS` and `UtilitiesCS.Test`. A count of lines
matching `: (warning|error) CS[0-9]+` over the full log returns **0**, so there is no `CS`
diagnostic, and no analyzer diagnostic of any kind was emitted by Meziantou, Roslynator, AsyncFixer,
BannedApiAnalyzers or SonarAnalyzer.

WarningCount: 5
BaselineAnalyzerWarningCount: 5
ErrorCount: 0

The warning count is equal to, not greater than, the Phase 0 baseline.

## This exit code is only reachable if both `+=` statements are gone

The acceptance condition notes that `EXIT_CODE: 0` is possible only if both designer wirings were
removed. P4-T1 and P4-T3 deleted the two `L0v2h2_WebView2_ParentChanged` member declarations; had
either `+=` statement in the two `.Designer.cs` files survived, the corresponding `InitializeComponent`
would reference a method that no longer exists and `QuickFiler.csproj` would fail to compile with
`CS0103`/`CS1061`. The build succeeding at zero errors is therefore itself the proof that P4-T2 and
P4-T4 removed both wirings.

Output Summary: `TaskMaster.sln` rebuilds at `EXIT_CODE: 0` with `Build succeeded.`, `5 Warning(s)`
and `0 Error(s)` across 18 projects, equal to the Phase 0 analyzer baseline and not greater than it.
All five warnings are the pre-existing `System.Reactive` `packages.config` advisory and there is no
`CS` or analyzer diagnostic of any kind. The gate is non-vacuous: the literal `Skipping target
"CoreCompile"` occurs 0 times in the `/v:normal` log, with 55 `CoreCompile` and 36 `csc.exe`
invocations. Because the four #487 deletions removed both the handler declarations and both designer
`+=` wirings, a zero-error build is itself proof that no dangling event subscription remains.
