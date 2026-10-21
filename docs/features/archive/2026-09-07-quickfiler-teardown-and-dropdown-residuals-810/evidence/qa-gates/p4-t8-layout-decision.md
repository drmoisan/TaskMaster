# [P4-T8] Measured Layout Decision

Timestamp: 2026-09-08T10-05

DESIGN-CHOSEN: B

BRANCH-TAKEN: branch B, selected by `DESIGN-A-EXCEEDS-CEILING: YES` recorded in [P4-T7]. `HOST-LINES-AFTER-DESIGN-A` measured 504, which is greater than 500, so the in-place Design A layout overruns the repository's 500-line ceiling and the relocation branch is the one this task must take. Had the measurement come back at 500 or below, `DESIGN-A-EXCEEDS-CEILING` would have read `NO`, no code action would have been taken, and `DESIGN-CHOSEN` would read `A`.

## What was moved

`FinishClose(BreadcrumbDropDownCloseReason reason)` and `RestoreAfterOpenFailure()` were moved out of `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` and into `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`. The destination part already declares `IsCommitPending` and owns the latch's other clear site `ShowPopup`, so both clear sites and the latch declaration now sit in one file.

The relocation is a pure move between parts of the same `public sealed partial class BreadcrumbDropDownHost`. Both files already carry `#nullable enable` and `using System;`, both are in namespace `QuickFiler.Viewers`, and every member the two methods reach — `CompleteAll`, `DropDown`, `OpenState`, `CloseNative`, `FocusAnchorIfPermitted`, `_cancelSelection`, `IsCommitPending` — is a member of the same class, so no using directive was added and no accessibility changed. `CompleteAll` itself remains in `BreadcrumbDropDownHost.cs`.

## `git grep` records for branch B

```
git grep -F "private void FinishClose(BreadcrumbDropDownCloseReason reason)" -- QuickFiler/Viewers/BreadcrumbDropDownHost.cs
```

exited 1 with no output: the member is no longer declared in the origin file.

```
git grep -c -F "private void FinishClose(BreadcrumbDropDownCloseReason reason)" -- QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:1
```

printed 1: the member is declared exactly once in the destination file.

## One departure from a verbatim move, disclosed

The moved text is byte-identical to the text removed, with a single exception. The AC5 rationale comment added by [P4-T4] ended with the clause `rethrows the first failing operation at :486-487`. That line citation pointed at `CompleteAll` as it was numbered in `BreadcrumbDropDownHost.cs` before the move. `CompleteAll` stays in that file and the move renumbers it, so carrying the citation across would have left a statement in `BreadcrumbDropDownHost.Open.cs` naming two line numbers that do not hold in either file. The citation was therefore dropped and the sentence now reads `because CompleteAll rethrows the first failing operation and a statement placed after the call would be skipped by that throw`, which is the same claim without the stale locator. No other character of either method changed, and no behaviour changed.

## Consequence for the ceiling

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` loses the 45 lines the two members and their separating blank line occupied. [P4-T9] reformats both files and [P4-T10] audits the resulting counts against the 500-line ceiling.
