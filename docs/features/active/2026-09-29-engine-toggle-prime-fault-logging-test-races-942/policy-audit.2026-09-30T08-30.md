# Policy Compliance Audit — engine-toggle-prime-fault-logging-test-races (Issue #942)

- Component: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (`CompletePrime` report-then-clear ordering) and its MSTest fixture
- Branch: `bug/engine-toggle-prime-fault-logging-test-races-942`
- Base (merge base with origin/main): `231e1c0b55105aeb626bf5a6e8d0266a567cacad` (origin/main as merged into the branch at `fadcb6417`; plan correction C1)
- Work mode: `full-bug` (issue.md line 12); acceptance-criteria source: `spec.md` only (14 items)
- Review date label: 2026-09-30T08-30. No clock was available to the review session (no shell tool); the label was assigned as the first quarter-hour later than every executor evidence label (latest 2026-09-30T07-57) and is disclosed as such. The executor's labels are local time: the final Cobertura document carries epoch `timestamp="1790769007"` (2026-09-30T11:50:07Z) against the executor's `07-50` label, an offset of UTC-4.
- Reviewer tooling: Read, Grep and Glob only. No `git`, `msbuild`, `vstest`, `dotnet` or MCP invocation was possible, so no toolchain step was re-run; every gate below is verified from the committed evidence projections, the raw post-processed Cobertura documents the executor left under the git-ignored `coverage/` directory, and direct reads of the four changed code files.

## Template Resolution Deviation

