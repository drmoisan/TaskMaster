# QA Gate — File-Size Limit (Issue #656)

Timestamp: 2026-09-01T14-53
Task: [P4-T9]
Satisfies: AC-13

Command:
```
(Get-Content -LiteralPath QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs).Count
(Get-Content -LiteralPath QuickFiler.Test\Viewers\BreadcrumbDropDownOpenCoordinatorTests.Part3.cs).Count
```

EXIT_CODE: 0

## Measured line counts

| File | Baseline (P0-T12) | Post-change | Limit | Under limit |
|---|---|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 378 | **395** | 500 | yes, by 105 lines |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | 173 | **213** | 500 | yes, by 287 lines |

Both counts are strictly less than 500, satisfying the file-size limit in
`.claude/rules/general-code-change.md` and AC-13.

Growth accounting:

- The coordinator grew by 17 lines: one hoisted statement, a 7-line `remarks` block on the
  `_closeCompleted` field, and a 9-line `remarks` block on `CloseCore`. The narrowed guard replaced
  an existing line in place and added none.
- `Part3.cs` grew by 40 lines: the added regression test method with its XML documentation.

This task runs after the final format pass in P4-T1, so both counts measure the CSharpier-formatted
files and no later formatting can change them.

Note on file placement: the regression test was appended to `Part3.cs` rather than to
`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` precisely because of this limit. `Part2.cs` stands
at 455 lines, so a roughly 40-line test would have brought it to about 495 — within a few lines of
the ceiling. `Part3.cs` is the same `public sealed partial class` and shares the same fixtures, so
the test lands in the correct class either way and no new file was created.

Output Summary: Both files in the authorized footprint are under the 500-line limit after the
change: 395 and 213 lines. AC-13 is satisfied.
