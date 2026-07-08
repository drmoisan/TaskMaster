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
- AC7 (end-to-end seam test): a pure readiness/coordinator seam is tested end-to-end through a
  simulated readiness timeline (MSTest + Moq + FluentAssertions): not-ready×N → transient COMException
  → ready → exactly-once hookup; never-give-up (continues past an extended not-ready run);
  `Unhook` interaction. This is the deterministic, CI-runnable end-to-end coverage of the managed
  orchestration. No live COM, no live timer, no filesystem, no temp files.
- AC8 (file-size remediation — explicit deliverable): `AppOlObjects.cs` (currently 523 lines) is
  brought ≤500 by extracting a cohesive partial (e.g., junk-folder code), as an explicit objective of
  this fix; all touched files end ≤500 lines.
- AC9 (cleanup): increment-3 deferral scaffolding removed (`RemindersProbeSchedule`,
  `RemindersProbeDelaySeconds` setting, `ScheduleDeferredRemindersProbe`); increment-1/2 timing logs
  retained for observability.
- AC10 (banned-API remediation — in scope): any banned API in a production file modified by this fix
  is remediated, not deferred — including the pre-existing `Task.Delay(100)` in
  `ProcessNewInboxItemsAsync`, replaced by a non-blocking, STA-pumping delay (a `DispatcherTimer`-based
  awaitable helper) consistent with the never-block-the-STA design. No `DateTime.Now`/`UtcNow`,
  `Random.Shared`, `Thread.Sleep`, or `Task.Delay` remains in any file this fix touches.
- AC11 (toolchain/coverage): full C# toolchain passes in order (CSharpier → analyzers → nullable/TWAE →
  MSTest with coverage); new/changed testable lines meet coverage policy; no repo-wide regression;
  net48 constraints honored (no positional `record struct`).
- AC12 (runtime validation): the irreducible COM/STA cold-start timing behavior is confirmed by a
  fresh runtime startup capture showing the hookups complete without a prolonged STA block and without
  a `ContextSwitchDeadlock` MDA; recorded under `evidence/`. See the testability note below for why
  this part is not CI-automatable.

- AC13 (developer-only integration harness): an opt-in integration test exercises the readiness
  gate + coordinator hookup against a live `Microsoft.Office.Interop.Outlook.Application` on an STA
  thread, logging the readiness wait and per-hookup latency and asserting the hookup completes and the
  STA was not blocked beyond a threshold. It is marked with a distinct category (e.g.,
  `[TestCategory("LiveOutlook")]`), EXCLUDED from the standard QC/CI test run and from the coverage
  denominator (CI agents have no Outlook; the run filters the category out), and documented with the
  explicit developer run command. It is a smoke/integration check, not a deadlock reproduction (a warm
  Outlook returns fast), and must never gate the build.

## Testability note (end-to-end automation boundary)

The fix splits cleanly into an automatable layer and an irreducible manual layer:

- **Automatable (AC7, in CI):** the managed orchestration — the readiness gate decision, the
  coordinator's never-give-up polling, exactly-once hookup, transient-COMException-as-retry — is fully
  exercised end-to-end at the seam by injecting a fake `IOutlookReadinessGate` that scripts a readiness
  timeline (not-ready×N → transient COMException → ready). This is deterministic and runs in CI with no
  Outlook.
- **Not reliably automatable (AC12, manual capture):** the real COM/STA cold-start behavior. Three
  reasons: (1) it requires a live Outlook profile, which CI agents do not have; (2) the failure only
  manifests under a non-deterministic cold/unready Exchange state — on a warm machine the call returns
  in milliseconds and the bug does not reproduce, so even a real-Outlook automated test would pass
  trivially without proving the fix; (3) `ContextSwitchDeadlock` is a debugger-only Managed Debugging
  Assistant, not a catchable runtime exception, so "the MDA did not fire" is not a programmatic assert.
  An optional developer-only integration harness against live Outlook could log the timing on demand,
  but it cannot assert the failure condition deterministically and adds little beyond the existing
  increment-1/2 instrumentation plus the manual capture.
