# etl-deadline-mechanics-follow-ups (Issue #825)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/etl-deadline-mechanics-follow-ups/ (Issue #825)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #825
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/825
- Last Updated: 2026-09-08
- Work Mode: full-bug

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
