# efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary (Issue #614)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary/ (Issue #614)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

> Redaction note: all mailbox addresses, user-profile paths and organization names below are
> substituted placeholders, per the host-identifier leakage constraint tracked in issue #602.

- Issue: #614
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/614
- Last Updated: 2026-08-26
- Work Mode: full-bug

## Summary

Filing an email from the Email Filer Controller (EFC) throws `ArgumentException` from
`FolderConverter.ToFsFolderpath` because a full Outlook hierarchy path (the mailbox store root,
e.g. `\\mailbox@example.com`) reaches `EmailFilerConfig.DestinationOlStem`, which by contract must
be an archive-relative stem. The reported "illegal character" (the `.` in the mailbox address) is a
downstream symptom of a path-representation contract that is enforced at no boundary; the true
defect set spans breadcrumb hierarchy selection, archive-root resolution, special-folder
resolution, and the Outlook-path-to-filesystem-path converter itself.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; Outlook desktop with a Microsoft 365 mailbox whose store
  display name is the account email address.
- Python version: Not applicable; the affected implementation and tests are C# (.NET Framework
  4.8.1 VSTO add-in).
- Command/flags used: Not applicable; reached interactively through the EFC folder-list breadcrumb
  surface and the OK button.
- Data source or fixture: Archive root `\\mailbox@example.com\Archive`; OneDrive commercial root
  `C:\Users\<user>\OneDrive - <Org>`.

## Steps to Reproduce

1. Open the QuickFiler EFC surface against a mailbox whose store root path is
   `\\mailbox@example.com` and whose archive root is `\\mailbox@example.com\Archive`.
2. Bind folder rows so that a breadcrumb row renders its ancestor chain
   (`IFolderHierarchyProvider.GetAncestorChainAsync`, which is requested with
   `FolderTreeRequest.AllStores` and therefore walks all the way up to the store root).
3. Activate an ancestor segment at or above the archive root. In the observed case this was the
   store-root segment itself.
4. Press OK to execute the move.

## Expected Behavior

Either the segment at or above the archive root is not selectable as a filing destination, or the
selection is clamped or rejected before it reaches the filing boundary. Every value that flows into
`EfcDataModel.MoveToFolderAsync` and `EmailFilerConfig.DestinationOlStem` remains an
archive-relative stem (for example `Clients\North`), and `FolderConverter.ToFsFolderpath` validates
only the segments it derives, never the caller-supplied filesystem ancestor root, which
legitimately contains `.`, spaces and `-`.

## Actual Behavior

`ArgumentException` is thrown and the move fails:

```
System.ArgumentException: fsPathExDividers has a value of
Users<user>OneDrive - <Org>mailbox@example.com which contains illegal characters .
Parameter name: fsPath
   at UtilitiesCS.FolderConverter.ToFsFolderpath(String olBranchPath, String olAncestorPath, String fsAncestorEquivalent, Boolean ask) in UtilitiesCS\OutlookObjects\Folder\FolderConverter.cs:line 169
   at UtilitiesCS.EmailIntelligence.EmailParsingSorting.EmailFilerConfig.ResolvePaths(Folder currentFolder) in UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFilerConfig.cs:line 188
   at UtilitiesCS.EmailIntelligence.EmailParsingSorting.EmailFiler.ResolvePaths(Folder currentFolder) in UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs:line 378
   at UtilitiesCS.EmailIntelligence.EmailParsingSorting.EmailFiler.SortAsync(...) in UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs:line 133
   at QuickFiler.Controllers.EfcDataModel.MoveToFolderAsync(...) in QuickFiler\Controllers\EfcDataModel.cs:line 293
   at QuickFiler.EfcHomeController.ExecuteMovesCoreAsync() in QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs:line 75
   at QuickFiler.EfcHomeController.ExecuteMovesAsync() in QuickFiler\Controllers\EfcHomeController.ExecuteMoves.cs:line 40
   at QuickFiler.Controllers.EfcFormController.ActionOkAsync() in QuickFiler\Controllers\EfcFormController.cs:line 716
   at QuickFiler.Controllers.EfcFormController.ButtonOK_Click(...) in QuickFiler\Controllers\EfcFormController.cs:line 441
```

