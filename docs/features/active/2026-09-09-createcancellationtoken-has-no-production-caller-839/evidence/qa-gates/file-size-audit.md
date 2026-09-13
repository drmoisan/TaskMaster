# Post-format file-size audit — issue #839

Timestamp: 2026-09-13T06-15
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; "QFC_LINES=$((Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs).Count)"'
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; "TEST_LINES=$((Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcHomeControllerTests.cs).Count)"'
EXIT_CODE: 0

## Output Summary

QFC_LINES=499
TEST_LINES=346

Both counts were taken after the clean format pass recorded by [P3-T6], so they are the numbers CSharpier leaves behind rather than the numbers the hand edits produced, and they are therefore load-bearing.

Against the 500-line ceiling in the General Code Change Policy:

- QuickFiler/Controllers/QfcHomeController.cs is 499 lines, one under the ceiling. The base file was exactly 500 lines, so the net effect of this diff is minus one: the inserted statement adds one line and the Decision D1 deletion of the dead comment plus its trailing blank line removes two. This is the reason the plan pairs the insertion with a dead-line deletion at all; without it the file would have been 501 lines and over the ceiling.
- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs is 346 lines, comfortably under the ceiling, up from 275 in the base tree by the 71 inserted lines.

`Set-Location -LiteralPath "REPO-ROOT";` was prepended to both spans. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one; both paths in the spans are worktree-relative and the counts are unaffected.
