# P4-T12 Hygiene Sweep (AC19)

Timestamp: 2026-09-29T09-45
Command: CMD-SWEEP over docs\features\active\2026-09-28-tests-depend-on-uncontrolled-environment-931 (tokens derived at run time and never written), then git diff --name-only MERGE-BASE with git status --porcelain --untracked-files=all for the raw-extension check (MERGE-BASE = 177b6d78e1b2408e5aedbd794cef3aad6b7fb372)
EXIT_CODE: 0

Output Summary:
- FILES: 43
- ACCOUNT-TOKEN-MATCHES: 0
- PROFILE-LEAF-MATCHES: 0
- MACHINE-TOKEN-MATCHES: 0
- WORKTREE-ROOT-MATCHES: 0
- USERS-PATH-MATCHES: 0
- RAW-DOCUMENT-FILES: 0
- RAW-EXTENSION-PATHS: NONE (no path in the anchored diff or the porcelain span ends .trx, .xml or .coverage)
- The sweep ran once; no repair was required.

Acceptance: the six match counts are 0; RAW-DOCUMENT-FILES 0; RAW-EXTENSION-PATHS NONE. All three hold.