Algebraic reconstruction of the reported message. This is the evidence that the stem, not the
character class, is the defect. `ToFsFolderpath` strips the first three characters (`C:\`) and then
removes every `\`, so the reported value implies:

```
fsPath == "C:\Users\<user>\OneDrive - <Org>" + "\" + "\\mailbox@example.com"
```

Working backwards through `EmailFilerConfig.ResolvePaths`:

```
DestinationOlPath = OlAncestor + "\" + DestinationOlStem
fsPath            = DestinationOlPath.Replace(OlAncestor, FsAncestorEquivalent)
```

With `OlAncestor = \\mailbox@example.com\Archive` and
`FsAncestorEquivalent = C:\Users\<user>\OneDrive - <Org>`, the only stem that reproduces the
reported string exactly is `DestinationOlStem == "\\mailbox@example.com"`, that is, the mailbox
store root carried through as a full Outlook path. No stem containing a real destination folder
name reproduces the message, because the message ends immediately after the mailbox address.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: the exception and stack shown above, captured at the throw site.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Filing fails outright for the affected selection path, and adjacent defects in the same chain can
silently produce a wrong destination rather than an exception.

## Suspected Cause / Notes

The Copilot hypothesis, "remove `.` from `IllegalFolderCharacters`", addresses the symptom only.
Removing `.` would let the call succeed while still filing to a destination derived from the
mailbox store root. The candidate defect set below must be confirmed or refuted by investigation;
it is a starting hypothesis set, not a verified list.

1. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` `ToArchiveRelativePath(fullPath)`: when
   `fullPath` is not at or under `_archiveRootPath`, the method returns the full path verbatim.
   That value is assigned to `SelectedFolderPath`, surfaces as `EfcFormController.SelectedFolder`,
   and is handed to `EfcDataModel.MoveToFolderAsync` as an archive-relative stem. There is no
   rejection, no clamp and no diagnostic.
2. `BreadcrumbBridgeRouter.ActivateSegment` and `ActivateChild` call
   `SelectHierarchyPath(row, segment.FullPath)` for any segment in the chain. The chain comes from
   `OutlookFolderHierarchyProvider.GetAncestorChainAsync`, which acquires
   `FolderTreeRequest.AllStores(allowStaleSnapshot: true)` and walks to the store root. Segments at
   or above the archive root, and segments belonging to a different store entirely, are therefore
   selectable as filing destinations.
3. `BreadcrumbBridgeRouter.ToHierarchyPath` is the mirror-image hazard: a presented target that is
   an absolute path outside the archive root is prefixed with the archive root, producing a
   nonexistent hierarchy path.
4. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` `ResolvePaths` and
   `ResolvePaths(Folder)` concatenate `OlAncestor + "\" + DestinationOlStem` with no validation that
   the stem is relative. Issue #609 (closed, PR #611) established this boundary but addressed only
   the "stem already carries the archive root prefix" case, not "stem is an unrelated absolute
   path". The stem contract is documented in prose and enforced nowhere.
5. `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` `ToFsFolderpath` carries several
   independent defects:
   - It validates the entire concatenated path, including the caller-supplied
     `fsAncestorEquivalent`, instead of only the segments it derives. Any legitimate OneDrive
     commercial root containing `.`, `[` or `]` fails regardless of the stem.
   - `IllegalFolderCharacters` (`[\/:*?"<>|].`) is wrong in both directions: `.`, `[` and `]` are
     legal in Windows names, while the control characters in `Path.GetInvalidFileNameChars()` are
     absent. The real Windows rules it should encode are per-segment: invalid characters, trailing
     dot or space, and reserved device names.
   - `fsPath.Substring(3)` assumes a rooted `X:\` path and will throw or silently mangle a UNC or
     relative `fsAncestorEquivalent`.
   - `olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent)` is an unanchored, case-sensitive
     replace-all. It should be a prefix-anchored `OrdinalIgnoreCase` strip; today a repeated
     ancestor substring is replaced at every occurrence.
   - The exception message embeds the full user-profile path and mailbox address, the runtime
     counterpart of issue #602.
   - `BuildAlternativesDictionary`'s "Remove illegal characters" option evaluates
     `illegalFolderName.Replace(illegalFolderName, "")`, which always returns the empty string.
   - `ResolveOlRoot` selects the root with `Contains` rather than a prefix test, so it can return
     the wrong ancestor when one root path is a substring of the other.
6. `TaskMaster/AppGlobals/AppOlObjects.cs` `ArchiveRootPath` is
   `Path.Combine(Root.FolderPath, "Archive")` over `App.Session.DefaultStore`. It is neither
   verified to exist nor scoped to the store that owns the current folder, so with multiple stores
   the ancestor never matches and every downstream `Replace` or `StartsWith` silently no-ops.
7. `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` `LoadFolders` falls back for the `OneDrive`
   key to `AppData` and finally to `SpecialFolders.First().Value`, an arbitrary dictionary entry. A
   wrong `FsAncestorEquivalent` is accepted silently. `MatchBestSpecialFolder` also matches with
   `Contains` rather than a path-prefix test.
8. `QuickFiler/Controllers/EfcDataModel.cs` `MoveToFolderAsync(MAPIFolder, olAncestor, ...)` strips
   exactly one leading backslash (`Substring(1)`), so a store-root path beginning with `\\` retains
   a leading separator, and its `Replace(olAncestor, "")` is likewise unanchored.

## Proposed Fix / Validation Ideas

- [ ] Introduce and enforce an explicit path-representation contract that separates a full Outlook
      hierarchy path from an archive-relative stem, validated at the filing boundary rather than
      documented in prose.
- [ ] Clamp or reject breadcrumb selections at or above the archive root, and selections outside the
      archive's store, with a deterministic diagnostic instead of a silent pass-through.
- [ ] Replace the hand-rolled `IllegalFolderCharacters` check with per-segment Windows name
      validation, scoped to derived segments only and never to the caller-supplied filesystem root.
- [ ] Make `ToFsFolderpath` prefix-anchored and case-insensitive, remove the `Substring(3)`
      assumption, and redact host identifiers from its exception messages.
- [ ] Validate `ArchiveRootPath` and the `OneDrive` special folder at resolution time; fail
      explicitly rather than falling back to an arbitrary special folder.
- [ ] Unit coverage areas: `BreadcrumbBridgeRouter` (store-root segment activation, cross-store
      segment activation, ancestor-at-archive-root activation, child activation), `EmailFilerConfig`
      (absolute-stem rejection, single-prefix construction), `FolderConverter` (dotted and bracketed
      filesystem roots, UNC roots, repeated ancestor substring, per-segment illegal characters,
      trailing dot or space, reserved names), `AppFileSystemFolderPaths.MatchBestSpecialFolder`
      (prefix versus `Contains`), `EfcDataModel` (double-backslash store-root stripping).
- [ ] Integration scenario to retest: the issue #609 scenarios (direct row selection, ancestor
      activation, child activation, banner and trash pseudo-rows) must retain their current
      behavior.
- [ ] Manual verification notes: file to a normal archive subfolder, then attempt the store-root
      ancestor selection and confirm the new deterministic behavior.
- [ ] Coordinate with open issue #499 (`breadcrumb-router-stale-selectedfolderpath-after-rebind`),
      which touches the same `SelectedFolderPath` field; do not silently absorb or regress it.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
