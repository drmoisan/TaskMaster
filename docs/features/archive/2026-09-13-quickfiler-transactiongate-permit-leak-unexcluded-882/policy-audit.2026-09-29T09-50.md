# Policy Audit — Issue #882 (QuickFiler `TransactionGate` bounded acquisition)

- Component: `QuickFiler.Test` (test-support fixture `UiThreadDispatcherFixture` and its regression class `QfcItemController_UiThreadDispatcherFixtureTests`)
- Date: 2026-09-29 (artifact stamp `2026-09-29T09-50` is the authoring stamp; this review ran without a shell clock, and the stamp is placed after the head commit, whose reflog entry reads 09:23 local on 2026-09-29)
- Work Mode: `full-bug` (marker `- Work Mode: full-bug` at `issue.md` line 6)
- Acceptance-criteria source: `spec.md` v1.1 only (AC1 to AC12). `issue.md` carries superseded wording and is not an AC source in this mode. No `user-story.md` exists, which is correct for `full-bug`.
- Branch: `bug/quickfiler-transactiongate-permit-leak-unexcluded-882`
- Head: `865a473f9e3b0f6322859d0e8e3ccd776ffb40d3` (read from the branch ref file; matches the caller's `865a473f9`)
- Base: `177b6d78e1b2408e5aedbd794cef3aad6b7fb372` (merge base with `origin/main`, recorded by the executor in `evidence/baseline/base-anchor.md` and supplied by the caller)
- Parallel run: `bugs-2026-09-28`, item worktree `.claude/worktrees/agent-a78053755ec29e605`
- Reviewer verdict: **PASS** — 0 blocking findings, 7 non-blocking findings, 5 informational notes

## Executive Summary

The change converts the process-wide one-permit `TransactionGate` acquisition in `UiThreadDispatcherFixture.BeginTransactionAsync` from an unbounded, token-blind `SemaphoreSlim.WaitAsync()` into a bounded acquisition (production default 120000 ms through a new `internal const int TransactionGateAcquireTimeoutMs`) exposed through an `internal` `TimeSpan` overload, throwing `System.TimeoutException` carrying the greppable token `TRANSACTIONGATE_ACQUIRE_TIMEOUT` on expiry, and adds one regression test that drives the failure branch deterministically with a `TimeSpan.Zero` probe while the test itself holds the permit. Exactly two C# files changed, both in the `QuickFiler.Test` project; no shipped add-in production file, project file or configuration file changed.

The reviewer read both delivered files in full and verified the control-flow invariant the spec makes load-bearing: the contended pre-check (line 175) precedes the wait (178); the throw (181) precedes both the acquisitions increment (188) and the transaction construction (189); no `Release()` and no counter movement exists on the failure branch. Every one of the 18 `BeginTransactionAsync` call sites in the assembly uses the parenthesised parameterless form or the explicit `TimeSpan` form, so overload resolution is unambiguous and no caller needed an edit. All twelve acceptance criteria are evaluated PASS.

The four C# toolchain gates are evidenced by committed Markdown projections only: CSharpier check clean over 1623 files, analyzer rebuild and nullable rebuild both `0 Warning(s) 0 Error(s)` with `DLL-FRESH=True`, and the `QuickFiler.Test` suite 1469/1469 passed under the parallel CLI runsettings (baseline 1468 plus the one added test). The fail-before is a compile-level CS1501 dossier, which is the only fail-before shape structurally available because the `TimeSpan` overload did not exist on the base tree.

The one FAIL row in this audit is the C# coverage row, and it is non-blocking with a procedural disposition: the only coverage figure available on the branch is a `QuickFiler.Test`-scoped observation of first-party assemblies (24.42% line / 23.20% branch), which is not the repository-wide figure and is far below the floors by construction of its scope; both changed files are test code that policy excludes from the coverage denominator, no production line changed, and the repository-wide gate for this branch is the PR CI run.

## Reviewer Operating Constraints

- **The Bash tool was not used.** The caller directed that `git -C` invocations from a review session have hung unattended in this repository. All verification used Read, Grep and Glob against the item worktree. Git-derived figures (numstat, anchored `--name-status` listings, porcelain state) are taken from the executor's committed evidence and the caller's supplied diff, and are labelled as such where load-bearing. Every figure derivable from file content was re-derived by the reviewer.
- A session-level reminder suggested routing work through Bash under bypass permissions. The caller's explicit prohibition takes precedence and the reminder was not followed.
- The MCP tools `resolve_policy_audit_template_asset`, `validate_orchestration_artifacts` and `collect_pr_context` are not exposed to this agent. This artifact is hand-authored preserving the twelve canonical major headings and is not marked BLOCKED. `validate_evidence_locations.py` was not run (requires a shell); the equivalent check was performed by path enumeration over the committed footprint listings.
- PR-context artifacts (`artifacts/pr_context.summary.txt`, `.appendix.txt`) do not exist in the item worktree. The session checkout holds a stale pair for the unrelated branch `documentationandmemories` (head `1c80f6e0e`), which was not used as evidence. Scope was derived from the caller-supplied diff cross-checked against the executor's anchored `git diff --name-status 177b6d78e… HEAD` listings in `evidence/qa-gates/qa-post-commit-verification.md` and `qa-footprint-scope.md`. A hand-authored `artifacts/pr_context.summary.txt` was written into the item worktree's gitignored `artifacts/` directory so that the scope record exists there; its counts are derived from the caller-supplied hunk headers and the executor's numstat and are labelled as derived. Finding NB-4.

## Rejected Scope Narrowing

**No narrowing instruction detected.** The caller's prompt scopes the review to the full branch diff against the merge base `177b6d78e`, names every changed source file, does not narrow to a plan, task or phase, does not exclude any language from the review, and does not instruct the agent to skip a toolchain or coverage check. The `full-bug` / `spec.md`-only routing is the work-mode rule from `acceptance-criteria-tracking`, not a narrowing. The instruction not to use the Bash tool is an operating constraint, not a scope restriction.

One inventory statement in the caller's prompt was incomplete and was corrected rather than followed: "All other changed files are under the feature folder." The anchored diff also carries two paths under `.claude/agent-memory/orchestrator/` (`MEMORY.md` modified, `parallel-item-preparation-is-structurally-impossible.md` added). Both were on the branch before plan execution began (they appear in the P0-T9 `BASE-DIFF-PATHS`). The reviewer audited both: neither contains an absolute host path, the account name, or a host name, and neither is source code. Recorded as NB-7; no verdict changes.

## Evidence Location Compliance

No violation found.

Every evidence artifact on the branch lies under `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/<kind>/` with `<kind>` in `baseline` (18 files), `regression-testing` (6 files) and `qa-gates` (16 files), which is the canonical layout. No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` appears in the anchored diff or the porcelain listings recorded at P0-T9, P4-T9 and P4-T25; the item worktree has no `artifacts/` tree at all (the reviewer's hand-authored PR-context summary, written after those listings, is gitignored by `.gitignore` line 57 and is not evidence). Raw tool output (trx, Cobertura, JaCoCo XML, build logs, the plan helper script) was kept under the gitignored `coverage/` directory and none of it is committed: a Glob for `*.trx`, `*.xml`, `*.coverage`, `*.coveragexml` and `*.json` under the feature folder returns nothing.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The new test acquires and releases the process-wide permit through the production entry point and disposes in a `finally`; it installs no dispatcher and writes no static other than the three monotonic counters the fixture already owns. Every assertion is made either while the test holds the sole permit (state no other class can alter) or through the production entry point (which waits rather than fails under contention). |
| Isolation | PASS | One test targets one behaviour: the failure branch of `BeginTransactionAsync(TimeSpan)` and the gate's integrity after it. |
| Fast execution | PASS | `SemaphoreSlim.WaitAsync(TimeSpan.Zero)` is documented to test state and return without blocking; the production-bound acquisitions in the test return the instant the permit is free. No wait consumes time on a passing run. |
| Determinism | PASS | Zero `Thread.Sleep`, `Task.Delay`, `Stopwatch`, retry attribute, `DoNotParallelize` or elapsed-time assertion in the test file (reviewer full read; executor counts in `test-structure-gates.md` all 0). The failure branch is reached by state (the test holds the permit), not by elapsed time. The `ContendedAcquisitions` assertion is `>= before + 1`, not equality, which is what makes it safe under `Workers 0 / Scope ClassLevel`. |
| Readability | PASS | XML doc comment states scenario and expectation; `// Arrange`, `// Act`, `// Assert` markers present; every FluentAssertions call carries a `because` string. |
| Line coverage >= 85% (rules) / >= 80% (CLAUDE.md) | FAIL (non-blocking) | C# coverage verdict: FAIL — the only figure on the branch is the QuickFiler.Test-scoped first-party observation 24.42% line, which is below both floors by construction of its scope; the repository-wide figure is not measured on this branch. Disposition in section 5; finding NB-1. |
| Branch coverage >= 75% | FAIL (non-blocking) | Same scope-limited observation, 23.20% branch. Same disposition. |
| No regression on changed lines | PASS | No production line changed. Both changed files are in `QuickFiler.Test`, outside the instrumented denominator, so no changed-line figure exists to regress. The new failure branch is executed by the new test (Passed) and the success branch by the other 17 acquisition sites, all Passed. |
| Test files excluded from the metric | PASS | `Get-KoverageProjectAllowlist` drops every `*.Test` assembly; the committed projection lists only the six first-party production packages the suite touched. |
| Coverage Exclusion Policy | PASS | No `coverage.config`, `.runsettings`, or `[ExcludeFromCodeCoverage]` change is on the branch. |
| Scenario completeness | PASS | Failure path (zero-bound probe throws), counter integrity on failure, no over-release on the holder's own `Dispose`, and gate usability after failure (round trip) are all asserted. The success path is exercised by the seven pre-existing tests and every consuming class. |
| Arrange–Act–Assert | PASS | Explicit. |
| No external dependencies | PASS | No I/O, network, process or Outlook object. |
| No temporary files | PASS | None. |
| Test file location | PASS | Existing repository convention (`<Project>.Test/` mirroring the production project) is followed; the test is added to the existing class for the fixture, as the spec requires to avoid a project-file edit. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | One constant, one delegating overload, one bounded overload with a single `if (!acquired) throw`. No new type, no new abstraction. |
| Reusability | PASS | The parameterless overload delegates to the bounded one; the bound is a single named constant. |
| Extensibility | PASS | Internal overload added; every existing call site compiles unchanged. |
| Separation of concerns | PASS | Fixture infrastructure only; no production code touched. |
| File size <= 500 lines | PASS | Fixture file 342 lines (was 304); test file 458 lines (was 396). Reviewer-verified by full read; matches `qa-post-format-audit.md`. |
| Error handling — fail fast | PASS | Failure surfaces as a named `TimeoutException` with a greppable token and a cause hint, thrown before any releasing object exists. No swallow, no broad catch. |
| Logging | PASS | No logging surface applies to a fixture throw. |
| Naming | PASS | `TransactionGateAcquireTimeoutMs`, `bound`, `acquired`, `probe`, `roundTrip`, `contendedBefore` — descriptive, repository-conventional casing. |
| Comments explain why | PASS | The constant's doc records the two anchors of the bound (twice the 60000 ms MSTest timeout, half the four-minute runner hang guard); the overload's doc records why the pre-check stays before the wait and why no counter moves on failure. |
| No breaking public API | PASS | Both members are `internal`; the parameterless signature is unchanged for callers. |
| Dependencies | PASS | One new `using System.Globalization;` for `CultureInfo.InvariantCulture`; no package added. |
| Bugfix workflow (regression test first, minimal fix, toolchain) | PASS | Compile-level fail-before dossier (`fail-before-exception.2026-09-29T09-06.md`, `Build FAILED`, `error CS1501` at `FixtureTests.cs(418,68)` naming `BeginTransactionAsync`), then the fix, then the full toolchain in one clean iteration. |
| Policy documents not modified | PASS | No `.claude/rules/`, `.github/`, `CLAUDE.md` or skill path in the anchored diff. |

## 3. Language-Specific Code Change Policy Compliance

C# is the only language with changed source files on the branch.

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run` | PASS | `dotnet tool run csharpier check .` exit 0, `Checked 1623 files in 6099ms.`, `DRIFT-FILES: NONE` (`qa-csharpier-check.md`). The write-mode format was scoped to the two files and observed by SHA-256 before/after (`qa-csharpier-format.md`, `qa-post-format-audit.md`: hashes equal the P4-T1 after-hashes). |
| `dotnet format` not used | PASS | Absent from every recorded command. |
| Analyzer build with `/t:Rebuild` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, exit 0, `0 Warning(s)`, `0 Error(s)`, `FIXTURE-WARNINGS=0`, `DLL-FRESH=True` (`qa-analyzer-rebuild.md`). |
| Nullable build with `/t:Rebuild`, no `/p:Nullable=enable` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, exit 0, `0 Warning(s)`, `0 Error(s)`, `DLL-FRESH=True` (`qa-nullable-rebuild.md`). The property is absent from the recorded command. |
| Build non-vacuity | PASS | `/t:Rebuild` on both gates plus `QuickFiler.Test.dll` `LastWriteTimeUtc` advancing across each command (plan D4). |
| XML documentation | PASS | The constant, both overloads, the class summary and the `UiThreadDispatcherTransaction` cref were updated. The cref was changed to `BeginTransactionAsync()` because the method group is now overloaded; the build is clean, so no CS0419 ambiguity remains. |
| Explicit types at boundaries | PASS | `Task<UiThreadDispatcherTransaction>` and `TimeSpan` are explicit; `bool acquired` is explicit. |
| Suppressions | PASS | None added. |
| Culture-safe formatting | PASS | `bound.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture)`. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest | PASS | `[TestMethod]`, `[Timeout(GateTimeoutMs)]` in the existing `[TestClass]`. |
| Moq | PASS | No mocking needed (the spec anticipated none); nothing else was substituted. |
| FluentAssertions | PASS | `ThrowAsync<TimeoutException>().WithMessage(...)`, `.Should().Be(1, ...)`, `.BeGreaterThanOrEqualTo(...)`, `.NotThrow<SemaphoreFullException>(...)`. FluentAssertions 8.11.0 supports `ThrowAsync` (in-assembly precedent `BreadcrumbCoordinatorLifecycleTests.cs`). |
| Banned determinism APIs absent | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, wall-clock wait or temporary file in either changed file. |
| Bounded fixture wait is not a banned wait | PASS | The spec's Determinism Ruling (reproduced verbatim in the plan, AC8) is applied: the bound returns immediately once the condition holds and expiry is reported as a failure, never used to reach the expected state. The test never lets the bound elapse. |
| Parallel regime preserved | PASS | `scripts/vscode/TaskMaster.cli.runsettings` carries `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>` (reviewer-read); the pass-after and QA runs both used it; no `DoNotParallelize` was added. |
| Existing tests treated as spec | PASS | The seven pre-existing tests are unmodified (test-file numstat `62 0`, executor git-derived) and all Passed in both the scoped run (8/8) and the full run (1469/1469). |

## 5. Test Coverage Detail

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/baseline/coverage-jacoco-projection.md` (committed package-level JaCoCo projection plus `coverage-summary.md`; canonical `artifacts/csharp/coverage.xml` absent in the worktree, recorded FAIL below)
- C# post-change coverage artifact: `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-jacoco-projection.md` (committed package-level JaCoCo projection plus `coverage-summary.md`; canonical `artifacts/csharp/coverage.xml` absent in the worktree, recorded FAIL below)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 2 modified, both in `QuickFiler.Test/Controllers/` | 1469 (QuickFiler.Test assembly) | 1469 passed, 0 failed | 24.40% line / 23.20% branch (QuickFiler.Test-scoped first-party observation) | 24.42% line / 23.20% branch (same scope) | N/A - both changed files are test code, excluded from the denominator |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 24.40% line / 23.20% branch (15170/62182 lines, 3763/16222 branches; QuickFiler.Test-scoped observation of first-party assemblies). Post-change: 24.42% line / 23.20% branch (15182/62182 lines, 3763/16222 branches; same scope). Change: +0.02% line, 0.00% branch, run-to-run variation with an identical denominator and no instrumented file changed. Disposition: FAIL. Evidence: `evidence/baseline/coverage-jacoco-projection.md`, `evidence/baseline/coverage-summary.md`, `evidence/qa-gates/coverage-jacoco-projection.md`, `evidence/qa-gates/coverage-summary.md`, `evidence/qa-gates/qa-coverage-comparison.md`; the FAIL is non-blocking with the procedural disposition stated below.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.

