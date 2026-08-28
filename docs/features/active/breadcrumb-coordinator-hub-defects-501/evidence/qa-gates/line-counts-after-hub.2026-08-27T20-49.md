# QA Gate — Line Counts After the Hub Rewrite (P5-T10)

Timestamp: 2026-08-27T20-49

Instrument: `(Get-Content -LiteralPath <path>).Count` — physical line count including blank lines. The
`Measure-Object -Line` form was not used; it drops blank lines and undercounts.

| File | Baseline | Now | Headroom to 500 | Verdict |
| --- | ---: | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | **490** | 10 | at or below 500 |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | **492** | 8 | at or below 500 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 434 | **500** | 0 | at or below 500 |

Three numeric counts, each at or below 500. PASS.

## Budget notes carried forward to P7-T8

All three files are close to the cap, and the third is exactly AT it. Two facts make that safe against
the Phase 7 formatting pass:

1. `dotnet tool run csharpier check` was run against each of the three files individually as they were
   edited and reported each **already formatted** (`EXIT_CODE: 0`, no diff). A formatter that has no
   change to make cannot add lines, so the Phase 7 repository-wide `csharpier format .` will not move
   these counts.
2. The two test files were deliberately authored to their exact stated headroom rather than allowed to
   overshoot. `BreadcrumbSelectorCoordinatorTests.cs` was drafted at 531 lines — 31 over the cap — and
   then compacted in place to 500 by shortening doc comments and reason strings and by inlining a
   reflection helper as an expression-bodied member. No test method was relocated, no test was weakened
   or removed, and no new file was created, so the SR-1 / AC-23 / AC-24 project-file budget of exactly
   two added lines is untouched.

`QuickFiler/Viewers/BreadcrumbMessengerHub.cs` landed at 490 rather than the ~474 the spec estimated,
because the broadcast loop was extracted into the private `Broadcast` helper (authorized by P5-T6 as the
remedy if the file would otherwise approach the cap) and because the rewrite carries an XML `<remarks>`
block recording the SR-3 containment decision and the cache-placement rationale. 490 is the exact bound
P5-T6 required.
