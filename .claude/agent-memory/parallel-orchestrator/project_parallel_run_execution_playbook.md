---
name: parallel-run-execution-playbook
description: TaskMaster-specific mechanics for running /parallel-run — kickoff artifact lives on the plan-home branch, no poetry but bare python is on PATH, no status template, plan-home worktree separate from the session worktree, and Bash heredocs die on apostrophes
metadata:
  type: project
---

Mechanics that the `parallel-orchestrate` skill assumes but that TaskMaster does not supply.

**Why:** TaskMaster is a C# repository whose `.claude` tree is push-down-owned from
drm-copilot with zero templating, so parts of the parallel surface that the skill treats as
present were never ported. Discovering each gap mid-run costs a stall.

**How to apply:**

- **The kickoff artifact is not in the session worktree.** `/parallel-plan` commits
  `docs/features/parallel/<slug>/parallel-kickoff.md` and `parallel.md` to the plan-home
  branch `parallel/<slug>-plan`, which is NOT the branch the session worktree is on. Read
  them with `git show parallel/<slug>-plan:<path>`. Their absence from the working tree is
  not the STOP condition the `parallel-run` skill describes — check the branch before
  concluding the run was never planned.
- **Use a dedicated plan-home worktree.** Check `parallel/<slug>-plan` out at
  `TaskMaster-wt/parallel-<slug>-plan` and write `parallel-status.md` there. Never check the
  plan-home branch out in the session worktree. Keep the checkpoint
  (`artifacts/orchestration/parallel-orchestrator-state.json`, gitignored) in the session cwd,
  because the hooks read it relative to the hook process cwd.
- **The cohort table is NOT in the manifest.** `parallel.md` frontmatter carries only
  `parallel`, `mode`, `max_concurrency`, `created_at`, and `items[]` — no `cohorts` key and no
  `conflict_edges` key. Both live solely in `artifacts/orchestration/parallel-planner-state.json`
  (which is in the session worktree, not on the plan-home branch). The manifest body's cohort
  column is prose. Read the planner state for `cohorts[]`, `conflict_edges[]`, and
  `recolor_generation`, and copy `blast_radius` verbatim from its `items[]` rather than
  re-transcribing from the manifest.
- **The checkpoint is a single file, so a new run OVERWRITES the previous run's state.** Before
  seeding, read `parallel_slug` and `next_step` from the existing
  `parallel-orchestrator-state.json` and confirm the prior run reads `COMPLETE` with every item
  terminal. Seed the new run only after that check.
- **`git show <ref>:<path>` needs `MSYS_NO_PATHCONV=1`.** With it the operand survives intact and
  the plan-home artifacts read correctly; without it the Bash tool mangles it (see
  [[issue-merge-and-removal-commands-bare]]). The same variable makes
  `git rev-parse <branch>:<plan-path>` usable for verifying the kickoff Integrity table's
  plan-hash column, which is a plain git blob SHA.
- **There is no poetry and no pyproject.toml.** Every `poetry run python -m ...` fallback the
  skill names is unavailable. Validate exclusively through
  `mcp__drm-copilot__validate_orchestration_artifacts`. The bash entry points
  (`validate-parallel-manifest.sh`, `compute-cohorts.sh`,
  `compute-concurrency-batches.sh`) DO work and need no interpreter.
- **A bare `python` interpreter IS on PATH, even though poetry is not.** This is the practical
  way to do the checkpoint read-modify-write and every count/projection cross-check: it sidesteps
  all three PowerShell traps in [[powershell-checkpoint-write-traps]] (`[ordered]` positional
  indexing, one-element pipeline collapse, long-heredoc death), because Python dicts preserve
  insertion order and a one-element list stays a list. Use it for the write and reserve `pwsh`
  for the blast-radius library, which has no Python port here. "No poetry" is not "no Python" —
  the playbook bullet above says only that the `poetry run` command forms are unavailable.
- **A Bash heredoc dies on an ASCII apostrophe, even when the delimiter is quoted (`<<'PY'`).**
  A possessive like `the item's pull request` inside the body returns
  `unexpected EOF while looking for matching '`, so the whole call is lost. Quoting the delimiter
  does NOT protect the body here. Two remedies: rewrite the prose to avoid apostrophes when the
  text is short, or — better for long prose — put the text in a `.md` file and apply it with the
  Edit tool, which the pre-implementation gate allows for `.md` anywhere. Writing prose-heavy
  status-doc sections through Edit rather than a heredoc avoids the class entirely.
