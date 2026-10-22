# Host-Identifier and Raw-Document Sweep (P2-T11)

Timestamp: 2026-09-30T11-29
Task: P2-T11
Command: CMD-SWEEP over docs\features\active\2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940 (this plan included); then git diff --name-only origin/main...HEAD with git status --porcelain --untracked-files=all (issued inside one pwsh payload that filtered the listed paths for the extensions .trx, .xml and .coverage)
EXIT_CODE: 0 (scoped to the CMD-SWEEP payload)
Output Summary: no account token, profile leaf, machine token, worktree root or user-profile path appears in any of the 48 files of the feature folder; no raw document is present; no committed or pending path of this branch ends in a raw-document extension. The tokens themselves are derived at run time and are not written here.
- FILES: 48
- ACCOUNT-TOKEN-MATCHES: 0
- PROFILE-LEAF-MATCHES: 0
- MACHINE-TOKEN-MATCHES: 0
- WORKTREE-ROOT-MATCHES: 0
- USERS-PATH-MATCHES: 0
- HYGIENE-PROFILE-PATTERN-MATCHES: 0
- RAW-DOCUMENT-FILES: 0
- DIFF-EXIT: 0 (46 paths listed; the three-dot range is anchored at the merged origin/main tip 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817, the P2-T7 `ANCHOR-SHA-2:` value)
- STATUS-EXIT: 0 (7 paths listed: two modified and five untracked feature-folder evidence or plan paths)
- RAW-EXTENSION-PATHS: NONE
