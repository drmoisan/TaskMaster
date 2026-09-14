# Phase 0 — Instructions read (P0-T1)

Timestamp: 2026-09-13T00-51
Task: [P0-T1]
Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/quality-tiers.md -> .claude/rules/tonality.md -> .claude/rules/csharp.md

## Files read (in this order, each in full, from the item worktree root)

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/tonality.md`
6. `.claude/rules/csharp.md`

## Notes recorded from the reads

- The C# toolchain order in force is: `dotnet tool run csharpier format .` / `check .`, then `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, then `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (never `/p:Nullable=enable`), then `vstest.console.exe`.
- Test code bans `Thread.Sleep`, `Task.Delay`, wall-clock reads and temporary files; MSTest, Moq and FluentAssertions are the only permitted test libraries.
- No production, test or reusable script file may exceed 500 lines.
- All agent-authored content follows the tonality policy (professional, evidence-first, no hyperbole).
