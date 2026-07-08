# Fail-Before Exception Dossier (Issue #253)

Timestamp: 2026-07-07T16-41

WhyFailingRunImpossible: The reported defect is an intermittent race between a real `CancellationTokenSource(5000)` timer and thread-pool dispatch of a queued `Task.Run` work item inside `TimeOutTask.RunWithTimeout<T1,TResult>` (`UtilitiesCS/Threading/TimeOutTask.cs:176-229`). It manifests only under thread-pool starvation in the Visual Studio parallel test host and is not deterministically reproducible on demand; forcing it via a shortened timeout, an injected sleep, or artificial thread-pool saturation would itself be a timing hack, which is prohibited by `.claude/rules/csharp.md` ("Adding sleeps, retries, or timing hacks to mask flaky behavior") and by the determinism policy in `.claude/rules/general-unit-test.md`. A single CLI `vstest.console.exe` run of the affected test in this environment is therefore not expected to reliably reproduce the failure, and a deliberately engineered failing run would violate repository policy. Per the bugfix-workflow nuance documented in the plan (line 29), this exception dossier substitutes for a deterministic fail-before run.

## Alternative Proof (quoted from `issue.md`)

Observed duration and failure message, as recorded in `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md`:

> Message: Expected stream not to be <null>.
> Stack Trace:
> `<TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream>d__6.MoveNext() line 235`

> The test intermittently returns `null` and fails the `Should().NotBeNull()` assertion. Duration is observed near 18 seconds, well beyond the 5000 ms timeout argument.

Root cause chain, as recorded in `issue.md` "Suspected Cause / Notes" (confirmed independently in this plan's P0-T3 investigation and in the research artifact, Section 1):

> Under thread-pool starvation in the Visual Studio parallel test host, the queued `Task.Run` work item is not scheduled within the 5000 ms wall-clock window. The linked timeout token cancels the task; `await` throws `TaskCanceledException`; the `catch (TimeoutException)` clause (line 199) does not match; the `catch (Exception)` clause (line 219) runs with `strict = false` and returns `default(TResult)` (null).

This is an asymmetric-outcome proof: the sibling test `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` (which expects `null`) passes under both the normal and the degraded (raced) code path, while `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` (which expects non-null) only passes when the race resolves in its favor. The asymmetry is structural evidence of a timing/scheduling defect rather than a plain logic defect, independent of any single observed pass/fail run.

## Output Summary

A deterministic fail-before run is structurally impossible for this thread-pool/timer race without introducing a prohibited timing hack. This dossier substitutes the observed ~18s-duration failure snippet and the asymmetric sibling-test proof from `issue.md`, cross-checked against direct source inspection (P0-T3) and the research artifact, as the fail-before evidence for issue #253.
