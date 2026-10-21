# Policy Compliance Audit — engine-toggle-prime-marker-registration-races-removal (Issue #944)

- Timestamp: 2026-09-30T16-00 (label supplied by the caller; this review ran without a shell clock and assigned the label later than every executor label in the evidence tree, the latest of which is 2026-09-30T15-23)
- Branch: `bug/engine-toggle-prime-marker-registration-races-removal-944`
- Head: `1f3614deb5182c6b52e2bf1c625aa5b7925019ee` (read from the worktree's `HEAD` -> loose ref `refs/heads/bug/engine-toggle-prime-marker-registration-races-removal-944`)
- Base: `main`, compared at the merged main commit `66afa6372fd82fc1ffd7c81f85a1ad65eebc5817` (MAIN-MERGE-SHA; merged into the branch by `7190a4bcddab8c519933d98b12ede739d4afede3` before the Phase 3 pass-2 loop, plan revision R3-2). The pre-merge anchor was `b305903e275b8abf58e8e65831c189f517568fe4` (ANCHOR-SHA); the two differ by no path under `TaskMaster/Ribbon`, `TaskMaster.Test/Ribbon`, `TaskMaster.Test/TaskMaster.Test.csproj` or `TaskMaster/TaskMaster.csproj` (plan R3-2 measurement).
- Work mode: `full-bug` (issue.md line 12); AC source `spec.md` only.
- Review worktree: the item worktree `.claude/worktrees/agent-a308c11880eff133b` under the session checkout (all reads rooted here; nothing written outside the feature folder).
- Review mechanics: no Bash tool was available. Every observation below was made with Read, Grep and Glob against files on disk: the two changed C# files, the project file, the main fixture (for the reused `Harness` members), the git-ignored post-processed Cobertura documents `coverage/final-944.cobertura.xml` and `coverage/baseline-944.cobertura.xml`, and the 42 Markdown evidence artifacts. Git commands were not run; where a claim rests on a git observation the executor recorded (byte-identity of protected files, the name-status footprint), this audit says so and cites the artifact.
- Template provenance: the MCP policy-audit template asset could not be resolved in this session (the `resolve_policy_audit_template_asset` tool is not surfaced), so this document is hand-authored against the canonical heading set listed in `.claude/skills/policy-audit-template-usage/SKILL.md`. No template instruction block is present.

## Executive Summary

Verdict: **PASS** — 0 Blocking findings, 0 FAIL verdicts, 4 Non-blocking notes, 3 Follow-up items.

The change registers the per-engine prime marker (a `TaskCompletionSource<bool>` created with `RunContinuationsAsynchronously`) in `_primeTasks` under `_primeGate` before the prime starts, and completes it in a `finally` after `CompletePrime` returns. Three new MSTest/Moq/FluentAssertions regression tests in a new partial cover the ordering invariant (program-order fail-before captured), the faulted re-prime and the canceled re-prime. The four-step C# toolchain passed in a single clean pass (pass 2 of the Phase 3 loop; pass 1 stopped on two pre-existing wall-clock QuickFiler test failures in a project the item does not touch, and a coordinator-ruled single restart was recorded as R3-1). First-party coverage 85.32% lines / 79.73% branches (baseline 85.31% / 79.71%); the coordinator class is 157/157 lines and 37/38 branches, and all 17 changed executable lines have hits, verified by this review directly in the raw Cobertura document. No production file is excluded from coverage, no suppression was added, no raw test or coverage document was committed, and the evidence tree carries no absolute host path, account name or machine name (0 hits on this review's own sweep).

Rejected scope narrowing: none. Evidence location compliance: no violation.

## Rejected Scope Narrowing

None detected. The caller's prompt states "This item writes no PowerShell file, so no PowerShell gate applies" — this is a factual statement about the diff (zero `.ps1`/`.psm1` paths on the branch, confirmed by the footprint listing in `evidence/qa-gates/footprint-scope.md` and by the caller-supplied diff), not a narrowing of a language that has changed files. The caller's instruction that the QuickFiler liveness and transaction-timing flakiness are "already-routed items" governs finding attribution, not audit scope; both are recorded below as non-blocking context. The audit scope is the full branch diff against the merged base.

## Evidence Location Compliance

- Scan method: the P3-T14 name-status footprint (43 paths, `evidence/qa-gates/footprint-scope.md`) was read in full, and the feature folder was grepped for `artifacts/(baselines|qa|evidence|coverage)/` — 0 matches. `validate_evidence_locations.py --root .` was not executed (no shell in this session); the disposition rests on the two reads above.
- Files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` in the branch diff: **none**.
- All 38 evidence artifacts live under `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/{baseline,regression-testing,qa-gates,other}/` (canonical `<FEATURE>/evidence/<kind>/`).
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none (no non-canonical path was supplied by the caller or the plan; the plan itself records "EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied").
- Verdict: PASS.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles (UT1)

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each of the three new tests constructs its own `Harness` (`EngineToggleStateCoordinatorTests.PrimeRegistration.cs` lines 35, 88, 135); no static state; the fixture runs under the repository's class-level parallel settings with no `DoNotParallelize` (determinism-tokens: 0). |
| Isolation | PASS | Each test targets one behaviour of `EngineToggleStateCoordinator.GetPressed`/`GetPrimeTask`: registration ordering (test 1), faulted re-prime (test 2), canceled re-prime (test 3). |
| Fast execution | PASS | Fixture run of 28 tests completed in about 2 s (`prime-registration-pass-after.md`, 15-08-31 to 15-08-33 UTC). No waits. |
| Determinism | PASS | Test 1 is decided by program order alone (the read callback runs synchronously inside the prime start on the test thread); tests 2 and 3 await the marker, which completes only after `CompletePrime` returns. No `Thread.Sleep`, `Task.Delay`, `SpinWait`, `DateTime`, `Stopwatch`, polling loop, `.Wait(`, `.Result` or `TaskScheduler` token in the added lines (`evidence/qa-gates/determinism-tokens.md`, 23 tokens all 0; confirmed by reading the file). |
| Readability | PASS | Descriptive names, XML `<summary>` on every test stating scenario and expected outcome, `// Arrange` / `// Act` / `// Assert` markers, FluentAssertions `because` strings on every assertion. |

### 1.2 Coverage and scenarios (UT2)

Coverage floors applied: CLAUDE.md states C# line >= 80% and branch >= 75% (maintainer-settled 2026-09-11, issue #563) and new code >= 90%; `.claude/rules/general-unit-test.md` and `quality-tiers.md` state line >= 85% and branch >= 75%. Both floors are reported against; the measured figures clear both, so no adjudication between the two documents is needed for this item.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-944.cobertura.xml` (git-ignored post-processed Cobertura in the item worktree, tree ANCHOR-SHA `b305903e`; committed projection in `evidence/baseline/coverage-baseline.md`)
- C# post-change coverage artifact: `coverage/final-944.cobertura.xml` (git-ignored post-processed Cobertura in the item worktree, tree HEAD `aac783905` after the main merge; committed projection in `evidence/qa-gates/coverage-summary.md`; root `timestamp="1790781012"` decodes to 2026-09-30T15:10:12Z, which agrees with the recorded collection window 15-09-14 to 15-10-13 UTC)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.31% lines (56078/65736) / 79.71% branches (13594/17054) -> Post-change: 85.32% lines (56098/65750) / 79.73% branches (13597/17054). Change: +0.01% lines (+20 covered, +14 valid) / +0.02% branches (+3 covered, +0 valid). New/changed-code coverage: 100%. Disposition: PASS. Evidence: `coverage/final-944.cobertura.xml` class node `TaskMaster.EngineToggleStateCoordinator` (line-rate 1, branch-rate 0.973684, 157/157 lines, 37/38 branches; the one uncovered branch is the pre-existing null-key arm of `RenderEngineName` at line 389, also 37/38 at baseline), `evidence/qa-gates/coverage-summary.md` COMPARISON section (CHANGED-LINES-WITH-ELEMENT 17, CHANGED-LINES-UNCOVERED 0), `evidence/baseline/coverage-baseline.md`.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State and Verification

C# coverage verdict: PASS (first-party line coverage 85.32% and branch coverage 79.73% clear both the 80/75 CLAUDE.md floors and the 85/75 rules floors; changed-line coverage 100%; no regression on any changed line).

Independent verification performed by this review on the raw Cobertura node (not the executor's transcription):

- `StartPrimeIfNeeded` (span 264-289): method node lists lines 265-269, 272-276, 283-289 — 17 lines, every one `hits="1"`; the two `branch="True"` lines (267, 274) are `100% (2/2)`.
- `StartObservedPrime` (span 303-327): method node lists 11 lines (309-312, 314, 318, 323-327) and the closure node `<StartObservedPrime>b__0` lists 8 lines (313, 315-317, 319-322) — 19 lines, every one `hits="1"`. The closure is retained by the runner's closure filter because its declaring member is present, as plan fact 8 predicted.
- `CompletePrime` (span 366-382): 10 lines, all `hits="1"`, `373` at `100% (4/4)`; unchanged from baseline apart from line offsets.
- Changed executable lines from the caller's diff: 283-287 and 310, 312-322 — all `hits="1"` in the method or closure node. The other 17 changed lines are XML documentation or comment lines with no `<line>` element, as the COMPARISON section records.
- Root element: `line-rate="0.853202" branch-rate="0.797291" lines-covered="56098" lines-valid="65750" branches-covered="13597" branches-valid="17054"` — identical to the committed projection.
- Denominator comparability (plan D-9 / spec AC15): lines-valid 65736 -> 65750 (+14, within 1% of baseline) so the runs are COMPARABLE; 0.853202 >= 0.853079 - 0.005. Note (Non-blocking, NB-3): the baseline tree is ANCHOR-SHA and the final tree includes the main merge `66afa6372` (item 929 content), so the +14/+20 repository delta is not solely this item's; the per-file and per-method figures, which the merge does not perturb, carry the no-regression weight.
- Canonical hook path `artifacts/csharp/coverage.xml`: absent in the item worktree (the `artifacts/` tree is git-ignored and was never populated for this item). The coverage artifact exists and was read at `coverage/final-944.cobertura.xml`; the floors were applied by this review from that document rather than by the hook's reader. Recorded as Non-blocking NB-4, consistent with the #424 disposition (committed projection plus on-disk Cobertura = artifact present). CLAUDE.md's Committed Test Evidence Format forbids committing the raw document, and the executor complied (RAW-DOCS-COMMITTED: 0).

Coverage exclusion policy: no `exclude` entry, `[ExcludeFromCodeCoverage]` attribute or `coverage.config` change is in the diff; the coordinator type's remarks (production file lines 35-43) explicitly keep it measured. PASS.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 3 (1 production `.cs` modified, 1 test `.cs` added, 1 `.csproj` modified) | 7327 total (28 in the coordinator fixture, 3 new) | PASS (7327/7327 passed, pass 2) | 85.31% lines / 79.71% branches | 85.32% lines / 79.73% branches | 100% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.3 Scenario completeness (UT2)

| Scenario class | Verdict | Evidence |
|---|---|---|
| Positive flow | PASS | Success-after-failure re-prime reads active, caches it, invalidates once (tests 2 and 3); existing success-path tests unchanged. |
| Negative flow | PASS | Faulted activation read (tests 1, 2); existing null/whitespace/unmapped-key and null-engines tests unchanged (28-test fixture green). |
| Edge cases | PASS | Already-faulted task (synchronous fault, earliest race window), already-canceled task. |
| Error handling | PASS | Exactly one `logError` report per failed prime, exception identity (`BeSameAs(failure)`), synthesized `OperationCanceledException` on the canceled path. |
| Concurrency | PASS | Ordering invariant under thread-pool continuation asserted by test 1 (registration precedes the read); the existing Race partial (6 tests) unchanged and green. |
| State transitions | PASS | absent -> registered-incomplete -> removed (failure) or retained-complete (success) is exercised across tests 1-3 and `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`. |

### 1.4 Structure and diagnostics (UT3)

PASS. Arrange/Act/Assert sections are marked in each test; every assertion carries a `because` reason; the fail-before message ("Expected handleCompletedDuringRead to be False because the prime handle must be registered before the activation read runs ... but found True") is specific to the failing invariant.

### 1.5 External dependencies and environment (UT4)

PASS. The only collaborator is the fixture's strict `Mock<IAppItemEngines>` (main fixture line 424). No filesystem, network, process, temporary file, wall clock or mutable global state (determinism-tokens `File.`, `GetTempFileName`, `GetTempPath`, `DateTime`, `Environment.TickCount` all 0).

### 1.6 Test file location

PASS by repository convention: the test lives in `TaskMaster.Test/Ribbon/`, mirroring `TaskMaster/Ribbon/` inside the sibling `*.Test` project, which is the layout every existing coordinator partial uses. It is not colocated with production source.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first | PASS | `prime-registration-fail-before.md`: test 1 Failed on the not-completed-during-read assertion against the production file whose hash equals the anchor hash (`D9C915AE...`); 28 tests discovered (25 + 3), no compile or load failure, SEQUENCE_FILES 0. `prime-registration-pass-after.md`: 28/28 after the edit, test side hash unchanged between runs. |
| Minimal targeted fix | PASS | Production numstat 34/12 confined to `StartPrimeIfNeeded`, `StartObservedPrime` and two field summaries; `CompletePrime`, `ApplyPrimeAsync` and `GetPrimeTask` byte-identical to the anchor (P2-T7 region compare, seven PROTECTED regions `equal=True`; the caller diff shows no hunk in them). |
| Full toolchain in order, restart on failure | PASS | `toolchain-final-pass.md`: pass 1 P3-T1..P3-T7 exit 0, P3-T8 exit 1 (two QuickFiler wall-clock failures, untouched project); full restart from P3-T1; pass 2 all eight steps exit 0, no file rewritten, `SKIP_CORECOMPILE_LINES: 0` on both rebuilds. The restart began at formatting as the loop rule requires. |
| Design principles (simplicity, separation of concerns) | PASS | One `TaskCompletionSource` allocation, one `try`/`finally`; no new lock, scheduler seam or `catch`; host-neutral type unchanged in its boundaries. |
| Error handling: fail fast, no swallow | PASS | No `catch` added (`catch (` count 1, the pre-existing click boundary at line 182); faults still flow to `CompletePrime` via the continuation and are reported through the injected sink. |
| Logging pattern | PASS | Unchanged injected `_logError` delegate. |
| File size <= 500 lines | PASS | Production 442 lines, new partial 175 lines (`file-line-counts.md`, both passes; confirmed by Read: 442/175 content lines). Other coordinator files unchanged at 470/277/77. |
| Naming, docs, comments | PASS | `marker`, `handleSeenDuringRead`, `handleCompletedDuringRead` are descriptive; XML docs on `_primeGate`, `_primeTasks`, `StartObservedPrime` updated; why-comment at lines 279-282 names issue #944. |
| Public API stability | PASS | All touched members are `private`; `GetPrimeTask` (internal) keeps its signature; the only production caller (`RibbonController.EngineCommands.cs`) is unchanged. |
| Existing tests as spec | PASS | All 25 pre-existing coordinator tests unchanged and passing, including the #942 `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`. |
| Dependencies | PASS | None added; `RunContinuationsAsynchronously` is .NET Framework 4.6+ and already used in production (`NonBlockingDelay.cs`, `AppOlObjects.FolderTreeService.cs`). |
| Supporting documents updated | PASS | spec.md check-offs (18/18), plan check-offs (72 boxes), `evidence/other/ac-status-summary.md`, `reduced-audit-handoff.md`. |

Architecture boundaries (`.claude/rules/architecture-boundaries.md`): no new reference to `Microsoft.Office.Tools.*`, `Microsoft.Office.Interop.Outlook`, `[ComVisible]` or ribbon callbacks; the production file's `using` set is unchanged (`System`, `System.Collections.Concurrent`, `System.Globalization`, `System.Threading`, `System.Threading.Tasks`, `UtilitiesCS`). PASS.

## 3. Language-Specific Code Change Policy Compliance

C# (`.claude/rules/csharp.md`, CLAUDE.md C#1-C#7):

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run`, format then check | PASS | P3-T1 `dotnet tool run csharpier format .` rewrote 0 files (Write Set hashes identical); P3-T4 `dotnet tool run csharpier check .` "Checked 1627 files", no differences. The partial's LF->CRLF normalisation happened at the P2-T8 scoped format before commit and was recorded. |
| Analyzer rebuild (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS | P3-T5 exit 0, ERRORS 0, WARNINGS 0, `SKIP_CORECOMPILE_LINES: 0`, `CSC_OUT_TASKMASTER 2`, `CSC_OUT_TASKMASTER_TEST 2` (the gate was not vacuous). |
| Nullable rebuild (`/t:Rebuild`, `TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) | PASS | P3-T6 exit 0, same counters; the command text in the evidence matches CLAUDE.md character for character. |
| Test run with coverage | PASS | DIRECT route: `dotnet-coverage collect ... -- vstest.console.exe` over 9 assemblies with `/InIsolation`, the LiveOutlook filter, the four shell-icon classes excluded, `/Blame:CollectHangDump;TestTimeout=4min`, post-processed by the runner's own helpers and floor checks (`LINE-FLOOR: MET`, `BRANCH-FLOOR: MET`). Route selection recorded (`STALL-PROBE: REPRODUCES`). See NB-2 on the probe classification. |
| Naming (`PascalCase`/`camelCase`) | PASS | `StartObservedPrime`, `marker`, `engineName`. |
| Null safety | PASS | No new nullable-flow warning (nullable rebuild green); the file carries no `#nullable` directive at the anchor and none was added, so the per-file opt-in state is unchanged. |
| Exceptions / boundaries | PASS | No broad catch added. |
| XML docs on non-obvious contracts | PASS | `_primeTasks` summary now states the marker lifecycle; `StartObservedPrime` remarks state that the marker never faults or cancels. |
| Analyzer stack constraints (no suppression, no severity change) | PASS | No `#pragma`, `[SuppressMessage]`, `.editorconfig` or `BannedSymbols.txt` change in the diff; the discarded continuation uses `_ =` (satisfies MA0134). |
| No `Thread.Sleep`/`Task.Delay`/`DateTime.Now` in touched code | PASS | Production diff adds none; test partial adds none. |
| Project file change | PASS | One `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs" />` at line 361, adjacent to the PrimeFaultOrdering entry (verified by Grep); `.csproj` is excluded from CSharpier by `.csharpierignore`. |

PowerShell, Python, TypeScript: no files of these languages changed on the branch; the corresponding language policies are not exercised by this diff.

## 4. Language-Specific Unit Test Policy Compliance

C# Unit Test Policy (CUT1-CUT3):

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestMethod]` x3 in a `public partial class` whose `[TestClass]` sits on the main fixture (line 22); `using Microsoft.VisualStudio.TestTools.UnitTesting;`. No xUnit/NUnit. |
| Moq for mocking | PASS | Reuses the fixture's `new Mock<IAppItemEngines>(MockBehavior.Strict)`; `Setup`, `SetupSequence`, `Verify(..., Times.Exactly(2))`. No new mock type. |
| FluentAssertions | PASS | 13 `.Should()` chains; `BeFalse`, `ContainSingle`, `BeSameAs`, `NotBeSameAs`, `BeTrue`, `Equal`, `BeAssignableTo<OperationCanceledException>`. No MSTest `Assert.*`. |
| Toolchain command selection | PASS | Section 3 table. |
| Deterministic test rules (no network/PATH/cwd/external services) | PASS | Section 1.5. |
| Seam-based mocking | PASS | Existing `IAppItemEngines` interface seam and injected delegates; no new seam. |
| Time seam guidance | PASS | No clock read in touched code. |
| Repository line coverage >= 80% / new code >= 90% / no changed-line regression | PASS | 85.32% / 100% / 0 regressions (section 1.2). |

## 5. Test Coverage Detail

| Unit | Baseline (ANCHOR-SHA) | Post-change (HEAD after main merge) | Verdict |
|---|---|---|---|
| `EngineToggleStateCoordinator` class lines | 143/143 (100%) | 157/157 (100%) | PASS |
| `EngineToggleStateCoordinator` class branches | 37/38 (97.37%) | 37/38 (97.37%) | PASS (unchanged; the missed branch is the pre-existing null-key arm of `RenderEngineName`, outside the diff) |
| `StartPrimeIfNeeded` | 13/13 (100%) | 17/17 (100%) | PASS (>= 90%) |
| `StartObservedPrime` incl. continuation closure | 9/9 (100%) | 19/19 (100%) | PASS (>= 90%) |
| `CompletePrime` | 10/10 (100%) | 10/10 (100%) | PASS (unchanged) |
| Changed executable lines (17) | (new lines, absent at baseline) | 17/17 hit | PASS (no regression) |
| TaskMaster package (JaCoCo projection) | LINE 2443 covered / 802 missed; BRANCH 517 / 211 | LINE 2457 / 802; BRANCH 517 / 211 | PASS (+14 covered lines, the coordinator's new lines; 0 new misses) |
| First-party repository | 85.31% lines / 79.71% branches | 85.32% lines / 79.73% branches | PASS |

The +8 covered lines / +2 covered branches in the `UtilitiesCS` package between the two projections belong to an assembly this item does not touch and fall inside the known run-to-run variance of that assembly's constants.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Fail-before run (P1-T4) | 28 executed, 27 passed, 1 failed (`GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`), exit 1, 13-32-01 to 13-32-03 UTC | `evidence/regression-testing/prime-registration-fail-before.md` |
| Pass-after run (P2-T4) | 28/28 passed, exit 0, 13-35-59 to 13-36-01 UTC | `evidence/regression-testing/prime-registration-pass-after.md` |
| Final fixture run (P3-T7, pass 2) | 28/28 passed, exit 0, 15-08-31 to 15-08-33 UTC | same artifact, PASS-2 section |
| Full coverage run, pass 1 (P3-T8) | 7327 executed, 7325 passed, 2 failed (QuickFiler `QfcDatamodelLivenessTests`, 5-second wall-clock waits), exit 1, 13-49-10 to 13-51-16 UTC | `evidence/qa-gates/coverage-summary.md` Details |
| Full coverage run, pass 2 (P3-T8) | 7327 executed, 7327 passed, 0 failed, exit 0, 15-09-14 to 15-10-13 UTC | same artifact, PASS-2 section; Cobertura root timestamp 2026-09-30T15:10:12Z |
| Baseline coverage run (P0-T18) | 7324/7324 passed, exit 0 | `evidence/baseline/coverage-baseline.md` |
| Test count delta | +3 (25 -> 28 in the fixture; 7324 -> 7327 repository) | population comparison in the pass-after artifact |

Timestamp consistency: every executor `Timestamp:` label agrees with the UTC wall-clock window embedded in the same artifact to the minute (for example 13-32 vs 13-32-01Z; 15-10 vs 15-09-14..15-10-13Z), and the Cobertura root epoch corroborates the pass-2 label. No synthetic-timestamp drift was found.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Confidentiality masking scan | Grep of the feature folder for drive-letter paths, user-profile folder segments, the developer account name, mail address and common machine-name prefixes | 0 hits (this review); executor sweep P3-T13/P3-T35 `ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0` over 45 files. Trx failure detail uses `REDACTED-PATH`; assembly paths are recorded root-stripped (`\QuickFiler.Test\bin\Debug\...`). |
| Suppression scan (added lines) | Read of both added hunks | 0 `#pragma warning`, 0 `[SuppressMessage]`, 0 `ExcludeFromCodeCoverage`, 0 `.editorconfig` edits. |
| Workflow change scan | Footprint listing | 0 `.github/` paths in the diff; no workflow modified. |
| Raw document scan | P3-T12 extension filter (.trx, .xml, .coverage, .coveragexml, .cobertura) against MAIN-MERGE-SHA | RAW-DOCS-COMMITTED: 0; RAW-DOCS-UNTRACKED-IN-FEATURE: 0. |
| Determinism token scan | Added test lines vs 23 banned tokens | all 0. |
| Protected region compare | SHA-256 of seven regions vs anchor | all `equal=True`; the two edit windows `equal=False` (positive control). |

## 8. Gaps and Exceptions

Non-blocking notes:

- NB-1 (context, already routed by the coordinator): pass 1 of the final coverage run failed two `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests` tests on five-second wall-clock waits in a project this item does not modify; the coordinator approved one full loop restart (R3-1). The wall-clock waits themselves (`SpinWait.SpinUntil(..., 5 s)`, `Task.Wait(5 s)`) are the flakiness the coordinator is filing separately; they are not this item's finding.
- NB-2 (evidence): the P0-T16 stall probe returned `REPRODUCES` because one shell-icon test failed (`Win32 handle that was passed to Icon is not valid`), not because a hang occurred (`SEQUENCE_FILES: 0`). The plan's rule treats any non-clean probe as REPRODUCES, so the DIRECT route excluded the four `UtilitiesCS.Test` shell-icon classes from the local coverage run. Those classes execute in CI; the PR-time CI run remains the gate for them. The exclusion does not touch any assembly this item changes.
- NB-3 (evidence): the coverage baseline was measured at ANCHOR-SHA and the final at HEAD after merging main `66afa6372`, so the repository-wide delta includes item-929 content; the per-file and per-method figures carry the no-regression conclusion (section 1.2.2).
- NB-4 (procedural): no copy of the coverage document exists at the review hook's canonical path `artifacts/csharp/coverage.xml`; the artifact was read at `coverage/final-944.cobertura.xml` and the floors were applied manually. No remediation is requested: the repository forbids committing the raw document and the on-disk document plus the committed projection satisfy the evidence model.

Follow-up items (out of this item's scope by spec; recommend promotion to issues so they survive the feature-folder archive):

- FU-1: a throwing `logError` sink skips `_primeTasks.TryRemove` under report-then-clear; with this fix the marker still completes but stays registered, so the engine cannot re-prime for the session, and the discarded continuation task faults unobserved (spec Rollout item 1).
- FU-2: after a permanently faulted configuration load, every cache-miss `getPressed` poll that reaches `StartPrimeIfNeeded` now re-primes and logs again; a back-off or `ResetConfigAsyncLazy`-based recovery is a separate decision (spec Rollout item 2).
- FU-3: `GetPrimeTask` `<returns>` (production lines 244-245) still opens with "The prime task"; the value is now the registration marker that completes after the prime's outcome has been observed. Plan decision D-3 ruled the sentence not false and froze the method; a one-sentence precision edit is a candidate for the next touch of the file.

Assumptions made because git could not be run in this session:

- The delta between `594c3eb9b` (PRE-FINAL-COMMIT-HEAD, the tree P3-T12/P3-T14 measured) and HEAD `1f3614deb` is the P3-T35 feature-folder commit only. Basis: plan D-10 (the P3-T35 commit stages feature-folder paths only), the P3-T14 porcelain showing no uncommitted path under `TaskMaster/` or `TaskMaster.Test/`, and the caller's statement of the diff.
- Byte-identity of the three protected test partials and `RibbonController.EngineCommands.cs` to main rests on the executor's `git diff --exit-code` results (`PROTECTED_FILES_DIFF_EXIT=0` on both passes) and on their absence from the caller-supplied diff.

## 9. Summary of Changes

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (+34/-12): `_primeGate` and `_primeTasks` summaries reworded; `StartPrimeIfNeeded` creates a `TaskCompletionSource<bool>` (`RunContinuationsAsynchronously`), stores `marker.Task` in `_primeTasks` under the existing lock after the `ContainsKey` probe, then calls `StartObservedPrime(engines, engineName, controlId, marker)`; `StartObservedPrime` becomes `void`, discards the `ContinueWith` task (`_ =`) and wraps `CompletePrime` in `try`/`finally { marker.SetResult(true); }` with the three continuation arguments unchanged. `CompletePrime`, `ApplyPrimeAsync`, `GetPrimeTask` untouched.
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (new, 175 lines): three regression tests reusing the fixture's `Harness`, `LoggedError`, `SpamEngine`, `SpamToggleControlId`.
- `TaskMaster.Test/TaskMaster.Test.csproj` (+1/-0): compile entry for the new partial.
- `docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md` (inherited promotion record, unchanged by the plan) and the feature folder (spec, issue, research, plan, 38 evidence artifacts).

## 10. Compliance Verdict

**PASS.** 0 Blocking, 0 FAIL. Remediation inputs: not produced (no remediation-required finding). Non-blocking: NB-1 to NB-4. Follow-up: FU-1 to FU-3.

## Appendix A: Test Inventory

| Test | File | Purpose | Result |
|---|---|---|---|
| `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns` (new) | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` lines 31-75 | Program-order discriminator: the handle observed inside the activation read is incomplete; awaiting it yields one logged error with the injected exception; afterwards `GetPrimeTask` returns a different, completed task. Carries the fail-before obligation. | Failed before fix (P1-T4), Passed after (P2-T4, P3-T7 x2, P3-T8 x2) |
| `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime` (new) | same file, lines 84-123 | After an already-faulted read, a later read starts a new prime: `EngineActiveAsync` x2, pressed true, one invalidation, one error `BeSameAs(failure)`. | Passed (all runs) |
| `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime` (new) | same file, lines 131-171 | Canceled variant: `Task.FromCanceled<bool>`, one error assignable to `OperationCanceledException`. | Passed (all runs) |
| `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` (existing, #942) | `EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` | Report-then-clear: handle identity inside the sink, cleared after. | Passed (all runs, unchanged) |
| `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` (existing) | `EngineToggleStateCoordinatorTests.cs` | At-most-one-prime guard. | Passed (unchanged) |
| Remaining 23 coordinator tests (main fixture 16 methods / 18 cases, Race partial 6) | `EngineToggleStateCoordinatorTests.cs`, `.Race.cs` | Existing behaviour spec. | 28/28 fixture total, Passed |
| Repository suite | 9 test assemblies | Full regression under coverage. | 7327/7327 Passed (pass 2) |

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded) | Exit |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (0 rewrites) |
| Format check | `dotnet tool run csharpier check .` | 0 |
| Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 warnings) |
| Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 (0 warnings) |
| Fixture test | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" ...` | 0 (28/28) |
| Coverage test | `dotnet-coverage collect --output coverage\final-944.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-944.config -- vstest.console.exe (9 assemblies) /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\944\final" "/Logger:trx;LogFileName=final-944.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` then the runner's post-processing helpers | 0 (7327/7327; both floors met) |
