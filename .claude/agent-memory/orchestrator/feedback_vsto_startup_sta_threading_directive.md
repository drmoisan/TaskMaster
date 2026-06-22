---
name: vsto-startup-sta-threading-directive
description: User's architectural directive for TaskMaster VSTO startup/event-hook/COM work — minimize STA reliance, always pump, offload only non-COM compute
metadata:
  type: feedback
---

For TaskMaster startup, event-hook, and Outlook-COM-access code, the user wants STA reliance minimized and the STA message pump never blocked.

Rules:
- Gate readiness-dependent COM hookups (e.g., `OlReminders`, `Ol.Inboxes`, `ToDoFolder.Items`) on a real Outlook store-readiness check; do not call them while Outlook is unready.
- The STA must always pump. Wait by polling a cheap, non-throwing readiness signal (DispatcherTimer / Application.Idle), never a synchronous block, never a fixed `Task.Delay`/`Thread.Sleep` (also banned APIs).
- Break long-running startup work into short STA calls.
- Offload only non-COM pure compute (tokenization, classification, deserialization) to worker threads and release the STA while they run. Outlook Interop objects are STA-apartment-bound: a COM call from a worker marshals back to the STA, so extract primitives on the STA, compute on a worker, marshal results back. Do not try to offload the COM calls themselves.
- Treat a not-ready COMException from an early COM access as a retry condition, never a fatal error that drops a subscription.

**Why:** On #207 the startup `Hook` made synchronous COM accesses on the STA during cold start; `OlReminders` blocked ~113 s and `Ol.Inboxes` ~53.9 s (or threw COMException 0xDAC40111/0x8E640111), tripping the `ContextSwitchDeadlock` MDA. Runtime probes proved the cost is a relocatable readiness wait, and that deferring one call just migrates the block to the next. See [[evidence-and-lifecycle-for-every-change]] and issue #207 evidence.

**How to apply:** When touching TaskMaster startup, `AppEvents`/`AppOlObjects`, IdleAsyncQueue, or any Outlook COM access path. Also apply the scope-by-causation rule: a residual stall (e.g., assembly-load/STA contention) is in scope only if this add-in causes it; attribute before classifying — do not assume external.
