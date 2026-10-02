---
name: project-956-r1-spec-self-hit-and-raw-doc-name-seams
description: #956 preflight round 1 - a spec-wording count gate also hits the AC line that quotes the wording; a bare "cobertura" substring gate matches tracked agent-memory Markdown; an AC amended by the orchestrator needs a P0 literal check and a PD sentence
metadata:
  type: project
---

Three seams from #956 preflight round 1 (2026-10-01):

1. A fixed-string count of spec wording (`pre-existing, unchanged exclusion`) must count every line that carries it, including the AC criterion that restates the wording (`exclusions` contains the literal). Count over the whole file, enumerate line numbers.
2. A raw-document path gate written as `-match "cobertura"` matches tracked `.claude/agent-memory/*cobertura*.md` files (Clause B subtraction does not apply to that line). Anchor to the extension in the last segment: `cobertura[^/]*\.xml$`.
3. When the orchestrator amends an AC to state a planner measurement exemption, add the amended literal to the P0 AC-text precondition and say in the PD that a check-off under the rule satisfies the AC as written; sweep every AC restatement (check-off task, AC-MAPPING, spec CITATION).

**Why:** each surfaced as a preflight REVISIONS REQUIRED defect.
**How to apply:** when authoring spec-wording counts or path-name gates, Grep the spec and `git ls-files` names for self-hits before fixing the expected count. Related: [[zero-hit-grep-gates-need-carveouts]], [[agent-memory-is-tracked-scope-git-gates]].
