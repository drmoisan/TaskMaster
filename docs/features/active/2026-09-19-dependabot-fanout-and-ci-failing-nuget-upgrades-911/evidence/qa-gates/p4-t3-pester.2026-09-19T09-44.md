# P4-T3 — Pester suite with coverage, Batch B close-out

Timestamp: 2026-09-20T01-08

Command: CMD-PESTER-ALL with `<OUTPATH>` set to `coverage/p4-t3-pester-coverage.xml`.

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies","tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p4-t3-pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

Per gate rule 4, the explicit `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` placed after
the count-emitting statement is what makes the exit code meaningful.

## Verbatim result line

```
PESTER Passed=227 Failed=0 Skipped=0 Total=227
```

227 against the 206 P2-T3 recorded at the Batch A boundary: the 21 new cases are the 8 from
`PackageCompatibility.Tests.ps1`, the 6 from `Sync-PackageReferences.Tests.ps1`, and the 7 from
`DependabotConfig.Tests.ps1` (5 `AC1-` plus 2 `AC4-`).

## Aggregate JaCoCo LINE coverage

Read from the report-level `counter` element of type `LINE` in
`coverage/p4-t3-pester-coverage.xml`.

| Measurement | Value |
|---|---|
| Covered lines | 1023 |
| Missed lines | 88 |
| Total instrumented lines | 1111 |
| **Aggregate LINE percentage** | **92.08** |

Computed as `covered / (covered + missed) * 100` = `1023 / 1111 * 100`.

**92.08 is at least 80**, the floor the execution worktree's `CLAUDE.md` states under issue #563,
per gate rule 13 — not the 85 in `.claude/rules/general-unit-test.md`, which that rule records as
superseded push-down-owned boilerplate with the discrepancy tracked at open issue #668. The figure
clears both, so the choice of floor does not decide this gate, but the authority is stated for the
record.

The absolute floor first becomes assertable at this task because P3-T4 and P3-T5 gave
`scripts/vscode/Sync-PackageReferences.ps1` its first suite. Its baseline of 0 covered of 84 lines
is what held the merge-base `scripts/vscode` population at 83.93 percent.

## Per-file LINE counters, all 16 instrumented files

| `sourcefile` name | Covered | Missed | Total | Percent |
|---|---|---|---|---|
| `dependencies/PackageCompatibility.psm1` | 33 | 0 | 33 | 100.00 |
| `dependencies/PackageGraph.psm1` | 164 | 0 | 164 | 100.00 |
| `vscode/Install-RepoDotNetSdk.ps1` | 13 | 20 | 33 | 39.39 |
| `vscode/Invoke-MSTest.ps1` | 49 | 7 | 56 | 87.50 |
| `vscode/Invoke-MSTest.TrxSummary.ps1` | 40 | 2 | 42 | 95.24 |
| `vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | 93 | 0 | 93 | 100.00 |
| `vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` | 32 | 1 | 33 | 96.97 |
| `vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | 204 | 8 | 212 | 96.23 |
| `vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` | 18 | 0 | 18 | 100.00 |
| `vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 39 | 1 | 40 | 97.50 |
| `vscode/Invoke-MSTestWithCoverage.ps1` | 113 | 13 | 126 | 89.68 |
| `vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 33 | 0 | 33 | 100.00 |
| `vscode/Invoke-Restore.ps1` | 22 | 1 | 23 | 95.65 |
| `vscode/Invoke-VSBuild.ps1` | 46 | 3 | 49 | 93.88 |
| **`vscode/Sync-PackageReferences.ps1`** | **95** | **32** | **127** | **74.80** |
| `vscode/TestProcessCleanup.ps1` | 29 | 0 | 29 | 100.00 |

A naming note for later readers: with the two-member `CodeCoverage.Path` the `sourcefile` `name`
attribute carries a package-directory prefix, whereas the single-member Phase 0 baseline emitted
bare leaf names. Selection by leaf name is the form that reads both documents.

