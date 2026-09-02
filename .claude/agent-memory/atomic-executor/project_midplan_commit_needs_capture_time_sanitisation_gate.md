---
name: midplan-commit-needs-capture-time-sanitisation-gate
description: A plan with a mid-plan commit task cannot rely on a final whole-tree sanitisation sweep; every artifact the intermediate commit carries needs its own capture-time rewrite instruction plus a zero-sweep gate.
metadata:
  type: project
---

When a plan commits evidence part-way through (for example a Phase 3 `git add`/`git commit`
that stages `evidence/` before Phase 4 runs), a whole-tree host-path sanitisation sweep placed
in the final phase does not protect those artifacts. Sanitisation in place cannot recover a
literal that is already in an earlier commit's tree.

The correct shape is per-artifact: each task that reproduces tool output into an artifact the
intermediate commit will carry must state (a) the rewrite instruction, naming the exact
placeholder spelling for each absolute root the tool prints, and (b) a capture-time zero-sweep
gate over that one artifact. State the gate even where the command's success-case output is
expected to name no path — the gate is cheap, it is failable, and its absence is what makes the
blanket "no artifact may contain an absolute host path" obligation unenforceable.

Two classes of command need this and are easy to miss:
- a command whose output enumerates roots the plan did not anticipate. `dotnet --list-sdks`
  under a `global.json` with `paths: [".dotnet-sdk", "$host$"]` prints the machine-wide install
  in addition to the repo-local one, so a repo-local-only rewrite leaves a bracketed absolute
  path behind.
- a command that invokes a script which resolves a tool internally. The plan's binding text must
  say it covers script-internal resolution, not only tasks that call `vswhere` directly.

A generated binary or XML artifact (`.trx`, Cobertura) cannot be authored with placeholders, so
it stays in the in-place sanitisation task's scope; the markdown artifacts move to capture-time
gating. Keep the two lists disjoint and keep the in-place task's file count arithmetic matching
its own enumeration.

**Why:** #670's plan needed three preflight rounds' worth of deltas to close this; rounds 2-4
each found another artifact whose only sanitisation reached it after the intermediate commit.

**How to apply:** when reviewing or authoring any plan that has both a mid-plan commit task and
a final sanitisation sweep, enumerate every artifact the mid-plan commit stages and check each
one has a gate of its own. See also [[project_sanitisation_task_cannot_sweep_its_own_record]]
and [[project_selftest_probe_literal_trips_the_next_sweep_pass]].
