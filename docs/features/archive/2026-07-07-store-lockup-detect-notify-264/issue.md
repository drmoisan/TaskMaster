# store-lockup-detect-notify (Issue #264)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/store-lockup-detect-notify/ (Issue #264)
- Promotion type: feature
- Epic: #260 (store-lockup-resilience)

- Issue: #264
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/264
- Last Updated: 2026-07-07
- Work Mode: full-feature

## Problem / Why

The epic's core behavior: when a particular `Store` locks up the UI thread for an extended
period due to repeated errors (transient store-not-ready HRESULTs, failing Exchange logon,
expensive per-store COM reads), the add-in must notice, attribute the lockup to a specific
store, auto-disable that store to restore responsiveness, and tell the user — all without
introducing further lockups or delays. Today `ThreadMonitor` can detect UI-thread stalls and log
the STA stack, and per-store timing probes (#211) localize blocking cost, but nothing ties a
detected stall to a specific store, auto-disables it, or notifies the user.

## Proposed Behavior

- Detection: use/extend the existing `ThreadMonitor` UI-responsiveness watchdog (and per-store
  timing attribution from #211) to recognize an extended UI lockup and attribute it to the store
  currently being processed, using a store-identity context captured cheaply before the blocking
  call (no expensive COM reads during or after the stall).
- Auto-disable: on attribution, immediately call `IStoreDisableService.DisableSessionOnly` (F1)
  to restore responsiveness.
- Notify: deliver a modeless (non-blocking) message via the existing `MyBox` action-button
  primitive marshalled through `UiThread`/`IUiDispatcher`, identifying the store as specifically
  as possible from cached identity, offering three buttons:
  - "Disable This Session Only" (confirm session disable; F1),
  - "Disable for Future Sessions" (persist via F1's `DisableForFutureSessions`),
  - "Reenable" (invoke F3 runtime rehook and clear the disablement).
- Log the event at WARN so it lands in the JSON important-logs file.

Depends on F1 (disable service) and F3 (reenable rehook). The store model/persistence and the
rehook mechanics are provided by those features; this feature owns detection, attribution,
auto-disable triggering, and the modeless message.

## Acceptance Criteria (early draft)

- [ ] An extended, error-driven UI lockup is detected via the watchdog and attributed to a
      single store using cheap cached identity, with a configurable/ injected threshold and clock.
- [ ] On attribution the store is auto-disabled (session scope) immediately.
- [ ] A modeless message identifies the store and offers the three options, each wired to the
      correct F1/F3 behavior; the message never blocks the UI thread.
- [ ] Detection and identification introduce no new expensive/blocking COM calls on the UI thread.
- [ ] The event is logged at WARN with store identity and timing context.
- [ ] Deterministic MSTest coverage with injected clock/timeout and Moq seams for the watchdog,
      disable service, rehook, and dialog; no live Outlook, no temp files, no real waits.

## Constraints & Risks

- Attribution correctness is the main risk: the stall must be tied to the right store without
  reading expensive members after the fact. Requires a store-context ambient/scope captured
  before blocking calls.
- Modeless dialog testability: reuse the `MyBox` `DialogInvoker` seam and the modeless `Show()`
  pattern; do not use modal `ShowDialog` for the notification.
- Must not re-trigger a lockup while identifying the store.
- Determinism: no `Thread.Sleep`/`Task.Delay`/real timers in tests; use injected time.

## Test Conditions to Consider

- [ ] Unit: attribution decision from a stall + store context; auto-disable call; three-button
      wiring; message content/identity formatting.
- [ ] Edge/negative: stall with no store context (no false attribution); repeated stalls for an
      already-disabled store (no duplicate notifications); identity unavailable.
- [ ] Determinism: threshold crossing via injected clock; dialog shown via non-modal seam.

## Next Step

- [ ] Promote to GitHub issue (feature) via MCP tooling and link to epic #260
