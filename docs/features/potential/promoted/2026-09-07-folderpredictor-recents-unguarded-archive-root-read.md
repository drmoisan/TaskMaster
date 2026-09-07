# folderpredictor-recents-unguarded-archive-root-read (Issue #801)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folderpredictor-recents-unguarded-archive-root-read/ (Issue #801)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #801
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/801
- Last Updated: 2026-09-07
## Summary

FolderPredictor.AddRecents and FolderPredictor.AddRecentRows read the archive root through an
unguarded property whose getter throws InvalidOperationException when the root cannot be resolved.
With zero suggestions and a non-empty recents list, these two members become the first reader on a
code path that previously never touched that property, so an unresolvable archive root now surfaces
as a throw where it previously did not.

## Environment

- OS/version: Windows, Outlook VSTO host
- Python version: not applicable; this is C# in UtilitiesCS
- Command/flags used: not applicable; the defect is reached through normal QuickFiler folder-row rendering
- Data source or fixture: an Outlook profile whose archive root cannot be resolved

## Steps to Reproduce

1. Run with an Outlook profile in which the archive root is unresolvable, so that the archive-root
   property getter throws rather than returning a path.
2. Reach a QuickFiler folder-row render in which the suggestions list is empty and the recents list
   is non-empty.
3. Observe that the recents projection reads the archive root and the exception propagates.

## Expected Behavior

An unresolvable archive root degrades the recents projection to the identity projection, leaving the
recent entries rendered as stored, in the same way the hierarchy provider degrades when its optional
root accessor is absent or throws.

## Actual Behavior

The archive-root read is unconditional, so the InvalidOperationException propagates out of the
recents projection.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: the getter is documented with an explicit exception tag in TaskMaster/AppGlobals/AppOlObjects.cs lines 260-270.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Raised as non-blocking finding CR-1 in the issue #799 code review, artifact
docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/code-review.2026-09-07T20-30.md.

Sites: UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs at AddRecents and at AddRecentRows.
The reachability argument is that FolderArray guards AddSuggestions on a non-empty suggestions count
separately from AddRecents on a non-empty recents count, and AddSuggestions only reaches the property
per element, so the zero-suggestions and non-empty-recents combination is the newly exposed path.

This is precisely the throw-site class that issue #799's specification introduced the provider's lazy
delegate accessor to avoid; the reasoning was applied to the hierarchy provider and not carried across
to the two recents projections. It was assessed non-blocking during that review because the QuickFiler
item-view path already reads the same property unconditionally a few lines later in
QuickFiler/Controllers/QfcItemController.FolderHandling.cs, and an unresolvable archive root is already
a degraded application state.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: read the root through a TryReadArchiveRoot-style helper that catches
      InvalidOperationException and returns null, then have both recents projections fall back to the
      identity projection on null. Add a test for the zero-suggestions and non-empty-recents
      combination against a throwing root, which no current test exercises.
- [ ] Integration scenario to retest: QuickFiler folder-row rendering against a profile with an
      unresolvable archive root.
- [ ] Manual verification notes: confirm recent entries still render, unprojected, rather than the
      view failing.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
