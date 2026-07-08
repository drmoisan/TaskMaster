# Code Review — Store Disable Service (F1, Issue #261) — Remediation Cycle 1 Reaudit

- Timestamp: 2026-07-08T04-42
- Reviewer: feature-reviewer
- Feature branch: `feature/store-disable-service-261` @ HEAD `8e11614e`
- Base (merge-base): `8bd91d1d`
- Diff scope: `git diff 8bd91d1d..HEAD` (full branch-vs-base diff, both commits)
- Prior-cycle code review reference: `code-review.2026-07-07T23-46.md`

## Executive Summary

The remediation commit (`8e11614e`) makes exactly two changes to test code and one build-file edit,
all narrowly scoped to the two findings raised in the entry-cycle review:

1. Extracted 11 `[TestMethod]`s (6 `InclusionFilters_*` filter tests + 5 disabled-store filter/
   serialization tests) plus their shared helper methods from `StoresWrapperTests.cs` into a new
   file `StoresWrapperDisableTests.cs`, wired into `UtilitiesCS.Test.csproj`.
2. Converted two `ReenableAsync` guard tests from `public void` to `public async Task` and added
   `await` before each `.ThrowAsync<...>()` assertion.

Both changes are verified correct, complete, and free of collateral changes. No new code-quality
finding is introduced by the remediation. The production code (`UtilitiesCS/OutlookObjects/Store/*`)
is untouched by this remediation cycle — this review's assessment of production-code quality is
unchanged from the entry-cycle code review and is not repeated in full here except where the
remediation's scope requires re-verification (test-file structure, test-method fidelity).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved | UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs, StoresWrapperDisableTests.cs | whole files | (Was Blocking, R1) 688-line file exceeded the 500-line cap. Now split into 415 and 368 lines. | None — resolved. | Independently confirmed via `wc -l`; `[TestMethod]` count preserved (22 -> 11 + 11); zero deleted lines via normalized-content diff. | `wc -l` output this cycle; `git diff 88366ad4..8e11614e --stat`. |
| Resolved | UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs | lines 212, 226, 248, 261 | (Was Non-blocking, N1) `ReenableAsync` exception assertions used unawaited `ThrowAsync<...>()`, never executing. Now `async Task` with `await`ed assertions. | None — resolved. | Independently confirmed via `git diff 88366ad4..8e11614e -- .../StoreDisableServiceTests.cs`: exactly the two signature changes and two added `await` keywords, no other change. vstest reports both as timed `Passed`. | `git diff` output this cycle; `evidence/qa-gates/qa-08-n1-verification-cycle1.md`. |
| Advisory (carried forward, unchanged) | UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs | static `StoreIsIncluded` signature | Public static method gained a trailing `bool isDisabled` parameter — a breaking signature change. | Acceptable as-is; keep the call-out. | Not touched by the remediation; disposition unchanged from entry cycle (no non-test caller in-repo). | grep of `StoreIsIncluded`; spec.md §6. |
| Advisory (carried forward, unchanged) | UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs | lines 91-95, 98-102 | Two empty `catch { }` blocks swallow all exceptions around guarded COM reads. | Narrow to specific COM exception type or add debug-level log. | Not touched by the remediation; disposition unchanged. | StoreIdentity.cs; spec §3.3/§7. |
| Advisory (carried forward, unchanged) | UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs | lines 93-95, 117-119 | Persisted-scope membership uses `List<string>` linear scans. | Acceptable for expected small list size; no change needed. | Not touched by the remediation; disposition unchanged. | StoreDisableService.cs. |
| Advisory (carried forward, unchanged) | UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs | line 30 | `_rehook = rehook ?? new NoOpStoreRehookService();` allocates a no-op per instance. | Optional shared static instance. | Not touched by the remediation; negligible impact. | StoreDisableService.cs. |

## Verification of the Split (StoresWrapperTests.cs / StoresWrapperDisableTests.cs)

