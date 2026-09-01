---
name: orchestrator-state-json-is-tracked-in-git
description: artifacts/orchestration/orchestrator-state.json is TRACKED on main despite .gitignore listing artifacts/, so writing your checkpoint dirties the tree and pollutes the change footprint; fix with git update-index --skip-worktree
metadata:
  type: project
---

`.gitignore` line 57 is `artifacts/`, but `artifacts/orchestration/orchestrator-state.json` is
**tracked anyway**: commit `e8e628f0` ("ci(format): recover CI formatter configuration") force-added it
onto main during a Codex recovery run. `.gitignore` has no effect on an already-tracked path, so every
orchestrator that writes its checkpoint in a fresh worktree immediately dirties a tracked file.

**Why it bites.** Any plan with a footprint assertion fails. On issue #647 the plan's AC19 required
that `git diff --name-only <BASE_SHA> -- ":(exclude).claude"` return only the five footprint paths plus
the feature folder. A written checkpoint puts `artifacts/orchestration/orchestrator-state.json` on that
list, and the criterion is then recorded unchecked and REMEDIATION-REQUIRED for a reason that has
nothing to do with the change. The `.claude` exclusion that plans usually carry does not cover it.

**Remedy, verified 2026-08-31 on #647:**

```
git update-index --skip-worktree artifacts/orchestration/orchestrator-state.json
```

Run it once, before the first checkpoint write. `git ls-files -v` then shows `S` for the path, and both
`git status --porcelain` and `git diff --name-only <sha>` stop reporting it. It is a local index flag
only: it commits nothing, changes no tracked content, and does not touch `.gitignore`. Verified by
appending a byte to the file and confirming both commands stayed empty.

Do NOT instead `git rm --cached` it (that stages a deletion onto your branch) and do NOT untrack it as
a drive-by fix inside a scoped feature branch. Tell the executor explicitly not to run any
`git update-index` command itself, and record the flag in the checkpoint `notes` so the next agent
does not read the clean status as evidence the file is untracked.

**Second reason to set the flag, found 2026-09-01 on #648.** The tracked blob is not a stale husk: it
is a *live checkpoint belonging to whichever item last force-added it*. On the #648 parallel item the
file present on `origin/main` carried `objective` for issue **#469**, `next_step: S8_local_stop`, and
that item's full delegation receipts. Overwriting it with your own checkpoint and committing does two
separate harms — it pollutes your footprint, and it destroys another item's committed record on main.
So read the file before your first write and check whose `issue-num` it carries. If it is not yours,
that is confirmation to set `--skip-worktree`, not a reason to "clean it up".

Watch the gate after the overwrite. A rich, well-formed checkpoint still fails
`PREIMPLEMENTATION_GATE_BLOCKED` if it omits the one boolean `lifecycle_ready: true` — the readiness
predicate `Test-OrchestrationReady` reads exactly `issue-num`, `feature-folder` (must start with
`docs/features/active/`), `route_id` or `path_selected`, and `lifecycle_ready`, and nothing else it
finds compensates. See [[bootstrapping-orchestrator-state-json-first-write]] for that key set.

The real defect is upstream: the file should never have been committed. Worth its own issue if it
recurs. Related: [[bootstrapping-orchestrator-state-json-first-write]],
[[model-routing-hook-reads-canonical-path-only]], [[stale-base-anchor-passes-ancestry-vacuously]].

**You cannot simply leave the inherited file alone (verified 2026-09-01, #670 preparation resume).**
The committed content on `main` belongs to whatever run last force-added it — on that resume it was
issue #469's CI-format-recovery state, a completely unrelated objective. That is not merely untidy:
`enforce-model-routing-receipt.ps1` reads this exact path and requires a `model_routing_receipts[]`
entry whose `agent` equals the `subagent_type` being delegated, so **every** `Agent(atomic-planner)`
and `Agent(atomic-executor)` call is denied `MODEL_ROUTING_RECEIPT_BLOCKED` until you write your own
checkpoint. The inherited #469 file carries `codex_model_routing_receipts` (the Codex-topology
spelling), which does **not** satisfy the hook — it scans `model_routing_receipts` only.

The working sequence is: `git update-index --skip-worktree` the path FIRST, then overwrite it with
your own checkpoint. The flag makes the overwrite invisible to `git status` and to
`git diff --name-only`, so the branch footprint stays exactly the plan and evidence you intended, and
the shared `main` content is never modified. Confirmed across five delegations and four checkpoint
edits: `git status --porcelain` never once listed the path. Record the flag in the checkpoint `notes`
so a later reader does not mistake the clean status for evidence the file is untracked.
