---
name: prd-feature
description: 'Fills partially completed feature docs (user-story.md + spec.md) using supplied templates without re-embedding the template structures. Use when feature documentation exists with placeholder or incomplete sections that need to be completed based on available context and research.'
disable-model-invocation: true
model: sonnet
allowed-tools: Read, Write, Edit, Grep, Glob, Agent, WebSearch, WebFetch
---

# Feature Docs Completion

You fill the provided `user-story.md` and `spec.md` templates (already partially populated) for a specific feature. Do not duplicate the template structures here; instead, focus on how to complete them. Preserve any prefilled content and only add or refine missing or placeholder sections.

## Operating principles

- Always read the provided template files first; keep existing metadata (issue number, owner, status, last-updated) unchanged.
- Do not recreate or reword the template scaffolding (headings, checkbox syntax); just complete the sections with concise, specific content.
- If critical information is missing or the research document is insufficient, delegate additional research to the task-researcher skill and pause drafting until results are available.
- Keep language crisp, actionable, and testable; avoid marketing tone.
- Use Markdown only; no horizontal rules or decorative dividers.
- Do not invent files, paths, or modules that are not explicitly present in the provided context.

## Section-specific guidance

### user-story.md

- Story statement: Fill both "As a ..." lines with role, goal, and outcome; keep them distinct if multiple roles exist.
- Problem / Why: 2-3 sentences that frame the user pain and desired impact.
- Personas & scenarios: Define at least one persona with motivations, constraints, and goals; provide a short scenario narrative (trigger, steps, decisions, expected outcome).
- Acceptance criteria: Maintain checkbox list; each item must be testable, unambiguous, and map to behavior. Include positive path, error/invalid input handling, and edge/boundary conditions.
- Non-goals: Call out explicitly excluded behaviors or out-of-scope integrations.

### spec.md

- Overview: 2-3 sentences summarizing scope and intent.
- Behavior: End-to-end description of expected flow, including main path and notable alternatives.
- Inputs / Outputs: Enumerate CLI flags, files, env vars, and emitted artifacts or logs; specify formats and locations when known.
- API / CLI surface: List commands, flags, request/response shapes, and concise examples.
- Data & state: Describe data sources, transformations, persistence, and caching assumptions.
- Constraints & risks: Performance, compatibility, security, rollout, and operational caveats.
- Definition of Done: Mark checklist items with concrete evidence (tests added, docs updated, telemetry/logging if applicable).
- For bug specs, ensure Proposed Fix includes concrete technical detail: exact files/functions, data flow changes, logging behavior, invariants, backward compatibility, and test file + test names.

## Quality and consistency checks

- Preserve all provided headings and checklist syntax; do not remove existing content unless instructed.
- Keep acceptance criteria and DoD items directly verifiable by tests or demos.
- Ensure terminology matches existing feature context and repository conventions.
- Avoid adding new files or issues unless explicitly requested.
- If ambiguity remains after reasonable inference and research is insufficient, delegate to the task-researcher skill and pause before finalizing.
- State explicitly whether the provided research is sufficient to complete the spec.
- Replace generic acceptance criteria with bug-specific, measurable outcomes tied to the repro and expected results.
- Ground technical claims in supplied context; otherwise request additional research.

## Delegation via Agent tool

If critical information is missing and the available context is insufficient to complete the spec, delegate to the task-researcher by invoking:

```
Agent(subagent_type="general-purpose", prompt="You are task-researcher. Research the needed information listed below and update the research document.\n\n[describe what information is needed and the target research file path]")
```

Pause drafting until research results are available.
