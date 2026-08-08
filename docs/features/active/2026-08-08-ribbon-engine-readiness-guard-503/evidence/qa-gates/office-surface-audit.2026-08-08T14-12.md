# AC27 Architecture-Boundary / Office-Surface Audit — Issue #503 (P5-T3)

Timestamp: 2026-08-08T14-12

Commands (run from `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55`; `<FOUR>` denotes the four new decision files `TaskMaster/Ribbon/EngineCommandCatalog.cs`, `TaskMaster/Ribbon/EngineReadinessGate.cs`, `TaskMaster/Ribbon/EngineGatedCommandRunner.cs`, `TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs`):

```
grep -n "Microsoft\.Office\." <FOUR>
grep -nE "(^|[^a-zA-Z.])Office\." <FOUR>
grep -c "System.Diagnostics.CodeAnalysis" <FOUR>
git diff 003c5715055d7d1933db68a742531332756e30b2..HEAD -- '*.cs' | grep -E "^\+\s*\[.*ComVisible"
git diff 003c5715055d7d1933db68a742531332756e30b2..HEAD -- '*.cs' | grep -E "^\+" | grep -oE "[A-Za-z_]+\(Office\.IRibbonControl" | sed 's/(Office.*//' | sort -u   # added
git diff 003c5715055d7d1933db68a742531332756e30b2..HEAD -- '*.cs' | grep -E "^-" | grep -oE "[A-Za-z_]+\(Office\.IRibbonControl" | sed 's/(Office.*//' | sort -u   # removed
comm -23 added removed
```

EXIT_CODE: 0 (all commands executed; individual grep exit codes recorded per fact below)

## Fact 1 — the four new decision files contain no Office surface

`grep -nE "(^|[^a-zA-Z.])Office\."` over the four files returns **no lines** (exit 1). There is no `Office.` type reference of any kind.

`grep -n "Microsoft\.Office\."` returns exactly four lines, all inside XML documentation comments asserting the absence:

```
TaskMaster/Ribbon/EngineCommandCatalog.cs:21:    /// decision logic, contains no COM and no <c>Microsoft.Office.*</c> reference, and is fully
TaskMaster/Ribbon/EngineReadinessGate.cs:26:    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is fully
TaskMaster/Ribbon/EngineGatedCommandRunner.cs:28:    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is fully
TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs:23:    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is fully
```

There is **zero** `using Microsoft.Office.*` and **zero** `using Microsoft.Office.Interop.Outlook` directive in any of the four. Their complete using sets are:

| File | Using directives |
|---|---|
| `EngineCommandCatalog.cs` | `System`, `System.Collections.Generic`, `System.Collections.ObjectModel` |
| `EngineReadinessGate.cs` | `System`, `UtilitiesCS` |
| `EngineGatedCommandRunner.cs` | `System`, `System.Globalization`, `System.Threading.Tasks` |
| `EngineCommandRefreshPlanner.cs` | `System` |

This satisfies `.claude/rules/architecture-boundaries.md` rules 1, 2, and 8: the decision logic is host-neutral and would port unchanged to an Office.js command surface.

## Fact 2 — exactly one new `Microsoft.Office.*`-typed member in the branch diff

Net new Office-typed member declarations, computed as (added member names matching `<name>(Office.IRibbonControl`) minus (removed member names matching the same pattern):

```
EngineCommand_GetEnabled
GetEnabled
```

`GetEnabled` is **not** a member. It is a single XML documentation-comment occurrence naming the Office-required signature shape:

```
+        /// <c>public bool GetEnabled(Office.IRibbonControl control)</c>.
```

The net new member set is therefore exactly:

```
public bool EngineCommand_GetEnabled(Office.IRibbonControl control)
```

declared in `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` on the pre-existing `[ComVisible(true)] [ExcludeFromCodeCoverage] RibbonViewer`.

The 26 other `Office.IRibbonControl`-typed members that appear as `+` lines in the raw diff (`ClearSpam_Click`, `TrainSpam_Click`, `TrainHam_Click`, `TestSpam_Click`, `TestSpamVerbose_Click`, `SpamMetrics_Click`, `SpamInvestigateErrors_Click`, `SpamBayesEnabled_Click`, `SpamBayesEnabled_GetPressed`, `SpamSaveNetwork_Click`, `SpamSaveLocal_Click`, `GetSaveLocation_Click`, `SpamFolderSettings_Click`, `TriageSelection_Click`, `TriageSetA_Click`, `TriageSetB_Click`, `TriageSetC_Click`, `ClearTriage_Click`, `ResetTriage_Click`, `SetPrecision_Click`, `FilterViewer_Click`, `TriageEnabled_Click`, `TriageEnabled_GetPressed`, `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`, `TriageGetSaveLocation_Click`) each have a matching `-` line from `RibbonViewer.cs`: they are the P3-T4 relocation, not new members. The `comm -23` subtraction removes them, which is why they do not appear in the net set.

## Fact 3 — the branch diff adds zero new `[ComVisible(true)]` attributes

`git diff <MERGE_BASE>..HEAD -- '*.cs' | grep -E "^\+\s*\[.*ComVisible"` returns **no lines** (exit 1).

The only added line containing the string `ComVisible` is an XML documentation comment in `RibbonViewer.EngineCommands.cs`:

```
+    /// Thin COM/VSTO glue only. The <c>[ComVisible(true)]</c> and <c>[ExcludeFromCodeCoverage]</c>
```

`TaskMaster\Ribbon\RibbonViewer.cs` still declares exactly one `ComVisible` attribute (verified: `grep -c "ComVisible" TaskMaster/Ribbon/RibbonViewer.cs` returns 1). Making the class `partial` added no second attribute and introduced no new COM-visible type, satisfying architecture-boundary rule 3.

Binary outcome: **PASS** on all three facts.
