# Post-format census (P4-T2)

Timestamp: 2026-10-02T01-06
P4-RESTART: 0
Command: (1) one pwsh -NoProfile -Command payload after PREFIX re-running the P2-T4 commands (CMD-TOKEN-COUNT on QfcDatamodel.cs with the five P2-T4 tokens, CMD-SPAN-TOKEN-COUNT on INITQ, CMD-LINECOUNT over CODE5, the QFCDATAMODEL-BOM check) and the P3-T15 commands (the fourteen THREE tokens, the seven reason-literal tokens, the R4 file tokens, the four R4 spans, CMD-TIMESPAN); each Tok call prints its counts as one ROW line in token order. (2) git -C WORKTREE diff --numstat 34c2ed88cbb009f2f231453db87bc64d45a9bd51 -- QuickFiler/Controllers/QfcDatamodel.cs. (3) git -C WORKTREE diff 34c2ed88cbb009f2f231453db87bc64d45a9bd51 -- QuickFiler/Controllers/QfcDatamodel.cs. (4) git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test. (5) Re-anchoring companion, see below: git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 -- QuickFiler QuickFiler.Test.
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4

## P2-T4 values (QfcDatamodel.cs)
Tokens (property line, constructor default, "Injectable worker-start seam", "null on instances built by GetUninitializedObject", loader-assignment literal): 1, 2, 1, 1, 2
TRIMMED-EQUAL [worker.RunWorkerAsync();] = 0
INITQ: 271-316; RunWorkerAsync 0; WorkerStarter(worker); 2
QFCDATAMODEL-BOM: True

## CMD-LINECOUNT (each at most 500)
LINES QuickFiler\Controllers\QfcDatamodel.cs = 495
LINES QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs = 312
LINES QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs = 244
LINES QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs = 232
LINES QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 470

## P3-T15 values
THREE rows (fourteen tokens, P3-T15 order):
- Liveness: 0, 0, 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 0, 0
- Teardown: 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0
- Zero-batch: 0, 0, 0, 0, 3, 3, 1, 0, 0, 0, 0, 0, 0, 1
Reason-literal rows (seven tokens, P3-T15 order):
- Liveness: 1, 1, 1, 1, 1, 0, 0
- Teardown: 1, 0, 0, 0, 1, 0, 0
- Zero-batch: 0, 0, 0, 0, 1, 1, 0
R4 file (flake-watch, Append an observation, Issue #950:, [Timeout(GateTimeoutMs)], the constant, three R-DOC tokens): 0, 0, 1, 8, 1, 1, 1, 1
R4SPAN: 212-284; 1, 1, 2, 1, 1
R4PRE: 212-218; 0, 0
R4HEAD: 212-224; 1, 1
R4TAIL: 267-273; 3
TIMESPAN lines: liveness 139, 141, 156 (fake.Advance) and teardown 124, 155 (QuiesceLoaderAsync), 160 (fake.Advance), all CLASSIFIED
TIMESPAN-UNCLASSIFIED: 0

## Anchored diff of QfcDatamodel.cs
numstat: 14	2	QuickFiler/Controllers/QfcDatamodel.cs
Added lines: two `WorkerStarter = worker => worker.RunWorkerAsync();` constructor defaults, the blank line and nine S1 lines, and two `WorkerStarter(worker);` start sites (14). Deleted lines (2): both have the trimmed text `worker.RunWorkerAsync();` (hunks at BASE lines 273 and 300).

## Porcelain span and re-anchoring note
Porcelain (git status --porcelain -- QuickFiler QuickFiler.Test):
 M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
 M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs

PLAN-CORRECTION (re-anchoring only): the P4-T2 acceptance expects the porcelain span to list all five CODE5 paths with ` M`. The delegation required a commit at every phase boundary, so the Phase 2 commit (0700ee1e3) already holds QfcDatamodel.cs and the Phase 3 commit (709123349) holds the four test files as written. The working tree therefore differs from HEAD only in the two files the P4-T1 formatter rewrote. The purpose of the check (all five Write Set files changed, nothing else under QuickFiler/ or QuickFiler.Test/) is re-anchored to BASE with the companion name-status, which covers committed plus working-tree changes:
M	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
M	QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
M	QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
M	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
M	QuickFiler/Controllers/QfcDatamodel.cs
Exactly the five CODE5 paths, status M, and no other path under the code trees. Every porcelain path is a CODE5 path.

This artifact is the evidence for AC1, AC2, AC3 and AC14 and the census half of AC6 to AC13.
