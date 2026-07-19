# Phase 0 — Policy Instructions Read Receipt

Timestamp: 2026-07-19T00-00

Policy Order:
1. CLAUDE.md (standing instructions, C# toolchain section)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and standards)

Files read (full contents reviewed):
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3211bcc5c56f78c6\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3211bcc5c56f78c6\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3211bcc5c56f78c6\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3211bcc5c56f78c6\.claude\rules\csharp.md

Note: this feature's plan (`plan.2026-07-18T22-05.md`, Open Questions) flags an unresolved
rules-vs-convention conflict: `.claude/rules/csharp.md` documents the type-check step as
`/p:Nullable=enable` globally, which conflicts with the epic's per-file `#nullable enable`
opt-in convention. Per the plan's explicit Scope Invariants and the issue/spec/user-story
constraints, this feature's verification uses the per-file pragma gate
(`msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`, no
`/p:Nullable=enable`) and defers resolution of the conflict to the Wave-2
`utilitiescs-nullable-ci-capstone` child, consistent with the approved (preflight-cleared) plan.
