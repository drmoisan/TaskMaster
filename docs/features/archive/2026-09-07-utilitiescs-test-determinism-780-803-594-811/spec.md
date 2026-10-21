# 2026-09-07-utilitiescs-test-determinism-780-803-594 (Spec)

- **Issue:** #811
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T22-40
- **Status:** Approved
- **Version:** 0.1

## Context
Consolidates the `UtilitiesCS.Test` nondeterminism filed as #780, #803, and #594 into one item so the required `mstest-coverage` check stops failing on unrelated pull requests. #803 is a duplicate of the `DfDeedle_COM_Tests` `NullReferenceException` already in #594: `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` fails under parallel load because the class mutates static seams and production code at `DfDeedle.cs:186` dereferences `tableSnapshot.Item1` without a null check. #780 is `DictionaryExtensions.TryAddValuesAsync` cancelling its inner `Task.Run` after a hard-coded 500 ms wall-clock window, which the thread pool exceeds under coverage instrumentation with 24 class workers. #594 also carries two `Console.Out` races between concurrently executing tests. #592 (QuickFiler pump-host 60 s expiry) is deliberately not included: it is a different assembly and needs its own investigation.

Correction to the paragraph above: the static-seam-pollution mechanism it attributes to `DfDeedle_COM_Tests` is refuted by the settled research. The paragraph is retained unchanged as the issue-as-filed record; the verified mechanism is stated in `## Root Cause Analysis` and derived in `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/research/root-cause.2026-09-07T22-10.md`.

