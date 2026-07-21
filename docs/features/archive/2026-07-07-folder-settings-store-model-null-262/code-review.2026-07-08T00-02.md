# Code Review — Issue #262 (folder-settings-store-model-null), Epic #260 F2

- Feature folder: `docs/features/active/2026-07-07-folder-settings-store-model-null-262/`
- Branch: `bug/folder-settings-store-model-null-262`
- Base: `origin/epic/store-lockup-resilience-integration` (merge-base `8bd91d1d`)
- Timestamp: 2026-07-08T00-02
- Overall verdict: PASS (no blocking findings)

## Executive Summary

The implementation is a focused, well-structured bug fix. `LoadStoresAsync` is restructured so that
the two recoverable null paths (config missing, config deserializes to null) fall through to a single
`StoresWrapper = BuildFreshStoresWrapper()` assignment, and the whole method is wrapped in one bounded
`try/catch (Exception)` that logs at `Error` with the exception object attached and
`StoresWrapper`-specific, user-consequence context. The genuine-failure path deliberately leaves the
model null with no retry, preserving the existing readiness-guard dialog as the single user-facing
surface.

Design quality is consistent with the repo's conventions and the CLAUDE.md general/C# code-change
policies:

- Separation of concerns and file structure: the store-load pipeline is extracted into a new partial
  `AppOlObjects.StoreLoading.cs`, following the documented `AppOlObjects.JunkFolders.cs` precedent, to
  bring `AppOlObjects.cs` under the 500-line cap (495 lines). The new partial is 75 lines and carries
  a clear XML summary explaining the extraction rationale.
- Seam design: `BuildFreshStoresWrapper()` is `protected internal virtual`, mirroring the existing
  `AwaitStoreRewireAsync` convention, keeping the new surface non-public and testable without exposing
  public API. The fresh build uses `new StoresWrapper(_globals).Init()` so `GetFilteredStores()` can
  read the live namespace stores; `Init()` (synchronous) is chosen over `CreateAsync` because the
  method is already async, avoiding an unnecessary `Task.FromResult` wrap.
- Error handling and logging: the two recoverable branches log at `Warn` (handled, model still
  populated); the genuine failure logs at `Error` with the exception attached, replacing the previous
  bare-string `logger.Error("StoresWrapper config not found.")`. The single broad catch is at a clear
  method boundary with added context and no swallowing, which is the sanctioned pattern for a phase
  whose escape would abort downstream startup phases.
- Behavior preservation: the valid-config path (`deserialized is not null` -> assign ->
  `AwaitStoreRewireAsync` -> return) is byte-for-byte equivalent to the prior behavior; the fresh-build
  path correctly bypasses `AwaitStoreRewireAsync` (a fresh `Init()` build is complete synchronously).

Test quality is strong. The previously mis-specified
`LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing` is inverted in place to assert the fallback
(treating the existing test as part of the spec). Three regression tests cover Path 1/2/3, and a
fourth exercises the real (non-overridden) `BuildFreshStoresWrapper()` body through a fully mocked COM
store chain, reaching the new seam for coverage without a live Outlook or temp files. Assertions use
FluentAssertions with reason strings; the COM chain is built with Moq enumerator setups.

No defects requiring code change were identified. Two non-blocking observations are recorded below.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | (repo, not a source file) | `artifacts/csharp/coverage.xml` | Canonical C# coverage artifact not deposited; coverage evidenced via Cobertura runsettings in the evidence tree instead. | Deposit the canonical JaCoCo/coverage artifact in future cycles to enable automated hook-side gating. | The review coverage hook parses the canonical path; its absence disables automated per-language gating even though coverage was measured. | qa-04-test-coverage.md, qa-05-coverage-delta.md |
| Informational | `.github/workflows/ci.yml` | line 140 (unchanged by F2) | `vstest` is invoked without `/TestCaseFilter:"TestCategory!=LiveOutlook"`, contradicting the LiveOutlook test's doc comment that CI excludes that category. | Epic owner: confirm integration-branch CI is green and/or reconcile the CI filter with the test's documented exclusion. | ci.yml and the LiveOutlook test are outside F2's four-file scope and unchanged; a red CI here would be a pre-existing base-branch condition, not an F2 defect. | ci.yml:140, LiveOutlookHookupIntegrationTests.cs:25-26,88-89 |
| Informational | `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` | `BuildFreshStoresWrapper()` | The fresh build can throw on a COM error enumerating `Stores`; handled once by the method-level catch with no retry (matches spec intent). | None required. | Confirms the documented risk is addressed by the bounded catch; noted for traceability only. | spec.md Risks; source lines 30-31 |

## Notes on Positive Practices (for feedback continuity)

- The F1 (#261) cross-feature interaction is documented in a code comment at the fresh-build site,
  preventing later misattribution of a re-enabled store as an F1/F5 regression. This matches the spec
  mitigation and is a good use of "comment why, not what".
- Unused `using System.Threading.Tasks;` was trimmed from `AppOlObjects.cs` after the extraction,
  keeping imports explicit and minimal.

## Verdict

PASS. No blocking or change-required findings. The two observations above are non-blocking.
