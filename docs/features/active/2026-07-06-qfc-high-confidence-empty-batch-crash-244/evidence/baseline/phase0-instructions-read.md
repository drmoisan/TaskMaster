# Phase 0 — Policy Read Evidence (Issue #244)

Timestamp: 2026-07-06T11-30

Policy Order:
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

Files read (in order):
1. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-11-13\CLAUDE.md`
2. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-11-13\.claude\rules\general-code-change.md`
3. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-11-13\.claude\rules\general-unit-test.md`
4. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-11-13\.claude\rules\csharp.md`

Output Summary: All four policy files read in the required order prior to any code change. Key obligations noted: MSTest + Moq + FluentAssertions for C# tests; csharpier -> analyzer build -> nullable build -> vstest toolchain order, restart-on-failure loop; bugfix workflow requires a failing regression test first, then a minimal targeted fix; evidence must live under `<FEATURE>/evidence/<kind>/`.
