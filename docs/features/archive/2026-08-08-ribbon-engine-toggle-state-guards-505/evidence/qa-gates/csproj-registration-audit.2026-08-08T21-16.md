# P4-T7 — csproj Registration Audit

Timestamp: 2026-08-08T21-16

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; Select-String -Path 'TaskMaster\TaskMaster.csproj','TaskMaster.Test\TaskMaster.Test.csproj' -Pattern 'EngineToggleCatalog|EngineToggleStateCoordinator|RibbonViewerEngineCallbackShapeTests'; 'TaskMaster\Ribbon\EngineToggleCatalog.cs','TaskMaster\Ribbon\EngineToggleStateCoordinator.cs','TaskMaster.Test\Ribbon\EngineToggleCatalogTests.cs','TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs','TaskMaster.Test\Ribbon\RibbonViewerEngineCallbackShapeTests.cs' | ForEach-Object { '{0}={1}' -f $_, (Test-Path $_) }"
```

Executed through a scratchpad `.ps1` so the path lists survive intact, with an added diff-derived
resolution check. `EngineToggleStateCoordinatorTests.Part2.cs` was **not** appended to the
`Test-Path` list: no Part2 split occurred, confirmed by `Test-Path` returning `False` for it.

EXIT_CODE: 0

## Output Summary

### Entries added to `TaskMaster\TaskMaster.csproj` — exactly two, per section 4.6

```xml
line 463:  <Compile Include="Ribbon\EngineToggleCatalog.cs" />
line 464:  <Compile Include="Ribbon\EngineToggleStateCoordinator.cs" />
```

Both were inserted into the `ItemGroup` that already contained
`<Compile Include="Ribbon\EngineCommandCatalog.cs" />`, preserving that group's alphabetical order
(they sit between `EngineReadinessGate.cs` and `RibbonController.cs`).

### Entries added to `TaskMaster.Test\TaskMaster.Test.csproj` — exactly three, per section 4.6

```xml
line 315:  <Compile Include="Ribbon\EngineToggleCatalogTests.cs" />
line 316:  <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.cs" />
line 319:  <Compile Include="Ribbon\RibbonViewerEngineCallbackShapeTests.cs" />
```

Three, not four: the Part2 split did not occur.

### Every added `Compile Include` path resolves to an existing file

The branch diff over the two `.csproj` files contains **5** added `<Compile Include>` entries — the
2 + 3 above and nothing else. Each resolves on disk under its own project root:

| Added entry | Project root | `Test-Path` |
|---|---|---|
| `Ribbon\EngineToggleCatalog.cs` | `TaskMaster` | **True** |
| `Ribbon\EngineToggleStateCoordinator.cs` | `TaskMaster` | **True** |
| `Ribbon\EngineToggleCatalogTests.cs` | `TaskMaster.Test` | **True** |
| `Ribbon\EngineToggleStateCoordinatorTests.cs` | `TaskMaster.Test` | **True** |
| `Ribbon\RibbonViewerEngineCallbackShapeTests.cs` | `TaskMaster.Test` | **True** |

Each entry resolves under exactly one root, with `False` under the other, so no entry is
mis-rooted. This registration is load-bearing: `TaskMaster.csproj` and `TaskMaster.Test.csproj` are
legacy non-SDK `packages.config` projects that enumerate every source file explicitly, so a new
`.cs` file without its entry would not compile into its assembly. The compile is confirmed
independently by the P2-T5 build (`csc.exe` invocations for both `TaskMaster.dll` and
`TaskMaster.Test.dll`) and by the 25 new seam tests executing.

Binary outcome: PASS.
