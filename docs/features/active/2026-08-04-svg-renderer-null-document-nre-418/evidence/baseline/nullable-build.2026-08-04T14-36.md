# Baseline — Nullable / Type-Check Build (Issue #418)

Task: `[P0-T8]`
Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`

Timestamp: 2026-08-04T15-01

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

EXIT_CODE: 1

Output Summary: **Build FAILED** with `5 Error(s)` and `5 Warning(s)`, elapsed `00:00:06.33`.
Error count: **5**. All five errors are confined to a single project,
`TaskMaster/TaskMaster.csproj`. Four are the same `CS0234` VSTO reference-resolution
failures already recorded in the `[P0-T7]` analyzer baseline; the fifth is one `CS8625`
nullable diagnostic in `TaskMaster/AppGlobals/AppEvents.cs`, promoted to an error by
`/p:TreatWarningsAsErrors=true`. No error came from `SVGControl`, `SVGControl.Test`, or any
other project. This is the pre-existing baseline state of this checkout; Phase 0 records it
without remediation.

## Error Detail (5 errors, all in `TaskMaster/TaskMaster.csproj`)

| File and position | Diagnostic |
| --- | --- |
| `TaskMaster/AppGlobals/AppEvents.cs(44,30)` | `error CS8625: Cannot convert null literal to non-nullable reference type.` |
| `TaskMaster/ThisAddIn.Designer.cs(18,76)` | `error CS0234: The type or namespace name 'OutlookAddInBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |
| `TaskMaster/ThisAddIn.Designer.cs(235,88)` | `error CS0234: The type or namespace name 'RibbonCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Ribbon'` |
| `TaskMaster/ThisAddIn.Designer.cs(257,93)` | `error CS0234: The type or namespace name 'FormRegionCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |
| `TaskMaster/ThisAddIn.Designer.cs(279,95)` | `error CS0234: The type or namespace name 'FormRegionCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |

Error codes by log occurrence: `CS0234` x8, `CS8625` x2 (each diagnostic is printed once
inline and once in the trailing summary, so the distinct set is 4 + 1 = 5, matching MSBuild's
`5 Error(s)`).

## Warning Detail (5 warnings)

| Code | Log occurrences | Meaning |
| --- | --- | --- |
| `MSB3245` | 8 | `Microsoft.Office.Tools.Outlook.v4.0.Utilities` and `Microsoft.Office.Tools.Common.v4.0.Utilities` (both `Version=10.0.0.0`, `PublicKeyToken=b03f5f7f11d50a3a`) could not be located |
| `MSB3327` | 2 | No code-signing certificate in the user certificate store (ClickOnce manifest) |

The two unresolved VSTO runtime assemblies are the root cause of the four `CS0234` errors.
The VSTO Office Developer Tools runtime is not installed on this host. This condition is
unrelated to issue #418.

## Incremental-Build Note (why elapsed time is 6.33 seconds)

This run followed the `[P0-T7]` analyzer build in the same session. Projects whose outputs
were already up to date were skipped, so their pre-existing nullable debt did not recompile
and therefore did not surface. `TaskMaster/TaskMaster.csproj` did recompile, because
`[P0-T7]` failed before producing its output, which is why its single `CS8625` appears here
and did not appear as an error in the analyzer baseline.

This matters for the `[P1-T7]` comparison. `[P1-T7]` measures `New errors vs baseline: 0`
against this recorded set. A comparison made from a materially different build state (for
example after a forced `-t:Rebuild`) would not be comparable to this baseline and would
surface a much larger pre-existing vendored-project error population. The `[P1-T7]`
comparison must be taken under the same incremental conditions.

## Baseline Significance for Later Phases

Tasks `[P1-T7]` and `[P2-T5]` state an acceptance of `EXIT_CODE: 0` and
`Output Summary: 0 errors` for this same command. That absolute condition is not reachable in
this checkout for a reason unrelated to issue #418 (the missing VSTO runtime assemblies plus
one pre-existing `CS8625`). The relative measure `New errors vs baseline: 0` against this
recorded set of 5 remains meaningful and satisfiable. The absolute condition is reported to
the orchestrator as a Phase 0 finding.
