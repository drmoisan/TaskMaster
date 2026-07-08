# Code Review: appevents-loadasync-inbox-gating (Issue #243)

**Review Date:** 2026-07-06  
**Reviewer:** Codex feature-review  
**Feature Folder:** `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243`  
**Feature Folder Selection Rule:** User supplied active feature folder for issue #243; it also matches the branch suffix.  
**Base Branch:** `main`  
**Head Branch:** `bug/appevents-loadasync-inbox-gating-243` working tree; committed `HEAD` equals `main` at `961a768e0b093ec468c8180c9dc53996e1e6421a`  
**Review Type:** Initial feature review

## Executive Summary

The implementation addresses the issue #243 readiness ordering problem. In hooked-events startup, `LoadAsync()` no longer awaits `ProcessNewInboxItemsAsync()` before readiness. Startup inbox processing is invoked from `PerformReadinessHookup()` after `Globals.Ol.Inboxes` has been enumerated into `OlInboxes`, and the new helper observes faulted async processing instead of discarding the task.

No code-level correctness blocker was found in the reviewed ordering change. Remediation is still required because policy and evidence gates fail: repository-wide C# coverage is 8.9566%, below the 80% threshold and below the 79.9234% baseline, the required `artifacts/csharp/coverage.xml` path is absent, and two changed files exceed the repository 500-line limit.

**What changed:**
`TaskMaster/AppGlobals/AppEvents.cs` moves startup inbox processing for the hooked path from `LoadAsync()` into readiness hookup. `TaskMaster.Test/AppGlobals/AppEventsTests.cs` and `TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs` add focused ordering coverage. Feature evidence under the active folder records fail-before, post-fix, final QA, and coverage artifacts.

**Top 3 risks:**
1. C# coverage evidence fails the mandatory repository-wide gate and may indicate the planned coverage command is not measuring the same surface as the baseline.
2. Changed files over 500 lines violate repository module/file structure policy.
3. Coverage tooling required a workaround and the broad baseline-comparable rerun timed out, so coverage verification needs remediation before PR readiness.

**PR readiness recommendation:** **Needs Revision** - behavior appears correct, but policy gates require remediation.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/csharp-coverage-delta.2026-07-06T11-02.md` | Output Summary | C# repository-wide coverage fails: baseline 79.9234%, final 8.9566%, policy threshold 80%. | Diagnose and rerun a baseline-comparable coverage command that produces a valid C# coverage artifact; remediate until repo-wide coverage is at least 80% and no lower than baseline. | Repository policy requires repo-wide coverage to remain at least 80% and not regress. | `csharp-coverage-delta.2026-07-06T11-02.md`; `post-refinement-verification.2026-07-06T12-26.md` records broad rerun timeout. |
| Major | `artifacts/csharp/coverage.xml` | Required coverage artifact path | The review-required C# coverage artifact path is absent. Feature-folder Cobertura evidence exists, but the mandated language artifact path was not present. | Produce or copy the final verified C# coverage artifact to the required path, while also preserving canonical feature evidence under the active feature folder. | Feature review policy requires explicit PASS/FAIL coverage verification for each changed language and names this path for C#. | `Test-Path artifacts/csharp/coverage.xml` returned missing. |
| Major | `TaskMaster/AppGlobals/AppEvents.cs`; `TaskMaster.Test/AppGlobals/AppEventsTests.cs` | File line counts | Both changed files are 507 lines after the change, above the repository 500-line limit. | Move cohesive issue #243 code or test helpers into existing partial/test files, or otherwise reduce each changed file to 500 lines or fewer without changing behavior. | The general code change policy applies the 500-line limit to production and test code. | `(Get-Content ...).Count` returned 507 for both files; baseline counts were 430 and 445. |
| Info | `TaskMaster/AppGlobals/AppEvents.cs` | Lines 72-92, 261-284 | The readiness ordering fix is consistent with the issue objective, and the new helper observes faulted processing tasks. | Keep this behavior when remediating file size. | The key bug objective is met by invoking startup inbox processing after readiness hookup populates `OlInboxes`. | Diff inspection; `AppEventsTests.LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs`; post-refinement focused tests passed 14/14. |

No correctness, async, or concurrency blocker was found beyond the evidence and policy-gate findings above.

## Implementation Audit

### C# implementation audit

#### What changed well

- `LoadAsync()` now branches so `ProcessNewInboxItemsAsync()` remains awaited only when events are not hooked.
- `PerformReadinessHookup()` starts startup inbox processing after `OlInboxes` has been populated from `Globals.Ol.Inboxes`.
- `ProcessStartupInboxItemsAfterReadinessHookup()` handles already-faulted tasks and attaches a fault-only continuation for asynchronous failures.

#### Type safety and API notes

- No public API surface was added.
- The new helper is private and does not alter existing interfaces.
- Nullable/type-check evidence reports 0 warnings and 0 errors under warnings-as-errors.

#### Error handling and logging

- Faulted startup processing is logged rather than being ignored.
- The implementation does not catch broad exceptions around readiness hookup; existing coordinator behavior still propagates non-transient COM exceptions and retries transient readiness failures.

## Test Quality Audit

The test coverage directly addresses the issue #243 behavior. The fail-before artifact demonstrates the prior incorrect call sequence. Post-fix focused tests and post-refinement focused tests passed. Full `TaskMaster.Test` also passed after refinement.

### Reviewed test and QA artifacts

- `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/regression-testing/fail-before-appevents-loadasync-inbox-gating.2026-07-06T11-02.md` - expected failure showing pre-readiness processing before the fix.
- `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/regression-testing/post-fix-focused-mstest.2026-07-06T11-02.md` - focused post-fix tests passed 13/13.
- `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/post-refinement-verification.2026-07-06T12-26.md` - formatter, analyzer, nullable, focused tests, full tests, and whitespace check passed; broad coverage rerun timed out.
- `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/csharp-coverage-delta.2026-07-06T11-02.md` - changed-line coverage passed at 100%, repository-wide coverage failed.

### Quality assessment prompts

- **Determinism:** Tests use Moq and direct coordinator ticks; no live Outlook or timers are required for the core assertions.
- **Isolation:** Added tests target `LoadAsync()` ordering and coordinator callback sequencing.
- **Speed:** Focused and full MSTest runs completed successfully; coverage rerun timed out.
- **Diagnostics:** Assertion messages identify readiness, hookup, and processing-order expectations.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection found no secrets or credentials. |
| No unsafe subprocess or command construction | N/A | No production subprocess invocation was added. |
| Input validation at boundaries | N/A | No new external input boundary was added. |
| Error handling remains explicit | PASS | Startup processing fault observation was added in `ProcessStartupInboxItemsAfterReadinessHookup()`. |
| Configuration / path handling is safe | N/A | No production path handling was changed. |
| Async/concurrency ordering | PASS | Hooked startup processing is started after readiness hookup; coordinator run-once tests remain in place. |

## Research Log

No external research was required. Review evidence came from local diff inspection, PR context artifacts, feature evidence, and repository policy files.

## Verdict

The code change is behaviorally acceptable for issue #243 based on the inspected diff and test evidence. It is not ready for normal PR flow until remediation resolves the failing coverage gate and the changed-file line-count violations. Remediation inputs and a remediation plan have been written in the active feature folder.
