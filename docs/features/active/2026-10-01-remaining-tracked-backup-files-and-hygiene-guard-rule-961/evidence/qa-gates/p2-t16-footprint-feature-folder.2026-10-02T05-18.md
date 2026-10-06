Timestamp: 2026-10-02T05-18
Command: git -C <worktree-root> diff --name-status <BASE-SHA> -- docs/features
EXIT_CODE: 0
Output Summary: Every listed path has status A. All begin with docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961/ (issue.md, the plan, and the evidence files of P0-T1 through P2-T12) except docs/features/potential/promoted/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule.md (status A), which is exactly the single path recorded in P0-T4. No other path under docs/ is listed.
Companion Command: git -C <worktree-root> status --porcelain -- docs/features
Companion Output: Entries are the two evidence files p2-t13-stage-footprint.2026-10-02T05-17.md and p2-t14-base-continuity.2026-10-02T05-17.md as untracked, and the P2-T1 to P2-T12 evidence files as staged additions; every entry begins with the active-folder prefix.
