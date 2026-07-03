# Remediation Plan: QuickFiler High-Confidence Dequeue Streaming (#233)

- **Issue:** #233
- **Owner:** drmoisan
- **Last Updated:** 2026-07-03T18-23
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature remediation
- **Primary Requirements Source:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T18-23.md`

## Context Package

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/issue.md`

## Evidence Contract

All new remediation evidence must be written under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`.

Command-bearing evidence artifacts must include:

- `Timestamp:`
- `Command:`
- `EXIT_CODE:`
- `Output Summary:`

## Implementation Plan

### Phase 0 — Compliance and Remediation Baseline

- [x] [P0-T1] Read remediation policy and review inputs
  - Read `AGENTS.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, `.agents/skills/csharp/SKILL.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/remediation-inputs.2026-07-03T18-23.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/code-review.2026-07-03T18-23.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/feature-audit.2026-07-03T18-23.md`, and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/policy-audit.2026-07-03T18-23.md`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/phase0-remediation-instructions-read.md`.
- [x] [P0-T2] Capture remediation starting state
  - Run `git status --short --branch --untracked-files=all` and `git diff --check 00507b595297c3e6970634a1855f1144c987dbdf...HEAD`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-start-state.md`.
- [x] [P0-T3] Capture coverage policy remediation baseline
  - Read `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md` and `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/coverage-policy-remediation-baseline.md`.
- [x] [P0-T4] Capture baseline C# formatting state
  - Run `dotnet tool run csharpier -- check .`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/csharpier-baseline.md`.
- [x] [P0-T5] Capture baseline .NET analyzer state
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/msbuild-analyzers-baseline.md`.
- [x] [P0-T6] Capture baseline nullable type-check state
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/msbuild-nullable-baseline.md`.
- [x] [P0-T7] Capture baseline MSTest coverage state
  - Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\remediation-baseline\vstest-baseline-results`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/vstest-coverage-baseline.md`.

### Phase 1 — Restore High-Confidence Coverage for Synchronous Live Paths

- [x] [P1-T1] [expect-fail] Add synchronous Run regression coverage
  - Add failing regression coverage for high-confidence enabled `QfcHomeController.Run()` proving it does not call `LoadItems` with an unfiltered `InitEmailQueue(itemsPerIteration)` batch.
  - Before implementation, run the targeted test and write nonzero exit evidence to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/sync-run-high-confidence.expect-fail.md`.
- [x] [P1-T2] [expect-fail] Add synchronous Iterate regression coverage
  - Add failing regression coverage for high-confidence enabled `QfcHomeController.Iterate()` or the synchronous datamodel path proving it cannot bypass the dequeue-time confidence gate.
  - Before implementation, run the targeted test and write nonzero exit evidence to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/sync-iterate-high-confidence.expect-fail.md`.
- [x] [P1-T3] Route synchronous high-confidence paths through the gate
  - Update `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Controllers/QfcHomeController.Iteration.cs`, and `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` so synchronous high-confidence startup and iteration use the same confidence decision as the async path, or remove/prove unreachable paths with tests and documented rationale.
- [x] [P1-T4] Verify synchronous high-confidence regressions
  - Run the targeted synchronous high-confidence regression tests.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/sync-high-confidence.pass.md`.

### Phase 2 — Add Source-Completion-Aware Streaming

- [x] [P2-T1] [expect-fail] Add source-active streaming regression coverage
  - Add a failing `QfcStreamingDequeueConfidenceGateTests` case where `_tryTakeNext` returns null across multiple polling intervals while the source remains active and later yields a qualifying item.
  - Before implementation, run the targeted test and write nonzero exit evidence to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/source-active-streaming.expect-fail.md`.
- [x] [P2-T2] Add source-active gate predicate
  - Update `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` to accept an explicit source-active or source-complete predicate and continue polling while candidates may still arrive.
- [x] [P2-T3] Connect datamodel queue source state
  - Update `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` to provide the gate with the correct source-active/source-complete signal from the background worker or equivalent queue state.
