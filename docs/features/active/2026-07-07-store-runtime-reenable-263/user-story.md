# store-runtime-reenable (User Story)

- Issue: #263
- Parent (epic): #260 (store-lockup-resilience)
- Feature ID: F3
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-07

## Story Statement

- As an Outlook user whose secondary mail store was disabled after it locked up the UI, I want to
  reenable that store while Outlook is still running, so that its mail, folders, and new-item
  handling are restored without restarting Outlook.
- As an Outlook user who reenables a store, I want the reenable to either succeed or report a clear
  reason it did not, so that I am never shown a store as reenabled while it remains non-functional.
- As an Outlook user, I want reenabling a store to keep the UI responsive even if that store is
  still slow to respond, so that reenabling does not re-freeze Outlook.

## Problem / Why

When a store is disabled — either auto-disabled after it locked up the UI thread (F4) or disabled
by the user in the settings surface (F5) — the add-in stops wiring that store's events. To restore
it, the add-in must re-add the live `Store` to `StoresWrapper.Stores` and re-register its handlers.
The original hookup runs exactly once during startup, across three subsystems
(`AppEvents.PerformReadinessHookup`, `AppOlObjects.LoadInboxes`, and
`OutlookFolderNotificationSink` subscriptions), and those methods have already terminated. There is
currently no supported path to hook up a single store after startup, so today a disabled store
cannot be restored without restarting Outlook. This feature provides that runtime rehook mechanism.

## Personas and Scenarios

### Persona: knowledge worker with multiple mail stores

- Who they are: an Outlook user with a primary mailbox plus one or more secondary stores (for
  example a shared mailbox or a secondary account).
- What they care about: keeping Outlook responsive and getting all their mail, including from
  secondary stores.
- Their constraints: they do not want to restart Outlook to recover a store; a restart interrupts
  their work and any long-running startup.
- Their goals and frustrations: after a store was disabled to stop it locking up the UI, they want
  a reliable way to bring it back once it recovers, and they are frustrated when a control claims to
  have reenabled a store that is still not working.
- Their context and motivations: they act from a notification (F4) or from the disabled-stores
  settings list (F5); they expect the reenable control to give a definite outcome.

### Scenario A: successful runtime reenable

1. A secondary store was disabled earlier in the session (auto-disabled after a lockup, or disabled
   by the user).
2. The store has since recovered and is responding normally.
3. The user clicks "Reenable" for that store (from the notification or the settings list).
4. The reenable request flows through the disable service (F1), which invokes the rehook mechanism
   (F3).
5. The store is re-added to `StoresWrapper.Stores`, its inbox item handling and its folder/store
   notifications are re-registered, and the cached folder-tree view for that store is refreshed.
6. Outcome: the store's mail and folders are available again, new mail in that store is processed,
   and the store is no longer shown as disabled — all without restarting Outlook.

### Scenario B: reenable while the store is still slow

1. The user clicks "Reenable" for a store that has not fully recovered and is still slow to respond.
2. The rehook mechanism waits for the store to become ready using the existing readiness/retry
   pattern, without performing an expensive synchronous read on the UI thread, so Outlook stays
   responsive during the wait.
3. If the store becomes ready within the retry window, the reenable completes as in Scenario A.
4. If the store never becomes ready within a bounded window, the operation stops retrying and
   reports a transient-timeout result. The store remains marked disabled so the user can try again
   later.
5. Outcome: the UI never re-freezes, and the user gets a definite result rather than an operation
   that appears to hang.

### Scenario C: reenable when the store is gone or fails permanently

1. The user clicks "Reenable" for a store whose underlying account has been removed, or that raises
   a non-transient error during rehook.
2. The mechanism reports a clear failure result (store-not-found or permanent-error) and logs the
   detail for diagnosis, without crashing Outlook.
3. Because the reenable did not succeed, the store stays marked disabled, so the UI does not present
   it as reenabled while it remains unusable.
4. Outcome: the failure is visible and honest; the user is not misled into thinking the store is
   working.

### Scenario D: reenable an already-active store

1. The user clicks "Reenable" for a store that is, in fact, already hooked up.
2. The mechanism detects that the store is already fully hooked and returns an already-hooked
   success result, making no duplicate event subscriptions.
3. Outcome: no double-hooking, no duplicate new-mail processing, and the reenable still reports
   success.

## Acceptance Criteria

The full, numbered, testable acceptance criteria for this feature are maintained in `spec.md`
(AC1–AC11). The user-facing outcomes below map to those criteria and are the conditions this story
considers done.

- [ ] A disabled store can be reenabled at runtime, restoring its mail, folders, and new-item
      handling without restarting Outlook (spec AC1, AC2).
- [ ] Reenabling a store that is already active does not create duplicate subscriptions or duplicate
      new-mail processing (spec AC3).
- [ ] Reenabling a slow store keeps the UI responsive and never re-freezes Outlook; if the store
      does not become ready in time, the user gets a clear transient-timeout outcome (spec AC4, AC5,
      AC6).
- [ ] Reenable failures (store missing, permanent error, transient timeout) are reported with a
      clear result and logged, and never crash Outlook; a store that cannot be rehooked stays marked
      disabled (spec AC6, AC7).
- [ ] The behavior is verified by deterministic MSTest coverage behind interfaces (Moq), with no
      live Outlook, no temporary files, and no real timers (spec AC10, AC11).

## Non-Goals

- The lockup detection, attribution, auto-disable, and notification that triggers a reenable
  (delivered by F4).
- The settings UI that lists disabled stores and offers per-store reenable (delivered by F5).
- The disabled-store model, disable/enable service, store filtering, and persistence (delivered by
  F1). Reenable is routed through F1's service; this feature is the rehook mechanism F1 invokes.
- The Folder Settings null-store load-pipeline fix (delivered by F2).
- Any redesign of the existing startup readiness/retry infrastructure; this feature reuses and
  extends it.
