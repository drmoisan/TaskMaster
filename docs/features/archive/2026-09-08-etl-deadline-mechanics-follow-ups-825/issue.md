# etl-deadline-mechanics-follow-ups (Issue #825)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/etl-deadline-mechanics-follow-ups/ (Issue #825)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #825
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/825
- Last Updated: 2026-09-09
- Work Mode: full-bug
- Delivery Status: Delivered 2026-09-09 on branch bug/etl-deadline-mechanics-follow-ups-825-exec. All 35 acceptance criteria in spec.md are checked off; see the Delivery Record section at the end of this document.

## Summary

Residual ETL deadline mechanics left in place by issue #811, which placed the 250 ms per-row ETL
deadline and the 1000 ms dataframe-transform deadline under an injectable `TimeProvider` and added
a descriptive guard for the null-snapshot path. Each item below was deliberately excluded from
#811 to keep that fix's blast radius to a bugfix, and each is recorded here rather than left
implicit. None is a regression introduced by #811.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: mocked `Table`, `MAPIFolder` and `Explorer` objects; no live Outlook

## Steps to Reproduce

Read the cited lines. These are design residuals rather than reproducible failures; item 4 below
is the only one with an observable symptom, and it requires a real Outlook store slow enough to
exceed the deadline.

## Expected Behavior

1. The per-row ETL budget is justified by measurement rather than by a constant chosen without one.
2. Every wall-clock deadline on the `GetEmailDataInViewAsync` path is under test control.
3. `EtlAsync` communicates a deadline expiry through its return type rather than through a null
   forced into a non-nullable tuple element.
4. Dead code and stale documentation do not outlive the thing they describe.
5. `[DoNotParallelize]` is present only where a documented, verified reason requires it.

## Actual Behavior

1. **The 250 ms-per-row ETL budget** at `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`
   (`int milliseconds = 250 * rowCount`) is small for real stores as well as for tests: a one-row
   folder gets 250 ms for two thread-pool hops plus COM enumeration. #811 preserved the value
   byte-for-byte and only made the clock injectable. Changing it is a timing change with no
   deterministic test, which is why it was excluded.

2. **The residual 2000 ms `GetTableInViewAsync` window** remains on the system clock. It is reached
   from `DfDeedle.GetEmailDataInViewAsync` and is seamed by a
   `Func<int, CancellationTokenSource>` factory rather than by a `TimeProvider`, so threading it
   would introduce a second seam type into the same call path. It has eight times the margin of
   the 250 ms window and did not trip in the recorded failure, but it is the next-smallest
   deadline on that path. Two options for the fix: pass a factory returning a never-cancelling
   source from the test, or unify on `TimeProvider` through
   `TimeProviderTaskExtensions.CreateCancellationTokenSource` (its availability in
   `Microsoft.Bcl.TimeProvider 10.0.11` has not been verified).

3. **`EtlAsync`'s null-through-suppression tuple contract.** On deadline expiry `EtlAsync` swallows
   the `TimeoutException`, calls `tokenSource.Cancel()`, and returns `(data!, columnDictionary)`
   with `data` null, forcing a null through a null-forgiving suppression into a non-nullable tuple
   element. #811 added a guard at the consumer instead of changing the contract, because changing
   it to `object[,]? data` with a rethrown `TimeoutException` widens the diff into every `EtlAsync`
   consumer. The surviving contract is now documented by a test,
   `OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`,
   so a future change to it will fail that test rather than pass silently.

4. **The inert `(int, int)` `TimeoutAfter` overloads** at `UtilitiesCS/Threading/TimeOutTask.cs`
   lines 824 and 924 wrap a call to the proxy-returning `(int, TimeProvider?)` overload in a
   `catch (TimeoutException)` that can never execute, because the inner overload returns a proxy
   that a timer later faults rather than throwing. `repeatAttempts` is never consulted and the
   "attempts remaining" log line is unreachable. #811 removed the last production callers from
   this path but did not delete the overloads: the file is 1011 lines, already over the 500-line
   cap, and the overloads retain two test callers at
   `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs:197,210` plus the dead `EtlAsyncOld`. This is
   also recorded as Finding 1 of issue #798's `evidence/other/followup-promotions.md`.

5. **A stale doc comment.** `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` line 96 names
   `TableEtlInvoker`, a static property #811 deleted and replaced with an optional `etl` parameter.
   The comment now refers to a member that does not exist. #811 left the file untouched so that
   its write set stayed at the 20 declared paths.

6. **`[DoNotParallelize]` retained on `OlTableExtensions_Tests` without a soak.** #811 removed the
   attribute from `StackGeek_Tests`, `PrettyPrint_Tests` and `DASLFilterParserTests` once the
   `TextWriter` seam removed their console dependency, but retained it on
   `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` under spec Mitigation 4. The
   class is 1846 lines of COM-mock tests, ten of which drive the 2000 ms `GetTableInViewAsync`
   window, and it has never been soaked under class-level parallelism. Its comment was rewritten so
   it no longer claims the console as the reason. Removing the attribute after a soak is the
   follow-up.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: none of these blocks a check today. Items 1 and 2 are latent robustness risks on slow stores;
items 3 to 6 are maintainability and clarity debt.

## Suspected Cause / Notes