The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` were not exposed to this review session. This artifact is hand-authored preserving all twelve canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5, plus the per-language coverage comparison block, the coverage evidence checklist and the coverage metrics table in the shape the orchestrator's validator expects. The audit is not marked BLOCKED on that basis: every section below is evidence-backed and the template's instruction block is not present.

## Executive Summary

- Overall verdict: **PASS**. 0 FAIL findings, 0 blocking PARTIAL findings, 4 non-blocking observations (PA-1 to PA-4 in section 8).
- The change is a two-statement reorder inside a private method plus documentation, a test-fixture observer hook, one new deterministic regression test in a new partial, and one `Compile Include` line. No public API, dependency, configuration, run-settings or run-regime change.
- Bugfix workflow satisfied in order: regression test written first and observed failing against the byte-identical base production file (SHA-256 `F2A961DD…CEF0` at control equals the baseline hash), then the minimal fix, then the full CLAUDE.md toolchain in one uninterrupted pass (format, check, analyzer rebuild, nullable rebuild, coverage-enabled tests; exit 0 at every step; `SKIP_CORECOMPILE_LINES: 0` on both rebuilds).
- C# coverage verdict: PASS. First-party processed Cobertura 85.31% lines (56080/65736) and 79.72% branches (13596/17054) after the change against 85.32%/79.73% before, both above the 85%/75% floors of `.claude/rules/quality-tiers.md` and the 80%/75% floors of CLAUDE.md; the only changed production file is unchanged at 100% lines (143/143) and 97.37% branches (37/38), and the one moved executable line (359) has hits=1.
- No language other than C# has changed files on the branch (`.cs` x3, `.csproj` x1, Markdown under the feature folder and the inherited promotion record).
- Scope narrowing attempts: none. Evidence-location violations: none.

## Rejected Scope Narrowing

None detected. The caller's prompt supplied the resolved base, the full branch footprint (four code files, the feature folder, the inherited promotion record) and asked that the spec's declared non-goal (hazard B) be listed as a follow-up rather than treated as blocking. Listing a spec non-goal as non-blocking is the spec's own scope decision, not a narrowing of the audit scope; the full branch diff against `231e1c0b` was audited.

## Evidence Location Compliance

- Every evidence artifact on the branch lives under `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/{baseline,regression-testing,qa-gates,other}/`, the canonical `<FEATURE>/evidence/<kind>/` layout (plan "Evidence location" paragraph; 37 Markdown files enumerated by Glob).
- Branch diff scan for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`: the executor's anchored `--diff-filter=A` enumeration (`evidence/qa-gates/footprint-scope.md`) lists 29 added paths, none under `artifacts/`; a Grep of the feature folder for those four prefixes returns no match; the item worktree contains no `artifacts/` directory at all. Zero FAIL-level findings.
- `validate_evidence_locations.py --root .` could not be executed (no shell); the manual scan above substitutes for it and is recorded as such.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none required; no caller instruction supplied a non-canonical evidence path.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence and isolation | PASS | The new test builds its own `Harness`, one strict mock, one held `TaskCompletionSource`; it shares no static state; it targets one behavior (marker registered at report time). |
| Fast, deterministic | PASS | Outcome is a function of program order on the pool thread (the sink reads `GetPrimeTask` on the same thread that will run `TryRemove`); no sleep, delay, timer, retry, gate or parallelism attribute (`evidence/qa-gates/determinism-tokens.md`, 20 tokens at 0; confirmed by direct read of both test files). |
| Readability, AAA, intent documented | PASS | `// Arrange`, `// Act`, `// Assert` markers; XML summary names issue #942 and states the invariant; every FluentAssertions call carries a reason string. |
| Scenario completeness | PASS (bug-fix scope) | Positive fault path (new test + existing fault test), cancellation path (two existing Race tests), success early-return (existing prime-success tests), null/unmapped keys (existing). The single `CompletePrime` path covers faulted and canceled outcomes together. |
| No external dependencies, no temp files | PASS | Strict Moq mock of `IAppItemEngines`; no filesystem, network, process or clock. |
| Test file location | PASS by repository convention | C# tests live in `<Project>.Test/<Area>/` mirroring `<Project>/<Area>/` throughout this repository; the new partial sits beside its two siblings. The rule file's `tests/` wording is the cross-language default and is not the convention applied to C# projects here (pre-existing, informational). |
| Coverage, no regression on changed lines | PASS | See the comparison block below and section 5. |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-942.cobertura.xml` (git-ignored, read directly at root and class level) projected to `evidence/baseline/coverage-baseline.md`
- C# post-change coverage artifact: `coverage/final-942.cobertura.xml` (git-ignored, read directly at root and class level) projected to `evidence/qa-gates/coverage-post-change.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.32% lines (56083/65736), 79.73% branches (13597/17054). Post-change: 85.31% lines (56080/65736), 79.72% branches (13596/17054). Change: -0.01% lines (-3 covered lines, denominator unchanged at 65736) and -0.01% branches (-1 covered branch), located in packages other than the changed file and within the plan's D-7 run-to-run tolerance; the changed file is identical at both stages at 100% lines (143/143) and 97.37% branches (37/38). New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/coverage-baseline.md, evidence/qa-gates/coverage-post-change.md (COMPARISON), coverage/final-942.cobertura.xml class element at document line 230308 read directly.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact read by this review | State |
|---|---|---|
| C# | `coverage/final-942.cobertura.xml` and `coverage/baseline-942.cobertura.xml` in the item worktree (post-processed by `ConvertTo-KoverageCoberturaXml`, workspace-relative filenames) | Present; root element and the coordinator class element read directly; figures match the committed projections exactly |
| C# (hook canonical path) | `artifacts/csharp/coverage.xml` in the item worktree | Not populated; the DIRECT route writes to `coverage/` and CLAUDE.md "Committed Test Evidence Format" prohibits committing the raw document. Recorded as observation PA-1, non-blocking, because the same document was verified at its runner location |

