# Final QC Stage 2 — Solution Analyzer Build

- Task: `[P2-T4]`
- Issue: #418
- Evidence series: `2026-08-05T01-50`
- Toolchain pass: **1**

Timestamp: 2026-08-05T02-02 (UTC)

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

EXIT_CODE: 0

Summary lines:

```
Build succeeded.
    6 Warning(s)
    0 Error(s)
Time Elapsed 00:00:11.43
```

- **Errors: 0**
- Warnings: 6
- **`csc.exe` invocations: 34** — this run genuinely recompiled the projects whose inputs changed, so it is
  **not** the incrementally vacuous shape `[P0-T7]` recorded.

## Per-code per-project warning inventory

| Count | Severity | Code | Emitting project | Text |
|---|---|---|---|---|
| 1 | warning | (no code) | `UtilitiesCS/UtilitiesCS.csproj` | `System.Reactive.PackagesConfigCheck.targets(31,5)`: "The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference." |
| 1 | warning | (no code) | `ToDoModel/ToDoModel.csproj` | same |
| 1 | warning | (no code) | `QuickFiler/QuickFiler.csproj` | same |
| 1 | warning | (no code) | `TaskMaster/TaskMaster.csproj` | same |
| 1 | warning | (no code) | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | same |
| 1 | warning | `CS2002` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | `Source file 'C:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` |
| **0** | **error** | — | — | — |

`SVGControl`, `SVGControl.Test`, and every other project emitted zero warnings and zero errors. Zero
`MSB3277`, zero `MSB3245`, zero `MSB3061`, zero `CS0234`, zero `CS8632`, zero `CA*`, zero `IDE*`.

## Line-by-line comparison against `evidence/remediation-baseline/analyzer-build.2026-08-05T01-50.md`

That baseline artifact records the pre-existing set as the **union** of two rows, because the `[P0-T7]`
run itself was incrementally vacuous (18/18 `CoreCompile` targets skipped, 0 `csc.exe` invocations) and
`CS2002` is emitted only at `CoreCompile` time. The union is stated in that artifact's
§ "Comparison basis for `[P2-T4]`".

| Diagnostic | Emitting project | Present in the `[P0-T7]` pre-existing set | Present in this run | Verdict |
|---|---|---|---|---|
| code-less System.Reactive `packages.config` warning | `UtilitiesCS` | yes (measured) | yes | pre-existing, unchanged |
| code-less System.Reactive `packages.config` warning | `ToDoModel` | yes (measured) | yes | pre-existing, unchanged |
| code-less System.Reactive `packages.config` warning | `QuickFiler` | yes (measured) | yes | pre-existing, unchanged |
| code-less System.Reactive `packages.config` warning | `TaskMaster` | yes (measured) | yes | pre-existing, unchanged |
| code-less System.Reactive `packages.config` warning | `UtilitiesCS.Test` | yes (measured) | yes | pre-existing, unchanged |
| `CS2002` duplicate `<Compile>` | `UtilitiesCS.Test` | yes (union row, from `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md` at 36-`CoreCompile` scope on this same HEAD) | yes | pre-existing, unchanged |

**Newly introduced diagnostics: none.** Every diagnostic in this run matches a pre-existing entry in code,
count, text, and emitting project. No diagnostic is attributable to any file in the Scope Lock. No fix was
required and **no loop restart was triggered by this task.**

Note on the raw warning totals (6 here versus 5 at `[P0-T7]`): the difference is entirely the
`CoreCompile`-gated `CS2002`. `[P0-T7]` executed 0 `csc.exe` invocations so it could not observe it; this
run executed 34 and does. The identical 6-warning total, with the identical composition, was independently
recorded at this HEAD before this cycle began, in
`evidence/qa-gates/analyzer-build.2026-08-04T14-36.md` (`6 Warning(s) 0 Error(s)`, 36 `CoreCompile`
targets) and again in the `[P0-T8]` build-state recovery run. The count is therefore unchanged relative to
the pre-existing non-vacuous baseline.

## Output Summary

`EXIT_CODE: 0`, **0 errors, 6 warnings**, 34 `csc.exe` invocations (a genuine recompile). The warning set
is the five code-less System.Reactive `packages.config` warnings plus the one pre-existing `CS2002`
duplicate `<Compile>` in `UtilitiesCS.Test` — identical in code, count, text, and emitting project to the
pre-existing baseline set. **Zero newly introduced diagnostics; no restart.** Stage 2 of toolchain pass 1
is clean.
