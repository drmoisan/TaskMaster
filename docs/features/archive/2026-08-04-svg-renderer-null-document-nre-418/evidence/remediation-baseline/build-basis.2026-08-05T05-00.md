# Build and Formatting Comparison Basis — Remediation Cycle 2

- Task: `[P0-T10]`
- Timestamp: 2026-08-04T23-43 — **this is the transcription time, not an execution time**
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`

## Field shape — read this before reading any figure below

**This artifact is a transcription, not an execution.** No csharpier run, no analyzer build, and no
nullable build was performed by `[P0-T10]`. Consequently:

- `Timestamp:` above is the time this transcription was written.
- Every `Command:` and `EXIT_CODE:` value below is **quoted from a named source artifact**, with that
  source stated alongside it. **None of these exit codes was produced by a command run in this cycle.**
- A reaudit must not read the quoted exit codes as evidence of commands executed during remediation
  cycle 2. The cycle-2 executions of these same commands are `[P2-T1]`, `[P2-T2]`, `[P2-T4]`, and
  `[P2-T6]`, which write their own artifacts in the `2026-08-05T05-00` series.

`EXIT_CODE: 0` for this task itself: the three source artifacts were read successfully and every
required figure was found as a number.

## Why reuse is valid here — the verified precondition

Per this plan's Design Decision 5, the `evidence/qa-gates/*.2026-08-05T01-50.md` series was captured in
and committed as `a62391f7` and records the end state of that commit's source tree. Reuse as this
cycle's comparison basis is valid for any HEAD whose source and build-configuration tree is identical to
`a62391f7`'s.

**That identity was verified, not assumed.** `[P0-T5]` invariant (c) measured
`git diff --name-only a62391f7 HEAD` and found **14 differing paths, all `.md`, with 0 matching
`.cs`, `.csproj`, `packages.config`, or `app.config`**. Evidence:
`evidence/remediation-baseline/tree-state.2026-08-05T05-00.md`.

No particular HEAD is named as the basis of this argument, deliberately: the gate is the measured
absence of source and build-configuration difference, which holds across any number of intervening
documentation or agent-memory commits. Documentation and memory files are not inputs to the formatting,
analyzer, or nullable gates, so the recorded figures are unaffected and directly comparable.

The `evidence/baseline/*.2026-08-04T14-36.md` series is **not** used as a comparison basis: it was
captured on a host lacking the VSTO runtime assemblies and its diagnostic set includes `CS0234`/`MSB3245`
failures that do not occur on the current host.

## 1. CSharpier

Source artifact: `evidence/qa-gates/csharpier-check.2026-08-05T01-50.md`.

| Field | Value | Provenance |
|---|---|---|
| Command (quoted) | `dotnet tool run csharpier check .` (run from the repository root) | quoted from the source artifact |
| `EXIT_CODE` (quoted) | **0** | quoted from the source artifact |
| Files checked | **1467** | quoted verbatim output: `Checked 1467 files in 5241ms.` |
| **Files needing formatting** | **0** | source artifact § "Files needing formatting: 0"; measured there as `grep -c "Was not formatted"` = 0 |

The source artifact notes the 1467 count is one higher than the `2026-08-04T14-36` series' 1466 because
cycle 1 added one C# file, `SVGControl/SvgAssemblyResolver.cs`.

**Basis for `[P2-T2]`:** `EXIT_CODE: 0` and **0** files needing formatting. This cycle modifies no `.cs`
file, so the expected cycle-2 figure is identical. Note that `packages.config` is **not**
csharpier-exempt, so the file count may differ if csharpier enumerates non-`.cs` files; the operative
comparison is the zero-files-needing-formatting figure.

## 2. Analyzer build

Source artifact: `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md`.

| Field | Value | Provenance |
|---|---|---|
| Command (quoted) | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild` | quoted from the source artifact |
| `EXIT_CODE` (quoted) | **0** | quoted from the source artifact |
| **Errors** | **0** | quoted summary line `0 Error(s)` |
| **Warnings** | **6** | quoted summary line `6 Warning(s)` |
| `csc.exe` invocations | **34** | quoted; establishes the run was a genuine recompile, not incrementally vacuous |
| Elapsed | `00:00:11.43` | quoted |

### Complete per-code per-project warning inventory (the comparison basis for `[P2-T4]`)

Transcribed exactly as the source artifact records it:

| Count | Severity | Code | Emitting project | Text |
|---|---|---|---|---|
| 1 | warning | (no code) | `UtilitiesCS/UtilitiesCS.csproj` | `System.Reactive.PackagesConfigCheck.targets(31,5)`: "The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference." |
| 1 | warning | (no code) | `ToDoModel/ToDoModel.csproj` | same |
| 1 | warning | (no code) | `QuickFiler/QuickFiler.csproj` | same |
| 1 | warning | (no code) | `TaskMaster/TaskMaster.csproj` | same |
| 1 | warning | (no code) | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | same |
| 1 | warning | `CS2002` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `Source file 'C:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` |
| **0** | **error** | — | — | — |

**Total: 6 warnings, 0 errors.** Five code-less `System.Reactive` `packages.config` advisories plus one
`CS2002`.

Also transcribed, because `[P2-T4]`/`[P2-T5]` compare against it: the source artifact records that
`SVGControl`, `SVGControl.Test`, and every other project emitted **zero** warnings and **zero** errors,
with **zero** `MSB3277`, **zero** `MSB3245`, **zero** `MSB3061`, **zero** `CS0234`, **zero** `CS8632`,
**zero** `CA*`, and **zero** `IDE*`.

### Note carried forward for `[P2-T5]`'s removal disposition

The source artifact records that `CS2002` is **`CoreCompile`-gated**: it is emitted only when the
emitting project actually recompiles. Its own baseline comparison observed 5 warnings in an incrementally
vacuous run (0 `csc.exe`) versus 6 in the 34-`csc.exe` run, the difference being exactly this `CS2002`.

This cycle's only changed inputs are under `SVGControl.Test`, so `UtilitiesCS.Test` may not recompile and
`CS2002` may legitimately be **absent** from the `[P2-T4]` inventory. Per `[P2-T5]`, such a removal is
**not** a regression, requires no fix, and triggers **no** loop restart — it must be recorded with that
reason and its emitting project.

## 3. Nullable gate

Source artifact: `evidence/qa-gates/nullable-build.2026-08-05T01-50.md`.

### 3.1 Mandated solution-level command

| Field | Value | Provenance |
|---|---|---|
| Command (quoted) | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors` | quoted from the source artifact |
| `EXIT_CODE` (quoted) | **0** | quoted from the source artifact |
| Errors | **0** | quoted summary line `0 Error(s)` |
| Warnings | **5** | quoted summary line `5 Warning(s)` — the five code-less `System.Reactive` advisories; zero `CS86xx`, zero `CS8630` |
| Elapsed | `00:00:00.89` | quoted |
| `CoreCompile:` occurrences | **18** | quoted measurement |
| `Skipping target "CoreCompile"` occurrences | **18** (all 18 skipped) | quoted measurement |
| `csc.exe` invocations | **0** | quoted measurement |

**This exit code is vacuous and is not evidence of nullable cleanliness.** `Invoke-VSBuild.ps1` hardcodes
MSBuild target `Build`, and legacy non-SDK up-to-date checks are timestamp-based rather than
property-based, so `/p:Nullable=enable /p:TreatWarningsAsErrors=true` forces no recompile and re-analyzes
no source file. This is policy-audit gap G-3(b). `[P2-T6]` must restate the same caveat and rely on its
two forced project-scope rebuilds for the actual evidence.

### 3.2 Supplementary forced project-scope rebuild — `SVGControl`

| Field | Value | Provenance |
|---|---|---|
| Command (quoted) | `& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl\SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` | quoted from the source artifact |
| `EXIT_CODE` (quoted) | **0** | quoted from the source artifact |

Complete per-code per-file diagnostic table, transcribed exactly:

| Count | Severity | Code | File | Text |
|---|---|---|---|---|
| 0 | — | — | — | — |

**Total diagnostics: 0** (0 errors, 0 warnings), measured in the source artifact by
`grep -cE "(warning|error) [A-Z]+[0-9]+"` = 0. Verbatim output was the single line
`SVGControl -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl\bin\Debug\SVGControl.dll`.

### 3.3 Supplementary forced project-scope rebuild — `SVGControl.Test`

| Field | Value | Provenance |
|---|---|---|
| Command (quoted) | `& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m` | quoted from the source artifact |
| `EXIT_CODE` (quoted) | **0** | quoted from the source artifact |

Complete per-code per-file diagnostic table, transcribed exactly:

| Count | Severity | Code | File | Text |
|---|---|---|---|---|
| 0 | — | — | — | — |

**Total diagnostics: 0** (0 errors, 0 warnings), measured in the source artifact by
`grep -cE "(warning|error) [A-Z]+[0-9]+"` = 0. Verbatim output was two lines, `SVGControl -> ...` and
`SVGControl.Test -> ...\SVGControl.Test\bin\Debug\SVGControl.Test.dll`. The source artifact records that
the single pre-existing `CS8630` present at its own `[P0-T8]` baseline was eliminated by cycle 1's
`<LangVersion>latest</LangVersion>` addition and that nothing replaced it.

### MSBuild path note for `[P2-T6]`

The source artifact's supplementary rebuilds resolved MSBuild at
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, which is the
path `[P2-T6]` names. Both used `/p:Platform=AnyCPU` without incident, so no `/p:Platform`-omission
fallback was required. `[P2-T6]` re-verifies the path exists on this host rather than assuming it.

## Consolidated basis table — the sole comparison basis for `[P2-T4]`, `[P2-T5]`, and `[P2-T6]`

| Gate | Source artifact (`2026-08-05T01-50` series) | Quoted `EXIT_CODE` | Key figures |
|---|---|---|---|
| csharpier check | `evidence/qa-gates/csharpier-check.2026-08-05T01-50.md` | 0 | 1467 checked; **0** needing formatting |
| Analyzer build | `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md` | 0 | **0** errors, **6** warnings (5 code-less `System.Reactive` + 1 `CS2002` in `UtilitiesCS.Test`); 34 `csc.exe` |
| Nullable, mandated | `evidence/qa-gates/nullable-build.2026-08-05T01-50.md` § 1 | 0 | **vacuous**: 18/18 `CoreCompile` skipped, 0 `csc.exe`; 5 warnings, 0 `CS86xx` |
| Nullable, forced `SVGControl` | same artifact § 2 | 0 | **0** diagnostics |
| Nullable, forced `SVGControl.Test` | same artifact § 3 | 0 | **0** diagnostics; baseline `CS8630` eliminated |

## Output Summary

The build and formatting comparison basis is registered as numbers, with no placeholder. csharpier:
`EXIT_CODE: 0`, 1467 files checked, **0** needing formatting. Analyzer build: `EXIT_CODE: 0`, **0**
errors, **6** warnings, with the complete six-row per-code per-project inventory transcribed (five
code-less `System.Reactive` `packages.config` advisories in `UtilitiesCS`, `ToDoModel`, `QuickFiler`,
`TaskMaster`, `UtilitiesCS.Test`, plus one `CS2002` in `UtilitiesCS.Test`) and 34 `csc.exe` invocations
establishing it as a genuine recompile. Nullable gate: the mandated command returned `EXIT_CODE: 0`
vacuously (18/18 `CoreCompile` targets skipped, 0 `csc.exe`), and both supplementary forced
project-scope rebuilds — `SVGControl.csproj` and `SVGControl.Test.csproj` — returned `EXIT_CODE: 0` with
complete diagnostic tables of **0** rows each. Every `Command:` and `EXIT_CODE:` above is quoted from its
named source artifact and none was executed by this task. Reuse is licensed by `[P0-T5]` invariant (c),
which measured **0** `.cs`/`.csproj`/`packages.config`/`app.config` differences between the executing
HEAD and `a62391f7`.
