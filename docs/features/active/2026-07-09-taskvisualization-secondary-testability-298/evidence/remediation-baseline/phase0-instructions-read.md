# Phase 0 Instructions Read — Remediation Cycle 1 (#298)

Timestamp: 2026-07-10T07-55

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md

Files Read:
- C:/Users/DanMoisan/repos/TaskMaster-wt/winforms-298/CLAUDE.md
- C:/Users/DanMoisan/repos/TaskMaster-wt/winforms-298/.claude/rules/general-code-change.md
- C:/Users/DanMoisan/repos/TaskMaster-wt/winforms-298/.claude/rules/general-unit-test.md
- C:/Users/DanMoisan/repos/TaskMaster-wt/winforms-298/.claude/rules/csharp.md

Notes:
- All four policy documents were read in full this cycle before any edits.
- C# toolchain order confirmed: csharpier (format) -> msbuild analyzers -> msbuild nullable/TreatWarningsAsErrors -> vstest with coverage.
- Test framework: MSTest + Moq + FluentAssertions. No live forms, no popups, no Thread.Sleep/Task.Delay, no temp files.
