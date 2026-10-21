# Final QA Gate 3 — Analyzer Build (P5-T3)

Timestamp: 2026-08-28T16-07
Command (CR-MSBUILD then CR-ANALYZE, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true'
```

EXIT_CODE: 0

## Output Summary

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:19.12
```

- Errors: **0**.
- Warnings: **5**, byte-identical in count and source to the P0-T7 baseline. All five are the
  uncoded `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`
  advisory ("The project contains a packages.config file, which is not supported by System.Reactive
  v7.0 or later"), raised once each by QuickFiler, TaskMaster, ToDoModel, UtilitiesCS and
  UtilitiesCS.Test. A search for the `warning <CODE>` form returns zero matches, confirming that no
  coded analyzer diagnostic (`CAxxxx`, `Sxxxx`, `MAxxxx`, `RCSxxxx`, `ASYNCxxxx`, `RS0030`, or any
  `CSxxxx`) is present.

**Delta versus baseline: zero.** The change introduces no analyzer diagnostic of any severity. In
particular the deliberate boundary `catch (Exception exception)` in
`QuickFiler/Controllers/QfcFormController.Deactivate.cs` — required because an escaping exception in
a WinForms deactivate handler surfaces as an unhandled UI-thread failure inside Outlook, and
required per item so a single failure cannot stop the remaining selectors being cancelled — raises
no diagnostic and needs no suppression.

`/t:Rebuild` is used rather than `/t:Build`, per `.claude/rules/csharp.md`: MSBuild's incremental
up-to-date check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` can exit 0
with `CoreCompile` skipped on every project and run no analyzers at all.
