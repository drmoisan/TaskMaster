# PRE-CHANGE Generator-Parity A/B (P0-T11)

Timestamp: 2026-08-10T22-30

Deterministic A/B over a fixed committed input. This is **not** a test-suite run; it dot-sources
unmodified `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and evaluates
`Get-CoberturaCoverageSummary` against the committed **raw** `dotnet-coverage` document, whose own
root attributes are the generator's ground truth.

Input:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
(17,473,869 bytes, raw generator output — absolute filenames, no `<sources>` element).

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
. (Join-Path $root 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1')
$sample = Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml'
[xml]$doc = Get-Content -LiteralPath $sample -Raw -Encoding UTF8
'INPUT root: lines-valid={0} lines-covered={1} branches-valid={2} branches-covered={3}' -f `
    $doc.coverage.'lines-valid', $doc.coverage.'lines-covered', $doc.coverage.'branches-valid', $doc.coverage.'branches-covered'
Get-CoberturaCoverageSummary -XmlDocument $doc | Format-List
```

EXIT_CODE: 0

Output Summary:

```
INPUT root: lines-valid=79957 lines-covered=56124 branches-valid=23109 branches-covered=13472

LineRate        : 0.702848
BranchRate      : 0.582976
LinesCovered    : 113219
LinesValid      : 161086
BranchesCovered : 26944
BranchesValid   : 46218
```

## Recorded figures

| Quantity | Input document's own root attribute (ground truth) | `Get-CoberturaCoverageSummary` PRE-CHANGE | Inflated? |
| --- | --- | --- | --- |
| `lines-valid` / `LinesValid` | **79957** | **161086** | yes (+81129) |
| `lines-covered` / `LinesCovered` | **56124** | **113219** | yes (+57095) |
| `branches-valid` / `BranchesValid` | **23109** | **46218** | yes (+23109, exactly 2x) |
| `branches-covered` / `BranchesCovered` | **13472** | **26944** | yes (+13472, exactly 2x) |

All four pre-change values are concrete integers and each is strictly greater than its ground-truth
counterpart. `LinesValid = 161086` matches the plan's stated expectation exactly. The branch figures
are exactly double the ground truth, which is the signature of the descendant-axis double count at
`Invoke-MSTestWithCoverage.Helpers.ps1:122` combined with the branch accumulator at `:128-131`
living inside that same loop.

The document's own `line-rate` is `0.7019272859161799` and the pre-change recomputation reports
`0.702848`; the *rate* is nearly unchanged while the *counts* are roughly doubled, which is the
same trap § Fixture-Design Trap describes for `branch-rate`.
