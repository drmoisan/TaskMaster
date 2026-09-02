# Phase 2 — Compile before the red run ([P2-T2])

Timestamp: 2026-09-01T22-46

The solution was compiled so that the Phase 2 red run is a runtime observation rather than a build break.
The seven new tests reference `QfcFormKeyHandler.ClaimsAltChord`, which exists in its behaviour-preserving
intermediate form after Phase 1, so the assembly compiles and three of the seven fail by assertion.

Command:

```
pwsh -NoProfile -Command '$mb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"'
```

The console stream was redirected to `coverage\663-red-build.console.txt`, which is under the gitignored
`coverage` directory.

EXIT_CODE: 0

## Acceptance reading — the test assembly was recompiled

`LastWriteTimeUtc` of `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, captured immediately **before** the
command ran:

```
2026-09-01T22:32:46.6246189Z
```

`LastWriteTimeUtc` of the same file, captured immediately **after** the command returned:

```
2026-09-01T22:35:32.0013036Z
```

The post-build value is strictly later than the pre-build value, which proves the test assembly was
recompiled rather than skipped by MSBuild incrementality. Both values are UTC file times reported by
`(Get-Item ...).LastWriteTimeUtc.ToString("o")`.

## Build diagnostics

Console lines matching `: error [A-Z]+[0-9]+:`: **0**.

MSBuild summary lines, verbatim:

```
    5 Warning(s)
    0 Error(s)
```

The five warnings are the same codeless `System.Reactive.PackagesConfigCheck.targets` MSBuild-target
notices recorded in the `[P0-T9]` artifact.

`/t:Build` rather than `/t:Rebuild` is correct for this task: no `/p:` switch differs from the previous
build, so incrementality is not being asked to invalidate on a property change, and the changed source
files are what triggers the recompile. The `LastWriteTimeUtc` comparison above is the observation that
confirms the recompile actually happened.

Output Summary: The solution built with exit code 0 and zero coded error lines. The
`QuickFiler.Test.dll` write time advanced from `2026-09-01T22:32:46.6246189Z` to
`2026-09-01T22:35:32.0013036Z`, so the test assembly containing the seven new methods was genuinely
recompiled and the Phase 2 red run will be a runtime observation.
