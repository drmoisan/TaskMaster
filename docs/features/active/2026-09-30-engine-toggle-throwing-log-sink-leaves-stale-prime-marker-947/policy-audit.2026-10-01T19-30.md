# Policy Compliance Audit — engine-toggle-throwing-log-sink-leaves-stale-prime-marker (Issue #947)

- Component: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (ribbon engine-toggle state coordinator) and its MSTest fixture
- Date: 2026-10-01
- Reviewer: feature-review agent (minor-audit work mode; parallel cohort bugs-2026-09-28, cohort index 4)
- Branch: `bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947`
- Base commit (BASE-SHA): `2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85` (the merge of origin/main into this branch immediately before execution)
- Head commit: `ec312202e8fdaa6ba70a958cf3ca4a0b7503d1ce` (read from the worktree reflog, last entry)
- Review timestamp label: `2026-10-01T19-30`. No shell clock was available to this review (Bash was withheld by the caller). The label was assigned later than every executor label (latest executor label 18-16; coordinator ruling label 19-00) and is a label, not a measured clock reading.
- Files under test: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modified), `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` (added), `TaskMaster.Test/TaskMaster.Test.csproj` (modified, one `Compile Include` line)
- Review tooling constraint: Read, Grep and Glob only. No command was run by this review. Every figure below was either read from a committed evidence artifact in the feature folder or re-verified directly from a file in the review worktree (the source files, the project file, the worktree reflog, and the gitignored post-processed Cobertura documents `coverage/baseline-947.cobertura.xml` and `coverage/final-947.cobertura.xml` retained by the executor).

## Executive Summary

**Verdict: PASS.** 0 Blocking findings. 6 Non-blocking findings (PA-1 to PA-6 below), 5 recorded gaps (section 8).

The change wraps the `_logError` sink call at both coordinator call sites (`CompletePrime` and the `catch (Exception ex)` clause of `HandleToggleClickAsync`) in a `try` / `catch (Exception)` whose block holds only a documented discard comment, following the in-repo precedent `RibbonCommandBoundary.SafeLog`. Report-then-clear ordering in `CompletePrime` is preserved (`_logError` at line 408, `_primeTasks.TryRemove` at line 415, outside the guard). Four MSTest regression tests were added in a new fifth partial of the coordinator fixture; all four failed on the hash-identical base production file and pass after the fix. The full C# toolchain passed in one clean pass (CSharpier check, analyzer Rebuild 0 errors / 0 warnings, nullable Rebuild 0 errors / 0 warnings, 7336/7336 tests). Coverage: first-party 85.34% lines / 79.73% branches post-change (baseline 85.34% / 79.73%); the coordinator file is 167/167 lines and 37/38 branches; every changed executable line (12/12) is covered, both new catch arms 3/3.

C# coverage verdict: PASS (first-party 85.34% lines / 79.73% branches, changed-line coverage 100%, no regression on changed lines).

The two broad `catch (Exception)` clauses are evaluated under CLAUDE.md C#4 as catches at a clear reporting boundary (the last reporting channel of the type), each documented in its method remarks and each following the existing `RibbonCommandBoundary.SafeLog` precedent; this is recorded as a documented, accepted exception to the general "re-raise or propagate with added context" rule (section 2.3), not as a violation.

## Rejected Scope Narrowing

No scope-narrowing instruction was detected in the caller prompt. Three caller statements were evaluated and classified as not narrowing:

1. "(This item changes no PowerShell file, so no PowerShell gate applies.)" — a factual statement corroborated by the branch footprint (`evidence/qa-gates/footprint-scope.md`, 29 name-status lines, none ending `.ps1`/`.psm1`). PowerShell has zero changed files on this branch, so no PowerShell coverage verdict is owed.
2. "do not require a raw coverage XML" — refers to the committed form of evidence under the CLAUDE.md Committed Test Evidence Format (projection and summary, never the raw document). Coverage was nonetheless verified by this review against the retained post-processed Cobertura documents in the worktree; no verification was skipped.
3. "do not use the Bash tool at all" — a tooling constraint on this review, not a scope limit. The full BASE-SHA..head diff footprint was audited from the committed name-status listing and the source files.

The coordinator ruling re-anchoring the P2-T11 and P2-T13 literal untracked/porcelain clauses to the paired BASE-SHA diff (recorded in `evidence/other/reduced-audit-handoff.md` and the plan Revision log) is accepted as instructed: it concerns the plan's own gate wording after per-phase commits, not the audit scope, and the ruling is transcribed into tracked files.

