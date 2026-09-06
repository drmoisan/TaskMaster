# Code Review — Issue #791 (quickfiler-high-confidence-cancel-teardown-and-deadline-defects)

- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (cycle 1)
- **Base:** `main` @ `7c8ac9ae34b8b3dda9134a5e310f39742fd2f0b6`
- **Head:** `bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791` @ `59536368756d979f3f72268dfb4dfd0d4b2f7d9f`
- **Scope:** the full branch diff against the merge base — 17 `.cs`/`.csproj` files, 40 documentation
  and evidence files, 6 agent-memory files. No caller instruction narrowed this scope.
- **Companion artifacts:** `policy-audit.2026-09-06T15-31.md`, `feature-audit.2026-09-06T15-31.md`

## Executive Summary

The change is well constructed and the review found no blocking defect. Two separate defects are
fixed with a design that is stated before it is written, justified in place, and pinned by tests that
retain real discriminating power.

Three things stand out as above the repository norm:

1. **The retargeting is honest.** Seven pre-existing tests encoded the superseded #424/#608
   empty-at-the-deadline behavior. All seven were retargeted rather than deleted, and each keeps the
   discrimination that made it valuable. `DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop`
   still reports `sourceActive: () => true`, so exhaustion is not an available explanation for the
   empty batch. `DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates` swaps a 4 s
   deadline for a cap of 4 and thereby preserves its original take-count and residual assertions at
   exactly 4 and 6. The new #608 pin injects a cap of 2 that is deliberately smaller than the 21
   candidates it scans, so a guard widened to evaluate the bounds after an acceptance would fail it.
2. **An architecture pin was respected rather than relaxed.** Introducing an `IEmailMoveMonitor` local
   inside an `async` method made the compiler-generated state machine a fourth type declaring such a
   field, which broke the #731 three-owner topology pin. The repair moved the snapshot into a
   synchronous helper so no state machine is generated, instead of changing the pin's expected count
   from 3 to 4. The reason is written into the helper's XML doc.
3. **The RED-first evidence reproduces the reported failure, not a proxy for it.**
   `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` fails before
   the fix with `System.ArgumentException: Delegate to an instance method cannot have null 'this'`,
   character-for-character the message in the attached production log, reproduced deterministically
   without Outlook.

Sixteen findings are recorded below: six Minor with a concrete recommendation, ten Observations. None
is blocking. Two of the Minor findings (N1, N2) concern the exception-safety guarantee the change
itself states, and are the most useful items to act on; N2 cannot be fixed on this branch because the
file it lives in is an explicit AC5 non-goal.

