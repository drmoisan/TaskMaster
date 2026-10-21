Timestamp: 2026-08-25T14-18
Policy Order: AGENTS.md -> .github/copilot-instructions.md -> .github/instructions/general-code-change.instructions.md -> .github/instructions/general-unit-test.instructions.md -> .github/instructions/csharp-code-change.instructions.md -> .github/instructions/csharp-unit-test.instructions.md -> .agents/skills/csharp/SKILL.md

Files read:
- AGENTS.md
- .github/copilot-instructions.md
- .github/instructions/general-code-change.instructions.md
- .github/instructions/general-unit-test.instructions.md
- .github/instructions/csharp-code-change.instructions.md
- .github/instructions/csharp-unit-test.instructions.md
- .agents/skills/csharp/SKILL.md

C# command order: dotnet tool run csharpier format .; msbuild analyzer rebuild; msbuild nullable/compiler rebuild with warnings as errors; MSTest coverage.

Test requirements: MSTest framework, Moq mocking, FluentAssertions preferred. Tests may not use temporary files, live Outlook, UI, network, external processes, or external services.

Coverage thresholds: repository line coverage at least 80 percent; new testable modules, classes, methods, or branches at least 90 percent; changed-line coverage may not regress.