## Evidence Location Compliance

- Every evidence artifact on the branch diff lives under `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/{baseline,regression-testing,qa-gates,other}/` (26 `A` lines in the P2-T13 name-status listing plus the 13 Phase 2 files written later; all `.md`).
- Paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: **none** in the diff footprint (`footprint-scope.md` lists no path under `artifacts/`; the worktree has no `artifacts/` directory at all, confirmed by Glob).
- `validate_evidence_locations.py --root .` could not be executed by this review (no shell). The manual scan above over the complete 29-line name-status listing and the 14-line porcelain span stands in for it; both lists were read verbatim.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none (no non-canonical evidence path was supplied by the caller or the plan).
- Raw documents (trx, Cobertura, msbuild logs) in the feature folder: 0 (`RAW-DOCS-IN-FEATURE: 0`, `NON-MD-IN-FEATURE: 0`; Glob of the feature folder lists 42 `.md` files and nothing else). This satisfies the CLAUDE.md Committed Test Evidence Format.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles (UT1)

| Principle | Status | Evidence |
|---|---|---|
| Independence | PASS | Each of the 4 new tests constructs its own `Harness` (fresh strict `Mock<IAppItemEngines>`, fresh coordinator, fresh recording lists); no static state, no `[DoNotParallelize]` (`determinism-tokens.md`: `DoNotParallelize` 0). Ran green under `Workers 0` / `Scope ClassLevel` in the 32-test fixture run and the 7336-test suite. |
| Isolation | PASS | Each test targets one behaviour: faulted-prime re-prime (AC1), canceled-prime re-prime (AC1), first-prime completion plus marker clear under a throwing sink (AC3), click-boundary containment (AC6). |
| Fast execution | PASS | All awaits are on `TaskCompletionSource` markers completed by program order (`RunContinuationsAsynchronously` marker, `finally { marker.SetResult(true) }`); no timers, no I/O. |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, timers or wall-clock reads (`determinism-tokens.md`, re-verified by reading the partial). Outcomes depend on program order, not scheduling. The hang-blame switch on every vstest run recorded `SEQUENCE_FILES: 0`. |
| Readability | PASS | Descriptive names in the fixture's `Method_Scenario_Outcome` style, XML `<summary>` on each test stating invariant and pre-fix failure mode, `// Arrange` / `// Act` / `// Assert` markers, every assertion carries a reason string. |

### 1.2 Coverage and Scenarios (UT2)

Coverage figures below are the executor's DIRECT-route runs (`dotnet-coverage collect` around `vstest.console.exe`, 9 test assemblies, four UtilitiesCS.Test shell-icon classes excluded locally per the recorded `STALL-PROBE: REPRODUCES`), re-verified by this review against the retained Cobertura documents.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 3 (1 production `.cs`, 1 test `.cs`, 1 `.csproj`) | 4 new; 32 in fixture; 7336 in suite | 7336/7336 passed, 0 failed | 85.34% lines / 79.73% branches | 85.34% lines / 79.73% branches | 100% |
| TypeScript | 0 | none | N/A | N/A | N/A | N/A |
| Python | 0 | none | N/A | N/A | N/A | N/A |
| PowerShell | 0 | none | N/A | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/coverage-baseline.md` (committed JaCoCo package projection and summary of `coverage/baseline-947.cobertura.xml`, root `line-rate=0.853369 branch-rate=0.797291`, epoch `timestamp="1790890899"`)
- C# post-change coverage artifact: `evidence/qa-gates/coverage-summary.md` (committed projection and summary of `coverage/final-947.cobertura.xml`, root `line-rate=0.853406 branch-rate=0.797291`, epoch `timestamp="1790892471"`)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.34% lines (56109/65750) / 79.73% branches (13597/17054). Post-change: 85.34% lines (56120/65760) / 79.73% branches (13597/17054). Change: +0.0037% lines (+11 covered lines, +10 valid lines; root line-rate 0.853369 to 0.853406) / +0.00% branches (unchanged). New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/qa-gates/coverage-summary.md` (COMPARISON section, `CHANGED-LINES-WITH-ELEMENT: 12`, `CHANGED-LINES-UNCOVERED: 0`, both `CATCH-ARM-UNCOVERED: 0`), re-verified at the `<class filename="TaskMaster\Ribbon\EngineToggleStateCoordinator.cs">` node of `coverage/final-947.cobertura.xml` (`line-rate="1" branch-rate="0.973684"`; lines 188-196 and 407-416 all `hits="1"`).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

