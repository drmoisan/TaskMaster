# Phase 0 — Policy instructions read (P0-T1)

Timestamp: 2026-09-03T01-04

Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`

Files Read: 7

## Files read, in the order above

- `CLAUDE.md` — repository standing instructions; embeds the General Code Change Policy,
  the General Unit Test Policy, the C# Code Change Policy, and the C# Unit Test Policy.
- `.claude/rules/general-code-change.md` — cross-language code change policy, seven-stage
  toolchain loop, 500-line file size limit.
- `.claude/rules/general-unit-test.md` — cross-language unit test policy, coverage
  requirements, determinism infrastructure (`TimeProvider`, `FakeTimeProvider`, banned
  wall-clock APIs in test code).
- `.claude/rules/csharp.md` — C# toolchain (CSharpier, .NET analyzers, nullable analysis,
  MSTest), coding standards, DI seams, analyzer stack.
- `.claude/rules/quality-tiers.md` — T1–T4 module rigor tiers and the uniform-vs-tier-dependent
  gate matrix.
- `.claude/rules/tonality.md` — required professional tone for all agent-authored content.
- `.claude/rules/plan-acceptance-gates.md` — acceptance-gate rules G1 through G9 applied to
  the shell commands an atomic plan states as acceptance conditions.

## Recorded policy conflict (see plan D5)

`CLAUDE.md` § UT2 states a repository-wide line coverage floor of `>= 80%` with `>= 90%` for
new modules. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state
`>= 85%` line and `>= 75%` branch uniformly across T1–T4. Both figures are recorded verbatim
rather than reconciled; the binding gates for this plan are the no-regression comparisons
stated in D5.

EXIT_CODE: 0

Output Summary: All seven policy files were read in the stated order from the workspace root
before any Phase 1 work began. No policy file was modified.
