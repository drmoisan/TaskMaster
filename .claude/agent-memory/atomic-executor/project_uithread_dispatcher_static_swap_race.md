---
name: uithread-dispatcher-static-swap-race
description: Any test that drives QfcItemController init must swap the process-wide static UiThread.Dispatcher; two classes doing so concurrently deadlock on the parked dispatcher
metadata:
  type: project
---

`QfcTipsDetails.ToggleAsync` marshals through the **process-wide static**
`UtilitiesCS.UiThread.Dispatcher` (`UtilitiesCS/Threading/UiThread.cs`, private
static `_dispatcher`, no fallback getter). Any test that drives
`QfcItemController.Initialize*/Create*` reaches it via `ToggleTipsAsync`.

In `QuickFiler.Test` that static is either unset (NullReferenceException) or holds
the deliberately **parked, never-pumped** dispatcher seeded by
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` — an `InvokeAsync` on it
**never completes**. So such a test must reflection-swap `_dispatcher` to a live
pumped dispatcher and restore it in `finally`.

**Why:** #230 hit this twice. First as an NRE in `ToggleTipsAsync` (fixed by the
swap). Then, only under the full-suite run, as two `[Timeout]` expiries — one from
each of the two test classes that both call the shared pump fixture. MSTest
class-level parallelization ran them concurrently, so class B's restore reverted
the static to the parked dispatcher while class A's member was still awaiting a
dispatcher operation. Filtered runs and even a two-class run passed; only the
full-suite run interleaved them.

**How to apply:** if more than one test class swaps a shared static, serialize the
whole swap-to-restore window with a static `SemaphoreSlim(1,1)` acquired in the
fixture builder and released in an **idempotent** restore; release it in a `catch`
if the builder throws. `[DoNotParallelize]` is not sufficient on its own (see
[[project_mstest_donotparallelize_overlaps_parallel_bucket]]). Symptom signature to
recognize: a `[Timeout]` expiry rather than an assertion failure, exactly one
failure per swapping class, and green in every filtered run.

Related: [[project_utilitiescs_test_parallelism_flakiness]],
[[project_dispatcherdelay_hangs_unit_tests]].
