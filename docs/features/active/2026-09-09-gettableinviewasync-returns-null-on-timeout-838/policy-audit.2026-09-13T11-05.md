# Policy Audit — gettableinviewasync-returns-null-on-timeout (Issue #838)

- Artifact timestamp: 2026-09-13T11-05
- Feature folder: `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/`
- Branch under review: `bug/gettableinviewasync-returns-null-on-timeout-838`
- Head commit: `d373ddb9d75213507762640c3b8f574a02d3c731`
- Resolved base: `origin/main`, merge base `2405a829d6afd3b12eb7c228d57158a97cb4e2ca`
- Audit scope: the full branch delta from the merge base to head. Six code files plus Markdown feature and evidence documents.
- Work mode: `full-bug`. Acceptance-criteria source is `spec.md` only; `user-story.md` carries no criteria and states so itself.

## Executive Summary

Verdict: **PASS**. Blocking findings: **0**.

The branch converts every null-producing exit of `GetTableInViewAsync` into an exception, deletes the null-forgiving suppression on its return statement, adds a five-test failure-contract class, and registers both new C# files in their non-SDK-style project files. The four CLAUDE.md toolchain steps each exited 0 in one clean final pass with 7197 tests executed, 7197 passed and 0 failed. Changed-and-added-line coverage for the two production files is 100 percent, 16 of 16 executable added lines. The modified production file rose from 255 covered of 281 to 265 covered of 283, so there is no regression on changed lines.

Two coverage rows are recorded FAIL and both are non-blocking. The repository-wide raw line rate is 70.67 percent, below the 85 percent uniform floor in `.claude/rules/general-unit-test.md`; it is a pre-existing repository condition that this change improved rather than caused, and it is not the testable-denominator figure the CLAUDE.md 80 percent floor is written against. The canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent because the spec's ratified evidence convention for this item directs that raw coverage XML be discarded and only Markdown projections be committed; that absence is a procedural FAIL against the reviewer's artifact-presence rule, not a code defect, and the numeric figures it would carry are present in the committed projections.

The coverage basis this audit's PASS verdict rests on is stated explicitly in section 1.2 rather than left implicit: the blocking figure is the changed-and-added-line figure under the CLAUDE.md 90 percent new-code figure, which the policy-compliance order places first, and it is 100 percent.

## Rejected Scope Narrowing

No scope narrowing was accepted. The audit covers the complete branch delta against the merge base, derived from the anchored diff supplied by the coordinator and independently corroborated by reading each changed file in the worktree. No plan, task or phase boundary was used to limit the audited surface, and every language with changed files on the branch carries an explicit PASS or FAIL coverage verdict in section 1.2.

Two caller-supplied operating constraints were applied. Neither narrows audit scope, and both are recorded here verbatim for transparency:

1. `DO NOT USE THE BASH TOOL AT ALL`, with the stated reason that the tool allow-list does not match the repository-mandated `git -C <path>` form and that the inherited working directory is a different checkout. Justification for compliance: the constraint restricts the means of evidence collection, not the scope of the evidence. Full-branch scope was preserved by reading every changed file directly and by corroborating each anchored-diff hunk against the file on disk.
2. `Do NOT create, write, or cause the creation of artifacts/csharp/coverage.xml.` Justification for compliance: the file is absent, the hook gates that read it are therefore dormant, and creating it would fabricate a blocking failure from a pre-existing repository-wide condition against a change whose own changed-line coverage is 100 percent. The consequence, an absent canonical artifact, is recorded as a FAIL row in section 1.2 rather than suppressed.

## Evidence Location Compliance

