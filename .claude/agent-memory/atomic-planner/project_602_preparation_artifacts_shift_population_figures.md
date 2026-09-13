---
name: project-602-preparation-artifacts-shift-population-figures
description: "#602 R4 (host-identifier sweep): expected population figures measured BEFORE the item's own preparation artifacts are committed go stale by exactly the artifact count; record orchestrator-supplied figures as supplied when the planner has no shell; new-finding prose about XML corruption must carry no angle bracket, no backtick, and no attribute-shaped text"
metadata:
  type: project
---

Issue 602 round-4 reconciliation (2026-09-12): a plan that enumerates tracked-file and per-class Markdown counts as Phase 0 expectations was measured by the predecessor before the item's own feature documents (plan, issue, spec, user-story, research = 5 under the active features tree) and one recovered agent-memory note were committed. Committing them shifted exactly three figures (tracked total +6, active-features Markdown +5, agent-memory Markdown +1) and nothing else; every identifier-bearing population was unchanged because the preparation artifacts are clean.

**Why:** the tracked-file total and any per-directory enumeration that includes the item's own folder are self-referential: the plan's own commit moves them. The spec, issue and research artifact keep the older figures, so the plan must say in prose why it disagrees with them rather than editing the spec.

**How to apply:**
- When a Phase 0 table includes a tracked-file total or a per-class enumeration covering the feature folder or agent-memory, state in the preamble that the figures include the item's own committed preparation artifacts and name the count, so a preflight reviewer measuring a different tree can reconcile.
- With no shell tool, record caller-supplied figures explicitly as "supplied, not measured" in the SELF-REVIEW enumeration; do not claim a measurement.
- Recording a finding that earlier redaction wrote placeholder tokens INTO XML attribute values: describe the tokens and attributes in prose only (no angle-bracket characters, no attribute-shaped text, no backticks), because the plan's own XML invariant and the blast-radius backtick harvest both apply to the paragraph that documents them.
- Under a "do not add or remove backticks" constraint, carried-forward SELF-REVIEW enumeration lines that contain a backticked token must stay verbatim; move backtick-free lines only.
- Detect CR bytes with a Grep for a carriage return (0 hits = LF-only); the Edit tool preserved LF here.
