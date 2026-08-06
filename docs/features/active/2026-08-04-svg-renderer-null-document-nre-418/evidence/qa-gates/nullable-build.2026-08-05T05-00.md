# Final QC Stage 3 — Nullable Gate + Supplementary Forced Rebuilds

- Task: `[P2-T6]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-15
- Comparison basis: `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` § 3

## Environment preconditions verified before running

**MSBuild path.** The path `[P2-T6]` names was verified present rather than assumed, so no `vswhere`
fallback was needed:

```
Command: ls -la "C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe"
Output:  -rwxr-xr-x 1 DanMoisan 197121 378712 Jul 17 13:09 .../MSBuild.exe
```

**Outlook not running.** Checked because `/t:Rebuild` deletes output files and a live Outlook process
holding a VSTO output would emit `MSB3061`:

```
Command: Get-Process -Name OUTLOOK
Output:  OUTLOOK NOT RUNNING
```

Consequently **zero `MSB3061` warnings** were emitted by either forced rebuild
(`grep -c 'MSB3061'` = 0 on both logs), so no baseline citation for `MSB3061` is required.

---

## 1. Mandated solution-level nullable gate

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors
```

```
EXIT_CODE: 0
```

Summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.90
```

### This exit code is NOT evidence of nullable cleanliness

Stated explicitly, as `[P2-T6]` requires. The run executed **zero `CoreCompile` targets**:

```
grep -c "CoreCompile:"                   -> 18
grep -c 'Skipping target "CoreCompile"'  -> 18   (all 18 skipped)
grep -c "csc.exe"                        ->  0
```

`scripts/vscode/Invoke-VSBuild.ps1` hardcodes MSBuild target `Build`, and legacy non-SDK up-to-date checks
compare **timestamps, not properties**, so `/p:Nullable=enable /p:TreatWarningsAsErrors=true` forces no
recompile and re-analyzes no source file. The command completed in 0.90 s. A legacy up-to-date check can
execute zero `CoreCompile` targets, and that is exactly what happened here: **`EXIT_CODE: 0` is a true
record of what the mandated command returns and nothing more.** This is policy-audit gap G-3(b), a
repository-level concern outside this feature's scope (the inputs' R-12, deliberately not this cycle's to
fix).

Its five warnings are the five code-less `System.Reactive.PackagesConfigCheck.targets(31,5)`
`packages.config` advisories. Measured: **zero `CS86xx`** (`grep -cE 'CS86[0-9][0-9]'` = 0) and **zero
`CS8630`**.

Basis comparison: identical in every respect — the basis also records `EXIT_CODE: 0`, 5 warnings,
0 errors, 18/18 `CoreCompile` skipped, 0 `csc.exe`, 0 `CS86xx`, in 0.89 s.

**The two supplementary forced project-scope rebuilds below are the actual nullable evidence.** This
satisfies the binding `## Do Not Do` requirement: "Do not report a green toolchain from a build that
compiled nothing. When verifying the type-check stage, force a recompile of the changed projects and state
that you did." A forced recompile of both in-scope projects was performed, and this is that statement.

---

## 2. Supplementary forced project-scope rebuild — `SVGControl.Test`

Labelled **supplementary**. Not the mandated command. Run first, because it is the project this cycle
modifies.

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m
```

```
EXIT_CODE: 0
```

`/p:Platform=AnyCPU` was accepted; MSBuild did not report the platform undefined, so the
`/p:Platform`-omission fallback was not needed.

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
`grep -cE "(warning|error) [A-Z]+[0-9]+"` = **0**.

This is a genuine full recompile under `/t:Rebuild`, so both `SVGControl.dll` and `SVGControl.Test.dll`
were re-emitted from source under the strictest property set, including every test file
(`SvgAssemblyProbeDirectoryTests.cs`, `SvgRendererParseContractTests.cs`,
`SvgRendererNullToleranceTests.cs`, `RelativePath` tests) and the newly added `ExCSS` reference.

### Comparison against the basis

| Metric | Basis (`build-basis` § 3.3) | This run | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | unchanged |
| Total diagnostics | **0** | **0** | unchanged |
| `CS8630` | 0 | **0** | unchanged |

**No diagnostic in this supplementary set is absent from the basis** — the basis set is empty and so is
this one, so the condition holds trivially and completely. **No newly introduced diagnostic exists, so no
fix was required and no loop restart was triggered by this task.**

---

## 3. Supplementary forced project-scope rebuild — `SVGControl`

Labelled **supplementary**. Not the mandated command.

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl\SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m
```

