# Fail-Before Method, Hang Hazard, and Operand Scope

Timestamp: 2026-08-08T16-27

Task: [P0-T13]

Companion to `<FEATURE>/evidence/regression-testing/fail-before.2026-08-08T16-26.md`.

## 1. Hang hazard

A WPF `Dispatcher` obtained by touching `Dispatcher.CurrentDispatcher` on an arbitrary thread is
created lazily and cached for that thread — but it does **not** pump unless something calls
`Dispatcher.Run()` (or a nested `PushFrame`). On a pooled worker thread nothing does.

The production code under test ends in:

```csharp
await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background, cancellationToken);
```

`DispatcherPriority.Background` work is only executed by a running dispatcher loop. Awaiting that
operation against a **non-pumping** dispatcher therefore never completes: the `DispatcherOperation`
stays queued forever and the awaiting test hangs. A hang in the MSTest host under
`Parallelize(Workers = 0, Scope = ClassLevel)` does not fail cleanly — it stalls the run and can
leave a detached runner behind.

Rule derived: **the probe must never await a yield against a non-pumping dispatcher.**

## 2. Mitigation applied in P0-T12

Three mitigations, all applied:

1. **Owned pumping dispatcher.** `ProbeStaDispatcherHost` starts a dedicated STA thread whose body
   captures `Dispatcher.CurrentDispatcher`, signals an `AutoResetEvent`, and then calls
   `Dispatcher.Run()`. The dispatcher the probe hands to `InvokeAsync` is therefore genuinely
   pumping, so `DispatcherPriority.Background` work executes and the await completes. The pattern is
   copied from the existing precedent at
   `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs:118-147`.
2. **Deterministic shutdown.** `Dispose()` calls `BeginInvokeShutdown(DispatcherPriority.Send)` then
   `_thread.Join()`, so the dispatcher loop exits and the thread is reaped before the test returns.
   The host is created in a `using` block.
3. **Bounded blast radius.** `[Timeout(30000)]` on the probe method converts any composition
   mistake into a 30-second test failure instead of an indefinite suite hang, and the host thread is
   marked `IsBackground = true` so an un-joined foreground thread could not delay testhost exit if
   the timeout did fire.

Observed outcome: the probe completed in 235 ms with a clean assertion failure. Neither the timeout
nor the background-thread safeguard was needed, which confirms the pumping dispatcher behaved as
intended.

## 3. Operand scope of the reproduction

The production resolution has two operands:

```csharp
Dispatcher dispatcher =
    Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher;
```

**Operand 1 — `Dispatcher.FromThread(Thread.CurrentThread)` — IS reproduced.** The probe makes this
operand non-null by executing on a thread that owns a dispatcher, and the test fails as a direct
result.

**Operand 2 — `UiThread.Dispatcher` — is deliberately NOT probed.** It is process-global set-once
static state (`UtilitiesCS/Threading/UiThread.cs:135-140`, a plain `static Dispatcher _dispatcher`
field behind a get-only property). Arranging it without a seam would require one of:

- calling `UiThread.Init()`, which shows a `SyncContextForm` — not permissible in a unit test; or
- reflection mutation of the process-global `UiThread._dispatcher` (precedent exists at
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:51-53`), which mutates shared state under
  class-level parallelization and would require serialization.

Both were considered and rejected in the plan's `## Design Decision — Seam Shape` section
(alternatives 2 and 3). Probing operand 2 by either route would reintroduce exactly the
process-global ambient coupling this issue exists to remove.

## 4. Why an operand-1 reproduction is sufficient for AC6

1. **One root cause.** Both operands are unarranged ambient state read through a single `??`
   expression. The defect is not "operand 1 is wrong" or "operand 2 is wrong" — it is that the test
   arranges neither. Demonstrating that the assertion's outcome flips when ambient state changes
   proves the test is order-dependent, regardless of which operand supplied the value.
2. **The fix covers both.** The Phase 1 seam replaces both operands with injected providers
   (P1-T2, P1-T4, P1-T5), and Phase 1 adds a test for each of the three resolution branches
   (P1-T10 operand 1 present, P1-T11 operand 1 null with operand 2 present, P1-T12 both null). The
   remedy is not operand-specific, so the fail-before evidence need not be either.
3. **Operand 2 is separately evidenced.** The plan's `## Notes` records that the observed baseline
   failure mode is `Failed`, not `Hang`, which implies the accidentally-resolved dispatcher in those
   real failing runs was pumping — consistent with operand 2 (`UiThread.Dispatcher`, populated by
   `UiThread.Init()`, which shows and pumps a `SyncContextForm`) being the dominant real-world
   contributor. `<FEATURE>/issue.md:50-54` records two consecutive baseline runs at merge-base
   `003c5715` with `Failed: 2` and `Failed: 1`, the latter naming
   `YieldAsync_WithoutDispatcher_RemainsStrict`. That is independent observational evidence of
   operand 2 in production conditions; the P0-T12 probe supplies the deterministic, on-demand
   reproduction that observational evidence cannot.

## 5. Exception dossier: not required

The task text requires a `fail-before-exception.<ts>.md` dossier **if and only if** P0-T12 could not
produce a genuinely failing run. P0-T12 did produce one (EXIT_CODE 1, `Failed: 1`, assertion message
"Expected a <System.InvalidOperationException> to be thrown, but no exception was thrown"), so the
conditional does not fire and no dossier is written.

Recorded for auditability per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
"Negative Evidence Claims":

- SearchScope: `<FEATURE>/evidence/regression-testing/` and `<FEATURE>/evidence/` (feature root; the
  feature is single-version, so there is no `vN/` scope to search)
- SearchPatterns: `fail-before-exception.*.md`
- SearchResult: none — and correctly none, because the failing-run branch was taken.

Output Summary: The hang hazard is awaiting `DispatcherPriority.Background` work on a non-pumping
dispatcher; P0-T12 avoids it with an owned STA thread running `Dispatcher.Run()`, deterministic
`BeginInvokeShutdown` + `Join` teardown, `[Timeout(30000)]`, and `IsBackground = true`. The probe
reproduces operand 1 only; operand 2 (`UiThread.Dispatcher`) is deliberately unprobed because
arranging it requires either showing a form or reflection-mutating process-global state, both
rejected by the plan. Both operands share one root cause and the Phase 1 seam arranges both, so
operand-1 reproduction is sufficient for AC6. No exception dossier required (genuine failing run
obtained).
