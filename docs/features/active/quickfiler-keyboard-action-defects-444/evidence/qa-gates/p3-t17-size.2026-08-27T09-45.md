# [P3-T17] Phase 3 post-format size gate

Timestamp: 2026-08-27T09-45
Command: `(Get-Content <path>).Count` for each path below, run after `[P3-T16]`'s formatting pass
EXIT_CODE: 0

| Path | `[P0-T21]` baseline | Post-format count | At or below 500 |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 228 | 252 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | 391 | 498 | yes |

## Line-budget outcome

The test file's budget was the binding constraint of this phase: 391 lines at baseline against a
500-line cap left 109 lines for the shared builder, the assertion helper, and the four named tests.
The delivered block consumes **107** of those 109 lines, leaving 2 spare.

`REMEDIATION-REQUIRED: Phase 3 line budget exhausted` is **not** written: the post-format count is 498,
below the cap.

## How the budget was met

The plan's three authorized compaction techniques were applied while the block was being authored,
before this gate ran, rather than as a remediation loop afterwards:

1. **One-line `///` summaries** on each of the four tests, whose method names already state the
   scenario. The only multi-line documentation retained is on the shared builder, where the reason
   `ItemHelper.UnRead` is asserted rather than assigned is not derivable from any name.
2. **Object and collection initializers** in the builder's mock arrangement, plus a local `set`
   delegate so the five reflection injections are one line each instead of five multi-line calls.
3. **A single assertion helper**, `BothRegistriesShouldHold`, that asserts both registries in one LINQ
   expression. Three of the four tests call it, which removed twelve separate multi-line
   FluentAssertions chains.

An intermediate draft measured 492 lines after the builder and the first test alone, which would have
left 8 lines for three further tests. It was compacted to 465 at that point using the same three
techniques. None of the four named `[TestMethod]`s was deleted or merged, no second test file was
created, no second `<Compile Include>` line was added, and no test was moved into
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`.

## Acceptance evaluation

- Both counts are at or below `500`. PASS (252 and 498).

Output Summary: `QfcItemController.Navigation.cs` 252 lines, `QfcItemController.NavigationTests.cs` 498
lines; both under the cap with 2 lines spare in the test file; no remediation branch taken.