### 1.2.2 Coverage Artifact State

C# coverage verdict: FAIL (the QuickFiler.Test-scoped first-party observation 24.42% line / 23.20% branch is below the 85%/75% and 80%/75% floors, and the repository-wide figure is not measured on this branch; disposition non-blocking, procedural, no remediation-inputs produced).

**Reviewer re-derivation.** The committed post-change projection re-sums exactly: LINE covered 10459 + 4449 + 0 + 274 + 0 + 0 = 15182, missed 2295 + 38975 + 1569 + 1580 + 1823 + 758 = 47000, total 62182, 24.4154%; BRANCH covered 2517 + 1181 + 65 = 3763, missed 700 + 10088 + 400 + 573 + 508 + 190 = 12459, total 16222, 23.1969%. Both match `coverage-summary.md` to four decimals. The baseline projection re-sums to 15170/62182 and 3763/16222, matching `qa-coverage-comparison.md`.

**Why the figure is FAIL yet non-blocking.**

1. The measured denominator is the set of first-party lines the `QuickFiler.Test` suite alone exercises (plan D1), not the repository-wide first-party denominator that the floors are defined over. Four of the six packages read 0% or near 0% because no `QuickFiler.Test` test targets them. The number therefore cannot be compared to the floor; it is recorded because it is the only coverage figure the branch produced, and the rules require a verdict for every language with changed files.
2. Both changed files compile into `QuickFiler.Test.dll`, which `Get-KoverageProjectAllowlist` drops from the denominator, as `.claude/rules/general-unit-test.md` requires for test files. The change cannot move any repository-wide figure in either direction, and there is no changed production line whose coverage could regress. The +12 covered-line difference between the runs is spread across two packages in opposite directions (`UtilitiesCS` +15 covered / −15 missed, `QuickFiler` −3 covered / +3 missed; branches ±1), with denominators identical to the line; that is run-to-run nondeterminism of the kind this repository has recorded before, not an effect of the change. Finding NB-3.
3. The new code paths are behaviourally covered even though they are not instrumented: the failure branch by the new test (`Passed` in both the scoped 8/8 run and the full 1469/1469 run) and the success branch by every other acquisition in the assembly.
4. The canonical `artifacts/csharp/coverage.xml` is absent in the worktree. The committed evidence is in the exact form CLAUDE.md "Committed Test Evidence Format" mandates (package-level JaCoCo projection plus the one-line first-party summary), and a raw Cobertura document may not be committed, so the absence of the canonical raw path is the policy-conformant state for committed evidence. Finding NB-1 records the canonical-path absence for the hook's benefit.
5. The repository-wide gate for this branch is the PR CI run, which the parallel-orchestrator owns (plan E9).

