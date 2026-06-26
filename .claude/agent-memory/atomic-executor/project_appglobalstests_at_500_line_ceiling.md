---
name: appglobalstests-at-500-line-ceiling
description: TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs sits exactly at the 500-line repo hard limit, so any plan adding an override/member to its TestableApplicationGlobals subclass must first extract or it breaks the file-size gate
metadata:
  type: project
---

`TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` is exactly 500 lines (verified 2026-06-24). Its private `TestableApplicationGlobals : ApplicationGlobals` subclass (declared ~line 429) no-op-overrides the `ApplicationGlobals` diagnosis seams (`LoadIntelConfigPhaseAsync`, `YieldWithContinuationProbeAsync`, `Load*PhaseAsync`, `StartStartupUiHeartbeat`, `StopStartupUiHeartbeat`, `BeginPhaseGcCapture`, `EmitPhaseGcDelta`).

**Why:** The repo General Code Change Policy 500-line hard limit (`.claude/rules/general-code-change.md`) applies to test code (only throwaway scripts, raw text fixtures, and Markdown are exempt). When `ApplicationGlobals` gains a new `protected internal virtual` live-read seam, all THREE subclasses must add an override (the other two — `ApplicationGlobalsStartupTimingTests.cs` at 317 and `ContinuationProbeSequenceTests.cs` at 123 — have headroom; this one does not).

**How to apply:** Any plan/task that adds a member to the `TestableApplicationGlobals` subclass in `ApplicationGlobalsTests.cs` must include a preceding file-size-remediation step (extract the subclass to its own file, or split the test file) so the file stays `<= 500`. A plan that adds the override without that step fails its own file-size QA gate and the repo policy. This was the sole preflight-blocking defect in the #211 Phase 3.6 plan (plan.2026-06-24T16-30.md): P4-T4 added the override but P6-T6 only allowed extraction for ApplicationGlobals.cs/StartupDiagnosticsProbe.cs, not the already-at-limit test file.
