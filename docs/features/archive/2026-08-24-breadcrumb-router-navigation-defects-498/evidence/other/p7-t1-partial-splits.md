# P7-T1 — Residual 500-Line Sweep of Owned `.cs` Files

Timestamp: 2026-08-26T10-54

Command: `pwsh -NoProfile -Command '$files = @(...); foreach ($p in $files) { "{0}|{1}" -f $p, (Get-Content -LiteralPath $p).Count }; "EXIT_CODE: $LASTEXITCODE"'` followed by the analyzer Rebuild recipe `pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Rebuild /m "/p:Configuration=Debug" "/p:Platform=Any CPU" "/p:EnableNETAnalyzers=true" "/p:EnforceCodeStyleInBuild=true"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**No residual split was required.** Every owned `.cs` file written by this plan measures at or under
the 500-line limit. `P7-T1` therefore created ZERO new partial-class siblings and made ZERO project-file
edits. The analyzer Rebuild recipe returned `EXIT_CODE: 0` with `5 Warning(s), 0 Error(s)`.

### A. Post-task line counts — every owned file measured

Measured with `(Get-Content -LiteralPath $path).Count` in this execution worktree at HEAD `ee3c51e8`.

| # | File | Lines | Headroom to 500 | Over limit? |
|---:|---|---:|---:|---|
| 1 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 310 | 190 | no |
| 2 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 204 | 296 | no |
| 3 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 211 | 289 | no |
| 4 | `QuickFiler/Controllers/KeyboardHandler.cs` | 414 | 86 | no |
| 5 | `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 141 | 359 | no |
| 6 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 248 | 252 | no |
| 7 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | 384 | 116 | no |
| 8 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 | 139 | no |
| 9 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 489 | 11 | no |
| 10 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 462 | 38 | no |
| 11 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` | 368 | 132 | no |
| 12 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 462 | 38 | no |
| 13 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` | 261 | 239 | no |
| 14 | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | 479 | 21 | no |
| 15 | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | 495 | 5 | no |
| 16 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` | 379 | 121 | no |
| 17 | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` | 395 | 105 | no |

`QuickFiler/Resources/FolderBreadcrumb.html` measures **490** lines. It is not a `.cs` file, is outside
this task's sweep, and the plan states it cannot be split; it is recorded here for completeness and is
verified against the limit by `P7-T2`.

The two tightest owned files are `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`
(495, five lines of headroom) and `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`
(489, eleven lines of headroom). Neither exceeds the limit, so neither is split here.

### B. New partial siblings — created by earlier phases, not by this task

`P7-T1` is a RESIDUAL sweep. The mandatory split of `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
was performed by `P1-T2` and was not redone. Three further siblings were created by earlier tasks. All
four already carry a `Compile Include` entry placed IMMEDIATELY ADJACENT to the entry for the file it
splits, verified in this task:

| Sibling | Created by | Project file | `Compile Include` line | Adjacent to |
|---|---|---|---:|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | `P1-T2` | `QuickFiler/QuickFiler.csproj` | 292 | `Controllers\BreadcrumbBridgeRouter.cs` at 290 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | Phase 6 | `QuickFiler/QuickFiler.csproj` | 291 | `Controllers\BreadcrumbBridgeRouter.cs` at 290 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs` | `P2-T1` | `QuickFiler.Test/QuickFiler.Test.csproj` | 59 | `Controllers\BreadcrumbBridgeRouterQueueTests.cs` at 58 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` | Phase 6 | `QuickFiler.Test/QuickFiler.Test.csproj` | 61 | `Controllers\BreadcrumbBridgeRouterTests.cs` at 60 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | Phase 6 | `UtilitiesCS/UtilitiesCS.csproj` | 638 | `OutlookObjects\Folder\BreadcrumbStateModel.cs` at 637 |

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` required no edit, because no `UtilitiesCS.Test` file needed
splitting.

### C. Analyzer Rebuild gate

The analyzer Rebuild recipe was run over the finished tree after the sweep:

- `EXIT_CODE: 0`
- MSBuild summary: `5 Warning(s), 0 Error(s)`, `Build succeeded.`, time elapsed 00:00:21.21
- All five warnings are the identical uncoded `System.Reactive` packages.config advisory emitted from
  the gitignored `packages/` directory on `QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS` and
  `UtilitiesCS.Test`. This is exactly the `P0-T13` baseline (0 errors, 5 warnings, same five projects).
- The command used `/t:Rebuild`, not `/t:Build`, and did not contain `/p:Nullable=enable`.

No `error CS0006` was observed: the `packages/Meziantou.Analyzer.3.0.156/` and
`packages/Roslynator.Analyzers.4.16.0/` provisioning recorded in `p0-t13-analyzer-rebuild.md` remains in
place, and `git status --porcelain -- packages` is empty, so the `P7-T3` ownership gate is unaffected.
