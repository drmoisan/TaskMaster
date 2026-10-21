# Test file sizes (P6-T6)

Timestamp: 2026-09-01T10-59
Task: [P6-T6]
Working directory: WORKTREE

Command: `(Get-Content -LiteralPath <path>).Count` for each added or modified test file.
`Measure-Object -Line` was deliberately not used.
EXIT_CODE: 0

| File | Lines | Baseline (P0-T13) | Delta | Under 500 |
|---|---|---|---|---|
| `QuickFiler.Test/Controllers/FilerQueueTests.cs` | 349 | 89 | +260 | yes, 151 to spare |
| `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` | 419 | 0 (new file) | +419 | yes, 81 to spare |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 470 | 436 | +34 | yes, 30 to spare |

All three recorded counts are at most 500.

Output Summary: Every added or modified test file stays under the 500-line limit.

`QfcItemController.SeamFactoryTests.cs` was the tightest constraint in the change, with only 64 lines of
headroom at baseline. The repair consumed 34 of those, leaving 30. The net growth is smaller than the
replacement text suggests because P3-T8 deleted the six-line reflection block, the `Queue.Count`
assertion, and two `using` directives while adding the gated-processor arrangement and the
received-item assertions.

The new ordering test file is the second tightest at 419 lines with 81 to spare. It holds five test
methods plus the shared construction fixture, the reflection helpers, the recording metrics delegate,
and the gated-enqueue helper.
