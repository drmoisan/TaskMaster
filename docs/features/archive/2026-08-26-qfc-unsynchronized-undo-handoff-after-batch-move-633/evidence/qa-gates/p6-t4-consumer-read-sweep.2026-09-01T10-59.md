# Production `Consumer` read sweep (P6-T4)

Timestamp: 2026-09-01T10-59
Task: [P6-T4]
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/p6sweeps.ps1`, searching every `*.cs` file under
`QuickFiler/` recursively with `Select-String -Pattern '\.Consumer\b'`
EXIT_CODE: 0

Files searched: 165.

Match count: **0**.

Full match list: empty. No line in any `*.cs` file under `QuickFiler/` reads `.Consumer`.

Output Summary: Both production reads of `FilerQueue.Consumer` are gone. The pre-change population was
exactly two, at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:167` on the catch path of
`MoveAndIterate` and at `:193` on that method's terminal branch, and P4-T3 deleted both.

Each deletion is safe because each read was strictly subsumed by the new barrier. Both sites were
immediately preceded by an await of the same `BackGroundMoveAsync` task, and that task now contains
`await _parent.FilerQueue.WhenDrainedAsync()`, which waits on the queue's whole outstanding-work count
rather than on a single worker task — a superset of what `Consumer` covered.

`Consumer` itself is retained on the public surface with its original declaration, accessibility, and
`Task.CompletedTask` default, which P3-T7 verified and which the still-passing test
`FilerQueue_NewInstance_HasCompletedConsumerByDefault` pins. What this sweep records is that production
no longer *reads* the property, not that the property was removed.

This artifact supplies the evidence for the AC10 check-off in P8-T14.
