# [P5-T4] `Remove`-contract gate on `KbdActions.cs`

Timestamp: 2026-08-27T20-08
Command: `@(Select-String -LiteralPath 'QuickFiler\Controllers\KbdActions.cs' -SimpleMatch -Pattern 'public bool Remove(string sourceId, TKey key)' -AllMatches).Count` and the same form with `-Pattern 'TryRemove'`
EXIT_CODE: 0
Output Summary: `remove_count=1`, `tryremove_count=0`. The declaration occurs exactly once and the
literal `TryRemove` does not occur at all.

## Counts

| Assertion | Required | Observed | Verdict |
| --- | --- | --- | --- |
| occurrences of `public bool Remove(string sourceId, TKey key)` | exactly `1` | **1** | PASS |
| occurrences of the literal `TryRemove` | exactly `0` | **0** | PASS |

`-SimpleMatch` is used so the parentheses, comma, and angle-free generic parameter name in the
declaration are matched literally rather than as regular-expression metacharacters.

A single occurrence establishes that the member retains its `bool` return type, its `public`
accessibility, and its two-parameter signature: had the return type or parameter list been altered,
the literal would match zero times. A zero count for `TryRemove` establishes that no
`TryRemove`-style alternative member was introduced alongside it, in either a declaration or a call.

The member's silent `false` for an absent pair is unchanged: `[P5-T3]` records that the diff for this
file contains zero deletion lines and adds no member declaration, so `Remove`'s body was not touched.
That behaviour is load-bearing for this feature — `SyncExpandedRegistrations` calls the unregister
methods unconditionally, and its idempotence depends on `Remove` returning `false` rather than
throwing when the entries are absent.

## Acceptance

- The first count is exactly `1` — met.
- The second is exactly `0` — met.