Environment:
- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, assembly-level parallelization with 24 class-scope workers
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation /Logger:trx "/TestCaseFilter:TestCategory!=LiveOutlook"`; the repository `mstest-coverage` job
- Data source or fixture: in-memory dictionaries; mocked `Table` and `MAPIFolder` in `DfDeedle_COM_Tests`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: a nondeterministic failure in a required check blocks unrelated pull requests and erodes the signal the gate exists to provide. Production callers of `TryAddValuesAsync` may also receive a spurious cancellation under starvation.


## Repro & Evidence
Steps to Reproduce:
1. Run the full suite under coverage with parallel class workers. Observe, intermittently: `TryAddValuesAsync_UpdatesExistingValue` failing with `TaskCanceledException` after ~20 s (passes in ~2 ms alone); `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` failing with `NullReferenceException` at `DfDeedle.cs:186`; two `Console.Out` races.
2. Re-run the identical head commit with no change and observe all checks passing. Observed on PR #802 at `b2132349` (7107 of 7108 passed, then green on re-run) and during PR #779 verification on 2026-09-04.

Expected:
- Every test in `UtilitiesCS.Test` passes deterministically regardless of class ordering and parallel load.
- `TryAddValuesAsync` does not fail production callers on thread-pool scheduling latency.
- Production code at `DfDeedle.cs:186` does not dereference a possibly-null snapshot element.
- A full nine-assembly `/InIsolation` run reports zero failures on ten consecutive runs.

Actual:
Intermittent failures as above, each blocking a required check on an unrelated pull request and inviting re-run-until-green.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet (#780, 2026-09-04): `System.Threading.Tasks.TaskCanceledException: A task was canceled. at UtilitiesCS.DictionaryExtensions.<TryAddValuesAsync>d__10\`2.MoveNext() in UtilitiesCS\Extensions\DictionaryExtensions.cs:line 179`. Local run 4767 tests, 4766 passed, the failing test alone took 21 s.
- Snippet (#803, PR #802 run 1): `System.NullReferenceException at UtilitiesCS.DfDeedle.GetEmailDataInViewAsync, UtilitiesCS/Extensions/DfDeedle.cs line 186`, Total 7108, Passed 7107, Failed 1.


## Scope & Non-Goals

- In scope:
  - AC1: remove the fixed wall-clock cancellation window from `DictionaryExtensions.TryAddValuesAsync` and lock the surviving cancellation contract with tests.
  - AC2: place every wall-clock deadline on the `GetEmailDataInViewAsync` ETL path under an injectable `TimeProvider`; add a descriptive guard in front of the null-snapshot dereference; replace the two ETL delegate statics on `DfDeedle` with optional parameters so `DfDeedle_COM_Tests` no longer mutates process-wide state.
  - AC3: introduce a `TextWriter` seam on the four production members whose output is captured by tests, convert those tests to their own writer, and remove the `[DoNotParallelize]` serialization stopgap that currently suppresses the race. Remove the `Console.Out` save/restore in `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` (see the count reconciliation below).
  - AC4: produce ten consecutive full-suite run records as committed evidence.
  - AC5: verify by inspection of the diff that no test is stabilized by a sleep, a retry, or a timing tolerance, including retirement of the existing `Returns(120)` tolerance at `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:960-963`.

- AC3 count reconciliation (the criterion says "two"; the tree says four):
  - The criterion wording is the maintainer's and is preserved verbatim under `## Acceptance Criteria`. It does not match the current tree, and this spec records the discrepancy rather than inheriting it.
  - There are FOUR capture-and-assert sites, not two: `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` (`Main_RunsSampleScenarioWithoutThrowing`), `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` (`DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput`), `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` (`PrintTree_WritesIndentedTreeToConsole`), and `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` (`EnumerateTable_WritesFormattedOutputAndMovesToStart`). The count of four is derived twice, by two distinct search strategies over independently enumerated member sets, in the `## Numeric Derivation Evidence` section of the research artifact; both derivations return the same four (file, method) pairs.
  - All four classes ALREADY carry `[DoNotParallelize]`, each with a comment naming this race. The race is therefore currently suppressed by a serialization stopgap, not eliminated. AC3 asks for elimination, so the work is to replace the stopgap with a `TextWriter` seam and then remove the attribute; removing the attribute is what makes the AC4 gate exercise the seam rather than the stopgap.
  - "Two" is most plausibly the number of such failures observed in one run. That is not verifiable from the tree.
  - A fifth site, `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs`, saves `Console.Out` at line 22 and restores it at line 56 with NO `[DoNotParallelize]`. It asserts nothing on console text, so it is not a victim, but it is a PROPAGATOR: under class-level parallelism its `TestInitialize` can capture a sibling class's writer and its `TestCleanup` then installs that foreign writer as the process-wide `Console.Out` for the remainder of the run. Removal of its save/restore is in scope.

- Out of scope / non-goals:
  - The roughly 24 further test classes that call `Console.SetOut(new DebugTextWriter())` with no restore. These are aggressors, not victims: none asserts on console content. Once no test captures `Console.Out`, they harm nothing, and touching approximately 24 files across five test projects would be a disproportionate blast radius for a bugfix.
  - Issue #592 (QuickFiler pump-host 60 s expiry). Different assembly; needs its own investigation.
  - Changing the 250 ms-per-row ETL budget at OlTableExtensions.Etl.cs line 81. Altering it is a timing change with no deterministic test; recorded as a follow-up.
  - The 2000 ms `GetTableInViewAsync` window reached from DfDeedle.cs line 156. It is seamed by a `Func<int, CancellationTokenSource>` factory rather than a `TimeProvider`, so threading it would add a second seam type; recorded as a residual risk and a follow-up.
  - Deleting the inert `(int, int)` `TimeoutAfter` overloads from UtilitiesCS/Threading/TimeOutTask.cs. That file is 1012 lines (already over the 500-line cap) and the overloads retain two test callers and one dead-code caller; recorded as a follow-up.
  - Changing `EtlAsync`'s tuple contract to `object[,]? data` and rethrowing the `TimeoutException`. Cleaner, but widens the diff into every `EtlAsync` consumer; recorded as a follow-up.

- Explicitly excluded systems, integrations, or datasets:
  - No live Outlook or COM interaction. All tests use mocked `Table`, `MAPIFolder`, and `Explorer` objects; the `TestCategory!=LiveOutlook` filter remains in force.
  - No package additions or version changes. `Microsoft.Bcl.TimeProvider 10.0.11` and `Microsoft.Extensions.TimeProvider.Testing 10.9.0` are already referenced by the affected projects.
  - No CI workflow change. The `mstest-coverage` job is unmodified.

## Root Cause Analysis

The mechanism recorded in the issue Summary is refuted. The verified mechanism below is established by static reading of the worktree at `04a54e681bd21e841e124c016df30672ee701b75` and documented in `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/research/root-cause.2026-09-07T22-10.md`, Section 1 (claims 1 to 5 and 7 confirmed; claim 6, the millisecond timings, is only partially confirmed because no TRX from the failing run exists in the tree). No build and no test run were possible in the research session, so every claim below is a static-reading claim and the line numbers should be re-verified at execution time.

### What the issue Summary got wrong

The Summary states that `DfDeedle_COM_Tests` "mutates static seams" and that this "races other classes". That is refuted on two independent grounds:

1. All seven static-seam writes in `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` save the prior value and restore it in a `finally` block: `MessageBoxInvoker` at lines 197-211, 222-236, 261-281, 317-334; `TableEtlInvoker` at lines 399-414; `StoreTableEtlInvoker` at lines 762-782 and 827-847.
2. None of the seams is read on the async path. `TableEtlInvoker` is read at exactly one production site, `DfDeedle.cs:94`, inside the SYNCHRONOUS `GetEmailDataInView(Explorer)`. `StoreTableEtlInvoker` is read at `DfDeedle.FrameUtilities.cs:143`, inside `FromDefaultFolder(Store, ...)`. `MessageBoxInvoker` is reachable in principle from `AddQfcColumnsAsync`, but only when the folder lacks the `Triage` user-defined property; the failing test builds the folder with `BuildFolderWithUdp("Triage")`, so no dialog seam is invoked.

The failing test itself writes no seam at all. The described race therefore has no second participant.

Two further facts reinforce this. The assembly runs at `ExecutionScope.ClassLevel` (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`), so tests within `DfDeedle_COM_Tests` execute sequentially on one worker. The only other test caller of the affected `DfDeedle` entry points is `DfDeedleQfcColumnTimeoutTests`, which is itself `[DoNotParallelize]`.

### The verified chain for the `NullReferenceException` (AC2, issues #803 and #594 item 1)

1. `OlTableExtensions.Etl.cs:81` sets `int milliseconds = 250 * rowCount`. The failing test's strict mock returns `GetRowCount() == 1`, so the deadline is 250 ms.
2. Branch selection at `Etl.cs:90-107` takes the `EtlByRowAsync` branch, because `MAPIFields.BinaryToStringFields` contains `ConversationId` and the test's column dictionary contains that column.
3. `EtlByRowAsync` awaits two `Task.Run(...).TimeoutAfter(timeout, attempts)` hops (`Etl.cs:244-245` and `248-259`). Each hop is a separate 250 ms wall-clock deadline that starts when `TimeoutAfter` is called, that is, BEFORE the thread pool has scheduled the `Task.Run` body. The deadline therefore measures scheduling latency in addition to the work.
4. The retry count is inert. The `(int, int)` overload at `TimeOutTask.cs:824` wraps a call to the proxy-returning `(int, TimeProvider?)` overload in a `catch (TimeoutException)`. The inner overload never throws: it returns the task itself, or a proxy `tcs.Task` that a `TimeProvider` timer later faults with `TrySetException(new TimeoutException())`. The `catch` can never fire, `repeatAttempts` is never consulted, and the "attempts remaining" log line is unreachable.
5. On expiry the `TimeoutException` propagates out of `EtlByRowAsync` into `Etl.cs:117-123`, which logs and calls `tokenSource.Cancel()` but swallows the exception. `data` remains null (initialised `object[,]? data = null` at line 83), and line 129 returns `(data!, columnDictionary)`, forcing a null through a null-forgiving suppression into a non-nullable tuple element. In the failing test the `tokenSource` is a fresh source unrelated to the `CancellationToken.None` passed as `token`, so nothing downstream observes the cancellation.
6. `DfDeedle.cs:188` then dereferences `tableSnapshot.Item1.GetLength(0)` inside a `LogDfTiming` call whose statement begins at line 186, which is the sequence point the CI stack trace reports.
7. The issue-#798 guard at `DfDeedle.cs:195` runs AFTER that dereference and inspects `Item2` (the column map) only, never `Item1`. Hoisting it would not guard this path.

This is a production defect, not only a test defect. In production, `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:82-109` wraps the call in `catch (TaskCanceledException)` and `catch (System.Exception)`; because the dereference fires before the cancelled token is next consulted, a real Outlook ETL timeout surfaces as an unattributed `NullReferenceException` rather than the intended cancellation.

The failing-run millisecond figures quoted in #803 (99 ms and 96 ms green, 389 ms failing, against a 250 ms deadline) are not verifiable from the tree. What the code does establish is that the path performs at least four thread-pool scheduling hops plus a progress timer, under 24 class-level workers and coverage instrumentation, and that nothing else on the path has a deadline smaller than 250 ms (the next smallest is 1000 ms at `DfDeedle.cs:208`).

### The verified chain for the `TaskCanceledException` (AC1, issue #780)

`DictionaryExtensions.TryAddValuesAsync` (`UtilitiesCS/Extensions/DictionaryExtensions.cs:169-180`) creates a linked token source and calls `CancelAfter(500)` at line 177, then passes the linked token to `Task.Run`.

- `TryAddValuesAsync` has ZERO production call sites. The only invocation anywhere in the repository is `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:244`; the other two search hits are the definition and a test method name.
- The body it guards, `TryAddValues` at `DictionaryExtensions.cs:123-136`, is a bounded compare-and-swap loop over `ConcurrentDictionary.TryUpdate` that terminates; it performs no I/O and cannot hang.
- The linked token is passed only to `Task.Run`, and a `Task.Run` body cannot be interrupted by its token once running. The window can therefore only cancel work that has not started.

The 500 ms window consequently guards nothing and can only convert thread-pool scheduling latency into a spurious `TaskCanceledException` at the `await`, which is exactly the failure recorded at line 179. Secondary defect: `linkedTS` is never disposed, so every call leaks a timer until it fires.

### The `Console.Out` races (AC3, issue #594 items 2 and 3)

Class A saves `originalOut` and installs `writer_A`; class B on another worker saves `originalOut` (now `writer_A`) and installs `writer_B`; A's `finally` restores the real console, detaching `writer_B`; B's production call writes to the console and `writer_B.ToString()` is empty, so B's assertion fails. Four victims and roughly 25 aggressors exist in `UtilitiesCS.Test`. All four victims currently carry `[DoNotParallelize]`; under MSTest's documented execution model those run sequentially after the parallel set, so as of this tree no victim can overlap an aggressor. That ordering claim is documentation-sourced and was not confirmed by a run.

Superseded issues: #780, #803, #594 (close with a pointer to this issue).


## Proposed Fix

### Design summary (what changes where):

Three independent defects, one shared technique. Where a wall-clock deadline is load-bearing in production, it is placed under an injectable `TimeProvider` so tests control the clock without widening the deadline. Where a wall-clock deadline is not load-bearing, it is deleted. Where a test depends on process-wide `Console.Out`, the production member gains a `TextWriter` seam so the test supplies its own writer.

- AC1: delete the linked token source and `CancelAfter(500)`; pass the caller's `token` straight to `Task.Run`.
- AC2: thread an optional `TimeProvider?` through `GetEmailDataInViewAsync`, `EtlAsync`, and `EtlByRowAsync`; replace the three inert `(int, int)` `TimeoutAfter` calls on that path with the `(int, TimeProvider?)` overload; insert a null guard on `tableSnapshot.Item1` before the `LogDfTiming` dereference; replace the `TableEtlInvoker` and `StoreTableEtlInvoker` statics with optional delegate parameters and delete the statics.
- AC3: add an optional `TextWriter?` to `DASLFilterParser.PrintTree`, the two `PrettyPrinters.PrettyPrint` overloads, and `OlTableExtensions.EnumerateTable`; extract `GFG.Main`'s body into `Run(TextWriter)`. Convert the four capture-and-assert tests to their own `StringWriter`, remove their `Console.Out` handling and `[DoNotParallelize]`, and remove the `NLogTraceWriter_Test` save/restore.

The scoping decision is settled and is not re-opened here. In particular, replacing the `DfDeedle` static seams with optional delegate parameters is IN SCOPE, because AC2's first clause literally requires that `DfDeedle_COM_Tests` stop mutating process-wide static seams. `MessageBoxInvoker` remains static: it is a modal-dialog stub reached through private methods that tests invoke by reflection with fixed argument arrays, so parameterising it would rewrite five tests for no determinism gain. Its restored-in-`finally`, reader-free status is documented in the test-class header instead.

### Boundaries and invariants to preserve:

- **Production timing behaviour is unchanged.** Every new `timeProvider` parameter defaults to `null`, which resolves to `TimeProvider.System`. The production deadline is preserved exactly; only the test controls the clock. This holds byte-for-byte because the `(int, int)` overload already delegated to the `(int, TimeProvider? = null)` overload and its retry was inert.
- **Every new parameter is optional and trailing, so all existing callers stay source-compatible.** The callers that must continue to compile unchanged are `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:82-89`, `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:51,66`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:74`, and `ToDoModel/Data Model/ID/IDList.cs:130,226`. An optional parameter is binary-breaking only for pre-compiled callers, of which there are none outside the solution.
- **Cancellation semantics of `TryAddValuesAsync` are preserved.** A pre-cancelled caller token still yields `TaskCanceledException` from `Task.Run`; only the internal timer is removed.
- **The 500-line file cap.** `PrettyPrint.cs` (680), `TimeOutTask.cs` (1012), `DfDeedle_COM_Tests.cs` (870), and `OlTableExtensions_Tests.cs` (1855) are PRE-EXISTING violations; the fix must not add net lines to any of them. `OlTableExtensions.Etl.cs` is at 475 of 500 and has little room, so added documentation must stay to two or three lines or the dead `EtlAsyncOld` (lines 132-169) must be deleted in the same edit. `DfDeedleQfcColumnTimeoutTests.cs` is at exactly 500 and cannot grow by one line, so the reusable `ArmingBarrierTimeProvider` must move out of it rather than be copied.
- **AC5 constrains the technique, not just the outcome.** No sleep, no retry loop, no widened deadline, and no mock value chosen to outrun a deadline may be introduced or retained in the touched tests.

### Dependencies or blocked work:

None. The `TimeProvider` seam is already in place end-to-end: `TimeOutTask.TimeoutAfter` already accepts a `TimeProvider?`, `DfDeedle.AddQfcColumnsAsync` already threads one (issue #798), and both packages are already referenced. No blocking upstream work.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

Production (`UtilitiesCS`, eight files; all carry `#nullable enable`):

1. `UtilitiesCS/Extensions/DictionaryExtensions.cs` — `TryAddValuesAsync`: delete the linked source and `CancelAfter(500)`.
2. `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` — `EtlAsync` and `EtlByRowAsync`: add trailing `TimeProvider? timeProvider = null`; convert the three `TimeoutAfter(ms, attempts)` calls to `TimeoutAfter(ms, timeProvider)`; drop the inert `attempts` local; correct the timeout log text, which currently claims "timed out {attempts} times" for a retry that never happened, and drop the banned `DateTime.Now` on that line.
3. `UtilitiesCS/Extensions/DfDeedle.cs` — `GetEmailDataInViewAsync`: add trailing `TimeProvider? timeProvider = null`; forward it to `AddQfcColumnsAsync` and `EtlAsync`; convert `.TimeoutAfter(1000, 2)` to `.TimeoutAfter(1000, timeProvider)`; insert the null-snapshot guard before the `LogDfTiming` call. `GetEmailDataInView(Explorer)`: replace the `TableEtlInvoker` static with an optional delegate parameter and delete the static.
4. `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` — `FromDefaultFolder(Store, ...)`: replace the `StoreTableEtlInvoker` static with an optional delegate parameter; the `FromDefaultFolder(Stores, ...)` overload forwards it.
5. `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` — `PrintTree`: add `TextWriter? writer = null`; write to `(writer ?? Console.Out)`; forward on recursion.
6. `UtilitiesCS/HelperClasses/PrettyPrint.cs` — the `PrettyPrint(DataFrame)` and `PrettyPrint(DataFrameRow)` overloads: add `TextWriter? writer = null`, with zero net line growth.
7. `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` — `EnumerateTable`: add `TextWriter? writer = null` and route its three `Console.WriteLine` calls to it.
8. `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` — extract `GFG.Main`'s body into `public static void Run(TextWriter writer)`; `Main` calls `Run(Console.Out)`.

Tests (`UtilitiesCS.Test`, eleven files; none of these carries `#nullable enable`, so annotations added there are inert):

9. `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs`
10. `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` (870 lines, over cap; modify in place)
11. `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` (new)
12. `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` (new)
13. `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` (exactly 500 lines; pure move of the nested `ArmingBarrierTimeProvider` out of it)
14. `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` (new; receives the moved helper)
15. `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs`
16. `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs`
17. `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs`
18. `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` (1855 lines, over cap; modify in place)
19. `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` (remove the `Console.Out` save/restore)

Project file (one edit):

20. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — add `<Compile Include=...>` items for the three new test files. These are legacy non-SDK `packages.config` projects, so a new `.cs` file that is not listed is silently not compiled. This is a Compile-item edit only; no package change is required, because `Microsoft.Bcl.TimeProvider 10.0.11` and `Microsoft.Extensions.TimeProvider.Testing 10.9.0` are already referenced by both `UtilitiesCS` and `UtilitiesCS.Test`.

Files cited elsewhere in this document that do not appear in items 1 to 20 are references only and are not modified.

#### Functions/classes/CLI commands impacted:

`DictionaryExtensions.TryAddValuesAsync`; `OlTableExtensions.EtlAsync`; `OlTableExtensions.EtlByRowAsync`; `OlTableExtensions.EnumerateTable`; `DfDeedle.GetEmailDataInViewAsync`; `DfDeedle.GetEmailDataInView(Explorer)`; `DfDeedle.FromDefaultFolder(Store, ...)` and its `Stores` overload; `DfDeedle.TableEtlInvoker` and `DfDeedle.StoreTableEtlInvoker` (deleted); `DASLFilterParser.PrintTree`; `PrettyPrinters.PrettyPrint(DataFrame)` and `PrettyPrint(DataFrameRow)`; `GFG.Main` and the new `GFG.Run`. No CLI command changes.

#### Data flow and validation changes:

The ETL result tuple keeps its shape `(object[,] data, Dictionary<string, int> columnInfo)`. The change is that `GetEmailDataInViewAsync` now validates `tableSnapshot.data` before use instead of dereferencing it. The guard is placed ahead of the `LogDfTiming` call and therefore ahead of the existing `ValidateRequiredEmailColumns` guard, which inspects the column map only. A null test against a non-nullable tuple element compiles without a nullable diagnostic under `#nullable enable`.

`EtlAsync`'s own contract is deliberately unchanged: on deadline expiry it still swallows the `TimeoutException`, still cancels `tokenSource`, and still returns a null `data`. Changing that contract is recorded as a follow-up, not part of this fix.

#### Error handling and logging updates:

- New: `GetEmailDataInViewAsync` throws `InvalidOperationException` naming the folder when the snapshot data is null, in the wording style of the existing `ValidateRequiredEmailColumns` message.
- Corrected: the `EtlAsync` timeout log no longer claims a retry count that was never consumed, and no longer formats the timestamp with the banned `DateTime.Now`.
- Unchanged: `QfcDatamodel.FrameBuilding.cs:102-108` continues to log and rethrow; it now receives an attributed exception instead of an unattributed `NullReferenceException`.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. Every production change is either a deletion of a guard with no consumer, an optional parameter whose default reproduces current behaviour, or a new exception on a path that previously threw `NullReferenceException` at the next statement. Rollback is a straight revert of the branch.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `TryAddValuesAsync(this ConcurrentDictionary<TKey,TValue>, TKey, TValue, CancellationToken)` — signature unchanged; returns `bool`. Cancellation is now governed solely by the caller's token.
- `EtlAsync(..., TimeProvider? timeProvider = null)` and `EtlByRowAsync(..., TimeProvider? timeProvider)` — return type unchanged.
- `GetEmailDataInViewAsync(Explorer, CancellationToken, CancellationTokenSource, ProgressTracker, TimeProvider? timeProvider = null)` — returns `Frame<int,string>`; now throws `InvalidOperationException` instead of `NullReferenceException` on a null snapshot.
- `GetEmailDataInView(Explorer, Func<object,(object[,] data, Dictionary<string,int> columnInfo)>? etl = null)` and `FromDefaultFolder(Store, ..., <same delegate> = null)` — replaces the static invoker properties.
- `PrintTree(TreeNode<string>, int, TextWriter? writer = null)`, `PrettyPrint(this DataFrame, TextWriter? writer = null)`, `PrettyPrint(this DataFrameRow, TextWriter? writer = null)`, `EnumerateTable(this Outlook.Table, TextWriter? writer = null)` — `null` means `Console.Out`, preserving current behaviour for all existing callers.

#### Required configuration keys and defaults:

None. No configuration key, `.runsettings` value, `.editorconfig` entry, or `packages.config` entry is added or changed.

#### Backward-compatibility expectations:

Source-compatible for every in-solution caller; all new parameters are optional and trailing. The two deleted static properties (`TableEtlInvoker`, `StoreTableEtlInvoker`) are a public-surface removal, but their only readers and writers are in-repo and all are updated in the same change. The one intentional behaviour change is the exception type on the null-snapshot path, described under `## Data / API / Config Impact`.

#### Performance constraints (latency/throughput/memory):

No production latency change. All deadlines resolve to `TimeProvider.System` when no provider is supplied. One small improvement: `TryAddValuesAsync` no longer allocates a linked `CancellationTokenSource` and an undisposed timer per call.


## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The research artifact's line numbers were read statically at `04a54e681bd21e841e124c016df30672ee701b75` with no build and no test run; the executor re-verifies them before editing.
  - MSTest runs `[DoNotParallelize]` tests sequentially after the parallel set. This is documentation-sourced and was not confirmed by a run; the AC3 seam fix removes the dependency on it either way.
  - `Workers = 0` resolves to 24 on the local workstation. The GitHub-hosted runner's core count is not verifiable from the tree; both environments have reproduced the failures.
  - Local full-suite runs require the shell-icon exclusion filter (`ShellUtilities`, `SysImageListHelper`, `OSBrowser`), per the `SHELL_ICON_EXCLUSION: REQUIRED` verdict recorded in the #798 baseline evidence. CI runs those classes unfiltered.
- Constraints (budget, performance, compatibility):
  - The 500-line file cap, with the pre-existing violations enumerated under "Boundaries and invariants to preserve".
  - AC5 forbids sleeps, retries, and timing tolerances, including in the RED-first regression test.
  - No production timing behaviour may change.
  - The CI `mstest-coverage` job carries `timeout-minutes: 30`, which bounds what can be asked of CI.
- External dependencies (services, libraries, releases):
  - `Microsoft.Bcl.TimeProvider 10.0.11` and `Microsoft.Extensions.TimeProvider.Testing 10.9.0`, both already referenced. No new dependency.

## Data / API / Config Impact
- User-facing or API changes:
  - **Intentional diagnostic improvement.** A swallowed ETL timeout on the QuickFiler frame-building path currently surfaces as a `NullReferenceException` that names neither the folder nor the step. After the fix it surfaces as an `InvalidOperationException` naming the folder and stating that the table ETL timed out or was cancelled before returning rows. `QfcDatamodel.FrameBuilding.cs:102-108` logs and rethrows it, so the user-visible failure text changes and the log entry becomes attributable. The failure still occurs; only its diagnosis improves. Whether QuickFiler should instead treat this as a cancellation and return null quietly is a product decision outside this fix.
  - Removal of the public static properties `DfDeedle.TableEtlInvoker` and `DfDeedle.StoreTableEtlInvoker`, replaced by optional delegate parameters. All in-repo readers and writers are updated in the same change.
  - Optional trailing parameters added to seven public members, all source-compatible.
- Data or migration considerations: none. No persisted format, schema, or settings file is touched.
- Logging/telemetry updates (if any): the `EtlAsync` timeout log message is corrected to stop reporting a retry count that is never consumed, and stops using `DateTime.Now`. The new `InvalidOperationException` message is the diagnostic carrier for the null-snapshot path.
- Compatibility notes (CLI flags, config schemas, versioning): none. No CLI flag, `.runsettings`, or workflow change.

## Test Strategy
Seeded from issue: the acceptance criteria for this bug are maintained in a single place, `## Acceptance Criteria` below. Work mode is `full-bug`, so `spec.md` is the sole acceptance-criteria source file and no `user-story.md` exists. This section describes how each criterion is validated; it does not restate the criteria.

Validation:

- Unit coverage areas: `DictionaryExtensions.TryAddValuesAsync` cancellation contract; `DfDeedle.GetEmailDataInViewAsync` null-snapshot path under a controlled clock; `OlTableExtensions.EtlAsync` deadline-expiry and green paths under a controlled clock; the four seamed `TextWriter` members.
- Integration scenario to retest: ten consecutive full-suite runs locally and one CI run.
- Manual verification notes: none.

- Regression tests to add or update:
  - **AC2 requires a RED-first regression test.** New `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs`: a strict `Table` mock whose `GetNextRow` blocks on a gate, with the clock advanced past the 250 ms deadline; the test asserts that `GetEmailDataInViewAsync` throws `InvalidOperationException` naming the folder. It must FAIL before the fix with `NullReferenceException` and PASS after with the descriptive `InvalidOperationException`. The gate is released in a `finally` so the orphaned `Task.Run` completes. A second test uses an un-advanced fake clock, never engages the gate, and asserts the one-row frame — the deterministic green path.
  - **`ArmingBarrierTimeProvider` is a hard constraint, not a convenience** (research risk 9). `FakeTimeProvider` schedules timers relative to its current time at creation, so the clock must be advanced only AFTER the first `TimeoutAfter` timer is armed. Advancing earlier means the timer is armed past an already-elapsed deadline and the test hangs. The existing pattern is at `DfDeedleQfcColumnTimeoutTests.cs:50-85` and is moved to a shared helper rather than duplicated, because that file is at exactly the 500-line cap.
  - New `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs`: `EtlAsync` on the fake clock — expiry returns `data == null` and cancels the token source (documenting the surviving contract), and an un-advanced clock returns the transformed rows.
  - `DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`: pass an un-advanced `FakeTimeProvider`, so every deadline on the path is armed on a clock that does not move and cannot fire regardless of host load. The assertions are unchanged. This is not a timing tolerance: the deadline is not widened, it is placed under the test's control.
- Unit tests (MSTest) for the fixed behavior and boundaries:
  - `TryAddValuesAsync_UpdatesExistingValue` is retained and becomes deterministic with no change to the test.
  - New: a pre-cancelled token yields `TaskCanceledException`, locking the surviving cancellation contract.
  - New: a token cancelled after the work is observed to start proves the outer token, and not an internal timer, governs cancellation. Note that no test can be made to fail deterministically before the AC1 fix without a sleep, because the original failure is load-dependent; the RED artifact for AC1 is therefore the contract-shape test, not a reproduction.
  - The four seamed `TextWriter` members are asserted against a test-owned `StringWriter` with no `Console.Out` interaction.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): deadline expiry on the first ETL hop and on the second; a null snapshot with a valid column map (proving the new guard fires before the existing `ValidateRequiredEmailColumns`); a pre-cancelled token; a null `TextWriter` argument resolving to `Console.Out`; a null `TimeProvider` argument resolving to `TimeProvider.System`.
- Error handling and logging verification: the `InvalidOperationException` message names the folder; the exception type is asserted explicitly, so a regression to `NullReferenceException` fails the test rather than passing a generic `Should().Throw<Exception>()`.
- Coverage impact and targets for changed lines/modules: repository-wide line coverage must remain at or above the current floor and must not regress on changed lines. The new guard, the new `Run(TextWriter)` extraction, and every new optional-parameter branch (supplied and defaulted) are exercised, so changed-line coverage on the eight production files is expected to be complete.
- Toolchain commands to run (format → lint → type-check → test), verbatim from `CLAUDE.md`, restarting from step 1 if any step fails or changes files:
  1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- AC4 gate, stated honestly:
  - Ten consecutive full-suite `/InIsolation` runs over the assembly set the `mstest-coverage` workflow discovers (every `*.Test.dll` under `\bin\Debug\`, excluding `\obj\` and `\ref\`; the set is enumerated in the research artifact, Section 2, AC4), with `TestCategory!=LiveOutlook`.
  - Estimated 10 to 20 minutes total, plus one build. The #798 baseline measured 54.5 s per run without in-proc coverage; `/EnableCodeCoverage` adds instrumentation overhead that has not been measured for the full set in any committed evidence.
  - Local runs must extend the filter with the shell-icon exclusion (`FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`), because one of those tests fails per run on this workstation with `Win32 handle that was passed to Icon is not valid`, independently of this fix. The unfiltered form is covered by the CI run on the pull request. One CI run is not ten; the evidence artifact must state both facts.
  - The gate omits `/Settings:` so parallelism comes from the assembly attribute exactly as in CI. Assembly discovery from a worktree under `.claude/worktrees/` must apply the dot-claude exclusion relative to the worktree root, or discovery returns zero assemblies and the gate is vacuous.
  - The CI `mstest-coverage` job carries `timeout-minutes: 30` in `.github/workflows/_mstest-coverage.yml`, so the ten-run gate cannot be a CI gate and must be produced as local evidence.
  - Result recording: a ten-row table (run, total, passed, failed, wall-clock) read from the `<Counters>` element of each TRX, written to `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/regression-testing/`. Raw TRX and `.coverage` files are not committed: the TRX embeds `computerName` and absolute paths, and `.coverage` files are large binaries under a gitignored directory.
- AC5 enforcement, stated honestly:
  - RS0030 is held at `suggestion` severity in `.editorconfig`, so NO toolchain step fails on a newly introduced `Thread.Sleep` or `Task.Delay`. `CancelAfter`, `TimeoutAfter`, and `WaitOne` are not in `BannedSymbols.txt` at all.
  - AC5 therefore requires an explicit search over the diff during the QA loop for `Thread.Sleep`, `Task.Delay`, `CancelAfter`, `WaitOne`, `new CancellationTokenSource(<int>)`, retry loops, and mock values chosen to outrun a deadline. Reliance on the analyzer is not sufficient.
  - The existing tolerance at `OlTableExtensions_Tests.cs:960-963` (`GetRowCount()` returning 120 with the comment "so the timeout cannot fire under test-host contention") is retired in this change and replaced with the fake clock.
- Manual validation steps (if required): none beyond the AC4 evidence run.


## Acceptance Criteria
- [x] AC1: `TryAddValuesAsync` no longer cancels on a fixed wall-clock window, or the window is driven by an injected `TimeProvider`; the test passes deterministically under 24-worker parallel coverage runs.
- [x] AC2: `DfDeedle_COM_Tests` no longer mutates process-wide static seams in a way another class can observe, and `DfDeedle.cs:186` guards the null snapshot element with a descriptive failure.
- [x] AC3: The two `Console.Out` races are removed by eliminating the shared-console dependency.
- [ ] AC4: A full nine-assembly `/InIsolation` run with `TestCategory!=LiveOutlook` reports zero failures on ten consecutive runs, recorded as evidence.
- [x] AC5: No test is stabilized by a sleep, a retry, or a timing tolerance.

## Risks & Mitigations
- Technical or operational risks:
  1. **The ETL timeout is partially load-bearing for real COM calls.** `Table.GetNextRow` and `Row.GetValues` are synchronous COM calls that can stall on a slow store, and the deadline is the only thing that returns control to the caller. It is only partially load-bearing because the deadline cannot stop the work: the `Task.Run` body keeps running inside the interop marshaller, exactly as the #798 comment at `DfDeedle.QfcColumns.cs:114-118` records.
  2. **The residual 2000 ms `GetTableInViewAsync` window** reached from DfDeedle.cs line 156 stays on the system clock. It has eight times the margin of the 250 ms window and did not trip in the recorded failure, but it is the next-smallest deadline on the same test path. It is seamed by a `Func<int, CancellationTokenSource>` factory rather than a `TimeProvider`, so covering it would introduce a second seam type into this fix.
  3. **The `[DoNotParallelize]` ordering claim is documentation-sourced, not run-verified.** If MSTest does not in fact run those tests after the parallel set, the four console-capture tests can still race today.
  4. **Re-parallelizing `OlTableExtensions_Tests`** removes `[DoNotParallelize]` from a 1855-line COM-mock class whose only documented serialization reason is the console. If undocumented shared state exists in that class, removing the attribute could introduce a new intermittent failure.
  5. **The 250 ms-per-row budget is small for real stores as well as for tests.** A one-row folder gets 250 ms for two pool hops plus COM enumeration.
  6. **`Workers = 0` makes the local worker count differ from the runner's**, and the local `Invoke-MSTest*.ps1` scripts pass a `/Settings:` file that parallelizes eight assemblies CI runs sequentially. Local and CI loads are not equivalent.
  7. **The RED test can hang** if the fake clock is advanced before the first `TimeoutAfter` timer is armed.
  8. **AC5 has no automated enforcement**, because RS0030 is at `suggestion` severity and the relevant timing APIs are largely unbanned.

- Mitigations and rollbacks:
  1. The design keeps the production deadline byte-for-byte (system clock when `timeProvider` is null) and only makes the test control the clock, so no production safety behaviour is removed. The new guard converts the post-expiry `NullReferenceException` into a descriptive `InvalidOperationException` that the QuickFiler boundary logs and rethrows, which is strictly better diagnosis than today.
  2. Record it as a residual risk and a follow-up rather than widening this fix. Two options exist for the follow-up: pass a factory returning a never-cancelling `CancellationTokenSource` from the test, or unify on `TimeProvider` via `TimeProviderTaskExtensions.CreateCancellationTokenSource` (its availability in `Microsoft.Bcl.TimeProvider 10.0.11` was not verified).
  3. The `TextWriter` seam removes the dependency on the ordering claim either way, which is why the seam and not the attribute is the fix.
  4. The AC4 ten-run gate is the detector. Conservative fallback: keep `[DoNotParallelize]` on `OlTableExtensions_Tests` and rewrite its comment so it no longer claims the console as the reason, since after the seam that reason is false. The class is still converted to the `TextWriter` seam either way.
  5. Not changed under this issue: altering it is a timing change with no deterministic test. Promoted as a potential defect for separate triage.
  6. The proposed gate omits `/Settings:` for CI parity, and the evidence artifact states which load it represents.
  7. Reuse the `ArmingBarrierTimeProvider` pattern documented at `DfDeedleQfcColumnTimeoutTests.cs:44-49`, moved to a shared helper. Release the blocking gate in a `finally` so no orphaned `Task.Run` outlives the test.
  8. An explicit diff search in the QA loop, listed under `## Test Strategy`, with the result recorded in the QA evidence.
  - Rollback: no feature flag is warranted. Every production change is a deletion of a consumer-free guard, an optional parameter whose default reproduces current behaviour, or a new exception on a path that previously threw at the next statement. Rollback is a straight revert of the branch.

## Rollout & Follow-up
- Release/rollout steps:
  1. Implement, then run the full toolchain in the order given under `## Test Strategy` until it passes in a single pass.
  2. Produce the AC4 ten-run evidence locally and commit the summary table (not the raw TRX or `.coverage` files).
  3. Open the pull request; the `mstest-coverage` CI run supplies the unfiltered single-run form.
- Post-fix monitoring or clean-up tasks:
  - Close #780, #803, and #594 with a pointer to #811.
  - Watch the next several `mstest-coverage` runs on unrelated pull requests for a recurrence of any of the three failure modes.
  - File follow-ups for: the 250 ms-per-row ETL budget; the residual 2000 ms `GetTableInViewAsync` window; deletion of the inert `(int, int)` `TimeoutAfter` overloads; changing `EtlAsync`'s tuple to a nullable `data` with a rethrown `TimeoutException`; the roughly 24 unrestored `DebugTextWriter` console aggressors; the two production `Console.WriteLine` diagnostics in `OlTableExtensions.TableAccess.cs` that should use the logger; and promotion of RS0030 from `suggestion` to `warning` after legacy cleanup.
  - Issue #592 (QuickFiler pump-host 60 s expiry) remains open and separate.
- Links: issue, PRs, related docs
  - Issue: https://github.com/drmoisan/TaskMaster/issues/811
  - Superseded: #780, #803, #594. Related and not included: #592. Prior related work: #798 (the `TimeProvider` shape this fix copies), #520 (the original console-race report), #181 (banned-symbol rollout).
  - Research: `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/research/root-cause.2026-09-07T22-10.md`
