# Banned-API Sweep — Touched Production Files (AC10)

Timestamp: 2026-06-22T15-07

Command:
```
grep -rnE 'DateTime\.Now|DateTime\.UtcNow|Random\.Shared|Thread\.Sleep|Task\.Delay' \
  TaskMaster/AppGlobals/AppEvents.cs \
  TaskMaster/AppGlobals/AppOlObjects.cs \
  TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs \
  TaskMaster/AppGlobals/HookReadinessCoordinator.cs \
  TaskMaster/AppGlobals/NonBlockingDelay.cs \
  UtilitiesCS/OutlookObjects/IOutlookReadinessGate.cs \
  UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs
```
(call-site filter excludes comment/XML-doc lines: `| grep -vE ':[0-9]+:\s*///|:[0-9]+:\s*//'`)

EXIT_CODE: 1 (grep returns 1 = zero banned-API call-site matches after excluding comment/doc lines)

Output Summary:
- Zero banned-API CALL SITES remain in any touched production file.
- The pre-existing `Task.Delay(100)` in `AppEvents.ProcessNewInboxItemsAsync` (P0-T4 inventory line ~456, the unprocessed-queue error-retry path) is remediated → `await NonBlockingDelay.WaitAsync(TimeSpan.FromMilliseconds(100))` (the pump-independent, `System.Threading.Timer`-backed helper).
- The only residual textual matches are in comments / XML-doc `<c>...</c>` references (e.g. `NonBlockingDelay.cs` XML doc describing it as the non-blocking replacement for `Task.Delay`/`Thread.Sleep`; the explanatory comment at `AppEvents.cs` line 458). These are documentation, not invocations, and are not RS0030-actionable.
- Cross-referenced against the P0-T4 baseline inventory (`evidence/baseline/banned-api-inventory-2026-06-21T16-30.md`): the single baseline call site (`Task.Delay(100)`) is eliminated; no new banned-API call site was introduced.
- `AppOlObjects.JunkFolders.cs` is created in P4-T7 from the cohesive junk-folder region (move-only, no timing/clock API); re-swept post-creation with zero matches.
- The delay helper was reworked from the prior `DispatcherTimer`-backed `DispatcherDelay` to the pump-independent `System.Threading.Timer`-backed `NonBlockingDelay` (file renamed `DispatcherDelay.cs` → `NonBlockingDelay.cs`). `System.Threading.Timer` is NOT in the banned list.
- New timing in this fix uses `System.Diagnostics.Stopwatch`; polling uses `System.Windows.Threading.DispatcherTimer`; the only sanctioned delay is `NonBlockingDelay.WaitAsync`.
