# Final QA step 1 of 4 — format — issue #839

Timestamp: 2026-09-13T05-58
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; $before = (git diff 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler QuickFiler.Test | Out-String); dotnet tool run csharpier format .; $fmt = $LASTEXITCODE; $after = (git diff 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler QuickFiler.Test | Out-String); "FORMAT_EXIT=$fmt"; "FORMAT_CHANGED_OWNED_PATCH=$($before -ne $after)"; $rest = @(git status --porcelain --untracked-files=all | Where-Object { $_ -notmatch "docs/features/" -and $_ -notmatch "[.]claude/agent-memory/" }); "PORCELAIN_NON_DOCS_LINES=$($rest.Count)"; $rest; exit $fmt'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; dotnet tool run csharpier check .; exit $LASTEXITCODE'
EXIT_CODE: 0

## Output Summary

Write-mode pass:

    Formatted 1624 files in 5559ms.
    FORMAT_EXIT=0
    FORMAT_CHANGED_OWNED_PATCH=False
    PORCELAIN_NON_DOCS_LINES=2
     M QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
     M QuickFiler/Controllers/QfcHomeController.cs

Read-only verification pass:

    Checked 1624 files in 4842ms.

CMD-FORMAT-CHECK EXIT_CODE: 0, and its output carries zero `Was not formatted` lines.

This is the clean pass and it is pass number 1: `FORMAT_CHANGED_OWNED_PATCH=False`, so the write-mode formatter did not alter the anchored patch over the two owned source trees and no restart of the toolchain loop was triggered. The write-mode command is therefore not being judged by its exit code alone, which would be 0 whether or not it rewrote a file: the before-and-after comparison of the anchored diff is the observation that distinguishes the two cases, and it reports no change.

Both listed porcelain lines name owned Write Set source files: QuickFiler/Controllers/QfcHomeController.cs, edited by [P2-T1] and [P2-T2], and QuickFiler.Test/Controllers/QfcHomeControllerTests.cs, edited by [P1-T1]. Neither is a formatter repair. No third path is listed, so the formatter rewrote nothing elsewhere in the tree that would drag an unowned file into this item's footprint. The D10 residue classes are filtered out of this span by the command itself, and the [P0-T8] inherited-porcelain set consists entirely of agent-memory paths, which that filter also removes.

Hand-formatting note bearing on Decision D1: the blank line that [P2-T2] deleted by hand is confirmed formatter-canonical by this pass. Had it been left in place, the two consecutive blank lines would have been collapsed here and `FORMAT_CHANGED_OWNED_PATCH` would have reported True, forcing a loop restart. It reported False, so the hand edit matched what CSharpier would have produced and the diff is deterministic.

## Command-transport adaptation

`Set-Location -LiteralPath "REPO-ROOT";` was prepended to both spans, and CMD-FORMAT-CHECK, which the plan writes as the bare invocation `dotnet tool run csharpier check .`, was run wrapped as a single `pwsh -NoProfile -Command` invocation with that prefix and a trailing `exit $LASTEXITCODE`. This is a transport change forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; the bare form would have formatted and checked the wrong tree. The command semantics are unchanged: the same manifest-pinned CSharpier 1.2.6 resolved through `dotnet tool run` is invoked over the same `.` scope with the same subcommand, and the exit code is propagated unaltered.
