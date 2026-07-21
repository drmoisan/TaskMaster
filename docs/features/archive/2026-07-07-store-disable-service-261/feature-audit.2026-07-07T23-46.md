# Feature Audit — Store Disable Service (F1, Issue #261)

- Timestamp: 2026-07-07T23-46
- Reviewer: feature-reviewer
- Work mode: `full-feature` (AC sources: `spec.md` §9 AC1-AC15 + `user-story.md`)
- Feature branch: `feature/store-disable-service-261` @ HEAD `88366ad4`
- Base (merge-base): `8bd91d1d`

## Scope and Baseline

The audit scope is the full branch diff `git diff 8bd91d1d..HEAD` against the epic integration
base. Changes are C# production + tests in `UtilitiesCS`, `TaskMaster`, `QuickFiler.Test`,
`TaskMaster.Test`, `UtilitiesCS.Test`, plus docs/evidence. Baseline test count 4995 -> 5032
(+37 new tests), all passing. AC1-AC15 are evaluated against the delivered branch relative to this
baseline, using spec.md §9 as the authoritative AC text.

## Acceptance Criteria Inventory

Source: `spec.md` §9 (AC1-AC15). All 15 are checkbox items and are currently marked `[x]` in
`spec.md`. `user-story.md` restates the same criteria in outcome terms (7 unchecked outcome bullets)
and defers to spec.md §9 for the testable form; they are covered by the AC1-AC15 evaluation below.

