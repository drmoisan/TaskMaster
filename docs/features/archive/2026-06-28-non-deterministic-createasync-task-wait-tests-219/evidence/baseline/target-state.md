# Phase 0 — Target Baseline State (Issue #219)

Timestamp: 2026-06-28T19-51

Command: Grep pattern `task\.Wait\(TimeSpan` over
UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs (output_mode content, line numbers),
plus direct Read of lines 640-728.

EXIT_CODE: 0

Output Summary:
- Forbidden pattern present (VisibleLabel method):
  line 719: `bool completed = task.Wait(TimeSpan.FromSeconds(10));`
  This method `CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState` is declared
  `public void` (line 696) and uses the timeout-based `completed` assertion (lines 720-722),
  `task.Exception.Should().BeNull(...)` (line 723), and `task.Result.Should().NotBeNull(...)`
  (lines 724-725).
- Forbidden pattern absent (HiddenLabel sibling):
  `CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails` (line 654) is
  already declared `public async Task`, uses `var details = await Task.Run(...)` (line 658),
  and asserts directly `details.Should().NotBeNull(...)` (line 679). No `Task.Wait` occurs in
  this method.
- Grep over the whole file returned exactly one match (line 719), confirming the VisibleLabel
  method is the only remaining occurrence of the forbidden `Task.Wait(TimeSpan)` pattern.