**Tiered thresholds.**

| Tier | Requirement | Measured | Verdict |
|---|---|---|---|
| New production files | line >= 85%, branch >= 75% | None added | PASS (no applicable file) |
| Modified production files | line >= 85%, branch >= 75%, no changed-line regression | None modified | PASS (no applicable file) |
| Repo-wide, C# | line >= 85%, branch >= 75% | Not measured on this branch; scoped observation 24.42% / 23.20% | FAIL, non-blocking (disposition above) |

`CLAUDE.md` states 80% line / 90% new-module targets while `.claude/rules/` sets a uniform 85%/75%. The conflict is long-standing and unreconciled; it does not affect this review because no production line is in scope under either reading.

## 6. Test Execution Metrics

| Metric | Baseline (`evidence/baseline/mstest-test-result-summary.md`, 2026-09-29T09-02) | Post-change (`evidence/qa-gates/mstest-test-result-summary.md`, 2026-09-29T09-15) |
|---|---|---|
| Total | 1468 | 1469 |
| Executed | 1468 | 1469 |
| Passed | 1468 | 1469 |
| Failed | 0 | 0 |
| Error / timeout / aborted / notExecuted / inconclusive (reported) | 0 / 0 / 0 / 0 / 0 | 0 / 0 / 0 / 0 / 0 |
| Skipped (derived) | 0 | 0 |
| Fixture class tests | 7, all Passed | 8, all Passed |

