# P3-T4 EDIT-PACKAGE TaskMaster/packages.config (issue #973)

Timestamp: 2026-10-03T11-26
Command: Edit tool on TaskMaster/packages.config (old_string `  <package id="System.Linq.Async" version="7.0.1" targetFramework="net481" />`; new_string that line, a line break and `  <package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />`); Grep `id="System.Linq.AsyncEnumerable"` -n; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- TaskMaster/packages.config
EXIT_CODE: 0
Output Summary: package entry inserted after the System.Linq.Async line (new line 44); id count 1; LINECOUNT 78 to 79 and CRCOUNT 77 to 78; numstat 1/0.

Before: LINECOUNT 78, CRCOUNT 77
After: LINECOUNT 79, CRCOUNT 78
`id="System.Linq.AsyncEnumerable"`: 1 (line 44)
NUMSTAT: 1	0	TaskMaster/packages.config
