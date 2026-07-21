# Phase 0 — Policy Instructions Read (P0-T1)

- Timestamp: 2026-07-10T23:05
- Policy Order:
  1. `CLAUDE.md`
  2. `.claude/rules/general-code-change.md`
  3. `.claude/rules/general-unit-test.md`
  4. `.claude/rules/csharp.md`

## Files Read (in order, current worktree root)

1. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a07a8dff4c16f3a93\CLAUDE.md`
2. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a07a8dff4c16f3a93\.claude\rules\general-code-change.md`
3. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a07a8dff4c16f3a93\.claude\rules\general-unit-test.md`
4. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a07a8dff4c16f3a93\.claude\rules\csharp.md`

## Note on Path Correction

The plan's P0-T1 task text referenced a stale preparation-worktree absolute path
(`C:\Users\DanMoisan\repos\TaskMaster-wt\swordfish-removal-integration\...`) which no
longer exists. Per explicit orchestrator instruction, the four policy files were instead
read from the current worktree root listed above (repo-relative paths are identical;
only the absolute worktree prefix differs). No policy content differs between the stale
and current path — this is a path resolution correction only, not a scope change.

## Key Requirements Confirmed

- C# toolchain: CSharpier (format) -> .NET analyzers (lint) -> nullable/TreatWarningsAsErrors (type-check) -> MSTest via vstest.console.exe with `/EnableCodeCoverage` (test), in that exact order, restart on any failure or file change.
- MSTest + Moq + FluentAssertions required for C# tests.
- Repository-wide line coverage >= 80% baseline (CLAUDE.md coverage policy); no regression on changed lines.
- File size limit 500 lines for production/test/script files (not applicable to this deletion-only change).
- Evidence must live under canonical `<FEATURE>/evidence/<kind>/` paths only.
- Scope lock for this plan: delete `ScoSortedDictionary.cs` and `ScoSortedDictionary_Tests.cs`; remove the two matching `<Compile Include>` csproj entries; no other production code, `ProjectReference`, or `TaskMaster.sln` change.