C# coverage verdict: PASS (first-party 85.34% lines / 79.73% branches post-change, above the 85% line and 75% branch floors of `.claude/rules/general-unit-test.md` and the 80% / 75% floors of CLAUDE.md UT2; changed-line coverage 100%; no regression on any changed line).

C# canonical-path coverage artifact `artifacts/csharp/coverage.xml`: absent from the review worktree (no `artifacts/` directory exists there); any stale copy at that path in another checkout is not this branch's measurement and is rated FAIL as evidence for this review and not used. The repository's coverage route writes under `coverage/` and the CLAUDE.md Committed Test Evidence Format prohibits committing the raw document, so the evidence basis for this review is the committed projection plus the retained post-processed Cobertura document, which this review read directly. This is a pre-existing path-convention mismatch (also recorded on #942, #944 and #940), not a defect of this change; recorded as gap G-4.

Per-file figures for the one modified production file, re-verified by this review at the Cobertura `<class>` node:

| Scope | Baseline | Post-change | Floor | Result |
|---|---|---|---|---|
| `EngineToggleStateCoordinator.cs` lines | 157/157 (100%) | 167/167 (100%) | 85% (modified file) | PASS |
| `EngineToggleStateCoordinator.cs` branches | 37/38 (97.37%) | 37/38 (97.37%) | 75% | PASS |
| `CompletePrime` method (13 line-map elements incl. the new guard) | 10/10 | 15/15 | 90% (CLAUDE.md new/changed method target) | PASS |
| `HandleToggleClickAsync` method | 13/13 | 18/18 | 90% | PASS |
| New catch arm, `HandleToggleClickAsync` lines 191-194 | (absent) | 3/3 | changed lines covered | PASS |
| New catch arm, `CompletePrime` lines 410-413 | (absent) | 3/3 | changed lines covered | PASS |
| Changed executable lines (git diff -U0 against BASE-SHA) | — | 12/12 | no regression | PASS |

The one uncovered branch in the file is the pre-existing `RenderEngineName` null-key arm (line 423, `condition-coverage="50% (1/2)"`), unchanged by this diff. The new test partial is test code and is excluded from the coverage denominator by design.

Repo-wide comparability: the executor's D-8 rule classified the two runs `COMPARABLE` (lines-valid 65750 vs 65760, within 1% of baseline; final root line-rate 0.853406 >= 0.853369 - 0.005). The +10 valid-line delta is the two new guard blocks plus one reflowed statement; the +11 covered-line delta is consistent with all of them being hit.

### 1.3 Scenario Completeness (UT2)

