---
name: agent-memory-is-tracked-scope-git-gates
description: .claude/agent-memory/** is a TRACKED, agent-written path — every plan gate built on git diff/status/grep must carry an explicit pathspec or it is unsatisfiable
metadata:
  type: feedback
---

`.claude/agent-memory/**` is tracked in git, is routinely already modified at branch head, and is written to *further* by agents while the plan executes. Any plan task whose acceptance is an unscoped git command is therefore unsatisfiable by construction. Scope every such gate:

- `git diff --name-only -- '*.cs' '*.csproj' '*.sln'` instead of bare `git diff --name-only` for "lists exactly these files" gates.
- `git status --porcelain -- '*.cs' '*.csproj' '*.sln'` instead of bare porcelain for clean-tree gates.
- `git diff -- <the two in-scope file paths>` instead of a whole-tree `git diff` for prohibited-token grep gates.

The grep case is the non-obvious one. `.claude/agent-memory/atomic-planner/MEMORY.md` is *prose about* prohibited patterns, so its text literally contains tokens like `DoNotParallelize`, `Thread.Sleep`, and `Ignore]`. An unscoped `git diff | grep DoNotParallelize` fires a false positive on a memory-index line and fails a task that has nothing wrong with it.

Scoping a grep gate costs no coverage as long as a sibling task independently proves the scoped set *is* the whole source diff — pair the grep task with a scoped `git diff --name-only -- '*.cs' ...` "lists exactly" task and cite it in the grep task's text.

**Why:** #508 revision pass 1 scoped the two Phase 0 gates (P0-T3, P0-T14) but left the same defect in three Phase 1/2 tasks, costing an entire extra preflight pass. Scoping is not a per-task judgment call; it is a property of the repo layout and applies to every git-based gate in the plan.

**How to apply:** When writing or revising any plan, sweep for `git diff`, `git status`, and "grep the diff" across *all* phases at once and apply the pathspec uniformly. Add a `## Notes` entry stating the scoping rule once and marking it binding on the specific task IDs, so a later reviewer does not read the pathspec as a weakened gate. This refines, and does not contradict, [[never-pin-head-sha-as-plan-expectation]]: a pathspec is a source-tree invariant, not a permitted-dirt enumeration — never list specific dirty files as tolerated.
