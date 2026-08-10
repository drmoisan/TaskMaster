---
name: full-bug-spec-only
description: In full-bug work mode author spec.md ONLY — do not create user-story.md, even though this agent's own Expected Outputs header lists it
metadata:
  type: feedback
---

When `issue.md` carries `- Work Mode: full-bug`, produce `spec.md` only and do not create `user-story.md`.

**Why:** `.claude/skills/acceptance-criteria-tracking/SKILL.md` resolves the authoritative AC source by work mode: `full-bug` -> `spec.md` only; `full-feature` -> `spec.md` + `user-story.md`. The prd-feature agent's static "Expected Outputs" header unconditionally lists both files, so the header conflicts with the skill on every bug. Creating a second AC source in `full-bug` mode splits the acceptance criteria and breaks the executor/reviewer check-off protocol, which reads exactly one file.

**How to apply:** Read the `- Work Mode:` marker in `issue.md` before writing anything. On `full-bug`, state in the spec header that it is the sole authoritative AC source and that no `user-story.md` exists, then report only `spec-path:` back to the caller. Related: [[ac-gates-verify-satisfiability]], [[promotion-scaffold-metadata-defects]].

**Known exception (agent-directed, not user-ratified):** on 2026-08-10 the epic-orchestrator for `build-ci-coverage-gate-fidelity` / issue #457 required `user-story.md` because the epic preparation deliverables list names it per child feature. It was created with an explicit note that it carries no acceptance criteria and no checkbox items, and `spec.md` carries a reciprocal note naming itself the sole AC source. If an epic forces the file again, use that same containment: zero checkboxes in `user-story.md`, cross-references in both files.