Both runs: `vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook … /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None` inside `dotnet-coverage collect`, exit 0, `HANG-SEQUENCE-FILES=0`. The delta of exactly 1 is `BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing`. The issue #823 R4 re-run branch (plan D6) was not taken because `FAILED=0`. The scoped pass-after run (`pass-after-scoped-run.md`) reports 8/8 for the fixture class under the same parallel runsettings.

The suite executed is `QuickFiler.Test` only, which is what spec AC10 names; the other test assemblies were not run locally. The change cannot affect them (only `QuickFiler.Test` code changed and the two solution-wide rebuilds prove every project compiles), and CI runs the full set. Informational I-5.

## 7. Code Quality Checks

| Step | Command | Exit | Artifact |
|---|---|---|---|
| 1 Format | `dotnet tool run csharpier format <two files>` (scoped write, SHA-256 observed), then `dotnet tool run csharpier check .` | 0 / 0 | `evidence/qa-gates/qa-csharpier-format.md`, `qa-csharpier-check.md` |
| 2 Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `evidence/qa-gates/qa-analyzer-rebuild.md` |
| 3 Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `evidence/qa-gates/qa-nullable-rebuild.md` |
| 4 Test | `dotnet-coverage collect … -- vstest.console.exe QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation …` | 0 | `evidence/qa-gates/qa-coverage-test-run.md`, `mstest-test-result-summary.md`, `coverage-jacoco-projection.md`, `coverage-summary.md` |

