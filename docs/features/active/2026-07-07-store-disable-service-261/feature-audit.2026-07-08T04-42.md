# Feature Audit — Store Disable Service (F1, Issue #261) — Remediation Cycle 1 Reaudit

- Timestamp: 2026-07-08T04-42
- Reviewer: feature-reviewer
- Work mode: `full-feature` (AC sources: `spec.md` §9 AC1-AC15 + `user-story.md`)
- Feature branch: `feature/store-disable-service-261` @ HEAD `8e11614e`
- Base (merge-base): `8bd91d1d`
- Prior-cycle feature audit reference: `feature-audit.2026-07-07T23-46.md`

## Scope and Baseline

The audit scope is the full branch diff `git diff 8bd91d1d..HEAD` against the epic integration
base, covering both the original feature commit `88366ad4` and the remediation cycle-1 commit
`8e11614e`. Changes are C# production + tests in `UtilitiesCS`, `TaskMaster`, `QuickFiler.Test`,
`TaskMaster.Test`, `UtilitiesCS.Test`, plus docs/evidence. Test count is unchanged across the
remediation (5032 total both before and after `8e11614e`); the remediation moves 11 existing test
methods into a new file and fixes two previously-inert assertions — it adds no new test methods and
no production code. AC1-AC15 are evaluated against the delivered branch relative to this baseline,
using `spec.md` §9 as the authoritative AC text.

## Acceptance Criteria Inventory

Source: `spec.md` §9 (AC1-AC15). All 15 are checkbox items and are currently marked `[x]` in
`spec.md`. `user-story.md` restates the same criteria in outcome terms and defers to `spec.md` §9
for the testable form; they are covered by the AC1-AC15 evaluation below.

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

AC1-AC14 are unchanged by the remediation cycle (no production file was touched by `8e11614e`);
their entry-cycle PASS verdicts are reconfirmed by inspection of `git diff 8bd91d1d..HEAD` showing
no further change to any of the files those criteria depend on. AC15 is reassessed in full because
it is the criterion the remediation targeted.

