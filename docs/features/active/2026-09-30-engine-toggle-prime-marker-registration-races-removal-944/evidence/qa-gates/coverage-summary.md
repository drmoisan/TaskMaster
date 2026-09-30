# Coverage Summary, Final (P3-T8)

Timestamp: 2026-09-30T15-10
Command: dotnet-coverage collect --output coverage\final-944.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-944.config -- vstest.console.exe (9 test assemblies) /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\944\final" "/Logger:trx;LogFileName=final-944.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-COVERAGE-DIRECT, STAGE final), then CMD-COVERAGE-POST (STAGE final, RAW True)
EXIT_CODE: 0
Output Summary:
ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4
MAIN-MERGE-SHA: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 (merge commit 7190a4bcddab8c519933d98b12ede739d4afede3)
COVERAGE-ROUTE: DIRECT
EXIT_CODE: 0 (COLLECT_EXIT_CODE)
LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56098/65750 (85.32%), branches 13597/17054 (79.73%)
ROOT line-rate=0.853202 branch-rate=0.797291 lines-covered=56098 lines-valid=65750 branches-covered=13597 branches-valid=17054
METHOD StartPrimeIfNeeded span=264-289 elements=17 covered=17 uncovered=0 rate=100
METHOD StartObservedPrime span=303-327 elements=19 covered=19 uncovered=0 rate=100
METHOD CompletePrime span=366-382 elements=10 covered=10 uncovered=0 rate=100
Tests 7327 total, 7327 passed, 0 failed. FAILED-SET: (empty). Branch (a) of P0-T18's rule holds.
Pass number: 2 (the run). FIRST-ATTEMPT-EXIT-CODE: 1 and FIRST-ATTEMPT-FAILED-SET are kept in the COORDINATOR-RULING section below.
SIBLING-PROCESS-PROBE: CLEAR (0 vstest.console, testhost or dotnet-coverage processes at 15-08-51 UTC; no wait)

## Details:

- ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4 (the origin/main commit anchored on, from P0-T5)
- COVERAGE-ROUTE: DIRECT (selected by STALL-PROBE: REPRODUCES in P0-T16)
- RAW: True
- Pass number: 1 (first attempt)
- COLLECT_EXIT_CODE: 1
- ASSEMBLY_COUNT: 9
- ASSEMBLY: \QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
- ASSEMBLY: \SVGControl.Test\bin\Debug\SVGControl.Test.dll
- ASSEMBLY: \Tags.Test\bin\Debug\Tags.Test.dll
- ASSEMBLY: \TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
- ASSEMBLY: \TaskTree.Test\bin\Debug\TaskTree.Test.dll
- ASSEMBLY: \TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
- ASSEMBLY: \ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
- ASSEMBLY: \UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
- ASSEMBLY: \VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
- TRX_PRESENT: True
- SEQUENCE_FILES: 0 (no hang dump; the run was not a stall, so neither COVERAGE RUN STALLED nor COVERAGE RUN ABORTED applies)
- DOCUMENT_PRESENT: True
- Collection wall clock: 13-49-10 to 13-51-16 UTC (background process; completion detected by the PAYLOAD-COMPLETE line of coverage\logs\final.result.log). STRAY_TEST_PROCESSES: 0 immediately before the collection started.
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56090/65750 (85.31%), branches 13595/17054 (79.72%)
- ROOT line-rate=0.85308 branch-rate=0.797174 lines-covered=56090 lines-valid=65750 branches-covered=13595 branches-valid=17054

Test-result summary (derived from the trx by Format-TrxRunSummary):

```
SUMMARY-BEGIN
Test run outcome: Failed
Total 7327, executed 7327, passed 7325, failed 2.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces, RemainingLoadActive_AfterLoaderCompletes_BecomesFalse
SUMMARY-END
```

FAILED-SET: RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces, RemainingLoadActive_AfterLoaderCompletes_BecomesFalse

Failure detail (read from the trx; absolute paths replaced with REDACTED-PATH):

- FAILED QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces (duration 00:00:07.04). MESSAGE: Expected SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5)) to be True because async void Worker_DoWork returns at its first await, but found False. Thrown from QfcDatamodelLivenessTests.WaitForState at REDACTED-PATH\QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs line 56.
- FAILED QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse (duration 00:00:05.02). MESSAGE: Expected entered.Task.Wait(TimeSpan.FromSeconds(5)) to be True because the started worker must reach the injected loader, but found False. Thrown from QfcDatamodelLivenessTests.StartHeldOpenLoader at REDACTED-PATH\QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs line 172.

