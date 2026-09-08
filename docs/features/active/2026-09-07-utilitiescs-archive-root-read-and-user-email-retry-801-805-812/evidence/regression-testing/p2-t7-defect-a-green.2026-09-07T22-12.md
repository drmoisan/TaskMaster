# Phase 2 — Defect A Scoped Regression Run, After the Fix (P2-T7)

Timestamp: 2026-09-08T08-03

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

The rebuild reported `Build succeeded.`, `0 Warning(s)`, and `0 Error(s)`. It compiles the new part file `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs`, which P2-T2 added to `UtilitiesCS/UtilitiesCS.csproj`.

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /ResultsDirectory:coverage/plan812/p2-t7 "/Logger:trx;LogFileName=p2-t7.trx" /TestCaseFilter:"(FullyQualifiedName~FolderPredictorArchiveRootDegradationTests&TestCategory!=LiveOutlook)|(FullyQualifiedName~ArchiveStemProjectionTests&TestCategory!=LiveOutlook)"`

EXIT_CODE: 0

`vstest.console.exe` was resolved per D5. The filter is character-for-character the expression P1-T6 used, so the two runs are comparable and the only variable between them is the Phase 2 fix.

Output Summary:

Counters read from `coverage/plan812/p2-t7/p2-t7.trx` rather than from console text:

- Total: 24
- Passed: 24
- Failed: **0**
- Skipped (`notExecuted`): 0

All twelve `FolderPredictorArchiveRootDegradationTests` methods executed and passed. The count of distinct test methods carrying that class name in the `.trx` is 12, which matches the twelve `[TestMethod]` attributes the file declares, so no method was filtered out or silently skipped.

All `ArchiveStemProjectionTests` methods executed and passed. The count of distinct test methods carrying that class name in the `.trx` is 12: the eleven pre-existing methods plus `ToDisplayStem_NullRoot_ReturnsInputUnchanged`, added by P1-T4.

Comparison against the P1-T6 expect-fail run, which is what makes this result a measurement of the fix rather than of a test that could not fail:

| Figure | P1-T6, before the fix | P2-T7, after the fix |
| --- | --- | --- |
| Total | 24 | 24 |
| Passed | 14 | 24 |
| Failed | 10 | 0 |
| Exit code | 1 | 0 |

The ten tests that failed at P1-T6 are exactly D15 items 1 through 10, and every one of them now passes. The two boundary tests that passed at P1-T6 — `FolderArray_WhenArchiveRootPathThrowsComException_PropagatesComException` and `FindFolder_WithNullEmailSearchRootsAndThrowingArchiveRoot_StillThrowsInvalidOperationException` — still pass, so the guarded accessor did not widen into a bare catch and did not degrade the functional archive-root read at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:305`.