## The two per-file clauses

| File | Clause | Baseline | Measured | Verdict |
|---|---|---|---|---|
| `PackageCompatibility.psm1` | LINE percentage at least 90 | n/a, new module | **100.00** | PASS |
| `Sync-PackageReferences.ps1` | LINE percentage strictly greater than the P0-T18 value | **0.00**, 0 covered of 84 | **74.80**, 95 covered of 127 | PASS |

The `Sync-PackageReferences.ps1` comparison is stated as strictly greater than the baseline rather
than against an absolute floor, which is what the task requires. The instrumented line count rose
from 84 to 127 because P3-T4 rewrote the file from 159 to 423 lines with comment-based help and
eight functions; the comparison the task names is of the **percentage** against the baseline
percentage, and 74.80 is strictly greater than 0.00. The covered-line count likewise rose from 0
to 95.

The 32 missed lines in that file are concentrated in `Get-PackageSyncSeam`, whose seven delegate
bodies are the production filesystem calls and are by design never executed under test — the
suites substitute an in-memory table for exactly that reason — together with the invocation guard
and the two `Write-Warning` diagnostic paths.

## No-regression over the shared `scripts/vscode` population

Recorded for continuity with P2-T3's per-file gate, though this task's stated acceptance is the
absolute floor rather than the per-file comparison. Every `scripts/vscode` file's covered count
equals its P0-T18 value except `Sync-PackageReferences.ps1`, which rose from 0 to 95. No file
regressed.

## Evidence-form limitation, per gate rule 12

This task is one of the six that record a JaCoCo LINE figure, so the standing-in statement is
required of it. The figures above are recorded in this `.md` artifact and **stand in for a
permitted evidence form that does not exist for the PowerShell route**. All three forms the
authoritative `CLAUDE.md` `## Committed Test Evidence Format` section permits — the package-level
JaCoCo projection of a post-processed Cobertura document, the one-line first-party coverage
summary, and the trx-derived test-result summary — are defined against the C# route. A Pester run
emits JaCoCo directly with no Cobertura stage, and `ConvertTo-JacocoPackageProjection` accepts
Cobertura only, so none of the three can be produced for this run. These recorded figures are a
fourth form the section does not define. The gap is stated rather than closed, because closing it
would mean either committing the prohibited collector document or building a Cobertura stage this
change has no reason to build.

The collector document itself is at `coverage/p4-t3-pester-coverage.xml`, which `.gitignore:144`
ignores. It is read there and left there; no `.xml` is written under `<FEATURE>/evidence/` and no
commit pathspec carries one.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Failed` | 0 | 0 | PASS |
| Aggregate JaCoCo LINE percentage, two decimals | at least 80 | **92.08** | PASS |
| `PackageCompatibility.psm1` LINE percentage | at least 90 | **100.00** | PASS |
| `Sync-PackageReferences.ps1` LINE percentage | strictly greater than the P0-T18 value of 0.00 | **74.80** | PASS |

`Failed=0` is guarded by `Total=227`, which is greater than the 206 the previous full run
recorded, so a run that discovered nothing is distinguishable from a clean one.

Output Summary: CMD-PESTER-ALL returned EXIT_CODE 0 with
`PESTER Passed=227 Failed=0 Skipped=0 Total=227`, up from 206 at the Batch A boundary. Aggregate
JaCoCo LINE coverage over `scripts/dependencies` and `scripts/vscode` is **92.08** percent, 1023
covered of 1111 instrumented lines, above the authoritative 80 percent floor.
`PackageCompatibility.psm1` reports **100.00** percent, 33 of 33, against a required 90.
`Sync-PackageReferences.ps1` reports **74.80** percent, 95 covered of 127, strictly greater than
its P0-T18 baseline of 0 covered of 84 and the first coverage that file has ever had. No
`scripts/vscode` file regressed against the baseline. The recorded figures stand in for a
permitted evidence form that does not exist for the PowerShell route.
