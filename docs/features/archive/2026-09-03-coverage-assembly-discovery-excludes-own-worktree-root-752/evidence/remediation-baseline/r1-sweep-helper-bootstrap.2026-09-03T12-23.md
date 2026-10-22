# Sweep Helper Bootstrap — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-40
- Task: `[P0-T2]`

Command:

1. `git -C <repo-root> check-ignore -q coverage/r1-host-path-sweep.ps1`
2. `git -C <repo-root> status --porcelain -uall -- coverage`
3. `git -C <repo-root> status --porcelain -uall`

EXIT_CODE:

1. `0`
2. `0`
3. `0`

Output Summary:

- The sweep helper now exists at the repo-relative path `coverage/r1-host-path-sweep.ps1`. It is a
  temporary throwaway script created and deleted within this agent session, per the first named
  exception in the File Size Limit section of `.claude/rules/general-code-change.md` (lines 47-50,
  exception at line 50). `[P2-T5]` step 9 performs the deletion.
- `check-ignore` exited `0`, so the helper is ignored by git and can never enter the branch diff or
  the staged index. `.gitignore` line 144 (`coverage/*`, with only `!coverage/.gitkeep` re-included)
  is the rule that matches it.
- `PORCELAIN_COVERAGE: <empty>` — the second command printed nothing, confirming that nothing under
  `coverage/` is visible to git even with `-uall`.
- `PORCELAIN_BASELINE:` (verbatim output of the third command, captured before any Phase 1 edit)

```
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/code-review.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/feature-audit.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/policy-audit.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md
?? docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md
```

Notes on the baseline set, for the set-difference consumers `[P1-T3]`, `[P2-T1]`, and `[P2-T5]`:

- All six entries are untracked (`??`) markdown files under `docs/features/`. The tracked tree was
  clean at the point this baseline was captured.
- Four of the six are this loop's audit artifacts (`policy-audit`, `code-review`, `feature-audit`,
  `remediation-inputs`); they are untracked, so a diff-mode enumeration cannot observe them and
  `[P2-T5]` step 4 discloses them in File mode instead.
- The fifth is this remediation's plan file, which `[P2-T2]` stages and `[P2-T5]` step 7 commits.
- The sixth is the `[P0-T1]` artifact, which was written immediately before this capture because
  `[P0-T1]` precedes `[P0-T2]` in the plan order. Its presence in the baseline is inert: it is a
  markdown file under `docs/`, so it satisfies the `[P2-T1]` classification whether it is counted as
  pre-existing or as created by this plan.
