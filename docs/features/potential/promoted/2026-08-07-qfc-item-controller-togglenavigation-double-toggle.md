# qfc-item-controller-togglenavigation-double-toggle (Issue #480)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-togglenavigation-double-toggle/ (Issue #480)
- Discovered during: preparation research for epic #136 child F10 (issue #453)

- Issue: #480
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/480
- Last Updated: 2026-08-08
## Summary

`QfcItemController.ToggleNavigation(bool async)` invokes the state-flipping
`QfcTipsDetails.Toggle(bool)` exactly twice on every call, so the navigation position tips return to
their original visibility. The feature is inert.

## Affected Code

`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:168-179`

```csharp
public void ToggleNavigation(bool async)
{
    _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));
    if (async)
    {
        _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));
    }
    else
    {
        _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(false)));
    }
}
```

Line 170 toggles unconditionally. Then exactly one of line 173 or line 177 toggles again. Both
branches lead to a second toggle, so there is no path through this method that toggles only once.

## Why This Is a Defect

`UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:193-203` shows `Toggle(bool sharedColumn)` is a
flip, not an idempotent set:

```csharp
public void Toggle(bool sharedColumn)
{
    if (_state.HasFlag(Enums.ToggleState.On))
    {
        Toggle(Enums.ToggleState.Off, sharedColumn);
    }
    else
    ...
}
```

Two flips return the control to its starting state. The user-visible effect is that invoking
navigation-tip toggling through this overload does nothing.

Contrast the sibling overload `ToggleNavigation(bool async, Enums.ToggleState desiredState)` at
`FocusAndTheme.cs:181`, which correctly calls the idempotent `Toggle(desiredState, false)` exactly
once per branch. The single-argument overload appears to have been written by analogy without
accounting for the flip semantics of the one-argument `Toggle`.

## Reproduction

Call `ToggleNavigation(async: false)` or `ToggleNavigation(async: true)` on a controller whose
`_itemPositionTips` is initialized, then observe `_labelControl.Visible`. It is unchanged.

## Suspected Fix

Remove the unconditional toggle at line 170, leaving exactly one toggle per branch. Confirm against
the intended behavior of the caller before changing, since some caller may have been written to
compensate for the no-op.

## Severity

Medium. No crash or data loss; a UI affordance silently does nothing.

## Detection Note

This was masked in the existing test suite by an assertion using `Times.AtLeastOnce()`, which is
satisfied by two invocations just as well as by one. A regression test must assert the exact
invocation count.

## Scope

Out of scope for epic #136 child F10, whose NFR prohibits behavior change to observable QuickFiler
flows. Filed for independent scheduling.
