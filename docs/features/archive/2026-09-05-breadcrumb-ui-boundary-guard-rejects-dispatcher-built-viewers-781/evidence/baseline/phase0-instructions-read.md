# Phase 0 — Repository Policy Read Record (issue #781)

Timestamp: 2026-09-05T16-14

Task: [P0-T1]

Policy Order: The nine files below were read in this exact order, which is the order
`.claude/skills/policy-compliance-order/SKILL.md` defines plus the three plan-contract,
acceptance-criteria, and evidence-convention skills the plan names.

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/csharp.md`
6. `.claude/rules/tonality.md`
7. `.claude/skills/atomic-plan-contract/SKILL.md`
8. `.claude/skills/acceptance-criteria-tracking/SKILL.md`
9. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`

## Files Read (repository-relative path and line count)

| # | Repository-relative path | Lines |
| --- | --- | --- |
| 1 | `CLAUDE.md` | 447 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/quality-tiers.md` | 51 |
| 5 | `.claude/rules/csharp.md` | 96 |
| 6 | `.claude/rules/tonality.md` | 80 |
| 7 | `.claude/skills/atomic-plan-contract/SKILL.md` | 245 |
| 8 | `.claude/skills/acceptance-criteria-tracking/SKILL.md` | 104 |
| 9 | `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` | 176 |

Line counts were obtained with `Get-Content -LiteralPath <path>` and its resulting element
count, run from the repository root inside a `pwsh -NoProfile -Command` process. The command
is recorded without a host path because this repository forbids an absolute filesystem path in
a tracked artifact.

EXIT_CODE: 0

Output Summary: All nine policy files exist and were read in the stated order; total 1384 lines
across the nine files. Governing constraints carried forward into execution: the four-step C#
toolchain order (CSharpier format, MSBuild analyzers, MSBuild nullable with
`/p:TreatWarningsAsErrors=true` and without `/p:Nullable=enable`, then vstest with coverage)
with a restart from step one on any failure or file change; MSTest plus Moq plus
FluentAssertions for C# tests; the 500-line per-file ceiling; the prohibition on temporary files
and on sleeps, timers, and wall-clock waits in tests; evidence written only under
`<FEATURE>/evidence/<kind>/`; acceptance criteria checked off one at a time in `issue.md` only,
this item being `minor-audit`; and the neutral, factual tone required by
`.claude/rules/tonality.md`.
