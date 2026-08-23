# quickfiler-keyboard-action-contract-defects (Issue #445)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-keyboard-action-contract-defects/ (Issue #445)
- Discovered during: research for issue #430 (`quickfiler-keyboard-actions-coverage`, child F3 of epic #136)

- Issue: #445
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/445
- Last Updated: 2026-08-08
- Work Mode: full-bug

## Summary

Three related contract defects in the QuickFiler keyboard-action types. All three were verified by
direct file read at `origin/epic/quickfiler-per-file-coverage-integration` (base commit `56ca1cea`).
None is fixed by issue #430, which carries a no-behavior-change acceptance criterion and characterizes
current behavior in tests instead.

## Defect 1 — `KaStringAsync.KeyEquals` applies the `Activated` gate inconsistently

`KeyEquals` guards its `Update` invocation with `Activated` in two of three branches but not the third:

```csharp
// QuickFiler/Controllers/KaStringAsync.cs:57-78
public bool KeyEquals(string other)
{
    if (Key.Contains(other))
    {
        if (Activated && Update is not null)          // gated
            Update(Key.Substring(other.Length - 1, 1));
        return true;
    }
    else if (other.Length == 1)
    {
        if (Activated && ToggleControl is not null)   // gated
            ToggleControl();
    }
    else if (other.Length > 1)
    {
        if (Update is not null)                       // NOT gated
            Update(Key.Substring(0, 1));
        if (Activated && ToggleControl is not null)
            ToggleControl();
    }
    Activated = false;
    return false;
}
```

The `other.Length > 1` branch invokes `Update` regardless of `Activated`. Whether this is intentional
or an omission is not determinable from the code; there is no comment explaining it. This is the
highest-value untested behavior in the cluster.

## Defect 2 — `KeyEquals("")` throws `ArgumentOutOfRangeException`

`Key.Contains("")` is `true` for every string, so an empty `other` enters the first branch and
evaluates `Key.Substring(other.Length - 1, 1)` — that is, `Substring(-1, 1)` — which throws
`ArgumentOutOfRangeException` (`KaStringAsync.cs:62`).

This is currently double-shielded and is therefore a robustness gap rather than a live crash:
`KeyboardHandler` only ever probes with length `>= 1`, and production supplies a null `Update`, so the
guarded call is not reached. Both shields are incidental, not contractual.

## Defect 3 — `KaChar.DelegateType` reports the wrong type

```csharp
// QuickFiler/Controllers/KaChar.cs:11
public class KaChar : IKbdAction<char, Action<char>>
// QuickFiler/Controllers/KaChar.cs:37
public Action<char> Delegate
// QuickFiler/Controllers/KaChar.cs:43-46
public Type DelegateType
{
    get => typeof(Action<Keys>);
}
```

`KaChar` stores an `Action<char>` but `DelegateType` reports `typeof(Action<Keys>)`. Impact today is
nil because no consumer reads `DelegateType`.

## Related — `Update` and `DelegateType` are orphaned public API

`Update` and `DelegateType` appear on four implementer types but on no interface. The corresponding
contract members are commented out:

```csharp
// QuickFiler/Interfaces/IKbdAction.cs:12-16
T Key { get; set; }
U Delegate { get; set; }
bool KeyEquals(T other);
//Action<string> Update { get; set; }
//Type DelegateType { get; }
```

Restoring `DelegateType` to the interface **will not compile**: `KaCharAsync` (`KaChar.cs:58`) and
`KaKeyAsync` do not declare it. The viable cleanup direction is therefore removal from the implementers
rather than restoration to the interface. Defect 3 disappears if `DelegateType` is removed.

## Impact

No confirmed user-visible failure. Defects 2 and 3 are latent. Defect 1 is a genuine behavioral
ambiguity that will become load-bearing the moment `Update` is non-null on a multi-character probe.
All three are the kind of contract inconsistency that makes the surrounding code unsafe to refactor.

## Why these were not fixed in issue #430

Issue #430 (child F3) carries an explicit acceptance criterion of **no behavior change to observable
QuickFiler keyboard flows**. Each of these fixes is a behavior change. F3's new tests characterize the
current behavior, including the ungated `Update` call and the empty-string throw, so that a later fix
has a red-before-green baseline to work against.

## Proposed Fix Direction

1. Decide whether the `other.Length > 1` branch should be `Activated`-gated, and make all three
   branches consistent with the decision.
2. Add an explicit guard or documented contract for empty `other` in `KeyEquals`.
3. Remove `DelegateType` from `KaChar`, `KaKey`, and any sibling implementer, or correct it to
   `typeof(Action<char>)` if a consumer is introduced. Remove the commented-out members from
   `IKbdAction.cs` or restore them deliberately with all implementers updated.

## Acceptance Criteria (early draft)

- [ ] The `Activated`-gating contract for `KaStringAsync.KeyEquals` is decided, applied consistently
      across all three branches, and documented in-code.
- [ ] `KeyEquals` handles an empty `other` without throwing `ArgumentOutOfRangeException`, or rejects it
      with an explicit, documented argument exception.
- [ ] `DelegateType` is either removed from all implementers or reports the actual stored delegate type.
- [ ] The commented-out members in `IKbdAction.cs` are resolved (removed or restored with all
      implementers updated).
- [ ] Regression tests cover each changed behavior, replacing the characterization tests added by #430.
- [ ] Full C# toolchain passes: csharpier, analyzer build, nullable build, coverage-enabled vstest.

## Next Step

- [ ] Promote to GitHub issue (bug template)
- [ ] Sequence after epic #136 child F3 (#430) merges, so the characterization tests exist first
