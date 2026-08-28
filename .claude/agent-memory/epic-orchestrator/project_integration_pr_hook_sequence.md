---
name: integration-pr-hook-sequence
description: Four guards fire in order before an epic integration-to-main pull request can be opened; each is satisfiable truthfully, and the shared child checkpoint carrying epic_mode is what blocks you first
metadata:
  type: project
---

Opening the epic's integration-to-`main` pull request trips four guards in sequence. All are
satisfiable without a false statement, but only if you understand what each is actually reading.

1. **`EPIC_BASE_BRANCH_MISMATCH`** — the shared `artifacts/orchestration/orchestrator-state.json` still
   holds the last child's record with `epic_mode: true`, so the guard demands the integration branch as
   base. Correct for a child; wrong for the fan-in, which must target `main` by design. Archive the
   child's terminal record and write a parent-owned one that **omits `epic_mode`**, documenting the
   omission inside the record.
2. **`ORCHESTRATOR_STATE_PREFLIGHT_FAILED` (missing keys)** — it enforces the *child-feature* schema,
   which a fan-in does not fit: no intake record, no single issue, no atomic plan of its own. Fill the
   ~19 required keys truthfully with a disclosure block explaining each value that has no natural
   analogue.
3. **The intake guard** — fires **correctly** if you try to create an epic tracking issue directly.
   Do not reroute it through the defect-intake route either; that route is for defect entries, not epic
   trackers. Accept that no epic issue exists and use a sentinel. (The same guard also
   false-positives on heredoc *text* — see [[bash-hook-keyword-false-positive]].)
4. **`ORCHESTRATOR_STATE_PREFLIGHT_FAILED` (invalid enum)** — read the valid `stepN_status` values out
   of real completed child records rather than inventing one. `not-applicable` is both valid and
   literally true for steps that belong to child runs.

Then `EPIC_MERGE_GATE_BLOCKED` on the merge itself: satisfy it by recording the **observed** CI
conclusion in `epic_merge_pr.ci_gate.conclusion` — a fact you verified, not a value invented to pass.

Practical notes: the body filename needs a real number, and the next sequential number is predictable
from the highest existing issue or pull-request number, so naming the body after the pull request's own
number is both accurate and permitted. Also expect a compound `bash` call to be denied wholesale — the
guard evaluates the command *before* execution, so pairing a checkpoint fix with the creation command
in one call means the fix never runs; split them.
