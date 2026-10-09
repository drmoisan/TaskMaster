# Pass-after: Identity Sweep (R1, B-1, issue #985)

Timestamp: 2026-10-09T15-20
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985r1-identity-sweep.ps1 -WorkspaceRoot WORKSPACE-ROOT -Base 9911fe138952e2b93476850582847c2831e1cbbd -Mode WorkingTree
EXIT_CODE: 0
Output Summary:
- HITS-TOTAL: 0; every HITS- line 0; CONTROL-SYNTHETIC all =1; CONTROL-GITFILE-ACCOUNT: 1.
- P1-T2 (plan.2026-10-09T13-06.md line 26): Grep -o `<encoded-worktree>.<session-id>.scratchpad` = 2 matches, both line 26; Grep -o `claude.C--Users-` = 0; line count 643 before (HEAD) and 643 after; `git diff --numstat HEAD` = 1 added, 1 removed.
- P1-T3: FURTHER-HITS: NONE (P1-T1 listed no hit other than plan line 26); no file rewritten beyond line 26; no XML-family file touched.

Sweep output (verbatim):
```
MODE: WorkingTree
TOKENS: ACCOUNT,HOST,EMAIL,ACCOUNT_SHORT
CONTROL-SYNTHETIC: ACCOUNT=1 HOST=1 EMAIL=1 ACCOUNT_SHORT=1
CONTROL-GITFILE-ACCOUNT: 1
ADDED-OR-UNTRACKED-LINES: 4918
UNTRACKED-FILE-COUNT: 17
HITS-ACCOUNT: 0
HITS-HOST: 0
HITS-EMAIL: 0
HITS-ACCOUNT_SHORT: 0
HITS-USERS_PATH: 0
HITS-TOTAL: 0
```
