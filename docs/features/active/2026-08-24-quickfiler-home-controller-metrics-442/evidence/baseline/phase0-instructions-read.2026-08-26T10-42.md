# Phase 0 — Policy Instructions Read

Timestamp: 2026-08-26T10-42
Task: [P0-T1]
Command: `wc -l CLAUDE.md .claude/rules/general-code-change.md .claude/rules/general-unit-test.md .claude/rules/csharp.md .claude/rules/plan-acceptance-gates.md .claude/rules/quality-tiers.md .claude/rules/tonality.md`
EXIT_CODE: 0

## Policy Order

The policy files were read in the `policy-compliance-order` sequence mandated by the plan:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/plan-acceptance-gates.md`
6. `.claude/rules/quality-tiers.md`
7. `.claude/rules/tonality.md`

## Files Read (repository-relative paths)

| # | Path | Lines |
| --- | --- | --- |
| 1 | `CLAUDE.md` | 447 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/csharp.md` | 96 |
| 5 | `.claude/rules/plan-acceptance-gates.md` | 116 |
| 6 | `.claude/rules/quality-tiers.md` | 51 |
| 7 | `.claude/rules/tonality.md` | 80 |

The C#-specific rule file `.claude/rules/csharp.md` is mandatory for this plan because every
production and test file in the owned surface is C#.

## Output Summary

All seven policy files were located and read in full. Line counts were confirmed with `wc -l`.
Operative constraints carried forward into execution:

- Toolchain order is format, lint, type-check, test; restart from step 1 on any failure or any
  file modification (`CLAUDE.md`, `.claude/rules/general-code-change.md`).
- CSharpier is invoked only through `dotnet tool run`; `dotnet format` is prohibited.
- The analyzer and nullable msbuild gates use `/t:Rebuild`; `/p:Nullable=enable` must not be added.
- MSTest plus Moq plus FluentAssertions is the mandated C# test stack.
- No production, test, or reusable script file may exceed 500 lines.
- Temporary files in tests are prohibited, as are `Thread.Sleep`, `Task.Delay`, and wall-clock waits.
- Acceptance conditions must be falsifiable per acceptance gates G1 through G6.
- Tone is professional, factual, and neutral.