Independently verified by this reviewer at head, not read from a delivery artifact: `csharpier check`
(1587 files, exit 0), both `/t:Rebuild` gate builds (exit 0, 0 warnings, 0 errors), the
`QuickFiler.Test` assembly (1362 tests, exit 0), and per-file coverage for all seven changed
production paths from both Cobertura documents.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | `QuickFiler/Controllers/QfcHomeController.cs` | `:389` (`_tokenSource?.Dispose();`) | The cancellation token source is disposed but the field is not nulled. The same `CancellationTokenSource` instance is handed to the datamodel at `:125` and to the form controller at `:144`, and `QfcDatamodel.Cleanup()` and `QfcDatamodel.QuiesceLoaderAsync()` both begin with `_tokenSource?.Cancel()`. `CancellationTokenSource.Cancel()` after `Dispose()` raises `ObjectDisposedException` on .NET Framework 4.8, so a repeat `Cleanup()` would throw where it previously could not. Before this change the source was never disposed, so this failure mode did not exist. It is currently unreachable: `QfcFormController.Cleanup()` nulls `_parentCleanup` after invoking it and nulls `_parent`, and `RibbonController` never calls `QfcHomeController.Cleanup()` directly — it only supplies `ReleaseQuickFiler` as the callback (`RibbonController.cs:106,120,141`). | Set `_tokenSource = null;` on the line after `Dispose()`, and extend `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` with a second `Cleanup()` call asserting no ERROR-level stage failure. | The change's own goal is that repeat teardown is inert. Leaving a disposed-but-reachable source in a field is the one place on the branch where a second pass acquires a throw it did not have before, and the only thing keeping it unreachable is a nulling in a file the branch deliberately does not touch. | Read of `QfcHomeController.cs:118-150,367-405`, `QfcDatamodel.cs:74-90`, `QfcDatamodel.QueueProcessing.cs:44-63`, `QfcFormController.SetupDisposal.cs:250-261`, `TaskMaster/Ribbon/RibbonController.cs:100-150` |
| Minor | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | `:213-261`, specifically `:251` and `:259` | The stated invariant — "`RibbonController.ReleaseQuickFiler` has been invoked exactly once regardless of which teardown stage threw" (`spec.md:116`) — holds for the two outer links but not the middle one. `ActionCancelAsync` runs `Cleanup` under `finally`, and `QfcHomeController.Cleanup()` invokes `ParentCleanup` under `finally`, but `QfcFormController.Cleanup()` calls `_parentCleanup?.Invoke()` as its last statement with no `try`/`finally`. A throw from `_formViewer?.Dispose()` at `:251`, or from `Controls.ForAllControls` at `:185`, skips the invocation and the ribbon buttons stay inert for the session — exactly the failure the fix is meant to eliminate. No test covers a throw inside `QfcFormController.Cleanup()`; the two existing exception tests cover the groups-cleanup stage and the datamodel-cleanup stage, both of which sit outside this method. | Do not change it on this branch: `QfcFormController.SetupDisposal.cs` is an explicit AC5 non-goal (`spec.md:85`) and editing it would break AC5. Promote a follow-up issue to wrap `:215-258` in a `try` with `_parentCleanup?.Invoke(); _parentCleanup = null;` in a `finally`, and add the matching exception test. | The invariant is written as unconditional. A reader who trusts it will not re-check the middle link, and the residual gap is the same class of defect the issue reports. Recording it now is cheaper than rediscovering it from a field log. | Read of `QfcFormController.SetupDisposal.cs:213-261`, `QfcFormController.EventHandlers.cs:168-172`, `QfcHomeController.cs:396-403`; `spec.md:85,116` |
| Minor | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` | class scope; corresponds to `QfcStreamingDequeueConfidenceGate.cs:346-361` | AC1 requires that "the bound decision is logged", and `LogScanBoundReached` emits a `Bound=scan-cap` / `Bound=zero-acceptance-ceiling` discriminator plus `Decision=stop`. No test asserts any of it. A search of the whole `QuickFiler.Test` tree for `scan bound reached`, `Bound=` and `Decision=stop` returns no match. The line executes during the two bound tests, so it is covered, but its content is unpinned: swapping the two bound names, or deleting the call, would not fail a test. The sibling lines are pinned — `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` asserts `Cutoff=900`, `Accepted=0`, `Scanned=3`, and `DequeueAsync_Launch_LogsCutoffQuantityAndBounds` asserts five fields. | Pass `debugLog: logs.Add` in `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` and assert the emitted line contains `Bound=scan-cap`; do the same in `_ZeroAcceptedAndCeilingReached_` asserting `Bound=zero-acceptance-ceiling`. Both are two-line additions using the seam the file already uses. | The bound discriminator is the single piece of information an operator needs to tell an item-cap exit from a time-ceiling exit, and diagnosability of the bounded exit is an explicit AC1 clause. Coverage alone does not protect a log message's content. | Grep of `QuickFiler.Test` for `scan bound reached\|Bound=\|Decision=stop`: no matches. Read of `QfcStreamingDequeueConfidenceGateTests.Part4.cs:127-207` and `QfcStreamingDequeueConfidenceGate.cs:340-362` |
| Minor | `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` | `:66-67` and `:219-222` | `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))` and `loaderEntered.Task.Wait(TimeSpan.FromSeconds(5))` are real wall-clock bounded waits. `.claude/rules/general-unit-test.md`, "Determinism Infrastructure", lists "real wall-clock waits" among the banned APIs in test code. Both are condition-driven rather than fixed sleeps, both fail with a clear message if the condition never holds, and both are verbatim copies of the pre-existing convention at `QfcDatamodelLivenessTests.cs:56,103,173` and `QfcInitEmailQueueZeroBatchTests.cs:161`, which the new file's own docstring cites as the convention it follows. The boundary being crossed is genuinely awkward: `Worker_DoWork` is `async void` on a `BackgroundWorker` thread and exposes no completion handle a test can await. | No change on this branch. Matching the established local style is what the General Code Change Policy §7.1 instructs, and diverging here would leave two idioms for the same boundary. Track repository-wide: an awaited completion seam on the worker boundary would let all four call sites drop the timed wait. | The rule is real and the exception is real. Recording both, with the precedent, is more useful than either scoring a FAIL against a repo-wide convention or leaving the divergence unmentioned. | Read of `QfcDatamodelTeardownTests.cs:59-67,201-233`; grep of `QuickFiler.Test` for `SpinUntil\|\.Wait\(` showing four pre-existing sites |
| Minor | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | whole file; attribute at `QuickFiler/Controllers/QfcDatamodel.cs:25` | 115 lines of new production code — `QuiesceLoaderAsync`, `LogQuiesceOutcome`, the relocated `TryQueueRemainingMailItemAsync`, `TryCreateRemainingQueueAdmission`, `_remainingLoadTask`, `QuiesceDebugLog` — land inside a type carrying a class-level `[ExcludeFromCodeCoverage]`. Both the baseline and the post-change Cobertura emit **zero** `class` elements for both `QfcDatamodel` partials, so this code is outside the coverage denominator entirely. The attribute is pre-existing and this branch neither added nor extended it. `.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy ("No production file may be excluded from coverage measurement") and `CLAUDE.md` UT2's ratified `[ExcludeFromCodeCoverage]` exemption are in direct conflict here; the conflict pre-exists the branch. | Promote a follow-up issue to extract the host-neutral queue and quiesce logic out of `QfcDatamodel` into a testable type, leaving only COM-bound wiring behind the attribute. Do not resolve it on this branch. | The immediate risk is low — `evidence/qa-gates/p3-t7-changed-line-coverage.md` names a passing test for each changed member and this reviewer confirmed all five are recorded `PASS-AFTER` — but each addition makes the excluded surface larger and the substitute-evidence table longer, which is a maintenance cost that compounds silently. | Per-`filename` enumeration of `class` elements in both Cobertura documents returns `ABSENT` for both partials; `evidence/baseline/p0-t12-coverage-measurability.md`; `evidence/qa-gates/p3-t7-changed-line-coverage.md:40-65` |
| Minor | `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/runbooks/live-outlook-cancel-teardown-verification.runbook.md` | `:16` | The committed runbook embeds an absolute host path including the account name: `C:\Users\DanMoisan\repos\TaskMaster\TaskMaster\bin\Debug\TaskMaster.vsto`. This is the only such occurrence anywhere on the branch; a scan of all 1671 added source lines for `C:\Users\` returns zero hits, and every other document in the feature folder is clean. | Replace with `<repo-root>\TaskMaster\bin\Debug\TaskMaster.vsto`, or with `%USERPROFILE%\repos\TaskMaster\...` if the runbook's human reader needs a runnable form. | Committed artifacts outlive the machine they were written on. An account name in a document that will be read by whoever performs HI-1 has no operational benefit that a placeholder does not also provide. | Grep of the feature folder for `DanMoisan`: one match, at the cited line |
| Observation | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, `QuickFiler/Controllers/QfcHomeController.cs` | `EventHandlers.cs:26-34,160-163`; `QfcHomeController.cs:381-394` | Five broad `catch (System.Exception)` handlers are added. `.claude/rules/general-code-change.md` allows a broad catch only at a defined boundary with added context; all five qualify — each logs the stage name and the exception at ERROR — and AC2 explicitly requires that a throwing stage cannot skip a later one and that every exception is logged, so this is the specified design, not a shortcut. The residual is that a programming error inside any teardown stage now surfaces only as a log line an operator has to go looking for. | No change. The two catches the spec says must not be widened were verified intact: the per-item boundary catch in the deactivate routine and the gate's rejection-sink catch. | Recorded so the tradeoff is visible rather than discovered later from an ERROR line nobody read. | Regex scan of added lines: 5 `catch (System.Exception` / `catch (Exception`; read of each site |
| Observation | `QuickFiler.Test/QuickFiler.Test.csproj` | whole file | 528 lines at head, 524 at base, above the 500-line ceiling in `.claude/rules/general-code-change.md`. The rule enumerates "production code, test code, or reusable script file"; an MSBuild project file is none of these, and the growth is four `<Compile Include>` entries that the legacy non-SDK project format requires for the four new test files. Pre-existing. | No change. | Recorded because the ceiling is otherwise applied mechanically to every changed file and this one would look like an unexplained omission. | Line counts at base and head for every changed `.cs`/`.csproj` path |
| Observation | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`, `QuickFiler/Controllers/QfcHomeController.cs` | whole files | Three changed files sit within four lines of the 500-line ceiling: 498, 497 and 496 respectively. All three are under the limit and all three are compliant. | No change now. The next edit to any of them will need a split; `QfcStreamingDequeueConfidenceGateTests` already demonstrates the pattern with four partial-class parts. | The branch author already anticipated this once, creating `Part4.cs` rather than growing an existing part. Naming the three files makes the next author's decision explicit. | Line counts at head |
| Observation | `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | `:376-397` | `Cleanup_SourceContainsNoSynchronousWait` is the #731 forward guard asserting `.Wait(`, `.Result`, `Thread.Sleep` and `Task.Delay` are absent from the teardown path. It reads only `QfcFormController.SetupDisposal.cs`. The Cancel-path ordering and the awaited loader quiesce now live in `QfcFormController.EventHandlers.cs`, which the guard does not scan. | Consider extending `ReadDisposalPartialSource()` to include the `EventHandlers.cs` teardown region in a follow-up. Not required by any acceptance criterion here. | This reviewer read the new path and confirmed it contains none of the four literals, so nothing is wrong today. The guard's coverage simply did not move with the code it protects. | Read of `QfcFormControllerCleanupTests.cs:371-397` and `QfcFormController.EventHandlers.cs:126-173` |
| Observation | `artifacts/pr_context.summary.txt` | "Changed files overview" section | The summary reports `Core logic changes: 0 files` and `Docs/templates/agents/tooling: 46 files` while the branch changes 17 `.cs`/`.csproj` files. The C# files are absent from every bucket, not misfiled into one. A simulation of `.claude/hooks/validate-feature-review-coverage.ps1`'s `Get-ChangedLanguageSet` against this summary returns an empty language set, meaning the coverage hook would classify this C#-only branch as having zero changed languages and would skip its own enforcement entirely. | Report the collector defect upstream. Reviewers should continue deriving the changed-file set from `git diff --numstat`, as this audit did. | The hook that exists to guarantee a coverage verdict is disarmed by the artifact that feeds it. The correctness of this audit does not depend on the summary, but the guarantee does. | `Get-ChangedLanguageSet` simulation: `count=0`; `git diff --numstat` shows 17 code paths |
| Observation | `docs/.../evidence/qa-gates/p3-t5-tests-coverage.md`, `p3-t8-coverage-delta.md` | absolute counters only | The delivery aggregates Cobertura with an all-descendant `.//line` selection, which counts a source line once under `class/lines/line` and again under `class/methods/method/lines/line`. Its absolute counters (112551/133187 lines, 26584/33568 branches) are therefore roughly double the class-level counters this reviewer computed (55783/66009 and 13292/16784). The derived percentages are unaffected: 84.51% and 79.19% reproduce exactly under both selections, and the delivery's own comparability precondition correctly refuses to compare the absolute counts across unequal denominators. | Prefer `classes/class/lines/line` in future aggregations so the absolute counters are directly meaningful. No correction is needed to any conclusion drawn on this branch. | An artifact whose absolute counters are twice the real figure invites a later reader to compare them against a differently-computed number and reach a wrong conclusion. | This reviewer's two-selection aggregation over the same document |
| Observation | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, `QuickFiler/Controllers/QfcHomeController.cs` | whole files | Per-file line coverage is 58.12% and 76.36%, both below the 85% uniform per-file floor. Both improved (from 49.61% and 75.85%), both improved on branch coverage (+10.86 and +3.90 points), and no changed line lost coverage. Both files carry `using Microsoft.Office.Interop.Outlook` and `using System.Windows.Forms` and are Outlook-Interop event-handler surfaces in `QuickFiler`, which is exemption class (c) of the maintainer-ratified `CLAUDE.md` UT2 exemption. | No change. The uncovered remainder is host-bound and has no injectable seam; the long-term answer is extraction, which is out of this branch's Write Set. | Dispositioned FAIL-but-non-blocking in `policy-audit.2026-09-06T15-31.md` section 1.2.1. Recorded here so the numbers are visible in both artifacts. | Per-file aggregation of `coverage/791-baseline.cobertura.xml` and `artifacts/csharp/coverage.xml` by this reviewer |
| Observation | test selection | `/TestCaseFilter` in every run | All runs, baseline and final, apply `TestCategory!=LiveOutlook` and exclude four `UtilitiesCS.Test` shell-icon classes (`ShellUtilities_Tests`, `ShellUtilitiesStatic_Tests`, `SysImageListHelperTests`, `OSBrowser_Tests`) that stall `vstest` on this machine. The exclusion is applied identically on both sides, so the baseline-to-final comparison is like-for-like, and none of the excluded classes is related to this change. | No change. CI runs the unfiltered suite. | Recorded because the headline "7023 tests passed" is a filtered figure, and the filter should be visible next to it. | `evidence/qa-gates/p3-t5-tests-coverage.md:8`; `evidence/baseline/p0-t11-coverage.md` |
| Observation | `docs/.../spec.md` | `:266-268` | AC5 as written ("The branch diff touches no file outside the Write Set…") is unsatisfiable over the whole tree, because delivering the fix requires writing evidence artifacts and checking the AC boxes in `spec.md` itself. The delivery narrows the evaluation to the pathspec `'*.cs' '*.csproj'` and records that narrowing explicitly in the AC's own evidence bullet rather than leaving it implicit. | No change. This is the correct handling of an over-broad criterion. | Recorded so a later reader who evaluates AC5 literally does not score it FAIL and conclude the delivery broke scope. Under the stated pathspec the criterion is fully satisfied and all five named exclusions are verifiably unmodified. | `spec.md:266-268`; `git diff --name-only` over `'*.cs' '*.csproj'` returning exactly 17 paths |
| Observation | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, `QuickFiler/Controllers/QfcHomeController.cs` | `EventHandlers.cs:171`; `QfcHomeController.cs:401` | Both files log `"… ribbon release callback invoked."` unconditionally. At `EventHandlers.cs:171` the line runs in a `finally` immediately after `RunTeardownStage("controller-cleanup", Cleanup)`, which swallows any exception, so the message is emitted even when `Cleanup` threw before reaching `_parentCleanup?.Invoke()`. At `QfcHomeController.cs:401` it runs after `ParentCleanup?.Invoke()`, so it is emitted even when `ParentCleanup` is null. | Make the claim conditional, or reword to describe the stage rather than the outcome (for example `"Cancel teardown finished."`), and log the callback invocation from the site that actually performs it. | The issue this change fixes is fundamentally a diagnosability failure — 37 minutes of silence. A log line that asserts something that may not have happened is a weaker outcome than silence, because it actively misleads the next reader of the log. | Read of `QfcFormController.EventHandlers.cs:168-172` and `QfcHomeController.cs:396-403` |

## Design and Correctness Review

### The gate change (AC1)

The zero-acceptance policy stays inside the same `deadlineEnabled && accepted.Count == 0` guard the
#424 deadline used (`QfcStreamingDequeueConfidenceGate.cs:224`). That is the right structural choice:
`Timeout.InfiniteTimeSpan` still means "no bound at all", and a non-empty prefix is still governed by
#608 fill-or-exhaust rather than by any new bound. The separate `checkpointOrigin` is necessary and
its comment says why — sharing one origin with `start` would make the first checkpoint also the last,
which is the superseded behavior.

The two bounds are evaluated before `_tryTakeNext()` (`:230`), so a bounded scan cannot consume one
extra candidate on its way out; `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached`
asserts exactly that with `takeCount.Should().Be(4)` and `source.Should().HaveCount(6)`.

The time ceiling is genuinely necessary and not redundant with the item cap: the empty-queue wait path
at `:244-257` does not increment `scanned`, so with the loader still refilling and `tryTakeNext`
returning null the cap can never be reached. The `await _timeProvider.Delay(...)` then `continue`
returns control to the loop top where the ceiling is evaluated, which is what terminates the wait.
`DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling` pins it with
`sourceActive: () => true`, `tryTakeNext` always null, and `Scanned=0`.

`MaxScanWithoutAcceptance` is a get-only auto-property rather than a `private readonly` field, with a
comment explaining that a private field assigned and never read raises CS0414, which
`/p:TreatWarningsAsErrors=true` promotes to an error. This reviewer confirmed the reasoning is correct
and the resulting gate build is warning-clean.

`ScanCapReached` is documented as requiring identical caller treatment to `DeadlineExpired`, and
`DeadlineExpired` is retained with an updated doc rather than removed, so existing switch arms and
mocks still compile. `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding` verifies
the new reason is not routed into the queue-closing branch, and its negative control
`IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce` verifies genuine exhaustion
still closes it. That pair is what makes #446 AC-6 verifiably preserved rather than merely untouched.

### The teardown change (AC2)

The implemented order in `ActionCancelAsync` matches the ten stages the spec specifies, in the
specified sequence: log entry, cancel token (before the first await), marshal to the UI context, reset
`KbdActive`, park focus and cancel selectors, unregister navigation and form handlers, hide, await the
loader quiesce, groups cleanup, and `Cleanup()` under `finally`. The two ordering constraints that
matter are separately asserted:
`ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` (handlers before rows) and
`ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup` (loader before field release).

`QuiesceLoaderAsync` is correct in the two places it is easy to get wrong. It snapshots
`_remainingLoadTask` into a local before testing it, because the field is written on the worker
thread. It passes `CancellationToken.None` to the bound delay, because the bound must survive the
token this method just cancelled — otherwise a hung loader would leave the Cancel path with no exit.
Both reasons are written in place.

`TryCreateRemainingQueueAdmission` is the correct guard placement. It refuses at the accept point
rather than throwing at the delegate-construction point, and it snapshots both fields into locals
before the null test, which is the right shape for cross-thread fields. Being a separate synchronous
method rather than the opening block of the `async` method is not stylistic: it is what keeps the
`IEmailMoveMonitor` local off a compiler-generated state machine and preserves the #731 topology pin.

`ButtonCancel_Click` no longer rethrows. This is a deliberate behavior change and it is the right one
for an `async void` handler, where a rethrow becomes an unhandled Outlook UI-thread exception carrying
nothing actionable. The replacement is stage-level ERROR logging, which is strictly more diagnosable.
`ButtonCancel_Click_ActionThrows_DoesNotRethrow` pins it by installing a capturing
`SynchronizationContext` and asserting nothing was posted — the only way to observe an `async void`
escape — and restores the previous context in a `finally`.

### Test quality

The 23 added tests use MSTest, Moq and FluentAssertions throughout, follow Arrange–Act–Assert with
explicit section comments, and carry XML-doc summaries naming the criterion and the failure each
prevents. Determinism is achieved through seams rather than timing: `FakeTimeProvider` for both
clocks, injected `Action<string>` delegates for both log sinks, `TaskCompletionSource` for the hanging
loader, `FormatterServices.GetUninitializedObject` to bypass COM-bound constructors, and a bare
`Control.ControlCollection` with an empty exclusion list to satisfy the unregister guard without
creating a window handle. The one exception is finding N4.

Assertion reasons are supplied consistently and are informative rather than restating the assertion
("toggling an inactive dialog would activate it, not reset it"; "a bounded zero-acceptance exit is
not source exhaustion"). Ordering assertions compare the first index of two markers and separately
assert each marker's presence, so they cannot pass vacuously.

## Verification Performed by This Reviewer

- Read the full diff of all seven production files and all nine test files against the merge base.
- Re-ran `dotnet tool run csharpier check .` — `Checked 1587 files in 4202ms`, exit 0.
- Re-ran both `/t:Rebuild` gate builds — exit 0; the nullable build printed `0 Warning(s) 0 Error(s)`.
- Re-ran the `QuickFiler.Test` assembly at head — `Test Run Successful. Total tests: 1362`, exit 0.
- Aggregated both Cobertura documents per package and per changed file with an independent
  `classes/class/lines/line` selection, reproducing the delivery's derived percentages exactly and
  producing per-file baseline-versus-head figures the delivery did not report.
- Scanned all 1671 added `.cs`/`.csproj` lines for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`,
  `DateTime.UtcNow`, `Random.Shared`, `GetTempPath`, `GetTempFileName`, `SuppressMessage`,
  `#pragma warning disable`, `ExcludeFromCodeCoverage`, `xunit`, `nunit` and `C:\Users\` — zero hits
  for every one of them.
- Verified that the five files `spec.md` names as non-goals are absent from `git diff --name-only`.
- Verified that no `.cs` change exists in any commit before `59536368`, so the delivery's use of
  `51b557df` as its changed-line base is equivalent to the merge base for source paths.
- Verified that `RibbonController` never calls `QfcHomeController.Cleanup()` directly, which is what
  keeps finding N1 unreachable.
- Simulated `.claude/hooks/validate-feature-review-coverage.ps1` against this review's policy audit:
  the C# coverage rows pass, and the summary-derived changed-language set is empty (finding N11).

## Recommendation

**GO for PR.** No blocking defect. The two highest-value follow-ups are N1 (null `_tokenSource` after
disposing it) and N2 (protect `_parentCleanup?.Invoke()` in `QfcFormController.Cleanup()`, which must
be a separate issue because that file is an AC5 non-goal). N3 is a two-line test addition that would
close the last unpinned clause of AC1. N5 should be promoted as a coverage-measurability refactor.
