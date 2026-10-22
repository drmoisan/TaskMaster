# P5-T5 — Final QC versus P0-T7 baseline delta (iteration 3, final)

Timestamp: 2026-09-02T23-29

Supersedes `evidence/qa-gates/toolchain-delta.2026-09-02T23-09.md`, which recorded criterion (d)
as NOT MET at iteration 2. That verdict was correct for the tree as it stood then; this artifact
records the tree after the targeted remediation that closed the gap.

Sources compared:
- Baseline: `evidence/baseline/poshqc-test.2026-09-02T21-50.md` (P0-T7).
- Final QC: `evidence/qa-gates/poshqc-test.iter3.2026-09-02T23-27.md` (P5-T4, iteration 3).

Scope note. The baseline run used `Run.Path` = the whole `tests/scripts/vscode` folder and
recorded 70 passed, of which 8 belong to two files outside this plan's write set
(`Install-RepoDotNetSdk.Tests.ps1` = 2, `Invoke-VSBuild.Tests.ps1` = 6). The in-scope baseline is
therefore 62. The final QC run used `Run.Path` = the 8 write-set test files exactly, and recorded
84. Every count below is stated on the in-scope basis so the two runs are comparable.

## (a) Net new It-case count

In-scope baseline: 62 It cases (25 + 11 + 26).
Final QC: 84 It cases.
Net change: +22.

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

The remaining +13 come from two orchestrator-directed tasks that sit outside the numbered plan and
have no plan checkbox:

| Task | Test file | It description |
|---|---|---|
| H1 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a value that is itself an array when discovery matches exactly one assembly |
| H1 | Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | returns a value that is itself an array when discovery matches nothing |
| R1 | Invoke-MSTest.Main.Tests.ps1 | returns the off-root CLI runsettings path alongside the script directory |
| R1 | Invoke-MSTest.Main.Tests.ps1 | fails fast with a specific error naming the missing runsettings path |
| R1 | Invoke-MSTest.Main.Tests.ps1 | forwards every argument array element as a separate positional argument |
| R1 | Invoke-MSTest.Main.Tests.ps1 | fails when the search root cannot be found |
| R1 | Invoke-MSTest.Main.Tests.ps1 | fails when vswhere.exe is not installed |
| R1 | Invoke-MSTest.Main.Tests.ps1 | fails when vswhere resolves no vstest.console.exe |
| R1 | Invoke-MSTest.Main.Tests.ps1 | fails when discovery finds no test assemblies, naming the search root and configuration |
| R1 | Invoke-MSTest.Main.Tests.ps1 | returns before launching vstest.console.exe when NoExecute is supplied |
| R1 | Invoke-MSTest.Main.Tests.ps1 | launches vstest.console.exe with the discovered assemblies and the resolved runsettings |
| R1 | Invoke-MSTest.Main.Tests.ps1 | defaults the search root to the repository root and the configuration to Debug |
| R1 | Invoke-MSTest.Main.Tests.ps1 | throws naming the exit code when vstest.console.exe returns a nonzero status |

9 + 2 + 11 = 22, reconciling with 62 + 22 = 84.

Per-file reconciliation, accounting for the two file splits Phase 1 and Phase 4 performed under
the plan's own file-size tasks (P1-T14, P4-T1) and the new file added by task R1:

| Test file | Baseline | Final QC | Change | Explanation |
|---|---|---|---|---|
| Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 25 | 20 | -5 | 5 `Assert-CoberturaLineCoverageThreshold` cases moved out to Threshold.Tests.ps1 by the P1-T14 size check; none removed |
| Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | did not exist | 5 | +5 | the 5 moved cases, unchanged in text |
| Invoke-MSTestWithCoverage.Merge.Tests.ps1 | did not exist | 2 | +2 | P1-T5 and P1-T6 |
| Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | did not exist | 2 | +2 | P1-T1 and P1-T2 |
| Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 11 | 12 | +1 | P3-T3 |
| Invoke-MSTest.RunSettings.Tests.ps1 | 26 | 27 | +1 | P2-T1 |
| Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | did not exist | 5 | +5 | P4-T2 (3) and H1 (2) |
| Invoke-MSTest.Main.Tests.ps1 | did not exist | 11 | +11 | R1 |
| **In-scope total** | **62** | **84** | **+22** | |

