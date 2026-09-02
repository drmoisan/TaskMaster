# P0-T11 — Nullable / type-check baseline (toolchain stage 3)

Timestamp: 2026-09-01T19-45
Command:

    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

The resolved MSBuild executable is recorded in the placeholder form the plan's section 0 prescribes:

    <vs-install>\MSBuild\Current\Bin\MSBuild.exe

EXIT_CODE: 0

## Output Summary

MSBuild summary lines, reproduced verbatim:

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:11.81

**Warning count: 5. Error count: 0.**

The five warnings are the same pre-existing System.Reactive `packages.config` diagnostic recorded in `p0-t10-msbuild-analyzers.md`. It is emitted by an MSBuild target rather than by the compiler and carries no diagnostic code, which is why `/p:TreatWarningsAsErrors=true` does not promote it: that property promotes compiler diagnostics, and a target-level `warning :` message with no code is outside its reach. This is the reason a five-warning count coexists with a zero-error count under warnings-as-errors.

A search of the build log for a coded diagnostic — matching `: error [A-Z]+[0-9]+:` — returns **zero**, and the same search for `: warning [A-Z]+[0-9]+:` also returns zero. No `CS86xx` nullable-flow diagnostic is present in the baseline.

The bare word "error" was deliberately not used as the search term, for the reason recorded in `p0-t10-msbuild-analyzers.md`: a successful MSBuild run prints it on every `csc` command line and in the summary, so a bare-word count cannot distinguish a passing run from a failing one.

## Command fidelity

This is character-for-character the command in `.github/workflows/ci.yml` for the step that builds with nullable warnings treated as errors. Two properties of it are load-bearing and were not altered:

- **`/p:Nullable=enable` was not added.** No project in this repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide opt-in that would conscript every file which has never adopted the `#nullable enable` pragma. Nullable enforcement here is per-file opt-in; omitting the property loses no enforcement over any file that has opted in.
- **`/t:Build` was not substituted.** MSBuild's up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project, and the gate could not fail. That the rebuild genuinely compiled is verified rather than assumed: the log contains **61** `CoreCompile:` target executions.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