- **`docs/features/templates/parallel/parallel-status.md` does not exist.** Generate the
  status doc from the documented section list instead: a `## Run` header block, `## Items`,
  item lifecycle timestamps, `## Cohorts`, and the three read-only projections
  `## Conflict Edges`, `## Mutations`, `## Drift Events` (empty renders as an empty section,
  never an omitted one).
- **Committing the status doc trips the pre-implementation gate** unless the pathspec form is
  right; see [[preimplementation-gate-scope]].
- **Any command gate can fire on PROSE you are merely storing. Two have.** Every Bash-matcher hook
  scans the whole command string, so a checkpoint-write whose note *describes* a gated operation is
  denied as though it were performing one. Nothing executes; the token sits in a Python string
  literal. Both observed instances:
  - `PROMOTION_MCP_ONLY_BLOCKED` — a `cleanup_note` quoting `new_potential_bug_entry` while
    explaining why promotion was impossible. Paraphrase to "the intake step writes under
    `docs/features/potential/`".
  - `EPIC_MERGE_GATE_BLOCKED` — a `merge_method_decision` note containing both `gh pr merge` and the
    merge-commit flag while explaining *why* the squash method was rejected. Paraphrase to "the
    merge-commit method" and "the squash method".

  Write gated operations in prose descriptively, never in their literal command form. Same family as
  [[issue-merge-and-removal-commands-bare]] and [[keyed-issue-num-in-delegation-prompts]].
- **The merge gate matches on BOTH the merge subcommand AND the merge-commit flag
  (`enforce-epic-merge-gate.ps1:377`), so the squash method is OUT OF SCOPE and runs unauthorized** —
  no `ci_green` requirement, no `pr_number` match. That matters whenever a child reports a
  merge-method-dependent finding, e.g. large blobs that squashing would drop: switching method to
  avoid the side effect ALSO discards the authorization check. Escalate the side effect instead and
  keep the prescribed method. Same trade as refusing to narrow a radius to buy concurrency.
- **Resolve each item's model from its OWN band; never copy the previous item's receipt.**
  `Resolve-DelegationModel -Agent orchestrator -Band <C1|C2|C3> -FablePolicy available` returns
  haiku / sonnet / opus respectively. Note the parameter names are `-Agent` and `-Band`, NOT
  `-ComplexityBand`, and `Get-ComplexityFloor` takes `-SignalsPresent`. On this run item 285 was C3
  (opus) and item 287 was C2 (sonnet), so copying the prior receipt would have over-provisioned.
- **An item branch can advance OUT OF BAND, and the local ref then lags the remote.** Before each
  launch, compare `git ls-remote origin refs/heads/<branch>` against the checkpoint `head_sha` AND
  against the local ref. On item 633 the checkpoint recorded `064ed05b`, the remote was at
  `e1bd7235`, and the local branch still sat at the checkpoint value, because someone had merged
  `main` into the item branch between sessions. Launching then would have checked out a stale local
  ref. Confirm with `git merge-base --is-ancestor <local> <remote>` and fast-forward with the
  refspec form `git fetch origin <branch>:<branch>`, which works precisely because the branch is not
  checked out anywhere. Tell the child in the prompt that an earlier `main` merge is already present,
  so a second reconciliation is expected rather than a symptom.
- **`main` also advances between your own merges.** Read the fetch output range, not just the new tip:
  `09eae2e8..e3e33ddb` revealed two commits that were not mine.
- **`validate-bash.ps1` blocks `git push --force-with-lease`** by literal substring match on
  `git push --force`, so the safe form is denied along with the unsafe one. The 287 child worked
  around it by deleting and re-pushing the branch after verifying single ownership and an unchanged
  remote tip. Do not hunt for a phrasing that evades the match.
