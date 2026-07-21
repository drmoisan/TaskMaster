# Phase 0 — Policy Instructions Read Evidence (P0-T1)

Timestamp: 2026-07-18T08-41

Policy Order:
1. `CLAUDE.md` (all sections, including the embedded General Code Change Policy, C# Code Change Policy, General Unit Test Policy, C# Unit Test Policy, and Tone Policy)
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md` plus the `## C# Code Change Policy` and `## C# Unit Test Policy` sections of `CLAUDE.md`

Files read (actual absolute paths in the execution worktree `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad8430e58353ba09b`):
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad8430e58353ba09b\CLAUDE.md` (441 lines; SHA-256 ed6ca760280cb5d2ed07d6771a7a0042487f920739f4517bf61d01234b8653e8)
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad8430e58353ba09b\.claude\rules\general-code-change.md` (80 lines; SHA-256 f2b8683b12d10dd2add50e9bd30fbb5a3657231f676fdbf024165dcf4ea21889)
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad8430e58353ba09b\.claude\rules\general-unit-test.md` (105 lines; SHA-256 39e83d3a9dc8c11c12c24bdc5fbfe533a65c4c90e0f38c03ff4067933c5221ef)
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad8430e58353ba09b\.claude\rules\csharp.md` (96 lines, read in full: toolchain order, coding standards, testing standards, deterministic test rules, DI seams, TimeProvider guidance, analyzer stack, prohibited behaviors)
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad8430e58353ba09b\.github\copilot-instructions.md` (10 lines, read in full)

Notes:
- The orchestrator session context stated `.github/copilot-instructions.md` and `.github/instructions/*` do not exist in this repo; in fact both exist in this worktree. `.github/copilot-instructions.md` (10 lines) was read and is consistent with CLAUDE.md (MSTest + Moq + FluentAssertions, strict toolchain order, professional tone). `.github/instructions/` contains per-language instruction files (`csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, etc.) whose policy content is embedded verbatim in `CLAUDE.md` per its `## Policy Compliance Order` section; CLAUDE.md and `.claude/rules/*` are the authoritative equivalents applied for this execution.
- `.claude/rules/csharp.md` exists in this worktree (the session context anticipated it might be absent); it was read directly.
- CLAUDE.md, general-code-change.md, and general-unit-test.md were verified byte-identical (SHA-256) between this execution worktree and the primary session worktree copies loaded in full into the executing agent's context, confirming the full text of all sections was read.

Key binding constraints extracted for this plan:
- Toolchain order: csharpier format -> analyzer build -> nullable/TreatWarningsAsErrors build -> vstest with coverage; restart from format on any failure or file change.
- MSTest + Moq + FluentAssertions; no temp files in tests; no external dependencies; deterministic tests.
- Repository line coverage >= 80% (testable denominator, COM/VSTO exemption ratified); new code >= 90%; no coverage regression on changed lines.
- 500-line file ceiling for production and test files.
- Fail fast, explicit exceptions; XML docs on non-obvious public contracts; PascalCase/camelCase naming.
- No new NuGet packages without approval; net4.8.1 non-SDK constraints (no record/init).
