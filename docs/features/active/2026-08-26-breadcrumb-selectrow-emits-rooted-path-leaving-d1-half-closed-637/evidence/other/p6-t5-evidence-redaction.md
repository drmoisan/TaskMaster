Timestamp: 2026-08-31T10:51:19-04:00
Command (search 1): `rg -F -n 'C:\Users' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**' --glob '!**/p6-t5-evidence-redaction.md'`
ExpectedExitCode (search 1): 1
Command (search 2): `rg -F -n '.trx' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**' --glob '!**/p6-t5-evidence-redaction.md'`
ExpectedExitCode (search 2): 1
Output Summary: Both searches returned zero matches with exit 1.

Rewritten evidence files:

- `evidence/baseline/p0-t15-wrapper.stdout.log`: 2 absolute worktree-path replacements.
- `evidence/baseline/p0-t15-wrapper.stderr.log`: 1 absolute worktree-path replacement.

A zero-match result over the two-component `C:\Users` prefix proves the account segment is absent as well, because on Windows that segment always follows the prefix immediately. Search 2 covers the vstest results filename, which carries account and machine identity without a preceding profile path and is therefore not covered by search 1.