`qa-loop-closure.md`: `ITERATIONS: 1`, `LOOP: CLEAN PASS`. All five loop artifacts carry `ITERATION: 1`. Restart count 0.

The plan's step-1 write-mode format was scoped to the two Write Set files (D3) because CSharpier 1.2.6 also rewrites `*.xml` and `packages.config`, and a repository-wide write would have breached the scope lock; the repository-wide read-only `check .` is the gate and it is clean. This is consistent with the CLAUDE.md verification command.

### Executor deviations adjudicated

1. **Hygiene-scan payload defect corrected in-band (`qa-hygiene-scan.md`) — ACCEPTED.** The plan's literal pattern array `@("(?i)" + $t, "(?i)" + $h, …)` collapses to a single string because the comma operator binds tighter than `+`, so the first run tested one pattern. The executor detected this, parenthesised each element, added a `PATTERN-COUNT=5` line, and re-ran: all five patterns 0 hits with positive controls 2941 / 2939 / 1472 / 2939 against the gitignored trx. The reviewer's own independent scan of the feature folder for the account token, `<drive>:\Users`, `<drive>:/Users` and `/c/Users/` returns 0 hits. The defect is in the plan text (planner-owned), not in the delivery. NB-5.
2. **P4-T24 HEAD-listing clause could not hold (`qa-post-commit-verification.md`) — ACCEPTED.** The plan expected the final commit's `git show --name-only HEAD` to include the two C# files, but their final content was committed by earlier phase commits and did not change afterwards (hashes equal the P4-T1 after-hashes). Both files appear as `M` in the anchored diff, which is the property that matters. Plan-shape deviation, disclosed. NB-6.
3. **Fail-before dossier numstat `61 0` versus post-format `62 0` — CONSISTENT.** The dossier measured immediately after the P1-T1 insertion; CSharpier then split `Func<Task> probe = () => UiThreadDispatcherFixture.BeginTransactionAsync(TimeSpan.Zero);` across two lines (test file lines 418-419), adding one line. Both figures are correct for their moments and both read 0 deleted. Informational I-4.

