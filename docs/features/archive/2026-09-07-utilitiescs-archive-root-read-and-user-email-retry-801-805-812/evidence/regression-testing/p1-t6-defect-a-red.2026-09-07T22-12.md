# Phase 1 — Defect A Scoped Regression Run, Before the Fix (P1-T6) [expect-fail]

Timestamp: 2026-09-08T07-57

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

The rebuild is the precondition for the run below: `UtilitiesCS.Test.csproj` uses explicit `<Compile Include>` items, so the file P1-T1 through P1-T3 created is only compiled once P1-T5 has added its item. The build reported `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`, and produced `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`.

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /ResultsDirectory:coverage/plan812/p1-t6 "/Logger:trx;LogFileName=p1-t6.trx" /TestCaseFilter:"(FullyQualifiedName~FolderPredictorArchiveRootDegradationTests&TestCategory!=LiveOutlook)|(FullyQualifiedName~ArchiveStemProjectionTests&TestCategory!=LiveOutlook)"`

EXIT_CODE: 1

ExpectedExitCode: 1

`vstest.console.exe` was resolved per D5 through `vswhere.exe -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`; neither executable is on `PATH`. The assembly is named explicitly rather than discovered by directory scan. Both disjuncts of the `|`-joined filter repeat `TestCategory!=LiveOutlook`, per D6, because `&` binds tighter than `|`.

Output Summary:

This is an `[expect-fail]` task. A failing run is the required outcome: it proves the ten degradation and read-count assertions genuinely fail before the Phase 2 fix lands, so the green run at P2-T7 measures the fix rather than a test that never could fail.

Counters read from `coverage/plan812/p1-t6/p1-t6.trx` rather than from console text:

- Total: 24
- Passed: 14
- Failed: 10
- Skipped (`notExecuted`): 0

FAILING-TESTS: exactly ten, and they are exactly D15 items 1 through 10.

1. `FolderArray_RecentsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
2. `FolderArray_SuggestionsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
3. `FolderArray_SuggestionsAndRecentsWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
4. `FolderRowArray_RecentsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
5. `FolderRowArray_SuggestionsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
6. `FolderRowArray_SuggestionsAndRecentsWithThrowingArchiveRoot_ReturnsEntriesUnchanged`
7. `FolderArrayAndFolderRowArray_WithThrowingArchiveRoot_ProduceIdenticalText`
8. `FolderArray_WithThrowingArchiveRootAndBothPopulated_ReadsArchiveRootPathExactlyTwice`
9. `FolderRowArray_WithThrowingArchiveRootAndBothPopulated_ReadsArchiveRootPathExactlyTwice`
10. `FolderArray_WithThrowingArchiveRootAndRecentsOnly_ReadsArchiveRootPathExactlyOnce`

The failing set matches the P1-T6 acceptance list exactly, with no member missing and no additional member, so the fixture reproduces the defect and no correction is required.

PASSED, as the acceptance condition requires:

- `FolderArray_WhenArchiveRootPathThrowsComException_PropagatesComException` — passes before the fix because the unguarded read already propagates every exception type. It is included so that the guarded accessor Phase 2 adds cannot silently widen into a bare catch without this test turning red.
- `FindFolder_WithNullEmailSearchRootsAndThrowingArchiveRoot_StillThrowsInvalidOperationException` — passes before the fix, pinning the AC3 non-degradation of the functional read at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:305`.
- `ToDisplayStem_NullRoot_ReturnsInputUnchanged` — the P1-T4 addition. It passes before the fix because `ArchiveStemProjection.ToDisplayStem` already returns its input unchanged for a null root; the gap it closes is the absence of coverage for that case, which is the case the whole degradation depends on.
- Every pre-existing `ArchiveStemProjectionTests` method. That class contributed 12 passing results, of which 11 are pre-existing and 1 is the P1-T4 addition.

Failure mode of the ten, read from the `.trx`: each fails on the archive-root read propagating out of the display path rather than on an assertion comparing two values. That is the defect under test — a display-only projection turning an unresolvable archive root into an exception that reaches the caller.
