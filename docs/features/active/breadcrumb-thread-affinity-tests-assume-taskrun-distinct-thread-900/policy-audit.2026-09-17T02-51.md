# Policy Audit — Issue #900: breadcrumb thread-affinity tests assume Task.Run yields a distinct thread

- Timestamp: 2026-09-17T02-51
- Reviewer: feature-review agent (hand-authored; see Template Resolution Deviation)
- Branch: `bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` @ `3538c3617edb1c0e26186bec2a2e3aa26171b7d1`
- Base: `origin/main` @ `66b65a4626095ade5a01643aee4a43c90cc58cbf` (fetched in the execution worktree before every diff; `git merge-base origin/main HEAD` = `66b65a46`, and `git merge-base --is-ancestor origin/main HEAD` exits 0, so `origin/main` is an ancestor of the head and the two-dot and three-dot forms are the same diff by construction; their agreement is a property of the ref topology and is not independent confirmation)
- Execution worktree: `<repo-root>/.claude/worktrees/agent-acb02d4502ebff3b7` (clean working tree at review time: `git status --porcelain` printed nothing)
- Work mode: `full-bug` (`issue.md` line 4); `spec.md` is the sole acceptance-criteria source; `user-story.md` is absent by design
- Feature folder: `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900`

## Template Resolution Deviation

The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` are not exposed to this session. This artifact is hand-authored and preserves all thirteen canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5. No template instruction block was copied, so none remains.

## Executive Summary

Verdict: **PASS**. Blocking findings: **0**. Non-blocking findings: **3 Low, 4 Info** (itemized in `code-review.2026-09-17T02-51.md`). Remediation inputs: not produced.

The branch contains exactly one source change, `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (+101/-30, 419 -> 490 lines), plus 46 feature-folder documents and five inherited `.claude/agent-memory/` files. No production file, no `.csproj`, no `packages.config`, no workflow, and no runsettings file changed (`git diff --name-status origin/main..HEAD`, read directly). `scripts/vscode/TaskMaster.cli.runsettings` diffs to zero bytes against `origin/main` and still reads `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>`.

The fix makes the scheduling property controlled rather than tolerated: the two `_WorkerThread_ThrowsBoundaryDiagnostic` tests now run the guarded call on a `System.Threading.Thread` the test constructs and joins, and assert the guard's exact predicate (`scope.Viewer.UiDispatcher.CheckAccess()` is false) inside the delegate before the guarded call. The branch introduces no serialisation, `[DoNotParallelize]`, retry, sleep, wall-clock wait, `[Timeout]`, timed `Join`, or widened tolerance (diff token scan, read directly: the only `Join(` occurrences are one untimed `thread.Join();` and three documentation mentions).

