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
  - "Bash(git worktree remove *)"
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
- `WORKTREE|<path>|DETACHED|<state>|<flags>` — a detached-HEAD worktree registration,
  classified on its own HEAD SHA. This five-field record replaces the four-field record
  above for a detached registration; it is not emitted in addition to it. `state` is one
  of `MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT | NOT_MERGED |
  HAS_UNIQUE_RESIDUALS | PROTECTED_CURRENT | ANCESTRY_ERROR`, where `ANCESTRY_ERROR` is
  the fail-closed verdict for a hard git failure at any rung of the classification. The
  fifth field carries the porcelain flag set unchanged, so the `locked` and `prunable`
  markers are preserved in the record.
- `WARN|main-divergence|<local-sha>|<origin-sha>` — local `main` differs from
  `origin/main` (advisory; classification still runs).
- `DIRTY|<worktree-path>|<status-porcelain-line>` — a dirty worktree that blocked
  removal. This record is apply mode only, and its three-field shape is unchanged by the
  dirt classifier: the classifier adds the two records below rather than altering or
  replacing this one.
- `DIRTFILE|<worktree-path>|<verdict>|<detail>|<xy>|<file-path>` — one record per
  `git status --porcelain` entry in a dirty worktree, emitted by report mode in porcelain
  order immediately after that worktree's `WORKTREE|` record. `verdict` is exactly one of
  `DISPOSABLE_BUILD_ARTIFACT | DISPOSABLE_SESSION_ARTIFACT | CONTENT_ON_MAIN |
  CONTENT_IN_HISTORY | STAGED_TREE_IS_COMMIT | UNIQUE`, and no other verdict token is ever
  produced. `detail` carries the commit SHA that justifies the verdict for
  `STAGED_TREE_IS_COMMIT` and `CONTENT_IN_HISTORY`, and is empty for the other four
  verdicts. `xy` is the porcelain two-character status field, preserved verbatim including
  its space. The file path is the last field, so a path containing a pipe character cannot
  shift any later field. A classification read that fails maps its entry to `UNIQUE`: that
  is the fail-closed direction, because an entry the tool could not classify is treated as
  content that must be preserved. `CONTENT_ON_MAIN` is emitted for a tracked entry
  only when `main` contains the path, so an entry whose content exists only as a staged
  blob is reported `UNIQUE` rather than as content that is already on `main`. An entry
  whose porcelain status shows content in both the index and the working tree is likewise
  reported `UNIQUE`, because the rungs that could otherwise resolve it compare working-tree
  content only and would leave the differing staged blob unaccounted for.
  `DISPOSABLE_SESSION_ARTIFACT` matches three fixed
  repository paths under `artifacts/`, is repository-agnostic, and cannot fire in a checkout
  that gitignores `artifacts/` — which drm-copilot does at `.gitignore:6` — because the
  status read never carries `--ignored`; the verdict is retained for consumer checkouts
  where those paths are not ignored, and it must not be made reachable by adding
  `--ignored`.
- `DIRTSUM|<worktree-path>|<aggregate>|<detail>` — exactly one record per dirty worktree,
  emitted by report mode immediately after that worktree's `DIRTFILE|` records.
  `aggregate` is `ALL_DISPOSABLE` if and only if the worktree has at least one status
  entry and none of them is `UNIQUE`, and `HAS_UNIQUE` otherwise. `detail` carries the
  first non-empty per-entry detail. A worktree with zero status entries emits neither this
  record nor any `DIRTFILE|` record. Both records are read-only: an `ALL_DISPOSABLE`
  aggregate unlocks nothing on its own, and clearing happens only when the operator
  supplies `--clear-disposable` in apply mode.

  Report-mode exit status. When a candidate worktree's `git status --porcelain` read
  fails, report mode emits no `DIRTFILE|` and no `DIRTSUM|` record for that worktree and
  returns git's non-zero exit code. A checkout containing such a worktree therefore exits
  non-zero from report mode where the same checkout previously exited 0 and produced a
  complete report. This is deliberate. A worktree whose status read failed produces no dirt
  records at all, so a report that also exited 0 would be indistinguishable from a report
  about a clean worktree, and an operator would make a deletion decision on silently
  incomplete data. The non-zero exit is the only channel that carries the incompleteness to
  a wrapping script. See the analogous apply-mode note in the End-to-End Workflow, where a
  blocked detached removal sets a non-zero exit status for the same reason.
