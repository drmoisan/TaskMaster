# Pester and Coverage Baseline ([P0-T10])

Timestamp: 2026-09-03T11-56

Command:
1. `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; "COVERAGE LinePercent=$($r.CodeCoverage.CoveragePercent)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`
2. `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $p = "docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/baseline/pester-coverage-baseline.2026-09-03T07-23.xml"; [xml]$x = Get-Content -LiteralPath $p -Raw; $sf = @($x.GetElementsByTagName("sourcefile") | Where-Object { $_.name -like "*Invoke-MSTestWithCoverage.ps1" }); "SOURCEFILE NODE COUNT=" + $sf.Count; $n = @(); foreach ($s in $sf) { $n += @($s.ChildNodes | Where-Object { $_.LocalName -eq "line" -and $_.nr -eq "301" }) }; "LINE301 NODE COUNT=" + $n.Count; if ($n.Count -gt 0 -and [int]$n[0].ci -gt 0) { "BASELINE CHANGED-LINE 301 COVERED: true" } else { "BASELINE CHANGED-LINE 301 COVERED: false" }; exit 0'`

EXIT_CODE: 0

## Emitted lines from command 1, verbatim

```
PESTER Passed=92 Failed=0 Skipped=0 Total=92
COVERAGE LinePercent=78.3042394014963
```

BASELINE LINE COVERAGE PERCENT: 78.3042394014963

## Emitted lines from command 2, verbatim

```
SOURCEFILE NODE COUNT=1
LINE301 NODE COUNT=0
BASELINE CHANGED-LINE 301 COVERED: false
```

Output Summary: PESTER Passed=92 Failed=0 Skipped=0 Total=92 on the pre-change tree, with `COVERAGE LinePercent=78.3042394014963` over `scripts/vscode`. Pester's own detailed console summary for the same run reads `Covered 78.3% / 75%. 802 analyzed Commands in 11 Files.` The JaCoCo XML carries exactly one `sourcefile` node for `Invoke-MSTestWithCoverage.ps1` but no `line` node numbered 301, so Pester attributes no per-line counter to the line this item changes even before the change. `[P3-T6]` reads its expected `CHANGED-LINE COVERAGE:` value from the `BASELINE CHANGED-LINE 301 COVERED: false` line above, which selects that task's `NOT REPORTED` branch.
