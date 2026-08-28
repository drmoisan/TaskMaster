# P11-T7 — Scoped vstest with coverage, QuickFiler.Test (loop iteration 1)

Timestamp: 2026-08-28T02-22
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p11-t7.trx" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\qa-gates
EXIT_CODE: 0
ExpectedExitCode: 0

FinalPassed: 1121
FinalFailed: 0
FinalSkipped: 0

Loop iteration: **1**.

`$vstest` resolved to the single path returned by
`vswhere.exe -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`.
TRX: `evidence/qa-gates/p11-t7.trx`.

TRX counters read verbatim: `total=1121 executed=1121 passed=1121 failed=0 error=0 timeout=0
aborted=0 inconclusive=0 notExecuted=0 notRunnable=0 warning=0`. Console reported
`Test Run Successful.`

## Acceptance, in four parts

### (a) `FinalFailed:` is not greater than `BaselineFailed:`

`0` is not greater than `0`, the value in
`evidence/baseline/phase0-vstest-quickfiler.2026-08-28T00-14.md`. Satisfied.

### (b1) Zero failures in the two classes this feature creates — absolute

| Class | Created by | Passed | Failed |
|---|---|---:|---:|
| `ToolStripMenuItemCbTests` | P1-T1 | 5 | **0** |
| `QfcItemController_ThemeMarshallingTests` | P5-T1 | 3 | **0** |

Both are at zero failures. This gate is absolute and satisfiable because both classes exist only
after this plan wrote them, and both are present in this run.

### (b2) Per-class failed count not greater than the P0-T13 per-class baseline

Read from `p11-t7.trx` by mapping every `UnitTestResult` to the `className` of its `UnitTest`
definition. All thirteen pre-existing classes P0-T13 records are listed, which is a superset of the
eight P11-T7 enumerates.

| Class | Baseline passed / failed | Final passed / failed | Failed regression? |
|---|---|---|---|
| `ItemViewerBreadcrumbDropDownContractTests` | 5 / 0 | 14 / **0** | No |
| `QfcItemController_EventWiringTests` | 13 / 0 | 15 / **0** | No |
| `QfcItemController_MailActionsTests` | 24 / 0 | 27 / **0** | No |
| `BreadcrumbSelectorOpenRetryTests` | 8 / 0 | 8 / **0** | No |
| `BreadcrumbDropDownIntegrationTests` | 10 / 0 | 10 / **0** | No |
| `QfcItemController_SeamDispatcherTests` | 14 / 0 | 14 / **0** | No |
| `QfcItemController_FolderSuggestionsTests` | 4 / 0 | 4 / **0** | No |
| `QfcItemController_FolderHandlingTests` | 17 / 0 | 17 / **0** | No |
| `QfcItemController_ViewerSetupTests` | 11 / 0 | 11 / **0** | No |
| `QfcItemController_NavigationTests` | 17 / 0 | 17 / **0** | No |
| `QfcItemController_ConversationTests` | 12 / 0 | 12 / **0** | No |

Every per-class failed count is `0` at both baseline and final, so none is greater than its baseline.
**No test is counted against a non-zero per-class baseline**, because no per-class baseline is
non-zero; the conditional obligation to name such a test and its owning sibling therefore does not
arise.

The three classes whose pass count rose are the three this feature adds tests to:
`ItemViewerBreadcrumbDropDownContractTests` +9 (P1-T3, P3-T1, P5-T3, P7-T1, P7-T2),
`QfcItemController_EventWiringTests` +2 (P1-T4, whose tests live in the `Part2` continuation file of
the same partial class) and `QfcItemController_MailActionsTests` +3 (P7-T3 and P7-T7, likewise in the
`Part2` continuation). The remaining eight are unchanged in both counts, consistent with their being
touched only by a token-for-token invocation rename or not at all.

### (c) `FinalSkipped:` equals `BaselineSkipped:`

`0` equals `0`. `QuickFiler.Test` carries zero `[Ignore]` attributes and none was added.

### (d) `FinalPassed:` is not less than `BaselinePassed:`

`1121` is not less than `1099`. The increase is **+22**, which reconciles exactly against the
per-class table: 9 + 2 + 3 = 14 in the three grown pre-existing classes, plus 5 in
`ToolStripMenuItemCbTests` and 3 in `QfcItemController_ThemeMarshallingTests`, both new — 22 in
total, with no other class changing.

