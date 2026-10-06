# qfc-datamodel-950-review-residuals (Issue #972)

- Date captured: 2026-10-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-datamodel-950-review-residuals/ (Issue #972)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #972
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/972
- Last Updated: 2026-10-02
## Summary

The #950 review (PR #971, merged at 860d67bf4) left five non-blocking residuals in QuickFiler production and test code:
1. Three duplicated synchronous-worker test helpers should be consolidated into one shared test-support helper.
2. The `_remainingLoadActive` comment should be reworded to match the new behavior.
3. `QuickFiler/Controllers/QfcDatamodel.cs` is at 495 of 500 lines and has apparently unused legacy members. Remove or move them before its next change.
4. The `SynchronousBackgroundWorker` instances in the liveness and zero-batch tests are not disposed.
5. `transactionA` in test R4 should be wrapped in `try`/`finally`, so a failing assertion cannot leak the transaction.

The theme-test ensure-scope residual is tracked in #968.

## Environment

- OS/version: n/a
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: review of PR #971
- Data source or fixture: n/a

## Steps to Reproduce

1. Read `QfcDatamodel.cs` and the #950 test files on `main`.
2. Compare them with the items above.

## Expected Behavior

- One shared helper.
- Accurate comments.
- `QfcDatamodel.cs` well under the 500-line limit.
- Disposable test objects are disposed.
- Transactions are released on failure.

## Actual Behavior

As listed in the summary.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: PR #971 body, Follow-ups 1 to 5.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

#950 was scoped to removing the wall-clock waits and the dispatcher race. Coordinate this with #968, which touches the same test fixtures.

## Proposed Fix / Validation Ideas

- [ ] Extract the shared synchronous-worker helper, and add `using` disposal in each test.
- [ ] Add `try`/`finally` around `transactionA` in R4.
- [ ] Confirm the `QfcDatamodel` legacy members have no callers, then remove them. Keep changed-line coverage from dropping.
- [ ] Run the tests in parallel. Never use `[DoNotParallelize]` or Workers=1.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
