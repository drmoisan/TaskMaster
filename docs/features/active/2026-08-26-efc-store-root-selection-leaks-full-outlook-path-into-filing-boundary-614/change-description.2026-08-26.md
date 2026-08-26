# Change description - #614 EFC store-root selection leaks a full Outlook path into the filing boundary

Branch: `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
Date: 2026-08-26
Work mode: `full-bug`

All Outlook and filesystem identifiers in this document are fabricated placeholders
(`\\mailbox@example.com`, `\\other@example.org`, `C:\Users\testuser\OneDrive - Contoso`) per
the host-identifier leakage constraint tracked in open issue #602.

---

## (a) The confirmed defect set D1-D9, and which are addressed

All nine confirmed defects are addressed.

| ID | Defect | Where fixed |
| --- | --- | --- |
| D1 | `BreadcrumbBridgeRouter.SelectHierarchyPath` stored the verbatim pass-through value produced by `ToArchiveRelativePath` when the activated path was not at or under the archive root, placing a full Outlook path into `SelectedFolderPath`. | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` - `SelectHierarchyPath` now routes through `ArchiveStemContract.TryMakeArchiveRelative`. A path outside the bound root, and the root itself, are deterministic non-selections. |
| D2 | Store-root, cross-store, and at-or-above-root ancestor activations all reached that same pass-through. | Same guard, covered by the `SegmentActivate_*` tests in `BreadcrumbBridgeRouterIssue614Tests`. |
| D3 | `SelectRow` passed `row.FilingTarget` through unguarded, and `ToHierarchyPath` fabricated an out-of-root hierarchy path by prefixing the bound root onto it. | `SelectRow` rejects an out-of-root FULL Outlook filing target; `ToHierarchyPath` returns a failure signal for such a target instead of fabricating a path. |
| D4 | `EmailFilerConfig.ResolvePaths` concatenated `DestinationOlStem` onto `OlAncestor` without checking that the stem was archive-relative; `GetStem` and `IsDeleteRelevant` used unanchored `Replace` and `Contains`. | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` - both overloads call `RequireArchiveRelativeStem` BEFORE concatenation, and both comparisons are prefix-anchored, separator-terminated, and `OrdinalIgnoreCase`. |
| D5a | `ToFsFolderpath` validated the whole concatenated filesystem path, including the caller-supplied ancestor. | `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` - validation applies only to the segments the converter derives. |
| D5b | The hand-rolled `IllegalFolderCharacters` class banned `.`, `[` and `]`, which are legal in a Windows folder name. This is the direct cause of the reported field crash, because the mailbox domain in the leaked path contains a dot. | Replaced by per-segment Windows folder-name validation: `Path.GetInvalidFileNameChars()`, trailing dot, trailing space, and reserved device names. |
| D5c | `fsPath.Substring(3)` assumed a three-character drive prefix, mangling UNC ancestors and throwing `ArgumentOutOfRangeException` for ancestors shorter than three characters. | Removed. The filesystem path is composed from the validated ancestor plus the derived stem. |
| D5d | `olBranchPath.Replace(olAncestorPath, fsAncestorEquivalent)` substituted the ancestor wherever it recurred in the branch. | Replaced by the prefix-anchored, separator-terminated, `OrdinalIgnoreCase` `TryMakeArchiveRelative`. |
| D5e | The thrown `ArgumentException` embedded `fsPathExDividers`, leaking a mailbox address and a user-profile path into a user-visible message. | The message names the violated segment rule only. `paramName` remains `nameof(fsPath)`. |
| D5f | `BuildAlternativesDictionary`'s "Remove illegal characters" option computed `illegalFolderName.Replace(illegalFolderName, "")`, replacing the WHOLE name with the empty string. | Replaced by per-character removal. |
| D5g | `ResolveOlRoot` selected a root with `Contains`, so a branch merely containing a root name anywhere resolved to that root. | Selection is now separator-terminated prefix matching. |
| D6 | `AppOlObjects.ArchiveRootPath` returned an unverified, default-store-scoped `Path.Combine` result. A profile whose archive lives in another store, or which has no Archive folder, silently produced a path that no folder answers to. | `TaskMaster/AppGlobals/AppOlObjects.cs` plus the new pure `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` - the composed path is cross-checked once, at resolution time, against the folder that actually resolves, and the validated value is cached. |
| D7 | `AppFileSystemFolderPaths.LoadFolders` fell back for the `OneDrive` key to `AppData` and then to `SpecialFolders.First().Value`, an arbitrary entry, producing a filing root unrelated to OneDrive. | Both fallbacks removed in favour of explicit, redacted failure. Environment access is now an injectable delegate seam. |
| D8 | `EfcDataModel.MoveToFolderAsync(MAPIFolder, olAncestor, ...)` derived its stem with an unanchored `Replace` plus a single `Substring(1)`, producing a mangled stem for a folder outside the ancestor. | Extracted the pure `ToArchiveRelativeStem` helper, backed by `TryMakeArchiveRelative`, which fails explicitly instead. |
| D9 | `EfcFormController.ActionOkAsync` and `IsValidSelection` applied DIFFERENT validity rules, so a value could be accepted by one and rejected by the other, and neither rejected a full Outlook path. | New `QuickFiler/Controllers/EfcSelectionGuard.cs` provides one shared predicate; both call sites delegate to it. |

The common root is that four independent, unanchored ancestor-strip implementations existed. They
are all now backed by one authority, `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`.

## (b) The "remove `.` from IllegalFolderCharacters" hypothesis, and why it is rejected

The reported crash is an `ArgumentException` naming an illegal `.` character, so the obvious
hypothesis is that `.` should be removed from `IllegalFolderCharacters`. That change is rejected as
a fix.

The dot in the failing value comes from the mailbox domain inside a full Outlook store path that
should never have reached the converter at all. Deleting `.` from the character class removes the
exception without removing the leak: the converter would then map the store-root path into a
filesystem path and the item would be filed somewhere unintended, with no error. The change would
convert a loud crash into silent misfiling, which is strictly worse for a filing tool.

The character class IS corrected, but only as part of the D5b per-segment rewrite that sits BEHIND
the D4 stem guard. The guard rejects the non-relative stem first; the corrected character class
then merely stops rejecting legitimate folder names such as `Acme Corp. Ltd`.

## (c) The `FolderConverterTests.cs:329` assertion change

Old assertion (a single line, at `:329` before this change):

```
result["Remove illegal characters"]().GetAwaiter().GetResult().Should().BeEmpty();
```

It codified D5f. The production expression was `illegalFolderName.Replace(illegalFolderName, "")`,
which replaces the whole name with the empty string, so the "Remove illegal characters" dialog
option always produced an empty folder name. The test asserted exactly that empty result, which
made the defect look like specified behaviour and would have blocked any correct fix.

New assertion:

```
result["Remove illegal characters"]().GetAwaiter().GetResult().Should().Be("BadName");
```

For the input `"Bad?Name"` the corrected implementation removes only the illegal `?` and returns
`"BadName"`. This is a deliberate, documented spec correction, not a weakened test: the assertion
became MORE specific, an exact value rather than emptiness.

## (d) The `ask` parameter decision

`FolderConverter.ToFsFolderpath(this string, string, string, bool ask = true)` carried a `bool ask`
parameter that was never read anywhere in the method body. The parameter is REMOVED, not marked
`[Obsolete]`.

Call-site search result: a repo-wide search of every `ToFsFolderpath(` call site - 11 production
and test call sites across `EmailFilerConfig.cs`, `SortEmail.cs`, `FolderPredictor.cs`,
`FolderConverterTests.cs`, and `FolderConverter_Tests.cs` - found NO call site supplying the
argument, positionally or by name. Removal is therefore source-compatible for every in-repo caller,
and the whole-solution build exits 0, which proves no orphaned call site remains.

## (e) Interaction with open issue #499

Issue #499 concerns a stale `SelectedFolderPath` surviving a rebind. This change confines router
writes to the selection actions themselves and does not absorb, fix, or regress #499. Concretely:
`BindRowsAsync`'s selection-clearing semantics are untouched; on rejecting an out-of-root
activation or an out-of-root row selection the router leaves `SelectedFolderPath` exactly as it
was and never sets it to `null`, which is asserted directly by
`SegmentActivate_StoreRootAncestor_LeavesSelectionUnchangedAndDiagnoses` and
`SegmentActivate_CrossStoreAncestor_LeavesSelectionUnchangedAndDiagnoses`; and the filing-boundary
guard in `EmailFilerConfig.ResolvePaths` plus the OK guard in `EfcSelectionGuard` independently
protect the #499 stale-value scenario, because a stale full Outlook path is now rejected at the
filing surface regardless of how it reached the selection. #499 remains open and unregressed.

## (f) The drive-rooted `IsFullOutlookPath` decision

`ArchiveStemContract.IsFullOutlookPath` returns `true` for three shapes: a `\\`-rooted store path,
a value leading with a single `\` or `/`, and a drive-rooted value whose second character is `:`
(for example `C:\Users\testuser\OneDrive - Contoso`).

Including the drive-rooted shape is a deliberate decision. A drive-rooted value can never be a
valid archive-relative stem; no legitimate stem carries a volume separator in position 1; and
rejecting it costs nothing while providing defence in depth against any future producer that leaks
a filesystem path into the Outlook filing chain. The rationale is restated in the XML documentation
of `IsFullOutlookPath` itself.

## (g) The empty-archive-root binding-mode preservation decision

`BreadcrumbBridgeRouter` exposes a public `BindRowsAsync(rows, scores, cancellationToken)` overload
that binds with NO archive root. In that mode the presented values are already the filing targets,
and the pre-existing behaviour is to pass an activated hierarchy path through verbatim. That mode
is preserved unchanged, and a test documents it
(`SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode`).

The reason is scope: that overload serves consumers outside the EFC filing chain; the filing
boundary (D4) and the OK guard (D9) independently protect filing; and altering the no-root mode
would be an out-of-scope behaviour change to non-EFC consumers. Every `BreadcrumbBridgeRouterTests`
(#349) test binds through this overload and remains green, unedited.

## (h) The `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` assertion change

`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` contains
`Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic`. Before this change its
assertion block read:

```
selected.Should().Equal(string.Empty, @"\External\Clients");
router.SelectedFolderPath.Should().Be(@"\External\Clients");
host.Verify(value => value.PostMessageJson(It.IsAny<string>()), Times.Exactly(2));
```

The test binds `archiveRoot = @"\Archive"`, activates segment index 1 whose full path is
`@"\External\Clients"` - a path OUTSIDE that bound root - and then asserts the router stores it
verbatim. That assertion IS D1: it codifies the defect as expected behaviour. Its index-0
activation additionally records a `string.Empty` selection for an archive-root-exact activation,
which is in tension with D9's treatment of the archive root as a non-destination.

Corrected assertions:

```
selected.Should().BeEmpty();
router.SelectedFolderPath.Should().BeNull();
host.Verify(value => value.PostMessageJson(It.IsAny<string>()), Times.Never);
```

Post-fix, the root-exact activation is a D9 non-selection and the out-of-root activation is a D1
rejection. Neither changes `SelectedFolderPath`, which therefore stays `null` - its value from
bind, since the test performs no prior selection - and neither raises `SelectedFolderPathChanged`,
so the `selected` event list stays empty. Because both selection actions now return before the
outbound render post, the `PostMessageJson` count drops from 2 to 0; `Times.Exactly(2)` is made
wrong by the behaviour change and becomes `Times.Never`. That is the only Moq count this correction
changes.

Everything the test still legitimately covers is preserved: `MockBehavior.Strict` on both mocks,
the `ResolveLeafKeyAsync` `Times.Once` and `GetAncestorChainAsync` `Times.Once` verifications - bind
still root-prefixes the relative presented target and resolves the chain exactly once, unchanged by
the fix - both activation channels (the `host.Raise` MessageReceived delivery for index 0 and the
direct `ProcessInboundAsync` call for index 1), and the entire arrange section.

Rationale: #439 deliberately permitted selecting a verified hierarchy path outside the archive
root. #614 intentionally revokes that for the filing surface, and treats archive-root-exact
activation as a non-selection. This is a user-visible behaviour change the spec already describes.
It is a deliberate, documented spec correction - the D1/D9 analogue of the D5f correction in item
(c) above - not a weakened test.

No other test in that class was edited. Four adjacent tests were specifically verified green and
unedited: `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`, whose
rooted-but-at-or-under-root target still passes through verbatim, which is why the `SelectRow`
guard is scoped to out-of-root full paths only; `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection`,
whose `@"\"` root trims to length 0 and therefore runs in the preserved empty-root pass-through
mode; and the three `Issue609_*` tests, which bind `\\mailbox@example.com\Archive` and select
only relative filing targets or activate only at-or-under-root chain paths.