- [x] [P2-T4] Verify source-active streaming behavior
  - Run targeted gate and datamodel tests for source-active streaming, source exhaustion, cancellation, scan-many-to-yield-few, and disabled-mode parity.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/source-active-streaming.pass.md`.

### Phase 3 — Strengthen Acceptance Tests

- [x] [P3-T1] Strengthen RunAsync acceptance assertions
  - Replace or supplement source-inspection assertions in `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` with behavior assertions for first-page high-confidence routing and returned candidate flow.
- [x] [P3-T2] Strengthen datamodel and queue acceptance assertions
  - Replace or supplement source-inspection assertions in `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` and `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` with behavior assertions that prove gate invocation and source-active behavior.
- [x] [P3-T3] Verify strengthened acceptance tests
  - Run targeted tests for the changed test classes.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/acceptance-test-strengthening.pass.md`.

### Phase 4 — Repair Evidence and Coverage Policy

- [x] [P4-T1] Repair coverage conversion whitespace evidence
  - Remove trailing whitespace from `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-conversion-remediation-final.md`.
  - Rerun `git diff --check 00507b595297c3e6970634a1855f1144c987dbdf...HEAD`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/git-diff-check-remediation.md`.
- [x] [P4-T2] Repair numeric baseline coverage evidence
  - Regenerate or repair numeric baseline coverage evidence in `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md` using the repository-approved VSTest coverage path and conversion tooling.
- [x] [P4-T3] Produce numeric remediation coverage comparison
  - Produce final numeric coverage comparison evidence at `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md` showing baseline coverage, post-change coverage, changed/new-code coverage, and PASS only if repository policy thresholds are met.

### Phase 5 — Acceptance Criteria Reconciliation

- [x] [P5-T1] Reconcile spec acceptance criteria
  - Re-evaluate AC1, AC3, AC4, AC6, and AC10 against the remediation evidence.
  - Update `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` checkboxes only for criteria with PASS evidence.
- [x] [P5-T2] Reconcile user-story acceptance criteria
  - Apply the same evidence-based acceptance criteria checkbox state to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`.
- [x] [P5-T3] Update issue 233 status mirror
  - Update the issue #233 local status mirror under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/issue-updates/` with the remediation outcome and remaining gaps, if any.

### Phase 6 — Final C# QA and Review Readiness

Execute [P6-T1] through [P6-T4] as one C# QA loop in order. If any step changes files or fails, fix the issue and restart at [P6-T1]. Only check off final QA tasks after formatting, analyzers, nullable/type-check, and MSTest coverage pass without errors in one final pass.

- [x] [P6-T1] Run final C# formatting
  - Run `dotnet tool run csharpier .`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/csharpier-remediation-rerun.md`.
- [x] [P6-T2] Run final .NET analyzer build
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/msbuild-analyzers-remediation-rerun.md`.
- [x] [P6-T3] Run final nullable type-check build
  - Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/msbuild-nullable-remediation-rerun.md`.
- [x] [P6-T4] Run final MSTest coverage
  - Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-rerun-results`.
  - Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-rerun.md` with numeric post-change coverage values in `Output Summary:`.
- [x] [P6-T5] Run post-remediation feature review
  - Run a post-remediation feature review using the feature-review workflow.
  - Write new timestamped policy, code, and feature audit artifacts into `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/`.

## Batch Limits

- Phase 1 may touch at most three production files and two test files.
- Phase 2 may touch at most two production files and two test files.
- Phase 3 may touch at most three test files.
- Phase 4 is evidence-only except for the whitespace repair in the named evidence file.
- If implementation scope expands beyond these limits, stop and create a revised remediation plan before editing additional files.

## Automated Validation Summary

- AC1: Phases 1 and 5.
- AC3: Phases 2, 3, and 5.
- AC4: Phases 2, 3, and 5.
- AC6: Phases 1, 2, 3, and 5.
- AC10: Phases 4 and 6.
