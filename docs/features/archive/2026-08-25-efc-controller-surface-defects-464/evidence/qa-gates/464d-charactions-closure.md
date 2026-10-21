# Phase 1 — #464 D closure: no `async void` lambda registered into `CharActions`

Timestamp: 2026-08-28T00-14
Task: [P1-T13]
Command: `grep -c 'CharActions.Add' QuickFiler/Controllers/EfcItemController.cs`; `grep -n 'CharActions' QuickFiler/Controllers/EfcItemController.cs`; `grep -n 'async (' QuickFiler/Controllers/EfcItemController.cs`; and the same searches against `git show 002335989830ba9f3ad802858ef0b794f6281750:QuickFiler/Controllers/EfcItemController.cs` for the pre-change figures
EXIT_CODE: 0

## Delivered state

- `CharActions.Add` call sites in the delivered file: **0**.
- `async` lambdas bound to `Action<char>` in the delivered file: **0**.

The only surviving reference to the synchronous registry is a **removal**, not a registration:
`keys.ForEach(key => _keyboardHandler.CharActions.Remove("Item", key));`. `Remove` takes a key, not a
delegate, so it cannot introduce an `async void` lambda.

`#464 D` is therefore closed **by the deletion asserted under #459**: the two offending lambdas lived
inside `ToggleExpansion(Enums.ToggleState)`, which `[P1-T6]` deleted whole. No literal was edited and no
lambda was rewritten.

## Pre-change state, read from BASELINE_SHA

`CharActions.Add` call sites: **2**, at `:879` and `:884`, both inside the deleted
`ToggleExpansion(Enums.ToggleState)` overload:

```
879:                _keyboardHandler.CharActions.Add(
884:                _keyboardHandler.CharActions.Add(
```

Their delegate arguments were the two genuine `async void` lambdas at `:882` and `:887`. They compiled as
`async void` because `CharActions` is `KbdActions<char, KaChar, Action<char>>` — a `void`-returning
delegate type — so an `async` lambda assigned to it has no `Task` for a caller to observe and its faults
escape to the synchronization context.

## The four surviving lambdas register into `CharActionsAsync`, not `CharActions`

All four sit in `RegisterAsyncFocusActions` and are unaffected by this feature's deletions. Their
pre-change locations, exactly as the plan states:

| Pre-change line | Key | Lambda | `async`? |
|---|---|---|---|
| `:699` | `'O'` | `(x) => _ = _explorerController.OpenQFItem(_itemInfo.Item)` | **no** |
| `:704` | `'E'` | `async (x) => await KbdExecuteAsync(this.ToggleExpansionAsync)` | yes |
| `:711` | `'B'` | `async (x) => await JumpToAsync(_itemViewer.L0v2h2_WebView2)` | yes |
| `:716` | `'D'` | `async (x) => await JumpToAsync(_itemViewer.TopicThread)` | yes |

Three of the four are `async`; the one at `:699` is not. All four are added through
`_keyboardHandler.CharActionsAsync.Add(...)`.

`CharActionsAsync` is declared at `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:22` as:

```csharp
KbdActions<char, KaCharAsync, Func<char, Task>> CharActionsAsync { get; set; }
```

Its delegate type is `Func<char, Task>`, so these are **`Task`-returning** lambdas, not `async void`. Their
faults are observable, and they are in fact observed at `QuickFiler/Controllers/KeyboardHandler.cs:176`:

```csharp
await CharActionsAsync[(char)e.KeyValue]((char)e.KeyValue);
```

The `await` propagates any fault into `KeyboardHandler_KeyDownAsync`, which catches and logs it. These
four lambdas are therefore not `#464 D` defects and are correctly left untouched, which is the
`spec.md` §RC3 revision **R2** finding that three of `issue.md`'s six cited lambdas are not defects.

Output Summary: The delivered `EfcItemController.cs` contains zero `CharActions.Add` call sites and zero
`async` lambdas bound to `Action<char>`; the pre-change count was 2, at `:879` and `:884`, both inside the
deleted synchronous expansion overload. The four surviving lambdas at pre-change `:699`, `:704`, `:711`
and `:716` register into `CharActionsAsync`, whose delegate type is `Func<char, Task>`, and their faults
are awaited at `KeyboardHandler.cs:176`. #464 D is closed by removal.
