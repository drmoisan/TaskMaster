# P0-T10 — Analyzer baseline (toolchain stage 2)

Timestamp: 2026-09-01T19-44
Command:

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

The resolved MSBuild executable is recorded in the placeholder form the plan's section 0 prescribes:

    <vs-install>\MSBuild\Current\Bin\MSBuild.exe
    MSBuild version 18.9.1+a81b43525 for .NET Framework

EXIT_CODE: 0

## Output Summary

MSBuild summary lines, reproduced verbatim:

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:13.56

**Warning count: 5. Error count: 0.**

All five warnings are the same diagnostic, emitted once per project that carries a `packages.config` reference to System.Reactive 7.0.0:

    The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.
    Please migrate to PackageReference. (You can suppress this message by setting the
    RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)

Emitting projects: `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`. The warning originates in `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)` and is a pre-existing repository condition unrelated to this change.

A search of the 5084-line build log for a coded compiler or analyzer diagnostic — matching `: error [A-Z]+[0-9]+:` — returns **zero**. The same search for `: warning [A-Z]+[0-9]+:` also returns zero, confirming that all five reported warnings are the uncoded MSBuild-level System.Reactive message and that no CS, CA, IDE, S, MA, RCS, AsyncFixer or RS diagnostic is present in the baseline.

The bare word "error" was deliberately not used as the search term. A successful MSBuild run prints it many times, in the `/errorreport:prompt` compiler switch on every `csc` command line and in the `0 Error(s)` summary itself, so a bare-word count would report a large non-zero figure on a clean build and could not distinguish a passing run from a failing one.

## The gate is non-vacuous

`/t:Rebuild` was used rather than `/t:Build`, as CLAUDE.md and `.claude/rules/csharp.md` require. This matters because MSBuild's incremental up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no analyzers at all — the gate would be incapable of failing.

That the rebuild genuinely compiled is verified rather than assumed: the log contains **67** `CoreCompile:` target executions. Analyzer diagnostics are produced during compilation, so 67 executed compilations establish that the analyzer set actually ran and that a diagnostic, had one existed, would have been reported.

`/p:Nullable=enable` was not added. That property is a solution-wide opt-in with no `<Nullable>` element in any project and no `Directory.Build.props`, so adding it would conscript every file that has never adopted the per-file pragma; CI omits it deliberately.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
