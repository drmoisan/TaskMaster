# P5 dispose-race audit

Timestamp: 2026-07-22T06:30:25.7050775Z

Command: `& { $ErrorActionPreference='Stop'; $hostPath='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\Viewers\BreadcrumbDropDownHost.cs'; $lifetimePath='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\Viewers\BreadcrumbDropDownOpenLifetime.cs'; $toggleTestsPath='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\Viewers\BreadcrumbSelectorToggleUiBoundaryTests.cs'; $retryTestsPath='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\Viewers\BreadcrumbSelectorOpenRetryTests.cs'; $planPath='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\remediation-plan.2026-07-21T21-37.md'; $diagnosticPath='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing\p5-dispose-race-hang-diagnostic.2026-07-22T06-26.md'; Write-Output 'INVALIDATION_SCHEDULING_AND_SYNCHRONIZATION'; & 'C:\Users\DanMoisan\AppData\Roaming\npm\node_modules\@openai\codex\node_modules\@openai\codex-win32-x64\vendor\x86_64-pc-windows-msvc\codex-path\rg.exe' -n 'InvalidateAndSchedule|DisposeAndSchedule|ScheduleInvalidating|InvalidateCore|CompleteInvalidation|ScheduleObserved|ObserveScheduledAsync|RunOnOwnerAsync|_openCompletion|SynchronizedRecorder|lock \(_sync\)|ExceptionSnapshot|ExecutedThreadSnapshot|DrainOne|CreatorThreadId|PendingCount|Dispose_WhenResetAndOpenWorkAreQueued|P6-T9|P6-T10|P6-T11|P6-T12|P6-T13|P6-T14|P6-T15|P6-T16|OWNED_TEST_PROCESSES=0' $hostPath $lifetimePath $toggleTestsPath $retryTestsPath $planPath $diagnosticPath; if($LASTEXITCODE -ne 0){ exit $LASTEXITCODE }; Write-Output 'LINES_AND_HASHES'; foreach($path in @($hostPath,$lifetimePath,$toggleTestsPath,$retryTestsPath)){ $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; $lines=(Get-Content -LiteralPath $path).Count; Write-Output "$path|LINES=$lines|SHA256=$hash" } }`

EXIT_CODE: 0

Output Summary: The audit found every host lifecycle schedule routed through `InvalidateAndSchedule` or `DisposeAndSchedule`, every scheduled operation routed through `ScheduleObserved`, both the kickoff task and returned operation task awaited by `ObserveScheduledAsync`, synchronized test queue/error snapshots, exact creator-thread drains, and the explicit P6-T9 through P6-T16 preservation route for the broader pending-open diagnostic. Final P5-T41 acceptance remains the exact two-class result: 12 passed, 0 failed/skipped, 2.3100 seconds. The broader legacy probe is diagnostic only and is not represented as passing evidence.

## Invalidation ordering

- `BreadcrumbDropDownHost.Close`, `Reset`, and `OnDropDownClosed` call `InvalidateAndSchedule`; `Dispose` calls `DisposeAndSchedule`. No host lifecycle callback is posted directly.
- `ScheduleInvalidating` takes `_sync`, rejects work after disposal, establishes disposal state where applicable, and calls `InvalidateCore` before it can invoke `ScheduleObserved`.
- `InvalidateCore` increments `_generation`, exchanges the cancellation source, captures and clears `_openCompletion`, and clears `_openTask` while still under `_sync`.
- After the lock is released, `CompleteInvalidation` settles the shared open completion with `false` and signals the old cancellation before `ScheduleObserved` can enqueue lifecycle work. Thus every caller holding the shared `OpenAsync` task completes deterministically when Reset/Dispose invalidates it.
- Each queued action captures the new generation and cancellation lease. Its boundary callback calls `IsLifecycleCurrent`; a later generation invalidation makes stale Reset, focus, close, error, and callback work return `Task.CompletedTask` without mutation. The one disposal lease is allowed after lifetime disposal so owned cleanup still runs once.

## Task observation and owner-boundary behavior

- `RunOnOwnerAsync` enters the captured owner-aware dispatch path, then invokes the actual operation through result/fault-propagating `RunAsync`. A scheduling failure leaves no operation task and becomes a deterministic fault after the dispatcher reports the boundary failure.
- `ScheduleObserved` passes the returned `Task<Task>` to `ObserveScheduledAsync`, which awaits both the dispatch kickoff and the lifecycle operation. Close, Reset, Dispose, and focus tasks therefore have an observer; no fire-and-forget task remains unobserved.
- `DisposeCoreAsync` runs on the owner boundary, takes the owned surface once, marks `_isOpen` false, detaches the native event, removes and disposes the host/control/messenger, and disposes the drop-down. It does not call native close, selection cancellation, or anchor focus after host disposal.
- `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity` passed with the pre-dispose operation count unchanged, zero late focus/cancel/anchor-focus/native-close callbacks, exactly-once surface and messenger disposal, an empty error sink and context exception snapshot, zero pending callbacks, and all executed work on the creator thread.

## Test synchronization

- `CapturingSynchronizationContext` protects pending work, exception snapshots, and executed-thread snapshots with `_sync`; `DrainOne` rejects a non-creator thread before dequeuing work.
- `BreadcrumbSelectorOpenRetryTests` uses `SynchronizedRecorder<Exception>` with locked add/snapshot operations for the error sink that may receive worker-boundary reports.
- The P5-T41 tests drain the captured queue explicitly and assert `PendingCount == 0`, empty exception snapshots, and creator-thread-only operation/execution records.

## Structure and deferred preservation obligations

- `BreadcrumbDropDownHost.cs` is SHA-256 `D67A0E8E407D5FD7BABAB90EAA8643F0CD2748EF075B6AE67379B17613680B5C` at 462 physical lines. `BreadcrumbDropDownOpenLifetime.cs` is SHA-256 `D8F914E556A4C9F2EEE14C530E86A0842E7A8FCE2EF485F714C844CC12727331` at 474 physical lines. Both are below the 480-line P5-T42 limit, CSharpier-stable, class-documented, behavior-named, and preserve the public `OpenAsync` parameter name `anchorScreenBounds`.
- The two P5 tests remain at 494 and 499 lines; their separate reduction is assigned to P5-T56 through P5-T62 and is not claimed here.
- The diagnostic artifact records `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup` timing out on an unpumped host-neutral async context and a late-success case observing the newly immediate false open completion before cleanup. It also verifies no workspace-owned VSTest/testhost process remained after bounded cleanup.
- These are open preservation obligations, not P5 acceptance results. P6-T9 through P6-T12 already authorize the lifetime/host/pending-open batch and require deterministic pending-close behavior plus exactly-once late resource disposal. P6-T13 through P6-T15 retain the format/analyzer/nullable gates. P6-T16 remains unchecked and explicitly requires `BreadcrumbDropDownLifecycleConcurrencyTests` together with pending-open, lifecycle, retry, and UI-boundary classes to pass. This audit does not close or claim P6-T16.
