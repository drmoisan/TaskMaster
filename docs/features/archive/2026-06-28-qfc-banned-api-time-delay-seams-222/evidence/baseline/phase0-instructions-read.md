# Phase 0 — Instructions Read (P0-T2)

Timestamp: 2026-06-28T19-00

Policy Order:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific policy)
5. BannedSymbols.txt (banned-API enforcement list)

Files Read:
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-28-18-49\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-28-18-49\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-28-18-49\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-28-18-49\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-28-18-49\BannedSymbols.txt
- Supporting: .claude/rules/ci-workflows.md, .claude/rules/tonality.md (auto-loaded)

Key constraints confirmed:
- BannedSymbols.txt bans DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep(int/TimeSpan), Task.Delay(int/TimeSpan). Replacement `TimeProvider.Delay(...)` is NOT a banned symbol.
- Toolchain order: csharpier -> analyzer build -> nullable build (TreatWarningsAsErrors) -> vstest with coverage.
- File size limit: <= 500 lines per file.
- Tests: MSTest + Moq + FluentAssertions; no external deps; no temp files; >= 90% new code, >= 80% repo-wide.
