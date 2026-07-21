# Preflight Validation — F2 (#262) plan.2026-07-07T18-00

- Timestamp: 2026-07-07T18-18
- Directive: PREFLIGHT VALIDATION ONLY (structure + readiness; no code changes, no toolchain runs, no Phase 0 execution)
- Plan: docs/features/active/2026-07-07-folder-settings-store-model-null-262/plan.2026-07-07T18-00.md
- Verdict: REVISIONS REQUIRED (1 defect)

## Structure (PASS)
- Canonical `### Phase N — <Title>` headings for Phases 0–5.
- Sequential IDs: P0-T1..T12, P1-T1..T4, P2-T1..T4, P3-T1..T4, P4-T1..T5, P5-T1..T5.
- Phase 0 policy reads (CLAUDE.md, general-code-change, general-unit-test, csharp) in order + baseline captures with Timestamp/Command/EXIT_CODE/Output Summary.
- Final QA loop present (Phase 4, restart rule P4-T1..T4) + coverage delta P4-T5.
- Evidence paths all resolve to `<FEATURE>/evidence/<kind>/`; no forbidden `artifacts/` paths.

## Bugfix ordering (PASS)
- Failing regression tests (Phase 2, all `[expect-fail]`) precede the behavioral fix (P3-T1).
- Existing test inversion P2-T1; Path 2 = P2-T2; Path 3 = P2-T3.
- Fail-before evidence P2-T4 (regression-testing/fail-before-262.md); pass-after P3-T3 (pass-after-262.md).
- Phase 1 (extraction + seam) is behavior-preserving, so Phase 2 tests still fail-before.

## Grounding (PASS)
- AppOlObjects.cs = 525 lines (over 500 cap). LoadStoresAsync :251-265; config-missing bare `logger.Error("StoresWrapper config not found.")` :263; AwaitStoreRewireAsync :246-249; StoresWrapper property :244; LoadAsync :34.
- Precedent AppOlObjects.JunkFolders.cs exists; TaskMaster.csproj uses explicit `<Compile Include>` (JunkFolders at line 413) — new partial needs wiring (P1-T2).
- Test seams exist: StubApplicationGlobals, StubIntelligenceConfig, TestableAppOlObjects; existing test LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing :75-87 asserts BeNull.
- StoresWrapper ctor(IApplicationGlobals) :29-33 and Init() :35-49 back the BuildFreshStoresWrapper seam.
- SmartSerializableBase.Deserialize<T,U>(SmartSerializable<U>) :166-187 throws ArgumentNullException (Path 3 throw site).
- StoreWrapperController.EvaluateLaunchReadiness :108 (ModelUnavailable/StoresUnavailable/Ready); "not available yet" dialog :136.

## Fix fidelity (PASS)
- Seam `protected internal virtual StoresWrapper BuildFreshStoresWrapper() => new StoresWrapper(_globals).Init();` matches available ctor+Init.
- Fresh-build fallback on both recoverable branches; bounded try/catch logs Error with exception; no retry; no new dialog.
- Scope lock: StoresWrapper.cs, IntelligenceConfig.cs, StoreWrapperController.cs not modified (P5-T2 verifies).

## Coverage / tone (PASS)
- CLAUDE.md 4-step toolchain (Phase 4); 80% repo / 90% new-code (P4-T4/P4-T5) per ratified repo policy.
- No temp files; no live Outlook (Moq seams).

## Defect (see final report)
- P5-T4 checks off ACs in issue.md (AC1–AC6, different numbering) instead of the authoritative full-bug AC source spec.md (AC1–AC7). Requires the delta stated in the preflight report.
