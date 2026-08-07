# Policy Audit — quickfiler-high-confidence-queue-init-stall (Issue #424)

- **Component:** QuickFiler high-confidence dequeue path (gate deadline, progress callback, producer-liveness flag)
- **Date:** 2026-08-06T23-40
- **Branch:** `bug/quickfiler-high-confidence-queue-init-stall-424`
- **Head:** `b3538c815745c1a8fd158fda4c6fb8e04c99c814` (commits `5fa0b3c1` fix, `b3538c81` closeout)
- **Base branch:** `main` (merge-base `fb32b923fa46574a78ef2bd8e18bacb4be2a69f1`, reviewer-recomputed via `git merge-base HEAD main` and `git merge-base HEAD origin/main`, both identical to the caller-supplied value)
- **Work mode:** `full-bug` (marker persisted in `issue.md`; AC source `spec.md`, 13 items)
- **Reviewer:** feature-review agent
- **Template provenance:** the MCP tool `resolve_policy_audit_template_asset` is not present in this session's tool surface. The canonical major-heading structure was reproduced from `.claude/skills/policy-audit-template-usage/SKILL.md` step 5 and no template instruction block is present. The MCP validator `validate_orchestration_artifacts` is likewise unavailable in this session; the repository's operative gate for this artifact is `.claude/hooks/validate-feature-review-coverage.ps1`, which was simulated against this text before finalization.

## Executive Summary

The branch delivers a bounded, tested fix for the unbounded pre-UI wait in QuickFiler High Confidence mode: a 12 s `TimeProvider`-measured first-batch deadline in `QfcStreamingDequeueConfidenceGate`, an incremental progress callback mapped into the 0-30 ProgressViewer band by a new pure module (`QfcScanProgressBandMapper`), an honest `volatile bool` producer-liveness flag replacing the dishonest `BackgroundWorker.IsBusy` signal, and a 1000 ms to 200 ms poll reduction at the pre-UI call site only. Production changes are confined to 6 QuickFiler files plus 2 project files; 31 new deterministic MSTest tests were added (FakeTimeProvider, Moq, FluentAssertions; no COM, no temp files).

All change-scoped quality gates pass and were independently re-verified by this review from the committed Cobertura artifacts: new module 100.00 percent line and 100.00 percent branch; changed gate module 96.63 percent line and 92.11 percent branch; changed-line coverage 100 percent with zero regression; full toolchain (CSharpier, analyzers, nullable, full test suite 6272/6272) evidenced at exit 0, with formatting re-verified check-only by this review at HEAD. Two mid-execution spec corrections (Issue218 test reclassification; AC 13 repo-wide coverage rescoping) were evaluated on their merits and judged legitimate corrections of mis-scoped gates, not weakenings — see Section 8. Zero blocking findings; 4 low-severity findings are recorded in the code review.

**Verdict: PASS — no remediation required.**

## Rejected Scope Narrowing

No caller-supplied scope narrowing was detected. The delegation prompt requested the full feature-vs-base audit and stated diff contents as facts. The trailing `DIRECTIVE: PREFLIGHT VALIDATION ONLY` line in `plan.2026-08-06T21-17.md` is standard planner-to-executor handoff text addressed to the atomic-executor's preflight, not an instruction to this reviewer, and was not treated as a narrowing attempt.

The generated `artifacts/pr_context.summary.txt` changed-files overview misclassified the branch as docs-only ("Core logic changes: 0 files") despite 14 changed `.cs` files. This is the known recurring pr-context tooling defect, not a caller narrowing attempt; the summary was corrected in place during this review so the changed-language enumeration reflects the real diff, and scope was determined from `git diff --numstat` directly.

## Evidence Location Compliance

