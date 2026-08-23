# CSharpier Format — Issue #503 (P6-T1)

Timestamp: 2026-08-08T14-45

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' format TaskMaster\Ribbon\EngineCommandCatalog.cs TaskMaster\Ribbon\EngineReadinessGate.cs TaskMaster\Ribbon\EngineGatedCommandRunner.cs TaskMaster\Ribbon\EngineCommandRefreshPlanner.cs TaskMaster\Ribbon\RibbonController.EngineCommands.cs TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs TaskMaster\Ribbon\RibbonViewer.cs TaskMaster\ThisAddIn.cs TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs TaskMaster.Test\Ribbon\EngineReadinessGateTests.cs TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs TaskMaster.Test\Ribbon\EngineCommandRefreshPlannerTests.cs TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs; Write-Host \"EXIT_CODE=$LASTEXITCODE\""
```

EXIT_CODE: 0

## Output Summary

```
Formatted 13 files in 2621ms.
EXIT_CODE=0
```

Exactly the thirteen scope-locked `.cs` paths from plan section 4.5 were passed. `TaskMaster\AppGlobals\AppItemEngines.cs` and `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` do **not** appear in the argument list, per section 3 rule 5. The mutating pass was never invoked repo-wide. `csharpier pipe-files` was not used.

**This run rewrote nothing.** File-content MD5 values were captured immediately before and immediately after the invocation and are byte-identical for all sixteen touched source paths, proving the format step is idempotent and that this pass begins from a formatter-stable tree.

## Phase 6 restart history (recorded for the P6-T9 single-pass criterion)

The Phase 6 loop was entered three times. Only the third is the recorded clean pass.

| Attempt | P6-T1 outcome | Why the phase restarted |
|---|---|---|
| 1 | Rewrote 10 of 13 files | P6-T1 mutated files on disk, so the loop restarted rather than continued. |
| 2 | Rewrote 0 files, but P6-T5 verification found three nullable diagnostics in authored code | Three minimal source fixes were applied (see below), which is a file change, so the loop restarted. |
| 3 | **Rewrote 0 files** | None — this is the recorded clean pass. |

### The three source fixes applied between attempt 2 and attempt 3

The P6-T5 command as written (`/t:Build`) reported `EXIT_CODE: 0`, but MSBuild's up-to-date check skips `CoreCompile` when only `/p:` values change, so that result did not prove the new code is nullable-clean. A forced `/t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` over `TaskMaster.csproj` was therefore run as verification. It surfaced 223 errors, of which exactly **three** originated in code authored by this change:

| File | Diagnostic | Fix |
|---|---|---|
| `TaskMaster\Ribbon\EngineGatedCommandRunner.cs` | `CS8604` — possible null `controlId` argument to `EngineCommandCatalog.TryGetEngineName` | `BuildNotReadyMessage` now passes the already-non-null `renderedControlId` instead of the raw `controlId`. Behaviour is identical: `"(null)"` is not a catalog key, so it still resolves to `"(unmapped)"`. |
| `TaskMaster\Ribbon\RibbonController.EngineCommands.cs` | `CS8603` — possible null reference return from `() => Globals?.Engines` | Null-forgiving operator added (`() => Globals?.Engines!`) with a comment recording that a null result is a supported value the gate treats as "not ready". |
| `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` | `CS8604` — possible null `control?.Id` argument to `IsEngineCommandEnabled` | Null-forgiving operator added (`control?.Id!`) with a comment recording that a null id is a supported input yielding `false`. |

After the fixes the same forced rebuild reports 220 errors and **zero** in any of the six new production files. The residual 220 are pre-existing nullable debt in files this change does not touch (`AppOlObjects.cs` 58, `AppAutoFileObjects.cs` 52, `AppToDoObjects.cs` 48, `AppOlObjects.FolderTreeService.cs` 48, `AppItemEngines.cs` 18, and others), and are recorded in `<FEATURE>\evidence\qa-gates\msbuild-nullable.<TS>.md`.

None of the three fixes changes runtime behaviour; each only records an already-documented nullability contract for the compiler.

## Source-state fingerprint at the start of the recorded clean pass

```
03e959b34fee6b3c4357148f762a49b9  TaskMaster/Ribbon/EngineCommandCatalog.cs
cdcb45ca79029ea502105042111baef9  TaskMaster/Ribbon/EngineReadinessGate.cs
add3305fcb4aab807fe5935493eea6fc  TaskMaster/Ribbon/EngineGatedCommandRunner.cs
d7ddac56b3474268f1602ced3ad9e4c3  TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs
57f643e929497a643f554a2b7699c177  TaskMaster/Ribbon/RibbonController.EngineCommands.cs
9990f963ac3f09a3d2917bb152fe4b23  TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs
388f1680c66443eb9e8697482ba81a1d  TaskMaster/Ribbon/RibbonViewer.cs
5eb04850a8edb4df30595f3ad374d5b9  TaskMaster/ThisAddIn.cs
e6081bfcc4a853be11e549024cbbbbe5  TaskMaster/Ribbon/RibbonExplorer.xml
1751fa6df7979bbc3f7a234c4993c0f5  TaskMaster/TaskMaster.csproj
fd30be9c1df80560ecfbd099d38a7060  TaskMaster.Test/TaskMaster.Test.csproj
2eae440dd29f188dccca61dd27896550  TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs
c0545768fa69282e50526990428f1a97  TaskMaster.Test/Ribbon/EngineReadinessGateTests.cs
25efd8b02f36fc87e5bf78426040a0a1  TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs
18c90184f94643ef4bcbb2832be9acd4  TaskMaster.Test/Ribbon/EngineCommandRefreshPlannerTests.cs
9b4c453318e284c445122d4953ac0135  TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs
```

These MD5 values are re-verified at P6-T9 to prove no `.cs`, `.csproj`, or `.xml` file changed between P6-T1 and P6-T6.
