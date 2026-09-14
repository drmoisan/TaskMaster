# Policy Audit — Issue #816, UiThread IsCompleted branch-2 residual and AC5 apartment measurement

- Component: `UtilitiesCS` (C# / .NET Framework 4.8 VSTO add-in), `UtilitiesCS.Test`
- Date: 2026-09-14
- Reviewer: feature-review agent
- Work mode: `full-bug` (persisted marker at `issue.md` line 12) — acceptance-criteria source is `spec.md` only
- Branch: `bug/uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816`
- Branch head verified on disk: `b4941e25229497b18316023794fbc4a2dfe83159`
- PR base branch: `main`; `git merge-base origin/main HEAD` recomputed in this review: `b63eaa4630d13da46f7ece130bedade53ac39e22` (identical to `origin/main`, so the three-dot and two-dot diffs coincide)
- Plan anchor ref: `refs/issue816/base` = `92cf2723451087550cdf019af8c138a4fee9b555` (the merge commit that brought `origin/main` onto the branch)
- Files under test: `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

## Template Resolution Deviation

The MCP tools `mcp__drm-copilot__resolve_policy_audit_template_asset` and
`mcp__drm-copilot__validate_orchestration_artifacts` are not present on this agent's tool surface for
this session. This artifact was hand-authored against the canonical major-heading set published in
`.claude/skills/policy-audit-template-usage/SKILL.md` section 5, which is reproduced here in full and
in order. The artifact is not marked BLOCKED, because the review is evidence-complete without the
templated scaffold.

## Executive Summary

The change adds a two-term conjunct to one exit of
`UtilitiesCS.UiThread.SynchronizationContextAwaiter.IsCompleted`, so that the captured-UI-context exit
now also demands a non-null captured WPF dispatcher that is reference-equal to the dispatcher of the
executing thread. The production diff against the plan anchor is exactly two removed lines and fifteen
added lines in one file, and I reproduced that diff in this review: the removed set is the one-line
comment `// The persistent UI context captured at Init() time.` and the one-line condition
`if (ReferenceEquals(_context, _uiSyncContext))`, and nothing else. The accessor still has five exits
in the same source order, which I re-derived by reading the post-change accessor rather than by citing
the executor.

Three new tests and one positive invariance twin were added, together with one project-file
registration. The two negative tests are recorded Failed against the unmodified predicate (exit code 1,
both with the message `Expected observed to be False, but found True.`) and Passed against the hardened
one; the positive twin is recorded Passed in both states. The full two-assembly suite reports 6336 of
6336 passed across three repetitions, four more cases than the 6332 baseline, which is exactly the
number of tests this delivery adds.

I independently re-derived every coverage figure from the two Cobertura documents present in the
worktree rather than accepting the executor's transcription. The baseline document records the
`get_IsCompleted` accessor at line-rate 0.90 with the pre-change `return true;` at line 178 carrying
`hits="0"`; the post-change document records the same accessor at line-rate 1, branch-rate 1, and the
new compound condition at line 182 with `condition-coverage="100% (6/6)"`. Every executable line the
delivery added is covered, and all six jump conditions of the new predicate are exercised in both
directions.

Overall verdict: **PASS**. **No finding in this review is Blocking.** All fourteen acceptance criteria
in this feature's `spec.md` and the residual AC5 in the sibling 2026-09-07 feature folder are earned on
the evidence and are correctly checked off. Two non-blocking findings are recorded: a latent
null-to-null residual in the sibling dispatcher exit that this delivery deliberately did not touch
(recommended for promotion to its own issue), and the ordinary evidence-strength limitation that the
TRX and Cobertura source documents live under the gitignored `coverage/` directory, so the test-outcome
and toolchain figures are executor-attested even though the two Cobertura documents happened to survive
in this worktree and were re-derived here.

## Rejected Scope Narrowing

No caller instruction narrowed the audit scope, and no language with changed files on this branch was
excluded from evaluation. The audit covers the complete branch diff against the resolved base
`b63eaa4630d13da46f7ece130bedade53ac39e22`, which is a superset of the plan-anchored diff: it adds the
three pre-merge commits `336a6e2a2`, `577640e3a` and `12456a876`, and therefore nine tracked files under
`.claude/agent-memory/` and the feature folder's `research/` document. Those are audited below and are
not excluded.

Three caller directives were examined against the scope invariant. None is a narrowing:

1. Verbatim: "Governance you must apply (repo-specific — do not use .claude/rules/*.md's 85/75
   figures) — CLAUDE.md governs coverage: C# line floor 80%, new/changed code 90%.
   `.claude/rules/general-unit-test.md`'s 85/75 figures and any push-down `quality-tiers.yml`
   reference are not authoritative here (maintainer ruling on issues 563/828)." Evaluated and accepted
   as a precedence ruling, not a narrowing. CLAUDE.md states its own compliance order and places
   itself first. Both floor sets are evaluated and reported in sections 1 and 5 below. The delivery
   clears the CLAUDE.md 80 percent line floor and the 90 percent new-code floor. The repository-wide
   line figure, 81.52 percent, would not clear an 85 percent line floor and the repository-wide branch
   figure would not clear a 75 percent branch floor; both are recorded explicitly rather than
   suppressed, both are pre-existing conditions of the repository, and both move in the improving
   direction under this delivery. The divergence between CLAUDE.md and the two rule files is recorded
   as an unresolved repository documentation conflict, not as a finding against this item.
