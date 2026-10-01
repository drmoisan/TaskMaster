# 2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker (Plan)

Status note: authored in two passes on 2026-10-01; scope consolidated per the maintainer comment of 2026-10-01T15:57:04Z; the phase sections, the planner internal review record and the self-review were written in pass B; awaiting MCP plan validation and atomic-executor preflight. Pass A history, for audit only (the phases below are authoritative): (1) scope consolidation per the maintainer comment of 2026-10-01T15:57:04Z on issue 947: issue.md gained a `## Scope Consolidation` section (heading line 39) and AC6 and AC7 (seven checkbox lines, 47 to 53, between `## Acceptance Criteria` at 45 and `## Logs / Screenshots` at 55; AC1 to AC5 text and ordinals unchanged); requirement sources, AC identity table, fact 1 (HandleToggleClickAsync remarks 163 to 168 and catch clause 182 to 185), fact 3 (click-boundary tests, Notifications 449), fact 11, D-1, D-2, D-3, D-4, D-7, D-11, the must-not-touch paragraph, the Delivered Source (new E5 remarks and E6 nested sink guard, E1 and E3 rewritten for three catch clauses, the fourth test and the fixture summary in the partial, the token and size paragraphs), NAMES-947, CMD-COVERAGE-POST (every `catch (Exception)` arm measured) and CMD-WINDOWS (windows E5 and E6) were updated; the research file gained section 9 (addendum). (2) corrections c1 to c4 of the second-pass self-review were applied: c1 in fact 1; c2 and the new edits in the production size paragraph (476 expected, gated only as greater than base and at most 500); c3 in the partial token and size paragraphs (215 delivered lines, gated only as at most 500; `.NotBeSameAs(` and `the later read registered a new prime` are the only gated tokens of the 102-character statement); c4 is carried into the decomposition below.
<!-- SUPERSEDED pass-A decomposition note; the phase sections below replace it and govern wherever the two differ. Phase 0: P0-T1 policy reads in the policy-compliance-order sequence (FEATURE/evidence/baseline/phase0-instructions-read.md with `Timestamp:`, `Policy Order:` and the files read). P0-T2 `BASE-SHA:` = `git rev-parse HEAD`; branch name equals `bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947` else `BRANCH MISMATCH`; `git diff --exit-code BASE-SHA -- TaskMaster TaskMaster.Test` exit 0; whole-tree `git status --porcelain --untracked-files=all` with no line under TaskMaster/ or TaskMaster.Test/; issue.md checks: `- Work Mode: minor-audit` line count 1, `## Scope Consolidation` heading count 1, exactly 7 `- [ ] ` and 0 `- [x] ` lines between `## Acceptance Criteria` and `## Logs / Screenshots`; spec.md and user-story.md absent (scope-and-anchor.md). P0-T3 on the base production file: CMD-TOKEN-COUNT with the fact-1 base counts (`catch (` 1, `catch (Exception ex)` 1, `catch (Exception)` 0, `lock (` 1, `exactly one <c>catch</c>` 1, `the only place in this type that observes a fault with a` 1, `can rely on the fault having been reported.` 1, `This method never throws, because its caller is an` 1, `Intentionally discarded` 0, `RibbonCommandBoundary.SafeLog` 0), CMD-SPANS (base: HandleToggleClickAsync SPAN-TRY 1, SPAN-CATCH 1, SPAN-LINE `catch (Exception)` 0; StartObservedPrime SPAN-TRY 1, SPAN-FINALLY 1, SPAN-CATCH 0; CompletePrime SPAN-TRY 0, SPAN-CATCH 0, `_logError(BuildPrimeFailedMessage(engineName), failure);` before `_primeTasks.TryRemove(engineName, out _);`) and CMD-WINDOWS with the gated base shape stated under CMD-WINDOWS (anchor-production-shape.md). P0-T4 CMD-LINECOUNT; csproj entry tokens with `PR-ENTRY-LINE:`; fixture tokens of facts 3 and 4 (including `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` 1 in the main fixture and the PrimeFaultOrdering test name 1); the four new test names and `ThrowingSink` absent under TaskMaster.Test (anchor-test-side.md). P0-T5 SDK; P0-T6 tool restore; P0-T7 NuGet restore with ANALYZER_MISSING check; P0-T8 dotnet-coverage; P0-T9 csharpier check baseline (exit 0 gate); P0-T10 analyzer CMD-REBUILD; P0-T11 nullable CMD-REBUILD; P0-T12 stall probe (CMD-VSTEST ASSEMBLY-UCS FILTER-STALL NAMES-NONE). P0-T13 fixture baseline (CMD-VSTEST ASSEMBLY-TM FILTER-COORD NAMES-947): `BASELINE-TOTAL:` recorded from COUNTERS total, failed=0, `RESULT ... = Passed` for the eight pre-existing NAMES-947 entries and no RESULT line for the four new names (coordinator-tests-baseline.md). P0-T14 coverage baseline by the D-6 route with CMD-COVERAGE-POST, expected `CATCH-ARM-COUNT: 0` and no `CATCH-ARM ` line, METHOD lines for the three methods recorded (coverage-baseline.md). P0-T15 CMD-HASH and CMD-LINECOUNT (`BASE-HASH-PROD:`, `BASE-HASH-PFO:`, ThrowingSink ABSENT) and the Phase 0 completeness listing appended to scope-and-anchor.md (file-line-counts-baseline.md). Phase 1: P1-T1 write the partial exactly as delivered (four tests). P1-T2 scoped `dotnet tool run csharpier format TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs` with CMD-HASH before and after (throwing-sink-partial-format.md). P1-T3 CMD-TOKEN-COUNT on the partial with every count in the Delivered Source paragraph that begins `Expected token counts on the formatted partial` (throwing-sink-partial-tokens.md). P1-T4 csproj entry after `PR-ENTRY-LINE:` with the issue 944 P1-T2 command shape and anchored `git diff --numstat BASE-SHA -- TaskMaster.Test/TaskMaster.Test.csproj` 1 insertion 0 deletions (csproj-registration.md). P1-T5 CMD-BUILD (build-before-fix.md). P1-T6 [expect-fail] CMD-VSTEST ASSEMBLY-TM FILTER-COORD NAMES-947 with `PROD-HASH-AT-CONTROL:` equal to `BASE-HASH-PROD:` and `git diff --exit-code BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` exit 0; expected: COUNTERS total = BASELINE-TOTAL plus 4 with failed=4; FAILED lines name exactly the four new tests; the two LaterReadStartsNewPrime MESSAGE lines contain `a throwing sink leaves no marker behind, so the later read starts a new prime` and match the case-insensitive pattern `exactly 2 times, but was 1 time`; the FirstPrimeCompletesAndMarkerIsCleared MESSAGE contains `the sink exception is contained, so the marker is still cleared`; the click-boundary MESSAGE contains `the click boundary contains a failure of the sink itself` and `sink failed`; `RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed` and `RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed`; `ExpectedExitCode:` equal to the observed non-zero exit (throwing-sink-fail-before.md). P1-T7 apply E1 to E6. P1-T8 scoped format of the production file with hashes (production-format.md). P1-T9 on the fixed file: CMD-TOKEN-COUNT with every count in the Delivered Source paragraph that begins `Expected post-fix file-level counts`, and CMD-SPANS with: CompletePrime SPAN-TRY 1, SPAN-CATCH 1, SPAN-FINALLY 0, SPAN-LOCK 0 and SPAN-LINE order `Report-then-clear is load-bearing` < SPAN-KEYWORD try < `_logError(BuildPrimeFailedMessage(engineName), failure);` < `catch (Exception)` < `Intentionally discarded: see the remarks on this method.` < `_primeTasks.TryRemove(engineName, out _);`; HandleToggleClickAsync SPAN-TRY 2, SPAN-CATCH 2, SPAN-FINALLY 0, SPAN-LOCK 0 and order SPAN-KEYWORD try < `catch (Exception ex)` < `_logError(BuildToggleFailedMessage(engineName), ex);` < `catch (Exception)` < `Intentionally discarded: see the remarks on this method.`, with `_logError(BuildPrimeFailedMessage(engineName), failure);` 0 in that span; StartObservedPrime unchanged from base (SPAN-TRY 1, SPAN-FINALLY 1, SPAN-CATCH 0) (production-edit-scope.md). P1-T10 CMD-WINDOWS `HUNKS-OUTSIDE-WINDOWS: 0` and `WINDOWS-TOUCHED: E1,E2,E3,E4,E5,E6`, plus `git diff --exit-code BASE-SHA --` over TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs, the Race, PrimeFaultOrdering and PrimeRegistration partials, TaskMaster/Ribbon/RibbonController.EngineCommands.cs, TaskMaster/Ribbon/RibbonCommandBoundary.cs, TaskMaster/TaskMaster.csproj, TaskMaster.runsettings and scripts/vscode/TaskMaster.cli.runsettings exit 0 (protected-regions-unchanged.md). P1-T11 CMD-BUILD (build-after-fix.md). P1-T12 pass-after CMD-VSTEST: all twelve NAMES-947 `Passed`, COUNTERS total = BASELINE-TOTAL plus 4 with failed=0, `PROD-HASH-AFTER:` differs from `BASE-HASH-PROD:` (throwing-sink-pass-after.md). Phase 2: P2-T1 repo-wide `dotnet tool run csharpier format .` with CMD-HASH and scoped porcelain before and after, rewritten count 0 (csharpier-format.md); P2-T2 `POST-FORMAT:` re-run of P1-T3, P1-T9 and P1-T10 appended to their artifacts; P2-T3 CMD-LINECOUNT: production greater than base and at most 500 (476 expected, recorded), partial at most 500, the main fixture and the Race, PrimeFaultOrdering and PrimeRegistration partials each equal to base (file-line-counts.md); P2-T4 csharpier check; P2-T5 analyzer CMD-REBUILD; P2-T6 nullable CMD-REBUILD; P2-T7 `FINAL-FIXTURE-RUN:` section appended to throwing-sink-pass-after.md with the P1-T12 expectations; P2-T8 coverage by the D-6 route with the issue 944 P3-T8 single re-run rule for issue 780 only (coverage-summary.md); P2-T9 toolchain-final-pass.md; P2-T10 `COMPARISON:` section of coverage-summary.md with CMD-CHANGED-LINES: `COORD-CLASS-NODES: 1`; `CATCH-ARM-COUNT: 2` with `CATCH-ARM 1 owner=HandleToggleClickAsync` and `CATCH-ARM 2 owner=CompletePrime`, each `CATCH-ARM-ELEMENTS:` at least 1 and `CATCH-ARM-UNCOVERED: 0` (an arm with 0 elements is `CATCH ARM UNMEASURED`: stop, never pass; the baseline `CATCH-ARM-COUNT: 0` is the negative control); `CHANGED-LINES-UNCOVERED: 0`; `CHANGED-LINES-WITH-ELEMENT:` at least 4 (the two re-indented sink-call lines plus at least one element in each arm); `METHOD CompletePrime` final rate at least 90.00 and final elements greater than baseline; `METHOD HandleToggleClickAsync` final elements greater than baseline and final uncovered at most baseline uncovered; `METHOD StartObservedPrime` final elements equal to baseline and final uncovered at most baseline; the D-8 root comparison. P2-T11 (prime-fault-ordering-identity.md): `git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` exit 0, hash equal to `BASE-HASH-PFO:`, positive control: the same diff on the production file exits 1; plus the c4 unchanged-sibling gate: `git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon` exit 0 beside `git status --porcelain -- TaskMaster.Test/Ribbon` printing exactly one line, `?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`. P2-T12 (c4) determinism tokens by CMD-TOKEN-COUNT over the partial's own content (the partial is untracked, so no `git diff BASE-SHA` form can see it): `Thread.Sleep` 0, `Task.Delay` 0, `GetTempPath` 0, `GetTempFileName` 0, `File.` 0, `Directory.` 0, `DoNotParallelize` 0, `using Microsoft.VisualStudio.TestTools.UnitTesting;` 1, `using Moq;` 1, `using FluentAssertions;` 1 (determinism-tokens.md). P2-T13 (c4) footprint (footprint-scope.md): `git diff --name-status BASE-SHA` lists only TaskMaster/Ribbon/EngineToggleStateCoordinator.cs, TaskMaster.Test/TaskMaster.Test.csproj and paths under FEATURE, paired with `git status --porcelain --untracked-files=all` showing exactly one `??` line under TaskMaster.Test/Ribbon, naming the partial, and no `??` line under TaskMaster/; raw-document check with `git status --porcelain --ignored` over FEATURE (no trx, cobertura, coveragexml or msbuild log under FEATURE). P2-T14 hygiene sweep with the issue 944 P3-T13 command over FEATURE, FILES_SCANNED at least 38 (evidence-hygiene.md). P2-T15 to P2-T21 one check-off per AC by ordinal checkbox position: AC1 (P2-T15, creates ac-status-summary.md with `Timestamp:`) cites the RESULT lines of both LaterReadStartsNewPrime tests in throwing-sink-pass-after.md (P1-T12 and FINAL-FIXTURE-RUN:) plus production-edit-scope.md POST-FORMAT: CompletePrime span; AC2 (P2-T16) cites prime-fault-ordering-identity.md, the CompletePrime SPAN-LINE order and `RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed` in both pass-after runs; AC3 (P2-T17) cites the FirstPrimeCompletesAndMarkerIsCleared RESULT lines and the `.Be(TaskStatus.RanToCompletion,` row of throwing-sink-partial-tokens.md POST-FORMAT:; AC4 (P2-T18) cites the three prime-site FAILED and MESSAGE lines of throwing-sink-fail-before.md and their Passed lines in throwing-sink-pass-after.md; AC5 (P2-T19) cites throwing-sink-partial-tokens.md POST-FORMAT:, determinism-tokens.md, toolchain-final-pass.md and the COMPARISON: section; AC6 (P2-T20) cites `RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed` and `RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed` in both pass-after runs, the HandleToggleClickAsync span results of production-edit-scope.md POST-FORMAT: and `CATCH-ARM 1 owner=HandleToggleClickAsync` with `CATCH-ARM-UNCOVERED: 0` in the COMPARISON: section; AC7 (P2-T21) cites the click-boundary FAILED and MESSAGE lines of throwing-sink-fail-before.md, its `PROD-HASH-AT-CONTROL:` equality, and its Passed lines in throwing-sink-pass-after.md. P2-T22 completes ac-status-summary.md in the acceptance-criteria-tracking format with exactly seven `ACn:` lines. P2-T23 writes reduced-audit-handoff.md (the reduced-audit artifact list, the AC status summary, the code footprint of P2-T13, and the statement that research section 7 item 1 was brought into scope by the 2026-10-01 consolidation and is fixed by E5 and E6, so no promotion is requested by this plan), then re-runs the hygiene sweep with FILES_SCANNED at least 41. Retained facts for the phase author: Helpers.ps1 dot-sources ClosureFilter, PackageRate, Threshold, FirstParty and Projection at its lines 2 to 6, so CMD-COVERAGE-POST resolves every function it calls; hook line 95 is the separator-bearing path pattern applied to each task's opening line, 238 the phase-heading pattern, 339 the final-phase QA-vocabulary check. Tool-surface note: the Bash tool was not used in pass A, so the plan file's line-ending state was not verified; the orchestrator's validator run covers it. -->


- **Issue:** #947
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** minor-audit (acceptance criteria come from the `## Acceptance Criteria` section of `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md` only; `spec.md` and `user-story.md` are intentionally absent and must stay absent)
- **Last Updated:** 2026-10-01 (pass B; the planner had no clock reading, so no time of day is recorded)
- **Status:** Draft for preflight (both authoring passes complete; awaiting MCP plan validation and atomic-executor preflight)
- **Version:** 1.0
- **Plan path continuity:** this file is updated in place for every preflight revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; no artifacts-tree evidence path appears in this plan. In task text the token FEATURE abbreviates `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947`; the Write Set below spells every path in full.

**Commits:** none. This plan stages and commits nothing (`git add`, `git commit`, `git merge` and `git stash` do not appear in any task); the orchestrator commits after execution. Every git gate therefore compares the working tree with the recorded base commit (two-dot form with BASE-SHA as the ref operand) and pairs each name-listing diff with a porcelain span in the same task.

## Requirement sources

- Acceptance criteria (sole source): `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, section `## Acceptance Criteria` (heading at line 45 after the 2026-10-01 scope consolidation), which holds exactly seven checkbox lines, each one physical line beginning `- [ ] ` (lines 47 to 53), followed by the `## Logs / Screenshots` heading (line 55). The check-off tasks locate a criterion by its ordinal position among the checkbox lines between those two headings, never by line number, and change only `- [ ] ` to `- [x] `.
- Scope consolidation: `issue.md` section `## Scope Consolidation` (heading at line 39, between `## Actual Behavior` and `## Acceptance Criteria`) records the maintainer comment of 2026-10-01T15:57:04Z on issue 947 that consolidates the second call site, `HandleToggleClickAsync`, into this issue. AC6 and AC7 were appended for it; AC1 to AC5 keep their text and ordinals.
- Mode marker: `issue.md` line 12 reads `- Work Mode: minor-audit`, and it is the only such line in the file.
- Design input (not a requirements source): `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/research/2026-10-01T06-47-engine-toggle-throwing-log-sink-research.md`, recommendation (b) in section 2, test design in section 4.2, coverage notes in section 5, the second call site in section 7 item 1 and the 2026-10-01 consolidation addendum in section 9. This plan adopts recommendation (b) at both call sites.
- Structural precedent: the approved and executed plan for issue #944, `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md`, and its evidence tree. Every citation below was re-derived against the current tree; no line number was copied from that plan.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change):

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: edits E1 to E6 below)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` (create)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile entry)

Feature documents:

- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md` (check-off edits only, in the `## Acceptance Criteria` section)
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/plan.2026-10-01T06-45.md` (task check-off edits only)

Evidence files, all new, fixed names (the write time is the `Timestamp:` field):

- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/phase0-instructions-read.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/scope-and-anchor.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/anchor-production-shape.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/anchor-test-side.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-sdk.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-tool-restore.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-nuget-restore.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/bootstrap-dotnet-coverage.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/csharpier-check-baseline.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/msbuild-analyzer-baseline.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/msbuild-nullable-baseline.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/stall-probe.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/coordinator-tests-baseline.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/coverage-baseline.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/baseline/file-line-counts-baseline.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-partial-format.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-partial-tokens.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/build-before-fix.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-fail-before.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/build-after-fix.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/throwing-sink-pass-after.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/csproj-registration.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/production-format.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/production-edit-scope.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/protected-regions-unchanged.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/csharpier-format.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/file-line-counts.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/csharpier-check-final.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/msbuild-analyzer-final.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/msbuild-nullable-final.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/coverage-summary.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/toolchain-final-pass.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/prime-fault-ordering-identity.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/determinism-tokens.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/footprint-scope.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/qa-gates/evidence-hygiene.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/other/ac-status-summary.md`
- `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/other/reduced-audit-handoff.md`

Files this plan must not touch, stated so the executor fails closed rather than infers: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs (the main fixture, including the private Harness and its OnLogError hook), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs (AC2 requires it byte-identical), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TaskMaster/Ribbon/RibbonController.EngineCommands.cs, TaskMaster/Ribbon/RibbonCommandBoundary.cs, TaskMaster/TaskMaster.csproj, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, every file under .claude/ (including .claude/agent-memory/, which this run never writes), every file under config/, every file under artifacts/, and every file under docs/features/potential/. Inside the production file only the six documented windows E1 to E6 change. Within `HandleToggleClickAsync` that means the two summary text lines (E1), the last two remarks text lines (E5) and the `catch (Exception ex)` clause from its header line through its closing brace (E6); the engines-unavailable refusal, the outer `try` and the `await ExecuteToggleAsync(engineName).ConfigureAwait(false);` statement are not edited. The constructor, `GetPressed`, `ExecuteToggleAsync`, `StartPrimeIfNeeded`, the `StartObservedPrime` code, the `GetPrimeTask` code, `ApplyPrimeAsync` and the message builders are not edited. The click-boundary hazard (a throwing sink escapes `HandleToggleClickAsync` into the `async void` Office handler) is in scope since the maintainer consolidation of 2026-10-01 (requirement sources) and is fixed by E6; it is no longer a promotion candidate. No `spec.md`, `user-story.md` or potential entry is written by this plan. No orchestration state file is written or named by any task. No raw test-result document (trx), raw coverage document (cobertura, coverage, coveragexml) or msbuild log is copied into the feature folder under any name; raw documents stay under the repository coverage directory, which .gitignore line 150 ignores (line 151 re-includes only its .gitkeep).

