# qfc-item-controller-cleanup-timer-and-stale-field-defects (Issue #484)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-cleanup-timer-and-stale-field-defects/ (Issue #484)
- Discovered during: preparation research for epic #136 child F10 (issue #453)

- Issue: #484
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/484
- Last Updated: 2026-08-08
## Summary

`QfcItemController.Cleanup()` nulls the armed `_emailIsReadTimer` field without disposing it,
orphaning a live 4-second `System.Threading.Timer` that will still fire against a torn-down
controller. The same method leaves other collaborator fields in an inconsistent state.

## Affected Code

### 1. Armed timer nulled without disposal

`_emailIsReadTimer` is declared at `QuickFiler/Controllers/QfcItemController.cs:53` as a
`System.Threading.Timer`.

It is created and armed at `QuickFiler/Controllers/QfcItemController.Navigation.cs:223-224`:

```csharp
_emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat);
_emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);
```

`Navigation.cs:211-213` demonstrates the correct pattern on the re-arm path:

```csharp
if (_emailIsReadTimer is not null)
{
    _emailIsReadTimer.Dispose();
```

But `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:420`, inside `Cleanup()`, does only:

```csharp
_emailIsReadTimer = null;
```

The timer is armed for 4000 ms and is not disposed. After `Cleanup()` the callback
`ApplyReadEmailFormat` still executes on a thread-pool thread against a controller whose fields have
just been nulled. The disposal path elsewhere in the same type shows this is an oversight rather
than an intentional handoff.

### 2. Stale collaborator fields across `Cleanup()` / `SaveParameters`

`Cleanup()` nulls 17 collaborator fields, but `_mailActions` is retained across the
`Cleanup()`/`SaveParameters` boundary, so a reused controller can act through a collaborator bound
to the previous mail item. This is latent rather than currently reachable, and is recorded here so
it is not rediscovered.

## Why This Is a Defect

QuickFiler pools and reuses item viewers and their controllers. A 4-second timer surviving cleanup
means the callback lands during or after the next item's setup, where it either throws a
`NullReferenceException` against nulled fields or applies read-formatting to the wrong item. Both
are silent: the callback runs on a thread-pool thread with no logging on the fault path.

## Reproduction Sketch

Arm the read timer by selecting an item, then tear the controller down within four seconds by
navigating away or closing the pane. Observe `ApplyReadEmailFormat` executing after `Cleanup()`.

## Suspected Fix

In `Cleanup()`, dispose the timer before nulling the field, mirroring the existing pattern at
`Navigation.cs:211-213`. Audit `_mailActions` lifetime against the pooled-reuse path and null it
with the other collaborators if no caller depends on its retention.

## Severity

Medium. Reachable `NullReferenceException` on a thread-pool thread, or misapplied UI formatting on a
recycled viewer.

## Related

- #481 — `QfcItemController` has no event unwiring path (same teardown-incompleteness class).

## Scope

Out of scope for epic #136 child F10, whose NFR prohibits behavior change to observable QuickFiler
flows. Disposing the timer changes teardown semantics and must be scheduled with its own regression
test.
