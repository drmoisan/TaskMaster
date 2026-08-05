# Baseline Nullable Gate + Supplementary Forced Rebuilds — Remediation Cycle 1

- Task: `[P0-T8]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-29 (UTC)

This artifact is the **sole comparison basis** for `[P1-T6]` and `[P2-T5]`.

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
Time Elapsed 00:00:01.17
```

### This exit code is NOT evidence of nullable cleanliness

Stated explicitly as `[P0-T8]` requires. The run executed **zero `CoreCompile` targets**:

```
grep -c "CoreCompile:"                   -> 18
grep -c 'Skipping target "CoreCompile"'  -> 18   (all 18 skipped)
grep -c "csc.exe"                        ->  0
```

`scripts/vscode/Invoke-VSBuild.ps1` hardcodes MSBuild target `Build`, and legacy non-SDK up-to-date
checks are **timestamp-based, not property-based**. Adding `/p:Nullable=enable
/p:TreatWarningsAsErrors=true` therefore changes no input timestamp, forces no recompile, and
re-analyzes no source file. The command completed in 1.17 s. `EXIT_CODE: 0` is a true record of what
the mandated command returns and nothing more. This is policy-audit gap G-3 and the plan's Design
Decision 10.

Its five warnings are the same five code-less `System.Reactive.PackagesConfigCheck.targets(31,5)`
`packages.config` warnings inventoried in `analyzer-build.2026-08-05T01-50.md`
(`UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test`). Zero `CS86xx`, zero
`CS8630`.

---

## 2. Supplementary forced project-scope rebuild — `SVGControl.Test`

Labelled **supplementary**. Not the mandated command.

Command:

```
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m
```

EXIT_CODE: 1

`/p:Platform=AnyCPU` was accepted; no rerun without `/p:Platform` was required.

Complete verbatim output:

```

  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl\bin\Debug\SVGControl.dll
CSC : error CS8630: Invalid 'nullable' value: 'Enable' for C# 7.3. Please use language version '8.0' or greater. [C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\SVGControl.Test.csproj]
```

### Per-code per-file diagnostic table

| Count | Severity | Code | File / scope | Emitting project | Text |
|---|---|---|---|---|---|
| 1 | error | `CS8630` | `CSC :` (project-level, no file) | `SVGControl.Test/SVGControl.Test.csproj` | `Invalid 'nullable' value: 'Enable' for C# 7.3. Please use language version '8.0' or greater.` |

Total diagnostics: **1**. Warnings: 0. Errors: 1.

### Confirmation required by `[P0-T8]`

**CONFIRMED: the `SVGControl.Test` supplementary set is exactly one `CS8630` and nothing else.**

No `CS86xx` nullable-flow diagnostic appears, and none can: `CS8630` is a compiler-configuration
rejection emitted before any source file is analyzed under nullable rules. The compiler rejected
`/p:Nullable=enable` outright because the project declares no `<LangVersion>` and therefore defaults to
C# 7.3. **Nullable analysis never ran on any file in this project.** This is precisely why R-2
(`[P1-T5]`) is a measure-then-gate item: adding `<LangVersion>latest</LangVersion>` lets nullable
analysis actually execute, and `[P1-T6]` measures what it then finds.

Note that the `SVGControl` project reference built successfully inside this invocation
(`SVGControl -> ...\SVGControl.dll`) under the same `/p:Nullable=enable /p:TreatWarningsAsErrors=true`
property set, emitting nothing. The `SVGControl` project-reference set for `[P1-T6]`'s partition is
therefore expected to be **empty**, and this measurement is **not** vacuous in the sense `[P1-T7]`
guards against.

---

## 3. Supplementary forced project-scope rebuild — `SVGControl`

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

Total diagnostics: **0**. Warnings: 0. Errors: 0.

This is a **genuine** recompilation of the entire `SVGControl` assembly — including the untouched
legacy files `DropDownEditor.cs`, `SVGParser.cs`, `ToggleSwitch.cs`, `SvgFileNameEditor.cs`, and the
three converters — under `/p:Nullable=enable /p:TreatWarningsAsErrors=true`. Risk item 3 in the plan
("`SVGControl` forced-rebuild diagnostics are an unmeasured quantity before `[P0-T8]` runs") is now
resolved: the measured quantity is **zero**. `[P2-T5]` therefore compares the `SVGControl`
supplementary set against **zero**, and any diagnostic there is newly introduced by this cycle.

`/p:Platform=AnyCPU` was accepted; no rerun without `/p:Platform` was required.

---

## 4. Build-state recovery after the failed `/t:Rebuild`

Disclosed side effect: `/t:Rebuild` on `SVGControl.Test` runs `Clean` before `Build`, and the build
then failed on `CS8630`, so `SVGControl.Test/bin/Debug/SVGControl.Test.dll` was deleted and not
regenerated. Left uncorrected, `[P0-T9]` would have discovered eight test assemblies instead of nine.

Recovery command (the mandated analyzer build, the same recovery the reviewer used for policy-audit gap
G-7):

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

EXIT_CODE: 0 — `Build succeeded. 6 Warning(s) 0 Error(s)`, elapsed 11.40 s, **32 `csc.exe`
invocations** (a genuine recompile), and `SVGControl.Test/bin/Debug/SVGControl.Test.dll` restored
(49152 bytes).

The 6 warnings are the 5 code-less System.Reactive warnings plus the 1 pre-existing
`CS2002` in `UtilitiesCS.Test`, which confirms the union statement recorded in
`analyzer-build.2026-08-05T01-50.md` § Comparison basis for `[P2-T4]`: `CS2002` is `CoreCompile`-gated
and reappears whenever a genuine recompile occurs. No source, test, or project file was modified by the
recovery.

---

## Output Summary

- Mandated command: `EXIT_CODE: 0`, 0 errors, 5 warnings — **vacuous**, 0 of 18 `CoreCompile` targets
  executed, 0 `csc.exe` invocations. Not evidence of nullable cleanliness.
- Supplementary `SVGControl.Test` forced rebuild: `EXIT_CODE: 1`, **exactly one `CS8630` and nothing
  else** (CONFIRMED). Nullable analysis did not run on this project.
- Supplementary `SVGControl` forced rebuild: `EXIT_CODE: 0`, **zero diagnostics**. This is the baseline
  `[P2-T5]` compares the `SVGControl` set against.
- Build state restored by the mandated analyzer build (exit 0, 32 `csc.exe` invocations).
