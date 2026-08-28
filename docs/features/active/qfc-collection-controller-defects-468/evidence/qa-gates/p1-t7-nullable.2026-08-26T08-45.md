# [P1-T7] Nullable / type-check gate

Timestamp: 2026-08-26T08-45

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Rebuild -TreatWarningsAsErrors`

Emitted MSBuild command line (host paths replaced with `<WS>`):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" <WS>\TaskMaster.sln /t:Rebuild /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /m
```

`/p:Nullable=enable` is deliberately absent, per `CLAUDE.md` §C#1.3 and `.claude/rules/csharp.md`.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**Exit code 0, 0 errors, 5 warnings, 18 projects compiled, 0 `CoreCompile` skips, 0 `CS86xx`
nullable-flow diagnostics. Identical outcome to the P0-T13 baseline. No new error.**

### Result counts, against the P0-T13 baseline

| Metric | P0-T13 baseline | P1-T7 | New? |
|---|---|---|---|
| Exit code | 0 | **0** | — |
| **Error count** | 0 | **0** | **none** |
| Warnings | 5 | **5** | none |
| `CS86xx` diagnostics anywhere in the log | 0 | **0** | none |
| Distinct projects that executed `CoreCompile` | 18 | **18** | — |
| `Skipping target "CoreCompile"` occurrences | 0 | **0** | — |
| Lines carrying `/warnaserror+` | 36 | **36** | — |
| Wall time | 00:00:29.27 | 00:00:14.52 | — |

### Non-vacuity proof

- `grep -c 'Skipping target "CoreCompile"'` returns **0**. No compilation was skipped.
- **18** distinct `/out:` compile targets, the same set as P0-T12/P0-T13/P1-T6.
- **36** log lines carry `/warnaserror+`, confirming `/p:TreatWarningsAsErrors=true` reached `csc`
  on every compilation rather than being absorbed by MSBuild. A gate that did not reach the compiler
  could not fail on a nullable regression.

Because `/warnaserror+` was applied to all 18 compilations and the compiler produced **0** errors,
every file carrying a `#nullable enable` pragma still type-checks clean after the 241-line deletion.

### The five warnings are the identical baseline set

One distinct warning text, unchanged from P0-T13:

```
warning : The project contains a packages.config file, which is not supported by System.Reactive
v7.0 or later. Please migrate to PackageReference. ...
```

These are MSBuild target warnings raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, not
compiler warnings, which is why `/p:TreatWarningsAsErrors=true` does not promote them: that property
maps to `csc`'s `/warnaserror+` and has no effect on a warning raised by a custom `.targets` file.
The build therefore reports `5 Warning(s)` and still succeeds, exactly as at baseline.

### Acceptance verification

- `EXIT_CODE: 0`.
- Non-zero `CoreCompile` project count: **18**.
- No new error relative to the P0-T13 baseline: both are **0**.

Result: PASS. Toolchain step 3 (Type checking) is green; the loop proceeds to step 4 (P1-T8, tests).
