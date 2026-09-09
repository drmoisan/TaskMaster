# Phase 4 — Write Set line counts after the progress-surface tests

Timestamp: 2026-09-09T13-20
Task: [P4-T13]

Command: the `[P0-T13]` line-count command, re-run unchanged.
EXIT_CODE: 0

Verbatim output:

```text
QuickFiler/Controllers/QfcHomeController.cs 500
QuickFiler/Controllers/EfcHomeController.cs 447
UtilitiesCS/Threading/ProgressViewer.cs 136
UtilitiesCS/Threading/ProgressPane.cs 107
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs 192
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs 492
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs 486
UtilitiesCS.Test/Threading/ProgressPane_Tests.cs 324
```

## Acceptance check for the two files this phase edited

| File | Baseline | Now | Budget | Met | Spare |
|---|---|---|---|---|---|
| `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` | 352 | **486** | at most 499 | yes | 13 |
| `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs` | 192 | **324** | at most 350 | yes | 26 |

Both are below the repository's 500-line ceiling. The other six files are unchanged from their
post-Phase-3 counts.

## Recorded decision — how the ProgressViewer test budget was met

A first draft of the five viewer tests, written with per-test scaffolding, brought
`UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` to **536 lines**, over both the 499-line phase
budget and the repository's 500-line ceiling. Splitting the file is forbidden by the plan: a ninth
file would require a `.csproj` `Compile Include` entry and would falsify AC19 and AC20.

The overage was resolved by factoring the repeated scaffolding into shared private helpers rather
than by removing or weakening any assertion:

- `WithViewer(Action<ProgressViewer>)`, which constructs a real viewer under a fresh
  synchronization context and disposes it in a `finally` guarded by `if (!viewer.IsDisposed)` —
  the exact disposal shape `[P4-T4]` specifies, applied once instead of per test. It is not a
  `using` block, so the constraint that the handler tests must not wrap the viewer in `using`
  continues to hold.
- `InvokeCancelClick(ProgressViewer)`, which performs the private-handler reflection lookup the two
  handler tests previously duplicated.

Every assertion named by `[P4-T3]` through `[P4-T7]` is present and unchanged in strength: the typed
throw with its `*SetCancellationTokenSource*` message assertion, both `NotThrow` assertions, both
`IsDisposed.Should().BeTrue(` assertions with `because` strings, and the two-step
`BeTrue(` then `BeFalse(` enabling assertion. The same helper pattern was used from the outset in
`ProgressPane_Tests.cs`, which is why that file needed no compression pass.

Output Summary: `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` is 486 lines, at most 499 as
required, and `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs` is 324 lines, at most 350 as
required. No Write Set file exceeds 500 lines. `[P6-T13]` re-verifies after the final CSharpier pass,
which is the last write to any `.cs` file.
