---
name: agent-memory-tracked-breaks-unscoped-git-gates
description: .claude/agent-memory/** is tracked and dirty at branch head, so any unscoped git diff/status/grep gate in a plan is unsatisfiable or false-positive; every git gate needs an explicit pathspec
metadata:
  type: project
---

`.claude/agent-memory/**` is a **tracked** directory in this repo, and agents write to it during
planning and execution. At the head of a fresh feature branch it is routinely already modified
versus the merge-base (verified on `bug/wpf-dispatcher-yield-test-order-dependent-508`: three
modified tracked files plus four untracked ones, while the scoped `.cs`/`.csproj`/`.sln` diff was
empty). Consequently:

- An unscoped `git diff --name-only` "lists exactly <the in-scope files>" gate is **unsatisfiable**.
- An unscoped `git status --porcelain` "is empty" gate is **unsatisfiable** (the untracked
  `<FEATURE>/` folder and every evidence artifact the plan writes also land here).
- A prohibited-fix grep over an unscoped `git diff` produces **false positives**: the memory files
  are prose about past defects, so `.claude/agent-memory/atomic-planner/MEMORY.md` literally
  contains tokens like `DoNotParallelize`, `Thread.Sleep`, and `Task.Delay`.

**Why:** this defect class survived two full preflight revision rounds on issue #508 (three passes
total) because it is invisible when reading the plan prose — it only surfaces when you actually run
`git status --porcelain` in the worktree. Planners write the natural unscoped command and it looks
correct.

**How to apply:** during preflight, grep the plan for every `git diff` / `git status` / `git grep`
occurrence and confirm each **gating** one carries an explicit pathspec — either
`-- '*.cs' '*.csproj' '*.sln'` or the literal in-scope file paths. Unscoped forms are acceptable
only for record-only capture that the task text explicitly says is not a gate. Pair the scoped
`git diff` (catches modified/deleted) with the scoped `git status --porcelain` (catches added/
untracked) — neither alone proves "no file was added or removed". Note also that bare
`git diff --name-only` is worktree-vs-index; if anything gets staged mid-execution, switch to
`git diff --name-only HEAD -- <pathspec>`.

**Re-check Phase 0 after a revision pass fixes the late phases.** On issue #501 the planner was told
to scope the clean-tree checks and scoped only the two *commit-time* tasks (P9-T4, P9-T6), leaving
the **Phase 0 baseline** task's `git status --porcelain` "produced zero output lines" acceptance
unscoped. That one is the *most* certain to fail: at execution start the planner's own dirty
agent-memory files and the not-yet-committed plan file are still in the tree (measured: three
entries). When you accept a fix for this defect class, re-grep the whole plan — the natural mental
model is "clean tree at the end", so authors patch the end and miss the baseline capture.

**A Phase-0 capture of the dirty set is NOT sufficient.** On issue #731 the planner fixed this class
by having Phase 0 record the pre-existing dirty set and having the Phase 5 gates subtract it. That
still fails, because the *executor itself* writes `.claude/agent-memory/atomic-executor/MEMORY.md`
(tracked; 720 tracked files under `.claude/agent-memory/`) mid-execution, i.e. AFTER the Phase 0
capture, so the path enters the tracked-modified set outside the recorded baseline and the terminal
exact-count gate fails with no escape clause. Evidence it is not hypothetical: on that branch the
planner's own `MEMORY.md` was already modified and a further untracked planner memory file appeared
between the plan's baseline observation and preflight round 2. Require a **standing** allowance
("every path under `.claude/agent-memory/` is out of scope for every gate, whenever it appeared"),
not a point-in-time snapshot.

Related: [[project_preflight_selfderived_gate_thresholds_are_blind]],
[[project_418_plan_rationale_clauses_are_evidence]],
[[project_preflight_ac_checkoff_and_tooloutput_paths]].