The Helpers/Threshold split is count-neutral: 25 becomes 20 + 5.

## (b) The deliberate assertion reversal, and why this gate is not vacuous

The existing test `preserves the primary class methods subtree and every hits value when merging`
lives at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` line 320. Its
assertion was deliberately reversed by P1-T4, per spec.md's Risks & Mitigations section.

Before (baseline):

```
$methodNodes.Count | Should -Be 1
$methodNodes[0].name | Should -Be 'M'
```

After (final QC, lines 350-351, re-derived against the current tree on this iteration):

```
$methodNodes.Count | Should -Be 2
(@($methodNodes | ForEach-Object { $_.name }) -join ',') | Should -Be 'M,N'
```

Its comment at line 321 was correspondingly rewritten from "Locks the decision not to merge or
strip `<methods>`" to "Locks the union-merge decision for `<methods>` (issue #733, finding 2)".

This test is counted as passing in the iteration-3 run: it resides in
`Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, which reported 20 passed / 0 failed / 0 skipped, so
all 20 of its cases including this one passed under the post-fix assertion
`$methodNodes.Count | Should -Be 2`. The gate is therefore not vacuous with respect to the
finding-2 behavior change: the same test that pinned the old single-method behavior now pins the
union-merge behavior, and it was observed failing under the new assertion before the fix landed
(`evidence/regression-testing/case-04-methods-union-existing-test.2026-09-02T22-21.md`). The
remediation on this iteration touched only `scripts/vscode/Invoke-MSTest.ps1` and three files
under `tests/scripts/vscode`, none of them this test's file, so the reversal is carried forward
unchanged.

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
| tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | 0 |

Skipped equals 0 for every named test file, and the run total is Skipped = 0.

## (d) Per-production-file coverage against the 85 percent floor — MET

| Production file | Baseline percent | Iteration 2 percent | Final QC percent | At or above 85% |
|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 90.2 (230/255) | 90.84 (228/251) | 90.84 (228/251) | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 90.09 (100/111) | 90.09 (100/111) | 90.09 (100/111) | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 100 (111/111) | 100 (111/111) | 100.00 (111/111) | yes |
| scripts/vscode/Invoke-MSTest.ps1 | 68.89 (31/45) | 72.34 (34/47) | 94.00 (47/50) | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | did not exist | 100 (25/25) | 100.00 (25/25) | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | did not exist | 88.24 (15/17) | 88.24 (15/17) | yes |

Aggregate: 90.42 percent over 522 commands in 4 files at baseline; 93.10 percent over 565 commands
in 6 files at final QC.

**Criterion (d) is MET.** All six production files in this plan's write set are at or above the
uniform 85 percent line-coverage floor in `.claude/rules/powershell.md` and
`.claude/rules/quality-tiers.md`.

### How the Invoke-MSTest.ps1 shortfall was closed

