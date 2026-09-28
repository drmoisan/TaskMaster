# Phase 0 — Instructions read (P0-T1)

Timestamp: 2026-09-14T17-49

Policy Order: the reading order required by `.claude/skills/policy-compliance-order/SKILL.md` and by the `## Policy Compliance Order` section of CLAUDE.md, extended by task P0-T1 with the domain rules in scope for this delivery (PowerShell, quality tiers, CI workflows, tonality) and then the feature requirement documents. Every path below was read from the item worktree with an absolute path, per part 4 of the plan's working-directory rule.

Files read, in the order read:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/powershell.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/ci-workflows.md`
7. `.claude/rules/tonality.md`
8. `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/spec.md`
9. `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/issue.md`
10. `docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/research/2026-09-12T11-05-ci-coverage-and-pester-gates-research.md`
11. `.github/workflows/README.md`

Count: eleven paths, matching the eleven the task names.

Output Summary: all eleven paths exist in the item worktree and were read in full. Observations that bear on later tasks:

- CLAUDE.md in this worktree states C# line coverage `>= 80%` and C# branch coverage `>= 75%`, PowerShell line coverage `>= 80%`, and new code `>= 90%`, attributed to the maintainer decision of 2026-09-11 under issue #563. The three rules files state 85 line and 75 branch with a PowerShell branch exemption. The divergence is recorded by settled decision D10 of the specification and is not resolved by this delivery.
- `.claude/rules/powershell.md` states that type checking is not applicable to PowerShell, which is the citation task P10-T3 requires.
- `.claude/rules/powershell.md` states the per-batch cap of at most 3 production files and 3 test files, which is the constraint the plan's batch structure is built around.
- `.claude/rules/ci-workflows.md` states the exit-code rule for a `pwsh` step that deliberately invokes a failing command.
- The specification's `## Acceptance Criteria` section holds 31 checkbox items at lines 241 through 271 and no other checkbox appears in that section.
- `.github/workflows/README.md` carries the single-line literal `were moved, not edited` at line 49, which is the pre-change observation tasks P7-T6 and P7-T7 assert against.
