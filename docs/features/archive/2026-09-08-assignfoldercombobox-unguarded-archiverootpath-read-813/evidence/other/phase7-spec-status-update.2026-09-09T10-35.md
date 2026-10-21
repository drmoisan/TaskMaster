Timestamp: 2026-09-09T10-35
All six Acceptance Criteria checkboxes in spec.md confirmed as `- [x]`:

1. [x] A regression test exists in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
   (or a new `...Part3.cs` if the file-size cap requires it) that stubs
   `IApplicationGlobals.Ol.ArchiveRootPath` to throw `InvalidOperationException` and asserts
   `AssignFolderComboBox()` completes without throwing. -- checked off by P4-T2.
2. [x] The same test asserts the folder combo box and suggestion rows are still populated
   (`AddFolderItems` and `SetFolderSuggestions` were invoked) despite the archive-root read failing.
   -- checked off by P4-T3.
3. [x] The same test asserts no preselection occurs (`SetFolderSelectedItem` is never called) and
   that the index-fallback path (`SetFolderSelectedIndex`) runs instead. -- checked off by P4-T4.
4. [x] The fix in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` catches only
   `InvalidOperationException`, not a broader exception type. -- checked off by P6-T2.
5. [x] No files owned by issue #812 (`AppOlObjects.cs`, `AppOlObjects.ArchiveRoot.cs`,
   `ArchiveRootPathGuard.cs`) or by sibling epic features (listed under Scope & Non-Goals) are
   modified. -- checked off by P6-T5.
6. [x] Full C# toolchain passes with no regression: CSharpier format check, `.NET` analyzer rebuild
   (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), nullable rebuild
   (`/p:TreatWarningsAsErrors=true`), and MSTest execution via `vstest.console.exe`. -- checked off
   by P5-T7.

Acceptance: all six read `- [x]`. PASS.
