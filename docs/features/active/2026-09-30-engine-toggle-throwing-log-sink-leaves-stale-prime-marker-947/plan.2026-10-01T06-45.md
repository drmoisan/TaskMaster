# 2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker (Plan)

INCOMPLETE: stopped for quota (second pass, 2026-10-01; stopped by coordinator directive after the self-review and before the phase sections were written). Written so far: header, requirement sources, Write Set, AC identity table, verified tree facts, design decisions D-1 to D-12, the full Delivered Source (production edits E1 to E4, the new ThrowingSink partial, the csproj entry), execution conventions and the complete command reference. Not yet written: the three phase sections (`### Phase 0 — Baseline Capture`, `### Phase 1 — Regression Tests First and the Minimal Sink Guard`, `### Phase 2 — Final QC Loop`) and the handoff records. This plan has not passed the validator and must not be executed in this state.
SECOND-PASS SELF-REVIEW RESULTS (every citation in facts 1 to 11, D-1 to D-12 and the Delivered Source was re-derived against the current tree on 2026-10-01; all hold except the four corrections below, which the next pass must apply to the sections they name): (c1) fact 1: the substring `finally` occurs twice in the production file (`<c>finally</c>` at 300 and the keyword at 318); only the trimmed-line keyword count is 1, which is what CMD-SPANS measures. (c2) Delivered Source size sentence: E4 replaces 32 base lines (351 to 382) with 53 delivered lines, so E4 adds 21, not 22; the expected post-change production size is 442 + 24 = 466 lines (E1 0, E2 1, E3 2, E4 21), not 467. (c3) The new partial as delivered is 183 lines, not 166, and its line `secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");` is 102 characters, so CSharpier will wrap it: gates must use the single-line tokens `.NotBeSameAs(` and `the later read registered a new prime`, never that whole statement, and the partial's line count is gated only as at most 500. (c4) Because this plan commits and stages nothing, the new partial is untracked and invisible to every `git diff BASE-SHA` form; the determinism-token gate must scan the partial's own content with CMD-TOKEN-COUNT (every line of a new file is an added line), the footprint gate must pair `git diff --name-status BASE-SHA` with `git status --porcelain --untracked-files=all` and require exactly one `??` line under TaskMaster.Test/Ribbon naming the partial, and the unchanged-sibling gate is `git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon` exit 0 beside `git status --porcelain -- TaskMaster.Test/Ribbon` printing only that `??` line. Additional verified facts for the phase author: Helpers.ps1 dot-sources ClosureFilter, PackageRate, Threshold, FirstParty and Projection at its lines 2 to 6, so CMD-COVERAGE-POST resolves every function it calls; hook line 95 is the separator-bearing path pattern applied to each task's opening line, 238 the phase-heading pattern, 339 the final-phase QA-vocabulary check; CMD-WINDOWS on the base text yields E1 154-155, E2 246-248, E3 295-301, E4 351-382 (record, do not gate the numbers; gate start and end non-zero, `WINDOW-E1-LINE2-IS-CATCH-CLAUSE: True`, `WINDOW-E4-STARTS-AT-SUMMARY: True`, E2 end = start + 2, E3 end = start + 6, E4 end minus start = 31, `HUNK-COUNT: 0` at base); after E1 to E4 the anchored -U0 diff is expected to produce hunks only inside the four windows (`HUNKS-OUTSIDE-WINDOWS: 0`, `WINDOWS-TOUCHED: E1,E2,E3,E4`); expected post-fix file-level counts: `catch (` 2, `lock (` 1, `catch (Exception)` 1, `exactly one <c>catch</c>` 0, `the only place in this type that observes a fault with a` 0, `can rely on the fault having been reported.` 0, `sink guard inside` 2, `returned or thrown` 2, `no remaining throw source of its own` 2, every other documented token 1; CompletePrime span post-fix: SPAN-TRY 1, SPAN-CATCH 1, SPAN-FINALLY 0, SPAN-LOCK 0 with SPAN-LINE order Report-then-clear < try < `_logError(BuildPrimeFailedMessage(engineName), failure);` < `catch (Exception)` < `Intentionally discarded: see the remarks on this method.` < `_primeTasks.TryRemove(engineName, out _);`; HandleToggleClickAsync and StartObservedPrime spans are unchanged by the edits (E1 and E3 sit above their signatures). Fail-before gates: the two LaterReadStartsNewPrime tests fail on the Moq verification, transcribed MESSAGE containing `a throwing sink leaves no marker behind, so the later read starts a new prime` and matching the case-insensitive pattern `exactly 2 times, but was 1 time`; the third test fails on its final assertion, MESSAGE containing `the sink exception is contained, so the marker is still cleared`; `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` is `Passed` in the fail-before run; COUNTERS total equals BASELINE-TOTAL plus 3. Coverage gates for P2-T10: `CATCH-ARM-ELEMENTS` at least 1 with `CATCH-ARM-UNCOVERED: 0` (otherwise `CATCH ARM UNMEASURED`: stop, never pass), `CHANGED-LINES-UNCOVERED: 0`, `CHANGED-LINES-WITH-ELEMENT:` at least 3, `METHOD CompletePrime` final rate at least 90.00 and final elements greater than baseline, `METHOD HandleToggleClickAsync` and `METHOD StartObservedPrime` final elements equal to baseline with uncovered at most baseline, plus the D-8 root comparison. Decomposition fixed for the next pass: P0-T1 policy reads (phase0-instructions-read.md); P0-T2 BASE-SHA = `git rev-parse HEAD`, branch name equals `bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947` else `BRANCH MISMATCH`, `git diff --exit-code BASE-SHA -- TaskMaster TaskMaster.Test` exit 0, whole-tree porcelain with no line under TaskMaster/ or TaskMaster.Test/, issue.md checks (Work Mode marker count 1, exactly 5 `- [ ] ` and 0 `- [x] ` lines between the two headings, spec.md and user-story.md absent) (scope-and-anchor.md); P0-T3 CMD-TOKEN-COUNT, CMD-SPANS, CMD-WINDOWS on the base production file (anchor-production-shape.md); P0-T4 CMD-LINECOUNT, csproj entry tokens with `PR-ENTRY-LINE:`, fixture and PrimeFaultOrdering tokens, new test names and `ThrowingSink` absent under TaskMaster.Test (anchor-test-side.md); P0-T5 SDK; P0-T6 tool restore; P0-T7 NuGet restore with ANALYZER_MISSING check; P0-T8 dotnet-coverage; P0-T9 csharpier check baseline (exit 0 gate); P0-T10 analyzer CMD-REBUILD; P0-T11 nullable CMD-REBUILD; P0-T12 stall probe (CMD-VSTEST ASSEMBLY-UCS FILTER-STALL NAMES-NONE); P0-T13 fixture baseline (CMD-VSTEST ASSEMBLY-TM FILTER-COORD NAMES-947, BASELINE-TOTAL, no RESULT line for the three new names); P0-T14 coverage baseline by the D-6 route with CMD-COVERAGE-POST (expected `CATCH-ARM span=0-0 CATCH-ARM-ELEMENTS: 0`); P0-T15 CMD-HASH and CMD-LINECOUNT (`BASE-HASH-PROD:`, `BASE-HASH-PFO:`, ThrowingSink ABSENT) and Phase 0 completeness listing appended to scope-and-anchor.md. P1-T1 write the partial; P1-T2 scoped `dotnet tool run csharpier format TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs` with CMD-HASH before and after (throwing-sink-partial-format.md); P1-T3 CMD-TOKEN-COUNT on the partial (throwing-sink-partial-tokens.md; `[TestMethod]` 3, `[TestClass]` 0, `new Mock<` 0, `MockBehavior` 0, `private sealed class` 0, `var harness = new Harness();` 3, `// Arrange`, `// Act`, `// Assert` 3 each, `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` 2, `Times.Exactly(2),` 2, `.NotBeSameAs(` 2, `.Be(TaskStatus.RanToCompletion,` 1, `Thread.Sleep` 0, `Task.Delay` 0, the three method lines 1 each, the thirteen quoted reason tokens at their counts); P1-T4 csproj entry after `PR-ENTRY-LINE:` with the 944 P1-T2 command shape and anchored numstat 1 insertion 0 deletions (csproj-registration.md); P1-T5 CMD-BUILD (build-before-fix.md); P1-T6 [expect-fail] CMD-VSTEST with `PROD-HASH-AT-CONTROL:` equal to `BASE-HASH-PROD:` and `git diff --exit-code BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` exit 0 (throwing-sink-fail-before.md); P1-T7 apply E1 to E4; P1-T8 scoped format of the production file with hashes (production-format.md); P1-T9 CMD-TOKEN-COUNT and CMD-SPANS on the fixed file (production-edit-scope.md); P1-T10 CMD-WINDOWS hunk confinement plus `git diff --exit-code BASE-SHA --` over the protected files and runsettings (protected-regions-unchanged.md); P1-T11 CMD-BUILD (build-after-fix.md); P1-T12 pass-after CMD-VSTEST, all ten NAMES-947 Passed, total BASELINE-TOTAL plus 3, `PROD-HASH-AFTER:` differs from base (throwing-sink-pass-after.md). P2-T1 repo-wide format with CMD-HASH and scoped porcelain before and after, rewritten count 0 (csharpier-format.md); P2-T2 POST-FORMAT: re-run of P1-T3, P1-T9, P1-T10; P2-T3 CMD-LINECOUNT, production greater than base and at most 500, partial at most 500, siblings equal to base (file-line-counts.md); P2-T4 csharpier check; P2-T5 analyzer; P2-T6 nullable; P2-T7 FINAL-FIXTURE-RUN: appended to throwing-sink-pass-after.md; P2-T8 coverage by route with the 944 P3-T8 single re-run rule for issue 780 only; P2-T9 toolchain-final-pass.md; P2-T10 COMPARISON: with CMD-CHANGED-LINES; P2-T11 PrimeFaultOrdering identity (`git diff --exit-code BASE-SHA -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` exit 0, hash equals `BASE-HASH-PFO:`, scoped porcelain empty, positive control: the same diff on the production file exits 1) (prime-fault-ordering-identity.md); P2-T12 determinism tokens per (c4) (determinism-tokens.md); P2-T13 footprint and raw-document check with `--ignored` porcelain over the feature folder (footprint-scope.md); P2-T14 hygiene sweep with the 944 P3-T13 command over this feature folder, FILES_SCANNED at least 38 (evidence-hygiene.md); P2-T15 to P2-T19 one check-off per AC1 to AC5 by ordinal checkbox position (AC1 cites throwing-sink-pass-after.md RESULT lines for the two LaterReadStartsNewPrime tests plus production-edit-scope.md POST-FORMAT:; AC2 cites prime-fault-ordering-identity.md, the SPAN-LINE order and `RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed` in both pass-after runs; AC3 cites the FirstPrimeCompletesAndMarkerIsCleared RESULT lines and the `.Be(TaskStatus.RanToCompletion,` row; AC4 cites throwing-sink-fail-before.md and throwing-sink-pass-after.md; AC5 cites throwing-sink-partial-tokens.md POST-FORMAT:, determinism-tokens.md, toolchain-final-pass.md and the COMPARISON: section); P2-T20 completes ac-status-summary.md in the acceptance-criteria-tracking format; P2-T21 reduced-audit-handoff.md naming the `HandleToggleClickAsync` unguarded `_logError` call (research section 7 item 1) for promotion by the orchestrator, then re-running the hygiene sweep with FILES_SCANNED at least 41. Tool-surface note: the Bash tool was unavailable in this session, so the plan file's line-ending state was not verified; the orchestrator's validator run covers it.

