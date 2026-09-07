# Feature Audit — Issue #798

- Timestamp: 2026-09-07T19-05
- Issue: #798
- Branch: `bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798`
- Head commit: `39433790e27585df3c4c2fab73fdd3227763e402`
- Work mode: `full-bug`
- Acceptance-criteria source: `spec.md` only

## Scope and Baseline

Baseline: `c431dc3297e864041d829e8d79b348960b8d8019`, which is `origin/main` and was re-verified
unchanged at this review boundary by the delegating orchestrator.

Scope: the full branch diff of `39433790` against `c431dc32`. Two commits, `4a29d7e7` carrying the
implementation and `39433790` carrying the evidence. The diff comprises sixteen code paths plus the
committed feature folder and the promoted potential entry. The sixteen code paths were re-derived by
enumerating every `diff --git` header in the supplied patch and match the `## Write Set` declaration
in `spec.md` element for element.

Work-mode resolution: `issue.md` line 12 carries `- Work Mode: full-bug`. Under the
acceptance-criteria-tracking skill, `spec.md` is therefore the sole authoritative acceptance-criteria
source. `user-story.md` is present but is not an AC source for this mode; the orchestrator checkpoint
records that it was authored only to satisfy a required-artifact hook. It was read and contains no
criteria that contradict `spec.md`.

Baseline condition: the test suite was green at the base commit, 7023 of 7023 passing with an empty
pre-existing failure set. The post-change suite is 7048 of 7048 passing. The arithmetic reconciles:
7023 plus the 25 tests this change adds equals 7048.

## Acceptance Criteria Inventory

`spec.md` carries 14 criteria under `## Acceptance Criteria`. AC1 through AC6 are reproduced verbatim
from `issue.md` as settled with the maintainer on 2026-09-06. AC7 through AC14 are supplementary
criteria added by the specification that constrain how AC1 through AC6 are delivered.

