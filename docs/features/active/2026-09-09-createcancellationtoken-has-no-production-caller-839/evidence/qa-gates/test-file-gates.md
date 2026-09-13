# Test-file content gates (AC3, AC7, AC8) — issue #839

Timestamp: 2026-09-13T06-16
Command: git -c grep.patternType=fixed grep -c -e "Init_CreatesTokenSourceBeforeAnyLoaderObservesIt" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "capturedSource.Should().NotBeNull();" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e ".Should().Be(capturedSource.Token);" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e ".Should().BeSameAs(capturedSource);" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "CanBeCanceled.Should().BeTrue();" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "_controller.Cleanup();" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "#839" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "QfcFormViewer" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "Assert.AreEqual(" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "Thread.Sleep" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git -c grep.patternType=fixed grep -c -e "Task.Delay" -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git diff --unified=0 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
Command: git diff --numstat 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
EXIT_CODE: 0

## Output Summary

Token counts, each printed as path followed by the count. Expected value in brackets; all eleven match.

    Init_CreatesTokenSourceBeforeAnyLoaderObservesIt    1   [1]
    capturedSource.Should().NotBeNull();                1   [1]
    .Should().Be(capturedSource.Token);                 3   [3]
    .Should().BeSameAs(capturedSource);                 1   [1]
    CanBeCanceled.Should().BeTrue();                    2   [2]
    _controller.Cleanup();                              1   [1]
    #839                                                1   [1]
    QfcFormViewer                                       1   [1]
    Assert.AreEqual(                                   10   [10]
    Thread.Sleep                                  no match  [no match]
    Task.Delay                                    no match  [no match]

THREAD_SLEEP_EXIT=1
TASK_DELAY_EXIT=1

Those two searches are deliberately non-matching and their exit code 1 is the expected result, recorded here inside the summary rather than as a second `EXIT_CODE:` row, because that field is per-file and a second row would make this artifact's normalized result ambiguous. The single `EXIT_CODE: 0` above is the exit code of the `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt` count invocation. No banned wall-clock waiting API appears in the file.

`Assert.AreEqual(` at 10 is unchanged from the base tree, which is one of the two signals that the pre-existing tests were not rewritten.

Anchored diff, exactly one hunk header:

    @@ -164,0 +165,71 @@ namespace QuickFiler.Controllers.Tests

That line contains `,0 +`, so the change is a pure insertion. The diff carries zero lines beginning with `-` other than the `---` header.

Numstat line, verbatim (tab-separated): `71` tab `0` tab `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`

The deleted count is exactly `0`. This is the assertion AC8 needs: `Init_InitializesCorrectly` and every other pre-existing line in the file is untouched. The hunk POSITION is deliberately not asserted. Git compacts the insertion group downward past the pre-existing blank line at base line 164 and reports the block one line later than the edit was made, so the position slides by one while the insertion itself is exactly where the plan places it, after the closing brace of `Init_InitializesCorrectly`.

Both diff figures were measured after the clean format pass recorded by [P3-T6], and the numstat is unchanged from the pre-format measurement taken at [P1-T1], consistent with that pass reporting `FORMAT_CHANGED_OWNED_PATCH=False`.

## Doc-comment confirmation for AC7

The executor read the inserted method's doc comment. It states the ordering rule explicitly: it records that the test pins the rule that `Init()` creates the cancellation token source before the datamodel loader runs, and it states the consequence that a call placed after that loader would pass the not-null assertion but leave the datamodel and queue tokens with `CanBeCanceled` false. It also discloses the inherited debt shared with `Init_InitializesCorrectly`, that `Init()` constructs a real `QfcFormViewer` which neither test replaces. The comment therefore documents both the scenario and the expected outcome, and the `#839` reference ties it to the issue.

## Command-transport note

The eleven `git grep` invocations were run inside one `pwsh -NoProfile -Command` loop that invoked each of them in turn with the token supplied by variable, so that each invocation's exit code could be captured alongside its output. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. Each invocation retains the plan's pinned fixed-string engine, the `-c` count switch, the `-e` token operand and the single pathspec exactly as written, and the counts are the counts the plan asks for. The two `git diff` spans were addressed to the assigned worktree with a repository-location option; their refs, operands and pathspecs are exactly as the plan writes them.
