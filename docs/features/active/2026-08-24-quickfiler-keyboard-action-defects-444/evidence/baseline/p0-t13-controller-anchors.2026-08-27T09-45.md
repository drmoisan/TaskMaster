# [P0-T13] `QfcCollectionController.cs` anchors re-derived by member name

Timestamp: 2026-08-27T09-45
Command: `Select-String -SimpleMatch -CaseSensitive` for each anchor name against `QuickFiler\Controllers\QfcCollectionController.cs`, plus a targeted declaration-line lookup for each member
EXIT_CODE: 0

File: `QuickFiler/Controllers/QfcCollectionController.cs`
Total lines at branch head: **2437**

Every anchor below was re-derived against the actual branch head. No line number was transcribed from
the plan or from `spec.md`; the plan carries zero line-number citations into this file (gated
mechanically by `[P5-T8]`).

| # | Anchor | Verdict | Declaration line | Declaration text as observed |
| --- | --- | --- | --- | --- |
| 1 | `RegisterNavigation` | PRESENT | 1167 | `public void RegisterNavigation()` |
| 2 | `UnregisterNavigation` | PRESENT | 1180 | `public void UnregisterNavigation()` |
| 3 | `RegisterNavigationAsyncAction` | PRESENT | 1195 | `internal void RegisterNavigationAsyncAction(int itemIndex, int digits)` |
| 4 | `GenerateStringKbdAction` | PRESENT | 1200 | `internal KaStringAsync GenerateStringKbdAction(int i, int digits)` |
| 5 | `RegisterAsyncKeyActions` | PRESENT | 1125 | `internal void RegisterAsyncKeyActions()` |
| 6 | `Digits` | PRESENT | 119 | `internal int Digits` |
| 7 | `_digits` | PRESENT | 118 | `private int _digits = 1;` |
| 8 | `_digitRefreshNeeded` | PRESENT | 117 | `private bool _digitRefreshNeeded = false;` |
| 9 | `SetVisualDigits` | PRESENT | 135 | `private void SetVisualDigits(int digits)` |
| 10 | class-level `ExcludeFromCodeCoverage` attribute | PRESENT | 21 | `[ExcludeFromCodeCoverage]` |

All ten anchors record `PRESENT` with an observed line number.

## Observations relevant to later phases

- `Digits` is `internal`, not `public`. The `public int Digits` spelling does not occur; the spec and
  research both refer to it as "the `Digits` property" without asserting an accessibility, so this is
  a clarification rather than a discrepancy.
- `SetVisualDigits` is `private`, not `public`.
- The `Digits` getter still carries `[MethodImpl(MethodImplOptions.Synchronized)]` and is still
  side-effecting: it computes `digitNeed` from `_itemGroups?.Count >= 10 ? 2 : 1` and, when that
  differs from `_digits`, sets `_digitRefreshNeeded = true` and mutates `_digits`. Upstream #468 did
  not change this member, matching the research delta table.
- `[ExcludeFromCodeCoverage]` occurs exactly once in the file, at the class declaration (line 21),
  confirming decision D-P4: no coverage figure in this plan is attributed to this file.
- Occurrence counts observed (informational): `RegisterNavigation` 11, `UnregisterNavigation` 7,
  `RegisterNavigationAsyncAction` 2, `GenerateStringKbdAction` 2, `RegisterAsyncKeyActions` 2,
  `Digits` 13, `_digits` 4, `_digitRefreshNeeded` 7, `SetVisualDigits` 6,
  `ExcludeFromCodeCoverage` 1.

Output Summary: all ten anchors PRESENT with observed line numbers against a 2437-line branch-head
file; acceptance condition met.
