# Feature Audit — gettableinviewasync-returns-null-on-timeout (Issue #838)

- Artifact timestamp: 2026-09-13T11-05
- Branch: `bug/gettableinviewasync-returns-null-on-timeout-838`
- Head commit: `d373ddb9d75213507762640c3b8f574a02d3c731`
- Baseline: `origin/main`, merge base `2405a829d6afd3b12eb7c228d57158a97cb4e2ca`
- Work mode: `full-bug`, read from the `- Work Mode: full-bug` marker in `issue.md`
- Acceptance-criteria source: `spec.md`, section `## Acceptance Criteria`, only. `user-story.md` is context only and states so in its own authority notice; `issue.md` carries no acceptance-criteria section for this mode.

## Acceptance Criteria Inventory

`spec.md` carries 20 checkbox lines in total. Sixteen of them are the numbered acceptance criteria under the `## Acceptance Criteria` heading, and all sixteen are checked. The remaining four are not acceptance criteria and are correctly excluded from this audit:

- Three form the Blocker / Medium / Low severity checklist in the Context section, of which High is the selected value. A severity checklist is a single-select control, so its unselected members are not unmet criteria; the feature-promotion lifecycle forbids treating them as such.
- The fourth is the High severity line itself, which is checked.

Criterion 12 carries a dated amendment authored by the atomic planner, recorded inline in `spec.md` rather than in a separate document. The amendment replaces the test that verifies the criterion's ordering claim and states why the original nominee could not decide it. The amendment is dated before delivery, is self-explanatory, and is corroborated by `evidence/baseline/ac12-amendment-confirmed.2026-09-12T16-09.md`. It is treated as part of the criterion.

| Count | Value |
|---|---|
| Acceptance criteria in the authoritative source | 16 |
| Checked in the source at audit time | 16 |
| Evaluated PASS by this audit | 16 |
| Evaluated PARTIAL | 0 |
| Evaluated FAIL | 0 |
| Check-offs found to be unjustified by evidence | 0 |

## Acceptance Criteria Evaluation

