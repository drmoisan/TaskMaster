# [P1-T20] Phase 1 post-format size gate

Timestamp: 2026-08-27T09-45
Command: `(Get-Content <path>).Count` for each path below, run after `[P1-T19]`'s formatting pass
EXIT_CODE: 0

| Path | Phase 0 baseline | Post-format count | At or below 500 |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | 146 | 182 | yes |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 88 | 125 | yes |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 181 | 272 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | n/a (added by this feature) | 92 | yes |

## Acceptance evaluation

- All four counts are at or below `500`. PASS (largest is 272).

The sizes are measured after formatting because CSharpier can change line counts; measuring before it
would report a figure the committed file does not carry.

Output Summary: 182, 125, 272, and 92 lines; all four at or below the 500-line cap with the tightest
at 272.