## AC identity table

Each ID names one checkbox in the `## Acceptance Criteria` section of `issue.md`, in document order (first to seventh checkbox line after the heading).

| ID | Opening words of the criterion |
|---|---|
| AC1 | When the `logError` sink throws while `CompletePrime` reports a faulted or canceled prime, the engine's prime marker is still removed, so a later `GetPressed` ... starts a new prime (`EngineActiveAsync` is invoked a second time) |
| AC2 | The report-then-clear ordering in `CompletePrime` is preserved ... and the existing tests in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` pass without modification |
| AC3 | A throwing `logError` sink leaves no faulted task unobserved ... a deterministic test asserts that the task returned by `GetPrimeTask` for the first prime ends in `RanToCompletion` |
| AC4 | A regression test reproducing the Steps to Reproduce fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/` |
| AC5 | New tests use MSTest, Moq, and FluentAssertions, create no temporary files, and use no `Thread.Sleep` or `Task.Delay`; the C# toolchain ... passes, and the changed lines in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` are covered |
| AC6 | When the `logError` sink throws while `HandleToggleClickAsync` reports a faulted toggle, `HandleToggleClickAsync` does not throw and still attempts the report ... by its own guarded sink call, the sink receives the toggle fault unchanged, and no control is invalidated |
| AC7 | A separate regression test for the `HandleToggleClickAsync` call site, using a throwing sink on a faulted toggle, fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/` |

## Verified tree facts (re-derived 2026-10-01 at the branch head; every one is re-checked by Phase 0)

1. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is 442 content lines with no `#nullable` directive. `_logError` field at 56; `_primeTasks` summary 72 to 77 and declaration 78 to 81. `HandleToggleClickAsync` summary 153 to 156 with the two text lines 154 (`The toggle-click boundary: the only place in this type that observes a fault with a`) and 155 (`<c>catch</c> clause.`); `<returns>` 158 to 161; `<remarks>` 162, remarks text 163 to 168, whose last two lines are 167 (`This method never throws, because its caller is an <c>async void</c> Office handler`) and 168 (`whose faults would otherwise become unobserved.`), `</remarks>` 169; method 170 to 186: the engines-unavailable refusal 172 to 176, outer `try` 178 with `await ExecuteToggleAsync(engineName).ConfigureAwait(false);` at 180, `catch (Exception ex)` at 182, its opening brace 183, the unguarded `_logError(BuildToggleFailedMessage(engineName), ex);` at 184, its closing brace 185 (twelve spaces and a closing brace) and the method's closing brace 186. `GetPrimeTask` summary 238 to 241, `<returns>` 243 to 249 with text lines 244 to 248, the last three (246 to 248) ending `can rely on the fault having been reported.`; signature `internal Task GetPrimeTask(string engineName)` at 250. `StartPrimeIfNeeded` 260 to 289 (`lock (_primeGate)` 272, `ContainsKey` 274). `StartObservedPrime` summary 291 to 293, `<remarks>` 294, remarks text 295 to 301 (295 carries `exactly one <c>catch</c>` after the line break: 295 reads `The observer is a continuation rather than a <c>catch</c> clause, so this type keeps` and 296 reads `exactly one <c>catch</c> — the click boundary. Reading`), `</remarks>` 302, signature 303 to 308, body 310 to 326 with `CompletePrime(completed, engineName);` at 316 inside a `try` (314) whose `finally` (318) calls `marker.SetResult(true);` (320). `ApplyPrimeAsync` 329 to 349. `CompletePrime` summary 351 to 356, remarks 357 to 365, signature `private void CompletePrime(Task completed, string engineName)` 366, body 367 to 382: early return 368 to 371, `var failure =` 373 to 375, the comment beginning `Report-then-clear is load-bearing` 377 to 379, `_logError(BuildPrimeFailedMessage(engineName), failure);` 380, `_primeTasks.TryRemove(engineName, out _);` 381, closing brace 382. `RenderEngineName` summary token `Renders an engine key for inclusion in a message, so a null key is never ambiguous.` at 385. Exactly one `catch (` (182) and one `lock (` (272) in the file; `catch (Exception)` (closing parenthesis immediately after the type) occurs 0 times; the substring `finally` occurs on two lines (`<c>finally</c>` in the remarks at 300 and the keyword at 318), and only one line (318) trims to the keyword `finally`, which is what CMD-SPANS counts; `exactly one <c>catch</c>` once (296); `the only place in this type that observes a fault with a` once (154); `can rely on the fault having been reported.` once (248); `This method never throws, because its caller is an` once (167); `Intentionally discarded` and `RibbonCommandBoundary.SafeLog` 0 times.
2. Precedent for containing a failure of the last reporting channel: `TaskMaster/Ribbon/RibbonCommandBoundary.cs` `SafeLog` 96 to 113, `catch (System.Exception)` at 109 with the comment `// Intentionally discarded: see remarks.` at 111; its remarks at 99 to 102. The analyzer and nullable gates already pass with that construct. `.editorconfig` line 27 sets `dotnet_analyzer_diagnostic.severity = suggestion` and line 365 sets `RCS1075` (empty catch of System.Exception) to `suggestion`, so neither new empty `catch (Exception)` block (one in `CompletePrime`, one nested inside the click-boundary `catch (Exception ex)` of `HandleToggleClickAsync`), each of which carries a comment, can add a warning-severity diagnostic.
3. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` is 470 content lines: `[TestClass]` 22 on `public partial class EngineToggleStateCoordinatorTests` 23; constants `private const string SpamEngine =` 25 and `private const string SpamToggleControlId =` 26; `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` 160; `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` 213; the region `#region HandleToggleClickAsync — the observed boundary` 331 to 396 with `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` 334 (its toggle fault set up as `harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(failure);` at 339, `Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);` at 342, `.NotThrowAsync(` at 346), `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing` 355 and `HandleToggleClickAsync_WhenEnginesAvailable_TogglesAndInvalidates` 375; `private sealed class Harness` 403 to 452 whose constructor wires the error sink as `Errors.Add(new LoggedError(message, exception));` (417) followed by `OnLogError?.Invoke(message, exception);` (418), so a throwing `OnLogError` hook records the report before it throws; strict mock `new Mock<IAppItemEngines>(MockBehavior.Strict);` at 424; `internal Action<string, Exception> OnLogError { get; set; }` 445; `internal List<string> Invalidations` 447; `internal List<string> Notifications` 449; `internal List<LoggedError> Errors` 451; `private sealed class LoggedError` 457 to 468.
4. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` is 77 content lines with the single test `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` (27 to 73), which captures `var prime = harness.Coordinator.GetPrimeTask(SpamEngine);` (35) before the trigger and probes the handle from inside `harness.OnLogError` (37 to 38). `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` is 175 content lines with three tests (32, 85, 132) using `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` with `.Returns(Task.FromResult(true));` and `Times.Exactly(2),`. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` carries `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` at 204. The four partials carry 25 `[TestMethod]` attributes in total; the fixture's executed test count is read at P0-T13, not assumed.
5. `TaskMaster.Test/TaskMaster.Test.csproj` uses explicit `<Compile Include>` items: the coordinator entries are at 352 (`EngineToggleStateCoordinatorTests.cs`), 359 (`.Race.cs`), 360 (`.PrimeFaultOrdering.cs`) and 361 (`.PrimeRegistration.cs`); no entry names `EngineToggleStateCoordinatorTests.ThrowingSink.cs`. The new entry is inserted immediately after the PrimeRegistration entry, whose line P0-T4 re-derives. `<LangVersion>latest</LangVersion>` at 18 (discard lambda parameters and throw expressions compile). `.csharpierignore` line 12 excludes project files from the formatter. `TaskMaster.Test/packages.config` pins Moq 4.21.0 (41), FluentAssertions 8.11.0 (7) and MSTest.TestFramework 4.4.1 (44). No file under `TaskMaster.Test/Ribbon` contains `ThrowingSink`.
6. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (462 lines): `Get-DotnetCoverageArgumentList` (41) hard-codes `/TestCaseFilter:TestCategory!=LiveOutlook` at 91 and passes no blame switch; `ConvertTo-DerivedCoverageSettingsXml` (97); the main function dot-sources `Invoke-MSTestWithCoverage.Helpers.ps1` (303), `Invoke-MSTest.TrxSummary.ps1` (309) and `Invoke-MSTestWithCoverage.Scope.ps1` (313); discovery at 348 to 355 excludes paths whose root-relative form matches a leading `.claude\` segment (353) and throws `No test assemblies found` (358) when nothing is discovered; the thresholds run at 406 to 409, the `First-party coverage:` line is printed at 410 (its text is built at `Invoke-MSTestWithCoverage.FirstParty.ps1` line 120), the JaCoCo projection at 415 to 423, the trx summary at 430 to 447, and the raw document is retained only when written into the repository coverage directory (449 to 453, `Test-RawCoverageDocumentRetained` in `Invoke-MSTestWithCoverage.Projection.ps1` 148). The entry guard at 459 lets the file be dot-sourced. `Invoke-MSTestWithCoverage.Helpers.ps1` defines `Get-CoberturaClassLineSummary` (160; parameter `-ClassNode` 189; both line axes 195 to 196; returns `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches` 252 to 256) and `ConvertTo-KoverageCoberturaXml` (407). `Invoke-MSTestWithCoverage.Threshold.ps1` defines `Assert-CoberturaLineCoverageThreshold` (3) and `Assert-CoberturaBranchCoverageThreshold` (58); `Invoke-MSTestWithCoverage.FirstParty.ps1` `Get-CoberturaFirstPartyCoverageReport` (123); `Invoke-MSTestWithCoverage.Projection.ps1` `ConvertTo-JacocoPackageProjection` (14) and `Assert-JacocoProjectionReconciliation` (83); `Invoke-MSTest.TrxSummary.ps1` `Get-TrxRunSummary` (12) and `Format-TrxRunSummary` (103), whose first line begins `Test run outcome:` (139) and whose last begins `Failed tests:` (147).
7. `scripts/vscode/TaskMaster.cli.runsettings` carries `Workers` 0 and `Scope` ClassLevel only: no logger, no timeout. A direct vstest run therefore needs an explicit trx logger and the hang-blame switch. `scripts/vscode/Install-RepoDotNetSdk.ps1` installs SDK 8.0.205 into `.dotnet-sdk` with the marker directory `sdk\8.0.205` (56); `global.json` pins 8.0.205 with `paths` `.dotnet-sdk`. `scripts/vscode/Invoke-Restore.ps1` takes `-SolutionPath`, `-Configuration`, `-Platform` and runs msbuild Restore with RestorePackagesConfig. `coverage.config` and `coverage/.gitkeep` exist. `.gitignore` ignores `*.trx` (146), `*cobertura*.xml` (147) and the coverage directory (150, re-include 151). `.csharpierignore` excludes the evidence tree (4), raw coverage and trx files (5 to 8), project files (12 to 14), packages.config (16) and app.config (18).
8. `.claude/hooks/validate-planner-output.ps1` line 95 requires a separator-bearing path token on each task's opening line; line 238 requires `### Phase N — <Title>` headings; line 339 requires QA vocabulary in the final phase.
9. Host constraint carried from the #944 run: four UtilitiesCS.Test shell-icon test classes (`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests`, `EmailIntelligence.OSBrowser_Tests`) fail or stall vstest on this workstation (the #944 probe recorded one failure, `STALL-PROBE: REPRODUCES`); CI executes them. P0-T12 measures it again and the result selects the coverage route (D-6).
10. The `.dotnet-sdk`, `packages` and `bin` trees are git-ignored and were not observable from the planning session, so every bootstrap task is guarded and gated on its post-task marker.
11. Issue #944 (marker registered before the prime starts, `marker.SetResult(true)` in a `finally`) is merged and present in this tree (fact 1), so a sink exception thrown inside `CompletePrime` still completes the first prime handle; the pre-fix fail-before run therefore cannot hang, and the hang-blame switch is a bound, not an expectation. The click-boundary test cannot hang either: before E6 the sink exception escapes the `catch (Exception ex)` clause, so the task returned by `HandleToggleClickAsync` completes in the faulted state and the awaited assertion observes it.

## Design decisions (do not redesign)

