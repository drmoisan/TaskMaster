# P4-T3 — Toolchain step 2 of 4: linting (.NET analyzers)

Timestamp: 2026-09-01T20-14
Command:

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

Resolved MSBuild executable: `<vs-install>\MSBuild\Current\Bin\MSBuild.exe`

EXIT_CODE: 0

## Output Summary

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:11.56

**Warning count: 5. Error count: 0.**

All five warnings are the pre-existing System.Reactive `packages.config` diagnostic, emitted once per project that references that package (`QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`). The count is **identical to the P0-T10 baseline**, so this change introduced no new warning.

A search of the build log for a coded diagnostic — matching `: error [A-Z]+[0-9]+:` — returns **zero**, and the same search for `: warning [A-Z]+[0-9]+:` also returns zero. No CS, CA, IDE, S (Sonar), MA (Meziantou), RCS (Roslynator), AsyncFixer or RS (BannedApi) diagnostic is present.

The bare word "error" was deliberately not used as the search term. A successful MSBuild run prints it many times — in the `/errorreport:prompt` switch on every `csc` command line and in the `0 Error(s)` summary itself — so a bare-word count returns a large non-zero figure on a clean build and cannot distinguish a passing run from a failing one.

## The gate is non-vacuous

The log contains **75** `CoreCompile:` target executions. Analyzer diagnostics are produced during compilation, so this establishes that the five-analyzer stack actually ran across the solution and that a diagnostic, had one existed, would have been reported.

This is why `/t:Rebuild` is mandatory rather than stylistic. MSBuild's incremental up-to-date check compares timestamps and does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers at all — the gate would be structurally incapable of failing. `.github/workflows/ci.yml` can use `/t:Build` for its analyzer step because a runner checkout is always cold; a local working tree is not.

`/p:Nullable=enable` was not added, per CLAUDE.md and `.claude/rules/csharp.md`.

## Position in the Phase 4 pass

This is stage 2 of the single uninterrupted toolchain pass P4-T1 through P4-T5. Stages 1 and 2 (`format`, `check`) preceded it and rewrote no file, so no restart was triggered.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
