# P0-T1 — Phase 0 policy reads

Timestamp: 2026-09-03T08-20

Policy Order: CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md

Command: read each of the six files below in the order listed, in the item worktree

EXIT_CODE: 0

## Files read (in order)

1. `CLAUDE.md` — 447 lines. Standing repository instructions: policy compliance order, General Code
   Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, tone policy,
   and the four-step C# toolchain (csharpier format, msbuild analyzer build, msbuild nullable build,
   vstest).
2. `.claude/rules/general-code-change.md` — 80 lines. Design principles, module rigor tiers,
   mandatory toolchain loop, 500-line file-size limit, error handling and logging, naming, public API
   compatibility, dependencies, I/O boundaries.
3. `.claude/rules/general-unit-test.md` — 105 lines. Core principles, coverage requirements and the
   coverage exclusion policy, scenario completeness, Arrange-Act-Assert structure, external
   dependency rules, test file location, determinism infrastructure and banned timing APIs in tests.
4. `.claude/rules/quality-tiers.md` — 51 lines. T1-T4 module rigor tiers, the source of truth in
   `quality-tiers.yml`, and the uniform-versus-tier-dependent gate matrix.
5. `.claude/rules/csharp.md` — 96 lines. C#-specific toolchain commands, coding standards, testing
   standards, deterministic test rules, DI seams, the analyzer stack, and prohibited behaviors
   (including "adding sleeps, retries, or timing hacks to mask flaky behavior").
6. `.claude/rules/tonality.md` — 80 lines. Required professional tone, prohibitions on humor,
   hyperbole, and metaphor, evidence-first wording, and rules for difficult messages.

## Output Summary

All six policy files were read in the order required by `policy-compliance-order`. All six exist in
the item worktree at the paths listed above. No policy file was modified.

Threshold conflict observed and recorded here for P0-T12: CLAUDE.md (rank 1) states repository line
coverage `>= 80%` and new module/class/method coverage `>= 90%`;
`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (ranks 3 and 4) state
`>= 85%` line and `>= 75%` branch. The rank-1 figures govern.

Test-determinism rules relevant to this plan's AC5: `.claude/rules/general-unit-test.md` bans
`Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `setTimeout` in test code;
`.claude/rules/csharp.md` lists "adding sleeps, retries, or timing hacks to mask flaky behavior"
under Prohibited Behaviors. The regression test this plan adds uses no timing construct.
