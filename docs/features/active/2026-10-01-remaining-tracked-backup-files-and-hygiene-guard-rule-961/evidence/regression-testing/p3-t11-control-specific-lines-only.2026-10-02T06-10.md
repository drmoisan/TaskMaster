Timestamp: 2026-10-02T06-10
Command: git -C <worktree-root> check-ignore -v -- foo.csproj.bak foo.rptproj.bak
EXIT_CODE: 0
Output Summary: Two lines in argument order, `.gitignore:258:*.csproj.bak` and `.gitignore:257:*.rptproj.bak`. The specific lines cover only their own names; together with P3-T10 they are not needed for anything `*.bak` covers.

Output:
.gitignore:258:*.csproj.bak	foo.csproj.bak
.gitignore:257:*.rptproj.bak	foo.rptproj.bak
