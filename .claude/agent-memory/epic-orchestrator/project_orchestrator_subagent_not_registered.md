---
name: orchestrator-subagent-not-registered
description: Runtime availability of 'orchestrator'/'pr-author' subagent types is a MOVING TARGET — 'orchestrator' was absent 2026-07-10T06:08Z but PRESENT and launchable 2026-07-10T11:33Z; 'pr-author' remained absent (use pr-author SKILL inline). Always verify per run.
metadata:
  type: project
---

The `orchestrator` subagent's runtime availability to `epic-orchestrator` is a moving target the maintainer actively manages. It has flipped within a single day.

- **2026-07-10T06:08Z / 06:30Z (epic #295):** `Agent(subagent_type="orchestrator")` returned `Agent type 'orchestrator' not found. Available agents: atomic-executor, atomic-planner, commit-message, csharp-typed-engineer, epic-orchestrator, epic-review, feature-review, human-exception-runbook, powershell-typed-engineer, prd-feature, python-typed-engineer, staged-review, status-updater, task-researcher, typescript-engineer.` `pr-author` also absent.
- **2026-07-10T11:33Z (same epic #295, later session):** the maintainer reported the block resolved; the re-attempted `Agent(subagent_type="orchestrator", model="opus")` spawn SUCCEEDED. `orchestrator` now appears in the launchable set. `pr-author` was STILL absent.

**pr-author fallback (stable so far):** `pr-author` has been absent every time checked. When `Agent(pr-author)` is unavailable, apply the pr-author SKILL inline (`.claude/skills/pr-author/SKILL.md`) — refresh context via `mcp__drm-copilot__collect_pr_context`, author the body file + SHA-256 provenance receipt, then `gh pr create --body-file`. This satisfies `enforce-pr-author-skill.ps1` and is how child PRs #299/#300/#301 were authored.

**Why it matters:** Combined with the prohibition on inline child execution ([[inline-child-lifecycle-prohibited]]), `orchestrator` availability gates wave-N child completion. When present, delegate the full child lifecycle to `Agent(orchestrator)`.

**How to apply:** VERIFY by attempting the spawn on each run; never pre-emptively assume the outcome in either direction. Record a block only after a real verbatim failure; record the success and clear the block when it works. Keep this memory's dates current.
