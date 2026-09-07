# Phase 6 — Nullable / Type-Check Gate

Timestamp: 2026-08-26T11-27
Task: [P6-T4]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

This is the exact command from [P0-T8].

## Output Summary

Error count: **0**
Warning count: 5

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:22.80
EXIT_CODE=0
```

### Comparison with the [P0-T8] baseline

| Metric | Baseline ([P0-T8]) | Post-change ([P6-T4]) | Delta |
| --- | --- | --- | --- |
| Errors | 0 | **0** | 0 |
| Warnings | 5 | 5 | 0 |

The five warnings are the same pre-existing, code-less `System.Reactive.PackagesConfigCheck` notice
recorded in the analyzer gate. Carrying no diagnostic code, they are not promoted to errors by
`/p:TreatWarningsAsErrors=true`.

This is the gate AC-10 cites for the `int` to `double` widening of the two `elapsedSeconds`
parameters: the widening compiles clean under warnings-as-errors across every call site in the
solution, including the call sites in `QuickFiler.Test` and the pre-existing tests in the
forbidden-to-write `EfcHomeControllerTests.cs`.

### `/p:Nullable=enable` deliberately omitted

No project in this repository carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property is a solution-wide opt-in that would conscript every file
which has never adopted the `#nullable enable` pragma. `.github/workflows/ci.yml` omits it for that
reason and this gate matches CI character for character. Nullable enforcement here is per-file
opt-in, and `/p:TreatWarningsAsErrors=true` promotes the `CS86xx` diagnostics of files that have
opted in.

### Gate non-vacuity

Occurrences of `Skipping target "CoreCompile"` in the build log: **0**. Every project genuinely
recompiled, so the zero-error result is a real type-check result and not an incremental no-op.
