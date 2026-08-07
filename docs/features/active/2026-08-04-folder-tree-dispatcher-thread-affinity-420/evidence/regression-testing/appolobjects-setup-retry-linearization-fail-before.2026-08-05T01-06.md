# P5-T22 fail-before evidence

Timestamp: 2026-08-05T01:06:00-04:00 (derived from the artifact filename)
Command: `vstest.console.exe TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll /Tests:SetupFailure_RequiresFreshOwnershipForRetry`
EXIT_CODE: 1
Output Summary: Expected-red result: both setup-failure cases showed retained failed initialization ownership, leaving the retry worker incomplete.

- Command: `vstest.console.exe TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll /Tests:SetupFailure_RequiresFreshOwnershipForRetry`
- Result: failed as expected for both dispatcher-factory and dispatcher-thread-check cases.
- The worker observed the exact controlled `InvalidOperationException` after the setup override signal. The subsequent retry worker remained incomplete after the next thread-check signal, demonstrating that the failed initialization ownership was retained.
- Cleanup disposed the subject and observed both workers. No runner was terminated.
