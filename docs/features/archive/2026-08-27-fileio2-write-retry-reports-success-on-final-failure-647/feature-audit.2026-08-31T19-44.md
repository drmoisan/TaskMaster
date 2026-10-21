# Feature Audit — Issue #647 (FileIO2 write retry reports success on final failure)

- Timestamp: 2026-08-31T19-44
- Branch: `bug/fileio2-write-retry-reports-success-on-final-failure-647`
- Head reviewed: `8e773f350671c29f2ff34803df63ac60d70ed648`
- Base: `9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c` (recomputed via `git merge-base HEAD origin/main`; equals `origin/main` tip)
- Work mode: `full-bug` (marker at `issue.md` line 12)
- Acceptance-criteria source: `spec.md` only, 21 criteria AC1–AC21

`user-story.md` does not exist in this feature folder. Under `full-bug` that absence is correct and is not recorded as a gap.

## Verdict

**PASS. Blocking findings: 0.** Non-blocking findings recorded in this artifact: 3 (F-1 through F-3).

Every criterion was evaluated independently against the tree at head rather than accepted from the executor's check-off. Where the executor's evidence was reconcilable against an on-disk artifact, it was re-derived; where it was not, the evaluation states what was reconciled and from where.

## Acceptance Criteria Evaluation

| AC | Verdict | Independent verification performed |
|---|---|---|
| AC1 | PASS | `FileIO2.cs:69-74`. Declared `public static Task<bool> WriteTextFileAsync(string filename, string[] strOutput, string folderpath, CancellationToken token)`. Parameter names, order and types are byte-identical to the pre-change declaration in the diff |
| AC2 | PASS | `FileIO2.cs:59-68`. The `<returns>` clause states `true` means the write completed and `false` means it did not, and states explicitly: "The method does not throw on a failed write" |
| AC3 | PASS | `FileIO2_Tests.cs:72-101`. Factory always throws `IOException`; asserts `exhaustionResult.Should().BeFalse()`, `exhaustionFactoryCalls.Should().Be(100)`, `exhaustionDelayCalls.Should().Be(99)`. Recorded Passed at 1 ms in `evidence/qa-gates/p6-t5-full-suite-vstest.md` |
| AC4 | PASS | `FileIO2_Tests.cs:36-65` with the `ThrowingOnWriteTextWriter` fake at `:258-266` whose `WriteLineAsync` throws `IOException`. Asserts `midWriteFactoryCalls == 1`, `midWriteDelayCalls == 0`, result `false`. Carries genuine fail-before evidence |
| AC5 | PASS | `FileIO2_Tests.cs:108-145`. Factory fails 3 times then returns a `StringWriter`; asserts `true`, 3 delay invocations, and content `"alpha" + Environment.NewLine + "beta" + Environment.NewLine` |
| AC6 | PASS | `FileIO2.cs:124`. `return true` sits after the `using` block closes, so the writer is disposed before success is reported. No assignment establishing success occurs between creation and completion of the writes; `opened = true` at `:117` marks post-open terminality, not success, and the comment at `:108-110` says so |
| AC7 | PASS | `FileIO2.cs:126` binds `catch (IOException ex)`. Both `logger.Error` calls pass `ex` to the two-argument overload: mid-write at `:130-133` ("Write to {filepath} failed after the writer opened. The file may hold a partial record.") and exhaustion at `:140-143` ("Failed to write to {filepath} after {attempts} attempts."). The two messages are textually distinct |
| AC8 | PASS | `FileIO2.cs:147` is `await delayAsync(100, token)`. The only `Task.Delay` in the method is the two-argument `Task.Delay(ms, t)` inside the production-default seam at `:102`. No single-argument `Task.Delay` remains |
| AC9 | PASS | `FileIO2_Tests.cs:218-251`. The delay seam appends its `CancellationToken` argument to `capturedTokens`; asserts `HaveCount(2)` and `OnlyContain(t => t.Equals(token))` against the token supplied to the method |
| AC10 | PASS | Both entry points present. Already-cancelled: `:152-176`, asserts `OperationCanceledException` and `cancelledFactoryCalls == 0`. Cancelled from inside the delay seam: `:184-211`, asserts `OperationCanceledException` and `retryCancelFactoryCalls == 1`, a bounded count |
| AC11 | PASS | `FileIO2.cs:83-90` declares the `internal static` overload with `Func<string, TextWriter>? writerFactory` and `Func<int, CancellationToken, Task>? delay`; the public overload forwards with `null, null` at `:74`. The diff adds no `static` field or property to `FileIO2`. Repository-wide `InternalsVisibleTo` occurrence count measured at head is 37, equal to the baseline 37 recorded in `evidence/baseline/p0-t18-internalsvisibleto-count.md` |
| AC12 | PASS | `QfcHomeController.Metrics.cs:28-34`. The `MetricsFileWriter` property's final type argument is `Task<bool>` |
| AC13 | PASS | `QfcHomeController.Metrics.cs:179-191`. `bool metricsWritten = await MetricsFileWriter(...)` assigns to a named local, followed by `if (!metricsWritten) { logger.Error(...); }`. Not a bare discarding `await` |
| AC14 | PASS | `QfcHomeController.Metrics.cs:183` is `CancellationToken.None`; the explanatory comment at `:176-178` is retained verbatim |
| AC15 | PASS | `AppOlObjects.cs:306-336`. Block-bodied lambda, `bool movedMailsWritten = await FileIO2.WriteTextFileAsync(...)`, `if (!movedMailsWritten) logger.Error(...)`, wrapped in `try { } catch (Exception ex) { logger.Error(..., ex); }` so no exception escapes the async void body |
| AC16 | PASS | `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` is absent from `FileIO2_Tests.cs` at head. Reading the whole file confirms no `FileShare.None`, no `new FileStream(`, and no call to the public four-argument overload — every `WriteTextFileAsync` call in the file binds the seam overload through the `writerFactory:` named argument |
| AC17 | PASS | Six `MetricsFileWriter` assignments at `QfcHomeControllerMetricsTests.cs:130, 335, 359, 383, 410, 439`. A grep for `Task.CompletedTask` in that file returns zero matches. The `async` double at `:359` now contains `return true;` at `:363`. The seam comment at `:125-129` describes the post-fix contract ("returns false rather than reporting success") |
| AC18 | PASS | Confirmed by reading both changed test files in full and by `evidence/qa-gates/p5-t10-banned-api-audit.md`: 0 occurrences of `Thread.Sleep`, `Task.Delay`, `GetTempPath`, `CreateDirectory`, `File.Create`, `File.WriteAllText`, `new FileStream(` |
| AC19 | PASS | Independently re-derived: `git diff --name-only 9b6aff2e..HEAD` returns exactly the 5 named source files plus feature-folder paths, and nothing else. No `.csproj`, `.editorconfig`, `coverage.config` or `AssemblyInfo.cs` appears. `FileIO2.WriteTextFile` (synchronous) and its callers are unmodified |
| AC20 | PASS (with two recorded literal deviations — see F-1) | Re-derived from `coverage/coverage.cobertura.xml`: changed-method span coverage 38/40 = 95.00% (clears the 90% clause); `FileIO2.cs` 121/137 covered against a baseline of 106/126, so no changed line regressed; repository-wide figures captured at both ends in `evidence/baseline/p0-t16-coverage-figures.md` and `evidence/qa-gates/p6-t6-full-suite-coverage.md`, and the head figure of 85.29% clears CLAUDE.md § UT2's testable-denominator floor of 80% |
| AC21 | PASS | `evidence/qa-gates/p6-t8-loop-closure.md`: `FINAL_ITERATION: 1`, all seven cited Phase 6 artifacts at iteration 1, all six command-bearing artifacts exit 0, in the required order. `p6-t1-format.md` records `REWRITTEN_FILE_COUNT: 0` with ten SHA-256 hashes, so the pass involved no auto-fix. All five hashes still match at head, binding the pass to the reviewed tree. The in-place test re-invocation is recorded as N-5 in the policy audit |