Checkbox state read directly from `spec.md`: AC1 through AC5 and AC7 through AC14 are checked; AC6
alone is unchecked. That is 13 checked and 1 remaining.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence | Assessment |
|---|---|---|---|
| AC1 | PASS | `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`; `AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep` | The method now throws a `TimeoutException` naming the folder and the step after the final deadline instead of completing normally. Non-overlap is delivered by the disjunction's second alternative: a single `Task.Run` outside the loop and `await work.TimeoutAfter(3000, timeProvider)` on that same instance inside it. Propagation was traced to the ribbon boundary with no intervening absorbing catch. |
| AC2 | PASS | Seven `LogDfTiming` call sites in the new partial; two AC2 tests | Three `Columns.Add` and three `Columns.Remove` operations are timed individually, and the `folder.UserDefinedProperties` enumeration is timed inside a `finally`. The existing `LogDfTiming` helper is reused, so prefix, level and context format are unchanged. Fail-before was re-derived against a working log capture after the original gate was found non-discriminating. |
| AC3 | PASS | `ValidateRequiredEmailColumns`; nine tests in `DfDeedleRequiredColumnValidationTests` | The validator is called at the caller, which AC3's "or its caller" wording permits, and throws an `InvalidOperationException` naming every missing column and the folder. Comparison is ordinal and is pinned by a case-variant negative case. |
| AC4 | PASS | `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` line 108; `GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack` | `throw e;` became `throw;`. The test walks the exception chain and requires the originating frame in at least one stack, assuming neither wrapping nor unwrapping, which is correct given that wrapping occurs upstream in the timeout helper's result marshalling. |
| AC5 | PASS | `TaskMaster/Ribbon/RibbonCommandBoundary.cs`; seven tests in `RibbonCommandBoundaryTests` | The three named handlers route through `RunAsync`, which catches every failure, forwards to both sinks and never rethrows. Containment is total: a throwing presentation sink is caught and a throwing log sink is caught. Outlook therefore cannot observe an escaped exception from these three callbacks. |
| AC6 | UNVERIFIED — PENDING MANUAL | `evidence/other/ac6-manual-verification-handoff.md` | Requires launching QuickFiler on the "T&E" folder against a live Exchange mailbox and observing either a successful launch or the AC1/AC3 dialog. This cannot be exercised from a code review or from an automated suite. Correctly left unchecked and byte-identical. This is the expected state, not a defect. |
| AC7 | PASS | `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` | The delivered implementation matches the five-step trace. The invocation-count assertion is a direct assertion of the non-overlap invariant across all three deadlines, not a proxy for it. |
| AC8 | PASS | `evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md`; analyzer build | The existing `TimeoutAfter(Task, int, TimeProvider?)` overload is re-applied and unchanged. SearchScope: the sixteen-path branch diff. SearchPattern: `UtilitiesCS/Threading/`. SearchResult: 0 changed paths, so all four overloads are untouched and no new overload is added. Zero `Task.Delay` and zero `Thread.Sleep` in the change; the analyzer step exits 0 with 0 warnings. |
| AC9 | PASS | Two `ValidateRequiredEmailColumns` call sites in `DfDeedle.cs`; `evidence/qa-gates/p7-ac9-fixed-arity.md` | The validator is called on both the asynchronous entry point at line 195 and the synchronous entry point, closing the duplicate unchecked indexing in `GetEmailDataFromTable`. `Email2dToRecords`, `Email2dArrayToDf` and `GetEmailDataFromTable` keep their parameter lists: none appears on any changed or removed line other than the Phase 1 relocation, and the fixed-arity reflection tests pass unmodified. |
| AC10 | PASS | Three `throw;` conversions in the diff | Applied at `QfcDatamodel.FrameBuilding.cs` line 108 and `QfcDatamodel.cs` lines 359 and 400. `QuickFiler/Controllers/QfcQueue.cs` and `QuickFiler/Helper Classes/cInfoMail.cs` are absent from the branch diff, so the different-type occurrence and the commented-out occurrence are both unchanged. |
| AC11 | PASS | `RibbonViewer.cs` diff; `RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail` | Exactly the three named handlers are rewritten. `UndoSort_Click` and the surrounding members appear as unchanged context in the diff, and the sibling ribbon partial is absent from the diff entirely. `RibbonCommandBoundary` carries no coverage-exemption attribute — a search of that file for `ExcludeFromCodeCoverage` returns zero matches — and is measured at 90.16 percent. The dialog renders inner exception detail through `CollectDetail`, which contributes an `AggregateException`'s inner exceptions rather than its own summary message. |
| AC12 | PASS | Absence of `QfcHomeController.cs` from the branch diff | The `catch (OperationCanceledException)` in `LaunchAsync` is neither widened nor removed, because the file is not modified. No existing catch was broadened to `System.Exception`: the two new broad catches are inside the newly created boundary type, which is the defined boundary AC5 requires, not a widening of a pre-existing handler. |
| AC13 | PASS | `evidence/qa-gates/p7-ac13-write-set-diff.md`, `p7-ac13-compile-entries.md`, `p7-ac13-line-cap.md` | The diff touches exactly the sixteen declared paths, re-derived independently by this review. Six new `.cs` files have six added `<Compile Include>` entries across the five project files. Ten of eleven `.cs` files are at or under 500 lines. The eleventh, `DfDeedle_COM_Tests.cs`, was already over the cap at 882 at base and stands at 869, strictly decreased, which is the rule AC13 sets for that one file. The pre-existing violation is recorded under Rollout and Follow-up and in `evidence/other/followup-promotions.md`. Every clause of AC13 as written is met. |
| AC14 | PASS | `evidence/qa-gates/final-toolchain-clean-pass.md`; `evidence/baseline/log4net-capture-probe.md` addendum | A full toolchain pass completes in the documented order with no errors and no restart: csharpier check exit 0 over 1593 files, analyzer msbuild exit 0 with 0 errors and 0 warnings, nullable msbuild exit 0 with 0 errors and 0 warnings, vstest exit 0 with 7048 of 7048 passing. The Phase 0 log-capture check is recorded with its outcome, including the addendum establishing that the original verdict is not reproducible, and the third strategy actually implemented is documented in the plan by correction A3. |

