---
name: configcontroller-sta-pump-deadlock
description: ConfigController.SaveAsync deadlocks if an STA test thread blocks on it; needs a DoEvents+Thread.Yield pump, not GetAwaiter().GetResult()
metadata:
  type: project
---

`UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config.ConfigController.SaveAsync()` installs a
`WindowsFormsSynchronizationContext` (when none exists) and then `await Task.Run(...)`, so its continuation is
posted back to the STA message queue. Any test that runs the save on a dedicated STA thread
(`thread.SetApartmentState(ApartmentState.STA)`) and BLOCKS that thread to wait for completion deadlocks: the
blocked STA thread never pumps the queue, so the continuation never runs.

**Why:** Surfaced during issue #181 cycle-6 timer-determinism remediation. The research-suggested D1 fix
(replace the `while(!task.IsCompleted){ Application.DoEvents(); Thread.Sleep(10); }` pump with a bare
`saveTask.GetAwaiter().GetResult()`) deadlocked `ConfigController_Tests` and hung the whole UtilitiesCS.Test
assembly under vstest — invisible to per-class/filtered runs that excluded it, only caught on the full-assembly run.

**How to apply:** To wait for `SaveAsync` (or any WinForms-SynchronizationContext-posting async) on an STA test
thread without a wall-clock sleep AND without deadlock, pump the queue and yield the scheduler:
`while (!task.IsCompleted) { System.Windows.Forms.Application.DoEvents(); Thread.Yield(); } task.GetAwaiter().GetResult();`
`Thread.Yield()` is a scheduler yield (not a fixed-duration wall-clock wait) and is NOT in `BannedSymbols.txt`
(only `Thread.Sleep` is), so it satisfies the deterministic-test policy while keeping the message pump alive.
When validating a change that touches UI/STA-thread tests, always run the FULL UtilitiesCS.Test assembly (not just
filtered subsets) because a deadlock there hangs vstest rather than reporting a failure.
