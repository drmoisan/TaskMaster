# Phase 1 temporary-edit census (P1-T1, P1-T2, P1-T3)

Timestamp: 2026-10-02T00-58
Command: CMD-TOKEN-COUNT on QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs (TOKENS "public void Issue950_AmbientSynchronizationContextProbe()", "PROBE-ALWAYS-FAILS"); CMD-SPAN-TOKEN-COUNT on R4SPAN (TOKENS "EnsureUiThreadDispatcher()"), plus a read-only echo of lines 213 to 218 to show placement; both in one pwsh -NoProfile -Command payload after PREFIX. Then git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs and git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test (separate calls).
EXIT_CODE: 0

Edits applied (temporary; reverted by P1-T7):
- P1-T1: Delivered Source P-PROBE inserted after liveness line 253 (one blank line, then the probe test).
- P1-T2: Delivered Source R-INJECT (`QfcItemControllerTestSupport.EnsureUiThreadDispatcher();`, sixteen spaces) inserted immediately before `transactionA.Install(liveA);` on the unmodified R4 file.

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
TOKEN [public void Issue950_AmbientSynchronizationContextProbe()] = 1
TOKEN [PROBE-ALWAYS-FAILS] = 1
R4SPAN SPAN: 206-273
SPAN-TOKEN [EnsureUiThreadDispatcher()] = 1
Placement: line 215 `Dispatcher original = UiThreadDispatcherFixture.Current;`, line 216 `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();`, line 217 `transactionA.Install(liveA);`
numstat:
15	0	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
1	0	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
porcelain:
 M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
 M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs

All P1-T1, P1-T2 and P1-T3 acceptance values hold.
