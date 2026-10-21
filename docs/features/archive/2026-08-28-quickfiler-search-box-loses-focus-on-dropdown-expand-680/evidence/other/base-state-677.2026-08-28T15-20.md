# P1-T1 — Pre-#677 Base-State Verification (DR-3, Issue #680)

Timestamp: 2026-08-28T15-20

## Verdict

**PRE-677 SHAPE CONFIRMED**

Issue #677 (PR #684) has NOT merged into this branch's base. This plan's line citations and
fix-composition tasks remain valid against the state verified here. The DR-3 halt-and-reverify
rule is not triggered.

## Base-state searches (production-scoped)

The token `MayTakeFocus` legitimately occurs under `docs/` and `.claude/agent-memory/`, so both
searches are pathspec-scoped to production and test trees.

Command: `git grep -n "MayTakeFocus" -- "QuickFiler/" "QuickFiler.Test/"`
EXIT_CODE: 1 (no output — zero occurrences)

Command: `git grep -n "Deactivate" -- "QuickFiler/*.cs"`
EXIT_CODE: 1 (no output — zero occurrences)

## Re-verified line anchors

Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680`
Commit: `c2d683d51d907d5591e313a550099fc267c10da6`

| File | Anchor | Planning-time citation | Verified line |
|---|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `AutoClose = true,` (constructor) | 165-170 | 167 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `internal void ShowPopup(Point location)` | 269 | 269 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `CompleteClose` | 368-382 | 368 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `OnDropDownClosed` | 397-408 | 397 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `FinishClose` | 410-420 | 410 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `RestoreAfterOpenFailure` | (referenced by P3-T3) | 422 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | already-open `takeFocus` branch | 60-70 | `if (takeFocus)` at 65, `_openLifetime.Schedule(_focusPending);` at 67 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `OpenCoreAsync` | 215-256 | 215 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `ShowCurrentSurface` | 258-278 | 258 (call site at 243, `_host.ShowPopup(...)` at 275) |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | `TextBoxSearch_KeyDown` | 184-193 | 184 (`Keys.Down` at 186, `SetFolderDroppedDown(true)` at 188, `FocusFolderDropDown()` at 189) |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | `WireIntentEvents` | 66-95 | 66 (`SearchKeyDown +=` at 91) |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | intent-unwire `SearchKeyDown -=` | 478 | 478 |

Every anchor is at its planning-time citation. No citation drift.

## DR-5 file-size baselines re-measured

| File | DR-5 planning-time | Measured now |
|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | 463 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 459 | 459 |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 484 | 484 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` (must NOT be edited) | 499 | 499 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | 234 | 234 |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 228 | 228 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | 105 | 105 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | (not in DR-5) | 75 |
| `QuickFiler/Viewers/IItemViewer.cs` | (not in DR-5) | 172 |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | (not in DR-5) | 81 |

Acceptance: satisfied — both grep results recorded with their exit codes, the verdict line is
present, and all line anchors are re-verified.
