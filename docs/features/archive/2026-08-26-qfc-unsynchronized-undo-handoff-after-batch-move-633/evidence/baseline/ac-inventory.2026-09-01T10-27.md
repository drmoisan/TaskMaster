# Acceptance-criterion inventory (P0-T2)

Timestamp: 2026-09-01T10-27
Task: [P0-T2]
Source: `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/spec.md`
Work Mode: full-bug — `spec.md` is the sole acceptance-criteria source. No `user-story.md` exists.
EXIT_CODE: 0

Section bounds: `## Acceptance Criteria` at `spec.md:574`; `## Risks & Mitigations` at `spec.md:659`.
Every row below was verified by reading `spec.md` in full and confirming that the stated line is the
first physical line of the bullet, so it matches `^- \[[ x]\] ` and the anchor resolves uniquely under
the section constraint.

| ID | spec.md line | Anchor fragment (verbatim, same line) | Criterion summary |
|---|---|---|---|
| AC1 | 580 | `exposes` | `FilerQueue.cs` exposes `public Task WhenDrainedAsync()`, completed on an idle queue. Verified by `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask`. |
| AC2 | 583 | `The drain task does not complete while any enqueued item` | Drain does not complete while an item is still processing. Verified by `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` and `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete`. |
| AC3 | 587 | `The drain task completes once every enqueued item has completed` | Drain completes once all items complete; each processor ran once. Verified by `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce`. |
| AC4 | 590 | `is idempotent: repeated and concurrent waiters all complete` | `WhenDrainedAsync()` idempotent across repeated and concurrent waiters. Verified by `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete`. |
| AC5 | 593 | `The orphaned-item window is closed` | Item enqueued after a drain is processed. Verified by `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch`. |
| AC6 | 596 | `An item whose processing throws still decrements` | Throwing item still decrements; drain completes. Verified by `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes`. |
| AC7 | 599 | `awaits` | `BackGroundMoveAsync` awaits `WhenDrainedAsync()` after the batch move and before both dispatches. Verified by the two `..._WithPendingQueueItem_...` tests. |
| AC8 | 604 | `The existing metrics-before-cleanup ordering is preserved` | Metrics once then cleanup once, in that order. Verified by `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp`. |
| AC9 | 607 | `The early-return guard in` | Guard includes a `_parent` null check. Verified by `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing`. |
| AC10 | 610 | `The two production reads of` | Both `FilerQueue.Consumer` reads removed; `\.Consumer\b` over `QuickFiler/**/*.cs` returns zero. |
| AC11 | 616 | `remains declared with the same type` | `Consumer` keeps type, accessibility, completed-task default. Verified by `FilerQueue_NewInstance_HasCompletedConsumerByDefault`, unmodified. |
| AC12 | 619 | `still raises` | `Enqueue(EmailFiler, IList)` still raises `ArgumentNullException` synchronously. Verified by `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException`, unmodified. |
| AC13 | 622 | `is reconciled with the new` | `QfcItemController.SeamFactoryTests.cs` reconciled; no reflection into a removed private field. |
| AC14 | 625 | `contains no banned wait API` | Zero matches for the banned-wait pattern across the three named test files. |
| AC15 | 630 | `is introduced` | No `init`, `record`, or `record struct` introduced; compiles on net481 without CS0518. |
| AC16 | 634 | `The production diff touches no file other than` | Production diff confined to the two named files; nothing else outside `QuickFiler.Test/` and `docs/`. |
| AC17 | 638 | `contains a` | `QuickFiler.Test.csproj` carries a `<Compile Include>` entry for the new test file; the new tests appear in run output. |
| AC18 | 641 | `Both changed production files remain under 500 lines` | Line count on both changed production files. |
| AC19 | 644 | `The full C# toolchain passes in a single uninterrupted pass` | Format, analyze, type-check, test all clean in one pass. |
| AC20 | 651 | `Coverage does not regress on any line changed by this fix` | No changed-line coverage regression; added/modified `FilerQueue.cs` members reach at least 90 percent. |

Row count: 20. Identifiers AC1 through AC20, each appearing exactly once. No duplicate identifier.

Output Summary: Twenty acceptance criteria transcribed, one row per criterion, each carrying its
identifier, its `spec.md` line number, and its verbatim same-line anchor fragment. All twenty checkboxes
are `- [ ]` at this point in execution.
