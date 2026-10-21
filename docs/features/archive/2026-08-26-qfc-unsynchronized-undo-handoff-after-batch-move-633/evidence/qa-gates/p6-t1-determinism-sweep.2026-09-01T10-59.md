# Determinism sweep over the three changed test files (P6-T1)

Timestamp: 2026-09-01T10-59
Task: [P6-T1]
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/regexscan.ps1` searching each file with
`Select-String -Pattern 'Thread\.Sleep|Task\.Delay|\.Wait\(|\.Result\b|DateTime\.(Now|UtcNow)'`
EXIT_CODE: 0

## Per-file match counts

| File | Matches |
|---|---|
| `QuickFiler.Test/Controllers/FilerQueueTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 0 |

Total match count across the three files: **0**.

Output Summary: No banned wait API appears in any added or modified test code.
`.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, and every real wall-clock wait
in test code, and this sweep also covers the blocking `.Wait(` and `.Result` forms and direct clock
reads.

The gate is satisfied by construction rather than by avoidance. Every concurrency assertion added by
this change is ordered by an explicit signal:

- the queue-level tests gate the `ItemProcessor` seam on `TaskCompletionSource<bool>` instances created
  with `TaskCreationOptions.RunContinuationsAsynchronously`, and wait on an entry signal the processor
  itself sets, so the worker's arrival is observed rather than assumed;
- the two barrier tests order the "has the method dispatched yet" question by posting a probe operation
  to the same pinned dispatcher at equal priority and awaiting it, which converts a timing question into
  a dispatcher enqueue-order fact;
- the repaired `SeamFactoryTests` case awaits a `TaskCompletionSource<FilerQueueItem>` that the queue
  worker completes, replacing a `Queue.Count` read whose value depended on when the worker happened to
  have run.

This artifact supplies the evidence for the AC14 check-off in P8-T18.
