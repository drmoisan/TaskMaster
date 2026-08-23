# Phase 5 — Dialog-Seam Tests (Issue #449, [P5-T12])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

## Preceding format and build

Command: `dotnet tool run csharpier format "QuickFiler.Test\Controllers\QfcExplorerControllerTests.cs"`
then `dotnet tool run csharpier check "QuickFiler.Test\Controllers\QfcExplorerControllerTests.cs"`
EXIT_CODE: 0 (check)

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```
EXIT_CODE: 0

### One test-file-only correction was required first

The first build attempt returned **EXIT_CODE 1** with seven diagnostics, all in the new test file and
all the same root cause — the file had no `using System.Windows.Forms;`, so the seam's parameter and
return types were unresolvable:

```
QfcExplorerControllerTests.cs(193,13): error CS0246: The type or namespace name 'MessageBoxButtons' could not be found
QfcExplorerControllerTests.cs(194,13): error CS0246: The type or namespace name 'MessageBoxIcon' could not be found
QfcExplorerControllerTests.cs(203,24): error CS0103: The name 'DialogResult' does not exist in the current context
QfcExplorerControllerTests.cs(215,41): error CS0103: The name 'MessageBoxButtons' does not exist in the current context
QfcExplorerControllerTests.cs(216,38): error CS0103: The name 'MessageBoxIcon' does not exist in the current context
QfcExplorerControllerTests.cs(228,83): error CS0103: The name 'DialogResult' does not exist in the current context
QfcExplorerControllerTests.cs(246,83): error CS0103: The name 'DialogResult' does not exist in the current context
```

`using System.Windows.Forms;` was added to the TEST file only. **No production file was changed to
accommodate the test**, consistent with the [P1-T5] rule that a CS0246 or Moq setup-shape error is
corrected in the test file. Adding the directive introduced no CS0104 ambiguity with
`Microsoft.Office.Interop.Outlook` (which also declares `View` and `Application`), because the test
file refers to those types only through the `Outlook` alias and never by bare name. The file was
re-formatted and the build re-run, returning EXIT_CODE 0.

## Test run

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" `
  "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings `
  /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~OpenQFItem_WhenItemNotSelectableInView_InvokesDialogSeamOnce|FullyQualifiedName~OpenQFItem_WhenDialogSeamReturnsYes_DisplaysMailItem|FullyQualifiedName~OpenQFItem_WhenDialogSeamReturnsNo_DoesNotDisplayMailItem"
```
EXIT_CODE: 0

The three test names are joined with the `|` operator. `vstest.console.exe` rejects the word `OR` in a
`/TestCaseFilter`, so `|` is the only usable disjunction. `/InIsolation` was supplied.

Output:
```
VSTest version 18.8.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Test Parallelization enabled for ...\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed OpenQFItem_WhenItemNotSelectableInView_InvokesDialogSeamOnce [337 ms]
  Passed OpenQFItem_WhenDialogSeamReturnsYes_DisplaysMailItem [6 ms]
  Passed OpenQFItem_WhenDialogSeamReturnsNo_DoesNotDisplayMailItem [1 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
 Total time: 2.4479 Seconds
```

| Metric | Value |
| --- | --- |
| Executed | **3** |
| Passed | **3** |
| Failed | **0** |
| Skipped | **0** |

## What the three tests establish

- **[P5-T9] `OpenQFItem_WhenItemNotSelectableInView_InvokesDialogSeamOnce`** — arranges
  `IsItemSelectableInView` to return `false` so the not-in-view branch is taken, replaces
  `NotInViewDialogInvoker` with a counting stub returning `DialogResult.No`, and asserts the stub was
  invoked **exactly once** with all four expected argument values: the text
  `"Selected message is not in view. Would you like to open it?"`, the caption `"Error"`,
  `MessageBoxButtons.YesNo`, and `MessageBoxIcon.Error`. This is the behavioural proof of AC-10's
  routing claim — the assertion can only hold if `OpenQFItem` calls the seam rather than the dialog
  API directly.
- **[P5-T10] `OpenQFItem_WhenDialogSeamReturnsYes_DisplaysMailItem`** — stub returns
  `DialogResult.Yes`; asserts `mailItem.Verify(m => m.Display(It.IsAny<object>()), Times.Once())`,
  using the `Display` overload shape the PIA actually declares (confirmed against the in-repo
  precedents at `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs:63` and
  `TaskTree.Test/TaskTreeControllerActivateTests.cs:82`).
- **[P5-T11] `OpenQFItem_WhenDialogSeamReturnsNo_DoesNotDisplayMailItem`** — stub returns
  `DialogResult.No`; asserts `Display` is never invoked.

Together these cover both outcomes of the previously untestable branch, which is the substantive
justification for D5's decision to REMOVE the class-level `[ExcludeFromCodeCoverage]` outright rather
than narrow it onto `OpenQFItem`.

## The seam default was never exercised — no dialog was displayed

Every one of the three tests **replaces** `NotInViewDialogInvoker` before invoking `OpenQFItem`, so the
production default at `QfcExplorerController.cs:63` — the only `MessageBox.Show` call site in the file
— is never reached. No modal dialog was displayed, no message pump was started, and no test blocked on
user input. The sub-second durations (337 ms, 6 ms, 1 ms) corroborate this: a real modal dialog would
have hung the run indefinitely.

## Determinism

No `Thread.Sleep`, no `Task.Delay`, no wall-clock read, no temporary file, no live `Form`, no
`Application.Run`, and no `MessageBox.Show` appears in the test file. The stubs are plain lambdas that
return a fixed `DialogResult`.

## Output Summary

**EXIT_CODE: 0. Three tests executed, three passed**, zero failed, zero skipped (337 ms / 6 ms / 1 ms).
The preceding scoped CSharpier check and the full-solution `/t:Rebuild` both returned EXIT_CODE 0,
after one test-file-only correction adding `using System.Windows.Forms;` to resolve seven CS0246/CS0103
diagnostics — no production file was changed to accommodate the tests. The three test names were joined
with `|` because `vstest.console.exe` rejects `OR`. All three tests replace the seam before acting, so
the production `MessageBox.Show` default was never exercised and no dialog was displayed.
