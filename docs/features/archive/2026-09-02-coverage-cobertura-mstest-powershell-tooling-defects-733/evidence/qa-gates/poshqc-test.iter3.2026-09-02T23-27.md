# P5-T4 — PoshQC test gate with coverage (Final QA Loop, iteration 3, final)

Timestamp: 2026-09-02T23-27

## Command 1 — MCP test run

Command: `mcp__drm-copilot__run_poshqc_test` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

EXIT_CODE: n/a (MCP tool returns an ok/summary payload only)

MCP payload:

```
ok: true
tool: run_poshqc_test
summary: Ran bundled PoshQC test against the item worktree with 2 selected scan folder(s).
```

This payload carries no pass/fail counts, no per-test names, and no coverage figure, so it is
recorded for the policy trail only. The numeric evidence comes from Command 2.

## Command 2 — Direct Pester run with code coverage

Command: `pwsh -NoProfile -Command` with a single-quoted outer wrapper and a double-quoted inner
script, building a `New-PesterConfiguration` with:

- `Run.Path` = the 8 write-set test files under `tests/scripts/vscode`,
- `Run.PassThru` = `$true`,
- `Output.Verbosity` = `"Detailed"`,
- `CodeCoverage.Enabled` = `$true`,
- `CodeCoverage.Path` = the 6 write-set production files under `scripts/vscode`,
- `CodeCoverage.OutputPath` =
  `evidence/qa-gates/pester-coverage.final-qc.iter3.2026-09-02T23-27.xml`,

followed by the explicit trailing branch
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

### Run.Path (8 files)

```
tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1
tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
```

### CodeCoverage.Path (6 files)

```
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
scripts/vscode/Invoke-MSTestWithCoverage.ps1
scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
scripts/vscode/Invoke-MSTest.ps1
scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
```

### Result counts

```
=== TOTALS ===
Passed=84 Failed=0 Skipped=0 Total=84
=== PER TEST FILE ===
Invoke-MSTest.AssemblyDiscovery.Tests.ps1 P=5 F=0 S=0
Invoke-MSTest.Main.Tests.ps1 P=11 F=0 S=0
Invoke-MSTest.RunSettings.Tests.ps1 P=27 F=0 S=0
Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 P=12 F=0 S=0
Invoke-MSTestWithCoverage.Helpers.Tests.ps1 P=20 F=0 S=0
Invoke-MSTestWithCoverage.Merge.Tests.ps1 P=2 F=0 S=0
Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 P=2 F=0 S=0
Invoke-MSTestWithCoverage.Threshold.Tests.ps1 P=5 F=0 S=0
```

### Per-production-file coverage

```
=== PER PRODUCTION FILE COVERAGE ===
Invoke-MSTestWithCoverage.Helpers.ps1 exec=228 miss=23 total=251 pct=90.84
Invoke-MSTestWithCoverage.ps1 exec=100 miss=11 total=111 pct=90.09
Invoke-MSTestWithCoverage.ClosureFilter.ps1 exec=111 miss=0 total=111 pct=100.00
Invoke-MSTest.ps1 exec=47 miss=3 total=50 pct=94.00
Invoke-MSTestWithCoverage.PackageRate.ps1 exec=25 miss=0 total=25 pct=100.00
Invoke-MSTestWithCoverage.Threshold.ps1 exec=15 miss=2 total=17 pct=88.24
AGGREGATE exec=526 miss=39 total=565 pct=93.10
```

Pester's own summary line for the same run: `Covered 93.1% / 75%. 565 analyzed Commands in
6 Files.`

### Remaining missed commands in scripts/vscode/Invoke-MSTest.ps1

```
L93: return & $VsWherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
L94: return & $VsWherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
```

L93 and L94 are the two commands of the single pipeline inside the `Get-VsTestConsolePath` seam.
That pipeline is the one remaining external-process invocation in the file, and covering it would
require launching a real `vswhere.exe`, which `.claude/rules/general-unit-test.md` prohibits in a
unit test.

```
L201: Invoke-MSTestMain @PSBoundParameters
```

L201 is the entire top-level host-bound wiring, guarded by
`if ($MyInvocation.InvocationName -ne '.')`. It is the thinnest remaining entry point: one
forwarding call, reached only when the script is executed rather than dot-sourced.

No production file was excluded from the coverage denominator, and no threshold was changed.

Branch coverage: not emitted by Pester 5. Measured fact, unchanged from the P0-T7 baseline.

## Output Summary

- MCP test: `ok` true across both scan folders.
- Direct Pester run: EXIT_CODE 0, Passed 84, Failed 0, Skipped 0.
- All six production files are at or above the 85 percent floor: 90.84, 90.09, 100.00, 94.00,
  100.00, 88.24. Aggregate 93.10 percent over 565 commands.
- `scripts/vscode/Invoke-MSTest.ps1` rose from 72.34 percent (iteration 2) to 94.00 percent after
  its entry-point body was extracted into `Invoke-MSTestMain`.
- No test failed and no file was changed by this task, so the Final QA Loop terminates on this
  iteration.
