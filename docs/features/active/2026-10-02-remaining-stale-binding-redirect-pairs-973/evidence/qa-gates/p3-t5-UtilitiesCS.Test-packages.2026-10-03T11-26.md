# P3-T5 EDIT-PACKAGE UtilitiesCS.Test/packages.config (issue #973)

Timestamp: 2026-10-03T11-26
Command: Edit tool on UtilitiesCS.Test/packages.config (old_string `  <package id="System.Linq.Async" version="7.0.1" targetFramework="net481" />`; new_string that line, a line break and `  <package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />`); Grep `id="System.Linq.AsyncEnumerable"` -n over glob */packages.config; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS.Test/packages.config
EXIT_CODE: 0
Output Summary: package entry inserted after the System.Linq.Async line (new line 92); LINECOUNT 109 to 110 and CRCOUNT 108 to 109; numstat 1/0; the glob Grep returns exactly the five Write Set manifests, count 1 each.

Before: LINECOUNT 109, CRCOUNT 108
After: LINECOUNT 110, CRCOUNT 109
NUMSTAT: 1	0	UtilitiesCS.Test/packages.config
Glob Grep `id="System.Linq.AsyncEnumerable"` over */packages.config:
- ToDoModel/packages.config:21
- UtilitiesCS/packages.config:98
- UtilitiesCS.Test/packages.config:92
- TaskMaster/packages.config:44
- QuickFiler/packages.config:48
(exactly five files, count 1 each)
