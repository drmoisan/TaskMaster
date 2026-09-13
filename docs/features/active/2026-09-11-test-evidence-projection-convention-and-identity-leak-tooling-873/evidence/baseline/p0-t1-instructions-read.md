# P0-T1 — Policy Documents Read

Timestamp: 2026-09-13T04-50
Task: [P0-T1]
Policy Order: CLAUDE.md, then the general code-change rule, then the general unit-test rule, then the PowerShell rule and the C# rule, then the quality-tiers rule, the tonality rule and the plan-acceptance-gates rule.

## Files Read (in mandated order, repository-relative paths)

1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/powershell.md
5. .claude/rules/csharp.md
6. .claude/rules/quality-tiers.md
7. .claude/rules/tonality.md
8. .claude/rules/plan-acceptance-gates.md

Count of files read: 8

## Output Summary

All eight policy documents were read end to end in the order listed above, in this worktree.
Threshold authority recorded for this delivery: CLAUDE.md is authoritative — C# repository-wide
line coverage >= 80%, new module/class/method >= 90%, no coverage regression on changed lines.
The 85% line and 75% branch figures in `.claude/rules/general-unit-test.md` and
`.claude/rules/quality-tiers.md` are push-down owned from an upstream repository and are recorded
here as read but not as the authoritative threshold for this item. No threshold is changed by this
delivery.

Key constraints carried forward into execution:
- 500-line ceiling per production, test or reusable script file (general code-change rule).
- PowerShell toolchain order format, analyze, test through the PoshQC MCP tools; no editor task wrappers.
- PowerShell per-batch cap of three production files and three test files.
- C# toolchain order csharpier check, msbuild analyzer rebuild, msbuild nullable rebuild, vstest;
  `/t:Rebuild` mandatory and `/p:Nullable=enable` prohibited.
- Temporary files in tests prohibited outright.
- Policy documents under `.claude/rules/` and `.github/` are not modified by this delivery.

EXIT_CODE: 0
