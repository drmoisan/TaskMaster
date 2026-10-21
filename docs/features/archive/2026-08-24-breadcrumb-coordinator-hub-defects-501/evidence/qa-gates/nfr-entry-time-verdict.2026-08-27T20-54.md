# QA Gate — Cross-Cutting NFR: TryRunCurrent Returns the Entry-Time Verdict Only (P6-T7, AC-28)

Timestamp: 2026-08-27T20-54

The NFR under test:

> `TryRunCurrent`'s `bool` must continue to mean "the action was invoked at entry-time currency" — the
> currency verdict taken at entry, never a verdict recomputed after the action returns.

## Full method body, read back verbatim from the file

`QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:175-192`:

```csharp
internal bool TryRunCurrent(BreadcrumbUpgradeLease lease, Action action)
{
    if (action == null)
    {
        throw new ArgumentNullException(nameof(action));
    }
    bool current;
    lock (_sync)
    {
        current = IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested;
    }
    if (!current)
    {
        return false;
    }
    action();
    return true;
}
```

## The statement that returns the value

There are two `return` statements:

- `:188` — `return false;`. Reached only when the verdict captured at `:184` is `false`, i.e. the action
  was NOT invoked. It is guarded by `if (!current)` at `:186`, and `current` was assigned once, at `:184`,
  before `action()` runs.
- `:191` — `return true;`. Reached only after `action()` at `:190` has been invoked. It is a literal
  `true`, not a re-evaluation of anything.

So the returned value is fully determined by the single evaluation at `:184`, which happens strictly
before the action.

## Explicit statement: no currency evaluation occurs after `action()`

**No currency evaluation of any kind occurs after `action()` is invoked at `:190`.** The only statement
after `:190` is `return true;` at `:191`.

Mechanically verified against the three forbidden constructs:

| Construct | Occurrences in the method | Line(s) | Position relative to `action()` at `:190` |
| --- | ---: | --- | --- |
| `IsGenerationCurrentCore` | 1 | 184 | BEFORE, inside `lock (_sync)` |
| `IsCurrent` | 0 | — | not called at all |
| `IsCancellationRequested` | 1 | 184 | BEFORE, inside `lock (_sync)` |

The method body therefore contains no call to `IsGenerationCurrentCore`, `IsCurrent`, or
`IsCancellationRequested` lexically after the `action()` invocation. **SATISFIED.**

## Why this matters, and what it forecloses

This is research section 6.2 **option A**. Option B — folding a post-action currency re-check into the
return value — is what the NFR forbids, and the reason is not stylistic. Under option B, `false` becomes
ambiguous between "did not run" and "ran but was superseded". The #502 fix branches on exactly that
`false`: `SetSuggestionsCore` assigns `SuggestionsUpgrade = Task.CompletedTask` on it. Under option B that
branch would fire AFTER the guarded lambda had already assigned the real population task, overwriting a
live handle with a completed one — turning the #502 remedy into a fresh instance of the #502 defect.

The method also carries an XML `<returns>` comment stating this contract in prose, so a future editor
meets the constraint at the call site rather than only in an audit artifact.

## Behavioural guard

The source-level fact above is backed by a live regression test, not left as source inspection alone:
`TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation` asserts that an action which
re-entrantly calls `lifetime.Invalidate()` still yields `TryRunCurrent == true`, while
`lifetime.IsCurrent(lease)` reports `false` immediately afterwards. Under option B that test fails. It was
green BEFORE the fix (`FF/evidence/regression-testing/green-500-nfr-guard-prefix.2026-08-27T20-25.md`) and
green AFTER (`FF/evidence/regression-testing/green-500-lifetime.2026-08-27T20-27.md`), which is what makes
it a standing regression detector rather than a restatement of the fix.
