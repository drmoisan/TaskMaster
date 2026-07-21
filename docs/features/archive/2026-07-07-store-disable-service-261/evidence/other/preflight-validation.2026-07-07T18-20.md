# Preflight Validation — F1 store-disable-service (#261)

- Timestamp: 2026-07-07T18-20
- Plan: docs/features/active/2026-07-07-store-disable-service-261/plan.2026-07-07T18-00.md
- Directive: PREFLIGHT VALIDATION ONLY (no code changes, no toolchain, no Phase 0 execution)
- Result: PREFLIGHT: REVISIONS REQUIRED

## Structure (PASS)
- Canonical `### Phase N — <Title>` headings for Phase 0 through Phase 8.
- Sequential `[P#-T#]` IDs per phase (P0-T1..T11, P1..P8) with `- [ ] [P#-T#]` prefix.
- Phase 0 present with policy-read evidence (P0-T5) and baselines (git P0-T7, csharpier P0-T8, analyzers P0-T9, nullable P0-T10, test+coverage P0-T11 with numeric coverage).
- Final QA loop present (Phase 8) with format -> analyzers -> nullable -> test-coverage, restart rule, coverage delta (P8-T5), 500-line cap (P8-T6).
- Evidence paths all resolve to `<FEATURE>/evidence/<kind>/` (canonical); no forbidden `artifacts/*` evidence paths.
- Toolchain = CLAUDE.md 4-step; coverage = 80/90 (authoritative per directive). MSTest+Moq+FluentAssertions; no temp files; no live Outlook.

## Grounding (verified against current source)
- StoresWrapper.cs: Stores list, ExcludedStoreNameContains/ExcludedStoreFilePathContains/GwsoFilePathContains, ShouldIncludeStore (ends `return true;`), static StoreIsIncluded (ends `return true;`), ShouldIncludeStoreInstrumented (calls Decide with 8 args) all present and match plan claims.
- StoreFilterAttribution.cs: enum `PublicFolder, NameContains, GwsoFilePath, FilePathContains, Included`; Decide(...) with 8 params; trailing-param + Disabled-before-Included insertion is consistent.
- IApplicationGlobals.cs: members present; adding read-only `StoreDisable` is grounded.
- ApplicationGlobals.cs: LoadBasicMethod() (line 99) constructs sub-services; `public IOlObjects Ol => _olObjects;` (line 420). Grounded. File is 464 lines (adds ~4 lines; under 500).
- UtilitiesCS.csproj and UtilitiesCS.Test.csproj use explicit `<Compile Include>` (no glob) -> new-file csproj wiring tasks are warranted.
- Test files to MODIFY exist and are wired: StoreFilterAttributionTests.cs, StoresWrapperTests.cs (CreateGlobalsWithStores helper at line 420).
- SmartSerializable provides parameterless Serialize() (line 426), SerializeToString() (line 487), DeserializeObject(json, settings) (line 398), and the protected TimerFactory/ITimerWrapper 3-second seam.
- StoreIsIncluded: only non-doc caller is UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs:408. Plan's "only current caller is a unit test" claim is grounded.

## Blocking defect: `init` / `record struct` does not compile on net48 (CS0518)
- Repo evidence: `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` (ResourceTimingRow, ~line 165) and `TaskMaster/AppGlobals/HookReadinessCoordinator.cs` (line 12) both state in-code that `System.Runtime.CompilerServices.IsExternalInit` is not available on this .NET Framework target (CS0518) and therefore use a plain `readonly struct` with a constructor and get-only properties.
- No IsExternalInit polyfill exists anywhere in the repo; no production `.cs` uses `{ get; init; }`.
- P1-T1 mandates `public readonly record struct StoreIdentity` with `public string Value { get; init; }`.
- P1-T3 mandates `public readonly record struct DisabledStoreEntry` with `Identity { get; init; }` and `Scope { get; init; }`.
- The `init` accessor is lowered with a `modreq(IsExternalInit)`; with IsExternalInit absent, these declarations produce CS0518 and fail the analyzer/nullable builds (P8-T2/T3) and even the P1 compile. The plan's mitigation note ("explicit body, not positional") is insufficient because the `init` accessor, not positional syntax, is the trigger.
- Note: this also conflicts with spec §3.1/§4.1, which prescribe `readonly record struct` with `init`; the spec shape is not compilable on net48 and must be reconciled with the repo's documented plain-`readonly struct` precedent.

## Re-check after revision (2026-07-07)

- Timestamp: 2026-07-07T18-20
- Directive: PREFLIGHT VALIDATION ONLY (re-check)
- Result: PREFLIGHT: ALL CLEAR

Verification of the four required fixes:
1. Neither plan nor spec still declares `record struct` or `{ get; init; }` for these types. Spec §3.1 (lines 68-79) and §4.1 (lines 139-156) declare both as plain `public readonly struct` with an ordinary constructor and get-only (`{ get; }`) properties; the only `record struct`/`init` tokens remaining in plan/spec are inside the intentional "NOT a `record struct` / NOT any `init`" constraint notes (plan lines 51, 71, 73, 100; spec lines 76, 78, 153, 154). The lone `readonly record struct` occurrence in the feature folder is in `research/...-research.md`, an upstream input artifact, not the plan or spec.
2. P5-T6 (plan line 100) constructs each entry via the constructor `new DisabledStoreEntry(identity, scope)` and explicitly forbids an object initializer.
3. P4-T3 (plan line 91) updates the sole `StoreIsIncluded` caller at `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs:408` with the trailing `isDisabled: false` argument so `UtilitiesCS.Test` compiles between Phase 4 and Phase 7. That file is in the test-file scope set (plan line 39).
4. No new inconsistencies: task IDs sequential per phase (P0-T1..T11, P1-T1..T4, P2-T1..T3, P3-T1..T2, P4-T1..T3, P5-T1..T6, P6-T1..T2, P7-T1..T4, P8-T1..T9); canonical `### Phase N — <Title>` headings for Phase 0-8; StoreIdentity.Resolve pure + COM overloads (P1-T1/T2); 5-method IStoreDisableService on member StoreDisable (P1-T3, P6-T1); no-op IStoreRehookService seam with no F3 dependency (P1-T4); Disabled reason checked last across all three surfaces (P3, P4-T1/T2/T3) — all intact. A new Spec Reconciliation note (plan line 51) records the binding plain-struct realization.
