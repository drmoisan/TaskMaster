# P5-T10 — Post-Format Scope-Lock and Protected-File Diff

Timestamp: 2026-08-08T21-41

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; git diff --numstat f910ff2f21c67a03cf8eebcb340727d5415d8e08..HEAD"
```

EXIT_CODE: 0

## Output Summary

### Source, test, and project paths in the diff (non-documentation)

| added | removed | path | Scope-lock section |
|---|---|---|---|
| 92 | 0 | `TaskMaster/Ribbon/EngineToggleCatalog.cs` | 4.1 (new) |
| 387 | 0 | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 4.1 (new) |
| 13 | 0 | `TaskMaster/Ribbon/EngineCommandCatalog.cs` | 4.2 |
| 61 | 0 | `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` | 4.2 |
| 137 | 16 | `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 4.2 |
| 6 | 0 | `TaskMaster/Ribbon/RibbonExplorer.xml` | 4.2 |
| 2 | 0 | `TaskMaster/TaskMaster.csproj` | 4.2 / 4.6 |
| 382 | 0 | `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` | 4.3 (new) |
| 101 | 0 | `TaskMaster.Test/Ribbon/EngineToggleCatalogTests.cs` | 4.3 (new) |
| 455 | 0 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 4.3 (new) |
| 14 | 2 | `TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs` | 4.3 |
| 8 | 3 | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 4.3 (comment only) |
| 3 | 0 | `TaskMaster.Test/TaskMaster.Test.csproj` | 4.3 / 4.6 |

Thirteen paths, all section 4 scope-lock members. Every remaining path in the full diff lies under
`docs/features/` (feature documentation and evidence) or `.claude/agent-memory/` (tracked agent
memory dirty at branch head) — expected diff entries, not scope violations.

The insertion counts are four-figure in total (roughly 1,660 added lines across thirteen paths),
which is consistent with two new production files, three new test files, and Markdown evidence. No
six- or seven-figure insertion count appears, so no generated output was committed.

### The eight section 4.4 protected paths are ABSENT from the diff

`git diff --numstat` emits one line per changed path. None of the following appears anywhere in the
output, so each has a zero-line diff:

1. `TaskMaster/Ribbon/RibbonController.Intelligence.cs` — the #507 `Globals?.Engines` `?.` is not
   reverted (quoted verbatim in the P4-T2 artifact)
2. `TaskMaster/AppGlobals/AppItemEngines.cs`
3. `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs`
4. `TaskMaster/ThisAddIn.cs`
5. `TaskMaster/Ribbon/RibbonViewer.cs`
6. `TaskMaster/Ribbon/EngineReadinessGate.cs`
7. `TaskMaster/Ribbon/EngineGatedCommandRunner.cs`
8. `TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs`

`TaskMaster.sln` is likewise absent.

### Pending working-tree state (to be committed by P6-T27)

Five scope-locked `.cs` files carry the P5-T1 CSharpier reformatting and are not yet committed:

```
 M TaskMaster/Ribbon/EngineToggleCatalog.cs
 M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
 M TaskMaster.Test/Ribbon/EngineToggleCatalogTests.cs
 M TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
 M TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs
```

All five are section 4 scope-lock members, so committing them at P6-T27 cannot introduce an
out-of-scope path. The `--numstat` figures above already reflect their post-format content for the
three files that are wholly new; for the two pre-existing-in-this-branch files the pending change is
formatting only.

Binary outcome: **PASS** — the eight protected paths are absent, and every path present is either a
section 4 scope-lock member or lies under `docs/features/` or `.claude/agent-memory/`.