2. Verbatim: "Note: a commit `77cf1ab9e` ... I have independently verified its contents are confined
   to feature-folder evidence files and plan check-offs already accounted for in the final state, with
   no deletions and nothing outside the plan's scope. Treat it as part of the ordinary evidence trail;
   it is not a defect to flag." Evaluated and accepted as an attribution rule after independent
   verification. `git show --stat 77cf1ab9e` in this review lists seven paths: six additive evidence
   Markdown files under the feature folder's `evidence/qa-gates/` and `evidence/regression-testing/`
   directories, plus twelve check-off flips in `plan.2026-09-12T13-23.md`. 247 insertions and 6
   deletions, all six deletions being the `- [ ]` halves of the flipped check-offs. Nothing outside the
   feature folder. The caller's claim is true and the commit is not flagged.
3. Verbatim: "Do not open a pull request. Do not merge anything." Evaluated and accepted. This
   constrains the reviewer's write actions, not the audit's scope.

## Evidence Location Compliance

Every evidence artifact this item produced lies under `<FEATURE>/evidence/<kind>/`, across the four
kinds `baseline`, `qa-gates`, `regression-testing` and `other`. I scanned the full branch diff for any
path under a prohibited evidence root with
`git diff --name-only origin/main...HEAD -- .github artifacts scripts`, which returned no paths at all,
and by reading the complete `git diff --numstat` name list.

| Prohibited evidence root | Occurrences in the branch diff | Verdict |
|---|---|---|
| `artifacts/baselines/` | 0 | PASS |
| `artifacts/qa/` | 0 | PASS |
| `artifacts/evidence/` | 0 | PASS |
| `artifacts/coverage/` | 0 | PASS |

`validate_evidence_locations.py --root .` was not executed: no such script exists in this repository
(`.claude/hooks/enforce-evidence-locations.ps1` is the local enforcement mechanism, and this agent's
Bash surface is restricted to `git` by binding caller directive). The scan above covers the same four
prohibited roots the script checks. Verdict: PASS by inspection.

