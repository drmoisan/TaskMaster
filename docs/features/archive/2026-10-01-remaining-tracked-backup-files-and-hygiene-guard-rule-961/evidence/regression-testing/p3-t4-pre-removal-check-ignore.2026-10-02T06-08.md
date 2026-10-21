Timestamp: 2026-10-02T06-08
Command: git -C <worktree-root> check-ignore -v -- foo.csproj.bak foo.rptproj.bak foo.bak
EXIT_CODE: 0
Output Summary: Three lines in argument order, each attributed to `.gitignore:259:*.bak`; the two specific lines (257, 258) are shadowed by the later `*.bak` rule, so the CR-3 redundancy premise holds.

Output:
.gitignore:259:*.bak	foo.csproj.bak
.gitignore:259:*.bak	foo.rptproj.bak
.gitignore:259:*.bak	foo.bak