### Exit code

The observed exit code is `0`. `vstest.console.exe` exits non-zero whenever any executed test fails;
`FinalFailed:` is `0`, so `ExpectedExitCode: 0` is declared per the task's branch rule and the
artifact normalizes to `pass`. The gate is the four-part comparison above, never the exit code.

## Named pins carried from P0-T13

All nine of the `BaselineNamedPins:` entries pass in this run, together with the test P11-T9 relies
on:

```
MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher                       = Passed
AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection           = Passed
MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems           = Passed
AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings  = Passed
AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult                  = Passed
SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending         = Passed
SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke                      = Passed
JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch                      = Passed
ExecutingAssembly_ContainsNoFormDerivedType                              = Passed
PicturesChanged_WhenRaised_RefreshesOptionsPictures                      = Passed
```

The two pin **names** are unchanged from the baseline block: the `Set`-to-`Add` rename this feature
performs is on the `IItemViewer` member, not on the test method identifiers, so
`…RetainsSetFolderItemsAndIndexOneSelection` and `…AppendsTrashToDeleteViaSetFolderItems` are still
the correct names to look up and both are found and passing. Searching this TRX for the
hypothetical renamed spellings `AssignFolderComboBox_RetainsAddFolderItemsAndIndexOneSelection` and
`MarkItemForDeletion_StillAppendsTrashToDeleteViaAddFolderItems` returns nothing, which confirms the
lookup is against the real names rather than silently matching zero tests.

## Artifact hygiene

`/EnableCodeCoverage` wrote two attachment directories into the results directory, one of them named
after the account and the machine. Both were deleted immediately after the run and neither is
committed; no acceptance condition in this plan references them, and P11-T8 is the task that produces
the coverage figures. The results directory now contains **zero** subdirectories.

`p11-t7.trx` was sanitised in place: 2242 occurrences of the worktree root replaced with the
repo-root placeholder, 1127 occurrences of the machine name with the host placeholder, and 4 of the
account name with the user placeholder — all case-insensitively, because vstest writes the `storage=`
attribute in all-lower-case and a case-sensitive substitution would leave those occurrences behind. A
case-insensitive search of the committed TRX for the account name or the machine name returns **0**
for each.

**XML-escaping note.** Each placeholder is written into the TRX in entity form —
`&lt;repo-root&gt;`, `&lt;host&gt;`, `&lt;user&gt;`. XML forbids a raw less-than character in a text
node or an attribute value, so writing the literal angle-bracket characters would make the document
unparseable; an XML reader decodes the entity form back to the required literal. The sanitised file
was re-parsed with a strict XML reader afterwards: it parses, and its `UnitTestResult` element count
is **1121**, matching the `Counters total` of 1121 and the `FinalPassed:` recorded above. The file
carries no BOM, matching the committed `evidence/baseline/p0-t13.trx`.

## Loop consequence

The stage passed and rewrote no tracked source file. No restart is triggered; the loop proceeds to
P11-T8.

Output Summary: The scoped `QuickFiler.Test` gate **passes** at loop iteration 1 with `EXIT_CODE: 0`,
`FinalPassed: 1121`, `FinalFailed: 0`, `FinalSkipped: 0` against a baseline of 1099 / 0 / 0. All four
acceptance parts hold: (a) 0 failures is not greater than the baseline 0; (b1) both feature-created
classes — `ToolStripMenuItemCbTests` (5 passed) and `QfcItemController_ThemeMarshallingTests` (3
passed) — are at absolute zero failures; (b2) every one of the thirteen pre-existing classes is at
zero failures, none greater than its baseline, and no test is counted against a non-zero per-class
baseline; (c) skipped equals the baseline 0; (d) 1121 passed is not less than 1099. The +22 pass
delta reconciles exactly as 9 + 2 + 3 new tests in three grown classes plus 5 + 3 in the two new
classes. All nine P0-T13 named pins pass, as does
`PicturesChanged_WhenRaised_RefreshesOptionsPictures`. The TRX is sanitised, parses strictly, and
reports 1121 results with zero residual account or machine identifiers; both coverage attachment
directories were deleted.
