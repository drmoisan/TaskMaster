# Batch 8 — Pragma-Only Nullable Build Verification (LAST)

- Timestamp: 2026-07-19T10-55
- Task: [P8-T4]
- Literal plan command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Executed equivalent (genuine recompile of the changed project): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /m`
- EXIT_CODE: 1 (pre-existing first-party TWAE noise only; zero CS86xx)

## Opted-in Batch 8 files (2)

- UtilitiesCS/Threading/AsyncMultiTasker.cs
- UtilitiesCS/Threading/TimeOutTask.cs

## Output Summary

- **CS86xx for the 2 opted-in Batch 8 files: 0.** CS86xx count anywhere: 0.
- AsyncMultiTasker: four `timerFactory` defaults annotated `Func<TimeSpan, ITimerWrapper>?` (then `??=`); the second overload's `ITimerWrapper timer = null` -> `ITimerWrapper?` with `timer!.StopTimer()`/`timer!.Dispose()` in `catch`/`finally` (PRESERVES the current NRE-if-unassigned behavior; NOT switched to `timer?.`); the unconstrained `((IItemInfo)x).Sw.Durations` deref resolved with `((IItemInfo)x!)`. `Task.Run` fan-out and `Task.WhenAll` ordering byte-unchanged.
- TimeOutTask: ~15 `RunWithTimeout`/`TimeoutAfter` overloads driven to zero CS86xx WITHOUT widening the public return type — `Task<TResult>` kept, `result = default!`/`default(TResult)!` and `return result!` used for the unconstrained-`TResult` default paths; `timeoutSourceFactory` params annotated `?`; `MarshalTaskResults` `castedSource = source as Task<TResult>` -> `Task<TResult>?` (already null-checked) with `default(TResult)!` in the `TrySetResult` ternary; the `Task<TResult>`/`Task` null-initialized locals in the retry `TimeoutAfter` overloads -> `null!` with `return result!`. No `Task<TResult?>` widening; no file split.
- Non-zero EXIT_CODE is the pre-existing first-party TWAE noise only (CS0618 x14 + CS0168 x2, unchanged from baseline). No new diagnostics elsewhere; vendored skipped. `/p:Nullable=enable` NOT passed.
