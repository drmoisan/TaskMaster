# Final QC Stage 2 — Solution Analyzer Build

- Task: `[P2-T4]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-10
- Comparison basis: `evidence/remediation-baseline/build-basis.2026-08-05T05-00.md` § 2

## Command

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

Run from the repository root.

```
EXIT_CODE: 0
```

Summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.05
```

- **Errors: 0**
- Warnings: 5
- `csc.exe` invocations: **0**; `CoreCompile:` occurrences 18, `Skipping target "CoreCompile"` 18 —
  **all 18 skipped**. This run is incrementally up-to-date and compiled nothing. Disclosed, with the
  probative companion run recorded in § "Compilation provenance" below.

## Per-code per-project warning inventory

Five distinct warnings. Each appears twice in the raw log because MSBuild repeats warnings in its
end-of-build summary block; the inventory below counts distinct diagnostics, verified by
`grep -oE 'warning [A-Z0-9]*\s*:.*' | sort | uniq -c` returning `2` for each of exactly five project
paths.

| Count | Severity | Code | Emitting project | Text |
|---|---|---|---|---|
| 1 | warning | (no code) | `UtilitiesCS/UtilitiesCS.csproj` | `System.Reactive.PackagesConfigCheck.targets(31,5)`: "The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference." |
| 1 | warning | (no code) | `ToDoModel/ToDoModel.csproj` | same |
| 1 | warning | (no code) | `QuickFiler/QuickFiler.csproj` | same |
| 1 | warning | (no code) | `TaskMaster/TaskMaster.csproj` | same |
| 1 | warning | (no code) | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | same |
| **0** | **error** | — | — | — |

Measured absences:

```
grep -c 'CS2002'                          -> 0
grep -cE 'MSB3243|MSB3245|MSB3277'        -> 0
grep -cE 'MSB3061'                        -> 0
warning/error lines mentioning SVGControl -> 0
```

**Zero** `MSB3243`, **zero** `MSB3245`, **zero** `MSB3277`, **zero** `MSB3061`, and **zero** warnings or
errors from `SVGControl` or `SVGControl.Test`.

The absence of `MSB3061` is expected and not evidentially interesting: `MSB3061` is a file-delete failure
that arises under `/t:Rebuild` when a process holds an output file, and this task runs target `Build`,
not `Rebuild`.

## Line-by-line comparison against the basis

Basis inventory: 6 warnings, 0 errors (`evidence/remediation-baseline/build-basis.2026-08-05T05-00.md`
§ 2, transcribing `evidence/qa-gates/analyzer-build.2026-08-05T01-50.md`).

| Diagnostic | Emitting project | In basis | In this run | Difference |
|---|---|---|---|---|
| code-less `System.Reactive` `packages.config` warning | `UtilitiesCS` | yes | yes | none |
| code-less `System.Reactive` `packages.config` warning | `ToDoModel` | yes | yes | none |
| code-less `System.Reactive` `packages.config` warning | `QuickFiler` | yes | yes | none |
| code-less `System.Reactive` `packages.config` warning | `TaskMaster` | yes | yes | none |
| code-less `System.Reactive` `packages.config` warning | `UtilitiesCS.Test` | yes | yes | none |
| `CS2002` duplicate `<Compile>` (`PercentageFormatterTests.cs` specified multiple times) | `UtilitiesCS.Test` | **yes** | **no** | **REMOVED** |
| — | — | — | — | **added: none** |

### Delta, stated explicitly

- **Added diagnostics: none.** Every diagnostic in this run matches a basis entry in code, count, text,
  and emitting project. No diagnostic is attributable to any file in the Scope Lock, and in particular
  neither `SVGControl.Test/SVGControl.Test.csproj` nor `SVGControl.Test/packages.config` produced any
  diagnostic.
- **Removed diagnostics: one** — `CS2002` in `UtilitiesCS.Test`.

The totals reconcile: basis 6 warnings − 1 removed + 0 added = **5**, which is the measured figure.

Both the added set (empty) and the removed set (one `CS2002`) are dispositioned by `[P2-T5]`, not by this
task, per the plan's division of responsibility.

## Compilation provenance — disclosed, with the probative companion run

This run executed **0** `csc.exe` invocations, so on its own it is not evidence that the changed project
compiles clean under analyzers. That is disclosed rather than glossed, and the probative evidence is
recorded here.

The **same command with the same properties** was run at `[P1-T4]`, immediately after the three Phase 1
edits, and it did genuinely recompile the affected chain:

| Measurement | `[P1-T4]` run | This `[P2-T4]` run | Basis run |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | 0 |
| Errors | **0** | **0** | 0 |
| Warnings | 5 | 5 | 6 |
| `csc.exe` invocations | **2** | 0 | 34 |
| `CoreCompile:` / skipped | 21 / 17 → **4 executed** | 18 / 18 → 0 executed | — |

The `[P1-T4]` log contains the explicit line
`Compilation request SVGControl.Test, PathToTool=...\Roslyn\csc.exe` and the output line
`SVGControl.Test -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\bin\Debug\SVGControl.Test.dll`.
**`SVGControl.Test` — the only project this cycle modifies — was therefore genuinely recompiled under
`-EnableNETAnalyzers -EnforceCodeStyleInBuild` and emitted zero warnings and zero errors.** Its
five-warning inventory is composed of the identical five code-less `System.Reactive` advisories from the
identical five projects measured above. Evidence:
`evidence/other/excss-copy-local.2026-08-05T05-00.md` § 2.

This run's exit 0 is consistent with that result and adds the confirmation that no other project in the
solution was invalidated by the change.

## Output Summary

`EXIT_CODE: 0`, **0 errors**, **5 warnings**, 0 `csc.exe` invocations (18 of 18 `CoreCompile` targets
skipped — incrementally up-to-date, disclosed). The inventory is the five pre-existing code-less
`System.Reactive` `packages.config` advisories from `UtilitiesCS`, `ToDoModel`, `QuickFiler`,
`TaskMaster`, and `UtilitiesCS.Test`. Compared line by line against the six-row basis: **added
diagnostics — none; removed diagnostics — one, `CS2002` in `UtilitiesCS.Test`.** Zero `MSB3243`,
`MSB3245`, `MSB3277`, `MSB3061`, and zero diagnostics from `SVGControl` or `SVGControl.Test`. The
genuinely-compiling analyzer run of the changed project is `[P1-T4]` (2 `csc.exe`, 4 `CoreCompile`
executed, explicit `Compilation request SVGControl.Test`, 0 errors), cited here as the probative
companion. Both delta sets are passed to `[P2-T5]` for disposition.
