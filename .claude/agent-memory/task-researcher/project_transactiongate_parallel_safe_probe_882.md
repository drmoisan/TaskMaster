---
name: transactiongate-parallel-safe-probe-882
description: "#882: how to test a bounded SemaphoreSlim(1,1) acquisition under Workers 0/ClassLevel with no DoNotParallelize — zero-bound probes assert only failure while the test holds the permit; success only via the production entry; SemaphoreFullException surfaces at the holder's Dispose, not at the wrong release"
metadata:
  type: project
---

Issue #882 research refresh, 2026-09-28. Subject: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
(`TransactionGate`, unbounded `WaitAsync()` at `:149` after the #743 merge; counters at `:41-54`).

**Rule (operator constraint: parallel regime stays, no `[DoNotParallelize]`, no retries, no sleeps):**
a `TimeSpan.Zero` acquisition may be used only to assert FAILURE, and only while the asserting test
itself holds the sole permit (then `CurrentCount == 0` is unchangeable by anyone else). SUCCESS is
asserted only through the production (blocking, bounded) entry point, because the instant the test
releases, any other class may take the permit and a zero-bound success probe becomes a flake.

**Why:** the spec of 2026-09-13 proposed `[DoNotParallelize]` on the new test's class; the operator
rejected that on 2026-09-28. The C1 construction (hold + zero probe) is parallel-safe without it as
long as every assertion is made either while holding or through the waiting entry point.

**How to apply:**
- #743 counters: `contended` increments BEFORE the wait (keep), `acquisitions` must increment only
  on the `true` branch; a failed bounded wait is +0/+0 so `acquisitions - releases == live` holds and
  the #743 test (`FixtureTests.cs:355-394`, asserts only the DIFFERENCE) keeps passing. No test in
  the assembly asserts absolute counter values or `CurrentCount`.
- `ContendedAcquisitions` after a probe: assert `>= before + 1` (monotonic), never `==` under
  parallelism.
- Wrong-shape release on the `false` branch does NOT throw `SemaphoreFullException` at the release
  (count 0 -> 1 succeeds silently); it throws later at the legitimate holder's `Dispose`. Put the
  `NotThrow<SemaphoreFullException>` assertion on the test's OWN `Dispose` inside the try, keep the
  `finally` Dispose as the idempotent safety net (R5 proves double-Dispose is safe).
- Hold WITHOUT `Install` — no dispatcher, no `UiThread._dispatcher` write, minimal hold window.
- `FixtureTests.cs` is 396/500 lines; budget the new test <= ~80 lines. `csproj` has 185 explicit
  `Compile Include` items (no globbing) so a new file costs a project-file edit.
- Acquisition inventory after #743: 15 statements / 5 files (was 14); the 4 no-`[Timeout]` methods
  (`QfcHomeControllerRunAsyncTests` x1 across 4 partials, `QfcFormControllerUndoHandoffTests` x3)
  are unchanged. MSTest is 4.4.1 now (packages.config `:43-45`).
