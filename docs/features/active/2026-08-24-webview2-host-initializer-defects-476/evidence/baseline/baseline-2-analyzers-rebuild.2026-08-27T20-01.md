# Baseline 2 of 4 — Analyzer Gate (`/t:Rebuild`) ([P0-T9])

Timestamp: 2026-08-27T20-01

Command:
```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```
(run through `pwsh -NoProfile` from the workspace root; MSBuild resolved through `vswhere`)

Resolved MSBuild path:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

Argument list as passed:
`TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

## Output Summary

- **Error count: 0** (`0 Error(s)` in the MSBuild summary).
- **Warning count: 5** (`5 Warning(s)` in the MSBuild summary).
- Distinct `: error XXnnnn` lines in the log: 0.
- **Non-vacuity check: `Skipping target "CoreCompile"` lines = 0.** `/t:Rebuild` cleaned and
  recompiled every project, so the analyzers actually ran. A warm `/t:Build` would have returned
  exit 0 with `CoreCompile` skipped and would have gated nothing.

### The five warnings (all pre-existing, none analyzer diagnostics)

All five are the same MSBuild-level message, one per project that still uses `packages.config`:

```
warning : The project contains a packages.config file, which is not supported by System.Reactive
v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the
RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.)
```

Emitting projects: `QuickFiler/QuickFiler.csproj`, `TaskMaster/TaskMaster.csproj`,
`ToDoModel/ToDoModel.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`,
`UtilitiesCS/UtilitiesCS.csproj`.

These are not Roslyn analyzer diagnostics and carry no rule ID; they are a packaging advisory
emitted by the System.Reactive build targets. They are pre-existing, unrelated to this feature, and
out of its writable file set (`QuickFiler/QuickFiler.csproj` is explicitly forbidden). The Phase 4
analyzer gate `[P4-T2]` requires `EXIT_CODE: 0` and an error count of zero, which this baseline
already meets, so any error introduced by this change would be attributable to the change.
