# store-runtime-reenable (Issue #263)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/store-runtime-reenable/ (Issue #263)
- Promotion type: feature
- Epic: #260 (store-lockup-resilience)

- Issue: #263
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/263
- Last Updated: 2026-07-07
- Work Mode: full-feature

## Problem / Why

When a store is disabled, or when the user chooses "Reenable" from the notification or the
settings UI, the add-in must re-establish that store at runtime. The original hookup runs once
during startup (`AppEvents.PerformReadinessHookup`, `AppOlObjects.LoadInboxes`,
`OutlookFolderNotificationSink` subscriptions) and that method has already terminated. There is
currently no supported path to add a single `Store` back to `Stores` and re-register its event
handlers after startup, so a disabled store cannot be restored without restarting Outlook.

## Proposed Behavior

- Provide a runtime rehook operation that, given a store identity, re-adds the corresponding
  live `Store` to `StoresWrapper.Stores` (rebuilding its `StoreWrapper` via the existing
  `Init`/rewire path) and re-registers its handlers:
  - item subscriptions equivalent to `AppEvents.PerformReadinessHookup` for that store's inbox
    (`OlInboxes.AddLast(items, items.ItemAdd += OlInboxItems_ItemAdd)`), and
  - per-store/per-folder subscriptions equivalent to
    `OutlookFolderNotificationSink.AddFolderSubscriptions` / `StoresNotificationSubscription`.
- The operation must be idempotent (re-enabling an already-hooked store does not double-hook),
  safe on the STA (reuse `OutlookReadinessGate`/`HookReadinessCoordinator` transient-retry
  patterns so a still-slow store does not re-freeze the UI), and observable via log4net.
- Expose the operation behind an interface the disable service (F1), the notification (F4), and
  the settings UI (F5) can call.

This feature delivers the rehook mechanics only. The trigger surfaces (auto-disable/notification
in F4, settings UI in F5) and the disabled model/service (F1) are separate.

## Acceptance Criteria (early draft)

- [ ] A runtime rehook operation re-adds a store to `StoresWrapper.Stores` and re-registers both
      item-level (AppEvents) and folder/store-level (`OutlookFolderNotificationSink`) handlers.
- [ ] The operation is idempotent: re-enabling an already-hooked store does not create duplicate
      subscriptions.
- [ ] The operation reuses the existing readiness-gate/transient-retry pattern so a slow store
      does not re-block the STA; it never introduces a synchronous expensive COM read on the UI thread.
- [ ] Failures are logged and surfaced without crashing; a store that cannot be rehooked reports
      a clear failure result.
- [ ] Deterministic MSTest coverage behind interfaces (Moq), no live Outlook, no temp files;
      subscription add/remove and idempotency verified via mockable seams.

## Constraints & Risks

- Highest-risk feature: touches live COM event wiring. Must not leak subscriptions or double-hook.
- Depends on F1 (`IStoreDisableService`) for the identity/state it acts on.
- Reuse existing hookup code paths rather than duplicating them; refactor a shared per-store
  hookup helper if needed to keep startup and rehook aligned.
- Must remain compatible with #207/#211/#242/#243 readiness work.

## Test Conditions to Consider

- [ ] Unit: rehook adds expected subscriptions; idempotency; failure path returns a clear result.
- [ ] Edge: rehook when the live store is no longer present; rehook during a transient-not-ready
      window (retry, no UI block).
- [ ] Regression: normal startup hookup unaffected by the extracted shared helper.

## Next Step

- [ ] Promote to GitHub issue (feature) via MCP tooling and link to epic #260
