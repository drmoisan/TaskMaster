---
name: cleanup-merged-worktrees
description: 'Detect, consolidate, and delete git worktrees/branches that are fully merged into main; use after an epic or feature''s PRs have merged and stale drm-copilot-wt-* branches/worktrees remain, driving the detect -> report -> consolidate -> pr-author handoff -> post-merge deletion workflow.'
allowed-tools:
  - Read
  - "Bash(bash scripts/bash/cleanup-worktrees.sh *)"
  - "Bash(git fetch *)"
  - "Bash(git merge-base *)"
  - "Bash(git push *)"
  - "Bash(git rev-parse *)"
---

# Cleanup Merged Worktrees

Drive the end-to-end cleanup of stale git worktrees and branches after their work has
merged into `main`. The deterministic classification, consolidation staging, and
deletion mechanics live in `scripts/bash/cleanup-worktrees.sh` (wrapping
`scripts/bash/cleanup_worktrees_lib.sh` and
`scripts/bash/cleanup_worktrees_actions_lib.sh`). This skill owns the editorial and
orchestration layer: deciding whether flagged unique content is genuinely
documentation/memory material, driving consolidation onto a single
`documentationandmemories` branch, delegating PR creation to `Agent(pr-author)`, and
running the destructive apply pass only after the consolidation PR has merged.

The script is deterministic and owns the safe/unsafe decision; the LLM/editorial
judgment (which flagged commits are documentation/memory content) is this skill's job
and is out of the script's scope.

## When to Use This Skill

- After an epic or feature's PRs have all merged and two to five stale
  `drm-copilot-wt-*` branches or worktrees remain.
- When you need a trustworthy, machine-parseable report of which branches/worktrees are
  safe to delete (`MERGED_CLEAN`, `MERGED_CONTENT_NEUTRAL`, `MERGED_EQUIVALENT`) versus
  which carry unmerged or unique work (`NOT_MERGED`, `HAS_UNIQUE_RESIDUALS`).
- When stranded documentation/agent-memory commits were appended to a worktree branch
  after its feature content already merged and must be preserved before deletion.
- Do not use this skill to manage remote branches; its scope is local branches and
  local worktree registrations only.

## Report Line Contract

The script emits pipe-delimited, `LC_ALL=C`-ordered records, one per line:

- `BRANCH|<name>|<state>` — `state` in `NOT_MERGED | MERGED_CLEAN |
  MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT | HAS_UNIQUE_RESIDUALS | PROTECTED_CURRENT`.
- `COMMIT|<branch>|<sha>|<state>|<paths-csv>|<author>|<author-date>` — per-commit state
  in `EQUIVALENT | CONTENT_ON_MAIN | EMPTY | UNIQUE | CONFLICT`. A `UNIQUE` COMMIT record
  is a cherry-pick candidate for editorial triage.
- `WORKTREE|<path>|<branch-or-DETACHED>|<flags>` — worktree registrations.
- `WARN|main-divergence|<local-sha>|<origin-sha>` — local `main` differs from
  `origin/main` (advisory; classification still runs).
- `DIRTY|<worktree-path>|<status-porcelain-line>` — a dirty worktree that blocked
  removal.
- `ACTION|<verb>|<target>|<result>` — apply-mode action results.

## End-to-End Workflow

1. **Detect and report (dry run).** Run `bash scripts/bash/cleanup-worktrees.sh`
   (report mode is the default and mutates nothing). It verifies local `main` against
   `origin/main` (emitting `WARN|main-divergence` on drift), enumerates branches and
   worktrees, and prints one `BRANCH|` line per branch plus `COMMIT|...|UNIQUE|...`
   records for each unique residual commit.

2. **Editorial triage of the cherry-pick candidates.** Review each
   `COMMIT|...|UNIQUE|...` record — this is the LLM-judgment boundary. Confirm
   editorially that the unique commits are genuinely documentation/agent-memory
   content (for example paths under `docs/**`, `.claude/agent-memory/**`, or `**/*.md`).
   The script only reports the deterministic facts (SHA, paths, author, date); deciding
   what counts as documentation is this skill's responsibility.

3. **Consolidate onto `documentationandmemories`.** When the candidate list is
   non-empty, the script creates the `documentationandmemories` branch off `main` in a
   dedicated worktree (never the caller's worktree) and cherry-picks the flagged commits
   oldest-first per source branch, branches in `LC_ALL=C` order, with `-x` provenance. A
   pre-existing `documentationandmemories` branch stops the run with a report — never
   reuse it silently. Conflicts are aborted and surfaced as `CONFLICT` for editorial
   resolution, never auto-resolved.

4. **Push and hand off PR creation to `Agent(pr-author)`.** Push the consolidation
   branch (`git push`). Refresh the PR-context bundle with
   `mcp__drm-copilot__collect_pr_context` using base branch `main` (producing
   `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`).
   Validate the orchestrator-state checkpoint
   (`artifacts/orchestration/orchestrator-state.json`) with `--require-pr-creation-ready`
   and record the `pr_author_preflight` result; delegation is prohibited when that
   validation fails. Then delegate PR creation to `Agent(pr-author)` per
   `.claude/skills/pr-author/SKILL.md`, using `<N> = 396` for the body-file and receipt
   contract. This skill never authors or creates the PR itself.

5. **Wait for merge and verify git-natively.** After the consolidation PR merges,
   verify it with `git fetch` followed by
   `git merge-base --is-ancestor documentationandmemories main`. Exit 0 confirms every
   consolidated commit is now reachable from `main`; that is the only state that unlocks
   deletion of branches whose unique content was consolidated.

6. **Run the apply-mode deletion.** Run `bash scripts/bash/cleanup-worktrees.sh --apply`.
   It re-verifies each candidate's ancestry/equivalence in-process, removes worktrees
   (without force; a dirty worktree is reported via `DIRTY|` lines and skipped), then
   deletes branches with `git branch -D`. The now-merged `documentationandmemories`
   branch and its worktree become `MERGED_CLEAN` instances and are cleaned up by the same
   mechanics.

## Nothing to Consolidate (Short Path)

When report mode classifies every candidate as `MERGED_CLEAN` or `MERGED_EQUIVALENT`
with an empty cherry-pick-candidate list, skip steps 3-5 entirely: proceed directly from
the report to `bash scripts/bash/cleanup-worktrees.sh --apply`. Cleanup completes in a
single session with no PR.

## Prohibited Shortcuts

- Never invoke `gh pr create` or `gh pr edit --body*` from this skill or the scripts. PR
  authoring is `Agent(pr-author)`'s exclusive responsibility and is enforced by the
  `enforce-pr-author-skill.ps1` PreToolUse hook.
- Never pass a force flag to `git worktree remove`. A dirty worktree blocks deletion and
  is reported for manual handling; it is never force-removed.
- Never execute `git worktree prune`. Prunable registrations are report-only.
- Never act on `NOT_MERGED`, `HAS_UNIQUE_RESIDUALS`, or `PROTECTED_CURRENT` candidates;
  the caller's worktree and branch, and the main worktree, are never mutated.
- Never use commit-message text matching as a classification input, and never
  auto-resolve cherry-pick conflicts.

## Cross-References

- `.claude/skills/pr-author/SKILL.md` — the PR body/receipt contract and the delegation
  target for step 4.
- `.claude/skills/pr-context-artifacts/SKILL.md` — how the PR-context bundle is collected
  and the base-branch resolution rules.
- `.claude/rules/shell.md` — the bash toolchain (shfmt/shellcheck/bats/kcov), the
  500-line cap, the no-temp-files test policy, and the `CLEANUP_WT_GIT_BIN` seam
  convention.