- **Issue:** #947
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** minor-audit (acceptance criteria come from the `## Acceptance Criteria` section of `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md` only; `spec.md` and `user-story.md` are intentionally absent and must stay absent)
- **Last Updated:** 2026-10-01T07-40
- **Status:** Ready for preflight (revision round 0)
- **Version:** 1.0
- **Plan path continuity:** this file is updated in place for every preflight revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; no artifacts-tree evidence path appears in this plan. In task text the token FEATURE abbreviates `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947`; the Write Set below spells every path in full.

**Commits:** none. This plan stages and commits nothing (`git add`, `git commit`, `git merge` and `git stash` do not appear in any task); the orchestrator commits after execution. Every git gate therefore compares the working tree with the recorded base commit (two-dot form with BASE-SHA as the ref operand) and pairs each name-listing diff with a porcelain span in the same task.

## Requirement sources

- Acceptance criteria (sole source): `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, section `## Acceptance Criteria` (heading at line 39 when this plan was authored), which holds exactly five checkbox lines, each one physical line beginning `- [ ] ` (lines 41 to 45 when authored), followed by the `## Logs / Screenshots` heading. The check-off tasks locate a criterion by its ordinal position among the checkbox lines between those two headings, never by line number, and change only `- [ ] ` to `- [x] `.
- Mode marker: `issue.md` line 12 reads `- Work Mode: minor-audit`.
- Design input (not a requirements source): `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/research/2026-10-01T06-47-engine-toggle-throwing-log-sink-research.md`, recommendation (b) in section 2, test design in section 4.2, coverage notes in section 5. This plan adopts recommendation (b).
- Structural precedent: the approved and executed plan for issue #944, `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md`, and its evidence tree. Every citation below was re-derived against the current tree; no line number was copied from that plan.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change):

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: edits E1 to E4 below)
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

