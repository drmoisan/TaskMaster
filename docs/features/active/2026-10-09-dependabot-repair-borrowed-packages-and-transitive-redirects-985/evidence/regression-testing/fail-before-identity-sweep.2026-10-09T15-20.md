# Fail-before: Identity Sweep (R1, B-1, issue #985)

Timestamp: 2026-10-09T15-20
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985r1-identity-sweep.ps1 -WorkspaceRoot WORKSPACE-ROOT -Base 9911fe138952e2b93476850582847c2831e1cbbd -Mode WorkingTree
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Right reason: exit 1; every synthetic control =1; git-file control 1; the only hit is ACCOUNT at `plan.2026-10-09T13-06.md:26`.
- FURTHER-HITS: NONE (no HIT- line other than the plan line 26).

Output (verbatim; the script prints token names, counts and locations only):
```
MODE: WorkingTree
TOKENS: ACCOUNT,HOST,EMAIL,ACCOUNT_SHORT
CONTROL-SYNTHETIC: ACCOUNT=1 HOST=1 EMAIL=1 ACCOUNT_SHORT=1
CONTROL-GITFILE-ACCOUNT: 1
ADDED-OR-UNTRACKED-LINES: 4890
UNTRACKED-FILE-COUNT: 16
HITS-ACCOUNT: 1
HIT-ACCOUNT: docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/plan.2026-10-09T13-06.md:26
HITS-HOST: 0
HITS-EMAIL: 0
HITS-ACCOUNT_SHORT: 0
HITS-USERS_PATH: 0
HITS-TOTAL: 1
```

Note: three git line-ending warnings (LF to CRLF on the three modified agent-memory files) were also printed on stderr; they carry no identifier and do not affect the result.
