# [P0-T1] Phase 0 policy read record

Timestamp: 2026-09-07T06-37

Policy Order: policy-compliance-order sequence — (1) CLAUDE.md, (2) .claude/rules/general-code-change.md,
(3) .claude/rules/general-unit-test.md, (4) language-specific rules for the files in scope (C#):
.claude/rules/csharp.md, (5) .claude/rules/tonality.md.

Command: Read tool applied to each of the five paths below, rooted at the item worktree; line counts measured with
`pwsh -NoProfile -Command "(Get-Content -LiteralPath <path>).Count"`.

EXIT_CODE: 0

## Files read (in order)

1. CLAUDE.md — 447 lines
2. .claude/rules/general-code-change.md — 80 lines
3. .claude/rules/general-unit-test.md — 105 lines
4. .claude/rules/csharp.md — 96 lines
5. .claude/rules/tonality.md — 80 lines

Output Summary: All five policy files exist in the item worktree and were read in full in the
policy-compliance-order sequence. Line counts: 447, 80, 105, 96, 80. Constraints carried into execution:
CSharpier via `dotnet tool run` only; the two MSBuild gate commands use `/t:Rebuild` and must not add
`/p:Nullable=enable`; MSTest + Moq + FluentAssertions for tests; 500-line file ceiling for production, test and
reusable script files; no temporary files in tests; professional and non-hyperbolic tone in all artifacts.