All 44 evidence artifacts added by this branch reside under the canonical `<FEATURE>/evidence/<kind>/` locations: `evidence/baseline/` (20), `evidence/regression-testing/` (7) and `evidence/qa-gates/` (17). Zero files were written to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/` or `artifacts/evidence/`.

| Scan | Result | Verdict |
|---|---|---|
| Files under `artifacts/baselines/` | none present in the tree | PASS |
| Files under `artifacts/qa/` | none present in the tree | PASS |
| Files under `artifacts/coverage/` | none present in the tree | PASS |
| Files under `artifacts/evidence/` | none present in the tree | PASS |
| Feature evidence outside the three permitted subdirectories | `EVIDENCE_OUTSIDE_THREE=0` per the P4-T20 gate | PASS |

The whole of `artifacts/` in this worktree holds three pull-request body files with their receipts, all predating this branch, and `artifacts/orchestration/orchestrator-state.json`. None is a prohibited evidence location.

Deviation recorded: `validate_evidence_locations.py --root .` was not executed, because executing it requires the shell tool the caller constraint forbids. The scan above was performed with directory enumeration and path inspection instead, which reaches the same four prohibited prefixes the script checks.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles, structure and dependencies

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence — tests run in any order | PASS | Each of the five new tests constructs its own mock explorer, its own cancellation source and its own counters in its own body. No static mutable state and no class-level fixture exist in `GetTableInViewAsyncFailureContractTests.cs`. |
| Isolation — one unit of behaviour per test | PASS | Each test drives exactly one exit of `GetTableInViewAsync`: absorbed default, outer-token cancellation, task-cancelled retry ceiling, timeout retry ceiling, absorbed default with a cancelled token. |
| Fast execution | PASS | The pass-after run records the regression test at 00:00:00.1993088. No test waits on a deadline; every injected source is already cancelled or never cancelled. |
| Determinism | PASS | No wall-clock wait, no timer advance, no thread-pool gate. `new-test-banned-symbols` records `SLEEP_COUNT=0`, `DELAY_COUNT=0`, `CANCELAFTER_COUNT=0`, `TIMED_CTOR_COUNT=0` with a non-zero positive control for each of the four searches. |
| Readability, documented intent | PASS | Every test carries a doc comment stating the scenario and the reason the construction is the one used, and every test body is labelled `// Arrange`, `// Act`, `// Assert`. |
| No external dependencies | PASS | The only collaborators are Moq mocks of `Outlook.Explorer`, `Outlook.TableView` and `Outlook.Table`. No network, database, process or filesystem access appears in the file. |
| No temporary files | PASS | The new test file contains no filesystem API call of any kind. Raw tool output was directed to an out-of-repository scratch root by the plan, not created by test code. |
| Scenario completeness | PASS | Positive flow is covered by the pre-existing success tests that continue to pass; the new class covers four negative exits plus the ordering property between cancellation and timeout reporting. |
| Test file location mirrors production tree | PASS | `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs` mirrors `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`. No test file was colocated in the production tree. |
| No production file excluded from coverage measurement | PASS | The branch adds no `exclude` entry and no `[ExcludeFromCodeCoverage]` attribute. Both new and modified production files appear in the coverage document and contribute to the measured figures. |

### 1.2 Coverage obligations

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 | 5 added, 7197 executed | PASS | 90.75% lines | 93.64% lines | 100% lines |
| TypeScript | 0 | 0 | not exercised | N/A | N/A | N/A |
| PowerShell | 0 | 0 | not exercised | N/A | N/A | N/A |
| Python | 0 | 0 | not exercised | N/A | N/A | N/A |

The baseline and post-change figures in the C# row are the per-file figures for the modified production file `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, 255 of 281 lines rising to 265 of 283, produced by the identical nine-class-node aggregation on both sides. The new-code figure is the changed-and-added-line figure for both production files together, 16 of 16 executable added lines.

**Which floor this verdict rests on.** Two thresholds govern this repository and they diverge. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform 85 percent line and 75 percent branch figure. `CLAUDE.md` states 80 percent repository-wide against the testable denominator after the ratified COM, VSTO, WinForms and Outlook-Interop exemptions, with 90 percent for new modules. The policy-compliance order places `CLAUDE.md` first, the spec records the divergence as a known unresolved constraint, and acceptance criterion 15 selects the CLAUDE.md figure. This audit's blocking coverage verdict therefore rests on the CLAUDE.md 90 percent new-code figure applied to the changed and added lines, which is met at 100 percent, and on the no-regression requirement, which is met. The 70.67 percent repository-wide figure is a raw whole-repository line rate with no exemption applied, so it is not the testable-denominator figure the CLAUDE.md 80 percent floor is written against and it cannot be compared to that floor directly. Measured against the 85 percent uniform floor in the rules files, which admits no exemption, it is below floor and is recorded FAIL below.

### Coverage Evidence Checklist

