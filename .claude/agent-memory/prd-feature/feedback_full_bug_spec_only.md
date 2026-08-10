---
name: full-bug-spec-only
description: In full-bug work mode author spec.md ONLY — do not create user-story.md, even though this agent's own Expected Outputs header lists it
metadata:
  type: feedback
---

When `issue.md` carries `- Work Mode: full-bug`, produce `spec.md` only and do not create `user-story.md`.

**Why:** `.claude/skills/acceptance-criteria-tracking/SKILL.md` resolves the authoritative AC source by work mode: `full-bug` -> `spec.md` only; `full-feature` -> `spec.md` + `user-story.md`. The prd-feature agent's static "Expected Outputs" header unconditionally lists both files, so the header conflicts with the skill on every bug. Creating a second AC source in `full-bug` mode splits the acceptance criteria and breaks the executor/reviewer check-off protocol, which reads exactly one file.

**How to apply:** Read the `- Work Mode:` marker in `issue.md` before writing anything. On `full-bug`, state in the spec header that it is the sole authoritative AC source and that no `user-story.md` exists, then report only `spec-path:` back to the caller. Related: [[ac-gates-verify-satisfiability]], [[promotion-scaffold-metadata-defects]].

**Exception — an explicit caller request (seen 2026-08-10, epic-child #512).** When the delegating agent explicitly asks for `user-story.md` in `full-bug` mode for audience context, produce it, but keep the AC surface single: put a note at the top declaring `spec.md` the sole AC source, and use a non-checkbox heading (for example `## Outcomes (non-authoritative)`) instead of `## Acceptance Criteria`, so no checkbox list exists for a tracker to pick up. The rule being protected is one AC source file, not the absence of the file.
