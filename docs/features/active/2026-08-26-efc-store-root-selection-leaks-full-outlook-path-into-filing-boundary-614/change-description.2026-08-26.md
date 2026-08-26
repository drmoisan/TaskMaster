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

---

## Remediation cycle 1 (2026-08-26T21-00) - review findings CR-1 and CR-2

The feature review recorded CR-1 and CR-2 as Major but non-blocking. The orchestrator overrode that
disposition and promoted both to blocking, because both are regressions this change itself
introduced on the filing chain and their effect is to make a correct destination unreachable. This
cycle fixes exactly those two findings. CR-3, CR-4, all Minor findings, the pre-existing repo-wide
coverage shortfall, AC26 manual validation, and `spec.md` edits are explicitly out of scope.

Three files were modified: `QuickFiler/Controllers/EfcSelectionGuard.cs`,
`QuickFiler/Controllers/EfcFormController.cs`, and
`QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`. No other file was touched; in particular
`BreadcrumbBridgeRouter.cs` and `BreadcrumbBridgeRouterIssue439Tests.cs` are unmodified.

### CR-1 - the filing path applied a folder-creation rule

`IsValidFilingSelection` carried a minimum-length conjunct requiring three characters. That rule came
from `IsValidSelection`, which before #614 gated folder *creation* only; consolidating the two guards
onto the stricter of them silently narrowed the filing path so that filing to an archive folder named
`HR`, `IT`, `PR`, `QA` or `Q1` failed with "Please select a valid folder."

Resolution: the predicate is **split in two** rather than shared.

- `IsValidFilingSelection(string? selection, string? archiveRoot)` carries NO minimum-length rule.
- A new `IsValidCreationSelection(string? selection)` keeps the full pre-existing rule set -
  null/whitespace, banner prefix, the three-character minimum via a named `MinimumCreationLength`
  constant, and full-path rejection - and is what the `IsValidSelection` property now delegates to.

Recorded consequence: the creation path retains its full-path rejection, because a rooted value is
never a valid creation stem for `CreateFolderAsync`, which concatenates the selection beneath the
archive root. The CR-2 router-agreement requirement is therefore scoped to the filing/OK path only.

Fail-before evidence: `evidence/regression-testing/cr1-expect-fail.2026-08-26T21-46.md`
(exit 1, exactly `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted` and
`IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted` failing on assertion, everything
else green). Pass-after: `evidence/regression-testing/cr1-pass-after.2026-08-26T21-50.md`.

### CR-2 - the router and the filing guard disagreed about rooted targets

`BreadcrumbBridgeRouter.SelectRow` was scope-pinned during plan delta E1 so that a rooted filing
target at or under the bound archive root passes through verbatim - the behaviour
`Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` requires. The filing
guard rejected every value for which `ArchiveStemContract.IsFullOutlookPath` is true, which is true
of any single-separator-leading value including such a target. That class was selectable in the
breadcrumb surface and unfilable at the OK button.

Resolution: `IsValidFilingSelection` is restructured to apply the **same `TryMakeArchiveRelative`
scope-pinning the router already applies** (the delivery plan P3-T2 pattern). After the
null/whitespace guard it rejects the banner, accepts any non-rooted value outright, and finishes by
requiring a non-blank archive root and a successful `ArchiveStemContract.TryMakeArchiveRelative`
resolution. A rooted value is now rejected only when it genuinely fails to resolve against the
archive root.

**D1, D4 and D9 are preserved, not weakened.** The resolution test is prefix-anchored,
separator-terminated, and ordinal case-insensitive, so a store-root value, a cross-store value, an
above-root value, a drive-rooted value, and a sibling that merely extends the root name all still
fail it. Each is pinned by a named passing guard-rail test:
`IsValidFilingSelection_StoreRootedSelection_IsRejected`,
`IsValidFilingSelection_CrossStoreRootedTarget_IsRejected`,
`IsValidFilingSelection_RootedTargetAboveArchiveRoot_IsRejected`,
`IsValidFilingSelection_DriveRootedSelection_IsRejected`, and
`IsValidFilingSelection_SeparatorBoundaryNearMiss_IsRejected`.