- C# baseline coverage artifact: Markdown projection at `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/tests-and-coverage.2026-09-12T16-09.md` with the per-file baseline figures restated in `evidence/qa-gates/coverage-no-regression.2026-09-12T16-09.md`, recording 255 of 281 lines, 90.75% — PASS.
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` is absent; the committed substitute is the Markdown projection `evidence/qa-gates/coverage-changed-lines.2026-09-12T16-09.md` recording 16 of 16 executable added lines, 100% — FAIL on canonical artifact presence, non-blocking, reasons in section 1.2.2.
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed on this branch`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed on this branch`
- PowerShell baseline coverage artifact: `N/A - zero .ps1 and .psm1 files changed on this branch`
- PowerShell post-change coverage artifact: `N/A - zero .ps1 and .psm1 files changed on this branch`
- Python baseline coverage artifact: `N/A - zero .py files changed on this branch`
- Python post-change coverage artifact: `N/A - zero .py files changed on this branch`
- Per-language comparison summary: the per-language coverage comparison block below.

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 90.75% lines (255 of 281 covered in the modified production file). Post-change: 93.64% lines (265 of 283 covered in the same file under the identical aggregation). Change: plus 10 covered lines against plus 2 total lines, an improvement on both an absolute and a proportional reading, and the repository-wide line rate moved up from 0.70630 to 0.70668. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/qa-gates/coverage-changed-lines.2026-09-12T16-09.md` and `evidence/qa-gates/coverage-no-regression.2026-09-12T16-09.md`.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Changed files | Verdict | Disposition |
|---|---|---|---|
| C#, changed and added lines | 6 | PASS | 16 of 16 executable added lines covered, which clears both the 90 percent new-code figure and the 85 percent line figure. This is the blocking figure. |
| C#, modified file no-regression | 1 | PASS | 255 of 281 rising to 265 of 283 under the identical aggregation. No changed line lost coverage. |
| C#, repository-wide raw line rate | 6 | FAIL | 70.67 percent measured against the 85 percent uniform floor in the rules files. Non-blocking: the condition is pre-existing, the change moved the figure up rather than down, the figure has no exemption applied so it is not the CLAUDE.md testable-denominator figure, and the spec records the floor divergence as an unresolved known constraint outside this item. |
| C#, canonical artifact presence | 6 | FAIL | `artifacts/csharp/coverage.xml` is absent. Non-blocking and procedural: the spec's ratified evidence convention for this item directs that raw coverage XML be discarded and only Markdown projections be committed, acceptance criterion 16 gates on that convention, and every numeric figure the artifact would carry is present in the committed projections. |
| C#, branch rate | 6 | FAIL | No branch percentage is recorded in the committed Markdown projections, so the 75 percent branch figure could not be evaluated for this branch. Non-blocking: the projections were authored under the evidence convention above, and the repository-wide branch rate is a pre-existing condition that a six-file change cannot move materially. |
| PowerShell | 0 | PASS | Zero `.ps1` and `.psm1` files in the branch delta, so no PowerShell coverage obligation arises and no Pester figure is owed. |
| TypeScript | 0 | PASS | Zero `.ts` and `.tsx` files in the branch delta, so no TypeScript coverage obligation arises. |
| Python | 0 | PASS | Zero `.py` files in the branch delta, so no Python coverage obligation arises. |

