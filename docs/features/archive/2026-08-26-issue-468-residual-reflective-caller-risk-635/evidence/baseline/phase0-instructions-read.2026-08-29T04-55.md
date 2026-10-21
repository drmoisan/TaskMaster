# Phase 0 — Policy Instructions Read (P0-T1)

- **Issue:** #635
- **Plan task:** [P0-T1]
- **Plan file:** `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md`

Timestamp: 2026-08-29T06-22

Policy Order: CLAUDE.md first; then the cross-language general code-change policy; then the cross-language general unit-test policy; then the quality-tiers rule; then the tonality rule; then the C#-specific rule; then the atomic-plan-contract, evidence-and-timestamp-conventions, and acceptance-criteria-tracking skill files. This is the order mandated by the `policy-compliance-order` skill and by the "Policy Compliance Order" section of CLAUDE.md, with the plan-contract, evidence-convention, and acceptance-criteria skills layered on top as the execution contract for this item.

## Files read, in the order read

All paths are repository-relative to the root of this checkout.

1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/quality-tiers.md
5. .claude/rules/tonality.md
6. .claude/rules/csharp.md
7. .claude/skills/atomic-plan-contract/SKILL.md
8. .claude/skills/evidence-and-timestamp-conventions/SKILL.md
9. .claude/skills/acceptance-criteria-tracking/SKILL.md

FILES_READ: 9

## Constraints carried forward into execution

- Evidence is written only under `<FEATURE>/evidence/<kind>/`; the `artifacts/` evidence sub-paths are forbidden (evidence-and-timestamp-conventions, Non-Overridable Authority).
- Artifact filenames use the `yyyy-MM-ddTHH-mm` stamp. The filename stamp for this item is the fixed planning stamp `2026-08-29T04-55`; each artifact's `Timestamp:` field records its actual execution time and therefore differs from the filename stamp.
- Work Mode is `full-bug`, so the acceptance-criteria source is `spec.md` only and `user-story.md` is legitimately absent (acceptance-criteria-tracking, AC Source Resolution).
- The C# toolchain order is format, then analyzer rebuild, then nullable rebuild, then test. This item modifies no C# file, so the branch condition recorded by [P4-T2] determines whether those gates have any input.
- Tone is professional, factual, and neutral in every artifact (.claude/rules/tonality.md).
- No artifact contains an absolute host path, an account name, or a machine name.

Output Summary: All nine mandated policy and skill files were read in the mandated order before any execution task ran. No conflict between the plan and repository policy was identified during the read. The plan's evidence locations, filename-stamp convention, `full-bug` AC source resolution, and read-only treatment of the QuickFiler production and test trees are consistent with the policies read.
