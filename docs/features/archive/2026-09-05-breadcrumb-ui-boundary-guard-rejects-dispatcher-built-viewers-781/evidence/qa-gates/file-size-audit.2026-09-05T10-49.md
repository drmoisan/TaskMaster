# File-Size Audit After Formatting (issue #781)

Timestamp: 2026-09-05T17-12

Task: [P2-T10]

Command: `pwsh -NoProfile -Command` over a block that reads each of the three paths with
`Get-Content -LiteralPath` and reports its resulting element count, run from the repository root.
This runs after [P2-T1] because CSharpier can change line counts.

EXIT_CODE: 0

## Output Summary

| Repository-relative path | Lines | Limit | Verdict |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 456 | 500 | PASS |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | 419 | 500 | PASS |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 388 | 500 | PASS |

All three counts are at or below the 500-line ceiling the General Code Change Policy sets, so the
acceptance condition holds.

Movement from the pre-change state, for context:

- `ItemViewer.Breadcrumb.cs` grew from 449 to 456 lines. The guard body is one line shorter, and
  the rewritten XML documentation on `ThrowIfOffUiBoundary` is eight lines longer, because it now
  carries the dispatcher-context explanation and the issue #781 reference AC6 requires.
- `ItemViewerBreadcrumbLifecycleRegressionTests.cs` fell from 480 to 388 lines, a decrease of 92,
  which is the two deleted D4 tests with their documentation blocks less the three lines added by
  the `SetViewerSyncContext` comment correction.
- `ItemViewerBreadcrumbThreadAffinityTests.cs` is new. CSharpier reduced it from 420 to 419 lines
  in [P2-T1] by collapsing one lambda assignment onto a single line. This is why the plan placed
  the deletion's line-count condition as a decrease rather than an exact figure: the exact
  post-format count is not predictable before the formatter runs.
