# Batch 4 — Pragma-Only Nullable Build Verification

- Timestamp: 2026-07-19T10-00
- Task: [P4-T5]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Executed equivalent (genuine recompile of the changed project): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /m`
- EXIT_CODE: 1 (pre-existing first-party TWAE noise only; zero CS86xx)

## Opted-in Batch 4 files (3)

- UtilitiesCS/Threading/IdleActionQueue.cs
- UtilitiesCS/Threading/IdleAsyncQueue.cs
- UtilitiesCS/Threading/ApplicationIdleTimer.cs

## Output Summary

- **CS86xx for the 3 opted-in Batch 4 files: 0.** CS86xx count anywhere: 0.
- IdleActionQueue: `_entries` field annotated `ConcurrentQueue<Action>?` (lazily `??=`-initialized); the `Application.Idle` scheduling and the single-shot subscribe-guard reset are byte-unchanged. `TryDequeue(out Action action)` needed no change (net481 BCL oblivious, non-null element type flows clean).
- IdleAsyncQueue: pragma only (`Entries` is `{ get; } = new()`, value-tuple `TryDequeue`).
- ApplicationIdleTimer: in-place annotations only — `instance = null!` (static-ctor set), `_timer = null!` (StartTimer-set), `syncContext` -> `SynchronizationContext?` (already null-checked), `event ApplicationIdleEventHandler?`, and `FindTriggeringEventHandler` return `Delegate?` with `EventInfo?`/`FieldInfo?`/`object?` locals. `Heartbeat`/`ComputeCPUUsage`/`OnApplicationIdle` timing math and the `Interlocked` subscription counting are byte-unchanged.
- Non-zero EXIT_CODE is the pre-existing first-party TWAE noise only (CS0618 x14 + CS0168 x2, unchanged from baseline). No new diagnostics elsewhere; vendored skipped. `/p:Nullable=enable` NOT passed.