At iteration 2 the file sat at 72.34 percent with 13 missed commands, all of them in its
unextracted top-level host-bound script body. `.claude/rules/general-unit-test.md`'s Coverage
Exclusion Policy forbids excluding a production file from measurement and prescribes the remedy
directly: extract the logic into host-neutral, testable units and leave only the thinnest possible
wiring in the host-bound entry point. That is what was done, following the shape the sibling
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` already uses for
`Invoke-MSTestWithCoverageMain`:

- The whole top-level body was moved into a new `Invoke-MSTestMain` function taking `SearchRoot`,
  `Configuration`, `NoExecute`, and `ScriptRoot`. Every guard, message, throw text, ordering, and
  the `-NoExecute` early return are byte-identical to the pre-remediation body.
- The one remaining direct external-process invocation, the `vswhere.exe` lookup, was moved behind
  a `Get-VsTestConsolePath` seam, mirroring the `Invoke-VsTestExe` seam already present in the same
  file and the `Invoke-VsWhereExe` seam in the sibling file. This is what makes the guards
  reachable from Pester without launching Visual Studio tooling.
- The top level now holds `Set-StrictMode`, `$ErrorActionPreference`, and a dot-source-guarded
  `Invoke-MSTestMain @PSBoundParameters`, matching the sibling's
  `if ($MyInvocation.InvocationName -ne '.')` guard.

No file was exempted, no file was added to any exclude list, and no threshold was changed.

The 3 remaining missed commands are:

| Line | Command | Why it remains uncovered |
|---|---|---|
| 93 | the `& $VsWherePath ...` pipeline in `Get-VsTestConsolePath` | covering it requires launching a real `vswhere.exe`, which the unit-test policy prohibits |
| 94 | the `Select-Object -First 1` half of the same pipeline | same pipeline as line 93 |
| 201 | `Invoke-MSTestMain @PSBoundParameters` | the host-bound entry point itself, one forwarding call |

### No-regression check on pre-existing lines

No production file's coverage percentage decreased relative to the P0-T7 baseline:

| Production file | Baseline | Final QC | Direction |
|---|---|---|---|
| Invoke-MSTestWithCoverage.Helpers.ps1 | 90.2 | 90.84 | up |
| Invoke-MSTestWithCoverage.ps1 | 90.09 | 90.09 | unchanged |
| Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 100 | 100.00 | unchanged |
| Invoke-MSTest.ps1 | 68.89 | 94.00 | up |
| Invoke-MSTestWithCoverage.PackageRate.ps1 | did not exist | 100.00 | new file |
| Invoke-MSTestWithCoverage.Threshold.ps1 | did not exist | 88.24 | new file, holds code moved out of Helpers.ps1 |

Two files changed shape rather than losing coverage:

- `Invoke-MSTestWithCoverage.Helpers.ps1`: total commands fell from 255 to 251 because P1-T14's
  size check moved `Assert-CoberturaLineCoverageThreshold` out to
  `Invoke-MSTestWithCoverage.Threshold.ps1`. The moved function's own coverage is now measured on
  the new file at 88.24 percent, so none of it left the denominator.
- `Invoke-MSTest.ps1`: total commands rose from 45 to 50. 47 are executed against 31 at baseline,
  and missed fell from 14 to 3.

Branch coverage: not emitted by Pester 5, at baseline and at final QC alike. Measured fact.

## Re-confirmation of task H1 after the remediation

Task H1's two array-shape assertions live in
`tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1`. Both are recorded passing in the
iteration-3 run:

- `returns a value that is itself an array when discovery matches exactly one assembly`
- `returns a value that is itself an array when discovery matches nothing`

`Get-MSTestAssemblyPathList` is confirmed to return an array at all three cardinalities on this
run, by these five passing cases in the same file: `returns an empty array when discovery matches
nothing` (zero), `returns a single-element array when discovery matches exactly one assembly`
(one), `returns every match when discovery matches multiple assemblies` (many), plus the two H1
shape assertions above. The unary comma in the function's `return` is unchanged by the
remediation, and the function itself was moved neither in text nor in behavior.

## Output Summary

- Counts: 62 in-scope at baseline, 84 at final QC. Net +22, of which 9 are plan-attributable, 2
  come from task H1, and 11 from remediation task R1. Failed 0, Skipped 0, direct-run EXIT_CODE 0.
- Criteria (a), (b), (c), and (d) are all met.
- All six production files are at or above the 85 percent floor: 90.84, 90.09, 100.00, 94.00,
  100.00, 88.24.
- No production file's coverage decreased against the P0-T7 baseline.
- P5-T5's acceptance is satisfied and its plan checkbox is checked.
