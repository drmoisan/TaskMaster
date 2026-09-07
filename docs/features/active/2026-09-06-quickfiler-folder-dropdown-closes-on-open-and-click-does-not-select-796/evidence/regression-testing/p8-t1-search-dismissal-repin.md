# P8-T1 — Deliberate re-pinning of TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent

Timestamp: 2026-09-07T15-00
Task: [P8-T1]
Issue: #796
Channel used: A

## What was changed and why

QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs arrived with the
original issue #680 fix at commit 660793e5, which predates this branch. Its test
`TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent` arranged an open
drop-down with no search-driven open and asserted one close intent. That is exactly the
mouse-driven state AC4 deliberately stops dismissing, so the test pinned a contract this
item supersedes.

The test is updated, never weakened and never deleted:

- the method name is unchanged;
- the assertion still reads `viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Once());`,
  so the issue #680 dismissal-ownership contract stays visibly pinned for the case that
  still holds;
- exactly one Arrange line was added, establishing search-driven ownership before the
  leave is raised;
- the doc comment was amended to state the search-ownership condition.

## The single added Arrange statement, verbatim

```
            QfcItemControllerTestSupport.SetField(controller, "_searchOwnedDismissal", true);
```

It is placed in the Arrange block immediately after the existing `BuildController(viewer)`
call. `QfcItemControllerTestSupport.SetField(QfcItemController, string, object)` is
declared at QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs line 40 and is
the same reflection injector this file already uses to install `_itemViewer`. The field
`_searchOwnedDismissal` is the AC4 provenance latch landed by task P6-T5, declared at
QuickFiler/Controllers/QfcItemController.EventHandlers.cs line 203 and read by
`TextBoxSearch_Leave` at line 263.

## Diff shape

Command:

```
git diff --numstat -- QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
```

Output:

```
7	0	QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
```

Seven lines added, none removed. Six of the seven are `///` documentation-comment lines
amending the test's doc comment. The seventh is the single Arrange statement quoted
above, which is therefore the only non-comment line this task adds anywhere. No
production file is edited by this task, and the other five `[TestMethod]` members in the
class are unchanged.

## `[TestMethod]` count, measured with the P1-T7 command form applied to this file

Command:

```
pwsh -NoProfile -Command '(Select-String -Path QuickFiler.Test\Controllers\QfcItemController.SearchDismissalTests.cs -SimpleMatch "[TestMethod]").Count'
```

Output:

```
6
```

Six, unchanged by this task: it adds no test and removes none.

## Compile gate — the P0-T8 command form

Command: the P0-T8 command form with the log path
`TestResults\796\p8-t1\analyzer-rebuild.log`.

RunStartedUtc: 2026-09-07T18:59:16.8282314Z

EXIT_CODE: 0

Build summary, verbatim:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:22.07
```

CscTaskCount=36
CscToolCount=36

| Assembly | LastWriteTimeUtc | At or later than RunStartedUtc |
|---|---|---|
| QuickFiler/bin/Debug/QuickFiler.dll | 2026-09-07T18:59:27.6510553Z | yes |
| QuickFiler.Test/bin/Debug/QuickFiler.Test.dll | 2026-09-07T18:59:31.5292881Z | yes |

Raw log (gitignored): TestResults/796/p8-t1/analyzer-rebuild.log

Output Summary: the method name and the `Times.Once()` assertion are unchanged; one
Arrange statement and six comment lines were added and nothing was removed; the file
still declares 6 `[TestMethod]` members; the solution compiles under the P0-T8 command
form with EXIT_CODE 0, 0 warnings and 0 errors, 36 Csc task and 36 csc.exe tool
invocations, and both touched assemblies rebuilt after RunStartedUtc.
