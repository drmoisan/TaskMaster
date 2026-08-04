# Prerequisite Analyzer Build — Solution Gate After SVGControl.Test Joins (Issue #418, task P1-T6)

Timestamp: 2026-08-04T18-20

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

EXIT_CODE: 1

Per the AC-6 amendment recorded in `issue.md` (human-interaction requirement H-3, resolved by
`scope_change`), the absolute `EXIT_CODE: 0` acceptance is not reachable in this checkout: the
VSTO runtime assemblies `Microsoft.Office.Tools.Outlook.v4.0.Utilities` and
`Microsoft.Office.Tools.Common.v4.0.Utilities` are not installed. The governing measure is the
relative one. `EXIT_CODE: 1` reproduces the Phase 0 baseline exactly at the error level.

Output Summary: **Build FAILED** with `4 Error(s)` and `9 Warning(s)`, elapsed `00:00:02.22`.

- **New analyzer diagnostics vs baseline: 0.** No new `CS`, `CA`, `S`, `MSTEST`, or other
  analyzer rule ID appeared. The four errors are byte-for-byte the four `CS0234` diagnostics
  recorded in `evidence/baseline/analyzer-build.2026-08-04T14-36.md`, all in
  `TaskMaster/ThisAddIn.Designer.cs`, all in `TaskMaster/TaskMaster.csproj`. `SVGControl` and
  `SVGControl.Test` produced zero errors and zero analyzer warnings.
- **New MSBuild (non-analyzer) diagnostics vs baseline: 1.** One `MSB3277` reference-conflict
  warning appeared that is absent from the Phase 0 baseline warning inventory. See the
  attribution section below. This is the finding that triggers the `SCOPE_EXCEEDED` stop
  clause in the task text.

Files edited for remediation: none

## Error set (4 errors — identical to the Phase 0 baseline)

| File and position | Diagnostic |
| --- | --- |
| `TaskMaster/ThisAddIn.Designer.cs(18,76)` | `error CS0234: The type or namespace name 'OutlookAddInBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |
| `TaskMaster/ThisAddIn.Designer.cs(235,88)` | `error CS0234: The type or namespace name 'RibbonCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Ribbon'` |
| `TaskMaster/ThisAddIn.Designer.cs(257,93)` | `error CS0234: The type or namespace name 'FormRegionCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |
| `TaskMaster/ThisAddIn.Designer.cs(279,95)` | `error CS0234: The type or namespace name 'FormRegionCollectionBase' does not exist in the namespace 'Microsoft.Office.Tools.Outlook'` |

Distinct error codes: `CS0234` only. Projects producing errors: `TaskMaster/TaskMaster.csproj`
only. No error came from `SVGControl` or `SVGControl.Test`.

## Warning set (9 warnings)

| Code | Warnings | Present in Phase 0 baseline | Emitting project(s) |
| --- | --- | --- | --- |
| `CS8632` | 3 | yes (baseline listed `CS8632`) | `TaskMaster/TaskMaster.csproj` (`AppGlobals/ApplicationGlobals.cs(251,57)`, `AppGlobals/EngineInitTimingProbe.cs(55,61)`, `AppGlobals/EngineInitTimingProbe.cs(57,57)`) |
| `MSB3245` | 4 | yes | `TaskMaster/TaskMaster.csproj` (the two VSTO utility assemblies) |
| `MSB3327` | 1 | yes | `TaskMaster/TaskMaster.csproj` (no ClickOnce code-signing certificate) |
| `MSB3277` | 1 | **no — NEW** | `SVGControl.Test/SVGControl.Test.csproj` (sole emitter) |

The total warning count is lower than the baseline's 44 because this run was incremental and
most projects were already up to date, so their pre-existing warnings did not recompile. The
comparison that matters is the per-code one above, not the aggregate.

## Attribution of the new `MSB3277`

```text
warning MSB3277: Found conflicts between different versions of
"System.Runtime.CompilerServices.Unsafe" that could not be resolved.
warning MSB3277: There was a conflict between
"System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, ..., PublicKeyToken=b03f5f7f11d50a3a" and
"System.Runtime.CompilerServices.Unsafe, Version=6.0.3.0, ..., PublicKeyToken=b03f5f7f11d50a3a".
"...Version=6.0.0.0..." was chosen because it was primary and "...Version=6.0.3.0..." was not.
```

Grep of the full build log confirms the only `[<project>.csproj]` tag attached to any
`MSB3277` line is `SVGControl.Test\SVGControl.Test.csproj`, and the only conflicting simple
name is `System.Runtime.CompilerServices.Unsafe`.

Root cause is a package pin divergence between the test project and the project it references:

| Project | `packages.config` pin | `<Reference>` `Version=` | HintPath |
| --- | --- | --- | --- |
| `SVGControl/SVGControl.csproj` | `System.Runtime.CompilerServices.Unsafe 6.1.2` | `6.0.3.0` | `..\packages\System.Runtime.CompilerServices.Unsafe.6.1.2\lib\net462\...` |
| `SVGControl.Test/SVGControl.Test.csproj` | `System.Runtime.CompilerServices.Unsafe 6.0.0` | `6.0.0.0` | `..\packages\System.Runtime.CompilerServices.Unsafe.6.0.0\lib\net461\...` |

`SVGControl/bin/Debug/System.Runtime.CompilerServices.Unsafe.dll` was verified on disk as
assembly version `6.0.3.0`. `SVGControl.Test` flows that file in through its `ProjectReference`
copy-local set while separately declaring a primary reference to `6.0.0.0`, so
`ResolveAssemblyReferences` reports an unresolvable conflict.

The divergence pre-exists in `SVGControl.Test/SVGControl.Test.csproj`; it becomes observable
only now, because task P1-T1 made the project a solution member and it therefore builds for the
first time. It is a direct and unavoidable consequence of delivering AC-9.

## `SCOPE_EXCEEDED` determination

Task P1-T6 restricts remediation to "`SVGControl.Test`-owned files only, restricted to the
Scope Lock's pre-existing-`SVGControl.Test`-files list" — that list contains only eight `.cs`
files (`Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs`,
`Resources.Designer.cs`, `Properties/AssemblyInfo.cs`, `GetRelativePath_Test.cs`,
`RelativePathCoverageTests.cs`).

`MSB3277` is an MSBuild `ResolveAssemblyReferences` diagnostic. It cannot be cleared by any
edit to a `.cs` file. Clearing it requires editing `SVGControl.Test/packages.config` and
`SVGControl.Test/SVGControl.Test.csproj`, both of which are outside the eight-file list.

Status: **`SCOPE_EXCEEDED` reported to the orchestrator.** Task P1-T6 is left unchecked pending
an orchestrator decision. The proposed minimal remediation, and the alternative of accepting
the warning, are stated in the executor's report.

## Log location

Full build log retained for this session at
`<scratchpad>/p1t6-analyzer.log` (session-scoped, not committed).