One artifact is written into a second feature folder,
`docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md`.
This is not a breach. The invariant constrains the *shape* of an evidence path to some feature folder's
own `evidence/<kind>/` subtree; it does not restrict which feature folder is written to, and the
criterion being discharged names that folder explicitly. The two copies are byte-identical, which the
executor evidenced with `git diff --no-index` plus a SHA-256 comparison and which I corroborated by
reading both files.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Both new test classes carry `[DoNotParallelize]` and wrap every mutation of the process-global `UiThread` statics in `using (UiThreadStateScope.Enter())`, which snapshots and restores them. Each test creates its own dedicated thread and its own dispatcher host and joins or disposes both before returning. |
| Isolation | PASS | Each of the three new tests targets one leg of one predicate or one measurement. The positive twin added to `UiThread_Tests.cs` targets the single out-of-scope leg. |
| Fast execution | PASS | The whole `UtilitiesCS.Test` assembly completed in 45.71 s at baseline and the two-assembly coverage run in 29.11 s post-change (`evidence/baseline/p0-t16-coverage-baseline.md`, `evidence/qa-gates/p4-t11-coverage-run.md`). |
| Determinism | PASS | I read all four changed test files. No `Thread.Sleep`, `Task.Delay`, `Stopwatch`, `DateTime.Now`, `DateTime.UtcNow` or `Environment.TickCount` appears in any new body. `ApartmentThreadRunner.RunOnThread` starts a background thread, joins it unconditionally, and returns the captured exception; `SharedStaDispatcherHost.Dispose` calls `BeginInvokeShutdown` then `Join`. The three repetitions produced identical totals, 6336/6336/0/0 each time. |
| Readability and maintainability | PASS | Every new test method and both new classes carry an XML `<summary>`, and the two class-level `<remarks>` state why the non-parallel attribute is required and why the guard value is asserted while the settling value is only recorded. Names follow `Member_Condition_ExpectedOutcome`. |

The positive twin in `UiThread_Tests.cs` was examined specifically for the trap that
`Dispatcher.Invoke` installs a throwaway `DispatcherSynchronizationContext` on the UI thread for the
duration of the operation. The test does not fall into it: it captures `SynchronizationContext.Current`
inside the invoke, replaces it with a fresh plain `SynchronizationContext`, and restores it in a
`finally`. The ambient context during the assertion is therefore genuinely non-null and genuinely not
the captured context, which is what makes the test exercise the intended exit rather than the
ambient-identity exit at the top of the accessor.

### 1.2 Coverage and Scenarios

