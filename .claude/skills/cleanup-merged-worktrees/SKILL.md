---
name: cleanup-merged-worktrees
description: 'Detect, consolidate, and delete git worktrees/branches that are fully merged into main; use after an epic or feature''s PRs have merged and stale drm-copilot-wt-* branches/worktrees remain, driving the detect -> report -> consolidate -> pr-author handoff -> post-merge deletion workflow.'
allowed-tools:
  - Read
  - Grep
  - Glob
  - Agent
  - "Bash(bash scripts/bash/cleanup-worktrees.sh *)"
  - "Bash(git fetch *)"
  - "Bash(git merge-base *)"
  - "Bash(git push *)"
  - "Bash(git rev-parse *)"
  - "Bash(git status *)"
  - "Bash(git log *)"
  - "Bash(git show *)"
  - "Bash(git diff *)"
  - "Bash(git branch -r*)"
  - "Bash(git worktree list*)"
  - "Bash(gh issue view *)"
  - mcp__drm-copilot__new_potential_bug_entry
  - mcp__drm-copilot__potential_to_issue
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
- When a worktree is reported `BLOCKED-DIRTY`, or its branch is classified `NOT_MERGED`
  or `HAS_UNIQUE_RESIDUALS`, and the uncommitted or unmerged content it holds must be
  triaged into disposable versus must-preserve before the worktree can ever be deleted.
- Do not use this skill to manage remote branches; its scope is local branches and
  local worktree registrations only, except for the explicitly confirmed origin-branch
  offer in the Dirty Worktree Triage Procedure's final step.

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
   mechanics. Any worktree left standing afterward — reported `BLOCKED-DIRTY`, or whose
   branch classified `NOT_MERGED` or `HAS_UNIQUE_RESIDUALS` — is not abandoned; it moves
   to the Dirty Worktree Triage Procedure below.

## Nothing to Consolidate (Short Path)

When report mode classifies every candidate as `MERGED_CLEAN` or `MERGED_EQUIVALENT`
with an empty cherry-pick-candidate list, skip steps 3-5 entirely: proceed directly from
the report to `bash scripts/bash/cleanup-worktrees.sh --apply`. Cleanup completes in a
single session with no PR.

## Dirty Worktree Triage Procedure

**Trigger.** A worktree reported `ACTION|worktree-remove|<path>|BLOCKED-DIRTY` (with
accompanying `DIRTY|<path>|<status-porcelain-line>` records), or a branch classified
`NOT_MERGED` or `HAS_UNIQUE_RESIDUALS`, carries uncommitted or unmerged content the
script correctly refuses to discard. That refusal is correct and this procedure never
overrides it — a dirty worktree is never force-removed. This procedure is the systematic
follow-up: deciding, per worktree, whether that content is disposable or must be
preserved before the worktree can ever be deleted.

Steps 1-7 are read-only investigation. Run them per worktree, or fan out one
`Agent(general-purpose)` investigation per worktree (or small batch) concurrently per
step 8, each returning a `SAFE_TO_DELETE` / `PRESERVE` verdict with justification citing
specific files or commit SHAs, before step 9 acts on any finding.

1. **Re-verify current state before analyzing.** Worktrees can be actively in use by
   another concurrent session. Re-run `git status --porcelain` in the worktree and
   re-check the branch's merge status fresh — do not reuse the original scan's
   snapshot. If the worktree's `.git`/index/HEAD mtimes show activity in the last few
   minutes, treat it as possibly live and pause rather than analyze it as abandoned.

2. **Check committed-but-unmerged commits, not only the working tree.** Run
   `git log main..<branch> --oneline`. Some worktrees carry real commits that never
   merged, separate from uncommitted working-tree changes. Both need the classification
   in step 5.

3. **Check for equivalent content already on `main`, by topic, not only by path.** For
   every dirty, untracked, or unmerged file, check `git show main:<path>` at the same
   path, and also grep broadly across the relevant shared namespace (for example
   `.claude/agent-memory/**` for lesson files, `docs/features/**` for feature docs)
   since the same fact is often re-recorded under a different filename on `main`.

4. **For feature-folder doc snapshots** (`issue.md`, `plan.md`, `spec.md`,
   `research/*`), check whether the feature is fully closed on `main` — acceptance
   criteria all checked, code-review/feature-audit/policy-audit artifacts present, an
   evidence trail present. An earlier draft of an already-closed feature is almost
   always fully superseded; diff it against the closed feature's final artifacts to
   confirm rather than assume.

5. **Classify any content that is not obviously superseded** into exactly one of:
   - `DEAD_ONE_OFF` — real, but tied to an already-executed, closed plan with no reuse
     elsewhere (check whether the same pattern appears in shared `.claude/skills/**`
     templates or in other feature plans). Low value; safe to discard even though it is
     not technically duplicated.
   - `ALREADY_SOLVED_ELSEWHERE` — the underlying problem it documents is fixed a
     different way on `main` (check `main`'s current code/config/script, not only its
     memory files — a memory file can describe a bug that no longer exists).
   - `STALE_OR_CONTRADICTED` — `main`'s current version of the same lesson has since
     been corrected to state something different or opposite. This is not merely
     redundant; it is actively wrong, and discarding is the right call.
   - `GENUINELY_NEW` / `STILL_RELEVANT` — not found anywhere else, or it corrects
     something `main` currently gets wrong, or it documents unresolved scope on a
     still-open issue (verify open/closed with `gh issue view <n>`; never assume). Must
     be preserved before the worktree is deleted.

