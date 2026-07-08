# Phase 0 Instructions Read — Remediation Cycle 1 (Issue #232)

Timestamp: 2026-07-03T16-58

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md

Files read (in required order):
1. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-03-10-11\CLAUDE.md` (standing instructions; loaded)
2. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-03-10-11\.claude\rules\general-code-change.md`
3. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-03-10-11\.claude\rules\general-unit-test.md`
4. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-03-10-11\.claude\rules\csharp.md`

Additional supporting rule files loaded via project context: `.claude/rules/ci-workflows.md`, `.claude/rules/tonality.md`.

Notes:
- C# is the only changed-source language for this remediation cycle.
- Toolchain order enforced: CSharpier -> msbuild analyzers -> msbuild nullable/TreatWarningsAsErrors -> vstest with Cobertura coverage.
- Coverage policy: repo-wide floor 80% governed by the ratified COM/VSTO/WinForms exemption (testable denominator ~76.57% baseline); changed non-exempt file target >= 90%.
