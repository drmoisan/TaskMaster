# narrow-fileio2-retryable-exception-set (Issue #707)

- Date captured: 2026-08-31
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/narrow-fileio2-retryable-exception-set/ (Issue #707)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #707
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/707
- Last Updated: 2026-08-31
## Summary

`FileIO2.WriteTextFileAsync` retries on every `IOException`. `DirectoryNotFoundException` derives from `IOException`, so an absent target folder consumes the full 100-attempt, 100-millisecond retry window even though no attempt in that window can succeed.

## Environment

- OS/version: Windows 11, .NET Framework 4.8.1
- Python version: not applicable
- Command/flags used: not applicable; reached through any caller of `UtilitiesCS.FileIO2.WriteTextFileAsync`
- Data source or fixture: `UtilitiesCS/To Depricate/FileIO2.cs`

## Steps to Reproduce

1. Call `FileIO2.WriteTextFileAsync` with a `folderpath` that does not exist on disk.
2. Observe that the writer factory throws `DirectoryNotFoundException` on every attempt.
3. Observe that the method spends roughly ten seconds in the retry loop before returning `false`.

## Expected Behavior

A failure that cannot be resolved by waiting should not consume the retry budget. The method should distinguish transient contention failures, for which retrying is the correct response, from structural failures such as a missing directory, and should return promptly on the latter.

## Actual Behavior

The catch clause is `catch (IOException ex)`. `DirectoryNotFoundException` is an `IOException`, so the loop performs all 100 attempts and awaits 99 delays before reporting failure.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the retry-exhaustion log line reads `after {attempts} attempts.` with `attempts` equal to 100, once per call against a missing directory.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Severity is Low because the one production caller that could reach the case guards against it: `QuickFiler/Controllers/QfcHomeController.Metrics.cs` calls `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` before writing. The stall is therefore latent rather than observed.

## Suspected Cause / Notes

Deferred from issue #647 as an explicit non-goal. Narrowing the caught set is a behavior change beyond that issue's stated Expected Behavior, so it was recorded for separate treatment rather than folded in. The relevant code is the catch clause in the `internal static` seam overload of `WriteTextFileAsync` in `UtilitiesCS/To Depricate/FileIO2.cs`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: drive the existing `writerFactory` seam with a factory that throws `DirectoryNotFoundException` and assert a writer-factory invocation count of exactly 1 and a delay-delegate invocation count of exactly 0.
- [ ] Integration scenario to retest: the `QfcHomeController` metrics flush and the `AppOlObjects` timed disk writer, both of which consume the boolean result.
- [ ] Manual verification notes: confirm that `UnauthorizedAccessException` is not an `IOException` and is therefore already outside the retry set, so no separate handling is needed for it.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
