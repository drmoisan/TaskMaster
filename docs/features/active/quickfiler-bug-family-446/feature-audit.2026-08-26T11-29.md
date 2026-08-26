# Feature Audit — quickfiler-bug-family-446

- Reviewer: feature-review agent
- Timestamp: 2026-08-26T11-29
- Work mode: `full-bug` — `spec.md` is the sole acceptance-criteria source (28 criteria)
- Baseline: merge base `61edc19b`; head `fd746f55`
- Verification style: independent re-derivation where feasible (diff reads, greps, TRX re-parse, Cobertura re-parse); executor evidence artifacts cited where the underlying command run is the evidence.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence (independently verified unless noted) |
| --- | --- | --- |
| AC1 | PASS | Both stop-reason tests present in `QfcStreamingDequeueConfidenceGateTests.Part3.cs` (`:165`, `:206`), FakeTimeProvider-driven; red TRXs `p1-t9`/`p1-t10` re-parsed: `outcome="Failed"`; green in final 6501/6501 run. |
| AC2 | PASS | Both tests present in `QfcHomeControllerIterationTests.cs` (`:415`, `:433`) asserting `CompleteAddingAsync` Times.Never / Times.Once respectively via `VerifyCompleteAdding`. |
| AC3 | PASS | `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` present (`QfcFormControllerSeamTests.cs:397`); red state recorded as an assertion failure in `p1/p3` evidence (`p3-t2` TRX Failed, evidence records assertion message and test-host cleanup, per D5). |
| AC4 | PASS | `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` present (`QfcStreamingDequeueConfidenceGateTests.cs:345`); `p1-t2` TRX records Failed against a compiling tree (seam landed without invocation, per plan [P1-T2]). |
| AC5 | PASS | `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` present (`QfcDatamodelTests.cs:327`), driven through the `ScoringServiceFactory` seam; red TRX under `p1-t11`/`p1-t12` evidence. |
| AC6 | PASS | Diff read: `CompleteAddingAsync` call sits only inside `else if (batch.Stop == QfcDequeueStop.SourceExhausted)` in `QfcHomeController.Iteration.cs`, with a why-comment naming the irreversibility. |
| AC7 | PASS | Diff read: all four gate exits return explicit stop reasons — degenerate-quantity and loop-completion `QuantitySatisfied`, deadline `DeadlineExpired`, drained source `SourceExhausted`. |
| AC8 | PASS | `UndoConsumer_IdleBeyondThreshold_Completes` with `[Timeout(10000)]` on FakeTimeProvider; passes in final run. |
| AC9 | PASS | `UndoConsumer_SuccessfulTake_ResetsIdleTimer`: three takes advance 6 s each (18 s aggregate) with zero idle gap; asserts the loop drained, parked, and only then exits on an 11 s idle advance. |
| AC10 | PASS | Diff read: `_undoConsumerTask = null` sits in the `finally` block; `UndoConsumer_OnExit_ResetsUndoConsumerTask` plants sentinels and asserts both the idle and the exception exit clear it. |
| AC11 | PASS | Diff read of the rewritten loop: the empty-queue branch either breaks (idle past threshold) or awaits `TimeProvider.Delay(200 ms)`; no branch reaches the loop head without an await or break. Verified together with AC3. |
| AC12 | PASS | `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` present (`QfcQueuePurePathsTests.cs:146`); production sink `TryReleaseRejectedHook` calls `_moveMonitor.UnhookItem` once per rejected item. |
| AC13 | PASS | `DequeueAsync_OnRejectedThrows_ScanContinues` present (`QfcStreamingDequeueConfidenceGateTests.cs:377`); gate wraps the sink invocation in try/catch with log-and-continue. |
| AC14 | PASS | Independently verified via `-U0` hunk map: nearest hunks in that file sit at base lines 285 and 324; the `DequeueAsync_BelowThresholdItemsAreDiscarded` body (base 298–310) is inside no hunk — byte-unchanged. Test passes in the final run. |
| AC15 | PASS | `git diff --name-status`: neither `EmailMoveMonitor.cs` nor `EmailMoveMonitorTests.cs` appears in the branch diff; both pinned tests pass in the final 6501/6501 run. |
| AC16 | PASS | `ScoreRemainingQueueMailItemAsync` returns `(long Score, string TopFolder)` (diff read); `DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult` present (`Part3.cs:237`); `QfcDequeueBatch.PreScored` carries accepted candidates to the boundary. |
| AC17 | PASS (with pre-PR obligation) | Independent scan of `git log 61edc19b..HEAD --format=%B` for closing-keyword-plus-issue-reference: zero matches (conventional-commit prefixes like `fix(448):` are not GitHub closing references). No file in the change set carries a closing keyword before #427. The PR body does not exist yet; `evidence/issue-updates/p4-t17-pr-closing-keyword-constraint.2026-08-26T10-41.md` records the binding constraint for the PR author, including that `issue.md:5` (`Also closes: … #427 …`) is superseded by D1 and must not be transcribed. See code review CR-4: correct that `issue.md` line before or during PR authoring. |
| AC18 | PASS | `git diff --name-only -- "QuickFiler/**/*.cs"` returns exactly the six owned production paths; all twelve named non-owned files absent. Independently confirmed the five sibling partial declarations of `QfcFormController`/`QfcHomeController` exist on the tree and are untouched. |
| AC19 | PASS | Diff contains zero `*.csproj`, `*.props`, `*.targets`, `packages.config` paths. |
| AC20 | PASS | `IQfcDatamodel.cs` diff is additive (new enum, struct, and one overload); the three pre-existing `DequeueNextItemGroupAsync`/`DequeueNextItemGroup` declarations are unaltered (the only touched pre-existing line is the BOM-bearing first `using` line, outside any declaration). |
| AC21 | PASS | Neither `QfcHomeControllerIssue218Tests.cs` nor `QfcHomeControllerRunAsyncHighConfidenceTests.cs` appears in the diff. |
| AC22 | PASS | `git diff --name-status` reports no `A` entry under `QuickFiler/` or `QuickFiler.Test/` (the only `A` entries are the four promoted potential documents under `docs/`). |
| AC23 | PASS | `git grep -c "GetConstructor"` on the gate test file returns 1; the helper is a single exact nine-type lookup guarded by `Should().NotBeNull`, fail-closed (read directly); all gate tests pass in the final run. |
| AC24 | PASS | All 13 changed `.cs` files re-counted: every one is at most 500 (max 497). `QfcFormControllerTests.cs` untouched, so its 827-line exception clause is not exercised. `QfcDatamodel.cs` is 480. |
| AC25 | PASS | The accepted Phase 5 pass ran `csharpier format` (0 rewrites, SHA-256-verified) before the final vstest; `LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval` passes in that post-format run (6501/6501). |
| AC26 | PASS | Independent grep of all 7 changed test files for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Path.GetTempPath`, `Path.GetTempFileName`: only an XML doc-comment mention of `Task.Delay` describing what the seam avoids. |
| AC27 | PASS | `p5-t9-clean-pass.2026-08-26T11-11.md`: all four gates EXIT_CODE 0 in one accepted pass (`csharpier check .`; both msbuild `/t:Rebuild` invocations; vstest `/InIsolation /EnableCodeCoverage`); neither msbuild uses `/t:Build` nor `/p:Nullable=enable`; TRX re-parse confirms 6501/6501/0. One documented restart preceded the accepted pass, per the toolchain-loop rule. |
| AC28 | PARTIAL — left unchecked (non-blocking) | Three components: (a) no regression on changed lines — PASS, independently re-parsed (all three changed files and both repo-wide rates improved against the same-session baseline); (b) plan-designated blocking changed-file gate — PASS (97.39 / 47.89-on-carve-out / 100.00 per [P5-T7], figures re-derived from the committed Cobertura); (c) literal whole-type >= 90% — FAIL (97.39 / 55.37 / 71.05). Component (c) is a genuine spec self-contradiction: reaching 90% whole-type requires coverage in five sibling-owned partial files, unreachable from this change set (owned-file coverage alone peaks both types at 71.0% arithmetically) without violating AC18 or annexing sibling scope. Checkbox correctly left unchecked per the "Leave unmet items unchecked" rule; requires maintainer spec amendment at or before epic close. Does not block merge. |

## Adjudication: could AC28 have been met another way?

Considered and rejected alternatives: (1) modifying sibling partials to add seams — forbidden verbatim by AC18; (2) writing tests that exercise sibling-partial code without touching production — arithmetically required to add roughly 245 covered lines in `QfcFormController`'s siblings and 85 in `QfcHomeController`'s, code that is WinForms/COM-bound (baseline rates 51.93/68.31 reflect this) and whose testability uplift is explicitly assigned to sibling epic children 442/484/444/489, so this route would annex sibling scope, duplicate their in-flight work, and mostly still require seams in files this branch may not modify; (3) `[ExcludeFromCodeCoverage]` on the untestable members — a coverage-exemption route this repository's policy direction disfavors and which no plan task or maintainer waiver authorized. Conclusion: the criterion is unsatisfiable within this feature's lawful scope; the contradiction is in the spec text, not the delivery.

## Regression-net assessment (caller-referred item 2)

`IterateQueueAsync_QueueEmpty` was compared base-to-head directly. Base assertions: dequeue verified `Times.Once`, `CompleteAddingAsync` `Times.Once`, `EnqueueAsync` `Times.Never`. Head assertions: identical trio (dequeue via the new outcome-bearing overload). The rearrangement to `stop: QfcDequeueStop.SourceExhausted` was mandatory — production no longer calls the old overload, so the unmodified arrangement would have produced a meaningless default-batch run — and the old test's implicit semantics (empty batch closes queue unconditionally) were the defect under repair. The discrimination previously hidden inside that single test is now explicit across the AC2 pair, which additionally pins the negative case (`DeadlineExpired` empty batch must NOT close). Net effect: the regression net is strictly stronger. Modification legitimate.

## Acceptance Criteria Status

- Source: `docs/features/active/quickfiler-bug-family-446/spec.md`
- Total AC items: 28
- Checked off (delivered): 27
- Remaining (unchecked): 1
- Items remaining: AC28 — coverage: no regression on changed lines, and >= 90% line coverage on `QfcStreamingDequeueConfidenceGate`, `QfcFormController` and `QfcHomeController` (left unchecked by design: whole-type reading conflicts with AC18; maintainer spec amendment required; both operative sub-conditions pass)

No AC checkbox was newly checked or unchecked by this review: all 27 PASS items were already checked by the executor with valid evidence, and AC28 (PARTIAL) correctly remains unchecked.

## Residual items for the epic orchestrator / maintainer

1. AC28 spec amendment (restate the 90% condition over the changed-file scope, or defer the whole-type target to the epic-level close-out) — maintainer decision.
2. Correct `issue.md:5` (`Also closes:` list includes #427) before PR authoring; enforce the P4-T17 closing-keyword constraint on the PR body (#446, #448, #426 only).
3. Route the `QfcFormController.Actions.cs` testability-seam debt (dialog service seam, loader seams) through the promotion lifecycle (code review CR-1).
4. Remove the dead `using System.Diagnostics;` in `QfcFormController.Actions.cs` on the next authorized touch (code review CR-3).

## Verdict

27 of 28 acceptance criteria PASS; AC28 PARTIAL and non-blocking with both operative sub-conditions passing. Zero blocking findings. The branch is fit to merge into `epic/quickfiler-bug-family-integration`.
