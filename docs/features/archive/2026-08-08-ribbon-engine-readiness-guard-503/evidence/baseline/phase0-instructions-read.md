# Phase 0 Instructions Read — Issue #503 (P0-T2)

Timestamp: 2026-08-08T13-03

Policy Order: as defined by `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\skills\policy-compliance-order\SKILL.md` — (1) `CLAUDE.md`, (2) `.claude/rules/general-code-change.md`, (3) `.claude/rules/general-unit-test.md`, (4) language/domain-specific rules for the files in scope (C#), then the remaining repository rules that bear on this change.

Files read, in order, with absolute paths:

1. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\CLAUDE.md`
2. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\general-code-change.md`
3. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\general-unit-test.md`
4. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\csharp.md`
5. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\architecture-boundaries.md`
6. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\quality-tiers.md`
7. `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\rules\tonality.md`

Supporting skill files read for execution mechanics:

- `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\skills\policy-compliance-order\SKILL.md`
- `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\skills\atomic-plan-contract\SKILL.md`
- `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\skills\evidence-and-timestamp-conventions\SKILL.md`
- `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\.claude\skills\acceptance-criteria-tracking\SKILL.md`

Output Summary: All seven policy files were read in the mandated order before any implementation task ran. Binding constraints extracted for this change: MSTest + Moq + FluentAssertions only; no temporary files in tests; no `Thread.Sleep` / `Task.Delay` / direct wall-clock reads in tests; no broad `catch (Exception)` without a defined boundary; 500-line file cap on production, test, and reusable script files (Markdown exempt); toolchain order format -> lint -> type-check -> test with a restart from step 1 on any failure or file mutation; CSharpier is the formatter and `dotnet format` is prohibited; new modules/classes/methods must reach >= 90% coverage; `[ComVisible(true)]` must not be added to new runtime code; new runtime code must not add `Microsoft.Office.Interop.Outlook` or `Microsoft.Office.Tools.*` references.

EXIT_CODE: 0
