# Phase 0 — Policy Documents Read (P0-T1)

Timestamp: 2026-07-16T09-00

Policy Order:
1. CLAUDE.md (standing project instructions — General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, Tone Policy, C# Toolchain)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards)

Files read (explicit list):
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a4422ad2fac1beb0b\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a4422ad2fac1beb0b\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a4422ad2fac1beb0b\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a4422ad2fac1beb0b\.claude\rules\csharp.md

Output Summary: All four policy documents read in policy-compliance order prior to any code change.
Key constraints acknowledged: MSTest + Moq + FluentAssertions; csharpier -> analyzer msbuild -> nullable/TreatWarningsAsErrors msbuild -> vstest with coverage (restart loop on any failure/auto-fix); net48 forbids record/record struct/init (no IsExternalInit polyfill) — use plain classes or readonly structs with explicit constructors; both test projects are non-SDK net4.8.1 with no glob compile (every new .cs needs an explicit <Compile Include>); coverage thresholds line >= 85%, branch >= 75%, new-module >= 90%; 500-line file limit; evidence only under this feature's evidence/{baseline,regression-testing,qa-gates}/.
