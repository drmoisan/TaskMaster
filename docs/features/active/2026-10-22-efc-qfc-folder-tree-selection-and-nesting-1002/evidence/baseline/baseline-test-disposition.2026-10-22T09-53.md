# Baseline: Test Disposition for Changed Members (#1002)

Timestamp: 2026-10-22T09-53
Command: Grep (ripgrep) over `UtilitiesCS.Test/`, `QuickFiler.Test/`, `tests/quickfiler-breadcrumb/` for `(GetField|GetMethod|GetProperty)\(.*(ApplyDeleteGesture|ActionDeleteAsync|SelectHierarchyPath|SelectRow|ExpandLeafAsync|BindRowsAsync|BindBreadcrumbRowsAsync|SetLeafChildren|AppendChildren|IsKnownInboundType|makeSubfolders)`; second pass for `ApplyDeleteGesture`, `ButtonDeleteClickAsync`, `nameof(<member>)`, and quoted member-name string literals.
EXIT_CODE: 0
Output Summary:
- Same-line reflection pattern: 0 matches.
- Name-string pass: 2 matches, both in `QuickFiler.Test/Controllers/EfcFormControllerTests.cs`; both unchanged.

| Test / location | Match | Disposition |
|---|---|---|
| `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` (`[DataRow("ButtonDeleteClickAsync")]`, resolved through `GetMethod(memberName, ...)`) | reaches `ButtonDeleteClickAsync` by name, which calls `ActionDeleteAsync` | Unchanged: `ActionDeleteAsync` keeps awaiting the viewer UI context as its first statement, so the null `_formViewer` still faults first and the boundary still logs once. |
| Comment above `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` | comment-only `ApplyDeleteGesture` reference | Unchanged: comment text, not a reflection lookup; the test itself relies on the null-router guard, which P5-T2 preserves. |

No further match; no test must change, so the plan proceeds without a planner revision.
