# Cross-Feature Notes Handoff

Timestamp: 2026-08-26T11-32
Task: [P7-T5]
Command: not applicable; this artifact records the handoff of three cross-feature notes
EXIT_CODE: 0

Three cross-feature notes are directed to sibling epic children. **None of the three is fixed in
this feature's diff.** Each is real, evidenced, and excluded from scope because fixing it requires
writing a file this feature does not own.

## CFN-1 — directed to feature 446

**Title.** `SwapStopWatch()` races the metrics write on the `MoveAndIterate` path.

**File and line reference.** `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:157` versus
`:161` leading to `:142`.

**Defect.** `BackGroundMoveAsync()` is started at `:157` without being awaited until `:175`, and
`LoadUiFromQueue()` performs the stopwatch swap at `:142`. The swap and the metrics write are
concurrent and unordered, so neither `_stopWatch` nor `_stopWatchMoved` is deterministically correct
on that path. This is root cause RC-4.

**Recommended remedy, as stated in the spec.** Relocate `_parent.SwapStopWatch()` out of
`LoadUiFromQueue()` at `:142` to immediately after `_groups.CacheMoveObjects()` at `:156`,
mirroring the end-of-database ordering at `:190-191`. That single relocation makes both branches
identical and removes the race.

**Not fixed here.** `QfcFormController.EventHandlers.cs` is on this feature's forbidden-to-write
list. Three owned-file workarounds were evaluated in the spec and each fails: a property-setter
snapshot does not change when the capture happens and breaks four reflection-based tests; having
`WriteMetricsAsync` call `SwapStopWatch()` itself converts one race into two; and capturing at
`CacheMoveObjects()` time requires two forbidden files.

**Partial improvement this feature does deliver.** [P4-T7] changed the duration read to
`_stopWatchMoved`, which makes the end-of-database path deterministically correct. On the
`MoveAndIterate` path the race remains, but its two outcomes improve from "correct interval or
zero" to "current interval or the previous batch's interval", both real durations of the right
order of magnitude.

## CFN-2 — directed to feature 468

**Title.** `GetMoveDiagnostics` returns an array one element longer than it fills.

**File and line reference.** `QuickFiler/Controllers/QfcCollectionController.cs:2284` allocates
`new string[_itemGroupsToMove.Count + 1]`; the loop at `:2286-2325` fills only indices
`0..Count-1`, so the final element is always `null`.

**Consequence.** `FileIO2.WriteTextFileAsync` calls `sw.WriteLineAsync(null)` at `FileIO2.cs:72`,
appending a blank line to the CSV on every write. This is invisible today only because nothing is
ever written; fixing #442 makes it manifest.

**Recommended remedy, as stated in the spec.** Size the array `_itemGroupsToMove.Count`, not
`+ 1`.

**Not fixed here.** `QfcCollectionController.cs` is on this feature's forbidden-to-write list.

**Owned-file mitigation this feature does deliver.** [P5-T8] filters `null` and whitespace-only
entries in `WriteMetricsAsync` before handing the array to the writer seam, asserted by
`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`. The mitigation is defensive and remains
correct regardless of what feature 468 does; it does not fix the allocation.

## CFN-3 — directed to feature 446

**Title.** The dispatcher continuation carrying the metrics write is not awaited.

**File and line reference.** `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-231`:

```csharp
await UiThread.Dispatcher.InvokeAsync(async () => await WriteMetrics(...), DispatcherPriority.ContextIdle)
```

**Defect.** `Dispatcher.InvokeAsync(Func<Task>, priority)` returns `DispatcherOperation<Task>`.
Awaiting that operation yields the inner `Task` without awaiting it, so the metrics write is
effectively fire-and-forget past its first suspension point and its failures do not surface.

**Recommended remedy, as stated in the spec.** Use `.Task.Unwrap()`, the pattern already present at
`UtilitiesCS/Threading/WpfUiDispatcher.cs:61`, so failures propagate and the write completes before
`ActionCancelAsync` cancels the token.

**Not fixed here.** `QfcFormController.EventHandlers.cs` is on this feature's forbidden-to-write
list.

**Owned-file mitigation this feature does deliver.** [P5-T8] passes `CancellationToken.None` to the
writer rather than the controller's session `Token`, asserted by
`WriteMetricsAsync_PassesUncancelledTokenToWriter`, so a cancellation raised while the write is
still in flight cannot abort it. It does not make the continuation awaited.

## Statement required by the task

**None of CFN-1, CFN-2, or CFN-3 is fixed in this feature's diff.** Each remains open and is
directed to its owning sibling child through the epic: CFN-1 and CFN-3 to feature 446, CFN-2 to
feature 468. The changed-file inventory recorded by [P7-T8] confirms that none of the three named
files was modified, and the ownership gate recorded by [P7-T6] gates that fact.

CFN-4 is the fourth note. It is not directed to a sibling; it is promoted to its own issue. Its
disposition is `PROMOTION BLOCKED`, recorded in
`evidence/issue-updates/cfn4-promotion-blocked.2026-08-26T11-32.md`.
