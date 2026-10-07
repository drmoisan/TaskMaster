---
name: project-glob-tool-blind-under-claude-worktrees
description: Glob returns "No files found" for paths inside an item worktree under .claude/worktrees/, even for files that exist; Read and Grep with absolute paths work
metadata:
  type: project
---

The Glob tool returned "No files found" for `docs/features/active/*956*/**` with `path` set to an item worktree under `.claude/worktrees/agent-*`, although the folder existed (observed 2026-10-01, issue 956 preflight). Read and Grep with absolute paths inside the same worktree worked normally.

Counter-observation (2026-10-03, issue 964 execution): Glob with an absolute `path` inside `.claude/worktrees/agent-a3fb...` and patterns `**/*.md`, `**/*` and `preflight-clearance.*.md` returned the full, correct listing. The blindness is therefore intermittent or pattern/path-form dependent, not universal.

**Why:** most likely the session checkout's ignore rules exclude `.claude/worktrees/`, so the glob walker skips it; not confirmed.

**How to apply:** in an item worktree, do not treat an empty Glob as proof of absence. Read the expected path directly, or use `git -C <worktree> status --porcelain --ignored` / Grep. A plan gate that relies on a Glob listing (for example a `SORTEMAIL-FILES:` style check) should be confirmed with Grep before reporting a missing file. Related: [[project-planner-and-executor-observe-different-worktrees]].
