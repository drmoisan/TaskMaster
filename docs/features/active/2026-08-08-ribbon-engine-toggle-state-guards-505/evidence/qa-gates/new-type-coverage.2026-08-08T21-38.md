# P5-T7 — Per-Type Line Coverage for the Two New Types (AC-18)

Timestamp: 2026-08-08T21-38

Command (the XML query used, run through a scratchpad `.ps1`):

```powershell
$doc = [xml](Get-Content 'coverage\coverage-final-505.cobertura.xml')
$classes = $doc.SelectNodes('//class')
foreach ($t in @('TaskMaster/Ribbon/EngineToggleStateCoordinator.cs',
                 'TaskMaster/Ribbon/EngineToggleCatalog.cs')) {
    # Separator-agnostic: normalize the document's filename attribute before comparing.
    $matched = @($classes | Where-Object { $_.GetAttribute('filename').Replace('\','/') -eq $t })
    $matched.Count            # must be exactly 1
    $matched[0].GetAttribute('line-rate')
}
```

EXIT_CODE: 0

## Query-shape notes (load-bearing)

`Invoke-MSTestWithCoverage.ps1` post-processes the raw dump before writing it:

- `ConvertTo-KoverageCoberturaXml` is called **without** `-PathSeparator`, so `filename` attributes
  use the **Windows separator** (`TaskMaster\Ribbon\EngineToggleStateCoordinator.cs`). A
  forward-slash query would match nothing. The query above therefore normalizes with
  `.Replace('\','/')` before comparing, so it survives a future `-PathSeparator` change.
- `Merge-CoberturaClassesByFilename` has already collapsed each file's `<Method>d__N`
  state-machine and `<>c` closure classes into a single `<class>` node with a recomputed
  `line-rate`, so that node's `line-rate` attribute is read directly rather than summed across
  siblings.

The document contains 547 `<class>` nodes in total.

## Output Summary

| File | `<class>` nodes matched | Class name | **line-rate** | branch-rate | Lines total / covered / uncovered |
|---|---|---|---|---|---|
| `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` | **1** | `TaskMaster.EngineToggleStateCoordinator` | **0.991489** | 0.944444 | 135 / 133 / 2 |
| `TaskMaster\Ribbon\EngineToggleCatalog.cs` | **1** | `TaskMaster.EngineToggleCatalog` | **1.000000** | 1.000000 | 18 / 18 / 0 |

Both match counts are exactly **1**, so neither figure is a query defect.

### The two uncovered lines

`EngineToggleStateCoordinator.cs` lines **219-220**:

```csharp
            {
                throw new InvalidOperationException(BuildUnavailableMessage(engineName));
```

This is the defensive fail-fast guard inside `ExecuteToggleAsync` for a direct caller that invokes
it with the engines unavailable. It is unreachable through the production path, because
`HandleToggleClickAsync` refuses that case first with a `notifyUnavailable` message (covered by
`HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing`). The guard exists so a
future direct caller fails explicitly rather than with a null dereference, per the fail-fast
requirement in `.claude/rules/general-code-change.md`. Both surviving lines are the `throw` and its
opening brace.

## Gate

The repository new-code floor is **>= 0.90 line coverage** (`CLAUDE.md` § UT2). Both files clear it:

- `EngineToggleStateCoordinator.cs`: **0.991489 >= 0.90** — PASS
- `EngineToggleCatalog.cs`: **1.000000 >= 0.90** — PASS

Neither file carries `[ExcludeFromCodeCoverage]` (P4-T4), so both are genuinely in the coverage
denominator; the figures are not manufactured by exemption.

Binary outcome: **PASS** — both values are at or above 0.90, so no test cases need adding and the
phase does not restart at P5-T1.
