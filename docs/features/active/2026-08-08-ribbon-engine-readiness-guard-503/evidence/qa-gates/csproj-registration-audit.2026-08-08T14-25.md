# csproj `<Compile Include>` Registration Audit — Issue #503 (P5-T9)

Timestamp: 2026-08-08T14-25

Commands (run from `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55` through `pwsh -NoProfile -File`):

```powershell
# 1. enumerate the added entries
Select-String -Path 'TaskMaster\TaskMaster.csproj' -Pattern 'Compile Include="Ribbon\\Engine|Compile Include="Ribbon\\RibbonController.EngineCommands|Compile Include="Ribbon\\RibbonViewer.EngineCommands'
Select-String -Path 'TaskMaster.Test\TaskMaster.Test.csproj' -Pattern 'Compile Include="Ribbon\\Engine'

# 2. prove every Compile Include in BOTH projects resolves to a file on disk
foreach ($proj in @('TaskMaster\TaskMaster.csproj','TaskMaster.Test\TaskMaster.Test.csproj')) {
    $dir = Split-Path -Parent $proj
    $xml = [xml](Get-Content -Raw -Path $proj)
    $ns  = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $ns.AddNamespace('m','http://schemas.microsoft.com/developer/msbuild/2003')
    $nodes = $xml.SelectNodes('//m:Compile[@Include]', $ns)
    ... Test-Path each Include relative to the project directory ...
}
```

EXIT_CODE: 0

## The six entries added to `TaskMaster\TaskMaster.csproj`

```xml
<Compile Include="Ribbon\EngineCommandCatalog.cs" />
<Compile Include="Ribbon\EngineCommandRefreshPlanner.cs" />
<Compile Include="Ribbon\EngineGatedCommandRunner.cs" />
<Compile Include="Ribbon\EngineReadinessGate.cs" />
<Compile Include="Ribbon\RibbonController.EngineCommands.cs" />
<Compile Include="Ribbon\RibbonViewer.EngineCommands.cs" />
```

Matches plan section 4.6 exactly — same six paths, same spelling.

## The four entries added to `TaskMaster.Test\TaskMaster.Test.csproj`

```xml
<Compile Include="Ribbon\EngineCommandCatalogTests.cs" />
<Compile Include="Ribbon\EngineCommandRefreshPlannerTests.cs" />
<Compile Include="Ribbon\EngineGatedCommandRunnerTests.cs" />
<Compile Include="Ribbon\EngineReadinessGateTests.cs" />
```

Matches plan section 4.6 exactly — same four paths, same spelling.

Ten entries total, as required.

## Every `Compile Include` path resolves to an existing file

| Project | `Compile Include` items | Items whose path is missing on disk |
|---|---|---|
| `TaskMaster\TaskMaster.csproj` | 48 | **0** |
| `TaskMaster.Test\TaskMaster.Test.csproj` | 48 | **0** |

Both projects are legacy non-SDK `packages.config` projects with no glob, so a `.cs` file absent from this list would silently fail to compile into the assembly, and conversely an item pointing at a missing file would break the build. Neither condition exists.

Independent corroboration: `git diff --numstat <MERGE_BASE>..HEAD` records `6	0	TaskMaster/TaskMaster.csproj` and `4	0	TaskMaster.Test/TaskMaster.Test.csproj` — six and four added lines respectively, with zero deletions, confirming that only the ten new entries were added and no existing entry was disturbed.

Binary outcome: **PASS**.
