# Policy Compliance Audit: tests-depend-on-uncontrolled-environment (Issue #931)

- Component: QuickFiler.Test and UtilitiesCS.Test (test-only change; no production file changed)
- Branch: bug/tests-depend-on-uncontrolled-environment-931
- Head under review: ce744bbba3db4701e070785fd782c8b45289f2da (origin/main c4ff0e2be0bc9c51acc43dacd2cc5954a448676c merged at 55a50e922 with no conflicts)
- Resolved base: origin/main (merge base for the pre-merge evidence: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372; post-merge evidence measured at 55a50e922)
- Work mode: full-bug (spec.md is the sole acceptance-criteria source)
- Audit timestamp: 2026-09-29T20-15
- Reviewer: feature-review agent (no-Bash run; evidence read from committed projections, the current worktree files, the git-ignored raw coverage documents left in the worktree, and the orchestrator-supplied git facts)
- Total blocking findings in this artifact: 0
- Total non-blocking findings in this artifact: 8

## Executive Summary

Verdict: PASS. Zero blocking findings.

The branch rewrites two thread-identity tests and four file-handle tests so that their outcomes no longer depend on thread-pool scheduling or on which other process holds the repository solution file. It introduces one shared internal test helper (`DedicatedWorkerThread.Run`), splits a 490-line test class into two partial files (294 and 202 lines), and registers both new files in `QuickFiler.Test.csproj`. Six code files changed; every one is a test-project file. No file under `QuickFiler/`, `UtilitiesCS/`, `.claude/` or `config/` changed (orchestrator-supplied `git diff --name-only origin/main...HEAD`, corroborated by `evidence/qa-gates/p4-t11-scope-boundary.2026-09-29T09-44.md` and `p4-t33-closure.2026-09-29T09-47.md`).

Gate results (pre-merge Phase 4 and post-merge re-run, both exit 0): csharpier check clean (1625 files); analyzer rebuild 0 errors / 0 warnings / 0 skipped CoreCompile; TreatWarningsAsErrors rebuild 0 errors / 0 warnings / 0 skipped CoreCompile; QuickFiler.Test 1468 then 1469 executed, 0 failed; UtilitiesCS.Test 4922 then 4924 executed, 0 failed; repository-wide coverage run 7320 then 7323 executed, 0 failed. C# first-party coverage 85.32% lines / 79.73% branches pre-merge and 85.31% lines / 79.73% branches post-merge, both above the 85% line and 75% branch floors: C# coverage verdict PASS.

Four negative controls each show the rewritten test failing against a deliberately broken guard or sentinel and passing after the revert, with porcelain output proving no residual production change. Committed evidence is projections only; the feature folder contains no `.trx`, `.xml` or `.coverage` file and no absolute host path, account name or host name.

Non-blocking findings: PA-1 spec AC15 wording omits the promotion-lifecycle record that the branch necessarily carries (disclosed by the plan and the footprint gate); PA-2 `quality-tiers.yml` absent at repo root (pre-existing); PA-3 coverage-floor documentation conflict (CLAUDE.md 80%/90% versus `.claude/rules` 85%/75%), both satisfied here; PA-4 post-merge UtilitiesCS package line rate 0.893789 is below the pre-merge baseline 0.893884, attributable to production edits origin/main contributed plus run-to-run collector variance, with zero production lines changed by this item; PA-5 canonical `artifacts/csharp/coverage.xml` not emitted in the worktree, evidence held as committed projections plus git-ignored raw documents; PA-6 PR-context artifacts absent in the worktree, scope taken from orchestrator-supplied git facts; PA-7 disclosed command substitutions (`/nodeReuse:false`, DIRECT coverage route); PA-8 repository test layout `<Project>.Test/` rather than `tests/` (pre-existing convention).

## Rejected Scope Narrowing

None detected. The caller's prompt supplied a code diff restricted to QuickFiler.Test and UtilitiesCS.Test together with the complete `git diff --name-only origin/main...HEAD` list (seven entries including the feature folder and the promoted potential record), and this audit covers the complete list. The caller's binding no-Bash directive constrains the verification method, not the audit scope; where it prevented a direct observation the check is recorded as UNVERIFIED with the reason (section 8). The caller's instruction not to raise the four unfiled follow-ups as a finding is a disposition instruction about work the spec explicitly places out of scope (spec `## Rollout & Follow-up`), not a narrowing of language or file coverage; it is honoured and recorded in section 8.

## Evidence Location Compliance

