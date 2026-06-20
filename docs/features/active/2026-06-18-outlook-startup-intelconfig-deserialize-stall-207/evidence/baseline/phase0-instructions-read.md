# Phase 0 — Instructions Read (Issue #207, increment 3)

Timestamp: 2026-06-19T21-15

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code + test standards)

Files read (in required order):
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\csharp.md

Supporting rules also in context:
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\tonality.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\ci-workflows.md

Requirements source (minor-audit AC source):
- docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/issue.md -> ## Acceptance Criteria — Increment 3 (OlReminders latency probe) (I3-AC1..I3-AC6)

Output Summary: All four required policy files plus tonality and CI-workflow rules read in the mandated order for increment 3. Work Mode is minor-audit; the sole AC source is the issue.md `## Acceptance Criteria — Increment 3` section. Scope is locked to six files: Settings.settings, Settings.Designer.cs, app.config, AppEvents.cs, new RemindersProbeSchedule.cs, and new RemindersProbeScheduleTests.cs. net48 target: no positional record struct (CS0518/IsExternalInit) — use plain class or readonly struct with explicit constructor. Banned APIs (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay) must not be introduced; latency uses System.Diagnostics.Stopwatch, the delay uses System.Windows.Threading.DispatcherTimer. 500-line file cap honored. AppEvents.Hook() DispatcherTimer/COM wiring is COM/VSTO logging-only exempt (verified by inspection); only RemindersProbeSchedule carries the new-code coverage obligation.
