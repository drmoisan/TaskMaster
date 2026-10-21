# Phase 3 Verification — UI-Facing Files (issue #742, [P3-T4])

Timestamp: 2026-09-14T02-18

Command: `git grep -c '[.]ToString[(]@\?"[^"]*[:/.\-][^"]*"[)]' -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs`

EXIT_CODE: 1

Output Summary: the command printed no line for any of the three paths and exited 1. The baseline
combined total for these three files was 1 + 3 + 2 = 6 ([P0-T9] control 1), distributed as
`QfcItemController.ViewerSetup.cs:1`, `QfcCollectionController.cs:3` and `EfcItemController.cs:2`.

Acceptance: satisfied, read per this plan's zero-match convention — `git grep -c` prints no line at
all, and exits 1, for a path with zero matching lines, and never prints a `0`.

## Discovery-control note (guard against a vacuous zero)

The identical pattern, in the same script and the same invocation form, printed
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:2` in [P2-T4], so the pattern still matches on
this tree and this zero is a real observation rather than a search that cannot match. The same three
paths returned 1, 3 and 2 under this pattern immediately before the Phase 3 edits were applied.

## Sites changed in Phase 3

- `QfcItemController.ViewerSetup.cs` — both `ToString` calls in `GetItemSummary`, plus a new
  `using System.Globalization;` directive (exactly one occurrence, confirmed in [P3-T1]).
- `QfcCollectionController.cs` — the `TryGetMoveReadiness` call, the two calls in the
  `ToggleExpansionStyle` exception-message builder, and the two calls in the `GetMoveDiagnostics`
  line builder, plus a new `using System.Globalization;` directive (exactly one occurrence,
  confirmed in [P3-T2]).
- `EfcItemController.cs` — the `SentDate` and `SentTime` property getters, plus a new
  `using System.Globalization;` directive (exactly one occurrence, confirmed in [P3-T3]).

Only argument lists and the three using-directive insertions were changed. No interpolation, field
order, or CSV column count was altered.
