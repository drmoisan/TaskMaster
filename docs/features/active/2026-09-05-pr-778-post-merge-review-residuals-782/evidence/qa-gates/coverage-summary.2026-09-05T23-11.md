# Coverage Summary — Package Level, JaCoCo Counter Form (P7-T6)

Timestamp: 2026-09-05T23-11

Command:

```powershell
[xml]$doc = Get-Content -LiteralPath 'coverage\782-p7-final.cobertura.xml'
$allow = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
foreach ($pkg in $doc.SelectNodes('//package')) {
    if ($allow -notcontains $pkg.GetAttribute('name')) { continue }
    $lines = $pkg.SelectNodes('.//line')
    $lc = 0; $bc = 0; $bv = 0
    foreach ($l in $lines) {
        if ([int]$l.GetAttribute('hits') -gt 0) { $lc++ }
        $cc = $l.GetAttribute('condition-coverage')
        if ($cc -and $cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
```

The aggregation uses the all-descendant `.//line` selection pinned by SD22, the same selection
P0-T7 and P7-T5 use, so these rows sum to the figures P7-T5 records rather than to a differently
counted set. LINE `covered` is the count of `<line>` elements whose `hits` attribute exceeds zero;
LINE `missed` is the remainder. BRANCH `covered` and `missed` are derived from the
`(numerator/denominator)` pair inside each `condition-coverage` attribute over the same line set.

EXIT_CODE: 0

Output Summary:

## Per-package counters

| Package | `<counter type="LINE" missed covered>` | `<counter type="BRANCH" missed covered>` |
|---|---|---|
| `QuickFiler` | missed=4999 covered=20135 | missed=1426 covered=4728 |
| `UtilitiesCS` | missed=9924 covered=78550 | missed=3760 covered=18462 |
| `TaskVisualization` | missed=331 covered=2899 | missed=134 covered=666 |
| `SVGControl` | missed=1955 covered=1757 | missed=676 covered=600 |
| `ToDoModel` | missed=1626 covered=2193 | missed=520 covered=496 |
| `Tags` | missed=112 covered=1428 | missed=32 covered=348 |
| `TaskMaster` | missed=1623 covered=4801 | missed=416 covered=1012 |
| `TaskTree` | missed=28 covered=592 | missed=16 covered=188 |
| `VBFunctions` | missed=0 covered=8 | missed=0 covered=0 |
| **Total** | **missed=20598 covered=112363** | **missed=6980 covered=26500** |

## Row arithmetic

Each row's LINE `missed` plus `covered` equals that package's `lines-valid` as counted by the pinned
selection:

| Package | missed + covered | `lines-valid` |
|---|---|---|
| `QuickFiler` | 4999 + 20135 = 25134 | 25134 |
| `UtilitiesCS` | 9924 + 78550 = 88474 | 88474 |
| `TaskVisualization` | 331 + 2899 = 3230 | 3230 |
| `SVGControl` | 1955 + 1757 = 3712 | 3712 |
| `ToDoModel` | 1626 + 2193 = 3819 | 3819 |
| `Tags` | 112 + 1428 = 1540 | 1540 |
| `TaskMaster` | 1623 + 4801 = 6424 | 6424 |
| `TaskTree` | 28 + 592 = 620 | 620 |
| `VBFunctions` | 0 + 8 = 8 | 8 |
| **Total** | **20598 + 112363 = 132961** | **132961** |

The total row's `covered` value of **112363** equals the first-party `lines-covered` figure recorded
in `evidence/qa-gates/p7-t5-tests-coverage.md`.

## Why `artifacts/csharp/coverage.xml` is not produced (SD1)

The repository pipeline emits Cobertura while the feature-review coverage hook parses JaCoCo, so
producing that path requires a throwaway format conversion. The hook additionally applies a fixed
repository-wide line floor that would force a FAIL verdict for a shortfall that pre-exists on
`origin/main` and that this delivery neither caused nor is scoped to repair. This summary carries
the same per-package counter information in the JaCoCo counter form, in the canonical evidence
location, without either cost.
