# P0-T2 — Phase 0 Policy Instructions Read

Timestamp: 2026-08-08T20-38

Policy Order: as defined by `.claude/skills/policy-compliance-order/SKILL.md` —
(1) `CLAUDE.md` standing instructions, (2) the cross-language code-change policy,
(3) the cross-language unit-test policy, then (4) the language- and domain-specific
rules that apply to the files in scope (C# for this delivery).

Files read, in order, with absolute paths:

1. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\CLAUDE.md`
   — all embedded sections: Project Guidelines, Policy Compliance Order, General Code
   Change Policy (including the Bugfix Workflow), C# Code Change Policy, General Unit
   Test Policy, C# Unit Test Policy, Tone Policy, and the C# Toolchain ordering.
2. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\.claude\rules\general-code-change.md`
3. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\.claude\rules\general-unit-test.md`
4. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\.claude\rules\quality-tiers.md`
5. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\.claude\rules\tonality.md`

Additional language-specific rule read because every production and test file in this
delivery is C# (step 4 of the policy-compliance order):

6. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\.claude\rules\csharp.md`

Binding constraints extracted for this delivery:

- Bugfix workflow: failing regression test first, then the minimal targeted fix.
- Toolchain order: format, lint/analyze, type-check, test; restart from format on any
  failure or file mutation.
- File size cap: 500 lines for production, test, and reusable script files; Markdown
  documentation is exempt.
- Tests: MSTest, Moq, FluentAssertions, Arrange-Act-Assert, no temporary files, no
  `Thread.Sleep`/`Task.Delay`, no wall-clock reads, no external dependencies.
- Coverage: repository-wide line coverage `>= 80%` on the testable denominator
  (`CLAUDE.md` § UT2); new modules/classes/methods `>= 90%`. A known threshold conflict
  exists with `.claude/rules/quality-tiers.md` (85% line / 75% branch); it is recorded
  in the P5-T8 comparison artifact rather than silently resolved.
- Tone Policy: neutral, factual, evidence-first prose in every artifact this delivery
  produces.
- No policy document under `.claude/rules/` is modified by this delivery.

Binary outcome: PASS.
