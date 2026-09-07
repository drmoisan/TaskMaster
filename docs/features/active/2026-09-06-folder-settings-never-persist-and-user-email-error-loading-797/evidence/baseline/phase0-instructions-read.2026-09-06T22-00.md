# Phase 0 — Policy Instructions Read (Issue #797)

Timestamp: 2026-09-07T09-10

Policy Order: CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/quality-tiers.md, then .claude/rules/csharp.md, then .claude/rules/tonality.md.

## Files read, in the required order

1. CLAUDE.md — 448 lines
2. .claude/rules/general-code-change.md — 81 lines
3. .claude/rules/general-unit-test.md — 106 lines
4. .claude/rules/quality-tiers.md — 52 lines
5. .claude/rules/csharp.md — 97 lines
6. .claude/rules/tonality.md — 81 lines

All six files were read in full from this worktree at the current HEAD before any task in this plan
performed a write.

## Constraints carried forward into execution

- C# toolchain order: csharpier format, csharpier check, msbuild analyzer rebuild, msbuild
  warnings-as-errors rebuild, vstest. Restart from step 1 on any failure or file change.
- Always `/t:Rebuild`, never `/t:Build`. Never add a solution-wide `/p:Nullable=enable`.
- MSTest, Moq, FluentAssertions. Arrange-Act-Assert. No temporary files in tests. No `Thread.Sleep`,
  no `Task.Delay`, no real wall-clock waits.
- 500-line cap on production, test and reusable script files. Markdown documents are exempt.
- Coverage: CLAUDE.md sets the repository-wide line floor at 80 percent and requires 90 percent for
  new and changed code, with no regression on changed lines. `.claude/rules/general-unit-test.md`
  records 85 percent line and 75 percent branch figures; plan rule R8 governs which of these is the
  binding gate for this change.
- Tone: professional, factual, neutral. No humor, hyperbole, or decorative metaphor.

EXIT_CODE: 0

Output Summary: All six policy files were read in the required order and their line counts recorded.
No conflict requiring a halt was found between them and this plan.
