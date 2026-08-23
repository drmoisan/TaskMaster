# Merge-Base Line Counts of Scope-Locked Paths — Issue #503 (P0-T11)

**ADVISORY.** The authoritative 500-line audit is P6-T3, measured after the final CSharpier format pass. These counts are recorded for the audit trail only.

Timestamp: 2026-08-08T13-13

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; 'TaskMaster\Ribbon\RibbonViewer.cs','TaskMaster\Ribbon\RibbonExplorer.xml','TaskMaster\ThisAddIn.cs','TaskMaster\Ribbon\RibbonController.Intelligence.cs','TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs','TaskMaster\AppGlobals\AppItemEngines.cs','UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs' | ForEach-Object { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines }"
```

EXIT_CODE: 0

## Output Summary — verbatim command output

```
TaskMaster\Ribbon\RibbonViewer.cs=363
TaskMaster\Ribbon\RibbonExplorer.xml=519
TaskMaster\ThisAddIn.cs=266
TaskMaster\Ribbon\RibbonController.Intelligence.cs=360
TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs=145
TaskMaster\AppGlobals\AppItemEngines.cs=263
UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs=16
```

## Measurement-method note (recorded so P6-T3 is not misread)

`Measure-Object -Line` sums the line count of each input string and contributes **zero** for an empty string, so blank lines are not counted. The command therefore reports a **blank-line-excluding** count, which is lower than the physical line count of the file.

Cross-check with physical line counts (`wc -l`), which is the measure the 500-line cap in `.claude/rules/general-code-change.md` is naturally read against:

| Path | Command value (blank-excluding) | Physical lines | Plan-stated merge-base value |
|---|---|---|---|
| `TaskMaster\Ribbon\RibbonViewer.cs` | 363 | **487** | 487 |
| `TaskMaster\Ribbon\RibbonExplorer.xml` | 519 | **519** | 519 |
| `TaskMaster\ThisAddIn.cs` | 266 | **300** | 300 |
| `TaskMaster\Ribbon\RibbonController.Intelligence.cs` | 360 | **412** | 412 |
| `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` | 145 | **161** | 161 |
| `TaskMaster\AppGlobals\AppItemEngines.cs` | 263 | **286** | 286 |
| `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` | 16 | **18** | 18 |

The physical counts reproduce every merge-base figure stated in the plan and the research artifact exactly. `RibbonExplorer.xml` matches under both measures because it contains no blank lines.

Consequence for P6-T3: the P6-T3 command (which uses the same `Measure-Object -Line` expression) will be executed verbatim as written, and its verbatim output recorded; the 500-line cap will additionally be evaluated against the **physical** line count, which is the stricter of the two measures. Using the stricter measure cannot weaken the AC25 gate.
