# P1-T5 — The Test This Change Deletes

Timestamp: 2026-08-31T19-17
Command: count the single-line tokens `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` and `FileShare.None` in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`
EXIT_CODE: 0

## Counts

- `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` — 1 occurrence, on line 30. Required: exactly 1. Matches.
- `FileShare.None` — 1 occurrence, on line 35. Required: exactly 1. Matches.

Recorded alongside, because P5-T7 and P7-T16 both assert it must reach zero: `new FileStream(` occurs 1 time, also on line 35, the same statement that carries the `FileShare.None` argument. The pre-change file therefore has exactly one exclusive-lock file stream and it belongs solely to the test being deleted, so a post-change count of zero for all three tokens is achievable by deleting that one test and nothing else.

The test occupies lines 29 through 47: the `[TestMethod]` attribute on line 29, the declaration on line 30, the fixture resolution on lines 32 and 33, the `using (new FileStream(...))` on line 35, the `Func<Task> act` wrapping the public overload call on lines 37 through 43, and the sole assertion `await act.Should().NotThrowAsync();` on line 45. Its only assertion is that the call does not throw, which is equally true after the fix, so the test cannot detect the defect in either direction. That is the reason it is replaced rather than renamed.

Output Summary: Both preconditions hold at exactly 1. The remaining fixture-reading tests in the same class — `CsvReaders_WithFixtureAndMissingFiles_ShouldRespectHeaderOptions`, `CsvReadTo2D_AndCsvReadToJagged_ShouldProjectFixtureRows` — do not use `FileShare.None` or `new FileStream(` and are left unchanged by P5-T7.
