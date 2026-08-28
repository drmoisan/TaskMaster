# [P4-T3] Post-format file-size audit

Timestamp: 2026-08-27T19-49
Command: `(Get-Content -LiteralPath <path>).Count` for each of the seven owned paths plus `QuickFiler.Test/QuickFiler.Test.csproj`, run after the `[P4-T1]` formatting pass and after the `[P4-T2]` repository-wide verification
EXIT_CODE: 0
Output Summary: all eight counts recorded. Every `.cs` path created by this feature and every
pre-existing `.cs` path except `QfcCollectionController.cs` is at or below 500.
`QfcCollectionController.cs` is 2437, exactly equal to and therefore not greater than its
`[P0-T21]` baseline, taking the second acceptance branch.

## Counts

| Path | Origin | `[P0-T21]` baseline | Post-change count | Branch | Verdict |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/KbdActions.cs` | pre-existing | 146 | 182 | at or below 500 | PASS |
| `QuickFiler/Controllers/QfcCollectionController.cs` | pre-existing | 2437 | 2437 | not greater than baseline | PASS |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | pre-existing | 228 | 252 | at or below 500 | PASS |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | pre-existing | 88 | 125 | at or below 500 | PASS |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | pre-existing | 181 | 272 | at or below 500 | PASS |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | **created by this feature** | none | 226 | at or below 500 | PASS |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | pre-existing | 391 | 498 | at or below 500 | PASS |
| `QuickFiler.Test/QuickFiler.Test.csproj` | pre-existing | not baselined | 491 | not a `.cs` path; recorded for completeness | recorded |

## Paths taking the second acceptance branch

`QuickFiler/Controllers/QfcCollectionController.cs` — post-change count 2437, `[P0-T21]` baseline
count 2437. The post-change count is **not greater than** the baseline, which is the second
disjunct of the acceptance condition.

**Decision D-P6 statement.** This file's excess over the 500-line cap is a **pre-existing**
condition. The file was already 2437 lines at the Phase 0 baseline, before any edit by this
feature. This feature neither created that excess nor is permitted to remediate it: splitting a
2437-line controller is a structural refactor outside a defect fix's scope and would collide with
three concurrently-live sibling features that share `QuickFiler.csproj`. The feature's diff against
the merge base is `8` insertions and `8` deletions on this file (`git diff --numstat`), a net change
of zero lines, so the excess is neither created nor enlarged here.

## Acceptance

- Every recorded count for a `.cs` path that this feature creates is at or below 500 —
  met. The only such path is `QfcCollectionControllerNavigationDigitsTests.cs` at 226.
- Every recorded count for a pre-existing `.cs` path is at or below 500 **or** is not greater than
  its `[P0-T21]` baseline count — met. Six of the seven pre-existing `.cs` paths satisfy the first
  disjunct; `QfcCollectionController.cs` satisfies the second.
- The artifact names each path taking the second branch together with the decision D-P6 statement —
  met above.

`QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` at 498 is within the cap with
two lines of headroom; no further addition to that file is possible without an extraction.
