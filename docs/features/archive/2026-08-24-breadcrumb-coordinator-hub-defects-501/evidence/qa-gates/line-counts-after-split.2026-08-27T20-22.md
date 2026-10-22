# QA Gate — Line Counts After the SR-1 Partial Split (P2-T5)

Timestamp: 2026-08-27T20-22

Instrument: `(Get-Content -LiteralPath <path>).Count` — the physical line count including blank lines.

The `Get-Content -LiteralPath <path> | Measure-Object -Line` form was NOT used. `Measure-Object -Line`
drops blank lines and undercounts: it reported 436 against 487 actual physical lines on the pre-split
primary file.

| File | Lines | Required bound | Verdict |
| --- | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | **437** | at or below 445 | SATISFIED |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | **72** | at or below 100 | SATISFIED |

## Budget consequence

The primary file moved from 487 to 437 lines, freeing 50 lines and taking its headroom to 500 from 13
to 63. That headroom is what makes the Phase 4 #502 call-site change safe: the change is estimated at
+13 to +17 lines, and it now lands in the new part rather than the primary file in any case.

The new part sits at 72 lines against the 120-line ceiling that P4-T5 and P4-T6 will enforce after the
`SetSuggestionsCore` seam and the two `false` branches are added, leaving 48 lines of headroom for
them.

Acceptance: the primary file is at or below 445 lines (437) and the new part is at or below 100 lines
(72). PASS.
