# P7-T5 — AppGlobals fail-fast tests (#614, D6 and D7; AC13 and AC14 test halves)

Timestamp: 2026-08-26T18-25

Command: `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~AppOlObjectsArchiveRootValidationTests|FullyQualifiedName~AppFileSystemFolderPathsOneDriveResolutionTests|FullyQualifiedName~AppFileSystemFolderPathsMatchBestSpecialFolderTests" "/Logger:trx;LogFileName=p7-t5.trx" "/ResultsDirectory:coverage\trx\p7-t5"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 22; Passed: 22; Failed: 0; Skipped: 0.
- Per-class counts from the TRX:
  `AppOlObjectsArchiveRootValidationTests` 6 passed;
  `AppFileSystemFolderPathsOneDriveResolutionTests` 7 passed;
  `AppFileSystemFolderPathsMatchBestSpecialFolderTests` 9 passed, **unedited**.
- The untouched `AppFileSystemFolderPathsMatchBestSpecialFolderTests` remaining green is the
  runtime half of the AC14 non-modification proof. The static half: `git diff -U0` for
  `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` produces hunks at old lines 14, 20, 150,
  197, 203 and 206-246 only; none intersects `MatchBestSpecialFolder` at old lines 77-91.
- D6 (AC13): `ArchiveRootPathGuard.RequireResolvedArchiveRoot` returns the composed path when the
  resolved archive folder matches it (including a case-differing match, since Outlook path
  comparison is case-insensitive); throws with a redacted diagnostic when the archive root is
  unresolvable; throws with a redacted diagnostic when the resolved folder lies in a different
  store; and throws for an empty composed path even when the diagnostic sink is null. Neither the
  thrown message nor the recorded diagnostic contains `mailbox@example.com` or `other@example.org`.
  Consumer-side behaviour is exercised through `Mock<IOlObjects>`, not against live Outlook, and
  the decision logic is a pure helper requiring no `Microsoft.Office.Interop.Outlook` object.
- D7 (AC14): `AppFileSystemFolderPaths.ResolveOneDriveRoot` picks `OneDriveCommercial`, then
  `OneDrive`, then `OneDrivePersonal`; treats whitespace-only values as unset; fails explicitly
  with a redacted `InvalidOperationException` when none is set (no `AppData` fallback and no
  arbitrary-first-entry fallback); and rejects a null reader. Every test supplies an in-memory
  reader delegate; no test mutates process environment state.
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p7-t5\` tree.
