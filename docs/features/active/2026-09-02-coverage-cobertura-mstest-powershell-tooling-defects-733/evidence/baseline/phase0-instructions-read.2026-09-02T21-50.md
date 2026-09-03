# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-09-02T21-50

Task: [P0-T1]

## Policy Order

Per `.claude/skills/policy-compliance-order/SKILL.md` and the plan's P0-T1, the four
policy files were read in exactly this order:

1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/powershell.md

## Files Read

- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/powershell.md

## Constraints Extracted (binding on this plan's execution)

- File size ceiling: no production, test, or reusable script file may exceed 500 lines
  (.claude/rules/general-code-change.md, File Size Limit; .claude/rules/powershell.md,
  Coding Standards — "Keep scripts cohesive and under 500 lines").
- PowerShell toolchain order: format (mcp__drm-copilot__run_poshqc_format) -> analyze
  (mcp__drm-copilot__run_poshqc_analyze) -> test (mcp__drm-copilot__run_poshqc_test).
  Type checking is Not Applicable for PowerShell (.claude/rules/powershell.md, Toolchain
  item 3). Restart from step 1 if any step fails or changes files.
- PowerShell change budget: per-batch cap of 3 production and 3 test files unless an
  explicit override has been approved (.claude/rules/powershell.md, Change Budget). This
  plan carries an approved override, recorded in its Change Budget Override subsection.
- Coverage: line coverage must remain >= 85% across all tiers
  (.claude/rules/powershell.md line 63; .claude/rules/general-unit-test.md, Coverage
  Requirements). Pester reports command and line coverage only; there is no PowerShell
  branch-coverage gate.
- Temporary files are strictly prohibited in tests and in this plan's evidence capture
  (.claude/rules/general-code-change.md, I/O Boundaries; .claude/rules/general-unit-test.md,
  External Dependencies).
- Bugfix workflow (CLAUDE.md, General Code Change Policy): failing regression test first,
  then the minimal targeted fix, then local verification. This is the ordering Phase 1's
  [expect-fail] tasks implement.
- Test file location: tests mirror the production tree under tests/ (for example
  scripts/vscode/Foo.ps1 -> tests/scripts/vscode/Foo.Tests.ps1)
  (.claude/rules/general-unit-test.md, Test File Location).
- Tone: strictly professional, factual, neutral (CLAUDE.md, Tone Policy).

## Output Summary

All four policy files read in the required order. No conflicting instruction was found
between them and this plan. The 500-line ceiling and the approved change-budget override
are the two constraints that materially shape Phase 1's file layout.
