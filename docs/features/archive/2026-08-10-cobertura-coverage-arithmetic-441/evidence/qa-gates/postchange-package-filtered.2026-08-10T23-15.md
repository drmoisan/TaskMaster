# POST-CHANGE Package-Filtered A/B (P5-T2)

Timestamp: 2026-08-10T23-15

Identical procedure and identical fixed input to P0-T12, run against the **fixed**
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Exercises the full
`ConvertTo-KoverageCoberturaXml` pipeline: package filtering, filename normalization, class merging
and root-attribute rewrite.

Input:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

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
lines-valid=62345 lines-covered=53013 line-rate=0.850317 branches-valid=15828 branches-covered=12445 branch-rate=0.786265
```

## Verdict

| Quantity | PRE-change (P0-T12) | POST-change (this run) | Required post-change | Match |
| --- | --- | --- | --- | --- |
| `lines-valid` | 110849 | **62345** | **62345** | **exact** |
| `lines-covered` | 94937 | **53013** | **53013** | **exact** |
| `line-rate` | 0.856453 | **0.850317** | **0.850317** | **exact** |
| `branches-valid` | 27848 | **15828** | (not pinned by the task) | — |
| `branches-covered` | 22001 | **12445** | (not pinned by the task) | — |
| `branch-rate` | 0.790039 | **0.786265** | (not pinned by the task) | — |

All three required figures reproduce exactly. AC-3 is satisfied.

### Independent corroboration of the branch figures

The post-change `branches-valid = 15828` and `branches-covered = 12445` were **predicted before the
implementation existed**, by the independent streaming `XmlReader` pass in P0-T18, which measured
union branches valid = 15828 and union branches covered = 12445 on this same document. The
implementation reproduces both exactly. The value also falls inside the range `[15730, 16582]` that
the research derived analytically for class-level `branches-valid`, against the emitted defective
value of 27848.

### Note on the line-rate delta

`line-rate` falls from 0.856453 to 0.850317, a reduction of **0.61 percentage points**. The
corrected figure of **85.0317%** sits 0.03 pp above the uniform 85% line floor in
`.claude/rules/general-unit-test.md`. That observation is recorded as fact and handed off to child
feature #494 (see `<FEATURE>/evidence/other/threshold-handoff-494.2026-08-10T23-15.md`). **No
threshold is changed by this feature.**