Reviewer note on the two FAIL rows: both are recorded as FAIL because the reviewer's artifact-presence and floor rules admit no softer verdict, and both are dispositioned non-blocking with the reasons stated in the table. Neither is a defect in the delivered change, neither is a regression, and neither produces a remediation trigger. No remediation-inputs artifact is therefore produced for this cycle.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow — failing regression test first | PASS | `evidence/regression-testing/fail-before.2026-09-12T16-09.md` records exit code 1 with the test failing on `no exception was thrown`, and confirms the message names no compiler error code, so the red state is a runtime assertion failure and not a compile failure. `evidence/regression-testing/pass-after.2026-09-12T16-09.md` records the same test passing after the production fix, with the test file unchanged between the two runs. |
| Minimal targeted fix | PASS | The change is confined to one method, one new 33-line partial file holding one helper, two project-file Compile items, one new test class and one comment line in an existing test file. No opportunistic refactor appears in the delta. |
| Simplicity first | PASS | The remedy is a bare rethrow, two wrapper throws and a three-line guard. No new abstraction, no sentinel type, no configuration surface. |
| Separation of concerns | PASS | The failure-construction helper is a pure function returning an exception; no I/O or logging was added to it. |
| Fail fast and explicitly | PASS | Every route that previously produced null now raises. `evidence/qa-gates/no-null-forgiving-return.2026-09-12T16-09.md` records `RETURN_BANG_COUNT=0` and `RETURN_ANY_BANG_COUNT=0` against a pre-change count of 1, so the suppression cannot have been preserved by renaming the local. |
| Error handling adds context rather than swallowing | PASS | Both catch-clause throw sites carry the caught exception as `InnerException`, asserted by reference identity in `GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner`. Existing warning logs are retained. |
| No broad catch-all added | PASS | `evidence/qa-gates/no-operationcanceled-catch.2026-09-12T16-09.md` records zero `catch (OperationCanceledException` clauses in both production files, with a positive control of 3 for `catch (TaskCanceledException` in the same file, unchanged from the pre-change count. |
| File size limit, 500 lines | PASS | 473, 33, 301 and 289 lines for the four C# files in the Write Set, measured after the final formatter pass. `OVER_LIMIT_COUNT=0`. |
| Module cohesion | PASS with one observation | Splitting the helper into its own partial file is justified in the spec by the 48-line budget the modified file had against the ceiling, and the delivered file ended 27 lines below it. The observation is recorded as an informational item in the code review, not a policy breach. |
| Naming | PASS | `AcquisitionTimeout`, `GetTableInViewAsyncFailureContractTests` and the five test method names are descriptive. Test names state condition and expected outcome. |
| Public API compatibility | PASS | The parameter list and return type of `GetTableInViewAsync` are byte-identical. All nine binding sites, three direct invocations and six reflective bindings, compile and pass without edit. |
| Documented behaviour change called out | PASS | The failure contract change is documented in the method's XML comments, in the spec, and in the issue's delivery note. |
| Comment why, not what; stale comments corrected | PASS | `evidence/qa-gates/stale-comments-corrected.2026-09-12T16-09.md` records `LATENT_COUNT=0` and `MAKING_NULL_COUNT=0` against pre-change counts of 1 each. |
| Mandatory toolchain loop, in order, one clean pass | PASS | See section 6. `evidence/qa-gates/toolchain-clean-pass.2026-09-12T16-09.md` records exit 0 for all four steps and `FORMAT_CHANGED_TREE=False`, so the loop did not restart. |
| No new dependency | PASS | No package reference was added to either project file; the only project-file change is one Compile item each. |

## 3. Language-Specific Code Change Policy Compliance

`.claude/rules/csharp.md` and the CLAUDE.md C# sections apply. No other language rule applies, because the branch delta contains no `.ts`, `.py`, `.ps1` or `.psm1` file.

