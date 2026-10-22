# Phase 0 Policy Read (issue 942)

Timestamp: 2026-09-30T07-23
Task: P0-T1

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md

Files read (in this order), with the top-level (`# `) heading count of each, counted from the file:

1. CLAUDE.md — top-level headings: 1
2. .claude/rules/general-code-change.md — top-level headings: 1
3. .claude/rules/general-unit-test.md — top-level headings: 1
4. .claude/rules/csharp.md — top-level headings: 1
5. .claude/rules/plan-acceptance-gates.md — top-level headings: 1
6. .claude/rules/tonality.md — top-level headings: 1

No policy document was modified by this task.

Key constraints carried into execution:

- C# toolchain order: CSharpier format and check, analyzer Rebuild, nullable Rebuild (TreatWarningsAsErrors, no Nullable override), coverage-enabled MSTest run.
- MSTest, Moq and FluentAssertions for tests; no temporary files; no sleeps, delays or wall-clock reads in tests.
- 500-line ceiling per source file.
- Committed test evidence is projections only; no raw trx or Cobertura document is committed.