| AC | Summary |
|----|---------|
| AC1 | Persisted `DisabledStoreIdentities` `[JsonProperty]` round-trips |
| AC2 | Session-only `[JsonIgnore]` set in-memory, not persisted, empty-not-null after deserialize |
| AC3 | `StoreIdentity.Resolve` pure resolver + COM overload |
| AC4 | `IStoreDisableService` on `IApplicationGlobals.StoreDisable`, constructed in `LoadBasicMethod()` |
| AC5 | Disable positive flows (both scopes) |
| AC6 | Persistence trigger (future serializes; session does not) |
| AC7 | Idempotency (double-disable) |
| AC8 | `ReenableAsync` clears both scopes; conditional single serialize |
| AC9 | Staged rehook seam (clear-before-rehook; no-op default) |
| AC10 | `GetDisabledStores` scope + both-scope de-dup as FutureSessions |
| AC11 | Identity validation (`ArgumentException`); reads do not throw |
| AC12 | Attribution `Disabled` checked last; existing byte-for-byte unchanged |
| AC13 | Filter integration across all three surfaces |
| AC14 | Null-model safety on reads |
| AC15 | Toolchain + coverage + 500-line cap |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|----|---------|----------|
| AC1 | PASS | `StoresWrapper.DisabledStoreIdentities` `[JsonProperty] List<string> = []`. Test `Serialization_RoundTrip_PreservesDisabledListAndOmitsSessionSet` asserts JSON contains `DisabledStoreIdentities`/`PersistedStore` and round-trips. |
| AC2 | PASS | `SessionDisabledStoreIdentities` `[JsonIgnore] HashSet<string>` (OrdinalIgnoreCase, field-initialized). Same test asserts JSON omits the session field and the set is empty-not-null after deserialize. |
| AC3 | PASS | `StoreIdentity.Resolve(string,string)` pure (DisplayName primary, fallback, sentinel, casing preserved) + `Resolve(Outlook.Store)` COM overload. `StoreIdentityTests` (100% cov) covers all branches incl. FilePath-throws and both-throw. |
| AC4 | PASS | `IApplicationGlobals.StoreDisable` returns `IStoreDisableService` with the five §4.2 methods; `ApplicationGlobals` constructs `new StoreDisableService(this)` inside `LoadBasicMethod()` (line 118) and reads the model lazily per call. |
| AC5 | PASS | `DisableSessionOnly_AddsToSessionSetOnly_AndDoesNotPersist`; `DisableForFutureSessions_RendersStoreDisabledForCurrentSessionViaUnion`. `IsDisabled` true after each. |
| AC6 | PASS | `DisableForFutureSessions_AddsToPersistedList_AndSerializesOnce` (timer StartCount==1); session test asserts StartCount==0. Observed via injectable-timer seam. |
| AC7 | PASS | `DisableSessionOnly_CalledTwice_IsIdempotent`; `DisableForFutureSessions_CalledTwice_NoDuplicateAndNoSecondSerialize`. |
| AC8 | PASS | `ReenableAsync_WhenDisabledInBothScopes_ClearsBothAndSerializesOnce` (StartCount==1); `ReenableAsync_WhenNotDisabled_SerializesZeroTimesButStillAwaitsRehook` (StartCount==0). |
| AC9 | PASS | `clearedBeforeRehook` callback confirms state cleared before `RehookAsync` awaited (Times.Once); `ReenableAsync_WithNoOpDefaultRehook_LeavesStateClearedAndCompletes`. |
| AC10 | PASS | `GetDisabledStores_ReportsScopes_AndDeDuplicatesBothScopesAsFutureSessions` (3 entries; both-scope reported once as FutureSessions); null-model returns empty. |
| AC11 | PASS (with test-quality caveat) | `Writes_ThrowArgumentException_ForSentinelIdentity` and `_ForDefaultUnresolvedIdentity` verify the two synchronous write methods; `Reads_AreSafeAndEmpty_WhenModelIsNull` confirms reads do not throw. Behavior for `ReenableAsync` is correct (shared `ValidateIdentity` runs first), but its throw assertions are unawaited `ThrowAsync` (do not execute) — see code-review Non-blocking finding. The AC behavior is satisfied; verification of the `ReenableAsync` branch specifically is incomplete. |
| AC12 | PASS | `StoreFilterAttribution.Decide` adds `isDisabled` after the four existing checks, before `Included`; enum inserts `Disabled` before `Included`. Tests: `Decide_WhenDisabledAndNoEarlierRuleMatches_ReturnsDisabled` plus four "keeps existing rule" tests + `StoreFilterRule_EnumOrder_...`. |
| AC13 | PASS | All three surfaces patched and tested: `ShouldIncludeStore_Excludes{Session,Future}DisabledStore_KeepsNonDisabled`; `StoreIsIncluded_WhenIsDisabledTrue_ReturnsFalse`; `Init_ExcludesSessionAndFutureDisabledStores_ViaInstrumentedPath`. |
| AC14 | PASS | `Reads_AreSafeAndEmpty_WhenModelIsNull`: `IsDisabled` false, `GetDisabledStores` non-null empty when model null. |
| AC15 | **PARTIAL** | Toolchain green (csharpier check clean, analyzers 0 errors/70 pre-existing warnings, nullable 0/0) and coverage meets CLAUDE.md policy (repo 81.08%, new-code >= 90%, no regression). However the clause "all touched files remain under 500 lines" is NOT met: `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` is 688 lines (baseline 563; this diff added ~125). See policy-audit §5 (Blocking). |

## AC Check-off

- AC1-AC14: PASS. Already `[x]` in `spec.md`; verdicts confirm the check-offs.
- AC15: assessed **PARTIAL**. It is currently marked `[x]` in `spec.md`, but the file-size sub-clause
  is not satisfied. Per the acceptance-criteria-tracking protocol a PARTIAL item should not be
  checked. This reviewer did not modify `spec.md` (review-only). Recommendation: after the narrow
  file-size remediation (extract the added disabled-store tests into a new file), AC15 is fully
  satisfied and the `[x]` is correct; until then the AC15 check-off is premature and is documented
  here as a gap.
- No new AC items were added (no phantom criteria).

## Summary

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-07-store-disable-service-261/spec.md` §9 (+ `user-story.md`)
- Total AC items: 15
- Checked off (delivered): 14 fully satisfied (AC1-AC14); AC15 is marked `[x]` but assessed PARTIAL
- Remaining (unchecked / not fully met): 1 (AC15 — file-size sub-clause)
- Items remaining: AC15 (Toolchain and coverage) — toolchain and coverage pass; the "all touched
  files remain under 500 lines" clause fails due to `StoresWrapperTests.cs` at 688 lines.

Overall feature verdict: **PARTIAL** — 14 of 15 ACs fully met; AC15 partial pending the narrow
file-size remediation.
