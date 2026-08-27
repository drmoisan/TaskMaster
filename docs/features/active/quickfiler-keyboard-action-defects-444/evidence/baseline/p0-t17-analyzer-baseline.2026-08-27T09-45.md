# [P0-T17] Baseline analyzer build

Timestamp: 2026-08-27T09-45
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

MSBuild resolved to `<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
via `vswhere`. The command was run from `WS` under `pwsh -NoProfile`; it was not run through a POSIX
shell, so no bare `/m` switch was rewritten into a drive-style path.

## Summary line (verbatim)

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.28
```

## Baseline figures

```
BaselineAnalyzerErrors   = 0
BaselineAnalyzerWarnings = 5
```

## Proof the gate actually compiled

`/t:Rebuild` was used deliberately. A warm `/t:Build` returns exit 0 having skipped `CoreCompile` on
every project, because MSBuild's up-to-date check does not invalidate on a command-line `/p:` change.

| Evidence | Value |
| --- | --- |
| Occurrences of `Skipping target "CoreCompile"` in the log | **0** |
| Log lines captured | 4755 |
| `csc.exe` references in the log | 36 |
| `CoreCompile` references in the log | 45 |
| Assembly output lines (`-> <path>`) | 18 |
| `QuickFiler/bin/Debug/QuickFiler.dll` | freshly written during this build |
| `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` | freshly written during this build |

A zero count of `Skipping target "CoreCompile"` is the assertion of record. The build genuinely
compiled: this was a fresh worktree with no `bin`/`obj` output before `[P0-T17]`, and 18 assemblies
were produced.

## The five warnings, characterised

All five are the same pre-existing diagnostic, one per project that consumes `System.Reactive 7.0.0`
through `packages.config`:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference.
```

They are emitted by a NuGet package's `.targets` file, not by a Roslyn analyzer, and are unrelated to
this feature's diff. `[P4-T4]` compares against `BaselineAnalyzerWarnings = 5` rather than asserting an
absolute zero, exactly as the plan's baseline-relative-gate convention requires.

## Notable absence

No `error CS0006` occurred. The analyzer version back-fills in `[P0-T9]` and `[P0-T10]` were the
reason: the hand-authored `<Analyzer Include>` items name `Meziantou.Analyzer 3.0.156` and
`Roslynator.Analyzers 4.16.0`, neither of which the solution restore provides.

Output Summary: exit code 0; `BaselineAnalyzerErrors = 0`, `BaselineAnalyzerWarnings = 5` (all five the
same pre-existing System.Reactive packages.config diagnostic); zero `Skipping target "CoreCompile"`
occurrences, so the gate compiled rather than short-circuiting.
