# 2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound (Spec)

- **Issue:** #798
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T02-30
- **Status:** Draft
- **Version:** 0.2

> **Authority.** issue.md records `- Work Mode: full-bug`. Under
> the acceptance-criteria-tracking skill this document is the sole authoritative
> acceptance-criteria source for the item. AC1 through AC6 below are reproduced verbatim from the
> criteria settled with the maintainer on 2026-09-06 and must not be renumbered, reworded, merged,
> split, or weakened. AC7 through AC14 are supplementary criteria added by this spec.

> **Formatting convention — do not "fix" this.** The `## Write Set` section is the only place in
> this document where a repository file path appears inside backticks. Every other file reference,
> including bare filenames and File.cs:123-style line citations, is deliberately written as plain
> prose. A downstream extractor derives this item's change footprint by harvesting backtick-delimited
> path tokens in order to schedule it against three concurrently-prepared sibling items, and that
> extractor cannot see negation: adding backticks to a path this change does not touch produces a
> false conflict, and removing them from a path it does touch silently drops the file from the
> footprint. Backticks elsewhere in the document mark code identifiers, not paths.

> **Source of technical truth.** The research record at
> research/2026-09-07T02-01-quickfiler-crash-column-add-timeout-swallowed-keynotfound-research.md
> re-verified every citation in issue.md against the worktree at base commit c431dc32. Where it
> corrects an issue.md citation, the research record wins; two of those corrections change the
> design and are called out in Root Cause Analysis below.

## Context
Launching QuickFiler from the ribbon on a folder named "T&E" crashed Outlook with an unhandled `AggregateException` wrapping `KeyNotFoundException: The given key was not present in the dictionary`. The column-add step that prepares the Outlook `Table` timed out three times, the timeouts were swallowed, and the ETL proceeded with Outlook's five default columns. The row builder then indexed the column dictionary by the missing key `SentOn`. The exception was rethrown with `throw e`, wrapped by the timeout helper, and escaped an `async void` ribbon handler with no boundary catch.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in, debug build loaded from TaskMaster\bin\Debug, HEAD c431dc32 (2026-09-06)
- Command/flags used: Outlook ribbon -> QuickFiler (`RibbonViewer.QuickFiler_Click` -> `RibbonController.LoadQuickFilerAsync` -> `QfcHomeController.LaunchAsync`)
- Data source or fixture: live Exchange mailbox, Explorer showing folder "T&E" (225 rows). Earlier launches on Inbox (3998 rows) in the same session succeeded.

Impact / Severity:
- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

Outlook-level unhandled exception on a normal ribbon action. The failure is folder-dependent and gives the user no diagnostic.


## Repro & Evidence
Steps to Reproduce:
1. In Outlook, select the folder "T&E" (or another folder on which adding the QuickFiler table columns takes more than 3 seconds).
2. Click QuickFiler on the ribbon.
3. Observe: no dialog appears; after roughly 9 seconds Outlook reports an unhandled `AggregateException` from `QfcDatamodel.GetEmailsInViewDfAsync`.

Not reproducible on demand on Inbox. The maintainer confirmed no message box was shown before the crash.

