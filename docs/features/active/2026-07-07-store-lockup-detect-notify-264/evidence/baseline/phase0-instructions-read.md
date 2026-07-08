# Phase 0 — Policy Instructions Read (P0-T1 / P0-T2)

Timestamp: 2026-07-08T07-54

Policy Order:
1. CLAUDE.md (standing project instructions — all sections)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards)

Files read start-to-end (absolute paths, this worktree):
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a9482bb2b78a348e7\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a9482bb2b78a348e7\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a9482bb2b78a348e7\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a9482bb2b78a348e7\.claude\rules\csharp.md

Note on plan P0-T1 path reference: the plan text names
`C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-07-13-21\CLAUDE.md`, an author-time
worktree path. The equivalent canonical files were read from the active execution worktree
(paths above); content is identical policy text.

Governing coverage floors (CLAUDE.md authority): repository line coverage >= 80% on the
testable denominator (COM/VSTO/WinForms exemption per CLAUDE.md), new code >= 90%. The
85%/75% figures in the .claude/rules summaries are superseded by CLAUDE.md 80/90 for this work.