- `ACTION|<verb>|<target>|<result>` — apply-mode action results.
- `PRESERVE|<worktree-path>|<source-path>|<verdict>` — a manifest `preserved_files[]`
  finding staged onto the consolidation branch; `verdict` in `DEAD_ONE_OFF |
  ALREADY_SOLVED_ELSEWHERE | STALE_OR_CONTRADICTED | GENUINELY_NEW | STILL_RELEVANT`. The
  per-file outcome is reported by the companion `ACTION|preserve-stage|...` record.
- `ORPHAN_DIR|<path>|<size>` — a directory under a worktree-tracking root that carries
  no `.git` pointer file and no `git worktree list` entry. `<size>` is best-effort and
  may be the literal `unknown`. The record is advisory: it reports the directory, and
  nothing in apply mode acts on it. For the disposition, see the Dirty Worktree Triage
  Procedure's step 7, which governs how an orphaned directory is handled.
- `STALE_REF|<refname>` — a `refs/remotes/<name>/*` ref whose `<name>` is not a
  configured remote, named in full ref form. Advisory only; no ref is ever pruned by
  this tool.
- `CHILD_OF|<branch>|<ancestor>` — `branch` is a git ancestor of `ancestor`, and both
  resolved exactly `NOT_MERGED` through their own full ladders. It is emitted alongside,
  never instead of, the branch's own `BRANCH|<branch>|NOT_MERGED` line, and names the
  containment relationship so an operator can see that the branch's work is not lost
  when the named ancestor is retained.
- `WARN|registration-lost|<path>` — a worktree directory whose `.git` pointer file names
  a gitdir target that no longer exists. Git commands run inside such a directory fail
  in confusing ways. Advisory only.

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
   `.claude/skills/pr-author/SKILL.md`, which owns the body-file and receipt contract;
   defer to that skill for it rather than restating any value from it here. For `<N>`,
   use the GitHub issue number the run executes under when the run has one. When the run
   has none, `<N>` is an arbitrary run-scoped identifier chosen by the pr-author agent.
   It is not a pull-request number, and the only requirement on it is that the body-file
   path, the receipt's `number` field, and the body bytes all agree. This skill never
   authors or creates the PR itself.

