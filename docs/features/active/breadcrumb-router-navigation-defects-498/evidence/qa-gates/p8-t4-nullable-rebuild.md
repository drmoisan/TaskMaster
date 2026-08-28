# P8-T4 — Toolchain Step 3, Nullable and Type-Check Gate

Timestamp: 2026-08-26T11-24

Pass number: **3** — the final pass.

Command: `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:TreatWarningsAsErrors=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

- Exit code: **0** — the primary acceptance condition, met absolutely.
- **Error count: 0.** MSBuild summary: `5 Warning(s), 0 Error(s)`. `Build succeeded.` Time elapsed
  00:00:25.03.

### Command-shape assertions

- The command contains **neither `/p:Nullable=enable` nor `/t:Build`**, as the acceptance condition
  requires. It is character-for-character the command in `.github/workflows/ci.yml` for the step "Build
  with nullable warnings treated as errors", except that `/t:Rebuild` is used instead of CI's `/t:Build`
  because a local working tree is warm and `/t:Build` would skip `CoreCompile` on every project.
- It is the Rebuild recipe with the two analyzer properties replaced by
  `"/p:TreatWarningsAsErrors=true"`, exactly as the task specifies.

### Diagnostic breakdown

A scan of the full build log for the pattern `(warning|error) [A-Z]+[0-9]+` returned **zero matches**.
There are therefore **zero `CS86xx` nullable-flow diagnostics** promoted to errors. Every file this
feature wrote or created that carries a `#nullable enable` pragma is clean under
`/p:TreatWarningsAsErrors=true`, and no diagnostic names any file on the `P8-T1` target list.

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` carries `#nullable enable` at line 1 and
is clean. The three tests appended in the `P8-T7` remediation introduced no nullable diagnostic.

All five warnings are the same uncoded `System.Reactive` packages.config advisory recorded by `P8-T3`
and by the `P0-T13`/`P0-T14` baselines, on the same five projects. It is not promoted to an error
because `TreatWarningsAsErrors` promotes coded compiler warnings, not uncoded MSBuild task messages.

### Comparison with the `P0-T14` baseline

| Metric | `P0-T14` baseline | This run | Change |
|---|---:|---:|---|
| Exit code | 0 | 0 | none |
| Errors | 0 | 0 | none |
| Warnings | 5 | 5 | none |

Exact parity with the baseline. Pass 1 produced the identical result.

### Degradation status

The conditional degradation is permitted ONLY IF the `P0-T14` baseline recorded a non-zero exit code. It
recorded `EXIT_CODE: 0`, so the degradation branch is **unavailable**. The gate stood at its primary
condition `EXIT_CODE: 0` and met it. No `ExpectedExitCode:` is declared.
