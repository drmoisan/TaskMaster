# `2026-08-26-qfc-remove-stackmoveditems-parameter` — User Story

- Issue: #629
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-30

## Story Statement

- As a maintainer reading `QfcCollectionController`, I want `MoveEmailsAsync` to declare only the
  parameters it actually uses, so that the signature does not misleadingly suggest the undo stack is
  populated by an argument the caller passes, when it is in fact populated elsewhere.

## Problem / Why

Issue #469's investigation (defect 4) established that `MoveEmailsAsync`'s `stackMovedItems` parameter
is never used to populate the undo stack — `EmailFiler.PushToUndoStack` writes onto the same global
`SloStack` instance directly. Issue #468's branch could not remove the parameter itself because doing
so required editing `QfcFormController.EventHandlers.cs`, a file outside that branch's declared scope,
so it added an explicit discard (`_ = stackMovedItems;`) and deferred the actual removal to this issue.
Leaving a discarded, unused parameter on a production interface is a maintenance hazard: a future
reader may reasonably (and incorrectly) assume it does something.

## Personas & Scenarios

- Persona: a maintainer or an AI coding agent reading or extending `QfcCollectionController` /
  `IQfcCollectionController`.
  - They care about the interface accurately describing what the method needs from its caller.
  - Their constraint is limited context on why the parameter exists; the discard statement is a signal
    something is off, but not an explanation.
  - Their goal is to trust the signature at face value.
- Scenario: a maintainer adds a new caller of `MoveEmailsAsync` and has to decide what to pass for
  `stackMovedItems`.
  - Trigger: a new code path needs to move a batch of emails.
  - Steps: they read the interface, see the parameter, and either (a) waste time tracing why it exists
    and discover it does nothing, or (b) pass something plausible-looking that is silently ignored.
  - Obstacle: the parameter's real behavior (ignored) is not discoverable from the signature alone.
  - Expected outcome after this change: there is no parameter to reason about; the call is
    `MoveEmailsAsync()`.

## Acceptance Criteria

See `spec.md`'s `## Acceptance Criteria` section — this feature is `full-feature` work mode and
`spec.md` is the authoritative acceptance-criteria source; this file does not duplicate the list.

## Non-Goals

- No change to how the undo stack is actually populated (`EmailFiler.PushToUndoStack` and its call
  sites are untouched).
- No change to `IMovedMailInfo`, `SloStack<T>`, or any other type referenced by the removed parameter's
  declared type, beyond removing the one now-unused usage site.
- No change to any file in the `qfc-collection-controller-defects-468` feature folder or branch; this
  issue is the deferred follow-up from that feature, executed independently.