Toolchain: CSharpier check exit 0 with the file hash unchanged; analyzer rebuild exit 0 with 0 errors and 0 warnings; nullable rebuild exit 0 with 0 errors; repository-wide vstest 7288/7288 under the CLI runsettings. The reviewer independently re-ran the seven-test class in the execution worktree under the unchanged runsettings (SHA-256 `98EF03A8...CEF57`, equal to the executor's anchor): 7/7 passed, exit 0.

C# coverage verdict: PASS (repository line coverage 85.26% is at or above the 85% floor in `.claude/rules/quality-tiers.md` and the 80% floor in CLAUDE.md; branch coverage 79.67% is at or above the 75% floor; no production line changed; changed-code coverage is NOT MEASURABLE by construction, judged honest in section 5).

PowerShell coverage verdict: PASS (vacuous: zero PowerShell files changed on this branch, so no PowerShell coverage threshold is engaged; recorded defensively because the session-cwd `artifacts/pr_context.summary.txt` is a stale copy from the #895 branch rather than this branch's own context).

TypeScript and Python: zero changed files on this branch.

## Rejected Scope Narrowing

No scope narrowing was detected in the caller directive. The directive named the single source change and the feature-folder documents, which is the full `origin/main..HEAD` diff (52 paths, re-derived by the reviewer), and asked that the changed-code figure recorded as `NOT MEASURABLE` be judged for honesty rather than demanded. That instruction was evaluated on its merits in section 5 and was not treated as a waiver: every language with changed files carries an explicit verdict above, and the repository-wide figures were read from the Cobertura root element directly rather than from the executor's transcription. The five `.claude/agent-memory/` files the directive marked as inherited were still scanned for host identifiers and prohibited evidence paths (section 7); they are inside the audited diff.

## Evidence Location Compliance

- Files in the branch diff under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`: **0** (`git diff --name-only origin/main..HEAD` filtered by prefix). PASS.
- Every evidence artifact lives under `<FEATURE>/evidence/<kind>/` with `<kind>` in `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other` (46 files enumerated). PASS.
- `artifacts/csharp/coverage.xml` exists in the execution worktree, is git-ignored (`.gitignore:57`), is untracked, and is a review-tooling input rather than evidence; PASS.
- `validate_evidence_locations.py` is not present in this repository (searched the session tree); the scan above was performed manually with `git diff --name-only` and is recorded as the substitute.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none; the caller supplied no non-canonical evidence path.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Independence (any order) | PASS | The two rewritten tests own a `ViewerScope` each, construct their own `Thread`, and share no static state; the class-level parallel run (`Workers=0`, `ClassLevel`) passed 7/7 in P0-T9, P4-T3, P5-T5 and the reviewer's own run. |
| Isolation (one behaviour per test) | PASS | Each test targets one guarded member (`InitializeBreadcrumbPipeline`, three-argument `ConfigureBreadcrumbDropDown`); both guards are the first statement of their member (`ItemViewer.Breadcrumb.cs:51`, `:233`, read directly). |
| Fast execution | PASS | Reviewer run durations 0.045 s and 0.047 s for the two rewritten tests. |
| Determinism | PASS | This is the defect fixed. A `Thread` object the test constructs cannot be the `Thread` object captured by `Dispatcher.CurrentDispatcher` in the viewer constructor (`ItemViewer.cs:27`), so `CheckAccess()` (object-identity comparison) is false under any scheduler. Mutation M2 (P3-T3) observed the precondition failing when the delegate ran inline; mutation M1 (P3-T1) observed the boundary assertions failing when the guard was escaped. |
| Readability / intent | PASS | Descriptive names retained; XML `<remarks>` on both tests and the helper explain the mechanism and why the untimed join is safe; `BeFalse`/`NotBeNull` carry reason strings. |
| Arrange–Act–Assert | PASS | Comment markers present in both tests; Act is the helper call, Assert is the four captured-exception assertions. |
| Scenario completeness | PASS | Negative cross-thread flow is the subject; positive owning-thread flows and the null-owner escape remain covered by the five unchanged siblings. |
| No external dependencies / no temporary files | PASS | No I/O, no network, no file creation in the diff (token scan for `File.`, `Path.GetTemp`, `Directory.` on added lines: 0). |
| Banned test APIs (`Thread.Sleep`, `Task.Delay`, wall-clock waits) | PASS | Census 0 each (P1-T1, P2-T1, P5-T7) and reviewer grep of the diff. `Thread.Join()` is a completion wait on a bounded synchronous call, not a wall-clock wait, and carries no timeout. |
| Test file location | PASS (pre-existing convention) | The C# test projects in this repository use `<Project>.Test/` mirrors; the file's location is unchanged. |
| Coverage exclusion policy | PASS | No `exclude` entry, `coverage.config`, or `[ExcludeFromCodeCoverage]` attribute was added or changed on this branch. The pre-existing `[ExcludeFromCodeCoverage]` on `ItemViewer` (`ItemViewer.cs:20`) predates the branch and is recorded in section 8. |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-900.cobertura.xml` (git-ignored; present in the execution worktree; six root attributes transcribed in `evidence/baseline/p0-t10-mstest-coverage.2026-09-17T02-18.md`)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` (present in the execution worktree; Cobertura root element `coverage`; `line-rate="0.852566"`, `branch-rate="0.796675"`, `lines-valid="65616"`, read directly by the reviewer)
- TypeScript baseline coverage artifact: zero TypeScript files changed on this branch; no artifact required
- TypeScript post-change coverage artifact: zero TypeScript files changed on this branch; no artifact required
- PowerShell baseline coverage artifact: zero PowerShell files changed on this branch; no artifact required
- PowerShell post-change coverage artifact: zero PowerShell files changed on this branch; no artifact required
- Python baseline coverage artifact: zero Python files changed on this branch; no artifact required
- Python post-change coverage artifact: zero Python files changed on this branch; no artifact required
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.27% lines (55948/65616) / 79.69% branches (13564/17022). Post-change: 85.26% lines (55942/65616) / 79.67% branches (13561/17022). Change: -0.01% lines (-6 covered lines on an identical 65616-line denominator; run-to-run variation of a nine-assembly parallel instrumented run, with no production line changed). Disposition: PASS. Evidence: `evidence/baseline/p0-t10-mstest-coverage.2026-09-17T02-18.md`, `evidence/qa-gates/p5-t6-coverage-delta.2026-09-17T02-36.md`, and the root element of `artifacts/csharp/coverage.xml` read directly.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 1 | 7288 | PASS | 85.27% lines / 79.69% branches | 85.26% lines / 79.67% branches | N/A |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

The last cell of the C# row carries no percentage because the branch adds no executable production line: the only changed file is a test assembly excluded from instrumentation by the derived settings (`.*\.Test\.dll$`), and the production type it exercises carries `[ExcludeFromCodeCoverage]`. See section 5.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Simplicity first | PASS | One private static helper (18 lines) replacing two `Task.Run(...).GetAwaiter().GetResult()` chains; no new abstraction or indirection. |
| Reusability | PASS | The helper is the same shape as the in-repo precedent `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` `ApartmentThreadRunner`; it is kept private per the file's own remark (lines 21-28) that this class declares its helpers locally rather than widening another file's accessibility. |
| Extensibility / public API | PASS | No public API changed; the helper is `private static`. |
| Separation of concerns | PASS | Test-only change; production guard untouched. |
| Fail fast / no silent errors | PASS | The helper's `catch (Exception)` captures rather than swallows: the captured value is asserted `NotBeNull`, exact type, message, and not `ObjectDisposedException` immediately after the join, and an unexpected type fails the exact-type assertion. |
| Logging | PASS (no console output added; a test helper has no production logging obligation) | Diff read directly. |
| File size <= 500 lines | PASS | 490 lines at head (`wc -l`), up from 419. Headroom is 10 lines; recorded as a Low finding in the code review. |
| Naming | PASS | `RunOnDedicatedWorkerThread`, `captured`, `isOwnerThread`, `thread`, `error` are descriptive. |
| Docs and comments (why, not what) | PASS | `<summary>` and `<remarks>` on the helper and both tests explain the inlining mechanism and the join-safety argument. |
| Mandatory toolchain loop, restart on failure | PASS | P5-T1 -> P5-T7 executed in order; iteration 1 failed at P5-T5 on an environmental file-contention failure in an unrelated `UtilitiesCS.Test` test and the loop restarted from P5-T1 (`ITERATIONS: 2`, `LOOP: CLEAN PASS` in P5-T8). Stages 4, 6 and 7 of the seven-stage rules-file loop have no repo-defined command for this solution and are not part of the CLAUDE.md C# toolchain; the four CLAUDE.md steps were run. |
| Bugfix workflow (failing regression test first) | PASS with documented exception | A deterministic failing run of the original tests is unattainable without mutating process-global `ThreadPool` state; the spec (Test Strategy item 1) pre-authorised the `fail-before-exception` dossier in place of a failing run, and the two mutation runs supply the observed-failing evidence for the replacement assertions. |
| Dependencies | PASS | No package added; `packages.config` and `.csproj` unchanged. |

## 3. Language-Specific Code Change Policy Compliance

Language in scope: C# only.

| C# requirement | Verdict | Evidence |
| --- | --- | --- |
| CSharpier via `dotnet tool run` (pinned 1.2.6) | PASS | Scoped `format` + `check` (P2-T2): file SHA-256 identical before/after, check exit 0. Repository-wide `format` (P5-T1): anchored patch hash identical before/after, `FORMAT_CHANGED_TREE: False`; `check .` (P5-T2): `Checked 1641 files`, exit 0. |
| Analyzer rebuild `/t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS | P5-T3: exit 0, `ERRORS: 0`, `WARNINGS: 0`, `FILE-DIAGNOSTIC-LINES: 0`, `CSC_OUT_LINES: 2` (proves `CoreCompile` ran for `QuickFiler.Test`). |
| Nullable rebuild `/t:Rebuild ... /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`, no `/t:Build`) | PASS | P5-T4: exit 0, `ERRORS: 0`, `WARNINGS: 0`, `FILE-DIAGNOSTIC-LINES: 0`. The file carries no `#nullable` directive (unchanged from base), so it is outside per-file nullable analysis; `Exception captured = null;` introduces no CS86xx diagnostic. |
| Explicit types at boundaries; `var` only when obvious | PASS | `private static Exception RunOnDedicatedWorkerThread(Action action)`; `Exception captured`; `bool isOwnerThread`; `var thread = new Thread(...)` is obvious from the initializer. |
| Null-safety | PASS | `captured` is null only when the delegate completes normally, and that case is asserted against with `NotBeNull(reason)`. |
| Resource safety | PASS | The thread is `IsBackground = true` and joined before return; `ViewerScope` remains the disposing owner of the viewer. |
| XML documentation on non-obvious members | PASS | Helper has `<summary>` and `<remarks>`. |
| No `dotnet format`; no global csharpier | PASS | Only `dotnet tool run csharpier` appears in `Command:` fields. |
| No `.csproj`/`.props`/`.targets` edit | PASS | `git diff --name-status` shows none. |

## 4. Language-Specific Unit Test Policy Compliance

| C# unit-test requirement | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework (`[TestClass]`, `[TestMethod]`) | PASS | Unchanged attributes; `[TestMethod]` census 7 before and after. |
| Moq for mocks | PASS | `new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` retained in the first test. |
| FluentAssertions for assertions | PASS | `Should().BeFalse(...)`, `NotBeNull(...)`, `BeOfType<InvalidOperationException>()`, `Message.Should().Contain(...)`, `NotBeOfType<ObjectDisposedException>()`. `BeOfType` is an exact-type check in FluentAssertions 8.10.0, so the derived `ObjectDisposedException` is excluded by it as well as by the explicit assertion (D-2). |
| No xUnit/NUnit introduced | PASS | Diff read directly. |
| Toolchain command selection (format -> analyzers -> nullable -> vstest) | PASS | Order and commands per section 3; the test step ran `vstest.console.exe` under `dotnet-coverage collect` with `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, the repository's LiveOutlook exclusion and the four documented workstation hang exclusions, which is the shape `scripts/vscode/Invoke-MSTestWithCoverage.ps1` uses. |
| Parallel execution preserved (`Workers=0`, `ClassLevel`) | PASS | Runsettings unchanged versus `origin/main` (zero-byte diff); every scoped and repository-wide run passed `/Settings:` and recorded `RUNSETTINGS-HASH-NOW` equal to the pre-change anchor. |

## 5. Test Coverage Detail

Repository-wide (Cobertura root attributes, post-processed by `ConvertTo-KoverageCoberturaXml`):

| Attribute | Baseline (P0-T10) | Final (P5-T6, and `artifacts/csharp/coverage.xml` read directly) | Difference |
| --- | --- | --- | --- |
| `line-rate` | 0.852658 | 0.852566 | -0.000092 |
| `branch-rate` | 0.796851 | 0.796675 | -0.000176 |
| `lines-covered` | 55948 | 55942 | -6 |
| `lines-valid` | 65616 | 65616 | 0 |
| `branches-covered` | 13564 | 13561 | -3 |
| `branches-valid` | 17022 | 17022 | 0 |

Floors: 85.26% clears the 85% line floor (`.claude/rules/quality-tiers.md`) by 0.26 points and the 80% CLAUDE.md floor by 5.26 points; 79.67% clears the 75% branch floor. The verdict is the same under either line floor, so the unreconciled 80-versus-85 wording difference between CLAUDE.md and the rules files does not affect this audit.

Changed-code accounting, judged: the executor recorded `CHANGED-CODE COVERAGE: NOT MEASURABLE` with three reasons, each verified by the reviewer:

1. No production line changed: `git diff --name-status origin/main..HEAD` lists one path under `QuickFiler.Test/` and none under `QuickFiler/`.
2. The test assembly is outside the instrumented denominator: `ConvertTo-DerivedCoverageSettingsXml` appends `.*\.Test\.dll$` as a module exclusion, and `ThreadAffinityTestsClassCount=0` in both P0-T10 and P5-T6.
3. The production type under test is excluded in source: `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/ItemViewer.cs:20`, read directly; `ItemViewer.Breadcrumb.cs` is a partial of that class; `ItemViewerBreadcrumbClassCount=0` in both runs.

The accounting is honest. A changed-line percentage has no denominator here, and the substitute behavioural evidence is the correct proof for a test-only fix: two pass-after runs (P4-T2, P4-T3), the repository-wide run (P5-T5), and two mutation runs with pre-declared failing assertions (P3-T1, P3-T3). The -6 covered lines on an identical denominator are consistent with the documented run-to-run variability of this repository's nine-assembly parallel instrumented runs and cannot be attributed to a test-only edit whose exercised production path (guard throws as the first statement) is identical before and after the fix in the branch that previously passed.

`ItemViewer`'s `[ExcludeFromCodeCoverage]` is pre-existing (ratified under the CLAUDE.md COM/VSTO/WinForms exemption) and is not introduced, widened, or relied upon by this branch; it is recorded in section 8 for completeness and is Not Blocking.

## 6. Test Execution Metrics

| Run | Scope and settings | Result | Exit | Artifact |
| --- | --- | --- | --- | --- |
| P0-T9 baseline class | 7 tests, CLI runsettings, unfixed file | 7/7 passed (`ORIGINAL-FLAKE-OBSERVED: NO`) | 0 | `evidence/baseline/p0-t9-mstest-thread-affinity-class.2026-09-17T02-15.md` |
| P0-T10 baseline repository-wide | 9 assemblies, CLI runsettings, `dotnet-coverage collect` | 7288/7288 passed | 0 | `evidence/baseline/p0-t10-mstest-coverage.2026-09-17T02-18.md` |
| P2-T4 pair, pre-mutation | 2 tests | 2/2 passed | 0 | `evidence/regression-testing/p2-t4-pair-run-before-mutation.2026-09-17T02-22.md` |
| P3-T1 mutation M1 (guard escaped) | 2 tests, expect-fail | 0/2 passed; both fail on `captured.Message.Should().Contain(...)` | 1 (expected) | `evidence/regression-testing/p3-t1-mutation-guard-disabled.2026-09-17T02-24.md` |
| P3-T3 mutation M2 (delegate inline) | 2 tests, expect-fail | 0/2 passed; both fail on `isOwnerThread ... BeFalse` | 1 (expected) | `evidence/regression-testing/p3-t3-mutation-inline-precondition.2026-09-17T02-25.md` |
| P4-T2 pair, post-revert (measured AC6) | 2 tests, runsettings hash equal to anchor | 2/2 passed | 0 | `evidence/regression-testing/p4-t2-pass-after-scoped.2026-09-17T02-26.md` |
| P4-T3 class, post-revert (measured AC7) | 7 tests | 7/7 passed | 0 | `evidence/regression-testing/p4-t3-pass-after-class.2026-09-17T02-27.md` |
| P5-T5 iteration 1 repository-wide | 9 assemblies | 7287/7288; one environmental failure (`FileInfoWrapper_Tests.OpenRead...`, `TaskMaster.sln` held by a resident MSBuild node) | 1 | `evidence/other/p5-t5-iteration-1-environmental-failure.2026-09-17T02-32.md` |
| P5-T5 iteration 2 repository-wide | 9 assemblies | 7288/7288 passed; seven in-scope results `Passed` | 0 | `evidence/qa-gates/p5-t5-mstest-coverage.2026-09-17T02-35.md` |
| Reviewer re-run (this review, 02-49) | 7 tests, `QuickFiler.Test.dll` built 02:34 by P5-T4, CLI runsettings | 7/7 passed | 0 | `TestResults/900/review-class/review-class.trx` (git-ignored, execution worktree) |

The iteration-1 failure was handled correctly: no test, setting, or tolerance changed; idle MSBuild node-reuse workers spawned by the plan's own `/m` rebuilds were terminated and the loop restarted from P5-T1 as the plan directs.

## 7. Code Quality Checks

| Check | Method | Result |
| --- | --- | --- |
| Confidentiality masking scan | Added lines of the full `origin/main..HEAD` diff (52 paths, including the five agent-memory files) grepped for drive-letter `Users` paths, the account name, and the machine name | 0 / 0 / 0 hits. The executor's own sweep (P5-T13) reports 0 / 0 over 42 files; the four artifacts written after it are covered by the reviewer's scan. |
| Suppression scan (added lines) | `#pragma`, `SuppressMessage`, `[ExcludeFromCodeCoverage]`, `<NoWarn>` on added lines | 0. |
| Workflow change scan | `.github/workflows/**` in the diff | 0 files. |
| Banned test API scan | `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize`, `SpinWait`, `Stopwatch`, `DateTime.Now`, `Join(<arg>)` on the source diff | 0; one untimed `thread.Join();`. |
| Runsettings integrity | `git diff origin/main..HEAD -- scripts/vscode/TaskMaster.cli.runsettings` byte count; file content | 0 bytes; `Workers 0`, `Scope ClassLevel`. |
| File size check | `wc -l` on the head file | 490 (limit 500). |
| Evidence field census | 40 evidence `.md` files scanned for `Timestamp:`, `Command`, `EXIT_CODE:`, `CHANNEL:`, `Output Summary` | All command-step artifacts carry `Timestamp:`, `Command`, `EXIT_CODE:` and `CHANNEL:`. Five command-step artifacts lack any `Output Summary` field or heading (P2-T1, P5-T8, P5-T9, P5-T15, P5-T5 iteration 1); their content sits under differently named headings. Recorded as Low. |
| Timestamp consistency | Artifact `Timestamp:` field versus filename stamp; stamps versus commit and reflog times | All 39 stamped filenames equal their field. The P5-T15 closure record is stamped `02-40` but is contained in the amend committed at 02:39:57 (reflog); a seconds-scale skew, recorded as Info. |
| Hash provenance | Reviewer SHA-256 of the head file and of the CRLF-normalized `origin/main` blob | `8EBC19F8...BB164` equals the executor's `FIX-HASH`; `CE87F6C2...A1CA` equals the executor's `PRE-EDIT-HASH`. The mutation tokens never entered history (`git log -S` over the branch range returns nothing). |

## 8. Gaps and Exceptions

1. **Changed-code coverage figure absent by construction** (section 5). Not a gap in evidence; the substitute is the observed-failing mutation evidence. Not Blocking.
2. **Five command-step artifacts lack the `Output Summary` field** (P2-T1 uses `## Census`; P5-T8 `## Reconciliation`; P5-T9 `## The four path lists`; P5-T15 `## Commit 1`; P5-T5 iteration 1 `## What happened`). The required observations are present under those headings. Low, non-blocking; future artifacts should carry the literal field.
3. **Closure record stamp skew**: `p5-t15-closure.2026-09-17T02-40.md` is inside the commit amended at 02:39:57. Info; no evidence value depends on it.
4. **PowerShell verdict written as vacuous PASS** rather than as a bare non-applicability marker, defensively: the session-cwd `pr_context.summary.txt` (head `144ff45b`, branch `bug/fsharp-core-hintpath-netstandard21-skew-895`) is stale; its hook-parsed changed-language set is C# only, which the reviewer confirmed by calling the hook's own `Get-ChangedLanguageSet` against it. The execution worktree has no `pr_context` summary at all, so the reviewer derived scope from `git diff` against `origin/main`. Assumption recorded.
5. **MCP template and validator tools unavailable**; hand-authored artifacts (see Template Resolution Deviation).
6. **`validate_evidence_locations.py` absent from the repository**; manual prefix scan substituted (Evidence Location Compliance).
7. **Pre-existing `[ExcludeFromCodeCoverage]` on `ItemViewer`** keeps the guard under test outside the coverage denominator. Ratified exemption; not introduced here; Not Blocking.
8. **Attribution trailer omitted on all five commits** by operator directive (the pre-implementation gate rejects any command line containing an angle bracket). Not a finding, per the directive.
9. **CI-versus-cold-worktree framing of the analyzer HintPath skew** (follow-up 3 in the code review): the executor observed `CS0006` on this cold worktree at P0-T7; the caller states CI is green on cold checkouts. `.github/workflows/_build-analyzers.yml:35-58` caches `packages/` with a bare-prefix `restore-keys` fallback, so a CI runner is not cold, and the caller's inference is not established from CI being green. Recorded for the coordinator; out of scope here.
10. **Test file headroom**: 490/500 lines. Low.

## 9. Summary of Changes

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (+101/-30): both `_WorkerThread_ThrowsBoundaryDiagnostic` tests replace `Task.Run(...).GetAwaiter().GetResult()` with `RunOnDedicatedWorkerThread(...)`, add an in-delegate precondition `scope.Viewer.UiDispatcher.CheckAccess().Should().BeFalse(...)`, and assert on the captured exception (`NotBeNull`, `BeOfType<InvalidOperationException>`, `Message.Contain(<operation>)`, `NotBeOfType<ObjectDisposedException>`); new private static helper constructs a background `Thread`, runs the delegate in `try`/`catch (Exception)`, `Join()`s without a timeout, and returns the captured exception; XML remarks added. The third `Task.Run` (line 332, `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`) is deliberately unchanged and out of scope.
- Feature folder: `issue.md`, `spec.md` (8/8 AC checked), `plan.2026-09-16T23-27.md` (46/46 tasks checked), research artifact, 40 evidence artifacts across five kinds, two commit-message files.
- Inherited: five `.claude/agent-memory/` files (atomic-planner and task-researcher) written during preparation; scanned, accepted per directive.

## 10. Compliance Verdict

**PASS.** 0 blocking findings. The branch is ready to merge from a policy-compliance standpoint. The Low/Info items in section 8 do not require a remediation cycle; `remediation-inputs` is not produced.

## Appendix A: Test Inventory

`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (7 `[TestMethod]`s; all `Passed` in P4-T3, P5-T5 and the reviewer's run):

| Test | Status on this branch |
| --- | --- |
| `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` | unchanged |
| `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | unchanged |
| `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | unchanged |
| `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | unchanged |
| `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | rewritten Act/Assert; remarks added |
| `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | rewritten Act/Assert; remarks added |
| `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | unchanged (still uses `Task.Run`; out of scope; follow-up 2) |

Helper added: `RunOnDedicatedWorkerThread(Action)` (private static, lines 373-403).

## Appendix B: Toolchain Commands Reference

Executor commands (repository-relative form as recorded in the evidence `Command:` fields; tools resolved through `vswhere`):

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` (P5-T1, P5-T2; scoped variants in P2-T2)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (P0-T7, P5-T3)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (P0-T8, P5-T4)
4. `dotnet-coverage collect --output coverage\<stage>-900.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-900.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&..." "/ResultsDirectory:TestResults\900\<stage>" "/Logger:trx;LogFileName=<stage>-900.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` (P0-T10, P5-T5)
5. Scoped: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:<class or pair>" "/ResultsDirectory:TestResults\900\<task>" "/Logger:trx;LogFileName=<task>.trx"` (P0-T9, P2-T4, P3-T1, P3-T3, P4-T2, P4-T3)

Reviewer commands (check-only, executed in the execution worktree via `git -C <worktree>` or a scratchpad `pwsh -NoProfile -File` script):

- `git fetch origin main`; `git rev-parse HEAD origin/main`; `git merge-base origin/main HEAD`; `git merge-base --is-ancestor origin/main HEAD`; `git status --porcelain`
- `git diff --numstat origin/main..HEAD`; `git diff --name-status origin/main..HEAD`; `git diff --numstat origin/main...HEAD | wc -l`
- `git diff origin/main..HEAD -- scripts/vscode/TaskMaster.cli.runsettings | wc -c`; `git diff 63142ec73..HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs | wc -c`; `git log -S"ClearViewerDispatcher(scope.Viewer);" origin/main..HEAD -- <file>`
- `sha256sum` of the head file; `git show origin/main:<file> | sed 's/$/\r/' | sha256sum`
- `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests" "/ResultsDirectory:TestResults\900\review-class" "/Logger:trx;LogFileName=review-class.trx"` (7/7 passed, exit 0)
