---
name: full-bug-spec-only
description: In full-bug work mode spec.md is the ONLY AC source — default to not creating user-story.md even though this agent's Expected Outputs header lists it; two documented exceptions (epic preparation route, explicit cross-reference instruction)
metadata:
  type: feedback
---

When `issue.md` carries `- Work Mode: full-bug`, `spec.md` is the sole authoritative acceptance-criteria source. By default produce `spec.md` only and do not create `user-story.md`.

**Why:** `.claude/skills/acceptance-criteria-tracking/SKILL.md` resolves the authoritative AC source by work mode: `full-bug` -> `spec.md` only; `full-feature` -> `spec.md` + `user-story.md`. The prd-feature agent's static "Expected Outputs" header unconditionally lists both files, so the header conflicts with the skill on every bug. Creating a second file that carries `- [ ]` items in `full-bug` mode splits the acceptance criteria and breaks the executor/reviewer check-off protocol, which reads exactly one file.

**How to apply:** Read the `- Work Mode:` marker in `issue.md` before writing anything. On `full-bug`, state in the spec header that it is the sole authoritative AC source, then report only `spec-path:` back to the caller. Related: [[ac-gates-verify-satisfiability]], [[promotion-scaffold-metadata-defects]].

## Exceptions

The hazard this rule guards against is **a second file containing markdown checkboxes**, not the filename itself. Both exceptions below were observed on 2026-08-10 during the `build-ci-coverage-gate-fidelity` epic, and both are handled by removing checkboxes rather than by refusing the file.

**Exception 1 — epic preparation route (seen on #494).** When a feature is prepared under an epic, the caller may require `user-story.md` as a route deliverable even in `full-bug` mode. Comply, but neutralize the AC-splitting hazard:

1. Put the AC checkboxes in `spec.md`, reproduced **verbatim** from `issue.md` with identical numbering, under a header stating they are the same criteria and not additional ones, and that check-off must be mirrored in both files. This satisfies the AC-tracking skill (which reads `spec.md` for `full-bug`) without inventing competing criteria.
2. Author `user-story.md` with **zero checkboxes**. Use a plain "Outcomes" bullet list instead of an "Acceptance Criteria" section, even though archived `user-story.md` files in this repo do use AC checkboxes.
3. Put a blockquote near the top of `user-story.md` stating it exists only because the preparation route requires it, that the AC authority is `issue.md` + `spec.md`, and that it must not be used as an AC source.

**Exception 2 — explicit cross-reference instruction (seen on #441).** An orchestrator may delegate a `user-story.md` in `full-bug` mode *and* instruct "do not restate the acceptance criteria; cross-reference `spec.md`". That instruction removes the actual hazard directly. Comply, but harden it: the file must contain **zero** `- [ ]` items and must carry an explicit banner stating it is narrative context only and that `spec.md` is the sole AC source. Report the deviation from this memory in the final message.
