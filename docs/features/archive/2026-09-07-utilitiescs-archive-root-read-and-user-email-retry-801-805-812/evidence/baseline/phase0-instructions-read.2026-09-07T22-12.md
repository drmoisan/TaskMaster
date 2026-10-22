# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-09-08T06-29

Policy Order: as defined by the `policy-compliance-order` skill and by the plan task [P0-T1]: repository standing instructions first, then the cross-language code-change policy, then the cross-language unit-test policy, then the tier system, then the language-specific C# rules, then the tonality policy, then the plan acceptance-gate rules.

Files read, in the order required:

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/csharp.md`
- `.claude/rules/tonality.md`
- `.claude/rules/plan-acceptance-gates.md`

Command: read-only file reads through the agent Read tool; no shell command was executed for this task.

EXIT_CODE: 0

Output Summary: All seven policy files were read from this worktree in the order listed above. Load-bearing constraints carried into execution: the four-stage C# toolchain order (format, analyze, type-check, test) with a restart from stage 1 on any failure or file rewrite; `/t:Rebuild` mandatory for the two MSBuild gates and `/p:Nullable=enable` prohibited; CSharpier invoked only through `dotnet tool run`; MSTest plus Moq plus FluentAssertions for tests; the 500-line file-size cap scoped to production code, test code, and reusable script files; no temporary files in tests; and the professional-tone requirement for all authored content.
