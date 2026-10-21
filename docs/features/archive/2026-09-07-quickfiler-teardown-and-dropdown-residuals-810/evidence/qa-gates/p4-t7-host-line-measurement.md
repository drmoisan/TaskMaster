# [P4-T7] Post-CSharpier Line Measurement of the Two Viewer Files

Timestamp: 2026-09-08T10-04
Command: Read tool, one measurement per file (the reported final content line of each file), taken on the tree as it stands after the [P4-T6] format pass

HOST-LINES-AFTER-DESIGN-A: 504
OPEN-LINES-AFTER-DESIGN-A: 133

DESIGN-A-EXCEEDS-CEILING: YES

## Basis

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` ends at content line 504, its last two lines being the closing brace of the type and the closing brace of the namespace. `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` ends at content line 133 in the same shape.

504 is greater than 500, so `DESIGN-A-EXCEEDS-CEILING` is `YES` and [P4-T8] must take branch B.

## Reconciliation with the baseline

[P0-T13] measured `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` at 496 lines with 4 lines of headroom, and [P0-T13] named this file as the one whose measured post-edit count decides Design A against Design B. The [P4-T4] edit added 8 net lines: the fourth `CompleteAll` operation with its rationale comment, and the replacement of the single comment line at the old `:450` with the three-line corrected block whose third line reads exactly `// The two gates are independent`. 496 plus 8 is 504, which overruns the 4 lines of headroom by 4 lines.

`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` moved from 131 to 133 lines under [P4-T5], which rewrapped the latch-lifetime `<para>` block from four `///` lines to six.

The [P4-T6] format pass rewrote neither file, so these counts are CSharpier's own output and not a layout that a later format pass will change.

## Consequence

[P4-T8] takes branch B: `FinishClose` and `RestoreAfterOpenFailure` move verbatim out of `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` into `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`. That file already declares `IsCommitPending` and owns the other clear site `ShowPopup`, so the two members join the part that owns the latch. The relocation is a pure move between parts of the same `public sealed partial class BreadcrumbDropDownHost`; both files already carry `#nullable enable` and `using System;`, and no test resolves either method by file path.