5. **Wait for merge and verify git-natively.** The consolidation PR's merge is
   **human-performed**: the operator merges it outside the agent session; this skill
   never issues the merge itself. This is why: `gh pr merge` is absent from this skill's
   `allowed-tools`; the project permission allow-list (`permissions.allow` in
   `.claude/settings.json`) carries no `gh` entry; and `.claude/hooks/enforce-epic-merge-gate.ps1`
   would deny the command with `EPIC_MERGE_GATE_BLOCKED` if it were attempted. At this
   boundary the agent reports the consolidation pull request's URL or number to the
   operator and stops. The ruleset on `main` sets `strict_required_status_checks_policy`,
   so the branch must be up to date with `main` before the merge becomes available to the
   operator; the wait for the merge is unbounded within a session. After the consolidation
   PR merges, verify it with `git fetch` followed by
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

   A consolidation branch whose tip equals `main`, which is the state of
   `documentationandmemories` between its creation off `main` and its first commit, is
   reported `NOT_ANCESTOR` by a tip-equality pre-check that runs before any network fetch
   and is therefore not delete-eligible; the post-merge cleanup described above is
   unaffected, because a merged consolidation branch's tip differs from `main`, and an
   empty or unresolvable tip on either side is reported `ANCESTRY_ERROR` rather than
   treated as equality.

   Detached-HEAD worktrees are handled on the same terms. Apply mode classifies each on
   its own HEAD SHA, emits the five-field `WORKTREE|<path>|DETACHED|<state>|<flags>`
   record from that verdict, and acts only on the delete-eligible states `MERGED_CLEAN`,
   `MERGED_CONTENT_NEUTRAL`, and `MERGED_EQUIVALENT`. A delete-eligible detached worktree
   is re-verified in the same process immediately before the destructive action and is
   then removed without force. A locked one is skipped with the result token
   `ACTION|worktree-remove|<path>|BLOCKED-LOCKED`; the locked test reads the porcelain
   flags already in hand and precedes every git invocation, so no `git worktree remove`
   command is issued for it. A prunable one is report-only: it produces its registration
   record and no further line, with no removal and no prune. A dirty one reaches the same
   non-forced removal path as a branch-backed worktree and is reported `BLOCKED-DIRTY`
   with its accompanying `DIRTY|` lines.
   In apply mode a blocked detached removal — `BLOCKED-DIRTY`, `BLOCKED-LOCKED`, or
   `BLOCKED-REVERIFY` — sets a non-zero exit status, so a checkout holding dirty or locked
   detached worktrees exits non-zero from `--apply` where the same checkout previously
   exited 0.

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
preserved before the worktree can ever be deleted. Report mode now precedes this
procedure with a machine-readable first pass: one `DIRTFILE|` record per status entry
giving that entry's verdict, and one `DIRTSUM|` record per worktree giving the aggregate
`ALL_DISPOSABLE` or `HAS_UNIQUE`. Steps 1-9 below apply to the worktrees carrying at
least one `UNIQUE` verdict, which are exactly the ones the classifier could not establish
as disposable and which therefore still require human editorial judgement.

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
   disposable. The classifier scopes this class narrowly: it labels a project file
   `DISPOSABLE_BUILD_ARTIFACT` only when every changed line in that entry's diff is an
   analyzer `HintPath` rewrite. A project file whose diff touches anything else — a
   `Compile Include` registration, a package reference, a target or property change — is
   reported `UNIQUE` and is not disposable, so this step's manual characterization is
   still required for it.

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
   clear the dirty working tree, delete a disposable `NOT_MERGED`/`HAS_UNIQUE_RESIDUALS`
   branch directly, or remove the worktree itself through a
   manifest-authorized removal. A manifest-authorized removal is a single
   `git worktree remove <path>` covering one worktree, issued as its own Bash tool call,
   one call per worktree, and it is authorized only when the Sanctioned Removal Manifest
   below carries a record for that exact path whose `removal_disposition` is
   `SAFE_TO_DELETE` and whose `branch_state` is `NOT_MERGED` or `HAS_UNIQUE_RESIDUALS`.
   Never pass a force flag to that command: a dirty worktree blocks deletion and is
   reported for manual handling, and it is never force-removed. This is never automated:
   the script's classification ladder and apply-mode allowlist are never changed to
   accept these states, so a `--apply` run never deletes them on its own, before or
   after triage. If discarding the working-tree content changes the branch's
   classification (for example to content-neutral against `main`), a follow-up
   report/apply pass then picks it up through the normal deterministic path.

   Distinguish two different discards. The sentence above governs the editorial discard
   of `UNIQUE` content — content a human judged safe to lose — and that discard stays
   manual and individually confirmed. Separately, dirt the classifier established as
   disposable, meaning a worktree whose `DIRTSUM|` aggregate is `ALL_DISPOSABLE`, can be
   cleared by the tool itself under the opt-in `--clear-disposable` flag in apply mode.
   That path never touches a `UNIQUE` verdict: one `UNIQUE` entry refuses the clear for
   the whole worktree, and a classification read that errors produces a fail-closed
   `UNIQUE` with the same effect. It is also not force-removal — it clears the working
   tree and retries the same unforced `git worktree remove` after a fresh in-process
   re-verification. The classification ladder and the apply-mode allowlist remain
   unchanged by it.

10. **After local branch deletion, check origin too.** This skill is local-only by
    design (see "When to Use This Skill"), which leaves stale branches on the remote for
    anything already merged. After `--apply` finishes, diff the deleted-local-branch
    list against `git branch -r` (post-prune) to find remote branches whose local
    counterpart is gone, and offer to delete the remainder on origin. Because this
    mutates shared, visible remote state, each deletion requires explicit user
    confirmation — never delete an origin branch as an automatic consequence of local
    cleanup, and never rely on this skill's general `Bash(git push *)` allowance to
    perform it silently.

## Sanctioned Removal Manifest

**Write the manifest before step 9 of the Dirty Worktree Triage Procedure acts on any
`SAFE_TO_DELETE` verdict.** The run records its triage verdicts in
`artifacts/orchestration/cleanup-worktrees-manifest.json`. The two PreToolUse gates
`.claude/hooks/enforce-epic-worktree-removal-gate.ps1` and
`.claude/hooks/enforce-parallel-worktree-removal-gate.ps1` read that document and admit a
`git worktree remove <path>` command only when a record in it covers that exact path. A removal
issued with no covering record is denied with each gate's existing, unchanged reason code.