- Scanned the full branch diff (66 files) for evidence written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **zero occurrences.** All 30 executor evidence artifacts resolve under the canonical `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/{baseline,regression-testing,qa-gates}/` tree, per the evidence-and-timestamp-conventions skill and the plan's non-overridable evidence rules.
- `scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository; the scan was performed directly against `git diff --numstat fb32b923..b3538c81`.
- Note on the canonical per-language artifact table: this repository's evidence-location invariant (and the plan's own rule "Any `artifacts/`-rooted evidence path is invalid") places coverage evidence in the feature folder, not at `artifacts/csharp/coverage.xml`. The executor's committed Cobertura reports (`evidence/baseline/coverage-baseline.cobertura.xml`, `evidence/qa-gates/coverage-final.cobertura.xml`) are therefore the canonical, existing coverage artifacts for this change and were inspected (not regenerated) per the evidence-verification model. Coverage artifacts for the changed language exist; the artifact-absence FAIL rule does not fire.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | All 31 new tests drive time via `FakeTimeProvider`, mock `MailItem` via Moq, and use no external services, no temp files, no network. Reviewed in full: `QfcStreamingDequeueConfidenceGateTests.Part2.cs`, `.Part3.cs`, `QfcScanProgressBandMapperTests.cs`, `QfcDatamodelLivenessTests.cs`, RunAsync test additions. |
| Fast execution | PASS | Scoped suite runs complete in seconds (e.g., `pinned-suites.2026-08-07T00-12.md`: 64 tests, exit 0). |
| AAA structure and documented intent | PASS | Every new test carries Arrange/Act/Assert comments and an XML doc or summary naming the AC it verifies. |
| Scenario completeness | PASS | Positive (fast path, mid-band mapping), negative (guard clauses, throwing callback), edge (zero accepted, in-flight expiry, accepted > quantity clamp, negative inputs), error handling (finally-clears-flag on loader throw), concurrency/state (liveness flag transitions, cancellation during wait and during scoring, both deadline configurations). |
| No temp files; no external dependencies | PASS | Verified by inspection of all new/changed test files. |
| Determinism infrastructure (clock seam) | PASS with one recorded deviation | Time flows through `TimeProvider`/`FakeTimeProvider` everywhere. Deviation: `QfcDatamodelLivenessTests` uses bounded event-driven waits (`SpinWait.SpinUntil(..., 5 s)`, `Task.Wait(5 s)` on `TaskCompletionSource` signals) because `BackgroundWorker` posts its completion asynchronously; this mirrors the pre-existing repo pattern in `QfcInitEmailQueueZeroBatchTests.cs` and returns immediately on the condition. Recorded as code-review finding L3 (Low, non-blocking). |
| Test files in test project mirroring production structure | PASS | All test files live in `QuickFiler.Test/Controllers/`, matching the production `QuickFiler/Controllers/` layout, consistent with this repository's established `<Project>.Test` convention. |

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix workflow (failing regression test first, minimal fix) | PASS | `deadline-fail-before.2026-08-06T22-41.md` (exit 1, 51 takes) precedes the fix; `deadline-pass-after.2026-08-06T22-48.md` (exit 0) follows. Same fail-before/pass-after pair for the liveness flag (`liveness-fail-before.2026-08-06T23-20.md` / `liveness-pass-after.2026-08-06T23-26.md`). Where a compiling fail-before test was impossible (seams did not exist pre-change), exception dossiers were recorded (`fail-before-exception.*.md`). |
| Simplicity / separation of concerns | PASS | Decision logic (band mapping) extracted into the pure `QfcScanProgressBandMapper`; the controller receives wiring only; the datamodel receives flag set/clear only. |
| 500-line file limit | PASS | All 14 touched `.cs` files re-counted by this review at HEAD: max production 496 (`QfcDatamodel.cs`), max test 473 (`QfcHomeControllerRunAsyncHighConfidenceTests.cs`). Two pre-decided test-file splits (`.Part3.cs`, `QfcDatamodelLivenessTests.cs`) fired and are cohesive (see Section 7). |
| Error handling / fail fast | PASS | Guard clauses on the deadline (`ArgumentOutOfRangeException`) and mapper sink (`ArgumentNullException`); progress-callback exceptions propagate deliberately (pinned by test); no broad catches added. |
| Logging pattern | PASS | Deadline-expiry line uses the existing `_debugLog` seam plus `logger.Debug`; per-candidate probability logging unchanged (pinned by test). |
| Public API compatibility | PASS | `IQfcDatamodel` gains one overload; the two-argument overload keeps its exact signature and delegates. All in-repo callers verified compiling and pinned suites pass. The deliberate inherited-deadline behavior change on the delegating paths is documented and assessed in Section 8. |
| No new dependencies | PASS | Zero package changes; `FakeTimeProvider` was already referenced by the test project. |
| Scope discipline | PASS | `scope-guard.2026-08-07T00-25.md`: zero changes under `.claude/rules/`, `.claude/skills/`, `.github/instructions/`, settings, Designer files, or ribbon; `QfcHomeController.Iteration.cs` byte-unmodified. Re-verified against `git diff --numstat` by this review. |
| modified-workflow-needs-green-run | Not triggered | The branch diff contains no paths under `.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**`. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Executor evidence `final-qc-format.2026-08-07T00-28.md` (exit 0). Independently re-verified by this review at HEAD: `./.dotnet-sdk/dotnet.exe tool run csharpier check .` — "Checked 1484 files", exit 0. |
| .NET analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) | PASS | `final-qc-analyzers.2026-08-07T00-30.md`: msbuild exit 0, 0 errors, at the reviewed content state (`5fa0b3c1`; the subsequent commit `b3538c81` is docs-only). |
| Nullable build (`Nullable=enable`, `TreatWarningsAsErrors=true`) | PASS | `final-qc-nullable.2026-08-07T00-31.md`: msbuild exit 0, 0 errors. |
| Naming, XML docs, internal-by-default surface | PASS | New type is `internal sealed`; new interface member and non-obvious contracts carry XML docs; PascalCase/camelCase conventions followed. |
| TimeProvider seam guidance | PASS | The deadline is measured through the already-injected `TimeProvider` (`GetTimestamp`/`GetElapsedTime`); no direct wall-clock reads added. |
| No-COM architecture rules (new runtime code) | PASS with context | The new module `QfcScanProgressBandMapper.cs` has zero Outlook/VSTO/COM references (verified by inspection). Modified members live in pre-existing legacy VSTO files; the change adds no new threads and no new COM call sites (spec invariant, verified in the gate/controller/datamodel diffs). |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework, `[TestClass]`/`[TestMethod]` | PASS | All new tests use MSTest attributes; the partial-class split keeps `[TestClass]` only on the base file (CS0579 avoidance, documented in-file). |
| Moq for mocking | PASS | `MailItem`, `IQfcDatamodel`, `IApplicationGlobals`, `IAppQuickFilerSettings`, form-controller/viewer mocks throughout. |
| FluentAssertions | PASS | All new assertions use FluentAssertions with because-messages. |
| No weakened assertions / no in-process retries | PASS | No existing assertion was relaxed anywhere in the diff. The full-suite flake handling is disclosure-based, not suppression-based (Section 6). |

## 5. Test Coverage Detail

Changed languages on this branch: **C# only** (14 `.cs` files, 2 `.csproj` files). TypeScript: zero changed files on this branch, so no coverage check is required. Python: zero changed files on this branch, so no coverage check is required. PowerShell: zero changed files on this branch, so no coverage check is required.

Coverage evidence: executor-committed full-suite Cobertura reports at merge-base (`evidence/baseline/coverage-baseline.cobertura.xml`) and at final state (`evidence/qa-gates/coverage-final.cobertura.xml`), produced by `scripts/vscode/Invoke-MSTestWithCoverage.ps1` over all 9 test assemblies. Every figure below was independently recomputed by this review from the raw XML (per-line dedup by filename+line; per-line max of condition-coverage for branches), not transcribed from the executor's delta report.

### 5.1 Per-language coverage verdicts

- C# repo-wide coverage at HEAD (full-suite Cobertura root): 85.65 percent line (94937/110849), 79.00 percent branch (22001/27848) — **PASS** against both the CLAUDE.md 80 percent floor and the general-unit-test 85/75 floors, with the like-for-like caveat in 5.3.
- C# new-module coverage, `QfcScanProgressBandMapper.cs`: 100.00 percent line (25/25), 100.00 percent branch (10/10) — **PASS** (>= 90 new-module gate and 85/75 floors).
- C# changed-module coverage, `QfcStreamingDequeueConfidenceGate.cs`: 96.63 percent line (86/89), 92.11 percent branch (35/38) — **PASS** (baseline 95.00 percent line; improved, no regression).
- C# changed-line coverage: 100 percent on every changed executable production line — gate 30/30, controller 7/7 (lines 298-304 all hits >= 1), mapper 25/25 — zero regression — **PASS**.
- C# changed file `QfcHomeController.cs` whole-file coverage: 67.62 percent baseline to 68.40 percent post-change (improved; residual shortfall is pre-existing COM/WinForms-bound members unchanged by this branch, and all changed lines are covered) — **PASS** on the no-regression-on-changed-lines rule that governs modified files.
- C# changed files `QfcDatamodel.cs` / `QfcDatamodel.QueueProcessing.cs`: outside the coverage denominator via the pre-existing, ratified `[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25` (present at merge-base, unchanged by this diff; CLAUDE.md § UT2 exemption mechanism). The changed behavior (liveness flag) is verified by the four `QfcDatamodelLivenessTests`/`QfcDatamodelTests` seam tests — **PASS**.
- C# changed file `IQfcDatamodel.cs`: interface-only file with no executable lines, legitimately absent from the coverage report per the type-only-module clarification in the general unit-test policy — **PASS**.

### 5.2 Baseline / post-change comparison (C#)

- Baseline: 70.19 percent line / 58.30 percent branch (56124/79957 lines, 13472/23109 branches) at merge-base `fb32b923`.
- Post-change: 85.65 percent line / 79.00 percent branch (94937/110849 lines, 22001/27848 branches) at `5fa0b3c1`.
- Change: +15.46 line points on paper; **not attributable to this branch** — see 5.3.
- New/changed-code coverage: 100 percent changed-line, 100.00 percent new-module line, 96.63 percent changed-module line.
- Disposition: PASS — change-scoped gates all met; repo-wide figure at HEAD clears the floors as measured.
- Evidence: `evidence/qa-gates/coverage-delta.2026-08-07T00-48.md`, independently recomputed by this review from both committed Cobertura XMLs.

### 5.3 Like-for-like caveat (independently verified)

The executor flagged, and this review confirms from the raw XML roots, that the two repo-wide figures are **not like-for-like**: `lines-valid` grew from 79,957 to 110,849 (+38.6 percent) between the baseline and final runs, which the roughly 600 lines added by this branch cannot explain. This is the known denominator instability of the coverage collector for this repository (the set of instrumented assemblies varies between full-suite runs). The apparent +15.5-point improvement is a measurement artifact and **must not be read as an improvement produced by this change**; the executor's evidence states this explicitly (`coverage-delta.2026-08-07T00-48.md` section C) and does not claim the improvement. The trustworthy figures are the per-file and changed-line numbers in 5.1, which are computed against fixed, identified source files and which this review reproduced exactly. The honest characterization for the record: the raw repo-wide figure was below every policy floor at the merge-base (70.19/58.30, pre-existing debt), this change did not lower it, and the HEAD measurement — the more completely instrumented of the two runs — reads above the floors.

## 6. Test Execution Metrics

| Metric | Value | Evidence |
|---|---|---|
| Full suite at final state | 6272/6272 passed, exit 0 | `final-qc-tests.2026-08-07T00-45.md` |
| Baseline suite | 6240/6241 (1 pre-existing out-of-scope flake, characterized and re-run green in isolation) | `test-coverage-baseline.2026-08-06T22-31.md` |
| Tests added by this branch | +31 (10 deadline + 3 progress + 4 liveness + 12 mapper + 2 RunAsync) — arithmetic verified against the diff | final-qc-tests table |
| Pinned suites | 64/64 passed; 4 files byte-unmodified; Issue218 diff limited to exactly 4 overload-shape hunks (re-verified by this review with `git diff -U0`) | `pinned-suites.2026-08-07T00-12.md` |
| Flakiness disclosure | 5 full-suite runs: 3 runs had 1-2 failures, all in out-of-scope `UtilitiesCS.Test` dispatcher/console tests, each re-run green in isolation without instrumentation; recorded run is a genuine clean pass of the unmodified suite | `final-qc-tests.2026-08-07T00-45.md` |

Honesty assessment of the flake reporting: the disclosure names every failing run, every failing test, and the isolation re-runs; the same test family flaked at baseline (1 failure) before any change existed, which corroborates environment sensitivity rather than a regression introduced here. No test was modified, annotated, or retried in-process; the diff contains no changes to `UtilitiesCS.Test`. The reporting is judged honest and complete. The executor also disclosed an intermediate loop restart (mapper at 88 percent triggered one added clamp test, then a full restart from the format step) rather than hiding it — consistent with the toolchain-loop policy.

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Toolchain order (format, lint, type-check, test) | PASS | Phase 6 artifacts in order, exit 0 each; restart rule honored (documented restart after the mapper clamp test). |
| Test-file split cohesion | PASS | `Part2.cs` = deadline/cancellation/logging tests; `Part3.cs` = progress-callback tests (relocated verbatim); `QfcDatamodelLivenessTests.cs` = liveness-flag tests with self-contained helpers per the established duplication convention. Splits are thematic, not mechanical truncation. |
| File sizes near the limit | Observation | `QfcDatamodel.cs` 496 and `QfcHomeController.cs` 487 lines approach the 500 limit; pre-decided cohesive fallback splits exist in the plan for future work. Non-blocking. |
| Analyzer suppressions | PASS | No new suppressions introduced in the diff. |
| Agent-memory diff entries | PASS | 14 `.claude/agent-memory/**` files added or updated by the working agents; reviewed for content — process notes only, no policy or code impact. |

## 8. Gaps and Exceptions

### 8.1 Mid-execution spec correction 1 — Issue218 test reclassification (judged legitimate)

Spec v1.0 classified `QfcHomeControllerIssue218Tests.cs` as a dormant byte-unmodified pin. That was factually wrong: both of its tests `Setup`/`Verify` the two-argument `DequeueNextItemGroupAsync` overload that the spec's own design retires at the pre-UI call site, so byte-unmodified and passing were mutually exclusive. The executor halted at the Phase 5 pinned-suite gate rather than improvising (fail-closed record `pinned-suites.2026-08-06T23-58.md` retained); the correction was applied deliberately, dated, and logged (`spec.md` Correction Log v1.0 to v1.1; plan Decisions item 14 and revision D1-D4). This review verified independently that the file's diff is exactly the four matcher hunks and that the #218 intent assertions (`preFilterInvoked`, `LoadItemsAsync` overload discipline, `Times.Once`) are unmodified. **Verdict: a correction of a misclassification, not a weakening. The gate it produced (diff-limited pass) is verifiable and was verified.**

### 8.2 Mid-execution spec correction 2 — AC 13 repo-wide coverage rescoping (judged legitimate, with one residual observation)

AC 13 originally required repository-wide line coverage >= 80 percent. The captured merge-base baseline is 70.19 percent line / 58.30 percent branch, so the clause was unsatisfiable before this branch existed — a dead gate, not a quality control. The orchestrator's resolution rewords AC 13 so the change-controlled gates remain strict and blocking (full toolchain pass, changed-line no-regression, >= 90 on new/changed modules) while the repo-wide raw figures become a record-and-report obligation with an explicit below-floor-at-merge-base statement. This review evaluated the reasoning on its merits and concurs: CLAUDE.md § UT2 scopes the 80 percent floor to the testable denominator after ratified COM/VSTO/WinForms/Outlook-Interop exemptions; the raw uninstrumented figure is not that denominator; the shortfall is pre-existing debt at the merge-base; and the bugfix-workflow policy directs minimal targeted fixes, not scope-widening debt retirement. Every gate within this change's control was kept or made more precise, and the shortfall was surfaced in the delivered artifacts rather than hidden. **Verdict: a correction of a mis-scoped clause, not a lowering of the bar.** Residual observation (Low, non-blocking): no computed testable-denominator figure was produced to affirmatively demonstrate that the exemption-adjusted rate clears the floor; the claim rests on the exemption structure. As it happens, the HEAD raw measurement (85.65/79.00) clears even the unadjusted floors, which bounds the concern.

### 8.3 Deliberate behavior change beyond the pre-UI path (adequately justified; one Low gap)

The two-argument `DequeueNextItemGroupAsync(int, int)` now delegates with `DefaultFirstBatchDeadline` (12 s), so the post-UI iteration call site (`QfcHomeController.Iteration.cs:23`, byte-unmodified) and the legacy synchronous `DequeueNextItemGroup(int)` inherit the deadline. This is documented in three places (spec Technical Specifications: "existing overload delegates with the default deadline"; plan [P4-T4] with the explicit intended-deadline statement per inherited path; `scope-guard.2026-08-07T00-25.md` "Recorded in-scope behavior change") and is behaviorally uniform: a partial batch at the deadline is a legal result shape the iteration loop already handles (same shape as source exhaustion), and the exact-argument pin passes. Deadline mechanics are thoroughly covered at the gate level. Gap: no test pins that the two-argument overload specifically forwards `DefaultFirstBatchDeadline` (rather than, for example, the disabled sentinel) — the delegation lives in the coverage-excluded datamodel partial. Recorded as code-review finding L1 (Low, non-blocking).

### 8.4 Deliberately-unfixed defects (confirmed recorded, confirmed untouched)

Both research-identified defects are genuinely recorded as non-goals/follow-ups in `spec.md` (Scope & Non-Goals; Rollout & Follow-up) and in `scope-guard.2026-08-07T00-25.md`: (a) `EmailMoveMonitor` hook retention for gate-rejected items; (b) post-`Show()` double-scoring of accepted items. This review confirmed neither was silently touched: `QuickFiler/Helper Classes/EmailMoveMonitor.cs`, `QfcItemController.FolderHandling.cs`, and `QfcFormController.Actions.cs` do not appear in the branch diff.

### 8.5 Minor spec-text inconsistency (Low)

The spec Test Strategy row for `QfcDatamodelTests.cs` says "Update lines 103-136 only", but the diff also updates the `WaitForQueue` test region (~lines 287-307) — a necessary companion change because `WaitForQueue` itself was rewired to the flag, and one explicitly authorized by plan [P3-T6]. The spec sentence is internally inconsistent with the spec's own design item 3; the admission-test regions it actually protects (49-100, 139-217) are byte-identical as required. Code-review finding L2 (Low, non-blocking).

## 9. Summary of Changes

- `QfcStreamingDequeueConfidenceGate.cs` (+66/-1): 12 s default first-batch deadline (constructor-injectable, `Timeout.InfiniteTimeSpan` sentinel, guard clause), per-candidate progress callback (exceptions propagate), one deadline-expiry debug line.
- `QfcScanProgressBandMapper.cs` (new, 79): pure, clamped, monotonic 0-30 band mapping with scanning label.
- `QfcDatamodel.cs` (+18/-1) / `QfcDatamodel.QueueProcessing.cs` (+42/-4): `volatile bool _remainingLoadActive` honest liveness flag (set before `RunWorkerAsync`, cleared in `finally` around the awaited loader), consumed by `sourceActive` and `WaitForQueue`; four-argument overload threading deadline+progress into the gate; two-argument overload delegates with the default deadline.
- `QfcHomeController.cs` (+11/-1): pre-UI call site wires the mapper and the new overload; poll 1000 ms to 200 ms (O1).
- `IQfcDatamodel.cs` (+19): new documented overload.
- Tests: +31 tests across 4 new and 4 updated files; 2 project-file `<Compile Include>` updates; 4 pinned suites byte-unmodified; Issue218 matchers updated shape-only.
- Docs/evidence: spec (with dated Correction Log), plan, research, issue, 30 evidence artifacts, 14 agent-memory entries.

## 10. Compliance Verdict

**PASS.** All policy gates evaluated in Sections 1-7 pass; the two mid-execution corrections are judged legitimate (Section 8); zero blocking findings. Four Low findings (L1-L4) are recorded in `code-review.2026-08-06T23-40.md` as non-blocking follow-up candidates. Remediation is not required; no remediation-inputs artifact is produced.

## Appendix A: Test Inventory

New tests (31):

- `QfcStreamingDequeueConfidenceGateTests.Part2.cs` (10): `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline` (the fail-before/pass-after regression test), `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound`, `DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem`, `DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates`, `DequeueAsync_QuantitySatisfiedBeforeExpiry_ReturnsUnchangedBatchAndOrder`, `DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior`, `Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause`, `DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging`, `DequeueAsync_CancelledDuringEmptyQueueWait_ThrowsOperationCanceled`, `DequeueAsync_CancelledDuringScoring_ThrowsOperationCanceled`.
- `QfcStreamingDequeueConfidenceGateTests.Part3.cs` (3): `DequeueAsync_ProgressCallback_FiresOncePerScannedCandidateMonotonically`, `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns`, `DequeueAsync_ThrowingProgressCallback_PropagatesAndLeavesSourceUsable`.
- `QfcScanProgressBandMapperTests.cs` (12): constructor guard pair; quantity zero/negative; zero accepted; mid-band; ceiling; over-quantity clamp; negative-accepted clamp; monotonic hold; rising-sequence monotonicity; label format.
- `QfcDatamodelLivenessTests.cs` (4): `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` (fail-before/pass-after), `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse`, `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`.
- `QfcHomeControllerRunAsyncHighConfidenceTests.cs` (2): `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand`, `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration`.

Updated tests: `QfcDatamodelTests.cs` (polling pin and `WaitForQueue` test retargeted from `BackgroundWorker.isRunning` reflection to the liveness flag); `QfcHomeControllerRunAsyncHighConfidenceTests.cs` (exact-argument overload update); `QfcHomeControllerIssue218Tests.cs` (four matcher hunks only); gate base file (`partial` keyword and `CreateGate` helper shape, backward-compatible reflection fallbacks retained).

Pinned, byte-unmodified suites: `QfcHomeControllerIterationTests.cs` (8), `QfcInitEmailQueueZeroBatchTests.cs` (3), `QfcHighConfidencePreFilterTests.cs` (10), `QfcFormControllerTests.cs` (41).

## Appendix B: Toolchain Commands Reference

Commands evidenced by the executor (all exit 0 on the final pass) and re-run or re-verified by this review where marked:

1. Format: `./.dotnet-sdk/dotnet.exe tool run csharpier format .` (CSharpier 1.2.6 v1 syntax; evidence `final-qc-format.2026-08-07T00-28.md`). Reviewer re-verification (check-only): `./.dotnet-sdk/dotnet.exe tool run csharpier check .` — Checked 1484 files, exit 0.
2. Lint: `MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (evidence `final-qc-analyzers.2026-08-07T00-30.md`, exit 0).
3. Type-check: `MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (evidence `final-qc-nullable.2026-08-07T00-31.md`, exit 0).
4. Test + coverage: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput '<feature>/evidence/qa-gates/coverage-final.cobertura.xml'"` driving `vstest.console.exe` over 9 test assemblies under the coverage collector (evidence `final-qc-tests.2026-08-07T00-45.md`, 6272/6272, exit 0). Reviewer re-verification: per-file and repo-wide figures recomputed directly from both committed Cobertura XMLs (Section 5).
5. Scoped suite runs: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~<Name>"` (plan Decisions item 12).
6. Reviewer scope determination: `git merge-base HEAD origin/main`; `git diff --numstat fb32b923fa46574a78ef2bd8e18bacb4be2a69f1..b3538c815745c1a8fd158fda4c6fb8e04c99c814`; `git diff -U0 -- QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`.
