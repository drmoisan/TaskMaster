```markdown
Suggested title: Restore QuickFiler backfill after non-empty deadline expiry

## Summary

- Restores fill-or-exhaust dequeue behavior after a non-empty accepted prefix passes the first-batch deadline.
- Retains the deadline-based empty-result behavior.
- Adds coverage for initial and subsequent screens with qualifying items after the deadline.

## Why

A non-empty accepted prefix could return an undersized result after the first-batch deadline, even when additional qualifying queue items remained.

## What Changed

- Core behavior
  - Apply first-batch deadline expiry only while no items have been accepted.
  - Continue scanning after a non-empty accepted prefix until the requested quantity is met or the source is exhausted.
- Tests
  - Update the deadline-expiry test to verify continued scanning through source exhaustion.
  - Add initial- and subsequent-screen scenarios that verify seven and eight qualifying items are returned in queue order.

## Architecture / How It Fits Together

The dequeue confidence gate retains the deadline as an empty-result safeguard while preventing it from authorizing a non-empty, undersized batch.

## Verification

### Completed

- Not verified in this PR (no tool outputs recorded in the PR-context summary).

### Recommended

- `dotnet tool run csharpier .`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Run the QuickFiler test assembly with `vstest.console.exe` and code coverage enabled.

## Backward Compatibility / Migration Notes

No migration is required. Empty first-batch deadline behavior remains unchanged.

## Risks and Mitigations

- Continued scanning can take longer after an item has been accepted; the empty-result deadline remains in place, and focused tests cover the changed control flow.

## Review Guide

- Review the deadline guard in `QfcStreamingDequeueConfidenceGate`.
- Review the updated source-exhaustion test and the new seven- and eight-item backfill scenarios.

## Follow-ups

- None identified in the canonical PR context.

## GitHub Auto-close

- None (no verified closing issues and readiness not PASS)

## Related issues / PRs

- None (no referenced issues or PRs were classified in the canonical context.)
```
