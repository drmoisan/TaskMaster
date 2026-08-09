---
name: missing-vsto-runtime-breaks-baseline-gates
description: HISTORICAL (2026-08-04) - absent VSTO Office Tools assemblies once broke the solution build and collapsed repo-wide coverage; NOT reproducing as of 2026-08-08, so always re-verify by building before citing this
metadata:
  type: project
---

> **RE-VERIFIED 2026-08-08 (issue #505 worktree) — DOES NOT REPRODUCE.** The two named
> assemblies (`Microsoft.Office.Tools.Common.v4.0.Utilities`,
> `Microsoft.Office.Tools.Outlook.v4.0.Utilities`) are still absent from the GAC, the VS
> install, and `Common Files\Microsoft Shared\VSTO` — yet
> `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU"` after
> `nuget restore TaskMaster.sln` completed **EXIT 0 with zero errors** and produced all
> 18 project outputs, including `TaskMaster.dll` and all **9** `*.Test.dll`. The
> `Microsoft.Office.Tools.*` (non-`v4.0.Utilities`) assemblies present in
> `C:\Windows\Microsoft.NET\assembly\GAC_MSIL` are apparently sufficient for
> `ThisAddIn.Designer.cs` to bind. **Never assert the CS0234/6-assembly failure below from
> memory — build first.** Treat the text that follows as a record of one historical
> environment state.

`msbuild TaskMaster.sln` (both the analyzer gate and the nullable gate) fails on this machine
with 4 `CS0234` errors in `TaskMaster/ThisAddIn.Designer.cs`, naming
`Microsoft.Office.Tools.Outlook.OutlookAddInBase`,
`Microsoft.Office.Tools.Ribbon.RibbonCollectionBase`, and
`Microsoft.Office.Tools.Outlook.FormRegionCollectionBase` (x2).

**Why:** two VSTO runtime assemblies cannot be located, reported as `MSB3245`:
`Microsoft.Office.Tools.Outlook.v4.0.Utilities` and
`Microsoft.Office.Tools.Common.v4.0.Utilities`, both `Version=10.0.0.0`,
`PublicKeyToken=b03f5f7f11d50a3a`. The Office Developer Tools for Visual Studio component is
not installed. This is an environment gap, not a source defect — the errors are entirely
confined to `TaskMaster/TaskMaster.csproj` and reproduce with a clean tree.

**How to apply:**

- Do NOT try to fix this inside a feature plan. It is out of scope for any feature and the
  source is correct; the missing piece is a Visual Studio installer component.
- Expect a plan acceptance clause of literal `EXIT_CODE: 0` for the analyzer or nullable
  solution build to be **unreachable** in this checkout. Report it to the orchestrator as a
  Phase 0 finding rather than improvising a fix. The relative measure
  (`New diagnostics vs baseline: 0`) against a recorded baseline is still meaningful and is
  the measure to use.
- Baseline figures captured 2026-08-04 (issue #418, clean `main`): analyzer gate
  `4 Error(s) / 44 Warning(s)`; nullable gate `5 Error(s) / 5 Warning(s)` (the same 4 `CS0234`
  plus one `CS8625` at `TaskMaster/AppGlobals/AppEvents.cs(44,30)` promoted by
  `TreatWarningsAsErrors`).

**Knock-on effect on coverage — this is the surprising part.** Because `TaskMaster.csproj`
never produces output, `TaskMaster.Test` and `UtilitiesCS.Test` produce **no**
`bin/Debug/*.Test.dll`, so `Invoke-MSTestWithCoverage.ps1 -SearchRoot .` discovers only **6**
test assemblies (QuickFiler, Tags, TaskTree, TaskVisualization, ToDoModel, VBFunctions). The
run still reports `Test Run Successful` with 0 failures, so nothing looks wrong — but
repo-wide Cobertura `line-rate` collapses to ~25.5% (vs the ~71% figure prior sessions
recorded) and the `UtilitiesCS` package reads ~10.7%. **Do not interpret that as a coverage
regression.** Always record the participating-assembly list next to any repo-wide coverage
number, and check for missing `bin/Debug` test DLLs before comparing against a historical
baseline. See [[project_dotnet_coverage_denominator_nondeterminism]] and
[[project_coverage_firstparty_denominator_method]] for the other denominator traps.
