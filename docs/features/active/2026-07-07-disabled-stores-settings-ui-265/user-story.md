# disabled-stores-settings-ui - User Story

- **Issue:** #265
- **Epic:** #260 (store-lockup-resilience)
- **Depends on:** F1 (#261), F2 (#262), F3 (#263)
- **Owner:** drmoisan
- **Status:** Draft
- **Last Updated:** 2026-07-07
- **Work Mode:** full-feature

## Story Statement

- As a TaskMaster user whose Outlook store was disabled after a lockup, I want a persistent place
  in Settings to see which stores are currently disabled, so that I do not have to rely on the
  transient lockup notification to know the current state.
- As that user, I want to reenable a disabled store from that list with a single action, so that I
  can restore a store to normal operation when I am ready, without editing configuration by hand.
- As that user, I want to tell at a glance whether a store is disabled only for this session or for
  future sessions too, so that I know whether it will come back automatically next time I start
  Outlook.

## Problem / Why

When a store repeatedly locks up the Outlook UI thread, the store-lockup-resilience epic disables
it to restore responsiveness and notifies the user with a transient modeless message that offers a
reenable option. Once that notification is dismissed, there is no persistent place to view or
manage disabled stores.

The existing "TaskMaster -> Settings -> Folder Settings" entry point opens a single-store detail
editor (a ComboBox plus labels for one store's archive/junk folder assignments). It has no list of
stores and no enable/disable surface, so it cannot serve this need. The user is left without a
durable way to see disabled stores or to turn them back on after the notification is gone.

## Personas & Scenarios

- **Persona: Outlook power user with multiple mailboxes.**
  - Who: a user who runs TaskMaster with several stores connected (for example a primary mailbox
    and one or more shared or archive stores).
  - What they care about: keeping Outlook responsive, and keeping every store they rely on working.
  - Constraints: they do not want to edit configuration files or restart Outlook to recover a
    store; they may not have seen or may have dismissed the lockup notification.
  - Goals and frustrations: they want a clear, discoverable view of what has been disabled and a
    reliable way to reverse it; they are frustrated by state changes they cannot see or undo.
  - Context and motivations: a store was auto-disabled earlier in the session, or persistently for
    future sessions, and the user now wants to check the state and possibly restore it.

- **Scenario: reenable a disabled store from Settings.**
  - Who is acting: the power user above.
  - Trigger: they notice a store's folders are missing, or they recall a lockup notification, and
    they open "TaskMaster -> Settings".
  - Steps: they click the new "Disabled Stores" button in the Settings menu. A dialog opens listing
    every currently disabled store, each row showing the store's display name and its scope
    ("Session Only" or "Future Sessions"), with future-sessions rows visually distinguished. They
    find the store they want and click its Reenable button.
  - Obstacles or decisions: they decide which store to restore based on its scope; if a reenable
    attempt fails, they see a clear message and the list stays accurate rather than showing a stale
    state.
  - Expected outcome: the store is reenabled through the disable service, the list refreshes to
    reflect the current state, and the store returns to normal operation. If the store was disabled
    for future sessions, reenabling it also clears that persisted state so it does not disable again
    next session.

- **Scenario: check state when nothing is disabled.**
  - Who is acting: the same user, verifying the current state.
  - Trigger: they open "Disabled Stores" to confirm no store is disabled.
  - Steps: the dialog opens with column headers and no rows.
  - Expected outcome: they see an empty list and understand that no store is currently disabled,
    with no error.

## Acceptance Criteria

- [x] The Settings menu offers a "Disabled Stores" action that opens a dialog listing the currently
      disabled stores. _(Evidence: evidence/other/non-interference-confirmation.md; ribbon wiring P6.)_
- [x] Each row shows the store and its disablement scope, distinguishing session-only from
      future-sessions disablement. _(Evidence: evidence/regression-testing/controller-tests-pass.md — PopulateRows_ProjectsServiceEntriesIntoRows; Designer CellFormatting.)_
- [x] Each row offers a Reenable action that reenables the store through the disable service; the
      list updates to reflect the resulting state after the action. _(Evidence: controller-tests-pass.md — Dgv_CellContentClick_OnReenableColumn... + ReenableAsync_OnSuccess...)_
- [x] The list reflects the current disable-service state when the dialog opens and after every
      reenable. _(Evidence: controller-tests-pass.md — PopulateRows + ReenableAsync_OnSuccess refetch.)_
- [x] If a reenable attempt fails, the failure is shown to the user without crashing, and the list
      still reflects the current state afterward. _(Evidence: controller-tests-pass.md — ReenableAsync_WhenServiceThrows_SurfacesViaMyBox...)_
- [x] When no store is disabled, the dialog opens with an empty list and no error. _(Evidence: controller-tests-pass.md — PopulateRows_WhenServiceReturnsEmpty...)_
- [x] The existing single-store Folder Settings and Junk Folder Settings editor is unchanged. _(Evidence: evidence/other/non-interference-confirmation.md; readiness-extraction-behavior-preserving.md.)_

## Non-Goals

- No surface for disabling a store from this dialog; disabling is owned by the lockup-detection and
  disable-service features. This feature lists and reenables only.
- No UI for the pre-existing store exclusion lists.
- No new persistence mechanism or configuration key; reenable and any persisted-state clearing go
  through the existing disable service.
- No direct interaction with the runtime rehook mechanics; reenable is routed through the disable
  service, which orchestrates the rehook internally.
