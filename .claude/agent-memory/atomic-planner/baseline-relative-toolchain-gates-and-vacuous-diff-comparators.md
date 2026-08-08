---
name: baseline-relative-toolchain-gates-and-vacuous-diff-comparators
description: two recurring plan-authoring defects — asserting absolute exit 0 for solution-wide CMD-ANALYZE/CMD-NULLABLE, and using `<merge-base>..HEAD` comparators in a plan that never commits
metadata:
  type: feedback
---

Two defects that recur in C# plans for this repo and were both blocking findings in #454 preflight.

**1. Do not assert absolute `exit 0` for CMD-ANALYZE / CMD-NULLABLE.** Both msbuild switches
(`/p:EnableNETAnalyzers`, `/p:Nullable=enable /p:TreatWarningsAsErrors=true`) apply SOLUTION-WIDE, and a
repo-wide nullable remediation epic is in flight, so the pre-change exit code is not guaranteed to be 0.
Write the gate as: `EXIT_CODE` equals a NAMED Phase 0 baseline exit code (e.g. `ANALYZE_BASELINE_EXIT`,
`NULLABLE_BASELINE_EXIT`) AND the diagnostic set scoped to the feature's touched files is empty. Add an
explicit Phase 0 task that records those named values, and state the convention once in the Command
Reference so each later task can cite it in one clause. CMD-FORMAT is NOT baseline-relative — `csharpier
check .` must exit 0 unconditionally.

**Why:** for an epic child, the per-child gate is scoped to its own branch while cross-child CS86xx
fan-in accumulates on the integration branch. An absolute exit-0 gate makes the child responsible for
sibling debt it cannot fix. Also watch for the self-contradiction of asserting BOTH "exit 0" AND "no new
diagnostics relative to baseline" — redundant if the baseline is 0, mutually exclusive if it is not.

**2. `<merge-base>..HEAD` is vacuous in a plan that schedules no commit.** Two-dot comparators read
committed history only. Most plans here commit once at the end (epic flow), so every mid-plan diff
verification passes trivially on an empty diff and proves nothing. Use the single-dot working-tree form
`git diff --exit-code <merge-base> -- <path>` / `git diff --stat <merge-base>`, which compares the
merge-base against the files on disk right now. Prefer this over inserting per-phase commit tasks.

**How to apply:** sweep every plan for `..HEAD` and for `exit 0` next to CMD-ANALYZE/CMD-NULLABLE before
handing off to preflight. When adding the baseline-exit-code task in a revision loop, append it at the
END of Phase 0 rather than inserting it next to the analyze/nullable tasks — the plan validator requires
sequential-by-appearance IDs, and mid-phase insertion forces renumbering plus cross-ref updates
([[plan-validator-task-id-sequential-constraint]]).
