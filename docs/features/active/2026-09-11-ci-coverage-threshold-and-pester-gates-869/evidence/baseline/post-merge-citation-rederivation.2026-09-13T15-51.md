# Post-merge citation re-derivation — issue 869

Timestamp: 2026-09-13T15-51
Purpose: re-derive every citation in `plan.2026-09-12T10-25.md` against the post-merge tree, before executing any task that depends on one.
Outcome: HALT. Four plan premises are falsified by the merged tree. No plan task was executed.

## Merge record

| Item | Value |
| --- | --- |
| Branch | `bug/ci-coverage-threshold-and-pester-gates-869` |
| Pre-merge head | `8aa9f13b5c325768c32b58a84dbc1571ec0056c7` |
| Branch point (merge-base) | `2405a829d6afd3b12eb7c228d57158a97cb4e2ca` |
| `origin/main` merged | `e6d86049e31096914eaf6dcbc7222e5b9f435258` |
| Post-merge head | `f12cf7debb676da8401582ac9609048ab8c647d1` |
| Conflicts | none; merged by the `ort` strategy |
| Worktree after merge | clean (`git status --porcelain=v1 --untracked-files=all` empty) |
| Ancestry | `git merge-base --is-ancestor origin/main HEAD` returns 0 |

Items merged into the branch point since it was cut: 583 (PR 874), 838 (PR 875), 839 (PR 876), 877 (PR 880), 873 (PR 881).

Measured against the branch point, the merge changed 1692 insertions and 111 deletions across 12 files under `scripts/vscode` and `tests/scripts/vscode`, all attributable to item 873.

## P0-T4 line-count assertions — 4 of 11 falsified

P0-T4 states: "A mismatch on any one of these eleven values halts the plan and is reported, because the plan's citations describe a different tree." Counts read with the Grep tool in count mode over the start-of-line pattern, as P0-T4 prescribes.

| File | P0-T4 asserts | Post-merge | Verdict |
| --- | --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` | 56 | 56 | match |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | 351 | 438 | MISMATCH (+87) |
| `scripts/vscode/Invoke-VSBuild.ps1` | 167 | 167 | match |
| `scripts/vscode/Invoke-Restore.ps1` | 39 | 39 | match |
| `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | 496 | 498 | MISMATCH (+2) |
| `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` | 86 | 86 | match |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` | 15 | 15 | match |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` | 99 | 106 | MISMATCH (+7) |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1` | 71 | 71 | match |
| `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` | 144 | 146 | MISMATCH (+2) |
| `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` | 28 | 28 | match |

## Falsified premises

### F1 — P2-T3: the literal to substitute does not exist

P2-T3 instructs: "Substitute, at line 48, the literal `<coverage line-rate="0.8"><packages /></coverage>`".

Post-merge that literal occurs nowhere in `tests/scripts/vscode`. The mock moved from line 48 to line 55, and item 873 replaced the document with an enriched one carrying `lines-covered`, `lines-valid` and a populated `<packages>` subtree with per-line `<line number= hits=>` elements. A verbatim substitution matches nothing. Applying the plan's replacement text would delete the per-line package data that item 873's own new tests depend on.

### F2 — P2-T4: sites moved and the same literal is absent

P2-T4 cites lines 371, 404 and 406, and instructs "Do not alter the literal at line 417". Post-merge the three substitution sites are at lines 382, 416 and 418, the literal is the same enriched document described in F1 rather than the one the plan quotes, and the `line-rate="0.5"` case the plan protects moved from line 417 to line 429.

The structural role of the sites also changed: line 382 is now a `Describe`-level setup mock governing roughly seven cases, not one of two peer case-level mocks.

P2-T4's acceptance condition "the Grep tool count of the start-of-line pattern against this file reports exactly 496" is unsatisfiable: the file is 498 lines and the task prescribes an in-place substitution that adds no lines.

### F3 — P10-T10: stale comparison figure

P10-T10 requires the artifact to name the count for `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, "which must be at most 500 and was 496 before the change". The pre-change count is now 498.

### F4 — P0-T7: the failure-attribution inference is unsound

P0-T7 reasons that a non-zero exit code from the coverage entry point means "the line assertion at line 344 threw before the report statement at line 345 could print", and its authorized halt branch concludes from a non-zero exit that the first-party line figure is already below the floor.

Item 873 inserted a JaCoCo projection block after the report statement. In the post-merge file the write-back is at line 384, the line assertion at 386 and the report statement at 387, but `Assert-JacocoProjectionReconciliation` now sits at line 397, downstream of the report statement. A non-zero exit can therefore originate below the report statement, with the `First-party coverage:` line already printed. The inference from a non-zero exit to a below-floor line figure no longer holds, and P0-T7's halt branch would misattribute a projection-reconciliation failure to a coverage shortfall.

## Unresolved hazard introduced by the merge, covered by no plan task

The replacement literal P2-T3 and P2-T4 prescribe sets `branch-rate="0.8" branches-valid="10"` on the root element while declaring an empty `<packages />` subtree. Item 873 added `Assert-JacocoProjectionReconciliation`, which reconciles root-level coverage attributes against package contents and throws when they disagree. Root branch attributes asserting ten valid branches over a package subtree declaring none is the disagreement that assertion exists to detect. Whether the prescribed literal survives the new reconciliation assertion is not determinable by reading, and no task in this plan covers the interaction. It is reported rather than resolved.

## Premises re-derived and re-confirmed

| Premise | Plan site | Post-merge state |
| --- | --- | --- |
| Line assertion is immediately followed by the first-party report statement, after the write-back | P2-T2 | Holds. Write-back 384, line assertion 386, report 387. Line numbers moved from 342/344/345; the adjacency P2-T2's ordering acceptance depends on is intact. |
| Seven workflow YAML files present | P0-T14, P10-T5 | Holds. Seven files. |
| `First-party coverage:` prints line counts and branch counts with percentages | P0-T7, P10-T8 | Holds. `Invoke-MSTestWithCoverage.FirstParty.ps1` line 120. |
| Splatting-seam case at line 36 of `Invoke-MSTest.Main.Tests.ps1` | P3-T1, P3-T2, P6-T1 | Holds exactly, still line 36. |
| `Invoke-MSTest.Main.Tests.ps1` holds 11 cases before P6-T1 | P6-T1 | Holds. 11 `It` cases despite the file growing by 2 lines. |
| `Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` holds 3 discovery cases | P2-T3 | Holds. Cases at lines 59, 74, 90. The expected post-change passed count of 5 is unaffected. |
| All eight new identifiers absent from the tree | P0-T15 | Holds. Zero occurrences across all `*.ps1`. The delivery is genuinely not yet present. |
| `Invoke-MSTestWithCoverage.Helpers.ps1` dot-sources the threshold part | P8-T1, plan line 51 | Holds, at Helpers line 4. The file is now 471 lines, not the 470 the plan's note records. |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1` exists | P8-T3 | Holds, 70 lines. |

## Baselines

Phase 0 baselines were not captured. Capturing them would anchor measurements to a plan whose Phase 0 gate (P0-T4) halts on this tree, and P0-T3's anchor tag `plan-869-base` was deliberately not created so that no later diff gate is silently anchored against a superseded plan.
