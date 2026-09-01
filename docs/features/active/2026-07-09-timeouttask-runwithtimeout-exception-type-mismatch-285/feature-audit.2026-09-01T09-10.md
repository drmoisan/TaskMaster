# Feature Audit — Issue #285 (`TimeOutTask.RunWithTimeout<T1, TResult>` exception-type mismatch)

- **Timestamp:** 2026-09-01T09-10
- **Work mode:** `full-bug`, read from `- Work Mode: full-bug` at `issue.md` line 12
- **Sole AC source:** `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md`, `## Acceptance Criteria` heading (lines 265-278)
- **`user-story.md`:** absent, which is correct for `full-bug` and is not a finding
- **HEAD reviewed:** `46df4bf3779f7404bb4c91c7c400c19f5629bb4a`
- **Baseline:** merge base `2b85134b42872e405602e6064e02dc9cda6c319b`, recomputed in this session

## Verdict

**12 of 12 acceptance criteria PASS. Blocking findings: 0.**

Two criteria pass with a declared deviation from their literal wording; both deviations were
declared in the approved plan before execution and both are adjudicated as sound below. No bullet
was found checked that this reviewer's own evaluation does not support, so no bullet was unchecked
in `spec.md`.

## Per-Criterion Evaluation

The twelve bullets are addressed by position, matching the identifier map in
`plan.2026-09-01T00-30.md` lines 25-38.

