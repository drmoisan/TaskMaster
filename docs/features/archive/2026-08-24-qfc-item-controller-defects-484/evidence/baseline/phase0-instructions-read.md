# Phase 0 — Policy Instructions Read

Timestamp: 2026-08-26T08-25
Task: [P0-T1]
Command: (read-only file reads; no shell gate command)
EXIT_CODE: 0

## Policy Order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/plan-acceptance-gates.md`
6. `.claude/rules/quality-tiers.md`
7. `.claude/rules/tonality.md`

## Files read (in the order above)

- `CLAUDE.md` — READ (447 lines). Standing project instructions: policy compliance order, General Code
  Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, tone policy, and
  the four-stage C# toolchain.
- `.claude/rules/general-code-change.md` — READ (80 lines). Design principles, module rigor tiers,
  mandatory toolchain loop, 500-line file-size limit, error handling, naming, I/O boundaries.
- `.claude/rules/general-unit-test.md` — READ (105 lines). Core principles, coverage requirements,
  coverage exclusion policy, scenario completeness, Arrange-Act-Assert, external-dependency ban,
  test file location, determinism infrastructure (banned `Thread.Sleep`, `Task.Delay`, wall-clock waits).
- `.claude/rules/csharp.md` — READ (96 lines). CSharpier formatting via `dotnet tool run`, analyzer and
  nullable msbuild commands with `/t:Rebuild`, explicit prohibition on `/p:Nullable=enable`, MSTest +
  Moq + FluentAssertions, coverage floors (repo >= 80%, new members >= 90%), DI seam preference order,
  analyzer stack, prohibited behaviors.
- `.claude/rules/plan-acceptance-gates.md` — READ (116 lines). Acceptance-gate rules G1 through G6 for
  atomic plans, checkable-literal definition, placeholder guard, message formatting, authoring guidance.
- `.claude/rules/quality-tiers.md` — READ (51 lines). T1-T4 module rigor tiers and the
  uniform-versus-tier-dependent gate matrix.
- `.claude/rules/tonality.md` — READ (80 lines). Required professional tone; prohibitions on humor,
  hyperbole, and decorative metaphor; evidence-first wording.

Output Summary: All seven policy files were read in the stated order. No conflicting instruction was
found between them and the approved plan. Three policy facts are load-bearing for this plan and are
recorded here: (1) `/t:Rebuild` is mandatory for both msbuild gate stages on a warm local worktree;
(2) `/p:Nullable=enable` must not be added to the nullable gate; (3) `Thread.Sleep`, `Task.Delay`, and
wall-clock waits are banned in test code, which is why the #484 timer test uses the
`Timeout.Infinite` + `ObjectDisposedException` technique.