| AC | Verdict | Evidence |
|----|---------|----------|
| AC1 | PASS (reconfirmed, unchanged) | `StoresWrapper.DisabledStoreIdentities` `[JsonProperty] List<string> = []`; round-trip test unaffected by the remediation (moved, not altered). |
| AC2 | PASS (reconfirmed, unchanged) | `SessionDisabledStoreIdentities` `[JsonIgnore] HashSet<string>`; same round-trip test. |
| AC3 | PASS (reconfirmed, unchanged) | `StoreIdentity.Resolve` pure + COM overload; `StoreIdentityTests.cs` untouched by remediation. |
| AC4 | PASS (reconfirmed, unchanged) | `IApplicationGlobals.StoreDisable`; `ApplicationGlobals.cs` untouched by remediation. |
| AC5 | PASS (reconfirmed, unchanged) | Positive-flow tests moved into `StoresWrapperDisableTests.cs` (filter-level) / remain in `StoreDisableServiceTests.cs` (service-level, untouched); both pass per cycle-1 vstest evidence. |
| AC6 | PASS (reconfirmed, unchanged) | Persistence-trigger tests in `StoreDisableServiceTests.cs`, untouched by remediation except the two N1 signature fixes (which do not affect AC6's tests). |
| AC7 | PASS (reconfirmed, unchanged) | Idempotency tests untouched by remediation. |
| AC8 | PASS (reconfirmed, unchanged) | `ReenableAsync` clear-both/conditional-serialize tests untouched by remediation. |
| AC9 | PASS (reconfirmed, unchanged) | Staged-rehook-seam tests untouched by remediation. |
| AC10 | PASS (reconfirmed, unchanged) | `GetDisabledStores` de-dup tests untouched by remediation. |
| AC11 | **PASS (upgraded from "PASS with test-quality caveat")** | The entry-cycle audit noted the `ReenableAsync` branch of AC11 was behaviorally satisfied but its two throw-assertions were unawaited and did not execute. The N1 fix converts both to `async Task` with `await`ed `ThrowAsync<...>()`; both now report individually timed `Passed` results (`evidence/qa-gates/qa-08-n1-verification-cycle1.md`). AC11 is now fully verified across all three write-path surfaces (`DisableSessionOnly`, `DisableForFutureSessions`, `ReenableAsync`), with no remaining caveat. |
| AC12 | PASS (reconfirmed, unchanged) | `StoreFilterAttribution.Decide`/enum-order tests untouched by remediation. |
| AC13 | PASS (reconfirmed, unchanged) | The three filter-surface tests (`ShouldIncludeStore_Excludes*`, `StoreIsIncluded_WhenIsDisabledTrue_*`, `Init_ExcludesSessionAndFutureDisabledStores_*`) were moved (not altered) into `StoresWrapperDisableTests.cs`; all pass per cycle-1 vstest evidence (`evidence/qa-gates/qa-04-mstest-cycle1.md`, "5 moved disabled-store tests ... all reported Passed"). |
| AC14 | PASS (reconfirmed, unchanged) | Null-model-safety test untouched by remediation. |
| AC15 | **PASS (upgraded from PARTIAL)** | All four sub-clauses now fully satisfied: (1) toolchain green — csharpier check clean (`qa-01-format-cycle1.md`), analyzers 0 errors/20 pre-existing unrelated warnings (`qa-02-analyzers-cycle1.md`), nullable/TreatWarningsAsErrors 0/0 (`qa-03-nullable-cycle1.md`), MSTest all directly-affected tests passing (`qa-04-mstest-cycle1.md`); (2) new-code coverage >= 90% (StoreIdentity.cs 100%, StoreDisableService.cs 97.92%, DisabledStoreEntry 100%, unchanged since the remediation added no production code); (3) no repo-wide regression — coverage 81.62% -> 81.61% (noise, not a regression), test count unchanged at 5032 (`qa-06-coverage-delta-cycle1.md`); (4) **all touched files remain under 500 lines** — `StoresWrapperTests.cs` = 415 lines, `StoresWrapperDisableTests.cs` = 368 lines, both independently confirmed via `wc -l` by this reviewer. The file-size sub-clause that drove the entry-cycle PARTIAL verdict is now met. |

## Pre-Existing, Out-of-Scope Test Failure (Not an AC Gate)

The full-suite run shows 5031 passed / 1 failed (of 5032 total), both before and after the
remediation edits. The single failure,
`TaskMaster.Test.AppGlobals.LiveOutlookHookupIntegrationTests.LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`,
is a live-Outlook COM/STA integration test unrelated to any file this feature's diff touches. This
reviewer independently confirmed the failure is environment-dependent rather than code-caused: the
entry-cycle audit ran the full suite at commit `88366ad4` and observed 0 failures, while the
cycle-1 baseline re-ran the full suite at the identical commit `88366ad4` and observed this one
failure — the same commit producing different outcomes for the same test is direct evidence of
local COM-server-availability variance, not a defect in this feature's diff. It is not an AC
criterion (no AC1-AC15 references live-Outlook hookup behavior) and is **not treated as a gating
condition for this feature's acceptance**. See `policy-audit.2026-07-08T04-42.md` for the full
disposition.

## AC Check-off

- AC1-AC14: PASS. Already `[x]` in `spec.md`; verdicts confirm the check-offs (no change needed).
- AC15: now assessed **PASS** (previously PARTIAL). It is currently marked `[x]` in `spec.md`; the
  file-size sub-clause that made the prior `[x]` premature is now genuinely satisfied. Per the
  acceptance-criteria-tracking protocol, no edit to `spec.md` is required this cycle — the existing
  `[x]` for AC15 is now fully backed by evidence and does not need to change from `[x]` to `[x]`.
  This reviewer made no edits to `spec.md` (its `[x]` markers for AC1-AC15 were already correct in
  intent and are now correct in fact for AC15 as well).
- No new AC items were added (no phantom criteria).

## Summary

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-07-store-disable-service-261/spec.md` §9 (+ `user-story.md`)
- Total AC items: 15
- Checked off (delivered): 15 (AC1-AC15, all fully satisfied)
- Remaining (unchecked / not fully met): 0
- Items remaining: none.

Overall feature verdict: **PASS** — all 15 acceptance criteria are fully met following remediation
cycle 1. The one Blocking finding (R1, file-size) and one Non-blocking finding (N1, unawaited async
assertions) from the entry-cycle review are both independently confirmed resolved with no new
findings introduced.
