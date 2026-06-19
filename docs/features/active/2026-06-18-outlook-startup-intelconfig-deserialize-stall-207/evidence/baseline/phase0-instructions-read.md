# Phase 0 — Instructions Read (Issue #207, increment 2)

Timestamp: 2026-06-19T23-35

Policy Order:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code and test standards)

Files read (explicit list):
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\tonality.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\ci-workflows.md

Requirements source (minor-audit AC source):
- docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/issue.md -> ## Acceptance Criteria (AC1-AC6, increment 2)

Output Summary: All four required policy files plus the tonality and CI-workflow rules were read in the required order for increment 2. Work Mode is minor-audit; the sole AC source is issue.md's `## Acceptance Criteria` section. Scope for increment 2 is locked to THREE files: TaskMaster/AppGlobals/AppEvents.cs, UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs, and UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs. net48 target: no positional record struct (CS0518/IsExternalInit) - reuse readonly struct + explicit constructor. Banned APIs (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay) must not be introduced; timing must use System.Diagnostics.Stopwatch. AppEvents.Hook() is COM/VSTO logging-only exempt (verified by inspection).
