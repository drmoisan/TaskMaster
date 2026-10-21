# P5-T5 — Final QC versus P0-T7 baseline delta (iteration 2)

Timestamp: 2026-09-02T23-09

SUPERSEDED by `evidence/qa-gates/toolchain-delta.2026-09-02T23-29.md`. The criterion (d) failure
recorded below was accurate for the tree at iteration 2 and is retained as the audit record of
that state. It was closed at iteration 3 by extracting `scripts/vscode/Invoke-MSTest.ps1`'s
host-bound top-level body into `Invoke-MSTestMain`, which raised that file from 72.34 percent to
94.00 percent. Read the iteration-3 artifact for the current verdict.

Sources compared:
- Baseline: `evidence/baseline/poshqc-test.2026-09-02T21-50.md` (P0-T7).
- Final QC: `evidence/qa-gates/poshqc-test.iter2.2026-09-02T23-07.md` (P5-T4, iteration 2).

Scope note. The baseline run used `Run.Path` = the whole `tests/scripts/vscode` folder and
recorded 70 passed, of which 8 belong to two files outside this plan's write set
(`Install-RepoDotNetSdk.Tests.ps1` = 2, `Invoke-VSBuild.Tests.ps1` = 6). The in-scope baseline is
therefore 62. The final QC run used `Run.Path` = the 7 write-set test files exactly, as P5-T4
specifies, and recorded 73. Every count below is stated on the in-scope basis so the two runs are
comparable.

## (a) Net new It-case count

In-scope baseline: 62 It cases (25 + 11 + 26).
Final QC: 73 It cases.
Net change: +11.

Attribution, by plan task:

| Task | Test file | It description | New or updated |
|---|---|---|---|
| P1-T1 | Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | accumulates line and branch totals across every class in the package | new |
| P1-T2 | Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | falls back to a zero rate when no class in the package carries any lines | new |
| P1-T3 | Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | computes the merged per-file line-rate from the merged rollup alone | updated (2 assertions added, no count change) |
| P1-T4 | Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | preserves the primary class methods subtree and every hits value when merging | updated (assertion reversed, no count change) |
| P1-T5 | Invoke-MSTestWithCoverage.Merge.Tests.ps1 | unions the methods of every group member into the merged class | new |
| P1-T6 | Invoke-MSTestWithCoverage.Merge.Tests.ps1 | takes the higher hits value when the second class seen for a filename is strictly higher | new |
| P2-T1 | Invoke-MSTest.RunSettings.Tests.ps1 | excludes assemblies discovered under a .claude worktree segment | new |
| P3-T3 | Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | retains a closure whose bare member name collides with a non-exempt overload | new |
| P4-T2 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns an empty array when discovery matches nothing | new |
| P4-T2 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a single-element array when discovery matches exactly one assembly | new |
| P4-T2 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns every match when discovery matches multiple assemblies | new |

Plan-attributable net new It cases across P1-T1 through P1-T6, P2-T1, P3-T3, and P4-T2: **9**.

The remaining +2 are the two array-shape assertions added by orchestrator-directed task H1, which
sits outside the numbered plan and has no plan checkbox:

| Task | Test file | It description |
|---|---|---|
| H1 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a value that is itself an array when discovery matches exactly one assembly |
| H1 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a value that is itself an array when discovery matches nothing |

9 + 2 = 11, reconciling with 62 + 11 = 73.

Per-file reconciliation, accounting for the two file splits Phase 1 and Phase 4 performed under
the plan's own file-size tasks (P1-T14, P4-T1):

| Test file | Baseline | Final QC | Change | Explanation |
|---|---|---|---|---|
| Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 25 | 20 | -5 | 5 `Assert-CoberturaLineCoverageThreshold` cases moved out to Threshold.Tests.ps1 by the P1-T14 size check; none removed |
| Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | did not exist | 5 | +5 | the 5 moved cases, unchanged in text |
| Invoke-MSTestWithCoverage.Merge.Tests.ps1 | did not exist | 2 | +2 | P1-T5 and P1-T6 |
| Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | did not exist | 2 | +2 | P1-T1 and P1-T2 |
| Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 11 | 12 | +1 | P3-T3 |
| Invoke-MSTest.RunSettings.Tests.ps1 | 26 | 27 | +1 | P2-T1 |
| Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | did not exist | 5 | +5 | P4-T2 (3) and H1 (2) |
| **In-scope total** | **62** | **73** | **+11** | |

The Helpers/Threshold split is count-neutral: 25 becomes 20 + 5.

## (b) The deliberate assertion reversal, and why this gate is not vacuous

