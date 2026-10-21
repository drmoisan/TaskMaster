# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-09-07T14-03
Task: [P0-T1]
Issue: #796
Work Mode: full-bug
Branch: bug/quickfiler-folder-dropdown-closes-on-open-796

Policy Order: CLAUDE.md, then .claude/rules/general-code-change.md, then
.claude/rules/general-unit-test.md, then .claude/rules/quality-tiers.md, then
.claude/rules/tonality.md, then .claude/rules/csharp.md, then
.claude/rules/plan-acceptance-gates.md.

## Files read, in the required order

1. CLAUDE.md — read in full (448 lines).
2. .claude/rules/general-code-change.md — read in full (81 lines).
3. .claude/rules/general-unit-test.md — read in full (106 lines).
4. .claude/rules/quality-tiers.md — read in full (52 lines).
5. .claude/rules/tonality.md — read in full (81 lines).
6. .claude/rules/csharp.md — read in full (97 lines).
7. .claude/rules/plan-acceptance-gates.md — read in full (258 lines).

All seven paths listed above were read. Every path is repository-relative to the
worktree root and was read from the worktree this plan executes in.

## Constraints carried forward into execution

- Toolchain order for C#: format (CSharpier), then analyze (.NET analyzers), then
  type-check (nullable), then test. Any failure or auto-fix restarts from format.
- `/t:Rebuild` is mandatory locally for the analyzer and nullable gates. `/t:Build`
  can skip `CoreCompile` through MSBuild incrementality and exit 0 without running
  analyzers.
- `/p:Nullable=enable` is not used. Nullable enforcement is per-file opt-in through
  the `#nullable enable` pragma.
- 500-line ceiling on any production, test, or reusable script file. Markdown
  documentation is exempt.
- MSTest as the test framework, Moq for mocking, FluentAssertions for assertions.
- No temporary files in tests.
- Tone policy: professional, factual, neutral. No humor, hyperbole, or decorative
  metaphor in any authored content.
- Policy documents under .claude/rules/ must not be modified.

Output Summary: All seven policy files read in the required order and recorded.
EXIT_CODE: 0