The document is UTF-8 JSON with `snake_case` keys. It carries six top-level fields. Its `removals`
and `preserved_files` entries are sibling arrays and are never nested: removal records are keyed by
worktree path, preserve records by the pair of worktree path and file path, and the gates must
never traverse preserve data.

### Top-level fields

- `tool` — string, required, exactly `cleanup-merged-worktrees`. The self-identifying
  discriminator. Absent, non-string, or any other value and no record authorizes anything.
- `schema_version` — integer, required, exactly `1`. Absent, non-integer, or any other value fails
  closed; a forward version is not accepted by silence.
- `generated_at` — string, required, an ISO-8601 UTC timestamp recording when the run wrote the
  document. Absent, unparseable, in the future, or older than the 24-hour freshness bound fails
  closed.
- `run_id` — string, required, non-empty. An opaque per-run identifier for audit correlation.
- `removals` — array, required, of removal records. It may be empty; absent, non-array, or empty
  authorizes no removal.
- `preserved_files` — array, required, of preserve records. It may be empty. Neither gate ever
  reads it, and its absence or malformation must not affect any gate decision.

### `removals[]` record fields

- `worktree_path` — string, required, non-empty. The absolute path of the worktree to remove. A
  record with it absent or empty is skipped.
- `branch` — string or `null`, key required. The worktree's branch, or `null` for a detached
  worktree. An absent key means the record does not authorize; `null` is a valid, meaningful value.
- `branch_state` — string, required. The classification the report emitted for that branch: one of
  `MERGED_CLEAN`, `MERGED_CONTENT_NEUTRAL`, `MERGED_EQUIVALENT`, `NOT_MERGED`,
  `HAS_UNIQUE_RESIDUALS`, or `PROTECTED_CURRENT`. Only `NOT_MERGED` and `HAS_UNIQUE_RESIDUALS`
  authorize a removal; every other member, and any value outside the vocabulary, does not.
- `removal_disposition` — string, required. The triage disposition that authorizes removal. The
  allowed set is exactly the single member `SAFE_TO_DELETE`. This key is deliberately distinct from
  the `merge_status` key the orchestration checkpoints carry.
- `verdict` — string, required. The step-5 content classification that produced the disposition:
  one of `DEAD_ONE_OFF`, `ALREADY_SOLVED_ELSEWHERE`, `STALE_OR_CONTRADICTED`, `GENUINELY_NEW`, or
  `STILL_RELEVANT`. Absent, out of vocabulary, or preserve-implying and the record does not
  authorize.
- `evidence` — string, required, non-empty. The justification step 9 requires, citing specific
  files or commit SHAs. Absent, empty, or whitespace-only and the record does not authorize.

### `preserved_files[]` record fields

These records serve the consolidation consumer and are never read by either gate.

- `worktree_path` — string, required, non-empty. The worktree holding the file, and where the
  consumer reads that worktree's `MEMORY.md`.
- `source_path` — string, required, repo-relative within that worktree. The file to preserve.
- `change_class` — string, required, either `untracked` or `modified`, recording whether staging
  adds a new file or carries a working-tree modification.
- `disposition` — string, required, exactly `PRESERVE`.
- `verdict` — string, required, a member of the step-5 verdict vocabulary listed above.
- `target_path` — string, required, repo-relative on the consolidation branch. Required because the
  same relative path may already exist on `main` with different content and a lesson file may be
  re-namespaced; the consumer must not guess it.
- `memory_index_line` — string or `null`, key required. The source worktree's `MEMORY.md` index
  line, or `null` when the file is not a memory entry.
- `line_ending` — string, required, one of `crlf`, `lf`, or `absent`. Advisory only: it records the
  target file's existing convention, and the consumer re-derives and compares rather than trusting
  it.
- `host_token_scan` — object, required, carrying a `result` member and a `pattern_set_id` member.
  It records the scan outcome and the identifier of the pattern set used, never the patterns
  themselves.
- `evidence` — string, required, non-empty.

### Example

