# AC33 — Baseline versus Post-Change Coverage Comparison

Timestamp: 2026-09-09T17-22

Sources: evidence/baseline/coverage-baseline.cobertura.xml and
evidence/qa-gates/coverage-postchange.cobertura.xml

## Derivation

Every package-level and class-level figure below is derived, not read. A Cobertura package element
carries line-rate, branch-rate, complexity and name only, and a class element adds filename and
nothing more. Each package figure was produced with Get-CoberturaPackageLineSummary, declared at
scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1, and each per-filename figure with
Get-CoberturaClassLineSummary, declared at scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
line 158. Both deduplicate by line number, and the same pair produces the root-element attributes
recorded below, so the package sums reconcile with the root totals by construction. A hand-rolled
count of line descendants double-counts and does not reconcile.

## Root-element values

| Figure | Baseline | Post-change |
| --- | --- | --- |
| LineRate | 0.856211 | 0.856686 |
| LinesCovered | 56033 | 56029 |
| LinesValid | 65443 | 65402 |
| BranchRate | 0.79807 | 0.798366 |
| BranchesCovered | 13481 | 13486 |
| BranchesValid | 16892 | 16892 |

Raw LineRate and BranchRate are reported informationally only. A raw-rate no-regression gate is not
used and must not be substituted: deleting fully covered lines lowers the aggregate rate by
arithmetic even when every surviving line keeps its coverage. In this case both raw rates in fact
rose, but that is an outcome rather than the gate.

## The four decided gates

All figures are for the `UtilitiesCS` package element only. The repository-wide figure is reported
informationally and is not gated, because it aggregates eight further production assemblies this
feature does not touch, so a movement in any of them would be misattributed to this change.

UtilitiesCS package, baseline: LinesCovered 39014, LinesValid 43287, BranchesCovered 9363,
BranchesValid 11161.
UtilitiesCS package, post-change: LinesCovered 39010, LinesValid 43246, BranchesCovered 9368,
BranchesValid 11161.

**Gate A — PASS.** LinesValid post (43246) is less than or equal to baseline (43287). The reduction
is 41.

**Gate B — PASS.** LinesCovered post (39010) is greater than or equal to baseline LinesCovered minus
the reduction in LinesValid, that is 39014 - 41 = 38973. The observed 39010 exceeds that floor by 37.

**Gate C — PASS.** BranchesValid post (11161) equals baseline (11161), a reduction of 0.
BranchesCovered post (9368) is greater than or equal to 9363 - 0 = 9363; it exceeds it by 5.

**Gate D — PASS.** Signed per-filename LinesCovered deltas, aggregated over every class element
sharing that filename:

| File | Base valid | Base covered | Post valid | Post covered | dValid | dCovered |
| --- | --- | --- | --- | --- | --- | --- |
| UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs | 271 | 216 | 281 | 238 | +10 | +22 |
| UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs | 296 | 284 | 273 | 268 | -23 | -16 |
| UtilitiesCS/Threading/TimeOutTask.cs | 610 | 576 | 578 | 562 | -32 | -14 |
| UtilitiesCS/Extensions/DfDeedle.cs | 163 | 163 | 167 | 167 | +4 | +4 |
| UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs | 167 | 163 | 167 | 163 | 0 | 0 |

Two files carry a negative LinesCovered delta, and both are files this feature shrank: Etl.cs lost
EtlAsyncOld at P4-T2 and TimeOutTask.cs lost the two inert TimeoutAfter overloads at P4-T1. Their
dValid figures, -23 and -32, are negative for the same reason. No file this feature did not shrink
has a negative delta, which is what Gate D requires.

## Testable denominator

ProductionLinesCovered: 56029
ProductionLinesValid: 65402
TestableDenominatorLineRate: 0.856686
ModulesFilteredOut: 0
TestableDenominatorFloorMet: true

