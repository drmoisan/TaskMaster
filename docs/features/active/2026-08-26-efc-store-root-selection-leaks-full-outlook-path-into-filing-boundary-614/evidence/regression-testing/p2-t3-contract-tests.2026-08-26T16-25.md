# P2-T3 — ArchiveStemContract unit tests (#614, AC1)

Timestamp: 2026-08-26T16-25

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~ArchiveStemContractTests" "/Logger:trx;LogFileName=p2-t3.trx" "/ResultsDirectory:coverage\trx\p2-t3"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 22; Passed: 22; Failed: 0; Skipped: 0.
- Discovery of `UtilitiesCS.Test.OutlookObjects.Folder.ArchiveStemContractTests` at 22 tests
  proves the explicit `<Compile Include>` wiring of BOTH new files: the test class could not be
  discovered unless `UtilitiesCS.Test\OutlookObjects\Folder\ArchiveStemContractTests.cs` were
  compiled into `UtilitiesCS.Test.dll`, and it could not compile unless
  `UtilitiesCS\OutlookObjects\Folder\ArchiveStemContract.cs` were compiled into `UtilitiesCS.dll`.
  These are non-SDK packages.config projects with no glob includes.
- Matrix covered: `IsFullOutlookPath` for store-rooted, single-backslash-leading,
  forward-slash-leading, drive-rooted, relative, and null/empty values;
  `RequireArchiveRelativeStem` for null, empty, whitespace, store-rooted, drive-rooted, and valid
  relative values including the message-names-parameter and message-withholds-value assertions;
  `TryMakeArchiveRelative` for under-root, exact-root (empty stem), out-of-root ancestor,
  cross-store, case-differing root, `Archive2` separator-boundary near-miss, repeated-ancestor
  substring, forward-separator boundary, trailing-separator root, and null/empty inputs.
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p2-t3\` tree.
