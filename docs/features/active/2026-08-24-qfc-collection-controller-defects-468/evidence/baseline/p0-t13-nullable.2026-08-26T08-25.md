# [P0-T13] Nullable / type-check baseline

Timestamp: 2026-08-26T08-25

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors`

Emitted MSBuild command line (host paths replaced with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /m
```

This is the `CLAUDE.md` §C#1.3 policy command, character-equivalent to the `.github/workflows/ci.yml`
step "Build with nullable warnings treated as errors" modulo the wrapper's `/m` placement.
**`/p:Nullable=enable` is deliberately absent**, per `CLAUDE.md` §C#1.3 and
`.claude/rules/csharp.md`: no project carries a `<Nullable>` element and there is no
`Directory.Build.props`, so forcing it would conscript every file that has never adopted the pragma.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

### Result counts

| Metric | Value |
|---|---|
| Exit code | **0** |
| **Error count** | **0** |
| Warnings | 5 |
| `CS86xx` nullable-flow diagnostics anywhere in the log | **0** |
| Distinct projects that executed `CoreCompile` | **18** |
| `Skipping target "CoreCompile"` occurrences | **0** |
| Wall time | 00:00:29.27 |

### Non-vacuity proof

- `grep -c 'Skipping target "CoreCompile"'` returns **0**.
- **18** distinct `csc.exe` invocations with 18 distinct `/out:` targets, identical to the P0-T12
  set.
- Every `csc.exe` command line carries `/warnaserror+`, confirming that
  `/p:TreatWarningsAsErrors=true` reached the compiler rather than being absorbed by MSBuild. A
  `grep -oE '/warnaserror[+-]?'` over the log yields 36 hits, all `/warnaserror+`.

Because `/warnaserror+` was on every compilation and the compiler produced **0** errors, the
per-file `#nullable enable` opt-ins currently present in the tree all type-check clean at the base
commit.

### The five warnings

Identical set and identical emitting projects to P0-T12: one System.Reactive
`packages.config` compatibility warning each from `UtilitiesCS.csproj`, `ToDoModel.csproj`,
`QuickFiler.csproj`, `TaskMaster.csproj`, and `UtilitiesCS.Test.csproj`.

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference. ...
```

These are **MSBuild target warnings**, not compiler warnings, which is why
`/p:TreatWarningsAsErrors=true` does not promote them: that property maps to `csc`'s `/warnaserror+`
and has no effect on a warning raised by a custom `.targets` file. This is why the build reports
`5 Warning(s)` and still succeeds.

This is the **baseline error set (empty)**. P1-T7 and every later nullable gate assert "no new error
relative to the P0-T13 baseline", meaning: exit code 0 and error count 0.

### Working-tree side effects

`git status --porcelain` after the build is unchanged from the P0-T10 capture. No `.csproj`, `.cs`,
`.xml`, or `.sln` file was modified.

Result: PASS. All four acceptance conditions are met — the exact MSBuild command line, the exit
code, the error count, and a non-zero `CoreCompile` project count.
