# CSharpier Formatting-Scope Rule — Issue #503 (P0-T12)

Timestamp: 2026-08-08T13-14

Derived from: `<FEATURE>\evidence\baseline\csharpier-check.2026-08-08T13-08.md` (P0-T6)

## Rule 5 of plan section 3, restated verbatim

> **CSharpier scope guard.** `csharpier format` is invoked with the explicit scope-locked `.cs` path list from section 4 and is NEVER invoked repo-wide, and NEVER with `TaskMaster\AppGlobals\AppItemEngines.cs` or `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` in its argument list. Reason: a repo-wide `csharpier format .` would reformat any file that is unformatted at the merge-base, which would break the AC15 zero-line-diff requirement. `csharpier check .` (read-only) is still run repo-wide as the comparison gate.

## The thirteen scope-locked `.cs` paths P6-T1 may pass to `csharpier format`

```
TaskMaster\Ribbon\EngineCommandCatalog.cs
TaskMaster\Ribbon\EngineReadinessGate.cs
TaskMaster\Ribbon\EngineGatedCommandRunner.cs
TaskMaster\Ribbon\EngineCommandRefreshPlanner.cs
TaskMaster\Ribbon\RibbonController.EngineCommands.cs
TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs
TaskMaster\Ribbon\RibbonViewer.cs
TaskMaster\ThisAddIn.cs
TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs
TaskMaster.Test\Ribbon\EngineReadinessGateTests.cs
TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs
TaskMaster.Test\Ribbon\EngineCommandRefreshPlannerTests.cs
TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs
```

That is exactly thirteen paths, matching plan section 4.5. No other path may be passed to `csharpier format` during this change.

## Protected-file status in the P0-T6 unformatted set

The P0-T6 measurement returned `EXIT_CODE: 0` over 1488 files, so the **merge-base unformatted set is empty**.

- `TaskMaster\AppGlobals\AppItemEngines.cs` — **does NOT appear** in the P0-T6 unformatted set.
- `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` — **does NOT appear** in the P0-T6 unformatted set.

Because neither protected file is unformatted at the merge-base, no pre-existing-condition statement about deliberately leaving them unformatted is required. Both files are already CSharpier-clean, so their AC15 zero-line diff is not in tension with the formatter.

The scope guard is nevertheless applied unchanged (plan decision D4): it is fail-safe if the measured state drifts during execution, and it makes the AC15 zero-line-diff guarantee structural rather than dependent on a measurement taken at Phase 0 time.

EXIT_CODE: 0
