---
name: preflight-catches-what-the-plan-validator-cannot
description: The MCP plan validator passed 3x on a plan carrying 8 blocking execution defects; budget multiple atomic-executor preflight cycles for large plans and never treat validator success as approval
metadata:
  type: project
---

The `mcp__drm-copilot__validate_orchestration_artifacts` plan gate checks **structure only** —
headings, task-ID format, sequencing. It passed three times on the F11 plan (#454, 24 phases,
437 tasks) while that plan still contained eight defects that would have broken execution.
`atomic-executor` preflight found all of them.

**Why:** structure validation cannot execute a command, resolve a path against the executing
worktree, read the source a task claims to seam, or check that a comparator proves what its
acceptance says. Only preflight, which reads the real tree, can.

**How to apply:** on any plan above roughly 100 tasks, expect 2-3 preflight cycles and budget for
them. Do not report a plan as approved on validator success alone — the skill's Validator Gate is
necessary, not sufficient. Re-run the validator after every revision, then re-delegate preflight.

Defect classes preflight caught that structure validation cannot, all worth checking for directly:

1. **Guards evaluated against the absolute path.** A stale-build guard matched `\.claude\`
   anywhere in the absolute path, but the plan executes *inside* `.claude/worktrees/agent-<id>/`,
   so it flagged the executing worktree's own freshly built assemblies. It passed only because
   nothing was built yet. Any path predicate must be evaluated relative to the repo root.
2. **Test tasks running against an unbuilt assembly.** Fifteen scoped `vstest` tasks had no build
   in their phase, so the file created earlier in the phase was not in the DLL, the filter matched
   zero tests, and the task reported green vacuously.
3. **Seam shapes that are unimplementable as specified.** One adapter field was specified to wrap
   a single instance while every call site it had to serve was per-element; another captured a
   field that is reassigned at five sites, so it went stale.
4. **Vacuous verification comparators.** Four tasks used `<merge-base>..HEAD`, which compares
   committed history, while the plan scheduled no commit before them — they would have passed on
   an empty diff and proved nothing.
5. **Unsatisfiable clean-tree assertions.** Two rounds were needed: the invariant must be scoped by
   *category of path* (no `.cs`/`.csproj`/`packages.config`/`app.config`), not by directory, because
   the feature folder, agent-memory, and promoted-defect docs are all legitimately untracked.
6. **Absolute exit-0 toolchain gates in an epic child.** See
   [[project_epic_child_nullable_fanin_debt_deferred]] — make analyzer/nullable gates
   baseline-relative, or the child inherits sibling debt it has no mandate to fix.

Fixing one defect can create another: narrowing a seam's member list made a later coverage task
contradict it. That is why the second and third passes were worth running.

Related: [[planner-executor-lack-mcp-validator]], [[mcp-plan-validator-defective-em-dash]].
