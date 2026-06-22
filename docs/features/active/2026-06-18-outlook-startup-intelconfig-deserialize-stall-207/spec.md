# Design Spec — STA-minimal startup hookup (Issue #207 corrective fix)

- Date: 2026-06-21
- Author: Dan Moisan (directive), orchestrator (authoring)
- Scope class: large-path refactor of the startup hookup path
- Inputs: `evidence/diagnostics/startup-timing-increment{2,3}-*.md`; `artifacts/research/2026-06-21-startup-hookup-readiness-gate-research.md`

## Problem

Startup trips a `ContextSwitchDeadlock` MDA because readiness-dependent Outlook COM accesses run
synchronously on the STA during cold start. Confirmed: `OlReminders` blocks ~113 s when accessed
early; deferring it migrates the block to `Ol.Inboxes` (~53.9 s), which can also throw a COMException
(0xDAC40111 / 0x8E640111) that fails the `IdleAsyncQueue` startup action and loses the inbox
subscription. The cost is a relocatable readiness wait (fast once Outlook is ready).

## Goal (maintainer directive)

Minimize STA reliance across the startup hookup path: gate all readiness-dependent COM hookups on a
real Outlook readiness check; keep the STA pumping at all times; break long work into short STA
calls; offload non-COM compute to worker threads and release the STA while they run.

## Design

1. **Readiness gate.** Before hooking `ToDoFolder.Items`, `OlReminders`, and `Ol.Inboxes`, evaluate a
   cheap, non-throwing readiness probe on the STA:
   `try { return App.Session?.DefaultStore?.GetDefaultFolder(olFolderInbox) != null; } catch (COMException) { return false; }`
   (pattern precedent: `AppOlObjects.ResolveCurrentUserEmailAddress`).
2. **STA always pumps.** Poll readiness on a `System.Windows.Threading.DispatcherTimer` (≈1 s tick,
   optional backoff to ≈5 s after a threshold to limit overhead). The pump runs between ticks; no
   synchronous block; no `Thread.Sleep`/`Task.Delay`/fixed sleep.
3. **Never give up.** Polling continues indefinitely until readiness is observed; then all hookups run
   and the timer stops. The hookup executes **exactly once** (idempotent guard). The inbox `ItemAdd`
   subscription is never dropped.
4. **COMException as not-ready.** A transient not-ready COMException from the store/inbox access
   (0xDAC40111 / 0x8E640111) is treated as not-ready and routes back through the gate; it never
   propagates to fail the `IdleAsyncQueue` action.
5. **Offload boundary.** COM touches stay on the STA but become short and pumped. Non-COM compute is
   offloaded: in `ProcessMailItemAsync`, extract COM-backed fields on the STA, `Task.Yield()`,
   tokenize/classify on a worker, then marshal back only for `SetUdf` writes. (`IntelligenceConfig`
   deserialization and `helper.Tokens` are already offloaded.)
6. **Testable seam.** `IOutlookReadinessGate` (probe + transient-error discrimination) and a pure
   `HookReadinessCoordinator` (tick → check gate → run-once / retry / continue), unit-tested with
   MSTest + Moq + FluentAssertions; the COM glue (`OutlookReadinessGate`, DispatcherTimer wiring,
   `AppEvents.Hook`) is COM/VSTO-exempt.
7. **Cleanup.** Remove the increment-3 diagnostic deferral scaffolding (`RemindersProbeSchedule`,
   `RemindersProbeDelaySeconds` setting in `Settings.settings`/`Settings.Designer.cs`/`app.config`,
   `ScheduleDeferredRemindersProbe`), superseded by the coordinator. **Keep** the increment-1/2 timing
   logs (`[IntelConfig timing]`, `[Startup timing]` per-COM/read timing) for ongoing observability.
8. **File-size remediation.** `AppOlObjects.cs` is 523 lines (over the 500 cap). Since the fix touches
   it (HRESULT discrimination in `LoadInboxes`), extract a cohesive partial (e.g., junk-folder code)
   to bring it ≤500. All touched files must end ≤500 lines.

## Out of scope

The ~115 s IntelConfig-phase `Task.Run` continuation stall attributed to Teams add-in STA starvation
(maintainer decision 2026-06-21). Not pursued in this fix.

## Acceptance Criteria

- AC1 (gate): `ToDoFolder.Items`, `OlReminders`, and `Ol.Inboxes` are not hooked until the STA
  readiness probe passes; the probe is cheap and non-throwing.
- AC2 (pump): waiting uses a `DispatcherTimer` poll; no synchronous block and no banned delay API;
  no single STA call blocks for a prolonged period during the wait.
- AC3 (never give up): if Outlook is not ready, polling continues indefinitely (bounded cadence) and
  the hookup eventually runs exactly once; the inbox `ItemAdd` subscription is never silently dropped.
- AC4 (COMException retry): a transient not-ready COMException (0xDAC40111 / 0x8E640111) is treated as
  not-ready and routed back through the gate; it never fails the `IdleAsyncQueue` startup action.
- AC5 (offload): non-COM compute in `ProcessMailItemAsync` runs off the STA after STA primitive
  extraction; the STA is released while workers compute; AutoProcessed marking and engine actions are
  preserved.
- AC6 (behavior preservation): after startup, ToDo item, reminder, and inbox `ItemAdd` events are
  hooked; `Unhook` cleanly reverses; inbox catch-up processing is unchanged in outcome.
- AC7 (testable seam): a pure readiness/coordinator seam is unit-tested (MSTest + Moq +
  FluentAssertions): not-ready→no hookup; ready→hookup once; transient COMException→retry;
  not-ready-then-ready→eventual single hookup. No live COM, no live timer, no filesystem, no temp files.
- AC8 (cleanup + file size): increment-3 deferral scaffolding removed; increment-1/2 timing retained;
  `AppOlObjects.cs` and all touched files ≤500 lines.
- AC9 (toolchain/coverage): full C# toolchain passes in order (CSharpier → analyzers → nullable/TWAE →
  MSTest with coverage); new/changed testable lines meet coverage policy; no repo-wide regression; no
  banned API; net48 constraints honored (no positional `record struct`).
- AC10 (validation): because this is a COM/STA timing defect not reproducible in MSTest, end-to-end
  validation is a fresh runtime startup capture showing the hookups complete without a prolonged STA
  block and without a `ContextSwitchDeadlock` MDA; the capture is recorded under `evidence/`.
