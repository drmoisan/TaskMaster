# Final QA — 500-Line Cap Audit, POST-FORMAT LEG (P7-T8, AC-25 artifact of record)

Timestamp: 2026-08-27T21-07

## Produced after the final Phase 7 formatting run

This audit was produced **after** the final Phase 7 formatting pass completed. The ordering is:

1. P7-T1 ran `dotnet tool run csharpier format .` at 2026-08-27T20-57, rewriting 3 files.
2. P7-T2 ran `dotnet tool run csharpier check .` at 2026-08-27T20-57 and reported `EXIT_CODE: 0` with
   zero files needing formatting, establishing the formatter is at a fixed point.
3. Steps 2, 3 and 4 of the toolchain (analyze, type-check, test) wrote no source file.
4. This audit measured the files at 2026-08-27T21-07.

The counts below are therefore the counts the formatter will not move, which is what makes this the
artifact of record for AC-25's "after the change" condition. P6-T1 is the pre-format leg.

## Instrument

Instrument used: `(Get-Content -LiteralPath <path>).Count`

The `Measure-Object -Line` form was NOT used; it drops blank lines and undercounts (436 against 487
actual physical lines on the pre-split `BreadcrumbBridgeCoordinator.cs`).

## Rows

| # | Path | Pre-format (P6-T1) | Post-format | Delta | At or below 500 |
| ---: | --- | ---: | ---: | ---: | --- |
| 1 | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 378 | **378** | 0 | yes |
| 2 | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 353 | **353** | 0 | yes |
| 3 | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 490 | **490** | 0 | yes |
| 4 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 437 | **437** | 0 | yes |
| 5 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | 108 | **111** | +3 | yes |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 455 | **455** | 0 | yes |
| 7 | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 272 | **271** | -1 | yes |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 492 | **492** | 0 | yes |
| 9 | `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 500 | **500** | 0 | yes |
| 10 | `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` | 140 | **140** | 0 | yes |

Ten rows, one per audited file. **Files exceeding 500 lines: 0.** Every count is at or below 500.
**PASS.**

## Formatter deltas explained

Only two counts moved, and both are attributable to the three files P7-T1 rewrote:

- Row 5 gained 3 lines: CSharpier split the `SetSuggestionsCore` parameter list across three lines.
- Row 7 lost 1 line: CSharpier re-wrapped a FluentAssertions reason string more compactly.
- The third rewritten file, `BreadcrumbBridgeCoordinatorSupersessionTests.cs` (row 10), changed content
  but not line count.

## No remediation was required

Because no file exceeds 500 lines, the task's remediation branch did not fire. Consequently:

- No member was extracted and no test method was relocated.
- No further `<Compile Include>` line was added and no further new file was created, so the SR-1 / AC-23 /
  AC-24 budget of exactly two added project-file lines is intact.
- P6-T1, P6-T4, P6-T5 and P6-T8 did NOT need re-running, and their recorded evidence is not stale.
- The Phase 7 toolchain loop did NOT need restarting from P7-T1.

Row 9 sits exactly at the cap with zero headroom. That is by construction: the file was drafted at 531
lines during Phase 5 and compacted in place to exactly its 66-line stated headroom. `csharpier check`
confirms it is already formatted, so the formatter has no change to make to it and cannot push it over.
It is nonetheless the file with the least room for any future change, and is called out here for the
reviewer.
