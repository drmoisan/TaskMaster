# P7-T7 — New-Code Coverage For The Two New Pure Part Files

Timestamp: 2026-09-13T07-14
Task: [P7-T7]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; Import-Module Pester -MinimumVersion 5.0; $cfg = New-PesterConfiguration; $cfg.Run.Path = @("tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1","tests/scripts/vscode/Invoke-MSTest.TrxSummary.Tests.ps1"); $cfg.Run.PassThru = $true; $cfg.Output.Verbosity = "None"; $cfg.CodeCoverage.Enabled = $true; $cfg.CodeCoverage.Path = @("scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1","scripts/vscode/Invoke-MSTest.TrxSummary.ps1"); $cfg.CodeCoverage.OutputFormat = "JaCoCo"; $cfg.CodeCoverage.OutputPath = "docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml"; $r = Invoke-Pester -Configuration $cfg; "PESTER_COUNTS: passed=" + $r.PassedCount + " failed=" + $r.FailedCount + " skipped=" + $r.SkippedCount; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'

EXIT_CODE: 0

Coverage output path:
`docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/evidence/qa-gates/p7-t7-new-code-coverage.jacoco.xml`

The `qa-gates` kind is used deliberately. There is no coverage evidence kind in this repository's
evidence scheme, and an artifact written under one would fail the evidence-path rule. No
repository-level coverage file was created anywhere outside this feature folder.

This task ran after P7-T1, so the figures apply to the formatted files.

## Derivation method

Per-file line coverage is derived from the emitted JaCoCo document by locating the `sourcefile`
element whose `name` attribute is the part file's leaf name and computing the count of its `line`
elements whose covered-instruction attribute `ci` is greater than zero against the total count of its
`line` elements. Any document type declaration is stripped from the emitted text before parsing.

The aggregate coverage percentage the runner exposes is not used. It is an aggregate across every
analysed file and cannot render a per-file verdict, which is what the 90 percent new-code floor
requires.

## Pass 1 — FAILED the gate, recorded rather than discarded

Timestamp: 2026-09-13T07-08

```
PESTER_COUNTS: passed=16 failed=0 skipped=0
```

| Part file | Line elements | Covered | Percent | Verdict against 90 |
|---|---|---|---|---|
| `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` | 42 | 39 | 92.86 | pass |
| `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 40 | 33 | 82.50 | FAIL |

Uncovered lines on pass 1:

- `Invoke-MSTest.TrxSummary.ps1`: 56, 70, 132.
- `Invoke-MSTestWithCoverage.Projection.ps1`: 121, 143, 187, 188, 189, 192, 194.

Diagnosis of the projection shortfall. Three distinct regions were unexercised by the two test files
this gate measures:

1. Line 121, the throw taken when the source document carries no root `coverage` element.
2. Line 143, the throw taken when the summed LINE missed plus covered total disagrees with the
   source root `lines-valid` attribute. `Assert-JacocoProjectionReconciliation` enforces two
   independent equalities and P1-T5's negative test exercises only the first of them, the
   covered-total equality at line 138.
3. Lines 187 through 194, the whole body of `Test-RawCoverageDocumentRetained`. That function is
   tested, but by `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`, which
   this gate's fixed two-file run path does not include, so its coverage is invisible to this
   measurement.

## Remediation

Two tests were added to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`. That
file is inside this delivery's declared Write Set, so the remediation widened no footprint. No
production file was changed, and no file outside the Write Set was touched.

- `throws naming the expected and the observed valid totals when the missed counters disagree`,
  which exercises the second reconciliation equality at line 143. The projection is written directly
  rather than derived, so the summed covered total agrees with the source root and the first check
  cannot fire; the valid-total check is the one under test.
- `returns false for an output path that has no parent directory`, which exercises the guard clause
  at lines 187 through 189. This is a genuinely untested boundary rather than a duplicate of the
  results-directory tests: those three tests all supply a full path, so none of them reaches the
  empty-parent branch.

Neither test writes, reads or deletes a file, and neither loads a fixture from a path.

The toolchain loop was then restarted from P7-T1, as the General Code Change Policy requires when a
step changes files. P7-T1, P7-T2, P7-T3, P7-T4, P7-T5 and P7-T6 were all re-run and all passed on
that second pass; each of their artifacts carries a `Pass 2` section recording the re-run.

## Pass 2 — operative result

Timestamp: 2026-09-13T07-14

```
PESTER_COUNTS: passed=18 failed=0 skipped=0
```

PESTER_FAILED_COUNT: 0

The passed count rose from 16 to 18, which is the two added tests, both passing.

### Per-file line coverage

PER_FILE_LINE_COVERAGE_TRXSUMMARY_PERCENT: 92.86
PER_FILE_LINE_COVERAGE_PROJECTION_PERCENT: 92.50

| Part file | Line elements | Covered | Percent | Verdict against 90 |
|---|---|---|---|---|
| `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` | 42 | 39 | 92.86 | pass |
| `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` | 40 | 37 | 92.50 | pass |

Uncovered lines remaining, recorded so the residual is visible rather than implied:

- `Invoke-MSTest.TrxSummary.ps1`: 56, 70, 132.
- `Invoke-MSTestWithCoverage.Projection.ps1`: 121, 192, 194.

Lines 192 and 194 of the projection part file are the retained-directory arithmetic of
`Test-RawCoverageDocumentRetained`. They are exercised by the three predicate tests in
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`, which this gate's fixed
run path excludes, so they are covered by the delivery's test suite but not by this measurement.
Line 121 is the no-root-element throw and remains unexercised by either test file.

## Output Summary

EXIT_CODE: 0 with 18 passed, 0 failed, 0 skipped. Both per-file line-coverage percentages are at
least 90: `Invoke-MSTest.TrxSummary.ps1` at 92.86 percent and
`Invoke-MSTestWithCoverage.Projection.ps1` at 92.50 percent. The first measurement failed at 82.50
percent for the projection part file and is recorded above together with its diagnosis and the
in-Write-Set remediation that closed it; the toolchain loop was restarted from P7-T1 and every
preceding Phase 7 gate was re-run and passed.
