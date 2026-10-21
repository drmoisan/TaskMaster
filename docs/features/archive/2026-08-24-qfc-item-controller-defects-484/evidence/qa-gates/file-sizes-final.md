# File sizes after the final formatting pass

Timestamp: 2026-08-26T14-08
Task: [P7-T8]

Command (run from the worktree root, after the `[P7-T1]` CSharpier pass):

```
grep -c '' <each of the nine owned files>
```

EXIT_CODE: 0

| File | Lines | <= 500 |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 499 | yes |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 482 | yes |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 257 | yes |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 338 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 499 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 489 | yes |

All nine owned files — four production partials and five test files — are named explicitly above with
their measured values. Every value is at most 500. The maximum observed value is 499
(`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`).

These counts are post-format: `[P7-T1]` confirmed by SHA-256 comparison that CSharpier rewrote none
of the nine, so no later reflow can change them.

## Divergence from the `spec.md` projected distribution (recorded, not corrected)

The `spec.md` file-size criterion states that the two owned test files receiving no added lines are
`QfcItemController.FocusAndThemeTests.cs` and `QfcItemController.ViewerSetupTests.cs`, "verified at
their unchanged 497 and 474 lines". In the delivered tree only the first of those is true:
`QfcItemController.FocusAndThemeTests.cs` is unchanged at 497, while
`QfcItemController.ViewerSetupTests.cs` is 498, because it received the #484
`Cleanup_NullsMailActions_AndSaveParametersRebindsIt` test.

That relocation is authorized by constraint C2 capacity rule 3 and was forced, not chosen: the planned
home `QfcItemController.MailActionsTests.cs` entered Phase 4 at 459 lines rather than the 184-line C2
baseline, because Phases 1 to 3 spent 275 of its lines, leaving 41 lines of headroom against the
500-line ceiling. The `[P4-T1]` and `[P4-T2]` tests consumed 39 of those. The 474 figure in `spec.md` is
a projection made under the superseded C2 assignment and became unsatisfiable at that point; no
allocation exists that keeps `ViewerSetupTests.cs` at 474 while placing all six Phase 4 and Phase 5
tests within the five owned test files.

The criterion's binding requirement - every production and test file touched by this feature is at most
500 lines after the change, with all nine owned files recorded with their post-change line counts - is
satisfied in full by the table above. The `spec.md` criterion text is not modified, per `[P8-T13]`.

Output Summary: Nine of nine owned files at most 500 lines; maximum 499. One divergence from the
`spec.md` projected per-file distribution is recorded above: `QfcItemController.ViewerSetupTests.cs` is
498 rather than the projected unchanged 474, under an authorized constraint C2 rule 3 relocation.