Files this plan must not touch, stated so the executor fails closed rather than infers: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs (the main fixture, including the private Harness and its OnLogError hook), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs, TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs (AC2 requires it byte-identical), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TaskMaster/Ribbon/RibbonController.EngineCommands.cs, TaskMaster/Ribbon/RibbonCommandBoundary.cs, TaskMaster/TaskMaster.csproj, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, every file under .claude/ (including .claude/agent-memory/, which this run never writes), every file under config/, every file under artifacts/, and every file under docs/features/potential/. Inside the production file, the `HandleToggleClickAsync` body (its `catch (Exception ex)` clause and the unguarded `_logError(BuildToggleFailedMessage(engineName), ex);` call), `StartPrimeIfNeeded`, the `StartObservedPrime` code, the `GetPrimeTask` code and `ApplyPrimeAsync` are not edited; only the four documented windows E1 to E4 change. The hazard at the click boundary (a throwing sink escapes `HandleToggleClickAsync` into the `async void` Office handler) is out of scope and is recorded for promotion in the reduced-audit handoff, not fixed here. No `spec.md`, `user-story.md` or potential entry is written by this plan. No orchestration state file is written or named by any task. No raw test-result document (trx), raw coverage document (cobertura, coverage, coveragexml) or msbuild log is copied into the feature folder under any name; raw documents stay under the repository coverage directory, which .gitignore line 150 ignores (line 151 re-includes only its .gitkeep).

## AC identity table

Each ID names one checkbox in the `## Acceptance Criteria` section of `issue.md`, in document order (first to fifth checkbox line after the heading).

| ID | Opening words of the criterion |
|---|---|
| AC1 | When the `logError` sink throws while `CompletePrime` reports a faulted or canceled prime, the engine's prime marker is still removed, so a later `GetPressed` ... starts a new prime (`EngineActiveAsync` is invoked a second time) |
| AC2 | The report-then-clear ordering in `CompletePrime` is preserved ... and the existing tests in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` pass without modification |
| AC3 | A throwing `logError` sink leaves no faulted task unobserved ... a deterministic test asserts that the task returned by `GetPrimeTask` for the first prime ends in `RanToCompletion` |
| AC4 | A regression test reproducing the Steps to Reproduce fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/` |
| AC5 | New tests use MSTest, Moq, and FluentAssertions, create no temporary files, and use no `Thread.Sleep` or `Task.Delay`; the C# toolchain ... passes, and the changed lines in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` are covered |

## Verified tree facts (re-derived 2026-10-01 at the branch head; every one is re-checked by Phase 0)

1. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is 442 content lines with no `#nullable` directive. `_logError` field at 56; `_primeTasks` summary 72 to 77 and declaration 78 to 81. `HandleToggleClickAsync` summary 153 to 156 with the two text lines 154 (`The toggle-click boundary: the only place in this type that observes a fault with a`) and 155 (`<c>catch</c> clause.`); method 170 to 186 with its `catch (Exception ex)` at 182 and `_logError(BuildToggleFailedMessage(engineName), ex);` at 184. `GetPrimeTask` summary 238 to 241, `<returns>` 243 to 249 with text lines 244 to 248, the last three (246 to 248) ending `can rely on the fault having been reported.`; signature `internal Task GetPrimeTask(string engineName)` at 250. `StartPrimeIfNeeded` 260 to 289 (`lock (_primeGate)` 272, `ContainsKey` 274). `StartObservedPrime` summary 291 to 293, `<remarks>` 294, remarks text 295 to 301 (295 carries `exactly one <c>catch</c>` after the line break: 295 reads `The observer is a continuation rather than a <c>catch</c> clause, so this type keeps` and 296 reads `exactly one <c>catch</c> — the click boundary. Reading`), `</remarks>` 302, signature 303 to 308, body 310 to 326 with `CompletePrime(completed, engineName);` at 316 inside a `try` (314) whose `finally` (318) calls `marker.SetResult(true);` (320). `ApplyPrimeAsync` 329 to 349. `CompletePrime` summary 351 to 356, remarks 357 to 365, signature `private void CompletePrime(Task completed, string engineName)` 366, body 367 to 382: early return 368 to 371, `var failure =` 373 to 375, the comment beginning `Report-then-clear is load-bearing` 377 to 379, `_logError(BuildPrimeFailedMessage(engineName), failure);` 380, `_primeTasks.TryRemove(engineName, out _);` 381, closing brace 382. `RenderEngineName` summary token `Renders an engine key for inclusion in a message, so a null key is never ambiguous.` at 385. Exactly one `catch (` (182) and one `lock (` (272) in the file; `catch (Exception)` (closing parenthesis immediately after the type) occurs 0 times; `finally` occurs once (318); `exactly one <c>catch</c>` once (296); `the only place in this type that observes a fault with a` once (154); `can rely on the fault having been reported.` once (248).
2. Precedent for containing a failure of the last reporting channel: `TaskMaster/Ribbon/RibbonCommandBoundary.cs` `SafeLog` 96 to 113, `catch (System.Exception)` at 109 with the comment `// Intentionally discarded: see remarks.` at 111; its remarks at 99 to 102. The analyzer and nullable gates already pass with that construct. `.editorconfig` line 27 sets `dotnet_analyzer_diagnostic.severity = suggestion` and line 365 sets `RCS1075` (empty catch of System.Exception) to `suggestion`, so the new empty `catch (Exception)` block, which carries a comment, cannot add a warning-severity diagnostic.
3. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` is 470 content lines: `[TestClass]` 22 on `public partial class EngineToggleStateCoordinatorTests` 23; constants `private const string SpamEngine =` 25 and `private const string SpamToggleControlId =` 26; `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` 160; `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` 213; `private sealed class Harness` 403 to 452 whose constructor wires the error sink as `Errors.Add(new LoggedError(message, exception));` (417) followed by `OnLogError?.Invoke(message, exception);` (418), so a throwing `OnLogError` hook records the report before it throws; strict mock `new Mock<IAppItemEngines>(MockBehavior.Strict);` at 424; `internal Action<string, Exception> OnLogError { get; set; }` 445; `internal List<string> Invalidations` 447; `internal List<LoggedError> Errors` 451; `private sealed class LoggedError` 457 to 468.
4. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` is 77 content lines with the single test `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` (27 to 73), which captures `var prime = harness.Coordinator.GetPrimeTask(SpamEngine);` (35) before the trigger and probes the handle from inside `harness.OnLogError` (37 to 38). `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` is 175 content lines with three tests (32, 85, 132) using `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` with `.Returns(Task.FromResult(true));` and `Times.Exactly(2),`. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` carries `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` at 204. The four partials carry 25 `[TestMethod]` attributes in total; the fixture's executed test count is read at P0-T13, not assumed.
5. `TaskMaster.Test/TaskMaster.Test.csproj` uses explicit `<Compile Include>` items: the coordinator entries are at 352 (`EngineToggleStateCoordinatorTests.cs`), 359 (`.Race.cs`), 360 (`.PrimeFaultOrdering.cs`) and 361 (`.PrimeRegistration.cs`); no entry names `EngineToggleStateCoordinatorTests.ThrowingSink.cs`. The new entry is inserted immediately after the PrimeRegistration entry, whose line P0-T4 re-derives. `<LangVersion>latest</LangVersion>` at 18 (discard lambda parameters and throw expressions compile). `.csharpierignore` line 12 excludes project files from the formatter. `TaskMaster.Test/packages.config` pins Moq 4.21.0 (41), FluentAssertions 8.11.0 (7) and MSTest.TestFramework 4.4.1 (44). No file under `TaskMaster.Test/Ribbon` contains `ThrowingSink`.
6. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (462 lines): `Get-DotnetCoverageArgumentList` (41) hard-codes `/TestCaseFilter:TestCategory!=LiveOutlook` at 91 and passes no blame switch; `ConvertTo-DerivedCoverageSettingsXml` (97); the main function dot-sources `Invoke-MSTestWithCoverage.Helpers.ps1` (303), `Invoke-MSTest.TrxSummary.ps1` (309) and `Invoke-MSTestWithCoverage.Scope.ps1` (313); discovery at 348 to 355 excludes paths whose root-relative form matches a leading `.claude\` segment (353) and throws `No test assemblies found` (358) when nothing is discovered; the thresholds run at 406 to 409, the `First-party coverage:` line is printed at 410 (its text is built at `Invoke-MSTestWithCoverage.FirstParty.ps1` line 120), the JaCoCo projection at 415 to 423, the trx summary at 430 to 447, and the raw document is retained only when written into the repository coverage directory (449 to 453, `Test-RawCoverageDocumentRetained` in `Invoke-MSTestWithCoverage.Projection.ps1` 148). The entry guard at 459 lets the file be dot-sourced. `Invoke-MSTestWithCoverage.Helpers.ps1` defines `Get-CoberturaClassLineSummary` (160; parameter `-ClassNode` 189; both line axes 195 to 196; returns `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches` 252 to 256) and `ConvertTo-KoverageCoberturaXml` (407). `Invoke-MSTestWithCoverage.Threshold.ps1` defines `Assert-CoberturaLineCoverageThreshold` (3) and `Assert-CoberturaBranchCoverageThreshold` (58); `Invoke-MSTestWithCoverage.FirstParty.ps1` `Get-CoberturaFirstPartyCoverageReport` (123); `Invoke-MSTestWithCoverage.Projection.ps1` `ConvertTo-JacocoPackageProjection` (14) and `Assert-JacocoProjectionReconciliation` (83); `Invoke-MSTest.TrxSummary.ps1` `Get-TrxRunSummary` (12) and `Format-TrxRunSummary` (103), whose first line begins `Test run outcome:` (139) and whose last begins `Failed tests:` (147).
7. `scripts/vscode/TaskMaster.cli.runsettings` carries `Workers` 0 and `Scope` ClassLevel only: no logger, no timeout. A direct vstest run therefore needs an explicit trx logger and the hang-blame switch. `scripts/vscode/Install-RepoDotNetSdk.ps1` installs SDK 8.0.205 into `.dotnet-sdk` with the marker directory `sdk\8.0.205` (56); `global.json` pins 8.0.205 with `paths` `.dotnet-sdk`. `scripts/vscode/Invoke-Restore.ps1` takes `-SolutionPath`, `-Configuration`, `-Platform` and runs msbuild Restore with RestorePackagesConfig. `coverage.config` and `coverage/.gitkeep` exist. `.gitignore` ignores `*.trx` (146), `*cobertura*.xml` (147) and the coverage directory (150, re-include 151). `.csharpierignore` excludes the evidence tree (4), raw coverage and trx files (5 to 8), project files (12 to 14), packages.config (16) and app.config (18).
8. `.claude/hooks/validate-planner-output.ps1` line 95 requires a separator-bearing path token on each task's opening line; line 238 requires `### Phase N — <Title>` headings; line 339 requires QA vocabulary in the final phase.
9. Host constraint carried from the #944 run: four UtilitiesCS.Test shell-icon test classes (`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests`, `EmailIntelligence.OSBrowser_Tests`) fail or stall vstest on this workstation (the #944 probe recorded one failure, `STALL-PROBE: REPRODUCES`); CI executes them. P0-T12 measures it again and the result selects the coverage route (D-6).
10. The `.dotnet-sdk`, `packages` and `bin` trees are git-ignored and were not observable from the planning session, so every bootstrap task is guarded and gated on its post-task marker.
11. Issue #944 (marker registered before the prime starts, `marker.SetResult(true)` in a `finally`) is merged and present in this tree (fact 1), so a sink exception thrown inside `CompletePrime` still completes the first prime handle; the pre-fix fail-before run therefore cannot hang, and the hang-blame switch is a bound, not an expectation.

