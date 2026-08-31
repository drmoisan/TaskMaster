Timestamp: 2026-08-31T09-33
Policy Order:
1. AGENTS.md — standing instructions
2. AGENTS.md — Agent Code Change Policy
3. AGENTS.md — General Unit Test Policy
4. .agents/skills/csharp/SKILL.md

Distinct Files Read:
- AGENTS.md
- .agents/skills/csharp/SKILL.md

Recorded requirements:
- The file-size limit is 500 lines for production and test code.
- The C# toolchain loop is format, analyzer build, nullable build, then coverage-enabled test; it restarts when a step changes files or fails.
- Repository-wide line coverage remains at least 80 percent; new modules, classes, and methods target at least 90 percent; changed lines must not regress.
- Tests use MSTest, Moq, and FluentAssertions.
