---
name: glob-hides-gitignored-checkpoint-read-before-first-write
description: Glob skips gitignored files, so "no checkpoint found" via Glob is false in a worktree where orchestrator-state.json is ignored; I overwrote a preparation run's checkpoint unread on 929
metadata:
  type: feedback
---

Before the first write of `artifacts/orchestration/orchestrator-state.json` in an item worktree, check for
an existing file with `git status --porcelain --ignored -- artifacts` or a direct Read, never with Glob.

**Why:** on the 929 parallel resume (2026-09-30) Glob over `artifacts/orchestration/*.json` returned
nothing, so I installed a fresh checkpoint over the path. A later `git status --ignored` showed the file
had existed all along (the path is gitignored and untracked on that branch). The preparation run's raw
promotion receipts in it were lost and had to be summarized from branch history instead.

**How to apply:** treat an empty Glob as "no tracked, non-ignored match" only. For any gitignored target
(checkpoints, `artifacts/pester/*`, `coverage/*`), probe with Read or `git status --ignored` and read the
existing content before overwriting. Related: [[bootstrapping-orchestrator-state-json-first-write]],
[[orchestrator-state-json-is-tracked-in-git]].