6. **Handle non-memory dirty content on its own terms.** Some worktrees carry stale
   build artifacts (a modified `.csproj`/`packages.config`/`app.config` from a build run
   in that worktree) rather than documentation. Diff a representative sample against
   `main` (`git diff main -- <path>`) to characterize the change before deciding it is
   disposable.

7. **Recognize orphaned non-worktree directories.** A path can still exist on disk
   under a worktree-tracking folder after `git worktree remove` partially ran or
   failed, with no `.git` file inside and no entry in `git worktree list`. These are no
   longer worktrees — flag them for plain filesystem removal, not `git worktree
   remove`, which will misfire or no-op on them. Filesystem removal of an orphaned
   directory is a destructive action outside this skill's pre-approved tool surface; it
   requires explicit user confirmation each time, the same as any other irreversible
   delete.

8. **Parallelize the triage.** Steps 1-7 are pure read-only investigation. Fan out one
   `Agent(general-purpose)` investigation per worktree (or a small batch) concurrently,
   each following steps 1-7 and returning a structured `SAFE_TO_DELETE` / `PRESERVE`
   verdict with justification. This scales far better than triaging serially.

9. **Route `PRESERVE` findings through the existing consolidation flow** (the
   `documentationandmemories` branch/PR mechanism in steps 3-4 of the End-to-End
   Workflow above) before that worktree's dirty content is discarded. If a finding
   describes unresolved product scope rather than a process lesson, promote it to a
   real follow-up issue instead of folding it into the docs/memory PR: file it with
   `mcp__drm-copilot__new_potential_bug_entry` and promote with
   `mcp__drm-copilot__potential_to_issue` per
   `.claude/skills/feature-promotion-lifecycle/SKILL.md`. For a `SAFE_TO_DELETE`
   verdict, discard the content as a distinct, individually confirmed manual action —
   clear the dirty working tree, or delete a disposable `NOT_MERGED`/
   `HAS_UNIQUE_RESIDUALS` branch directly. This is never automated: the script's
   classification ladder and apply-mode allowlist are never changed to accept these
   states, so a `--apply` run never deletes them on its own, before or after triage. If
   discarding the working-tree content changes the branch's classification (for example
   to content-neutral against `main`), a follow-up report/apply pass then picks it up
   through the normal deterministic path.

10. **After local branch deletion, check origin too.** This skill is local-only by
    design (see "When to Use This Skill"), which leaves stale branches on the remote for
    anything already merged. After `--apply` finishes, diff the deleted-local-branch
    list against `git branch -r` (post-prune) to find remote branches whose local
    counterpart is gone, and offer to delete the remainder on origin. Because this
    mutates shared, visible remote state, each deletion requires explicit user
    confirmation — never delete an origin branch as an automatic consequence of local
    cleanup, and never rely on this skill's general `Bash(git push *)` allowance to
    perform it silently.

## Prohibited Shortcuts

- Never invoke `gh pr create` or `gh pr edit --body*` from this skill or the scripts. PR
  authoring is `Agent(pr-author)`'s exclusive responsibility and is enforced by the
  `enforce-pr-author-skill.ps1` PreToolUse hook.
- Never pass a force flag to `git worktree remove`. A dirty worktree blocks deletion and
  is reported for manual handling; it is never force-removed.
- Never execute `git worktree prune`. Prunable registrations are report-only.
- Never act on `NOT_MERGED`, `HAS_UNIQUE_RESIDUALS`, or `PROTECTED_CURRENT` candidates
  through the script or its apply-mode allowlist; `--apply` never mutates them, and the
  caller's worktree and branch, and the main worktree, are never mutated under any
  disposition. The Dirty Worktree Triage Procedure's `SAFE_TO_DELETE` verdict authorizes
  only a distinct, individually confirmed manual action outside that automated path for
  `NOT_MERGED`/`HAS_UNIQUE_RESIDUALS` — never a change to the classification ladder
  itself, and never for `PROTECTED_CURRENT`.
- Never use commit-message text matching as a classification input, and never
  auto-resolve cherry-pick conflicts.
- Never delete an origin branch, or run plain filesystem removal on an orphaned
  worktree-tracking directory, without explicit per-item user confirmation — both are
  outside this skill's pre-approved tool surface regardless of how the triage verdict
  came out.

## Cross-References

- `.claude/skills/pr-author/SKILL.md` — the PR body/receipt contract and the delegation
  target for step 4.
- `.claude/skills/pr-context-artifacts/SKILL.md` — how the PR-context bundle is collected
  and the base-branch resolution rules.
- `.claude/rules/shell.md` — the bash toolchain (shfmt/shellcheck/bats/kcov), the
  500-line cap, the no-temp-files test policy, and the `CLEANUP_WT_GIT_BIN` seam
  convention.
- `.claude/skills/feature-promotion-lifecycle/SKILL.md` — the potential-entry-to-issue
  promotion path used by the Dirty Worktree Triage Procedure's step 9 for `PRESERVE`
  findings that describe unresolved product scope.
