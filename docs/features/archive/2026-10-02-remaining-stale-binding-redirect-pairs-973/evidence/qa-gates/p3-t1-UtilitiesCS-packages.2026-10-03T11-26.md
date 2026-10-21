# P3-T1 EDIT-PACKAGE UtilitiesCS/packages.config (issue #973)

Timestamp: 2026-10-03T11-26
Command: Edit tool on UtilitiesCS/packages.config (old_string `  <package id="System.Linq.Async" version="7.0.1" targetFramework="net481" />`; new_string that line, a line break and `  <package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />`); Grep `id="System.Linq.AsyncEnumerable"` -n; CMD-LINECOUNT; CMD-CRCOUNT; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/packages.config
EXIT_CODE: 0
Output Summary: package entry inserted after the System.Linq.Async line (new line 98); id count 1; LINECOUNT 146 to 147 and CRCOUNT 145 to 146 (each +1); numstat 1/0.

Before: LINECOUNT 146, CRCOUNT 145
After: LINECOUNT 147, CRCOUNT 146
`id="System.Linq.AsyncEnumerable"`: 1 (line 98)
NUMSTAT: 1	0	UtilitiesCS/packages.config
