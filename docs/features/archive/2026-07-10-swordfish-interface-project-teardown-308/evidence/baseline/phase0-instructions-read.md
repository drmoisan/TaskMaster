# Phase 0 — Policy-Read Evidence (P0-T1)

- **Timestamp:** 2026-07-11T12-52
- **Feature:** swordfish-interface-project-teardown (#308), epic swordfish-removal child F5
- **Branch:** feature/swordfish-interface-project-teardown-308 @ 1b65f7a7 (integration tip)
- **Worktree:** C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-ad68b716cb2fe3638

## Policy Order

The four policy documents were read fresh from this worktree in the required
policy-compliance order:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and coding standards)

## Files Read (in order)

- C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-ad68b716cb2fe3638/CLAUDE.md
- C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-ad68b716cb2fe3638/.claude/rules/general-code-change.md
- C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-ad68b716cb2fe3638/.claude/rules/general-unit-test.md
- C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-ad68b716cb2fe3638/.claude/rules/csharp.md

## Key constraints applied to this execution

- C# toolchain order: csharpier -> analyzers msbuild -> nullable msbuild -> vstest.console.exe /EnableCodeCoverage; restart from step 1 on any failure or auto-fix.
- Repo-wide line coverage >= 80% on the testable denominator; changed/new code >= 90%.
- Evidence artifacts resolve only to `<FEATURE>/evidence/<kind>/` (no `artifacts/` evidence paths).
- F5 stays strictly within its deletion scope; any out-of-scope first-party Swordfish binding is a HALT.
