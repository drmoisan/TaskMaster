# P0-T2 — Phase 0 Instructions Read

Issue: #230
Task: [P0-T2]

Timestamp: 2026-08-07T21-30

## Policy Order

Read in the exact sequence mandated by
`.claude/skills/policy-compliance-order/SKILL.md`:

1. `CLAUDE.md` — standing repository instructions (all sections, including the
   embedded General Code Change Policy, General Unit Test Policy, C# Code Change
   Policy, C# Unit Test Policy, Tone Policy, and the C# toolchain ordering).
2. `.claude/rules/general-code-change.md` — cross-language code change policy
   (design principles, module rigor tiers, mandatory toolchain loop, 500-line
   file limit, error handling, naming, I/O boundaries).
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy (core
   principles, coverage requirements, coverage exclusion policy, scenario
   completeness, Arrange-Act-Assert, external-dependency prohibition, test file
   location, determinism infrastructure and banned APIs).
4. `.claude/rules/csharp.md` — C#-specific toolchain and coding standards
   (CSharpier, .NET analyzers, nullable analysis, MSTest/Moq/FluentAssertions,
   deterministic test rules, DI seams, analyzer stack, prohibited behaviors).

## Files Read

### Policy documents (in order)

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`

### Additional repository rules loaded in this session

- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `.claude/rules/benchmark-baselines.md`
- `.claude/rules/ci-workflows.md`
- `.claude/rules/orchestrator-state.md`

### Skill documents

- `.claude/skills/policy-compliance-order/SKILL.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

### Feature documents

- `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/spec.md`
  (design constraints; 13 acceptance criteria at lines 244-296)
- `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/user-story.md`
  (6 acceptance criteria at lines 70-86)
- `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/issue.md`
- `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/research/2026-08-07T21-00-winforms-message-pump-seam-research.md`
  (design of record)
- `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/plan.2026-08-07T20-36.md`
  (plan of record, Version 1.2)

## Constraints Carried Into Execution

- MSTest + Moq + FluentAssertions only (CUT1/CUT2).
- No temporary files in tests (no approved exceptions).
- No `Thread.Sleep`, `Task.Delay`, wall-clock polling, or unbounded waits without
  a deterministic signal (general-unit-test.md Determinism Infrastructure; the
  same APIs are listed in `BannedSymbols.txt`).
- net481: no `init` accessors, no `record`, no `record struct` (CS0518).
- No non-markdown file over 500 lines.
- Toolchain order: csharpier format -> analyzer msbuild -> nullable msbuild ->
  coverage-enabled vstest; restart from step 1 on any failure or file change.
- Evidence paths resolve only under
  `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/<kind>/`.