Scenario completeness for the hardened predicate is complete rather than merely adequate. The negative
flow on the recycled-managed-thread-id leg, the negative flow on the null-captured-dispatcher leg, and
the positive flow on the genuine-owner leg are each covered by a dedicated test, and the pre-existing
`IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` remains the guard
against a bare owning-thread-identity predicate. The Cobertura evidence confirms this at the branch
level: the new compound condition records `condition-coverage="100% (6/6)"`, so every one of the three
jump conditions is exercised in both directions.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/evidence/baseline/p0-t16-coverage-baseline.md`
- C# post-change coverage artifact: `docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/evidence/qa-gates/p4-t12-ac10-coverage.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 81.51% lines (152,996/187,704). Post-change: 81.52% lines (153,116/187,821). Change: +0.01% lines (+120 covered, +117 valid). New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/p0-t16-coverage-baseline.md and evidence/qa-gates/p4-t12-ac10-coverage.md, both re-derived in this review from the two Cobertura documents in the worktree.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Canonical artifact path | Verdict | Disposition |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` is absent from the canonical hook path in both the review worktree and the coordinator session root | FAIL | Non-blocking and procedural. The two Cobertura documents the delivery produced are written under the gitignored `coverage/` directory per the ratified evidence-hygiene convention that forbids committing raw coverage documents; both survive in this worktree and every figure in this audit was re-derived from them by direct parse. No file was manufactured by this review. |
| TypeScript | `coverage/lcov.info` | N/A | Zero TypeScript files changed on this branch. |
| Python | `artifacts/python/lcov.info` | N/A | Zero Python files changed on this branch. |
| PowerShell | `artifacts/pester/powershell-coverage.xml` | N/A | Zero PowerShell files changed on this branch. |

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 5 | 4 added | 6336/6336 passed | 81.51% | 81.52% | 100% |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

### 1.3 Test Structure and External Dependencies

| Requirement | Verdict | Evidence |
|---|---|---|
| Arrange-Act-Assert | PASS | All four new or changed test bodies carry explicit `// Arrange`, `// Act`, `// Assert` separators. |
| Clear failure messages | PASS | FluentAssertions throughout. The fail-before run recorded the intended message verbatim: `Expected observed to be False, but found True.` |
| No external services | PASS | No live Outlook process, no network, no database. `evidence/qa-gates/p4-t1-outlook-precondition.md` records that `Get-Process -Name OUTLOOK` returned no process before the Phase 4 rebuilds and runs. |
| Temporary files prohibited | PASS | No temporary file is created by any new test. The measurement constructs an in-process `SyncContextForm` and disposes it in a `finally`. |
| Test file location | PASS | All four files live under `UtilitiesCS.Test/Threading/`, mirroring `UtilitiesCS/Threading/`. This is the established repository convention for a sibling `<Project>.Test` assembly and matches every pre-existing file in that directory. |
| No mutable global state left behind | PASS | `UiThreadStateScope.Enter()` is used as a `using` in every test that writes a `UiThread` static, and `UiThread.ResetForTesting()` restores all eleven statics to their declared initial values. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The change is two additional conjuncts on one existing `if`. No new type, method, field, interface, seam or indirection was introduced anywhere in production. |
| Reusability | PASS | The dispatcher-identity comparison the new conjunct performs is textually the same expression the sibling exit already uses, deliberately not extracted, because extraction would have altered the sibling exit and violated AC1's confinement requirement. |
| Extensibility | PASS | No public API changed. `IsCompleted` keeps its signature, its five exits and their source order. |
| Separation of concerns | PASS | No I/O, UI or framework glue was added to the predicate; it reads two statics and one framework lookup, as before. |
| File size limit, 500 lines | PASS | I counted the four files directly: `UiThread.cs` 306, `UiThread_Tests.cs` 493, `UiThreadInitContract_Tests.cs` 464, `UiThreadApartmentMeasurement_Tests.cs` 164. All at or below 500, and all four agree with the executor's `evidence/qa-gates/p4-t13-ac13-file-sizes.md`. The tightest headroom is 7 lines on `UiThread_Tests.cs`; see the code review for the associated recommendation. |
| Error handling, fail fast | PASS | The change makes the predicate strictly more conservative: it returns `true` in a strict subset of the states it previously did. Nothing is silently swallowed. |
| Logging | PASS | No logging statement was added, removed or changed. |
| Comment why, not what | PASS | The seven-line replacement comment explains why a reference match alone is insufficient (managed thread id reuse after thread death) and why the null test is load-bearing rather than redundant. Both are non-obvious and both are accurate. |
| Public API stability | PASS | The two-line removed set contains no declaration. No signature, accessibility or type changed. |
| Dependencies | PASS | No package reference added or changed. The single `.csproj` edit is one `<Compile Include>` item. |
| Bugfix workflow, failing test first | PASS | `evidence/regression-testing/p1-t8-fail-before-run.md` and `p1-t9-ac02-ac03-fail-before.md` record the two negative tests Failed at exit code 1 against the unmodified production source, and the commit order on the branch confirms it: `3f228e320 test(816): add failing regression tests` precedes `3936b5428 fix(threading): harden the captured-UI-context exit`. |

