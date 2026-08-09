# P5-T7 — Per-Type Line Coverage for the Two New Types (AC-18)

Timestamp: 2026-08-08T21-21

Command (the XML query, executed through a scratchpad `.ps1` so the PowerShell literals survive
the shell wrapper):

```powershell
Set-Location '<REPO>'
[xml]$doc = Get-Content 'coverage\coverage-final-505.cobertura.xml'
$targets = @(
    'TaskMaster/Ribbon/EngineToggleStateCoordinator.cs',
    'TaskMaster/Ribbon/EngineToggleCatalog.cs'
)
foreach ($t in $targets) {
    $nodes = @($doc.coverage.packages.package.classes.class | Where-Object {
            $_.filename -and ($_.filename.Replace('\', '/') -eq $t)
        })
    Write-Host ("TARGET={0} MATCHCOUNT={1}" -f $t, $nodes.Count)
    foreach ($n in $nodes) {
        Write-Host ("  class={0} line-rate={1} branch-rate={2} complexity={3}" -f $n.name, $n.'line-rate', $n.'branch-rate', $n.complexity)
        $lines = @($n.lines.line)
        $hit = @($lines | Where-Object { [int]$_.hits -gt 0 }).Count
        Write-Host ("  lines-total={0} lines-hit={1} computed-rate={2}" -f $lines.Count, $hit, [math]::Round($hit / [double]$lines.Count, 6))
    }
}
```

Source document: `coverage\coverage-final-505.cobertura.xml`, the P5-T6 run's post-processed dump.

EXIT_CODE: 0

## Output Summary

Raw output:

```
TARGET=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs MATCHCOUNT=1
  class=TaskMaster.EngineToggleStateCoordinator line-rate=0.991489 branch-rate=0.944444 complexity=32
  lines-total=135 lines-hit=133 computed-rate=0.985185
TARGET=TaskMaster/Ribbon/EngineToggleCatalog.cs MATCHCOUNT=1
  class=TaskMaster.EngineToggleCatalog line-rate=1 branch-rate=1 complexity=2
  lines-total=18 lines-hit=18 computed-rate=1
```

| File | `<class>` nodes matched | **`line-rate`** | `branch-rate` | Gate (>= 0.90) |
|---|---|---|---|---|
| `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` | **1** | **0.991489** | 0.944444 | **PASS** |
| `TaskMaster\Ribbon\EngineToggleCatalog.cs` | **1** | **1.000000** | 1.000000 | **PASS** |

### Query-correctness notes (as the task requires)

- **Match count is exactly 1 per file**, not 0. A count of 0 would be a query defect, not a
  coverage result. The count is 1 because `Invoke-MSTestWithCoverage.ps1` runs
  `Merge-CoberturaClassesByFilename` during post-processing, which has already collapsed each
  file's `<Method>d__N` async state-machine classes and `<>c` closure classes into a single
  `<class>` node with a recomputed `line-rate`. That node's `line-rate` attribute is read
  directly rather than summed across siblings.
- The query is **separator-agnostic**: `filename` is normalized with `.Replace('\','/')` before
  matching. The document as emitted uses the **Windows** separator
  (`TaskMaster\Ribbon\EngineToggleStateCoordinator.cs`) because
  `ConvertTo-KoverageCoberturaXml` is called without `-PathSeparator`; the normalization means the
  query also survives a future `-PathSeparator` change. The forward-slash form is never queried
  directly against the raw attribute.
- The secondary `computed-rate` figures (0.985185 and 1.0) are a cross-check computed by counting
  `<line hits>` children of the merged node. They differ slightly from the `line-rate` attribute on
  the coordinator (0.985185 vs 0.991489) because Cobertura repeats line entries under both
  `<method>` and the class-level `<lines>` collection, so a naive per-`<line>` count uses a
  different denominator than the recomputed attribute. **The `line-rate` attribute is the
  plan-specified value and is the one gated.** Both measures clear 0.90 regardless of which
  counting method is used, so the gate outcome is not sensitive to the discrepancy.
- Neither file carries `[ExcludeFromCodeCoverage]` (verified at P4-T4), so both are genuinely in
  the coverage denominator; the figures are real, not artifacts of an exemption.

Binary outcome: **PASS** — both values are at or above 0.90 (0.991489 and 1.000000). No test
cases need to be added and the phase does not restart at P5-T1.
