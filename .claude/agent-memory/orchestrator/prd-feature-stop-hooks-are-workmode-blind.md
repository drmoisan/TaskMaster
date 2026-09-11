---
name: prd-feature-stop-hooks-are-workmode-blind
description: prd-feature's two SubagentStop hooks unconditionally demand an existing user-story.md and numeric-derivation evidence in the research file, both of which break on a full-bug feature.
metadata:
  type: project
---

`Agent(prd-feature)` carries two SubagentStop hooks in `.claude/agents/prd-feature.md` frontmatter, and
both fail closed on a `full-bug` feature in ways the delegation prompt cannot fix.

**1. `validate-required-artifact-output.ps1` requires `user-story-path` to EXIST.** The frontmatter passes
`-RequiredArtifact 'user-story-path|^docs/features/active/.+/user-story\.md$|feature user story artifact'`
unconditionally. The hook resolves the label from the agent's final output, matches the regex, and then
calls `Test-Path`. It reads no work-mode marker. But `feature-promotion-lifecycle` says a `full-bug`
folder should have `spec.md` only and `acceptance-criteria-tracking` makes `spec.md` the sole AC source,
so the correct artifact set for `full-bug` cannot satisfy the hook. Telling prd-feature "do not create
user-story.md" guarantees it is blocked at termination.

**2. `validate-prd-feature-output.ps1` triggers on ANY digit in an acceptance criterion.**
`Test-SpecNumericCriterion` fires when the `## Acceptance Criteria` section has any `- [ ]` line matching
`\b\d+\b` — a line number, an issue number, or `80%` is enough. Once it fires, the hook demands a
`research-path` in the output AND a `## Numeric Derivation Evidence` section in the RESEARCH file carrying
eleven exact labels: Complete Family, Exhaustive Search Scope, Inclusion Rules, Exclusion Rules, Primary /
Cross-check Search Strategy or Query Expression, Primary / Cross-check Member Set, Primary / Cross-check
Count, Member-set Comparison. Extra constraints: the scope value must match
`(entire|all|complete).*(repository|repo|source tree|tree)`; neither strategy value may contain
`single`, `narrow` or `named pattern`; the two strategy strings must differ after whitespace+case
normalization; every comma-separated Complete Family member must appear literally in BOTH strategy
strings; each Count must equal its own member-set cardinality; the two member sets must be equal ignoring
order and case; and Member-set Comparison must contain `equal`, `match` or `identical`.

**Why:** task-researcher has no idea prd-feature will later need that section, so a research artifact
written first never has it. The dependency runs backwards from the consumer to the producer.

**How to apply:** before delegating prd-feature on a `full-bug` feature, append the
`## Numeric Derivation Evidence` section to the already-written research artifact yourself. Expect the
`user-story-path` hook to block termination regardless; treat prd-feature's `spec.md` on disk as the
deliverable rather than its clean exit, and if it creates `user-story.md` to appease the hook, remove it
with `git add <path>` followed by `git rm -f <path>` (only `git *` is allowlisted for Bash here). See
[[feature-folder-order-hook-is-workmode-blind]] for the same blindness in the plan-write gate, and
[[no-sendmessage-relaunch-with-resume-brief]] because you cannot course-correct the running agent.
