Timestamp: 2026-09-09T10-39

Intended text (mirror of issue #813 completion status):

Issue #813 (AssignFolderComboBox unguarded ArchiveRootPath read) has been implemented and verified
on branch `bug/assignfoldercombobox-unguarded-archiverootpath-read-813-exec`.

Fix: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` now wraps the `Ol.ArchiveRootPath`
read inside `AssignFolderComboBox()` in a narrow `try`/`catch (InvalidOperationException)`, degrading
to an empty archive root (no preselection, index fallback) instead of propagating the exception onto
the UI dispatcher thread.

Test: a new regression test,
`AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing`, was added to
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`. Confirmed failing
pre-fix (unhandled InvalidOperationException) and passing post-fix.

Toolchain: CSharpier format/check, .NET analyzer rebuild, nullable rebuild, and MSTest (via
vstest.console.exe with coverage) all passed with no regression. Repo-wide line coverage improved
slightly (85.5936% -> 85.6063%); repo-wide branch coverage changed by 0.0059 points (79.7998% ->
79.7939%), within the no-further-regression allowance. New-code coverage for the added try/catch
block is 100%.

Scope: only `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` were functionally
changed. No file owned by issue #812 was modified.

All six spec.md Acceptance Criteria are checked off.

PostedAs: unknown
Note: the GitHub issue itself has not been updated as part of this plan's execution. Posting this
update to the live issue is a separate, later action not performed by this plan.
