# `[expect-fail]` — Defect 2 Regression Test Observed FAILING Before the Fix (Issue #449, [P1-T6])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Test: `QuickFiler.Controllers.Tests.QfcExplorerControllerTests.OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer`
Production code state: **UNFIXED**. `QuickFiler/Controllers/QfcExplorerController.cs:140` still reads
`_globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)mailItem.Parent;`. [P2-T1] has not run.

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" `
  "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings `
  /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer"
```
EXIT_CODE: 1
ExpectedExitCode: 1

A non-zero exit code is the EXPECTED and REQUIRED outcome of this task. `vstest.console.exe` returns
`1` when any test in the run fails. The assembly discovery of [P0-T12] was applied and restricted to
`QuickFiler.Test.dll`; `/InIsolation` was supplied so the assembly's `app.config` binding redirects
take effect (without it a mass of roughly 1,695 phantom failures with empty messages and
sub-millisecond durations appears via a Moq `TypeInitializationException`). The single failure below
has a real message and a 399 ms duration, so it is a genuine assertion failure, not a load failure.

## Result

```
Total tests: 1
     Failed: 1
Test Run Failed.
 Total time: 2.0951 Seconds
```

**The named test FAILED.** One test executed, one failed.

## Which of the two `VerifySet` assertions fired first

The **first** assertion fired — the one asserting the destination is assigned to the CAPTURED
explorer. Verbatim failure text:

```
  Failed OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer [399 ms]
  Error Message:
   Test method QuickFiler.Controllers.Tests.QfcExplorerControllerTests.OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: e => e.CurrentFolder = Mock<MAPIFolder:2>.Object

Performed invocations:

   Mock<Explorer:1> (e):

      _Explorer.CurrentFolder  => Mock<MAPIFolder:1>
      _Explorer.CommandBars  => Mock<CommandBars:1>
      _Explorer.CommandBars  => Mock<CommandBars:1>
      _Explorer.IsItemSelectableInView(Mock<MailItem:1>.Object)
      _Explorer.ClearSelection()
      _Explorer.AddToSelection(Mock<MailItem:1>.Object)

   Mock<MAPIFolder:1>:

      MAPIFolder.FolderPath

   Mock<CommandBars:1>:

      _CommandBars.GetPressedMso("ShowInConversations")
      _CommandBars.GetPressedMso("ShowInConversations")

  Stack Trace:
     at Moq.Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage) in /_/src/Moq/Mock.cs:line 332
   at Moq.Mock.VerifySet(Mock mock, LambdaExpression expression, Times times, String failMessage) in /_/src/Moq/Mock.cs:line 355
   at Moq.Mock`1.VerifySet(Action`1 setterExpression, Times times) in /_/src/Moq/Mock`1.cs:line 1045
   at QuickFiler.Controllers.Tests.QfcExplorerControllerTests.<OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer>d__14.MoveNext() in ...\QuickFiler.Test\Controllers\QfcExplorerControllerTests.cs:line 157
```

The stack trace resolves to the FIRST of the two assertions, at test-file line 157:
`_explorer.VerifySet(e => e.CurrentFolder = destination.Object, Times.Once());`
The second assertion (`driftedExplorer ... Times.Never()`) was never reached, because the first threw.

## Why this failure is the defect, and proof the arrangement reached line 140

The `Performed invocations` block is the substantive evidence. On the captured explorer
(`Mock<Explorer:1>`) the recorded interactions are:

| Invocation | Production site | Meaning |
| --- | --- | --- |
| `_Explorer.CurrentFolder => Mock<MAPIFolder:1>` | line 136 | the **GETTER**, read by the guard. Not a setter. |
| `_Explorer.CommandBars` (x2) | lines 141 and 152, via `AutoFile.AreConversationsGrouped` | see the non-short-circuit note below |
| `_Explorer.IsItemSelectableInView(...)` | line 156 | the in-view branch was taken |
| `_Explorer.ClearSelection()` | line 158 | positive path executed |
| `_Explorer.AddToSelection(...)` | line 159 | positive path executed |

There is **no `CurrentFolder` setter invocation on the captured explorer**, only a getter. The
assignment at line 140 went to the drifted explorer instead, which is exactly defect 2: the
controller re-resolved `_globals.Ol.App.ActiveExplorer()` at call time rather than using the
`_activeExplorer` it captured at construction (line 35).

The arrangement provably reached line 140 rather than short-circuiting earlier:

1. `MAPIFolder.FolderPath` was read, so the guard at lines 135-137 was evaluated.
2. The guard's two paths differ (`\\Mailbox\A` against `\\Mailbox\B`), so the branch was ENTERED.
3. `ClearSelection` and `AddToSelection` fired at lines 158-159, which are downstream of line 140, so
   execution passed through line 140.
4. The `SetupSequence` second element was consumed — the drifted explorer received the assignment,
   which is only reachable through the line-140 re-resolution.

Had the test PASSED at this point, the plan requires treating it as an arrangement that never reaches
line 140 and correcting the test before proceeding. It failed, and it failed for the right reason at
the right assertion, so no correction is needed.

### The `CommandBars` double read confirms the non-short-circuiting `&`

`GetPressedMso("ShowInConversations")` was invoked **twice** even though the controller was
constructed with `QfEnums.InitTypeEnum.Find` (value 2), which makes
`_initType.HasFlag(QfEnums.InitTypeEnum.Sort)` (`Sort` = 1) false at both lines 151 and 179. Both
conjunctions use the non-short-circuiting `&` operator rather than `&&`, so
`AutoFile.AreConversationsGrouped(_activeExplorer)` is still evaluated, and that helper reads
`ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations")`
(`UtilitiesCS/EmailIntelligence/EmailParsingSorting/AutoFile.cs:122-136`). The second read is from
line 141. This empirically confirms that the `CommandBars` mock setup is **mandatory**, not optional —
the detail [P6-T4] records in the fixture comment.

## Determinism

The test uses no `Thread.Sleep`, no `Task.Delay`, no wall-clock read, no temporary file, no live
`Form`, no message pump, and no `MessageBox.Show`. The dialog branch is deliberately unreachable
because `IsItemSelectableInView` is arranged to return `true`. The failure is a deterministic
consequence of production logic, not of timing.

## Output Summary

The defect-2 regression test was observed **FAILING** against unfixed production code:
**EXIT_CODE 1** (ExpectedExitCode 1), 1 test executed, 1 failed, 399 ms. The failure is a
`Moq.MockException` from the FIRST of the two `VerifySet` assertions —
`Expected invocation on the mock once, but was 0 times: e => e.CurrentFolder = Mock<MAPIFolder:2>.Object`
— proving the destination folder was never assigned to the constructor-captured explorer. The
`Performed invocations` list shows the captured explorer received only a `CurrentFolder` GETTER (the
guard read at line 136) plus the downstream `ClearSelection`/`AddToSelection` calls, which together
prove execution passed through line 140 and that the assignment landed on the drifted explorer. This
is the fail-before observation required by the Bugfix Workflow; [P2-T1] applies the fix and [P2-T2]
records the pass-after run.
