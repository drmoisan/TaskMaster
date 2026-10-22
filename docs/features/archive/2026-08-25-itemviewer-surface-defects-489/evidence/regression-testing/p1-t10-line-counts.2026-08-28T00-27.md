# P1-T10 — Line counts after the Phase 1 RED tests

Timestamp: 2026-08-28T00-27
Command: (Get-Content -LiteralPath <path>).Count for each of the five paths below
EXIT_CODE: 0

```
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs                    = 165
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs   = 189
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs      = 499
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs = 81
QuickFiler.Test/QuickFiler.Test.csproj                                 = 495
```

Counts are taken with `(Get-Content -LiteralPath <path>).Count`, not `Measure-Object -Line`; the two
disagree on files without a trailing newline.

## Acceptance

| Path | Count | Condition | Met |
|---|---:|---|---|
| `ToolStripMenuItemCbTests.cs` | 165 | at most 500 | yes |
| `ItemViewerBreadcrumbDropDownContractTests.cs` | 189 | at most 500 | yes |
| `QfcItemController.EventWiringTests.cs` | 499 | at most 500 **and** equal to its P0-T15 baseline of 499 | yes |
| `QfcItemController.EventWiringTests.Part2.cs` | 81 | at most 500 | yes |
| `QuickFiler.Test.csproj` | 495 | equal to its P0-T15 baseline of 493 plus exactly 2 | yes |

The `EventWiringTests.cs` parent is **unchanged in size** at 499, which is the measurable proof that
P1-T4's `public class` to `public partial class` edit is line-neutral:
`git diff --numstat <BASELINE_SHA> -- QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`
reports exactly `1` added and `1` deleted. Had the two new tests been appended there as the plan
originally routed them, the file would have breached the 500-line ceiling that
`.claude/rules/general-code-change.md` sets and that spec AC47 asserts; that is why P1-T4 created
the `Part2` continuation instead.

The project file gained exactly two lines in Phase 1 — `Viewers\ToolStripMenuItemCbTests.cs` at
P1-T2 and `Controllers\QfcItemController.EventWiringTests.Part2.cs` at P1-T4. It will gain two more
across the whole plan, at P5-T2 and P7-T3, for a final total of four appended entries against the
baseline of 493. `git diff --numstat <BASELINE_SHA>` reports `2` added and `0` deleted for the
project file, so no pre-existing entry moved, was reordered, or was dropped — including the eight
`Controllers\QfcCollectionController*` entries owned by merged sibling 444 and the two
`Controllers\QfcItemController.UiThreadDispatcherFixture*` entries owned by merged sibling 493.

Output Summary: All five counts satisfy P1-T10. The three test files this phase created or appended
to sit well under the 500-line ceiling at 165, 189 and 81 lines. The
`QfcItemController.EventWiringTests.cs` parent is exactly 499, equal to its P0-T15 baseline, which
confirms the `partial` modifier edit added and removed no line. `QuickFiler.Test.csproj` is 495,
exactly the 493 baseline plus the two entries Phase 1 appended, with `2` added and `0` deleted in
the diff so no sibling-owned entry was disturbed.
