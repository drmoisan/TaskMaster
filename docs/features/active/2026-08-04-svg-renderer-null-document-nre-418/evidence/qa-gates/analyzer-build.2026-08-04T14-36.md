# [P2-T5] Solution Analyzer Build — Final QC Pass 1

Timestamp: 2026-08-04T19-58

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`

EXIT_CODE: 0

Output Summary:

- `Build succeeded. 6 Warning(s) 0 Error(s)`. Elapsed 00:00:14.78. **Zero analyzer errors.**
- 36 `CoreCompile` targets executed, so this run genuinely recompiled the projects whose inputs changed
  (`SVGControl` and `SVGControl.Test` among them) rather than reporting an all-up-to-date no-op.

## Diagnostic inventory

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 5 | warning | (no code) `System.Reactive.PackagesConfigCheck.targets(31,5)` | `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj` |
| 1 | warning | `CS2002` | `UtilitiesCS.Test.csproj` |
| 0 | error | — | — |

`CS2002` text: `Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` — a pre-existing duplicate `<Compile>` include in `UtilitiesCS.Test.csproj`, out of scope for issue #418.

## Comparison against the `[P0-T7]` baseline

Baseline of record: `evidence/baseline/analyzer-build.2026-08-04T21-04.md` — **0 errors, 6 warnings.**

| Metric | Baseline `2026-08-04T21-04` | This run | Verdict |
|---|---|---|---|
| Errors | 0 | 0 | no worse |
| Warnings | 6 | 6 | no worse |
| Distinct warning codes | `CS2002` + code-less System.Reactive | `CS2002` + code-less System.Reactive | identical |
| `MSB3277` | 0 | 0 | identical |

**New diagnostics versus baseline: none.** The warning set is identical in count, code, text, and
emitting project. No `CSxxxx`, `CAxxxx`, or `IDExxxx` diagnostic appeared that was absent from the
baseline, and no diagnostic is attributable to any file in the Scope Lock — `SVGControl` and
`SVGControl.Test` each emitted zero warnings and zero errors.

Two baseline conditions correctly did not surface, consistent with the plan's Open Questions note:
`MSB3061` (Outlook file locks) and the `UtilitiesCS` nullable set appear only under `/t:Rebuild`, and
`Invoke-VSBuild.ps1` hardcodes `/t:Build`. No `/t:Rebuild` was run by this task.
