# Phase 0 — Instructions Read (#211 Phase 3.2 all-phase UI-heartbeat + per-phase GC probe)

Timestamp: 2026-06-23T22-30

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code + test standards)

Files Read (explicit list, in order):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md

Notes:
- Work mode for this plan: full-bug (diagnosis-only, behavior-preserving instrumentation). Per plan, AC source is `spec.md`; the increment introduces AC14, tracked via the P5-T7 check-off artifact.
- Toolchain order (mandatory, restart from step 1 on any change/failure): CSharpier -> .NET analyzers -> nullable/TWAE -> MSTest with coverage (`/TestCaseFilter:"TestCategory!=LiveOutlook"`).
- Banned APIs (BannedSymbols.txt): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. Stopwatch only for interval timing.
- Scope: widen the existing Engines-only heartbeat + GC probe to span the entire LoadSequentialAsync with per-phase annotation and per-phase GC deltas. Behavior-preserving; no latency fix; no phase order/set/semantics change; PreserveReferencesHandling.All untouched. net48.
- All touched files (production AND test) must remain <= 500 lines. ApplicationGlobalsTests.cs is at 500 lines at baseline; seam-override edits must not increase it.
- Evidence root (canonical, non-overridable): docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/.