## Baseline Comparison

The defect and its fix are both measured relative to the pre-change state rather than asserted.

| Behavior | Pre-change (baseline) | At head | Source |
|---|---|---|---|
| Retry exhaustion | 100 factory calls, 99 delays, then normal return with no failure signal, and the exhaustion log unreachable in the mid-write case | 100 factory calls, 99 delays, then `false` with the causing `IOException` logged | `evidence/regression-testing/p3-t4-exhaustion-characterization.md`; test at `FileIO2_Tests.cs:72` |
| Mid-write failure | Delay invoked once, method returns reporting success, no log entry | Delay invoked zero times, method returns `false`, distinct log entry with the exception | `evidence/regression-testing/p3-t2-midwrite-fail-before.md` (failing, `found 1`) and `p4-t10-midwrite-pass-after.md` |
| Retry delay cancellation | `Task.Delay(100)`, uncancellable | `await delayAsync(100, token)` routed through the caller's token | `FileIO2.cs:147`; test at `FileIO2_Tests.cs:218` |
| Test suite | 6899 total, 0 failed; `FileIO2_Tests` held a ~10 s exclusive `FileShare.None` lock on a shared fixture | 6899 total, 0 failed; all six new tests complete in 1–8 ms each with no filesystem access | `evidence/baseline/p0-t19-baseline-failure-set.md` (`none`); `evidence/qa-gates/p6-t5-full-suite-vstest.md` |
| Analyzer build | 0 errors, 5 warnings | 0 errors, 5 warnings | `p0-t13` vs `p6-t3` |
| Nullable / TreatWarningsAsErrors build | 0 errors, 5 warnings | 0 errors, 5 warnings | `p0-t14` vs `p6-t4` |
| Repository line rate | 0.853296 (54820/64245) | 0.852919 (54835/64291), re-derived directly from the Cobertura root element | `p0-t16` vs parsed `coverage/coverage.cobertura.xml` |
| Repository branch rate | 0.793089 | 0.792754, re-derived directly | same |
| `FileIO2.cs` covered lines | 106 of 126 (84.13%) | 121 of 137 (88.32%), re-derived directly | `p0-t17` vs parsed Cobertura class element |
| `WriteTextFileAsync` line rate | 0.793103 (23/29) | 0.950000 (38/40), re-derived by span selection over the class's `<line>` elements | `p0-t17` vs parsed Cobertura |

