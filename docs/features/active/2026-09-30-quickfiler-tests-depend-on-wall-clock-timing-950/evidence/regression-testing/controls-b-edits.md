# Negative-control batch B edit census (P5-T13, P5-T14)

Timestamp: 2026-10-02T01-12
Command: CMD-TOKEN-COUNT on QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs (TOKENS "WorkerStarter = StartSynchronously;", "WorkerStarter = _ => { };") in one pwsh -NoProfile -Command payload after PREFIX, plus a read-only echo of the no-op line and the line after it; then git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test and git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test (separate calls).
EXIT_CODE: 0

Edit applied (temporary; reverted by P5-T17):
- B1 (StartHeldOpenLoader, used by test 2 RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces): `model.WorkerStarter = StartSynchronously;` replaced with `model.WorkerStarter = _ => { };`

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
TOKEN [WorkerStarter = StartSynchronously;] = 1 (test 1)
TOKEN [WorkerStarter = _ => { };] = 1 (line 202, inside StartHeldOpenLoader, directly above `model.InitEmailQueue(0, worker);`)
numstat: 1	1	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
porcelain:  M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (only that path)