| # | Criterion, abbreviated | Verdict | Evidence establishing it |
|---|---|---|---|
| 1 | Absorbed-default path throws `TimeoutException` instead of returning null | PASS | `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException` recorded `Passed` in `evidence/qa-gates/named-test-outcomes.2026-09-12T16-09.md`. The red-green pair is independently recorded: `evidence/regression-testing/fail-before...` exit 1 with the assertion message `no exception was thrown`, and `evidence/regression-testing/pass-after...` exit 0 with the same test passing and the test file unchanged between runs. Code confirmed at `OlTableExtensions.TableAccess.cs` lines 151-157. |
| 2 | That test proves the failure came from the shared helper's internal retry, factory invoked twice and table read zero times | PASS | The two assertions are present in the test body at lines 133-134, `factoryInvocations.Should().Be(2)` and `tableReadInvocations.Should().Be(0)`, and the test is recorded `Passed`. The counters are incremented inside the injected factory and inside the mocked `GetTable` setup, so they measure the two quantities the criterion names. |
| 3 | Retry-ceiling branch of the task-cancelled catch throws `TimeoutException` naming counter and budget | PASS | `GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException` recorded `Passed`; its body asserts the message contains `retry 2` and `750 ms`. Code confirmed at line 124, `throw AcquisitionTimeout(counter, timeoutMs, e);`. |
| 4 | Retry-ceiling branch of the timeout catch wraps the caught exception as `InnerException` | PASS | `GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner` recorded `Passed`; it asserts `InnerException` is the same instance the factory threw, by reference. Code confirmed at line 147. |
| 5 | Outer-token cancellation still surfaces as `OperationCanceledException` and the cancellation branch rethrows | PASS | Both named tests recorded `Passed`: the pre-existing `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException`, whose containing file `OlTableExtensions_Tests.cs` is absent from the branch diff and from the porcelain status per `evidence/qa-gates/existing-tests-unmodified...` (`OLTABLE_TESTS_COUNT=0`), and the new `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation`. Code confirmed at lines 105-108, a bare `throw;`. |
| 6 | No `OperationCanceledException` catch added and no consumer-side catch list widened | PASS | `evidence/qa-gates/no-operationcanceled-catch...` records `OCE_CATCH_TA=0` and `OCE_CATCH_FAILURES=0` with a positive control of 3 for `catch (TaskCanceledException` in the same file, unchanged from the pre-change count. `evidence/qa-gates/consumers-untouched...` records `FRAMEBUILDING_COUNT=0` and `DFDEEDLE_COUNT=0` over a 52-entry diff listing and a 2-entry status listing. I independently read `QfcDatamodel.FrameBuilding.cs` and confirmed its catch list is still one `TaskCanceledException` clause and one `System.Exception` clause. |
| 7 | Null-forgiving operator gone from the return; compiles clean under per-file nullable with warnings as errors | PASS | `evidence/qa-gates/no-null-forgiving-return...` records `RETURN_BANG_COUNT=0` and `RETURN_ANY_BANG_COUNT=0` against a pre-change count of 1, the generalised pattern closing the rename loophole. `evidence/qa-gates/msbuild-nullable...` records `MSBUILD_EXIT=0`, `ERROR_CS86_LINES=0` and `CORECOMPILE_SKIPPED=0`, so the gate compiled rather than returning a warm exit. Direct read confirms line 159 is `return table;`. |
| 8 | No existing test assertion weakened, deleted or relaxed | PASS | `evidence/qa-gates/existing-tests-unmodified...` records all three protected files absent from both listings, and the one touched existing test file differing by exactly 2 content lines with `SHOULD_DIFF_LINES=0` and `NONCOMMENT_DIFF_LINES=0`. The two lines are quoted verbatim in the artifact and are comment prose. The assertion-bearing line count in that file is 10 before and 10 after. |
| 9 | Both stale prose comments corrected | PASS | `evidence/qa-gates/stale-comments-corrected...` records `LATENT_COUNT=0` and `MAKING_NULL_COUNT=0`, each against a pre-change count of 1 in its named carrying file, with both searches scoped to explicit paths because both phrases also occur in `spec.md` and the plan. I confirmed both replacements by direct read. |
| 10 | Each C# file in the Write Set at or under 500 lines | PASS | `evidence/qa-gates/file-line-counts...` records 473, 33, 301 and 289 with `OVER_LIMIT_COUNT=0`, measured after the final formatter pass. I confirmed 473 and 33 by direct read of the two production files and 301 by direct read of the new test file. |
| 11 | Both new `.cs` files registered for compilation in their non-SDK-style project files | PASS | Verified directly: `UtilitiesCS/UtilitiesCS.csproj` line 1069 carries the Compile item for the failures partial, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 550 carries the Compile item for the new test class. The analyzer build exited 0, which is what makes the registration effective rather than merely present. |
| 12 | Delivered implementation matches the four-step trace, guard checks the token before it throws | PASS | Each step checked against the code. The view cast at lines 66-73 remains the only pre-deadline validation and is unchanged. The deadline is raised inside the shared helper, invoked at lines 89-96. The absorbed default can no longer reach the return, because lines 153-157 intercept it. The guard calls `token.ThrowIfCancellationRequested()` at line 155 before `throw AcquisitionTimeout(...)` at line 156. The ordering is decided empirically by `GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout`, recorded `Passed`, which reaches the guard with a null local and a cancelled token and therefore distinguishes the two possible orderings. The amendment that substituted this test is recorded in `spec.md` with its reason and corroborated by `evidence/baseline/ac12-amendment-confirmed...`. |
| 13 | New tests introduce no banned symbol and no non-deterministic timing | PASS | `evidence/qa-gates/new-test-banned-symbols...` records `CANCELAFTER_COUNT=0`, `SLEEP_COUNT=0`, `DELAY_COUNT=0`, `TIMED_CTOR_COUNT=0` and `PLAIN_CTOR_COUNT=7`, each absence backed by a non-zero positive control from `evidence/baseline/absence-gate-positive-controls...`. I read the file in full and found no timing API and no argument-bearing cancellation-source constructor. |
| 14 | Full toolchain passes in order in one clean pass with no step auto-modifying a file | PASS | `evidence/qa-gates/toolchain-clean-pass...` tabulates all four canonical commands in CLAUDE.md order with exit code 0 each, and records `FORMAT_CHANGED_TREE=False` with `DIFFERING_ROW_COUNT=0` from the write-mode formatter pass, so the loop did not restart and nothing outside the Write Set changed. The four source artifacts each record their own exit code independently. |
| 15 | No regression on changed lines and at least 90 percent line coverage on changed and added code | PASS | `evidence/qa-gates/coverage-changed-lines...` records `DENOMINATOR_C_EXECUTABLE=16`, `DENOMINATOR_C_COVERED=16`, `DENOMINATOR_C_PERCENT=100`, with the ten executable added lines in the modified file and the six in the new file enumerated individually. `evidence/qa-gates/coverage-no-regression...` records the modified file rising from 255 covered of 281 to 265 covered of 283 under the identical nine-class-node aggregation on both sides, so the figures are comparable. The repository-wide figure is recorded in the same artifact as a report-only observation, which is what the criterion's own text prescribes. |
| 16 | No raw test-result or raw coverage XML artifact committed | PASS | `evidence/qa-gates/no-raw-xml-committed...` records `TRX_COUNT=0` and `COBERTURA_COUNT=0` over a 51-member union of added diff paths and untracked status paths, with `EVIDENCE_MEMBERS=44` and `EVIDENCE_OUTSIDE_THREE=0`. I independently enumerated the feature folder and the whole of `artifacts/` and found no `.trx` and no `.cobertura.xml` file anywhere in the tree. |

