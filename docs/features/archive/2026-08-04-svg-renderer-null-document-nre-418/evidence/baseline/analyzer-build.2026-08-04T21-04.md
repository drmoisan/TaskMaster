# [P0-T7] Baseline Analyzer State — re-capture on VSTO-enabled host

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T7]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)
MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, `18.8.2+ce25c0108 for .NET Framework`

## Command (plan-commanded, authoritative)

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

EXIT_CODE: 0

## Output Summary

Build succeeded. **0 errors, 6 warnings.** Elapsed 00:00:06.06.

The VSTO premise holds on this host: **zero `CS0234` and zero `MSB3245`** occurrences in any
project. The four `CS0234` errors in `TaskMaster/ThisAddIn.Designer.cs` and the `MSB3245`
reference-resolution failure recorded in `analyzer-build.2026-08-04T14-36.md` do not reproduce here.
`Microsoft.Office.Tools.Outlook.v4.0.Utilities.dll` and
`Microsoft.Office.Tools.Common.v4.0.Utilities.dll` are both present at
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\`.

### MSB3277 status at solution scope (explicit)

**MSB3277 count: 0.** Emitting projects: **none**.

`SVGControl.Test\SVGControl.Test.csproj` emitted one `MSB3277` for
`System.Runtime.CompilerServices.Unsafe` on the originating host. That pin was realigned by
PR #419 (`SVGControl.Test/packages.config` now pins
`System.Runtime.CompilerServices.Unsafe 6.1.2`), and the warning no longer appears. No corrective
action was taken or required.

### Diagnostic inventory — plan-commanded incremental run

| Count | Severity | Code | Emitting project |
|---|---|---|---|
| 5 | warning | (no code) `System.Reactive.PackagesConfigCheck.targets(31,5)` | `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj` |
| 1 | warning | `CS2002` | `UtilitiesCS.Test.csproj` |
| — | error | none | — |

Distinct diagnostic codes: `CS2002`, plus one code-less MSBuild warning from
`System.Reactive.PackagesConfigCheck.targets`. Zero errors of any code.

`CS2002` text: `Source file 'C:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times`.
This is a pre-existing duplicate `<Compile>` include in `UtilitiesCS.Test.csproj`, out of scope for
issue #418.

### Incrementality caveat and the supplementary full-recompile inventory

The plan-commanded run used MSBuild target `Build`. Only **1** `CoreCompile` target executed
(61 up-to-date / skipped-target notices), because build outputs from earlier work on this host were
newer than their inputs. Legacy non-SDK up-to-date checks are timestamp-based, not property-based,
so adding `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` does not by itself force a
recompile. An incremental run therefore cannot enumerate the analyzer diagnostics of projects that
were skipped, and would be an incomplete comparison basis for tasks P1-T6 and P2-T4.

A supplementary run with the **identical property set** and target `Rebuild` was executed solely to
produce a complete inventory. It is recorded here as supplementary evidence; it is not the plan
command and does not replace the result above.

Supplementary command:
```
MSBuild.exe TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m
```
Supplementary EXIT_CODE: 0 — Build succeeded, **0 errors, 8 warnings**, elapsed 00:00:15.29,
55 `CoreCompile` targets and 36 `csc` invocations (genuine full recompile of all 19 solution
projects, `SVGControl` and `SVGControl.Test` included).

Complete diagnostic inventory at full-recompile scope:

| Count | Severity | Code | Emitting project | Note |
|---|---|---|---|---|
| 5 | warning | (no code) | `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`, `UtilitiesCS.Test.csproj` | `System.Reactive.7.0.0` `PackagesConfigCheck.targets(31,5)`: packages.config unsupported by System.Reactive v7.0+ |
| 2 | warning | `MSB3061` | `TaskMaster.csproj` | `Microsoft.Common.CurrentVersion.targets(5954,5)`: cannot delete `TaskMaster\bin\Debug\x64\leptonica-1.82.0.dll` and `...\tesseract50.dll` — "The file is locked by: Microsoft Outlook (46608)". Environmental (Outlook running), CoreClean-only, non-fatal |
| 1 | warning | `CS2002` | `UtilitiesCS.Test.csproj` | duplicate `PercentageFormatterTests.cs` Compile include |
| 0 | error | — | — | — |

**Distinct diagnostic codes present in the baseline at full-recompile scope: `CS2002`, `MSB3061`,
and one code-less System.Reactive MSBuild warning. There are zero analyzer errors and zero
`CSxxxx` analyzer/code-style warnings other than `CS2002`.** `MSB3061` appears only during
`CoreClean` and only because Outlook holds two native Tesseract DLLs; it does not appear in a
non-cleaning `Build`.

Consequence for tasks P1-T6 and P2-T4: any `CSxxxx` or `CAxxxx`/`IDExxxx` diagnostic other than
`CS2002` that appears after the Phase 1 edits was **absent from this baseline** and must be treated
as newly introduced.