## 3. Language-Specific Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, pinned version via `dotnet tool run` | PASS | `evidence/qa-gates/p4-t2-csharpier-format.md` and `p4-t3-csharpier-check.md`: both exit 0, 1634 files checked, zero lines matching `Was not formatted`. The rewrote-nothing property rests on four SHA-256 content-hash pairs captured either side of the format command, equal pair for pair, not on the exit code alone. |
| `dotnet format` not used | PASS | No `dotnet format` invocation appears in any evidence artifact on this branch. |
| Analyzer gate with `/t:Rebuild` | PASS | `evidence/qa-gates/p4-t4-msbuild-analyzers.md`: exit 0, `0 Error(s)`, `0 Warning(s)`, with `/t:Rebuild /m`, `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true`. Non-vacuity rests on the compile-task count of 36, not on the skipped-`CoreCompile` count, and the artifact says so explicitly. `/t:Build` was not substituted. |
| Nullable gate with `/t:Rebuild`, no solution-wide `Nullable=enable` | PASS | `evidence/qa-gates/p4-t5-msbuild-nullable.md`: exit 0, `0 Error(s)`, 36 Csc invocations, `TreatWarningsAsErrors=true` present and `/p:Nullable=enable` absent. The gate is non-vacuous for the changed file because I confirmed `UtilitiesCS/Threading/UiThread.cs` carries `#nullable enable` at line 1, so its `CS86xx` diagnostics are promoted to errors. |
| Null-safety by default | PASS | The added `_dispatcher is not null` test is exactly the shape nullable flow analysis requires before a `SynchronizationContext?`-adjacent nullable field is passed to `ReferenceEquals`, and the zero-error nullable rebuild confirms the flow state is satisfied. |
| `internal` preferred for non-public API | PASS | No accessibility was changed. The two new test classes and their helpers are the assembly-default internal or public-in-test-assembly shapes already used by the directory. |
| Narrow, documented suppression | PASS | I searched the four changed C# files for `#pragma warning disable`, `[ExcludeFromCodeCoverage]` and `SuppressMessage`. Zero occurrences of any of them. Nothing was suppressed and nothing was excluded from measurement. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[DoNotParallelize]` and `TestContext` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference is introduced. |
| Moq for mocking | PASS by non-applicability | The three new tests need no mock: the seams are process-global statics installed through the pre-existing `UiThreadStateScope`, and the measurement constructs the real production form on purpose. No hand-rolled substitute for a mockable interface was introduced. |
| FluentAssertions for assertions | PASS | Every assertion in the new file and in the positive twin is FluentAssertions (`Should().BeNull()`, `Should().BeFalse()`, `Should().BeTrue()`, `Should().Be(ApartmentState.MTA)`). The tightened assertion in `UiThreadInitContract_Tests.cs` uses the FluentAssertions `.Throw<T>().WithMessage(...)` form. |
| Assertion strength, no weakening | PASS | The delivery strengthens an existing assertion rather than weakening one: `failing.Should().Throw<InvalidOperationException>()` gains `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`, and the same test gains an explicit apartment assertion as its first Arrange step. I verified independently that the two `InvalidOperationException` sources reachable from `UiThread.Init()` carry different message text, so the constraint discriminates. |
| Coverage of new code | PASS | Every executable line the delivery adds to the production file carries `hits="1"` in the post-change Cobertura document, and the new compound condition carries `condition-coverage="100% (6/6)"`. |

## 5. Test Coverage Detail

Both Cobertura documents produced by the delivery are present in the review worktree under the
gitignored `coverage/` directory, so this section is a re-derivation rather than a transcription of the
executor's projection. All figures below were read directly out of
`coverage/p0-t16-baseline.cobertura.xml` and `coverage/p4-t11-postchange.cobertura.xml`.

| Scope | Baseline | Post-change | Source |
|---|---|---|---|
| `UtilitiesCS.UiThread` class | line-rate 0.969072 (94/97) | line-rate 0.969072 (94/97) | `class` element `line-rate` attribute |
| `UtilitiesCS.UiThread.SynchronizationContextAwaiter` class | line-rate 0.931034 (27/29), branch-rate 0.928571, complexity 14 | line-rate 1 (36/36), branch-rate 1, complexity 18 | `class` element attributes |
| `get_IsCompleted` accessor | line-rate 0.90, branch-rate 0.916667, complexity 12 | line-rate 1, branch-rate 1, complexity 16 | `method` element attributes |
| `UtilitiesCS/Threading/UiThread.cs` file, union of three class elements | 121/126 = 96.03% | 130/133 = 97.74% | union by `line number` |
| Repository-wide, all instrumented modules | 152,996/187,704 = 81.51% lines; 18,440/31,204 = 59.09% branches | 153,116/187,821 = 81.52% lines; 18,449/31,210 = 59.11% branches | root `coverage` element attributes |

