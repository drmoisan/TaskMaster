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

**The commonest way to cause this accidentally: there is no `SendMessage` tool.** The Agent tool's own
description tells you to "use SendMessage with the agent's ID to continue a previously spawned agent",
but `SendMessage` is NOT in the orchestrator's function list in this repo. Reaching for it to send a
mid-flight correction to a running executor and falling back to `Agent(...)` **launches a second
executor into the live worktree**. Confirmed 2026-08-27 on epic child #501: an `Agent(atomic-executor)`
call carrying the literal prompt `SendMessage placeholder - not used` started a real second agent while
the incumbent was in Phase 0.

**Consequence: you cannot correct a running executor at all.** Front-load every correction into the
INITIAL delegation prompt. On #501 the plan's Phase 9 required `git commit` and `gh issue create`,
while the delegation prompt had forbidden both — and that contradiction could no longer be repaired,
so the orchestrator had to complete those four tasks itself afterwards. Read the plan's LAST phase
before writing the delegation prompt, not after.

**Reassuring finding:** a second `atomic-executor` reliably self-detects and blocks harmlessly. On
#501 it sampled the plan sha256 / `[x]` count / evidence-file count four times, observed
`.dotnet-sdk/` growing to 764 MB and `packages/` appearing between samples, concluded "a single
synchronous agent turn cannot write files in the background", returned `BLOCKED at preflight`, and
wrote nothing but one agent-memory note. It also explicitly declined `git stash push -u`/`pop` as a
remedy, because that three-way-merges the live writer's in-flight edits back into the popped files.
Let the incumbent run; the duplicate costs a few minutes of reads, not a corrupted tree.

Related: [[preflight-catches-vacuous-gates]], [[vstest-aggregate-crash-isolate-per-assembly]],
[[feedback_reverify_ground_truth_after_user_midcycle_commit]].
