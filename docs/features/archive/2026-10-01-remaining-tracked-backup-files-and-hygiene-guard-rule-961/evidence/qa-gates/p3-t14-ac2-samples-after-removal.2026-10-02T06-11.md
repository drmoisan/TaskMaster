Timestamp: 2026-10-02T06-11
Command: git -C <worktree-root> check-ignore -v -- foo.csproj.bak foo.rptproj.bak foo.bak
EXIT_CODE: 0
Output Summary: Three lines in argument order, each attributed to `.gitignore:257:*.bak`. P3-T4 printed `.gitignore:259:*.bak` for the same command and P3-T10 printed nothing with `*.bak` absent; the line change confirms the removal and that `*.bak` is the covering rule.

Output:
.gitignore:257:*.bak	foo.csproj.bak
.gitignore:257:*.bak	foo.rptproj.bak
.gitignore:257:*.bak	foo.bak