### Verdict counts

| Verdict | Count |
|---|---|
| PASS | 13 |
| PARTIAL | 0 |
| FAIL | 0 |
| UNVERIFIED | 1 |

## Notes on Individual Criteria

**AC1 and the non-overlap clause.** AC1 states "the underlying task is cancelled or the retry waits
for it." The first alternative is not achievable: a blocking synchronous COM call cannot be
interrupted on .NET Framework, and `Task.Run(action, token)` suppresses scheduling only. The delivered
implementation satisfies the second alternative literally — the retry awaits the same task instance
rather than starting a new one. This is not a weakening of AC1; the criterion is written as a
disjunction and the achievable branch is delivered and asserted directly.

**AC6 and the manual gate.** AC6 is the only criterion that requires the fix to be exercised against
a live host. It is correctly unchecked. Two consequences follow. First, the highest risk the
specification records — that AC1 converts an intermittent crash into a reproducible inability to open
QuickFiler on a persistently slow folder — is not retired until AC6 is performed. Second, the AC2
timing lines are what make that outcome diagnosable, so the manual verification should read the new
`[Df timing]` lines and attribute the delay to either the `UserDefinedProperties` enumeration or a
specific `Columns` call, as `evidence/other/ac6-manual-verification-handoff.md` directs.

**AC13 and the residual cap violation.** AC13 as written is satisfied, because the criterion holds the
one pre-existing over-cap file to a strictly decreasing count rather than to the absolute cap. The
underlying repository policy in `.claude/rules/general-code-change.md` is still not met by that file
at 869 lines. That residual is recorded in the policy audit and the code review as a Non-blocking
finding with a follow-up promotion owed after merge; it is not an AC failure.

## Acceptance Criteria Check-off

No check-off action was taken by this review. Every criterion evaluated PASS is already checked in
`spec.md`, and the sole unchecked criterion, AC6, did not evaluate PASS.

- Criteria checked off by this review: none.
- Criteria left unchecked: AC6, evaluated UNVERIFIED pending manual verification.
- `spec.md` was not modified by this review.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/spec.md
- Total AC items: 14
- Checked off (delivered): 13
- Remaining (unchecked): 1
- Items remaining: AC6: Launching QuickFiler on the "T&E" folder either succeeds or shows the AC1/AC3 error message (manual verification).
```

## Summary

The change delivers every acceptance criterion that can be verified without a live Outlook host.
Thirteen of fourteen criteria evaluate PASS with concrete evidence; the fourteenth, AC6, is a manual
verification that is correctly left unchecked and is not a defect.

The defect chain described in the issue is closed at three independent points. The column-add step no
longer swallows its final timeout and now fails with a message naming the folder and the step. The
row builder's unchecked dictionary indexing is guarded on both the asynchronous and the synchronous
path, so a short column set produced by any route is reported at the folder that produced it. The
ribbon callbacks no longer allow an exception to reach Outlook unhandled, and the dialog renders
inner exception detail so an `AggregateException` does not present as "One or more errors occurred."
alone. The overlapping concurrent COM calls that aggravated the original failure are removed by
holding a single task rather than restarting the work on each retry.

Blocking findings: 0. Non-blocking findings: 11, enumerated in `policy-audit.2026-09-07T19-05.md` and
detailed in `code-review.2026-09-07T19-05.md`. No remediation cycle is required and no
`remediation-inputs` artifact is produced.

Two obligations carry forward past this review: the maintainer must perform the AC6 manual
verification on the reproduction folder and on Inbox, and the three follow-up findings recorded in
`evidence/other/followup-promotions.md` must be promoted through the potential-to-issue lifecycle
after this branch merges.

**Overall verdict: PASS.**
