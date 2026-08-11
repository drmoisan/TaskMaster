---
name: mcp-tools-available-to-orchestrator
description: The drm-copilot MCP gate and promotion lifecycle run from the orchestrator; if a spawned worker reports MCP tools unavailable, run them yourself rather than accepting the block
metadata:
  type: feedback
---

When a delegated worker (e.g. powershell-typed-engineer) reports that the `mcp__drm-copilot__*` tools are unavailable in its session and therefore blocks on the QA gate or promotion lifecycle, do not accept that as a terminal block. The orchestrator main thread has these tools. Run the canonical gate (`run_poshqc_format/analyze/test`) and the promotion lifecycle (`new_potential_bug_entry`, `potential_to_issue`, `new_active_feature_folder`) yourself, then re-delegate implementation with the lifecycle already established.

**Why:** On 2026-05-27, a powershell-typed-engineer worker correctly stopped rather than implement without lifecycle, citing missing MCP tools. The orchestrator verified the same tools worked from the main thread (format/analyze/test ok; full promotion ran) and completed the work. A self-reinforcing false worker memory had been telling each spawned worker the gate was unrunnable; see the worker-side correction [[mcp-gate-is-runnable]].

**Confirmed again 2026-08-10 (#441, epic child).** `atomic-executor`'s MCP surface is exactly the four PoshQC tools (`run_poshqc_format/analyze/test/analyze_autofix`) — the promotion tools are genuinely absent from it, so a worker reporting this is telling the truth about its own session, not making an excuse. The #441 plan encoded a sanctioned `POSTING BLOCKED` branch for its Phase 6 follow-up filing, and the executor correctly took it, checked off P6-T1..T4 on the recorded blockage, and left AC-20 unchecked rather than fabricating issue numbers. **Do not accept that as the end state.** Resolve the branch from the orchestrator: file each candidate yourself (`new_potential_bug_entry` -> author the body from the worker's prepared verbatim text -> `potential_to_issue`), append a `RESOLUTION` section to the worker's evidence artifact rather than rewriting its honest `POSTING BLOCKED` record, then check off the AC. A plan's blocked-branch is a handoff to a more privileged session, not a permanent deferral.

**How to apply:** Promotion and the canonical QA gate are orchestrator-owned regardless. On a worker "MCP unavailable" report: (1) confirm the tools work from the main thread with one call, (2) establish the lifecycle yourself and persist the promotion receipts under `delegation_receipts.promotion.*`, (3) have the worker implement + write tests + run local self-checks, (4) run the canonical MCP gate from the orchestrator after delivery. Do not let an unverifiable worker session claim downgrade a deliverable to UNVERIFIED.
