# P3-T3 EDIT-PACKAGE ToDoModel/packages.config (issue #973)

Timestamp: 2026-10-03T11-26
Command: Edit tool on ToDoModel/packages.config (old_string `  <package id="System.Linq.Async" version="7.0.1" targetFramework="net481" />`; new_string that line, a line break and `  <package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />`); Grep `id="System.Linq.AsyncEnumerable"` -n; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- ToDoModel/packages.config
EXIT_CODE: 0
Output Summary: package entry inserted after the System.Linq.Async line (new line 21); id count 1; LINECOUNT 27 to 28 and CRCOUNT 26 to 27; numstat 1/0.

Before: LINECOUNT 27, CRCOUNT 26
After: LINECOUNT 28, CRCOUNT 27
`id="System.Linq.AsyncEnumerable"`: 1 (line 21)
NUMSTAT: 1	0	ToDoModel/packages.config
