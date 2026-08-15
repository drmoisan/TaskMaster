# PRE-CHANGE Package-Filtered A/B (P0-T12)

Timestamp: 2026-08-10T22-30

Deterministic A/B over a fixed committed input. Reprocesses the already post-processed committed
document through the full `ConvertTo-KoverageCoberturaXml` pipeline (package filtering, filename
normalization, class merging, root-attribute rewrite) against **unmodified**
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.

Input:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(10,398,170 bytes; 186,913 lines).

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
. (Join-Path $root 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$sample = Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml'
$content = Get-Content -LiteralPath $sample -Raw -Encoding UTF8
[xml]$out = ConvertTo-KoverageCoberturaXml -XmlContent $content -RepoRoot $root -PathSeparator '\'
'lines-valid={0} lines-covered={1} line-rate={2} branches-valid={3} branches-covered={4} branch-rate={5}' -f `
    $out.coverage.'lines-valid', $out.coverage.'lines-covered', $out.coverage.'line-rate', `
    $out.coverage.'branches-valid', $out.coverage.'branches-covered', $out.coverage.'branch-rate'
```

EXIT_CODE: 0

Output Summary:

```
lines-valid=110849 lines-covered=94937 line-rate=0.856453 branches-valid=27848 branches-covered=22001 branch-rate=0.790039
```

## Recorded figures

| Quantity | PRE-CHANGE value |
| --- | --- |
| `lines-valid` | **110849** |
| `lines-covered` | **94937** |
| `line-rate` | **0.856453** |
| `branches-valid` | 27848 |
| `branches-covered` | 22001 |
| `branch-rate` | 0.790039 |

All three plan-required figures (`lines-valid = 110849`, `lines-covered = 94937`,
`line-rate = 0.856453`) reproduce exactly. The reprocessed output is byte-identical in these six
attributes to the input document's own committed root attributes, which is expected: the input was
itself produced by this same defective code path, so reprocessing it is idempotent pre-change.
