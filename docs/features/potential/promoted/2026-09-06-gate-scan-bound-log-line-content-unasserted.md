# gate-scan-bound-log-line-content-unasserted (Issue #794)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/gate-scan-bound-log-line-content-unasserted/ (Issue #794)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #794
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/794
- Last Updated: 2026-09-06
## Summary

The #791 fix added three log lines to `QfcStreamingDequeueConfidenceGate` (launch, zero-acceptance checkpoint, scan bound reached). The launch and checkpoint lines are content-asserted by tests; the scan-bound line emitted by `LogScanBoundReached` is not asserted by any test, so a regression in its content (the `Bound=` value or the `Decision=stop` token) would pass the suite. AC1 of #791 requires the bound decision to be logged.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: static review of branch bug/quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791 at 59536368
- Data source or fixture: none (code review finding N3 in code-review.2026-09-06T15-31.md)

## Steps to Reproduce

1. Search QuickFiler.Test for the literals `scan bound reached`, `Bound=`, and `Decision=stop`: zero matches.
2. Compare with the launch and checkpoint lines, which `QfcStreamingDequeueConfidenceGateTests.Part4.cs` asserts through the injected `debugLog` delegate.

## Expected Behavior

A test drives the gate to `ScanCapReached` (item cap and time ceiling) and asserts the emitted line carries the cutoff, scanned and accepted counts, the bound that fired, and the stop decision.

## Actual Behavior

The `ScanCapReached` tests assert the stop reason and that no further take occurs, but not the log line content.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; static finding.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: a diagnostics-only gap; the behavior itself is tested.

## Suspected Cause / Notes

- Test-coverage gap left by #791; a two-line addition to the existing cap and ceiling tests in `QfcStreamingDequeueConfidenceGateTests.Part4.cs` closes it (the file has headroom under the 500-line ceiling; `.Part2.cs` does not).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `DequeueAsync_ZeroAcceptedAndCapReached_LogsBoundDecision`, `DequeueAsync_ZeroAcceptedAndCeilingReached_LogsBoundDecision` asserting via the `debugLog` delegate.
- [ ] Integration scenario to retest: none.
- [ ] Manual verification notes: none.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
