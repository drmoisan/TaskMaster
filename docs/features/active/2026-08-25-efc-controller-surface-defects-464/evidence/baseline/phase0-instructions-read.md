# Phase 0 — repository policy read proof

Timestamp: 2026-08-27T23-15
Task: [P0-T1]
Command: (file reads; no shell gate)
EXIT_CODE: 0

Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/plan-acceptance-gates.md`
6. `.claude/rules/quality-tiers.md`
7. `.claude/rules/tonality.md`

## Per-file confirmation

- `CLAUDE.md` (447 lines) — READ. Policy compliance order, General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, tone policy, and the four-step C# toolchain.
- `.claude/rules/general-code-change.md` (80 lines) — READ. Design principles, module rigor tiers pointer, mandatory toolchain loop, 500-line file-size limit, error handling, naming, I/O boundaries.
- `.claude/rules/general-unit-test.md` (105 lines) — READ. Five core principles, coverage requirements and exclusion policy, scenario completeness, Arrange-Act-Assert, external-dependency prohibition, test file location, determinism infrastructure and banned APIs (`Thread.Sleep`, `Task.Delay`).
- `.claude/rules/csharp.md` (96 lines) — READ. CSharpier/msbuild/vstest commands, `/t:Rebuild` rationale, prohibition on `/p:Nullable=enable`, MSTest + Moq + FluentAssertions, DI seam ordering, analyzer stack, prohibited behaviors.
- `.claude/rules/plan-acceptance-gates.md` (128 lines) — READ. Acceptance-gate rules G1 through G6, checkable-literal definition, placeholder guard, authoring guidance for falsifiable acceptance conditions.
- `.claude/rules/quality-tiers.md` (51 lines) — READ. T1 through T4 tiers, uniform vs tier-dependent gate matrix, uniform coverage thresholds.
- `.claude/rules/tonality.md` (80 lines) — READ. Professional tone, prohibitions on humor, hyperbole and metaphor, evidence-first wording.

Output Summary: All seven policy files exist in this worktree and were read in the order listed above. No policy file was modified. Conflicts requiring a halt: none identified.
