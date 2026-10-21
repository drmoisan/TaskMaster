# Phase 0 — Instructions Read ([P0-T1])

Timestamp: 2026-09-08T00-12

Policy Order: The four ordered documents required by `.claude/skills/policy-compliance-order/SKILL.md` were read in this order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md` (the language-specific rule for the C# files in scope)

## Files read (all ten)

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `.claude/rules/plan-acceptance-gates.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`

The additional file `.claude/skills/policy-compliance-order/SKILL.md` was also read, because it is the document that defines the required order above.

## Constraints carried into execution

- C# toolchain order is format, lint, type-check, test, restarting from step 1 on any failure or any auto-fix (`CLAUDE.md`, `.claude/rules/csharp.md`).
- `dotnet format` is prohibited; formatting is CSharpier through `dotnet tool run` (`.claude/rules/csharp.md` item 1).
- Both MSBuild gates use `/t:Rebuild`; `/p:Nullable=enable` is not added (`.claude/rules/csharp.md` items 2 and 3).
- No production, test, or reusable script file may exceed 500 physical lines (`.claude/rules/general-code-change.md`, "File Size Limit").
- Tests use MSTest, Moq and FluentAssertions; no temporary files; no `Thread.Sleep`, `Task.Delay` or wall-clock waits in test code (`.claude/rules/csharp.md`, `.claude/rules/general-unit-test.md`).
- Repository-wide line coverage floor is 80% per `CLAUDE.md`, which takes precedence over the 85% figure in `.claude/rules/general-unit-test.md` under the precedence order in `.claude/skills/policy-compliance-order/SKILL.md`. New modules, classes and methods target 90%.
- Policy documents under `.claude/rules/` must not be modified.
- All evidence resolves under `<FEATURE>/evidence/<kind>/`; nothing under `artifacts/` (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`).
- Acceptance criteria are checked off one at a time in `spec.md` only, work mode being `full-bug` (`.claude/skills/acceptance-criteria-tracking/SKILL.md`).
- Tone in every artifact is factual and neutral; no humor, hyperbole or decorative metaphor (`.claude/rules/tonality.md`).