## Design decisions (do not redesign)

- **D-1 Fix shape (research recommendation (b)).** In `CompletePrime`, the `_logError(BuildPrimeFailedMessage(engineName), failure);` statement is wrapped in `try` / `catch (Exception)` whose block holds only the comment `// Intentionally discarded: see the remarks on this method.`, and `_primeTasks.TryRemove(engineName, out _);` stays after the block. No `finally`, no reorder, no change to `StartObservedPrime`, `StartPrimeIfNeeded`, `GetPrimeTask` code or the constructor, no constructor-level wrapper (research (c), rejected as scope widening), no change at the click boundary (line 184). The guard follows the in-repo precedent `RibbonCommandBoundary.SafeLog` (fact 2) and is the catch at a reporting boundary that CLAUDE.md C#4 admits, documented in the method remarks.
- **D-2 Documentation edits are confined to four windows.** E1: the two text lines of the `HandleToggleClickAsync` summary. E2: the last three text lines of the `GetPrimeTask` `<returns>` element. E3: the remarks text of `StartObservedPrime`. E4: `CompletePrime` from its `/// <summary>` line through its closing brace. Every other line of the file is byte-identical to BASE-SHA, proven by P1-T10 (every hunk of the anchored unified diff lies inside one window).
- **D-3 Three new tests in a fifth partial, no harness change.** `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` holds `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime` (AC1 faulted variant, carries the fail-before obligation with a Moq `Times.Exactly(2)` verification failure), `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime` (AC1 canceled variant, same failure mode) and `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` (AC3 and the report-then-clear observation under a throwing sink: its `RanToCompletion` assertion holds before and after the fix because of the #944 `finally`; its final assertion, that the marker is cleared once the handle has completed, fails before the fix by program order). The throwing sink is modelled by `harness.OnLogError`, which the harness invokes after `Errors.Add` (fact 3), so every test also observes that the report was attempted. Each test constructs its own `Harness`; no new mock, type or harness member.
- **D-4 Fail-before is a real run.** The partial uses only `internal` API present at BASE-SHA, so it compiles against the unchanged production file; P1-T5 builds and P1-T6 runs it before any production edit. Before the fix the sink exception escapes `CompletePrime`, the marker stays registered, the second `GetPressed` returns at `ContainsKey`, and `EngineActiveAsync` is verified once rather than twice. Every `await` in the three tests is on a task completed by the continuation's `finally` (fact 11) or already complete, so no await can block; the hang-blame switch bounds the run anyway.
- **D-5 Anchor and substitution rule.** P0-T2 records `BASE-SHA:` as the output of `git rev-parse HEAD` at execution start and requires the two code trees to equal it. Wherever the literal BASE-SHA appears in a command or payload of this plan, the executor substitutes that recorded 40-character value. BASE-SHA is the branch head, not origin/main, because origin/main moves during the run; no merge is performed by this plan.
- **D-6 Coverage route is selected by a recorded observation.** P0-T12 runs the four shell-icon classes alone and records `STALL-PROBE: CLEAR` or `REPRODUCES`. `COVERAGE-ROUTE: RUNNER` (CLEAR) runs scripts/vscode/Invoke-MSTestWithCoverage.ps1 verbatim (CLAUDE.md step 4). `COVERAGE-ROUTE: DIRECT` (REPRODUCES) issues the runner's own inner collector invocation with those four classes excluded and the vstest hang-blame switch appended, post-processed with the runner's own helpers, because the runner hard-codes its filter and carries no blame switch (fact 6). A RUNNER attempt whose log carries `No test assemblies found` is recorded as `ROUTE-REASON: RUNNER-DISCOVERY-FAILED` and the stage is re-issued once by the DIRECT route with the LiveOutlook-only filter. Both routes yield the same committed forms: the `First-party coverage:` line, the JaCoCo package projection text, the trx-derived summary and the per-method coordinator figures, all transcribed into Markdown. Under DIRECT the CLAUDE.md floors are applied by the runner's own threshold functions and a NOT MET result is treated as a runner failure.
- **D-7 Per-method coverage figure.** From the post-processed Cobertura document, take the single `class` element whose `filename` attribute, after replacing backslashes with forward slashes, ends with `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, and reduce it with `Get-CoberturaClassLineSummary` (fact 6). A method's span runs from the first source line matching eight spaces, `private`, a return type and the method name followed by an opening parenthesis, through the first following line that is exactly eight spaces and a closing brace. The method's line coverage is 100 times the number of line-map entries inside the span with hits at least 1, divided by the number of line-map entries inside the span, rounded to two decimals. The catch arm is the lines from the `catch (Exception)` line through the first following line that is exactly twelve spaces and a closing brace; its line-map entries are counted separately (`CATCH-ARM-ELEMENTS`, `CATCH-ARM-UNCOVERED`).
- **D-8 Repository-wide rate is recorded under a comparability rule.** The merged repository line rate is not reproducible across runs of an identical tree. P2-T10 compares it in two branches: `COMPARABLE` when the two root lines-valid figures differ by at most 1 percent of the baseline figure (then the final root line-rate must be at least the baseline root line-rate minus 0.005), otherwise `INCOMPARABLE` (recorded, not gated, with a one-sentence reason). The no-regression weight rests on the per-file, per-method and per-changed-line figures.
- **D-9 Git gates without a commit.** Every `git diff` carries BASE-SHA as its ref operand (working tree against the base commit); every name-listing diff is paired with a `git status --porcelain --untracked-files=all` span in the same task; no gate asserts an empty unscoped porcelain. Porcelain gates assert scope (which trees the lines fall under), never membership counts, because this plan file and `issue.md` gain check-off marks throughout the run and other agents may write under `.claude/agent-memory/`. A PreToolUse refusal of any `.cs` or `.csproj` edit is reported verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run; the executor does not modify hooks, checkpoints or permission configuration.
- **D-10 Fixed artifact names.** No artifact name carries a timestamp; acceptance conditions name files exactly. The write time is the `Timestamp:` field (ISO yyyy-MM-ddTHH-mm).
- **D-11 Check-offs follow the loop.** Every acceptance criterion's evidence is a Phase 1 fail-before observation, a post-format observation or a final-run observation, so every check-off task sits in Phase 2 after the loop and reads the artifact section that survived the final pass. Each check-off task flips exactly one checkbox and completes with the box unchecked when its evidence does not hold, appending exactly one line, `ACn: MET` or `ACn: NOT MET` followed by its failing values, to FEATURE/evidence/other/ac-status-summary.md; P2-T15 creates that file with a `Timestamp:` line and P2-T20 completes it.
- **D-12 Restart rule for the final loop.** The scoped formats at P1-T2 and P1-T8 make both Write Set source files formatter-stable before the loop, so P2-T1 is expected to rewrite nothing. If any step of P2-T1 through P2-T8 fails or rewrites a file, the executor stops and reports the failing step with its artifact; a repair confined to the Write Set is made by the small-path engineer, after which the loop restarts at P2-T1 and each repeated task appends a `PASS-n:` section to its artifact. P2-T9 records every pass and requires the last one to be clean with no rewrite. No `SKIPPED` outcome exists for any Phase 2 command task.

## Delivered source (the executor writes these texts; CSharpier output wins on any layout difference, and the token gates are counted on the formatted text)

Indentation rule: the production-file blocks are shown at their in-file indentation (eight, twelve or sixteen leading spaces) and are written exactly as shown. The new-partial block is shown with four leading spaces of Markdown indent on every line; those four spaces are removed on every line when the file is written, so `using` and `namespace` sit at column 0. Every gated token sits whole on one physical line of the delivered text, and CSharpier does not reflow comments or string literals.

**Production file `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, edit E1 — `HandleToggleClickAsync` summary.** Replace the two text lines of the summary above `internal async Task HandleToggleClickAsync(string engineName)` (the line carrying `The toggle-click boundary: the only place in this type that observes a fault with a` and the line after it, which reads `/// <c>catch</c> clause.`) with:

        /// The toggle-click boundary: one of the two <c>catch</c> clauses in this type; the other
        /// is the sink guard inside <see cref="CompletePrime"/>.

**Edit E2 — `GetPrimeTask` returns.** Replace the three text lines that end the `<returns>` element above `internal Task GetPrimeTask(string engineName)` (from the line carrying `itself and reported through <c>logError</c>. For a key whose prime did not run to` through the line carrying `can rely on the fault having been reported.`) with:

        /// itself and reported through <c>logError</c>. For a key whose prime did not run to
        /// completion, the marker is cleared only after that report has returned or thrown, so a
        /// caller that receives <see cref="Task.CompletedTask"/> can rely on the report having
        /// been attempted.

**Edit E3 — `StartObservedPrime` remarks.** Replace the seven remarks text lines between the `/// <remarks>` and `/// </remarks>` lines above `private void StartObservedPrime(` (from the line carrying `The observer is a continuation rather than a <c>catch</c> clause, so this type keeps` through the line reading `/// never faults or cancels.`) with:

        /// The observer is a continuation rather than a <c>catch</c> clause; the two
        /// <c>catch</c> clauses in this type both sit at reporting boundaries, the click boundary
        /// and the sink guard inside <see cref="CompletePrime"/>. Reading
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

The blank line that separates `CompletePrime` from the `RenderEngineName` summary is retained. Documented tokens quoted here in prose so the presence gates are exonerated: one of the two <c>catch</c> clauses in this type; sink guard inside; returned or thrown; can rely on the report having; both sit at reporting boundaries; no remaining throw source of its own; A failure thrown by the sink itself is contained here; The sink call is guarded (issue #947).; RibbonCommandBoundary.SafeLog; is guaranteed the report has already been; Intentionally discarded: see the remarks on this method.; catch (Exception). Expected post-change size: the base file (442 lines) plus 25 lines (E1 0, E2 1, E3 2, E4 22), well under 500.

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
        /// report after the clear, and must leave no faulted task behind. A fifth partial of the
        /// coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types and the
        /// fixture constants are reused without adding any harness member. The harness invokes
        /// <c>OnLogError</c> after it has recorded the error, so a throwing hook both records the
        /// report and models a throwing sink.
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

            #endregion Issue #947 — a throwing log sink leaves no stale prime marker
        }
    }

