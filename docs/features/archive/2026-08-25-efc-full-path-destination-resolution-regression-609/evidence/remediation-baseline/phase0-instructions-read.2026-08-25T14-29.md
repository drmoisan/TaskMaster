Timestamp: 2026-08-25T14-41
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

Required C# toolchain order: `dotnet tool run csharpier format .`, CSharpier check, analyzer rebuild, nullable/compiler rebuild, then coverage-enabled MSTest; restart from formatting if a gate changes files or fails.

Test requirements: MSTest with Moq and FluentAssertions; tests must be deterministic and must not create temporary files, use external processes, network services, or Outlook UI/COM integration.

Coverage thresholds: repository line coverage must remain at least 80 percent; new or changed testable behavior must target at least 90 percent coverage, with no coverage reduction on changed lines.
