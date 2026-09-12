---
name: subagent-limit-shared-across-epic-children
description: The 20-concurrent-subagent cap is session-wide, so parallel epic children compete for it — plan delegation fan-out small and expect to retry launches
metadata:
  type: project
---

The concurrent-subagent cap (20, `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS`) is a
**session-wide** pool, not a per-agent budget. When `epic-planner` runs up to 8
preparation children concurrently and each child fans out to several researchers,
the pool saturates and `Agent(...)` returns `Concurrent subagent limit reached.`

**Why:** observed on epic #136 (`quickfiler-per-file-coverage`) child F4. A 4-way
research fan-out launched 2 agents; the other 2 were rejected repeatedly over many
minutes while sibling children held slots. The tool result says "Do not retry", but
the launch DOES succeed once capacity frees — the instruction means "not immediately",
not "never".

**How to apply:**
- Keep per-child delegation fan-out to 2-4 agents; prefer sequencing over breadth.
- When a launch is rejected, wait and relaunch rather than degrading to doing the work
  in-thread (that would violate the mandatory-delegation rule).
- A held slot is proof the agent is alive: if `Agent(...)` keeps reporting the limit
  while your own agents show no output, they are running, not dead. Do not declare them
  blocked.
- Give later-launched agents a pointer to artifacts the earlier ones already produced
  (e.g. a cluster-overview research file) so they reuse findings instead of re-deriving
  them.
