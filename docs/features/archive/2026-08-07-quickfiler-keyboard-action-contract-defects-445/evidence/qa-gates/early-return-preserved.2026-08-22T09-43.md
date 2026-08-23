# Phase 2 — Hard Anti-Regression Constraint Verification (Issue #445, AC3)

Timestamp: 2026-08-22T09-43

Command:
```powershell
(git grep -n -F 'return true;' -- 'QuickFiler/Controllers/KaStringAsync.cs' | Measure-Object -Line).Lines
(git grep -n -F 'Activated = false' -- 'QuickFiler/Controllers/KaStringAsync.cs' | Measure-Object -Line).Lines
```
Run from `WS`.

EXIT_CODE: 0

## Numeric gates

| Token | P0-T19 baseline | Now | Required | Pass |
|---|---|---|---|---|
| `return true;` | 1 | **1** | exactly 1 | yes |
| `Activated = false` | 1 | **1** | exactly 1 | yes |

Both counts equal their P0-T19 baseline exactly.

## Why each count is the right discriminator

**`return true;` must be exactly 1.** A count of 0 would mean branch 1's early return was deleted, letting a matching probe fall through to the trailing latch reset. A count of 2 would mean the return was duplicated or relocated, which could place a `return true` after the reset and change the latch semantics. Exactly 1, in its original position immediately after branch 1's gated `Update` call, is the only configuration that preserves the invariant.

**`Activated = false` must be exactly 1.** This is the trailing latch reset. The search is case-sensitive, so it does **not** match the field initializer `_activated = false` at the top of the class — the lower-case backing-field assignment is a different token. Were the search case-insensitive it would return 2 and the gate would be measuring the wrong thing. A count of 2 would indicate a second reset was added, most plausibly inside branch 1 "for symmetry", which is precisely the prohibited change.

## Structural read-back

The method body was read back after all Phase 2 edits. Branch 1 is unchanged in structure:

```csharp
            if (Key.Contains(other))
            {
                if (Activated && Update is not null)
                    Update(Key.Substring(other.Length - 1, 1));
                return true;
            }
```

The `return true;` is inside branch 1's block, before the `else if` chain, and therefore before the single trailing `Activated = false;`. Branch 1 does **not** fall through to the reset. The trailing reset remains at the end of the method, reachable only from branches 2 and 3 and from the no-branch-taken path:

```csharp
            Activated = false;
            return false;
```

## Reason the constraint exists

`KeyboardHandler` re-arms `Activated` only when the filter length is 1, and it then performs three passes within a single keystroke: a `ContainsKey` probe, a `FilterKeys` probe, and an indexer/`Find` probe. If branch 1 cleared the latch, the first (`ContainsKey`) pass would consume the activation and the two later passes would see `Activated == false`, so the matching row's `Update` would not fire and the item-number label would stop advancing. The early return is what makes the matching row's idempotent `Update` repeat across those passes, which is the intended and load-bearing behaviour.

This is the single highest-impact regression risk in the change, because it is a plausible "tidy-up" that no compiler error and no pre-existing test would have caught.

## Behavioural corroboration

Two tests pin this invariant behaviourally, in addition to the structural counts above:

- `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` (pre-existing) asserts `ka.Activated.Should().BeTrue()` after a matching probe. It passed in the P0-T15 baseline, passed in the P1-T9 pre-fix red run, and must pass unmodified after the fix. It fails immediately if branch 1 begins clearing the latch.
- `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` (new test (b), added by P1-T3) drives a matching probe then a non-matching probe on the same instance and asserts the captured `Update` sequence is `"b"` then `"a"`. The second element can only be captured if the latch survived the first, matching probe. It passed in the P1-T9 pre-fix run, confirming it pins existing correct behaviour rather than asserting the fix.

Both are verified green at P2-T6.

Output Summary: Both anti-regression counts hold at their P0-T19 baseline values: `return true;` occurs exactly 1 time and `Activated = false` occurs exactly 1 time in `QuickFiler/Controllers/KaStringAsync.cs`. A structural read-back confirms branch 1's `return true;` sits inside branch 1's block ahead of the `else if` chain, so a matching probe returns before reaching the single trailing `Activated = false;` and does not clear the latch. The `Activated = false` search is case-sensitive and therefore correctly excludes the `_activated = false` field initializer, which would otherwise have inflated the count to 2 and made the gate measure the wrong thing. The invariant is additionally pinned behaviourally by the pre-existing `KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue` and by new test (b) `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar`, both of which passed in the pre-fix run and are re-verified at P2-T6.
