# P10-T9 — No `UtilitiesCS` file appears in the diff

Timestamp: 2026-08-28T01-53
Command: git diff --name-only cecd78130a489fcfdc2ddac7970f344256f4a75a -- UtilitiesCS/
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Result

The command produces **zero output lines**.

`UtilitiesCS/` is one of the nineteen directories in the P10-T2 scope-lock pathspec, and it
contributes no path to that 25-path list. This targeted check confirms the same conclusion by naming
only the directory it asserts about, as the plan's convention for a targeted absence check requires:
widening the pathspec cannot change the outcome for `UtilitiesCS/`.

## The three named files

| File | Tracked path on this branch | In diff |
|---|---|---|
| `Theme.cs` | `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` | **Absent** |
| `ThemeControlGroup.cs` | `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs` | **Absent** |
| `WpfUiDispatcher.cs` | `UtilitiesCS/Threading/WpfUiDispatcher.cs` | **Absent** |

A targeted diff naming those three paths explicitly also produces zero output lines. All three are
tracked and present, so these are genuine no-change observations.

These three are named specifically because the #489 D2 theme-marshalling fix touches the theme and
dispatch surface: `HtmlDarkConverter` in the 484-owned `QfcItemController.FocusAndTheme.cs` now
guards its WebView2 write with `_itemViewer.InvokeRequired` and marshals through
`_itemViewer.Invoke`, and the new test drives that path through the `IUiDispatcher` seam. The fix was
made entirely on the `QuickFiler` side of that boundary. `Theme.cs`, `ThemeControlGroup.cs` and
`WpfUiDispatcher.cs` are the `UtilitiesCS` types the fix consumes, and none of them was modified.

## `UtilitiesCS.Test/`

`UtilitiesCS.Test/` is likewise in the P10-T2 pathspec and likewise contributes no path to that list,
so no `UtilitiesCS` test file was touched either.

Output Summary: **No `UtilitiesCS` file appears in the diff.**
`git diff --name-only <BASELINE_SHA> -- UtilitiesCS/` produces zero output lines with `EXIT_CODE: 0`,
and a targeted diff over the three specifically named paths —
`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`,
`UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs` and
`UtilitiesCS/Threading/WpfUiDispatcher.cs` — likewise produces zero output lines. All three are
tracked and present on this branch, so the result is genuine. `UtilitiesCS.Test/` is also absent from
the P10-T2 scope-lock list.
