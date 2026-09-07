# Phase 0 — the two runtime facts of constraint C4

Timestamp: 2026-08-27T23-40
Task: [P0-T17]
Command: source reads of `UtilitiesCS/Threading/UiThread.cs:85-111`, `QuickFiler/Viewers/ItemViewer.cs:68-75` with its `using` block, and `QuickFiler/Viewers/EfcViewer.cs:36-40`, at `BASELINE_SHA` = `002335989830ba9f3ad802858ef0b794f6281750`
EXIT_CODE: 0

This artifact discharges the assumption that `spec.md` §`Assumptions, Constraints, Dependencies` marks
**UNVERIFIED** and requires the plan to verify before any test relies on it: "the behavior of
`await (SynchronizationContext)null` under this repository's awaiter extension."

## Fact 1 — `await` on a null `SynchronizationContext` throws `ArgumentNullException`

`UtilitiesCS/Threading/UiThread.cs` declares `SynchronizationContextAwaiter` at `:85`. Its constructor,
quoted verbatim from `:91-98`:

```csharp
public SynchronizationContextAwaiter(SynchronizationContext? context)
{
    if (context is null)
    {
        throw new ArgumentNullException(nameof(context));
    }
    _context = context;
}
```

The awaiter is reached through the extension method at `:108-111`:

```csharp
public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
{
    return new SynchronizationContextAwaiter(context);
}
```

`GetAwaiter` is an **extension method**, so it binds on a null receiver without a
`NullReferenceException` and forwards the null straight into the constructor's guard.

**Conclusion.** `await someNullSynchronizationContext` throws `ArgumentNullException` deterministically
and synchronously, at the point the `await` obtains its awaiter. This is the fault injection every RC3
boundary test uses: `await _formViewer.UiSyncContext` on an all-fields-null controller faults every time,
with no timing dependency.

**The converse also holds, and `[P7-T4]` relies on it.** The constructor's *only* guard is the null
check; there is no other throw path and no other precondition. `IsCompleted` compares `_context` with
`SynchronizationContext.Current`, and `OnCompleted` posts the continuation through `_context.Post`.
Therefore, given any non-null `SynchronizationContext`, the same `await` completes.

`EfcViewer.UiSyncContext` (`EfcViewer.cs:37-40`) is an ordinary getter over the private field `_context`
(`:36`):

```csharp
private SynchronizationContext _context;
public SynchronizationContext UiSyncContext
{
    get => _context;
}
```

So injecting `new SynchronizationContext()` into `_context` through the `SetPrivateField` helper at
`EfcFormControllerTests.cs:159-166` makes an awaited UI-thread marshal runnable headlessly against an
uninitialized concrete `EfcViewer`. Nothing is shown, no window handle is created, and no message pump is
required: the default `SynchronizationContext.Post` queues the continuation to the thread pool.

## Fact 2 — `Dispatcher.InvokeAsync` cannot be awaited headlessly

`QuickFiler/Viewers/ItemViewer.cs:71-75`:

```csharp
private Dispatcher _uiDispatcher;
public Dispatcher UiDispatcher
{
    get => _uiDispatcher;
}
```

with `UiDispatcher` declared at `:72`. The `Dispatcher` type resolves to
**`System.Windows.Threading.Dispatcher`** — the WPF dispatcher — through `using System.Windows.Threading;`
at `ItemViewer.cs:13`. It is not a WinForms type and it is not `System.Windows.Forms.Control.Invoke`.

A WPF `Dispatcher` executes queued delegates only when a dispatcher loop is pumping it. On the
dispatcher's own thread `InvokeAsync` **queues** the delegate rather than running it inline, so awaiting
the returned operation never completes without a running message loop. Constraint C3 prohibits a message
loop in any test this feature writes.

**Conclusion.** No test in this plan awaits
`EfcItemController.ToggleExpansionAsync(Enums.ToggleState)`, whose body dispatches through
`_itemViewer.UiDispatcher.InvokeAsync(...)` at `EfcItemController.cs:913` and `:922`. The dispatched
bodies `ToggleExpansionOn` (`:944-956`) and `ToggleExpansionOff` (`:931-942`) are invoked directly by
reflection instead. `[P1-T15]` records the fail-before exception dossier for that substitution.

Output Summary: Both C4 facts verified from source. `SynchronizationContextAwaiter`'s constructor
(UiThread.cs:91-98) throws `ArgumentNullException` on a null context and has no other guard, so an await
on a null SynchronizationContext faults deterministically and an await on any non-null one completes.
`ItemViewer.UiDispatcher` (ItemViewer.cs:72) is a WPF `System.Windows.Threading.Dispatcher` whose
`InvokeAsync` cannot complete on an unpumped thread. The `spec.md` UNVERIFIED assumption is discharged.