Both failures are in QuickFiler.Test, a project this item does not modify, and both are five-second wall-clock waits that timed out; the baseline run at the anchor (P0-T18) passed all 7324 tests. Whether the failures are load-induced or reproducible is not established by this run; the plan admits no re-run for them, so no re-run was performed.

COORDINATOR-RULING (recorded 2026-09-30 by the item orchestrator before the restart):

- FIRST-ATTEMPT-EXIT-CODE: 1
- FIRST-ATTEMPT-FAILED-SET: QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces (duration 00:00:07.04), QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse (duration 00:00:05.02); the failure messages are the two FAILED lines above, recorded verbatim from the trx.
- Basis: every first-attempt failure is in QfcDatamodelLivenessTests (QuickFiler.Test), which this item neither changes nor covers: the item's footprint is TaskMaster/Ribbon/EngineToggleStateCoordinator.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs and TaskMaster.Test/TaskMaster.Test.csproj, and QuickFiler.Test carries no ProjectReference to TaskMaster.csproj.
- Ruling: one full restart of P3-T1 through P3-T8 is approved on that basis only. The restart must pass with zero failures; there is no second restart; AC14 wording does not change. This is a single re-measurement of a gate, not a fix, and it sets no precedent. The plan revision log records it as R3-1.

JaCoCo package projection:

```
PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4616" covered="38807" />
    <counter type="BRANCH" missed="1860" covered="9411" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="143" covered="1426" />
    <counter type="BRANCH" missed="67" covered="333" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="977" covered="877" />
    <counter type="BRANCH" missed="338" covered="300" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="762" covered="1061" />
    <counter type="BRANCH" missed="260" covered="248" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="56" covered="702" />
    <counter type="BRANCH" missed="16" covered="174" />
  </package>
  <package name="TaskMaster">
    <counter type="LINE" missed="802" covered="2457" />
    <counter type="BRANCH" missed="211" covered="517" />
  </package>
  <package name="TaskTree">
    <counter type="LINE" missed="11" covered="295" />
    <counter type="BRANCH" missed="8" covered="94" />
  </package>
  <package name="VBFunctions">
    <counter type="LINE" missed="0" covered="4" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
PROJECTION-END
```

Coordinator figures (D-8):

- COORD-CLASS-NODES: 1
- COORD-LINES covered=157 valid=157
- COORD-BRANCHES covered=37 valid=38
- METHOD StartPrimeIfNeeded span=264-289 elements=17 covered=17 uncovered=0 rate=100
- METHOD StartObservedPrime span=303-327 elements=19 covered=19 uncovered=0 rate=100
- METHOD CompletePrime span=366-382 elements=10 covered=10 uncovered=0 rate=100

METHOD-LINE rows:

```
METHOD-LINE StartPrimeIfNeeded 265 hits=1
METHOD-LINE StartPrimeIfNeeded 266 hits=1
METHOD-LINE StartPrimeIfNeeded 267 hits=1
METHOD-LINE StartPrimeIfNeeded 268 hits=1
METHOD-LINE StartPrimeIfNeeded 269 hits=1
METHOD-LINE StartPrimeIfNeeded 272 hits=1
METHOD-LINE StartPrimeIfNeeded 273 hits=1
METHOD-LINE StartPrimeIfNeeded 274 hits=1
METHOD-LINE StartPrimeIfNeeded 275 hits=1
METHOD-LINE StartPrimeIfNeeded 276 hits=1
METHOD-LINE StartPrimeIfNeeded 283 hits=1
METHOD-LINE StartPrimeIfNeeded 284 hits=1
METHOD-LINE StartPrimeIfNeeded 285 hits=1
METHOD-LINE StartPrimeIfNeeded 286 hits=1
METHOD-LINE StartPrimeIfNeeded 287 hits=1
METHOD-LINE StartPrimeIfNeeded 288 hits=1
METHOD-LINE StartPrimeIfNeeded 289 hits=1
METHOD-LINE StartObservedPrime 309 hits=1
METHOD-LINE StartObservedPrime 310 hits=1
METHOD-LINE StartObservedPrime 311 hits=1
METHOD-LINE StartObservedPrime 312 hits=1
METHOD-LINE StartObservedPrime 313 hits=1
METHOD-LINE StartObservedPrime 314 hits=1
METHOD-LINE StartObservedPrime 315 hits=1
METHOD-LINE StartObservedPrime 316 hits=1
METHOD-LINE StartObservedPrime 317 hits=1
METHOD-LINE StartObservedPrime 318 hits=1
METHOD-LINE StartObservedPrime 319 hits=1
METHOD-LINE StartObservedPrime 320 hits=1
METHOD-LINE StartObservedPrime 321 hits=1
METHOD-LINE StartObservedPrime 322 hits=1
METHOD-LINE StartObservedPrime 323 hits=1
METHOD-LINE StartObservedPrime 324 hits=1
METHOD-LINE StartObservedPrime 325 hits=1
METHOD-LINE StartObservedPrime 326 hits=1
METHOD-LINE StartObservedPrime 327 hits=1
METHOD-LINE CompletePrime 367 hits=1
METHOD-LINE CompletePrime 368 hits=1
METHOD-LINE CompletePrime 369 hits=1
METHOD-LINE CompletePrime 370 hits=1
METHOD-LINE CompletePrime 373 hits=1
METHOD-LINE CompletePrime 374 hits=1
METHOD-LINE CompletePrime 375 hits=1
METHOD-LINE CompletePrime 380 hits=1
METHOD-LINE CompletePrime 381 hits=1
METHOD-LINE CompletePrime 382 hits=1
```