### Notes on evidence quality

Three properties of this evidence set are worth recording, because they are what distinguish a verified check-off from a claimed one.

The gates that assert an absence carry demonstrated positive controls. Each zero-count search has a recorded non-zero count for the same literal from the same search form, either pre-change in the same file or in a named other file. A zero from a search that cannot match is the standard failure mode of absence gates, and it is excluded here.

The two coverage figures for the modified file were produced by the same aggregation on both sides, and the artifact states why that matters: the file is a partial class contributing nine class nodes to the coverage document, six of them async state machines, so reading any single node would have under-counted the member the change touches. Figures produced under different aggregations would not have been comparable.

The red-green pair is real rather than reconstructed. The fail-before artifact records an assertion failure, explicitly confirms the failure message names no compiler error code, and pairs with a separate clean-build artifact, so the red state cannot be a compile failure masquerading as a defect reproduction.

## Acceptance Criteria Check-off

All 16 criteria were already checked in `spec.md` before this audit. Each check-off is justified by the evidence cited above, so no check-off was corrected and no criterion was newly checked or unchecked by this review. No edit was made to any requirement document.

- Criteria verified as correctly checked: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16.
- Criteria requiring un-checking: none.
- Criteria left unchecked in the source: none.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md`, section `## Acceptance Criteria`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Verdict: **PASS**. Sixteen of sixteen acceptance criteria are met and each check-off is justified by a named test outcome, a named gate artifact, or direct inspection of the delivered code. Blocking findings: 0.

The delivered behaviour matches the requirement the issue states. `GetTableInViewAsync` returns a non-null table or raises: a `TimeoutException` naming the retry counter and the millisecond budget when the acquisition budget is exhausted, an `OperationCanceledException` when the caller's token is cancelled, and the unchanged `InvalidOperationException` when the current view is not a table view. The null-forgiving suppression is gone, the method signature is byte-identical so all nine binding sites are unaffected, and the two files the spec designates as non-goals are absent from the diff.

Two items are recorded for the record and neither blocks. The repository-wide raw line rate of 70.67 percent is below the 85 percent figure in the rules files; it is pre-existing, this change moved it up, and criterion 15 deliberately does not gate on it. The canonical C# coverage artifact is absent by the spec's ratified commit-projections-only evidence convention, which criterion 16 gates on positively. Both are dispositioned in the policy audit for this timestamp.

One Low code-review finding stands: the reflective test scaffolding is now duplicated across three test classes. It is test-only, it mirrors an existing repository pattern, and resolving it requires editing files this spec protects, so it belongs in a follow-up rather than on this branch.
