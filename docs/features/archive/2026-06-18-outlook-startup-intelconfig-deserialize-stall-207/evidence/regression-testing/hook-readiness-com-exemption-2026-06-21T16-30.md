# Hook-Readiness COM/VSTO + DispatcherTimer Coverage Exemption (UT5 explicit exception)

Timestamp: 2026-06-22T00-00

Rule: General Unit Test Policy UT5 ("call out the exception explicitly") + CLAUDE.md / `.claude/rules/general-unit-test.md` COM-host-bound (Outlook VSTO / WinForms / Interop) coverage exemption. The maintainer-ratified COM/VSTO exemption in CLAUDE.md governs Outlook Interop event-handler and lifecycle classes that depend on `Microsoft.Office.Interop.Outlook.Application`/`MailItem`/`Store`/`MAPIFolder` with no injectable seam.

WhyNoUnitTest: The following members depend on `Microsoft.Office.Interop.Outlook` and/or a live `System.Windows.Threading.Dispatcher` STA message pump with no injectable seam below the COM/timer boundary, and therefore cannot be unit-tested without a live Outlook process or a live message pump (both prohibited as external dependencies under UT4/UT5):
- `AppEvents.Hook()` — constructs the live `OutlookReadinessGate` from `Globals.Ol.App` and wires a live `DispatcherTimer` poll; requires a running Dispatcher on the add-in STA.
- `AppEvents.PerformReadinessHookup()` — performs the three readiness-dependent COM hookups (`Globals.Ol.ToDoFolder.Items`, `Globals.Ol.OlReminders`, per-inbox `Items.ItemAdd`) on the STA.
- The `DispatcherTimer` wiring inside `Hook()` (Interval/Tick/backoff/Stop) — requires a live message pump.
- `DispatcherDelay.WaitAsync(TimeSpan)` — a one-shot `DispatcherTimer` helper that completes only on a real timer tick under a live Dispatcher; no deterministic test without a live pump.
- `OutlookReadinessGate.IsReady()` — live `App.Session.DefaultStore.GetDefaultFolder(...)` probe against `Microsoft.Office.Interop.Outlook.Application`.
- `AppOlObjects.LoadInboxes()` — enumerates `NamespaceMAPI.Stores` and calls `store.GetDefaultFolder(...)` against live COM.

The pure decision seam is `HookReadinessCoordinator`, which is unit-tested separately and deterministically with `Mock<IOutlookReadinessGate>` (no COM, no timer, no clock). `OutlookReadinessGate.IsTransientError(COMException)` is pure HRESULT discrimination and is exercised indirectly through the coordinator's transient-retry test; the live `IsReady()` probe is the only COM-bound member.

VerificationMethod: Inspection of (a) the coordinator-driven branch in `Hook()` (DispatcherTimer → `coordinator.Tick()` → stop on `Completed`, backoff after threshold, never-give-up); (b) the run-once guard in `HookReadinessCoordinator.Tick()` (covered by `HookReadinessCoordinatorTests`); (c) the `LoadInboxes()` transient-vs-permanent COMException discrimination using the shared `OutlookReadinessGate.Transient*HResult` constants (transient → rethrow/retry, permanent → log-and-skip, subscription never silently dropped); and (d) the `DispatcherDelay` tick/stop/complete logic. No live-Outlook test and no live-timer test runs in the gated CI loop.

RuntimeValidationNote: End-to-end deadlock resolution is the manual AC12 runtime capture (cold-start Outlook, observe `[Startup timing]` lines, no prolonged STA block, no `ContextSwitchDeadlock` MDA), performed by the maintainer and explicitly NOT part of this plan's QC loop. The live gate+hookup smoke check is the developer-only AC13 `LiveOutlookHookupIntegrationTests` (`[TestCategory("LiveOutlook")]`), excluded from the gated CI run via `/TestCaseFilter:"TestCategory!=LiveOutlook"` and excluded from the coverage denominator. Neither is a CI unit test.
