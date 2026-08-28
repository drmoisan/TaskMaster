---
name: agent-worktree-hooks-resolve-to-agent-cwd
description: When the orchestrator runs in a .claude/worktrees/agent-<id> isolated worktree, PreToolUse + SubagentStop hooks resolve relative paths against the agent worktree, not the session root
metadata:
  type: project
---

When the child orchestrator runs inside an Agent-tool isolated worktree at `.claude/worktrees/agent-<id>/` (the env "Working directory"), the `gh`/checkpoint enforcement hooks resolve their relative paths against THAT agent worktree, not the epic session root.

Evidence (epic child #324, folder-probability-plumbing, 2026-07-16): `gh pr create` and `gh pr merge --merge` both succeeded reading only the agent-worktree `artifacts/orchestration/orchestrator-state.json` + `artifacts/pr_body_<N>.md` + receipt. The merge is decisive: the child-path epic-merge-gate allows only when it reads `orchestrator-state.json` with `epic_mode==true` and `step9_status=="passed"`; that file existed ONLY in the agent worktree (the session root held only `epic-orchestrator-state.json`), yet the merge was allowed — so the hook read the agent worktree. The SubagentStop `validate-orchestrator-output.ps1` therefore also reads the agent-worktree checkpoint.

**Why:** an Agent-tool isolated worktree makes the agent's own cwd == the worktree, and hooks inherit that cwd. This differs from [[child-orchestrator-pr-hook-reads-session-root]], which applied when the session cwd was a *separately-created named* worktree distinct from the feature worktree (session cwd != feature worktree). Distinguish the two topologies before deciding whether to stage artifacts in the session root.

**How to apply:** in an Agent-tool isolated worktree, author the PR body/receipt and keep the child checkpoint in the agent worktree's `artifacts/` and do NOT copy them to the session root — copying `orchestrator-state.json` to the shared session root can clobber a concurrent sibling child's merge-gate checkpoint. Run `collect_pr_context` with `workspace_root` = the agent worktree, but note (re-verified 2026-07-18, child #349/PR #355): even with that arg it WRITES `pr_context.*` to the SESSION ROOT while its JSON response claims agent-worktree paths; the head/base/merge-base refs are still resolved correctly by branch name. Copy `pr_context.summary.txt`/`.appendix.txt` into the agent worktree's `artifacts/` before authoring the receipt (whose `context_summary_path` points at the worktree copy). Other MCP tools (`resolve_execute_hard_lock_prompt`, `validate_orchestration_artifacts`) DO honor `workspace_root`; hard-lock target paths must be absolute or resolution fails. If a future run shows the hook reading the session root (e.g. `PR_CONTEXT_MISSING`/`ORCHESTRATOR_STATE_PREFLIGHT_FAILED` despite valid agent-worktree artifacts), only then stage into the session root.

**Correction (feature 488, 2026-08-28).** Do not infer "the hook read my worktree" from a PR that
merely succeeded. On 488 the session cwd was the epic session root (NOT an isolated agent worktree),
the child checkpoint existed only in the feature worktree, and PR creation still passed. The reason was
not worktree resolution: the session root held merged sibling 489's *completed* checkpoint, which is
itself pr-creation-ready and carries the same `epic_context.integration_branch`, so it satisfied the
gate on 489's record. The prior run recorded that pass as proof the hook resolved to the agent
worktree; that was a misattribution. The merge gate exposed it, because it demands
`step9_status == "passed"` exactly and 489's said `verified`.

**How to apply:** determine the topology from the env "Working directory", not from a hook outcome. A
gate that passes on a *sibling's* leftover checkpoint is a false green. Before relying on a session-root
pass, read that file's `issue-num` and `branch_name` and confirm they are yours; if they are not, stage
your own checkpoint (archiving the occupant under a disclosed name, and checking for an existing
`orchestrator-state.<issue>-master.json` first) so the gate evaluates your record.

Also confirmed this run: on an epic integration-based branch the Python validator (`scripts/dev_tools/*.py`) is absent, so `Test-PythonOrchestratorValidatorAvailable` returns false and the portable `OrchestratorStateCompletion.psm1` gate is authoritative at SubagentStop (base presence + model-routing existence only). The bundled MCP `require_complete` check is much stricter (full large-route pr_gate/ci_gate + promotion/research/planning receipts) and will FAIL for a prepared-epic child that resumes at execution — that divergence is expected and is NOT the real Stop gate. See [[orchestrator-state-validator-divergence]].
