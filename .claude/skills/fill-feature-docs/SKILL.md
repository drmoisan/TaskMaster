---
name: fill-feature-docs
description: Invoke the prd-feature worker to produce feature-document outputs from issue and research inputs.
---

# Fill Feature Docs Skill

This direct-use wrapper delegates feature-document work to the `prd-feature` worker.

## Inputs

- Feature folder issue and research context
- Existing spec and user-story files when present

For a numeric `spec.md` acceptance criterion, the research context must contain complete `## Numeric Derivation Evidence` with `Complete Family`, `Exhaustive Search Scope`, `Inclusion Rules`, `Exclusion Rules`, `Primary Search Strategy or Query Expression`, `Primary Member Set`, `Primary Count`, `Cross-check Search Strategy or Query Expression`, `Cross-check Member Set`, `Cross-check Count`, and `Member-set Comparison`. The two derivations must be non-empty, independently constructed, distinct in strategy or query expression, exhaustive across the complete family, independently enumerated, and explicitly compared. The worker must withhold a numeric assertion for missing, copied, incomplete, non-exhaustive, narrow, or disagreeing evidence; a single grep, a named-pattern-only query, matching totals, distinct query text, or equal member sets alone does not approve a number.

## Output Paths

- `docs/features/active/<feature>/spec.md`
- `docs/features/active/<feature>/user-story.md`

## Worker Routing

- Worker: `prd-feature`
- Require the worker to report the authoritative `research-path` when a numeric acceptance criterion is written.
