# Issue #608 remediation policy-read evidence

Timestamp: 2026-08-25T12-50
Policy Order: AGENTS.md; policy-compliance-order; atomic-plan-contract; evidence-and-timestamp-conventions; csharp; csharp-qa-gate; remediation inputs.
Files Read:

1. `AGENTS.md`
2. `.agents/skills/policy-compliance-order/SKILL.md`
3. `.agents/skills/atomic-plan-contract/SKILL.md`
4. `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`
5. `.agents/skills/csharp/SKILL.md`
6. `.agents/skills/csharp-qa-gate/SKILL.md`
7. `docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/remediation-inputs.2026-08-25T12-33.md`

Remediation Constraints:

- Issue #608 is `full-bug`; the authoritative acceptance-criteria source is `spec.md`.
- Preserve the original plan, execution-sequence-deviation receipt, and failed global-nullable receipt without modification.
- Do not use `/p:Nullable=enable`; use the per-file nullable gate with `/p:TreatWarningsAsErrors=true`.
- Do not change production, test, project, configuration, policy, API, or #446-worktree files in this remediation cycle.
- Run CSharpier, analyzer rebuild, type/nullable rebuild, and coverage-enabled MSTest in order; require zero regressions against the documented baseline.
