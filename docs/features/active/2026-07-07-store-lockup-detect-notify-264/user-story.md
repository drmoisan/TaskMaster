# store-lockup-detect-notify (User Story)

- **Issue:** #264
- **Parent (epic):** #260 (store-lockup-resilience)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07
- **Status:** Draft
- **Work Mode:** full-feature

## Story Statement

As a TaskMaster user whose Outlook contains a mailbox (store) that periodically freezes the
Outlook UI — because that store's connection keeps failing, its logon is slow, or its per-store
reads block — I want TaskMaster to notice the freeze, identify which mailbox caused it,
automatically disable that mailbox so Outlook becomes responsive again, and tell me what happened
with clear options, so that one misbehaving mailbox no longer makes Outlook unusable and I stay in
control of whether and when that mailbox is used again.

## Problem / Why

Today, when a single store repeatedly locks up the UI thread, TaskMaster cannot connect the freeze
to a specific mailbox or act on it. The user experiences an unresponsive Outlook with no
explanation and no remedy short of restarting or removing the add-in. Diagnostic instrumentation
exists (the `ThreadMonitor` watchdog and the #211 per-store timing probes), but nothing turns a
detected stall into an attributed, actionable response. This feature closes that gap: it detects
the extended stall, attributes it to the mailbox currently being processed, disables that mailbox,
and notifies the user without introducing further delays.

## Personas & Scenarios

- **Persona: TaskMaster user with a misbehaving mailbox.**
  - Who: a person running Outlook with the TaskMaster add-in and one or more mailboxes (stores),
    at least one of which intermittently fails to connect or is slow to log on.
  - What they care about: an Outlook that stays responsive; not losing work to a full freeze;
    understanding why a freeze happened and having a say in the remedy.
  - Constraints: cannot modify the failing mailbox's server-side behavior; does not want to lose
    access to their other, healthy mailboxes; is not a developer and needs a plain-language
    prompt, not a stack trace.
  - Goals and frustrations: wants the add-in to isolate the problem mailbox automatically, but
    also wants to decide whether the isolation is temporary or persistent, and to turn the mailbox
    back on when it recovers.

- **Scenario: a mailbox freezes the UI and is isolated.**
  1. The user launches or is working in Outlook with the add-in loaded.
  2. One mailbox repeatedly stalls the Outlook UI thread past the configured duration.
  3. TaskMaster detects the extended stall and attributes it to the mailbox currently being
     processed, using only the mailbox display name it already had in hand (no new slow lookups
     that would deepen the freeze).
  4. TaskMaster disables that mailbox for the current session so it is not retried on the next
     pass, which lets Outlook become responsive again.
  5. A non-blocking message appears naming the mailbox and offering three choices; the user can
     keep working in Outlook while it is on screen.
  6. The user chooses whether to keep the mailbox off for this session only, keep it off for future
     sessions, or reenable it now.

## What the User Sees

The non-blocking message identifies the mailbox and offers three options:

- **Disable This Session Only** — keep the mailbox off for this Outlook session only.
- **Disable for Future Sessions** — keep the mailbox off in future sessions as well (persisted).
- **Reenable** — turn the mailbox back on now and re-establish its connection and event handlers.

Because the message is non-blocking, the user is never forced to dismiss a frozen dialog to regain
control of Outlook. The mailbox is already disabled by the time the message appears, so
responsiveness is restored first and the choice about what to do next is left to the user.

## Scope Boundary (user-facing)

- "Restores responsiveness" means the offending mailbox is not retried on the next processing pass.
  It does not mean an in-progress operation is forcibly cancelled — a COM call already running
  cannot be aborted, so a single in-flight call may still complete before the effect is visible.
- The three options are handled by the store disable/enable service (feature F1). Reenable
  re-establishes the mailbox connection through F1, which coordinates the runtime rehook (feature
  F3) on the user's behalf. This feature triggers those behaviors; it does not own them.

## Guardrails (from the user's perspective)

- If TaskMaster cannot attribute a UI stall to a specific mailbox, it takes no action and shows no
  message — it will not disable an arbitrary mailbox or raise a false alarm.
- If the attributed mailbox has already been disabled, the user is not shown a duplicate message
  and the mailbox is not disabled a second time.
- If the mailbox identity cannot be read, the situation is treated the same as "no attribution":
  no disable, no message.
- Detection and identification add no new slow mailbox lookups on the UI thread, so the act of
  noticing the freeze does not make the freeze worse.

## Acceptance Criteria

The authoritative, testable acceptance criteria for this feature are the numbered AC list in
`spec.md`. The user-facing statements below summarize the outcomes a user should be able to
confirm; each maps to the corresponding numbered criterion in `spec.md`.

- [ ] An extended UI-thread freeze caused by one mailbox is detected and attributed to that
      mailbox using its cached display name, with a configurable threshold (spec AC1, AC3, AC4).
- [ ] The offending mailbox is automatically disabled for the session before any message is shown,
      restoring responsiveness on the next pass (spec AC2, AC5).
- [ ] A modeless message identifies the mailbox and offers "Disable This Session Only", "Disable
      for Future Sessions", and "Reenable", each wired to the correct behavior; the message never
      blocks the UI thread (spec AC6).
- [ ] No message and no disable occur when the freeze cannot be attributed to a mailbox, when the
      identity is unavailable, or when the mailbox is already disabled (spec AC7, AC8).
- [ ] The event is recorded at WARN with the mailbox identity and stall duration so it appears in
      the important-logs file for later review (spec AC9).

## Non-Goals

- No detection or handling of stalls that cannot be attributed to a specific mailbox (those are out
  of scope; see the "no context" guardrail).
- No changes to how mailboxes are disabled, persisted, or reenabled — those are owned by features
  F1 and F3; this feature only triggers them.
- No aborting of an operation already in progress; only prevention of the next retry.
- No settings surface for listing or managing disabled mailboxes — that is feature F5 (#265).
