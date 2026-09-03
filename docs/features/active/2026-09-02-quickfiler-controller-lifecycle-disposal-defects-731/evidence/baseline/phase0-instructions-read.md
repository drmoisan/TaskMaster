# Phase 0 — Policy instructions read

Timestamp: 2026-09-03T13-20

Task: [P0-T1]
Issue: #731
Work Mode: full-bug

Policy Order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/csharp.md`, `.claude/rules/tonality.md`

## Files read, in the order above

- `CLAUDE.md` = 447 lines
- `.claude/rules/general-code-change.md` = 80 lines
- `.claude/rules/general-unit-test.md` = 105 lines
- `.claude/rules/quality-tiers.md` = 51 lines
- `.claude/rules/csharp.md` = 96 lines
- `.claude/rules/tonality.md` = 80 lines

Command: `pwsh -NoProfile -Command 'foreach ($p in @(...)) { (Get-Content -LiteralPath $p).Count }'`

EXIT_CODE: 0

Output Summary: All six policy files were read in full, in the declared Policy Order, and their line counts recorded above. No conflicting instruction was found between the six documents. Points carried into execution: the 500-line file ceiling and its disclosed pre-existing exceptions; the C# toolchain order format then analyze then type-check then test, with `/t:Rebuild` mandatory for both msbuild gates and `/p:Nullable=enable` prohibited; MSTest plus Moq plus FluentAssertions for all new tests; the prohibition on `Thread.Sleep`, `Task.Delay` and wall-clock waits in test code, and the requirement to use `FakeTimeProvider` for time-dependent async tests; no temporary files in tests; and the tonality policy governing every artifact written by this plan.
