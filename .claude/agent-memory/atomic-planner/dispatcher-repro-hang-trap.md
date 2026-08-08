---
name: dispatcher-repro-hang-trap
description: Never plan a WPF Dispatcher flake repro that touches Dispatcher.CurrentDispatcher on a pooled worker — an awaited InvokeAsync(Background) against that non-pumping dispatcher hangs forever instead of failing
metadata:
  type: feedback
---

When planning a fail-before repro for a `System.Windows.Threading.Dispatcher` order-dependence defect, do NOT plan "touch `Dispatcher.CurrentDispatcher` on the current test thread, then call the code under test". `Dispatcher.CurrentDispatcher` creates and caches a dispatcher for the calling thread, but an MSTest pooled worker never runs `Dispatcher.Run()`, so any awaited `dispatcher.InvokeAsync(..., DispatcherPriority.Background, ...)` never completes. The repro deadlocks the test run instead of producing the failing assertion the plan needs.

The only hang-free deterministic shapes are: (a) supply a **pumping** dispatcher from an STA thread the test itself owns and shuts down (`Dispatcher.CurrentDispatcher` + `Dispatcher.Run()` on that owned thread, `BeginInvokeShutdown` + `Join` in `Dispose`), or (b) assert on synchronously observable resolution state without awaiting the yield at all.

**Why:** #508 (2026-08-08). The naive repro looked deterministic and was the obvious first choice, but it converts an intermittent `Failed` into an indefinite hang. Corroborating signal: the reported baseline flake manifested as `Failed`, not `Hang`, which means the accidentally-resolved dispatcher in those runs was already pumping — i.e. the real contributor was the process-global `UiThread.Dispatcher` populated by `UiThread.Init()` (which shows and pumps a `SyncContextForm`), not a bare pooled-thread dispatcher.

**How to apply:** Any `[expect-fail]` task involving `Dispatcher` must state the pumping requirement explicitly and pair it with a task recording the hang hazard and mitigation, so the executor does not substitute the naive shape. Also check whether the production fallback reads a plain static field (safe) or a property whose getter calls `Init()` (pops a form / touches COM — never acceptable in a unit test). Related: [[reference-invoke-mstest-with-coverage-script]].
