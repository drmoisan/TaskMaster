# QfcItemController.InitializationTests.Part2.cs Migrated (P2-T2)

Timestamp: 2026-08-27T10-54
Task: [P2-T2]
Command: `git diff --stat -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` and four `Select-String -SimpleMatch` searches against the same path
EXIT_CODE: 0
Output Summary: Post-edit line count is 393 (from 418). The single-path diff-stat line shows 47
deletions, a non-zero deletion count. All four required searches return zero matches:
`BindingFlags` = 0, `System.Reflection` = 0, `System.Windows.Threading` = 0,
`FluentAssertions` = 0. The signatures of `BuildPumpHarnessAsync` and `PumpHarness.Restore` are
unchanged, so the four unowned call sites in `QfcItemController.SeamFactoryTests.cs` compile
untouched.

## Acceptance verification

| Item | Required | Observed |
| --- | --- | --- |
| Post-edit line count | recorded | 393 |
| Diff-stat deletion count | non-zero | 47 |
| `Select-String -SimpleMatch 'BindingFlags'` | 0 matches | 0 |
| `Select-String -SimpleMatch 'System.Reflection'` | 0 matches | 0 |
| `Select-String -SimpleMatch 'System.Windows.Threading'` | 0 matches | 0 |
| `Select-String -SimpleMatch 'FluentAssertions'` | 0 matches | 0 |

Single-path diff-stat line:

```
 .../QfcItemController.InitializationTests.Part2.cs | 69 +++++++---------------
 1 file changed, 22 insertions(+), 47 deletions(-)
```

Two further counts are recorded here for the audit trail; the plan states them as the gate in
`P4-T4` rather than here, and this artifact does not restate that condition:
`UiThreadDispatcherGate` = 0 and `SwapUiThreadDispatcher` = 0.

## Edits made, against § Part2 Migration

1. **Gate field and its doc block deleted** (lines 36-51 at `HEAD`) and replaced by a five-line
   comment that names `UiThreadDispatcherFixture` and preserves the #230 rationale. The replacement
   comment deliberately contains **neither** the identifier `UiThreadDispatcherGate` **nor**
   `SwapUiThreadDispatcher`, because `P4-T4` rows 1 and 2 assert zero matches for those two tokens
   against this file and a rationale comment naming them would silently defeat both rows.
2. **`BuildPumpHarnessAsync`** now acquires
   `await UiThreadDispatcherFixture.BeginTransactionAsync().ConfigureAwait(false)` at build start,
   passes the transaction to the core builder, and its `catch` calls `transaction.Dispose()` before
   rethrowing. **Its signature is unchanged.**
3. **`BuildPumpHarnessCoreAsync`** takes a third parameter
   `UiThreadDispatcherTransaction transaction`. The former
   `Dispatcher previousUiThreadDispatcher = SwapUiThreadDispatcher(viewer.UiDispatcher);` is now
   `transaction.Install(viewer.UiDispatcher);`, and the return is
   `return new PumpHarness(controller, viewer, cts, webView, transaction);`.
4. **`SwapUiThreadDispatcher` and its doc block deleted** (lines 143-158 at `HEAD`).
5. **`PumpHarness`** replaces `private readonly Dispatcher _previousUiThreadDispatcher;` with
   `private readonly UiThreadDispatcherTransaction _transaction;`, assigned in its single
   constructor whose fifth parameter changed type accordingly. `private bool _restored;` is retained
   unchanged. `Restore()` keeps its `_restored` guard; its body is now `TokenSource.Dispose();`
   followed by `_transaction.Dispose();`. **Its signature is unchanged.**
6. **Three using directives deleted**: `using System.Reflection;`,
   `using System.Windows.Threading;`, and `using FluentAssertions;`.

## Evidence that the three deleted using directives were dead

Measured after the member edits and before the directives were removed:

| Token | Match count | Consequence |
| --- | --- | --- |
| `FieldInfo` | 0 | `System.Reflection` unused |
| `BindingFlags` | 0 | `System.Reflection` unused |
| `Should()` | 0 | `FluentAssertions` unused |
| `(Dispatcher)` cast | 0 | no bare `Dispatcher` cast remains |
| bare `Dispatcher ` as a type name | 0 | `System.Windows.Threading` unused |

The five remaining hits on the substring `Dispatcher ` are one comment line and four occurrences of
`IUiDispatcher`, which comes from `UtilitiesCS.Threading`, not from `System.Windows.Threading`.

Directives deliberately **retained**, each with a live consumer:

| Retained directive | Live consumer | Count |
| --- | --- | --- |
| `using System.Threading;` | `CancellationTokenSource` / `CancellationToken` | 5 |
| `using UtilitiesCS;` | `IApplicationGlobals` | 6 |
| `using UtilitiesCS.Threading;` | `IUiDispatcher` | 3 |

`using UtilitiesCS;` survives even though the `UiThread` reference this task deleted came from it,
because `UiThread` is declared in namespace `UtilitiesCS` despite its `Threading/` folder path, and
`IApplicationGlobals` also comes from `UtilitiesCS`.

## Invariants preserved

- The two-phase `BeginTransactionAsync` then `Install` shape is kept; it is not collapsed into a
  single `SwapAsync(replacement)`, so the gate hold window still starts at build start rather than at
  install time.
- The acquisition remains at build start.
- `PumpHarness.Restore` remains idempotent via `_restored`.
- Restore-then-release ordering is preserved and is now indivisible, because both halves live inside
  `UiThreadDispatcherTransaction.Dispose()`. Per § Decisions Record D4 this is a deliberate
  reordering relative to `HEAD`: the token-source disposal now precedes the restore instead of
  sitting between the restore and the release. `TokenSource.Dispose()` neither reads nor writes
  `UiThread._dispatcher`, so the move is safe, and it is necessary because restore and release are
  now a single action.
- Unowned call-site compatibility: `QfcItemController.SeamFactoryTests.cs` calls
  `BuildPumpHarnessAsync` at lines 313 and 384 and `harness.Restore()` at lines 358 and 429. Both
  signatures are unchanged, so those four call sites are untouched.

## Shared-harness semantics change observable by existing callers

Recorded explicitly because this is shared test infrastructure. The gate that serializes pump
fixtures moved from a `private static SemaphoreSlim` owned by
`QfcItemController_InitializationTests` to `UiThreadDispatcherFixture.TransactionGate`, which is now
also acquired by the six new regression tests in
`QfcItemController.UiThreadDispatcherFixtureTests.cs`. The **number of permits (1) and the hold
window (build start to restore) are unchanged**, so no existing caller's ordering guarantee weakens.
The observable change is that pump-fixture consumers now additionally serialize against the six new
regression tests, each of which holds the gate briefly. This is residual risk R-5 in spec § Risks,
accepted there in advance.
