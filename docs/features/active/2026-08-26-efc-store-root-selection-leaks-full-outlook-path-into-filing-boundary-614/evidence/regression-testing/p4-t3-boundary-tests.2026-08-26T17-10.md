# P4-T3 — EmailFilerConfig filing-boundary tests (#614, D4; AC5 test half, AC17 pass-after)

Timestamp: 2026-08-26T17-10

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailFilerConfig_Tests" "/Logger:trx;LogFileName=p4-t3.trx" "/ResultsDirectory:coverage\trx\p4-t3"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 18; Passed: 18; Failed: 0; Skipped: 0.
- **AC17 pass-after.**
  `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers`
  PASSED. Paired with the P1-T2 fail-before artifact (EXIT_CODE 1, ExpectedExitCode 1) this
  completes the fail-before / pass-after evidence pair for AC17.
- New D4 tests, all PASSED:
  `Issue614_ResolvePathsWithFolder_RejectsStoreRootStemThroughTheFolderOverload` (the same
  rejection through the `ResolvePaths(Folder)` overload via `Mock<IApplicationGlobals>` /
  `Mock<IOlObjects>` / `Mock<Folder>`),
  `Issue614_ResolvePaths_RejectsSingleSeparatorLeadingStem` (the D8 output shape),
  `Issue614_ResolvePaths_RejectsEmptyStem`,
  `Issue614_IsDeleteRelevant_NonPrefixAncestorSubstring_ReturnsFalse`,
  `Issue614_IsDeleteRelevant_SeparatorBoundaryNearMiss_ReturnsFalse` (`...\Archive2\...` against
  ancestor `...\Archive`).
- Pre-existing tests confirmed unedited and green, including
  `Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce`,
  `GetStem_RemovesAncestorAndLeadingSlash`, and all three original `IsDeleteRelevant_*` tests.
  `git diff -U0` for the test file shows zero deleted content lines: the P4-T2 change is purely
  additive.
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p4-t3\` tree.