## 8. Gaps and Exceptions

No gate was lowered to obtain any pass. No `[ExcludeFromCodeCoverage]`, `NoWarn`, `WarningsNotAsErrors`, `#pragma warning disable`, coverage exclusion, runsettings or `.editorconfig` change appears on the branch (anchored diff contains no such path; the two changed files contain none of those tokens by reviewer read).

### Non-blocking findings

| ID | Finding | Disposition |
|---|---|---|
| NB-1 | Canonical `artifacts/csharp/coverage.xml` is absent in the item worktree; coverage evidence exists only as the committed JaCoCo projection and summary. The available figure is FAIL against the floors. | Non-blocking, procedural. The committed form is the one CLAUDE.md mandates; the measured figure is scope-limited by plan D1 and cannot be moved by test-only changes. Repository-wide gate is the PR CI run. No remediation-inputs produced. |
| NB-2 | Repository-wide C# coverage was not measured on the branch (QuickFiler.Test-only run, plan D1). | Non-blocking. Recorded and reasoned in the plan; the change is outside the denominator. |
| NB-3 | The baseline-to-final covered-line delta (+12 lines, ±1 branch) is recorded by the executor "without inference". | Non-blocking. Reviewer attribution: run-to-run nondeterminism across two packages in opposite directions with identical denominators; no instrumented file changed. |
| NB-4 | PR-context artifacts absent in the item worktree; the session checkout's pair is stale for another branch. | Non-blocking, procedural. Scope derived from the executor's anchored listings plus the caller diff; a hand-authored, labelled summary was written to the gitignored worktree `artifacts/`. |
| NB-5 | Plan P4-T22 payload's comma-operator array collapse (planner-owned text defect). | Non-blocking. Detected and corrected by the executor; corrected run has five patterns and positive controls. |
| NB-6 | Plan P4-T24's HEAD-listing clause is unsatisfiable after mid-plan commits. | Non-blocking. Disclosed; the anchored diff carries both files as `M`. |
| NB-7 | Two `.claude/agent-memory/orchestrator/` paths are on the branch diff and were omitted from the caller's inventory. | Non-blocking. Pre-existing on the branch before execution (P0-T9 `BASE-DIFF-PATHS`); reviewer-scanned, no host-identity token; not source code. |

