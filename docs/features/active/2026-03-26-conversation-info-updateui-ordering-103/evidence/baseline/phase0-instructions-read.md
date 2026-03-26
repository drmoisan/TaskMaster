# Phase 0 — Instructions Read Evidence

- Timestamp: 2026-03-26T18:48:00
- Policy Order:
  1. `CLAUDE.md`
  2. `.github/instructions/general-code-change.instructions.md`
  3. `.github/instructions/general-unit-test.instructions.md`
  4. `.github/instructions/csharp-code-change.instructions.md`
  5. `.github/instructions/csharp-unit-test.instructions.md`

## Files Read (in order)

1. CLAUDE.md — General code change policy, C# code change policy, general unit test policy, C# unit test policy, tone policy, toolchain commands
2. .github/instructions/general-code-change.instructions.md — Design principles, module structure, post-change toolchain loop
3. .github/instructions/general-unit-test.instructions.md — Core test principles, coverage targets (>= 80% repo, >= 90% new code), scenario completeness, AAA pattern
4. .github/instructions/csharp-code-change.instructions.md — csharpier formatting, .NET analyzers, nullable analysis, C# naming and design
5. .github/instructions/csharp-unit-test.instructions.md — MSTest framework, Moq for mocking, FluentAssertions for assertions, toolchain command sequence

## Key Policy Constraints Noted

- Formatter: `dotnet tool run csharpier format .` (not `dotnet format`)
- No new xUnit/NUnit; MSTest only
- FluentAssertions preferred; MSTest Assert fallback
- Temporary files in tests: PROHIBITED
- Toolchain order: format → lint → nullable-build → test (restart from step 1 if any step fails or changes files)
- Bug workflow: failing regression test FIRST, then minimal fix, then verify
- File size limit: 500 lines (ConversationResolverTests.cs must stay under 500 lines)
