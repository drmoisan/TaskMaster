# QuickFiler High Confidence — "EntryId does not exist in the column index" Root-Cause Diagnosis

- Date: 2026-07-06
- Scope: one-off diagnostic research (no active feature folder)
- Defect: `System.Exception: The interface member 'EntryId' does not exist in the column index` thrown from Deedle at `QfcDatamodel.InitEmailQueue` line 225 when launching QuickFiler High Confidence.

## Summary

The crash is a zero-batch slicing defect, not a column-construction or naming defect. In High Confidence mode `QfcHomeController.RunAsync` sets the initialization batch size to `0` (`QfcHomeController.cs:281`) and passes it down to `QfcDatamodel.InitEmailQueue`. `InitEmailQueue` clamps `batchSize` to `0`, then unconditionally builds `firstIteration = _frame.GetRowsAt(Enumerable.Range(0, 0))` (`QfcDatamodel.cs:216-217`) and calls `firstIteration.GetRowsAs<IEmailSortInfo>()` (`QfcDatamodel.cs:225`). Slicing a frame to an empty ordinal set yields a Deedle frame whose column index is empty (Deedle reconstructs the sliced frame from rows, so zero rows produces zero columns), and `GetRowsAs<IEmailSortInfo>()` validates its interface members against that now-empty column index and throws on the first member, `EntryId`. The underlying data frame `_frame` is well-formed and does contain `EntryId`; the failure is entirely in projecting an intentionally empty batch.

## Evidence

### Call path and the zero-batch trigger (confirmed)

- `QfcHomeController.RunAsync` computes `int initializationBatchSize = highConfidenceModeEnabled ? 0 : itemsPerIteration;` (`QuickFiler/Controllers/QfcHomeController.cs:281`) and passes it as the first argument to `_datamodel.InitEmailQueueAsync(initializationBatchSize, ...)` (`QfcHomeController.cs:284`). In High Confidence mode the batch size is deterministically `0`.
- `InitEmailQueueAsync` forwards to the synchronous `InitEmailQueue(batchSize, worker)` via `Task.Run` (`QuickFiler/Controllers/QfcDatamodel.cs:253`).
- `InitEmailQueue` (`QfcDatamodel.cs:211-238`):
  - Line 216: `batchSize = batchSize < _frame.RowCount ? batchSize : _frame.RowCount;` — with input `0` and a non-empty `_frame`, this stays `0`.
  - Line 217: `var firstIteration = _frame.GetRowsAt(Enumerable.Range(0, batchSize).ToArray());` — with `batchSize == 0` this is `_frame.GetRowsAt(new int[0])`, an empty ordinal selection.
  - Line 225: `var rows = firstIteration.GetRowsAs<IEmailSortInfo>().Values.ToArray();` — the crashing frame.
- The design intent for High Confidence is to load nothing in the first batch and stream all items afterward via `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` (`QfcHomeController.cs:292-295`), which pulls from `_masterQueue` filled by the background worker (`LoadRemainingEmailsToQueueAsync`, `QfcDatamodel.cs:258+`). So `batchSize == 0` is intended; the missing empty-batch guard is the defect.

### Hypothesis 1 — Empty/zero-row slice produces an empty column index (CONFIRMED, as the batchSize=0 variant)

`GetRowsAt(Enumerable.Range(0, 0))` is the exact operation that fails. Deedle's positional row slicing reconstructs the result frame from the selected rows; an empty selection is equivalent to `Frame.FromRows(<empty>)`, which produces a frame with an empty column index (no row supplies column keys to union). `GetRowsAs<TRow>()` then resolves each interface member of `IEmailSortInfo` against the frame column index and throws `"The interface member 'EntryId' does not exist in the column index"` on the first member because the index is empty.

Corroborating repo-internal evidence that naive empty Deedle frames lose their column index:
- `DfDeedle.FromArray2D` special-cases the zero-row input (`UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs:51-60`) by explicitly building the empty frame with `Frame.FromColumns` over empty `SeriesBuilder<int>` series, precisely so the column keys survive with zero rows.
- The existing test `FromArray2D_EmptyData_ReturnsFrameWithColumnsButNoRows` (`UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs:206-225`) asserts that this special construction yields `ColumnKeys == {EntryID, MessageClass, SentOn}` with `RowCount == 0`. The need for that explicit `FromColumns` path is direct evidence that the default row-based empty construction does NOT preserve columns — which is exactly what `GetRowsAt(empty)` hits.
- `LoadRemainingEmailsToQueueAsync` (`QfcDatamodel.cs:258-264`) already guards `if ((_frame is null) || (_frame.RowCount == 0)) { ... return false; }` before its own `GetRowsAs<IEmailSortInfo>()` call (`QfcDatamodel.cs:267`). The sibling method that also calls `GetRowsAs` has the empty guard; `InitEmailQueue` lacks the equivalent guard for its empty slice.

