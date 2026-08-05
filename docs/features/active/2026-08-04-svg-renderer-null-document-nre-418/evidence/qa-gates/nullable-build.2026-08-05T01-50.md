# Final QC Stage 3 — Nullable Gate + Supplementary Forced Rebuilds

- Task: `[P2-T5]`
- Issue: #418
- Evidence series: `2026-08-05T01-50`
- Toolchain pass: **1**

Timestamp: 2026-08-05T02-03 (UTC)

Comparison basis: `evidence/remediation-baseline/nullable-build.2026-08-05T01-50.md` (`[P0-T8]`).

---

## 1. Mandated solution-level nullable gate

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors
```

EXIT_CODE: 0

Summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.89
```

### Restated: this exit code is vacuous and is NOT evidence of nullable cleanliness

The run executed **zero `CoreCompile` targets**:

```
grep -c "CoreCompile:"                   -> 18
grep -c 'Skipping target "CoreCompile"'  -> 18   (all 18 skipped)
grep -c "csc.exe"                        ->  0
```

`scripts/vscode/Invoke-VSBuild.ps1` hardcodes MSBuild target `Build`, and legacy non-SDK up-to-date checks
are timestamp-based rather than property-based, so `/p:Nullable=enable /p:TreatWarningsAsErrors=true`
forces no recompile and re-analyzes no source file. The command completed in 0.89 s. `EXIT_CODE: 0` is a
true record of what the mandated command returns and nothing more (policy-audit gap G-3, plan Design
Decision 10). Its five warnings are the five code-less `System.Reactive.PackagesConfigCheck.targets(31,5)`
`packages.config` warnings; zero `CS86xx`, zero `CS8630`.

The two supplementary forced project-scope rebuilds below are the actual nullable evidence.

---

## 2. Supplementary forced project-scope rebuild — `SVGControl`

Labelled **supplementary**. Not the mandated command.

Command:

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl\SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m
```

EXIT_CODE: 0

Complete verbatim output:

```

  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl\bin\Debug\SVGControl.dll
```

### Per-code per-file diagnostic table

| Count | Severity | Code | File | Text |
|---|---|---|---|---|
| 0 | — | — | — | — |

Total diagnostics: **0** (0 errors, 0 warnings), measured by
`grep -cE "(warning|error) [A-Z]+[0-9]+"` = 0.

### Comparison against the `[P0-T8]` baseline

| Metric | `[P0-T8]` baseline | This run | Verdict |
|---|---|---|---|
| EXIT_CODE | 0 | 0 | unchanged |
| Total diagnostics | **0** | **0** | unchanged |

**The `SVGControl` supplementary set contains no diagnostic absent from the baseline** — the requirement
`[P2-T5]` states. This is a genuine full recompile of the assembly under the strictest property set,
including the three files this cycle changed (`SvgRenderer.cs`, `SvgAssemblyProbe.cs`) and created
(`SvgAssemblyResolver.cs`), and the untouched legacy files (`DropDownEditor.cs`, `SVGParser.cs`,
`ToggleSwitch.cs`, `SvgFileNameEditor.cs`, the three converters).

---

## 3. Supplementary forced project-scope rebuild — `SVGControl.Test`

Labelled **supplementary**. Not the mandated command.

Command:

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m
```

EXIT_CODE: 0

Complete verbatim output:

```

  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl\bin\Debug\SVGControl.dll
  SVGControl.Test -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\bin\Debug\SVGControl.Test.dll
```

### Per-code per-file diagnostic table

| Count | Severity | Code | File | Text |
|---|---|---|---|---|
| 0 | — | — | — | — |

Total diagnostics: **0** (0 errors, 0 warnings), measured by
`grep -cE "(warning|error) [A-Z]+[0-9]+"` = 0.

### Gate-token conformance — the clause `[P2-T5]` makes decisive

`[P1-T7]` recorded the token **`R2_KEEP`**. `[P2-T5]` requires that under `R2_KEEP` the
`SVGControl.Test` supplementary set be **zero diagnostics**.

| Metric | `[P0-T8]` baseline | Required under `R2_KEEP` | This run | Verdict |
|---|---|---|---|---|
| EXIT_CODE | 1 | 0 | **0** | satisfied |
| `CS8630` | 1 | 0 | **0** | satisfied |
| Any other diagnostic | 0 | 0 | **0** | satisfied |

**Outcome matches the gate token.** The single pre-existing `CS8630` is eliminated, which is R-2's entire
claim, and no diagnostic replaced it. No newly introduced diagnostic exists, so **no fix was required and
no loop restart was triggered by this task.**

Unlike the `[P0-T8]` run, this `/t:Rebuild` **succeeded**, so `SVGControl.Test/bin/Debug/SVGControl.Test.dll`
was regenerated rather than left deleted. No build-state recovery was needed and none was performed; the
nine test assemblies are all present for `[P2-T6]`.

---

## Output Summary

- Mandated command: `EXIT_CODE: 0`, 0 errors, 5 warnings — **vacuous** (0 of 18 `CoreCompile` targets
  executed, 0 `csc.exe` invocations); restated explicitly as not evidence of nullable cleanliness.
- Supplementary `SVGControl` forced rebuild: `EXIT_CODE: 0`, **0 diagnostics**, identical to the
  `[P0-T8]` baseline of 0. No diagnostic absent from the baseline.
- Supplementary `SVGControl.Test` forced rebuild: `EXIT_CODE: 0`, **0 diagnostics**, matching the
  `R2_KEEP` requirement exactly. The baseline `CS8630` is gone.
- **Newly introduced nullable diagnostics: none. No restart.** Stage 3 of toolchain pass 1 is clean.
