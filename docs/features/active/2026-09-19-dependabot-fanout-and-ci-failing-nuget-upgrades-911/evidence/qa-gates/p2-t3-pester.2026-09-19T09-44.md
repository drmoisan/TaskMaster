# P2-T3 — Pester suite with coverage, Batch A close-out

Timestamp: 2026-09-19T15-06

Command: CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p2-t3-pester-coverage.xml`.

```
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies","tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p2-t3-pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Verbatim result line

```
PESTER Passed=206 Failed=0 Skipped=0 Total=206
```

Per gate rule 4, the explicit `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` placed after
the count-emitting statement is what makes the exit code meaningful: `New-PesterConfiguration`
defaults `Run.Exit` to `$false`, so a bare Pester run exits 0 whatever the tests do.

## Aggregate JaCoCo LINE coverage

| Measurement | Value |
|---|---|
| Covered lines | 895 |
| Missed lines | 140 |
| Total instrumented lines | 1035 |
| Aggregate LINE percentage | 86.47 |

The aggregate is **recorded but not asserted** at this task. The task text is explicit that the
absolute floor is not asserted here, because this run instruments `scripts/dependencies` as well as
`scripts/vscode` while the P0-T18 baseline instruments `scripts/vscode` alone, so the two
aggregates measure different populations and are not comparable. The gate at this task is the
per-file no-regression comparison below. The absolute floor is asserted from P4-T3 onward, once
P3-T4 and P3-T5 give `scripts/vscode/Sync-PackageReferences.ps1` its own suite; until then that
file contributes 0 covered of 84 lines.

For completeness: the floor when it is asserted is **80** percent, the figure the execution
worktree's `CLAUDE.md` states under issue #563, not the 85 in
`.claude/rules/general-unit-test.md`. Gate rule 13 records the authority and the open discrepancy
at issue #668.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| `EXIT_CODE: 0` | 0 | PASS |
| `Failed=0` | 0 | PASS |
| `Total` greater than the `Total` P0-T18 recorded | **206** against **174** | PASS |
| `sourcefile` LINE percentage for `PackageGraph.psm1` recorded and at least 90 | **100.00** (164 covered, 0 missed, 164 total) | PASS |
| For every `sourcefile` under `scripts/vscode`, covered and missed recorded and covered >= the P0-T18 value for that file | all 14 files equal, none below | PASS |

## Per-file no-regression comparison over the shared `scripts/vscode` population

The comparison is stated per file rather than in aggregate, for the reason recorded above. The
`sourcefile` element's `name` attribute in this document carries the folder prefix
(`vscode/Invoke-MSTest.ps1`); the file names below are the leaf names, which is the same population
P0-T18 tabulated.

| `sourcefile` | P0-T18 covered | This run covered | Missed | Total | Percent | Verdict |
|---|---|---|---|---|---|---|
| `Install-RepoDotNetSdk.ps1` | 13 | **13** | 20 | 33 | 39.39 | no regression |
| `Invoke-MSTest.ps1` | 49 | **49** | 7 | 56 | 87.50 | no regression |
| `Invoke-MSTest.TrxSummary.ps1` | 40 | **40** | 2 | 42 | 95.24 | no regression |
| `Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | **93** | 0 | 93 | 100.00 | no regression |
| `Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | **32** | 1 | 33 | 96.97 | no regression |
| `Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | **204** | 8 | 212 | 96.23 | no regression |
| `Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | **18** | 0 | 18 | 100.00 | no regression |
| `Invoke-MSTestWithCoverage.Projection.ps1` | 39 | **39** | 1 | 40 | 97.50 | no regression |
| `Invoke-MSTestWithCoverage.ps1` | 113 | **113** | 13 | 126 | 89.68 | no regression |
| `Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | **33** | 0 | 33 | 100.00 | no regression |
| `Invoke-Restore.ps1` | 22 | **22** | 1 | 23 | 95.65 | no regression |
| `Invoke-VSBuild.ps1` | 46 | **46** | 3 | 49 | 93.88 | no regression |
| `Sync-PackageReferences.ps1` | 0 | **0** | 84 | 84 | 0.00 | no regression |
| `TestProcessCleanup.ps1` | 29 | **29** | 0 | 29 | 100.00 | no regression |

Fourteen files compared, fourteen equal, none below baseline. The `scripts/vscode` sub-population
totals 731 covered of 871 — identical to the P0-T18 aggregate — which is the expected result: no
Batch A task modifies any file in that folder.

## The new module

| `sourcefile` | Covered | Missed | Total | Percent |
|---|---|---|---|---|
| `dependencies/PackageGraph.psm1` | 164 | 0 | 164 | **100.00** |

Every instrumented line of the module created at P1-T4 is executed by the suite created at P1-T5,
against the `>= 90` per-new-module requirement. This run also supersedes P1-T6 as the behavioural
confirmation for `Get-PackageManifestPath`: P1-T6 ran before the `[OutputType([string[]])]`
correction P2-T2 required, and this run exercises the corrected module with all four of that
function's cases passing.

## Non-vacuity

`Total=206` against the baseline's 174 is the positive guard. A discovery glob that matched
nothing would report `Total=0`, and a run that silently dropped the new suite would report 174
rather than 206. The 32-test increase is accounted for: the P1-T5 suite contributes 32 `It` blocks
across 7 `Describe` blocks, and 174 plus 32 is 206.

The per-file comparison is likewise positive rather than absence-shaped: it asserts 14 specific
covered counts against 14 recorded values, so a coverage document that instrumented nothing would
report 0 covered for every file and fail, rather than passing vacuously.

## Evidence-form limitation, per gate rule 12

The figures above are recorded in this `.md` artifact and **stand in for a permitted evidence form
that does not exist for the PowerShell route**. All three forms the authoritative `CLAUDE.md`
`## Committed Test Evidence Format` section permits — the package-level JaCoCo projection of a
post-processed Cobertura document, the one-line first-party coverage summary, and the trx-derived
test-result summary — are defined against the C# route. A Pester run emits JaCoCo directly with no
Cobertura stage, and `ConvertTo-JacocoPackageProjection` accepts Cobertura only, so none of the
three can be produced for this run. These recorded figures are a fourth form the section does not
define. The gap is stated rather than closed, because closing it would mean either committing the
prohibited collector document or building a Cobertura stage this change has no reason to build.

The collector document itself is at `coverage/p2-t3-pester-coverage.xml`, which `.gitignore:144`
ignores. It is read there and left there; no `.xml` is written under `<FEATURE>/evidence/` and no
commit pathspec carries one.

Output Summary: CMD-PESTER-ALL returned EXIT_CODE 0 with
`PESTER Passed=206 Failed=0 Skipped=0 Total=206`, against the 174 P0-T18 recorded.
`PackageGraph.psm1` reports **164 covered of 164**, 100.00 percent, above the `>= 90` per-new-module
requirement. All 14 `scripts/vscode` files hold their P0-T18 covered counts exactly, so the
per-file no-regression gate passes with no file below baseline. The aggregate over the widened
two-folder population is 86.47 percent, recorded as an observation only because the absolute floor
is first asserted at P4-T3.
