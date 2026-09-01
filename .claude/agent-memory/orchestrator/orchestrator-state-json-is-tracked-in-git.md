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

The real defect is upstream: the file should never have been committed. Worth its own issue if it
recurs. Related: [[bootstrapping-orchestrator-state-json-first-write]],
[[model-routing-hook-reads-canonical-path-only]], [[stale-base-anchor-passes-ancestry-vacuously]].
