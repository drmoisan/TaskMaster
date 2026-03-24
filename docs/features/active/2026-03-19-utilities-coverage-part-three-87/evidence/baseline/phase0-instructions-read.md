# Phase 0 — Policy Instructions Read

Timestamp: 2026-03-23T00:00:00Z

Policy Order:
1. `.github/copilot-instructions.md` — Tone policy; MSTest/Moq/FluentAssertions requirement for C#
2. `.github/instructions/general-code-change.instructions.md` — Design principles, toolchain loop (format → lint → type-check → test), file structure, naming, error handling
3. `.github/instructions/general-unit-test.instructions.md` — Independence, isolation, determinism, coverage thresholds (≥80% repo-wide, ≥90% new modules), no external deps, no temp files
4. `.github/instructions/csharp-code-change.instructions.md` — csharpier format, .NET analyzers, nullable/type-safety, C# design principles
5. `.github/instructions/csharp-unit-test.instructions.md` — MSTest framework, Moq mocking, FluentAssertions assertions, C# toolchain commands

Files Read:
- `.github/copilot-instructions.md` — read (loaded via session instruction attachment)
- `.github/instructions/general-code-change.instructions.md` — read (loaded via session instruction attachment)
- `.github/instructions/general-unit-test.instructions.md` — read (loaded via session instruction attachment)
- `.github/instructions/csharp-code-change.instructions.md` — read (read_file tool, lines 1–50)
- `.github/instructions/csharp-unit-test.instructions.md` — read (read_file tool, lines 1–50)

Key constraints noted for this plan:
- All new tests: MSTest ([TestClass]/[TestMethod]), Moq for mocking, FluentAssertions for assertions
- Tests must be deterministic, isolated, no external deps, no temporary filesystem files
- All new test files must be registered in UtilitiesCS.Test.csproj
- Toolchain loop: csharpier → analyzer build → nullable build → vstest with coverage
- Coverage threshold: ≥80% repo-wide, ≥90% for new modules/classes
