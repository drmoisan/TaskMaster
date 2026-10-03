# P3-T2 EDIT-PACKAGE QuickFiler/packages.config (issue #973)

Timestamp: 2026-10-03T11-26
Command: Edit tool on QuickFiler/packages.config (old_string `  <package id="System.Linq.Async" version="7.0.1" targetFramework="net481" />`; new_string that line, a line break and `  <package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />`); Grep `id="System.Linq.AsyncEnumerable"` -n; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- QuickFiler/packages.config
EXIT_CODE: 0
Output Summary: package entry inserted after the System.Linq.Async line (new line 48); id count 1; LINECOUNT 82 to 83 and CRCOUNT 81 to 82; numstat 1/0.

Before: LINECOUNT 82, CRCOUNT 81
After: LINECOUNT 83, CRCOUNT 82
`id="System.Linq.AsyncEnumerable"`: 1 (line 48)
NUMSTAT: 1	0	QuickFiler/packages.config
