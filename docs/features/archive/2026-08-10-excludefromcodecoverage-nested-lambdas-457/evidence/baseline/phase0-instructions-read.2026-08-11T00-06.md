# [P0-T2] Phase 0 policy documents read

Timestamp: 2026-08-11T00-06
Command: file reads only (no shell command)
EXIT_CODE: 0

## Policy Order

Per `.claude/skills/policy-compliance-order/SKILL.md` and the plan's `[P0-T2]` read order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/powershell.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`

## Files Read

All paths are repo-relative to the executing worktree
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a`.

| # | Repo-relative path | Lines |
|---|---|---|
| 1 | `CLAUDE.md` | 441 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/powershell.md` | 97 |
| 5 | `.claude/rules/quality-tiers.md` | 51 |
| 6 | `.claude/rules/tonality.md` | 80 |

## Constraints extracted that bind this plan

- 500-line ceiling applies to production code, test code, and reusable scripts
  (`.claude/rules/general-code-change.md` § File Size Limit; `.claude/rules/powershell.md` "Keep
  scripts cohesive and under 500 lines").
- PowerShell toolchain order is format -> analyze -> test; type checking is not applicable and is
  skipped by policy (`.claude/rules/powershell.md` § Toolchain, step 3). Restart from step 1 if any
  step fails or changes files.
- PowerShell change budget: up to 2 production files in direct mode; per-batch cap of 3 production
  and 3 test files (`.claude/rules/powershell.md` § Change Budget). This feature's surface is 2
  production files and 2-3 test files, within budget.
- Coverage floors: line >= 85%, branch >= 75% uniformly across T1-T4
  (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`). `CLAUDE.md` § UT2 states
  line coverage >= 80%. The conflict is recorded, not resolved (owned by issue #494).
- Temporary files are prohibited in tests and in code (`.claude/rules/general-code-change.md` § I/O
  Boundaries; `.claude/rules/general-unit-test.md` § External Dependencies).
- Test files must live under `tests/` mirroring the production tree
  (`.claude/rules/general-unit-test.md` § Test File Location): production
  `scripts/vscode/*.ps1` maps to `tests/scripts/vscode/*.Tests.ps1`.
- `Remove-` verb functions require `SupportsShouldProcess` (`.claude/rules/powershell.md` § Coding
  Standards, "Implement ShouldProcess/SupportsShouldProcess for state-changing actions").
- Tone: professional, factual, no humor, no hyperbole (`.claude/rules/tonality.md`).

## Output Summary

Six policy documents read in the required order. No conflicting instruction was found that halts
execution. The `CLAUDE.md` § UT2 80% versus `.claude/rules/*` 85% line-coverage divergence is a known
documented conflict assigned to issue #494 and is recorded as an observation by `[P3-T9]`, not
resolved here.
