Timestamp: 2026-08-25T12-47
Command: Get-Content -Raw for each required policy file
EXIT_CODE: 0
Output Summary: Required repository policies were read before baseline execution. The C# unit-test policy requires MSTest, Moq, and FluentAssertions. Unit tests must not create temporary files and must retain required coverage. The C# loop is CSharpier format, analyzers, nullable analysis, then MSTest coverage testing.

# Phase 0 policy-read record

Policy Order:

1. `AGENTS.md`
2. `.github/copilot-instructions.md`
3. `.github/instructions/general-code-change.instructions.md`
4. `.github/instructions/general-unit-test.instructions.md`
5. `.github/instructions/csharp-code-change.instructions.md`
6. `.github/instructions/csharp-unit-test.instructions.md`
7. `.agents/skills/csharp/SKILL.md`

Files read:

- `AGENTS.md`
- `.github/copilot-instructions.md`
- `.github/instructions/general-code-change.instructions.md`
- `.github/instructions/general-unit-test.instructions.md`
- `.github/instructions/csharp-code-change.instructions.md`
- `.github/instructions/csharp-unit-test.instructions.md`
- `.agents/skills/csharp/SKILL.md`

Confirmed policy controls:

- Tests use MSTest, Moq, and FluentAssertions.
- Tests do not create temporary files or access external services.
- Repository line coverage remains at least 80 percent; new modules, classes, and methods target at least 90 percent.
- C# verification runs in this order: CSharpier formatting, .NET analyzer build, nullable/compiler build, and MSTest coverage testing.
