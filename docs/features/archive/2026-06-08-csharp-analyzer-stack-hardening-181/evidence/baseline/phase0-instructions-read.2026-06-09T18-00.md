# Phase 0 — Policy Instructions Read (Cycle 7)

Timestamp: 2026-06-09T18-00

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific rules; language in scope is C#)

Files read (in order):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md

Key constraints acknowledged for this cycle:
- C# toolchain order: csharpier -> analyzer msbuild -> nullable msbuild -> vstest /EnableCodeCoverage /InIsolation; restart on any change/failure.
- MSTest + Moq + FluentAssertions only; no temp files; no external dependencies in tests.
- Banned symbols (BannedApiAnalyzers RS0030): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. None to be introduced.
- Repo-wide line coverage >= 80%; new/changed code targets >= 90%; no changed-line coverage regression.
- Prefer internal for non-public APIs (S8 inner-timer interface is internal).
- Authorized production files this cycle: TimeOutTask.cs, OlTableExtensions.TableAccess.cs, TimerWrapper.cs. IGenericTimer.cs NOT touched (plan option (b)).