The transition that carries the claim is falsifiable and I verified both ends of it. In the baseline
document, inside the `get_IsCompleted` method element, line 176 (the pre-change
`if (ReferenceEquals(_context, _uiSyncContext))`) reads `hits="1"` with
`condition-coverage="50% (1/2)"` — only the false arm was ever taken — and lines 177 and 178 read
`hits="0"`. In the post-change document the same method element records line 182, the new compound
condition, at `hits="1"` with `condition-coverage="100% (6/6)"` across three jump conditions, and lines
183 through 191, the continuation lines and the exit body, each at `hits="1"`. The previously dead
`return true;` is now live.

Explicit floor verdicts, stated one per line:

- C# new and changed production code line coverage, 100% measured against the 90% new-code floor in CLAUDE.md: PASS. All ten executable lines the delivery adds to `UiThread.cs` carry non-zero hits.
- C# per-file line coverage for the changed production file, 97.74% measured against the 80% line floor in CLAUDE.md: PASS, and up from 96.03% at baseline.
- C# repository-wide line coverage, 81.52% measured against the 80% line floor in CLAUDE.md: PASS, and up from 81.51% at baseline.
- C# changed-line regression check: PASS. The post-change uncovered set for the changed file is exactly {38, 39, 40}; the baseline uncovered set was exactly {38, 39, 40, 177, 178}. Every line uncovered after was already uncovered before, so no line lost coverage; lines 38 to 40 are the body of the `if (onLockupDetected is not null)` guard in `UiThread.Init`, which this delivery does not touch.
- C# repository-wide line coverage measured against the non-authoritative 85% figure in `.claude/rules/general-unit-test.md`: FAIL. Non-blocking and pre-existing. The caller's governance directive places CLAUDE.md's 80% floor first and both floors are reported here rather than one being suppressed. The figure improved under this delivery.
- C# repository-wide branch coverage measured at 59.11% against the non-authoritative 75% figure in `.claude/rules/quality-tiers.md`: FAIL. Non-blocking and pre-existing; CLAUDE.md sets no branch floor. The figure improved from 59.09% and the branch-rate of the one accessor this delivery changes rose from 0.9167 to 1.
- C# canonical coverage artifact presence at the hook path `artifacts/csharp/coverage.xml`: FAIL. Non-blocking and procedural, for the reason recorded in the artifact-state table in section 1.
- C# coverage-exclusion policy compliance: PASS. `coverage.config` is unchanged by this branch and its only `Exclude` entries are seven third-party `ModulePath` patterns (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). No production source path is excluded, and no `[ExcludeFromCodeCoverage]` attribute was added anywhere.

Two qualifications on the coverage evidence, recorded so the reader can weigh it:

1. The repository-wide denominator includes the instrumented test assemblies, which is why adding one
   164-line test file moves `lines-valid` by +117. The repository-wide delta of +0.01 percentage points
   is therefore close to noise and should not be read as an uplift claim; the load-bearing figures are
   the per-file and per-accessor ones.
2. Both Cobertura documents are untracked and gitignored. They exist in this worktree today and were
   parsed here, but a third party working from the committed tree alone would have only the Markdown
   projections. That is the ratified convention for this repository and is not treated as a defect.

## 6. Test Execution Metrics

Recorded as a bullet list rather than a table, deliberately.

