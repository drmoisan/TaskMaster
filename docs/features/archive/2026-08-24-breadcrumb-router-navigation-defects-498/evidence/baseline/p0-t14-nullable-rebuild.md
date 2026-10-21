# P0-T14 — Baseline Nullable and Type-Check Gate

Timestamp: 2026-08-26T08-44

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:TreatWarningsAsErrors=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

Observed exit code: **0**. MSBuild summary: **5 Warning(s), 0 Error(s)**. `Build succeeded.` Time
elapsed 00:00:22.03.

Total error count: **0**. This is the baseline consumed by the "Baseline-Comparison Rule for
Whole-Solution Gates" and by `P8-T4`. Because the baseline exit code is `0`, `P8-T4` has NO authorized
degradation: it must itself return `EXIT_CODE: 0`.

The command contains `/t:Rebuild` and does **not** contain `/p:Nullable=enable` or `/t:Build`, as
required. It is the Rebuild recipe with the two analyzer properties (`/p:EnableNETAnalyzers=true`,
`/p:EnforceCodeStyleInBuild=true`) replaced by `"/p:TreatWarningsAsErrors=true"`.

### Diagnostic breakdown

Zero errors of any code. Zero `CS86xx` nullable-flow diagnostics were promoted to errors, so every file
in the repository that carries a `#nullable enable` pragma is currently clean under
`/p:TreatWarningsAsErrors=true`.

All five warnings are the same `System.Reactive` packages.config advisory recorded by `P0-T13`, one
each on `QuickFiler/QuickFiler.csproj`, `TaskMaster/TaskMaster.csproj`, `ToDoModel/ToDoModel.csproj`,
`UtilitiesCS/UtilitiesCS.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. The advisory carries
no warning code and is emitted by a third-party targets file inside the gitignored `packages/`
directory. It is not promoted to an error because `TreatWarningsAsErrors` promotes coded compiler
warnings, not uncoded MSBuild task messages.

### Correction of an earlier, invalid reading of this same gate

An earlier execution of this task in this worktree recorded `EXIT_CODE: 1` with 10 `error CS0006`
diagnostics and declared `ExpectedExitCode: 1`. **That reading was not a true repository baseline and
has been discarded. This artifact supersedes it in full, and the `ExpectedExitCode:` declaration has
been removed.**

Cause of the earlier red reading: a **worktree provisioning gap**, identical to the one documented in
`p0-t13-analyzer-rebuild.md`. Sixteen project files carry `<Analyzer Include>` item paths naming
`Meziantou.Analyzer` 3.0.156 and `Roslynator.Analyzers` 4.16.0 while `packages.config` was bumped to
3.0.174 and 4.16.1. Developer checkouts and CI mask that skew because their `packages/` directory
retains the older versions alongside the newer ones; a freshly restored `packages/` does not, so
`UtilitiesCS.csproj` and `VBFunctions.csproj` failed with `CS0006` before compiling anything.

Correction applied: the two missing package directories `packages/Meziantou.Analyzer.3.0.156/` and
`packages/Roslynator.Analyzers.4.16.0/` were provisioned into this worktree.

**No tracked file was changed by that correction.** `packages/` is gitignored, and
`git status --porcelain -- packages` produces no output, so AC-30 and `P7-T3` are unaffected. The
sixteen skewed `<Analyzer Include>` entries were NOT edited; that repair lies outside this feature's
File Ownership and is tracked separately by the epic orchestrator.

Consequence for the rest of the plan: the true nullable baseline is green, so every intermediate
nullable Rebuild check in Phases 2 through 6 is a live gate over a fully compiled solution, and
`P8-T4` cannot be degraded.