Test-side tokens quoted here in prose so the presence gates are exonerated: a throwing sink leaves no marker behind, so the later read starts a new prime; the later read registered a new prime; the sink was invoked once before it threw; the sink receives the injected exception unchanged; the new prime read the engine as active and cached that value; only the successful prime changed state to display; a canceled task carries no exception to unwrap, so one is synthesized; the prime handle never faults; the report is attempted before the marker is cleared; a failed prime leaves nothing to display; the sink exception is contained, so the marker is still cleared; Regression for issue #947; sink failed. Expected size: 166 lines.

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

Substitutions: `ASSEMBLY-TM` is TaskMaster.Test\bin\Debug\TaskMaster.Test.dll; `ASSEMBLY-UCS` is UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll; `FILTER-COORD` is `FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests` (every partial of the fixture); `FILTER-STALL` is `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests`; `NAMES-947` is `"GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime", "GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared", "GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged", "GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime", "GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse", "GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker", "GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns", "GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime", "GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime"`; `NAMES-NONE` is empty. The three new names are the first three entries; the seven others are the pre-existing prime tests of the fixture.

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
        foreach ($m in @("HandleToggleClickAsync", "StartObservedPrime", "CompletePrime")) {
            $start = 0; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i] -match ("^\s{8}(internal async|private) \w+ " + $m + "\(")) { $start = $i + 1; break } }
            $end = 0; for ($i = $start; $i -lt $src.Count; $i++) { if ($src[$i].TrimEnd() -eq "        }") { $end = $i + 1; break } }
            $inSpan = @($s.LineMap.Keys | Where-Object { $_ -ge $start -and $_ -le $end } | Sort-Object)
            $cov = @($inSpan | Where-Object { $s.LineMap[$_].Hits -ge 1 }).Count
            $rate = if ($inSpan.Count -gt 0) { [math]::Round(100.0 * $cov / $inSpan.Count, 2) } else { "NA" }
            Write-Output ("METHOD " + $m + " span=" + $start + "-" + $end + " elements=" + $inSpan.Count + " covered=" + $cov + " uncovered=" + ($inSpan.Count - $cov) + " rate=" + $rate)
            foreach ($n in $inSpan) { Write-Output ("METHOD-LINE " + $m + " " + $n + " hits=" + $s.LineMap[$n].Hits) }
        }
        $cs = 0; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Trim() -eq "catch (Exception)") { $cs = $i + 1; break } }
        $ce = 0; if ($cs -gt 0) { for ($i = $cs; $i -lt $src.Count; $i++) { if ($src[$i].TrimEnd() -eq "            }") { $ce = $i + 1; break } } }
        $arm = @($s.LineMap.Keys | Where-Object { $_ -ge $cs -and $_ -le $ce -and $cs -gt 0 } | Sort-Object)
        Write-Output ("CATCH-ARM span=" + $cs + "-" + $ce + " CATCH-ARM-ELEMENTS: " + $arm.Count + " CATCH-ARM-UNCOVERED: " + @($arm | Where-Object { $s.LineMap[$_].Hits -lt 1 }).Count)
        foreach ($n in $arm) { Write-Output ("CATCH-ARM-LINE " + $n + " hits=" + $s.LineMap[$n].Hits) }
    }

