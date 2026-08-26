# breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed (Issue #637)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed/ (Issue #637)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #637
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/637
- Last Updated: 2026-08-26
## Summary

Issue #614 established the invariant that `SelectedFolderPath` is an archive-relative stem, and
created `ArchiveStemContract` to express it. The invariant is enforced at the filing boundary, where
`EmailFilerConfig.ResolvePaths` calls `RequireArchiveRelativeStem`, but it is not enforced at the
producer. `BreadcrumbBridgeRouter.SelectRow` still commits a rooted filing target verbatim when that
target is at or under the bound archive root, so a rooted value can still become
`SelectedFolderPath`. That is defect D1 half-closed: the store-root and cross-store leaks are
stopped, but rootedness as such still escapes the producer.

Two things make this worth fixing rather than tolerating.

First, it left a live trap that has already fired once. During #614 remediation cycle 1 the OK-path
guard was widened to accept rooted under-root values so that it would agree with `SelectRow`. Because
nothing between the guard and the filing boundary normalizes the value, the accepted value reached
`RequireArchiveRelativeStem` and threw. `ButtonOK_Click` is `async void` and rethrows, and
`ExecuteMovesAsync` wraps its core in try/finally with no catch, so the `ArgumentException` became an
unhandled UI-thread exception after the form had already been hidden. The re-audit caught it and the
change was reverted. The underlying asymmetry that made the widening look reasonable is still
present.

Second, the D8 normalizer is only half-wired. `EfcDataModel.ToArchiveRelativeStem` exists and is
correct, but it is called only from the `MAPIFolder` overload of `MoveToFolderAsync`. The `string`
overload assigns `DestinationOlStem = folderpath` verbatim, so it performs no normalization at all.
Any rooted value arriving through that overload depends entirely on the boundary throw.

The fix is to normalize at the producer: in `SelectRow`, when `TryMakeArchiveRelative` succeeds with
a non-empty stem, commit the stem rather than the rooted input; when it succeeds with an empty stem
the value is the archive root itself, which `SelectHierarchyPath` already treats as a deterministic
non-selection and `SelectRow` should too. Once the producer cannot emit a rooted value, the OK guard
and the filing boundary agree by construction rather than by coincidence, and the composition test
added during remediation keeps them agreeing.

This also requires updating the existing test that asserts a rooted input survives selection, so that
it asserts the stem instead. That is a deliberate spec correction of the same kind #614 already
applied twice, and should be recorded as such rather than treated as a weakened test.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 VSTO add-in.
- Python version: Not applicable; this is C#.
- Command/flags used: Static tracing during the issue #614 remediation re-audit, plus the failing
  path reproduced by remediation cycle 1.
- Data source or fixture: Repository source on the issue #614 branch.

## Steps to Reproduce

1. Bind breadcrumb rows with an archive root, and present a suggestion row whose filing target is a
   rooted path at or under that root. `FolderPredictor.ProjectSuggestionPath` strips the archive
   prefix only when the suggestion is strictly under it, so a suggestion whose folder is the archive
   root is returned rooted and verbatim.
2. Select that row. `BreadcrumbBridgeRouter.SelectRow` commits the rooted value to
   `SelectedFolderPath`.
3. Observe that the value reaching the filing boundary is rooted, and is rejected there by
   `RequireArchiveRelativeStem` rather than having been normalized at the producer.

## Expected Behavior

`SelectedFolderPath` is always an archive-relative stem. The producer normalizes; the boundary guard
is a backstop that never fires in normal operation. A row whose filing target is the archive root
itself is a non-selection, consistently with `SelectHierarchyPath`.

## Actual Behavior

`SelectRow` commits a rooted value verbatim. The invariant is enforced only at the boundary, where
violating it is an exception rather than a corrected value.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not applicable; established by source tracing. See `BreadcrumbBridgeRouter.SelectRow`, the
  `string` overload of `EfcDataModel.MoveToFolderAsync`, and
  `ArchiveStemContract.RequireArchiveRelativeStem`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

No user-visible defect on the shipped code: a rooted selection is rejected at the OK guard with a
clear dialog. Severity is Medium because the asymmetry is a live trap for future changes, as
demonstrated by remediation cycle 1, and because the half-wired D8 normalizer means one filing
overload relies entirely on a throw.

## Suspected Cause / Notes

- `BreadcrumbBridgeRouter.SelectRow` commits the presented filing target verbatim for rooted
  at-or-under-root values.
- `EfcDataModel.ToArchiveRelativeStem` is called only from the `MAPIFolder` overload of
  `MoveToFolderAsync`; the `string` overload assigns `DestinationOlStem` verbatim.
- The existing issue #439 rooted-target test pins the current producer behavior and will need a
  recorded spec correction.
- Related: issue #614 (the parent fix), and its remediation cycle 1, which was reverted for
  introducing a crash on this path.

### Additional finding carried here from the #614 remediation

The `string` overload of `EfcDataModel.MoveToFolderAsync` reads `Globals.Ol.ArchiveRootPath` inside
the OK chain. After the #614 D6 change that property throws `InvalidOperationException` when the
archive root is unresolvable or cross-store. The chain has no handler: `ExecuteMovesAsync` uses
try/finally with no catch, and `ButtonOK_Click` is `async void` and rethrows, so an unresolvable
archive root is an unhandled UI-thread exception. This was assessed during the #614 remediation and
deliberately not folded into that revert, because a benign degrade requires deciding what aborting a
filing operation should look like to the user and needs its own tests. It belongs with this work
because it is the same defect class on the same chain.

## Proposed Fix / Validation Ideas

- [ ] Normalize in `SelectRow`: commit the stem when `TryMakeArchiveRelative` succeeds non-empty;
      treat an empty stem as a non-selection, matching `SelectHierarchyPath`.
- [ ] Wire `ToArchiveRelativeStem` into the `string` overload of `MoveToFolderAsync`, or converge the
      two overloads on one normalization path.
- [ ] Give the OK-path read of `ArchiveRootPath` a benign degrade so an unresolvable archive root
      cannot become an unhandled UI-thread exception.
- [ ] Update the issue #439 rooted-target test to assert the stem, recorded as a deliberate spec
      correction.
- [ ] Unit coverage areas: `SelectRow` for rooted under-root, rooted root-exact, rooted out-of-root,
      relative, and empty-bound-root inputs; the `string` overload of `MoveToFolderAsync`; the
      unresolvable-archive-root degrade.
- [ ] Integration scenario to retest: the composition test added by #614 remediation, asserting that
      any value the OK guard accepts does not cause `ResolvePaths` to throw, must still pass.
- [ ] Manual verification notes: select a suggestion row whose folder is the archive root and confirm
      it is a deterministic non-selection rather than a rejected selection.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