The existing test `preserves the primary class methods subtree and every hits value when merging`
lives at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` line 320. Its
assertion was deliberately reversed by P1-T4, per spec.md's Risks & Mitigations section:

Before (baseline, lines 346-347):

```
$methodNodes.Count | Should -Be 1
$methodNodes[0].name | Should -Be 'M'
```

After (final QC, lines 350-351):

```
$methodNodes.Count | Should -Be 2
(@($methodNodes | ForEach-Object { $_.name }) -join ',') | Should -Be 'M,N'
```

Its comment at line 321 was correspondingly rewritten from "Locks the decision not to merge or
strip `<methods>`" to "Locks the union-merge decision for `<methods>` (issue #733, finding 2)".

This test is counted as passing in the final QC run: it resides in
`Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, which reported 20 passed / 0 failed / 0 skipped,
so all 20 of its cases including this one passed under the post-fix assertion
`$methodNodes.Count | Should -Be 2`. The gate is therefore not vacuous with respect to the
finding-2 behavior change: the same test that pinned the old single-method behavior now pins the
union-merge behavior, and it was observed failing under the new assertion before the fix landed
(`evidence/regression-testing/case-04-methods-union-existing-test.2026-09-02T22-21.md`).

## (c) Skipped counts

| Test file | Skipped |
|---|---|
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 0 |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | 0 |
| tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | 0 |

Skipped equals 0 for every named test file, and the run total is Skipped = 0.

## (d) Per-production-file coverage against the 85 percent floor — ONE FAILURE

| Production file | Baseline percent | Final QC percent | Change | At or above 85% |
|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 90.2 (230/255) | 90.84 (228/251) | +0.64 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 90.09 (100/111) | 90.09 (100/111) | 0.00 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 100 (111/111) | 100 (111/111) | 0.00 | yes |
| scripts/vscode/Invoke-MSTest.ps1 | 68.89 (31/45) | 72.34 (34/47) | +3.45 | **NO** |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | did not exist | 100 (25/25) | n/a | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | did not exist | 88.24 (15/17) | n/a | yes |

Aggregate: 90.42 percent over 522 commands in 4 files at baseline; 91.28 percent over 562
commands in 6 files at final QC.

**Criterion (d) is NOT MET.** `scripts/vscode/Invoke-MSTest.ps1` is at 72.34 percent, below the
uniform 85 percent line-coverage floor in `.claude/rules/powershell.md` and
`.claude/rules/quality-tiers.md`. The measured figure is recorded rather than waived. No file was
exempted and no production file was removed from the coverage denominator;
`.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy prohibits both, and
`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` — a production file this item created —
was deliberately kept in the denominator so the assertion could not pass vacuously.

The shortfall is pre-existing, not introduced: it was 68.89 percent at baseline and moved up
3.45 percentage points. The 13 remaining missed commands sit on 12 lines, enumerated in the
P5-T4 artifact: lines 31, 74, 124, 131, 136, 145, 146, 148, 150, 154, 155, 156 — five `throw`
guards, the `& $VsTestPath @VsTestArgs` external-process invocation, two `Write-Host` progress
lines, and the remaining top-level script body. Closing them requires extracting the whole
remaining top-level body into functions, or launching `vswhere.exe` and `vstest.console.exe` for
real. Neither is a task in this plan, and no plan task authorizes the additional extraction.

### No-regression check on pre-existing lines

No production file's coverage percentage decreased relative to the P0-T7 baseline. Two files
changed shape rather than losing coverage:

- `Invoke-MSTestWithCoverage.Helpers.ps1`: total commands fell from 255 to 251 because P1-T14's
  size check moved `Assert-CoberturaLineCoverageThreshold` out to
  `Invoke-MSTestWithCoverage.Threshold.ps1`. Missed commands fell from 25 to 23 and the
  percentage rose. The moved function's own coverage is now measured on the new file at 88.24
  percent, so none of it left the denominator.
- `Invoke-MSTest.ps1`: total commands rose from 45 to 47 because P4-T4 added
  `Get-MSTestAssemblyPathList`. Executed rose from 31 to 34 and missed fell from 14 to 13, so the
  extraction added covered commands and removed an uncovered one.

`Invoke-MSTestWithCoverage.ps1` and `Invoke-MSTestWithCoverage.ClosureFilter.ps1` are numerically
identical to baseline.

Branch coverage: not emitted by Pester 5, at baseline and at final QC alike. Measured fact.

## Output Summary

- Counts: 62 in-scope at baseline, 73 at final QC. Net +11, of which 9 are plan-attributable and
  2 come from task H1. Failed 0, Skipped 0, direct-run EXIT_CODE 0.
- Criteria (a), (b), and (c) are met.
- Criterion (d) is NOT met: `scripts/vscode/Invoke-MSTest.ps1` is at 72.34 percent against an
  85 percent floor. The no-regression half of (d) is met — no file decreased.
- This task's acceptance is therefore not fully satisfied, and its plan checkbox is left
  unchecked. Phase 5 is not reported as passing.