Items 1 to 3 are consequences of a deadline design that predates the `TimeProvider` seam. Items 4
and 5 are residue from that seam's incremental adoption across #798 and #811. Item 6 is a
deliberate conservative choice pending evidence.

## Proposed Fix / Validation Ideas

- Measure real ETL durations against folder size before changing the 250 ms constant, and record
  the measurement in the issue.
- Choose one seam type for the `GetTableInViewAsync` window and apply it end to end.
- Change the `EtlAsync` tuple to `object[,]? data` and rethrow, updating every consumer in one
  change; the existing contract test will need updating in the same commit.
- Delete the two inert `TimeoutAfter` overloads together with `EtlAsyncOld` and its test, which
  also reduces `TimeOutTask.cs` toward the 500-line cap.
- Correct the `DfDeedle.QfcColumns.cs:96` doc comment.
- Soak `OlTableExtensions_Tests` without `[DoNotParallelize]` over at least ten full-suite runs
  before removing the attribute.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Delivery Record

Delivered 2026-09-09. All six items are implemented and all 35 acceptance criteria in spec.md are
checked off; the summary is mirrored at evidence/issue-updates/ac-status-summary.md.

### What was delivered

Item 1 records the 250 ms per-row budget rationale in a comment at the expression, without changing
the value. Item 2 threads a trailing optional `TimeProvider` from `DfDeedle.GetEmailDataInViewAsync`
into `GetTableInViewAsync`, resolves the deadline source once so an explicitly supplied factory
still wins, and makes the `TimeoutException` retry propagate the caller's `timeoutMs` instead of a
literal 2000. Item 3 widens `EtlAsync`'s first tuple element to `object[,]?` and deletes the
null-forgiving suppression. Item 4 deletes the two inert `(int, int)` `TimeoutAfter` overloads,
`EtlAsyncOld`, and their three tests. Item 5 corrects the stale `TableEtlInvoker` doc comment to
name `DefaultTableEtl`. Item 6 removes `[DoNotParallelize]` from `OlTableExtensions_Tests` with a
corrected class comment, and documents the verified reason for retaining it on `TimeOutTask_Tests`.

Four regression tests were added in the new file
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs, with one compile item added
to the test project.

### The three acceptance-criteria amendments

AC6, AC20 and AC35 each carry an `Amended 2026-09-09` marker in spec.md. **All three amendments were
made during preparation by the orchestrator, before the plan was handed to an executor, and not by
the executor.** Authorship of acceptance criteria belongs with planning and scoping agents; an
executor free to rewrite the criterion it is judged against is not gated by that criterion. The
executor's Phase 3 opened with a read-only verification that the working tree carried the amended
spec, recorded at evidence/qa-gates/ac6-ac20-amended-spec-verification.md, and **no acceptance
criterion was amended during execution.**

The measured reason for the AC6 and AC20 amendments is the same in both cases. Threading the
provider into `GetTableInViewAsync` inserts the table-acquisition timer as the first arming signal
on the latch-based `ArmingBarrierTimeProvider` that the test at DfDeedleEtlTimeoutTests.cs line 135
consumes in a fixed order. The barrier's `Armed` signal is a latch, so it drops a signal whenever
two timers arm inside one await window; the test's `barrier.Advance(250)` would then run before the
250 ms ETL timer existed, firing nothing, and because its assertion sits inside the `try` the
`finally` that releases the gates would never run. The failure mode would be a hang rather than a
clean failure. The original wording required DfDeedleEtlTimeoutTests.cs to be absent from the diff
while still passing, and those obligations could not both hold. The amendment admits a bounded
timer-ordering update to that one file, adding no assertion and removing none, which the delivered
change respects: the anchored diff for it adds and removes zero `.Should()` lines.
DfDeedle_COM_Tests.cs remains absent from the diff entirely.

The measured reason for the AC35 amendment is a contradiction with AC20. The original wording
required the reachability observation to be filed through the promotion lifecycle, which writes a
record under docs/features/potential/promoted/ and would therefore put a file under docs/features/**
that is not one of this feature's own documents, falsifying AC20 on the same branch. The amendment
substitutes an evidence artifact plus a deferred epic handoff for the on-branch promotion. That
artifact is evidence/other/ac35-reachability-observation.md and it records
`PromotionWrittenOnThisBranch: false`.

### Deferred follow-ups, to be filed by the epic after this feature merges

1. Capture real ETL durations against folder size from a live Outlook session and revisit the 250 ms
   per-row budget with that measurement in hand.
2. Reduce TimeOutTask.cs below the 500-line cap. This feature took it from 1011 lines to 966, which
   is a reduction and not a resolution.
3. The reachability observation concerning the two `catch` blocks in `GetTableInViewAsync` under
   `strict: false`, and the adjacent null-return defect it exposes.
4. Convert the `TimeOutTask_Tests` wall-clock races to an injected clock, which would then allow that
   class's `[DoNotParallelize]` to be removed rather than documented.

### One idea in the original capture that was deliberately not followed

The Proposed Fix section above suggested repeated full-suite runs as the basis for removing
`[DoNotParallelize]`. That approach was not used and is not the justification for the removal.
Repeated runs sample one machine and one suite composition and establish nothing durable, and
repository policy separately forbids stabilising a test with a timing tolerance. The justification is
the item 2 code change, which removes the wall-clock deadline the attribute guarded against; it is
recorded at evidence/other/ac21-justification.md with `RunsObserved: 0`.
