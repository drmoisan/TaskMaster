---
name: one-executor-per-worktree
description: Never launch a second executor against a worktree that already has one running; a stale checkpoint is NOT evidence of a dead delegation, and the cure is re-verifying the committed tree
metadata:
  type: feedback
---

Do not launch a second `atomic-executor` (or any second writing agent) against a worktree that
already has one in flight. If you suspect a delegation died, prove it before acting.

**Why:** on the #505/#506/#518 delivery a second executor was launched against
`.claude/worktrees/agent-a406ae4b7a2ce151f` while the original was mid-Phase-5. The stated reason was
"the S8 delegation terminated without returning; the checkpoint was never advanced" — a **false
premise**. An `atomic-executor` does not own `artifacts/orchestration/orchestrator-state.json`; only
the orchestrator that launched it advances the checkpoint. A stale checkpoint is therefore the
*expected* state during a long execution, not a death certificate. The two executors interleaved
writes into the same feature folder and the same coverage output for ~4 minutes, producing two
parallel evidence series and a plan checkbox state neither agent fully owned.

**How to tell a live delegation from a dead one** (in order of strength):
1. Live processes: `Get-Process MSBuild, vstest.console, testhost, dotnet-coverage` with recent
   `StartTime`. Fresh test processes mean the executor is alive.
2. File mtimes advancing under the feature folder.
3. Interleaved timestamp series in `evidence/qa-gates/` is the *signature of the incident*, not of
   progress — two series minutes apart with different task coverage means two writers.

**How to recover** (this worked and is the pattern to reuse): do not kill processes, delete
artifacts, stash, or commit from the duplicate. Halt it, let the incumbent finish, then **re-verify
the committed tree yourself** rather than adjudicating whose artifact is whose. Running
`csharpier check` + analyzer `/t:Rebuild` + type-check + per-assembly tests against the final commit
reproduced every recorded figure exactly (18 `csc.exe`, 0 errors, 6435 passed), which made the
provenance question moot for delivery. Record the incident in the checkpoint honestly instead of
erasing it; also keep the superseded evidence series, explicitly marked superseded.

Related: [[preflight-catches-vacuous-gates]], [[vstest-aggregate-crash-isolate-per-assembly]],
[[feedback_reverify_ground_truth_after_user_midcycle_commit]].
