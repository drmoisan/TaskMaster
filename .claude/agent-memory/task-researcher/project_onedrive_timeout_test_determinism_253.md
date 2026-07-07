---
name: project-onedrive-timeout-test-determinism-253
description: Issue #253 root-cause verification for flaky OneDriveDownloader writer test — confirms TimeOutTask.cs T1,TResult overload catches TimeoutException instead of TaskCanceledException (unlike all sibling overloads), plus recommended DI-seam fix
metadata:
  type: project
---

Issue #253 (`docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/`):
independently verified (2026-07-07) that the Copilot-style root-cause narrative in the issue was
correct, and pinned down the exact mechanism.

- `UtilitiesCS/Threading/TimeOutTask.cs` has 8+ `RunWithTimeout` overloads. Every overload that
  wraps a `Task.Run`/awaited task under a real `CancellationTokenSource` catches
  `TaskCanceledException` — **except** the sync `Func<T1,TResult>` overload (lines 176-229),
  which catches `TimeoutException` at line 199. `TaskCanceledException` derives from
  `OperationCanceledException`, unrelated to `TimeoutException`; a real `CancellationTokenSource`
  timeout on this overload is therefore never caught by its own retry branch — it falls through
  to the generic `catch (Exception)` and returns default with zero retries. This is confirmed by
  the overload's own tests (`TimeOutTask_OverloadCoverageTests.cs:105-122`,
  `TimeOutTask_AdditionalTests.cs:147-173`), which simulate "timeout" by having the delegate
  throw `TimeoutException` directly rather than exercising the real timer — i.e. the test authors
  already worked around the mismatch instead of fixing it.
- `OneDriveDownloader.TryGetFileStreamWriter` (`UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs:82-103`)
  is the only real caller of this overload, so under thread-pool starvation (VS parallel test
  host, `[assembly: Parallelize(Workers=0, ClassLevel)]`) both the queued delegate AND the
  `CancellationTokenSource`'s own timer callback (also thread-pool-serviced) can be delayed well
  past the nominal timeout, producing multi-second (~18s observed vs 5000ms argument) flaky
  failures.
- **Why sibling test `..._WhenWriterThrows_ReturnsNull` never flakes**: both outcomes of the
  race (delegate throws, or delegate never runs due to cancellation) converge on `null` through
  the same generic catch — so its expected assertion is invariant to the race, unlike
  `..._WhenWriterReturnsMemoryStream_ReturnsStream` which requires the delegate to actually run.
- Recommended fix (documented in the research artifact): add an injectable delegate seam
  (`WriterTimeoutRunner` property on `OneDriveDownloader`, defaulting to today's exact
  `RunWithTimeout` call) rather than touching `TimeOutTask.cs`. Naively replacing
  `catch (TimeoutException)` with `catch (TaskCanceledException)` breaks two existing
  `TimeOutTask` retry tests (confirmed by reading them); an additive second catch clause would
  be a legitimate but separate production fix, filed as its own issue per the Bugfix Workflow
  minimal-fix rule.

See [[feedback-exemption-audit-check-proven-techniques]] for the general practice of grepping
sibling test/overload patterns before accepting a diagnosis.
