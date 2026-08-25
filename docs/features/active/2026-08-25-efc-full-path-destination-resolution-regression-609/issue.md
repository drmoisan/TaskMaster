# efc-full-path-destination-resolution-regression (Issue #609)

- Date captured: 2026-08-25
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-full-path-destination-resolution-regression/ (Issue #609)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #609
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/609
- Last Updated: 2026-08-25
- Work Mode: full-bug

## Summary

Efc destination selection must keep two distinct path representations: an archive-relative filing target and a full Outlook hierarchy path. A full hierarchy path is required only for hierarchy-provider lookup; if it reaches the filing boundary, `EmailFilerConfig` prefixes the archive root a second time and destination resolution fails.

## Environment

- OS/version: Windows with Outlook/MAPI folder-path semantics.
- Python version: Not applicable; the affected implementation and tests are C#.
- Command/flags used: Headless MSTest coverage for `QuickFiler.Test` and `UtilitiesCS.Test`.
- Data source or fixture: Archive root `\\mailbox@example.com\Archive` with the archive-relative target `Clients\North`.

## Steps to Reproduce

1. Bind a search or suggestion row whose presented filing target is `Clients\North` while the archive root is `\\mailbox@example.com\Archive`.
2. Resolve the row through `IFolderHierarchyProvider.ResolveLeafKeyAsync`, then select the row directly or through an ancestor segment or immediate child.
3. Pass the selected destination to `EmailFilerConfig` with the same archive root and inspect the constructed Outlook destination.

## Expected Behavior

The hierarchy provider receives `\\mailbox@example.com\Archive\Clients\North`, while every value passed to `EfcDataModel` and `EmailFilerConfig.DestinationOlStem` remains `Clients\North`. `EmailFilerConfig` then constructs exactly `\\mailbox@example.com\Archive\Clients\North` and its corresponding save path.

## Actual Behavior

The filing constructor unconditionally combines `OlAncestor` and `DestinationOlStem`. A full hierarchy value at that boundary therefore produces a duplicated archive root and causes `FolderPredictor.GetFolder` to return no destination. Existing regression tests do not exercise this representation boundary with a mailbox identifier containing `@`; no runtime log was supplied with the issue.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: No runtime log or screenshot is available. The implementation trace and the planned deterministic unit tests are the available evidence.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`BreadcrumbBridgeRouter` already derives a full path for `ResolveLeafKeyAsync` and converts hierarchy selections back to archive-relative text. `EfcDataModel` forwards its selected value unchanged as `DestinationOlStem`, and `EmailFilerConfig.ResolvePaths` is the single intended authority that prefixes `OlAncestor`. The boundary is implicit and lacks coverage for mailbox roots containing `@`.

## Proposed Fix / Validation Ideas

- [ ] Add deterministic router coverage for direct row selection, ancestor activation, and child activation using `\\mailbox@example.com\Archive`.
- [ ] Add `EmailFilerConfig.ResolvePaths` coverage that verifies a single archive-root prefix and the resulting filesystem destination.
- [ ] Retest the existing banner, trash, and archive-root-boundary scenarios without changing their behavior.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