The production-only aggregate is defined mechanically as the sum of the
Get-CoberturaPackageLineSummary results over every package element in the Cobertura file whose name
does not end in .Test or .Tests. No per-class judgment is exercised. That filter selected every
package in the file and filtered none out, which is the expected result:
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 line 6 builds a first-party allowlist that
drops every project whose assembly name ends in .Test (lines 22-47), and ConvertTo-KoverageCoberturaXml
at line 405 strips every package outside that allowlist from both the numerator and the denominator
during post-processing. The nine packages present are QuickFiler, UtilitiesCS, TaskVisualization,
SVGControl, ToDoModel, Tags, TaskMaster, TaskTree and VBFunctions, all production. The filter is
retained as a mechanical guard and ModulesFilteredOut records its zero result.

Test assemblies are instrumented at run time and removed during post-processing, so a raw Cobertura
is not interchangeable with a processed one for this figure. Both artifacts read here are processed.

CLAUDE.md § UT2 permits exemption from the coverage denominator through exactly two mechanisms, an
[ExcludeFromCodeCoverage] attribute in source and an assembly-level exclude in coverage.config, and
each contributes zero first-party exclusions in this tree. A git grep for ExcludeFromCodeCoverage
across the five in-scope production files returns no hit, and coverage.config's ModulePaths Exclude
block names only the third-party modules Deedle, FSharp, Castle.Core, FluentAssertions, Moq,
Microsoft.Testing and MSTest, to which the runner adds a run-time `.*\.Test\.dll$` pattern. No
first-party production module is excluded by either mechanism.

The testable denominator therefore equals the production-only denominator, and this artifact asserts
that identity rather than deriving an exemption list. The resulting rate of 0.856686 is at or above
the 0.80 floor, so TestableDenominatorFloorMet is true and no NotLoweredVersusBaseline computation is
required. For information, the same figure derived from the baseline Cobertura is 0.856211, so the
rate rose rather than fell.

## DeletionAttribution

Every per-construct figure below is obtained by differencing the baseline and post-change
per-filename line-number sets for the file that construct lived in, using
Get-CoberturaClassLineSummary. A count of deleted source lines is not a substitute, because a
comment, a declaration and a closing brace carry no line element.

The two inert `(int, int)` TimeoutAfter overloads, both in UtilitiesCS/Threading/TimeOutTask.cs,
contributed a LinesValid reduction of 32. That file's whole dValid is -32, and P4-T1 is the only
task in this plan that changes it and deletes only; no line was added to it.

EtlAsyncOld, in UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs, contributed a LinesValid
reduction of 23. That file's whole dValid is -23. The other two edits to it add no line element:
P6-T1 adds a comment block, and P5-T1 replaces one return statement with another, so the statement
count is unchanged.

The three deleted tests, EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData,
TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult and
TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully, contributed exactly 0 each.
All three live in UtilitiesCS.Test, and that package is stripped from the processed Cobertura by the
first-party allowlist before any figure in this plan is read, so no test line is in either the
numerator or the denominator of any figure recorded here.

DeletionSum: 55

## AdditionAccounting

UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs contributed a LinesValid increase
of 10, from the P3-T2 trailing parameter, the P3-T3 resolved deadline-source factory local and its
clock-derived lambda, and the trailing arguments P3-T4 and P3-T5 added to the two retry recursions.

UtilitiesCS/Extensions/DfDeedle.cs contributed a LinesValid increase of 4, from the P3-T6 named
provider argument, which CSharpier then broke across four lines at P8-T1.

UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs contributed 0. Its single changed line is the P6-T2
doc comment, which carries no line element.

AdditionSum: 14

## Reconciliation

DeletionSum minus AdditionSum is 55 - 14 = 41.

The UtilitiesCS-package LinesValid delta recorded by Gate A is a reduction of 41, from 43287 to
43246.

Residual: 0

The reconciliation closes exactly. A reconciliation of the deletion sum alone against the total
could not close, because the total is deletions net of additions and this feature adds a parameter,
a resolved local and a comment block. The delta is therefore attributed to the deletions of item 4
and the additions of item 2, and not to any coverage regression.