- Files in the branch diff under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`: none. The orchestrator-supplied name list contains no `artifacts/` path; a Glob of `artifacts/**` in the worktree returns nothing (the worktree has no `artifacts/` directory).
- All evidence written by execution lives under `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/{baseline,regression-testing,qa-gates}/`, the canonical `<FEATURE>/evidence/<kind>/` layout. 43 evidence files pre-merge plus 8 post-merge files.
- `validate_evidence_locations.py --root .`: not run (no Bash in this run). Disposition by inspection: PASS. Recorded as UNVERIFIED-by-script in section 8.
- Canonical coverage artifact reconciliation: `artifacts/csharp/coverage.xml` is absent in the worktree. The committed coverage evidence is the CLAUDE.md-mandated projection form (`coverage-baseline.md`, `coverage-final.md`, `post-merge-coverage-final.md`), and the raw documents remain git-ignored under `coverage/` in the worktree (`baseline-931`, `final-931`, `postmerge-931`, `postmerge2-931` in both `.cobertura.xml` and `.jacoco.xml` forms). The reviewer read the four `.jacoco.xml` documents directly and confirmed each equals its committed projection counter for counter. Treated as artifact present (see finding PA-5).
- EVIDENCE_LOCATION_OVERRIDE_REJECTED: none (no caller instruction named a non-canonical evidence path).

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each rewritten test constructs and disposes its own viewer, dispatcher or stream; no shared static state introduced (diff read in full). Full assemblies pass under Workers=0 / ClassLevel parallelism (`parallel-suite-quickfiler-test.md`, `parallel-suite-utilitiescs-test.md`, post-merge twins). |
| Isolation | PASS | Each test targets one guard or one wrapper member; the in-thread precondition assertions name the guard under test in their reason strings. |
| Fast execution | PASS | Each rewritten test starts and joins exactly one thread; the post-merge QuickFiler.Test run of 1469 tests and UtilitiesCS.Test run of 4924 tests completed in the recorded runs with no timeout. |
| Determinism | PASS | The defect being fixed is non-determinism. The distinct-thread precondition now holds by construction (a `Thread` the test creates is never the calling thread); the file test opens a read-shared handle on the test host's own loaded image, which no other process can deny. Negative control M3 (`mutation-inline-precondition.md`) proves the preconditions are live. |
| Readability | PASS | Every rewritten test carries a `<summary>` and `<remarks>` explaining the mechanism and the issue reference; reason strings are explicit. |

### 1.2 Coverage requirements

Languages with changed files on the branch: C# only (`.cs` x 5 and `.csproj` x 1). TypeScript, Python and PowerShell have zero changed files.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/coverage-baseline.md` (committed JaCoCo package projection plus first-party summary; raw `coverage/baseline-931.jacoco.xml` and `.cobertura.xml` git-ignored in the worktree, read directly by the reviewer) - PASS
- C# post-change coverage artifact: `evidence/qa-gates/coverage-final.md` (pre-merge, MEASUREMENT 2) and `evidence/qa-gates/post-merge-coverage-final.md` (post-merge, MEASUREMENT 2); raw `coverage/final-931.*` and `coverage/postmerge2-931.*` git-ignored in the worktree - PASS
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.32% lines (56085/65737) / 79.73% branches (13595/17052) -> Post-change: 85.32% lines (56085/65737) / 79.73% branches (13595/17052). Change: 0.00% lines / 0.00% branches (identical first-party totals on the pre-merge final MEASUREMENT 2; per-package UtilitiesCS and QuickFiler LINE and BRANCH counters equal to baseline; the post-merge re-run reads 85.31% lines (56080/65736) / 79.73% branches (13597/17054), see section 5.3). New/changed-code coverage: N/A - no new executable production code (test-only change; zero production lines changed). Disposition: PASS. Evidence: evidence/baseline/coverage-baseline.md; evidence/qa-gates/coverage-final.md; evidence/qa-gates/post-merge-coverage-final.md; worktree coverage/*.jacoco.xml read directly.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 (5 .cs, 1 .csproj; all test-project files) | 8 rewritten or relocated; 1469 + 4924 executed post-merge | PASS (0 failed) | 85.32% lines / 79.73% branches | 85.32% lines / 79.73% branches (pre-merge final); 85.31% lines / 79.73% branches (post-merge) | N/A - no new production code |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |

C# threshold evaluation (single line for the record): C# repository-wide first-party coverage 85.31% lines and 79.73% branches post-merge (85.32% / 79.73% pre-merge) meets the 85% line floor and the 75% branch floor of `.claude/rules/quality-tiers.md`, and the 80% / 75% floors of CLAUDE.md UT2: C# coverage verdict PASS.

Per-file production coverage on the classes the rewritten tests exercise, read from the raw Cobertura documents (class rows, identical in baseline, final and post-merge documents): `QuickFiler\Viewers\BreadcrumbUiDispatcher.cs` line-rate 1.000, branch-rate 0.972; `UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs` line-rate 1.000, branch-rate 0.750. No changed-line regression is possible: zero production lines changed (`CHANGED-PRODUCTION-LINES: 0` in `coverage-final.md`, confirmed by the name-only diff).

### 1.3 Scenario completeness

| Scenario class | Verdict | Evidence |
|---|---|---|
| Positive flows | PASS | Owner-thread admission tests retained in the primary partial (4 tests); `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` positive escape. |
| Negative flows | PASS | Two cross-thread `ThrowsBoundaryDiagnostic` tests; `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` rejection path. |
| Edge cases | PASS | Null-owner escape; owner-only (null context) dispatcher. |
| Error handling | PASS | Exception type, message content and non-`ObjectDisposedException` asserted. |
| Concurrency | PASS | Every cross-thread case runs on a dedicated thread with an in-thread distinctness precondition; full suites pass under class-level parallelism. |
| State transitions | PASS | `BreadcrumbCoordinator` identity asserted before and after the repeated initialization. |

### 1.4 Structure, diagnostics, external dependencies

- Arrange-Act-Assert: PASS. The three relocated tests and the rewritten null-owner test carry explicit `// Arrange`, `// Act`, `// Assert` markers; `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` follows the unmarked compact style of its file (see code-review CR-3, informational).
- Clear failure messages: PASS. Every precondition assertion has a reason string; negative controls captured them verbatim (`mutation-inline-precondition.md`).
- No external dependencies: PASS. No network, database, external process or repository-tracked file. The `OpenRead` sentinel is the running host's own loaded assembly image opened read-only with `FileShare.ReadWrite` (six pre-existing opens of the same shape exist in the same file).
- Temporary files: PASS. No `File.Create`, `File.WriteAll`, `File.Delete`, `Path.GetTempFileName`, `Path.GetTempPath` in `FileInfoWrapper_Tests.cs` (Grep of the current file); the only `FileMode` is `Open` and the only `FileAccess` is `Read`. The rooted literal `C:\Repo\fixture.sln` is never opened or created.
- Determinism infrastructure: PASS. No `Thread.Sleep`, `Task.Delay`, `[Timeout]`, retry attribute or loop, `DateTime.Now`, or RNG in the added lines (`p4-t11-scope-boundary` seven `ADDED-` counts 0 over 306 added lines; reviewer Grep of the four QuickFiler.Test files confirms `Task.Run` 0 and `Thread.Sleep` 0; the only `GetAwaiter().GetResult()` occurrences are the pre-existing `PumpSynchronizationContext` drain at lines 322-333 of `BreadcrumbPopupBoundaryCoverageTests.cs`, unchanged).
- Parallel-regime invariants: PASS. `[DoNotParallelize]` occurrences in QuickFiler.Test remain exactly the two pre-existing ones (`Helper Classes/EmailMoveMonitorTests.cs:24`, `Helper Classes/ViewerQueueStaticWrapperTests.cs:11`); `TaskMaster.runsettings` SHA-256 unchanged across every run (`RUNSETTINGS-HASH-NOW` equals the P0-T4 value in every projection).
- Test file location: PASS with note. Tests live in `QuickFiler.Test/` and `UtilitiesCS.Test/`, the repository's established `<Project>.Test/` convention; no test file is colocated with production source. The `tests/` mirror-tree wording of `.claude/rules/general-unit-test.md` is a cross-repository rule not adopted by this repository's C# layout (PA-8, pre-existing).

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow step 1 (failing regression test first) | PASS with disclosed substitution | `evidence/regression-testing/fail-before-exception.2026-09-29T09-07.md` explains that a committed failing run is structurally impossible without mutating process-global thread-pool state or spawning a handle-holding process (both prohibited); plan D-13 substitutes four negative controls (M1-M4) as the deterministic observed-failing evidence. All four controls show the rewritten test failing against the mutation and passing after the revert. |
| Bugfix workflow step 2 (minimal, targeted fix) | PASS | Two affected sites rewritten; the twenty UNAFFECTED `Task.Run` sites untouched (name-only diff excludes all eight files AC3 names); no production file changed. |
| Bugfix workflow step 3 (full toolchain, restart on change) | PASS | `toolchain-pass.md` records one clean iteration in CLAUDE.md order; `post-merge-toolchain-pass.md` repeats all gates at 55a50e922. |
| Simplicity / reusability | PASS | The private helper is promoted to one shared `internal static` type with an identical body; duplication removed rather than added. |
| Separation of concerns | PASS | No I/O added to production; test helper contains only threading glue. |
| Fail fast / no broad catch | PASS with note | `DedicatedWorkerThread.Run` catches `Exception` on the worker and returns it to the caller for assertion; this is the capture mechanism at a test-helper boundary, documented in its remarks (code-review CR-2). |
| 500-line file limit | PASS | Current line counts (reviewer Read of each file; matches `p4-t9-post-format-census`): `DedicatedWorkerThread.cs` 48; `ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` 202; `ItemViewerBreadcrumbThreadAffinityTests.cs` 294 (was 490); `BreadcrumbPopupBoundaryCoverageTests.cs` 386 (was 361); `FileInfoWrapper_Tests.cs` 357 (was 359). `QuickFiler.Test.csproj` 570 lines is a project file, exempt. |
| Naming, docs, comments | PASS | `PascalCase` type and member, `camelCase` locals; XML `<summary>`/`<remarks>` on the helper and every rewritten test; comments explain why. |
| Dependencies | PASS | No package added; `packages.config` untouched. The `.csproj` analyzer version strings changed only through the merge from origin/main (MSTest.Analyzers 4.4.1, Meziantou.Analyzer 3.0.290), taken from main. |
| Public API compatibility | PASS | `DedicatedWorkerThread` is `internal` to the test assembly; no production API touched. |
| Supporting documents updated | PASS | spec.md acceptance criteria checked off; plan tasks checked off; evidence tree complete. |
| Committed Test Evidence Format (CLAUDE.md) | PASS | Projections only: JaCoCo package projections, one-line first-party summaries, TRX-derived summaries stating derived figures ("Skipped 0, derived as total minus executed"). No `.trx`, `.xml`, `.coverage` under the feature folder (Glob) or in the diff name list. |
| Tonality | PASS | Evidence artifacts and code comments are neutral and factual. |

## 3. Language-Specific Code Change Policy Compliance

C# (`.claude/rules/csharp.md`, CLAUDE.md C#1-C#7):

| Requirement | Verdict | Evidence |
|---|---|---|
| Formatting via `dotnet tool run csharpier` | PASS | `p4-t1-csharpier-format` REWRITTEN 0; `p4-t2-csharpier-check` exit 0, 1625 files (delta +2 for the two new `.cs` files); `post-merge-csharpier-check` exit 0, no file reported. |
| Analyzer rebuild (`/t:Rebuild`, EnableNETAnalyzers, EnforceCodeStyleInBuild) | PASS | `p4-t3-msbuild-analyzers` exit 0, 0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0, both test assemblies compiled; `post-merge-msbuild-analyzers` exit 0, 0/0, SKIP 0, merged analyzer versions loaded (MSTest.Analyzers.4.4.1 x36, Meziantou.Analyzer.3.0.290 x32). |
| Nullable / TreatWarningsAsErrors rebuild (`/t:Rebuild`, no `/p:Nullable=enable`) | PASS | `p4-t4-msbuild-nullable` exit 0, 0/0, SKIP 0, "no Nullable property override"; `post-merge-msbuild-nullable` exit 0, 0/0, SKIP 0. |
| Approved command forms | PASS with disclosure | Each msbuild invocation adds `/nodeReuse:false` (plan D-16; additive, changes no diagnostic) to avoid resident node-reuse workers, the very mechanism behind the #906 failure. Recorded verbatim in each artifact (PA-7). |
| Explicit `using` directives, `internal` for non-public | PASS | `System.Reflection` removed from the primary partial and added to Part2 where `FieldInfo` moved; helper is `internal static`. |
| No `dotnet format` | PASS | Not used anywhere in the evidence. |
| Analyzer stack / severity ordering | PASS (not touched) | No `.editorconfig` or `<Analyzer Include>` change originates on this branch. |
| Banned symbols (`Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `Random.Shared`) | PASS | None added (section 1.4). |
| Prohibited behaviours (weakening assertions, sleeps/retries/timing hacks, unrun toolchain) | PASS | Assertions were added (in-thread precondition, `captured` null, `BeSameAs(sentinel)`), none removed; no timing hack; toolchain evidence complete. |

## 4. Language-Specific Unit Test Policy Compliance

C# unit test policy (CLAUDE.md CUT1-CUT3, `.claude/rules/csharp.md` Testing Standards):

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework (`[TestClass]`, `[TestMethod]`) | PASS | Both partials share one `[TestClass]` on the primary declaration (plan D-2); every rewritten test carries `[TestMethod]`; no xUnit/NUnit. |
| Moq for mocks | PASS | `Mock<IFileInfo>(MockBehavior.Strict)` for the `OpenRead` seam; `Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` in the affinity tests. |
| FluentAssertions preferred | PASS | All assertions use `.Should()`; no MSTest `Assert` introduced. |
| Test toolchain step 4 (MSTest with coverage route) | PASS with disclosure | COVERAGE-ROUTE DIRECT (plan D-4): the runner's own inner `dotnet-coverage` collect around `vstest.console.exe` with the runner's functions, because the runner hard-codes its filter and cannot apply the recorded shell-icon exclusion; the same three committed forms are produced. Exit 0, 7320 then 7323 executed, 0 failed, LINE-FLOOR MET, BRANCH-FLOOR MET: PASS. |
| Deterministic test rules (no PATH/profile/cwd assumptions) | PASS | The old `AppDomain.CurrentDomain.BaseDirectory` walk-up to `TaskMaster.sln` is removed; the fixture is a rooted literal and the stream is the host's own image. |
| Seam-based mocking for filesystem | PASS | The existing internal `FileInfoWrapper(IFileInfo)` seam is used; no seam added to production. |
| New module/class coverage >= 90% | PASS (vacuous) | No new production module, class or method; `DedicatedWorkerThread` is test code outside the denominator. |
| Coverage regression on changed lines | PASS | Zero changed production lines. |

## 5. Test Coverage Detail

### 5.1 Baseline (pre-edit, 2026-09-29T09-05, merge base 177b6d78e)

First-party: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%). Package counters (JaCoCo projection, verified against `coverage/baseline-931.jacoco.xml`): QuickFiler LINE 10461 covered / 2293 missed, BRANCH 2518 / 699; UtilitiesCS LINE 38816 / 4608, BRANCH 9411 / 1858. Sum of the nine package LINE counters: 56085 covered, 9652 missed, 65737 total (reviewer arithmetic agrees with the first-party line).

### 5.2 Final (pre-merge, 2026-09-29T09-42, MEASUREMENT 2)

First-party: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%). QuickFiler LINE 10461 / 2293, BRANCH 2518 / 699; UtilitiesCS LINE 38816 / 4608, BRANCH 9411 / 1858. All four PACKAGE comparisons NOT-LOWER=True. MEASUREMENT 1 read QuickFiler LINE one covered line lower (10460) on an identical denominator with zero production change and was re-measured once per plan D-7; D-7 repeats the measurement, not a test, and no test failed in either measurement.

### 5.3 Post-merge (55a50e922, 2026-09-29T19-59, MEASUREMENT 2)

First-party: lines 56080/65736 (85.31%), branches 13597/17054 (79.73%). QuickFiler LINE 10461 / 2293, BRANCH 2518 / 699 (equal to baseline); UtilitiesCS LINE 38811 / 4612 (rate 0.893789 versus baseline 0.893884, NOT-LOWER=False), BRANCH 9413 / 1858 (rate 0.835152, NOT-LOWER=True).

Reviewer assessment of the UtilitiesCS LINE finding (PA-4, non-blocking): the executor's per-file attribution is corroborated by the raw Cobertura class rows. `UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILGlobals.cs` reads line-rate 0.95 (38/40) in baseline and final and 0.947368 (36/38) post-merge; `UtilitiesCS\Threading\UiThread.cs` reads 0.977444 (130/133) in baseline and final and 0.977612 (131/134) post-merge. Those two files were changed by origin/main (issue #930), not by this branch. Removing 2 covered lines and adding 1 covered line yields 38815/43423 = 0.893881, already below 0.893884 by 0.000003 on arithmetic alone; the remaining 4 covered lines move between unchanged files across the two identical measurements, which is collector variance. The comparison of a post-merge run against a pre-merge baseline is not like-for-like (denominator 43424 -> 43423) and no baseline exists at the merged main. This item changed zero production lines, so the no-regression-on-changed-lines clause has an empty subject. AC18 is defined on `coverage-final.md` (pre-merge, like-for-like), where every package comparison holds.

### 5.4 Floors

| Floor source | Line floor | Branch floor | Pre-merge | Post-merge | Verdict |
|---|---|---|---|---|---|
| `.claude/rules/quality-tiers.md`, `general-unit-test.md` | 85% | 75% | 85.32% / 79.73% | 85.31% / 79.73% | PASS (line margin 0.31 points post-merge) |
| CLAUDE.md UT2 (maintainer decision 2026-09-11, #563) | 80% | 75% | 85.32% / 79.73% | 85.31% / 79.73% | PASS |

The two floor documents disagree (PA-3, pre-existing); both are satisfied.

## 6. Test Execution Metrics

| Run | Assembly / route | Runsettings | Filter | Total | Passed | Failed | Evidence |
|---|---|---|---|---|---|---|---|
| Baseline | QuickFiler.Test | TaskMaster.runsettings, /InIsolation | none | 1468 | 1468 | 0 | `evidence/baseline/test-run-baseline.md` |
| Baseline | UtilitiesCS.Test | TaskMaster.runsettings, /InIsolation | shell-icon exclusion (4 classes named) | 4922 | 4922 | 0 | same |
| Baseline | 9 assemblies, coverage route DIRECT | TaskMaster.cli.runsettings, /InIsolation | LiveOutlook + shell-icon exclusion | 7320 | 7320 | 0 | `evidence/baseline/coverage-baseline.md` |
| Post-fix | QuickFiler.Test | TaskMaster.runsettings, /InIsolation | none | 1468 | 1468 | 0 | `evidence/regression-testing/parallel-suite-quickfiler-test.md` |
| Post-fix | UtilitiesCS.Test | TaskMaster.runsettings, /InIsolation | shell-icon exclusion | 4922 | 4922 | 0 | `evidence/regression-testing/parallel-suite-utilitiescs-test.md` |
| Post-fix | 9 assemblies, coverage route DIRECT (x2) | TaskMaster.cli.runsettings, /InIsolation | LiveOutlook + shell-icon exclusion | 7320 | 7320 | 0 | `evidence/qa-gates/coverage-final.md` |
| Post-merge | QuickFiler.Test | TaskMaster.runsettings, /InIsolation | none | 1469 | 1469 | 0 | `evidence/qa-gates/post-merge-parallel-suite-quickfiler-test.md` |
| Post-merge | UtilitiesCS.Test | TaskMaster.runsettings, /InIsolation | shell-icon exclusion | 4924 | 4924 | 0 | `evidence/qa-gates/post-merge-parallel-suite-utilitiescs-test.md` |
| Post-merge | 9 assemblies, coverage route DIRECT (x2) | TaskMaster.cli.runsettings, /InIsolation | LiveOutlook + shell-icon exclusion | 7323 | 7323 | 0 | `evidence/qa-gates/post-merge-coverage-final.md` |

Test-count arithmetic: QuickFiler.Test 1468 before and after the split proves the three relocated tests were discovered from the Part2 file and none was lost; the post-merge +1 is a test main contributed (`QfcItemController.UiThreadDispatcherFixtureTests.cs`). UtilitiesCS.Test 4922 before and after; post-merge +2 from main's changes to `ILGlobals_Tests.cs` and `UiThreadApartmentMeasurement_Tests.cs`. The eight `ItemViewerBreadcrumbThreadAffinityTests` and `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` names and the eight `FileInfoWrapper_Tests` names are listed as Passed in every relevant projection.

Negative controls (each: mutated run exit 1 with the predicted failure text; revert proven by `git diff --exit-code HEAD` 0, porcelain EMPTY, SHA-256 equal to the anchor; confirming run exit 0):

| Control | Mutation | Failed test(s) and message excerpt | Evidence |
|---|---|---|---|
| M1 | `BreadcrumbUiDispatcher.IsCurrentBoundary` thread-id branch replaced by `return true;` | `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`: "Expected executions to be 0, but found 1" | `mutation-owner-only-dispatcher-guard.md` |
| M2 | `ItemViewer.ThrowIfOffUiBoundary` null-owner `return;` replaced by the pre-#781 context-reference throw | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`: "Expected captured to be <null> ... but found System.InvalidOperationException" | `mutation-null-owner-escape.md` |
| M3 | `action();` inserted as first statement of `DedicatedWorkerThread.Run` | all four dedicated-thread tests fail at their precondition ("dedicated worker thread must not be ...") | `mutation-inline-precondition.md` |
| M4 | mock `OpenRead()` pointed at a second `FileStream` | `OpenRead_ShouldReturnReadableStreamForWrappedFile`: "Expected stream to refer to ..." | `mutation-openread-sentinel.md` |

## 7. Code Quality Checks

| Check | Command / method | Result |
|---|---|---|
| Confidentiality masking scan | Grep of the feature folder for `DanMoisan`, `C:/Users`, `C:\Users`, `DANMOI~1`, `Program Files`, `DESKTOP-`, `LAPTOP-`, `runUser`, `MachineName`, `COMPUTERNAME`, UNC prefixes; Grep `-o` for every drive-letter path | PASS. Zero account, profile, host or `Program Files` matches. The only drive-letter paths are the fixture literal `C:\Repo` (spec.md:89, plan lines 95/130/695, research line 259). The one `COMPUTERNAME` hit is the plan's sweep command reading `$env:COMPUTERNAME` as a token source, not a value. Placeholders `<repo-root>` used in the mutation projections. |
| Suppression scan (added lines) | Read of the full diff | PASS. No `#pragma warning disable`, `[SuppressMessage]`, `[ExcludeFromCodeCoverage]`, or `.editorconfig` change. |
| Workflow change scan | Name-only diff | PASS. No `.github/workflows` file changed on this branch. |
| Raw evidence document scan | Glob `**/*.{trx,xml,coverage}` under the feature folder; diff name list | PASS. Zero matches. |
| Coverage exclusion policy | Read of `coverage.config` | PASS (not changed). The `<ModulePath>` excludes name only third-party modules (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest); no production path excluded; file not in the diff. |
| Architecture boundaries | Read of the diff | PASS (not applicable to the diff content: no production code; test code adds no VSTO/Interop reference beyond the pre-existing `System.Windows.Threading.Dispatcher` and WinForms `Panel` uses already in these files). |
| Runsettings integrity | `RUNSETTINGS-HASH-NOW` in every projection | PASS. 199408CA...7FFA in every run, equal to P0-T4. |

## 8. Gaps and Exceptions

| ID | Severity | Blocking | Finding | Evidence | Disposition |
|---|---|---|---|---|---|
| PA-1 | Low | Non-blocking | Spec AC15 states the changed-file set "equals the Write Set entries plus documents inside the feature folder", but the branch diff also carries `docs/features/potential/promoted/2026-09-28-tests-depend-on-uncontrolled-environment.md`, written by the promotion-lifecycle commit (e13757267) before execution. The plan (D-11, Execution Conventions Clause A) defines the footprint gate as subtracting inherited paths, and `p4-t11-scope-boundary` records the subtraction explicitly as `INHERITED-PROMOTION-RECORD`. The AC's protective purpose (no production, `.claude`, or `config` change; no stray file) is met. | `p4-t11-scope-boundary.2026-09-29T09-44.md` lines 91-94, 123-124; plan D-11 | Accepted as a spec-wording residual; AC15 graded PASS with disclosure in the feature audit. Recommend the spec template name the promoted record in future footprint criteria. |
| PA-2 | Low | Non-blocking | `quality-tiers.yml` is absent at the repository root, so tier-dependent gates (property tests, mutation score) cannot be evaluated for any project. Pre-existing on main; outside the diff. | Glob `quality-tiers.yml` returns nothing | Pre-existing; record only. |
| PA-3 | Low | Non-blocking | Coverage-floor documentation conflict: CLAUDE.md UT2 states 80% line / 90% new-code; `.claude/rules/general-unit-test.md` and `quality-tiers.md` state 85% line / 75% branch. Both are satisfied here (85.31% / 79.73% post-merge). | Section 5.4 | Pre-existing; report against both; the policy-compliance-order skill lists CLAUDE.md first. |
| PA-4 | Low | Non-blocking | Post-merge UtilitiesCS package LINE rate 0.893789 is below the pre-merge baseline 0.893884 in both post-merge measurements. Attributable to production edits from origin/main (`ILGlobals.cs` -2 lines / -2 covered; `UiThread.cs` +1 / +1; both verified in the raw Cobertura class rows) plus 4 covered lines of run-to-run variance in unchanged files. Zero production lines changed by this item. | `post-merge-coverage-final.md`; section 5.3 | Not a regression of this branch; AC18 is defined on the pre-merge like-for-like comparison, which holds. No remediation. |
| PA-5 | Info | Non-blocking | Canonical `artifacts/csharp/coverage.xml` not emitted in the worktree. Coverage evidence is the CLAUDE.md-mandated committed projections plus the git-ignored raw documents under `coverage/`, which the reviewer read and reconciled. The session checkout holds a stale Cobertura document at that canonical path from another branch (line-rate 0.848316), which is not this branch's evidence. | Section "Evidence Location Compliance" | Artifact treated as present; C# coverage verdict PASS from the reconciled figures. |
| PA-6 | Info | Non-blocking | `artifacts/pr_context.summary.txt` / `.appendix.txt` absent in the worktree and the PR-context MCP unavailable in this no-Bash run; scope taken from the orchestrator-supplied `git diff --name-only origin/main...HEAD` list and the supplied three-dot code patch. UNVERIFIED by the reviewer: byte-identity of the eight AC3 files (established instead by their absence from the name-only diff), `validate_evidence_locations.py` execution, and a live `git merge-base`. | Caller prompt; `p4-t33-closure` diff listings | Best-effort assumptions documented; no finding depends on the unverified items. |
| PA-7 | Info | Non-blocking | Two disclosed command substitutions: `/nodeReuse:false` appended to every msbuild invocation (plan D-16) and the DIRECT coverage route replacing the runner entry point because the runner hard-codes its filter (plan D-4). Neither changes a diagnostic or a committed form. | `toolchain-pass.md`; `coverage-final.md`; plan D-4, D-16 | Accepted. |
| PA-8 | Info | Non-blocking | Repository C# test layout is `<Project>.Test/`, not the `tests/` mirror tree named in `general-unit-test.md`. Pre-existing convention; the new files follow it and no test is colocated with production source. | `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` | Pre-existing; record only. |

Follow-ups deliberately not filed on this branch (four potential entries in `evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md`): the promotion tools write under `docs/features/potential/`, which would contradict AC15, so the orchestrator files them from a separate branch and names them in the PR body. Not a finding against this branch (caller instruction, consistent with spec `## Rollout & Follow-up`).

