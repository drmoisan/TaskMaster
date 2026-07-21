# Store Disable Service (F1, Issue #261) — User Story

- Issue: #261
- Epic: #260 (store-lockup-resilience), Wave 0 foundation
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-07

## Story Statement

- As a TaskMaster user whose Outlook profile contains a store that intermittently fails or locks
  up the add-in, I want the add-in to isolate that single store so the rest of my mail, tasks, and
  folders keep working, so that one bad store no longer degrades my whole session.
- As a user who has isolated a problem store, I want to choose whether the isolation lasts only for
  this session or persists across restarts, so that I control how durable the decision is without
  editing configuration files by hand.
- As a user whose problem store has recovered, I want to reenable it later, so that I regain access
  once the underlying issue is resolved.

## Problem and Impact

The store-lockup-resilience epic needs a single, testable foundation that models which stores are
disabled and enforces that decision at store-filter time. Today `StoresWrapper` has only
substring-based exclusion lists with no notion of a user- or system-disabled store by identity, no
runtime service to toggle disablement, and no distinction between session-only and persisted
disablement.

From the user's perspective, the absence of this foundation means there is no supported, reversible
way to take a single failing store out of the add-in's scope. The larger epic goal — detecting a
lockup, isolating the offending store, notifying the user, and allowing reenable — cannot be built
until a store can be identified, marked disabled at a chosen scope, and honored by the store filter.
F1 delivers that capability as internal plumbing; the user-visible behaviors that consume it
(automatic isolation, the notification with options, and the settings list) arrive in later
features (F4 and F5).

## Personas and Scenarios

### Persona: user with a failing store

- Who: a TaskMaster user with multiple Outlook stores (for example a primary mailbox plus one or
  more secondary/archive/shared stores), at least one of which periodically errors or stalls.
- Cares about: keeping the add-in responsive; not losing access to healthy stores; not having to
  hand-edit settings files; being able to undo an isolation decision later.
- Constraints: works inside the existing Outlook add-in; cannot tolerate long UI-thread stalls;
  expects a persisted choice to survive an Outlook restart.
- Goals and frustrations: wants a single failing store to stop degrading the whole session; is
  frustrated when one bad store forces a workaround affecting everything.

### Scenario A — isolate for this session only

1. A secondary store begins failing during the current session.
2. The add-in (via a later feature that calls this foundation) marks that store disabled for the
   session only.
3. The store filter immediately stops including that store when building the store list, inbox
   subscriptions, and folder tree; healthy stores are unaffected.
4. Expected outcome: the session becomes responsive again; nothing is written to disk; on the next
   Outlook restart the store is present again because session-only isolation was never persisted.

### Scenario B — isolate across restarts, then reenable

1. The user decides a store should stay isolated until they say otherwise.
2. The store is marked disabled for future sessions; the decision is persisted through the existing
   settings path (no new file or config key).
3. The store remains excluded for the rest of the current session and after every restart.
4. Later, the user reenables the store. The isolation is cleared from both the session-only and the
   persisted scope, the persisted change is saved, and the add-in prepares to bring the store back
   into scope (the actual re-add and event re-registration are delivered by F3).
5. Expected outcome: the store returns to normal handling; the persisted disabled list no longer
   lists it.

## Foundation Behaviors Delivered by F1

- A stable store identity: `StoreIdentity.Resolve(displayName, filePathFallback)` — DisplayName
  primary, a documented fallback, no blocking COM read — used consistently by every feature that
  disables, tests, or reenables a store.
- A two-scope disabled model on `StoresWrapper`: a persisted future-sessions list
  (`[JsonProperty] DisabledStoreIdentities`) and an in-memory session-only set
  (`[JsonIgnore] SessionDisabledStoreIdentities`).
- `IStoreDisableService`, reachable via `IApplicationGlobals.StoreDisable`, exposing
  `DisableSessionOnly`, `DisableForFutureSessions`, `ReenableAsync`, `IsDisabled`, and
  `GetDisabledStores` (identity + scope).
- Store-filter enforcement: a distinct `Disabled` attribution reason, checked last, applied
  identically across all three include/exclude surfaces, leaving existing exclusion behavior
  unchanged.

## Acceptance Criteria

These user-facing acceptance criteria are refined and made independently testable in
`spec.md` §9 (AC1–AC15). They are stated here in outcome terms.

- [ ] A persisted future-sessions disabled list exists on `StoresWrapper`, keyed by stable identity,
      and survives a serialize/deserialize round-trip.
- [ ] A session-only disabled set exists in memory, is never persisted, and is empty after a restart
      or deserialize.
- [ ] `IStoreDisableService` exposes DisableSessionOnly / DisableForFutureSessions / ReenableAsync /
      IsDisabled / GetDisabledStores with documented contracts and is reachable via
      `IApplicationGlobals.StoreDisable`.
- [ ] The store filter excludes stores in either disabled scope with a distinct
      `StoreFilterAttribution` reason checked last; existing exclusion behavior is unchanged, across
      all three include/exclude surfaces.
- [ ] `DisableForFutureSessions` persists via the existing `Model.Serialize()` path; `ReenableAsync`
      clears both scopes, persists when it affects the future-sessions list, and then invokes the
      injected rehook collaborator (a no-op in wave 0).
- [ ] Disabling and reenabling are idempotent, and an unresolvable-identity input is rejected rather
      than silently disabling an unrelated store.
- [ ] Deterministic MSTest coverage (Moq for `IApplicationGlobals`/`StoresWrapper`), no live Outlook,
      no temporary files; new-code coverage meets target.

## Non-Goals

F1 provides seams only; the following are explicitly out of scope and delivered by later features:

- Lockup detection, attribution, and automatic disablement (F4, #264).
- Runtime rehook mechanics — re-adding the `Store` and re-registering event handlers (F3, #263). F1
  defines the rehook seam and ships a no-op default.
- The modeless notification with user options (F4, #264).
- The settings UI listing disabled stores with per-store reenable (F5, #265).
