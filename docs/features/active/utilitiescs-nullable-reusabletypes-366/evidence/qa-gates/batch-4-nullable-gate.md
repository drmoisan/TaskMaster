# Batch 4 — Nullable Pragma Gate (P4-T3)

Timestamp: 2026-07-19T09-40

## Commands

1. `dotnet tool run csharpier format .` — EXIT_CODE 0 (clean).
2. Pragma gate: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`), isolated-compile methodology per P0-T5.

## Output Summary

Batch 4 (5 files: TimerWrapper, TimedAsyncTask, TimedBatchAction, TimedQueueOfActions,
TimedDiskWriter) cluster diagnostics:
- CS86xx count attributed to `ReusableTypeClasses/`: 0 (AC1 for Batch 4)
- CS8714 count: 0
- Pre-existing non-cluster UtilitiesCS TWAE errors: 14 (unchanged; out of scope)

Annotations applied (annotation/null-safety only, no behavior change; IO seams reused as-is):
- `TimerWrapper.cs`: `EventHandler<TimeElapsedEventArgs>?` uninitialized event.
- `TimedAsyncTask.cs`: nullable `_action` / internal-ctor `Func<Task>? action` (parameterless ctor
  passes null), `ITimerWrapper? _timer`.
- `TimedBatchAction.cs`: nullable `_action` / `System.Action? action`, `ITimerWrapper? _timer`.
- `TimedQueueOfActions.cs` and `TimedDiskWriter.cs`: `GetCurrentMethod()!.DeclaringType!` logger
  init (justified), nullable `BatchActions`/`DiskWriter` delegate property+field (parameterless ctor
  leaves unset; StartTimer throws if still null), `ITimerWrapper? _timer`/`Timer`, `BatchActions!`/
  `DiskWriter!` at the callback (timer only starts when the delegate is set), nullable
  `Configuration.PropertyChanged` event, and `_config = null!` backing field (always assigned by
  every constructor through the Config setter; the compiler cannot prove assignment through a manual
  property setter).

No `System.Diagnostics.CodeAnalysis` post-condition attribute was added. No temp files; no new IO.