Note on the alternate variant of Hypothesis 1 (IPM.Note filter or `MostRecentByConversation` yielding zero rows in `_frame` itself): this variant is not the observed failure. If `_frame` were empty because of an upstream zero-row condition, `SortTriageDate` (`QfcDatamodel.FrameBuilding.cs:112-132`) would throw earlier at `dfClone.GetColumn<DateTime>("SentOn")` (line 118) during `InitDfAsync`, not at `InitEmailQueue` line 225. The reported stack terminates at line 225, which is only reachable when `_frame` is well-formed and the empty selection is introduced by the `batchSize == 0` slice.

### Hypothesis 2 — `EntryId` column dropped due to all-missing values / object-vs-string typing (ELIMINATED as trigger)

- The production High Confidence path is async: `GetEmailDataInViewAsync` -> `Email2dArrayToDf` -> `Email2dToRecords`, which builds the `EmailRecord` struct with `EntryId = (string)data[i, columnInfo["EntryID"]]` (`DfDeedle.cs:222`, struct at `:235-262`). `Frame.FromRecords` types columns from the record's declared members, so the `EntryId` column exists even if some values are null.
- The `object`-typed variant (`GetEmailDataFromTable`, `DfDeedle.cs:118`, `EntryId = data[i, columnInfo["EntryID"]]` uncast) is used only by the synchronous `GetEmailDataInView`/`InitDf` constructor path (`QfcDatamodel.cs:47`), which is not the High Confidence launch path.
- Decisive elimination: standard (non-High-Confidence) QuickFiler passes `batchSize == itemsPerIteration > 0` through the same `InitEmailQueue` -> `GetRowsAs<IEmailSortInfo>()` code and works. If the `EntryId` column were dropped during frame construction, standard mode would fail identically. It does not, so `_frame` demonstrably contains `EntryId`.

### Hypothesis 3 — `Frame.FromRows` in `MostRecentByConversation` losing/renaming columns (ELIMINATED as trigger)

- `MostRecentByConversation` (`QfcDatamodel.FrameBuilding.cs:134-152`) unions each row's keys via `Frame.FromRows(rows)` (line 150); each row is a full `Rows.FirstValue()` series carrying all six keys, so `EntryId` is preserved.
- This transform runs during `InitDfAsync` for both modes. Standard mode works, proving the post-transform `_frame` retains `EntryId`. Not the differentiator.

### Hypothesis 4 — `SortTriageDate` altering the column index (ELIMINATED as trigger)

- `SortTriageDate` (`QfcDatamodel.FrameBuilding.cs:112-132`) performs `Clone`, `AddColumn("NewKey")`, `SortRows`, `IndexRowsWith`, `SortRowsByKey`, `DropColumn("NewKey")`. It only adds and then drops `NewKey`; it never touches `EntryId`. It runs in both modes; standard mode works. Not the differentiator.

### Hypothesis 5 — `EntryId` vs `EntryID` casing mismatch (ELIMINATED)

- The mixed casing exists only where the raw 2-D array is indexed: dictionary key `columnInfo["EntryID"]` (`DfDeedle.cs:118, 222`). The frame column name derives from the record member `EntryId` (anonymous record at `DfDeedle.cs:118`; `EmailRecord.EntryId` at `DfDeedle.cs:256`), matching the interface member `IEmailSortInfo.EntryId` (`QuickFiler/Controllers/EmailSorter.cs:78`). No `EntryID`-cased key ever becomes a frame column. Standard mode working confirms consistent casing in the live frame.

### Existing test coverage (gap confirmed)

- `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs` covers `AcceptableTriage`, `DateFrom2dPosition`, `GetFirstNonNull`, `GetColumnEid`, `FromArray2D` (including the empty-data column-preservation case), and `Email2dArrayToDf` — all frame-construction concerns. None exercises the empty-slice/zero-batch projection.
- `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` covers `QfcRemainingQueueAdmission`, `DequeueNextItemGroupAsync` waiting behavior, and the injected time/delay seams. No test exercises `InitEmailQueue` with `batchSize == 0`, and none asserts the frame-slicing contract used at line 217/225.
- Net: the zero-batch first-iteration slice is entirely uncovered, which is why the defect reached runtime.

## Confirmed Root Cause

`QfcDatamodel.InitEmailQueue` (`QuickFiler/Controllers/QfcDatamodel.cs:211-238`) does not handle a `batchSize` of `0`. High Confidence mode deterministically supplies `0` (`QfcHomeController.cs:281`). With `batchSize == 0`, line 217 evaluates `_frame.GetRowsAt(new int[0])`, producing a Deedle frame with an empty column index, and line 225 `firstIteration.GetRowsAs<IEmailSortInfo>()` throws `"The interface member 'EntryId' does not exist in the column index"` when validating the first interface member against that empty index. The exact failing condition is `batchSize == 0` reaching the unconditional slice-and-project block at lines 217-225.

## Reproduction Strategy

Deterministic, Outlook-free, red-before-green. Two complementary layers; the second is the primary production-facing regression test and depends on the recommended seam extraction.

