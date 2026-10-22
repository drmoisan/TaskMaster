# Policy Compliance Audit: engine-toggle-permanent-config-fault-logs-every-poll (Issue #948)

- **Component:** `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (repeat prime-failure report suppression) and its regression partial
- **Audit date:** 2026-10-02T04-30
- **Branch:** `bug/engine-toggle-permanent-config-fault-logs-every-poll-948` (parallel cohort `bugs-2026-09-28`, cohort index 6)
- **Head:** `601f06174` (orchestrator-supplied; the last evidence artifact records PRE-FINAL-COMMIT-HEAD `4030f6d42`)
- **Merge base (origin/main):** `59cbab04f1c854baa2a03b6cbf755c1df4f961b4` (orchestrator-supplied; matches `evidence/baseline/anchor-merge-base.md` MERGE-BASE and `git merge-base origin/main HEAD` recorded there)
- **Work mode:** `full-bug` (from `issue.md`; acceptance-criteria source is `spec.md` v1.3 only)
- **Reviewer:** feature-review agent (read-only review; Read, Grep and Glob only; no shell commands were run in this review by caller directive)
- **Template provenance:** the MCP template tool `resolve_policy_audit_template_asset` is not available in this session, so this document hand-authors the canonical heading set named in `.claude/skills/policy-audit-template-usage/SKILL.md` (Executive Summary, sections 1 to 10, Appendix A and Appendix B) and omits the template instruction block.

## Executive Summary

**Verdict: PASS.** Blocking findings: 0. Non-blocking findings: 2 (documentation drift in an untouched test partial; evidence timestamp labels not clock-derived). Observations: 4.

- The fix is confined to one production method, one message, one new field and XML documentation, plus one new test partial and one `Compile` item. The branch diff against the merge base contains exactly these three code paths; everything else is Markdown under the feature folder, the inherited promotion record, and three inherited agent-memory files committed by the preparation passes.
- Bugfix workflow followed: seven regression tests were compiled against the unchanged production file and all seven failed deterministically (test A: `but found 5`), then passed after the minimal fix; the four existing fixture partials are untouched and all 39 fixture tests pass.
- Toolchain: CSharpier format and check clean; analyzer Rebuild and nullable Rebuild both exit 0 with 0 warnings and no skipped `CoreCompile`; repository coverage run 7361 of 7361 passed.
- C# coverage verdict: PASS (repository-wide first-party lines 85.35%, branches 79.74%; coordinator file 100% lines and 97.5% branches; 15 of 15 changed executable lines covered; the new guard line reports 2 of 2 branch outcomes).
- Correctness of the review focus items (guard under concurrent polls, throwing sink leaves the report owed, toggle path still reports every click) is confirmed by code reading and by the passing pins; see `code-review.2026-10-02T04-30.md`.

## Rejected Scope Narrowing

None detected. The caller directives were examined against the Scope Invariant:

- The caller listed the three changed code paths and stated that every other path is Markdown or inherited. This was verified against `evidence/qa-gates/footprint-scope.md` (`git diff --name-status` over the merge base) and is a factual description of the full diff, not a restriction of it. The audit scope remains the full branch diff.
- The caller stated that this item touches no PowerShell file. Verified: no `.ps1`, `.psm1` or `.psd1` path appears in the name-status list, so PowerShell has zero changed files on the branch.
- The caller asked that coverage floors be judged against CLAUDE.md. Both CLAUDE.md (80% line, 75% branch, 90% new code) and `.claude/rules/general-unit-test.md` (85% line, 75% branch) are reported below; the observed figures satisfy both, so no verdict depends on which document governs.
- The caller prohibited shell commands in this review. That is a procedural constraint on the reviewer, not a scope narrowing; the consequences (no PR-context regeneration, no `validate_evidence_locations.py` run) are recorded in section 8.

## Evidence Location Compliance

- The branch diff (43 added paths and 4 modified paths in `evidence/qa-gates/footprint-scope.md`, confirmed against the three inherited paths in `evidence/baseline/anchor-merge-base.md`) contains no path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. Finding count: 0.
- Every evidence artifact lives under the canonical kinds `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/` and `evidence/other/` of the feature folder (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`). Confirmed by listing the feature folder (44 files).
- `validate_evidence_locations.py --root .` was not run: no shell was available to this review. The name-status list above substitutes for the scan.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none supplied by the caller or the plan (the plan's Evidence location paragraph records the same).
- No raw collector or test-platform document is committed: `RAW-DOCS-COMMITTED: 0` and `RAW-DOCS-UNTRACKED-IN-FEATURE: 0` in `evidence/qa-gates/footprint-scope.md`; the committed test evidence consists of the JaCoCo package projection, the one-line first-party summary and trx-derived summaries, which is the Committed Test Evidence Format CLAUDE.md requires.

## 1. General Unit Test Policy Compliance

Scope: the new partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (290 lines, seven `[TestMethod]` tests A to G and one private static `PollAsync` helper). No existing test was modified.

| Check | Verdict | Evidence |
|---|---|---|
| Independence (any order) | PASS | Each test constructs its own `Harness` (7 occurrences, lines 42, 77, 109, 151, 185, 217, 250); no static mutable state; no shared fixture field is written. |
| Isolation (single unit) | PASS | Each test targets one behaviour of `CompletePrime`/`GetPressed` or the toggle boundary; failure messages name the asserted property. |
| Fast execution | PASS | Every activation read returns an already completed task (`Task.FromException`, `Task.FromCanceled`, `Task.FromResult`); no I/O. |
| Determinism | PASS | Each poll awaits the coordinator's own prime handle (`PollAsync`, lines 278-288); outcomes are decided by program order. Banned tokens in added test lines: `Thread.Sleep` 0, `Task.Delay` 0, `SpinWait` 0, `while (` 0, `DateTime` 0, `Stopwatch` 0, `.Wait(` 0, `.Result` 0, `Timeout` 0, `ManualResetEvent` 0, `SemaphoreSlim` 0 (`evidence/qa-gates/determinism-tokens.md`, re-confirmed by reading the file). |
| Readability and maintainability | PASS | Descriptive names; every assertion carries a reason string; one bounded helper. |
| Arrange-Act-Assert | PASS | `// Arrange`, `// Act`, `// Assert` present in all seven tests. |
| Documentation of intent | PASS | File-level `<summary>` and `<remarks>`; each test has a `<summary>` stating scenario and expected outcome. |
| Scenario completeness | PASS | Positive (C recovery); negative/error (A, B, G); edge (D new kind, E per key); boundary interaction (F toggle path); state transitions absent -> priming -> absent-with-record -> cached (A, B, C). Concurrency: sequential by construction; the at-most-one-prime pins in the existing partials are unchanged and pass. |
| No external dependencies | PASS | Strict `Mock<IAppItemEngines>` from the shared harness (`MockBehavior.Strict`, fixture line 424); injected delegates record errors, notifications and invalidations. |
| No temporary files | PASS | `GetTempFileName` 0, `GetTempPath` 0, `File.` 0 in added test lines. |
| Clear failure messages | PASS | FluentAssertions with reason strings; the fail-before messages in `evidence/regression-testing/repeat-fault-suppression-fail-before.md` are readable (`Expected harness.Errors.Count to be 1 ... but found 5`). |
| Test file location | PASS (repository convention) | The C# test projects in this repository are per-assembly `<Project>.Test/` directories mirroring the production folder (`TaskMaster.Test/Ribbon/` for `TaskMaster/Ribbon/`); the new partial follows that established layout, as CLAUDE.md section 7.1 requires. Colocation in the production tree did not occur. |
| Determinism infrastructure (clock, RNG, fake timers) | PASS | The change reads no clock and uses no randomness; no `TimeProvider` is required and none is used (`TimeProvider` 0 in added lines). |

### 1.1 Coverage Requirements

Thresholds applied: CLAUDE.md UT2 (C# line >= 80%, branch >= 75%, new code >= 90%, no regression on changed lines) and `.claude/rules/general-unit-test.md` / `quality-tiers.md` (line >= 85%, branch >= 75%, no regression on changed lines). Both sets are satisfied by the figures below.

- Repository-wide first-party (post-processed Cobertura, final run): lines 56204/65855 = 85.35%, branches 13618/17078 = 79.74%. Verified in this review by reading the root element of the git-ignored `artifacts/csharp/coverage.xml` in the worktree (`line-rate="0.853451" branch-rate="0.7974" lines-covered="56204" lines-valid="65855" branches-covered="13618" branches-valid="17078"`), which matches the committed projection `evidence/qa-gates/coverage-projection.md`.
- Baseline (same route, merge-base production file): lines 56201/65845 = 85.35%, branches 13618/17076 = 79.75% (`evidence/baseline/coverage-baseline.md`).
- Changed file `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`: 177/177 lines (100%), 39/40 branches (97.5%); baseline 167/167 and 37/38. Verified at the `<class>` node (`line-rate="1" branch-rate="0.975"`). The single uncovered branch is the pre-existing null-key arm of `RenderEngineName` (line 442, `condition-coverage="50% (1/2)"`), unchanged by this fix.
- Changed executable lines: 15 of 15 covered (`CHANGED-LINES-WITH-ELEMENT: 15`, `CHANGED-LINES-UNCOVERED: 0`); the guard line 421 reports `condition-coverage="100% (2/2)"`, so both the report outcome and the skip outcome ran. Confirmed by reading the method node for `CompletePrime` (lines 406 to 435 all `hits="1"`) and `BuildPrimeFailedMessage` (lines 474 to 482 all `hits="1"`).
- New production files: none. New test file: excluded from the coverage denominator by design.
- Regression on changed lines: none (baseline `CompletePrime` 15/15 elements, final 20/20; `BuildPrimeFailedMessage` 8/8 to 9/9).
- The repository branch rate moved from 79.75% to 79.74%. Branches covered are equal (13618) and branches valid rose by 2, both added by the guard and both covered; the UtilitiesCS package, untouched by this change, lost 2 covered branches and 8 covered lines between runs while the TaskMaster package gained (LINE covered 2467 to 2477, BRANCH covered 517 to 519, missed unchanged). This is collector variance in an unrelated package, recorded, not a regression attributable to the change.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-948.cobertura.xml` (git-ignored; projected into `evidence/baseline/coverage-baseline.md`)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` (git-ignored Cobertura document in the worktree, read in this review; projected into `evidence/qa-gates/coverage-projection.md`)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.35% lines (56201/65845) / 79.75% branches (13618/17076) -> Post-change: 85.35% lines (56204/65855) / 79.74% branches (13618/17078). Change: +0.00% lines / -0.01% branches (collector variance in the untouched UtilitiesCS package; the TaskMaster package rose and the coordinator file rose from 167/167 to 177/177 lines and 37/38 to 39/40 branches). New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/baseline/coverage-baseline.md`, `evidence/qa-gates/coverage-projection.md`, root and class nodes of `artifacts/csharp/coverage.xml`.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Changed files on branch | Artifact state | Verdict line |
|---|---|---|---|
| C# | 3 (`.cs` production, `.cs` test, `.csproj`) | Cobertura document present at the canonical path in the worktree (git-ignored); committed projection present | C# coverage verdict: PASS (85.35% lines, 79.74% branches repository-wide; 100% lines on the changed file; 100% on changed lines) |
| TypeScript | 0 | no artifact expected | zero changed files |
| PowerShell | 0 | no artifact expected | zero changed files |
| Python | 0 | no artifact expected | zero changed files |

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 3 | 7 new; 39 in fixture; 7361 repository | PASS (7361/7361) | 85.35% lines / 79.75% branches | 85.35% lines / 79.74% branches | 100% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first | PASS | `evidence/regression-testing/repeat-fault-suppression-fail-before.md`: production file hash equal to the merge-base hash (`PROD-HASH-AT-CONTROL` = `ANCHOR-HASH-PROD`), 39 tests, 7 failed, all 7 the new tests, test A message `but found 5`. |
| Bugfix workflow: minimal targeted fix | PASS | Production numstat 35/15 after formatting (29/9 before); edits confined to one field, `CompletePrime`, `BuildPrimeFailedMessage` and XML docs. Twelve protected method spans and the `_primeTasks` declaration hash equal to the merge base (`evidence/qa-gates/protected-regions-unchanged.md`). |
| Bugfix workflow: verify locally, full toolchain in order | PASS | `evidence/qa-gates/toolchain-final-pass.md`: one pass, format -> check -> analyzers -> nullable -> fixture -> coverage, all exit 0. |
| Simplicity first | PASS | Two `ConcurrentDictionary` operations and a value-tuple key; no new abstraction, lock, timer or reset API. |
| Reusability / no copy-paste | PASS | The test helper `PollAsync` replaces seven inline loops; production reuses `BuildPrimeFailedMessage`. |
| Extensibility / public API stability | PASS | Constructor, `GetPressed`, `HandleToggleClickAsync`, `ExecuteToggleAsync`, `GetPrimeTask` signatures unchanged; the production wiring compiles unchanged (both rebuilds exit 0). |
| Separation of concerns | PASS | Host-neutral decision logic; the only sink is the injected delegate. |
| Error handling: fail fast, no silent swallow | PASS with accepted exception | The suppressed repeat is a duplicate of an already delivered report, the fault is still observed (`Task.Exception` read) and the marker still cleared. The empty `catch (Exception)` around the sink is pre-existing (#947) and unchanged in count (3 catch lines before and after); see section 8, X-1. |
| Logging pattern | PASS | Reporting stays on the injected `logError` delegate; the appended sentence tells the operator that repeats are suppressed (test G pins it). |
| File size <= 500 lines | PASS (observation) | Production file 496 lines; new partial 290; primary fixture 470 unchanged (`evidence/qa-gates/file-line-counts.md`, confirmed by Read: the production file ends at line 496). Headroom is four lines; see code review CR-2. |
| Naming | PASS | `_reportedPrimeFaults`, `reportKey`, tuple elements `EngineName`/`FaultType`; camelCase locals, PascalCase members. |
| Docs and comments (why, not what) | PASS | `CompletePrime` remarks gain the suppression paragraph, including why the record follows the sink and the placement constraint; `GetPrimeTask` return contract reworded; the "Report-then-clear" comment updated. |
| Dependencies | PASS | None added; `System` and `System.Collections.Concurrent` already imported; `ValueTuple` is in mscorlib on net481; `packages.config` files unchanged (`PROTECTED_FILES_DIFF_EXIT=0`). |
| Supporting documents updated | PASS | spec.md check-offs, plan task boxes, evidence artifacts; the stale Race-partial remark is recorded as a follow-up in spec Rollout. |
| Seven-stage loop stages 4, 6, 7 (architecture tests, contract checks, integration tests) | PASS (no applicable tooling) | No `*.ArchitectureTests` project, contract schema or integration suite exists for this legacy VSTO solution; CLAUDE.md's four-step C# loop is the governing toolchain and was completed. |

## 3. Language-Specific Code Change Policy Compliance

Language: C# (CLAUDE.md C#1 to C#7 and `.claude/rules/csharp.md`).

| Check | Verdict | Evidence |
|---|---|---|
| C#1.1 CSharpier via `dotnet tool run`, format then check | PASS | `evidence/qa-gates/csharpier-format.md` (rewrite count 0 on the final pass), `evidence/qa-gates/csharpier-check-final.md` (`Checked 1637 files`, exit 0). |
| C#1.2 Analyzer Rebuild with `EnableNETAnalyzers`/`EnforceCodeStyleInBuild` | PASS | `evidence/qa-gates/msbuild-analyzer-final.md`: exit 0, ERRORS 0, WARNINGS 0, `SKIP_CORECOMPILE_LINES: 0`, CSC output lines for both TaskMaster projects >= 1 (non-vacuous). |
| C#1.3 Nullable Rebuild with `TreatWarningsAsErrors`, no `/p:Nullable=enable` | PASS | `evidence/qa-gates/msbuild-nullable-final.md`: exit 0, 0 errors, 0 warnings, no skipped CoreCompile, "no Nullable property override". The production file carries no `#nullable` directive (token count 0), consistent with the per-file opt-in rule. |
| Toolchain order and restart rule | PASS | One pass in order; no step changed files after the P3-T1 format (rewrite count 0). |
| C#2 strong contracts, explicit types | PASS | Field declared with its full generic type; `var` used only where the type is evident (`reportKey` from a tuple literal with named elements). |
| C#3 small focused methods | PASS | `CompletePrime` remains a single-purpose observer (31 lines). |
| C#4 exceptions at boundaries, no broad catch | PASS with accepted exception | No new catch; the pre-existing sink guard is retained as it was (section 8, X-1). |
| C#5 file structure | PASS | One type per file; `internal sealed`. |
| C#6 XML docs on non-obvious behaviour | PASS | New field, `CompletePrime` summary/remarks, `GetPrimeTask` returns element. |
| C#7 analyzer configuration, no suppressions | PASS | No `#pragma`, `[SuppressMessage]` or `.editorconfig` change in the diff (added-line token scan in `evidence/qa-gates/production-edit-scope.md`; the diff touches no config file). |
| Time seam guidance | PASS | The design reads no clock; `DateTime` and `TimeProvider` tokens 0 in added production lines. |
| Prohibited behaviours (broad refactors, weakened assertions, sleeps/retries) | PASS | None observed; existing tests byte-identical. |

Other languages: no TypeScript, Python or PowerShell file is in the branch diff.

## 4. Language-Specific Unit Test Policy Compliance

Language: C# (CLAUDE.md CUT1 to CUT3 and `.claude/rules/csharp.md` Testing Standards).

| Check | Verdict | Evidence |
|---|---|---|
| CUT1 MSTest framework | PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting;`, seven `[TestMethod]`; the `[TestClass]` attribute lives on the primary partial (line 22) and applies to all partials. |
| CUT2 Moq for mocks | PASS | `Mock<IAppItemEngines>` (strict) from the shared harness; `Setup`, `SetupSequence`, `ThrowsAsync`, `Verify(..., Times.Exactly(n))`. |
| CUT2 FluentAssertions preferred | PASS | All assertions are FluentAssertions (`Should().Be`, `ContainSingle`, `HaveCount`, `BeSameAs`, `BeAssignableTo`, `Equal`, `Contain`, `EndWith`, `BeEmpty`); no MSTest `Assert`. |
| CUT3 toolchain commands | PASS | Exactly the CLAUDE.md commands, with the step-4 route recorded as DIRECT (section 8, X-2). |
| Per-file coverage of the changed production file >= 90% | PASS | 100% lines (177/177), 97.5% branches (39/40). |
| New method coverage >= 90% | PASS | No new method; the amended `CompletePrime` is 20/20 elements. |
| Changed-line regression (blocking if present) | PASS | 0 uncovered changed lines; baseline uncovered 0. |
| Deterministic test rules (no network, PATH, cwd, external services) | PASS | All boundaries mocked; no filesystem or process access. |
| Fail-before / pass-after evidence | PASS | Both runs recorded with command, exit code, counters and per-test RESULT lines; the only difference is the production edit (`PROD-HASH-AFTER` differs from the control hash; the partial hash is identical in both runs). |

## 5. Test Coverage Detail

| File | Status | Lines (post) | Branches (post) | Lines (baseline) | Branches (baseline) |
|---|---|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | modified | 177/177 (100%) | 39/40 (97.5%) | 167/167 (100%) | 37/38 (97.4%) |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` | added (test) | excluded from denominator | excluded | not present | not present |
| `TaskMaster.Test/TaskMaster.Test.csproj` | modified (1 insertion) | not instrumentable | not instrumentable | n/a | n/a |

Method detail (from the Cobertura class node, confirmed in this review):

| Method | Post elements | Post covered | Baseline elements | Baseline covered |
|---|---|---|---|---|
| `CompletePrime` (lines 405-435) | 20 | 20 | 15 | 15 |
| `BuildPrimeFailedMessage` (lines 473-482) | 9 | 9 | 8 | 8 |
| `.ctor` (field initialisers incl. lines 87-90) | all | all | all | all |

Guard proof: line 421 `if (!_reportedPrimeFaults.ContainsKey(reportKey))` carries `branch="True" condition-coverage="100% (2/2)"`; line 426 (record) `hits="1"`; line 425 (sink) `hits="1"`. The collector writes `hits` as a binary flag (maximum 1 across 133,464 line elements at baseline), so the plan's original hit-count comparison was unsatisfiable and was replaced at plan version 0.6 by the branch-row proof; that replacement was preflighted and is the stronger proof of the same property.

Repository comparability: root `lines-valid` 65845 -> 65855 (within 1% of baseline), root line-rate 0.853535 -> 0.853451 (within 0.005). Both floors (80/75 and 85/75) met in both runs.

## 6. Test Execution Metrics

| Run | Command (abridged) | Exit | Result |
|---|---|---|---|
| Coordinator baseline (P0-T16, merge-base production and tests) | `vstest.console.exe TaskMaster.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~...EngineToggleStateCoordinatorTests` | 0 | 32/32 passed |
| Stall probe (P0-T15, four UtilitiesCS.Test shell-icon classes) | `vstest.console.exe UtilitiesCS.Test.dll` with the four-class filter | 1 (expected 1) | 22/23, one `Win32 handle ... not valid` failure; `STALL-PROBE: REPRODUCES` |
| Coverage baseline (P0-T17) | `dotnet-coverage collect ... vstest.console.exe <9 assemblies>` DIRECT route | 0 | 7354/7354 passed |
| Fail-before (P1-T4) | same fixture filter, new partial against the unchanged production file | 1 (expected 1) | 39 total, 32 passed, 7 failed (all seven new tests) |
| Pass-after (P2-T5) | same fixture filter after the fix | 0 | 39/39 passed |
| Final fixture (P3-T7) | same fixture filter on the P3-T6 rebuild | 0 | 39/39 passed |
| Final coverage (P3-T8) | `dotnet-coverage collect ... vstest.console.exe <9 assemblies>` DIRECT route | 0 | 7361/7361 passed; 0 failed, 0 skipped |

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | `CMD-HYGIENE` over all Markdown under the feature folder (`evidence/qa-gates/evidence-hygiene.md`), plus a Grep in this review for drive-letter user paths and account/machine tokens over the feature folder, the promotion record and the inherited agent-memory file | FILES_SCANNED 45, ACCOUNT_HITS 0, MACHINE_HITS 0, DRIVE_USERS_HITS 0; reviewer Grep: 0 matches | PASS |
| Suppression scan (added lines) | Token scan of added production lines (`evidence/qa-gates/production-edit-scope.md`); Read of both C# files in this review | No `#pragma`, `SuppressMessage`, `ExcludeFromCodeCoverage`, `DoNotParallelize` or `Timeout` added | PASS |
| Workflow change scan | Name-status diff over the merge base (`evidence/qa-gates/footprint-scope.md`) | No `.github/`, `.editorconfig`, `.csharpierignore`, run-settings or `coverage.config` path in the diff | PASS |

Additional checks recorded in prose: the host-path sweep also covered the three `Command:` lines of the test-run artifacts (results directories are relative `coverage\test-results\948\...`, trx names explicit); the raw-document scan found no `.trx`, `.xml`, `.coverage` or `.cobertura` path added to git; no `exclude` entry matching a production source path was introduced (no coverage configuration file is in the diff).

## 8. Gaps and Exceptions

Accepted exceptions (not violations):

- **X-1 Broad `catch (Exception)` around the `logError` sink in `CompletePrime` (lines 428-431).** Pre-existing from issue #947 and kept as it was; the catch count on code lines is 3 before and after (`FILE-CATCH-CODE-LINES`). Disposition follows the #947 ruling: the sink is the type's last reporting channel, the discard is documented in the remarks and an in-block comment, and `RibbonCommandBoundary.SafeLog` is the precedent. The #948 record statement sits inside that try directly after the sink call, so a throwing sink leaves the pair unrecorded and the report owed.
- **X-2 Coverage route.** The MSTest-with-coverage step ran as the runner's own inner `dotnet-coverage collect` plus `vstest.console.exe` invocation with the four known local shell-icon test classes excluded through a `TestCaseFilter` and a `/Blame` hang timeout appended, because the stall probe reproduced the local shell-icon failure (`evidence/baseline/stall-probe.md`). Spec v1.3 AC-N admits this route under exactly that condition; CI runs the four classes unfiltered. The route is recorded in both coverage artifacts.
- **X-3 Test layout.** The `tests/`-mirror rule in `.claude/rules/general-unit-test.md` is realised in this solution by per-assembly `<Project>.Test/` directories; the new partial follows the existing fixture's location.

Gaps (procedural, non-blocking):

- **G-1 PR-context artifacts absent.** `artifacts/pr_context.summary.txt` and `.appendix.txt` do not exist in the worktree and could not be regenerated without a shell. Scope and evidence were derived from the orchestrator-supplied diff and the committed name-status footprint evidence, which agree with each other and with the files as read.
- **G-2 `validate_evidence_locations.py` not executed** (no shell). Substituted by the name-status scan in the Evidence Location Compliance section.
- **G-3 Policy-audit template not resolved through MCP** (tool unavailable in this session). The canonical heading set was hand-authored.
- **G-4 Head commit not independently confirmed.** The caller states head `601f06174`; the last committed evidence records `4030f6d42` as the pre-final-commit head, consistent with one further docs commit. No code file changed after `c4c4e7585` per `evidence/qa-gates/coverage-projection.md` and the empty porcelain spans.
- **G-5 Evidence `Timestamp:` labels are not clock-derived.** The Cobertura root `timestamp="1790911441"` decodes to 2026-10-02T03:24:01Z; the projection's label is `2026-10-02T00-27`, about 63 minutes later than that clock at UTC-4. Labels are internally ordered and consistent across artifacts, so the evidence chain is intact; recorded as code-review finding CR-5 and a follow-up.
- **G-6 `quality-tiers.yml` absent at the repository root** (pre-existing; the tier-classification gate is unevaluable repository-wide; already promoted to an issue by the #956 review). Not introduced by this change.

## 9. Summary of Changes

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (476 -> 496 lines): new `_reportedPrimeFaults` field keyed by `(string EngineName, Type FaultType)`; `CompletePrime` reports a prime failure only when its pair is not yet recorded and records the pair as the statement directly after the sink call, inside the pre-existing sink guard; `TryRemove` remains the last statement; `BuildPrimeFailedMessage` gains the sentence "Further failures of this kind for this engine are not logged again."; XML docs updated on the field, `CompletePrime` and `GetPrimeTask`.
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (new, 290 lines): seven tests A to G and the `PollAsync` helper.
- `TaskMaster.Test/TaskMaster.Test.csproj`: one `Compile` item after the ThrowingSink entry (line 363).
- Feature folder: spec.md v1.3 (16/16 AC checked), plan v0.7 (all tasks checked), 40 evidence artifacts, research record.

## 10. Compliance Verdict

**PASS.** Zero blocking findings. The change satisfies the General Code Change Policy, the General Unit Test Policy, the C# Code Change Policy and the C# Unit Test Policy as embedded in CLAUDE.md, and the coverage floors of both CLAUDE.md and `.claude/rules/`. Two non-blocking findings and four observations are recorded in `code-review.2026-10-02T04-30.md`; none requires remediation before merge. No `remediation-inputs` artifact is produced.

## Appendix A: Test Inventory

New tests (all in `EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs`; fail-before Failed, pass-after Passed):

| Test | Spec letter / AC | Fail-before | Pass-after |
|---|---|---|---|
| `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly` | A / AC-A, AC-H | Failed (`but found 5`) | Passed |
| `GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly` | B / AC-B | Failed (5 entries) | Passed |
| `GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce` | C / AC-C | Failed (2 entries) | Passed |
| `GetPressed_WhenFailureKindChanges_LogsNewKindOnce` | D / AC-D | Failed (4 entries) | Passed |
| `GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged` | E / AC-E | Failed (3 entries) | Passed |
| `HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault` | F / AC-F | Failed (3 entries) | Passed |
| `GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain` | G / AC-G | Failed (sentence absent) | Passed |

Existing pins named by the spec (unchanged, Passed in baseline, fail-before and pass-after): `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime`, `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`, `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`, `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse`, `ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged`, `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate`.

Fixture totals: 32 at baseline, 39 after; repository 7354 at baseline, 7361 after.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool restore` (once per worktree), then `dotnet tool run csharpier format .` and `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `dotnet-coverage collect --output coverage\final-948.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-948.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\948\final" "/Logger:trx;LogFileName=final-948.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` (the DIRECT route admitted by spec v1.3 AC-N; the RUNNER route is `scripts/vscode/Invoke-MSTestWithCoverage.ps1`)
5. Per-class evidence runs: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\948\<task>" "/Logger:trx;LogFileName=<task>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`
