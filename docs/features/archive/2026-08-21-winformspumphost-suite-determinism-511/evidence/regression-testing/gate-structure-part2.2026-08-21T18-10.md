# P2-T2 — `Part2.cs` Gate-Structure Invariants After the Fixture Change

Timestamp: 2026-08-22T10-19

Command:
```
grep -n "UiThreadDispatcherGate\|SwapUiThreadDispatcher\|_restored" QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs
git diff --numstat -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs
wc -l QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs
```

EXIT_CODE: 0

Output Summary:

All four invariants hold after the P2-T1 edit. The edit is a pure 7-line insertion
(`git diff --numstat` reports `7	0`), so no existing line of the file changed.

| # | Invariant | Observed | Verdict |
| --- | --- | --- | --- |
| 1 | `UiThreadDispatcherGate` declared as `SemaphoreSlim(1, 1)` | line 51: `private static readonly SemaphoreSlim UiThreadDispatcherGate = new SemaphoreSlim(1, 1);` | HOLDS |
| 2 | `BuildPumpHarnessAsync` calls `UiThreadDispatcherGate.WaitAsync` before delegating, and `UiThreadDispatcherGate.Release` in its `catch` | line 67 `await UiThreadDispatcherGate.WaitAsync().ConfigureAwait(false);` precedes the line 70 delegation to `BuildPumpHarnessCoreAsync`; line 74 `UiThreadDispatcherGate.Release();` sits inside the `catch` block | HOLDS |
| 3 | `PumpHarness.Restore` calls `UiThreadDispatcherGate.Release` exactly once behind the `_restored` guard | field `_restored` at line 307; guard `if (_restored) { return; }` at lines 340-343; `_restored = true;` at line 345; a single `UiThreadDispatcherGate.Release();` at line 348 | HOLDS |
| 4 | File line count < 500 | 416 | HOLDS |

Post-edit line count: **416** (pre-change 409, +7).

Cross-class serialization is therefore intact: `QfcItemController_SeamFactoryTests` reaching
`BuildPumpHarnessAsync` still acquires the same process-wide gate, and the acquire-and-release
structure is unchanged. P3-T7 exercises both classes in one invocation as the runtime proof.

Line indices relevant to the inserted statement (P2-T1):

- viewer construction: line 84
- inserted `_ = await host.InvokeAsync(() => viewer.Handle).ConfigureAwait(false);`: line 92
- `SwapUiThreadDispatcher(viewer.UiDispatcher)` call: line 136
