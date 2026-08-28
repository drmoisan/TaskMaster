# Phase 0 — Scoped vstest Baseline, QuickFiler.Test (P0-T13) — re-run, supersedes 2026-08-27T23-28

Timestamp: 2026-08-28T00-14
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p0-t13.trx" /ResultsDirectory:docs\features\active\itemviewer-surface-defects-489\evidence\baseline
EXIT_CODE: 0
ExpectedExitCode: 0

BaselinePassed: 1099
BaselineFailed: 0
BaselineSkipped: 0

## Supersession

This artifact supersedes `evidence/baseline/phase0-vstest-quickfiler.2026-08-27T23-28.md`, which
recorded all three integers as `UNMEASURED` because the inherited `CS0006` analyzer version skew
failed the build and the `/t:Rebuild` clean left `QuickFiler.Test.dll` absent, so no run occurred
and no `p0-t13.trx` existed. The skew is cleared for this worktree without changing any tracked
file, the assembly is present, and the run completed. The superseded artifact is retained as the
audit record of the blocked first attempt.

## Acceptance

- All three integers are recorded above.
- `evidence/baseline/p0-t13.trx` exists on disk.
- `$vstest` resolved to the single path returned by `vswhere.exe -latest -products * -find
  "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`.
- `EXIT_CODE:` is `0` and `BaselineFailed:` is `0`, so `ExpectedExitCode: 0` is declared and the
  artifact normalizes to `pass`. Because `BaselineFailed:` is `0`, the conditional obligation in
  P0-T13 to name every failing test and attribute it to a sibling child **does not arise**: there
  is no failing test to name.
- `BaselineSkipped: 0` matches the expectation stated in P0-T13; `QuickFiler.Test` carries zero
  `[Ignore]` attributes.

TRX counters read verbatim: `total=1099 executed=1099 passed=1099 failed=0 error=0 timeout=0
aborted=0 inconclusive=0 notExecuted=0`. Total run time 12.78 seconds.

## BaselinePerClass:

Per-class `passed`/`failed`, read from `p0-t13.trx`, for the ten classes P11-T7 part (b)
enumerates plus the three further pre-existing classes P0-T13 names. P11-T7 part (b2) compares
against this block.

```
ToolStripMenuItemCbTests                  = ABSENT (created by P1-T1; no baseline exists)
QfcItemController_ThemeMarshallingTests   = ABSENT (created by P5-T1; no baseline exists)
ItemViewerBreadcrumbDropDownContractTests = passed 5,  failed 0
QfcItemController_EventWiringTests        = passed 13, failed 0
QfcItemController_MailActionsTests        = passed 24, failed 0
BreadcrumbSelectorOpenRetryTests          = passed 8,  failed 0
BreadcrumbDropDownIntegrationTests        = passed 10, failed 0
QfcItemController_SeamDispatcherTests     = passed 14, failed 0
QfcItemController_FolderSuggestionsTests  = passed 4,  failed 0
QfcItemController_FolderHandlingTests     = passed 17, failed 0
QfcItemController_ViewerSetupTests        = passed 11, failed 0
QfcItemController_NavigationTests         = passed 17, failed 0
QfcItemController_ConversationTests       = passed 12, failed 0
```

The two `ABSENT` rows are the two classes this plan creates. P11-T7 part (b1) is an **absolute**
zero-failure gate over exactly those two, so the absence of a baseline row for them removes
nothing from the gate.

## BaselineNamedPins:

Per-test outcome, read from `p0-t13.trx`, for the nine named tests P0-T13 enumerates. P6-T7,
P8-T11, P8-T15, P9-T9 and P11-T12 compare against this block.

```
MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher                       = passed
AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection           = passed
MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems           = passed
AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings  = passed
AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult                  = passed
SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending         = passed
SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke                      = passed
JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch                      = passed
ExecutingAssembly_ContainsNoFormDerivedType                              = passed
```

All nine pass at baseline. The ninth, `ExecutingAssembly_ContainsNoFormDerivedType`
(`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16`, class `NoLiveFormInTestAssemblyTests`), is
the pre-existing structural guard P11-T12 asserts; its class is not among the thirteen above,
which is why it is carried here by name.

## Artifact hygiene

`/EnableCodeCoverage` wrote two attachment directories into the results directory, one holding a
`.coverage` binary whose **filename embedded the account and machine name**. Both directories were
deleted and neither is committed; no acceptance condition in this plan references them, and
P0-T14 is the task that produces the coverage figures. `p0-t13.trx` itself was sanitised in place:
2198 occurrences of the worktree root replaced with `<repo-root>` (case-insensitively, because
vstest writes the `storage=` attribute in all-lower-case), 1105 occurrences of the machine name
with `<host>`, and 4 of the account name with `<user>`. A case-insensitive search of the committed
TRX for either identifier returns **0**.

Output Summary: The scoped `QuickFiler.Test` gate **passes**. `EXIT_CODE: 0` with
`BaselinePassed: 1099`, `BaselineFailed: 0`, `BaselineSkipped: 0` over a 12.78-second run;
`evidence/baseline/p0-t13.trx` exists. Because `BaselineFailed:` is `0`, `ExpectedExitCode: 0` is
declared and no failing test needs attributing to a sibling child. The unconditional
`BaselinePerClass:` block records eleven pre-existing classes all at zero failures — the largest
being `QfcItemController_MailActionsTests` at 24 passed, `QfcItemController_FolderHandlingTests`
and `QfcItemController_NavigationTests` at 17 each — and marks the two classes this plan creates
`ABSENT`, which is correct because they do not yet exist. The unconditional `BaselineNamedPins:`
block records all nine named pins as `passed`. This run supersedes the 2026-08-27T23-28 artifact,
which recorded `UNMEASURED` because the analyzer skew left the test assembly absent.

**XML-escaping note.** The placeholder is written into the TRX in entity form as `&lt;repo-root&gt;` (likewise `&lt;host&gt;` and `&lt;user&gt;`).
XML forbids a raw less-than character in a text node or an attribute value, so writing the
five literal characters would make the document unparseable; an XML reader decodes the
entity form back to the required literal. Each sanitised TRX was re-verified with a strict
parser and its UnitTestResult element count matches the test total recorded above.