- **D-1 Fix shape (research recommendation (b)).** In `CompletePrime`, the `_logError(BuildPrimeFailedMessage(engineName), failure);` statement is wrapped in `try` / `catch (Exception)` whose block holds only the comment `// Intentionally discarded: see the remarks on this method.`, and `_primeTasks.TryRemove(engineName, out _);` stays after the block. No `finally`, no reorder, no change to `StartObservedPrime`, `StartPrimeIfNeeded`, `GetPrimeTask` code or the constructor, no constructor-level wrapper (research (c), rejected: it would change the type's sink contract for every caller rather than the two call sites). At the click boundary (fact 1, line 184), the 2026-10-01 maintainer consolidation brings the second call site into scope: inside the existing `catch (Exception ex)` clause of `HandleToggleClickAsync`, the `_logError(BuildToggleFailedMessage(engineName), ex);` statement is wrapped in the same `try` / `catch (Exception)` shape with the same comment `// Intentionally discarded: see the remarks on this method.`; the outer `try`, the refusal path and the `await` are unchanged, and the toggle fault is still not rethrown. Both guards follow the in-repo precedent `RibbonCommandBoundary.SafeLog` (fact 2) and are catches at a reporting boundary that CLAUDE.md C#4 admits, each documented in its own method remarks. A shared private helper called from both sites was considered and not adopted: it would add a member and change both call sites to route through it, which is a refactor beyond the minimal fix, and the inline form keeps each guard inside the edit window of its own call site.
- **D-2 Production edits are confined to six windows.** E1: the two text lines of the `HandleToggleClickAsync` summary. E2: the last three text lines of the `GetPrimeTask` `<returns>` element. E3: the remarks text of `StartObservedPrime`. E4: `CompletePrime` from its `/// <summary>` line through its closing brace. E5: the last two remarks text lines of `HandleToggleClickAsync`. E6: the `catch (Exception ex)` clause of `HandleToggleClickAsync`, from its header line through the first following line that is exactly twelve spaces and a closing brace. Every other line of the file is byte-identical to BASE-SHA, proven by P1-T10 (every hunk of the anchored unified diff lies inside one window, and every window is touched).
- **D-3 Four new tests in a fifth partial, no harness change.** `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` holds `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` (AC6 and AC7, the click-boundary call site: the toggle faults through `ThrowsAsync(failure)` exactly as the existing fact-3 test sets it up, the sink then throws, and the test asserts that the awaited call does not throw, that the report was attempted once with the toggle fault unchanged, and that nothing was invalidated or notified; it is kept in this partial rather than in the main fixture because the main fixture is outside the Write Set and every throwing-sink regression of this issue then sits in one file), plus the three prime-site tests `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime` (AC1 faulted variant, carries the fail-before obligation with a Moq `Times.Exactly(2)` verification failure), `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime` (AC1 canceled variant, same failure mode) and `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` (AC3 and the report-then-clear observation under a throwing sink: its `RanToCompletion` assertion holds before and after the fix because of the #944 `finally`; its final assertion, that the marker is cleared once the handle has completed, fails before the fix by program order). The throwing sink is modelled by `harness.OnLogError`, which the harness invokes after `Errors.Add` (fact 3), so every test also observes that the report was attempted. Each test constructs its own `Harness`; no new mock, type or harness member.
- **D-4 Fail-before is a real run.** The partial uses only `internal` API present at BASE-SHA, so it compiles against the unchanged production file; P1-T5 builds and P1-T6 runs it before any production edit. Before the fix the sink exception escapes `CompletePrime`, the marker stays registered, the second `GetPressed` returns at `ContainsKey`, and `EngineActiveAsync` is verified once rather than twice. Before the fix the click-boundary test fails independently of the prime site: the toggle fault reaches the `catch (Exception ex)` clause, the harness records the report and the hook throws, the sink exception escapes `HandleToggleClickAsync`, so the awaited call faults and `NotThrowAsync` fails with a message carrying its reason `the click boundary contains a failure of the sink itself` and the sink exception text `sink failed`. Every `await` in the four tests is on a task completed by the continuation's `finally` (fact 11) or already complete, so no await can block; the hang-blame switch bounds the run anyway.
- **D-5 Anchor and substitution rule.** P0-T2 records `BASE-SHA:` as the output of `git rev-parse HEAD` at execution start and requires the two code trees to equal it. Wherever the literal BASE-SHA appears in a command or payload of this plan, the executor substitutes that recorded 40-character value. BASE-SHA is the branch head, not origin/main, because origin/main moves during the run; no merge is performed by this plan.
- **D-6 Coverage route is selected by a recorded observation.** P0-T12 runs the four shell-icon classes alone and records `STALL-PROBE: CLEAR` or `REPRODUCES`. `COVERAGE-ROUTE: RUNNER` (CLEAR) runs scripts/vscode/Invoke-MSTestWithCoverage.ps1 verbatim (CLAUDE.md step 4). `COVERAGE-ROUTE: DIRECT` (REPRODUCES) issues the runner's own inner collector invocation with those four classes excluded and the vstest hang-blame switch appended, post-processed with the runner's own helpers, because the runner hard-codes its filter and carries no blame switch (fact 6). A RUNNER attempt whose log carries `No test assemblies found` is recorded as `ROUTE-REASON: RUNNER-DISCOVERY-FAILED` and the stage is re-issued once by the DIRECT route with the LiveOutlook-only filter. Both routes yield the same committed forms: the `First-party coverage:` line, the JaCoCo package projection text, the trx-derived summary and the per-method coordinator figures, all transcribed into Markdown. Under DIRECT the CLAUDE.md floors are applied by the runner's own threshold functions and a NOT MET result is treated as a runner failure.
- **D-7 Per-method coverage figure.** From the post-processed Cobertura document, take the single `class` element whose `filename` attribute, after replacing backslashes with forward slashes, ends with `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, and reduce it with `Get-CoberturaClassLineSummary` (fact 6). A method's span runs from the first source line matching eight spaces, `private`, a return type and the method name followed by an opening parenthesis, through the first following line that is exactly eight spaces and a closing brace. The method's line coverage is 100 times the number of line-map entries inside the span with hits at least 1, divided by the number of line-map entries inside the span, rounded to two decimals. A catch arm is the lines from a line that trims to `catch (Exception)` through the first following line that consists of exactly that line's leading spaces and a closing brace (sixteen spaces for the nested guard in `HandleToggleClickAsync`, twelve for the guard in `CompletePrime`). Every such arm in the file is measured, in file order, with its owning method (the METHOD span that contains its header line) and its own line-map counts (`CATCH-ARM-ELEMENTS`, `CATCH-ARM-UNCOVERED`), followed by `CATCH-ARM-COUNT:`. At the baseline the file has no such line, so `CATCH-ARM-COUNT: 0` is the negative control; after the fix the expected count is 2, with owners `HandleToggleClickAsync` then `CompletePrime`.
- **D-8 Repository-wide rate is recorded under a comparability rule.** The merged repository line rate is not reproducible across runs of an identical tree. P2-T10 compares it in two branches: `COMPARABLE` when the two root lines-valid figures differ by at most 1 percent of the baseline figure (then the final root line-rate must be at least the baseline root line-rate minus 0.005), otherwise `INCOMPARABLE` (recorded, not gated, with a one-sentence reason). The no-regression weight rests on the per-file, per-method and per-changed-line figures.
- **D-9 Git gates without a commit.** Every `git diff` carries BASE-SHA as its ref operand (working tree against the base commit); every name-listing diff is paired with a `git status --porcelain --untracked-files=all` span in the same task; no gate asserts an empty unscoped porcelain. Porcelain gates assert scope (which trees the lines fall under), never membership counts, because this plan file and `issue.md` gain check-off marks throughout the run and other agents may write under `.claude/agent-memory/`. A PreToolUse refusal of any `.cs` or `.csproj` edit is reported verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run; the executor does not modify hooks, checkpoints or permission configuration.
- **D-10 Fixed artifact names.** No artifact name carries a timestamp; acceptance conditions name files exactly. The write time is the `Timestamp:` field (ISO yyyy-MM-ddTHH-mm).
- **D-11 Check-offs follow the loop.** Every acceptance criterion's evidence is a Phase 1 fail-before observation, a post-format observation or a final-run observation, so every check-off task sits in Phase 2 after the loop and reads the artifact section that survived the final pass. Each check-off task flips exactly one checkbox and completes with the box unchecked when its evidence does not hold, appending exactly one line, `ACn: MET` or `ACn: NOT MET` followed by its failing values, to FEATURE/evidence/other/ac-status-summary.md; P2-T15 creates that file with a `Timestamp:` line and P2-T22 completes it. There are seven check-off tasks, P2-T15 to P2-T21, one per AC1 to AC7 in ordinal order.
- **D-12 Restart rule for the final loop.** The scoped formats at P1-T2 and P1-T8 make both Write Set source files formatter-stable before the loop, so P2-T1 is expected to rewrite nothing. If any step of P2-T1 through P2-T8 fails or rewrites a file, the executor stops and reports the failing step with its artifact; a repair confined to the Write Set is made by the small-path engineer, after which the loop restarts at P2-T1 and each repeated task appends a `PASS-n:` section to its artifact. P2-T9 records every pass and requires the last one to be clean with no rewrite. No `SKIPPED` outcome exists for any Phase 2 command task.

## Delivered source (the executor writes these texts; CSharpier output wins on any layout difference, and the token gates are counted on the formatted text)

Indentation rule: the production-file blocks are shown at their in-file indentation (eight, twelve or sixteen leading spaces) and are written exactly as shown. The new-partial block is shown with four leading spaces of Markdown indent on every line; those four spaces are removed on every line when the file is written, so `using` and `namespace` sit at column 0. Every gated token sits whole on one physical line of the delivered text, and CSharpier does not reflow comments or string literals.

**Production file `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, edit E1 — `HandleToggleClickAsync` summary.** Replace the two text lines of the summary above `internal async Task HandleToggleClickAsync(string engineName)` (the line carrying `The toggle-click boundary: the only place in this type that observes a fault with a` and the line after it, which reads `/// <c>catch</c> clause.`) with:

        /// The toggle-click boundary: the only <c>catch</c> clause in this type that observes an
        /// engine fault. The other two are sink guards, here and in <see cref="CompletePrime"/>.

**Edit E2 — `GetPrimeTask` returns.** Replace the three text lines that end the `<returns>` element above `internal Task GetPrimeTask(string engineName)` (from the line carrying `itself and reported through <c>logError</c>. For a key whose prime did not run to` through the line carrying `can rely on the fault having been reported.`) with:

        /// itself and reported through <c>logError</c>. For a key whose prime did not run to
        /// completion, the marker is cleared only after that report has returned or thrown, so a
        /// caller that receives <see cref="Task.CompletedTask"/> can rely on the report having
        /// been attempted.

**Edit E3 — `StartObservedPrime` remarks.** Replace the seven remarks text lines between the `/// <remarks>` and `/// </remarks>` lines above `private void StartObservedPrime(` (from the line carrying `The observer is a continuation rather than a <c>catch</c> clause, so this type keeps` through the line reading `/// never faults or cancels.`) with:

        /// The observer is a continuation rather than a <c>catch</c> clause. The three
        /// <c>catch</c> clauses in this type all sit in <see cref="HandleToggleClickAsync"/> and
        /// <see cref="CompletePrime"/>: the click boundary and the two sink guards. Reading
        /// <see cref="Task.Exception"/> inside <see cref="CompletePrime"/> marks the fault
        /// observed, so no unobserved task remains. The continuation task itself is discarded;
        /// the value a test awaits is the marker, which the continuation completes only through
        /// <c>SetResult</c> in a <c>finally</c> after <see cref="CompletePrime"/> exits, so it
        /// never faults or cancels. Because <see cref="CompletePrime"/> also contains a failure
        /// of the sink, the discarded continuation has no remaining throw source of its own.

**Edit E4 — `CompletePrime`.** Replace every line from the `/// <summary>` line directly above the line carrying `Observes the outcome of a prime. On any outcome other than ran-to-completion the cache` through the closing brace of `CompletePrime` (the first line that is exactly eight spaces and a closing brace after `private void CompletePrime(Task completed, string engineName)`) with:

        /// <summary>
        /// Observes the outcome of a prime. On any outcome other than ran-to-completion the cache
        /// is left unset — so the key still reports unchecked — the failure is reported through
        /// <c>logError</c>, and only then is the in-flight marker cleared so a later read may
        /// re-prime. A failure thrown by the sink itself is contained here, so the marker is
        /// cleared whether or not the report succeeded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The status is tested rather than the exception. A CANCELED task carries a null
        /// <see cref="Task.Exception"/>, so a handler keyed on the exception returned early for a
        /// cancellation: nothing was logged, the cache stayed unset, and the in-flight marker stayed
        /// registered, which blocked any re-prime for the rest of the session. When there is no
        /// exception to unwrap a <see cref="TaskCanceledException"/> is synthesized so the sink
        /// always receives one. The faulted path is unchanged and still reports the unwrapped base
        /// exception.
        /// </para>
        /// <para>
        /// The sink call is guarded (issue #947). The sink is the last reporting channel of this
        /// type, so a failure inside it has nowhere else to go; letting it escape skipped the clear
        /// below, which left a stale marker that blocked every later re-prime, and faulted the
        /// discarded continuation unobserved. The guard follows
        /// <c>RibbonCommandBoundary.SafeLog</c>. With the sink contained, the continuation in
        /// <see cref="StartObservedPrime"/> has no remaining throw source of its own, so it
        /// completes rather than faulting.
        /// </para>
        /// </remarks>
        private void CompletePrime(Task completed, string engineName)
        {
            if (completed.Status == TaskStatus.RanToCompletion)
            {
                return;
            }

            var failure =
                (Exception)completed.Exception?.GetBaseException()
                ?? new TaskCanceledException(completed);

            // Report-then-clear is load-bearing: the marker stays registered until the report has
            // returned or thrown, so a caller that observes the marker absent — including one that
            // fetched the prime handle after the fault — is guaranteed the report has already been
            // attempted.
            try
            {
                _logError(BuildPrimeFailedMessage(engineName), failure);
            }
            catch (Exception)
            {
                // Intentionally discarded: see the remarks on this method.
            }

            _primeTasks.TryRemove(engineName, out _);
        }

The blank line that separates `CompletePrime` from the `RenderEngineName` summary is retained.

**Edit E5 — `HandleToggleClickAsync` remarks.** Replace the last two remarks text lines above `internal async Task HandleToggleClickAsync(string engineName)` (the line carrying `This method never throws, because its caller is an <c>async void</c> Office handler` and the line after it, which carries `whose faults would otherwise become unobserved.`) with:

        /// The sink call is itself guarded (issue #947): the sink is the last reporting channel,
        /// so a failure inside it has nowhere else to go and is discarded deliberately, following
        /// <c>RibbonCommandBoundary.SafeLog</c>. This method therefore never throws, even when the
        /// sink throws, because its caller is an <c>async void</c> Office handler whose faults
        /// would otherwise become unobserved.

The four remarks text lines above them (from `When the engines are not available the click is refused with exactly one` through `a fault is reported through <c>logError</c>, is not rethrown, and does not invalidate.`) are retained unchanged.

**Edit E6 — `HandleToggleClickAsync` catch clause.** Replace every line from the line that trims to `catch (Exception ex)` through the first following line that is exactly twelve spaces and a closing brace with:

            catch (Exception ex)
            {
                try
                {
                    _logError(BuildToggleFailedMessage(engineName), ex);
                }
                catch (Exception)
                {
                    // Intentionally discarded: see the remarks on this method.
                }
            }

The method's own closing brace (eight spaces and a closing brace) follows unchanged. The toggle fault `ex` is still reported and not rethrown; only a failure of the sink itself is discarded.

Documented tokens quoted here in prose so the presence gates are exonerated: the only <c>catch</c> clause in this type that observes an; The other two are sink guards, here and in; the click boundary and the two sink guards.; returned or thrown; can rely on the report having; no remaining throw source of its own; A failure thrown by the sink itself is contained here; The sink call is guarded (issue #947).; The sink call is itself guarded (issue #947); This method therefore never throws, even when the; RibbonCommandBoundary.SafeLog; is guaranteed the report has already been; Intentionally discarded: see the remarks on this method.; catch (Exception).

Expected post-fix file-level counts on the formatted production file (CMD-TOKEN-COUNT, one count per physical line containing the token): `catch (` 3; `catch (Exception ex)` 1; `catch (Exception)` 2; `lock (` 1; `Intentionally discarded: see the remarks on this method.` 2; `RibbonCommandBoundary.SafeLog` 2; `returned or thrown` 2; `no remaining throw source of its own` 2; `_logError(BuildToggleFailedMessage(engineName), ex);` 1; `_logError(BuildPrimeFailedMessage(engineName), failure);` 1; `the only <c>catch</c> clause in this type that observes an` 1; `The other two are sink guards, here and in` 1; `the click boundary and the two sink guards.` 1; `can rely on the report having` 1; `A failure thrown by the sink itself is contained here` 1; `The sink call is guarded (issue #947).` 1; `The sink call is itself guarded (issue #947)` 1; `This method therefore never throws, even when the` 1; `is guaranteed the report has already been` 1; and the removed base tokens each 0: `exactly one <c>catch</c>`, `the only place in this type that observes a fault with a`, `can rely on the fault having been reported.`, `This method never throws, because its caller is an`. The base file gives `catch (` 1, `catch (Exception ex)` 1, `catch (Exception)` 0, `Intentionally discarded: see the remarks on this method.` 0 and `RibbonCommandBoundary.SafeLog` 0 (fact 1).

Expected post-change size: E1 replaces 2 base lines with 2 (0), E2 replaces 3 with 4 (+1), E3 replaces 7 with 9 (+2), E4 replaces 32 base lines (351 to 382) with 53 delivered lines (+21), E5 replaces 2 with 5 (+3) and E6 replaces 4 base lines (182 to 185) with 11 (+7), so the fixed file is the base 442 lines plus 34, that is 476 lines, under 500. The gate is that the formatted production file is greater than the base count and at most 500 lines; the figure 476 is recorded, not gated, because CSharpier output wins on layout.

**New partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` (whole text).**

    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    namespace TaskMaster.Test.Ribbon
    {
        /// <summary>
        /// Regression tests for issue #947: a <c>logError</c> sink that throws while a faulted or
        /// canceled prime is reported must not leave the prime marker registered, must not move the
        /// report after the clear, and must leave no faulted task behind; a sink that throws while
        /// the click boundary reports a toggle fault must not escape that boundary. A fifth partial
        /// of the coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types
        /// and the fixture constants are reused without adding any harness member. The harness
        /// invokes <c>OnLogError</c> after it has recorded the error, so a throwing hook both
        /// records the report and models a throwing sink.
        /// </summary>
        public partial class EngineToggleStateCoordinatorTests
        {
            #region Issue #947 — a throwing log sink leaves no stale prime marker

            /// <summary>
            /// Regression for issue #947 and the faulted variant that carries the fail-before
            /// obligation. Invariant: when the sink throws while a faulted prime is reported, the
            /// marker is still removed, so a later read starts a new prime. The first prime handle is
            /// captured before the trigger, because the fixed code clears the marker before that
            /// handle completes. Without the fix the sink exception skips the clear, the later read
            /// finds the stale marker and starts nothing, and the activation read is verified once
            /// rather than twice. No sleep, delay, timer or parallelism attribute.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime()
            {
                // Arrange
                var harness = new Harness();
                var probe = new TaskCompletionSource<bool>();
                var failure = new InvalidOperationException("configuration load failed");
                harness
                    .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(probe.Task)
                    .Returns(Task.FromResult(true));
                harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed");
                harness.Coordinator.GetPressed(SpamEngine);
                var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

                // Act
                probe.SetException(failure);
                await firstPrime;
                harness.Coordinator.GetPressed(SpamEngine);
                var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

                // Assert
                harness.Engines.Verify(
                    x => x.EngineActiveAsync(SpamEngine),
                    Times.Exactly(2),
                    "a throwing sink leaves no marker behind, so the later read starts a new prime"
                );
                secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");
                await secondPrime;
                harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeSameAs(failure, "the sink receives the injected exception unchanged");
                harness
                    .Coordinator.GetPressed(SpamEngine)
                    .Should()
                    .BeTrue("the new prime read the engine as active and cached that value");
                harness
                    .Invalidations.Should()
                    .Equal(
                        new[] { SpamToggleControlId },
                        "only the successful prime changed state to display"
                    );
            }

            /// <summary>
            /// Regression for issue #947, canceled variant: when the sink throws while a canceled
            /// prime is reported with the synthesized cancellation exception, the marker is still
            /// removed and a later read starts a new prime. Fails before the fix for the same reason
            /// as the faulted variant.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime()
            {
                // Arrange
                var harness = new Harness();
                var probe = new TaskCompletionSource<bool>();
                harness
                    .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(probe.Task)
                    .Returns(Task.FromResult(true));
                harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed");
                harness.Coordinator.GetPressed(SpamEngine);
                var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

                // Act
                probe.SetCanceled();
                await firstPrime;
                harness.Coordinator.GetPressed(SpamEngine);
                var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

                // Assert
                harness.Engines.Verify(
                    x => x.EngineActiveAsync(SpamEngine),
                    Times.Exactly(2),
                    "a throwing sink leaves no marker behind, so the later read starts a new prime"
                );
                secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");
                await secondPrime;
                harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeAssignableTo<OperationCanceledException>(
                        "a canceled task carries no exception to unwrap, so one is synthesized"
                    );
                harness
                    .Coordinator.GetPressed(SpamEngine)
                    .Should()
                    .BeTrue("the new prime read the engine as active and cached that value");
                harness
                    .Invalidations.Should()
                    .Equal(
                        new[] { SpamToggleControlId },
                        "only the successful prime changed state to display"
                    );
            }

            /// <summary>
            /// Regression for issue #947, the no-unobserved-fault guarantee. The hook probes the prime
            /// handle from inside the throwing sink, so report-then-clear is observed under a throwing
            /// sink as well. The first prime handle, captured before the trigger, must end
            /// ran-to-completion, and once it has completed the marker must be cleared, which is only
            /// possible when the sink exception was contained inside the coordinator. Without the fix
            /// the marker stays registered after the handle completes.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared()
            {
                // Arrange
                var harness = new Harness();
                var probe = new TaskCompletionSource<bool>();
                var failure = new InvalidOperationException("configuration load failed");
                harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
                harness.Coordinator.GetPressed(SpamEngine);
                var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
                Task handleSeenBySink = null;
                harness.OnLogError = (_, _) =>
                {
                    handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);
                    throw new InvalidOperationException("sink failed");
                };

                // Act
                probe.SetException(failure);
                await firstPrime;

                // Assert
                firstPrime
                    .Status.Should()
                    .Be(TaskStatus.RanToCompletion, "the prime handle never faults");
                handleSeenBySink
                    .Should()
                    .BeSameAs(firstPrime, "the report is attempted before the marker is cleared");
                harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeSameAs(failure, "the sink receives the injected exception unchanged");
                harness.Invalidations.Should().BeEmpty("a failed prime leaves nothing to display");
                harness
                    .Coordinator.GetPrimeTask(SpamEngine)
                    .Should()
                    .BeSameAs(
                        Task.CompletedTask,
                        "the sink exception is contained, so the marker is still cleared"
                    );
            }

            /// <summary>
            /// Regression for issue #947, the click-boundary call site. Invariant: when the toggle
            /// faults and the sink then throws, the click boundary still attempts the report and
            /// does not throw, because its caller is an <c>async void</c> Office handler. Without
            /// the fix the sink exception escapes the boundary, so the awaited call faults with the
            /// sink exception.
            /// </summary>
            [TestMethod]
            public async Task HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport()
            {
                // Arrange
                var harness = new Harness();
                var failure = new InvalidOperationException("toggle failed");
                harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(failure);
                harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed");

                // Act
                Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

                // Assert
                await act.Should()
                    .NotThrowAsync("the click boundary contains a failure of the sink itself");
                harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeSameAs(failure, "the sink receives the toggle fault unchanged");
                harness.Invalidations.Should().BeEmpty("a failed toggle changed no state to display");
                harness.Notifications.Should().BeEmpty("a fault is logged, not surfaced as a notice");
            }

            #endregion Issue #947 — a throwing log sink leaves no stale prime marker
        }
    }

Test-side tokens quoted here in prose so the presence gates are exonerated: a throwing sink leaves no marker behind, so the later read starts a new prime; the later read registered a new prime; the sink was invoked once before it threw; the sink receives the injected exception unchanged; the new prime read the engine as active and cached that value; only the successful prime changed state to display; a canceled task carries no exception to unwrap, so one is synthesized; the prime handle never faults; the report is attempted before the marker is cleared; a failed prime leaves nothing to display; the sink exception is contained, so the marker is still cleared; the click boundary contains a failure of the sink itself; the sink receives the toggle fault unchanged; a failed toggle changed no state to display; a fault is logged, not surfaced as a notice; toggle failed; Regression for issue #947; sink failed; .NotThrowAsync(; ThrowsAsync(failure); HandleToggleClickAsync(SpamEngine); HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport.

Expected token counts on the formatted partial (CMD-TOKEN-COUNT over the partial's own content, one count per physical line; every line of the new file is an added line): `[TestMethod]` 4; `[TestClass]` 0; `new Mock<` 0; `MockBehavior` 0; `private sealed class` 0; `var harness = new Harness();` 4; `// Arrange` 4; `// Act` 4; `// Assert` 4; `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` 2; `Times.Exactly(2),` 2; `.NotBeSameAs(` 2; `.Be(TaskStatus.RanToCompletion,` 1; `.NotThrowAsync(` 1; `ThrowsAsync(failure)` 1; `HandleToggleClickAsync(SpamEngine)` 1; `Thread.Sleep` 0; `Task.Delay` 0; each of the four method names `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` and `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` 1; reason and literal tokens: `a throwing sink leaves no marker behind, so the later read starts a new prime` 2, `the later read registered a new prime` 2, `the sink was invoked once before it threw` 4, `the sink receives the injected exception unchanged` 2, `the new prime read the engine as active and cached that value` 2, `only the successful prime changed state to display` 2, `a canceled task carries no exception to unwrap, so one is synthesized` 1, `the prime handle never faults` 1, `the report is attempted before the marker is cleared` 1, `a failed prime leaves nothing to display` 1, `the sink exception is contained, so the marker is still cleared` 1, `the click boundary contains a failure of the sink itself` 1, `the sink receives the toggle fault unchanged` 1, `a failed toggle changed no state to display` 1, `a fault is logged, not surfaced as a notice` 1, `toggle failed` 1, `Regression for issue #947` 4, `sink failed` 4. The statement carrying `.NotBeSameAs(` is 102 characters as delivered, so CSharpier wraps it; no gate quotes that whole statement, only the single-line tokens `.NotBeSameAs(` and `the later read registered a new prime`. The line carrying `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` exceeds the print width but has no break point, so CSharpier leaves it on one line.

Size: the delivered text above is 215 lines (183 lines for the three prime-site tests and the fixture header, plus 1 line in the fixture summary and 31 lines for the click-boundary test with its separating blank line). CSharpier output wins on layout, so the post-format count is recorded and gated only as at most 500.

**Project file `TaskMaster.Test/TaskMaster.Test.csproj`.** One line inserted immediately after the line carrying `EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (its line number is re-derived by P0-T4): `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />` with the same four-space indentation as its neighbours.

## Execution conventions

- **Working directory and paths.** `WORKTREE` denotes the absolute path of the item worktree supplied in the delegation prompt; it is substituted into every payload's first line and is never written into an artifact. Every artifact records repository-relative paths only. No artifact, and no line of this plan, carries an absolute host path, an account name or a machine name.
- **pwsh starts elsewhere.** A `pwsh -NoProfile -Command` process does not start in the worktree, and `-WorkingDirectory` does not resolve a `-File` operand, so every payload begins with `Set-Location -LiteralPath "WORKTREE"` and every script it invokes is addressed by an absolute path built with `Join-Path (Get-Location).Path`. No payload uses `cd`, and no git command is chained with `&&`, `;` or `|` outside a payload.
- **Anchor substitution.** BASE-SHA is substituted as D-5 states.
- **Payload channel.** Each indented payload block below is executed as one PowerShell 7 invocation (`pwsh -NoProfile -Command` with the payload in single quotes, or the session's PowerShell tool), with the worktree as the current directory set by the payload's own `Set-Location`. Payloads use double quotes only, and no gated token contains an apostrophe or a double quote, so the outer single quotes never conflict. The `Command:` field of the artifact records the canonical command the payload runs (named in each payload's note), not the payload text.
- **Encoding.** Every payload that reads git output containing source text first sets `[Console]::OutputEncoding` to UTF-8, so the em dashes in the coordinator's documentation decode identically to the working file read with `-Encoding UTF8`. No gated token contains a non-ASCII character.
- **Exit codes.** `EXIT_CODE:` records the printed exit value of the payload's principal command. Deliberately failing runs carry `ExpectedExitCode:` equal to the observed non-zero value. A task that runs several commands names one as the row and records the others as named `Output Summary:` lines.
- **Tool resolution.** MSBuild and vstest.console.exe are resolved through vswhere inside each payload (`TOOLS` prelude below); the resolved paths are used, never printed into an artifact.
- **TOOLS prelude** (the first lines of every build and test payload after the `Set-Location`):

        $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
        New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null

- **Stall handling.** Every direct vstest run carries the hang-dump blame switch (CollectHangDump, TestTimeout 4min, HangDumpType None, spelled out in the CMD-VSTEST payload), so a stalled test is named in a Sequence document under the results directory; a run that produces one is recorded as failed with that test name. The RUNNER coverage route passes no blame argument, so P0-T14 and P2-T8 bound it by wall clock: a run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop and report.
- **Long-running payloads.** `CMD-COVERAGE-RUNNER`, `CMD-COVERAGE-DIRECT`, `CMD-REBUILD` and any payload expected to exceed 8 minutes may be started as background processes with the payload's own standard output redirected to `coverage\logs\<task id>.result.log`; completion is detected by polling that file for the payload's final line, `PAYLOAD-COMPLETE`. Before any coverage collection the executor runs `pwsh -NoProfile -Command '"STRAY_TEST_PROCESSES: " + @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -like "vstest*" -or $_.ProcessName -like "testhost*" -or $_.ProcessName -like "dotnet-coverage*" }).Count'` and starts only when it prints `STRAY_TEST_PROCESSES: 0`; it never runs two collections at once.
- **Restart rule (Phase 2).** D-12.
- **Git working directory.** Every git command this plan writes without -C is run as git -C WORKTREE followed by the same arguments; the Command: field records it without -C. The WORKTREE operand of -C is written with forward slashes and without quotes, or wrapped in single quotes. A git command inside a payload block runs in the directory that payload's own `Set-Location` establishes, which is WORKTREE, so it satisfies this rule as written.

## Command reference

**CMD-REBUILD** (`GATEARGS` is either the analyzer pair, EnableNETAnalyzers true with EnforceCodeStyleInBuild true, or the nullable switch, TreatWarningsAsErrors true, each in the msbuild property form the `Command:` field quotes; `TASKID` substituted; the `Command:` field records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS`, resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory):

    Set-Location -LiteralPath "WORKTREE"
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("WARNINGS: " + (($lines | Select-String -Pattern "^\s*(\d+) Warning\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("SKIP_CORECOMPILE_LINES: " + @($lines | Where-Object { $_.Contains("Skipping target ""CoreCompile""") }).Count)
    Write-Output ("CSC_OUT_TASKMASTER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.dll") }).Count)
    Write-Output ("CSC_OUT_TASKMASTER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.Test.dll") }).Count)
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + @($lines | Where-Object { ($_.Contains("EngineToggleStateCoordinator")) -and ($_ -match "(error|warning) [A-Z]+\d+") }).Count)
    Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll"))
    Write-Output ("UCS_TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll"))
    Write-Output "PAYLOAD-COMPLETE"

Under /t:Rebuild the `SKIP_CORECOMPILE_LINES` count is 0 by construction; the two `CSC_OUT_` counts are the observation that the compiler ran for the two Write Set projects. `ERRORS:` is read from the summary line, so `0 Error(s)` is never mistaken for a substring of a larger count. `WRITESET_DIAGNOSTIC_LINES` counts every error or warning line naming either Write Set source file, because both file names contain `EngineToggleStateCoordinator`.

**CMD-BUILD** (plain incremental build so that a scoped test run observes a fresh assembly; `TASKID` substituted; `Command:` records `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`):

    Set-Location -LiteralPath "WORKTREE"
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $before = (Get-Item -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Build /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    $after = (Get-Item -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll").LastWriteTimeUtc
    Write-Output ("TEST_DLL_ADVANCED: " + ($null -eq $before -or $after -gt $before))
    Write-Output ("CSC_OUT_TASKMASTER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.Test.dll") }).Count)
    Write-Output ("CSC_OUT_TASKMASTER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.dll") }).Count)

**CMD-VSTEST** (`ASSEMBLY`, `FILTER`, `TASKID` and the `NAMES` list substituted; `Command:` records `vstest.console.exe ASSEMBLY /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:coverage\test-results\947\TASKID" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, resolved through vswhere):

    Set-Location -LiteralPath "WORKTREE"
    TOOLS
    $results = "coverage\test-results\947\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $names = @(NAMES)
    $global:LASTEXITCODE = 0
    & $vstest "ASSEMBLY" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log" | Out-Null
    Write-Output ("VSTEST_EXIT_CODE: " + $LASTEXITCODE)
    $trxPath = Join-Path $results "TASKID.trx"
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath $trxPath))
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (-not (Test-Path -LiteralPath $trxPath)) { exit 3 }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw -Encoding UTF8
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $counters = $trx.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
    Write-Output ("COUNTERS total=" + $counters.GetAttribute("total") + " executed=" + $counters.GetAttribute("executed") + " passed=" + $counters.GetAttribute("passed") + " failed=" + $counters.GetAttribute("failed"))
    $all = @($trx.SelectNodes("//t:UnitTestResult", $ns))
    Write-Output ("RESULT_COUNT: " + $all.Count)
    foreach ($r in $all) { if ($names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) } }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -eq "Failed") { Write-Output ("FAILED " + $r.GetAttribute("testName")); $msg = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($msg) { $msg.InnerText } else { "(no message)" })) } }

The trx stays under the ignored coverage directory. The artifact transcribes the `COUNTERS`, `RESULT_COUNT:`, `RESULT`, `FAILED` and `MESSAGE` lines (absolute paths inside a message are replaced by the placeholder REDACTED-PATH before transcription).

Substitutions: `ASSEMBLY-TM` is TaskMaster.Test\bin\Debug\TaskMaster.Test.dll; `ASSEMBLY-UCS` is UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll; `FILTER-COORD` is `FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests` (every partial of the fixture); `FILTER-STALL` is `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests`; `NAMES-947` is `"GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared", "HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport", "HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate", "GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged", "GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime", "GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse", "GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker", "GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns", "GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime", "GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime"`; `NAMES-NONE` is empty. NAMES-947 has twelve entries: the four new names are the first four entries; the fifth is the pre-existing click-boundary fault test of the main fixture (fact 3, line 334), and the last seven are the pre-existing prime tests of the fixture.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4 route; `STAGE` is `baseline` or `final`; `Command:` records `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1`):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\coverage.cobertura.xml", "coverage\coverage.cobertura.jacoco.xml", "coverage\test-results\mstest-coverage-run.trx", "coverage\test-results\mstest-coverage-run.summary.txt")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    foreach ($f in @("coverage\STAGE-947.cobertura.xml", "coverage\STAGE-947.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-947.runner.log" | Out-Null
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\STAGE-947.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("DISCOVERY_FAILURE_MESSAGE: " + [regex]::Match($log, "No test assemblies found[^\r\n]*").Value.Replace((Get-Location).Path, "REDACTED-PATH"))
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold\.").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\STAGE-947.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\STAGE-947.trx" -Force }
    Write-Output "PAYLOAD-COMPLETE"

The runner's lines naming the resolved vstest path and the coverage output carry absolute paths and stay in the ignored log; only the named `_LINE`, `_MESSAGE` and `_PRESENT` values are transcribed. The stale-output removal makes every `_PRESENT` value an observation of this run. A non-empty `DISCOVERY_FAILURE_MESSAGE:` is the D-6 fallback trigger.

**CMD-COVERAGE-DIRECT** (the runner's inner invocation issued directly with the vstest hang-blame switch appended; `STAGE` and `EXCLUSION` substituted, where `EXCLUSION` is the empty string under `STALL-PROBE: CLEAR` (fallback use) and `&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests` under `REPRODUCES`; `Command:` records `dotnet-coverage collect --output coverage\STAGE-947.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-947.config -- vstest.console.exe <N test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\947\STAGE" "/Logger:trx;LogFileName=STAGE-947.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\STAGE-947.cobertura.xml", "coverage\STAGE-947.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-947.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlookEXCLUSION"
    $output = Join-Path $repo "coverage\STAGE-947.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\947\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-947.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-947.collect.log" | Out-Null
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("FILTER: " + $filter)
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-947.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-947.trx") -Destination "coverage\STAGE-947.trx" -Force }
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\STAGE-947.trx"))
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath $output))
    Write-Output "PAYLOAD-COMPLETE"

**CMD-COVERAGE-POST** (post-process if raw, summarise the trx, apply the floors, print the first-party line, the projection, the root counters and the per-method coordinator figures of D-7; `STAGE` substituted; `RAW` is `True` under DIRECT and under a RUNNER run whose `COLLECT_FAILURE_MESSAGE:` is non-empty, otherwise `False`, because a completed runner run has already post-processed the document in place):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $summary = Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath "coverage\STAGE-947.trx" -Raw -Encoding UTF8)
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + (@($summary.FailedTestName) -join ", "))
    $doc = Get-Content -LiteralPath "coverage\STAGE-947.cobertura.xml" -Raw -Encoding UTF8
    if ("RAW" -eq "True") { $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo; Set-Content -LiteralPath "coverage\STAGE-947.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline }
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    [xml]$xml = $doc
    $root = $xml.SelectSingleNode("/coverage")
    Write-Output ("ROOT line-rate=" + $root.GetAttribute("line-rate") + " branch-rate=" + $root.GetAttribute("branch-rate") + " lines-covered=" + $root.GetAttribute("lines-covered") + " lines-valid=" + $root.GetAttribute("lines-valid") + " branches-covered=" + $root.GetAttribute("branches-covered") + " branches-valid=" + $root.GetAttribute("branches-valid"))
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"
    $target = "TaskMaster/Ribbon/EngineToggleStateCoordinator.cs"
    $classes = @($xml.SelectNodes("//class[@filename]") | Where-Object { $_.GetAttribute("filename").Replace([string][char]92, "/").EndsWith($target) })
    Write-Output ("COORD-CLASS-NODES: " + $classes.Count)
    if ($classes.Count -eq 1) {
        $s = Get-CoberturaClassLineSummary -ClassNode $classes[0]
        Write-Output ("COORD-LINES covered=" + $s.CoveredLines + " valid=" + $s.TotalLines)
        Write-Output ("COORD-BRANCHES covered=" + $s.CoveredBranches + " valid=" + $s.TotalBranches)
        $src = @(Get-Content -LiteralPath "TaskMaster\Ribbon\EngineToggleStateCoordinator.cs" -Encoding UTF8)
        $spans = @()
        foreach ($m in @("HandleToggleClickAsync", "StartObservedPrime", "CompletePrime")) {
            $start = 0; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i] -match ("^\s{8}(internal async|private) \w+ " + $m + "\(")) { $start = $i + 1; break } }
            $end = 0; for ($i = $start; $i -lt $src.Count; $i++) { if ($src[$i].TrimEnd() -eq "        }") { $end = $i + 1; break } }
            $spans += ,@($m, $start, $end)
            $inSpan = @($s.LineMap.Keys | Where-Object { $_ -ge $start -and $_ -le $end } | Sort-Object)
            $cov = @($inSpan | Where-Object { $s.LineMap[$_].Hits -ge 1 }).Count
            $rate = if ($inSpan.Count -gt 0) { [math]::Round(100.0 * $cov / $inSpan.Count, 2) } else { "NA" }
            Write-Output ("METHOD " + $m + " span=" + $start + "-" + $end + " elements=" + $inSpan.Count + " covered=" + $cov + " uncovered=" + ($inSpan.Count - $cov) + " rate=" + $rate)
            foreach ($n in $inSpan) { Write-Output ("METHOD-LINE " + $m + " " + $n + " hits=" + $s.LineMap[$n].Hits) }
        }
        $armCount = 0
        for ($i = 0; $i -lt $src.Count; $i++) {
            if ($src[$i].Trim() -ne "catch (Exception)") { continue }
            $armCount++; $cs = $i + 1
            $closer = (" " * ($src[$i].Length - $src[$i].TrimStart().Length)) + "}"
            $ce = 0; for ($j = $i + 1; $j -lt $src.Count; $j++) { if ($src[$j].TrimEnd() -eq $closer) { $ce = $j + 1; break } }
            $owner = "NONE"; foreach ($o in $spans) { if ($cs -ge $o[1] -and $cs -le $o[2]) { $owner = $o[0] } }
            $arm = @($s.LineMap.Keys | Where-Object { $_ -ge $cs -and $_ -le $ce } | Sort-Object)
            Write-Output ("CATCH-ARM " + $armCount + " owner=" + $owner + " span=" + $cs + "-" + $ce + " CATCH-ARM-ELEMENTS: " + $arm.Count + " CATCH-ARM-UNCOVERED: " + @($arm | Where-Object { $s.LineMap[$_].Hits -lt 1 }).Count)
            foreach ($n in $arm) { Write-Output ("CATCH-ARM-LINE " + $armCount + " " + $n + " hits=" + $s.LineMap[$n].Hits) }
        }
        Write-Output ("CATCH-ARM-COUNT: " + $armCount)
    }

Each `METHOD` span is derived from the source file as it stands when the payload runs (the base file in Phase 0, the fixed file in Phase 2), so each stage measures its own statement set. Every line that trims to `catch (Exception)` opens one measured arm (D-7); its end is the first following line made of exactly the header line's leading spaces and a closing brace, so the nested guard in `HandleToggleClickAsync` (sixteen spaces) ends at its own brace rather than at the enclosing `catch (Exception ex)` brace. At the baseline the file contains no line that trims to `catch (Exception)`, so the baseline prints no `CATCH-ARM ` line and prints `CATCH-ARM-COUNT: 0`, which is the negative control for P2-T10. After the fix the expected output is `CATCH-ARM 1 owner=HandleToggleClickAsync` and `CATCH-ARM 2 owner=CompletePrime`, each with `CATCH-ARM-ELEMENTS:` at least 1 and `CATCH-ARM-UNCOVERED: 0`, and `CATCH-ARM-COUNT: 2`; an arm with `CATCH-ARM-ELEMENTS: 0` is `CATCH ARM UNMEASURED`: stop and report, never pass. The projection and the summary block are the two CLAUDE.md committed forms; the `COORD-`, `METHOD` and `CATCH-ARM` lines are figures, not documents.

**CMD-CHANGED-LINES** (hits of every production line the anchored diff adds, read from the final document):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    [xml]$xml = Get-Content -LiteralPath "coverage\final-947.cobertura.xml" -Raw -Encoding UTF8
    $target = "TaskMaster/Ribbon/EngineToggleStateCoordinator.cs"
    $classes = @($xml.SelectNodes("//class[@filename]") | Where-Object { $_.GetAttribute("filename").Replace([string][char]92, "/").EndsWith($target) })
    Write-Output ("COORD-CLASS-NODES: " + $classes.Count)
    $s = Get-CoberturaClassLineSummary -ClassNode $classes[0]
    $added = New-Object System.Collections.Generic.List[int]
    foreach ($h in @(git diff -U0 BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | Where-Object { $_.StartsWith("@@") })) { $mm = [regex]::Match($h, "\+(\d+)(,(\d+))?"); $c = [int]$mm.Groups[1].Value; $d = if ($mm.Groups[3].Success) { [int]$mm.Groups[3].Value } else { 1 }; for ($k = 0; $k -lt $d; $k++) { $added.Add($c + $k) } }
    Write-Output ("CHANGED-LINE-COUNT: " + $added.Count)
    $withElement = 0; $uncovered = 0
    foreach ($n in $added) { if ($s.LineMap.Contains($n)) { $withElement++; $hits = $s.LineMap[$n].Hits; if ($hits -lt 1) { $uncovered++ }; Write-Output ("CHANGED-LINE " + $n + " hits=" + $hits) } else { Write-Output ("CHANGED-LINE " + $n + " no line element") } }
    Write-Output ("CHANGED-LINES-WITH-ELEMENT: " + $withElement)
    Write-Output ("CHANGED-LINES-UNCOVERED: " + $uncovered)

The diff is taken between BASE-SHA and the working tree, which is the tree the coverage document was generated from (this plan commits nothing), so its line numbers align with the document.

**CMD-HASH** (SHA-256 of the two formatter-visible Write Set source files and the AC2-protected partial; hashes only, never the Path property):

    Set-Location -LiteralPath "WORKTREE"
    foreach ($p in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs")) { if (Test-Path -LiteralPath $p) { Write-Output ("HASH " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) } else { Write-Output ("HASH " + $p + " = ABSENT") } }

**CMD-LINECOUNT** (content line counts of the six coordinator source files):

    Set-Location -LiteralPath "WORKTREE"
    foreach ($p in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs")) { if (Test-Path -LiteralPath $p) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count) } else { Write-Output ("LINES " + $p + " = ABSENT") } }

**CMD-TOKEN-COUNT** (`FILE` and the `TOKEN` list substituted; ordinal, case-sensitive substring counts per physical line, so a wrapped token reads 0 and the single-line requirement is enforced by the count):

    Set-Location -LiteralPath "WORKTREE"
    $src = @(Get-Content -LiteralPath "FILE" -Encoding UTF8)
    foreach ($t in @(TOKEN)) { Write-Output ("TOKEN [" + $t + "] = " + @($src | Where-Object { $_.Contains($t) }).Count) }
    foreach ($t in @(TOKEN)) { $idx = 0; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains($t)) { $idx = $i + 1; break } }; Write-Output ("FIRST-LINE [" + $t + "] = " + $idx) }

**CMD-SPANS** (method-span measurements on the production working file):

    Set-Location -LiteralPath "WORKTREE"
    $src = @(Get-Content -LiteralPath "TaskMaster\Ribbon\EngineToggleStateCoordinator.cs" -Encoding UTF8)
    $tokens = @("_logError(BuildToggleFailedMessage(engineName), ex);", "catch (Exception ex)", "CompletePrime(completed, engineName);", "marker.SetResult(true);", "_logError(BuildPrimeFailedMessage(engineName), failure);", "_primeTasks.TryRemove(engineName, out _);", "catch (Exception)", "Intentionally discarded: see the remarks on this method.", "Report-then-clear is load-bearing")
    foreach ($sig in @("internal async Task HandleToggleClickAsync(", "private void StartObservedPrime(", "private void CompletePrime(")) {
        $s = -1; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains($sig)) { $s = $i; break } }
        $e = -1; if ($s -ge 0) { for ($i = $s + 1; $i -lt $src.Count; $i++) { if ($src[$i].TrimEnd() -eq "        }") { $e = $i; break } } }
        Write-Output ("SPAN [" + $sig + "] = " + ($s + 1) + "-" + ($e + 1))
        if ($s -lt 0 -or $e -lt 0) { continue }
        $span = @($src[$s..$e])
        Write-Output ("SPAN-TRY [" + $sig + "] = " + @($span | Where-Object { $_.Trim() -eq "try" }).Count)
        Write-Output ("SPAN-FINALLY [" + $sig + "] = " + @($span | Where-Object { $_.Trim() -eq "finally" }).Count)
        Write-Output ("SPAN-CATCH [" + $sig + "] = " + @($span | Where-Object { $_ -cmatch "\bcatch\b" }).Count)
        Write-Output ("SPAN-LOCK [" + $sig + "] = " + @($span | Where-Object { $_.Contains("lock (") }).Count)
        foreach ($k in @("try", "finally")) { $idx = 0; for ($i = $s; $i -le $e; $i++) { if ($src[$i].Trim() -eq $k) { $idx = $i + 1; break } }; Write-Output ("SPAN-KEYWORD [" + $sig + "] [" + $k + "] = " + $idx) }
        foreach ($t in $tokens) { $idx = 0; for ($i = $s; $i -le $e; $i++) { if ($src[$i].Contains($t)) { $idx = $i + 1; break } }; Write-Output ("SPAN-LINE [" + $sig + "] [" + $t + "] = " + $idx) }
    }

Every printed line number is absolute in the file; 0 means the token does not occur inside that span. `catch (Exception)` (closing parenthesis directly after the type) does not occur in `catch (Exception ex)`, so the two tokens discriminate the two clauses.

**CMD-WINDOWS** (locates the six edit windows E1 to E6 on the base text by their start and end tokens and prints each as `WINDOW <name> = <start>-<end>`; used at P0-T3 on the base text and at P1-T10 to check that every hunk of the anchored diff lies inside a base-side window):

    Set-Location -LiteralPath "WORKTREE"
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $base = @(git show ("BASE-SHA:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs")); if ($base.Count -gt 0) { $base[0] = $base[0].TrimStart([char]0xFEFF) }
    function Find-Line([string[]]$lines, [string]$token, [int]$from) { for ($i = $from; $i -lt $lines.Count; $i++) { if ($lines[$i].Contains($token)) { return $i + 1 } }; return 0 }
    $w1s = Find-Line $base "The toggle-click boundary: the only place in this type that observes a fault with a" 0; $w1e = $w1s + 1
    $w2s = Find-Line $base "itself and reported through <c>logError</c>. For a key whose prime did not run to" 0; $w2e = Find-Line $base "can rely on the fault having been reported." 0
    $w3s = Find-Line $base "The observer is a continuation rather than a <c>catch</c> clause, so this type keeps" 0; $w3e = Find-Line $base "never faults or cancels." 0
    $w4a = Find-Line $base "Observes the outcome of a prime. On any outcome other than ran-to-completion the cache" 0; $w4s = $w4a - 1
    $w4sig = Find-Line $base "private void CompletePrime(Task completed, string engineName)" 0; $w4e = 0; for ($i = $w4sig; $i -lt $base.Count; $i++) { if ($base[$i].TrimEnd() -eq "        }") { $w4e = $i + 1; break } }
    $w5s = Find-Line $base "This method never throws, because its caller is an <c>async void</c> Office handler" 0; $w5e = Find-Line $base "whose faults would otherwise become unobserved." 0
    $w6s = 0; for ($i = 0; $i -lt $base.Count; $i++) { if ($base[$i].Trim() -eq "catch (Exception ex)") { $w6s = $i + 1; break } }
    $w6e = 0; if ($w6s -gt 0) { for ($i = $w6s; $i -lt $base.Count; $i++) { if ($base[$i].TrimEnd() -eq "            }") { $w6e = $i + 1; break } } }
    $windows = @(@("E1", $w1s, $w1e), @("E2", $w2s, $w2e), @("E3", $w3s, $w3e), @("E4", $w4s, $w4e), @("E5", $w5s, $w5e), @("E6", $w6s, $w6e))
    foreach ($w in $windows) { Write-Output ("WINDOW " + $w[0] + " = " + $w[1] + "-" + $w[2]) }
    Write-Output ("WINDOW-E1-LINE2-IS-CATCH-CLAUSE: " + ($w1e -gt 1 -and $base[$w1e - 1].Trim() -eq "/// <c>catch</c> clause."))
    Write-Output ("WINDOW-E4-STARTS-AT-SUMMARY: " + ($w4s -gt 0 -and $base[$w4s - 1].Trim() -eq "/// <summary>"))
    Write-Output ("WINDOW-E6-LINE3-IS-TOGGLE-SINK-CALL: " + ($w6s -gt 0 -and $base[$w6s + 1].Trim() -eq "_logError(BuildToggleFailedMessage(engineName), ex);"))
    $hunks = @(git diff -U0 BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | Where-Object { $_.StartsWith("@@") })
    Write-Output ("HUNK-COUNT: " + $hunks.Count)
    $outside = 0; $touched = @{}
    foreach ($h in $hunks) { $mm = [regex]::Match($h, "^@@ -(\d+)(,(\d+))? "); $a = [int]$mm.Groups[1].Value; $n = if ($mm.Groups[3].Success) { [int]$mm.Groups[3].Value } else { 1 }; $b = if ($n -eq 0) { $a } else { $a + $n - 1 }; $in = ""; foreach ($w in $windows) { if ($a -ge $w[1] -and $b -le $w[2]) { $in = $w[0]; $touched[$in] = $true } }; if ($in -eq "") { $outside++ }; Write-Output ("HUNK " + $h + " base=" + $a + "-" + $b + " window=" + $(if ($in -eq "") { "NONE" } else { $in })) }
    Write-Output ("HUNKS-OUTSIDE-WINDOWS: " + $outside)
    Write-Output ("WINDOWS-TOUCHED: " + (@($touched.Keys | Sort-Object) -join ","))

A deletion-only hunk has a zero-length new side but a non-empty base range, and an insertion-only hunk (`,0` on the base side) is attributed to the base line it follows; both forms are checked against the windows by their base range. On the base text the windows are E1 154-155, E2 246-248, E3 295-301, E4 351-382, E5 167-168 and E6 182-185 (fact 1); these numbers are recorded, not gated. The gated base shape is: every start and end non-zero, `WINDOW-E1-LINE2-IS-CATCH-CLAUSE: True`, `WINDOW-E4-STARTS-AT-SUMMARY: True`, `WINDOW-E6-LINE3-IS-TOGGLE-SINK-CALL: True`, E1 end = start + 1, E2 end = start + 2, E3 end = start + 6, E4 end minus start = 31, E5 end = start + 1, E6 end = start + 3, and `HUNK-COUNT: 0` before any edit. After E1 to E6 the expected result is `HUNKS-OUTSIDE-WINDOWS: 0` and `WINDOWS-TOUCHED: E1,E2,E3,E4,E5,E6`. The E6 search takes the first line that trims to `catch (Exception ex)`; the base file has exactly one (fact 1), and the window end is the first following line of exactly twelve spaces and a closing brace, which is the clause's own closing brace at 185.

### Phase 0 — Baseline Capture

Every Phase 0 task is read-only with respect to the code trees: no file under TaskMaster/ or TaskMaster.Test/ is edited before P1-T1. A stop condition named in a task ends the run at that task; the executor reports it and does not work around it.

- [ ] [P0-T1] Read the policy documents in the policy-compliance-order sequence — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — plus .claude/rules/plan-acceptance-gates.md, .claude/rules/tonality.md and .claude/skills/acceptance-criteria-tracking/SKILL.md, and record the read in FEATURE/evidence/baseline/phase0-instructions-read.md.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line naming CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md and .claude/rules/csharp.md in that order, and one `Read:` line per document read giving its repository-relative path. No policy document is modified (P2-T13 observes this in the footprint).
- [ ] [P0-T2] Record the anchor, the branch, the code-tree state and the minor-audit source shape of `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md` in FEATURE/evidence/baseline/scope-and-anchor.md (this task creates the file; P0-T15 appends to it).
  - Command: `git rev-parse HEAD` (its output is BASE-SHA, D-5); `git rev-parse --abbrev-ref HEAD`; `git diff --exit-code BASE-SHA -- TaskMaster TaskMaster.Test`; `git status --porcelain --untracked-files=all`; then `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $f = "docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947"; $l = @(Get-Content -LiteralPath "$f/issue.md" -Encoding UTF8); $a = [array]::IndexOf($l, "## Acceptance Criteria"); $b = [array]::IndexOf($l, "## Logs / Screenshots"); "HEADINGS ac=$($a + 1) logs=$($b + 1)"; "WORK_MODE_LINES=$(@($l | Where-Object { $_ -eq "- Work Mode: minor-audit" }).Count)"; "SCOPE_CONSOLIDATION_HEADINGS=$(@($l | Where-Object { $_ -eq "## Scope Consolidation" }).Count)"; $sec = @(); if ($a -ge 0 -and $b -gt $a) { $sec = @($l[($a + 1)..($b - 1)]) }; "AC_OPEN=$(@($sec | Where-Object { $_.StartsWith("- [ ] ") }).Count) AC_DONE=$(@($sec | Where-Object { $_.StartsWith("- [x] ") }).Count)"; "SPEC_PRESENT=$(Test-Path -LiteralPath "$f/spec.md") USER_STORY_PRESENT=$(Test-Path -LiteralPath "$f/user-story.md")"'` (this payload is the `ISSUE-SHAPE` payload; P2-T15 to P2-T22 re-run it unchanged).
  - Acceptance, all required: `BASE-SHA:` records the 40-character output of the first command, and the `EXIT_CODE:` row is the diff's exit code; the branch command prints `bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947` (any other value is `BRANCH MISMATCH`: stop); the anchored diff exits 0 (otherwise `CODE TREE DIFFERS FROM BASE`: stop); no porcelain line names a path under TaskMaster/ or TaskMaster.Test/ (otherwise `CODE TREE DIRTY AT ANCHOR`: stop), and every porcelain line is recorded verbatim under `PRE-EXISTING-WORKTREE-PATHS:`, or `NONE`; `HEADINGS` is recorded (45 and 55 are expected; the check-off tasks locate criteria by ordinal, so the line numbers are not gated); `WORK_MODE_LINES=1`; `SCOPE_CONSOLIDATION_HEADINGS=1`; `AC_OPEN=7 AC_DONE=0`; `SPEC_PRESENT=False USER_STORY_PRESENT=False`. Any failing issue-shape clause is `MINOR-AUDIT SOURCE SHAPE MISMATCH`: stop. The artifact also lists the three Write Set code paths verbatim and names the must-not-touch paths of the Write Set section.
- [ ] [P0-T3] Verify the base shape of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` and its six edit windows, and record FEATURE/evidence/baseline/anchor-production-shape.md.
  - Command: `CMD-TOKEN-COUNT` with `FILE` `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` and `TOKEN` `"catch (", "catch (Exception ex)", "catch (Exception)", "lock (", "exactly one <c>catch</c>", "the only place in this type that observes a fault with a", "can rely on the fault having been reported.", "This method never throws, because its caller is an", "Intentionally discarded", "RibbonCommandBoundary.SafeLog", "_logError(BuildToggleFailedMessage(engineName), ex);", "_logError(BuildPrimeFailedMessage(engineName), failure);", "_primeTasks.TryRemove(engineName, out _);", "Report-then-clear is load-bearing"`; then `CMD-SPANS`; then `CMD-WINDOWS`.
  - Acceptance, tokens: `catch (` 1, `catch (Exception ex)` 1, `catch (Exception)` 0, `lock (` 1, `exactly one <c>catch</c>` 1, `the only place in this type that observes a fault with a` 1, `can rely on the fault having been reported.` 1, `This method never throws, because its caller is an` 1, `Intentionally discarded` 0, `RibbonCommandBoundary.SafeLog` 0, and each of the last four tokens 1. Every `FIRST-LINE` value is recorded as an observation.
  - Acceptance, spans: for `internal async Task HandleToggleClickAsync(`: `SPAN-TRY` 1, `SPAN-CATCH` 1 and the `SPAN-LINE` of `catch (Exception)` 0; for `private void StartObservedPrime(`: `SPAN-TRY` 1, `SPAN-FINALLY` 1, `SPAN-CATCH` 0; for `private void CompletePrime(`: `SPAN-TRY` 0, `SPAN-CATCH` 0, and the `SPAN-LINE` of `_logError(BuildPrimeFailedMessage(engineName), failure);` non-zero and less than the `SPAN-LINE` of `_primeTasks.TryRemove(engineName, out _);`, recorded as `COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR`.
  - Acceptance, windows: the gated base shape stated after `CMD-WINDOWS` in the Command reference holds (every start and end non-zero, the three `WINDOW-` flags `True`, the six width relations, `HUNK-COUNT: 0`); the six `WINDOW` lines are recorded as `BASE-WINDOW-E1:` to `BASE-WINDOW-E6:`. Any failing token, span or window clause is `ANCHOR SHAPE MISMATCH`: stop for re-planning.
- [ ] [P0-T4] Re-derive the test-side anchor facts for TaskMaster.Test/TaskMaster.Test.csproj and the four existing coordinator partials under TaskMaster.Test/Ribbon, and record FEATURE/evidence/baseline/anchor-test-side.md.
  - Command: `CMD-LINECOUNT`; `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\TaskMaster.Test.csproj` and `TOKEN` `"EngineToggleStateCoordinatorTests.cs", "EngineToggleStateCoordinatorTests.Race.cs", "EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs", "EngineToggleStateCoordinatorTests.PrimeRegistration.cs", "EngineToggleStateCoordinatorTests.ThrowingSink.cs"`; `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs` and `TOKEN` `"private sealed class Harness", "new Mock<IAppItemEngines>(MockBehavior.Strict);", "Errors.Add(new LoggedError(message, exception));", "OnLogError?.Invoke(message, exception);", "internal Action<string, Exception> OnLogError { get; set; }", "internal List<string> Invalidations", "internal List<string> Notifications", "internal List<LoggedError> Errors", "private sealed class LoggedError", "private const string SpamEngine =", "private const string SpamToggleControlId =", "HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate", "GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime", "GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse"`; `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` and `TOKEN` `"GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged", "[TestMethod]"`; `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs` and `TOKEN` `"GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker"`; `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs` and `TOKEN` `"GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns", "GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime", "GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime"`; then `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $src = @(Get-ChildItem -LiteralPath "TaskMaster.Test" -Recurse -File -Filter "*.cs" | ForEach-Object { Get-Content -LiteralPath $_.FullName -Encoding UTF8 }); foreach ($n in @("GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared", "HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport", "ThrowingSink")) { "NEW-NAME [$n] = $(@($src | Where-Object { $_.Contains($n) }).Count)" }; "CS_LINES_SCANNED=$($src.Count)"'`.
  - Acceptance, all required: `LINES` of the production file, the main fixture, the Race, PrimeFaultOrdering and PrimeRegistration partials are recorded as `BASE-LINES-PROD:`, `BASE-LINES-MAIN:`, `BASE-LINES-RACE:`, `BASE-LINES-PFO:` and `BASE-LINES-PR:` (442, 470, 277, 77 and 175 are expected and recorded; only "each at most 500" is gated here), and the ThrowingSink path reads `ABSENT`; in the project file the first four tokens count exactly 1 each and the ThrowingSink token 0, and the `FIRST-LINE` of the PrimeRegistration token is recorded as `PR-ENTRY-LINE:`; in the main fixture every token counts exactly 1, and the `FIRST-LINE` of `Errors.Add(new LoggedError(message, exception));` is less than that of `OnLogError?.Invoke(message, exception);` (the harness records the report before the hook can throw, D-3); in the PrimeFaultOrdering partial both tokens count exactly 1; the Race token and each PrimeRegistration token count exactly 1; every `NEW-NAME` count is 0 and `CS_LINES_SCANNED=` is at least 1000 (the scan read the test tree). Any failing clause is `FIXTURE SHAPE MISMATCH`: stop.
- [ ] [P0-T5] Provision the repository .NET SDK with scripts/vscode/Install-RepoDotNetSdk.ps1 (guarded) and record FEATURE/evidence/baseline/bootstrap-sdk.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version'`
  - Acceptance: `SDK_MARKER=True`; `dotnet --version` prints a version string rather than the global.json error message; `EXIT_CODE: 0`. The installer's filesystem marker is the gate; version equality is not asserted because global.json rolls forward within the feature band.
- [ ] [P0-T6] Restore the manifest tools with `dotnet tool restore` against the repository manifest dotnet-tools.json and record FEATURE/evidence/baseline/bootstrap-tool-restore.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'`
  - Acceptance: `RESTORE_EXIT=0`; the local tool list carries a row whose Package Id is `csharpier` and whose Version is `1.2.6`; `CHECK_HELP_EXIT=0`. The artifact transcribes only the Package Id and Version columns, because the Manifest column carries an absolute path.
- [ ] [P0-T7] Restore NuGet packages with scripts/vscode/Invoke-Restore.ps1, check the analyzer paths of TaskMaster/TaskMaster.csproj and TaskMaster.Test/TaskMaster.Test.csproj, and record FEATURE/evidence/baseline/bootstrap-nuget-restore.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"; foreach ($proj in @("TaskMaster\TaskMaster.csproj", "TaskMaster.Test\TaskMaster.Test.csproj")) { $dir = Split-Path -Parent $proj; [xml]$x = Get-Content -LiteralPath $proj -Raw; $missing = @($x.SelectNodes("//*[local-name()=""Analyzer""]") | Where-Object { -not (Test-Path -LiteralPath (Join-Path $dir $_.GetAttribute("Include"))) }).Count; "ANALYZER_MISSING $proj = $missing" }'`
  - Acceptance: `RESTORE_EXIT=0`; `PACKAGE_DIRS=` at least 1; both `ANALYZER_MISSING` values 0. A non-zero `ANALYZER_MISSING` is `ANALYZER PATH SKEW`: stop and report the unresolved Include values.
- [ ] [P0-T8] Provision the dotnet-coverage global tool (guarded) and record FEATURE/evidence/baseline/bootstrap-dotnet-coverage.md.
  - Command: `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'`
  - Acceptance: `DOTNET_COVERAGE_RESOLVED=True`; a version line is printed; `EXIT_CODE: 0`.
- [ ] [P0-T9] Capture the read-only formatter baseline with `dotnet tool run csharpier check .` over the whole worktree (formatter scope set by .csharpierignore) and record FEATURE/evidence/baseline/csharpier-check-baseline.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`
  - Acceptance: the printed `CSHARPIER_EXIT_CODE:` value is the `EXIT_CODE:` row and must be 0. A non-zero value records every path CSharpier reported as unformatted, one per line, and stops the run with `FORMAT BASELINE NOT CLEAN`: D-12 relies on a clean base, and repairing pre-existing drift would widen the footprint.
- [ ] [P0-T10] Capture the analyzer baseline with `CMD-REBUILD` (`GATEARGS` analyzers, `TASKID` p0-t10) over TaskMaster.sln and record FEATURE/evidence/baseline/msbuild-analyzer-baseline.md (`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).
  - Acceptance, all required: `EXIT_CODE: 0` (the printed `MSBUILD_EXIT_CODE:`); `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; `CSC_OUT_TASKMASTER:` and `CSC_OUT_TASKMASTER_TEST:` each at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded as `ANALYZER-BASELINE-WARNINGS:`; `TEST_DLL_EXISTS: True`; `UCS_TEST_DLL_EXISTS: True`. A non-zero exit is `ANALYZER BASELINE NOT CLEAN`: stop.
- [ ] [P0-T11] Capture the nullable type-check baseline with `CMD-REBUILD` (`GATEARGS` nullable, `TASKID` p0-t11) over TaskMaster.sln and record FEATURE/evidence/baseline/msbuild-nullable-baseline.md (`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; no Nullable property override).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded as `NULLABLE-BASELINE-WARNINGS:`; both `_DLL_EXISTS:` values `True`. A non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop.
- [ ] [P0-T12] Run the stall probe over UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll with `CMD-VSTEST` (`ASSEMBLY-UCS`, `FILTER-STALL`, `TASKID` p0-t12, `NAMES-NONE`) and record FEATURE/evidence/baseline/stall-probe.md.
  - Acceptance: the artifact records `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`, or 3 when the trx is absent), `ExpectedExitCode:` equal to the observed value when it is non-zero, `TRX_PRESENT:`, `SEQUENCE_FILES:`, the `COUNTERS` line when present and every `MESSAGE` line with absolute paths replaced by REDACTED-PATH; then exactly one `STALL-PROBE:` line — `CLEAR` when `EXIT_CODE: 0`, `failed` is 0 and `SEQUENCE_FILES: 0`, otherwise `REPRODUCES` — and exactly one `COVERAGE-ROUTE:` line — `RUNNER` under `CLEAR`, `DIRECT` under `REPRODUCES` — with the sentence that the four classes are a pre-existing local stall that CI executes (fact 9). The probe runs once and is never re-run; both `STALL-PROBE:` values complete this task.
- [ ] [P0-T13] Capture the pre-change coordinator fixture run over TaskMaster.Test\bin\Debug\TaskMaster.Test.dll with `CMD-VSTEST` (`ASSEMBLY-TM`, `FILTER-COORD`, `TASKID` p0-t13, `NAMES-947`) and record FEATURE/evidence/baseline/coordinator-tests-baseline.md.
  - Acceptance, all required: `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `EXIT_CODE: 0`; the `COUNTERS` line has `executed` at least 25 and `failed=0`, and is recorded as `BASELINE-COUNTERS:` with its total as `BASELINE-TOTAL:`; each of the eight pre-existing `NAMES-947` entries (the fifth to the twelfth) has a `RESULT` line reading `Passed`; no `RESULT` line names any of the first four `NAMES-947` entries; no `FAILED` line. Any other outcome is `BASELINE NOT GREEN`: stop, because the fail-before and pass-after populations are measured against this one.
- [ ] [P0-T14] Capture the baseline repository-wide test-and-coverage run for TaskMaster/Ribbon/EngineToggleStateCoordinator.cs by the route P0-T12 fixed and record FEATURE/evidence/baseline/coverage-baseline.md. Run the `STRAY_TEST_PROCESSES` check first (Execution conventions); under `RUNNER` run `CMD-COVERAGE-RUNNER` with `STAGE` baseline; under `DIRECT` run `CMD-COVERAGE-DIRECT` with `STAGE` baseline; then, unless branch (d0) applies, run `CMD-COVERAGE-POST` with `STAGE` baseline and `RAW` per the Command reference rule.
  - Fallback (D-6): a RUNNER attempt whose `DISCOVERY_FAILURE_MESSAGE:` is non-empty is recorded as `ROUTE-REASON: RUNNER-DISCOVERY-FAILED`, and the stage is re-issued once by `CMD-COVERAGE-DIRECT` with `EXCLUSION` empty; the recorded `COVERAGE-ROUTE:` then reads `DIRECT`. A RUNNER run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop.
  - Artifact: `Timestamp:`, `Command:` (the route's canonical command and the filter it applied), `EXIT_CODE:` (`RUNNER_EXIT_CODE:` or `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when it is non-zero, an `Output Summary:` of at most 20 lines carrying `BASE-SHA:`, `COVERAGE-ROUTE:`, the exit code, `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line (the numeric baseline headline), the `ROOT` line, the three `METHOD` rows and `CATCH-ARM-COUNT:`, and a `Details:` section recording `RAW:`, `DISCOVERED_LINE:` or `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `TRX_PRESENT:`, `SEQUENCE_FILES:` (DIRECT), `THRESHOLD_MESSAGE:` and `COLLECT_FAILURE_MESSAGE:` (RUNNER), the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the summary lines verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, `FAILED-SET:`, `COORD-CLASS-NODES:`, `COORD-LINES`, `COORD-BRANCHES` and every `METHOD-LINE` row.
  - Branches, checked in order: (d0) `SEQUENCE_FILES:` greater than 0 (DIRECT) or `TRX_PRESENT: False` is `COVERAGE RUN ABORTED`: stop, report the last lines of the collector log with absolute paths replaced, do not run `CMD-COVERAGE-POST`, do not re-run. (c) a non-empty `THRESHOLD_MESSAGE:` (RUNNER) or a `LINE-FLOOR: NOT MET` or `BRANCH-FLOOR: NOT MET` line is `COVERAGE FLOOR BASELINE NOT MET`: the projection is recorded and the run stops. (b) a non-zero exit with no floor failure and a `FAILED-SET:` that is exactly the single name `TryAddValuesAsync_UpdatesExistingValue` (the sporadic failure tracked as issue 780, unrelated to this change) is recorded as `BASELINE-ADMISSIBLE-FAILURE:` and completes this task with `ExpectedExitCode:` equal to the observed value; any other non-empty `FAILED-SET:` is `BASELINE NOT GREEN`: stop and report the `Failed tests:` summary line. (a) exit 0 with both floors met: complete. (d) anything else, in particular a non-zero exit with an empty `FAILED-SET:`, is `COVERAGE RUN ABORTED` with the handling of (d0).
  - Acceptance, all required: branch (a) or branch (b); `COORD-CLASS-NODES: 1`; `METHOD HandleToggleClickAsync`, `METHOD StartObservedPrime` and `METHOD CompletePrime` each with `elements=` at least 1; `CATCH-ARM-COUNT: 0` and no line beginning `CATCH-ARM ` (the negative control for P2-T10, D-7); the projection block contains a `package` element named `TaskMaster` with a `LINE` and a `BRANCH` counter; the summary block's first line begins `Test run outcome:`; the artifact contains no absolute path. coverage\baseline-947.cobertura.xml and coverage\baseline-947.trx stay on disk, git-ignored, for P2-T10.
- [ ] [P0-T15] Record the pre-change hashes and line counts of TaskMaster/Ribbon/EngineToggleStateCoordinator.cs and the coordinator partials under TaskMaster.Test/Ribbon with `CMD-HASH` and `CMD-LINECOUNT` in FEATURE/evidence/baseline/file-line-counts-baseline.md, then list FEATURE/evidence/baseline/ and append the listing to FEATURE/evidence/baseline/scope-and-anchor.md under a `PHASE0-ARTIFACTS:` heading.
  - Acceptance, all required: the production hash is recorded as `BASE-HASH-PROD:` and the PrimeFaultOrdering hash as `BASE-HASH-PFO:`; the ThrowingSink path reads `ABSENT` in both commands; the five other `LINES` values each equal their `BASE-LINES-*:` value from P0-T4; the listing names all fifteen Phase 0 artifacts of the Write Set (phase0-instructions-read.md through file-line-counts-baseline.md) and no other file; every command-bearing artifact among them carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`; every artifact whose `EXIT_CODE:` is non-zero carries `ExpectedExitCode:` with the same value. A missing artifact or field leaves its Phase 0 task unchecked and stops the run with `PHASE 0 EVIDENCE INCOMPLETE`.

### Phase 1 — Regression Tests First and the Minimal Sink Guard

This phase is the constrained small-path implementation block. P1-T1 and P1-T7 are the two code-writing steps (the delegated small-path engineer, or the executor where the orchestrator assigns it, writes the Delivered Source texts verbatim); every other task is a command-bearing gate. The four new tests are written and observed failing against the unchanged production file before any production edit (CLAUDE.md bugfix workflow, D-4).

- [ ] [P1-T1] Create `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` with the whole text of the Delivered Source block for the new partial, removing the four Markdown-indent spaces from every line.
  - Acceptance (observed by P1-T2 and P1-T3): the file exists, untracked; it carries the four test methods and no other member; no other file is modified by this task. A PreToolUse refusal of the write is `PRE-IMPLEMENTATION GATE BLOCKED`: stop (D-9).
- [ ] [P1-T2] Format the new partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` alone with CSharpier and record FEATURE/evidence/regression-testing/throwing-sink-partial-format.md with a before-and-after observation.
  - Command: `CMD-HASH` before; `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool run csharpier format TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; `CMD-HASH` after; `git status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test` after.
  - Acceptance, all required: `CSHARPIER_EXIT_CODE: 0` is the `EXIT_CODE:` row; the six hashes are recorded and `PARTIAL-FORMAT-REWROTE:` is `True` or `False` according to whether the two partial hashes differ (either value completes this task: a first format of a new file may change layout, and the token gates of P1-T3 are counted on the formatted text); the production hash equals `BASE-HASH-PROD:` before and after, and the PrimeFaultOrdering hash equals `BASE-HASH-PFO:` before and after (the scoped format touched no other file); the porcelain span prints exactly one line, `?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`. The console line `Formatted N files` is not used as a rewrite count, because CSharpier reports files processed, not files changed.
- [ ] [P1-T3] Verify the formatted content of `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` with `CMD-TOKEN-COUNT` and `CMD-LINECOUNT`, and record FEATURE/evidence/regression-testing/throwing-sink-partial-tokens.md (this task creates the file; P2-T2 appends a `POST-FORMAT:` section).
  - Command: `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs` and `TOKEN` `TOKENS-PARTIAL-947`: `"[TestMethod]", "[TestClass]", "new Mock<", "MockBehavior", "private sealed class", "var harness = new Harness();", "// Arrange", "// Act", "// Assert", "SetupSequence(x => x.EngineActiveAsync(SpamEngine))", "Times.Exactly(2),", ".NotBeSameAs(", ".Be(TaskStatus.RanToCompletion,", ".NotThrowAsync(", "ThrowsAsync(failure)", "HandleToggleClickAsync(SpamEngine)", "Thread.Sleep", "Task.Delay", "GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared", "HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport", "a throwing sink leaves no marker behind, so the later read starts a new prime", "the later read registered a new prime", "the sink was invoked once before it threw", "the sink receives the injected exception unchanged", "the new prime read the engine as active and cached that value", "only the successful prime changed state to display", "a canceled task carries no exception to unwrap, so one is synthesized", "the prime handle never faults", "the report is attempted before the marker is cleared", "a failed prime leaves nothing to display", "the sink exception is contained, so the marker is still cleared", "the click boundary contains a failure of the sink itself", "the sink receives the toggle fault unchanged", "a failed toggle changed no state to display", "a fault is logged, not surfaced as a notice", "toggle failed", "Regression for issue #947", "sink failed"`; then `CMD-LINECOUNT`.
  - Acceptance, all required: every count equals the value the Delivered Source paragraph beginning `Expected token counts on the formatted partial` gives for that token (`[TestMethod]` 4; `[TestClass]`, `new Mock<`, `MockBehavior`, `private sealed class`, `Thread.Sleep` and `Task.Delay` 0; `var harness = new Harness();`, `// Arrange`, `// Act` and `// Assert` 4 each; `SetupSequence(x => x.EngineActiveAsync(SpamEngine))`, `Times.Exactly(2),` and `.NotBeSameAs(` 2 each; `.Be(TaskStatus.RanToCompletion,`, `.NotThrowAsync(`, `ThrowsAsync(failure)`, `HandleToggleClickAsync(SpamEngine)` and each of the four method names 1; the eighteen reason and literal tokens at the counts that paragraph lists); the partial's `LINES` value is at most 500 and is recorded as `PARTIAL-LINES-AFTER-P1:`. A count below its expected value on a token that is present in the file only across a line break is `FORMATTER SPLITS GATED TOKEN`: record the token and the formatter's layout and stop for re-planning; any other mismatch is repaired by rewriting the partial to the Delivered Source text and repeating P1-T2 and this task, recorded under `REPAIR-n:`.
- [ ] [P1-T4] Register the new partial in `TaskMaster.Test/TaskMaster.Test.csproj` by inserting `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />` on the line immediately after the PrimeRegistration entry (`PR-ENTRY-LINE:` from P0-T4), with the four-space indentation of its neighbours, and record FEATURE/evidence/qa-gates/csproj-registration.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $p = @(Get-Content -LiteralPath "TaskMaster.Test\TaskMaster.Test.csproj" -Encoding UTF8); $pr = 0; $new = 0; for ($i = 0; $i -lt $p.Count; $i++) { if ($p[$i].Contains("EngineToggleStateCoordinatorTests.PrimeRegistration.cs")) { $pr = $i + 1 }; if ($p[$i].Contains("EngineToggleStateCoordinatorTests.ThrowingSink.cs")) { $new = $i + 1 } }; "PR_LINE=$pr NEW_LINE=$new NEW_COUNT=$(@($p | Where-Object { $_.Contains("EngineToggleStateCoordinatorTests.ThrowingSink.cs") }).Count)"; $q = [string][char]34; $want = "<Compile Include=" + $q + "Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" + $q + " />"; "NEW_ENTRY_EXACT=$(@($p | Where-Object { $_.Trim() -eq $want }).Count)"; $ind = { param($s) $s.Length - $s.TrimStart().Length }; "NEW_ENTRY_INDENT_MATCHES=$(($new -gt 0) -and ($pr -gt 0) -and ((& $ind $p[$new - 1]) -eq (& $ind $p[$pr - 1])))"; git diff --numstat BASE-SHA -- TaskMaster.Test/TaskMaster.Test.csproj'` and `git status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test`.
  - Acceptance, all required: `NEW_COUNT=1`; `NEW_ENTRY_EXACT=1`; `PR_LINE` equals `PR-ENTRY-LINE:` (the insertion landed below it); `NEW_LINE` equals `PR_LINE` plus 1; `NEW_ENTRY_INDENT_MATCHES=True`; the anchored numstat line reports 1 insertion and 0 deletions for TaskMaster.Test/TaskMaster.Test.csproj (this is the `EXIT_CODE:` row's command, exit 0); the porcelain span prints exactly the two lines ` M TaskMaster.Test/TaskMaster.Test.csproj` and `?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`. The project file is outside the formatter (.csharpierignore line 12), so no format pass follows this edit.
- [ ] [P1-T5] Build TaskMaster.sln with `CMD-BUILD` (`TASKID` p1-t5) so TaskMaster.Test\bin\Debug\TaskMaster.Test.dll carries the new partial, and record FEATURE/evidence/regression-testing/build-before-fix.md.
  - Acceptance, all required: `MSBUILD_EXIT_CODE: 0` is the `EXIT_CODE:` row; `ERRORS: 0`; `TEST_DLL_ADVANCED: True`; `CSC_OUT_TASKMASTER_TEST:` at least 1. A green build is what makes P1-T6's failures assertion failures rather than compile errors; a non-zero exit is `NEW PARTIAL DOES NOT COMPILE`: stop and report the error lines naming the partial.
- [ ] [P1-T6] [expect-fail] Run the coordinator fixture against the unchanged production file `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with `CMD-VSTEST` (`ASSEMBLY-TM`, `FILTER-COORD`, `TASKID` p1-t6, `NAMES-947`) and record FEATURE/evidence/regression-testing/throwing-sink-fail-before.md.
  - Command, control environment (before the run): `CMD-HASH`; `git diff --exit-code BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`; `git diff --exit-code BASE-SHA -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings`.
  - Artifact: `Timestamp:`, `Command:`, `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed non-zero value, an `Output Summary:` with the `COUNTERS` line and every `RESULT`, `FAILED` and `MESSAGE` line (absolute paths replaced by REDACTED-PATH), and an `Environment of the control:` paragraph recording `PROD-HASH-AT-CONTROL:` (the production hash from `CMD-HASH`), the two anchored diff exit codes, and the statement that the new partial and its compile entry are present.
  - Acceptance, all required: `TRX_PRESENT: True`; `SEQUENCE_FILES: 0` (no hang); `EXIT_CODE:` non-zero with `ExpectedExitCode:` equal to it; `PROD-HASH-AT-CONTROL:` equals `BASE-HASH-PROD:` and both anchored diffs exit 0 (the production file and the run settings are at the base); the `COUNTERS` total equals `BASELINE-TOTAL:` plus 4 with `failed=4`; the `FAILED` lines name exactly the first four `NAMES-947` entries and no other test; the `MESSAGE` lines of `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime` and `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime` each contain `a throwing sink leaves no marker behind, so the later read starts a new prime` and each match the case-insensitive pattern `exactly 2 times, but was 1 time`; the `MESSAGE` line of `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` contains `the sink exception is contained, so the marker is still cleared`; the `MESSAGE` line of `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` contains `the click boundary contains a failure of the sink itself` and `sink failed`; `RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed`; `RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed`; every other pre-existing `NAMES-947` entry reads `Passed`. If any of the four new tests reads `Passed`, or a failure message lacks its reason string, the negative control did not reproduce the defect: stop with `FAIL-BEFORE NOT REPRODUCED` and do not proceed to P1-T7.
- [ ] [P1-T7] Apply edits E1 to E6 of the Delivered Source section to `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, each replacing exactly the base lines that edit names, working from the bottom of the file upward (E4, E3, E2, E6, E5, E1), so no edit shifts the anchor text of a later one, and modify no other line or file.
  - Acceptance (observed by P1-T8 to P1-T10): every documented token sits whole on one physical line; the constructor, `GetPressed`, `ExecuteToggleAsync`, `StartPrimeIfNeeded`, the `StartObservedPrime` code, the `GetPrimeTask` code, `ApplyPrimeAsync` and the message builders are unchanged. A PreToolUse refusal of the edit is `PRE-IMPLEMENTATION GATE BLOCKED`: stop (D-9).
- [ ] [P1-T8] Format the edited production file `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` alone with CSharpier and record FEATURE/evidence/qa-gates/production-format.md with a before-and-after observation.
  - Command: `CMD-HASH` before; `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool run csharpier format TaskMaster\Ribbon\EngineToggleStateCoordinator.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; `CMD-HASH` after; `git status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test` after.
  - Acceptance, all required: `CSHARPIER_EXIT_CODE: 0` is the `EXIT_CODE:` row; the six hashes are recorded and `PRODUCTION-FORMAT-REWROTE:` is `True` or `False` according to whether the two production hashes differ (either value completes this task; the P1-T9 gates are counted on the formatted text); the partial hash is identical before and after, and equals the after-hash of P1-T2; the PrimeFaultOrdering hash equals `BASE-HASH-PFO:`; the production hash after differs from `BASE-HASH-PROD:` (the edit is present); the porcelain span prints exactly the three lines ` M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, ` M TaskMaster.Test/TaskMaster.Test.csproj` and `?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`.
- [ ] [P1-T9] Verify the fixed shape and documentation tokens of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with `CMD-TOKEN-COUNT` and `CMD-SPANS`, and record FEATURE/evidence/qa-gates/production-edit-scope.md (this task creates the file; P2-T2 appends a `POST-FORMAT:` section).
  - Command: `CMD-TOKEN-COUNT` with `FILE` `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` and `TOKEN` `TOKENS-PROD-947`: `"catch (", "catch (Exception ex)", "catch (Exception)", "lock (", "Intentionally discarded: see the remarks on this method.", "RibbonCommandBoundary.SafeLog", "returned or thrown", "no remaining throw source of its own", "_logError(BuildToggleFailedMessage(engineName), ex);", "_logError(BuildPrimeFailedMessage(engineName), failure);", "the only <c>catch</c> clause in this type that observes an", "The other two are sink guards, here and in", "the click boundary and the two sink guards.", "can rely on the report having", "A failure thrown by the sink itself is contained here", "The sink call is guarded (issue #947).", "The sink call is itself guarded (issue #947)", "This method therefore never throws, even when the", "is guaranteed the report has already been", "exactly one <c>catch</c>", "the only place in this type that observes a fault with a", "can rely on the fault having been reported.", "This method never throws, because its caller is an"`; then `CMD-SPANS`.
  - Acceptance, tokens (all required): every count equals the value the Delivered Source paragraph beginning `Expected post-fix file-level counts` gives: `catch (` 3; `catch (Exception)` 2; `Intentionally discarded: see the remarks on this method.`, `RibbonCommandBoundary.SafeLog`, `returned or thrown` and `no remaining throw source of its own` 2 each; `catch (Exception ex)`, `lock (`, the two `_logError(` statements and the next nine documentation tokens 1 each; the four removed base tokens (the last four of the list) 0 each.
  - Acceptance, spans (all required): for `private void CompletePrime(`: `SPAN-TRY` 1, `SPAN-CATCH` 1, `SPAN-FINALLY` 0, `SPAN-LOCK` 0, and the order `SPAN-LINE` of `Report-then-clear is load-bearing` < `SPAN-KEYWORD` `try` < `SPAN-LINE` of `_logError(BuildPrimeFailedMessage(engineName), failure);` < `SPAN-LINE` of `catch (Exception)` < `SPAN-LINE` of `Intentionally discarded: see the remarks on this method.` < `SPAN-LINE` of `_primeTasks.TryRemove(engineName, out _);`, all non-zero (report precedes clear, and the clear sits after the guard, outside it); for `internal async Task HandleToggleClickAsync(`: `SPAN-TRY` 2, `SPAN-CATCH` 2, `SPAN-FINALLY` 0, `SPAN-LOCK` 0, the order `SPAN-KEYWORD` `try` < `SPAN-LINE` of `catch (Exception ex)` < `SPAN-LINE` of `_logError(BuildToggleFailedMessage(engineName), ex);` < `SPAN-LINE` of `catch (Exception)` < `SPAN-LINE` of `Intentionally discarded: see the remarks on this method.`, all non-zero, and the `SPAN-LINE` of `_logError(BuildPrimeFailedMessage(engineName), failure);` 0 in that span; for `private void StartObservedPrime(`: `SPAN-TRY` 1, `SPAN-FINALLY` 1, `SPAN-CATCH` 0 (unchanged from P0-T3). A failing clause is repaired by re-applying the affected edit to the Delivered Source text and repeating P1-T8 and this task, recorded under `REPAIR-n:`; a token split by the formatter is `FORMATTER SPLITS GATED TOKEN`: stop for re-planning.
- [ ] [P1-T10] Verify with `CMD-WINDOWS` that every change to `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` lies inside the six edit windows, and that every protected file is byte-identical to BASE-SHA, and record FEATURE/evidence/qa-gates/protected-regions-unchanged.md (this task creates the file; P2-T2 appends a `POST-FORMAT:` section).
  - Command: `CMD-WINDOWS`; then `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs TaskMaster/Ribbon/RibbonController.EngineCommands.cs TaskMaster/Ribbon/RibbonCommandBoundary.cs TaskMaster/TaskMaster.csproj | Out-Null; "PROTECTED_FILES_DIFF_EXIT=$LASTEXITCODE"; git diff --exit-code BASE-SHA -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings | Out-Null; "RUNSETTINGS_DIFF_EXIT=$LASTEXITCODE"'`.
  - Acceptance, all required: the six `WINDOW` lines equal `BASE-WINDOW-E1:` to `BASE-WINDOW-E6:` from P0-T3 (the windows are read from the base text, so they cannot move); `HUNK-COUNT:` at least 6; every `HUNK` line names a window; `HUNKS-OUTSIDE-WINDOWS: 0`; `WINDOWS-TOUCHED: E1,E2,E3,E4,E5,E6` (each edit landed; this is the positive control); `PROTECTED_FILES_DIFF_EXIT=0`; `RUNSETTINGS_DIFF_EXIT=0`. The `EXIT_CODE:` row is the payload's exit code (0). A hunk outside every window is `EDIT OUTSIDE WINDOW`: record the hunk header and repair by restoring the base text of that region.
- [ ] [P1-T11] Build TaskMaster.sln with `CMD-BUILD` (`TASKID` p1-t11) so TaskMaster.Test\bin\Debug\TaskMaster.Test.dll and its copy of TaskMaster.dll carry the fix, and record FEATURE/evidence/regression-testing/build-after-fix.md.
  - Acceptance, all required: `MSBUILD_EXIT_CODE: 0` is the `EXIT_CODE:` row; `ERRORS: 0`; `TEST_DLL_ADVANCED: True`; `CSC_OUT_TASKMASTER:` at least 1; `CSC_OUT_TASKMASTER_TEST:` at least 1.
- [ ] [P1-T12] Re-run the original reproduction and the regression tests together over TaskMaster.Test\bin\Debug\TaskMaster.Test.dll with `CMD-VSTEST` (`ASSEMBLY-TM`, `FILTER-COORD`, `TASKID` p1-t12, `NAMES-947`), preceded by `CMD-HASH`, and record FEATURE/evidence/regression-testing/throwing-sink-pass-after.md (this task creates the file; P2-T7 appends a `FINAL-FIXTURE-RUN:` section).
  - Artifact: `Timestamp:`, `Command:` (identical to P1-T6's except the task id segments), `EXIT_CODE:`, an `Output Summary:` with the `COUNTERS` line and every `RESULT` line, `PROD-HASH-AFTER:` from `CMD-HASH`, and the sentence that the only difference between this run and P1-T6 is edits E1 to E6 in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (the partial and its compile entry were present in both runs).
  - Acceptance, all required: `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; the `COUNTERS` total equals `BASELINE-TOTAL:` plus 4 with `failed=0`; all twelve `NAMES-947` entries have a `RESULT` line reading `Passed`; no `FAILED` line; `PROD-HASH-AFTER:` differs from `BASE-HASH-PROD:`. Any failure is `PASS-AFTER NOT GREEN`: stop and report the `FAILED` and `MESSAGE` lines.

### Phase 2 — Final QC Loop

The loop is format, then the read-only format check, then the analyzer rebuild, then the nullable rebuild, then the coverage-enabled test run, in the CLAUDE.md order (P2-T1 and P2-T4 to P2-T8; P2-T2, P2-T3 and P2-T7 are observation steps inside the loop). The restart rule is D-12: a failing or rewriting step stops the run; after a repair confined to the Write Set the loop restarts at P2-T1 and each repeated task appends a `PASS-n:` section to its artifact. The only automatic restart is the single issue 780 re-run that P2-T8 admits. Every acceptance condition after P2-T9 reads the section that survived the last pass.

- [ ] [P2-T1] Run the formatter repository-wide with `dotnet tool run csharpier format .` and record FEATURE/evidence/qa-gates/csharpier-format.md with a before-and-after observation of TaskMaster/Ribbon/EngineToggleStateCoordinator.cs and the new partial.
  - Command: `CMD-HASH` before; `git status --porcelain --untracked-files=all -- . ":(exclude)docs/features" ":(exclude).claude"` before; `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; `CMD-HASH` after; the same scoped porcelain span after.
  - Acceptance, all required: `CSHARPIER_EXIT_CODE: 0` is the `EXIT_CODE:` row; the six hashes are recorded and `REWRITTEN-WRITESET-FILES:` is the number of the two Write Set source paths whose two hashes differ, which must be 0; the PrimeFaultOrdering hash equals `BASE-HASH-PFO:` before and after; both scoped porcelain outputs are recorded verbatim and are identical line sets (a difference is `FORMAT WIDENED FOOTPRINT`: stop, because P0-T9 established a clean base). A non-zero rewritten count is a rewriting step under D-12. The console line `Formatted N files` is not used as a rewrite count.
- [ ] [P2-T2] Re-run the P1-T3, P1-T9 and P1-T10 commands unchanged on the formatted tree and append a `POST-FORMAT:` section to each of FEATURE/evidence/regression-testing/throwing-sink-partial-tokens.md, FEATURE/evidence/qa-gates/production-edit-scope.md and FEATURE/evidence/qa-gates/protected-regions-unchanged.md.
  - Acceptance: every acceptance clause of P1-T3, P1-T9 and P1-T10 holds on the post-format tree, with every `TOKEN`, `FIRST-LINE`, `SPAN`, `SPAN-KEYWORD`, `SPAN-LINE`, `WINDOW` and `HUNK` row re-printed in the section. A failing clause is a failing step under D-12. These `POST-FORMAT:` sections are the sections the check-off tasks cite.
- [ ] [P2-T3] Audit the post-format line counts of TaskMaster/Ribbon/EngineToggleStateCoordinator.cs and the five coordinator partials under TaskMaster.Test/Ribbon with `CMD-LINECOUNT` and record FEATURE/evidence/qa-gates/file-line-counts.md.
  - Acceptance, all required: the production `LINES` value is greater than `BASE-LINES-PROD:` and at most 500 (476 is expected and recorded, not gated, because CSharpier output wins on layout); the ThrowingSink partial's `LINES` value is at most 500; the main fixture, Race, PrimeFaultOrdering and PrimeRegistration values each equal their `BASE-LINES-*:` value from P0-T4. This is the file-size audit of the General Code Change Policy for the two Write Set source files.
- [ ] [P2-T4] Verify formatting repository-wide, read-only, with `dotnet tool run csharpier check .` (formatter scope set by .csharpierignore) and record FEATURE/evidence/qa-gates/csharpier-check-final.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`
  - Acceptance: `CSHARPIER_EXIT_CODE: 0` is the `EXIT_CODE:` row and the output names no unformatted file. A non-zero exit is a failing step under D-12.
- [ ] [P2-T5] Run the analyzer lint gate with `CMD-REBUILD` (`GATEARGS` analyzers, `TASKID` p2-t5) over TaskMaster.sln and record FEATURE/evidence/qa-gates/msbuild-analyzer-final.md (`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; `CSC_OUT_TASKMASTER:` and `CSC_OUT_TASKMASTER_TEST:` each at least 1; `WRITESET_DIAGNOSTIC_LINES: 0` (neither new empty catch block adds a diagnostic, fact 2); `WARNINGS:` at most `ANALYZER-BASELINE-WARNINGS:` from P0-T10.
- [ ] [P2-T6] Run the nullable type-check gate with `CMD-REBUILD` (`GATEARGS` nullable, `TASKID` p2-t6) over TaskMaster.sln and record FEATURE/evidence/qa-gates/msbuild-nullable-final.md (`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; no Nullable property override).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` at most `NULLABLE-BASELINE-WARNINGS:` from P0-T11; `TEST_DLL_EXISTS: True`.
- [ ] [P2-T7] Re-run the coordinator fixture test run on the rebuilt TaskMaster.Test\bin\Debug\TaskMaster.Test.dll with `CMD-VSTEST` (`ASSEMBLY-TM`, `FILTER-COORD`, `TASKID` p2-t7, `NAMES-947`) and append a `FINAL-FIXTURE-RUN:` section to FEATURE/evidence/regression-testing/throwing-sink-pass-after.md.
  - Acceptance, all required: `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; the `COUNTERS` total equals `BASELINE-TOTAL:` plus 4 with `failed=0`; all twelve `NAMES-947` entries `Passed`; no `FAILED` line. This confirms the fixture on the exact assembly the coverage run measures, because the coverage summary carries counts and failed names only.
- [ ] [P2-T8] Run the coverage-enabled test gate for TaskMaster/Ribbon/EngineToggleStateCoordinator.cs by the route P0-T12 fixed and record FEATURE/evidence/qa-gates/coverage-summary.md: run the `STRAY_TEST_PROCESSES` check, then under `RUNNER` `CMD-COVERAGE-RUNNER` with `STAGE` final, under `DIRECT` `CMD-COVERAGE-DIRECT` with `STAGE` final (the same `EXCLUSION` as P0-T14), then `CMD-COVERAGE-POST` with `STAGE` final and `RAW` per the rule, with the same artifact fields and the same D-6 fallback as P0-T14.
  - Re-run rule (issue 780 only): when the first attempt's `FAILED-SET:` is exactly the single name `TryAddValuesAsync_UpdatesExistingValue`, the loop restarts once at P2-T1 and P2-T1 through P2-T8 are repeated in order as pass 2, each appending a `PASS-2:` section to its own artifact; the first attempt is recorded as `FIRST-ATTEMPT-FAILED-SET:` and `FIRST-ATTEMPT-EXIT-CODE:`, and after pass 2 the top-level `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` fields of this artifact are rewritten with pass 2's values, with no `ExpectedExitCode:` added. No other failure and no second re-run is admitted.
  - Acceptance, all required: branch (a) of P0-T14's branch rule (exit 0, both floors met, `FAILED-SET:` empty) on the recorded attempt; `TRX_PRESENT: True`, and `SEQUENCE_FILES: 0` under DIRECT; the `Output Summary:` holds at most 20 lines and carries the `First-party coverage:` line as the numeric post-change headline; the summary block reports zero failed tests; `COORD-CLASS-NODES: 1`; the three `METHOD` rows are present; `CATCH-ARM-COUNT: 2` with `CATCH-ARM 1 owner=HandleToggleClickAsync` and `CATCH-ARM 2 owner=CompletePrime`; the projection block contains the `TaskMaster` package with both counters. Under `RUNNER` the runner's own 80 percent line and 75 percent branch assertions are the floor gate; under `DIRECT` the `LINE-FLOOR:` and `BRANCH-FLOOR:` lines are. Any other outcome is a failing step: stop (`COVERAGE RUN STALLED` or `COVERAGE RUN ABORTED` for a stall). coverage\final-947.cobertura.xml stays on disk, git-ignored, for P2-T10.
- [ ] [P2-T9] Record the loop closure of the format, lint, type-check and test steps in FEATURE/evidence/qa-gates/toolchain-final-pass.md.
  - Acceptance: the artifact carries `Timestamp:`, `Command:` (the five canonical loop commands in order), `EXIT_CODE: 0` and an `Output Summary:` listing P2-T1 through P2-T8 with each step's canonical command and exit code; it states the pass number (1, or 2 when P2-T8's re-run rule restarted the loop, in which case pass 1 is recorded with its admitted first-attempt failure set and pass 2 is recorded as the clean pass); for P2-T1 that `REWRITTEN-WRITESET-FILES: 0` and the two scoped porcelain sets were identical; for P2-T4 that the check named no file; for P2-T5 and P2-T6 that `SKIP_CORECOMPILE_LINES: 0` and both `CSC_OUT_` counts were at least 1; for P2-T8 the `COVERAGE-ROUTE:`, the `STALL-PROBE:` value from P0-T12 that selected it, and that the run exited 0. The last pass recorded is clean with no rewrite; otherwise this task stays unchecked.
- [ ] [P2-T10] Compute the coverage comparison for `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` from FEATURE/evidence/baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/coverage-summary.md, run `CMD-CHANGED-LINES`, and append a `COMPARISON:` section to FEATURE/evidence/qa-gates/coverage-summary.md.
  - Sources: the `METHOD` rows, the `First-party coverage:` lines, the `ROOT` lines and `CATCH-ARM-COUNT:` are read from each artifact's `Output Summary:`; the `COORD-LINES`, `COORD-BRANCHES`, `METHOD-LINE`, `CATCH-ARM ` and `CATCH-ARM-LINE` rows from each artifact's `Details:` section.
  - Rows, all recorded: `COORD-LINES-BASELINE:` and `COORD-LINES-FINAL:`; `COORD-BRANCHES-BASELINE:` and `COORD-BRANCHES-FINAL:`; for each of `HandleToggleClickAsync`, `StartObservedPrime` and `CompletePrime`, `METHOD-BASELINE:` and `METHOD-FINAL:` (the two `METHOD` rows verbatim); `CATCH-ARM-COUNT-BASELINE:` and `CATCH-ARM-COUNT-FINAL:` with every final `CATCH-ARM ` row verbatim; the `CMD-CHANGED-LINES` output verbatim; `FIRST-PARTY-BASELINE:` and `FIRST-PARTY-FINAL:`; `ROOT-BASELINE:` and `ROOT-FINAL:`; `DENOMINATOR-BRANCH:` per D-8.
  - Acceptance, all required: `COORD-CLASS-NODES: 1` in the `CMD-CHANGED-LINES` output; `CATCH-ARM-COUNT-BASELINE:` 0 (the negative control) and `CATCH-ARM-COUNT-FINAL:` 2 with `CATCH-ARM 1 owner=HandleToggleClickAsync` and `CATCH-ARM 2 owner=CompletePrime`, each with `CATCH-ARM-ELEMENTS:` at least 1 and `CATCH-ARM-UNCOVERED: 0` (an arm with `CATCH-ARM-ELEMENTS: 0` is `CATCH ARM UNMEASURED`: stop, never pass); `CHANGED-LINES-UNCOVERED: 0`; `CHANGED-LINES-WITH-ELEMENT:` at least 4; `METHOD CompletePrime` final `rate=` at least 90.00 and final `elements=` greater than its baseline `elements=`; `METHOD HandleToggleClickAsync` final `elements=` greater than its baseline and final `uncovered=` at most its baseline `uncovered=`; `METHOD StartObservedPrime` final `elements=` equal to its baseline and final `uncovered=` at most its baseline; exactly one `DENOMINATOR-BRANCH:` value, and under `COMPARABLE` its rate clause holds. The figures are computed as D-7 states, so a third party re-running `CMD-COVERAGE-POST` on the two retained documents obtains the same numbers.
- [ ] [P2-T11] Verify that TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs is byte-identical to BASE-SHA, that no tracked file under TaskMaster.Test/Ribbon changed, and that the only new file there is the partial, and record FEATURE/evidence/qa-gates/prime-fault-ordering-identity.md.
  - Command: `git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` (the `EXIT_CODE:` row); `CMD-HASH`; `git diff --quiet BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (positive control); `git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon`; `git status --porcelain --untracked-files=all -- TaskMaster.Test/Ribbon`.
  - Acceptance, all required: the PrimeFaultOrdering diff exits 0; its `CMD-HASH` value equals `BASE-HASH-PFO:`; the positive-control diff exits 1, recorded as `POSITIVE-CONTROL-DIFF-EXIT: 1` (the comparison detects a change where one exists); the TaskMaster.Test/Ribbon diff exits 0; the porcelain span prints exactly one line, `?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` (c4: the untracked partial is invisible to the anchored diff, so the porcelain span is the gate that sees it).
- [ ] [P2-T12] Verify the determinism and library constraints of the new partial over the content of TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs itself with `CMD-TOKEN-COUNT` and record FEATURE/evidence/qa-gates/determinism-tokens.md.
  - Command: `CMD-TOKEN-COUNT` with `FILE` `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs` and `TOKEN` `"Thread.Sleep", "Task.Delay", "GetTempPath", "GetTempFileName", "File.", "Directory.", "DoNotParallelize", "using Microsoft.VisualStudio.TestTools.UnitTesting;", "using Moq;", "using FluentAssertions;"`.
  - Acceptance, all required: `Thread.Sleep`, `Task.Delay`, `GetTempPath`, `GetTempFileName`, `File.`, `Directory.` and `DoNotParallelize` each 0; `using Microsoft.VisualStudio.TestTools.UnitTesting;`, `using Moq;` and `using FluentAssertions;` each exactly 1 (the three positive tokens show the scan read the file; c4: the partial is untracked, so no anchored diff form can see its content). `EXIT_CODE: 0`.
- [ ] [P2-T13] Verify the change footprint against BASE-SHA, including untracked files and raw documents under docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947, and record FEATURE/evidence/qa-gates/footprint-scope.md.
  - Command: `git diff --name-status BASE-SHA` together with `git status --porcelain --untracked-files=all`; then `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $ext = @(".trx", ".xml", ".coverage", ".coveragexml", ".log", ".binlog"); $lines = @(git status --porcelain --untracked-files=all --ignored -- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947); $paths = @($lines | ForEach-Object { $_.Substring(3) }); "FEATURE-STATUS-LINES: $($lines.Count)"; "RAW-DOCS-IN-FEATURE: $(@($paths | Where-Object { $ext -contains [IO.Path]::GetExtension($_).ToLowerInvariant() }).Count)"; "NON-MD-IN-FEATURE: $(@($paths | Where-Object { [IO.Path]::GetExtension($_).ToLowerInvariant() -ne ".md" }).Count)"'` (`--ignored` is required because .gitignore lines 146 and 147 ignore trx and cobertura names repository-wide).
  - Acceptance, all required: `DIFF-FOOTPRINT:` lists every name-status line, and every listed path is `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `TaskMaster.Test/TaskMaster.Test.csproj`, a path under the feature folder, or a path under .claude/agent-memory/ (recorded under `FOREIGN-MEMORY-PATHS:` as another agent's session memory, never staged or attributed to this plan); both code paths are present with status `M`; `PORCELAIN-FOOTPRINT:` lists every porcelain line verbatim, exactly one `??` line lies under TaskMaster.Test/Ribbon and names the partial, no `??` line lies under TaskMaster/, and every other line is one of the two code paths, under the feature folder, under .claude/agent-memory/, or a member of `PRE-EXISTING-WORKTREE-PATHS:` from P0-T2; no path under scripts/, config/, artifacts/, docs/features/potential/ or any other .claude/ subtree appears in either list; `FEATURE-STATUS-LINES:` at least 34 (the untracked evidence files are visible to the span, the positive control); `RAW-DOCS-IN-FEATURE: 0` and `NON-MD-IN-FEATURE: 0` (no trx, cobertura, coveragexml or msbuild log under the feature folder). A path outside these sets is `FOOTPRINT OUTSIDE WRITE SET`: recorded by path.
- [ ] [P2-T14] Sweep the feature folder docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947, including this plan, for host identifiers and record FEATURE/evidence/qa-gates/evidence-hygiene.md (this task creates the file; P2-T23 appends to it).
  - Command (`HYGIENE-SWEEP`): `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; $acct = Split-Path -Leaf $env:USERPROFILE; $machine = $env:COMPUTERNAME; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947" -Recurse -File -Filter "*.md"); $a = 0; $m = 0; $d = 0; foreach ($f in $files) { $c = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8; $a += ([regex]::Matches($c, [regex]::Escape($acct), "IgnoreCase")).Count; $m += ([regex]::Matches($c, [regex]::Escape($machine), "IgnoreCase")).Count; $n = $c.Replace([string][char]92, "/"); $d += ([regex]::Matches($n, "[a-z]:/+users/+[a-z0-9_.~-]", "IgnoreCase")).Count }; "FILES_SCANNED=$($files.Count) ACCOUNT_HITS=$a MACHINE_HITS=$m DRIVE_USERS_HITS=$d"'`
  - Acceptance: `ACCOUNT_HITS=0`, `MACHINE_HITS=0`, `DRIVE_USERS_HITS=0` and `FILES_SCANNED=` at least 38 (issue.md, the research record, this plan, and the 35 artifacts written by P0-T1 through P2-T13: fifteen under baseline, six under regression-testing and fourteen under qa-gates). The two host tokens are derived at run time and neither value is written into the artifact. The drive-path check normalises backslashes and applies the user-profile pattern of scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 (line 21) case-insensitively. A non-zero count is repaired by replacing the occurrence with REDACTED-PATH and re-running this task.
- [ ] [P2-T15] Check off AC1 (the first checkbox line between `## Acceptance Criteria` and `## Logs / Screenshots`) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing FEATURE/evidence/regression-testing/throwing-sink-pass-after.md and the `POST-FORMAT:` section of FEATURE/evidence/qa-gates/production-edit-scope.md.
  - Evidence required: `RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed` and `RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed` in the P1-T12 run and in the `FINAL-FIXTURE-RUN:` section (each asserts `Times.Exactly(2),` on the activation read); in the `POST-FORMAT:` section the `CompletePrime` span shows the `SPAN-LINE` of `_primeTasks.TryRemove(engineName, out _);` greater than that of `Intentionally discarded: see the remarks on this method.` (the clear runs after the guard, whether or not the sink threw).
  - Acceptance: either exactly one line of the file changes, the first checkbox line of the section, from `- [ ] ` to `- [x] `, because the evidence holds, or the box stays unchecked; the criterion text is unmodified; the `ISSUE-SHAPE` payload of P0-T2 then reports `AC_OPEN=6 AC_DONE=1` (flipped) or `AC_OPEN=7 AC_DONE=0` (not flipped). This task creates FEATURE/evidence/other/ac-status-summary.md with a `Timestamp:` line and appends exactly one line: `AC1: MET` when the box flipped, or `AC1: NOT MET` followed by the failing values. This task completes in either case.
- [ ] [P2-T16] Check off AC2 (the second checkbox line of the section) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing FEATURE/evidence/qa-gates/prime-fault-ordering-identity.md, the `POST-FORMAT:` section of FEATURE/evidence/qa-gates/production-edit-scope.md and FEATURE/evidence/regression-testing/throwing-sink-pass-after.md.
  - Evidence required: the PrimeFaultOrdering diff exit 0 with its hash equal to `BASE-HASH-PFO:` and `POSITIVE-CONTROL-DIFF-EXIT: 1`; the `CompletePrime` span order `_logError(BuildPrimeFailedMessage(engineName), failure);` < `_primeTasks.TryRemove(engineName, out _);` in the `POST-FORMAT:` section; `RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed` in both pass-after runs.
  - Acceptance: exactly one checkbox (the second) flips and `AC2: MET` is appended to FEATURE/evidence/other/ac-status-summary.md, or the box stays unchecked and `AC2: NOT MET` followed by the failing values is appended there; the criterion text is unmodified; the `ISSUE-SHAPE` payload's `AC_DONE` equals the number of `: MET` lines in that file.
- [ ] [P2-T17] Check off AC3 (the third checkbox line of the section) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing FEATURE/evidence/regression-testing/throwing-sink-pass-after.md, the `POST-FORMAT:` section of FEATURE/evidence/regression-testing/throwing-sink-partial-tokens.md and the `POST-FORMAT:` section of FEATURE/evidence/qa-gates/production-edit-scope.md.
  - Evidence required: `RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed` in both pass-after runs; the `.Be(TaskStatus.RanToCompletion,` row at 1 in the partial's `POST-FORMAT:` section (the deterministic `RanToCompletion` assertion on the handle `GetPrimeTask` returned for the first prime); the `CompletePrime` span with `SPAN-CATCH` 1 in the production `POST-FORMAT:` section (the sink exception is contained inside `CompletePrime`).
  - Acceptance: exactly one checkbox (the third) flips and `AC3: MET` is appended to FEATURE/evidence/other/ac-status-summary.md, or the box stays unchecked and `AC3: NOT MET` followed by the failing values is appended there; the criterion text is unmodified; the `ISSUE-SHAPE` payload's `AC_DONE` equals the number of `: MET` lines in that file.
- [ ] [P2-T18] Check off AC4 (the fourth checkbox line of the section) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing FEATURE/evidence/regression-testing/throwing-sink-fail-before.md and FEATURE/evidence/regression-testing/throwing-sink-pass-after.md.
  - Evidence required: the fail-before artifact carries `Timestamp:`, `Command:`, a non-zero `EXIT_CODE:` with a matching `ExpectedExitCode:`, `SEQUENCE_FILES: 0`, `PROD-HASH-AT-CONTROL:` equal to `BASE-HASH-PROD:`, and the `FAILED` and `MESSAGE` lines of the three prime-site tests with the reason strings P1-T6 requires; the same three tests read `Passed` in both pass-after runs.
  - Acceptance: exactly one checkbox (the fourth) flips and `AC4: MET` is appended to FEATURE/evidence/other/ac-status-summary.md, or the box stays unchecked and `AC4: NOT MET` followed by the failing values is appended there; the criterion text is unmodified; the `ISSUE-SHAPE` payload's `AC_DONE` equals the number of `: MET` lines in that file.
- [ ] [P2-T19] Check off AC5 (the fifth checkbox line of the section) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing the `POST-FORMAT:` section of FEATURE/evidence/regression-testing/throwing-sink-partial-tokens.md, FEATURE/evidence/qa-gates/determinism-tokens.md, FEATURE/evidence/qa-gates/toolchain-final-pass.md and the `COMPARISON:` section of FEATURE/evidence/qa-gates/coverage-summary.md.
  - Evidence required: `[TestMethod]` 4, `[TestClass]` 0, `new Mock<` 0 and `Thread.Sleep` and `Task.Delay` 0 in the partial's `POST-FORMAT:` section; the three `using` rows at 1 and every prohibited token at 0 in determinism-tokens.md; one clean final pass with all five loop steps at exit 0 in toolchain-final-pass.md; `CHANGED-LINES-UNCOVERED: 0` and both `CATCH-ARM-UNCOVERED: 0` rows in the `COMPARISON:` section.
  - Acceptance: exactly one checkbox (the fifth) flips and `AC5: MET` is appended to FEATURE/evidence/other/ac-status-summary.md, or the box stays unchecked and `AC5: NOT MET` followed by the failing values is appended there; the criterion text is unmodified; the `ISSUE-SHAPE` payload's `AC_DONE` equals the number of `: MET` lines in that file.
- [ ] [P2-T20] Check off AC6 (the sixth checkbox line of the section) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing FEATURE/evidence/regression-testing/throwing-sink-pass-after.md, the `POST-FORMAT:` section of FEATURE/evidence/qa-gates/production-edit-scope.md and the `COMPARISON:` section of FEATURE/evidence/qa-gates/coverage-summary.md.
  - Evidence required: `RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed` and `RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed` in both pass-after runs (the new test asserts the awaited call does not throw, one attempted report carrying the toggle fault unchanged, and no invalidation); the `HandleToggleClickAsync` span results of the production `POST-FORMAT:` section (`SPAN-TRY` 2, `SPAN-CATCH` 2 and the guarded-sink-call order); `CATCH-ARM 1 owner=HandleToggleClickAsync` with `CATCH-ARM-UNCOVERED: 0` in the `COMPARISON:` section.
  - Acceptance: exactly one checkbox (the sixth) flips and `AC6: MET` is appended to FEATURE/evidence/other/ac-status-summary.md, or the box stays unchecked and `AC6: NOT MET` followed by the failing values is appended there; the criterion text is unmodified; the `ISSUE-SHAPE` payload's `AC_DONE` equals the number of `: MET` lines in that file.
- [ ] [P2-T21] Check off AC7 (the seventh checkbox line of the section) in `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, citing FEATURE/evidence/regression-testing/throwing-sink-fail-before.md and FEATURE/evidence/regression-testing/throwing-sink-pass-after.md.
  - Evidence required: the fail-before artifact's `FAILED HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` line and its `MESSAGE` line containing `the click boundary contains a failure of the sink itself` and `sink failed`, with `PROD-HASH-AT-CONTROL:` equal to `BASE-HASH-PROD:` and the non-zero `EXIT_CODE:` with matching `ExpectedExitCode:`; the same test `Passed` in both pass-after runs.
  - Acceptance: exactly one checkbox (the seventh) flips and `AC7: MET` is appended to FEATURE/evidence/other/ac-status-summary.md, or the box stays unchecked and `AC7: NOT MET` followed by the failing values is appended there; the criterion text is unmodified; the `ISSUE-SHAPE` payload's `AC_DONE` equals the number of `: MET` lines in that file.
- [ ] [P2-T22] Complete the acceptance-criteria status summary for `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md` in FEATURE/evidence/other/ac-status-summary.md, in the acceptance-criteria-tracking format.
  - Required contents, appended below the seven per-criterion lines: a `### Acceptance Criteria Status` heading followed by `- Source: docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, `- Total AC items: 7`, `- Checked off (delivered): M`, `- Remaining (unchecked): R` and `- Items remaining:` with the text of every unchecked criterion or `none`, where M and R are read from a fresh run of the `ISSUE-SHAPE` payload (`AC_DONE` and `AC_OPEN`), not summed from this plan's claims.
  - Acceptance: the file holds exactly seven lines beginning `AC1:` to `AC7:`, one per criterion; M plus R equals 7; M equals the number of those lines reading `MET` with no `NOT`; the file carries `Timestamp:`.
- [ ] [P2-T23] Record the reduced-audit handoff in FEATURE/evidence/other/reduced-audit-handoff.md, then re-run the P2-T14 `HYGIENE-SWEEP` command unchanged and append its counts to FEATURE/evidence/qa-gates/evidence-hygiene.md under `POST-HANDOFF-HYGIENE:`.
  - Handoff contents: the reduced-audit artifact list (pointers to FEATURE/evidence/baseline/phase0-instructions-read.md, FEATURE/evidence/baseline/coverage-baseline.md, FEATURE/evidence/regression-testing/throwing-sink-fail-before.md, FEATURE/evidence/regression-testing/throwing-sink-pass-after.md, FEATURE/evidence/qa-gates/toolchain-final-pass.md, FEATURE/evidence/qa-gates/coverage-summary.md, FEATURE/evidence/qa-gates/footprint-scope.md and FEATURE/evidence/other/ac-status-summary.md); the `### Acceptance Criteria Status` block copied from the summary; the `DIFF-FOOTPRINT:` and `PORCELAIN-FOOTPRINT:` code paths from P2-T13; `BASE-SHA:` and `COVERAGE-ROUTE:`; the statement that research section 7 item 1 (the click-boundary sink call) was brought into scope by the 2026-10-01 consolidation and is fixed by E5 and E6, so no promotion is requested by this plan, and that research section 7 item 2 is the open #944 follow-up on log volume, which this plan neither changes nor re-files; the statement that the committed test evidence is projections and summaries only; and `HANDOFF-HEAD:` (the output of `git rev-parse HEAD` at write time, expected equal to BASE-SHA because this plan commits nothing) as an observation.
  - Acceptance: every listed pointer resolves to an existing artifact; the handoff carries `Timestamp:`; the appended hygiene counts read `ACCOUNT_HITS=0`, `MACHINE_HITS=0`, `DRIVE_USERS_HITS=0` and `FILES_SCANNED=` at least 41 (the P2-T14 set plus evidence-hygiene.md, ac-status-summary.md and reduced-audit-handoff.md). A non-zero hit count is repaired by replacing the occurrence with REDACTED-PATH and re-running the sweep.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | HandleToggleClickAsync summary 153-156, remarks 162-169, catch (Exception ex) 182 with the unguarded _logError at 184 and closing brace 185; GetPrimeTask returns 243-249; StartObservedPrime remarks 294-302 and try/finally 314-321; CompletePrime 351-382 with _logError at 380 before TryRemove at 381; 442 lines
CITATION: TaskMaster/Ribbon/RibbonCommandBoundary.cs | SafeLog 96-113, catch (System.Exception) 109, comment 111
CITATION: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs | Harness 403-452, Errors.Add 417 before OnLogError?.Invoke 418, strict mock 424, OnLogError 445, Notifications 449; HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate 334; 470 lines
CITATION: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs | GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged 27-73; 77 lines
CITATION: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs | tests at 32, 85, 132; 175 lines
CITATION: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs | GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker 204; 277 lines
CITATION: TaskMaster.Test/TaskMaster.Test.csproj | LangVersion 18; coordinator Compile entries 352, 359, 360, 361; no ThrowingSink entry
CITATION: TaskMaster.Test/packages.config | FluentAssertions 8.11.0 line 7, Moq 4.21.0 line 41, MSTest.TestFramework 4.4.1 line 44
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | filter 91, dot-sources 303/309/313, discovery exclusion 353, No test assemblies found 358, entry guard 459
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | dot-sources 2-6, Get-CoberturaClassLineSummary 160, ConvertTo-KoverageCoberturaXml 407
CITATION: scripts/vscode/Invoke-MSTest.TrxSummary.ps1 | Get-TrxRunSummary 12, Format-TrxRunSummary 103
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | Version 8.0.205 at 3, sdk marker path 56
CITATION: scripts/vscode/TaskMaster.cli.runsettings | Workers 0 at 5, Scope ClassLevel at 6
CITATION: scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | user-profile pattern at 21
CITATION: .claude/hooks/validate-planner-output.ps1 | path pattern 95, phase-heading pattern 238, final-phase QA vocabulary 339
CITATION: docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md | Work Mode 12, Scope Consolidation 39, Acceptance Criteria 45 with seven checkboxes 47-53, Logs / Screenshots 55
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7
AC-MAPPING: AC1 | IMPLEMENTATION: P1-T7 edit E4 (guarded sink call in CompletePrime, TryRemove after the guard) | TESTS: GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime and GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime | EVIDENCE: throwing-sink-pass-after.md and production-edit-scope.md POST-FORMAT, checked off by P2-T15
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T7 edit E4 keeps _logError before TryRemove; PrimeFaultOrdering partial untouched | TESTS: GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged | EVIDENCE: prime-fault-ordering-identity.md, production-edit-scope.md POST-FORMAT and throwing-sink-pass-after.md, checked off by P2-T16
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T7 edits E3 and E4 (sink exception contained in CompletePrime) | TESTS: GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared | EVIDENCE: throwing-sink-pass-after.md, throwing-sink-partial-tokens.md POST-FORMAT and production-edit-scope.md POST-FORMAT, checked off by P2-T17
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T1 partial with the three prime-site tests, run before P1-T7 | TESTS: the three GetPressed_WhenLogSinkThrows tests | EVIDENCE: throwing-sink-fail-before.md (P1-T6) and throwing-sink-pass-after.md, checked off by P2-T18
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T1 partial (MSTest, Moq, FluentAssertions, no sleep, delay or temporary file) and the Phase 2 loop | TESTS: all four new tests and the full coverage run of P2-T8 | EVIDENCE: throwing-sink-partial-tokens.md POST-FORMAT, determinism-tokens.md, toolchain-final-pass.md and coverage-summary.md COMPARISON, checked off by P2-T19
AC-MAPPING: AC6 | IMPLEMENTATION: P1-T7 edits E1, E5 and E6 (guarded sink call inside the click-boundary catch) | TESTS: HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport and HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate | EVIDENCE: throwing-sink-pass-after.md, production-edit-scope.md POST-FORMAT and coverage-summary.md COMPARISON CATCH-ARM 1, checked off by P2-T20
AC-MAPPING: AC7 | IMPLEMENTATION: P1-T1 partial with the click-boundary test, run before P1-T7 | TESTS: HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport | EVIDENCE: throwing-sink-fail-before.md (P1-T6) and throwing-sink-pass-after.md, checked off by P2-T21
UNRESOLVED-GAPS: NONE
PREFLIGHT: AWAITING ATOMIC-EXECUTOR

The line above is a status marker, not an executor signal: the planner cannot issue a preflight result for its own plan. The clearance signal is the atomic-executor's return under DIRECTIVE: PREFLIGHT VALIDATION ONLY.

## Planner Adversarial Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

Pass B, 2026-10-01, in the assigned worktree. Every entry below was read directly from the current tree in this pass, with its sibling region re-read; none is carried forward from pass A.

- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs — 442 lines; `_logError` field 56; `_primeTasks` summary 72-77 and declaration 78-81; HandleToggleClickAsync summary 153-156 (text 154-155), returns 158-161, remarks 162-169 (last two text lines 167-168), method 170-186 (refusal 172-176, outer try 178, await 180, `catch (Exception ex)` 182, brace 183, unguarded sink call 184, clause close 185, method close 186); GetPrimeTask returns 243-249 (246-248 end `can rely on the fault having been reported.`), signature 250; StartPrimeIfNeeded 260-289 (lock 272, ContainsKey 274); StartObservedPrime summary 291-293, remarks 294-302 (`exactly one <c>catch</c>` on 296, `<c>finally</c>` on 300), signature 303-308, try 314, CompletePrime call 316, finally 318, SetResult 320; ApplyPrimeAsync 329-349; CompletePrime 351-382 (Report-then-clear comment 377-379, sink call 380, TryRemove 381); RenderEngineName summary 385. Sibling checks: one `catch (`, one `lock (`, zero `catch (Exception)`; the six windows E1 154-155, E2 246-248, E3 295-301, E4 351-382, E5 167-168, E6 182-185 do not overlap, and the E2 and E3 replacement texts share leading lines with the base, which leaves every hunk's base range inside its window.
- TaskMaster/Ribbon/RibbonCommandBoundary.cs — SafeLog 96-113, remarks 99-102, `catch (System.Exception)` 109, comment 111.
- .editorconfig — `dotnet_analyzer_diagnostic.severity = suggestion` at 27; RCS1075 at 365.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs — 470 lines; namespace TaskMaster.Test.Ribbon at 9; `[TestClass]` 22, partial class 23; constants 25-26; tests 160, 213, 334 (ThrowsAsync 339, act 342, NotThrowAsync 346), 355, 375; region 331; Harness 403-452 with Errors.Add 417 before the OnLogError invoke 418, strict mock 424, OnLogError 445, Invalidations 447, Notifications 449, Errors 451; LoggedError 457-468.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs — 77 lines; test 26-73 (attribute 26, name 27), GetPrimeTask capture 35, hook 37-38.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs — 175 lines; tests 32, 85, 132.
- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs — 277 lines; six tests (38, 81, 122, 158, 204, 253), the canceled-prime test at 204. The four partials carry 25 `[TestMethod]` attributes (15, 6, 1, 3).
- TaskMaster.Test (all .cs files) — no occurrence of `ThrowingSink` or `WhenLogSinkThrows`.
- TaskMaster.Test/TaskMaster.Test.csproj — LangVersion latest at 18; coordinator entries 352, 359, 360, 361.
- TaskMaster.Test/packages.config — FluentAssertions 8.11.0 (7), Moq 4.21.0 (41), MSTest.TestFramework 4.4.1 (44).
- scripts/vscode/Invoke-MSTestWithCoverage.ps1 — Get-DotnetCoverageArgumentList 41, filter 91, ConvertTo-DerivedCoverageSettingsXml 97, dot-sources 303, 309, 313, discovery exclusion 353, throw 358, entry guard 459.
- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 — dot-sources ClosureFilter, PackageRate, Threshold, FirstParty and Projection at 2-6; Get-CoberturaClassLineSummary 160; ConvertTo-KoverageCoberturaXml 407.
- scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 3 and 58; Invoke-MSTestWithCoverage.FirstParty.ps1 120 and 123; Invoke-MSTestWithCoverage.Projection.ps1 14, 83, 148; Invoke-MSTest.TrxSummary.ps1 12 and 103.
- scripts/vscode/Install-RepoDotNetSdk.ps1 — default version 8.0.205 at 3, marker path 56; global.json — version 8.0.205, paths `.dotnet-sdk`.
- scripts/vscode/Invoke-Restore.ps1 — `-SolutionPath` default `TaskMaster.sln` at 3.
- scripts/vscode/TaskMaster.cli.runsettings — Workers 0 at 5, Scope ClassLevel at 6, no logger or timeout element.
- scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 — user-profile pattern at 21.
- .gitignore — `*.trx` 146, `*cobertura*.xml` 147, `coverage/*` 150, `!coverage/.gitkeep` 151.
- .csharpierignore — evidence tree 4, raw documents 5-8, project files 12-14, packages.config 16, app.config 18.
- .claude/hooks/validate-planner-output.ps1 — path pattern 95, phase-heading pattern 238 (em dash), task pattern 239, final-phase QA vocabulary 339; planner internal review validation 113-227 (read to place the record).
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md — Work Mode 12, Scope Consolidation 39, Acceptance Criteria 45, seven checkboxes 47-53, Logs / Screenshots 55; no spec.md or user-story.md in the folder.
- docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/research/2026-10-01T06-47-engine-toggle-throwing-log-sink-research.md — section 7 items 1 (click-boundary sink call) and 2 (log volume), section 9 addendum.
- docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md — the command shapes adapted here: P0-T9 to P0-T13 bootstrap and baseline commands, P1-T2 csproj check, P3-T8 re-run rule, P3-T12 raw-document check and P3-T13 hygiene sweep.
- Evidence counts re-derived for P2-T13, P2-T14 and P2-T23 from the Write Set: fifteen baseline, six regression-testing, fifteen qa-gates and two other artifacts. 34 untracked artifacts exist when P2-T13 runs, 35 when P2-T14 runs (so 38 Markdown files with issue.md, the research record and this plan), and 41 after P2-T23.
