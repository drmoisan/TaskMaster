Timestamp: 2026-10-02T06-10
Command: git -C <worktree-root> check-ignore -v -- foo.bak
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Output empty and exit 1: with `*.bak` removed and neither specific line matching `foo.bak`, no rule matches. `*.bak` is the only rule covering a plain `.bak` name and the check can fail.
