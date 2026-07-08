# 2026-07-07-store-lockup-resilience - Initiative Overview

- Issue: #260
- Owner: drmoisan
- Last Updated: 2026-07-07T17-34

## Goal & Outcomes

Make the add-in resilient to a single Outlook `Store` that repeatedly locks up the UI thread:
detect and attribute the lockup, isolate (disable) the store to restore responsiveness, notify
the user with governed options (session-only / future-sessions / reenable), allow runtime
reenable, and expose a settings list of disabled stores. Also fix the related defect that
prevents Folder Settings from opening after startup. Outcome: the UI stays responsive in the
presence of a failing store, and the user retains full control over which stores are active.

## Decomposition (Child Features/Workstreams)

- F1 store-disable-service (Issue #261) - `../2026-07-07-store-disable-service-261/` — model + service + filter + persistence
- F2 folder-settings-store-model-null (Issue #262) - `../2026-07-07-folder-settings-store-model-null-262/` — root-cause load-pipeline bugfix
- F3 store-runtime-reenable (Issue #263) - `../2026-07-07-store-runtime-reenable-263/` — runtime rehook of a store
- F4 store-lockup-detect-notify (Issue #264) - `../2026-07-07-store-lockup-detect-notify-264/` — detection + auto-disable + modeless notification
- F5 disabled-stores-settings-ui (Issue #265) - `../2026-07-07-disabled-stores-settings-ui-265/` — settings list + reenable

Dependencies (waves): Wave 0 = {F1, F2}; Wave 1 = {F3} (needs F1); Wave 2 = {F4 (needs F1, F3),
F5 (needs F1, F2, F3)}. See `../../epics/store-lockup-resilience/epic-plan.md` for the machine
manifest and DAG.

## Cross-Cutting Constraints & Assumptions

- COM/STA safety: no new expensive/blocking store-member reads on the UI thread; identification
  uses cached identity only.
- Reuse existing primitives (StoresWrapper/SmartSerializable, StoreFilterAttribution,
  AppEvents/OutlookFolderNotificationSink, ThreadMonitor/TimeOutTask/OutlookReadinessGate, MyBox,
  UiThread/IUiDispatcher, IApplicationGlobals). No new persistence mechanism, no IoC container.
- Determinism/testability: MSTest + Moq + FluentAssertions; injected clock/timeouts; no live
  Outlook; no temporary files.
- Shared store-identity convention and `IStoreDisableService` contract (F1) are reused by all
  dependent features; a shared per-store hookup helper (F3) keeps startup and rehook aligned.
- Quality gates across all children: csharpier → analyzers → nullable/TreatWarningsAsErrors →
  MSTest with coverage.

## Milestones & Status

- M1 Epic + 5 child issues promoted and linked - Done
- M2 epic-plan.md manifest authored - Done
- M3 Per-feature research - Not started
- M4 Per-feature spec/user-story - Not started
- M5 Per-feature atomic plan + preflight clear + docs committed - Not started
- M6 User approval to begin implementation - Not started

## Initiative-Level Validation

- End-to-end: with a simulated failing store, the UI remains responsive, the store is disabled,
  the modeless message appears with working options, reenable restores the store, and the
  future-sessions choice survives a restart.
- Integration: F4/F5 reenable paths both route through F3's shared rehook; F5 depends on F2 so
  the settings dialog opens with a populated model.
- Determinism/Regression: existing startup hardening (#207/#211/#240/#242/#243) and standalone
  behavior remain green.
- Error handling/Resilience: config-missing/null-deserialize load paths recover; rehook is
  idempotent and non-blocking; attribution never fires on a store without cached identity.

## Notes / Follow-Ups

- Root-cause reframing for F2 confirmed with the user on 2026-07-07: the fix is in the store
  load/deserialize pipeline, robust to all three null paths, not a startup-notification wire-up.
- F4 auto-disables the offending store immediately (session scope), then shows the message
  (confirmed 2026-07-07).
- Implementation (worktrees, integration branch, PRs, waves) is deferred until the user reads
  and approves this planning package.
