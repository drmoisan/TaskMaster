# AC15 Zero-Line Diff Re-Verification (Post-Format) — Issue #503 (P6-T10)

Timestamp: 2026-08-08T14-59

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git diff --numstat 003c5715055d7d1933db68a742531332756e30b2..HEAD"
```

Corroborating commands (the CSharpier format pass and the three nullable fixes are not yet committed at this point; P7-T32 commits them, so the working tree must be audited as well as the commit range):
```
git status --porcelain | grep -E "\.(cs|csproj|xml|sln)$"
{ git diff --numstat <MERGE_BASE>..HEAD; git status --porcelain; } | grep -E "AppItemEngines\.cs|IAppItemEngines\.cs|ApplicationGlobals\.cs"
```

EXIT_CODE: 0

## AC15 protected-path assertion

The combined search over **both** the `<MERGE_BASE>..HEAD` diff **and** the uncommitted working tree returns **no lines** (grep exit 1) for any of the three protected paths:

- `TaskMaster/AppGlobals/AppItemEngines.cs` — **absent from the diff and from the working tree**
- `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs` — **absent from the diff and from the working tree**
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` — **absent from the diff and from the working tree**

Each therefore still has a zero-line diff against the merge-base after the final CSharpier format pass. The section 3 rule 5 scope guard held: because `csharpier format` was never invoked repo-wide and never received either protected path in its argument list, the formatter could not have rewritten them.

## Scope-lock assertion

Non-documentation entries in `git diff --numstat <MERGE_BASE>..HEAD`:

```
116	0	TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs
52	0	TaskMaster.Test/Ribbon/EngineCommandRefreshPlannerTests.cs
344	0	TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs
221	0	TaskMaster.Test/Ribbon/EngineReadinessGateTests.cs
148	0	TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs
4	0	TaskMaster.Test/TaskMaster.Test.csproj
89	0	TaskMaster/Ribbon/EngineCommandCatalog.cs
58	0	TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs
133	0	TaskMaster/Ribbon/EngineGatedCommandRunner.cs
103	0	TaskMaster/Ribbon/EngineReadinessGate.cs
97	0	TaskMaster/Ribbon/RibbonController.EngineCommands.cs
23	3	TaskMaster/Ribbon/RibbonExplorer.xml
202	0	TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs
1	100	TaskMaster/Ribbon/RibbonViewer.cs
6	0	TaskMaster/TaskMaster.csproj
7	0	TaskMaster/ThisAddIn.cs
```

Uncommitted source modifications in the working tree (all produced by the P6-T1 format pass and the three P6-T5 nullable fixes; committed by P7-T32):

```
 M TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs
 M TaskMaster.Test/Ribbon/EngineCommandRefreshPlannerTests.cs
 M TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs
 M TaskMaster.Test/Ribbon/EngineReadinessGateTests.cs
 M TaskMaster/Ribbon/EngineCommandCatalog.cs
 M TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs
 M TaskMaster/Ribbon/EngineGatedCommandRunner.cs
 M TaskMaster/Ribbon/EngineReadinessGate.cs
 M TaskMaster/Ribbon/RibbonController.EngineCommands.cs
 M TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs
```

Every path appearing in either list is a member of the plan's section 4 scope lock. Every remaining path in the full `--numstat` output (omitted above for brevity, unchanged from the P5-T1 artifact) lies under `docs/features/` or `.claude/agent-memory/` — documentation and evidence, which are expected diff entries and are not source-scope violations.

Binary outcome: **PASS** — the three protected paths are absent from both the commit range and the working tree, and no `.cs`, `.csproj`, `.xml`, or `.sln` path outside the section 4 scope lock appears in either.
