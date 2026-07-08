# Phase 0 — Policy Instructions Read (issue #211)

Timestamp: 2026-06-24T16-30

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md

Files read, in order:
1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/csharp.md

Notes:
- C# toolchain order confirmed: CSharpier (format) -> .NET analyzers (lint) -> nullable/TreatWarningsAsErrors (type-check) -> MSTest with coverage (test). Restart from CSharpier on any change/failure.
- Banned APIs (BannedApiAnalyzers, RS0030): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. Use Stopwatch only.
- Coverage policy: repo-wide line coverage >= 80%; new code >= 90%; no regression on changed lines.
- Test framework: MSTest + Moq + FluentAssertions; no external dependencies; no temporary files.
- Legacy (non-SDK) projects require explicit `<Compile Include>` for new .cs files.
