# P9-T9 — Coverage reconciliation

Timestamp: 2026-09-20T09-44

## C# — baseline, post-change and delta

The baseline read here is the pair **P2-T7 measured on the compiling tree**, from
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md`.
The post-change pair is from
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md`.

| Metric | Baseline, P2-T7 | Post-change, P9-T7 | Delta, fractional | Delta, points |
|---|---|---|---|---|
| Line | 0.8593 (56486/65737) | **0.8591** (56476/65737) | **-0.0002** | -0.02 |
| Branch | 0.8009 (13657/17052) | **0.8007** (13654/17052) | **-0.0002** | -0.02 |

The denominators are identical at 65737 lines and 17052 branches, which is the expected result for a
change that modifies no `.cs` file. The numerators differ by 10 covered lines and 3 covered branches,
a run-to-run variation in a 7343-test parallel suite rather than a change in the measured population.

### Why the preflight pair is not the baseline

The preflight figures of `0.820056` line and `0.782406` branch are **not** used. They were taken
before the #898 correction, on a tree that did not fully compile, so reading them as the baseline
would report a spurious improvement of 3.92 line points and 2.65 branch points that this change did
not produce. Gate rule 14 names the class: a baseline figure is assertable later only if it is
invariant under the transformations in between, and the #898 correction is exactly such a
transformation.

### Floors and margins

| Metric | Post-change | Runner floor | Margin |
|---|---|---|---|
| Line | 0.8591 | 0.80 | +5.91 points |
| Branch | 0.8007 | 0.75 | +5.07 points |

The floors are the ones `scripts/vscode/Invoke-MSTestWithCoverage.ps1` enforces; the run exited 0,
which is itself the runner's assertion that neither floor was breached.

### Regression verdict

| Clause | Required | Observed | Result |
|---|---|---|---|
| C# post-change line coverage | at least 0.80 | 0.8591 | PASS |
| C# post-change branch coverage | at least 0.75 | 0.8007 | PASS |
| C# line delta | at least -0.005 | -0.0002 | PASS |
| C# branch delta | at least -0.005 | -0.0002 | PASS |

Both deltas lie between -0.005 and 0, so both are **recorded as within measurement noise and are not
regression findings**. A delta below -0.005 would be a blocking regression; neither is.

## PowerShell — baseline, post-change and per-module

Aggregate baseline from
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/baseline/p0-t18-pester.2026-09-19T09-44.md`;
post-change aggregate and per-module figures from
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md`.

| Measurement | Baseline, P0-T18 | Post-change, P9-T3 | Delta, points |
|---|---|---|---|
| Aggregate LINE percentage | 83.93 | **93.89** | +9.96 |

The two aggregates are computed over different populations: the P0-T18 baseline could only measure
`scripts/vscode`, because `scripts/dependencies` did not exist until P1-T4. The delta is therefore
recorded as an observation and is not read as a like-for-like improvement. The acceptance clause the
aggregate must satisfy is the floor, not the delta.

### Per-new-module figures

| Module | LINE percentage | Required |
|---|---|---|
| `scripts/dependencies/PackageGraph.psm1` | **100.00** | at least 90 |
| `scripts/dependencies/PackageCompatibility.psm1` | **100.00** | at least 90 |
| `scripts/dependencies/AnalyzerItemRepair.psm1` | **100.00** | at least 90 |
| `scripts/dependencies/ProjectConsistency.psm1` | **100.00** | at least 90 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | **98.74** | at least 90 |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | **94.12** | at least 90 |

`scripts/vscode/Sync-PackageReferences.ps1` moved from **0 covered of 84** at P0-T18 to **95 covered
of 127**, which satisfies the strictly-greater-than-zero clause P9-T3 asserts. It is a rewritten file
rather than a new module and the at-least-90 clause does not name it; its figure of 74.80 is recorded
as measured.

### PowerShell verdict

| Clause | Required | Observed | Result |
|---|---|---|---|
| PowerShell aggregate | at least 80, per gate rule 13 | 93.89 | PASS |
| Every new module | at least 90 | 94.12 lowest | PASS |

The floor of 80 is the figure the execution worktree `CLAUDE.md` states under issue #563. The
at-least-90 per-module requirement is this change's own stricter requirement on its own code and is
unaffected by the floor.

## PowerShell branch coverage

**No branch figure exists for PowerShell.** Pester emits no branch counter in any output format, in
JaCoCo or otherwise, so the branch threshold is unevaluable for this language. The tooling reason is
named rather than the threshold waived: this is a capability limit of Pester, not an exclusion of any
file from measurement. Every production PowerShell file under `scripts/dependencies` and
`scripts/vscode` is in the line-coverage denominator recorded at P9-T3.

## Every figure is a number

No placeholder appears above. The eight C# figures, the two PowerShell aggregates, the six
per-module percentages and the `Sync-PackageReferences.ps1` counter are all numeric and all read from
the artifacts named at the head of each section.