- Baseline total, pre-change, two assemblies: 6332 passed, 0 failed (`evidence/baseline/p0-t16-coverage-baseline.md`, inner run `Test Run Successful.`).
- Post-change coverage run: 6336 total, 6336 passed, 0 failed, 0 skipped (`evidence/qa-gates/p4-t11-coverage-run.md`).
- Repetition 1, task P4-T6: exit 0, 6336/6336 passed, 0 failed, 0 skipped.
- Repetition 2, task P4-T7: exit 0, 6336/6336 passed, 0 failed, 0 skipped.
- Repetition 3, task P4-T8: exit 0, 6336/6336 passed, 0 failed, 0 skipped.
- Arithmetic check: 6332 + 4 = 6336, consistent. The delivery adds exactly four test methods, so no pre-existing case was lost or silently filtered and discovery of the new file succeeded.
- Fail-before run, task P1-T8: exit 1, with the two new negative tests recorded Failed and the positive twin recorded Passed against the unmodified production source.
- Named flaky test `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`: Passed in each of the three repetitions.
- Structural guard `UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`: Passed in each of the three repetitions.
- Test-case filter applied to every run: `TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`, with the stated reason being two probe runs that each produced one non-deterministic Win32 icon-handle failure. The artifact explicitly declines to give the older stall as the reason, which is the accurate attribution.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Format apply | `dotnet tool run csharpier format .` | exit 0, four owned files byte-identical before and after by SHA-256 | PASS |
| Format check | `dotnet tool run csharpier check .` | exit 0, 1634 files, zero `Was not formatted` lines | PASS |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0, 0 errors, 0 warnings, 36 Csc invocations | PASS |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 0 errors, 0 warnings, 36 Csc invocations | PASS |
| Test execution | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` | 6336/6336, failed=0, three times | PASS |
| Toolchain ordering and restart discipline | four commands in CLAUDE.md order, single pass | the formatter modified no file, so no restart from step one was required or performed | PASS |
| Confidentiality masking scan | search of the whole feature folder for `C:\Users`, `C:/Users`, the account token and `TaskMaster-wt` | zero matches across every committed artifact in both feature folders | PASS |
| Suppression scan on changed files | search for `#pragma warning disable`, `[ExcludeFromCodeCoverage]`, `SuppressMessage` across the four changed C# files | zero occurrences | PASS |
| Workflow change scan | `git diff --name-only origin/main...HEAD -- .github artifacts scripts` | zero files | PASS |
| Working tree cleanliness | `git status --porcelain` in the review worktree | empty | PASS |

## 8. Gaps and Exceptions

1. **Latent null-to-null residual in the sibling dispatcher exit — Medium, non-blocking, pre-existing.**
   The final exit of `IsCompleted` still reads
   `return _context is DispatcherSynchronizationContext && ReferenceEquals(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread), _dispatcher);`.
   When `_dispatcher` is null and the executing thread owns no dispatcher, that `ReferenceEquals` is a
   null-to-null match and evaluates true, so the exit returns true on exactly the thread shape the new
   conjunct exists to reject — reachable when `Initialize()` throws between the `UiThreadId` assignment
   and the `Dispatcher` assignment, leaving `_uiThreadId` set and `_dispatcher` null, and the awaited
   context is a `DispatcherSynchronizationContext`. This is not a defect introduced by this delivery:
   the exit is byte-identical to its pre-change form, and AC1 makes changing it a FAIL condition. It is
   also not recorded anywhere in the spec's Non-Goals, which address the dispatcher leg only in its
   fully-initialized form. Recommended action: promote to a follow-up issue. See the code review for
   the full derivation.
2. **Repository documentation conflict on coverage floors — informational, pre-existing.** CLAUDE.md
   states 80 percent repository-wide and 90 percent for new code; `.claude/rules/general-unit-test.md`
   and `.claude/rules/quality-tiers.md` state 85 percent line and 75 percent branch. The caller's
   binding governance directive resolves the conflict in favour of CLAUDE.md for this review, citing a
   maintainer ruling on issues 563 and 828. Both figure sets are reported in section 5. Resolving the
   documentation conflict is a repository-governance task, not a task for this item.
3. **Test outcomes and toolchain exit codes are executor-attested — Low, non-blocking.** The TRX
   documents, the MSBuild file logs and the CSharpier console logs all live under the gitignored
   `coverage/` directory and were not committed, per the ratified evidence-hygiene convention. This
   agent's Bash surface is restricted to `git` by binding caller directive and no build or test
   execution was possible, so the four toolchain results and the per-test outcomes in this audit are
   cited from the committed Markdown projections rather than re-run. The independently re-derived
   facts are: the branch diff and its confinement, the five-exit count, the four file line counts, the
   absence of suppressions and exclusions, the absence of host-path leaks, the two Cobertura documents
   in full, and the commit ordering that establishes the red-then-green sequence.
