# Pass-After — Defect 2 Regression Test Passes With the Fix Applied (Issue #449, [P2-T2])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Test: `QuickFiler.Controllers.Tests.QfcExplorerControllerTests.OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer`

## Cross-reference to the fail-before observation

Fail-before artifact: **`expect-fail-defect2.2026-08-22T09-16.md`**
(`<FEATURE>/evidence/regression-testing/expect-fail-defect2.2026-08-22T09-16.md`)

That artifact records the SAME test method, by the same fully-qualified name, failing with
`EXIT_CODE: 1` against unfixed production code, with the first of its two `VerifySet` assertions
throwing `Moq.MockException: Expected invocation on the mock once, but was 0 times: e => e.CurrentFolder = Mock<MAPIFolder:2>.Object`.
No test code changed between the two runs; the only difference is the [P2-T1] production fix.

## The fix under test ([P2-T1])

`QuickFiler/Controllers/QfcExplorerController.cs`, inside the private helper
`NavigateToOutlookFolder(MailItem)`, line 140:

```diff
                 ExplConvView_ReturnState();
-                _globals.Ol.App.ActiveExplorer().CurrentFolder = (MAPIFolder)mailItem.Parent;
+                _activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;
                 BlShowInConversations = AutoFile.AreConversationsGrouped(_activeExplorer);
```

The right-hand side `(MAPIFolder)mailItem.Parent` is unchanged. Only the assignment TARGET changed,
from a freshly re-resolved explorer to the one captured at construction (line 35). The file's line
count is unchanged at **323**, and the diff for this phase is exactly one changed line.

## Preceding rebuild

Command:
```
"C:\Program Files\...\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug `
  "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```
EXIT_CODE: 0

`/t:Rebuild` was used so the test assembly genuinely picked up the changed production binary. A warm
`/t:Build` could have skipped `CoreCompile` and left the test running against the pre-fix DLL, which
would have made this pass-after observation meaningless.

## Test run

Command (identical to [P1-T6] apart from nothing — same binary path, same settings, same filter):
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" `
  "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings `
  /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer"
```
EXIT_CODE: 0

Output:
```
VSTest version 18.8.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Test Parallelization enabled for ...\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer [343 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 2.0412 Seconds
```

**One test executed, one passed.**

Both assertions now hold:

- `_explorer.VerifySet(e => e.CurrentFolder = destination.Object, Times.Once())` — the destination is
  assigned to the constructor-captured explorer exactly once.
- `driftedExplorer.VerifySet(e => e.CurrentFolder = It.IsAny<MAPIFolder>(), Times.Never())` — the
  drifted explorer, reachable only by re-resolving `ActiveExplorer()`, is never navigated. This
  assertion was not even reached in the fail-before run because the first one threw; it passes now.

The `SetupSequence` second element is consequently never consumed after the fix, which is the direct
observable signature of the re-resolution having been removed.

## Output Summary

**EXIT_CODE: 0. One test executed, one passed** (343 ms), after a clean `/t:Rebuild` (EXIT_CODE 0).
The same test, by the same fully-qualified name and with unchanged test code, failed with EXIT_CODE 1
in the fail-before artifact `expect-fail-defect2.2026-08-22T09-16.md`. The fail-before / pass-after
pair required by the Bugfix Workflow is therefore complete for defect 2, and the one-line [P2-T1] fix
at `QfcExplorerController.cs:140` is the only difference between the two runs.
