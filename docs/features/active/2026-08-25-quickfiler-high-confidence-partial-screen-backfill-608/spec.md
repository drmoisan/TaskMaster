# 2026-08-25-quickfiler-high-confidence-partial-screen-backfill (Spec)

- **Issue:** #608
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-25T11-53
- **Status:** Draft
- **Version:** 0.1

## Context
QuickFiler high-confidence mode stops a scan when the first-batch deadline expires and returns a non-empty partial batch immediately, even though fewer than the current screen's `ItemsPerIteration` messages qualified and more source messages remain. The regression affects the first screen and subsequent screens.

Environment:
- OS/version: Windows 11; live Outlook desktop session
- Python version: n/a (C# / .NET Framework 4.8.1 VSTO add-in)
- Command/flags used: QuickFiler launched with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Outlook folder with sparse messages above the configured confidence threshold; the observed form displayed seven or eight items per screen

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the mode violates its page-fill contract and can require repeated undersized screens while qualifying messages remain. The behavior affects normal use of high-confidence mode and contradicts the completed issue #233 streaming-backfill acceptance criteria.


## Repro & Evidence
Steps to Reproduce:
1. Enable QuickFiler high-confidence mode with a threshold that few available messages meet.
2. Launch QuickFiler against a folder where the current form size yields an `ItemsPerIteration` value of seven or eight.
3. Let the first high-confidence scan evaluate many candidates. In the observed run it scanned nearly 40 messages and accepted one before the deadline expired.
4. Observe that QuickFiler returns and displays that one accepted message instead of continuing until the requested screen count is satisfied or the source is exhausted.
5. File the displayed messages and advance to later screens; the same partial-return behavior can recur.

Expected:
QuickFiler must use the current form's `ItemsPerIteration` value as the requested high-confidence batch size. It must continue dequeuing, scoring, and discarding below-threshold candidates until it has collected that many qualifying messages or it has genuinely exhausted the available source. This contract must apply to the initial screen and every subsequent screen.

Actual:
`QfcStreamingDequeueConfidenceGate.DequeueAsync` returns its current `accepted` list when `DefaultFirstBatchDeadline` expires, even when `accepted.Count` is greater than zero but less than `quantity` and `_sourceActive()` indicates that more messages remain. The initial `QfcHomeController.RunAsync` path and later `QfcHomeController.IterateQueueAsync` path both use deadline-bearing dequeue calls, so either path can surface an undersized screen without source exhaustion.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: observed approximately 40 candidates scanned, one accepted, then an immediate return to a one-item screen


## Scope & Non-Goals
- In scope:
  - Correct the deadline decision in `QfcStreamingDequeueConfidenceGate.DequeueAsync` so a non-empty accepted prefix cannot be returned below the requested `quantity` while the source can still yield candidates.
  - Update the gate's deadline documentation and its existing deterministic test suite in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`.
  - Cover the shared gate behavior for the initial seven-item and subsequent eight-item screen requests with `FakeTimeProvider`.
- Out of scope / non-goals:
  - Do not change `QfcDatamodel.QueueProcessing.cs`, `QfcHomeController.cs`, `QfcHomeController.Iteration.cs`, `IQfcDatamodel`, controller wiring, public method signatures, result types, settings, or configuration defaults.
  - Do not change ordinary-mode dequeue behavior, confidence-score calculation, or the accepted-message ordering model.
  - Do not implement the prepared Issue #446 stop-reason/result-interpretation work, or modify its separate epic worktree.
- Explicitly excluded systems, integrations, or datasets:
  - Live Outlook/COM integration, external services, production mail folders, temporary filesystem fixtures, new dependencies, migrations, and UI redesign are excluded.

## Root Cause Analysis
- Closed issue #233 explicitly required a request for N high-confidence items to scan until N qualifying items were collected or the source was exhausted, including a request-seven / scan-many regression case.
- Closed issue #424 introduced `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` to bound pre-UI latency. The live gate returns the accepted prefix when that deadline expires.
- Open issue #446 is already prepared in the in-progress `quickfiler-bug-family` epic, but its current specification addresses only an empty deadline result being mistaken for source exhaustion. It explicitly states that the #446 fix does not change how long a scan runs, so it does not cover this non-empty partial-batch regression.
- Primary files to inspect are `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, and `QuickFiler/Controllers/QfcHomeController.Iteration.cs` plus their existing tests.


## Proposed Fix

### Design summary (what changes where):
Change only `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`. Its deadline branch must return early only when no qualifying message has yet been accepted. Once `accepted.Count > 0`, the existing sequential dequeue-and-score loop must continue past the deadline until it reaches `quantity` or reaches the existing producer-aware source-exhaustion return path. Update the gate documentation to state that the deadline bounds an empty result; it does not authorize a non-empty undersized screen.

### Boundaries and invariants to preserve:
- **Fill or exhaust:** A high-confidence request returns exactly `quantity` qualifying messages when they remain available. A result shorter than `quantity` is permitted only after the existing source-exhaustion path has established that no more source messages can be produced.
- **Deadline rule:** At or after `DefaultFirstBatchDeadline`, an empty accepted list may retain the current deadline result. A non-empty accepted list must continue scanning; deadline expiry alone is not source exhaustion and cannot return a partial screen.
- **Source-exhaustion rule:** Retain the current null-item, poll-timeout, and `_sourceActive` behavior. Do not infer exhaustion from a deadline result, and do not add a new exhaustion signal in this issue.
- **Cancellation:** Retain all existing `CancellationToken.ThrowIfCancellationRequested` checks before the loop, at each loop turn, and after awaiting the score loader. Cancellation continues to surface as `OperationCanceledException`.
- **Order and qualification:** Preserve source order among accepted messages, the inclusive `score >= _cutoff` comparison, and discard behavior for below-threshold messages.
- **Call-path parity:** `QfcHomeController.RunAsync` and `QfcHomeController.IterateQueueAsync` already forward their calculated `ItemsPerIteration` values to this shared gate. The correction must preserve those quantities unchanged for initial and subsequent screens.

### Dependencies or blocked work:
- No new library or service dependency is required. The existing `Microsoft.Bcl.TimeProvider`, `FakeTimeProvider`, MSTest, Moq, and FluentAssertions seams are sufficient.
- Issue #446 overlaps the gate but owns only the later interpretation of an **empty** deadline result as distinct from source exhaustion. #608 must retain that empty deadline outcome and must not change controller or datamodel APIs. Before merge, the overlapping changes require coordination so the #446 result-state work preserves #608's non-empty continuation rule.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- Production: `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` only.
- Tests: `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` only.
- File budget: exactly one production file and one test file. Do not expand the near-cap controller wiring test files unless implementation scope changes and is re-approved.

#### Functions/classes/CLI commands impacted:
- `QfcStreamingDequeueConfidenceGate.DequeueAsync(int quantity, int timeOut, CancellationToken token)` and its deadline documentation are the only production targets.
- Existing controller wiring tests remain verification pins; their call sites and API surface are not implementation targets.

#### Data flow and validation changes:
- The data flow remains `RunAsync` or `IterateQueueAsync` -> `IQfcDatamodel.DequeueNextItemGroupAsync` -> `QfcDatamodel.DequeueWithHighConfidenceGateAsync` -> `QfcStreamingDequeueConfidenceGate.DequeueAsync`.
- Replace the unconditional deadline termination with a condition that terminates only if the deadline has elapsed **and** `accepted.Count == 0`. With an accepted prefix, continue consuming one candidate at a time, await its score, append only qualifying messages, and rely on the unchanged source-exhaustion path for any partial result.
- Retain constructor validation: `Timeout.InfiniteTimeSpan` remains valid and any other non-positive first-batch deadline remains invalid.

#### Error handling and logging updates:
- Do not introduce catches, suppression, or a new error path. Preserve cancellation propagation and existing logging conventions.
- Retain deadline logging for the retained empty-result deadline path. If wording is updated, it must distinguish an empty deadline result from source exhaustion and must not imply that a non-empty partial list was returned.

#### Rollback/feature-flag considerations (if applicable):
- No feature flag or migration is required. Rollback is a targeted revert of the gate conditional and its accompanying tests/documentation; it would restore the known partial-screen defect and therefore requires explicit approval.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Inputs remain `quantity`, polling timeout, cancellation token, existing queue-take delegate, score loader, source-active delegate, and first-batch deadline.
- Output remains `Task<IList<MailItem>>`; no stop reason, result wrapper, controller response, datamodel contract, CLI command, or configuration schema is added.
- For `quantity` seven or eight, a non-empty result is either the requested seven/eight qualifying messages in source order or a shorter list returned through genuine source exhaustion. An empty deadline result remains unchanged for the #446 workflow.

#### Required configuration keys and defaults:
- Continue using `QfSettings.HighConfidenceModeEnabled`, the form-calculated `ItemsPerIteration`, the existing confidence cutoff, and `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` (currently 12 seconds).
- No new key, default, environment variable, command-line flag, or project reference is allowed.

#### Backward-compatibility expectations:
- #233: restore its request-N / scan-many fill-or-exhaust invariant for non-empty partial batches.
- #424: preserve the latency bound for a scan that has accepted no qualifying messages by preserving the empty deadline result.
- #446: preserve the empty deadline outcome that #446 will distinguish from exhaustion; #608 changes neither empty-result interpretation nor controller/datamodel APIs.
- Ordinary mode, source ordering, inclusive threshold behavior, public signatures, and configuration remain compatible.

#### Performance constraints (latency/throughput/memory):
- The empty-result first-batch latency bound remains the existing deadline. A scan with at least one accepted message may run beyond that deadline only until it fills the requested batch or the source is exhausted; this is required to avoid an undersized UI screen.
- Continue sequential streaming with no materialization beyond the accepted list and no new allocation, caching, or asynchronous work model.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): The existing gate test helpers can construct mocked `MailItem` values, advance `FakeTimeProvider` deterministically, and control source activity without Outlook, a network, or temporary files. The supplied research is sufficient to author and execute this scoped specification.
- Constraints (budget, performance, compatibility): Use a failing-first, deterministic MSTest approach; retain the 500-line cap; touch one production file and one test file only; do not alter the current issue work mode's requirements source (`spec.md` only).
- External dependencies (services, libraries, releases): No new dependency, service, release, or migration is required. Existing test dependencies are `Microsoft.Bcl.TimeProvider`, MSTest, Moq, and FluentAssertions.

## Data / API / Config Impact
- User-facing or API changes: High-confidence UI screens no longer display a non-empty undersized batch solely because the first-batch deadline expired. There is no new UI, controller, datamodel, API, CLI, or result-type surface.
- Data or migration considerations: None. Queue contents, confidence scores, and persisted settings are unchanged.
- Logging/telemetry updates (if any): Retain existing deadline logging for an empty deadline result; do not emit a deadline-as-exhaustion signal or introduce telemetry solely for #608.
- Compatibility notes (CLI flags, config schemas, versioning): No flag, schema, version, setting, or public contract changes. The retained empty deadline result remains compatible with #446.

## Test Strategy
Seeded from issue:

- [ ] Add a failing deterministic unit test that requests seven qualifying items, interleaves approximately 40 below-threshold candidates, crosses the current deadline, and still returns seven while the source remains active.
- [ ] Add source-exhaustion boundary tests proving that fewer than the requested count returns only when the source is genuinely exhausted, including zero and non-zero partial results.
- [ ] Verify both the initial-screen and subsequent-screen call paths pass the calculated `ItemsPerIteration` quantity through unchanged and cannot treat a deadline as permission to display an undersized non-exhausted batch.
- [ ] Preserve ordinary-mode parity and inclusive confidence-threshold semantics.
- [ ] Reconcile the issue #424 latency objective and the prepared issue #446 outcome contract explicitly so the implementation does not make either plan's assumptions stale without documentation.
- [ ] Run the full C# toolchain in the required format, analyzer, nullable/compiler, and MSTest-with-coverage order.

- Regression tests to add or update:
  - In `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`, add `DequeueAsync_InitialScreenNonEmptyAcceptedPrefixPastDeadline_ReturnsSevenInQueueOrder`: request seven, accept one item before the deterministic deadline, interleave approximately 40 below-cutoff candidates with six later qualifying items, advance `FakeTimeProvider` past the deadline, keep the source active, and assert seven ordered accepted messages.
  - In the same file, add `DequeueAsync_SubsequentScreenNonEmptyAcceptedPrefixPastDeadline_ReturnsEightInQueueOrder`: use a fresh queue, request eight, accept one item before deadline and seven after it, advance `FakeTimeProvider` past deadline, and assert all eight ordered accepted messages. This represents the shared gate behavior used for subsequent screens without expanding the near-cap controller test file.
  - Before the production change, run both new tests separately and record their assertion failures as fail-before evidence; after the change, rerun them and the existing `DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults` test.
- Unit tests (pytest) for the fixed behavior and boundaries:
  - Not applicable: this is C# MSTest work, not pytest. Use mocked `MailItem` values, `FakeTimeProvider`, Moq, and FluentAssertions in the existing gate test suite.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Preserve and execute the source-exhaustion test for empty and non-empty partial results; a partial result is valid only after the existing exhaustion logic.
  - Preserve tests for deadline expiration before any acceptance, cancellation, inclusive cutoff, queue order, rejection/discard behavior, ordinary-mode parity, infinite deadline, and invalid deadline validation.
  - Verify that the existing initial- and subsequent-screen controller wiring tests remain green and continue to supply the calculated quantities unchanged.
- Error handling and logging verification:
  - Confirm cancellation remains an `OperationCanceledException` at the existing check points. Verify empty-result deadline logging remains consistent with its retained outcome and does not state source exhaustion.
- Coverage impact and targets for changed lines/modules:
  - Cover both branches of the revised deadline condition: zero accepted returns at expiry; non-zero accepted continues past expiry. Maintain repository coverage requirements (at least 80% overall and 90% for new/changed methods where applicable) with no changed-line regression.
- Toolchain commands to run (format → lint → type-check → test):
  1. `csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

  Restart at formatting whenever a stage changes files or fails; do not mark an AC complete until a single final pass succeeds.
- Manual validation steps (if required):
  - After automated evidence is green, use a non-production Outlook session only if the executor determines a manual smoke test is needed. Enable high-confidence mode with a sparse folder and confirm seven/eight-item screens are full unless the folder is exhausted. Do not make live Outlook validation a prerequisite for the deterministic gate tests.

### Execution evidence requirements

- Store all Issue #608 evidence under `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/`; non-canonical `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`, and similar paths are prohibited.
- Store baseline snapshots in `evidence/baseline/`, failing and passing regression-test receipts in `evidence/regression-testing/`, and final formatter/analyzer/nullable/test receipts in `evidence/qa-gates/`.
- Every receipt must contain `Timestamp: <yyyy-MM-ddTHH-mm>`, `Command: <exact command>`, and `EXIT_CODE: <int>`. Baseline receipts also require `Output Summary:`. Failing test receipts must declare `ExpectedExitCode: <non-zero integer>`.
- If fail-before execution is structurally impossible, first search `evidence/regression-testing/` for a failing receipt and `fail-before-exception.*.md`; then create a schema-valid `fail-before-exception.<timestamp>.md` in that same canonical folder with `WhyFailingRunImpossible:` and alternative proof. Record the exact search scope, patterns, and result for any absence claim.


## Acceptance Criteria
- [x] `QfcStreamingDequeueConfidenceGate.DequeueAsync` returns all seven qualifying messages, in queue order, after `FakeTimeProvider` crosses the deadline when one message was accepted before deadline, approximately 40 below-cutoff candidates are interleaved, and the source remains active.
- [x] The same gate returns all eight qualifying messages, in queue order, after deadline expiry for the subsequent-screen scenario with one pre-deadline acceptance and seven later qualifying candidates.
- [x] A high-confidence result shorter than the requested `quantity`, including empty and non-empty partial results, is returned only through the existing source-exhaustion path; the existing `DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults` test remains green.
- [x] Deadline expiry with `accepted.Count == 0` retains the current empty-result behavior, and #608 does not alter how that empty result is interpreted by controllers or the datamodel.
- [x] Existing cancellation propagation, inclusive `score >= _cutoff` qualification, below-cutoff discard behavior, accepted-message ordering, infinite-deadline validation, and ordinary-mode behavior remain green.
- [x] Existing initial-screen and subsequent-screen wiring tests remain green and verify the unchanged form-calculated `ItemsPerIteration` quantities reach the shared gate.
- [x] The implementation changes only `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`, and `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`; no controller, datamodel, API, configuration, migration, or Issue #446 epic-worktree change is included.
- [x] Fail-before/pass-after regression evidence and final baseline/QA receipts are stored only in the Issue #608 canonical `evidence/regression-testing/`, `evidence/baseline/`, and `evidence/qa-gates/` folders with required schema fields.
- [x] A final single-pass C# quality loop completes successfully in format, analyzer, nullable/compiler, and MSTest-with-coverage order; each required command and exit code is recorded in canonical evidence.
- [x] The gate documentation records the non-empty continuation rule and the #233/#424/#446 reconciliation without adding a public API or configuration change.

## Risks & Mitigations
- Technical or operational risks:
  - Continuing after deadline for a non-empty prefix can extend the time before a full screen is displayed when qualifying messages are sparse.
  - Issue #446 overlaps the gate and could accidentally reintroduce partial returns or conflate empty deadline results with exhaustion during integration.
  - Enlarging the near-cap controller test files would risk the repository 500-line limit and widen concurrent ownership.
- Mitigations and rollbacks:
  - Retain #424's empty-result deadline to bound no-qualifier startup latency; limit post-deadline continuation to scans that already have an accepted prefix.
  - Use deterministic seven/eight `FakeTimeProvider` regressions plus existing source-exhaustion and controller wiring tests, and require merge coordination with #446.
  - Keep the one-production/one-test-file budget. Revert only the gate conditional and its tests if an approved rollback is required.

## Rollout & Follow-up
- Release/rollout steps:
  - Complete failing-first evidence, the final C# quality loop, and required review before normal release promotion. No configuration rollout or data migration is needed.
  - Coordinate the overlapping gate change with the prepared #446 work before merging either change.
- Post-fix monitoring or clean-up tasks:
  - During follow-up verification, distinguish an intended empty deadline result from source exhaustion and watch for any report of a non-empty undersized high-confidence screen.
  - If a separate controller/result-state change becomes necessary, route it to #446 or a newly scoped issue rather than extending #608.
- Links: Issue #608; archived Issue #233 specification; archived Issue #424 specification; prepared Issue #446 `quickfiler-bug-family` specification; `artifacts/research/2026-08-25T12-03-quickfiler-high-confidence-partial-screen-backfill-608-research.md`.