```json
{
  "tool": "cleanup-merged-worktrees",
  "schema_version": 1,
  "generated_at": "2026-09-07T03:40:00Z",
  "run_id": "cleanup-2026-09-07T03-40-00Z-a47a5e33",
  "removals": [
    {
      "worktree_path": "C:/Users/DanMoisan/repos/drm-copilot/.claude/worktrees/agent-0f1c2d",
      "branch": "drm-copilot-wt-2026-08-14T09-02",
      "branch_state": "HAS_UNIQUE_RESIDUALS",
      "removal_disposition": "SAFE_TO_DELETE",
      "verdict": "ALREADY_SOLVED_ELSEWHERE",
      "evidence": "Unique residual commit 3f9a1c2 records the cleanup-worktrees ancestry error; main already fixes it at scripts/bash/cleanup_worktrees_lib.sh:214-231 under issue #612."
    }
  ],
  "preserved_files": [
    {
      "worktree_path": "C:/Users/DanMoisan/repos/drm-copilot/.claude/worktrees/agent-91ee43",
      "source_path": ".claude/agent-memory/general-purpose/hook-payload-anomaly.md",
      "change_class": "untracked",
      "disposition": "PRESERVE",
      "verdict": "GENUINELY_NEW",
      "target_path": ".claude/agent-memory/general-purpose/hook-payload-anomaly-envelope.md",
      "memory_index_line": "- [Hook payload anomaly envelope](hook-payload-anomaly-envelope.md) - the deny path a malformed envelope takes",
      "line_ending": "crlf",
      "host_token_scan": {
        "result": "clean",
        "pattern_set_id": "child-f-host-tokens-v1"
      },
      "evidence": "No equivalent file on main under .claude/agent-memory/**; grep for 'payload anomaly' returns only this worktree."
    }
  ]
}
```

### Accepted residual

Both gates read the command text of the `Bash` tool call they are given. A removal routed
indirectly — for example by writing the command into a file and invoking that file with `bash
<file>` — presents command text the gates do not match, so the manifest requirement does not reach
it. This design does not close that indirection, and the requirement is recorded here as an
accepted residual rather than left implicit.

The manifest requirement is a policy-level integrity check, on the same terms
`.claude/hooks/enforce-pr-author-skill.ps1` records for its own receipt mechanism: it prevents
accidental bypass and requires a deliberate, documented act to circumvent. It is not a
cryptographic or security boundary, and it must not be described as tamper-proof. Routing a
removal through such an indirection in order to avoid the manifest requirement is a deliberate act
and is prohibited by this skill, which is the term the residual rests on.

## Prohibited Shortcuts

- Never invoke `gh pr create` or `gh pr edit --body*` from this skill or the scripts. PR
  authoring is `Agent(pr-author)`'s exclusive responsibility and is enforced by the
  `enforce-pr-author-skill.ps1` PreToolUse hook.
- Never pass a force flag to `git worktree remove`. A dirty worktree blocks deletion and
  is reported for manual handling; it is never force-removed. The `--clear-disposable`
  flag is not an exception to this rule and is not force-removal: it clears the working
  tree first and then retries the same unforced `git worktree remove`, so the removal it
  retries is the identical unforced call that was blocked. It runs only when every
  per-file verdict for that worktree is non-`UNIQUE`, only in apply mode, only when the
  operator explicitly requests it, and only after a fresh in-process re-verification that
  the branch is still delete-eligible. A single `UNIQUE` verdict refuses the clear for the
  whole worktree, including the fail-closed `UNIQUE` assigned when a classification read
  errors.
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
- Never issue the consolidation merge command, and never write or edit an orchestration
  checkpoint in order to satisfy `.claude/hooks/enforce-epic-merge-gate.ps1`. This
  forecloses the specific evasion of writing an
  `artifacts/orchestration/orchestrator-state.json` whose `epic_mode` and `step9_status`
  fields the gate's child-feature accept shape would honour for any pull-request number.
- Never widen the definition of disposable dirt. The session-artifact list is a fixed
  in-script array of exactly three paths with no configuration override, no environment
  variable, and no per-repository extension point; adding a path to it is a change to the
  classifier, not a setting. The build-artifact rule requires both conditions together —
  the `*.csproj` path pattern and the content confinement that every changed line in the
  entry's diff is an analyzer `HintPath` rewrite — and a project file whose diff touches
  anything else is `UNIQUE`. Ignored files are never cleared, because they are never
  classified: `git status --porcelain` is read without `--ignored`, so an ignored file
  never becomes an entry, and `git clean` is never given `-x`, `-X`, or `-ff`.

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
- `.claude/hooks/enforce-epic-merge-gate.ps1` — the PreToolUse gate on the consolidation
  merge command, backed by orchestration checkpoints. A cleanup run satisfies none of its
  three checkpoint shapes: it is neither a per-feature orchestration, nor an epic
  integration, nor a parallel run, so it writes none of the three checkpoints the gate
  reads.
