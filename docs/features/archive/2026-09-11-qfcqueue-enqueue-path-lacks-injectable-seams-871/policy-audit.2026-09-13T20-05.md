# Policy Audit — Issue #871, QfcQueue enqueue-path injectable seams

- Component: `QuickFiler` (C# / .NET Framework 4.8 VSTO add-in), `QuickFiler.Test`
- Date: 2026-09-13
- Reviewer: feature-review agent
- Work mode: `full-bug` (from `issue.md` line 12) — acceptance-criteria source is `spec.md` only
- Diff anchor supplied by the caller: `8213826f695439e86e3ed34faa575de493a11ec7`
- Branch head supplied by the caller: `8277b0c4c`
- PR base branch: `main`
- Files under test: `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, `QuickFiler/Controllers/QfcQueue.Tlp.cs`, `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, `QuickFiler/Interfaces/IUiIdleDispatcher.cs`, `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`, `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`

## Executive Summary

The change adds six injectable seams to the `QfcQueue` enqueue path, splits a file that was already
seven lines over the 500-line ceiling into three partial parts, introduces one narrow internal
interface, and adds a 28-case headless MSTest regression suite. The full toolchain passes in a single
pass in the mandated order with the formatter rewriting nothing, both rebuild gates report zero errors
and zero warnings, and the test assembly reports 1423 of 1423 passed at `failed=0` against a baseline
of 1395.

Overall verdict: **PASS**. **No finding in this review is Blocking.**

Twenty-one of the twenty-two acceptance criteria are earned on the evidence. One, AC22, is graded
PARTIAL: its universal clause ("The final diff contains no change to any file outside the Write Set")
is contradicted by three tracked Markdown files under `.claude/agent-memory/orchestrator/` that the
branch diff carries and that the spec's Write Set does not list. AC22's substantive clause — the three
pre-existing QfcQueue test files, the UtilitiesCS threading types and the UtilitiesCS control-clone
extension all unmodified, with the full suite green — is verified and holds. The criterion has been
un-checked in `spec.md` per the acceptance-criteria-tracking protocol. The correct remedy is a one-line
amendment to the spec's Write Set or to AC22's wording; no code change is implied, and the orchestrator
should not treat this as a remediation trigger against the implementation.

Two evidence-strength observations are recorded rather than waived. First, both raw Cobertura documents
were deleted after their last consumer under the ratified issue-671 evidence-hygiene convention, so
every per-file and per-line coverage figure in this item is executor-attested and cannot be re-derived
by a third party from the committed tree; the one figure I re-derived independently is the item-825
reference document used by the repository-wide projection. Second, the repository-wide figure is a
projection over a committed reference document plus this item's package delta, not a measurement,
because a whole-solution local run is blocked on this host by four shell-icon test classes in another
assembly.

## Rejected Scope Narrowing

No caller instruction narrowed the audit scope, and no language with changed files on this branch was
excluded from evaluation. Three caller directives were examined against the scope invariant and none is
a narrowing:

1. "Do NOT create or request `artifacts/csharp/coverage.xml`." Evaluated and accepted. This instructs
   the reviewer not to *manufacture* an artifact, not to skip a coverage check. The coverage obligation
   is discharged in full below from the committed Markdown projections, and the absence of the canonical
   artifact is recorded as an explicit FAIL row in section 5 rather than suppressed.
2. "The 85 percent / 75 percent figures in the two rule files are not authoritative here; apply
   CLAUDE.md's 80 / 90." Evaluated and accepted as a precedence ruling, not a narrowing. CLAUDE.md
   states its own compliance order and places itself first, and it names neither rule file. Both floor
   sets are evaluated below and reported; the change clears the 80 percent line floor and the 90 percent
   new-code floor, and the projected repository-wide figure would also clear an 85 percent line floor.
   The divergence between the two documents is recorded as an unresolved repository documentation
   conflict, not as a finding against this item.
3. "Do not report the unfixed running-jobs counter leak itself as a finding of this item." Evaluated and
   accepted as an attribution rule. I verified independently that the defect is pre-existing (the
   increment at `QfcQueue.Enqueue.cs:94` precedes the `try` at line 101 whose `finally` at line 126 holds
   the decrement at line 128, and no hunk of the anchored diff touches any of those three lines) and that
   it is separately promoted. AC21's two clauses are verified on their merits below. The defect is
   therefore attributed to its own follow-up record rather than to this item; it is not suppressed.

## Evidence Location Compliance

Every evidence artifact this item produced lies under `<FEATURE>/evidence/<kind>/`, split across the
three canonical kinds `baseline`, `qa-gates` and `regression-testing`. I scanned the complete
name-status listing of the anchored branch diff reproduced in
`evidence/qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md` for any path under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. There are none.

| Prohibited evidence root | Occurrences in the branch diff | Verdict |
|---|---|---|
| `artifacts/baselines/` | 0 | PASS |
| `artifacts/qa/` | 0 | PASS |
| `artifacts/evidence/` | 0 | PASS |
| `artifacts/coverage/` | 0 | PASS |

`validate_evidence_locations.py --root .` was not executed: the Bash tool was withheld for this review
by binding caller directive, and no alternative execution path was available. The scan above was
performed by direct inspection of the committed name-status listing and of the feature folder tree, and
it covers the same four prohibited roots the script checks. Verdict: PASS by inspection.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every test builds its own queue through `NewQueue()` / `NewHeadlessQueue()`; all recording collections are instance fields re-created per test instance. No static or shared mutable state is written by the suite. |
| Isolation | PASS | Each test targets one behaviour of the enqueue path; the six seam-contract tests target one seam each. |
| Fast execution | PASS | The whole assembly, 1423 cases, completes in 12.78 s under the coverage runner (`evidence/qa-gates/p5-t5-coverage-postchange.2026-09-12T10-25.md`). |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `.Result`, `.Wait()`, filesystem, network or process API appears in either new test file; verified by direct pattern search over `QfcQueueEnqueueTests*.cs`, which returned no matches. The substituted dispatcher runs every marshalled callback inline on the calling thread. |
| Readability and maintainability | PASS | Every test method carries an XML summary stating its scenario, and names follow `Member_Condition_ExpectedOutcome`. |

The `[TestInitialize]` that calls `SynchronizationContext.SetSynchronizationContext(null)` was examined
specifically. It is a legitimate determinism fix, not a hidden dependency: constructing the two
`TableLayoutPanel` fields in the class's field initializers installs a `WindowsFormsSynchronizationContext`
on the MSTest worker thread, and the enqueue path's first genuinely asynchronous await would then post
its continuation back to a thread no unit-test host pumps. Clearing the context makes continuations
complete on the thread pool. The suite is therefore removing an ambient dependency rather than adding
one. One weakness is recorded as a Low code-review finding: `[TestCleanup]` disposes the panels but does
not restore the previous context, so the class leaves the worker thread's ambient context null.

### 1.2 Coverage and Scenarios

Scenario completeness for the members under test is strong. Positive flow (AC11), negative flows
(AC10, both guards), error handling on both catch arms (AC12), boundary conditions on the digit-width
ternary at totals of 9, 10 and 11 (AC16), state transitions on the running-jobs counter observed
mid-flight and after each outcome (AC13), and both arms of the null-conditional event invocation
(AC14) are all covered.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/p0-t12-coverage-baseline.2026-09-12T10-25.md`
- C# post-change coverage artifact: `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p5-t5-coverage-postchange.2026-09-12T10-25.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 81.05% lines (10,215/12,603). Post-change: 81.69% lines (10,314/12,626). Change: +0.64% lines (+99 covered, +23 valid). New/changed-code coverage: 95.83%. Disposition: PASS. Evidence: evidence/qa-gates/p5-t5-coverage-postchange.2026-09-12T10-25.md and evidence/qa-gates/p6-t2-new-code-coverage.2026-09-12T10-25.md.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Coverage artifact | Verdict | Disposition |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` is absent from the canonical hook path; the measured figures live in committed Markdown projections | FAIL | Non-blocking. Both raw Cobertura documents were written, consumed, and then deleted under the ratified issue-671 evidence-hygiene convention that permits committing projections only. Every figure survives in five committed Markdown artifacts. The file was deliberately not generated by this review. |
| TypeScript | `coverage/lcov.info` | N/A | Zero TypeScript files changed on this branch. |
| Python | `artifacts/python/lcov.info` | N/A | Zero Python files changed on this branch. |
| PowerShell | `artifacts/pester/powershell-coverage.xml` | N/A | Zero PowerShell files changed on this branch. |

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 9 | 28 added | 1423/1423 passed | 81.05% | 81.69% | 95.83% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.3 Test Structure and External Dependencies

| Requirement | Verdict | Evidence |
|---|---|---|
| Arrange-Act-Assert | PASS | Every test body separates arrangement, the awaited call, and the assertions. |
| Clear failure messages | PASS | FluentAssertions throughout; `AssertSeamContract` supplies an explicit `because` string. |
| No external services | PASS | No Outlook process, no live WPF dispatcher, no filesystem, no network. `UiThread.Dispatcher` throws `InvalidOperationException` until `UiThread.Init()` runs on an STA thread, and `UiThread.Init` is never called anywhere in `QuickFiler.Test`; that is what makes the headless-construction assertion in AC7 a real discriminator rather than a vacuous one. |
| Temporary files prohibited | PASS | No temporary file is created. The viewer stand-in is an in-memory object produced by `FormatterServices.GetUninitializedObject`. |
| Test file location | PASS | Both new files live in `QuickFiler.Test/Controllers/`, mirroring `QuickFiler/Controllers/`, alongside the three pre-existing QfcQueue test files. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | Each seam is a property over a backing field with a null guard. No framework, no container, no generic runner was introduced. |
| Reusability | PASS | The three marshalling members became one-line forwards onto a single abstraction; the three former bodies moved verbatim into one adapter type. |
| Extensibility | PASS | `IUiIdleDispatcher` declares exactly three members and is implemented by one production adapter and one test fake. |
| Separation of concerns | PASS | The untestable regions (process-wide dispatcher marshalling, reflection-driven control clone, live WinForms row placement) are now behind named seams instead of inlined in the queue class. |
| File size limit, 500 lines | PASS | Post-format counts: 269, 200, 329, 108, 35, 425 and 343. All seven strictly under 500. `QfcQueue.cs` stood at 507 before the change, so the split was a precondition rather than a cleanup. Evidence: `evidence/qa-gates/p5-t6-line-counts-final.2026-09-12T10-25.md`, independently corroborated by reading each file. |
| Error handling, fail fast | PASS | Six seam setters throw `ArgumentNullException`. The two pre-existing catch clauses on the enqueue path are unchanged byte for byte. |
| Logging | PASS | The single `logger.Error` call and its interpolated message are byte-identical to the anchor. |
| Public API stability | PASS | All 16 public and protected member declarations of `QfcQueue` at the anchor are present unchanged. All six seams are `internal`. Nothing was added, removed, retyped or re-signed on the public surface. |
| Dependencies | PASS | No package reference added. The two `.csproj` edits are `<Compile Include>` items only. |
| Bugfix workflow, failing test first | PARTIAL | Non-blocking, and the deviation is argued rather than asserted. `evidence/regression-testing/fail-before-exception.2026-09-12T10-25.md` shows the fail-before step is structurally unavailable for this defect class: the defect is untestability itself, so a seam-substituting test cannot compile before the seam exists and a non-substituting test fails identically before and after. I agree with the reasoning. The only test that *could* have failed before and passed after is a reflection assertion that the seam member exists, which would be vacuous. The absence-of-test premise is measured, not assumed: zero `EnqueueAsync` matches across the three named pre-existing QfcQueue test files, with the folder-wide count of 7 recorded separately so the narrowed search scope is auditable. |

## 3. Language-Specific Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, pinned version via `dotnet tool run` | PASS | `evidence/qa-gates/p5-t1-format.2026-09-12T10-25.md` and `p5-t2-check.2026-09-12T10-25.md`: exit 0, 1632 files inspected, none named. The rewrote-nothing property rests on two byte-identical porcelain captures taken either side of the format command, not on the exit code. |
| `dotnet format` not used | PASS | No `dotnet format` invocation appears in any evidence artifact. |
| Analyzer gate with `/t:Rebuild` | PASS | `evidence/qa-gates/p5-t3-analyze.2026-09-12T10-25.md`: exit 0, 0 errors, 0 warnings, with `/t:Rebuild /m`, `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true`. `/t:Build` was not substituted. |
| Nullable gate with `/t:Rebuild`, no solution-wide `Nullable=enable` | PASS | `evidence/qa-gates/p5-t4-nullable.2026-09-12T10-25.md`: exit 0, 0 errors, 0 warnings. The command carries `TreatWarningsAsErrors=true` and does not carry `/p:Nullable=enable`, which CLAUDE.md section C#1 records as load-bearing. |
| Strong contracts and explicit APIs | PASS | Each seam declares its full delegate type explicitly; each carries an XML summary and an `<exception>` tag. |
| Null-safety per-file opt-in | PASS | Exactly one `#nullable` directive exists across the seven Write Set code paths, in the brand-new `IUiIdleDispatcher.cs`. No relocated file gained one, which matters because adding one would conscript relocated bodies into nullable analysis and break the verbatim-move property. |
| Composition over inheritance | PASS | The adapter is `internal sealed` and implements the interface; no type hierarchy was introduced. |
| `internal` preferred for non-public API | PASS | All six seams, the interface and the adapter are `internal`. `MoveMonitor` must be `internal` because `IEmailMoveMonitor` is itself internal; a public member of that type is an inconsistent-accessibility error. I confirmed the same constraint does not force the other five, which follow the convention for consistency. |
| Narrow, documented suppression | PASS | The single `#pragma warning disable CS0618` / `restore CS0618` pair is pre-existing, appears exactly once each, brackets only the async-enumerable projection, and carries an in-code rationale. Neither directive appears in any hunk of the anchored diff. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[DataTestMethod]`, `[DataRow]`, `[TestInitialize]`, `[TestCleanup]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference. |
| Moq for mocking | PASS | `Mock<IApplicationGlobals>`, `Mock<IEmailMoveMonitor>`, `Mock<IQfcCollectionController>`, `Mock<IQfcItemController>`, `Mock<IFolderSearchHandler>`, `Mock<MailItem>`, and one `MockBehavior.Strict` move-monitor mock. |
| FluentAssertions for assertions | PASS | Every assertion in both files is FluentAssertions. No MSTest `Assert` call appears. |
| Hand-written fake where Moq cannot express the shape | PASS | `SynchronousIdleDispatcher` is hand-written because two of the three interface members are generic methods whose return type depends on the type parameter; the file records that reason. |
| Test coverage of new code | PASS | 23 of 24 genuinely-new executable lines covered, 95.83%. |

## 5. Test Coverage Detail

Measured on a single-assembly Cobertura run scoped to `QuickFiler.Test`, post-processed to
repository-relative filenames. The post-processing discriminator was checked explicitly rather than
inferred from artifact existence: all four required `filename` attributes appear in backslash-separated
repository-relative form, the class count is 537 against 535 at baseline and 3166 in a raw document, and
a scan of all 12,151,131 bytes found zero absolute host paths.

| File | Baseline line rate | Post-change line rate | Covered / valid |
|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 0.152941 | 1.000000 | 85 / 85 |
| `QuickFiler/Controllers/QfcQueue.cs` | 0.496795 (pre-split, 155/312) | 0.703226 | 109 / 155 |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | new file | 0.443709 | 67 / 151 |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | new file | 0.172414 | 5 / 29 |
| Combined three parts vs pre-split base | 0.496795 | 0.540299 | 181 / 335 |
| QuickFiler package | 0.810521 | 0.816886 | 10,314 / 12,626 |

Explicit floor verdicts, stated one per line:

- C# new-code line coverage, 95.83% measured against the 90% new-code floor in CLAUDE.md: PASS.
- C# repository-wide line coverage, 85.79% projected against the 80% floor in CLAUDE.md: PASS.
- C# QuickFiler package line coverage, 81.69% against the 80% floor in CLAUDE.md: PASS.
- C# canonical coverage artifact presence at the hook path: FAIL, non-blocking, for the reason recorded in the artifact-state table above.
- C# changed-line regression check, zero relocated statements covered at the anchor and uncovered after: PASS.

Branch coverage is reported for information at the file level — `QfcQueue.Enqueue.cs` 16/16,
`QfcQueue.cs` 32/48, `QfcQueue.Tlp.cs` 23/40, `QfcQueue.UiIdle.cs` 4/4. No repository-wide branch
figure exists for this item, because no whole-solution run was possible on this host.

Three qualifications on the coverage evidence, recorded so the reader can weigh it:

1. The repository-wide figure of 0.857898 is a **projection**, not a measurement: 56,029 + 99 covered
   over 65,402 + 23 valid, where the reference pair comes from the committed repository-wide Cobertura
   document of item 825. I re-derived that reference pair independently from the committed document and
   read `lines-covered="56029" lines-valid="65402"`, which matches the projection's stated inputs
   exactly. The delta clause is the discriminating one, and it is 99 covered over 23 valid.
2. Both raw Cobertura documents produced by this item were deleted after their last consumer. Every
   per-file and per-line figure above is therefore executor-attested. The attestation is unusually
   detailed — a 187-row line-by-line table with per-line hit counts and an anchor comparison — but it
   cannot be recomputed from the committed tree.
3. The 51 statements uncovered at both the anchor and the head, the 24 adapter-body lines, the three
   reflection-driven clone overloads and the deliberately uninvoked `BackgroundTlpFactory` default are
   each named, attributed to a member and explained in
   `evidence/regression-testing/residual-uncovered-regions.2026-09-12T10-25.md`. No
   `[ExcludeFromCodeCoverage]` attribute and no assembly-level or file-level exclusion was introduced
   anywhere in this change, so every residual stays in the denominator. That satisfies the Coverage
   Exclusion Policy in `.claude/rules/general-unit-test.md`, which makes any exclusion matching a
   production source path a Blocking finding.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Baseline total | 1395 | `evidence/baseline/p0-t11-test-baseline.2026-09-12T10-25.md` |
| Post-change total | 1423 | `evidence/qa-gates/p5-t7-tests-final.2026-09-12T10-25.md` |
| Passed | 1423 | same |
| Failed | 0 | same |
| Cases added | 28 | 25 `[TestMethod]` plus one `[DataTestMethod]` carrying three `[DataRow]` attributes; I counted 29 attribute occurrences in `QfcQueueEnqueueTests.cs`, which decomposes as 25 + 1 + 3 |
| Arithmetic check | 1395 + 28 = 1423 | consistent; no pre-existing case was lost or silently filtered |

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Format check | `dotnet tool run csharpier check .` | exit 0, 1632 files, none named | PASS |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 errors, 0 warnings | PASS |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 0 errors, 0 warnings | PASS |
| Test execution | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` | 1423/1423, failed=0 | PASS |
| Confidentiality masking scan | scan of the committed artifacts for absolute host paths and account names | zero absolute host path occurrences reported by the post-processor check; no raw Cobertura or trx committed | PASS |
| Suppression scan (added lines) | search for `#pragma warning disable`, `[ExcludeFromCodeCoverage]`, `SuppressMessage` across the seven Write Set code paths | one pre-existing `CS0618` pair, unchanged; zero coverage-exclusion attributes; zero new suppressions | PASS |
| Workflow change scan | search of the branch diff for `.github/workflows/**` | zero workflow files changed | PASS |

## 8. Gaps and Exceptions

1. **AC22 universal clause not satisfied — Medium, non-blocking.** Three tracked files under
   `.claude/agent-memory/orchestrator/` appear in the anchored branch diff (`MEMORY.md` modified, two
   new memory records added) and are not in the spec's Write Set. The plan anticipated them and admitted
   them through a pre-declared scope-lock clause, but a plan clause cannot amend an acceptance criterion.
   AC22 is graded PARTIAL and un-checked. Remedy: amend the spec's Write Set or AC22's wording. No code
   change is implied.
2. **Raw coverage documents deleted — Low, non-blocking.** Recorded in section 5. AC19's literal text
   asks for "a pre-change Cobertura artifact committed" and "a post-change Cobertura artifact committed";
   neither is on disk, because the ratified issue-671 hygiene convention forbids committing either. The
   substitution to committed Markdown projections is disclosed in the plan, in the P5-T5 artifact, in the
   P6-T4 artifact and in the P7-T24 reconciliation, and every figure survives. Graded as a disclosed
   substitution rather than a failure.
3. **Repository-wide coverage is projected, not measured — Low, non-blocking.** A whole-solution local
   run stalls on four shell-icon test classes in another assembly on this host. The projection's inputs
   were independently re-derived by this review.
4. **Bugfix-workflow fail-before step unavailable — Low, non-blocking.** Argued in section 2 and
   accepted.
5. **Repository documentation conflict on coverage floors — informational, pre-existing.** CLAUDE.md
   states 80 percent repository-wide and 90 percent for new code; `.claude/rules/general-unit-test.md`
   and `.claude/rules/quality-tiers.md` state 85 percent line and 75 percent branch. The item recorded
   the divergence factually and applied CLAUDE.md's figures under its stated precedence. This item clears
   the CLAUDE.md floors, and the projected repository-wide figure would also clear an 85 percent line
   floor. Resolving the conflict is a repository-governance task, not a task for this item.
6. **Tier classification unavailable — informational.** No `quality-tiers.yml` exists at the repository
   root, so no tier-dependent gate (property-test density, mutation score) can be evaluated for
   `QuickFiler`. No criterion in this item depends on one.
7. **PR context artifacts are foreign to this item — informational.** The artifacts at
   `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` in the coordinator session
   root describe branch `bug/minor-audit-trio-gate-cts-tracker-872` at head `05e44b31`, which belongs to
   a different cohort item. They were not used to derive scope for this review.
8. **Merge-base against `main` not independently recomputed — UNVERIFIED.** The Bash tool was withheld
   for this review by binding caller directive, and the item worktree is not the session working
   directory, so no git form available to this agent could read the correct repository. Scope was derived
   from the caller-supplied anchored name-status listing, cross-checked against the committed
   `evidence/qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md` listing, which agrees with it path for path,
   and against direct inspection of every file in the worktree.

## 9. Summary of Changes

Three production files added (`QfcQueue.Tlp.cs`, `QfcQueue.UiIdle.cs`, `IUiIdleDispatcher.cs`), three
modified (`QfcQueue.cs`, `QfcQueue.Enqueue.cs`, `QuickFiler.csproj`), two test files added, one test
project file modified. 1271 insertions and 264 deletions across nine code and project paths. The
enqueue part's diff is three hunks, one per seam substitution; the base part's diff is four hunks, of
which two are whole-region deletions to the new partial parts, one is the S1 seam addition and one is
the loss of a UTF-8 byte-order mark that CSharpier dropped during the mandated repository-wide format.

## 10. Compliance Verdict

**PASS.** No Blocking finding. One Medium non-blocking finding (AC22's universal clause), three Low
non-blocking evidence-strength findings, and a set of informational observations. The change is
policy-compliant on formatting, analyzers, nullable analysis, test framework and library selection,
determinism, file size, coverage floors, coverage-exclusion policy and evidence location. Remediation
inputs are not produced, because no finding requires a code or test change before merge.

## Appendix A: Test Inventory

| Test method | Criterion exercised |
|---|---|
| `MoveMonitor_SeamContract_HasNonNullDefaultAndRejectsNull` | AC1, AC7 |
| `UiIdleDispatcher_SeamContract_HasNonNullDefaultAndRejectsNull` | AC2, AC7 |
| `ItemViewerFactory_SeamContract_HasNonNullDefaultAndRejectsNull` | AC3, AC7 |
| `ViewerRowPlacer_SeamContract_HasNonNullDefaultAndRejectsNull` | AC4, AC7 |
| `ItemGroupFactory_SeamContract_HasNonNullDefaultAndRejectsNull` | AC5, AC7 |
| `BackgroundTlpFactory_SeamContract_HasNonNullDefaultAndRejectsNull` | AC6, AC7 |
| `Construction_InHeadlessHost_SucceedsAndDefaultsToProductionAdapter` | AC7, AC9 |
| `ItemViewerFactory_Default_IsTheViewerQueueDequeueMethodGroup` | AC3 |
| `EnqueueAsync_WithNullItemList_ThrowsArgumentNullException` | AC10 |
| `EnqueueAsync_WithEmptyItemList_ThrowsArgumentException` | AC10 |
| `EnqueueAsync_WithOnePage_QueuesTheTemplatePanelAndGroupsInInputOrder` | AC11 |
| `EnqueueAsync_WhenItRuns_IncrementsRunningJobsAndDecrementsOnCompletion` | AC13 |
| `EnqueueAsync_WhenLoaderIsCancelled_SwallowsAndLeavesNothingQueued` | AC12, AC13 |
| `EnqueueAsync_WhenLoaderFails_SwallowsAndLeavesNothingQueued` | AC12, AC13 |
| `EnqueueAsync_WithSubscriber_RaisesExactlyOneAddNotification` | AC14 |
| `EnqueueAsync_WithNoSubscriber_CompletesWithoutThrowing` | AC14 |
| `EnqueueAsync_WithStrictMoveMonitor_HooksEachItemExactlyOnce` | AC15 |
| `EnqueueAsync_WithItemTotal_PassesExpectedDigitsToEachController` (3 rows) | AC16 |
| `EnqueueAsync_WithMatchingCarrier_PassesTheCarriedHandler` | AC16 |
| `EnqueueAsync_WithNoCarrierList_PassesANullCarriedHandler` | AC16 |
| `EnqueueAsync_WithOneItem_PassesEveryControllerArgumentThrough` | AC16 |
| `EnqueueAsync_WithThreeItems_AwaitsInitializeOncePerRow` | AC16 |
| `LoadControllersViewersAsync_WithNonZeroStart_MapsIndexAndWidensDigits` | AC5, AC16 |
| `AddAsync_WithSubstitutedViewerSeams_BuildsTheGroupAndPlacesTheViewer` | AC3, AC4, AC17 |
| `EnqueueAsync_WithDefaultItemGroupFactory_UsesEachDispatcherShapeOnce` | AC2, AC17 |
| `EnqueueAsync_WithSubstitutedTemplate_FlowsThatPanelToTheDequeuedEntry` | AC6 |

26 test members, 28 executed cases.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
```

Loop outcome: pass 1 completed with no restart and no skipped step
(`evidence/qa-gates/p5-t8-loop-result.2026-09-12T10-25.md`).
