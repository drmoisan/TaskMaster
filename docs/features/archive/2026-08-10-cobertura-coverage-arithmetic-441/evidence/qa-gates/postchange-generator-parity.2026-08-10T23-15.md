# POST-CHANGE Generator-Parity A/B (P5-T1)

Timestamp: 2026-08-10T23-15

The primary acceptance oracle for AC-1. Identical procedure and identical fixed input to P0-T11,
run against the **fixed** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Deterministic;
no test run is involved.

Input:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
(raw `dotnet-coverage` output; its own root attributes are the generator's ground truth).

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

LineRate        : 0.701927
BranchRate      : 0.582976
LinesCovered    : 56124
LinesValid      : 79957
BranchesCovered : 13472
BranchesValid   : 23109
```

## Verdict — exact generator parity

| Quantity | Required (the document's own root attribute) | Post-change measured | Match |
| --- | --- | --- | --- |
| `LinesValid` | **79957** | **79957** | **exact** |
| `LinesCovered` | **56124** | **56124** | **exact** |
| `BranchesValid` | **23109** | **23109** | **exact** |
| `BranchesCovered` | **13472** | **13472** | **exact** |

**All four figures reproduce the input document's own root attributes exactly.** The corrected
post-processor now returns, to the line, the figures the instrumentation tool itself computed. This
is the strongest available acceptance evidence and satisfies AC-1.

As a further consistency check not required by the task: the derived `LineRate` is now `0.701927`,
which is the document's own `line-rate="0.7019272859161799"` rounded to six decimal places. The
pre-change run reported `0.702848`, a value the input document never carried.

## A/B against P0-T11

| Quantity | PRE-change (P0-T11) | POST-change (this run) | Ground truth | Reduction |
| --- | --- | --- | --- | --- |
| `LinesValid` | 161086 | **79957** | 79957 | -81129 |
| `LinesCovered` | 113219 | **56124** | 56124 | -57095 |
| `BranchesValid` | 46218 | **23109** | 23109 | -23109 (was exactly 2x) |
| `BranchesCovered` | 26944 | **13472** | 13472 | -13472 (was exactly 2x) |

Each pre-change figure is strictly greater than its post-change counterpart, and every post-change
figure lands exactly on ground truth.