## 9. Summary of Changes

- `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` (new, 48 lines): `internal static Exception Run(Action)` runs the delegate on a dedicated background thread, joins it without a timeout, returns the captured exception or null; no assertion, sleep, delay, timeout or retry.
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (490 -> 294 lines): class becomes `partial`; retains the four owner-thread admission tests, `InertOperations`, and the three nested helper types; loses the three cross-thread tests, `ClearViewerDispatcher` and `RunOnDedicatedWorkerThread`; `using System.Reflection` removed.
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` (new, 202 lines): the three cross-thread tests now calling `DedicatedWorkerThread.Run`; `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` rewritten to capture the owning `Dispatcher` before it is cleared and assert `owner.CheckAccess()` is false inside the delegate; `ClearViewerDispatcher` relocated.
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` (361 -> 386 lines): `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` rewritten from `Task.Run` + blocking wait to `DedicatedWorkerThread.Run` with an in-thread `CurrentManagedThreadId` distinctness assertion; remark added.
- `QuickFiler.Test/QuickFiler.Test.csproj`: two `Compile Include` entries added in the neighbouring backslash form (lines 99, 229); analyzer version strings updated by the merge from main only.
- `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` (359 -> 357 lines): `GetSolutionFile()` and the `AppDomain` walk-up removed; three metadata tests use the rooted literal `FixturePath`; `OpenRead` test routed through the internal `FileInfoWrapper(IFileInfo)` seam with a strict mock returning a test-owned `FileStream` over the test assembly image, asserting `BeSameAs(sentinel)`, `CanRead` and `Length > 0`.
- Feature folder: research record, spec, plan, 43 pre-merge and 8 post-merge evidence projections.

