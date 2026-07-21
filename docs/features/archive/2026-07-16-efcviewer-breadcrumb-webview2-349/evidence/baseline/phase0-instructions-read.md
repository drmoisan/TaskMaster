# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-07-18T08-41
Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md (per policy-compliance-order skill)

Files read (from the current worktree `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a7071cb39df527237`):

1. `CLAUDE.md` (sha256 ed6ca760280cb5d2ed07d6771a7a0042487f920739f4517bf61d01234b8653e8)
2. `.claude/rules/general-code-change.md` (sha256 f2b8683b12d10dd2add50e9bd30fbb5a3657231f676fdbf024165dcf4ea21889)
3. `.claude/rules/general-unit-test.md` (sha256 39e83d3a9dc8c11c12c24bdc5fbfe533a65c4c90e0f38c03ff4067933c5221ef)
4. `.claude/rules/csharp.md` (sha256 3e2fe077d79fe40eaf851d066404185f1fdcf6c05d7a8d98d662623ed629165a)

Key constraints noted for this feature:
- Toolchain order: csharpier -> analyzers msbuild -> nullable msbuild -> vstest with coverage; restart loop on any failure or file change.
- MSTest + Moq + FluentAssertions; Arrange-Act-Assert; no temp files; no external dependencies in tests.
- Coverage: repo floor >= 80% (testable denominator per CLAUDE.md COM/VSTO exemption); new modules >= 90% line; no changed-line regression.
- Banned APIs (BannedSymbols.txt / RS0030): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay.
- 500-line file cap; net48-safe types only (no record / record struct / init accessors); #nullable enable in new files.
- Legacy packages.config projects require explicit <Compile Include> items for every new .cs file.
