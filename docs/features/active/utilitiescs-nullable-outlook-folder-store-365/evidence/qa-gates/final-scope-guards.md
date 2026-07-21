# Final Scope Guards (P12-T8)

Timestamp: 2026-07-19T16-40

## Pre-existing >500-line files — not split
| File | Baseline lines | Final lines | Status |
| --- | --- | --- | --- |
| `FolderPredictor.cs` | 974 | 983 | single file, NOT split (grew +9 from annotation `!`-justification comments) |
| `FolderScorer.cs` | 663 | 664 | single file, NOT split (+1 pragma line) |
| `FolderWrapper .cs` | 531 | 532 | single file, NOT split (+1 pragma line); filename unchanged |

All three remain single files; none was split. The FolderPredictor growth is from added `// why`-style comments
justifying the `null!` partial-init and `!` uses; it stays one file (the pre-existing >500-line exception).

## Filename hazard
`UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` (literal trailing space before `.cs`) is **PRESENT and
unchanged** — not renamed.

## Near-limit file
`OutlookFolderNotificationSink.cs`: baseline 498 lines -> final **499 lines** (+1 pragma line). Still under the
500-line limit; not split and no flag required (annotation edits used only `?` on existing declarations, adding
no lines).
