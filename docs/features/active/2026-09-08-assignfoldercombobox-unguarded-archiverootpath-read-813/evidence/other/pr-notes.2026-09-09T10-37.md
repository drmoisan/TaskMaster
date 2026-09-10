Timestamp: 2026-09-09T10-37

## PR Notes -- Issue #813: AssignFolderComboBox unguarded ArchiveRootPath read

### Fix summary
One production change: in
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, the `Ol.ArchiveRootPath` read inside
`AssignFolderComboBox()` is now wrapped in a narrow `try`/`catch (InvalidOperationException)`. When
the archive root is unconfigured or unresolvable, the read now degrades to an empty archive root
(`archiveRootPath = string.Empty`) instead of propagating the exception onto the UI dispatcher
thread. `ProjectPredeterminedFolder`/`ArchiveStemProjection.ToDisplayStem` already treat an empty
archive root as the identity projection, so this degrades cleanly to "no preselection, index
fallback" rather than crashing.

### Test summary
One new regression test added to
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`:
`AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing`. It stubs
`IApplicationGlobals.Ol.ArchiveRootPath` to throw `InvalidOperationException` and asserts:
`AssignFolderComboBox()` does not throw; the combo box (`AddFolderItems`) and suggestion rows
(`SetFolderSuggestions`) still populate; `SetFolderSelectedItem` is never called; and the
index-fallback path (`SetFolderSelectedIndex`) runs instead. The test was confirmed failing
pre-fix (unhandled `InvalidOperationException`) and passing post-fix.

### Risk
None. The fix is behaviorally equivalent to the existing null-`_globals` path (research §2): both
paths converge on `archiveRootPath = null` or `string.Empty` feeding into
`ProjectPredeterminedFolder`, which already has defined boundary behavior for null/empty archive
roots (pinned by the existing `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection`
test). No new dependency, no public API change, no change outside the single method.

### Toolchain and coverage
Full C# toolchain (CSharpier format/check, .NET analyzer rebuild, nullable rebuild, MSTest via
vstest.console.exe with coverage) passed with no regression. Repo-wide line coverage: 85.5936%
(baseline) -> 85.6063% (post-change), an improvement. Repo-wide branch coverage: 79.7998%
(baseline) -> 79.7939% (post-change), a 0.0059-point change, well inside the 0.5-point
no-further-regression allowance and still above the 75% floor. New-code coverage: 100% of the
executable lines in the added try/catch (InvalidOperationException) block are hit (9/9), exceeding
the 90% new-code floor.

### Scope
Only two files are functionally changed by this plan:
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (production fix)
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` (new regression test)
No file owned by issue #812 (`AppOlObjects.cs`, `AppOlObjects.ArchiveRoot.cs`,
`ArchiveRootPathGuard.cs`) or by any sibling epic feature was modified (verified in Phase 6).

### Related issues
- Fixes: #813 (AssignFolderComboBox unguarded ArchiveRootPath read)
- Related: #812 (issue #812's frozen write set -- `AppOlObjects.cs` / `AppOlObjects.ArchiveRoot.cs`
  / `ArchiveRootPathGuard.cs` -- explicitly untouched by this fix)
- Related: #797

This artifact is for use by a later `pr-author` skill run. This plan does not create or submit the
PR.
