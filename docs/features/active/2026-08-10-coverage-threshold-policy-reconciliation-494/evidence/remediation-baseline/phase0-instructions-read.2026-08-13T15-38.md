Timestamp: 2026-08-13T15-38
Policy Order:
1. `AGENTS.md`
2. `.agents/skills/policy-compliance-order/SKILL.md`
3. `.agents/skills/atomic-plan-contract/SKILL.md`
4. `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`
5. `.agents/skills/acceptance-criteria-tracking/SKILL.md`
6. `.agents/skills/powershell/SKILL.md`
7. `.github/instructions/general-code-change.instructions.md`
8. `.github/instructions/general-unit-test.instructions.md`
9. `.github/instructions/powershell-code-change.instructions.md`
10. `.github/instructions/powershell-unit-test.instructions.md`

Read Files:

1. `AGENTS.md`
2. `.agents/skills/policy-compliance-order/SKILL.md`
3. `.agents/skills/atomic-plan-contract/SKILL.md`
4. `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`
5. `.agents/skills/acceptance-criteria-tracking/SKILL.md`
6. `.agents/skills/powershell/SKILL.md`
7. `.github/instructions/general-code-change.instructions.md`
8. `.github/instructions/general-unit-test.instructions.md`
9. `.github/instructions/powershell-code-change.instructions.md`
10. `.github/instructions/powershell-unit-test.instructions.md`

Work Mode: `full-bug`

Acceptance Criteria Source: `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`

Scope-Change Disposition: Issue #494 requires no upstream receipt, release, or external-repository work. `evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` is the complete TaskMaster deliverable for prohibited Claude-runtime changes.

Prohibited-Path Guard: Do not modify `CLAUDE.md`, `.claude/**`, `.agents/skills/**`, or paths outside this TaskMaster repository.

Permitted Production Paths:

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
