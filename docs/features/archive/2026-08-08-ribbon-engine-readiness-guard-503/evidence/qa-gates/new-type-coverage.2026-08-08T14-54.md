# AC23 Per-Type Coverage for the Four New Types — Issue #503 (P6-T7)

Timestamp: 2026-08-08T14-54

Source artifact: `<FEATURE>\evidence\qa-gates\coverage-final.cobertura.xml` (produced by P6-T6)

Command (XML query):
```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load('...\evidence\qa-gates\coverage-final.cobertura.xml')
foreach ($t in 'TaskMaster.EngineCommandCatalog','TaskMaster.EngineReadinessGate',
                'TaskMaster.EngineGatedCommandRunner','TaskMaster.EngineCommandRefreshPlanner') {
    $nodes = $doc.SelectNodes("//class[@name='$t']")
    # report the declared line-rate / branch-rate, and independently recompute
    # line coverage as (count of .//line with hits > 0) / (count of .//line)
}
```

EXIT_CODE: 0

## Output Summary

| Type | Source file | Declared `line-rate` | Recomputed line-rate | Lines | Covered | `branch-rate` | >= 0.90? |
|---|---|---|---|---|---|---|---|
| `TaskMaster.EngineCommandCatalog` | `TaskMaster\Ribbon\EngineCommandCatalog.cs` | **1** | 1.000000 | 48 | 48 | 1 | **Yes** |
| `TaskMaster.EngineReadinessGate` | `TaskMaster\Ribbon\EngineReadinessGate.cs` | **1** | 1.000000 | 48 | 48 | 1 | **Yes** |
| `TaskMaster.EngineGatedCommandRunner` | `TaskMaster\Ribbon\EngineGatedCommandRunner.cs` | **1** | 1.000000 | 72 | 72 | 0.928571 | **Yes** |
| `TaskMaster.EngineCommandRefreshPlanner` | `TaskMaster\Ribbon\EngineCommandRefreshPlanner.cs` | **1** | 1.000000 | 18 | 18 | 1 | **Yes** |

Each type resolves to exactly one `<class>` element in the Cobertura document, so no aggregation across duplicate entries was required. The recomputed line-rate — derived independently as the fraction of `<line>` descendants whose `hits` attribute is greater than zero — agrees with the declared `line-rate` for all four types.

Every one of the four values is **1.000000**, comfortably at or above the 0.90 floor. No additional test cases are required, so no restart of the Phase 6 loop at P6-T1 is triggered.

The one sub-1.0 figure is a *branch* rate (`EngineGatedCommandRunner` at 0.928571, 13 of 14 branches). AC23 gates on **line** coverage, and 0.928571 is in any case above both the CLAUDE.md 0.90 new-code floor and the `.claude/rules/general-unit-test.md` 0.75 branch floor.

These figures are only meaningful because none of the four types carries `[ExcludeFromCodeCoverage]` — verified independently in `<FEATURE>\evidence\qa-gates\no-coverage-exclusion.2026-08-08T14-10.md`. Every line of readiness decision logic is in the coverage denominator.

Binary outcome: **PASS** — all four values are at or above 0.90.