Expected:
- If the required table columns cannot be added within the timeout, the launch fails with a logged, user-visible error naming the folder and the step, and QuickFiler does not proceed to build the data frame.
- The row builder validates that every required column (`EntryID`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`) is present before indexing, and throws a descriptive exception naming the missing column and the folder.
- Exceptions are rethrown with `throw;` so the original stack is preserved.
- Ribbon `async void` handlers catch exceptions at the boundary, log them, and show an error dialog instead of crashing Outlook.
- The column-add step logs its timing so a slow COM call is diagnosable.

Actual:
Unhandled exception dialog. Top of the stack:

```
System.AggregateException: One or more errors occurred.
   at QuickFiler.Controllers.QfcDatamodel.<GetEmailsInViewDfAsync>d__47.MoveNext() in ...\QuickFiler\Controllers\QfcDatamodel.FrameBuilding.cs:line 108
   ...
   at QuickFiler.Controllers.QfcHomeController.<InitAsync>d__5.MoveNext() in ...\QuickFiler\Controllers\QfcHomeController.cs:line 149
   at QuickFiler.Controllers.QfcHomeController.<LaunchAsync>d__3.MoveNext() in ...\QuickFiler\Controllers\QfcHomeController.cs:line 60
   at TaskMaster.RibbonController.<LoadQuickFilerAsync>d__24.MoveNext() in ...\TaskMaster\Ribbon\RibbonController.cs:line 118
   at TaskMaster.RibbonViewer.<QuickFiler_Click>d__23.MoveNext() in ...\TaskMaster\Ribbon\RibbonViewer.cs:line 150
   at System.Threading.QueueUserWorkItemCallback.System.Threading.IThreadPoolWorkItem.ExecuteWorkItem()
Inner Exception 1: System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
```

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet (TaskMaster\bin\Debug\logs\debug_2026-09-06.log, storeId redacted). Note the 9.085-second gap between table acquisition and ETL start, and `columnCount=5` at ETL:

```
2026-09-06 19:21:50,529 [VSTA_Main] DEBUG UtilitiesCS.DfDeedle - [Df timing] GetEmailDataInViewAsync explorer/table acquisition complete | ... | folder=T&E; storeId=<redacted>
2026-09-06 19:21:59,614 [VSTA_Main] DEBUG UtilitiesCS.OlTableExtensions - [Table timing] EtlAsync start | ETL over table snapshots | ...
2026-09-06 19:21:59,644 [VSTA_Main] DEBUG UtilitiesCS.OlTableExtensions - [Table timing] EtlAsync complete | ... | rowCount=225; columnCount=5; elapsedMs=29
2026-09-06 19:21:59,646 [VSTA_Main] DEBUG UtilitiesCS.DfDeedle - [Df timing] GetEmailDataInViewAsync table snapshot ready | ... | rowCount=225; columnCount=5; etlElapsedMs=59
2026-09-06 19:21:59,647 [VSTA_Main] DEBUG UtilitiesCS.DfDeedle - [Df timing] GetEmailDataInViewAsync dataframe transform start | ...
2026-09-06 19:21:59,892 [4] ERROR QuickFiler.Controllers.QfcDatamodel - GetEmailDataInViewAsync Error.
 One or more errors occurred.
   at UtilitiesCS.DfDeedle.<GetEmailDataInViewAsync>d__9.MoveNext() in ...\UtilitiesCS\Extensions\DfDeedle.cs:line 186
```

- Comparison, successful Inbox launch in the same session: acquisition complete 17:36:59,968 -> EtlAsync start 17:37:00,021 (53 ms gap), ETL `columnCount=5`, dataframe transform complete with `columnCount=6`.

Independent corroboration recorded by the research record: log lines 14100 and 14101 are 9.085 s apart with no log line of any kind between them, which confirms that the bound timeout overload emits nothing on timeout and that the retry loop is silent.


## Scope & Non-Goals

### In scope
- AC1 through AC6 exactly as written under `## Acceptance Criteria`, plus the supplementary criteria AC7 through AC14 that constrain how they are delivered.
- The 16 files enumerated under `## Write Set`, and nothing else.

### Out of scope / non-goals

Both items below were identified by the research record and deliberately excluded. Each will be promoted to its own GitHub issue through the potential-to-issue lifecycle; neither is fixed in this change.

1. **Unreachable `catch (TimeoutException)` in the two `repeatAttempts` `TimeoutAfter` overloads.** The overloads at TimeOutTask.cs lines 824-849 and 924-940 wrap a non-awaited call — `try { result = task.TimeoutAfter(ms); } catch (TimeoutException) { ... }`. `TimeoutAfter(ms)` returns a proxy task that faults later and never throws synchronously, so the `catch` can never execute and the documented retry never happens. This affects DfDeedle.cs line 190 and four call sites in the OlTableExtensions ETL partial. Recorded here so no plan task assumes a retry that does not occur.
2. **Pre-existing unguarded shared-static mutation in the existing COM test class.** DfDeedle_COM_Tests mutates the shared static `MessageBoxInvoker` in five tests while the assembly parallelizes at class level, without `[DoNotParallelize]`. This is a latent flake that predates this defect.

### Explicitly excluded systems, integrations, and datasets

The following files are **not** modified by this change. They are named as plain prose deliberately; see the formatting blockquote above.

- UtilitiesCS/Threading/TimeOutTask.cs — the fix reuses the existing `TimeoutAfter(Task, int, TimeProvider?)` overload unchanged. The file is 1011 lines, already over the 500-line cap, and editing it would both widen the blast radius and require a cap remediation unrelated to this defect.
- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs and UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs — `GetColumnDictionary` and `EtlAsync` are generic table utilities whose other callers legitimately have different column sets, so the AC3 validator does not belong there.
- QuickFiler/Controllers/QfcQueue.cs — the `throw e;` at line 71 is in a different type and is out of AC4 scope.
- QuickFiler/Helper Classes/cInfoMail.cs — the occurrence at line 162 is commented out.
- TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs — its 20 `async void` members are out of AC5 scope; RibbonController.EngineCommands.cs lines 98-106 document that their awaited tasks are contractually non-faulting.
- The dot-claude, dot-codex and dot-agents trees; the two published JSON files under the config directory; any GitHub workflow file; the solution file; the repository-root build property files.


## Root Cause Analysis

Chain confirmed by code read plus the log timing above. Citations below are the research record's corrected ones; where they differ from issue.md, the research record's citation is authoritative.

1. `DfDeedle.GetEmailDataInViewAsync` (DfDeedle.cs lines 134-201) calls `AddQfcColumnsAsync(table, currentFolder!, token, 0)` at line 164.
2. `AddQfcColumnsAsync` (DfDeedle.cs lines 318-343) runs `AddQfcColumns` inside `Task.Run(...).TimeoutAfter(3000)` at line 327. On `TimeoutException` it recurses at lines 333 and 340 while `counter < 2`. With `counter == 2` the `catch` body does nothing and the returned task completes successfully. Three attempts of 3000 ms account for the 9.085 s gap.
3. `AddQfcColumns` (DfDeedle.cs lines 296-316) adds `SentOn`, `ConversationId` and `Triage` at lines 310-312 and removes `Subject`, `CreationTime` and `LastModificationTime` at lines 313-315. When it has not completed, the table keeps Outlook's default five columns, matching `columnCount=5`.
4. `table.EtlAsync` (OlTableExtensions.Etl.cs lines 66-130) builds `columnDictionary` at line 84 from `GetColumnDictionary` (OlTableExtensions.cs lines 200-237), mapping schema names to friendly names via `MAPIFields.SchemaToField` (MAPIFields.cs lines 83-95).
5. `Email2dToRecords` (DfDeedle.cs lines 214-237) indexes `columnInfo["EntryID"]`, `["MessageClass"]`, `["SentOn"]`, `["ConversationId"]` and `["Triage"]` at lines 226-230 with no presence check. `SentOn` is the first missing key in access order. The **same five unchecked reads appear a second time** in `GetEmailDataFromTable` at DfDeedle.cs lines 120-124, on the synchronous path used by `GetEmailDataInView` and `QfcDatamodel.InitDf`.
6. `GetEmailsInViewDfAsync` (QfcDatamodel.FrameBuilding.cs lines 102-109) catches, logs only the message and stack, and rethrows with `throw e;` at line 108, discarding the original stack.
7. `QfcHomeController.LaunchAsync` (QfcHomeController.cs lines 58-84) catches only `OperationCanceledException`. `RibbonViewer.QuickFiler_Click` (RibbonViewer.cs lines 148-151), `QuickFilerHighConfidence_Click` (lines 153-156) and `SortEmail_Click` (lines 158-159) are `async void` with no try/catch, and the three controller methods they call contain no catch, so the exception reaches the thread pool and Outlook reports it.

### Two corrections that change the design

**Correction 1 — the call binds to the `TimeProvider` overload, not the `repeatAttempts` overload.** issue.md cites TimeOutTask.cs lines 924-940, which is `TimeoutAfter(Task, int, int repeatAttempts)`. `Task.Run(Action, CancellationToken)` returns a non-generic `Task`, eliminating both generic overloads; of the remaining pair, the `repeatAttempts` overload requires two explicit arguments after the receiver and only `3000` is supplied. **The call at DfDeedle.cs line 327 therefore binds to `TimeoutAfter(Task, int, TimeProvider? = null)` at TimeOutTask.cs lines 949-1009.** Consequences: that overload accepts a `TimeProvider`, so the timeout path is deterministically testable without any wall-clock wait; it emits no log line, which is why the 9.085 s window is silent; and it neither cancels nor awaits the wrapped task, so each recursion starts a *new* `Task.Run` against the same COM `Table`, allowing up to three concurrent column-add calls.

**Correction 2 — the `AggregateException` wrapping happens upstream of the `throw e;`.** The issue.md summary orders the events as "rethrown with `throw e`, wrapped by the timeout helper". The wrapping is first: DfDeedle.cs lines 186-190 run the dataframe transform as `Task.Run(...).TimeoutAfter(1000, 2)`, and on fault `MarshalTaskResults` (TimeOutTask.cs line 805) calls `proxy.TrySetException(source.Exception)` where `source.Exception` is already an `AggregateException`. The awaited proxy therefore throws `AggregateException`, and `throw e;` merely resets that already-wrapped exception's stack. **Consequence for AC4: `throw;` restores the original stack but does not unwrap the `AggregateException`.** The AC5 dialog must therefore render inner exceptions, or the user sees only "One or more errors occurred."

### Why the column add exceeded 9 seconds

Not recorded. The maintainer saw no dialog, which rules out the `MessageBoxInvoker` prompts in `EnsureTriageColumnExists` (DfDeedle.cs lines 345-390) blocking on input. Remaining candidates are the `folder.UserDefinedProperties` enumeration inside `HasUserDefinedProperty` (DfDeedle.cs line 399) and the six `table.Columns.Add`/`Remove` COM calls themselves, possibly aggravated by the overlapping retries. AC2 exists so that the next occurrence is attributable.

The draft analysis produced by another assistant proposed that a missing `Triage` user property or a non-mail folder view was the trigger. That hypothesis is rejected: the missing-property path throws `InvalidOperationException` at DfDeedle.cs line 307, not `KeyNotFoundException`.


## Proposed Fix

### Invariant established by this change

**After a QuickFiler launch reaches the column-add step, exactly one of three things is true: the required columns `EntryID`, `MessageClass`, `SentOn`, `ConversationId` and `Triage` are all present in the column dictionary before any row is indexed; or a descriptive exception naming the folder and the failing step (column-add timeout, or the specific missing columns) has been thrown, logged with its original stack, and presented to the user in a dialog; and in no case does more than one column-add operation run concurrently against the same COM `Table`, and in no case does an exception originating in this path reach Outlook unhandled.**

### Trace of one accepted value

The path below has no guard anywhere between the accept point and the absorption point, which is what makes the fix load-bearing.

1. **Accept point — DfDeedle.cs line 164.** `GetEmailDataInViewAsync` awaits `AddQfcColumnsAsync(table, currentFolder!, token, 0)`. The method returns a bare `Task`, so a completed await is the *only* success signal available. It validates nothing about the resulting column set.
2. **Absorption point — DfDeedle.cs lines 336-342.** On the third timeout, `counter < 2` is false, the `catch (TimeoutException)` body does nothing, and the task completes successfully. No exception, no return value, no log line. This location cannot report the failure: the method has no failure channel, and the bound `TimeoutAfter` overload emits nothing either. Control returns to step 1, which cannot distinguish this from success, and proceeds to `table.EtlAsync(...)` at DfDeedle.cs line 168 with Outlook's five default columns.
3. **Throw point — DfDeedle.cs lines 226-230.** Roughly 250 ms later and two layers away, `Email2dToRecords` indexes `columnInfo["SentOn"]` and raises `KeyNotFoundException`. The exception names neither the folder nor the timed-out step, so the reported symptom is unrelatable to the cause.
4. **Where the fix moves the failure.** AC1 throws at the exhausted-timeout site inside the relocated column-add method — the only location that knows both the folder and the step that timed out. AC3 adds a second, independent guard in `GetEmailDataInViewAsync` between DfDeedle.cs lines 177 and 181, where `currentFolder` is in scope and the folder name has already been captured for the line-158 log, and a matching guard in the synchronous `GetEmailDataInView` between lines 94 and 96 to close the duplicate unchecked indexing at lines 120-124.
5. **Where the fix moves the catch.** AC5 places a boundary catch around the three ribbon handlers. That boundary *can* report: it runs after the awaited task completes, has an injected logging sink and an injected presentation sink, and is the last frame before the thread pool. Today no frame between step 3 and the thread pool contains a catch that can report — `QfcHomeController.LaunchAsync` catches only `OperationCanceledException`, and the three `RibbonController` methods contain no catch at all.

**Why neither half suffices alone.** AC1 without AC3 leaves every other route to a short column dictionary unguarded, including the duplicate-key fallback in `GetColumnDictionary` and the synchronous `GetEmailDataFromTable` path, which does not call the column-add at all. AC3 without AC1 reports the symptom (a missing column) but not the cause (a timed-out column add), and still burns the full 9-second budget silently before failing. AC5 without AC1 and AC3 converts a crash into a dialog reading "One or more errors occurred." with no actionable content.

**Inverse constraint — catches that must not be widened.** The `catch (OperationCanceledException)` in `QfcHomeController.LaunchAsync` (QfcHomeController.cs lines 58-84) must remain exactly that narrow: user cancellation must continue to be silent and must not reach the new ribbon dialog. The `throw e;` at QfcQueue.cs line 71 is a different type and must not be touched. No existing catch may be broadened to `System.Exception` as an alternative to delivering AC1 or AC3.

### Design summary (what changes where)

- **AC1 and AC2** — relocate the four column methods (`AddQfcColumns`, `AddQfcColumnsAsync`, `EnsureTriageColumnExists`, `HasUserDefinedProperty`) from the existing `DfDeedle` file into a new `DfDeedle` partial dedicated to QuickFiler column handling, and rewrite the timeout loop there. This keeps both files under the 500-line cap (the source file is at 410 lines with 90 lines of headroom) and gives the new logging and validation code a home.
- **AC1 non-overlap decision.** A blocking synchronous COM call cannot be cancelled on .NET Framework: `Task.Run(..., token)` only suppresses *scheduling*, and there is no mechanism to interrupt a call already inside the interop marshaller. AC1's clause "the underlying task is cancelled or the retry waits for it" is therefore satisfied by the second alternative: **start the work exactly once** (`var work = Task.Run(() => columnAdder(table, folder), token);`), then apply `work.TimeoutAfter(3000, timeProvider)` to **that same instance** up to three times. No second COM call is ever started, the 9000 ms total budget is preserved, and after the third `TimeoutException` a descriptive exception naming the folder and the step is thrown.
- **A `Task.Delay`-based timeout is prohibited.** BannedSymbols.txt lines 4-7 ban `Thread.Sleep(int)`, `Thread.Sleep(TimeSpan)`, `Task.Delay(int)` and `Task.Delay(TimeSpan)` with the message "Do not call Task.Delay directly in production code. Inject a time abstraction (System.TimeProvider)." A `Task.WhenAny(work, Task.Delay(...))` implementation would fail the analyzer gate. Re-applying the existing `TimeoutAfter(ms, timeProvider)` overload to a single held task is the only shape that is both analyzer-clean and deterministically testable.
- **`RunWithTimeout` is not a substitute.** The existing `RunWithTimeout<T1,T2,TResult>` helper logs a warning and returns `default` on exhaustion instead of throwing, starts a new `Task.Run` per retry (reproducing today's overlap), and exposes no `TimeProvider` at that arity.
- **AC3** — add a pure validator to the new partial and call it from both `GetEmailDataInViewAsync` and the synchronous `GetEmailDataInView`. Do **not** add a parameter to `Email2dToRecords`, `Email2dArrayToDf` or `GetEmailDataFromTable`: all three are pinned by fixed-arity tests, and none of them has the folder name in scope. AC3's "or its caller" wording is satisfied at the caller.
- **AC4** — change `throw e;` to `throw;` at QfcDatamodel.FrameBuilding.cs line 108 (the AC4 target) and at the two identical sibling defects in the same partial family, QfcDatamodel.cs lines 359 and 400.
- **AC5** — extract a new host-neutral `internal sealed class RibbonCommandBoundary` and route the three named handlers through it. The handlers become one-liners.

### Boundaries and invariants to preserve

- `AddQfcColumnsAsync` becomes `internal` rather than `public`; `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` at AssemblyInfo.cs line 19 makes that sufficient for testing.
- The new parameters are **optional parameters on the existing method**, not a new static seam. The test assembly declares `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`, so a new class mutating shared statics would race the existing COM test class. Optional parameters carry no cross-class state.
- `RibbonViewer` keeps its `[ExcludeFromCodeCoverage]` and `[ComVisible(true)]` attributes and its existing internal test constructor. No handler is renamed, reordered or reformatted.
- The total column-add budget stays at 9000 ms (3 x 3000 ms) so no folder that succeeds today begins to fail on timing alone.
- The `[Df timing]` log format is unchanged: the existing `LogDfTiming(string phase, string? details = null)` helper is reused, so the new lines carry the same `[Df timing] ` prefix, the same `threadId=...; syncContext=...` context, and the same `logger.Debug` level.

### Dependencies or blocked work

None. `Microsoft.Bcl.TimeProvider` and `Microsoft.Extensions.TimeProvider.Testing` are already referenced by the affected projects for `net481`; no package is added. No sibling item blocks this one.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

See `## Write Set`. Six production files, five test files and five project compile-entry files.

#### Functions/classes/CLI commands impacted

- Relocated and rewritten: `AddQfcColumns`, `AddQfcColumnsAsync`, `EnsureTriageColumnExists`, `HasUserDefinedProperty`.
- Added: `ValidateRequiredEmailColumns`, `RibbonCommandBoundary`, `ReportRibbonCommandFailure`.
- Modified call sites: `GetEmailDataInViewAsync`, `GetEmailDataInView`, `GetEmailsInViewDfAsync`, `LoadRemainingEmailsToQueueAsync`, `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)`, `QuickFiler_Click`, `QuickFilerHighConfidence_Click`, `SortEmail_Click`.
- Unmodified by design: `Email2dToRecords`, `Email2dArrayToDf`, `GetEmailDataFromTable`, `GetColumnDictionary`, `EtlAsync`, and all four `TimeoutAfter` overloads.

#### Data flow and validation changes

`GetEmailDataInViewAsync` captures the folder name as a `string` on the STA near DfDeedle.cs line 158 and passes that string — never the COM `MAPIFolder` — into the later `Task.Run` lambda at lines 186-189, avoiding a cross-apartment property read. The validator compares keys **ordinally**, because the dictionary produced at OlTableExtensions.cs line 217 uses the default ordinal comparer, and the required names carry an intentional casing asymmetry (`EntryID` with a capital D, `ConversationId` with a lowercase d) sourced from `MAPIFields.SchemaToField`.

#### Error handling and logging updates

- New timing lines via the existing `LogDfTiming` helper around the `folder.UserDefinedProperties` enumeration inside `HasUserDefinedProperty`, and around each of the three `Columns.Add` and three `Columns.Remove` calls individually, so a single slow column is attributable.
- New descriptive exception on column-add timeout exhaustion, naming the folder and the step.
- New descriptive exception on missing required columns, naming every missing column and the folder.
- Ribbon failures are logged through the established `logger.Error(string, Exception)` shape and presented through `MessageBox.Show`. The repository has no non-modal notice surface; RibbonController.EngineCommands.cs lines 151-157 states this and names `logger.Warn` plus `MessageBox.Show` as the established mechanism. There is no `MessageBoxInvoker`-style seam in the TaskMaster assembly, and `MyBox` is not used under the ribbon folder.
- The presentation sink must render inner exceptions (see Correction 2), or the dialog reads "One or more errors occurred."

#### Rollback/feature-flag considerations (if applicable)

No feature flag. The change is revertible as a single commit. AC4 is a one-token change at three sites and is trivially re-appliable if a sibling item conflicts.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

```csharp
// New signature (was: private static async Task AddQfcColumnsAsync(Table, MAPIFolder, CancellationToken, int))
internal static async Task AddQfcColumnsAsync(
    Table table,
    MAPIFolder folder,
    CancellationToken token,
    int counter,
    Action<object, object>? columnAdder = null,
    TimeProvider? timeProvider = null);
```

**Why the injected column-adder is typed over `object` rather than the interop types.** `Action<Table, MAPIFolder>` does not compile across the assembly boundary: embedded interop types cannot be used as generic type arguments, which produces CS1769. The existing seams `TableEtlInvoker` (DfDeedle.cs lines 69-72) and `StoreTableEtlInvoker` (lines 81-84) already carry an in-source comment recording exactly this constraint, so `Action<object, object>` is a required repetition of an established pattern, not a stylistic choice. Ordinary interop-typed *parameters* such as `Table table` remain fine; only the generic type arguments are affected. The implementation casts inside the default adder.

```csharp
// New pure validator. Throws naming every missing key and the folder.
internal static void ValidateRequiredEmailColumns(
    Dictionary<string, int> columnInfo,
    string folderName);
```

`columnInfo` is concretely `System.Collections.Generic.Dictionary<string, int>` at every producer and consumer, so no interface widening is needed. The required key set is exactly `EntryID`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`.

```csharp
// New host-neutral boundary. NOT marked [ExcludeFromCodeCoverage].
internal sealed class RibbonCommandBoundary
{
    internal RibbonCommandBoundary(
        Action<string, System.Exception> logFailure,
        Action<string> presentFailure);

    internal Task RunAsync(string commandName, Func<Task> action);
}
```

`RunAsync` awaits `action`, catches `System.Exception`, forwards to both injected sinks, and never rethrows. A throwing sink must not escape. This mirrors the ratified pattern from issue #503's `EngineGatedCommandRunner`, whose own doc records that it is deliberately not coverage-exempt because it is host-neutral decision logic while presentation belongs to the coverage-exempt ribbon shim.

#### Required configuration keys and defaults

None. The 3000 ms per-attempt deadline and the three-attempt budget are existing behaviour and are unchanged; both remain in-source constants, not configuration.

#### Backward-compatibility expectations

- No public API changes. `AddQfcColumnsAsync` moves from `private` to `internal`, which widens rather than narrows access.
- The two reflection invocations in the existing COM test class pass a four-element `object[]` and will throw `TargetParameterCountException` against the six-parameter method, because reflection does not apply C# default values absent `Type.Missing` plus `BindingFlags.OptionalParamBinding`. The reflection helper is replaced with direct `internal` calls, which also reduces that 882-line test file. This is a required, explicitly planned repair, not incidental churn.
- Callers of `Email2dToRecords`, `Email2dArrayToDf` and `GetEmailDataFromTable` are unaffected; their arities are unchanged.

#### Performance constraints (latency/throughput/memory)

- Worst-case column-add latency is unchanged at 9000 ms; the change removes up to two redundant concurrent COM calls, so the load on the `Table` decreases.
- The new timing logs are `logger.Debug` calls on a path executed once per launch; seven additional debug lines per launch is not a measurable cost.
- The validator is O(5) dictionary lookups.


## Assumptions, Constraints, Dependencies

- **Assumptions.** The reproduction folder remains available to the maintainer for AC6 manual verification. The `[Df timing]` debug level remains enabled in the debug build. The research record's citations hold at base commit c431dc32; an executor rebasing onto a newer main must re-verify the line numbers before editing.
- **Unverified, promoted to a Phase 0 check.** It is not established by execution that a `MemoryAppender` attached from the UtilitiesCS test assembly captures log output emitted by the `UtilitiesCS.DfDeedle` logger. The supporting evidence is that neither assembly declares a log4net repository attribute, so the default repository should be shared, but no build or test run was performed. Phase 0 must confirm this by running one throwaway assertion before AC2's log-content tests are written. If it does not hold, assert AC2 indirectly through the injected column-adder and the timing values it observes, and record the fallback in the plan.
- **Constraints.** .NET Framework 4.8.1, non-SDK-style MSBuild projects with explicit `<Compile Include>` items. No blocking COM call can be cancelled. The 500-line file cap applies to every file this change creates or modifies. `Task.Delay` and `Thread.Sleep` are analyzer-banned in production code.
- **External dependencies.** None added. Existing package versions, identical across the two affected test projects and all `targetFramework="net481"`: MSTest.TestFramework 4.4.0, MSTest.TestAdapter 4.4.0, MSTest.Analyzers 4.4.0, Moq 4.20.72, FluentAssertions 8.10.0, Microsoft.Bcl.TimeProvider 10.0.11, Microsoft.Extensions.TimeProvider.Testing 10.9.0.


## Data / API / Config Impact

- **User-facing changes.** A launch that previously crashed Outlook now shows a modal error dialog naming the folder and the failing step. A launch on a folder whose column add exceeds the 9-second budget now fails deterministically with that dialog instead of proceeding into a data frame built from the wrong columns. See Risks item 1.
- **Data or migration considerations.** None. No persisted format, schema or stored setting changes.
- **Logging/telemetry updates.** Seven new `[Df timing]` debug lines per launch: one around the `folder.UserDefinedProperties` enumeration, three around the individual `Columns.Add` calls, three around the individual `Columns.Remove` calls. Two new error paths log through the existing loggers. Format, prefix and level match the established helpers exactly.
- **Compatibility notes.** No CLI flags, no config schema, no version bump.


## Write Set

Every file this change creates or modifies, as a repository-relative path. Counts are derived in the research record under Numeric Derivation Evidence N3, whose two independent enumerations (direct from the derived write set, and independently re-derived from AC1 through AC6) agree element-for-element in both partitions.

`production_file_count = 6`
`test_file_count = 5`

Project compile-entry files: 5. Total paths: 16.

### Production files (6)

- `UtilitiesCS/Extensions/DfDeedle.cs` — modify. Relocate the four column methods to the new partial; add the two validator calls (after line 177 on the async path, between lines 94 and 96 on the synchronous path); capture the folder name as a string on the STA near line 158. Net line-count reduction.
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` — create. Partial holding the relocated methods with the AC1 single-held-task timeout loop and descriptive throw, the AC2 timing instrumentation, and the pure AC3 validator.
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` — modify. Line 108, `throw e;` becomes `throw;`.
- `QuickFiler/Controllers/QfcDatamodel.cs` — modify. Lines 359 and 400, `throw e;` becomes `throw;`.
- `TaskMaster/Ribbon/RibbonCommandBoundary.cs` — create. Host-neutral `internal sealed class` with injected log and presentation sinks; not marked `[ExcludeFromCodeCoverage]`.
- `TaskMaster/Ribbon/RibbonViewer.cs` — modify. Route the three handlers at lines 148, 153 and 158 through the boundary; add one field and a `ReportRibbonCommandFailure` sink modelled on the existing failure reporter at lines 311-315.

### Test files (5)

- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` — create. AC1 and AC2.
- `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs` — create. AC3.
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` — modify. Replace the reflection helper at lines 495-499 and its two invocations at lines 511-515 and 534-538 with direct `internal` calls.
- `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs` — create. AC5.
- `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` — create. AC4.

### Project compile-entry files (5)

All five are non-SDK-style MSBuild projects with explicit `<Compile Include>` items, so every added .cs file requires an entry. Insert each new entry **adjacent to the existing entry for a neighbouring file in the same folder**, not appended at the end of the `ItemGroup`, to minimise the chance of a conflicting insertion against a concurrently-prepared sibling item.

- `UtilitiesCS/UtilitiesCS.csproj` — 1 new entry, for the new `DfDeedle` partial.
- `TaskMaster/TaskMaster.csproj` — 1 new entry, for the new ribbon boundary class.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — 2 new entries.
- `TaskMaster.Test/TaskMaster.Test.csproj` — 1 new entry.
- `QuickFiler.Test/QuickFiler.Test.csproj` — 1 new entry.


## Test Strategy

### Deterministic time

`TimeProvider` and `FakeTimeProvider` are already available to the affected `net481` test projects; the issue's open question about deterministic time is resolved. Production already threads `TimeProvider?` through the bound `TimeoutAfter` overload, and an existing test class in the UtilitiesCS test assembly already drives `TimeoutAfter` from a frozen clock for exactly this reason. Threading a `TimeProvider?` into the column-add method plus the injected `Action<object, object>` adder drives the entire timeout path with **zero wall-clock wait and no COM object**.

### Regression tests to add or update

Five test files, listed with their backticked paths under `## Write Set`:

1. **DfDeedleQfcColumnTimeoutTests** (new, UtilitiesCS test assembly) — AC1 and AC2. Uses a `FakeTimeProvider` and a never-returning injected adder. Asserts: the adder is invoked **exactly once** across all three deadlines (the non-overlap clause); the method throws after the third deadline rather than returning; the exception message names the folder and the step; the elapsed budget advances by 3 x 3000 ms of fake time. Carries `[DoNotParallelize]`.
2. **DfDeedleRequiredColumnValidationTests** (new, UtilitiesCS test assembly) — AC3. Five negative cases, one per required key removed in turn; one positive case with all five present; one message-content assertion confirming the missing column names and the folder name both appear. Ordinal comparison is pinned by a case-variant negative case.
3. **DfDeedle_COM_Tests** (existing, UtilitiesCS test assembly) — repair. The two reflection invocations pass a four-element `object[]` and break against the six-parameter method; replace the reflection helper with direct `internal` calls.
4. **RibbonCommandBoundaryTests** (new, TaskMaster test assembly) — AC5. Pass-through on success; log-once and present-once on failure; no propagation to the caller; containment when the presentation sink itself throws; plus a reflection shape pin mirroring the existing `AssertAwaitedAsyncVoidShape` helper, asserting that the three named handlers are still `async void` methods whose bodies await the boundary. The TaskMaster test assembly does **not** reference the QuickFiler assembly, so this test must not depend on any QuickFiler type.
5. **QfcDatamodelRethrowTests** (new, QuickFiler test assembly) — AC4. Construct the datamodel with `FormatterServices.GetUninitializedObject`, set `_globals` so that `Ol.NamespaceMAPI.Offline == true` and `ToggleOfflineMode` short-circuits without touching `CommandBars`, set `Token` and `TokenSource` through the public setters, reflection-invoke `GetEmailsInViewDfAsync`, and assert the observed `StackTrace` still contains the originating frame. Because of Correction 2 the observed exception is still an `AggregateException`; the assertion must therefore inspect the wrapped inner exception's stack, and must not assume unwrapping.

### Unit tests for the fixed behavior and boundaries

Covered by items 1, 2, 4 and 5 above. All use MSTest, Moq where a mock is needed, and FluentAssertions.

### Edge cases and negative scenarios

- Column add completes on the first, second and third deadline (three positive variants) — the method returns normally and starts no second task in any of them.
- Cancellation requested mid-loop — the loop exits without throwing the timeout-exhausted exception, and cancellation is not converted into a user dialog.
- Column dictionary missing each required key in turn, and missing more than one key at once (the message must name all of them).
- Column dictionary containing a case-variant key such as `Entryid` — must still be reported missing.
- Presentation sink throws — the boundary contains it and does not propagate.
- Action succeeds — neither sink is invoked.

### Error handling and logging verification

AC2 is asserted by attaching a log4net `MemoryAppender` and matching the `[Df timing]` lines for the property enumeration and each of the six `Columns` calls. **Phase 0 must first confirm that an appender attached from the UtilitiesCS test assembly captures output from the `UtilitiesCS.DfDeedle` logger; this is unverified by execution.** No equivalent `MemoryAppender` helper exists in that test assembly, so a small helper of roughly 18 lines is required, modelled on the existing one in the TaskMaster test assembly. If the Phase 0 check fails, assert AC2 indirectly through the injected adder and record the fallback.

### Coverage impact and targets for changed lines/modules

- The new column partial, the validator and the ribbon boundary class are new modules and must reach **>= 90% line coverage** each, per the repository's new-module target.
- No changed line may lose coverage relative to the merge base.
- The ribbon boundary logic must live in `RibbonCommandBoundary`, which is **not** marked `[ExcludeFromCodeCoverage]`. `RibbonViewer` carries that attribute and its handlers are `async void`, so any behaviour placed directly in them is neither measurable nor observable. Presentation of the dialog stays in the coverage-exempt shim; the decision logic does not.
- No repository-wide coverage figure is asserted as a blocking gate here: no merge-base baseline has been captured in this feature folder, and the repository floor applies to the testable denominator after the COM/VSTO/WinForms exemptions. The repository figure is a record-and-report obligation in the final QA evidence, and the change must not lower it.

### Toolchain commands to run (format -> lint -> type-check -> test)

Run in this order, restarting from the top if any step fails or changes a file:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

The Rebuild target is required in steps 2 and 3: a warm Build target skips `CoreCompile` and the gate cannot fail. Step 2 is also the gate that would reject a `Task.Delay`-based timeout.

### Manual validation steps

AC6 is manual. Launch QuickFiler on the "T&E" folder and confirm either a successful launch or the AC1/AC3 error dialog naming the folder and the step, with no unhandled Outlook exception. Launch on Inbox as a regression check. In both cases confirm the debug log contains the per-step column-add timing lines.


## Acceptance Criteria

AC1 through AC6 are reproduced verbatim from issue.md as settled with the maintainer on 2026-09-06. They are authoritative and must not be renumbered, reworded, merged, split, or weakened. AC7 through AC14 are supplementary criteria added by this spec; they constrain how AC1 through AC6 are delivered and do not replace any of them.

- [ ] AC1: `AddQfcColumnsAsync` fails loudly after its final timeout (throws a descriptive exception naming the folder and the step) instead of returning normally; the underlying task is cancelled or the retry waits for it, so concurrent COM calls against the same table do not overlap.
- [ ] AC2: The column-add step logs timing for `HasUserDefinedProperty`, each `Columns.Add`, and each `Columns.Remove`, using the existing `[Table timing]` / `[Df timing]` pattern.
- [ ] AC3: `Email2dToRecords` (or its caller) validates the presence of every required column before indexing and throws an exception whose message names the missing column(s) and the folder.
- [ ] AC4: `QfcDatamodel.GetEmailsInViewDfAsync` rethrows with `throw;` so the original stack is preserved.
- [ ] AC5: `RibbonViewer.QuickFiler_Click`, `QuickFilerHighConfidence_Click`, and `SortEmail_Click` catch exceptions at the boundary, log them with full detail, and show an error dialog; Outlook does not surface an unhandled exception.
- [ ] AC6: Launching QuickFiler on the "T&E" folder either succeeds or shows the AC1/AC3 error message (manual verification).

Supplementary criteria (added by this spec):

- [ ] AC7: The delivered implementation matches the five-step trace under Proposed Fix. Specifically, the column-add work is started exactly once per call and the timeout is re-applied to that same task instance; a test asserts the injected adder is invoked exactly once across all three deadlines.
- [ ] AC8: The timeout is implemented by re-applying the existing `TimeoutAfter(Task, int, TimeProvider?)` overload. The four existing `TimeoutAfter` overloads are unchanged, no new overload is added, and no `Task.Delay` or `Thread.Sleep` call is introduced; the analyzer step of the toolchain passes.
- [ ] AC9: The AC3 validator is called from both the asynchronous and the synchronous data-frame entry points, so the duplicate unchecked column indexing on the synchronous path is closed. `Email2dToRecords`, `Email2dArrayToDf` and `GetEmailDataFromTable` keep their current parameter lists, and the fixed-arity reflection tests that pin them continue to pass unmodified.
- [ ] AC10: AC4 is applied at all three `throw e;` sites in the `QfcDatamodel` partial family. The occurrence in `QfcQueue` is a different type and is left unchanged; the commented-out occurrence in the helper class is left unchanged.
- [ ] AC11: AC5 changes exactly the three `async void` handlers it names. The one already-guarded `async void` member in the same file and the remaining 20 out-of-scope `async void` members in that file are unchanged, as are all `async void` members in the sibling ribbon partial. The boundary logic lives in a type that is not marked `[ExcludeFromCodeCoverage]`, and the error dialog renders inner exception detail so an `AggregateException` does not present as "One or more errors occurred." alone.
- [ ] AC12: The `catch (OperationCanceledException)` in `QfcHomeController.LaunchAsync` is neither widened nor removed, and no other existing catch is broadened to `System.Exception` in place of delivering AC1 or AC3.
- [ ] AC13: The diff touches only the files enumerated under `## Write Set` (`production_file_count = 6`, `test_file_count = 5`, plus 5 project compile-entry files). Every new .cs file has a `<Compile Include>` entry in its project. Every file this change creates, and every modified file that was at or under 500 lines at base commit c431dc32, is at or under 500 lines after the change. One modified file already exceeded the cap before this change: `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, at 882 lines at base. It must be modified because widening the column-add seam invalidates its two fixed-arity reflection invocations. Its line count must strictly decrease, and it is not required to reach 500 in this change: splitting an 882-line pre-existing test file is an opportunistic refactor, which the bugfix workflow in CLAUDE.md prohibits ("change only what is needed... If you uncover deeper design problems, open a new issue instead of widening scope"). The pre-existing violation is recorded as a follow-up promotion, listed under Rollout & Follow-up.
- [ ] AC14: A full toolchain pass completes in the documented order with no errors: `dotnet tool run csharpier check .`, the analyzer build, the nullable build, and the test run. The Phase 0 log-capture check is recorded with its outcome, and if it failed, the AC2 fallback assertion strategy is documented in the plan.


## Risks & Mitigations

1. **Highest risk — AC1 converts a silent degradation into a hard, reproducible launch failure on a slow folder.** Today the crash occurs only when a missing column is later indexed. After AC1, a launch fails deterministically at the column-add step on any folder whose column add exceeds the 9-second budget. If "T&E" is slow for a persistent environmental reason — a large `UserDefinedProperties` collection, a throttled connection — AC1 turns an intermittent crash into a reproducible inability to open QuickFiler on that folder. AC6 accepts this trade, but only the AC2 timing logs make the difference diagnosable. **Mitigation: AC2 must land in the same change as AC1, not in a follow-up.** The plan must not sequence AC2 after AC1 in a way that allows AC1 to ship alone.
2. **`throw;` alone does not unwrap the `AggregateException`.** Wrapping happens upstream, inside the timeout helper's result marshalling. Mitigation: the AC5 presentation sink renders inner exceptions, and the AC4 test asserts against the wrapped inner exception's stack rather than assuming unwrapping. Pinned by AC11.
3. **The signature change breaks two existing reflection tests.** The four-element `object[]` invocations throw `TargetParameterCountException` against a six-parameter method. Mitigation: an explicit plan task replaces the reflection helper with direct `internal` calls. Pinned as a named write-set entry so it cannot be discovered late.
4. **Class-level parallelization in the UtilitiesCS test assembly.** New classes touching process-wide state need `[DoNotParallelize]`. Mitigation: the new timeout test class carries it; the new validator test class is pure and does not need it. The pre-existing unguarded shared-static mutation in the existing COM test class is a latent flake and is a named non-goal.
5. **Unverified cross-assembly log capture.** Whether a `MemoryAppender` attached from the UtilitiesCS test assembly captures `UtilitiesCS.DfDeedle` output is not established by execution. Mitigation: Phase 0 check with a documented fallback (assert AC2 indirectly through the injected adder). Pinned by AC14.
6. **Concurrent sibling items.** Three sibling items are being prepared in parallel: one rewrites the QuickFiler folder drop-down and breadcrumb selector, one rewrites archive-root path projection across the UtilitiesCS folder classes, one rewrites the store settings serializer. Overlap analysis: an exhaustive search of the three UtilitiesCS files in this write set for `RelativePath`, `ArchiveRoot`, `OlRoot`, `FolderPath`, `ToArchiveRelative`, `Serializ` and `JsonConverter` returned **no matches**, so siblings 2 and 3 have no source-level reason to touch them. The one likely collision is the UtilitiesCS project file, where sibling 2 may add or move file entries. Mitigation: insert each `<Compile Include>` adjacent to the existing entry for a neighbouring file in the same folder rather than at the end of the `ItemGroup`, so two insertions land in different places. The QuickFiler datamodel edits are one-token changes and trivially re-appliable. The ribbon edits are confined to three handler bodies plus one field and one sink, with no reordering or reformatting, and no sibling is ribbon work.
7. **Scope creep into the timeout helper.** That file is 1011 lines and already over the 500-line cap; editing it would force an unrelated cap remediation. Mitigation: the design reuses the existing overload unchanged, and the file is an explicit non-goal.


## Rollout & Follow-up

- **Release/rollout steps.** Single branch, single PR onto main. Full toolchain pass before review. AC6 manual verification by the maintainer on the reproduction folder and on Inbox after the debug build is loaded.
- **Post-fix monitoring.** After the first launch on the reproduction folder, read the new `[Df timing]` lines to attribute the 9-second delay to either the `folder.UserDefinedProperties` enumeration or a specific `Columns.Add`/`Columns.Remove` call. If a single COM call is responsible, open a follow-up issue for that call specifically; the 9000 ms budget may then be reconsidered on evidence rather than on assumption.
- **Follow-up issues to promote (all three non-goals of this change).** (1) The unreachable `catch (TimeoutException)` in the two `repeatAttempts` `TimeoutAfter` overloads. (2) The unguarded shared-static `MessageBoxInvoker` mutation in the existing COM test class. (3) The pre-existing 500-line-cap violation in the existing COM test class, which stands at 882 lines at base commit c431dc32 and is only reduced, not brought under the cap, by this change; see AC13. Promote each through the potential-to-issue lifecycle so none is lost when this feature folder is archived.
- **Links.** Issue: https://github.com/drmoisan/TaskMaster/issues/798. Research record: research/2026-09-07T02-01-quickfiler-crash-column-add-timeout-swallowed-keynotfound-research.md. Related ratified pattern: issue #503 (`EngineGatedCommandRunner`).
