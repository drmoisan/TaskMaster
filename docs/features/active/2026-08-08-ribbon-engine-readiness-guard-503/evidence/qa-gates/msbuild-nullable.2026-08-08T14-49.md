# MSBuild Nullable / Type-Check Gate — Issue #503 (P6-T5)

Timestamp: 2026-08-08T14-49

Command (exactly as specified by P6-T5):
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""
```

EXIT_CODE: **0**

## Output Summary

- Result: `Build succeeded.`
- Error count: **0**
- Warning count: **5** (the pre-existing System.Reactive `packages.config` notices recorded in the P0-T8 baseline; they are emitted by an MSBuild `.targets` file rather than the compiler, so `/p:TreatWarningsAsErrors=true` does not promote them)
- Zero `CS86xx` nullable-flow diagnostics
- Elapsed: 00:00:01.81

Baseline comparison: the P0-T8 merge-base run of the same command reported `Build succeeded.`, 0 errors, 5 warnings, 00:00:01.65. The gate result is unchanged by this work.

## Supplementary verification — forced recompile

MSBuild's up-to-date check does not re-run `CoreCompile` when only `/p:` values change, so the `/t:Build` form above can report success without actually type-checking under `Nullable=enable`. To confirm the criterion holds substantively rather than vacuously, a forced recompile of the two projects this change touches was run as verification (not as a substitute for the P6-T5 command, which was executed and recorded above):

```
MSBuild.exe TaskMaster\TaskMaster.csproj      /t:Rebuild /p:Configuration=Debug /p:Platform='AnyCPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false
MSBuild.exe TaskMaster.Test\TaskMaster.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform='AnyCPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false
```

### Result of the forced recompile

| Measure | Before the three fixes | After the three fixes |
|---|---|---|
| Total errors in `TaskMaster.csproj` | 223 | **220** |
| Errors originating in the six new/changed files of this change | **3** | **0** |

**All six new production files are nullable-clean** under `/p:Nullable=enable /p:TreatWarningsAsErrors=true`:

- `TaskMaster\Ribbon\EngineCommandCatalog.cs`
- `TaskMaster\Ribbon\EngineReadinessGate.cs`
- `TaskMaster\Ribbon\EngineGatedCommandRunner.cs`
- `TaskMaster\Ribbon\EngineCommandRefreshPlanner.cs`
- `TaskMaster\Ribbon\RibbonController.EngineCommands.cs`
- `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs`

This satisfies the spec constraint "New files must be nullable-clean under that global override."

The three fixes are documented in `<FEATURE>\evidence\qa-gates\csharpier-format.2026-08-08T14-30.md` under "Phase 6 restart history". None changes runtime behaviour.

### Pre-existing nullable debt (recorded, out of scope)

The residual **220** errors are entirely pre-existing debt in files this change does not touch. Distribution by file (top entries):

| File | Errors |
|---|---|
| `TaskMaster\AppGlobals\AppOlObjects.cs` | 58 |
| `TaskMaster\AppGlobals\AppAutoFileObjects.cs` | 52 |
| `TaskMaster\AppGlobals\AppToDoObjects.cs` | 48 |
| `TaskMaster\AppGlobals\AppOlObjects.FolderTreeService.cs` | 48 |
| `TaskMaster\AppGlobals\AppStagingFilenames.cs` | 40 |
| `TaskMaster\AppGlobals\ApplicationGlobals.cs` | 40 |
| `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs` | 20 |
| `TaskMaster\AppGlobals\AppItemEngines.cs` | 18 |
| `TaskMaster\Ribbon\TryFunctionalityInConstruction.cs` | 16 |
| `TaskMaster\Ribbon\RibbonController.cs` | 14 |
| `TaskMaster\Ribbon\RibbonViewer.cs` | 12 |
| `TaskMaster\Ribbon\RibbonController.Intelligence.cs` | 12 |

(Error counts are double-counted by MSBuild, which prints each diagnostic once inline and once in the summary; the relative distribution is what matters.)

The `TaskMaster\Ribbon\RibbonViewer.cs` diagnostics are at merge-base constructs (the two constructors at lines 35 and 42, `_controller = null;` at line 47, and `return null;` at line 81), none of which this change authored — the only line this change touched in that file is the `partial` keyword on the class declaration.

Two of the pre-existing files, `TaskMaster\AppGlobals\AppItemEngines.cs` (18 errors) and `TaskMaster\AppGlobals\ApplicationGlobals.cs` (40 errors), are AC15-protected zero-line-diff paths and must not be edited by this change under any circumstances.

This pre-existing debt is recorded here for the orchestrator; it is out of scope for #503 and is not remediated. The plan-specified P6-T5 gate (`/t:Build`) is symmetric between the merge-base baseline and this run, and both report `EXIT_CODE: 0`.

Binary outcome: **PASS**.

This task mutated no source file.
