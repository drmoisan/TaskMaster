# AC12 — Per-File Coverage for ProgressPackage.cs

Timestamp: 2026-09-13T15-40
Task: [P2-T9]

Verdict: PASS

Command: git diff -U0 $b -- UtilitiesCS/Threading/ProgressPackage.cs
Command: pwsh -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; [xml]$x = Get-Content -Raw "TestResults/coverage/coverage-postchange.cobertura.xml"; $c = @($x.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like "*Threading\ProgressPackage.cs" }); "ClassElements: " + $c.Count; foreach ($n in $c) { $s = Get-CoberturaClassLineSummary -ClassNode $n; "LineRateAttribute: " + $n.GetAttribute("line-rate"); "TotalLines: " + $s.TotalLines; "CoveredLines: " + $s.CoveredLines; foreach ($k in ($s.LineMap.Keys | Sort-Object)) { "Line " + $k + " hits " + $s.LineMap[$k].Hits } }'
EXIT_CODE: 0

BaselineCoveredLines: 52
BaselineTotalLines: 52
PostChangeCoveredLines: 61
PostChangeTotalLines: 61

ClassElements: 1
LineRateAttribute: 1

Output Summary: the baseline ratio is 52 divided by 52, which is exactly 1. The post-change ratio is 61
divided by 61, which is also exactly 1. The post-change ratio is greater than or equal to the baseline
ratio, so AC12 passes. The nine executable lines the change adds are all covered at a hits value of 1,
and no executable line of the file is uncovered.

## Like-For-Like Check On The Document Read

`ClassElements:` is 1, which is the same class-element count the P0-T11 baseline recorded. Both
derivations therefore read a post-processed Cobertura document, in which the post-processing step has
merged the compiler-generated classes for this source file into a single class element per filename. A
count of 6 would have indicated a raw, unprocessed document left behind by a runner that threw before
post-processing, and the two figure sets would not have been comparable. The P2-T7 run printed its
post-processing announcement and exited 0, which corroborates the count.

The derivation matches P0-T11 exactly: the same dot-sourced helper `Get-CoberturaClassLineSummary`,
which de-duplicates by line number because a Cobertura class element repeats every line under its
method tree and again in its class-level rollup; and the same merge-by-line-number rule with a repeated
line number resolved by its maximum hits value, stated even though the class-element count is one.

## Added And Changed Lines, With Their Hits Values

The line numbers below are the plus-side hunk headers of the anchored unified diff with zero context,
taken against the base commit `430e2a11db0fa7069d02d42e18df46d21f7db7b5` recorded by P0-T2. Line
numbers are not compared across the change — the file grows from 150 to 186 lines — so only the
aggregate ratio and this new-line floor are load-bearing.

| Line(s) | Content | In line map | Hits |
|---|---|---|---|
| 13 | `public class ProgressPackage : IDisposable` (changed) | no | type declaration, non-executable |
| 26 | `_ownsCancelSource = cancelSource is null;` — tracker overload | yes | 1 |
| 42 | `_ownsCancelSource = cancelSource is null;` — pane overload | yes | 1 |
| 49–54 | XML doc comment on `CreateAsTupleAsync` | no | comment, non-executable |
| 73–78 | XML doc comment on `CreateAsTuplePaneAsync` | no | comment, non-executable |
| 103–107 | explanatory comment above the ownership field | no | comment, non-executable |
| 108 | `private bool _ownsCancelSource;` | no | field declaration with no initializer, emits no sequence point |
| 109 | blank line | no | blank, non-executable |
| 170 | blank line | no | blank, non-executable |
| 171–177 | XML doc comment on `Dispose` | no | comment, non-executable |
| 178 | `public void Dispose()` | no | method signature; the method's first sequence point is its opening brace at 179 |
| 179 | `{` — Dispose opening brace | yes | 1 |
| 180 | `if (_ownsCancelSource)` | yes | 1 |
| 181 | `{` | yes | 1 |
| 182 | `_cancelSource?.Dispose();` | yes | 1 |
| 183 | `_ownsCancelSource = false;` | yes | 1 |
| 184 | `}` | yes | 1 |
| 185 | `}` — Dispose closing brace | yes | 1 |

Nine lines enter the line map: 26, 42 and 179 through 185. That is exactly the difference between the
post-change total of 61 and the baseline total of 52, which confirms that no pre-existing executable
line left the map and that the growth is entirely attributable to this change.

## The Mandatory Floor

The floor requires a hits value greater than zero for both ownership assignment lines and for every
line of the Dispose body. Measured:

- Line 26, the assignment inside the tracker overload of `InitializeAsync`: hits 1.
- Line 42, the assignment inside the pane overload of `InitializeAsync`: hits 1.
- Lines 179 through 185, every line of the Dispose body: hits 1 each.

No line in the floor measures zero. The pane overload's assignment is in the floor and is covered; the
exclusion an earlier revision granted it was withdrawn as false against measurement, and the
measurement above confirms the withdrawal was correct. Per D6 the coverage runner's population is
repository-wide and wider than the per-assembly runs, which is why the pane overload is reached even
though no test in `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` calls it.

No exemption was reinstated, no line was reclassified as a residual, and no test was added to chase
coverage. AC11 pins the UtilitiesCS executed-test delta at exactly minus six and that figure was
measured at exactly minus six, so no fourth test was added.

## Dispose Method Body Ratio

The Dispose body contributes 7 lines to the post-change line map and 7 of them are covered. Its ratio
is 7 divided by 7, which is 1.00 and clears the 0.90 floor CLAUDE.md sets for a newly added method.

Both branches of the ownership test are exercised. The owned path, on which line 182 and line 183 run,
is driven by `Dispose_WhenPackageConstructedTheSource_ReleasesIt` and by the parent in
`Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource`. The not-owned path, on which the `if` at line
180 evaluates false, is driven by `Dispose_WhenCallerSuppliedTheSource_LeavesItUsable` and by the child
in `Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource`.

## Anchoring

The diff carries the base commit as its ref operand rather than being left unanchored. An unanchored
diff compares the worktree against the index and would pass vacuously once the change is committed.
