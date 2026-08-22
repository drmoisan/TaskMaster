# Baseline Toolchain Step 1 — Worktree Bootstrap and `dotnet tool restore` (Issue #449, [P0-T8])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

[P0-T8] specifies three commands, (a) SDK install, (b) `nuget restore`, (c) `dotnet tool restore`,
each conditioned on the absence of a directory. (a) and (b) were performed as part of the
agent-worktree bootstrap BEFORE this executor was launched; their preconditions are therefore already
satisfied and their guard conditions are false. Each is recorded below with the command that VERIFIED
its completed state. (c) is per-worktree and was run by this executor.

---

## (a) Repo-local SDK — guard condition FALSE, `.dotnet-sdk` already present

Conditional command per [P0-T8]: `./scripts/vscode/Install-RepoDotNetSdk.ps1`
Guard: run only "if `.dotnet-sdk` is absent". `.dotnet-sdk` is PRESENT, so the installer was NOT
re-run. Re-running it was explicitly prohibited by the execution conditions for this session.

Verification Command: `ls -d .dotnet-sdk`
EXIT_CODE: 0
Output: `.dotnet-sdk/`

Verification Command:
`pwsh -NoProfile -Command 'Set-Location "<WORKTREE>"; dotnet --version; "DOTNET_VERSION_EXIT=$LASTEXITCODE"'`
EXIT_CODE: 0
Output:
```
8.0.205
DOTNET_VERSION_EXIT=0
```

`global.json` pins SDK `8.0.205` with `paths: [".dotnet-sdk", "$host$"]`. The resolved version is
exactly `8.0.205`, which proves the repo-local SDK is the one being used and that no `global.json`
`errorMessage` is being raised. Had `.dotnet-sdk` been missing, this command would have failed with
that message instead of printing a version.

## (b) NuGet packages — guard condition FALSE, `packages/` already present

Conditional command per [P0-T8]: `nuget restore TaskMaster.sln`
Guard: run only "if `packages/` is absent". `packages/` is PRESENT and mirrored from the main
checkout, so `nuget restore` was NOT re-run.

Verification Command: `ls -d packages`
EXIT_CODE: 0
Output: `packages/`

Verification Command: `ls -1 packages/ | grep -c ''`
EXIT_CODE: 0
Output: `265` package directories present.

### Analyzer version skew — verified benign, NOT repaired here

`QuickFiler/packages.config` pins `Meziantou.Analyzer` at `3.0.174` and `Roslynator.Analyzers` at
`4.16.1`, while `QuickFiler/QuickFiler.csproj` `<Analyzer Include>` items point at `3.0.156` and
`4.16.0`. That divergence between `packages.config` and the `<Analyzer Include>` HintPaths is a
pre-existing repo-wide condition and is sibling child #511's scope. **No `.csproj` and no
`packages.config` is edited by this child to "fix" it.**

Command: `ls -d packages/Meziantou.Analyzer.* packages/Roslynator.Analyzers.*`
EXIT_CODE: 0
Output:
```
packages/Meziantou.Analyzer.3.0.101/
packages/Meziantou.Analyzer.3.0.123/
packages/Meziantou.Analyzer.3.0.156/
packages/Meziantou.Analyzer.3.0.174/
packages/Roslynator.Analyzers.4.16.0/
packages/Roslynator.Analyzers.4.16.1/
```

Command: `grep -n -E "Meziantou|Roslynator" QuickFiler/QuickFiler.csproj`
EXIT_CODE: 0
Output:
```
3:   <Import Project="..\packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props" Condition="Exists(...)" />
576: <Error Condition="!Exists('..\packages\Meziantou.Analyzer.3.0.174\build\Meziantou.Analyzer.props')" ... />
582: <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
583: <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
584: <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
585: <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
586: <Analyzer Include="..\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
```

**Both** version families are present on disk: `3.0.174` satisfies the `<Import>` at line 3 and the
hard `<Error Condition="!Exists(...)">` at line 576, and `3.0.156`/`4.16.0` satisfy the
`<Analyzer Include>` HintPaths at lines 582-586. No `CS0006` (metadata file not found) is expected
from the analyzer wiring, and the baseline analyzer build in [P0-T10] is the empirical confirmation.

## (c) `dotnet tool restore` — RUN BY THIS EXECUTOR

Command:
`pwsh -NoProfile -Command 'Set-Location "<WORKTREE>"; dotnet tool restore; "TOOL_RESTORE_EXIT=$LASTEXITCODE"'`
EXIT_CODE: 0
Output:
```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
TOOL_RESTORE_EXIT=0
```

The manifest-pinned CSharpier version `1.2.6` was restored, matching the version
`.github/workflows/ci.yml` uses after its own `dotnet tool restore`. Every later CSharpier invocation
in this plan goes through `dotnet tool run csharpier`, never a global install.

## Gitignore confirmation — bootstrap trees do not dirty the tree

`.gitignore` carries `.dotnet*/` and `**/[Pp]ackages/*`, so neither `.dotnet-sdk/` nor `packages/`
appears in `git status`. `git status --porcelain` was empty at handoff and remains empty after
`dotnet tool restore`; see `git-state.2026-08-22T09-16.md`.

---

## Output Summary

All three [P0-T8] commands accounted for. (a) `.dotnet-sdk` present; `dotnet --version` prints
`8.0.205`, EXIT_CODE 0, matching the `global.json` pin — installer NOT re-run because its guard
condition is false. (b) `packages/` present with 265 package directories — `nuget restore` NOT re-run
because its guard condition is false; the pre-existing `packages.config`-versus-`<Analyzer Include>`
version skew was verified benign because both version families are on disk, and no build file was
edited. (c) `dotnet tool restore` was run by this executor and returned **EXIT_CODE: 0**, restoring
CSharpier 1.2.6. The required `dotnet tool restore` exit code of `0` is satisfied.
