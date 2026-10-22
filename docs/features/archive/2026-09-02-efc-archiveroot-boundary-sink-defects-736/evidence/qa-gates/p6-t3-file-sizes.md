# P6-T3 — File-size audit after the formatting pass

Timestamp: 2026-09-04T01-38

**This artifact records the second execution of P6-T3**, run after the toolchain-loop restart that
P6-T13 caused. Exactly one measured figure changed: P6-T13 appended three success-path tests and the
shared `AttachSucceedingKeyboardHandler` helper to
`QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs`, taking that file from 339 lines to
**490**, which remains under the repository's 500-line ceiling with 10 lines of headroom. No
production file was touched by P6-T13, so every other count below, and the whole per-remedy
itemisation for `QuickFiler/Controllers/EfcFormController.cs`, is unchanged and was re-measured
rather than carried forward. The 111-insertion / 7-deletion numstat for the controller file was
re-derived against `origin/main` in this pass and is unchanged.

Command: `Get-Content -LiteralPath <path> | Measure` line counting over the eleven ratified Write Set
paths, plus `git diff --cached -U0 origin/main` and `git diff --cached --numstat origin/main` over
`QuickFiler/Controllers/EfcFormController.cs`, all run after P6-T1's formatting pass.

EXIT_CODE: 0

## Post-format line counts of all eleven Write Set files

| Write Set path | Post-format line count | Governing ceiling | Within it |
|---|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` | 95 | 500 | yes |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 493 | 500 | yes |
| `TaskMaster/TaskMaster.csproj` | 586 | n/a (project file) | n/a |
| `QuickFiler/Controllers/EfcFormController.cs` | **1320** | 1330 (D7 budgeted) | yes |
| `QuickFiler/Controllers/EfcDataModel.cs` | 499 | 500 | yes |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | **485** | exactly 485 | yes |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs` | 490 | 500 | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 523 | n/a (project file) | n/a |
| `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` | 399 | 500 | yes |
| `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs` | 207 | 500 | yes |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 397 | n/a (project file) | n/a |

The six files this task's acceptance requires to be under 500 lines —
`AppOlObjects.ArchiveRoot.cs` (95), `AppOlObjects.cs` (493), `EfcDataModel.cs` (499),
`EfcFormControllerTests.Part2.cs` (490), `EfcDataModelArchiveRootTests.cs` (399), and
`AppOlObjectsArchiveRootComGuardTests.cs` (207) — are each under it.
`EfcFormControllerTests.cs` is exactly 485. `EfcFormController.cs` is 1320, within the D7 budgeted
ceiling of 1330.

`EfcDataModel.cs` at 499 has one line of headroom. Its seam documentation was trimmed during P5-T3
for exactly this reason, which P5-T5's artifact records.

## Delivered net delta for `QuickFiler/Controllers/EfcFormController.cs`, itemised per remedy

The file was **1216** lines at the merge base and is **1320** now, a net delta of **+104**. The
anchored, index-reading diff reports **111 insertions and 7 deletions**, which is the same +104. Its
six hunks partition cleanly by remedy:

| Remedy | Hunk (old → new) | Insertions | Deletions | Net |
|---|---|---|---|---|
| Default-sink change (P4-T5) | `-129 +129,13` | 13 | 1 | **+12** |
| Notifier seam (P4-T1) | `-157,0 +170,61` | 61 | 0 | **+61** |
| Keyboard guard (P2-T5, P2-T9) | `-921 +994,7` | 7 | 1 | +6 |
| Keyboard guard (P2-T5, P2-T9) | `-923,2 +1002,24` | 24 | 2 | +22 |
| Keyboard guard (P2-T5, P2-T9) | `-929,2 +1030,5` | 5 | 2 | +3 |
| Breadcrumb reroute (P3-T3) | `-1022 +1126` | 1 | 1 | **0** |
| | **Totals** | **111** | **7** | **+104** |

The four per-remedy figures are therefore:

| Remedy | Net delta |
|---|---|
| Keyboard guard | **+31** |
| Breadcrumb reroute | **0** |
| Notifier seam | **+61** |
| Default-sink change | **+12** |
| **Sum** | **+104** |

**31 + 0 + 61 + 12 = 104**, which is exactly the post-format count of 1320 minus the merge-base count
of 1216.

The notifier seam is the largest single contributor at +61 lines, and D5 records why: the in-repo
modeless helper `MyBoxModeless` in UtilitiesCS could not be adopted, because the type is `internal`
and UtilitiesCS grants `InternalsVisibleTo` to only three assemblies, none of which is QuickFiler,
and because its only entry points take a store identity plus three button actions rather than a
single-string general notice. Making it reachable would have required editing a file outside the
ratified Write Set. The accepted cost is roughly ten lines of duplicated WinForms construction plus
its documentation, itemised here rather than absorbed.

The breadcrumb reroute is net zero because it replaces one statement with another of the same shape.

## Pre-existing 500-line-ceiling violation

`QuickFiler/Controllers/EfcFormController.cs` exceeded the repository's 500-line ceiling before this
item began, at 1216 lines, and this item does not repair that violation: splitting the file is an
explicit non-goal of spec.md, the class is declared `internal class` rather than `partial` so
relieving it would require a declaration change, and the pre-existing violation must be called out in
the PR description as separately tracked debt.

Output Summary: all eleven Write Set files measured after the formatting pass. The six size-gated
source files are each under 500 lines (95, 493, 499, 490, 399, 207);
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is exactly 485; and
`QuickFiler/Controllers/EfcFormController.cs` is 1320, within the D7 budgeted ceiling of 1330. Its
net delta of +104 is itemised as keyboard guard +31, breadcrumb reroute 0, notifier seam +61, and
default-sink change +12, which sum exactly to 1320 minus 1216. The pre-existing 500-line-ceiling
violation on that file is not repaired by this item and is to be called out in the PR description.
