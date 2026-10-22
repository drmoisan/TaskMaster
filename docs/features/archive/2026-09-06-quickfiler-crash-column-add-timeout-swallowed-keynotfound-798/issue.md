# quickfiler-crash-column-add-timeout-swallowed-keynotfound (Issue #798)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-crash-column-add-timeout-swallowed-keynotfound/ (Issue #798)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #798
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/798
- Last Updated: 2026-09-07
- Work Mode: full-bug

## Summary

Launching QuickFiler from the ribbon on a folder named "T&E" crashed Outlook with an unhandled `AggregateException` wrapping `KeyNotFoundException: The given key was not present in the dictionary`. The column-add step that prepares the Outlook `Table` timed out three times, the timeouts were swallowed, and the ETL proceeded with Outlook's five default columns. The row builder then indexed the column dictionary by the missing key `SentOn`. The exception was rethrown with `throw e`, wrapped by the timeout helper, and escaped an `async void` ribbon handler with no boundary catch.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in, debug build loaded from `TaskMaster\bin\Debug`, HEAD `c431dc32` (2026-09-06)
- Command/flags used: Outlook ribbon -> QuickFiler (`RibbonViewer.QuickFiler_Click` -> `RibbonController.LoadQuickFilerAsync` -> `QfcHomeController.LaunchAsync`)
- Data source or fixture: live Exchange mailbox, Explorer showing folder "T&E" (225 rows). Earlier launches on Inbox (3998 rows) in the same session succeeded.

## Steps to Reproduce

1. In Outlook, select the folder "T&E" (or another folder on which adding the QuickFiler table columns takes more than 3 seconds).
2. Click QuickFiler on the ribbon.
3. Observe: no dialog appears; after roughly 9 seconds Outlook reports an unhandled `AggregateException` from `QfcDatamodel.GetEmailsInViewDfAsync`.

Not reproducible on demand on Inbox. The maintainer confirmed no message box was shown before the crash.

## Expected Behavior