Fail-before evidence: `evidence/regression-testing/cr2-expect-fail.2026-08-26T21-56.md`
(exit 1, exactly `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted` and
`IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted` failing on assertion). Pass-after plus
router agreement across all three `BreadcrumbBridgeRouter*` test classes:
`evidence/regression-testing/cr2-pass-after.2026-08-26T22-02.md`.

### Design decision - how the filing guard obtains the archive root

The archive root is passed to the predicate as a second parameter, resolved in `ActionOkAsync`
through a new throw-tolerant helper on the guard,
`EfcSelectionGuard.ResolveArchiveRootOrEmpty`, which takes two delegate seams: a `Func<string>`
accessor for the root and an `Action<string>` diagnostic sink. Delegates rather than an interface
(DI-seam preference 2), because the two call paths are a single property read and a single log call.

`_globals.Ol.ArchiveRootPath` throws after the #614 D6 fix when the archive root is unresolvable or
cross-store. On the OK-button path an unhandled throw would tear the form down, so the helper
catches that one documented failure, invokes the diagnostic sink with the fixed redaction-safe
`RootUnavailableDiagnostic` message, and returns `string.Empty`. Every other exception propagates -
the catch is narrow - and the underlying cause is not lost, because `AppOlObjects` logs it through
its own sink before throwing.

**Degrade behaviour:** an empty root makes the guard reject every rooted selection while relative
stems continue to file normally, which is the conservative direction. This is pinned by
`IsValidFilingSelection_RootedTargetWithUnavailableRoot_IsRejected`.

Alternatives rejected: an inline try/catch in `ActionOkAsync` (adds about 12 permanently uncoverable
lines to a file with 5 lines of headroom, and puts the catch branch beyond unit-test reach); and
resolving the root inside the predicate (couples a pure predicate to exception handling and to the
globals object graph). Placing the resolver on the guard keeps it 100% unit-coverable including its
catch branch, and holds the new logic in `EfcFormController.cs` to 7 lines.

### Root-exact consequence (recorded)

With the CR-2 fix, a rooted value exactly equal to the archive root passes the filing guard, because
`TryMakeArchiveRelative` returns true for the exact root and `SelectRow` - the agreement target -
admits at-or-under-root rooted targets verbatim. This is pinned by
`IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted`. The `SelectHierarchyPath` root-exact
non-selection is a different surface and is untouched by this cycle.

### Spec AC16 reading (recorded; no spec edit this cycle)

The AC16 phrase "reject a full Outlook path" now reads as "reject a full Outlook path that is not
resolvable against the archive root". The narrowing is mandated by the CR-2 required outcome under
the orchestrator disposition override, and it preserves the D1/D4/D9 protection because store-root,
cross-store and above-root values all still fail the resolution test. `spec.md` is not edited: spec
edits are out of scope for this cycle, and this paragraph is the recorded reading.

### Verification

| Gate | Result |
| --- | --- |
| `dotnet tool run csharpier check .` | exit 0, `Checked 1530 files` |
| analyzer `/t:Rebuild` | exit 0, 0 errors, 5 pre-existing System.Reactive advisories |
| nullable `/t:Rebuild` (no `/p:Nullable=enable`) | exit 0, 0 errors, 0 CS86xx |
| full suite with coverage | exit 0, 6587 total / 6587 passed / 0 failed (baseline 6569/6569/0; delta +18 new tests) |
| filtered line coverage | 84.8790% against an 84.8712% baseline - no regression |
| filtered branch coverage | 78.8523% against a 78.8454% baseline - no regression |
| `EfcSelectionGuard.cs` coverage | 100% line, 100% branch |
| file sizes | `EfcFormController.cs` 1079 of 1084; `EfcSelectionGuard.cs` 147 of 500; `EfcSelectionGuardTests.cs` 316 of 500 |
