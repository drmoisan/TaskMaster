# QA Gate 3 of 4 — Type Check / Nullable ([P4-T3], post-base-merge re-run)

Timestamp: 2026-08-27T23-15

Command:
```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:normal
```
(run through `pwsh -NoProfile` from the workspace root)

Resolved MSBuild path: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

EXIT_CODE: 0

## Output Summary

- **Errors: 0.** **Warnings: 5.** MSBuild summary line: `5 Warning(s) / 0 Error(s)`,
  `Time Elapsed 00:00:15.22`.
- `/p:Nullable=enable` was **not** added, as the task and `CLAUDE.md` both require. Nullable
  participation in this repository is per-file opt-in through the `#nullable enable` pragma, and
  `.github/workflows/ci.yml` omits the solution-wide property deliberately. The argument list after
  the executable, apart from the `/v:normal` verbosity switch added to make the non-vacuity count
  readable, is character-for-character the command mandated by `CLAUDE.md` §C#1.3.
- **`CS86xx` diagnostic count: 0.** Under `/p:TreatWarningsAsErrors=true` any nullable-flow warning
  in a `#nullable enable` file — which `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:1` is — would be
  promoted to a build error and fail this gate. None was produced, so all new code in that file is
  nullable-clean.
- The same five non-code `System.Reactive` `packages.config` warnings recorded by
  `qa-2-analyzers-rebuild.2026-08-27T23-14.md` are the entire warning set here as well, raised by
  `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`, `UtilitiesCS.csproj`, and
  `UtilitiesCS.Test.csproj`. They do not become errors because they are emitted by an MSBuild target
  rather than by the compiler.
- Acceptance requires `EXIT_CODE: 0`. Satisfied.

## Non-vacuity proof

| Measure | Count |
| --- | --- |
| `Skipping target "CoreCompile"` occurrences | **0** |
| `csc.exe` invocations | 36 |
| `CoreCompile:` target headers | 59 |

`/t:Rebuild` was used, not `/t:Build`. Zero skipped `CoreCompile` targets against 36 `csc.exe`
invocations establishes that every project was recompiled with `TreatWarningsAsErrors` in force, so
the zero-error result is a real type-check measurement.
