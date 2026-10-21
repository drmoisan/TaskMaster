# Production seam census (P2-T1 to P2-T4)

Timestamp: 2026-10-02T01-01
Command: one pwsh -NoProfile -Command payload after PREFIX: CMD-TOKEN-COUNT on QuickFiler\Controllers\QfcDatamodel.cs (TOKENS "internal Action<BackgroundWorker> WorkerStarter { get; set; }", "WorkerStarter = worker => worker.RunWorkerAsync();", "Injectable worker-start seam", "null on instances built by GetUninitializedObject", "RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;"); CMD-SPAN-TOKEN-COUNT on INITQ (TOKENS "RunWorkerAsync", "WorkerStarter(worker);"); CMD-LINECOUNT; the plan's QFCDATAMODEL-BOM byte check; plus read-only placement echoes (property line against the #endregion line, and the line above each constructor default).
EXIT_CODE: 0

Edits applied (in place, BOM preserved): S1 inserted after line 140 (one blank line plus nine lines); S2 inserted after each constructor's `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` line; S3 replaced `worker.RunWorkerAsync();` at the two InitEmailQueue start sites (pre-edit lines 273 and 300) with `WorkerStarter(worker);`, keeping each line's indentation.

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
TOKEN [internal Action<BackgroundWorker> WorkerStarter { get; set; }] = 1
TOKEN [WorkerStarter = worker => worker.RunWorkerAsync();] = 2
TOKEN [Injectable worker-start seam] = 1
TOKEN [null on instances built by GetUninitializedObject] = 1
TOKEN [RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;] = 2
TRIMMED-EQUAL [worker.RunWorkerAsync();] = 0
INITQ SPAN: 271-316; SPAN-TOKEN [RunWorkerAsync] = 0; SPAN-TOKEN [WorkerStarter(worker);] = 2
LINES QuickFiler\Controllers\QfcDatamodel.cs = 495 (other CODE5 files unchanged: 255, 235, 212, 458)
QFCDATAMODEL-BOM: True
Placement: property at line 152, before `#endregion Private Variables` at line 154; constructor defaults at lines 41 and 53, each directly below a loader-assignment line.

All P2-T1 to P2-T4 acceptance values hold (tokens 1, 2, 1, 1, 2; TRIMMED-EQUAL 0; INITQ 0 and 2; LINES 495; BOM True).
