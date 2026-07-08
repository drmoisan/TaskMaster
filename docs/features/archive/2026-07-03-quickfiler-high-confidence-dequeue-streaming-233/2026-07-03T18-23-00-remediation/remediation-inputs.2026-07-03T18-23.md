# Remediation Inputs: QuickFiler High-Confidence Dequeue Streaming (#233)

Timestamp: 2026-07-03T18-23
Feature Folder: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
Primary Review Artifacts:
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T18-23-00-audit/policy-audit.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T18-23-00-audit/code-review.2026-07-03T18-23.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/2026-07-03T18-23-00-audit/feature-audit.2026-07-03T18-23.md`

## Remediation Trigger

Remediation is required because:

1. The code review contains blocker findings.
2. The feature audit has FAIL and PARTIAL acceptance criteria.
3. The policy audit contains FAIL results.
4. `git diff --check 00507b595297c3e6970634a1855f1144c987dbdf...HEAD` fails.
5. `coverage-comparison-remediation-final.md` exits 1 and fails repository coverage policy.

## Required Fix List

1. Route synchronous high-confidence live paths through the dequeue-time gate.
   - Files:
     - `QuickFiler/Controllers/QfcHomeController.cs`
     - `QuickFiler/Controllers/QfcHomeController.Iteration.cs`
     - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
     - tests under `QuickFiler.Test/Controllers/`
   - Expected behavior:
     - `Run()` and `Iterate()` must not surface an unfiltered fixed batch when `HighConfidenceModeEnabled == true`.
     - The synchronous `DequeueNextItemGroup(int quantity)` path must either use equivalent high-confidence gate behavior or be proven unreachable/retired for live high-confidence processing.
   - Verification commands:
     - Targeted MSTest command for new synchronous high-confidence tests.
     - Full final C# QA loop after implementation.

2. Add source-completion-aware streaming behavior to the high-confidence gate.
   - Files:
     - `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
     - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
     - `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`
   - Expected behavior:
     - The gate must continue polling/backfilling while the background source can still produce candidates.
     - The gate may return fewer than the requested count only when the candidate source is exhausted or cancellation is requested.
     - A test must cover repeated empty queue reads while the source remains active and later yields a qualifying item.
   - Verification commands:
     - Targeted MSTest command for `QfcStreamingDequeueConfidenceGateTests`.
     - Full final C# QA loop after implementation.

3. Replace source-inspection-only acceptance tests with behavior assertions where they protect AC1, AC3, AC4, and AC6.
   - Files:
     - `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`
     - `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
     - `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`
   - Expected behavior:
     - Tests should prove actual calls, source-active state, returned pages, and high-confidence filtering outcomes instead of only checking that source text contains names.
   - Verification commands:
     - Targeted MSTest command for the changed test classes.

4. Repair whitespace in issue #233 evidence.
   - File:
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-conversion-remediation-final.md`
   - Expected behavior:
     - `git diff --check 00507b595297c3e6970634a1855f1144c987dbdf...HEAD` exits 0.

5. Produce a passing numeric C# coverage comparison.
   - Files:
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md`
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md`
     - final Cobertura or equivalent coverage artifacts under the feature folder evidence path
   - Expected behavior:
     - Baseline and final coverage evidence contain numeric values.
     - New/changed non-COM-bound code coverage remains >= 90%.
     - Repository-wide coverage remains >= 80% or an approved policy exception is recorded before any PASS claim.

6. Reconcile acceptance criteria checkbox state after fixes.
   - Files:
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
     - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
   - Expected behavior:
     - AC1, AC3, AC4, AC6, and AC10 must be checked only after evidence supports PASS.
     - Any criterion still failing or partial must remain unchecked.

## Do Not Do

- Do not weaken or remove coverage policy requirements.
- Do not mark AC10 complete without numeric coverage baseline, final coverage, and comparison evidence.
- Do not reintroduce post-display high-confidence removal as the live enforcement path.
- Do not create a third high-confidence filtering pipeline.
- Do not copy evidence from issue #232 into the issue #233 feature folder.
- Do not use live Outlook, temporary files, or external services for unit tests.
- Do not modify repository policy documents.

## Required Context Package

- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Original feature plan: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/plan.2026-07-03T16-57.md`
- Requirements sources:
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/issue.md`
