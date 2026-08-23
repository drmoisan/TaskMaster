# quickfiler-explorer-controller-latent-defects (Issue #449)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-explorer-controller-latent-defects/ (Issue #449)
- Found during: research for issue #435 (child F6 of epic #136, QuickFiler per-file coverage)

- Issue: #449
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/449
- Last Updated: 2026-08-08
- Work Mode: full-bug

## Summary

Two independent latent defects in `QuickFiler/Controllers/QfcExplorerController.cs`, plus a block of
dead duplicated code. All three were found by reading during F6 coverage research and none is fixed by
F6, whose acceptance criteria forbid behavior changes.

## Defect 1 — `ExplConvView_Cleanup()` throws `NotImplementedException`

`ExplConvView_Cleanup()` is declared on the public interface `IQfcExplorerController`
(`QuickFiler/Interfaces/IQfcExplorerController.cs:12`) but its implementation throws
`NotImplementedException`. Any caller reaching it fails at runtime rather than degrading.

The intended semantics appear to be recoverable from the legacy implementation at
`QuickFiler/Legacy/QuickFileController.cs:851-869` (not compiled), which should be read before
implementing rather than reinventing the behavior.

Mitigating factor: the member currently has no production callers, so the throw is not reachable in
normal operation today. That makes it latent rather than active — but it is a live trap for the next
caller.

## Defect 2 — `OpenQFItem` re-resolves the active explorer

`OpenQFItem` calls `_globals.Ol.App.ActiveExplorer()` a second time at
`QuickFiler/Controllers/QfcExplorerController.cs:140` instead of reusing the `_activeExplorer` field
captured in the constructor at line 35.

This is both a redundant COM round-trip and a correctness hazard: if the active explorer changed
between construction and the call, the method operates on a different `Explorer` than the rest of the
type, so the object's view of "the" explorer becomes internally inconsistent.

## Defect 3 — dead duplicated code block

`QuickFiler/Controllers/QfcExplorerController.cs:183-321` (the `#region Email Sorting To Rewrite`)
contains six private/internal statics — `SanitizeArrayLineTSV`, `StripTabsCrLf`,
`WriteCSV_StartNewFileIfDoesNotExist`, `SanitizeArray`, `SaveMessageAsMSG`,
`GetCurrentExplorerFolder`. A repo-wide search confirms they are referenced only from inside that same
region (lines 193, 241, 264). Every external caller binds to separate copies in
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`,
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`, and
`ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`, which carry their own tests in
`UtilitiesCS.Test`.

Two latent defects were additionally observed inside this dead block:
- `WriteCSV_StartNewFileIfDoesNotExist` passes transposed arguments to `Path.Combine`.
- `SanitizeArray` writes into a `null` `ref string[]`, which would throw if ever reached.

Because the block is unreachable, neither defect can fire today. Deleting the region is
behavior-neutral for the QuickFiler assembly and removes roughly 139 lines of uncoverable
filesystem-I/O code from the coverage denominator.

## Why This Is Filed Separately

All three items were found during read-only research for the F6 coverage child (issue #435). F6's
acceptance criteria require no behavior change to observable QuickFiler flows, and fixing a
`NotImplementedException` or changing which `Explorer` instance is used are both behavior changes.
Recording them only as prose inside a feature folder would lose them at merge.

## Impact

- Defect 1: runtime failure for the next caller of a public interface member.
- Defect 2: redundant COM call plus a potential inconsistency window.
- Defect 3: no runtime impact; carrying cost is coverage-denominator pollution and duplicated code
  that can drift from the maintained copies in `UtilitiesCS`.

## Acceptance Criteria (early draft)

- [ ] `ExplConvView_Cleanup()` either implements the legacy semantics from
      `QuickFiler/Legacy/QuickFileController.cs:851-869` or is removed from `IQfcExplorerController`
      with all implementers updated; the decision is recorded with rationale.
- [ ] `OpenQFItem` reuses the constructor-captured `_activeExplorer` field, or the reason a fresh
      `ActiveExplorer()` call is required is documented in code.
- [ ] The dead `#region Email Sorting To Rewrite` block is deleted, with a test run confirming no
      behavior change.
- [ ] Deterministic regression tests cover each changed path; no temporary files, no live forms.
- [ ] Full C# toolchain passes: csharpier, analyzer build, nullable build, coverage-enabled vstest.

## Coordination Note

The dead-code deletion overlaps the file F6 is actively covering. Sequence this issue AFTER F6 merges,
or coordinate through the epic, to avoid a conflict on `QfcExplorerController.cs`.

## Next Step

- [ ] Promote to GitHub issue (bug template)
