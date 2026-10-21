# P0-T18 — Pester Baseline

Timestamp: 2026-09-19T23-08

Command: CMD-PESTER-BASELINE with `<OUTPATH>` set to `coverage/p0-t18-pester-coverage.xml`.

```
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p0-t18-pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Verbatim result line

```
PESTER Passed=174 Failed=0 Skipped=0 Total=174
```

`Total` is **174**, greater than zero.

Per gate rule 4, the explicit `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` is what makes
the exit code meaningful: `New-PesterConfiguration` defaults `Run.Exit` to `$false`, so a bare
Pester run exits 0 whatever the tests do. The clause is placed after the count-emitting statement so
the counts are printed before the exit.

## Aggregate JaCoCo LINE coverage

Read from the report-level `counter` element of type `LINE` in
`coverage/p0-t18-pester-coverage.xml`.

| Measurement | Value |
|---|---|
| Covered lines | 731 |
| Missed lines | 140 |
| Total instrumented lines | 871 |
| **Aggregate LINE percentage** | **83.93** |

Computed as `covered / (covered + missed) * 100` = `731 / 871 * 100`.

This is above the authoritative PowerShell line floor of 80 percent stated in the execution
worktree's `CLAUDE.md` under issue #563. It is below the 85 percent in
`.claude/rules/general-unit-test.md`, which gate rule 13 records as superseded push-down-owned
boilerplate with the discrepancy tracked at open issue #668.

## Per-file LINE counters, all 14 instrumented files

| `sourcefile` name | Covered | Missed | Total | Percent |
|---|---|---|---|---|
| `Install-RepoDotNetSdk.ps1` | 13 | 20 | 33 | 39.39 |
| `Invoke-MSTest.ps1` | 49 | 7 | 56 | 87.50 |
| `Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 42 | 95.24 |
| `Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 93 | 100.00 |
| `Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 33 | 96.97 |
| `Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 212 | 96.23 |
| `Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 18 | 100.00 |
| `Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 40 | 97.50 |
| `Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 126 | 89.68 |
| `Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 33 | 100.00 |
| `Invoke-Restore.ps1` | 22 | 1 | 23 | 95.65 |
| `Invoke-VSBuild.ps1` | 46 | 3 | 49 | 93.88 |
| **`Sync-PackageReferences.ps1`** | **0** | **84** | **84** | **0.00** |
| `TestProcessCleanup.ps1` | 29 | 0 | 29 | 100.00 |

The full per-file table is recorded, not only the file the acceptance names, because P2-T3's
no-regression gate is stated per file over this same `scripts/vscode` population and reads each
covered count from this artifact.

### `Sync-PackageReferences.ps1` counter, as the acceptance requires

**covered = 0, missed = 84.** It is the only script in `scripts/vscode/` with no test file, and it
is what holds the `scripts/vscode` population at 83.93 percent. P3-T5 gives it a suite, which is
why the absolute floor is first asserted at P4-T3 rather than at P2-T3.

## Comparison against the figures the plan records

| Figure | Plan | Measured | Verdict |
|---|---|---|---|
| Aggregate LINE percent | 83.93 | 83.93 | equal, well inside the 0.5-point tolerance |
| `Total` | 174 | 174 | equal |
| `Sync-PackageReferences.ps1` | 0 covered of 84 | 0 covered of 84 | equal |

Nothing differs, so nothing is reported for absorption.

## Why CMD-PESTER-BASELINE is used here rather than CMD-PESTER-ALL

`scripts/dependencies` does not yet exist — it is created at P1-T4, and
`tests/scripts/dependencies` at P1-T5. Naming a directory that does not exist in
`CodeCoverage.Path` makes Pester emit a `Write-Error` for the missing coverage path and produce no
JaCoCo document at all, so the four-member form of the command yields no baseline whatsoever at this
point in the run. The two-member baseline variant is therefore mandatory here, and CMD-PESTER-ALL
becomes valid from P1-T6 onward.

The consequence for later comparison is recorded so it is not mistaken for a regression: this run
instruments `scripts/vscode` alone, while every run from P2-T3 onward also instruments
`scripts/dependencies`. The two aggregates measure different populations and are not comparable.
That is why P2-T3's no-regression gate is stated per file over the shared `scripts/vscode`
population rather than aggregate against aggregate.

## Evidence-form limitation, per gate rule 12

The figures above are recorded in this `.md` artifact and stand in for a permitted evidence form
that does not exist for the PowerShell route. All three forms the authoritative `CLAUDE.md`
`## Committed Test Evidence Format` section permits — the package-level JaCoCo projection of a
post-processed Cobertura document, the one-line first-party coverage summary, and the trx-derived
test-result summary — are defined against the C# route. A Pester run emits JaCoCo directly with no
Cobertura stage, and `ConvertTo-JacocoPackageProjection` accepts Cobertura only, so none of the
three can be produced for this run. These recorded figures are a fourth form the section does not
define. The gap is stated rather than closed, because closing it would mean either committing the
prohibited collector document or building a Cobertura stage this change has no reason to build.

The collector document itself is at `coverage/p0-t18-pester-coverage.xml`, which `.gitignore:144`
ignores. It is read there and left there; no `.xml` is written under `<FEATURE>/evidence/` and no
commit pathspec carries one.

## Acceptance evaluation

- The verbatim `PESTER Passed=... Failed=... Skipped=... Total=...` line is recorded with `Total`
  greater than zero — **174**. PASS.
- The aggregate JaCoCo LINE percentage is recorded as a number with two decimals — **83.93**. PASS.
- The `sourcefile` LINE counter for `Sync-PackageReferences.ps1` is recorded as covered and missed
  integers — **0 covered, 84 missed**. PASS.
- The aggregate does not differ from 83.93 by more than 0.5 points and `Total` does not differ from
  174, so neither triggers the report-rather-than-absorb clause. PASS.
- The artifact records why CMD-PESTER-BASELINE is used here rather than CMD-PESTER-ALL. PASS.

Output Summary: CMD-PESTER-BASELINE returned EXIT_CODE 0 with
`PESTER Passed=174 Failed=0 Skipped=0 Total=174`. Aggregate JaCoCo LINE coverage over
`scripts/vscode` is **83.93** percent, 731 covered of 871 instrumented lines, above the
authoritative 80 percent floor. `Sync-PackageReferences.ps1` reports **0 covered of 84** lines and
is the only script in the folder without a test file. All three figures equal the values the plan
records, so none is reported for absorption. The two-member baseline command form is mandatory here
because `scripts/dependencies` does not exist until P1-T4. The recorded figures stand in for a
permitted evidence form that does not exist for the PowerShell route.
