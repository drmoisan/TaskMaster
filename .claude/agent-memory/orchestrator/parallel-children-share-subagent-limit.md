---
name: parallel-children-share-subagent-limit
description: Epic children running in parallel share one global concurrent-subagent pool, so batched Agent launches partially fail with "Concurrent subagent limit reached"
metadata:
  type: feedback
---

When running as an epic child alongside parallel siblings, the concurrent-subagent cap
(`CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS`, default 20) is **global across all sibling children**, not
per-orchestrator. A single message containing four `Agent` calls can return three
`Concurrent subagent limit reached. Do not retry.` errors and one success.

**Why:** the cap is a session-wide resource. During epic preparation, a dozen sibling orchestrators
are each fanning out research agents, so the pool is usually near-saturated.

**How to apply:**
- Note *which* call in the batch succeeded — the errors are returned positionally, and it is easy to
  misattribute the surviving `agentId` to the wrong prompt. On a 4-call batch where calls 1-3 failed,
  the running agent is call **4**, not call 1.
- Re-issue the rejected launches later rather than abandoning them; capacity frees as siblings
  finish. Waiting on a completion notification and then relaunching works.
- Do not collapse the delegations into fewer, larger agents to dodge the limit — issue #136-style
  mandates ("one research artifact per production file") still require the full artifact set, and a
  single agent covering ten files produces materially shallower per-file findings.
- Bash loops that poll for a file are rejected by the worktree-isolation guard as "too complex";
  use a bare `perl -e 'select(undef,undef,undef,N)'` sleep plus a separate simple `ls` check.

Related: [[parallel-preparation-children-shared-worktree]],
[[parallel-epic-children-conflict-on-agent-memory-index]]