Each `METHOD` span is derived from the source file as it stands when the payload runs (the base file in Phase 0, the fixed file in Phase 2), so each stage measures its own statement set. At the baseline the file contains no line that trims to `catch (Exception)`, so the baseline prints `CATCH-ARM span=0-0 CATCH-ARM-ELEMENTS: 0 CATCH-ARM-UNCOVERED: 0`, which is the negative control for P2-T10. The projection and the summary block are the two CLAUDE.md committed forms; the `COORD-`, `METHOD` and `CATCH-ARM` lines are figures, not documents.

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

**CMD-WINDOWS** (locates the four edit windows E1 to E4 on the working file by their start and end tokens and prints each as `WINDOW <name> = <start>-<end>`; used at P0-T3 on the base text and at P1-T10 to check that every hunk of the anchored diff lies inside a base-side window):

    Set-Location -LiteralPath "WORKTREE"
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $base = @(git show ("BASE-SHA:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs")); if ($base.Count -gt 0) { $base[0] = $base[0].TrimStart([char]0xFEFF) }
    function Find-Line([string[]]$lines, [string]$token, [int]$from) { for ($i = $from; $i -lt $lines.Count; $i++) { if ($lines[$i].Contains($token)) { return $i + 1 } }; return 0 }
    $w1s = Find-Line $base "The toggle-click boundary: the only place in this type that observes a fault with a" 0; $w1e = $w1s + 1
    $w2s = Find-Line $base "itself and reported through <c>logError</c>. For a key whose prime did not run to" 0; $w2e = Find-Line $base "can rely on the fault having been reported." 0
    $w3s = Find-Line $base "The observer is a continuation rather than a <c>catch</c> clause, so this type keeps" 0; $w3e = Find-Line $base "never faults or cancels." 0
    $w4a = Find-Line $base "Observes the outcome of a prime. On any outcome other than ran-to-completion the cache" 0; $w4s = $w4a - 1
    $w4sig = Find-Line $base "private void CompletePrime(Task completed, string engineName)" 0; $w4e = 0; for ($i = $w4sig; $i -lt $base.Count; $i++) { if ($base[$i].TrimEnd() -eq "        }") { $w4e = $i + 1; break } }
    $windows = @(@("E1", $w1s, $w1e), @("E2", $w2s, $w2e), @("E3", $w3s, $w3e), @("E4", $w4s, $w4e))
    foreach ($w in $windows) { Write-Output ("WINDOW " + $w[0] + " = " + $w[1] + "-" + $w[2]) }
    Write-Output ("WINDOW-E1-LINE2-IS-CATCH-CLAUSE: " + ($w1e -gt 1 -and $base[$w1e - 1].Trim() -eq "/// <c>catch</c> clause."))
    Write-Output ("WINDOW-E4-STARTS-AT-SUMMARY: " + ($w4s -gt 0 -and $base[$w4s - 1].Trim() -eq "/// <summary>"))
    $hunks = @(git diff -U0 BASE-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs | Where-Object { $_.StartsWith("@@") })
    Write-Output ("HUNK-COUNT: " + $hunks.Count)
    $outside = 0; $touched = @{}
    foreach ($h in $hunks) { $mm = [regex]::Match($h, "^@@ -(\d+)(,(\d+))? "); $a = [int]$mm.Groups[1].Value; $n = if ($mm.Groups[3].Success) { [int]$mm.Groups[3].Value } else { 1 }; $b = if ($n -eq 0) { $a } else { $a + $n - 1 }; $in = ""; foreach ($w in $windows) { if ($a -ge $w[1] -and $b -le $w[2]) { $in = $w[0]; $touched[$in] = $true } }; if ($in -eq "") { $outside++ }; Write-Output ("HUNK " + $h + " base=" + $a + "-" + $b + " window=" + $(if ($in -eq "") { "NONE" } else { $in })) }
    Write-Output ("HUNKS-OUTSIDE-WINDOWS: " + $outside)
    Write-Output ("WINDOWS-TOUCHED: " + (@($touched.Keys | Sort-Object) -join ","))

A deletion-only hunk has a zero-length new side but a non-empty base range, and an insertion-only hunk (`,0` on the base side) is attributed to the base line it follows; both forms are checked against the windows by their base range.

<!-- CONTINUE -->
