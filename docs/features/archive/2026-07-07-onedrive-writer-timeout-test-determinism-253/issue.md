# onedrive-writer-timeout-test-determinism (Issue #253)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/onedrive-writer-timeout-test-determinism/ (Issue #253)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #253
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/253
- Last Updated: 2026-07-07
- Work Mode: minor-audit

## Summary

The unit test `OneDriveDownloader_Tests.TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` is non-deterministic. It fails intermittently in the Visual Studio test runner (observed ~18s duration, "Expected stream not to be <null>") but passes in the VS Code runner. The test drives production timeout/concurrency infrastructure through a real wall-clock timer and thread-pool scheduling, which violates the repository determinism policy for unit tests.

## Environment

- OS/version: Windows (developer workstation)
- Python version: N/A (C# / .NET Framework, MSTest)
- Command/flags used: Visual Studio Test Explorer (parallel test host) vs. VS Code test runner
- Data source or fixture: In-memory `MemoryStream` factory; no external fixture

## Steps to Reproduce

1. Run the C# test suite in the Visual Studio Test Explorer with parallel execution and other tests concurrently loading the thread pool.
2. Observe `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` (OneDriveDownloader_Tests.cs line 227).
3. The test intermittently fails with "Expected stream not to be <null>" and an elevated duration.

## Expected Behavior

`TryGetFileStreamWriter` returns the non-null stream produced by the injected writer factory, and the test passes deterministically in every runner (IDE and CLI), consistent with the repository policy that tests produce identical results in the IDE test runner and in CLI runs.

## Actual Behavior

The test intermittently returns `null` and fails the `Should().NotBeNull()` assertion. Duration is observed near 18 seconds, well beyond the 5000 ms timeout argument.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
  ```
  Message: Expected stream not to be <null>.
  Stack Trace:
  <TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream>d__6.MoveNext() line 235
  ```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Flaky test that fails a required CI/build check in the Visual Studio runner; blocks reliable green builds.

## Suspected Cause / Notes

Root cause (verified by direct code inspection):

- `OneDriveDownloader.TryGetFileStreamWriter` (OneDriveDownloader.cs lines 82-103) routes the synchronous writer factory `GetFileStreamWriter` through `TimeOutTask.RunWithTimeout<T1,TResult>`.
- That overload (TimeOutTask.cs lines 176-229) executes the delegate via `await Task.Run(() => function(arg1), combinedToken.Token)` guarded by a real `CancellationTokenSource(milliseconds)` timer (5000 ms in the test).
- Under thread-pool starvation in the Visual Studio parallel test host, the queued `Task.Run` work item is not scheduled within the 5000 ms wall-clock window. The linked timeout token cancels the task; `await` throws `TaskCanceledException`; the `catch (TimeoutException)` clause (line 199) does not match; the `catch (Exception)` clause (line 219) runs with `strict = false` and returns `default(TResult)` (null).
- The sibling test `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` expects `null` and therefore passes under both the normal and the degraded paths. This asymmetry confirms a timing/scheduling failure rather than a plain logic defect.

This is a test-design policy violation: a unit test must not depend on real wall-clock timeouts or thread-pool scheduling (`.claude/rules/general-unit-test.md` determinism infrastructure; `.claude/rules/csharp.md` deterministic test rules). Copilot's proposed production exception-handling change (catch `TaskCanceledException`) is a separate behavioral question and was shown to break other `TimeOutTask` retry tests; it does not address the determinism root cause.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: introduce the smallest deterministic seam (per the repo DI-seam preference order) so the wrapper contract of `TryGetFileStreamWriter` (returns the writer's stream on success, `null` on writer failure) can be verified without exercising a real timer or the thread pool. Production default behavior is preserved.
- [x] Integration scenario to retest: full `OneDriveDownloader_Tests` class in both the Visual Studio and VS Code runners.
- [x] Manual verification notes: confirm the test is fast (no multi-second duration) and passes repeatedly under both runners.

## Acceptance Criteria

- [x] AC1: `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` no longer depends on a real wall-clock timeout or thread-pool scheduling for its outcome, and passes deterministically.
- [x] AC2: The fix preserves production behavior of `OneDriveDownloader.TryGetFileStreamWriter` (default path still applies the real timeout runner); any seam introduced defaults to current behavior.
- [x] AC3: The wrapper contract remains covered: writer-returns-stream yields a non-null stream, and writer-throws yields `null`, both verified deterministically.
- [x] AC4: The full `OneDriveDownloader_Tests` class passes in both the Visual Studio and VS Code runners with no multi-second duration for the affected test.
- [x] AC5: The full C# toolchain passes in order (csharpier -> analyzers -> nullable/type-check -> MSTest) with no regressions, and repository coverage does not regress on changed lines.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
