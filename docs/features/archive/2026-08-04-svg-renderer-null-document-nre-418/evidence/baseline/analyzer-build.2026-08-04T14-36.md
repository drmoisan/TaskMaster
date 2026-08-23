# Baseline — .NET Analyzer Build (Issue #418)

Task: `[P0-T7]`
Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`

Timestamp: 2026-08-04T14-58

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

EXIT_CODE: 1

Output Summary: **Build FAILED** with `44 Warning(s)` and `4 Error(s)`, elapsed
`00:01:00.40`. All four errors are `CS0234` in a single file,
`TaskMaster/ThisAddIn.Designer.cs`, and all four are downstream of two unresolved VSTO
runtime assemblies reported as `MSB3245`
(`Microsoft.Office.Tools.Outlook.v4.0.Utilities` and
`Microsoft.Office.Tools.Common.v4.0.Utilities`, both `Version=10.0.0.0`,
`PublicKeyToken=b03f5f7f11d50a3a`). This is a pre-existing environment condition in this
checkout, not a condition introduced by issue #418, and it is recorded here without
remediation because Phase 0 is baseline capture only. `Invoke-VSBuild.ps1` additionally
emitted seven `[SVGControl.Test] Cannot resolve ...` package warnings, which corroborate the
`[P0-T10]` buildability finding.

## Error Detail (4 errors, all `CS0234`, all in `TaskMaster/TaskMaster.csproj`)

| File and position | Diagnostic |
| --- | --- |
| `TaskMaster/ThisAddIn.Designer.cs(18,76)` | `error CS0234: The type or namespace name 'OutlookAddInBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |
| `TaskMaster/ThisAddIn.Designer.cs(235,88)` | `error CS0234: The type or namespace name 'RibbonCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Ribbon'` |
| `TaskMaster/ThisAddIn.Designer.cs(257,93)` | `error CS0234: The type or namespace name 'FormRegionCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |
| `TaskMaster/ThisAddIn.Designer.cs(279,95)` | `error CS0234: The type or namespace name 'FormRegionCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |

Distinct error codes: `CS0234` only. Projects producing errors: `TaskMaster/TaskMaster.csproj`
only. No error came from `SVGControl`, `SVGControl.Test`, or any other project.

### Root cause of the four errors (reference resolution, not source)

```text
warning MSB3245: Could not resolve this reference. Could not locate the assembly
"Microsoft.Office.Tools.Outlook.v4.0.Utilities, Version=10.0.0.0, Culture=neutral,
PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL".

warning MSB3245: Could not resolve this reference. Could not locate the assembly
"Microsoft.Office.Tools.Common.v4.0.Utilities, Version=10.0.0.0, Culture=neutral,
PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL".
```

The VSTO "Office Developer Tools" runtime assemblies are not installed on this host. The four
`CS0234` diagnostics are the compiler consequence of those two unresolved references in the
VSTO designer-generated file.

## Warning Inventory (44 total per MSBuild; counts below are raw log occurrences)

| Code | Log occurrences | Meaning |
| --- | --- | --- |
| `CS0618` | 48 | Obsolete API use (pre-existing `IAsyncEnumerable` overloads) |
| `CS0108` | 8 | Member hides inherited member |
| `MSB3245` | 8 | Assembly reference could not be resolved (the two VSTO utilities above) |
| `CS0169` | 6 | Field never used |
| `CS8632` | 6 | Nullable annotation outside a `#nullable` annotations context |
| `CS0649` | 4 | Field never assigned |
| `CS0168` | 2 | Variable declared but never used |
| `CS4014` | 2 | Awaitable call not awaited |
| `MSB3327` | 2 | No code-signing certificate in the user certificate store (ClickOnce manifest) |
| `MSTEST0032` | 2 | Assertion condition known to be always true (`QuickFiler.Test`) |

Raw occurrence counts exceed the MSBuild `44 Warning(s)` total because MSBuild prints each
diagnostic once inline and again in the trailing summary block.

## `SVGControl.Test` Package-Resolution Warnings (baseline corroboration)

`Invoke-VSBuild.ps1` emitted seven warnings for `SVGControl.Test` even though the project is
not a solution member, because the script scans project files independently of the solution
graph:

```text
[SVGControl.Test] Cannot resolve Castle.Core.dll from Castle.Core.5.1.1
[SVGControl.Test] Cannot resolve FluentAssertions.dll from FluentAssertions.6.12.0
[SVGControl.Test] Cannot resolve Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll from MSTest.TestFramework.3.1.1
[SVGControl.Test] Cannot resolve Microsoft.VisualStudio.TestPlatform.TestFramework.dll from MSTest.TestFramework.3.1.1
[SVGControl.Test] Cannot resolve Moq.dll from Moq.4.20.69
[SVGControl.Test] Cannot resolve System.Runtime.CompilerServices.Unsafe.dll from System.Runtime.CompilerServices.Unsafe.6.0.0
[SVGControl.Test] Cannot resolve System.Threading.Tasks.Extensions.dll from System.Threading.Tasks.Extensions.4.5.4
```

These match the research artifact's section 8.3 finding and are recorded in full under task
`[P0-T10]`.

## Baseline Significance for Later Phases

Tasks `[P1-T6]` and `[P2-T4]` state an acceptance of `EXIT_CODE: 0` for this same command.
That acceptance is not currently reachable in this checkout for a reason wholly unrelated to
issue #418: the VSTO runtime assemblies are missing from the host. `[P1-T6]` measures
`New diagnostics vs baseline: 0`, which remains a meaningful and satisfiable comparison
against this recorded baseline of 4 errors / 44 warnings. The absolute `EXIT_CODE: 0`
condition is reported to the orchestrator as a Phase 0 finding.

Analyzer build log line count: available in the session scratchpad; the diagnostics above are
the complete error set and the complete distinct-code warning set.
