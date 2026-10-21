# Phase 0 — Policy Instructions Read ([P0-T1])

Timestamp: 2026-08-28T05-07

Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/plan-acceptance-gates.md`
6. `.claude/rules/quality-tiers.md`
7. `.claude/rules/tonality.md`

Command: `wc -l CLAUDE.md .claude/rules/general-code-change.md .claude/rules/general-unit-test.md .claude/rules/csharp.md .claude/rules/plan-acceptance-gates.md .claude/rules/quality-tiers.md .claude/rules/tonality.md`
EXIT_CODE: 0

## Files read, in order

- `CLAUDE.md` (447 lines) — READ. Establishes the policy compliance order, the General Code Change Policy, the General Unit Test Policy, the C# Code Change Policy, the C# Unit Test Policy, the Tone Policy, and the four-step C# toolchain (format, analyze, type-check, test) with `/t:Rebuild` and without `/p:Nullable=enable`.
- `.claude/rules/general-code-change.md` (80 lines) — READ. Design principles, module rigor tiers by reference, the mandatory toolchain loop with restart-on-change, the 500-line file-size limit, error handling and logging, naming, public API compatibility, dependencies, and I/O boundaries.
- `.claude/rules/general-unit-test.md` (105 lines) — READ. Independence, isolation, fast execution, determinism, readability; coverage thresholds; the coverage exclusion policy; scenario completeness; Arrange-Act-Assert; the ban on external dependencies and temporary files; test file location; determinism infrastructure and the banned-API list (`Thread.Sleep`, `Task.Delay`, real wall-clock waits).
- `.claude/rules/csharp.md` (96 lines) — READ. CSharpier formatting through `dotnet tool run`; the analyzer and nullable msbuild commands with `/t:Rebuild` and the explicit instruction not to pass `/p:Nullable=enable`; MSTest + Moq + FluentAssertions; coverage floors; deterministic test rules; DI seams; the five-package analyzer stack and the SecurityCodeScan deferral; prohibited behaviors including weakening assertions and adding sleeps or timing hacks.
- `.claude/rules/plan-acceptance-gates.md` (128 lines) — READ. Acceptance-gate rules G1 through G6, the attribution window, graceful degradation, the G5 and G6 severity decisions, the checkable-literal definition and placeholder guard, message-formatting prohibitions, and authoring guidance (prefer a named test over a phrase search; assert short single-line non-interpolated tokens).
- `.claude/rules/quality-tiers.md` (51 lines) — READ. The T1 through T4 module rigor tiers, `quality-tiers.yml` as the source of truth, and the uniform-versus-tier-dependent gate matrix.
- `.claude/rules/tonality.md` (80 lines) — READ. Required professional tone; prohibitions on humor, hyperbole, and decorative metaphor; evidence-first wording; handling of difficult messages.

Output Summary: All seven policy files exist and were read in the stated order. No conflicting instruction was found between them and the plan of record. The binding operational consequences carried into execution are: the four-stage C# toolchain in order with restart-on-change; `/t:Rebuild` for both msbuild gates; no `/p:Nullable=enable`; MSTest + Moq + FluentAssertions only; no `Thread.Sleep`, `Task.Delay`, wall-clock wait, or temporary file in any test; and the 500-line file ceiling.
