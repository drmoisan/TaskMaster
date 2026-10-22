# P3-T4 — Pre-fix occurrence count of the literal `Parent Changed`

Timestamp: 2026-08-28T00-42
Command: git grep -F -n "Parent Changed" -- QuickFiler/Viewers/
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance — exactly 2 matches, one per file

```
QuickFiler/Viewers/ItemViewer.cs:168:            Console.WriteLine("Parent Changed");
QuickFiler/Viewers/ItemViewerExpanded.cs:156:            Console.WriteLine("Parent Changed");
```

MatchCount: 2
MatchInItemViewer: 1
MatchInItemViewerExpanded: 1

Exactly two matches in total, exactly one of which is in `QuickFiler/Viewers/ItemViewer.cs` and
exactly one of which is in `QuickFiler/Viewers/ItemViewerExpanded.cs`. No other file under
`QuickFiler/Viewers/` contains the literal. The acceptance condition is file identity and match count
only; no line number is asserted.

## Observed line numbers, recorded for the record

ObservedLineItemViewer: 168
ObservedLineItemViewerExpanded: 156

Both agree with the plan's prediction. At `BASELINE_SHA` the two matches stood at `ItemViewer.cs:168`
and `ItemViewerExpanded.cs:160`. P2-T1 deleted four lines from `ItemViewerExpanded.cs` at `:24-27`,
so the second match now reports four lines earlier, at `:156`. `ItemViewer.cs:168` is unshifted
because every P2-T4 deletion in that file is at `:171` or below.

Each match is the sole statement in the body of the `L0v2h2_WebView2_ParentChanged` member — the
member declared at `ItemViewer.cs:166` and `ItemViewerExpanded.cs:154` — so removing the two members
in Phase 4 necessarily removes both matches.

The `git grep` here is deliberately left bare rather than wrapped in `(… | Measure-Object).Count`,
because a *passing* pre-fix state has matches and therefore exits `0`. P4-T7, which asserts the
post-fix count is zero, must use the wrapped form: a bare `git grep` exits `1` when nothing matches,
which would record `EXIT_CODE: 1` on a passing gate and normalize that artifact to `fail`.

This is the fail-before record for the AC13 zero-match assertion.

Output Summary: The literal `Parent Changed` occurs exactly twice under `QuickFiler/Viewers/` before
the Phase 4 deletions — once in `ItemViewer.cs` at `:168` and once in `ItemViewerExpanded.cs` at
`:156` — with `EXIT_CODE: 0`. Both are the single-statement bodies of the dead
`L0v2h2_WebView2_ParentChanged` handlers. The observed line numbers match the plan's predicted shift
of the second match from `:160` to `:156` caused by P2-T1's four-line deletion. This is the
fail-before record for AC13's zero-match assertion, which P4-T7 verifies.
