# matchbestspecialfolder-substring-matching-codified-by-tests (Issue #618)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/matchbestspecialfolder-substring-matching-codified-by-tests/ (Issue #618)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #618
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/618
- Last Updated: 2026-08-26
## Summary

`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` exposes a pure static helper,
`MatchBestSpecialFolder` (lines 77-91), that selects the best-matching special folder for a candidate
path. At line 87 it matches with `Contains` rather than a path-prefix test. Substring matching is the
wrong relation for filesystem paths: a candidate can match a special folder it is not actually under,
and, because the comparison is not path-aware, a partial final-segment match such as
`C:\Users\<user>\OneDriveArchive` matches a special folder rooted at `C:\Users\<user>\OneDrive`
despite being a sibling rather than a descendant. The correct relation is a segment-aware prefix
test using an ordinal, case-insensitive comparison, with a directory-separator boundary so a longer
sibling name cannot match.

A repository-wide search found no production caller of `MatchBestSpecialFolder` today. Only the
interface member, the instance delegator, and test doubles reference it, so nothing currently
misbehaves at runtime. This is why the finding is filed on its own rather than as part of issue #614:
it is not on that issue's path-representation chain and fixing it would change nothing #614
exercises.

The reason it is still worth an issue is the test suite.
`TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115` asserts the
substring semantics directly, which converts an unreviewed implementation detail into a pinned
contract. Any future caller — and the natural caller is exactly the special-folder resolution that
supplies `FsAncestorEquivalent` on the #614 chain — would inherit the wrong relation, and a
maintainer correcting the helper would first have to recognize that the failing tests encode the bug
rather than the requirement. Correcting the helper and its tests now, while there is no caller to
regress, is materially cheaper than correcting them under a live dependency.

Found during the issue #614 defect census. The census initially treated this as part of a broader
`AppFileSystemFolderPaths` hypothesis, then reclassified it as off-chain latent hardening once the
absence of a production caller was confirmed.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1.
- Python version: Not applicable; this is C#.
- Command/flags used: Static inspection and repository-wide caller search during the issue #614
  defect census.
- Data source or fixture: Repository source at commit `c279d40b`.

## Steps to Reproduce

1. Call `AppFileSystemFolderPaths.MatchBestSpecialFolder` with a special-folder dictionary containing
   a `OneDrive` entry rooted at `C:\Users\<user>\OneDrive` and a candidate path of
   `C:\Users\<user>\OneDriveArchive`.
2. Observe that the sibling path matches the `OneDrive` special folder, because line 87 tests
   `Contains` rather than a separator-bounded prefix.
3. Inspect
   `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115` and
   observe that the substring behavior is asserted as expected.

## Expected Behavior

A candidate path matches a special folder only when it is that folder or a descendant of it,
determined by an ordinal case-insensitive prefix test that respects directory-separator boundaries.
The tests assert that relation.

## Actual Behavior

Matching is a plain substring test, so unrelated and sibling paths can match. The existing tests
codify this behavior as correct.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not applicable; established by static inspection of `AppFileSystemFolderPaths.cs:87` and
  the cited test assertions.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

No production caller today, therefore no current runtime impact. The cost is a wrong relation pinned
by tests, which would be inherited silently by the first caller added.

## Suspected Cause / Notes

- `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:77-91`, `Contains` at line 87.
- `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs:96-115` codifies
  the substring semantics.
- Related but separate: the `LoadFolders` fallback chain in the same file resolves the `OneDrive` key
  through `AppData` and finally an arbitrary `SpecialFolders.First().Value`. That fallback IS on the
  issue #614 chain (`EfcDataModel` consumes `SpecialFolders["OneDrive"]`) and is handled there, not
  here.

## Proposed Fix / Validation Ideas

- [ ] Replace `Contains` with a separator-bounded, ordinal case-insensitive prefix test, normalizing
      trailing separators on both operands before comparing.
- [ ] Update `AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` so the assertions state the
      prefix relation; treat the currently-passing substring assertions as encoding the defect.
- [ ] Unit coverage areas: exact match; descendant match; sibling with a longer name sharing a prefix
      (must not match); case-differing paths (must match); trailing-separator variants; candidate not
      under any special folder; multiple candidates where the longest valid prefix must win.
- [ ] Integration scenario to retest: none; the helper has no production caller. Re-run the
      `TaskMaster.Test` suite.
- [ ] Manual verification notes: before closing, re-run the caller search to confirm the helper is
      still uncalled, so the fix cannot regress a caller added in the interim.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
