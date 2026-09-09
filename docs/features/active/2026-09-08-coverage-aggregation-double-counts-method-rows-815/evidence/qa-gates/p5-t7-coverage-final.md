# P5-T7 — Post-Change Changed-File Coverage (Direct Pester Run)

Timestamp: 2026-09-09T11-36
Task: [P5-T7]
EXIT_CODE: 0

## Command 1 — the coverage run

Command: `pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml"; $r = Invoke-Pester -Configuration $c; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`
EXIT_CODE: 0

Tail of the run output:

```
Tests Passed: 103, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0
Covered 79.67% / 75%. 861 analyzed Commands in 12 Files.
PASSED=103 FAILED=0
```

The `79.67%` figure Pester prints is **command (instruction) coverage**, not line coverage, and is
recorded here only so the transcript is complete. The line figures below are the ones the thresholds
are compared against.

The JaCoCo document exists at
`docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml`.

## Command 2 — the per-file LINE counter reader

Command: `pwsh -NoProfile -Command '[xml]$j = Get-Content -Raw -LiteralPath "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/evidence/qa-gates/p5-t7-coverage-final.jacoco.xml"; foreach ($n in @($j.SelectNodes("//sourcefile"))) { foreach ($c in @($n.SelectNodes("./counter"))) { if ($c.type -eq "LINE") { Write-Output ($n.name + " covered=" + $c.covered + " missed=" + $c.missed) } } }'`
EXIT_CODE: 0

```
Install-RepoDotNetSdk.ps1 covered=3 missed=30
Invoke-MSTest.ps1 covered=38 missed=3
Invoke-MSTestWithCoverage.ClosureFilter.ps1 covered=93 missed=0
Invoke-MSTestWithCoverage.FirstParty.ps1 covered=32 missed=1
Invoke-MSTestWithCoverage.Helpers.ps1 covered=192 missed=19
Invoke-MSTestWithCoverage.PackageRate.ps1 covered=18 missed=0
Invoke-MSTestWithCoverage.ps1 covered=90 missed=10
Invoke-MSTestWithCoverage.Threshold.ps1 covered=14 missed=1
Invoke-Restore.ps1 covered=0 missed=16
Invoke-VSBuild.ps1 covered=36 missed=7
Sync-PackageReferences.ps1 covered=53 missed=31
TestProcessCleanup.ps1 covered=0 missed=29
```

All three files this feature adds or modifies appear with numeric values:

| File | LINE covered | LINE missed | Line % |
| --- | --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 96.97% |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 192 | 19 | 91.00% |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 90 | 10 | 90.00% |

Folder LINE totals across all twelve measured files: covered 569, missed 147, denominator 716.
Overall folder line percentage: **79.47%**, against a pre-change baseline of 78.56%.

## Why the bundled artifact is not used

`artifacts/pester/powershell-coverage.xml` measures no file under the scripts tree. Its nine
`<package>` elements all name paths under `.claude/` or `.codex/`, and its report-level LINE counter
is covered 0 / missed 6583. The full observation, with every package name, is recorded in
`evidence/baseline/p0-t9-bundled-coverage-nonprobative.md`. The bundled artifact therefore cannot
supply changed-file coverage for this feature, which is why AC10 is measured by this direct Pester
run with `CodeCoverage.Path` set explicitly to `scripts/vscode`.

Output Summary: `FAILED=0` with 103 tests passing. The JaCoCo document exists at the stated evidence
path, and all three changed files appear in the per-file listing with numeric values:
`Invoke-MSTestWithCoverage.FirstParty.ps1` at covered 32 / missed 1,
`Invoke-MSTestWithCoverage.Helpers.ps1` at covered 192 / missed 19, and
`Invoke-MSTestWithCoverage.ps1` at covered 90 / missed 10. The `scripts/vscode` folder line
percentage rose from 78.56% to 79.47%. The threshold comparison is carried by
`evidence/qa-gates/p5-t8-coverage-comparison.md`.
