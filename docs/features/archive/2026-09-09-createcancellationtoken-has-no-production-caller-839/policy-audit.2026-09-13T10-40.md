# Policy Compliance Audit — Issue #839 (createcancellationtoken-has-no-production-caller)

- Timestamp: 2026-09-13T10-40
- Branch: bug/createcancellationtoken-has-no-production-caller-839
- Head: 36c65b1875f89c0e3d885dc52324b60959ae3dbb
- Merge base with main: 2405a829d6afd3b12eb7c228d57158a97cb4e2ca
- Work mode: full-bug (spec.md is the sole acceptance-criteria source)
- Reviewer tooling: Read, Grep and Glob only. Git was unavailable to this review by caller directive; diff scope was taken from the caller's verbatim numstat and name-only listing and cross-checked against the executor's anchored diff artifacts and the live files in the item worktree.
- Review worktree: bugs-2026-09-11-item-839 (sibling of the session worktree; artifact paths in this document are repo-relative to that worktree)

## Executive Summary

Overall verdict: **PASS**. Zero blocking findings. Zero remediation-required findings. The change is ready to merge.

The branch adds one statement (`CreateCancellationToken();`) as the first statement of `QfcHomeController.Init()`, deletes one dead comment line plus its adjacent blank line to stay within the 500-line file limit, and adds one MSTest regression test. The bugfix workflow was followed in order (failing test first, minimal fix, full toolchain), the four-step C# toolchain passed on pass number 1 with non-vacuous Rebuild gates, the QuickFiler.Test population went from 1393/1393 to 1394/1394, and every evidence artifact is a Markdown projection under the feature folder with no raw tool output committed.

Coverage summary line for the changed language: C# coverage verdict: FAIL (procedural and pre-existing; dispositioned NON-BLOCKING; no remediation on this branch). Two mechanical conditions produce the FAIL token and neither is attributable to this diff: the canonical artifact `artifacts/csharp/coverage.xml` is absent by design under the issue-671 projections-only decision, and the modified file QfcHomeController.cs sits at 77.99% line coverage, below the 80% floor, having started at 77.91% before the change. Everything attributable to this diff passes: changed-line coverage is 100% (the single inserted executable line is hit), the per-file figure rose rather than regressed, and the new-code figure clears the 90% new-code floor.

Threshold basis applied: CLAUDE.md (rank 1 in the policy order) governs — repository-wide line 80%, new code 90%, no changed-line regression. The 85%/75% figures in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` are recorded as an observed, unreconciled conflict and are not applied (caller ruling citing issues 563 and 828; not independently verifiable in this session and recorded as an assumption). The verdict for the modified file is identical under either floor (77.99% is below both 80% and 85%), so the choice of floor does not change any verdict in this document.

| Area | Verdict |
|---|---|
| General Code Change Policy | PASS |
| General Unit Test Policy | PASS (one declared UT4 exception, inherited) |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Toolchain (format, analyzers, nullable, tests) | PASS |
| Coverage (changed language) | FAIL, non-blocking (see section 1.2.1) |
| Evidence location and hygiene | PASS |
| Blocking findings | 0 |

## Rejected Scope Narrowing

No scope narrowing was attempted by the caller. The caller's directive stated the full branch diff against the resolved merge base as the audit scope, supplied the complete anchored diff for both source files, and confirmed the full name-only listing (43 paths: two source files plus feature-folder documents and evidence). The Bash prohibition in the caller prompt is a tooling restriction with a stated, checkable reason (allow-list and cwd jointly unsatisfiable), not a reduction of audit scope; it was honoured and is recorded here for completeness.

The caller also asked that two files outside the branch diff (`scripts/vscode/TaskMaster.cli.runsettings` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`) be treated as candidates for a separate item rather than as a blocking finding against this branch. Those files are unchanged on this branch, so this is a disposition request about unchanged files, not a narrowing of the changed-file audit; the reviewer independently confirmed the defect they describe (section 7, finding F5) and agrees with the disposition.

## Evidence Location Compliance

