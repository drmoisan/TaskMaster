# P0-T13 — Baseline Analyzer Gate (Rebuild recipe)

Timestamp: 2026-08-26T08-41

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:EnableNETAnalyzers=true" "/p:EnforceCodeStyleInBuild=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

Observed exit code: **0**. MSBuild summary: **5 Warning(s), 0 Error(s)**. `Build succeeded.` Time
elapsed 00:00:28.75. All twenty solution projects rebuilt and produced assemblies; both test
assemblies required by `P0-T15` (`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` and
`UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`) are present.

These counts — **0 errors, 5 warnings** — are the baseline consumed by the "Baseline-Comparison Rule
for Whole-Solution Gates" and by `P8-T3`. Because the baseline exit code is `0`, `P8-T3` has NO
authorized degradation: it must itself return `EXIT_CODE: 0`.

The command used `/t:Rebuild` (not `/t:Build`) and did not contain `/p:Nullable=enable`.

### Diagnostic breakdown

Zero errors of any code. Zero `CS86xx` diagnostics. Zero analyzer rule-ID diagnostics. No diagnostic
names any first-party source file, and therefore none names any file on this plan's written-file list.

All five warnings are the identical `System.Reactive` packages.config advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one per
project, on these five projects:

| Project | Warning |
|---|---|
| `QuickFiler/QuickFiler.csproj` | System.Reactive v7.0 packages.config advisory |
| `TaskMaster/TaskMaster.csproj` | System.Reactive v7.0 packages.config advisory |
| `ToDoModel/ToDoModel.csproj` | System.Reactive v7.0 packages.config advisory |
| `UtilitiesCS/UtilitiesCS.csproj` | System.Reactive v7.0 packages.config advisory |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | System.Reactive v7.0 packages.config advisory |

The advisory carries no warning code, is emitted by a third-party targets file shipped inside the
gitignored `packages/` directory, and is unrelated to this feature. It is pre-existing on the
integration branch and this plan neither causes nor repairs it.

### Correction of an earlier, invalid reading of this same gate

An earlier execution of this task in this worktree recorded `EXIT_CODE: 1` with 10 `error CS0006`
diagnostics and declared `ExpectedExitCode: 1`. **That reading was not a true repository baseline and
has been discarded. This artifact supersedes it in full, and the `ExpectedExitCode:` declaration has
been removed.**

Cause of the earlier red reading: a **worktree provisioning gap**, not a defect on the integration
branch. A Dependabot change bumped `Meziantou.Analyzer` to 3.0.174 and `Roslynator.Analyzers` to
4.16.1 in `packages.config` and in each project's `<Import>`/`<Error>` lines, while leaving sixteen
project files' `<Analyzer Include>` item paths naming the prior versions 3.0.156 and 4.16.0. Developer
checkouts and CI do not fail on that skew because their `packages/` directory accumulates prior
versions side by side; only a `packages/` directory restored from scratch, as in a freshly created
worktree, is missing them. `UtilitiesCS.csproj` and `VBFunctions.csproj` were the two projects whose
`<Analyzer Include>` paths were dereferenced before the skew was masked.

Correction applied: the two missing package directories `packages/Meziantou.Analyzer.3.0.156/` and
`packages/Roslynator.Analyzers.4.16.0/` were provisioned into this worktree, restoring parity with CI
and with developer checkouts.

**No tracked file was changed by that correction.** `packages/` is gitignored, and
`git status --porcelain -- packages` produces no output. AC-30 (ownership) and `P7-T3` are therefore
unaffected, and the sixteen skewed `<Analyzer Include>` entries were NOT edited — that repair is real
but lies outside this feature's File Ownership and is tracked separately by the epic orchestrator.

Consequence for the rest of the plan: because the true baseline is `EXIT_CODE: 0` with zero errors,
every intermediate analyzer Rebuild check in Phases 1 through 7 is a live gate that compiles the whole
solution, and `P8-T3` cannot be degraded.
