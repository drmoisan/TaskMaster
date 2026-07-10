---
name: feedback-verify-subagent-capability-claims
description: Never relay or act on a subagent's "agent type not registered / tool unavailable" claim without verifying against .claude/agents definitions; require verbatim error evidence and blocked-state reporting instead of silent fallback
metadata:
  type: feedback
---

When a delegated agent reports that a required delegation target or tool is
unavailable (e.g., "Agent(orchestrator) is not registered in this session"),
do NOT accept, relay, or build directives on that claim without verification:

1. Check the agent definition (`.claude/agents/<name>.md` frontmatter `tools:`)
   for what the agent is actually granted.
2. Require the agent to produce the verbatim spawn/tool error as evidence.
3. If a required handoff genuinely cannot start, the correct response is a
   recorded BLOCKED state surfaced to the maintainer — never a silent local
   fallback that abbreviates the lifecycle.

**Why:** During epic #295 (2026-07-10) the epic-orchestrator claimed
`Agent(orchestrator)` was unregistered and fell back to running children
itself; I repeated the claim to the maintainer and embedded it in a corrective
directive. The maintainer checked `.claude/agents/epic-orchestrator.md`, which
explicitly grants `Agent(orchestrator)` and `Agent(pr-author)`, and called the
statement inaccurate. Related earlier pattern: a worker claiming MCP tools
unavailable when the orchestrator could run them
([[mcp-tools-available-to-orchestrator]]).

**How to apply:** On any capability-unavailability claim: verify the definition
file, demand the verbatim error, and if confirmed, record blocked state rather
than endorsing a fallback. When kicking off epic-orchestrator, state explicitly
that children MUST be delegated via `Agent(orchestrator, isolation: worktree)`
and PRs via `Agent(pr-author)`, and that a failed spawn is a blocking,
maintainer-visible condition, not a license to run the work inline. See
[[feedback-epic-children-require-full-lifecycle-and-prs]].

**Resolution (2026-07-10, epic #295):** The verbatim spawn attempt was made and
FAILED: `Agent type 'orchestrator' not found. Available agents: atomic-executor,
atomic-planner, commit-message, ...` — so in THIS runtime the claim was
substantiated: `orchestrator` is defined in `.claude/agents/orchestrator.md` but
excluded from the launchable subagent set (it is the main-thread persona), and
`pr-author` has no agent definition at all (it is a skill, run in-thread with a
body+SHA-256 receipt; the hook permits gh pr create when the receipt verifies).
Definition-grants in epic-orchestrator frontmatter do NOT prove runtime
launchability. The process lesson stands: the verbatim-attempt-then-block
protocol is what converts an assertion into evidence a maintainer can act on.
