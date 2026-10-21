# P0-T13 — Physical line-count baseline

Timestamp: 2026-09-13T15-02
Command: (Get-Content -LiteralPath <P>).Count, applied once to each of the five paths below
EXIT_CODE: 0

## Measured counts

QfcQueueLineCount: 507
QfcQueueEnqueueLineCount: 200
QfcQueueTestsLineCount: 67
QfcQueueCoverageExpansionTestsLineCount: 290
QfcQueuePurePathsTestsLineCount: 418

Mapped to their paths:

- `QuickFiler/Controllers/QfcQueue.cs` = 507
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs` = 200
- QuickFiler.Test/Controllers/QfcQueueTests.cs = 67
- QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs = 290
- QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs = 418

The three test files are the three existing QfcQueue test files in the QuickFiler test project's
Controllers folder, enumerated from that folder rather than assumed. They are named here without
backticks because this plan does not write to them, and the document's formatting contract reserves
backticked path tokens for the Write Set.

## Acceptance

The acceptance condition names two exact figures. Both hold:

- The recorded count for `QuickFiler/Controllers/QfcQueue.cs` is exactly 507.
- The recorded count for `QuickFiler/Controllers/QfcQueue.Enqueue.cs` is exactly 200.

No divergence. The tree did not move in either file, so the split ranges in Phase 1 do not need to be
re-derived and are used exactly as the plan states them.

## Re-measured after the merge, not carried forward

These counts were measured against the post-merge tree at
8213826f695439e86e3ed34faa575de493a11ec7, not carried forward from the authoring pass. The merge that
produced that commit is the reason the check matters here: it brought 91 files in from origin/main, so
a count taken before it would have been evidence about a superseded tree. An anchored diff confirms the
merge touched none of the five production paths in this item's Write Set; within the Write Set it
touched only QuickFiler.Test/QuickFiler.Test.csproj, which this task does not measure.

The two region citations Phase 1 depends on were re-derived in the same pass and are also unmoved: in
`QuickFiler/Controllers/QfcQueue.cs` the Tlp Manipulation region opens at line 230 and closes at line
453, and the Helper Methods region opens at line 472 and closes at line 505. Those are exactly the
ranges P1-T1 and P1-T2 name.

Output Summary: All five files measured with CMD-LINECOUNT against the post-merge tree.
`QuickFiler/Controllers/QfcQueue.cs` is exactly 507 lines and
`QuickFiler/Controllers/QfcQueue.Enqueue.cs` is exactly 200, which are the two figures the acceptance
condition requires, so the Phase 1 split ranges stand as written. The three existing QfcQueue test files
measure 67, 290 and 418 lines. Acceptance met.