| Requirement | Verdict | Evidence |
|---|---|---|
| Formatting by the manifest-pinned CSharpier through `dotnet tool run` | PASS | `evidence/qa-gates/csharpier-check.2026-09-12T16-09.md` records `CHECK_EXIT=0 ERROR_LINES=0` over 1626 files, two more than the pre-change 1624, accounting for the two new C# files. The write-mode pass rewrote nothing. |
| `dotnet format` not used | PASS | No evidence artifact records a `dotnet format` invocation; the format step is the CSharpier check and the CSharpier write pass. |
| Analyzer build with `/t:Rebuild` and the two analyzer properties | PASS | `evidence/qa-gates/msbuild-analyzers.2026-09-12T16-09.md` records `MSBUILD_EXIT=0 ERROR_CS_LINES=0 WARNING_CS_LINES=0 CORECOMPILE_SKIPPED=0`, so compilation actually occurred and the analyzers actually ran. |
| Nullable and type-check build with warnings as errors | PASS | `evidence/qa-gates/msbuild-nullable.2026-09-12T16-09.md` records `MSBUILD_EXIT=0 ERROR_CS_LINES=0 ERROR_CS86_LINES=0 CORECOMPILE_SKIPPED=0`. |
| No solution-wide `/p:Nullable=enable` introduced | PASS | The nullable gate artifact states explicitly that no solution-wide nullable property was passed, and gives the reason. |
| Per-file nullable opt-in retained | PASS | Both production files open with `#nullable enable`, verified by direct read. |
| Null-safety by default; optional values modelled explicitly | PASS | `Outlook.Table? table` is narrowed by the guard before the non-null return; the helper's `inner` parameter is `System.Exception?` with a documented null contract. |
| Non-SDK-style project registration | PASS | `UtilitiesCS/UtilitiesCS.csproj` line 1069 and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 550 carry the two new Compile items, confirmed by direct read as well as by the P4-T14 gate. |
| XML documentation on non-obvious contracts | PASS | `GetTableInViewAsync` documents all three exception types it can raise; the helper documents every parameter, its return value and the reason it returns rather than throws. |
| net48 constraints respected | PASS | No `init` accessor, `record` or `record struct` was introduced. The helper returns the exception precisely because the does-not-return attribute is unavailable on this target, and the reason is recorded in the code. |
| Banned symbols | PASS | `evidence/qa-gates/new-test-banned-symbols.2026-09-12T16-09.md` records zero occurrences of `CancelAfter`, zero timed `CancellationTokenSource` constructor invocations, zero `Thread.Sleep` and zero `Task.Delay`, with `PLAIN_CTOR_COUNT=7`. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest as the framework | PASS | `[TestClass]` and `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit reference appears. |
| Moq for mocking | PASS | `Mock<Outlook.Explorer>`, `Mock<Outlook.TableView>` and `Mock<Outlook.Table>` with `Setup` and `SetupGet`. |
| FluentAssertions for assertions | PASS | Every assertion is a FluentAssertions call: `ThrowAsync<TimeoutException>`, `ThrowAsync<OperationCanceledException>`, `Should().Be`, `Should().Contain`, `Should().BeSameAs`, `Should().NotBeNull`, `Should().BeAssignableTo<Task>`. No MSTest `Assert` call appears in the new file. |
| Assertion messages actionable | PASS | Each throw assertion carries a because-clause naming the contract under test, which is what made the fail-before message self-explanatory. |
| Arrange-Act-Assert | PASS | All five tests carry the three section comments in order. |
| Reflection justified rather than preferred | PASS | The doc comment states the CS1769 embedded-interop constraint that makes a direct await from the test assembly impossible, and the binding is asserted non-null so a signature drift fails loudly rather than silently. |
| Determinism infrastructure | PASS | Cancellation state, not elapsed time, decides every outcome. No fake-timer facility is needed because no test advances a clock; the one existing test that does advance a clock is unchanged apart from a comment. |

## 5. Test Coverage Detail

| Item | Figure | Verdict |
|---|---|---|
| Executable added lines, both production files | 16 | PASS |
| Executable added lines covered | 16, 100 percent | PASS |
| Modified file, baseline | 255 covered of 281 | PASS |
| Modified file, post-change | 265 covered of 283 | PASS |
| Changed-line regression | none; plus 10 covered against plus 2 total | PASS |
| New production file, executable lines | 6 of 6 covered | PASS |
| Repository-wide raw line rate, baseline | 0.70630 | FAIL against the 85 percent uniform floor, non-blocking |
| Repository-wide raw line rate, post-change | 0.70668 | FAIL against the 85 percent uniform floor, non-blocking, direction of travel positive |

The ten executable added lines in the modified file are the two bound catch clauses, the two retry-ceiling wrapper throws, the guard's null test, its cancellation check, its timeout throw and the unsuppressed return. The six in the new file are the helper body. Independent corroboration that the guard is not dead code comes from `GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout`, which reaches the guard with a null local and a cancelled token and so executes the cancellation check, and from the regression test, which reaches the same guard with an uncancelled token and so executes the timeout throw.

## 6. Test Execution Metrics

| Counter | Phase 0 baseline | Final run | Delta |
|---|---|---|---|
| total | 7192 | 7197 | plus 5 |
| executed | 7192 | 7197 | plus 5 |
| passed | 7192 | 7197 | plus 5 |
| failed | 0 | 0 | 0 |
| not run | 0 | 0 | 0 |

Both runs discovered 9 assemblies under the identical assembly-discovery rule and the identical test-case filter, which is what makes the two counter sets comparable. `NEWLY-FAILING: NONE` and `NO-LONGER-FAILING: NONE`. The delta of exactly five is the five new failure-contract tests, so no existing test was removed and none changed outcome. The known environmental flake tracked by issue #780 did not occur in either run, so no re-run and no carve-out was used.

Toolchain pass, in CLAUDE.md order, single clean pass:

| Order | Step | Command | Exit code | Evidence |
|---|---|---|---|---|
| 1 | Format | `dotnet tool run csharpier check .` | 0 | `evidence/qa-gates/csharpier-check.2026-09-12T16-09.md` |
| 2 | Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `evidence/qa-gates/msbuild-analyzers.2026-09-12T16-09.md` |
| 3 | Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `evidence/qa-gates/msbuild-nullable.2026-09-12T16-09.md` |
| 4 | Test | `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` | 0 | `evidence/qa-gates/tests-and-coverage.2026-09-12T16-09.md` |

## 7. Code Quality Checks

| Check | Command or method | Result | Verdict |
|---|---|---|---|
| Format check | `dotnet tool run csharpier check .` | 1626 files checked, zero error lines | PASS |
| Analyzer diagnostics | analyzer msbuild with rebuild | zero error lines, zero warning lines | PASS |
| Nullable diagnostics | nullable msbuild with warnings as errors | zero `CS86` error lines | PASS |
| Suppression scan (added lines) | direct read of both production files | zero null-forgiving operators, zero pragma suppressions, zero suppression attributes added | PASS |
| Banned symbol scan | fixed search-gate form over the new test file | four absence counts zero with a non-zero positive control for each | PASS |
| File size scan | line count of the four C# Write Set files | 473, 33, 301, 289 against a 500 limit | PASS |
| Protected test file scan | anchored name-listing diff plus porcelain status | the three protected test files absent from both listings | PASS |
| Consumer file scan | anchored name-listing diff plus porcelain status | both consumer files absent from both listings | PASS |
| Assertion weakening scan | unified diff of the one touched existing test file | 2 content lines, both comment prose, `SHOULD_DIFF_LINES=0` | PASS |
| Raw artifact scan | added-path union of anchored diff and porcelain status | `TRX_COUNT=0`, `COBERTURA_COUNT=0` over a 51-member union | PASS |
| Host-path hygiene scan | direct read of every committed evidence artifact and both audit-relevant feature documents | scratch locations are written as `$env:TEMP` and `${env:ProgramFiles(x86)}` expressions; no account name, host name or absolute user path appears | PASS |
| Workflow change scan | branch delta inspection | no file under `.github/workflows/` is touched, so no green-run obligation arises | PASS |

## Appendix A: Test Inventory

| Test | Class | Scenario | Outcome |
|---|---|---|---|
| `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException` | `GetTableInViewAsyncFailureContractTests` | Absorbed default from the shared helper with an uncancelled token; asserts the throw plus factory count 2 and table-read count 0 | Passed |
| `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation` | `GetTableInViewAsyncFailureContractTests` | Factory cancels the outer source then throws task-cancelled; asserts cancellation propagates | Passed |
| `GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException` | `GetTableInViewAsyncFailureContractTests` | Counter 2 with a task-cancelled factory; asserts the message names retry 2 and 750 ms | Passed |
| `GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner` | `GetTableInViewAsyncFailureContractTests` | Counter 2 with a timeout-throwing factory; asserts inner-exception reference identity | Passed |
| `GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout` | `GetTableInViewAsyncFailureContractTests` | Generic absorb with a cancelled token; asserts cancellation is not relabelled as a timeout | Passed |
| `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` | `OlTableExtensions_Tests` | Pre-existing cancellation contract, file deliberately unmodified | Passed |
| Remaining suite | 9 assemblies | Full regression population | 7197 executed, 7197 passed, 0 failed |

## Appendix B: Toolchain Commands Reference

| Order | Command | Exit code |
|---|---|---|
| 1 | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | 0 and 0 |
| 2 | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 |
| 3 | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 |
| 4 | `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` | 0 |

## Deviations and Assumptions

1. PR context artifacts `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are absent from this worktree and could not be regenerated, because regeneration requires the shell tool the caller constraint forbids. Scope was instead derived from the coordinator's anchored merge-base diff and independently corroborated file by file against the worktree. The corroboration matched the supplied diff on every hunk examined, so the substitution is treated as sound for scope purposes.
2. Coverage figures are read from committed Markdown projections rather than from raw coverage XML, because the spec's ratified evidence convention for this item directs that raw XML be discarded. The projections state their own commands, denominators and aggregation rules, and their two independent figures for the modified file agree with each other.
3. The head commit `d373ddb9` is later than the `CODE-COMMIT-SHA: 3d680a4f` recorded in the evidence, because subsequent commits carry plan check-offs and the final evidence artifacts. The code content audited is the content on disk at head, read directly, not the content implied by the earlier SHA.
