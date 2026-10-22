Timestamp: 2026-10-02T06-14
Command: git -C <worktree-root> diff --name-status <BASE-SHA> -- docs/features
EXIT_CODE: 0
Output Summary: Every listed path begins with docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/ except docs/features/potential/promoted/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule.md, which is the single path recorded as outside the active-folder prefix by P0-T4. All entries carry status A relative to <BASE-SHA>.

Companion Command: git -C <worktree-root> status --porcelain -- docs/features
Companion Output: Only entries beginning with docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/ (Phase 3 evidence files staged as A, the plan file, and the P3-T22 and P3-T23 artifacts pending staging); no path outside that prefix.

Hash After (issue.md): 1afdb760bb1a20bdf67362fe40c6a9b9e49a6704 (equals the hash recorded by P3-T2; issue.md is unchanged by Phase 3).
