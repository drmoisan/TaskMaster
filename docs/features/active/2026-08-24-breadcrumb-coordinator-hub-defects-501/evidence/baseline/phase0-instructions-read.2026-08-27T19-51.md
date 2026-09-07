# Phase 0 — Policy Documents Read (P0-T1)

Timestamp: 2026-08-27T19-51

Policy Order: the order defined by `.claude/skills/policy-compliance-order/SKILL.md`, extended by the
task text of [P0-T1] to include the plan-acceptance-gate rules and the two contract skills.

Files read, in order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/plan-acceptance-gates.md`
6. `.claude/skills/atomic-plan-contract/SKILL.md`
7. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`

All seven paths are relative to `WS`. Line counts observed at read time: 447, 80, 105, 96, 128, 204, 176.

## Binding constraints carried forward into execution

- Toolchain order is format -> lint -> type-check -> test; restart from step 1 on any failure or file change.
- `/t:Rebuild` is mandatory; `/t:Build` is prohibited (a warm incremental build skips `CoreCompile` and runs no analyzers).
- `/p:Nullable=enable` must NOT be added; nullable is per-file opt-in via `#nullable enable`.
- CSharpier must be invoked as `dotnet tool run csharpier`; a global install is prohibited.
- Tests: MSTest + Moq + FluentAssertions; no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temporary file, no external dependency.
- No production, test, or reusable script file may exceed 500 physical lines.
- Evidence paths resolve only to `<FEATURE>/evidence/<kind>/`; paths under `artifacts/` are forbidden.
- Timestamps use `yyyy-MM-ddTHH-mm`; no absolute host path may appear in any artifact.
