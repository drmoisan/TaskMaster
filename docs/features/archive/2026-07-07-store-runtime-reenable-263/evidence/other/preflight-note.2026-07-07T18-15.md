# F3 Preflight Validation Note (#263)

- Timestamp: 2026-07-07T18-15
- Plan: docs/features/active/2026-07-07-store-runtime-reenable-263/plan.2026-07-07T18-00.md
- Directive: PREFLIGHT VALIDATION ONLY (planning-phase readiness check; no code/toolchain/Phase-0 execution)
- Result: REVISIONS REQUIRED (1 blocking defect)

## Wave context (acknowledged, not a defect)

F3 is wave 1 and depends on F1 (#261) and F2 (#262), which are NOT merged into this
planning worktree. The plan correctly references not-yet-present contracts
(`IStoreDisableService`, `StoreIdentity`, `IApplicationGlobals.StoreDisable`,
`AppOlObjects.StoreLoading.cs`) and gates them behind a fail-closed Phase-0 prerequisite
gate (P0-T7 F2, P0-T8 F1), each of which marks FAIL and halts if the upstream seam is
absent or divergent. Preflight does NOT fail on the absence of those upstream contracts.

## Grounding confirmed (existing seams F3 extracts)

- AppEvents.cs: `PerformReadinessHookup` (line 215) inbox `ForEach` loop; `OlInboxes` (line 150).
- AppOlObjects.cs: `LoadInboxes` (line 124); `EmitPerStoreInboxAttribution` delegate precedent (line 204).
- StoresWrapper.cs: `RewireOlObjectsAsync` per-store loop (lines 83-127) with Find/Init/Restore.
- OutlookReadinessGate.cs / IOutlookReadinessGate.cs: `IsReady()`, `IsTransientError(COMException)`.
- HookReadinessCoordinator.cs: run-once state machine; per-call construction safe.
- OutlookFolderNotificationSink.cs: frozen IReadOnlyList; per-store `AddFolderSubscriptions(Store,...)` (line 183).
- IOutlookFolderTreeService.MarkStale(string, FolderTreeRefreshReason); FolderTreeRefreshReason.StoreAdded.
- Test fixtures: FakeSubscription, SubscriptionCount, Mock<Items>, BuildInboxSubscriptions.
- Legacy explicit `<Compile Include>` wiring required (non-SDK packages.config projects).

## Structure / atomicity / design fidelity

- Canonical `### Phase N —` headings; sequential IDs per phase (P0-T1..T13, P1..P6). OK.
- Phase 0: policy reads + fail-closed prerequisite gate + baselines incl. coverage. OK.
- Final QA loop (Phase 6) format->analyzers->nullable->test with restart rule. OK.
- AC1-AC11 all mapped in traceability table. OK.
- Five-outcome enum, per-call new HookReadinessCoordinator, store-scoped IsReady overload,
  StoreID idempotency, F1->F3 call direction, no F3 dependency on IStoreDisableService. OK.
- Highest-risk COM `+=` tasks (P3-T3, P3-T5, P4-T2) isolated with Times.Never()/branch
  verification; no temp files, no live Outlook, no real timers. OK.
- Evidence paths all canonical (`.../evidence/<kind>/`); no forbidden paths. OK.

## Blocking defect

D1 (P3-T3 vs Scope Lock): P3-T3 requires editing `PerformReadinessHookup`'s inbox loop
body to call `SubscribeInboxForStore`. `PerformReadinessHookup` is physically defined in
`TaskMaster/AppGlobals/AppEvents.cs` (line 215); a partial cannot redefine an existing
method body elsewhere, so the edit MUST modify `AppEvents.cs`. `AppEvents.cs` is NOT in
the plan's "Modified production files" table, and the Scope Lock states "No production or
test file outside this scope lock is modified. If execution requires touching any other
file, execution halts and notifies the user." A verbatim executor would therefore halt at
P3-T3, preventing completion, and AC1's "each existing startup loop body ... call the same
per-store implementation" cannot be met for the AppEvents subsystem without this edit.

Fix: add `TaskMaster/AppGlobals/AppEvents.cs` to the "Modified production files" table and
correct P3-T3 to name `AppEvents.cs` as the definitive location of `PerformReadinessHookup`
(the `AppEvents.ReadinessHookup.cs` partial explicitly contains no PerformReadinessHookup logic).
