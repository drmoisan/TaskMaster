# Batch 7 — Pragma-Only Nullable Build Verification (CRITICAL)

- Timestamp: 2026-07-19T10-40
- Task: [P7-T5]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Executed equivalent (genuine recompile of the changed project): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /m`
- EXIT_CODE: 1 (pre-existing first-party TWAE noise only; zero CS86xx)

## Opted-in Batch 7 files (3)

- UtilitiesCS/Threading/UiThread.cs
- UtilitiesCS/Threading/ThreadMonitor.cs
- UtilitiesCS/Threading/StoreLockupResponder.cs

## Output Summary

- **CS86xx for the 3 opted-in Batch 7 files: 0.** CS86xx count anywhere: 0.
- UiThread: conditionally-set static fields annotated (`_onLockupDetected`/`_monitorTimeProvider`/`_syncContextForm`/`_threadMonitor` -> `?`); `_dispatcher = null!` and `_uiSyncContext` -> `?` with a justified `return _uiSyncContext!` in the getter (Init populates it); Init `onLockupDetected`/`timeProvider` params -> `?`; awaiter ctor param -> `SynchronizationContext?` (keeps the existing `is null` throw guard valid). The public `UiSyncContext`/`Dispatcher`/`UiThreadId`/`AutoScaleFactor` contract stays non-null; the `SynchronizationContextAwaiter.Post` marshaling, the `_loaded` single-shot init guard, and the `ThreadMonitor` wiring order are byte-unchanged.
- ThreadMonitor: `thread` param/field -> `Thread?`; `_onLockupDetected` -> `Action<LockupAttribution>?`; `_pollTimer` -> `ITimer?`; ctor `timeProvider`/`onLockupDetected` defaults -> `?`; `GetStackTrace` return + local -> `StackTrace?`; justified `!` in the `[ExcludeFromCodeCoverage]` ping path (`dispatcher!.InvokeAsync`, `thread!.Name`, `GetStackTrace(thread!)` — production non-null, null only on the test seam). The polling loop, one-shot timer re-arm (`_pollTimer?.Change` in `finally`), the `_lockupReported` once-per-episode latch, and the `Thread.Suspend/Resume` diagnostic path are byte-unchanged. `EvaluatePoll` consumes `CurrentStoreContext.Current` (`string?`) into `new LockupAttribution(..., string?)` cleanly. The `[ExcludeFromCodeCoverage]` methods compile clean under the pragma.
- StoreLockupResponder: only the two optional ctor params annotated (`StoreLockupNotifier? notify`, `Action<string>? logSink`); `displayName` (`= attribution.StoreIdentity`, now `string?`) flows through the four guards unchanged; a single justified `displayName!` at the `_notify(displayName!, ...)` call site (the delegate's `identity` param is non-null; net481 `IsNullOrWhiteSpace` does not refine null-state so an explicit `!` is used rather than a new guard). The existing ctor `?? throw` guards are unchanged. No null-branch was added, removed, reordered, or altered.
- Non-zero EXIT_CODE is the pre-existing first-party TWAE noise only (CS0618 x14 + CS0168 x2, unchanged from baseline). No new diagnostics elsewhere; vendored skipped. `/p:Nullable=enable` NOT passed.