C# coverage verdict (thresholds and changed-line regression, first-party processed Cobertura): PASS.
C# coverage note (canonical hook path `artifacts/csharp/coverage.xml`): not populated in the item worktree; substitute document verified directly at `coverage/final-942.cobertura.xml`; this is an observation, not a threshold verdict.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 4 (1 production, 2 test, 1 project file) | 25 fixture tests (1 new); 7324 in the coverage-enabled suite | 7324/7324 passed, 0 failed | 85.32% lines / 79.73% branches | 85.31% lines / 79.72% branches | 100% |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test first | PASS | `evidence/regression-testing/prime-fault-ordering-fail-before.md`: EXIT_CODE 1 = ExpectedExitCode 1; 24 passed / 1 failed; the failing message is the FluentAssertions `BeSameAs` text ("Expected handleSeenBySink to refer to ... ContinuationTaskFromTask ... but found System.Threading.Tasks.Task"), i.e. the sink observed `Task.CompletedTask`; production file hash equals the baseline hash. |
| Minimal, targeted fix | PASS | Numstat `10 5` on the production file: five removed lines are three documentation lines, the `TryRemove` statement and one blank line; no line of the early return, the failure computation or the synthesized exception was rewritten (`evidence/qa-gates/production-reorder-scope.md`, confirmed by reading lines 344–360). |
| Verify locally before review, full toolchain in order | PASS | `evidence/qa-gates/toolchain-final-pass.md`: pass 1 clean; format (0 rewrites), check ("no differences"), analyzer rebuild (0 errors, 0 warnings, `SKIP_CORECOMPILE_LINES: 0`), nullable rebuild (same), fixture run 25/25, coverage-enabled suite 7324/7324. |
| Design principles (simplicity, separation of concerns) | PASS | Two statements reordered; no new abstraction, seam, lock or constructor parameter; I/O stays behind the injected delegates. |
| Error handling, fail fast | PASS | No new `try`/`catch`/`finally`; the type keeps exactly one `catch (` (line 181) and one `lock (` (line 271). |
| Module and file size (500 lines) | PASS | 420 / 470 / 77 lines (`evidence/qa-gates/file-line-counts.md`; confirmed by direct read: 420, 470, 77). |
| Naming, docs, comments explain why | PASS | Summary of `CompletePrime`, returns of `GetPrimeTask` and a three-line "Report-then-clear is load-bearing" comment state the invariant and its reason. |
| Dependencies | PASS | None added; the new partial omits `using Moq;` because it does not name Moq directly. |
| Public API compatibility | PASS | `CompletePrime` and `Harness` are private; `GetPrimeTask` is internal with an unchanged signature and a strengthened post-condition. `GetPrimeTask(` has no production caller (Grep over `TaskMaster/`: only the declaration at line 249). |
| Seven-stage loop stages 4, 6, 7 (architecture-boundary, contract/schema, integration) | Not evaluable, pre-existing | No `*.ArchitectureTests` project, contract-check tooling or integration-test stage exists for C# in this repository; CLAUDE.md's four-step C# toolchain is the operative loop and was run. Observation PA-3. |

## 3. Language-Specific Code Change Policy Compliance

C# (`.claude/rules/csharp.md`, CLAUDE.md C#1–C#7):

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run`, check clean | PASS | Step 1/2 of the final pass; "Checked 1626 files", no differences. The csproj is excluded by `.csharpierignore` line 12 (confirmed). |
| Analyzer rebuild command verbatim, `/t:Rebuild` | PASS | Command matches CLAUDE.md character-for-character; `SKIP_CORECOMPILE_LINES: 0`, `CSC_OUT_TASKMASTER 2`, `CSC_OUT_TASKMASTER_TEST 2`. |
| Nullable rebuild command verbatim, no `/p:Nullable=enable` | PASS | Command matches; no `/p:Nullable=enable`; 0 errors, 0 warnings. |
| Test step (`Invoke-MSTestWithCoverage.ps1` / VS Code task) | PASS with disclosed substitution | The runner hard-codes its assembly filter; plan D-6 selects the DIRECT route when the shell-icon stall probe does not read CLEAR (`evidence/baseline/stall-probe.md`: one `ShellUtilities_Tests` test failed with a Win32 icon-handle `ArgumentException`, a pre-existing workstation defect). The DIRECT route issues the runner's own inner `dotnet-coverage collect ... vstest.console.exe` invocation with the four classes excluded and post-processes with the runner's own helpers; CI executes the four classes. Observation PA-2. |
| Naming, XML docs, `internal` surface | PASS | PascalCase members, camelCase locals; XML docs updated on both touched members; nothing made public. |
| Null-safety | PASS | No nullable directive in any touched file (pre-existing state); no new nullable warning under `/p:TreatWarningsAsErrors=true`. |
| Banned symbols (`Thread.Sleep`, `Task.Delay`, `DateTime.Now`) | PASS | Zero occurrences added (determinism-tokens; direct read). |
| Prohibited behaviors (sleeps, retries, weakened assertions, `[DoNotParallelize]`) | PASS | None; the original test is byte-for-byte unchanged (method SHA equal at base and head, `evidence/qa-gates/original-test-unchanged.md`). |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestMethod]` on the new test; `[TestClass]` remains on the primary partial only (the Race partial follows the same shape). |
| Moq for mocks, strict | PASS | `Harness.Engines` is `new Mock<IAppItemEngines>(MockBehavior.Strict)` (line 424); the new test sets up exactly the one call the prime makes. |
| FluentAssertions with reasons | PASS | Six assertions, each with a reason string. |
| AAA structure | PASS | Explicit markers at lines 29, 40, 44 of the new partial. |
| Deterministic test rules; parallel regime unchanged | PASS | `TaskMaster.runsettings` and `scripts/vscode/TaskMaster.cli.runsettings` diff-clean against the base (`RUNSETTINGS_DIFF_EXIT=0`); the test passes under Workers=0 / ClassLevel in the pass-after and final runs. |
| Coverage: new module/class/method >= 90% | PASS (vacuous) | No new production module, class or method; the changed method remains 100% covered. |
| Coverage: changed lines not reduced | PASS | Line 359 (moved `TryRemove`) hits=1; lines 345–348, 351–353, 358, 360 hits=1; branch conditions 2/2 (346) and 4/4 (351) — read directly from the final Cobertura. |
| Test files excluded from the denominator | PASS | `coverage.config` excludes third-party and test modules; the JaCoCo projection lists production packages only. |