4. **Tier classification unavailable — informational.** No `quality-tiers.yml` exists at the repository
   root, so no tier-dependent gate (property-test density, mutation score) can be evaluated for
   `UtilitiesCS`. No criterion in this item depends on one.
5. **Branch carries nine `.claude/agent-memory/` files and one `research/` document that the spec's
   Write Set does not list — informational.** The Write Set is a spec section, not an acceptance
   criterion, and no criterion in this item constrains the change footprint outside
   `UtilitiesCS/Threading/UiThread.cs`. The nine memory files were audited: none contains an absolute
   host path, an account token or a machine name, and all are agent-memory records authored for this
   repository. Recorded so the footprint is visible, not as a finding.
6. **PR context artifacts are foreign to this item — informational.** The artifacts at
   `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` in the coordinator session
   root describe branch `bug/quickfiler-itemviewer-ui-marshalling-seam-743` at head `841fba743`, which
   belongs to a different item. The review worktree has no `artifacts/pr_context.*` pair at all. Scope
   was therefore derived from `git merge-base` and `git diff` against the resolved base branch, which
   the scope invariant names as an authoritative source, and not from either artifact.

## 9. Summary of Changes

One production file modified (`UtilitiesCS/Threading/UiThread.cs`, +15/-2), one test file added
(`UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`, 164 lines, two test classes, three
test methods), two test files modified (`UiThread_Tests.cs` +35/-0, `UiThreadInitContract_Tests.cs`
+5/-1), and one test project file modified (one `<Compile Include>` item). Alongside the code, the
branch carries the feature documents, a 757-line research document, 34 evidence artifacts across four
evidence kinds, the 74-task plan with every task checked off, the sibling feature folder's AC5
check-off plus a byte-identical copy of the measurement artifact, and nine agent-memory records.

The production change is a strict narrowing of one predicate: the captured-UI-context exit returns
`true` in a proper subset of the states it previously did, and returns the same value as before on the
one leg declared out of scope.

## 10. Compliance Verdict

**PASS.** No Blocking finding. One Medium non-blocking finding (the pre-existing null-to-null residual
in the sibling dispatcher exit, recommended for promotion to its own issue), one Low non-blocking
evidence-strength finding (executor-attested toolchain and test outcomes), and a set of informational
observations. The change is policy-compliant on formatting, analyzers, nullable analysis, test
framework and library selection, determinism, file size, coverage floors under the governing CLAUDE.md
figures, coverage-exclusion policy, confidentiality masking and evidence location. Remediation inputs
are not produced, because no finding requires a code or test change before merge.

## Appendix A: Test Inventory

| Test method | Criterion exercised |
|---|---|
| `UiThreadPredicateHardening_Tests.IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse` | AC2, AC12 |
| `UiThreadPredicateHardening_Tests.IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse` | AC3, AC12 |
| `SynchronizationContextAwaiter_Tests.IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` | AC4 |
| `UiThreadApartmentMeasurement_Tests.SyncContextFormShow_OnAThreadMeasuredAsMta_RecordsTheOutcome` | AC6, AC7, AC12, and clause (i) of issue #809's AC5 |
| `UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields` (modified) | AC5, AC8 finding 4B |
| `UiThreadInitRetryContract_Tests.Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` (unchanged, re-recorded) | AC5 |
| `NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` (unchanged, structural guard) | AC7 |
| `SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse` (unchanged, regression guard) | in-repository guard against a bare owning-thread-identity predicate |

Four test methods added by this delivery; 6336 executed cases in the post-change two-assembly run.

## Appendix B: Toolchain Commands Reference

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
```

Loop outcome: one pass, no restart, no skipped step
(`evidence/qa-gates/p4-t10-ac14-toolchain.md`).