The raw documents coverage\final-944.cobertura.xml (post-processed in place) and coverage\final-944.trx remain on disk under the git-ignored coverage directory and are not committed.

## PASS-2:

P3-T8, pass 2, Timestamp: 2026-09-30T15-10. Command: CMD-COVERAGE-DIRECT (STAGE final), then CMD-COVERAGE-POST (STAGE final, RAW True); the canonical command is the top-level `Command:` field. EXIT_CODE: 0. The top-level fields of this artifact were rewritten with the values below per the P3-T8 re-run rule; the section headed `Details:` above is the first attempt (pass 1).

### Details:

- ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4 (the origin/main commit anchored on, from P0-T5)
- MAIN-MERGE-SHA: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 (origin/main merged before pass 2 by merge commit 7190a4bcddab8c519933d98b12ede739d4afede3; the measured tree is HEAD aac783905)
- COVERAGE-ROUTE: DIRECT (selected by STALL-PROBE: REPRODUCES in P0-T16)
- RAW: True
- Pass number: 2
- SIBLING-PROCESS-PROBE: immediately before the collection, Get-CimInstance Win32_Process found 0 processes named vstest.console, testhost or dotnet-coverage (15-08-51 UTC), so none was rooted in another worktree and no wait was needed. STRAY_TEST_PROCESSES: 0 by the same observation.
- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9
- ASSEMBLY: \QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
- ASSEMBLY: \SVGControl.Test\bin\Debug\SVGControl.Test.dll
- ASSEMBLY: \Tags.Test\bin\Debug\Tags.Test.dll
- ASSEMBLY: \TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
- ASSEMBLY: \TaskTree.Test\bin\Debug\TaskTree.Test.dll
- ASSEMBLY: \TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
- ASSEMBLY: \ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
- ASSEMBLY: \UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
- ASSEMBLY: \VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- DOCUMENT_PRESENT: True
- Collection wall clock: 15-09-14 to 15-10-13 UTC (background process; completion detected by the PAYLOAD-COMPLETE line of coverage\logs\final.result.log)
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56098/65750 (85.32%), branches 13597/17054 (79.73%)
- ROOT line-rate=0.853202 branch-rate=0.797291 lines-covered=56098 lines-valid=65750 branches-covered=13597 branches-valid=17054
- Substitutions: the payload's output lines were printed by string concatenation, and the whole CMD-COVERAGE-DIRECT payload ran inside one script block whose output was redirected to coverage\logs\final.result.log; the collector command, assemblies and filter are unchanged.

Test-result summary (derived from the trx by Format-TrxRunSummary):

```
SUMMARY-BEGIN
Test run outcome: Completed
Total 7327, executed 7327, passed 7327, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END
```

FAILED-SET: (empty)

JaCoCo package projection:

```
PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4608" covered="38815" />
    <counter type="BRANCH" missed="1858" covered="9413" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="143" covered="1426" />
    <counter type="BRANCH" missed="67" covered="333" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="977" covered="877" />
    <counter type="BRANCH" missed="338" covered="300" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="762" covered="1061" />
    <counter type="BRANCH" missed="260" covered="248" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="56" covered="702" />
    <counter type="BRANCH" missed="16" covered="174" />
  </package>
  <package name="TaskMaster">
    <counter type="LINE" missed="802" covered="2457" />
    <counter type="BRANCH" missed="211" covered="517" />
  </package>
  <package name="TaskTree">
    <counter type="LINE" missed="11" covered="295" />
    <counter type="BRANCH" missed="8" covered="94" />
  </package>
  <package name="VBFunctions">
    <counter type="LINE" missed="0" covered="4" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
PROJECTION-END
```

Coordinator figures (D-8):

- COORD-CLASS-NODES: 1
- COORD-LINES covered=157 valid=157
- COORD-BRANCHES covered=37 valid=38
- METHOD StartPrimeIfNeeded span=264-289 elements=17 covered=17 uncovered=0 rate=100
- METHOD StartObservedPrime span=303-327 elements=19 covered=19 uncovered=0 rate=100
- METHOD CompletePrime span=366-382 elements=10 covered=10 uncovered=0 rate=100

METHOD-LINE rows:

```
METHOD-LINE StartPrimeIfNeeded 265 hits=1
METHOD-LINE StartPrimeIfNeeded 266 hits=1
METHOD-LINE StartPrimeIfNeeded 267 hits=1
METHOD-LINE StartPrimeIfNeeded 268 hits=1
METHOD-LINE StartPrimeIfNeeded 269 hits=1
METHOD-LINE StartPrimeIfNeeded 272 hits=1
METHOD-LINE StartPrimeIfNeeded 273 hits=1
METHOD-LINE StartPrimeIfNeeded 274 hits=1
METHOD-LINE StartPrimeIfNeeded 275 hits=1
METHOD-LINE StartPrimeIfNeeded 276 hits=1
METHOD-LINE StartPrimeIfNeeded 283 hits=1
METHOD-LINE StartPrimeIfNeeded 284 hits=1
METHOD-LINE StartPrimeIfNeeded 285 hits=1
METHOD-LINE StartPrimeIfNeeded 286 hits=1
METHOD-LINE StartPrimeIfNeeded 287 hits=1
METHOD-LINE StartPrimeIfNeeded 288 hits=1
METHOD-LINE StartPrimeIfNeeded 289 hits=1
METHOD-LINE StartObservedPrime 309 hits=1
METHOD-LINE StartObservedPrime 310 hits=1
METHOD-LINE StartObservedPrime 311 hits=1
METHOD-LINE StartObservedPrime 312 hits=1
METHOD-LINE StartObservedPrime 313 hits=1
METHOD-LINE StartObservedPrime 314 hits=1
METHOD-LINE StartObservedPrime 315 hits=1
METHOD-LINE StartObservedPrime 316 hits=1
METHOD-LINE StartObservedPrime 317 hits=1
METHOD-LINE StartObservedPrime 318 hits=1
METHOD-LINE StartObservedPrime 319 hits=1
METHOD-LINE StartObservedPrime 320 hits=1
METHOD-LINE StartObservedPrime 321 hits=1
METHOD-LINE StartObservedPrime 322 hits=1
METHOD-LINE StartObservedPrime 323 hits=1
METHOD-LINE StartObservedPrime 324 hits=1
METHOD-LINE StartObservedPrime 325 hits=1
METHOD-LINE StartObservedPrime 326 hits=1
METHOD-LINE StartObservedPrime 327 hits=1
METHOD-LINE CompletePrime 367 hits=1
METHOD-LINE CompletePrime 368 hits=1
METHOD-LINE CompletePrime 369 hits=1
METHOD-LINE CompletePrime 370 hits=1
METHOD-LINE CompletePrime 373 hits=1
METHOD-LINE CompletePrime 374 hits=1
METHOD-LINE CompletePrime 375 hits=1
METHOD-LINE CompletePrime 380 hits=1
METHOD-LINE CompletePrime 381 hits=1
METHOD-LINE CompletePrime 382 hits=1
```

P3-T8 acceptance on pass 2: branch (a) (exit 0, both floors met, FAILED-SET empty); SEQUENCE_FILES: 0; TRX_PRESENT: True; the Output Summary holds 14 lines; the summary block reports failed 0; COORD-CLASS-NODES: 1; the three METHOD rows are in the Output Summary; the projection contains the TaskMaster package with LINE and BRANCH counters. All clauses hold. The raw documents coverage\final-944.cobertura.xml (post-processed in place) and coverage\final-944.trx remain on disk under the git-ignored coverage directory and are not committed.