- If the required table columns cannot be added within the timeout, the launch fails with a logged, user-visible error naming the folder and the step, and QuickFiler does not proceed to build the data frame.
- The row builder validates that every required column (`EntryID`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`) is present before indexing, and throws a descriptive exception naming the missing column and the folder.
- Exceptions are rethrown with `throw;` so the original stack is preserved.
- Ribbon `async void` handlers catch exceptions at the boundary, log them, and show an error dialog instead of crashing Outlook.
- The column-add step logs its timing so a slow COM call is diagnosable.

## Actual Behavior

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

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (`TaskMaster\bin\Debug\logs\debug_2026-09-06.log`, storeId redacted). Note the 9.085-second gap between table acquisition and ETL start, and `columnCount=5` at ETL:

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

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

Outlook-level unhandled exception on a normal ribbon action. The failure is folder-dependent and gives the user no diagnostic.

## Suspected Cause / Notes

Verified chain (code read plus the log timing above):

1. `DfDeedle.GetEmailDataInViewAsync` (`UtilitiesCS\Extensions\DfDeedle.cs:134-201`) calls `AddQfcColumnsAsync(table, currentFolder, token, 0)` at `:164`.
2. `AddQfcColumnsAsync` (`:318-343`) runs `AddQfcColumns` inside `Task.Run(...).TimeoutAfter(3000)`. On `TimeoutException` it recurses while `counter < 2`, then returns normally. Three attempts of 3000 ms equal the 9-second gap in the log. After the third timeout the method returns without any log line and without signalling failure. `TimeoutAfter` (`UtilitiesCS\Threading\TimeOutTask.cs:924-940` and the single-timeout overload) does not cancel the underlying task, so up to three `AddQfcColumns` calls can run concurrently against the same COM `Table` from pool threads.
3. `AddQfcColumns` (`:296-316`) is what adds `SentOn`, `ConversationId` (schema), `Triage` (schema) and removes `Subject`, `CreationTime`, `LastModificationTime`. When it has not completed, the table keeps Outlook's default five columns (`EntryID`, `Subject`, `CreationTime`, `LastModificationTime`, `MessageClass`), which matches `columnCount=5` and the absence of `SentOn`.
4. `table.EtlAsync` (`UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs:66-130`) builds `columnInfo` from `GetColumnDictionary` (`OlTableExtensions.cs:200-230`), mapping schema names to friendly names via `MAPIFields.SchemaToField`.
5. `Email2dToRecords` (`DfDeedle.cs:214-240`) indexes `columnInfo["EntryID"]`, `["MessageClass"]`, `["SentOn"]`, `["ConversationId"]`, `["Triage"]` with no presence check. `SentOn` is the first missing key in access order.
6. `GetEmailsInViewDfAsync` (`QuickFiler\Controllers\QfcDatamodel.FrameBuilding.cs:102-109`) catches, logs only the message and stack, and rethrows with `throw e;`, discarding the original stack.
7. `QfcHomeController.LaunchAsync` (`QuickFiler\Controllers\QfcHomeController.cs:58-81`) catches only `OperationCanceledException`. `RibbonViewer.QuickFiler_Click` (`TaskMaster\Ribbon\RibbonViewer.cs:148-151`) and the sibling handlers at `:153-159` are `async void` with no try/catch, so the exception reaches the thread pool and Outlook reports it.

Why the column add exceeded 9 seconds on "T&E" is not recorded. The maintainer saw no dialog, which rules out the `MessageBoxInvoker` prompts in `EnsureTriageColumnExists` (`:345-390`) blocking on input. Remaining candidates are the `folder.UserDefinedProperties` enumeration in `HasUserDefinedProperty` (`:392-408`) and the `table.Columns.Add/Remove` COM calls themselves, possibly aggravated by the overlapping retries. The fix must add timing logs around each so the next occurrence is attributable.

The draft analysis produced by another assistant proposed that a missing `Triage` user property or a non-mail folder view was the trigger. The log evidence above shows the trigger was the swallowed triple timeout; the missing-property path throws `InvalidOperationException` at `:307`, not `KeyNotFoundException`.

## Proposed Fix / Validation Ideas

Acceptance criteria settled with the maintainer on 2026-09-06:

- [ ] AC1: `AddQfcColumnsAsync` fails loudly after its final timeout (throws a descriptive exception naming the folder and the step) instead of returning normally; the underlying task is cancelled or the retry waits for it, so concurrent COM calls against the same table do not overlap.
- [ ] AC2: The column-add step logs timing for `HasUserDefinedProperty`, each `Columns.Add`, and each `Columns.Remove`, using the existing `[Table timing]` / `[Df timing]` pattern.
- [ ] AC3: `Email2dToRecords` (or its caller) validates the presence of every required column before indexing and throws an exception whose message names the missing column(s) and the folder.
- [ ] AC4: `QfcDatamodel.GetEmailsInViewDfAsync` rethrows with `throw;` so the original stack is preserved.
- [ ] AC5: `RibbonViewer.QuickFiler_Click`, `QuickFilerHighConfidence_Click`, and `SortEmail_Click` catch exceptions at the boundary, log them with full detail, and show an error dialog; Outlook does not surface an unhandled exception.
- [ ] AC6: Launching QuickFiler on the "T&E" folder either succeeds or shows the AC1/AC3 error message (manual verification).

Validation:

- [ ] Unit coverage areas: `AddQfcColumnsAsync` timeout path with `FakeTimeProvider` and a stubbed column-add delegate; required-column validation with a dictionary missing each key in turn; `throw;` preservation (assert `InnerException` stack contains the origin frame); ribbon boundary catch via an injected controller that throws.
- [ ] Integration scenario to retest: QuickFiler launch on Inbox (regression) and on a folder where the column add is slow.
- [ ] Manual verification notes: confirm the log contains per-step timing lines for the column add on every launch.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