- **No test logic altered.** This reviewer independently reproduced the "moved verbatim" claim
  (rather than accepting the remediation evidence at face value): concatenating the two post-split
  files and diffing (content-normalized: `using`/`namespace`/brace-only lines stripped, then sorted)
  against the same normalization of the pre-split file (`git show 88366ad4:...StoresWrapperTests.cs`)
  produces **zero deleted lines** — every line of the original file is present in the union of the
  two new files. The only additions are lines belonging to shared test-helper methods
  (`CreateStore`, `CreateGlobals`, `AssertInclusionDecision`, and the private nested `FakeStoresCollection`
  set-up) that necessarily now appear once in each file since both files need them independently.
  This is the expected and correct shape of a mechanical test-file split, not a sign of duplicated
  or diverged test logic.
- **Test count preserved.** `grep -c '\[TestMethod\]'` reports 22 in the pre-split file and 11 + 11
  = 22 across the two post-split files.
- **Namespace and using-directive hygiene.** `StoresWrapperDisableTests.cs` declares
  `namespace UtilitiesCS.Test.OutlookObjects.Store` (matching its sibling) and its own complete
  `using` block; no missing imports (build succeeds per `evidence/qa-gates/qa-02-analyzers-cycle1.md`
  and `qa-03-nullable-cycle1.md`).
- **Project wiring.** `UtilitiesCS.Test.csproj` gained exactly one new
  `<Compile Include="OutlookObjects\Store\StoresWrapperDisableTests.cs" />` item; no other csproj
  entries were touched by the remediation.

## Verification of the N1 Fix (StoreDisableServiceTests.cs)

- `Writes_ThrowArgumentException_ForSentinelIdentity` and `Writes_ThrowInvalidOperation_WhenModelIsNull`
  changed from `public void` to `public async Task`; the pre-existing `service.Invoking(...).Should().ThrowAsync<...>()`
  statement is now prefixed with `await`.
- The file already imported `System.Threading.Tasks` prior to this change (needed elsewhere in the
  file for other `async Task` test methods), so no new `using` directive was required.
- No other line in the file was touched by the remediation (confirmed via `git diff`), so the
  surrounding synchronous `.Should().Throw<ArgumentException>()` / `.Throw<InvalidOperationException>()`
  assertions for `DisableSessionOnly`/`DisableForFutureSessions` are unaffected.
- Effective verification: vstest reports both methods as individually timed `Passed` results
  (`evidence/qa-gates/qa-08-n1-verification-cycle1.md`), which is only possible for an `async Task`
  test method if MSTest awaited the returned task before recording the outcome — confirming the
  `ReenableAsync` guard-path assertions now genuinely execute.

## Test-Design Assessment (Reconfirmed)

Tests remain deterministic (injectable never-fired timer seam, no sleeps/real timers), mock-based
(no live Outlook, no temp files), AAA-structured, and use FluentAssertions with reason strings. The
remediation introduces no new test-design defect. The single defect identified in the entry-cycle
review (N1) is fixed; no other test-quality issue was found in the remediation diff.

## Scope Discipline

The remediation commit (`8e11614e`) touches exactly: `StoreDisableServiceTests.cs` (2 signature
changes), `StoresWrapperTests.cs` (11 test methods + helpers removed), `StoresWrapperDisableTests.cs`
(new file, 368 lines), and `UtilitiesCS.Test.csproj` (1 new `<Compile Include>` line), plus
documentation/evidence files under `docs/features/active/2026-07-07-store-disable-service-261/`.
No production file (`UtilitiesCS/OutlookObjects/Store/*`, `TaskMaster/AppGlobals/*`, etc.) was
touched by the remediation. This matches the narrow, in-scope remediation instruction from
`remediation-inputs.2026-07-07T23-46.md` exactly — no scope creep.

## Verdict

No Blocking or Non-blocking code-quality findings remain from this review. All Advisory items are
carried forward unchanged from the entry-cycle review (none touched by the remediation, none
gating).