Result: **PASS**. No file in the branch diff is written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`.

- Method: the caller's verified name-only listing (43 paths) and the executor's `evidence/qa-gates/scope-and-footprint.md` (37 paths at [P3-T23], all matching the three Write Set entries) were compared; every non-source path begins `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/`. A Glob of `artifacts/**` in the item worktree returned only `artifacts/orchestration/orchestrator-state.json` and three unrelated `pr_body_*` files, none of which is in the diff (`artifacts/` is gitignored at `.gitignore:57`).
- `validate_evidence_locations.py --root .` could not be executed in this session (no shell); the manual path scan above is the substitute and is recorded as such.
- Evidence kinds used: `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/` — all canonical `<FEATURE>/evidence/<kind>/` locations.
- Raw-output check: no path in the diff ends in `.trx`, `.xml` or `.coverage` (caller-verified; executor-verified at `scope-and-footprint.md` "Raw-artifact conditions"). Raw dotnet-coverage, msbuild and vstest logs were written under `coverage/` at the worktree root, which `.gitignore:144` (`coverage/*`) excludes.
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required; no caller instruction specified a non-canonical evidence path.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles (UT1)

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | New test uses the `[TestInitialize]` controller and its own loader lambdas; no static state; calls `Cleanup()` at the end (QfcHomeControllerTests.cs:233). |
| Isolation | PASS | Targets one behaviour: token-source creation order inside `Init()`. |
| Fast execution | PASS | Scoped run reported `[1 s]` (`evidence/regression-testing/init-token-source-scoped-pass.md:22`). |
| Determinism | PASS | No timers, no `Thread.Sleep`, no `Task.Delay`, no filesystem, no wall clock (`evidence/qa-gates/test-file-gates.md:32-33`; reviewer read of lines 165-234). |
| Readability | PASS | Arrange/Act/Assert comments present; XML doc comment states scenario, ordering rule and expected outcome. |

### 1.2 Coverage and scenarios (UT2)

Scope of the coverage evidence: the plan (Decision D5) measured the QuickFiler.Test assembly only, under dotnet-coverage with the repository's derived exclusion settings, and computed a per-file figure for QfcHomeController.cs by de-duplicating `line` elements across the 8 `class` elements that share that filename. The repository-wide floor was not measured by the plan and is recorded as unmeasured, not waived. The caller reports approximately 71.15% line and 59.91% branch repository-wide raw as a pre-existing condition; that figure could not be reproduced in this session and is not relied on for any verdict.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/baseline/coverage-baseline.md` (Markdown projection; raw Cobertura discarded per issue 671)
- C# post-change coverage artifact: `docs/features/active/2026-09-09-createcancellationtoken-has-no-production-caller-839/evidence/qa-gates/coverage-comparison.md` (Markdown projection; canonical `artifacts/csharp/coverage.xml` intentionally absent per issue 671, recorded FAIL in section 1.2.1)
- TypeScript baseline coverage artifact: zero TypeScript files changed on this branch; no artifact required
- TypeScript post-change coverage artifact: zero TypeScript files changed on this branch; no artifact required
- PowerShell baseline coverage artifact: zero PowerShell files changed on this branch; no artifact required
- PowerShell post-change coverage artifact: zero PowerShell files changed on this branch; no artifact required
- Python baseline coverage artifact: zero Python files changed on this branch; no artifact required
- Python post-change coverage artifact: zero Python files changed on this branch; no artifact required
- Per-language comparison summary: section 1.2.1 of this document

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 2 | 1394 | 1394 passed / 0 failed | 77.91% lines (QfcHomeController.cs, 201/258) | 77.99% lines (QfcHomeController.cs, 202/259) | 100% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 77.91% lines (201/258). Post-change: 77.99% lines (202/259). Change: +0.08% lines (+1 valid line, +1 covered line; the inserted statement at line 88 is hit, LINE88_HITS=1). New/changed-code coverage: 100%. Disposition: FAIL. Evidence: evidence/baseline/coverage-baseline.md, evidence/qa-gates/coverage-comparison.md; FAIL is procedural and pre-existing (canonical artifact absent per issue 671; modified file below the 80% floor before and after), dispositioned NON-BLOCKING because changed-line coverage is 100%, the per-file figure did not regress, and the new-code figure clears 90%.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Artifact expected by the review procedure | Observed | Disposition |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` | Absent in the item worktree (Glob of `artifacts/**`); absent by maintainer decision on issue 671 (projections only). Markdown projections present at the two paths in the checklist above. | FAIL token recorded per the absence rule; non-blocking. Creating the file was deliberately not done: it would have activated a repository-wide floor check against a pre-existing condition that this one-statement diff cannot move. |

Additional coverage observations:

- Branch coverage was not transcribed in any projection. The inserted statement is a straight-line method call with no conditional, so this diff cannot change any branch figure; the omission is a reporting gap, not a regression (code-review finding F4, Low).
- New-code 90% floor (CLAUDE.md UT2): the diff adds no new module, class or method; the single new executable line is covered (1/1 = 100%). PASS.
- Changed-line no-regression rule (CLAUDE.md UT2): per-file 77.91% -> 77.99%, numerator and denominator each +1, both explained by the diff. PASS.
- Repository-wide 80% floor (CLAUDE.md UT2): unmeasured by this plan (Decision D5). The plan's reasoning that a +1 covered / +1 valid line change cannot lower the repository rate is arithmetically sound. Recorded as unmeasured, not waived.
- Scenario completeness for the new behaviour: positive flow (source created, tokens equal, `CanBeCanceled` true) is covered; the negative flow (call placed after the datamodel loader) is pinned by the `CanBeCanceled` assertions on the datamodel and queue tokens rather than by a separate test, which the spec explicitly designed (spec.md:104, 168). PASS.

### 1.3 Structure and diagnostics (UT3)

PASS. Arrange/Act/Assert sections are labelled (QfcHomeControllerTests.cs:175, 220, 223). FluentAssertions messages are self-describing; the fail-before run recorded `Expected capturedSource not to be <null>.` (`init-token-source-fail-before.md:27`). The XML doc comment (lines 165-171) states the issue number, the scenario and the expected outcome.

### 1.4 External dependencies and environment (UT4)

PASS with one declared exception, inherited. `Init()` constructs a real `QfcFormViewer` (QfcHomeController.cs:91), a `Form`-derived type, so the new test constructs one too. This is pre-existing debt shared with `Init_InitializesCorrectly`, declared in spec.md ("Declared inherited exception", lines 170-172) and in the test's doc comment, per UT5. No temporary files, no network, no database, no Outlook interop object is created by the test (the `Mock<Outlook.Application>` in `Setup()` is pre-existing and strict, and `Init()` does not touch it).

### 1.5 Policy audit (UT5)

PASS. The exception above is called out explicitly in the spec and in the test source.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow step 1: failing regression test first | PASS | Test added at [P1-T1]; fail-before run exit 1 against the unfixed assembly (`init-token-source-fail-before.md:6-7, 18, 27`), before the production edit at [P2-T1]. |
| Bugfix workflow step 2: minimal targeted fix | PASS | Production numstat 1 added / 2 deleted; the single added line is the call; the deleted lines are a dead comment and a blank line (`production-file-gates.md:29-40`; reviewer read of QfcHomeController.cs:86-107 and 464-470). No opportunistic refactor. |
| Bugfix workflow step 3: verify locally, full toolchain in order | PASS | `toolchain-final-pass.md` PASS-NUMBER 1; per-step artifacts exit 0. |
| Design principles (simplicity, separation of concerns) | PASS | The fix reuses the existing factory; no new abstraction. |
| Error handling / logging | PASS | No change; spec records that no log line is added and why (spec.md:131-133). |
| 500-line file limit | PASS | QfcHomeController.cs 499 lines (was 500); QfcHomeControllerTests.cs 346 lines (was 275) (`file-size-audit.md:10-11`; reviewer read confirms 499 and 346). |
| Naming, docs, comments | PASS | Test name states the behaviour; doc comment explains why. |
| Dependencies | PASS | No new package; no csproj change (caller-verified name-only list; `scope-and-footprint.md:66`). |
| Match existing style | PASS | Loader-lambda arrangement mirrors `Init_InitializesCorrectly`; CSharpier clean. |
| Public API stability | PASS | No signature change; `Init()` keeps its return type. |
| Existing tests treated as spec | PASS | `Init_InitializesCorrectly` byte-identical (pure-insertion hunk `,0 +`, numstat deleted 0, `test-file-gates.md:44-48`); `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` passes (`init-token-source-pass-after.md:19`). |
| After-change reporting (commands run, all four passed) | PASS | `toolchain-final-pass.md` and the four per-step artifacts state the commands and results. |
| Supporting documents updated | PASS | spec.md check-offs; plan check-offs; evidence index. |

## 3. Language-Specific Code Change Policy Compliance

C# Code Change Policy (CLAUDE.md C#1-C#7).

| Requirement | Verdict | Evidence |
|---|---|---|
| Formatting via `dotnet tool run csharpier` (pinned 1.2.6), not global | PASS | `final-format.md:4-5, 12, 21`: format then check, 1624 files, zero `Was not formatted`, `FORMAT_CHANGED_OWNED_PATCH=False`. |
| Analyzers: msbuild `/t:Rebuild` with `EnableNETAnalyzers` and `EnforceCodeStyleInBuild` | PASS | `final-analyzers.md:4, 10-14`: `MSBUILD_EXIT=0`, `CSC_TASK_LINES=18`, `0 Error(s)`. Rebuild target used; compiler ran (non-vacuous). |
| Nullable: msbuild `/t:Rebuild` with `TreatWarningsAsErrors=true`, no `/p:Nullable=enable` | PASS | `final-nullable.md:4, 10-14`: `MSBUILD_EXIT=0`, `CSC_TASK_LINES=18`, `CS86_ERROR_LINES=0`; no solution-wide Nullable property passed. File carries no `#nullable` directive before or after (`production-file-gates.md:27`; reviewer grep of the live file found none). |
| Strong contracts, explicit types at public boundaries | PASS | No public surface change. |
| Null-safety | PASS | The fix establishes the invariant the downstream null guards protect (QfcFormController.Actions.cs:38, 75, 131 confirmed by reviewer grep). |
| File structure / 500-line limit | PASS | 499 lines. |
| Naming (PascalCase/camelCase) | PASS | Test locals camelCase; no new members. |
| Analyzer configuration unchanged; no suppressions added | PASS | No `.editorconfig`, `.globalconfig` or pragma change in the diff. |
| Command transport adaptations | PASS (recorded) | Each artifact records a `Set-Location -LiteralPath "REPO-ROOT"` prefix and, where applicable, a PowerShell quoting workaround, each with a stated reason and an assertion that semantics are unchanged. The reviewer accepts these as semantics-preserving: the msbuild switches, targets and properties match CLAUDE.md character-for-character apart from the two logging-only additions the plan declares. |

## 4. Language-Specific Unit Test Policy Compliance

C# Unit Test Policy (CUT1-CUT3).

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework (`[TestClass]`, `[TestMethod]`) | PASS | QfcHomeControllerTests.cs:22, 172. |
| Moq for mocks | PASS | `Mock<IQfcDatamodel>`, `Mock<IQfcExplorerController>`, `Mock<IQfcKeyboardHandler>`, `Mock<IQfcQueue>`, `Mock<IQfcFormController>` (lines 181-203). |
| FluentAssertions for new assertions | PASS | Seven `.Should()` assertions (lines 224-230); no MSTest `Assert` in the new method. |
| Toolchain command selection (CUT3) | PASS | Steps 1-3 as in section 3; step 4 `vstest.console.exe` with `/InIsolation` and the LiveOutlook filter under dotnet-coverage (`final-tests.md:4`). CI parity: the CI workflow at `.github/workflows/_mstest-coverage.yml:99` passes `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` and no settings file; the plan's Decision D16 aligned the local invocation with that. |
| Test file location | PASS | Repository convention is `<Project>.Test/` mirroring the production tree; the test sits in the existing `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` alongside its siblings. |
| Banned APIs in tests (`Thread.Sleep`, `Task.Delay`, wall clock) | PASS | Zero hits (`test-file-gates.md:32-33`; reviewer read). |

## 5. Test Coverage Detail

Per-file, QfcHomeController.cs (QuickFiler.Test assembly, dotnet-coverage, derived exclusions, de-duplicated across 8 class elements, max hits per line, both class-level and method-level views):

| Metric | Before | After | Delta |
|---|---|---|---|
| Valid lines | 258 | 259 | +1 |
| Covered lines | 201 | 202 | +1 |
| Line percent | 77.91 | 77.99 | +0.08 |
| Line 88 hits | 1 (was the datamodel-loader line) | 1 (now `CreateCancellationToken();`) | changed line covered |
| QuickFiler package line-rate (raw, unpost-processed) | 0.80567 | 0.80569 | +0.00002 |

Sources: `evidence/baseline/coverage-baseline.md:9-17`, `evidence/qa-gates/coverage-comparison.md:10-22`. Both sides were measured under one identical method (same assembly, same derived settings, same isolation switch, same filter, no runsettings file), so the comparison is not skewed.

Changed-line accounting: one executable line added (covered), zero executable lines removed (the deleted comment and blank line are not coverable). Changed-line coverage 1/1 = 100%.

The 57 uncovered lines in the file are pre-existing and are not enumerated in any projection; the reviewer did not reconstruct them (no Cobertura document is committed, by design).

## 6. Test Execution Metrics

| Run | Artifact | Total | Passed | Failed | Exit and notes |
|---|---|---|---|---|---|
| Baseline (pre-fix, whole assembly under coverage) | evidence/baseline/baseline-quickfiler-tests.md | 1393 | 1393 | 0 | Exit 0. Supersedes an earlier RED record (3 failures under class-level parallelism; see finding F5). |
| Fail-before (scoped, unfixed production) | evidence/regression-testing/init-token-source-fail-before.md | 1 | 0 | 1 | Exit 1 (expected 1). `Expected capturedSource not to be <null>.` |
| Scoped pass-after (fixed production) | evidence/regression-testing/init-token-source-scoped-pass.md | 1 | 1 | 0 | Exit 0. Same command as fail-before. |
| Pass-after (whole assembly, no coverage) | evidence/regression-testing/init-token-source-pass-after.md | 1394 | 1394 | 0 | Exit 0. Baseline + 1. |
| Final (whole assembly under coverage) | evidence/qa-gates/final-tests.md | 1394 | 1394 | 0 | Exit 0. Toolchain step 4. |

Named tests observed passing on the fixed tree: `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt`, `Init_InitializesCorrectly`, `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` (`final-tests.md:18-20`).

## 7. Code Quality Checks

Findings are detailed in `code-review.2026-09-13T10-40.md`. Summary:

| ID | Severity | Blocking | Summary |
|---|---|---|---|
| F1 | Low | No | Test teardown `_controller.Cleanup()` is not in a `finally`; on assertion failure the source and viewer are not cleaned up (same as the pre-existing sibling test). |
| F2 | Low | No | A second `Init()` call on the same instance now allocates a new source without disposing the previous one; pre-existing pattern in `EfcHomeController`; path has no live caller and is scheduled for removal (remedy d). |
| F3 | Low | No | `Init_InitializesCorrectly` now allocates a source it never disposes; AC8 permitted a trailing `Cleanup()` and Decision D2 declined it to keep the hunk pure-insertion. |
| F4 | Low | No | Branch figures not transcribed in the projections; no branch delta is possible from this diff. |
| F5 | Info | No (outside footprint) | `scripts/vscode/TaskMaster.cli.runsettings` (Workers 0, ClassLevel) diverges from CI and triggers three Deedle static-init failures; confirmed by reading the file and `_mstest-coverage.yml:99`. Separate item recommended. |
| F6 | Info | No (outside footprint) | More than 100 `.trx` files are tracked under other active feature folders (#501, #498, #468, #446 and others), contrary to the issue-671 projections-only decision; none is in this footprint. |
| F7 | Info | No | Spec cites `IQfcHomeController.cs line 12`; two files carry that name (`QuickFiler/Controllers/` declares `Init()` at line 12; `QuickFiler/Interfaces/` does not declare it). The remedy (d) follow-up should name the Controllers path. |

Tone check (`.claude/rules/tonality.md`): the spec, plan and evidence artifacts use neutral, evidence-first wording; the reachability finding is stated plainly as latent, not user-visible. PASS.

## 8. Documentation and Evidence Hygiene

| Check | Verdict | Evidence |
|---|---|---|
| No absolute host path, account name or machine name in the feature folder | PASS | Reviewer grep of the feature folder for the account token, `C:\Users`, `C:/Users` and machine-name patterns: zero hits. Executor gate `evidence-sanitization.md:9-12` (34 files, 0 hits, positive control 19578 hits). |
| Every command artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` | PASS | Spot-read of 24 artifacts; all carry the fields; the one expected-failure artifact carries `ExpectedExitCode: 1`. |
| Timestamps in `yyyy-MM-ddTHH-mm` | PASS | All artifacts. Timestamps are monotonic across the run (02-30 anchor through 06-25 commit-2). |
| Projections only (issue 671) | PASS | No `.trx`, `.xml` or `.coverage` added; raw output under gitignored `coverage/`. |
| Spec check-offs limited to `- [ ]` -> `- [x]` | PASS | Reviewer read of spec.md:211-222; criterion text matches the plan's quoted AC text. |
| user-story.md present under full-bug | PASS (not a defect) | Narrative only, no checkboxes (reviewer read); spec.md:9 and user-story.md:8 both state it is not an AC source. |

## 9. Remediation Triggers

None. The single FAIL token in this document (section 1.2.1) is procedural and pre-existing, and its two components have no remedy available on this branch: the canonical artifact is absent by maintainer decision (issue 671), and raising QfcHomeController.cs above the per-file floor would require testing the dead synchronous path that the spec's remedy (d) follow-up is scheduled to delete. No `remediation-inputs` artifact is produced.

Recommended follow-ups (caller's post-run actions, not conditions of merge):

1. File remedy (d), removal of the dead synchronous entry path (five symbols enumerated at spec.md:236), naming `QuickFiler/Controllers/IQfcHomeController.cs` explicitly.
2. File a separate item for `scripts/vscode/TaskMaster.cli.runsettings` class-level parallelism versus CI (finding F5).
3. File a separate item for the tracked `.trx` files under other feature folders (finding F6).

## 10. Verdict

**PASS. Ready to merge.** Zero blocking findings; zero remediation-required findings; twelve of twelve acceptance criteria verified (see `feature-audit.2026-09-13T10-40.md`).

## Appendix A: Test Inventory

| Test | File | Status on this branch | Role |
|---|---|---|---|
| `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` | QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:172-234 | New; fails before fix, passes after | Regression test for #839; pins ordering via `CanBeCanceled` on datamodel and queue tokens |
| `Init_InitializesCorrectly` | QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:112-163 | Unchanged; passes | Pre-existing `Init()` test; now also exercises the inserted call |
| `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` | QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs | Unchanged; passes | Issue #810 AC3 contract; disposal path the fix relies on |
| `InitAsync_InitializesCorrectly` | QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:248-311 | Unchanged; passes | Asynchronous path, untouched |
| QfcHomeControllerMetricsTests (direct `CreateCancellationToken()` caller at line 124) | QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | Unchanged | Pre-existing test invocation of the factory |
| Whole QuickFiler.Test assembly | — | 1394 passed / 0 failed | Population = baseline 1393 + 1 |

## Appendix B: Toolchain Commands Reference

| Step | Command (as recorded) | Artifact | Exit |
|---|---|---|---|
| 1 Format (write) | `dotnet tool run csharpier format .` wrapped with a before/after anchored-diff comparison | evidence/qa-gates/final-format.md | 0 |
| 1 Format (check) | `dotnet tool run csharpier check .` | evidence/qa-gates/final-format.md, evidence/qa-gates/post-checkoff-revalidation.md | 0 |
| 2 Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/839-final-analyzers.detailed.log"` | evidence/qa-gates/final-analyzers.md | 0 |
| 3 Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/839-final-nullable.detailed.log"` | evidence/qa-gates/final-nullable.md | 0 |
| 4 Tests | `dotnet-coverage collect --output coverage/839-final.cobertura.xml --output-format cobertura --settings coverage/839-effective-coverage.config -- vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:TestCategory!=LiveOutlook"` | evidence/qa-gates/final-tests.md | 0 |
| Regression pair | `vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /InIsolation "/logger:console;verbosity=normal" "/TestCaseFilter:FullyQualifiedName=QuickFiler.Controllers.Tests.QfcHomeControllerTests.Init_CreatesTokenSourceBeforeAnyLoaderObservesIt"` | evidence/regression-testing/init-token-source-fail-before.md (exit 1), init-token-source-scoped-pass.md (exit 0) | 1 / 0 |

All msbuild invocations were resolved through vswhere; the vstest binary is the `Common7\IDE\Extensions\TestPlatform` copy, matching CI. The reviewer did not re-run any command in this session.
