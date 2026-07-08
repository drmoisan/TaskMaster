# Issue #264 Update Mirror — AC1–AC10 Status

Timestamp: 2026-07-08T08-42

PostedAs: body (mirrored into local `spec.md` `## Acceptance Criteria`; GitHub posting deferred to
the orchestrator, which owns git/PR steps for this feature branch).

## Exact text mirrored into spec.md

All AC1–AC10 checked off (`- [x]`) under `## Acceptance Criteria`, plus an evidence-traceability
table. Status:

- [x] AC1 — Detection on injected clock + configurable threshold (LockupStallDecider + ThreadMonitor.EvaluatePoll; FakeTimeProvider tests).
- [x] AC2 — Watchdog enabled in production (ThisAddIn.cs UiThread.Init(monitorUiThread: true); responder wired lazily from globals.StoreDisable + WpfUiDispatcher).
- [x] AC3 — Attribution via static volatile CurrentStoreContext; three set/clear points (StoreWrapper.Init, StoresWrapper.AddOrRestoreStore, AppOlObjects.EmitPerStoreInboxAttribution).
- [x] AC4 — No new expensive/blocking COM reads; existing [Startup timing]/[loadinboxes] lines unchanged (additive wraps).
- [x] AC5 — Auto-disable immediately, then notify, in that order (StoreLockupResponder).
- [x] AC6 — Modeless three-button notification; injectable showAction; BeginInvoke; F1 buttons; no direct F3 call (MyBoxModeless).
- [x] AC7 — Guard: no context → no disable, no notify.
- [x] AC8 — Guard: already disabled → no duplicate.
- [x] AC9 — One [store-lockup] WARN line via injected sink; pure StoreLockupAttribution formatter.
- [x] AC10 — Determinism + full toolchain in order + coverage + <=500 lines.

## Verification summary

- csharpier check: EXIT 0 (1306 files clean).
- Analyzer Rebuild: EXIT 0, 75 warnings = P0-T7 baseline, 0 F4-file warnings.
- Nullable gate: EXIT 0 (no-op, no regression; F4 follows the repo's established pattern).
- vstest (coverage): EXIT 0, 4481 passed (+40 new F4 tests), 0 failed.
- New-code coverage per F4 file: CurrentStoreContext 92.3%, LockupStallDecider 100%,
  StoreLockupAttribution 100%, StoreLockupResponder 96.1%, ThreadMonitor 100%, MyBoxModeless 100%
  (aggregate 97.7%).
- Testable denominator (UtilitiesCS) 90.5% >= 80%; no regression (all first-party packages up
  same-methodology: UtilitiesCS 88.25%->88.41%, TaskMaster 66.53%->66.57%, overall 56.51%->56.69%).