## 10. Compliance Verdict

PASS. Blocking findings: 0. Non-blocking findings: 8 (PA-1 to PA-8), none requiring remediation. No `remediation-inputs` artifact is produced.

## Appendix A: Test Inventory

Rewritten or relocated tests (all `[TestMethod]`, MSTest, FluentAssertions, Moq where a mock is used):

| Test | File | Change | Status (post-merge run) |
|---|---|---|---|
| `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | rewritten (dedicated thread, in-thread id precondition) | Passed |
| `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` | relocated and rewritten (owner captured before clear, in-thread `CheckAccess()` precondition) | Passed |
| `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | same | relocated; helper call retargeted | Passed |
| `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | same | relocated; helper call retargeted | Passed |
| `Properties_ShouldMirrorWrappedFileInfo` | `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` | rewritten (rooted literal fixture) | Passed |
| `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory` | same | rewritten (rooted literal fixture) | Passed |
| `OpenRead_ShouldReturnReadableStreamForWrappedFile` | same | rewritten (seam + test-owned sentinel) | Passed |
| `ToString_ShouldDelegateToWrappedFileInfo` | same | rewritten (rooted literal fixture) | Passed |

Unchanged tests in the touched classes (all Passed): `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext`, `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow`, `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow`, `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow`; `Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException`, `PropertyDelegates_ShouldMirrorMockedIFileInfo`, `StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo`, `AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo`; the four other tests of `BreadcrumbPopupBoundaryCoverageTests`.

Test helper (not a test): `QuickFiler.Test.TestSupport.DedicatedWorkerThread.Run(Action)`, four call sites.

## Appendix B: Toolchain Commands Reference

Commands as recorded in `evidence/qa-gates/toolchain-pass.md` and `post-merge-toolchain-pass.md` (CLAUDE.md order; each exit 0 in the final pass):

1. `dotnet tool run csharpier format .` (REWRITTEN 0) then `dotnet tool run csharpier check .` (Checked 1625 files).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` plus `/nodeReuse:false` (0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0).
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` plus `/nodeReuse:false` (0 errors, 0 warnings, SKIP_CORECOMPILE_LINES 0).
4. MSTest-with-coverage route, DIRECT: `dotnet-coverage collect --output-format cobertura --settings coverage\effective-coverage-931.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&<four shell-icon class exclusions>" "/ResultsDirectory:coverage\test-results\931\<stage>" "/Logger:trx;LogFileName=<stage>-931.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, post-processed with the runner's own helpers into the JaCoCo package projection and first-party summary: exit 0, floors met, PASS.

Parallel-suite runs: `vstest.console.exe <assembly> "/Settings:TaskMaster.runsettings" /InIsolation [shell-icon exclusion filter for UtilitiesCS.Test] "/ResultsDirectory:coverage\test-results\931\<task>" "/Logger:trx;LogFileName=<task>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`.

Reviewer verification method (this run): Read, Grep and Glob over the worktree only; no command executed. Git facts (head, base, merge commit, name-only diff) were supplied by the orchestrator and cross-checked against `p4-t11-scope-boundary` and `p4-t33-closure`.
