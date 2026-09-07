# QA Gate 2 of 4 — .NET Analyzers ([P4-T2], post-base-merge re-run)

Timestamp: 2026-08-27T23-14

Command:
```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal
```
(run through `pwsh -NoProfile` from the workspace root)

Resolved MSBuild path: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

EXIT_CODE: 0

## Output Summary

- **Errors: 0.** **Warnings: 5.** MSBuild summary line: `5 Warning(s) / 0 Error(s)`, `Time Elapsed 00:00:13.83`.
- All five warnings are the same non-code diagnostic emitted by
  `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`:
  "The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
  later." It is raised once per affected project, and the five affected projects are exactly
  `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`, `UtilitiesCS.csproj`, and
  `UtilitiesCS.Test.csproj`. The diagnostic carries no `CSxxxx`, `CAxxxx`, or `IDExxxx` identifier.
  Zero Roslyn analyzer and zero code-style diagnostics were produced. None of the five projects is
  a file this feature owns.
- Acceptance requires `EXIT_CODE: 0` and an error count of zero. Both hold.

## Non-vacuity proof

`/t:Rebuild` was used, not `/t:Build`. Counted over the full normal-verbosity log:

| Measure | Count |
| --- | --- |
| `Skipping target "CoreCompile"` occurrences | **0** |
| `csc.exe` invocations | 36 |
| `CoreCompile:` target headers | 65 |
| Projects reported building | 19 |

Zero skipped `CoreCompile` targets against 36 `csc.exe` invocations establishes that every project
was genuinely recompiled and every analyzer genuinely ran, so the zero-error result is a real
measurement rather than an up-to-date short-circuit. (`Task "Csc"` was deliberately not counted; it
reads zero even on a real compile at this verbosity.)
