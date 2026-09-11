# minor-audit-trio-gate-log-assertion-cts-disposal-dormant-tracker (Potential Bug)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

Three minor-audit findings consolidated into one delivery, following the #823 precedent of closing several small residuals in a single item: #794 (the scan-bound log line in `QfcStreamingDequeueConfidenceGate` is not content-asserted by any test), #840 (two `CancellationTokenSource` instances are constructed and never disposed), and #841 (`ProgressTrackerAsync` has no production construction site). Their files are disjoint, each fix is confined to one or two files, and none changes a public contract.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C# / .NET Framework 4.8, MSTest + Moq + FluentAssertions)
- Command/flags used: static inspection against `main` at 3cb974422 on 2026-09-11
- Data source or fixture: none

## Steps to Reproduce

1. #794: search `QuickFiler.Test` for the literals `scan bound reached`, `Bound=`, and `Decision=stop`. Zero matches. The emitting method is `LogScanBoundReached` at `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:349`, called from line 232.
2. #840: read `UtilitiesCS/Threading/ProgressPackage.cs:25` and `:40` (`_cancelSource = cancelSource ?? new CancellationTokenSource()`) and `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs:228` (`var tokenSource = new CancellationTokenSource()`). Search for a matching `Dispose` on either instance; `ProgressPackage` implements no `IDisposable`.
3. #841: search the repository for `new ProgressTrackerAsync(`. The only hit is `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`.

## Expected Behavior

- #794: a test drives the gate to `ScanCapReached` for both the item cap and the time ceiling and asserts the emitted line carries the cutoff, the scanned and accepted counts, the bound that fired, and the `Decision=stop` token, through the injected `debugLog` delegate the sibling launch and checkpoint assertions already use.
- #840: every `CancellationTokenSource` has an owner that disposes it. `ProgressPackage` disposes the source it created (not one it was handed), and `SubjectMapSco.Orchestration` disposes its local source when the orchestration completes.
- #841: `ProgressTrackerAsync` and its test class are removed, since no shipped path constructs it. Removal is preferred to wiring a caller because the type has had no caller since it was added and the #778 null-race fix on it protected code nothing executes.

## Actual Behavior

- #794: the `ScanCapReached` tests assert the stop reason and that no further take occurs, but not the log content, so a regression in `Bound=` or `Decision=stop` passes the suite.
- #840: neither source is disposed by any holder; timer and registration resources are released only by finalization.
- #841: the type is exercised only by its own tests, inflating the coverage denominator with code no shipped path executes.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; all three are static findings.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium by the highest member (#840). #794 and #841 are Low individually.

## Suspected Cause / Notes

- Files: `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` (or a new part), `UtilitiesCS/Threading/ProgressPackage.cs`, `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`, `UtilitiesCS/Threading/ProgressTrackerAsync.cs` (88 lines, delete), `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` (193 lines, delete).
- The `ProgressPackage` disposal must distinguish an owned source from an injected one; a flag set in the constructor branch that hits `new CancellationTokenSource()` is sufficient.
- Removing `ProgressTrackerAsync` also removes its `<Compile Include>` from `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj`; verify no other Compile item is dropped (see the NuGet-update precedent in agent memory).

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: scan-bound log content for both bounds; `ProgressPackage` dispose-owned versus dispose-injected; orchestration disposes its source on completion and on exception.
- [ ] Integration scenario to retest: none.
- [x] Manual verification notes: full C# toolchain; coverage on `ProgressPackage.cs` and `SubjectMapSco.Orchestration.cs` must not regress on changed lines.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Closes #794, #840, #841 on merge.
