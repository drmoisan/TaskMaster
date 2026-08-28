# [P3-T14] Single-owner gate, evaluated positionally

Timestamp: 2026-08-27T09-45
File: `QuickFiler/Controllers/QfcItemController.Navigation.cs`
EXIT_CODE: 0

This gate is positional rather than a whole-file occurrence count, because a count gate cannot
distinguish a moved call from an unmoved one. All three member bodies are recorded verbatim below and
the four registration names plus the delegating call are counted **inside each body separately**.

## `SyncExpandedRegistrations` body (verbatim)

```csharp
        private void SyncExpandedRegistrations(bool expanded)
        {
            UnregisterExpandedActions();
            UnregisterExpandedAsyncActions();
            if (expanded)
            {
                RegisterExpandedActions();
                RegisterExpandedAsyncActions();
            }
        }
```

| Name | Present in this body |
| --- | --- |
| `UnregisterExpandedActions` | yes |
| `UnregisterExpandedAsyncActions` | yes |
| `RegisterExpandedActions` | yes |
| `RegisterExpandedAsyncActions` | yes |

All four are present, so this member is the sole owner of expansion registration in this file.

## `ToggleExpansion(Enums.ToggleState desiredState)` body (verbatim)

```csharp
        public virtual void ToggleExpansion(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                ToggleExpansionOn();
            }
            else
            {
                ToggleExpansionOff();
            }
            SyncExpandedRegistrations(_expanded);
        }
```

| Measure | Count |
| --- | --- |
| `UnregisterExpandedActions` | 0 |
| `UnregisterExpandedAsyncActions` | 0 |
| `RegisterExpandedActions` | 0 |
| `RegisterExpandedAsyncActions` | 0 |
| `SyncExpandedRegistrations(_expanded)` | **1** |

## `ToggleExpansionAsync(Enums.ToggleState desiredState)` body (verbatim)

```csharp
        public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            await _parent.ToggleExpansionStyleAsync(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                await _uiDispatcher.InvokeAsync(() => ToggleExpansionOn());
            }
            else
            {
                await _uiDispatcher.InvokeAsync(() => ToggleExpansionOff());
            }
            SyncExpandedRegistrations(_expanded);
        }
```

| Measure | Count |
| --- | --- |
| `UnregisterExpandedActions` | 0 |
| `UnregisterExpandedAsyncActions` | 0 |
| `RegisterExpandedActions` | 0 |
| `RegisterExpandedAsyncActions` | 0 |
| `SyncExpandedRegistrations(_expanded)` | **1** |

## Position of the delegating call

In both overloads `SyncExpandedRegistrations(_expanded)` is the **last** statement, after the branch
has run `ToggleExpansionOn()` or `ToggleExpansionOff()`. Those two private members are what write
`_expanded`, so the flag is already correct when the owner reads it. That ordering is the substance of
the fix: the owner is keyed on the resulting state, not on which path performed the toggle.

## Measurement note

`Select-String -SimpleMatch` matches case-insensitively by default, so a raw count of
`RegisterExpandedActions` inside the helper body reports 2 rather than 1: the pattern also matches the
`registerExpandedActions` tail of `UnregisterExpandedActions`. The table above records presence for the
helper (which is all this gate asserts of it) and exact counts for the two overload bodies, where every
count is zero and no such collision is possible.

## Acceptance evaluation

- The `SyncExpandedRegistrations` body contains all four of `UnregisterExpandedActions`,
  `UnregisterExpandedAsyncActions`, `RegisterExpandedActions`, and `RegisterExpandedAsyncActions`. PASS.
- Each of the two `ToggleState` overload bodies contains zero occurrences of all four of those names.
  PASS.
- Each of the two `ToggleState` overload bodies contains exactly one occurrence of
  `SyncExpandedRegistrations(_expanded)`. PASS.

Output Summary: all four registration calls live only in `SyncExpandedRegistrations`; both `ToggleState`
overloads contain zero of them and exactly one delegating call each, placed after the flag write.