## 5. Test Coverage Detail

Per-file figures (first-party processed Cobertura, both stages read directly at the class element):

| File | Baseline | Post-change | Changed executable lines | Notes |
|---|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 143/143 lines (100%), 37/38 branches (97.37%) | 143/143 lines (100%), 37/38 branches (97.37%) | 1 (line 359, hits=1) | The single uncovered branch is the `RenderEngineName` null-name arm (line 367, 1/2), pre-existing and outside the change. Lines-valid unchanged at 143: the change adds no executable statement. |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | excluded (test) | excluded (test) | — | Harness hook only. |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` | excluded (test) | excluded (test) | — | New partial; its one test executed and passed (RESULT line in the pass-after and FINAL-FIXTURE-RUN sections). |
| `TaskMaster.Test/TaskMaster.Test.csproj` | not coverage-bearing | not coverage-bearing | — | One `Compile Include` at line 360, immediately after the Race entry. |

Repo-wide (package-level JaCoCo projection, identical package set at both stages): the -3 covered lines are UtilitiesCS -4 and QuickFiler +1; the -1 covered branch is UtilitiesCS. Denominators are identical at every package. The TaskMaster package is unchanged at 2443/3245 lines and 517/728 branches. This variation is run-to-run noise outside the changed file, consistent with the nondeterminism recorded for this suite on earlier reviews, and is recorded rather than attributed.

Thresholds evaluated: `.claude/rules/quality-tiers.md` 85% lines / 75% branches — met (85.31 / 79.72). CLAUDE.md 80% lines / 75% branches, 90% for new code — met (no new production code; changed method 100%). Observation PA-4 records the floor discrepancy between the two documents; the verdict is identical under either.

## 6. Test Execution Metrics

| Run | Command (abbreviated) | Total | Passed | Failed | Exit | Evidence |
|---|---|---|---|---|---|---|
| Baseline fixture (P0-T12) | vstest, coordinator class filter | 24 | 24 | 0 | 0 | `evidence/baseline/coordinator-tests-baseline.md` |
| Baseline suite with coverage (P0-T14) | dotnet-coverage collect + vstest, 9 assemblies, DIRECT filter | 7323 | 7323 | 0 | 0 | `evidence/baseline/coverage-baseline.md` |
| Fail-before (P1-T5) | vstest, coordinator class filter, base production file | 25 | 24 | 1 (new test, `BeSameAs`) | 1 (expected 1) | `evidence/regression-testing/prime-fault-ordering-fail-before.md` |
| Pass-after (P2-T4) | same command after the reorder | 25 | 25 | 0 | 0 | `evidence/regression-testing/prime-fault-ordering-pass-after.md` |
| Final fixture (P3-T7) | same command on the analyzer/nullable-rebuilt assembly | 25 | 25 | 0 | 0 | same file, FINAL-FIXTURE-RUN |
| Final suite with coverage (P3-T8) | dotnet-coverage collect + vstest, 9 assemblies, DIRECT filter | 7324 | 7324 | 0 | 0 | `evidence/qa-gates/coverage-post-change.md` |

Population arithmetic corroborated independently: 15 `[TestMethod]` + 1 `[DataTestMethod]` with 3 rows in the primary partial, 6 in the Race partial, 1 in the new partial = 25; suite 7323 + 1 = 7324.

## 7. Code Quality Checks

| Check | Result | Evidence |
|---|---|---|
| Formatting (CSharpier check) | PASS, no differences | toolchain-final-pass step 2 |
| Linting (.NET analyzers, `/t:Rebuild`) | PASS, 0 errors, 0 warnings, CoreCompile ran | toolchain-final-pass step 3 |
| Type checking (`/p:TreatWarningsAsErrors=true`, `/t:Rebuild`) | PASS, 0 errors, 0 warnings, CoreCompile ran | toolchain-final-pass step 4 |
| Testing (MSTest, coverage-enabled) | PASS, 7324/7324 | toolchain-final-pass step 5b |
| File-size ceiling | PASS, 420 / 470 / 77 | direct read |
| Host-path hygiene of committed evidence | PASS | `evidence/qa-gates/evidence-hygiene.md` (36 files, 0 account/machine/drive-profile hits); independent Grep of the feature folder for any drive-letter path (`[A-Za-z]:[\\/][A-Za-z]`) returned no match, so the tool-banner escape seen on #930 is absent here |
| Raw tool documents committed | PASS, none | footprint enumeration (`RAW-DOCS-COMMITTED: 0`, `RAW-DOCS-UNTRACKED-IN-FEATURE: 0` with `--ignored`); Glob of the feature folder lists Markdown only |

## 8. Gaps and Exceptions

| ID | Severity | Blocking | Description | Disposition |
|---|---|---|---|---|
| PA-1 | Low | No | The hook-canonical C# coverage path `artifacts/csharp/coverage.xml` is not populated in the item worktree; the DIRECT route writes `coverage/final-942.cobertura.xml`, which this review read directly, and the committed projection carries every figure. | Accept. Populate the canonical path only if a later gate requires the hook to parse it; do not commit the raw document (CLAUDE.md Committed Test Evidence Format). |
| PA-2 | Low | No | Toolchain step 4 ran the DIRECT route (inner collector invocation with four shell-icon test classes excluded) instead of `Invoke-MSTestWithCoverage.ps1` verbatim, per plan D-6 after the stall probe recorded a workstation-local `ArgumentException` in `ShellUtilities_Tests`. Baseline and final used the identical exclusion, so the comparison is like-for-like; CI executes the excluded classes. | Accept as a disclosed substitution. The PR CI `mstest-coverage` run remains the repo-wide gate for those four classes. |
| PA-3 | Informational | No | `quality-tiers.yml` is absent at the repository root (Glob), so tier-dependent gates (property-test density, mutation score) cannot be evaluated; stages 4, 6 and 7 of the seven-stage loop have no C# tooling in this repository. Pre-existing; unchanged by this branch. | Record only. |
| PA-4 | Informational | No | CLAUDE.md states 80% line / 90% new-code floors (maintainer decision 2026-09-11, #563) while `.claude/rules/quality-tiers.md` and `general-unit-test.md` state 85% / 75% uniform floors. Both are met here; no verdict depends on which governs. | Record only; maintainer-owned reconciliation. |
| PA-5 | Informational | No | PR context artifacts (`artifacts/pr_context.summary.txt` / `.appendix.txt`) do not exist in the item worktree; the session checkout holds a stale pair for a different branch (#936). They could not be regenerated (no shell or MCP). Scope was taken from the resolved base, the orchestrator's verified footprint and the executor's anchored `git diff --name-status` enumeration, and cross-checked against direct reads. | Record only. |

No exception to any unit-test rule was claimed by the change and none is needed.

## 9. Summary of Changes

| File | Status | Lines (+/-) | Nature |
|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | M | +10 / -5 | `CompletePrime`: `_primeTasks.TryRemove` moved after `_logError`; three-line why-comment; summary and `GetPrimeTask` returns documentation updated. Early return, failure unwrap, synthesized `TaskCanceledException` unchanged. |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | M | +12 / -1 | `Harness.OnLogError` (`Action<string, Exception>`, documented) invoked null-conditionally after `Errors.Add` in the error-log lambda; both hunks inside the `Harness` type; `[TestMethod]` count unchanged at 15. |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` | A | +77 | Third partial; one test `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`. |
| `TaskMaster.Test/TaskMaster.Test.csproj` | M | +1 / -0 | `Compile Include` for the new partial at line 360. |
| `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/**` | A | — | issue, spec (v0.3), research, plan (v1.4 with C1/C2), 37 evidence projections. |
| `docs/features/potential/promoted/2026-09-29-engine-toggle-prime-fault-logging-test-races.md` | A (inherited) | +60 | Promotion record written by the promotion commit; named as AC13's sole exemption. |

