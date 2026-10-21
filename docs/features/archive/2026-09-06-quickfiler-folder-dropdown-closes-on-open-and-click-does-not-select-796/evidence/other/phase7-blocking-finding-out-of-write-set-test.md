# Blocking finding at the end of Phase 7 — a pre-existing test outside the write set now fails

Timestamp: 2026-09-07T14-39
Raised by: the executor running plan Phases 4 through 7
Issue: #796
Status: BLOCKING for the whole-assembly gate at task P9-T5. Not blocking for any gate in Phases 4
through 7, all of which passed.

## What was observed

A whole-assembly run of QuickFiler.Test, executed as a final toolchain pass after task P7-T4 and
NOT as a plan task, reports one failing test.

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" /ResultsDirectory:TestResults\796\p7-full "/Logger:trx;LogFileName=p7-full.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 1

```
Total tests: 1380
     Passed: 1379
     Failed: 1
```

The single failure:

```
Failed TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent
Moq.MockException:
Expected invocation on the mock once, but was 0 times: v => v.SetFolderDroppedDown(False)
Performed invocations:
   Mock<IItemViewer:3> (v):
      IItemViewer.IsFolderDropDownOpen
```

It is declared in QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs.

A scoped re-run of that one class confirms the failure is isolated:

```
Total tests: 6
     Passed: 5
     Failed: 1
```

## Why it fails

The test arranges a viewer reporting `IsFolderDropDownOpen == true` and no search-driven open, then
raises the search box's `Leave` and asserts `SetFolderDroppedDown(false)` exactly once. That
arrangement is precisely the mouse-driven-open state, and asserting that the leave dismisses it is
asserting the behaviour AC4 exists to remove:

> AC4: The #680 leave-handoff latch covers the mouse open path as well as the Down-arrow path.

The AC4 fix landed at task P6-T5 makes `TextBoxSearch_Leave` dismiss only a popup the search box
itself opened. This test therefore now pins the defect rather than the contract, and it fails for
the intended reason rather than through a defect in the fix.

## Why the issue #680 contract is nevertheless intact

The two sibling tests in the same class that pin #680 both PASS after the fix:

| Test | Result |
|---|---|
| TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose | Passed |
| TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown | Passed |
| TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent | Passed |
| TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled | Passed |
| TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent | Passed |

The Down-arrow handoff test passes because the Down-arrow branch is one of the two search-driven
open sites and therefore takes dismissal ownership: its first leave is still consumed by the
one-shot handoff latch and its second leave still dismisses.

## Why the executor did not resolve it

Two constraints forbid every available workaround, and neither may be relaxed by the executor:

1. QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs is NOT one of the sixteen
   write-set paths recorded in evidence/baseline/p0-t14-scope-baseline.md. Editing it would breach
   the scope boundary that task P7-T4 and the Phase 9 scope gate both enforce.
2. Narrowing the AC4 fix so this test passes would mean not delivering AC4, because the state the
   test arranges is exactly the state AC4 requires to stop being dismissed. That is weakening a
   test's subject to make a gate pass.

## Required plan delta

The plan needs a Phase 8 or Phase 9 task, and a seventeenth write-set path, before task P9-T5 can
pass. Proposed shape, for `atomic-planner` to author properly:

- Add `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` to the write set under
  "Test — modify", raising the count from sixteen to seventeen, and update the matching count
  statements in this plan and in spec.md's `## Write Set` section.
- Add a task that performs a DELIBERATE UPDATE of
  TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent: keep the method name, keep the
  `Times.Once()` assertion, and add one Arrange line driving a search-driven open before the leave,
  so the test pins the same contract for the case that still holds. Add a paired negative test
  asserting `Times.Never()` for the mouse-driven case, or record that
  SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown in
  QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs already provides it.
- Update the P7-T4 and Phase 9 scope gates to admit the seventeenth path.

The whole-assembly baseline recorded at task P0-T10 is the reference for whether this is the only
such test; this finding reports the one failure that run surfaced and asserts nothing about tests
that run did not exercise.

Output Summary: 1380 tests, 1379 passed, 1 failed; the one failure is a pre-existing test outside
the write set that pins the behaviour AC4 removes; resolving it requires a plan delta adding a
seventeenth write-set path, which the executor may not author on its own authority.