| ID | Criterion (leading text) | Verdict | Basis |
| --- | --- | --- | --- |
| AC1 | New MSTest method exists; its failure output against unmodified production code is captured showing an escaping `TaskCanceledException` | **PASS (declared deviation)** | Method verified present in the post-change source. `evidence/regression-testing/p1-t6-red-new-test.md` records `EXIT_CODE: 1` against `ExpectedExitCode: 1`, `Total tests: 1`, `Failed: 1`, and the verbatim text `System.Threading.Tasks.TaskCanceledException: A task was canceled.` with stack frames through `<RunWithTimeout>d__6` and `<RunWithTimeout>d__5`. Deviation on "unmodified production code": see D-1. |
| AC2 | After the fix the test passes, asserting `"result-42"`, exactly one delegate invocation, exactly two factory invocations | **PASS** | `evidence/regression-testing/p2-t4-green-new-test.md`: `Total tests: 1`, `Passed: 1`, `Failed: 0`, `EXIT_CODE: 0`, 55 ms. The three assertions are present verbatim in the source at `TimeOutTask_OverloadCoverageTests.cs` lines 420-422: `result.Should().Be("result-42")`, `delegateCalls.Should().Be(1)`, `factoryCalls.Should().Be(2)`. Reviewer traced the control flow by hand and confirms the asserted counts are the only possible outcome. |
| AC3 | Zero matches for `Task.Delay`, `Thread.Sleep`, `Thread.SpinWait` in the test file; the new test passes `milliseconds: 30_000` with `CancellationToken.None` | **PASS** | Reviewer's own search of the post-change file returned zero matches for all three banned APIs. `milliseconds: 30_000` and `CancellationToken.None` are both present in the new method at lines 412 and 411. Corroborated by `evidence/qa-gates/p3-t9-test-hygiene.md`, which records 0/0/0 for the banned APIs, 1 for `milliseconds: 30_000`, and a file-level `CancellationToken.None` count of 17 against a baseline of 16. |
| AC4 | Both at-risk tests pass and `git diff` shows no change to either test method body | **PASS** | Verified independently by this reviewer, not accepted from the artifact. `git diff 2b85134b...HEAD -- UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` produced no output. The same diff on `TimeOutTask_OverloadCoverageTests.cs` contains zero deletion lines; hunk header `@@ -383,5 +383,45 @@` confirms a pure append. `evidence/regression-testing/p2-t5-at-risk-tests.md` records `Total tests: 2`, `Passed: 2`, `Failed: 0`. |
| AC5 | Line-anchored `catch` census returns exactly 9 `TaskCanceledException`, 3 `TimeoutException`, 10 `System.Exception e`, and exactly one filtered clause inside the private `Func<T1, TResult>` implementation | **PASS** | Reviewer ran an independent line-anchored census of the post-change file: `catch (TaskCanceledException)` at 65, 130, 286, 369, 447, 516, 599, 681, 762 (**9**); `catch (TimeoutException)` at 290, 836, 932 (**3**, the former lines 272, 818, 914 offset by the change's +18); `catch (System.Exception e)` at 85, 149, 238, 308, 390, 468, 537, 621, 703, 784 (**10**); one filtered clause at line 217, inside the private implementation declared at line 186. All four counts match the criterion exactly. |
| AC6 | Zero matches for `OperationCanceledException` in the production file | **PASS** | Reviewer's own search of the post-change file returned zero matches. Corroborated by `evidence/qa-gates/p3-t8-source-census.md`. The handler was not widened beyond `TaskCanceledException`; see the code review's Q1 analysis for why that narrowing is correct rather than merely compliant. |
| AC7 | Both the wrapper and the private implementation declare a trailing `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` with the `?`; the wrapper forwards it; the retry recursion inside the widened clause forwards it | **PASS** | Verified directly in the source. Public wrapper declaration at line 172, forwarding argument at line 182. Private implementation declaration at line 194, seam construction at line 200, recursion forwarding argument at line 230 inside the clause body opened at line 217. The `?` annotation is present on both declarations. Reviewer's own token search returns 10 occurrences at lines 27, 36, 47, 53, 77, 172, 182, 194, 200, 230 — five pre-existing on the `Func<TResult>` sibling and five added here. |
| AC8 | `dotnet tool run csharpier check .` reports no unformatted files | **PASS** | `evidence/qa-gates/p3-t2-format-check.md`: `EXIT_CODE: 0`, `Checked 1565 files in 4487ms.`, unformatted-file count 0. The pinned 1.2.6 manifest version was restored at P0-T5. |
| AC9 | Analyzer build completes with 0 errors and 0 new analyzer warnings | **PASS** | `evidence/qa-gates/p3-t3-analyzer-build.md`: `EXIT_CODE: 0`, `0 Error(s)`, `5 Warning(s)` against a P0-T7 baseline of 5, delta 0. All five are the ID-less `System.Reactive.PackagesConfigCheck.targets` warning. Zero diagnostics carrying a diagnostic code name either changed file. `/t:Rebuild` was used, so the gate is not vacuous. |
| AC10 | Nullable build completes with 0 errors, with no `/p:Nullable=enable` added | **PASS** | `evidence/qa-gates/p3-t4-nullable-build.md`: `EXIT_CODE: 0`, `0 Error(s)`. The quoted argument vector is `TaskMaster.sln \| /t:Rebuild \| /m \| /p:Configuration=Debug \| /p:Platform=Any CPU \| /p:TreatWarningsAsErrors=true` — contains `TreatWarningsAsErrors=true`, contains no `Nullable=enable`. This matches `.github/workflows/ci.yml` character for character. |
| AC11 | `vstest.console.exe` with `/EnableCodeCoverage` runs the full `UtilitiesCS.Test` and `QuickFiler.Test` assemblies with 0 failures, and the coverage report shows the modified catch clause and the modified timeout-source construction as covered | **PASS** | `evidence/qa-gates/p3-t5-vstest-utilitiescs.md`: 4771 passed, 0 failed, 0 skipped, against a baseline of 4770/0/0 — an increase of exactly one, the new test. `evidence/qa-gates/p3-t6-vstest-quickfiler.md`: 1272 passed, 0 failed, 0 skipped, unchanged from baseline. Both Phase 0 `BASELINE_FAILURE_SET`s are recorded as empty with cardinality 0, so "0 failures" is met literally. Coverage: `evidence/qa-gates/p3-t7-coverage.md` records hits of 1 at lines 217, 219, 199, 200 and 201. Independently corroborated by this reviewer from `coverage/p3-t7.cobertura.xml`: the class element `UtilitiesCS.TimeOutTask.<RunWithTimeout>d__6<T1, TResult>` reads `line-rate="1" branch-rate="1" complexity="10"` post-change against `line-rate="1" branch-rate="1" complexity="4"` at baseline — complexity rose by 6 with both rates held at 1.0, proving every branch the change introduced is executed. See N-1 on the two-run split. |
| AC12 | `git status --porcelain` and the branch diff against the merge base list only the two source files and paths under the feature folder | **PASS (declared deviation)** | `git status --porcelain` is empty, verified in this session. The branch diff lists 43 paths: the two source files, 39 feature-folder paths, and 4 files under `.claude/agent-memory/`. The four agent-memory files fall inside the exclusion class the approved plan provisions for this criterion at P3-T11 and P4-T12, and are attributable to branch commits `21a47aac` and `46df4bf3` rather than to the code change. Deviation: see D-2. |

## Declared Deviations

### D-1 — AC1, "against unmodified production code"

The red run necessarily carried the `timeoutSourceFactory` seam, because the regression test binds
that parameter by name and raises CS1739 without it. This reviewer judges the fail-before evidence
**sound**. Reasoning:

1. The seam is behaviour-preserving on the default path by construction: `(timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms)))(milliseconds)` with a `null` argument yields the same `CancellationTokenSource` the pre-change statement produced, and all seven pre-existing call sites bind unchanged.
2. The defect under test is the handler type, not the timeout source. The P1-T1 acceptance measurement, taken immediately after the seam edits and before any handler change, recorded an anchored `catch (TimeoutException)` count of 4 — unchanged from the P0-T12 baseline of 4 — and a filtered-clause count of 0. The defective clause was demonstrably in place at the moment of the red run.
3. The recorded stack trace positively identifies the defect mechanism rather than merely showing a failed assertion: the exception escapes through the private implementation state machine, then the public wrapper, then the test's `await`, which is the general-handler-rethrow path the spec's Root Cause Analysis predicts for `strict: true`.
4. The only alternative red mechanism — a genuinely short `milliseconds` value — is a banned wall-clock dependency under `.claude/rules/general-unit-test.md` and is the flakiness class that produced issue #253. The ordering was forced, was declared in the approved plan before execution, and is recorded in two evidence artifacts.

No remediation. The bullet stays checked.

### D-2 — AC12, exclusion of `.claude/agent-memory/`

AC12's own text carries no exclusion clause. The approved plan amends it explicitly at P3-T11 and
P4-T12, which define the exclusion set as exactly `.claude/agent-memory/` plus the P0-T6
unformatted-file list (cardinality 0), require every excluded entry to be enumerated by full path,
and specify the not-met condition under which the bullet must stay unchecked. That condition — a
non-empty P0-T6 list with one of its paths in the P3-T11 output — does not trigger.

This reviewer accepts the amendment. The four agent-memory files were read in full: they are
orchestration-trap documentation, contain no product code, no host paths, and no secrets, and do not
affect the change under review. Three were introduced by branch commit `21a47aac` before execution
began; the fourth by the post-execution commit `46df4bf3`.

One gap: the fourth file,
`.claude/agent-memory/atomic-executor/project_count_idiom_pitfalls_csharpier_and_measureobject.md`,
is not enumerated in `evidence/qa-gates/p3-t11-footprint.md` or
`evidence/qa-gates/p4-t14-commit-footprint.md`, because it was created after both were written.
Both artifacts enumerate three entries where the diff at HEAD carries four. This affects the plan's
enumeration requirement, not AC12's substantive requirement, and is recorded as **PA-6 in the policy
audit, non-blocking**. The bullet stays checked.

## Notes

### N-1 — AC11's two-run split

AC11 names one run: `vstest.console.exe` with `/EnableCodeCoverage` producing both the zero-failure
result and the coverage report. The executor split it into two invocations over the same assembly
and the same run settings: P3-T5 with `/EnableCodeCoverage` producing the 4771/0/0 result and a
binary `.coverage` attachment, and P3-T7 under `dotnet-coverage collect --output-format cobertura`
producing the readable report used for the hit-count analysis. P3-T7's own run reports the same
4771/0 result, so the two are consistent and the criterion is satisfied in substance by the pair.
The split is the standard workaround for the binary `.coverage` format not being directly readable.
Recorded for transparency; no verdict change.

### N-2 — Bug remediation actually achieves the stated intent

Beyond criterion-by-criterion compliance, this reviewer confirms the change does what the issue
asked for. The reported defect was that `catch (TimeoutException)` at the former line 200 could
never match a timer-driven timeout, so `maxAttempts` had no effect at either production call site.
After the change, the filter at line 217 matches `TaskCanceledException`, the retry recursion at
lines 223-231 threads the seam through, and the coverage report shows the clause body executing with
a hit count of 1. The two call sites at
`UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs` line 80 and
`UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs` line 139 now retry as configured. One
qualification on the framing of that benefit is recorded as CR-3 in the code review: because
`Task.Run` checks its token only before invoking the delegate, the retry fires when the work item was
not dequeued within `timeoutMs`, not when an already-started call is stalling.

### N-3 — Spec's five Non-Goals confirmed deferred, not silently dropped

All five remain resident and untouched, verified by this reviewer's own census: the two dead
`catch (TimeoutException)` clauses now at lines 836 and 932; the four inert-timeout implementations;
the inverted handler pair now at lines 286 and 290; the 527-line
`UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` (byte-identical to the merge base); and
no sibling overload modified. The spec's Rollout section commits to promoting each to its own issue
after merge. That promise plus the file-size follow-up recorded as PA-3 makes six items riding on a
post-merge action; the policy audit recommends filing them at PR time instead.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/spec.md
- Total AC items: 12
- Checked off (delivered): 12
- Remaining (unchecked): 0
- Items remaining: none
```

All twelve bullets were already `- [x]` in `spec.md` when this review began. This reviewer
independently evaluated each and found none whose evidence fails to support it, so no bullet was
unchecked and no bullet was newly checked. The counts were verified by counting checkbox lines
between the `## Acceptance Criteria` heading at `spec.md` line 265 and the next equal-or-shallower
heading (`## Risks & Mitigations`, line 280): `CHECKED=12`, `UNCHECKED=0`, `TOTAL=12`.

## Summary of Findings Across All Three Artifacts

| ID | Artifact | Severity | Classification |
| --- | --- | --- | --- |
| PA-1 | policy-audit | Major | Non-blocking |
| PA-2 | policy-audit | Major | Non-blocking |
| PA-3 | policy-audit | Major | Non-blocking |
| PA-4 | policy-audit | Minor | Non-blocking |
| PA-5 | policy-audit | Minor | Non-blocking |
| PA-6 | policy-audit | Minor | Non-blocking |
| PA-7 | policy-audit | Minor | Non-blocking |
| CR-1 | code-review | Major | Non-blocking |
| CR-2 | code-review | Minor | Non-blocking |
| CR-3 | code-review | Minor | Non-blocking |
| CR-4 | code-review | Minor | Non-blocking |
| CR-5 | code-review | Minor | Non-blocking |
| CR-6 | code-review | Minor | Non-blocking |

**Total blocking findings: 0.** No `remediation-inputs` artifact is produced.
