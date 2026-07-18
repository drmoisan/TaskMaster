# Phase 0 — Policy / Instructions Read Evidence

Timestamp: 2026-07-18T00-00

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> language/domain rules (.claude/rules/csharp.md) -> .claude/rules/quality-tiers.md

Files read (in mandated policy-compliance order):
1. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abcc3c09e55e1b55a\CLAUDE.md
2. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abcc3c09e55e1b55a\.claude\rules\general-code-change.md
3. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abcc3c09e55e1b55a\.claude\rules\general-unit-test.md
4. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abcc3c09e55e1b55a\.claude\rules\csharp.md
5. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-abcc3c09e55e1b55a\.claude\rules\quality-tiers.md

Notes:
- CLAUDE.md, general-code-change.md, general-unit-test.md, and quality-tiers.md content was provided in-session and confirmed against the on-disk files.
- csharp.md read directly from disk this session.
- Applicable constraints for this feature: MSTest + Moq + FluentAssertions; CSharpier formatting; .NET analyzers; nullable/TreatWarningsAsErrors; no live Outlook/COM in tests; no temporary files; deterministic tests (no Task.Delay/Thread.Sleep/wall-clock); new code >= 90% coverage; every file < 500 lines; net48 constraint (no init/record/record struct).
- Toolchain (VS 18 Community): CSharpier 1.3.0 (global), MSBuild.exe at "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe", vstest.console.exe at "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe".
- Fresh worktree required nuget.exe restore of TaskMaster.sln (169 packages) before any build; completed prior to baseline capture.

Created before any Phase 1 change: yes.
