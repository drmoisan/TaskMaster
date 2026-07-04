# Code Review: QuickFiler High-Confidence Dequeue Streaming (#233)

**Review Date:** 2026-07-04T10-53
**Reviewer:** Codex
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Feature Folder Selection Rule:** Supplied active feature folder for canonical issue #233.
**Base Branch:** `main`
**Head Branch:** `feature/quickfiler-high-confidence-dequeue-streaming-233`
**Review Type:** Full feature branch review

## Executive Summary

The branch moves high-confidence filtering toward a dequeue-layer gate and adds tests for dequeue-time scoring, sparse qualifying candidates, source exhaustion, boundary inclusivity, disabled-mode parity, and first-page high-confidence startup. The main production design is consistent with the issue #233 direction: queue admission no longer rejects below-threshold candidates, `QfcStreamingDequeueConfidenceGate` scores at dequeue time, and high-confidence initial page loading routes through dequeue rather than loading an unfiltered fixed batch and trimming afterward.

The implementation is not ready for normal PR flow. There is one policy blocker from AC10 coverage and one major test-quality finding: several changed unit tests assert production source text by reading `.cs` files from disk. Those tests are brittle and should be replaced by behavior tests or converted into feature-audit/search evidence outside the unit-test suite.

**What changed:**
The branch adds `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, changes datamodel queue processing to use it when high-confidence mode is enabled, removes live post-display high-confidence removal from the mail-item load path, adjusts first-page high-confidence loading, and expands MSTest coverage across QuickFiler queue and controller seams.

**Top 3 risks:**
1. AC10 remains failed because repository-path C# coverage is 22.87%, below the 80% floor.
2. Source-text tests create brittle implementation coupling and filesystem dependence in unit tests.
3. Live PR and CI status could not be verified because GitHub CLI is unavailable in this environment.

**PR readiness recommendation:** **Blocked** - coverage policy and test-quality remediation are required before merge readiness.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` | AC10 | AC10 remains unchecked and failed because repository-path C# coverage is 22.87%, below the required 80% floor. | Raise repository-path C# coverage to the required floor or record an approved repository exception that explicitly authorizes AC10 disposition. | Repository policy requires explicit PASS/FAIL coverage verdicts and does not allow coverage to be treated as not applicable when C# files changed. | `evidence/qa-gates/remediation-22-18-coverage-comparison.md`; current review VSTest execution passed 387/387 but did not emit a new coverage attachment. |
| Major | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | lines 22-37 and 133 | Changed tests read production source files from disk and assert implementation strings for logging and gate construction. | Replace source-text assertions with behavior tests that exercise the public or internal seam, or move repository-wide source-search checks into feature audit evidence instead of unit tests. | Unit tests should verify behavior and avoid filesystem-dependent implementation checks. These tests can pass while behavior regresses or fail after harmless refactoring. | `Select-String` found `File.ReadAllText`, `AppDomain.CurrentDomain.BaseDirectory`, and `ReadControllerSource` in changed test files. |
| Major | `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | lines 35-44 | The changed test file adds another source-file reading helper that is not used by behavioral assertions in the inspected diff. | Remove the unused helper, or replace it with direct behavior coverage if a missing behavior needs verification. | Dead or unused test helpers add maintenance cost and preserve the same filesystem dependency pattern. | Diff inspection and `Select-String` output for `ReadControllerSource`. |

## Implementation Audit

### C# implementation audit

#### What changed well

- The new `QfcStreamingDequeueConfidenceGate` keeps the high-confidence scoring/backfill behavior in a focused internal type.
- Queue admission now adds and hooks candidates without rejecting below-threshold items up front, matching dequeue-time scoring requirements.
- First-page high-confidence loading uses dequeue output rather than loading an unfiltered batch and applying UI removal.
- Disabled-mode behavior is explicitly preserved by direct dequeue paths and tests.

#### Type safety and API notes

- Analyzer and nullable builds both passed with 0 warnings and 0 errors.
- The public interface surface was kept stable except for documentation clarifying that `RemoveBelowThresholdAsync` is not the live issue #233 gate.
- The `QfcRemainingQueueAdmission` constructor still accepts `globals` and `scoreLoader`; `scoreLoader` is now only null-checked. This appears to preserve constructor compatibility, but it should be considered for cleanup after the feature is accepted.

#### Error handling and logging

- Cancellation is checked in the gate loop before taking candidates and after scoring.
- Dequeue-time score logging includes caller context, subject, entry ID, and score.
- The log path reads `MailItem.Subject` and `MailItem.EntryID` during dequeue logging; this is consistent with existing diagnostic intent, but it remains an Outlook COM-bound access point.

## Test Quality Audit

The branch has broad targeted MSTest coverage for the issue #233 behaviors and a current full QuickFiler test run passed 387/387. The strongest tests exercise dequeue-time score selection, sparse candidate backfill, source exhaustion, inclusive threshold comparison, disabled-mode parity, startup routing, and no post-display removal.

The test suite also includes source-text assertions that should not remain as unit tests. Those checks are better expressed as feature-audit search evidence because they verify source shape rather than behavior.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` - verifies the new gate behavior through a narrow seam.
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` - verifies first-page high-confidence routing through dequeue.
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md` - records no regression and 95.00% focused new gate coverage, but repository-path coverage remains 22.87%.
- Current review VSTest run - 387 passed, 0 failed.

### Quality assessment prompts

- **Determinism:** Most tests use mocks and `FakeTimeProvider`; source-text tests depend on repository file layout.
- **Isolation:** Gate tests are well isolated. Source-text tests are not isolated from source file formatting and implementation naming.
- **Speed:** Current VSTest run completed in 6.2545 seconds.
- **Diagnostics:** Behavioral tests have clear assertions; source-text tests would report string-presence failures rather than behavior failures.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection did not show secrets or credential material in reviewed C# paths. |
| No unsafe subprocess or command construction | PASS | Reviewed C# implementation paths do not add subprocess execution. |
| Input validation at boundaries | PASS | Gate handles non-positive quantity by returning an empty list and checks cancellation. |
| Error handling remains explicit | PASS | Datamodel unhook errors are logged and rethrown as before; cancellation is propagated. |
| Configuration / path handling is safe | PARTIAL | Production path handling is not materially changed; changed tests add filesystem source reads that should be removed or moved to audit evidence. |

## Research Log

No external research was required. The review used repository policy files, canonical PR context artifacts, feature evidence, direct diff inspection, and local toolchain output.

## Verdict

The production design appears aligned with the issue #233 feature requirements, and the current toolchain execution is clean except for coverage policy. The branch is blocked for PR readiness because AC10 remains failed and because changed tests include brittle source-text assertions. Remediation should keep the production behavior intact, replace source-text unit tests with behavioral tests or audit evidence, and resolve AC10 through coverage improvement or an approved exception.
