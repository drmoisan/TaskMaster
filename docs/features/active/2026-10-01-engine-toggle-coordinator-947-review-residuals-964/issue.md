# engine-toggle-coordinator-947-review-residuals (Issue #964)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Active -> docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/ (Issue #964)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #964
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/964
- Last Updated: 2026-10-01
- Work Mode: minor-audit

## Summary

The #947 review (PR #963) left three residuals in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`:
1. On the refusal path of `HandleToggleClickAsync`, the "engines unavailable" notification call is still unguarded. A throwing notification sink can escape into the Office ribbon callback, so the method's "never throws" comment overstates that path. This is the same root cause as #947.
2. The `GetPrimeTask` doc comment opens with "The prime task", but the method returns the registration marker.
3. The file is 476 of 500 lines, so it needs splitting before its next change.

Re-measured on origin/main at 942873699 (2026-10-02): the file is 496 of 500 lines after #948, and the refusal-path call sits at line 186. All three residuals are still outstanding.

## Acceptance Criteria

Each criterion below is falsifiable by the named test or measurement. Related defects in the same file are folded in under the 2026-10-02 related-defect remediation directive (criteria AC6 and AC7).

- [x] AC1 (refusal-path notification guard, regression-first): with the engines accessor returning null and a `notifyUnavailable` sink that throws, `EngineToggleStateCoordinator.HandleToggleClickAsync` completes without throwing. A new MSTest regression test proves this; it is recorded failing against the unmodified coordinator (fail-before evidence under `evidence/regression-testing/`) and passing after the fix.
- [x] AC2 (notification failure is reported, not lost): in the AC1 scenario the notification sink is attempted exactly once, the sink's exception is delivered exactly once to `logError` (the same exception instance), no engine member is invoked, and no control is invalidated. When `logError` also throws in that scenario, `HandleToggleClickAsync` still completes without throwing. Both behaviours are asserted by named MSTest tests.
- [x] AC3 (one shared sink guard): all three sink call sites (refusal-path `notifyUnavailable`, click-boundary `logError`, prime-fault `logError` in `CompletePrime`) route through a single private guard helper; no other `catch` clause that discards a sink exception remains in the coordinator's source files. Verified by reading the split source and by the existing #947 tests in `EngineToggleStateCoordinatorTests.ThrowingSink.cs` passing unchanged.
- [x] AC4 (prime-marker ordering invariants preserved across the split): the #942/#944/#947/#948 invariants still hold: the marker is registered before the prime starts; a prime fault is reported before the marker is cleared; a sink that throws leaves the fault kind unrecorded (report still owed); a repeated fault kind for the same engine is reported once. Verified by every existing test in the `EngineToggleStateCoordinatorTests` partials (`.cs`, `.Race.cs`, `.PrimeFaultOrdering.cs`, `.PrimeRegistration.cs`, `.ThrowingSink.cs`, `.RepeatFaultSuppression.cs`) passing with no assertion weakened or removed.
- [x] AC5 (`GetPrimeTask` documentation accuracy): the `GetPrimeTask` `<summary>` and `<returns>` describe the registration marker (completed only after the prime outcome has been observed and reported), not "the prime task" or "the in-flight prime". Verified by reading the doc comment.
- [x] AC6 (file-size split): `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is split into cohesive partial-class files of the same `internal sealed partial class EngineToggleStateCoordinator`, each file at or below 450 lines, each new file registered as a `Compile` item in `TaskMaster/TaskMaster.csproj`; every touched test file stays at or below 500 lines. Verified by a line count of every coordinator source file and test file and by the build compiling the new files.
- [x] AC7 (comment drift in touched files): every comment that counts or locates the coordinator's `catch` clauses (the `HandleToggleClickAsync` summary and remarks, the `StartObservedPrime` remarks, the `CompletePrime` remarks) and the `HandleToggleClickAsync` "never throws" remark match the post-change code, and the constructor's `notifyUnavailable` and `enginesAccessor` parameter docs state the guarded behaviour and the accessor's non-throwing precondition. Verified by reading each named comment against the code.
- [x] AC8 (toolchain and coverage): the CLAUDE.md C# toolchain passes in one clean pass (csharpier check, analyzer `/t:Rebuild`, `TreatWarningsAsErrors` `/t:Rebuild` without `/p:Nullable=enable`, `Invoke-MSTestWithCoverage.ps1`), with no new failing test relative to baseline and the coordinator's line coverage not lower than its baseline figure.

## Environment

- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: review of PR #963
- Data source or fixture: n/a

## Steps to Reproduce

1. Construct the coordinator with engines unavailable and a notification delegate that throws.
2. Invoke the toggle click.

## Expected Behavior

- The click handler never throws into the ribbon callback on any path.
- Doc comments match behavior.
- The file has room under the 500-line limit.

## Actual Behavior

- The notification exception escapes on the refusal path.
- The `GetPrimeTask` doc is inaccurate.
- The file is 476 lines.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: PR #963 body, Follow-ups 1 to 3; #947 review artifacts.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

#947 guarded the two `_logError` call sites only. The duplicated guard could become one helper that is applied to both log sinks and notification sinks.

## Proposed Fix / Validation Ideas

- [ ] Write a regression test first: a throwing notification on the refusal path must not escape. Then add a shared "invoke sink safely" helper and use it at all three sites.
- [ ] Correct the `GetPrimeTask` doc comment.
- [ ] Split the file into partials before or with the change, keeping each partial under 500 lines.
- [ ] Sequence this with #948, which edits the same file.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
