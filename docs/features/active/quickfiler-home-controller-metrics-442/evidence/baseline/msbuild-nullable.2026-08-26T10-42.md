# Phase 0 — Nullable / Type-Check Baseline

Timestamp: 2026-08-26T10-42
Task: [P0-T8]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

## Output Summary

Error count: **0**
Warning count: 5

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:20.70
EXIT_CODE=0
```

`/p:Nullable=enable` was deliberately not added. No project in this repository carries a
`<Nullable>` element and there is no `Directory.Build.props`, so the property is a solution-wide
opt-in that conscripts every file which has never adopted the `#nullable enable` pragma. CI omits
it for the same reason. Nullable enforcement here is per-file opt-in, and
`/p:TreatWarningsAsErrors=true` promotes the `CS86xx` diagnostics of files that have opted in.

The five warnings are the same pre-existing, code-less `System.Reactive.PackagesConfigCheck`
notice recorded in the analyzer baseline, one per `packages.config` project. Carrying no
diagnostic code, they are not promoted to errors by `/p:TreatWarningsAsErrors=true`.

### Gate non-vacuity

`/t:Rebuild` is required. Occurrences of `Skipping target "CoreCompile"` in the build log: **0**.
Every project genuinely recompiled, so the zero-error result is a real type-check result and not
an incremental no-op.
