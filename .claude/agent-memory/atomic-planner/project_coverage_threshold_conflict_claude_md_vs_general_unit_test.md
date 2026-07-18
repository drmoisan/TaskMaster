---
name: project-coverage-threshold-conflict-claude-md-vs-general-unit-test
description: CLAUDE.md's C# Unit Test Policy (80% repo-wide / 90% new-code) conflicts with .claude/rules/general-unit-test.md's uniform 85% line / 75% branch, no-tier-floor rule — unresolved as of 2026-07-18
metadata:
  type: project
---

`CLAUDE.md` (top-level, "always loaded") states C# repository-wide line coverage must remain `>= 80%` and new modules/classes/methods must reach `>= 90%`. `.claude/rules/general-unit-test.md` (also auto-loaded for every session per its frontmatter) states line coverage must remain `>= 85%` and branch coverage `>= 75%` **uniformly across all tiers T1–T4**, and explicitly says "Tier-specific lower coverage thresholds are not used in this repository" — with no separate new-code floor mentioned.

**Why this matters:** `policy-compliance-order`'s hard constraint requires halting and notifying the user on conflicting instructions rather than silently picking one. This conflict was encountered while authoring the #209 atomic plan (tesseract-engine-initialization-failure) and was not resolved in-session; the plan was written to record actual baseline/final coverage numbers (line-rate and branch-rate) without hard-gating on either specific threshold, and the conflict was flagged to the user in the final response instead of blocking plan authoring entirely.

**How to apply:** When a future plan or audit needs a hard pass/fail coverage gate, flag this conflict explicitly to the user before picking a number. If forced to choose without user input, the stricter combined bar (>=85% line / >=75% branch, uniform, plus >=90% for genuinely new modules per CLAUDE.md) is the safer default since it satisfies both documents' literal text simultaneously. See related: [evidence-path-normalization](evidence-path-normalization.md), [Coverage Evidence Path Normalization].
