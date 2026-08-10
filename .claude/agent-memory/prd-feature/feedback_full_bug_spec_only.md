---
name: full-bug-spec-only
description: In full-bug work mode spec.md is the ONLY AC source — default to not creating user-story.md; if a caller explicitly demands one, make it checkbox-free narrative
metadata:
  type: feedback
---

When `issue.md` carries `- Work Mode: full-bug`, `spec.md` is the sole authoritative acceptance-criteria source. By default produce `spec.md` only and do not create `user-story.md`.

**Why:** `.claude/skills/acceptance-criteria-tracking/SKILL.md` resolves the authoritative AC source by work mode: `full-bug` -> `spec.md` only; `full-feature` -> `spec.md` + `user-story.md`. The prd-feature agent's static "Expected Outputs" header unconditionally lists both files, so the header conflicts with the skill on every bug. Creating a second file that carries `- [ ]` items in `full-bug` mode splits the acceptance criteria and breaks the executor/reviewer check-off protocol, which reads exactly one file.

**How to apply:** Read the `- Work Mode:` marker in `issue.md` before writing anything. On `full-bug`, state in the spec header that it is the sole authoritative AC source, then report only `spec-path:` back to the caller.

**Exception (observed on `cobertura-coverage-arithmetic-441`, 2026-08-10):** an orchestrator may explicitly delegate a `user-story.md` in `full-bug` mode *and* instruct "do not restate the acceptance criteria; cross-reference `spec.md`". That instruction removes the actual hazard, because the hazard is a second file containing markdown checkboxes, not the filename. Comply, but harden it: the file must contain **zero** `- [ ]` items and must carry an explicit banner stating it is narrative context only and that `spec.md` is the sole AC source. Report the deviation from this memory in the final message. Related: [[ac-gates-verify-satisfiability]], [[promotion-scaffold-metadata-defects]].
