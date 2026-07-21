# Phase 0 — Instructions Read (Issue #283 Remediation)

Timestamp: 2026-07-08T18-52

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and standards)
5. .claude/rules/powershell.md (PowerShell-specific toolchain and standards)

Files Read (in order):
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-08-12-12\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-08-12-12\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-08-12-12\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-08-12-12\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-08-12-12\.claude\rules\powershell.md

Notes:
- This remediation is evidence-persistence plus a maintainer-ratified PowerShell coverage exemption. It makes no source-behavior change to the shipped fix (the seam `TaskMaster.Test/AppGlobals/LiveOutlookHarnessRunner.cs`, the integration test, `.github/workflows/ci.yml`, or the QC argument builders).
- Work Mode: minor-audit. Sole AC source: `docs/features/active/2026-07-08-liveoutlook-harness-construction-scoped-skip-283/issue.md` `## Acceptance Criteria`.
