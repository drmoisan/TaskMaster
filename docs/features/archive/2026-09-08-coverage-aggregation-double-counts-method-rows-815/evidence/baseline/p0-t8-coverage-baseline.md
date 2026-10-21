# P0-T8 — Pre-Change Changed-File Coverage Baseline (Direct Pester Run)

Timestamp: 2026-09-09T10-47
Task: [P0-T8]
EXIT_CODE: 0

## Command 1 — the coverage run

Command: `pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml"; $r = Invoke-Pester -Configuration $c; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`
EXIT_CODE: 0

Tail of the run output:

```
Tests Passed: 96, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
Covered 78.33% / 75%. 803 analyzed Commands in 11 Files.
PASSED=96 FAILED=0
```

The `78.33%` figure Pester prints is **command (instruction) coverage**, not line coverage.
`.claude/rules/powershell.md` records that Pester reports both and that the repository threshold is a
line threshold, so the line figures below are the ones this plan compares against.

The JaCoCo document was written to
`docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml`
and exists.

## Command 2 — the per-file LINE counter reader

Command: `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/baseline/p0-t8-coverage-baseline.jacoco.xml"; foreach ($n in @($j.SelectNodes("//sourcefile"))) { foreach ($c in @($n.SelectNodes("./counter"))) { if ($c.type -eq "LINE") { Write-Output ($n.name + " covered=" + $c.covered + " missed=" + $c.missed) } } }'`
EXIT_CODE: 0

```
Install-RepoDotNetSdk.ps1 covered=3 missed=30
Invoke-MSTest.ps1 covered=38 missed=3
Invoke-MSTestWithCoverage.ClosureFilter.ps1 covered=93 missed=0
Invoke-MSTestWithCoverage.Helpers.ps1 covered=191 missed=19
Invoke-MSTestWithCoverage.PackageRate.ps1 covered=18 missed=0
Invoke-MSTestWithCoverage.ps1 covered=89 missed=10
Invoke-MSTestWithCoverage.Threshold.ps1 covered=14 missed=1
Invoke-Restore.ps1 covered=0 missed=16
Invoke-VSBuild.ps1 covered=36 missed=7
Sync-PackageReferences.ps1 covered=53 missed=31
TestProcessCleanup.ps1 covered=0 missed=29
```

The reader used the shape the plan predicted: a `<counter>` child of a `<sourcefile>` element,
carrying `type="LINE"`, `covered` and `missed` attributes. No selector substitution was required and
no shape discrepancy was observed.

## Recorded baseline values

| File | LINE covered | LINE missed |
| --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 191 | 19 |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 89 | 10 |

Folder LINE totals across all eleven measured files: covered 535, missed 146, denominator 681.
Overall folder line percentage: **78.56%**.

Output Summary: `FAILED=0` with 96 tests passing. The JaCoCo document exists at the stated evidence
path and both files this feature modifies appear in the per-file listing with numeric values:
`Invoke-MSTestWithCoverage.Helpers.ps1` at covered 191 / missed 19, and
`Invoke-MSTestWithCoverage.ps1` at covered 89 / missed 10. The `scripts/vscode` folder line
percentage before the change is 78.56% (535 of 681 lines). P5-T8 compares against the two per-file
baselines recorded here: the Helpers covered value may not fall, and the entry point's missed value
may rise by at most 1.
