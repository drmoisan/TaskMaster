# Phase 0 — Policy Instructions Read (Issue #445)

Timestamp: 2026-08-22T09-14

Command: file reads (no shell command required). Line counts verified with `wc -l CLAUDE.md .claude/rules/general-code-change.md .claude/rules/general-unit-test.md .claude/rules/csharp.md .claude/rules/tonality.md .claude/rules/plan-acceptance-gates.md`

EXIT_CODE: 0

Policy Order: `CLAUDE.md` then `.claude/rules/general-code-change.md` then `.claude/rules/general-unit-test.md` then `.claude/rules/csharp.md`

## Files read end to end

- `CLAUDE.md` (447 lines) — all sections: Project Guidelines, Policy Compliance Order, General Code Change Policy (including Bugfix Workflow), C# Code Change Policy, General Unit Test Policy (UT1-UT5), C# Unit Test Policy (CUT1-CUT3), Tone Policy, C# Toolchain, Key Skills Reference. [P0-T1]
- `.claude/rules/general-code-change.md` (80 lines) — design principles, module rigor tiers, mandatory toolchain loop, 500-line file size limit, error handling and logging, naming, public APIs, dependencies, I/O boundaries. [P0-T2]
- `.claude/rules/general-unit-test.md` (105 lines) — core principles, coverage requirements (line >= 85%, branch >= 75%), coverage exclusion policy, scenario completeness, Arrange-Act-Assert, external dependencies, test file location, documentation, test categories, determinism infrastructure. [P0-T3]
- `.claude/rules/csharp.md` (96 lines) — toolchain (CSharpier, .NET analyzers, nullable analysis, MSTest), coding standards, testing standards (repository line coverage >= 80%, new code >= 90%), deterministic test rules, DI seams, analyzer stack, prohibited behaviors. [P0-T4]
- `.claude/rules/tonality.md` (80 lines) — required professional tone, humor prohibited, hyperbole prohibited, metaphors tightly restricted, evidence-first wording, difficult messages, final rule. [P0-T5]
- `.claude/rules/plan-acceptance-gates.md` (116 lines) — acceptance-gate rules G1 through G6, scope of invocation, rule table, attribution window, graceful degradation, severity decisions, checkable-literal definition and placeholder guard, message-formatting prohibitions, authoring guidance for plan authors. [P0-T5]

## Note on `.claude/**`

Reading a rule file is never a licence to edit it. `.claude/**` is push-down-owned per Hard Constraint 1 of the plan: a sync overwrites the tree from an upstream bundle with no merge, so any local edit is destroyed. The rule files above are the policy this fix is measured against, never edit targets. The only path under that tree an executor may write is `.claude/agent-memory/**`, and no task in this plan writes there.

Output Summary: All six policy documents were read end to end in the mandated order (`CLAUDE.md`, then `general-code-change.md`, then `general-unit-test.md`, then `csharp.md`, then the two supplementary rule files `tonality.md` and `plan-acceptance-gates.md`), totalling 924 lines. No policy file was modified. Two coverage-threshold sets were confirmed divergent and pre-existing: CLAUDE.md UT2 with `csharp.md` (repository line >= 80%, new code >= 90%) against `general-unit-test.md` with `quality-tiers.md` (line >= 85%, branch >= 75%). Both figures are reported against in P5-T8 per the plan's Coverage Policy Position; the divergence is not adjudicated by this issue.
