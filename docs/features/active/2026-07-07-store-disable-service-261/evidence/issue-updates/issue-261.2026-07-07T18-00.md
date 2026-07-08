# Issue #261 — Acceptance Criteria Status Mirror (P8-T8)

Timestamp: 2026-07-07T23-35

PostedAs: unknown
(This is a local evidence mirror of the updated spec.md §9 acceptance-criteria state. It has not been
posted to GitHub by the executor; the orchestrator owns issue/PR posting. If posted later, update
PostedAs and add the comment/issue URL.)

## Exact text (spec.md §9 checked state)

All 15 acceptance criteria for F1 (store-disable-service, #261) are delivered and checked off in
`docs/features/active/2026-07-07-store-disable-service-261/spec.md` §9:

- [x] AC1 — Persisted future-sessions list (`DisabledStoreIdentities` round-trips). — P2-T1, P7-T4
- [x] AC2 — Session-only set in-memory, not persisted, empty-not-null after deserialize. — P2-T2, P7-T4
- [x] AC3 — `StoreIdentity.Resolve` pure (+ guarded COM overload). — P1-T1, P1-T2, P7-T1
- [x] AC4 — `IStoreDisableService` on `IApplicationGlobals.StoreDisable`, built in `LoadBasicMethod()`. — P1-T3, P5-T1, P6-T1, P6-T2
- [x] AC5 — Disable positive flows (both scopes). — P5-T2, P5-T3, P5-T5, P7-T2
- [x] AC6 — Persistence trigger (future serializes; session does not). — P5-T2, P5-T3, P7-T2
- [x] AC7 — Idempotency (session + future). — P5-T2, P5-T3, P7-T2
- [x] AC8 — `ReenableAsync` clears both scopes, persists conditionally. — P5-T4, P7-T2
- [x] AC9 — Staged rehook seam (no-op default; clear-before-rehook ordering). — P1-T4, P5-T4, P7-T2
- [x] AC10 — `GetDisabledStores` scope + de-duplication. — P5-T6, P7-T2
- [x] AC11 — Identity validation (`ArgumentException`). — P5-T2, P5-T3, P5-T4, P7-T2
- [x] AC12 — Attribution `Disabled` checked last; existing byte-for-byte unchanged. — P3-T1, P3-T2, P7-T3
- [x] AC13 — Filter integration across all three surfaces. — P2-T3, P4-T1, P4-T2, P4-T3, P7-T4
- [x] AC14 — Null-model safety on reads. — P5-T5, P5-T6, P7-T2
- [x] AC15 — Toolchain + coverage + 500-line cap for new files. — P8-T1..P8-T6

## Verification summary

- All 5032 MSTest tests pass (baseline 4995; +37 new).
- Repo line coverage 81.08% (>= 80%); new-code StoreIdentity 100%, StoreDisableService 97.92%.
- csharpier clean, analyzers 0 new diagnostics, nullable/TreatWarningsAsErrors green.