The 5 build warnings at both ends originate in `System.Reactive.PackagesConfigCheck.targets`, carry no diagnostic identifier, and are unchanged by this work.

## Non-blocking Findings

**F-1 — AC20 carries two literal sub-clause deviations, both pre-authorized by the same document that states the criterion.** Recorded so the divergence is auditable rather than silently absorbed.

1. *"Every changed line in `UtilitiesCS/To Depricate/FileIO2.cs` is exercised by the new tests."* Two changed lines measure `hits="0"`: line 74, the public overload's forwarding expression, and line 101, the wrapped right operand of the writer-factory coalescing expression. I re-derived this zero-hit set directly from the Cobertura document and it is exactly the pair the executor enumerated. The literal clause is therefore not satisfied for those two lines.
2. *"The repository-wide line-coverage figure ... is not lowered by this change."* The rate fell from 0.853296 to 0.852919, a shortfall of 0.000377, so on a strict reading the figure was lowered. The absolute covered-line count rose, from 54820 to 54835.

Both deviations are covered by provisions in `spec.md` itself, not merely by the plan:

- The Test Strategy section states: "One line is expected to remain uncovered — the production default delay lambda inside the public overload's forwarding call — and that is accepted rather than covered by a wall-clock test." The spec therefore pre-accepts uncovered production-default lines in this method. The observed uncovered lines are of exactly that character, and covering either would require filesystem I/O, which `.claude/rules/general-unit-test.md` prohibits.
- The same section states: "no merge-base coverage baseline has been captured for this feature yet, so no repository-wide figure is asserted as a blocking gate here. The blocking obligations are change-scoped." The spec designates the change-scoped obligations as the binding ones, and both are met and independently verified: 95.00% on the changed method against a 90% floor, and zero regression on changed lines.

AC20 is therefore graded PASS on the obligations its own source document designates as blocking, and the checkbox is left checked. The two literal deviations are recorded here in full so a maintainer can overturn this grading on the evidence rather than having to rediscover it. No remediation is proposed, because neither deviation admits an achievable remedy under the test policy.

**F-2 — The plan widened the spec's accepted-uncovered-line set from one line to three.** `spec.md` accepts a single uncovered line and describes it as "the production default delay lambda inside the public overload's forwarding call", a description that does not match the implemented shape: the production defaults ended up in the internal seam overload, not in the public forwarder. The plan enumerated three permitted lines (74, 101, 102) and the evidence gates against that list. Two of the three are observed uncovered, so the observed set is a subset of the plan's list but a superset in count of the spec's. The mismatch is a documentation drift between the spec's description and the delivered code shape, not a functional gap. Recommendation: correct the spec's Test Strategy sentence at close-out so the accepted set matches what was built.

**F-3 — AC3's fail-before evidence is a dossier rather than a failing test run.** This is anticipated and argued in `spec.md` Risk 5: a test asserting a `false` return can only be written against the post-fix signature, and that signature change is itself the fix, so no ordering exists in which such a test fails against unfixed source. The substitute proof is adequate: `evidence/regression-testing/p3-t4-exhaustion-characterization.md` records a deterministic pre-fix run in which the always-failing open path consumed 100 factory invocations and 99 delays and still returned with `NotThrowAsync` passing, which measures the defect rather than asserting it. Defect 2 carries a genuine failing pre-fix run. The bugfix-workflow requirement in CLAUDE.md is met in substance for both defects. Recorded for transparency.

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/spec.md
- Total AC items: 21
- Checked off (delivered): 21
- Remaining (unchecked): 0
- Items remaining: none
```

No criterion was unchecked by this review. AC20 was the only criterion whose grading required judgment; it is graded PASS with the two literal deviations recorded under F-1, and no checkbox state in `spec.md` was modified.

## Summary

- Blocking findings: **0**
- Non-blocking findings in this artifact: **3** (F-1 through F-3)
- Acceptance criteria: 21 of 21 PASS
- The change fixes both defects the spec identifies, and both fixes are backed by measured pre-change and post-change behavior
- Recommendation: **GO**