Target project/class: `QuickFiler.Test` (the `IEmailSortInfo` interface is public in `QuickFiler.Controllers`, and `QuickFiler` already exposes internals via `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, `QuickFiler/Properties/AssemblyInfo.cs:5`). Add a new class, for example `QfcEmailBatchSliceTests`, under `QuickFiler.Test/Controllers/`.

Layer A — pin the Deedle behavior (proves the root cause exists today):
- Arrange: build a valid `Frame<int, string>` in memory with the six columns via `Frame.FromRecords` of anonymous records carrying `EntryId, MessageClass, SentOn, ConversationId, Triage, StoreId` (two or three rows). No COM, no Outlook.
- Act/Assert (red today): `frame.GetRowsAt(System.Array.Empty<int>()).Invoking(f => f.GetRowsAs<IEmailSortInfo>()).Should().Throw<System.Exception>()` reproduces the missing-column failure, demonstrating the empty-slice behavior. Also assert the full frame projects cleanly: `frame.GetRowsAt(new[] { 0 }).GetRowsAs<IEmailSortInfo>().ValueCount` is positive.

Layer B — production regression on the guarded seam (turns red before the fix, green after):
- Extract the batch selection/projection into an internal, COM-free helper (see Recommended Fix), e.g. `internal static IReadOnlyList<IEmailSortInfo> SelectFirstBatchRows(Frame<int, string> frame, int batchSize)`.
- Test 1 (the regression): `SelectFirstBatchRows(validFrame, 0)` must return an empty collection without throwing. Before the guard is added this test is red (it throws the missing-column exception); after the guard it is green.
- Test 2 (no behavior change for the normal path): `SelectFirstBatchRows(validFrame, 2)` returns two rows whose `EntryId`/`StoreId` match the fixture, proving the non-zero path is unchanged.
- Test 3 (clamp): `SelectFirstBatchRows(validFrame, 999)` clamps to `frame.RowCount` and returns all rows.

All fixtures are hand-built `Frame<int, string>` instances (or built through the existing internal `DfDeedle.GetEmailDataFromTable` from an in-memory `object[,]` + `columnInfo`, reachable through UtilitiesCS internals if a cross-assembly builder is preferred). No live `Outlook.Table`, `Explorer`, or `MailItem` is required; `GetItemFromID` is never reached for the zero-batch case because the projected row set is empty.

## Recommended Fix

Minimal, fail-fast, no broad refactor. Guard the empty batch before the frame slice/projection and extract the pure slice-and-project step into a testable internal helper so the guard is covered without COM.

Direction:
- In `QfcDatamodel.InitEmailQueue` (`QuickFiler/Controllers/QfcDatamodel.cs`), after clamping `batchSize` (line 216), short-circuit when `batchSize == 0`: skip the `GetRowsAt`/`GetRowsAs` block (lines 217-225) and the email-list projection, set `emailList` to an empty list, and preserve the existing `SetupWorker(worker); worker.RunWorkerAsync();` tail (lines 234-235) so High Confidence streaming still starts. Leaving `_frame` unchanged for a zero batch is correct (dropping zero rows is a no-op).
- Move the "take the first `batchSize` rows and project to `IEmailSortInfo[]`" logic into an internal static helper (a new method on the existing partial, for example in `QfcDatamodel.FrameBuilding.cs`) that takes `Frame<int, string>` and `int batchSize` and returns an empty result for `batchSize <= 0`. This keeps the COM-free logic unit-testable while `InitEmailQueue` retains only the thin COM wiring (`GetItemFromID`). This mirrors the sibling guard already present in `LoadRemainingEmailsToQueueAsync` (`QfcDatamodel.cs:260`).

Production files changed and count estimate: 1-2 files, both within the `QuickFiler` project.
1. `QuickFiler/Controllers/QfcDatamodel.cs` — add the zero-batch guard in `InitEmailQueue`.
2. (Optional, recommended for coverage) `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` — host the extracted internal `SelectFirstBatchRows` helper.

This fits the small-path budget of 1-3 production files. No change is required in `QfcHomeController` (`batchSize == 0` is a valid, intended input) or in the `DfDeedle` frame builders. If the team prefers the absolute minimum single-file change, option 1 alone fixes the crash; option 2 is the small addition that makes the guard directly testable and is consistent with the repository's preference for extracting logic out of `[ExcludeFromCodeCoverage]` COM-bound classes.

Policy note: `QfcDatamodel` carries `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:24`). The extracted helper should remain COM-free so it is measured and covered; keep the exclusion only on the thin COM entry method. This aligns with General Unit Test Policy and the C# coverage-uplift direction (extract testable seams out of host-bound classes).

## Automation Feasibility

Fully automatable with no human interaction. The failing operation is a pure Deedle transform (`GetRowsAt` over an empty ordinal set followed by `GetRowsAs<IEmailSortInfo>()`) reproducible from an in-memory `Frame<int, string>` or an `object[,]` + `columnInfo` fixture. Diagnosis required only static code reading; reproduction and fix require only MSTest + Deedle + FluentAssertions already present in the solution. No live Outlook process, COM object, or manual step is needed to reproduce the crash, verify the red test, apply the guard, or confirm green. The C# toolchain loop (csharpier -> analyzer build -> nullable build -> vstest) runs unattended.