## 10. Compliance Verdict

**PASS.** 0 FAIL, 0 blocking PARTIAL. Non-blocking observations: PA-1 to PA-5. No remediation inputs are produced. The bugfix workflow (RED first, minimal fix, full toolchain) is evidenced end to end, the invariant the fix restores is stated in code and documentation, the regression test discriminates on program order rather than scheduling, and coverage of the changed file is unchanged at 100% lines with the moved statement covered.

## Appendix A: Test Inventory

Coordinator fixture (`TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests`, three partials), 25 executed tests in the pass-after and final runs:

| Partial | Test | Status (final) |
|---|---|---|
| Primary | `Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException` | Passed |
| Primary | `Constructor_WithNullInvalidateDelegate_ThrowsArgumentNullException` | Passed |
| Primary | `Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException` | Passed |
| Primary | `Constructor_WithNullLogErrorDelegate_ThrowsArgumentNullException` | Passed |
| Primary | `GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate` (3 data rows) | Passed x3 |
| Primary | `GetPressed_WithUnmappedKey_ReturnsFalseWithoutPrime` | Passed |
| Primary | `GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing` | Passed |
| Primary | `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` | Passed |
| Primary | `GetPressed_AfterPrimeCompletes_ReturnsPrimedValueAndInvalidatesMappedControl` | Passed |
| Primary | `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` (original reproduction, unchanged) | Passed |
| Primary | `ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder` | Passed |
| Primary | `ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged` | Passed |
| Primary | `ExecuteToggleAsync_WithUnmappedKey_ThrowsArgumentException` | Passed |
| Primary | `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` | Passed |
| Primary | `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing` | Passed |
| Primary | `HandleToggleClickAsync_WhenEnginesAvailable_TogglesAndInvalidates` | Passed |
| Race | `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` | Passed |
| Race | `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` | Passed |
| Race | `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` | Passed |
| Race | `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine` | Passed |
| Race | `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` | Passed |
| Race | `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` | Passed |
| PrimeFaultOrdering (new) | `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` | Failed at fail-before (expected); Passed at pass-after and final |

Suite: 7324 tests across 9 assemblies, 7324 passed, 0 failed, 0 skipped (derived), `TestCategory!=LiveOutlook` plus the four shell-icon classes excluded under the DIRECT route.

## Appendix B: Toolchain Commands Reference

Executed by the executor and recorded in `evidence/qa-gates/toolchain-final-pass.md` (none re-run by this review):

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `dotnet-coverage collect --output coverage\final-942.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-942.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\942\final" "/Logger:trx;LogFileName=final-942.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` followed by the runner's own post-processing (`ConvertTo-KoverageCoberturaXml`, threshold functions, first-party line, JaCoCo projection, trx summary) — the DIRECT substitution for `Invoke-MSTestWithCoverage.ps1` (PA-2)

Regression-test commands: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" ...` (fail-before P1-T5, pass-after P2-T4, final fixture P3-T7; identical apart from the results-directory and trx-name segments).