| Scenario class | Status | Evidence |
|---|---|---|
| Positive flow | PASS | Prime-site tests: after the sink throws on the first prime, a later `GetPressed` starts a new prime that succeeds (`EngineActiveAsync` returns `true`, cache set, exactly one invalidation). |
| Negative / error handling | PASS | Throwing sink on a faulted prime, on a canceled prime (synthesized `OperationCanceledException` asserted), and on a faulted toggle; the sink exception is contained at both sites. |
| Edge / boundary | PASS | Canceled prime (null `Task.Exception`) variant; marker handle observed from inside the throwing sink (report-then-clear under a throwing sink). |
| State transitions | PASS | Marker registered -> report attempted -> marker cleared -> re-prime registered; `NotBeSameAs(firstPrime)` proves a new marker. |
| Concurrency | PASS (as applicable) | Continuations run on `TaskScheduler.Default`; tests synchronize solely through the marker task. The pre-existing registration/removal race tests (#944 partial) are unchanged and pass. |

### 1.4 Test Structure and Diagnostics (UT3)

| Check | Status | Evidence |
|---|---|---|
| Arrange-Act-Assert | PASS | Explicit markers in all four tests; the click-boundary test defines `Func<Task> act` under Act and awaits `NotThrowAsync` under Assert, matching the existing fixture test at `EngineToggleStateCoordinatorTests.cs` lines 342-346. |
| Clear failure messages | PASS | Every assertion carries a reason; the fail-before run shows the reasons verbatim (`a throwing sink leaves no marker behind, so the later read starts a new prime`, `the click boundary contains a failure of the sink itself`). |
| Documented intent | PASS | `<summary>` on the partial class and on each test, stating invariant and pre-fix failure mode. |

### 1.5 External Dependencies and Environment (UT4)

| Check | Status | Evidence |
|---|---|---|
| No external services | PASS | Strict Moq mock of `IAppItemEngines`; delegates for the three sinks. |
| No temporary files | PASS | `GetTempPath`, `GetTempFileName`, `File.`, `Directory.` each 0 in the partial (`determinism-tokens.md`, re-verified by reading). |
| No mutable global state | PASS | All state is per-`Harness`. |

### 1.6 Policy Audit of Tests (UT5)

PASS. Each of the four tests was reviewed individually against UT1-UT4 above; no exception is claimed.

### 1.7 Test File Location

PASS with repo-convention note. The repository's C# layout places tests in sibling `*.Test` projects (`TaskMaster.Test/Ribbon/` mirrors `TaskMaster/Ribbon/`); the new partial follows that established layout and the existing four partials of the same fixture. It is not colocated with production code.

## 2. General Code Change Policy Compliance

### 2.1 Bugfix Workflow

| Step | Status | Evidence |
|---|---|---|
| Failing regression test first | PASS | `evidence/regression-testing/throwing-sink-fail-before.md` (P1-T6): 4 failed / 28 passed on a build where `PROD-HASH-AT-CONTROL` equals `BASE-HASH-PROD` (`B3C6FEB2...3086`); `git diff --exit-code BASE-SHA -- <production file>` exit 0 at the control. The click-boundary failure stack names base line 184 (the unguarded sink call). |
| Minimal, targeted fix | PASS | Six edit windows only (E1-E6); `HUNKS-OUTSIDE-WINDOWS: 0`; seven protected files byte-identical to BASE-SHA (`protected-regions-unchanged.md`, POST-FORMAT re-run). No refactor, no helper extraction, no constructor-level wrapper. |
| Verify locally before review | PASS | `throwing-sink-pass-after.md` (P1-T12 and P2-T7, 32/32) and the Phase 2 loop (section 7). |

### 2.2 Design Principles

| Principle | Status | Evidence / Note |
|---|---|---|
| Simplicity first | PASS | Inline `try`/`catch` at each site; no indirection added. |
| Reusability | PASS with note | The identical 8-line guard appears at two sites. The plan (D-1) considered and rejected a shared private helper as a refactor beyond the minimal fix. Recorded as PA-1 (non-blocking, Minor). |
| Extensibility | PASS | No public API change; all members `internal`/`private`. |
| Separation of concerns | PASS | The type remains host-neutral decision logic; sinks stay injected delegates. |

### 2.3 Error Handling, Logging, and Contracts

| Check | Status | Evidence / Note |
|---|---|---|
| Fail fast; no silent broad catch | PASS (documented exception) | Two new `catch (Exception)` blocks discard the exception without re-raise or added context. Both sit at the type's last reporting channel: the only consumer of the discarded exception would be the sink that just threw. The alternative (escape) is the defect under repair: a faulted, discarded continuation and a stale marker at the prime site, and an exception propagating into an `async void` Office handler at the click site. Each guard is documented in its method's `<remarks>` (lines 167-171, 382-388) and in-block (`// Intentionally discarded: see the remarks on this method.`), and follows `RibbonCommandBoundary.SafeLog` (lines 103-113, `catch (System.Exception)` with the same discard comment), which is already on main and passes the analyzer gate (`RCS1075` held at `suggestion` by `.editorconfig`). Disposition: accepted under CLAUDE.md C#4 ("at a clear boundary"); see PA-2 for the residual observability note. |
| Logging pattern | PASS | Logging remains the injected `logError` delegate; no console output added. |
| Invariants at construction | PASS | Unchanged constructor null-guards (lines 111-117). |

### 2.4 Module and File Structure

| File | Baseline lines | Post-change lines | Limit | Status |
|---|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 442 | 476 | 500 | PASS (PA-3: 95% of the cap) |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` | (new) | 215 | 500 | PASS |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (untouched) | 470 | 470 | 500 | PASS (pre-existing 94% of the cap) |
| `.Race.cs` / `.PrimeFaultOrdering.cs` / `.PrimeRegistration.cs` (untouched) | 277 / 77 / 175 | 277 / 77 / 175 | 500 | PASS |

Line counts from `evidence/qa-gates/file-line-counts.md`; the production and new-partial counts were re-verified by reading the files (476 and 215 content lines).

### 2.5 Naming, Docs, and Comments

PASS. XML documentation updated at four places (E1, E2, E3, E4/E5) and verified accurate against the delivered code: the type now holds exactly three `catch` clauses (one boundary, two sink guards) and the remarks say so; `GetPrimeTask` `<returns>` now says the marker is cleared "after that report has returned or thrown"; `CompletePrime` remarks explain why the guard exists and cite the precedent. Comments explain why, not what. Two pre-existing wording residuals are noted in PA-5 and PA-6.

### 2.6 Performance, I/O, Dependencies

PASS. No new dependency; `packages.config` unchanged (Moq 4.21.0, FluentAssertions 8.11.0, MSTest 4.4.1 already present). No I/O introduced.

### 2.7 Interaction with Existing Code

PASS. Repo style matched (CSharpier-stable; `check` exit 0 over 1628 files). No public API change. Existing tests treated as spec: `EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` byte-identical (hash `AA88DC05...F8DB` equals `BASE-HASH-PFO`, diff exit 0) and its test passes; all 28 pre-existing fixture tests pass.

### 2.8 After Making Changes (toolchain loop, docs, next steps)

PASS. One clean pass recorded (section 7). Supporting documents: plan task check-offs complete, issue.md AC check-offs complete, `reduced-audit-handoff.md` written. Next steps: PR authoring by the orchestrator; follow-ups listed in the feature audit.

## 3. Language-Specific Code Change Policy Compliance

**Language-specific policies evaluated:** C# (CLAUDE.md C#1-C#7 and `.claude/rules/csharp.md`). No TypeScript, Python or PowerShell files changed.

### 3.1 Tooling and Baseline (C#1)

| Tool | Command (as recorded) | Result | Evidence |
|---|---|---|---|
| CSharpier format | `dotnet tool run csharpier format .` | exit 0; `REWRITTEN-WRITESET-FILES: 0`; hashes identical before/after | `evidence/qa-gates/csharpier-format.md` |
| CSharpier check | `dotnet tool run csharpier check .` | exit 0; `Checked 1628 files`; no file named | `evidence/qa-gates/csharpier-check-final.md` |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0; ERRORS 0; WARNINGS 0; `SKIP_CORECOMPILE_LINES: 0`; `CSC_OUT_TASKMASTER 2`, `CSC_OUT_TASKMASTER_TEST 2` | `evidence/qa-gates/msbuild-analyzer-final.md` |
| Nullable type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) | exit 0; ERRORS 0; WARNINGS 0; `SKIP_CORECOMPILE_LINES: 0` | `evidence/qa-gates/msbuild-nullable-final.md` |

`/t:Rebuild` was used and the recorded `SKIP_CORECOMPILE_LINES: 0` with non-zero `CSC_OUT_` counts shows the compiler ran for both affected projects, so the analyzer and nullable gates were not vacuous.

### 3.2 Design and Type-Safety (C#2)

PASS. Explicit types at the (internal) boundaries; the file carries no `#nullable` directive (pre-existing; nullable enforcement is per-file opt-in in this repository) and the change introduces no nullable-flow warning. No new `async` surface; `ConfigureAwait(false)` retained.

### 3.3 Classes, Methods, APIs (C#3)

PASS. No new member. Methods remain small; branching shallow (one nested `try` per site).

### 3.4 Error Handling (C#4)

See section 2.3. PASS as a documented boundary catch; PA-2 records the residual.

### 3.5 Module and File Structure (C#5)

PASS (section 2.4). `internal` surface unchanged.

### 3.6 Naming, Docs (C#6)

PASS (section 2.5).

### 3.7 Dependencies and Analyzer Configuration (C#7)

PASS. No suppression added; no `.editorconfig` change; no `<Analyzer Include>` change.

## 4. Language-Specific Unit Test Policy Compliance

### 4.1 Framework Selection (CUT1)

PASS. `using Microsoft.VisualStudio.TestTools.UnitTesting;` (line 4), `[TestMethod]` on all four tests; the `[TestClass]` attribute is on the primary partial declaration (`EngineToggleStateCoordinatorTests.cs` line 22). No xUnit/NUnit.

### 4.2 Libraries and Conventions (CUT2)

PASS. Moq via the shared `Harness` strict mock (`SetupSequence`, `Setup(...).ThrowsAsync`, `Verify(..., Times.Exactly(2), reason)`); FluentAssertions for every assertion (`Should().NotBeSameAs`, `.BeSameAs`, `.ContainSingle`, `.BeAssignableTo<OperationCanceledException>`, `.NotThrowAsync`, `.BeEmpty`, `.Equal`); no MSTest `Assert.*` call.

### 4.3 Toolchain Command Selection (CUT3)

PASS with recorded substitution at step 4. Steps 1-3 ran the literal CLAUDE.md commands. Step 4 did not run `Invoke-MSTestWithCoverage.ps1` directly: the plan's recorded stall probe (`STALL-PROBE: REPRODUCES`, `evidence/baseline/stall-probe.md`, one shell-icon test failing with an invalid Win32 icon handle) selected the DIRECT route, which issues the runner's own inner `dotnet-coverage collect ... -- vstest.console.exe` invocation with the four UtilitiesCS.Test shell-icon classes excluded and the hang-blame switch appended, then applies the runner's own threshold and projection helpers. The committed forms (first-party line, JaCoCo package projection, trx-derived summary) are the ones CLAUDE.md requires. The excluded classes are executed by CI. Recorded as gap G-1 (non-blocking, environmental, pre-existing on this workstation and identical to the #942/#944/#940 reviews).

## 5. Test Coverage Detail

| Item | Value | Source |
|---|---|---|
| Baseline first-party | lines 56109/65750 (85.34%), branches 13597/17054 (79.73%) | `coverage-baseline.md`; Cobertura root re-read |
| Post-change first-party | lines 56120/65760 (85.34%), branches 13597/17054 (79.73%) | `coverage-summary.md`; Cobertura root re-read |
| `TaskMaster` package (JaCoCo projection) | LINE missed 802 / covered 2457 -> missed 802 / covered 2467; BRANCH 211/517 unchanged | both projections |
| Coordinator class | 157/157 -> 167/167 lines; 37/38 -> 37/38 branches | Cobertura `<class>` node re-read |
| Changed executable lines | 12 with a line element, 0 uncovered | `CMD-CHANGED-LINES` output |
| New catch arms | 2 arms, 3 elements each, 0 uncovered | COMPARISON rows; Cobertura lines 191/192/194 and 410/411/413 re-read, all `hits="1"` |
| Test counts | 7332 -> 7336 total, all passed, 0 skipped (derived) | trx-derived summaries |

New code added by this change consists of two guard blocks inside two existing methods (no new class, module or method); both methods are at 100% line coverage post-change, which meets the CLAUDE.md UT2 ">= 90% for new methods" target and the rules' 85%/75% floors.

## 6. Test Execution Metrics

| Run | Scope | Result | Evidence |
|---|---|---|---|
| P0-T13 baseline fixture | 28 tests | 28 passed | `evidence/baseline/coordinator-tests-baseline.md` |
| P0-T14 baseline suite | 7332 tests | 7332 passed | `evidence/baseline/coverage-baseline.md` |
| P1-T6 fail-before (expect-fail) | 32 tests, base production file | 28 passed, 4 failed (exactly the 4 new tests) | `evidence/regression-testing/throwing-sink-fail-before.md` |
| P1-T12 pass-after | 32 tests | 32 passed | `evidence/regression-testing/throwing-sink-pass-after.md` |
| P2-T7 final fixture | 32 tests (assembly rebuilt by the nullable gate) | 32 passed | same file, `FINAL-FIXTURE-RUN:` section |
| P2-T8 final suite with coverage | 7336 tests | 7336 passed, 0 failed, exit 0, both floors MET | `evidence/qa-gates/coverage-summary.md` |

Every vstest run recorded `SEQUENCE_FILES: 0` (no hang dump) and `STRAY_TEST_PROCESSES: 0` before the run. The issue 780 flake (`TryAddValuesAsync_UpdatesExistingValue`) did not occur; no re-run was taken.

## 7. Code Quality Checks

| Check | Command / Method | Result |
|---|---|---|
| Format (CSharpier) | `dotnet tool run csharpier check .` | PASS (exit 0, 1628 files) |
| Lint (.NET analyzers) | analyzer Rebuild, section 3.1 | PASS (0 errors, 0 warnings, `WRITESET_DIAGNOSTIC_LINES: 0`) |
| Type check (nullable) | nullable Rebuild, section 3.1 | PASS (0 errors, 0 warnings) |
| Tests | fixture and suite runs, section 6 | PASS |
| Confidentiality masking scan | Grep of the feature folder for drive-letter paths, `\Users\`, account and machine tokens (this review) plus the executor's `HYGIENE-SWEEP` (`ACCOUNT_HITS=0 MACHINE_HITS=0 DRIVE_USERS_HITS=0`, 42 files) | PASS (only `https://` URL schemes and one XML namespace matched; the one absolute path in a trx message was replaced by `REDACTED-PATH` before transcription) |
| Suppression scan (added lines) | Read of both changed `.cs` files for `#pragma`, `[SuppressMessage]`, `[ExcludeFromCodeCoverage]`, `// ReSharper disable` | PASS (none added) |
| Workflow change scan | diff footprint for `.github/**` | PASS (no workflow changed) |
| File-size audit | section 2.4 | PASS |
| Coverage exclusion policy | `coverage.config` unchanged; no `exclude` entry added | PASS |

Toolchain pass count: 1 (clean; no step rewrote a file or failed), recorded in `evidence/qa-gates/toolchain-final-pass.md`.

## 8. Gaps and Exceptions

| ID | Severity | Gap / Exception | Disposition |
|---|---|---|---|
| G-1 | Non-blocking | Local coverage route was DIRECT with four UtilitiesCS.Test shell-icon classes excluded (pre-existing workstation stall/failure, `STALL-PROBE: REPRODUCES`). CI executes them. | Accepted; environmental; same as #942/#944/#940. The repo-wide gate of record is the PR CI run. |
| G-2 | Non-blocking | `quality-tiers.yml` is absent at the repository root (Glob confirmed), so tier-dependent gates (property tests, mutation score) cannot be evaluated for `TaskMaster`. Uniform gates (format, lint, type, coverage floors) were evaluated. | Pre-existing repository condition; not introduced by this branch. |
| G-3 | Non-blocking | `artifacts/pr_context.summary.txt` / `.appendix.txt` do not exist in the review worktree and could not be regenerated (no shell or MCP tool in this review). Scope was derived from the caller-supplied BASE-SHA and head, the committed `git diff --name-status BASE-SHA` listing in `footprint-scope.md`, and the worktree reflog (head `ec312202e`, BASE-SHA `2e6ce2cab` as the merge entry). | Documented assumption; the three sources agree. |
| G-4 | Non-blocking | Canonical C# coverage path `artifacts/csharp/coverage.xml` not populated; the runner writes `coverage/` and the raw document must not be committed. Verification used the retained post-processed Cobertura documents directly. | Pre-existing convention mismatch; see 1.2.2. |
| G-5 | Non-blocking | This review's timestamp label (`19-30`) is assigned, not measured (no clock available). Executor labels were cross-checked against the Cobertura root epochs (`1790890899` -> 17:41 local, label 17-42; `1790892471` -> 18:07 local, label 18-09) and the reflog (`1790893108` -> 18:18 local for the final commit) and are consistent to the minute. | Disclosed. |
| X-1 | Accepted exception | Two broad `catch (Exception)` discards (section 2.3). | Documented boundary catch with precedent; accepted. |
| X-2 | Accepted ruling | P2-T11 / P2-T13 literal untracked/porcelain clauses re-anchored to the paired BASE-SHA diff after per-phase commits (coordinator ruling 19-00, transcribed into `reduced-audit-handoff.md` and the plan Revision log). | Accepted as instructed; not a finding. |

## 9. Summary of Changes

| File | Change | Lines (base -> head) |
|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `CompletePrime`: `_logError` call wrapped in `try`/`catch (Exception)` with discard comment; `TryRemove` stays after the block. `HandleToggleClickAsync`: `_logError` call inside the existing `catch (Exception ex)` wrapped the same way; toggle fault still not rethrown. XML docs updated: method summary (E1), `GetPrimeTask` returns (E2), `StartObservedPrime` remarks (E3), `CompletePrime` summary/remarks (E4), `HandleToggleClickAsync` remarks (E5). | 442 -> 476 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` | New fifth partial: `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared`, `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport`. No harness change. | 0 -> 215 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | One `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />` at line 362, after the PrimeRegistration entry; CRLF preserved. | +1 |
| Feature folder | plan check-offs, issue.md AC check-offs (7), 39 evidence artifacts, research record, preflight clearance | docs only |

## 10. Compliance Verdict

**PASS.** Blocking findings: 0. Non-blocking findings: 6 (PA-1 to PA-6). Remediation inputs: not produced.

### Non-blocking findings

| ID | Severity | File / location | Rule | Finding | Evidence |
|---|---|---|---|---|---|
| PA-1 | Non-blocking (Minor) | `EngineToggleStateCoordinator.cs` lines 187-194 and 406-413 | General 1.2 Reusability (avoid copy-paste) | The same 8-line guard block is duplicated at two sites. The plan's D-1 deliberately rejected a shared private helper as beyond the minimal fix; defensible for a bugfix. A `SafeLog`-style private helper mirroring `RibbonCommandBoundary.SafeLog` would remove the duplication and is the natural vehicle when the third sink site (PA-4) is addressed. | Read of both blocks; plan D-1. |
| PA-2 | Non-blocking (Informational) | same two sites | CLAUDE.md C#4 / General 3 | The discarded sink exception is observable nowhere once swallowed. This matches the `RibbonCommandBoundary.SafeLog` precedent and is accepted (section 2.3); a `System.Diagnostics.Trace` fallback was not required and is not recommended as part of this fix. | Read of `RibbonCommandBoundary.cs` 96-113. |
| PA-3 | Non-blocking (Informational) | `EngineToggleStateCoordinator.cs` (476 lines); `EngineToggleStateCoordinatorTests.cs` (470 lines, untouched) | General 4.1 (500-line limit) | Both files are within 30 lines of the cap. The next non-trivial change to either will need a split (e.g., the four message builders to a partial). | `file-line-counts.md`; re-counted by reading. |
| PA-4 | Non-blocking (Minor) | `EngineToggleStateCoordinator.cs` line 177 (`_notifyUnavailable(...)`) and the remark at lines 169-171 | General 3 / C#4; C#6 (docs synchronized with behaviour) | A third injected sink call, `_notifyUnavailable`, sits in the engines-unavailable refusal path outside the boundary `try`. A throwing notify sink still escapes `HandleToggleClickAsync` into the `async void` Office handler, so the (pre-existing, now extended) remark "This method therefore never throws" is not strictly true for that path. Pre-existing; outside AC6 (which names the `logError` sink only) and outside the six edit windows; same defect class as this issue. Recommended follow-up, not a defect of this change. | Read of lines 173-196. |
| PA-5 | Non-blocking (Informational) | `EngineToggleStateCoordinator.cs` line 254 | C#6 (docs) | `GetPrimeTask` `<returns>` still opens "The prime task, or `Task.CompletedTask`" although the value is the registration marker (#944 follow-up FU-3). Outside edit window E2 (which replaced lines 246-248 of the base only). Pre-existing. | Read of lines 253-260. |
| PA-6 | Non-blocking (Informational) | `EngineToggleStateCoordinatorTests.ThrowingSink.cs` lines 139-180 | UT3 (test documents intent) | In `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` the `RanToCompletion` assertion also holds before the fix (the #944 `finally` completes the marker regardless); the discriminating assertion is the final `BeSameAs(Task.CompletedTask)`. This is disclosed in the plan (D-3) and in the test summary ("Without the fix the marker stays registered after the handle completes"); AC3's wording is satisfied. No action required. | `throwing-sink-fail-before.md` MESSAGE line for this test. |

## Appendix A: Test Inventory

New tests (all in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`, class `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests`):