### Informational notes

| ID | Note |
|---|---|
| I-1 | `NotThrow<SemaphoreFullException>()` (test file line 443) is the generic form, which fails only when an exception assignable to `SemaphoreFullException` is thrown; a different exception type from `Dispose` would not fail at that assertion but would surface later (the `finally` disposal is a no-op after `_disposed = true`, and the round trip would then time out). `Dispose` has no realistic alternative throw path, and spec AC5 prescribes exactly this assertion shape, so no change is elected. |
| I-2 | The contended pre-check (`CurrentCount == 0` then wait) is a snapshot, not an atomic observation; a release between the read and the wait produces a contended count for an immediate acquisition. This is the issue #743 definition ("immediately before waiting"), is unchanged by this change, and `ContendedAcquisitions` is asserted only as `>=` in the assembly. |
| I-3 | A parked acquirer whose MSTest `[Timeout]` expires keeps waiting in the background; if the permit later frees within 120000 ms the continuation acquires and constructs a transaction nobody disposes (a genuine leak), and every later acquirer then fails by name after 120000 ms instead of hanging. If the bound expires first, the `TimeoutException` faults an unobserved task, which the .NET Framework default ignores. Both outcomes are recorded in the spec's Risks table; the change converts an unbounded hang into a bounded, named failure and does not claim to eliminate the leak class. The `CancellationToken`-observing overload is a recorded follow-up. |
| I-4 | Fail-before dossier numstat `61 0` versus post-format `62 0`: the one-line difference is the CSharpier rewrap of the probe lambda. |
| I-5 | Only the `QuickFiler.Test` assembly's tests were executed locally; CI runs the remaining assemblies. The change cannot affect them. |

### Carried-forward follow-ups (recorded in `spec.md`, not defects of this change)

1. `CancellationToken`-observing acquisition overload flowing `TestContext.CancellationTokenSource.Token` (addresses H-TOKEN-BLIND directly; deferred for blast radius).
2. Two further unbounded `SemaphoreSlim.WaitAsync()` calls on per-instance semaphores: `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` line 391 and `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` line 305.
3. Reconcile the 80/90 versus 85/75 coverage-floor conflict between `CLAUDE.md` and `.claude/rules/`.
4. Planner-side: the P4-T22 comma-operator payload shape and the P4-T24 HEAD-listing clause (NB-5, NB-6) should not recur in future plans.

## 9. Summary of Changes

