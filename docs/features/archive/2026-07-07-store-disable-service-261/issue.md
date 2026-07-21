# store-disable-service (Issue #261)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/store-disable-service/ (Issue #261)
- Promotion type: feature
- Epic: #260 (store-lockup-resilience)

- Issue: #261
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/261
- Last Updated: 2026-07-07
- Work Mode: full-feature

## Problem / Why

The store-lockup-resilience epic needs a single, testable foundation that models which stores
are disabled and enforces that decision at store-filter time. Today `StoresWrapper` has only
substring-based exclusion lists (`ExcludedStoreNameContains`, `ExcludedStoreFilePathContains`,
etc.) with no notion of a user- or system-disabled store by identity, no runtime service to
toggle disablement, and no distinction between session-only and persisted disablement. Every
other feature in the epic (detection/auto-disable, reenable, notification, settings UI) depends
on this foundation, so it is the wave-0 prerequisite.

## Proposed Behavior

- Add a disabled-store model with two scopes: session-only (in-memory) and future-sessions
  (persisted). Persist the future-sessions list on `StoresWrapper` as a new `[JsonProperty]`
  list keyed by a stable store identity (DisplayName, with a documented fallback), beside the
  existing exclusion lists, serialized through the existing `SmartSerializable` path.
- Introduce `IStoreDisableService` (exposed on `IApplicationGlobals`) with a clear contract:
  `DisableSessionOnly(identity)`, `DisableForFutureSessions(identity)`, `Reenable(identity)`,
  `IsDisabled(identity)`, and `GetDisabledStores()`.
- Integrate the disabled set into the store include/exclude decision via the pure
  `StoreFilterAttribution.Decide` helper and `StoresWrapper.ShouldIncludeStore` /
  `StoreIsIncluded`, adding a new exclusion reason for user/system disablement.
- Persist future-sessions changes via the existing debounced `Model.Serialize()`.

This feature delivers the model, the service contract + implementation, filter integration, and
persistence only. It does not perform lockup detection (F4), runtime rehook (F3), the modeless
message (F4), or the settings UI (F5); it provides the seams they call.

## Acceptance Criteria (early draft)

- [ ] A persisted future-sessions disabled list exists on `StoresWrapper` (new `[JsonProperty]`)
      keyed by stable identity, round-tripping through `SmartSerializable` serialize/deserialize.
- [ ] A session-only disabled set exists in-memory and is not persisted.
- [ ] `IStoreDisableService` exposes DisableSessionOnly / DisableForFutureSessions / Reenable /
      IsDisabled / GetDisabledStores with documented contracts and is reachable via
      `IApplicationGlobals`.
- [ ] The store filter excludes stores in either disabled scope, with a distinct
      `StoreFilterAttribution` reason; existing exclusion behavior is unchanged.
- [ ] `DisableForFutureSessions` triggers persistence via the existing `Model.Serialize()` path;
      `Reenable` removes from both scopes and persists when it affects the future-sessions list.
- [ ] Deterministic MSTest coverage (Moq for `IApplicationGlobals`/`StoresWrapper`), no live
      Outlook, no temp files; new-code coverage meets target.

## Constraints & Risks

- Identity must be stable and cheap: prefer `DisplayName` (already the persisted key on
  `StoreWrapper`) with a documented fallback; do not read expensive/blocking COM members.
- Reuse `StoresWrapper`/`SmartSerializable`; do not add a new settings file or config key.
- Keep the service pure/host-neutral where possible so it is testable without Outlook.
- Do not regress the existing filter semantics used by #207/#211/#240.

## Test Conditions to Consider

- [ ] Unit: each service method; filter decision with each disabled scope; persistence trigger.
- [ ] Edge: disabling an already-disabled store (idempotency); reenabling a non-disabled store;
      identity fallback when DisplayName is unavailable.
- [ ] Serialization: future-sessions list survives serialize/deserialize round-trip.

## Next Step

- [ ] Promote to GitHub issue (feature) via MCP tooling and link to epic #260