```
EXIT_CODE: 0
```

Complete verbatim output:

```

  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl\bin\Debug\SVGControl.dll
```

### Per-code per-file diagnostic table

| Count | Severity | Code | File | Text |
|---|---|---|---|---|
| 0 | — | — | — | — |

Total diagnostics: **0** (0 errors, 0 warnings), measured by
`grep -cE "(warning|error) [A-Z]+[0-9]+"` = **0**.

A genuine full recompile of the assembly under the strictest property set, covering the three files cycle
1 changed or created (`SvgRenderer.cs`, `SvgAssemblyProbe.cs`, `SvgAssemblyResolver.cs`) and the untouched
legacy files (`DropDownEditor.cs`, `SVGParser.cs`, `ToggleSwitch.cs`, `SvgFileNameEditor.cs`, the three
converters).

### Comparison against the basis

| Metric | Basis (`build-basis` § 3.2) | This run | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | **0** | unchanged |
| Total diagnostics | **0** | **0** | unchanged |

**No diagnostic absent from the basis.** No fix required, no loop restart.

---

## 4. Post-rebuild output-tree state, verified for `[P2-T7]` and `[P2-T9]`

`/t:Rebuild` performs Clean then Build, which deletes the output directory contents. Because `[P2-T7]`
and `[P2-T9]` consume those outputs, the tree was verified rather than assumed. This also re-confirms the
copy-local mechanism survives a full clean.

`SVGControl.Test/bin/Debug`:

```
PRESENT: SVGControl.Test.dll
PRESENT: ExCSS.dll
PRESENT: Svg.dll
PRESENT: SVGControl.dll
ABSENT:  Fizzler.dll
```

**`ExCSS.dll` was re-copied by the clean rebuild**, which is stronger evidence than the `[P1-T4]`
incremental copy: the reference genuinely drives copy-local from a clean output directory. `Fizzler.dll`
remains absent, confirming Design Decision 3 continues to hold.

All nine test assemblies are present, so `[P2-T7]`'s expected assembly count of 9 is achievable:

```
PRESENT: QuickFiler.Test.dll      PRESENT: Tags.Test.dll             PRESENT: TaskMaster.Test.dll
PRESENT: TaskTree.Test.dll        PRESENT: TaskVisualization.Test.dll PRESENT: ToDoModel.Test.dll
PRESENT: UtilitiesCS.Test.dll     PRESENT: VBFunctions.Test.dll      PRESENT: SVGControl.Test.dll
```

Unlike a failing rebuild, both `/t:Rebuild` invocations **succeeded**, so no assembly was left deleted and
no build-state recovery was needed or performed.

Disclosed for completeness: because the `SVGControl` rebuild ran after the `SVGControl.Test` rebuild,
`SVGControl/bin/Debug/SVGControl.dll` is a later emission than the copy in
`SVGControl.Test/bin/Debug/SVGControl.dll`. Both are compiled from identical, unmodified source; the
property difference (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`) governs diagnostics rather than
emitted semantics, so there is no behavioral divergence. `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
does not build — it discovers existing `*.Test.dll` assemblies and throws if none are found — so
`[P2-T7]` runs against exactly the tree verified above.

---

## Output Summary

- **Mandated command:** `EXIT_CODE: 0`, 0 errors, 5 warnings, in 0.90 s — **vacuous**, with 18 of 18
  `CoreCompile` targets skipped and 0 `csc.exe` invocations. Stated explicitly as **not** evidence of
  nullable cleanliness, because a legacy timestamp-based up-to-date check can execute zero `CoreCompile`
  targets. Zero `CS86xx`, zero `CS8630`.
- **Supplementary forced `SVGControl.Test` rebuild:** `/t:Rebuild` at `EXIT_CODE: 0` with a **0-row**
  per-code per-file diagnostic table, identical to the basis's 0. A genuine recompile of the only project
  this cycle modifies.
- **Supplementary forced `SVGControl` rebuild:** `/t:Rebuild` at `EXIT_CODE: 0` with a **0-row**
  diagnostic table, identical to the basis's 0.
- Environment: MSBuild resolved at the named path; Outlook not running; **zero `MSB3061`** on both
  rebuilds.
- Post-rebuild tree verified: `ExCSS.dll` re-copied into a cleaned output directory, `Fizzler.dll` still
  absent, all nine test assemblies present.
- **Newly introduced nullable diagnostics: none. No restart.** Stage 3 of toolchain pass 1 is clean and
  the loop proceeds to `[P2-T7]`.