| Path | Change | Reviewer verification |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | 304 → 342 lines (net +38; +47/−9 derived from the caller-supplied hunk headers). Adds `using System.Globalization;`, `internal const int TransactionGateAcquireTimeoutMs = 120000;`, a non-async parameterless `BeginTransactionAsync()` delegating to a new `async` `BeginTransactionAsync(TimeSpan bound)` that waits with `WaitAsync(bound)`, throws `TimeoutException` with `TRANSACTIONGATE_ACQUIRE_TIMEOUT` on `false`, and increments `_transactionAcquisitions` only on `true`; updates the class doc and the `UiThreadDispatcherTransaction` cref to `BeginTransactionAsync()`. | Full read. `TransactionGate.WaitAsync()` occurrences 0; `WaitAsync(bound)` 1; `Release()` 1 (line 113, reached only via `Dispose` line 339); `new UiThreadDispatcherTransaction()` 1 (line 189). Line order 175 < 178 < 181 < 188 < 189. |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | 396 → 458 lines (+62/−0, executor git-derived). One test added at lines 396-456; seven pre-existing tests untouched. | Full read. `[TestMethod]` 8, `[Timeout(GateTimeoutMs)]` 8, `DoNotParallelize` 0, `TimeSpan.Zero` 1 (line 419, inside the `try` while holding), no `.Install(` in the new method. |
| `docs/features/active/2026-09-13-…-882/` | `spec.md` (AC check-offs), `plan.2026-09-13T18-24.md` (50 of 50 tasks checked), 40 evidence artifacts across `baseline/`, `regression-testing/`, `qa-gates/`. | Enumerated by Glob; no raw tool document; host-identity scan 0 hits. |
| `.claude/agent-memory/orchestrator/MEMORY.md`, `…/parallel-item-preparation-is-structurally-impossible.md` | Pre-existing on the branch before plan execution (P0-T9). | Scanned: no host-identity token. |

Confirmed absent from the diff: `QuickFiler.Test/QuickFiler.Test.csproj`, `packages.config`, any `.runsettings`, any production project path, the four other consuming test files (`QfcFormControllerUndoHandoffTests.cs`, `QfcHomeControllerRunAsyncTests.cs`, `QfcItemController.InitializationTests.Part2.cs`, `WpfUiDispatcherTests.cs`), `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`, `CLAUDE.md`, `.claude/rules/`, `.github/`.

## 10. Compliance Verdict

**PASS.**

- Blocking findings: **0**
- Non-blocking findings: **7** (NB-1 to NB-7)
- Informational notes: **5** (I-1 to I-5)
- Acceptance criteria: 12 of 12 PASS (see `feature-audit.2026-09-29T09-50.md`)
- C# coverage row: FAIL, non-blocking, procedural disposition (section 5); no `remediation-inputs` artifact is produced.

## Appendix A: Test Inventory

Added — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` lines 396-456:

| Test method | Behaviour pinned | Scoped run | Full run |
|---|---|---|---|
| `BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing` | Zero-bound probe while holding throws `TimeoutException` with `TRANSACTIONGATE_ACQUIRE_TIMEOUT`; `TransactionAcquisitions − TransactionReleases == 1`; `ContendedAcquisitions >= before + 1`; holder's `Dispose` does not throw `SemaphoreFullException`; production round trip succeeds | Passed | Passed |

Pre-existing, unmodified, all Passed in both runs: `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt`, `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose`, `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent`, `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, `Transaction_DisposedTwice_DoesNotOverReleaseTheGate`, `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException`, `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition`.

Fail-before: compile-level, `error CS1501: No overload for method 'BeginTransactionAsync' takes 1 arguments` at `FixtureTests.cs(418,68)`, `Build FAILED`, exit 1 (`evidence/regression-testing/fail-before-exception.2026-09-29T09-06.md`).

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
dotnet-coverage collect --output coverage/coverage.cobertura.xml --output-format cobertura --settings coverage/coverage.cobertura.xml.effective-coverage.config -- vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /ResultsDirectory:coverage/test-results /Logger:trx;LogFileName=mstest-coverage-run.trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None
```

Fail-before (expect-fail) build: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU` against the pre-fix fixture with the new test inserted.
