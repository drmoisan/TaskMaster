# P0-T1 Policy Read

Timestamp: 2026-08-31T13:34:00Z

Policy Order:

1. `AGENTS.md` standing instructions
2. `AGENTS.md` general code-change instructions
3. `AGENTS.md` general unit-test instructions
4. `.agents/skills/csharp/SKILL.md`

Exact files read:

- `AGENTS.md`
- `.agents/skills/policy-compliance-order/SKILL.md`
- `.agents/skills/acceptance-criteria-tracking/SKILL.md`
- `.agents/skills/csharp/SKILL.md`

Relevant requirements:

- Production, test, and reusable script files must not exceed 500 lines.
- C# unit tests use MSTest, Moq, and FluentAssertions.
- Unit tests must not create or use temporary files.
- The C# toolchain runs CSharpier, analyzers, nullable analysis, then MSTest coverage, restarting after a failure or formatting change.