| Test | AC | Fail-before | Pass-after (P1-T12 / P2-T7) |
|---|---|---|---|
| `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime` | AC1, AC4 | Failed (`Expected invocation on the mock exactly 2 times, but was 1 times`) | Passed / Passed |
| `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime` | AC1, AC4 | Failed (same) | Passed / Passed |
| `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` | AC3, AC4 | Failed (`found System.Threading.Tasks.Task`1[System.Boolean]` where `Task.CompletedTask` expected) | Passed / Passed |
| `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` | AC6, AC7 | Failed (`found System.InvalidOperationException: sink failed`, stack at base line 184) | Passed / Passed |

Pre-existing tests exercised as regression guards (unchanged files): `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` (AC2, `.PrimeFaultOrdering.cs`), `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` (AC6, main fixture), `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` (`.Race.cs`), the three `.PrimeRegistration.cs` tests, `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime`, `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` — all Passed in every run. Fixture total 28 -> 32; suite total 7332 -> 7336.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. Coverage test run (DIRECT route, recorded substitution for `scripts/vscode/Invoke-MSTestWithCoverage.ps1`): `dotnet-coverage collect --output coverage\final-947.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-947.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\947\final" "/Logger:trx;LogFileName=final-947.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, post-processed with the runner's own helpers (`CMD-COVERAGE-POST`).
5. Fixture-scoped runs: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" ...` (P0-T13, P1-T6, P1-T12, P2-T7).

All commands were run by the executor and recorded in the cited evidence artifacts; none was re-run by this review.