- **`delegation_receipts[]` means YOUR OWN delegations, and nothing else.** The validator derives the
  delegated-agent set from that array and then demands a matching model-routing receipt per entry, so
  padding it with lifecycle phases you did not delegate BREAKS a previously-passing validation by
  demanding receipts for delegations that never happened. The 633 child hit this by recording its
  preparation-run phases there to satisfy a completion gate, and had to remove them and put the
  lifecycle in a narrative key instead. Record context in a prose field; keep the array to one entry
  per real `Agent(...)` call you made.
- **A footprint check run by the committer cannot see what a concurrent writer contributed.** On 633
  the executor checked off its footprint criterion truthfully from its own vantage and was still
  wrong: two `.claude/agent-memory/` paths written by the parent orchestrator while it worked had
  been swept into its close-out commit. Verify a footprint claim with the three-dot diff of what the
  merge actually added (`git diff --name-only <prev-main> <merge-commit>`), which is observer-side
  and sees every contributor. Same family as the merge-time worktree reading in
  [[defer-dirty-worktree-cleanup-never-force]]: a reading taken by a participant races the other
  writers.
- **`main` can advance WHILE a child runs, silently superseding the tree its subagents read.** On item
  646 a sibling item merged mid-run, so the researcher's citations described a tree that no longer
  existed. Nothing local signals this: `git status` stays clean and `HEAD` matched `origin/main` at
  branch time, so the divergence shows only by comparing `git show HEAD:<file>` against
  `git show origin/main:<file>`. Tell each child to re-fetch and re-compare at every phase boundary
  rather than once at start, and expect a second reconciliation to be normal rather than a symptom.
- **A direct barrier probe needs the `tool_input` wrapper, or it denies for the wrong reason.**
  `Invoke-ParallelCohortBarrierDecision -ToolInputRaw <json>` expects the full hook envelope
  `{"tool_input":{"subagent_type":...,"prompt":...}}`. Passing the inner object at the root returns
  `deny` with reason `PARALLEL_COHORT_BARRIER_BLOCKED: payload anomaly - ... no tool_input key`,
  which reads exactly like a real barrier block and is not one. Build it with
  `@{ tool_input = $inner } | ConvertTo-Json -Depth 6 -Compress`. **Always print
  `permissionDecisionReason`, never just `permissionDecision`**: the decision alone cannot
  distinguish a genuine block from a malformed probe, and treating an envelope anomaly as a real
  block would stall a launch that the barrier actually permits.
- **`scripts/dev_tools/` does not exist in TaskMaster at all, so every mutation-engine function the
  `/parallel-add`, `/parallel-remove`, and `/parallel-close` skills name is unreachable by the path
  they cite.** `parallel_mutation_protocol.py`, `parallel_cohort_computation.py`, and the
  `_parallel_mutation_models.py` helper live only in `C:/Users/DanMoisan/repos/drm-copilot`. Run them
  from there with `python -c` after `sys.path.insert(0, r'C:/Users/DanMoisan/repos/drm-copilot')`,
  reading the checkpoint by absolute path out of the TaskMaster session worktree. Do not conclude the
  engine is missing and hand-roll the decision: the pure functions import and run correctly
  cross-repo, and hand-rolling loses the exact rejection type and its collected key tuple. Same
  push-down gap family as the missing status template and the absent poetry environment.
- **The close gate keys on item `state == in_flight` ONLY, never on `merge_status`.**
  `decide_close` collects `record.is_pinned`, so an item resting at `merged` with its worktree still
  present does NOT block a close, while a single `in_flight` item does regardless of how far along it
  is. On the `bugs-638-644-647` run thirteen items sat at `merged`/`worktree_removed` and item 678
  alone — branch pushed, two local commits, no pull request — rejected the close. That same run then
  CLOSED cleanly on 2026-09-02 once 678 merged, with all 14 items at `state: merged` and SEVEN item
  worktrees still on disk under deferred cleanup — which confirms the rule from the accepting side,
  not just the rejecting one. The practical reading: deferred worktree cleanup never stands between a
  run and its close, so do not force a removal to unblock one (see
  [[defer-dirty-worktree-cleanup-never-force]]). The close is also cheap to re-attempt — a rejected
  close writes nothing at all — so a periodic retry is the right response to a single blocking item,
  never a workaround.
- **Free the item branches before launching**; see [[free-item-branches-by-detaching]].
- **`main` is unprotected**, so same-cohort merges need no `gh pr update-branch` re-green
  cycle and may merge in any order.
