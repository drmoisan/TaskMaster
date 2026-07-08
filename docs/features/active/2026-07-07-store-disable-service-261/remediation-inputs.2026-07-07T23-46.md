# Remediation Inputs — Store Disable Service (F1, Issue #261)

- Timestamp: 2026-07-07T23-46
- Reviewer: feature-reviewer
- Feature branch: `feature/store-disable-service-261` @ HEAD `88366ad4`
- Base (merge-base): `8bd91d1d`
- Source artifacts:
  - `docs/features/active/2026-07-07-store-disable-service-261/policy-audit.2026-07-07T23-46.md`
  - `docs/features/active/2026-07-07-store-disable-service-261/code-review.2026-07-07T23-46.md`
  - `docs/features/active/2026-07-07-store-disable-service-261/feature-audit.2026-07-07T23-46.md`

## Remediation-Required Findings (Blocking)

### R1 — File exceeds 500-line limit (Blocking)

- Rule violated: CLAUDE.md §4.1 ("Do not exceed 500 lines for any one file");
  `.claude/rules/general-code-change.md` ("No production code, test code, or reusable script file may
  exceed 500 lines").
- File + location: `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` — 688 lines
  (baseline `8bd91d1d`: 563 lines; this diff added ~125 lines via P7-T4).
- Impact: hard file-size limit violated; drives feature-audit AC15 to PARTIAL.
- Required action (narrow, in-scope): extract the newly-added disabled-store filter/serialization
  tests (the P7-T4 block, `ShouldIncludeStore_Excludes*`, `StoreIsIncluded_WhenIsDisabledTrue_*`,
  `Init_ExcludesSessionAndFutureDisabledStores_ViaInstrumentedPath`,
  `Serialization_RoundTrip_PreservesDisabledListAndOmitsSessionSet`) into a new test file
  (e.g., `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs`, added to the test
  `.csproj`), so the feature's added lines no longer sit in an over-limit file.
- Out of scope: remediating the pre-existing 563-line baseline of `StoresWrapperTests.cs` is repo-wide
  test-debt not attributable to F1; a separate refactor should address it. Preserve all existing test
  behavior; do not change assertions.
- Verification after fix: `wc -l` on both resulting files < 500; full C# toolchain green (csharpier
  check, analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage); test count and pass rate
  unchanged (5032 passing); then AC15 is fully satisfied.

## Recommended (Non-blocking, not gating merge)

### N1 — Unawaited async throw assertions (test-quality)

- File + location: `UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs` lines 226-229
  and 261-263.
- Issue: `.Should().ThrowAsync<ArgumentException>()` / `.ThrowAsync<InvalidOperationException>()` for
  `ReenableAsync` are not `await`ed, so the assertions never execute.
- Action: `await` each `ThrowAsync` assertion so the `ReenableAsync` guard paths are actually verified.
- Not Blocking: production behavior is correct (shared `ValidateIdentity`/null-model guards run first
  and are exercised by the two synchronous write methods).

## Handoff

- Blocking count requiring remediation before merge: 1 (R1).
- The Non-blocking item (N1) should be addressed in the same pass if the file is being edited, but does
  not gate merge on its own.
