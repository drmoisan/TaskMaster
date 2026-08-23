# Phase 0 — Structural-Count Baseline for the Literal Register (Issue #445)

Timestamp: 2026-08-22T09-36

Command: the Uniform Count Idiom, once per Literal Register token, run from `WS` via `pwsh -NoProfile`:
```powershell
(git grep -n -F 'TOKEN' -- 'PATHSPEC' | Measure-Object -Line).Lines
```
Plus the physical line count of each of the five in-scope files:
```powershell
(Get-Content -LiteralPath 'PATH').Count
```

EXIT_CODE: 0

`git grep` searches the working-tree contents of tracked files, so an uncommitted edit is visible and no commit task is required. Every pathspec is restricted to `*.cs` or to a single named file, so this plan document, `spec.md`, and the research artifact are never counted.

## Token counts — measured versus the Literal Register "now" column

| Token | Pathspec | Measured | Register "now" | Match |
|---|---|---|---|---|
| `DelegateType` | `*.cs` | 3 | 3 | yes |
| `_update` | `QuickFiler/Controllers/KaChar.cs` | 6 | 6 | yes |
| `_update` | `QuickFiler/Controllers/KaKey.cs` | 6 | 6 | yes |
| `_update` | `QuickFiler/Controllers/KaStringAsync.cs` | 3 | 3 (retained) | yes |
| `using System.Windows.Forms;` | `QuickFiler/Controllers/KaChar.cs` | 1 | 1 | yes |
| `using System.Windows.Forms;` | `QuickFiler/Controllers/KaKey.cs` | 1 | 1 (retained) | yes |
| `if (Activated && Update is not null)` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | 1 | yes |
| `if (Update is not null)` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | 1 | yes |
| `nameof(other)` | `QuickFiler/Controllers/KaStringAsync.cs` | 0 | 0 | yes |
| `ArgumentNullException(nameof(other))` | `QuickFiler/Controllers/KaStringAsync.cs` | 0 | 0 | yes |
| `bool KeyEquals(T other);` | `QuickFiler/Interfaces/IKbdAction.cs` | 1 | 1 (retained) | yes |
| `Keys` | `QuickFiler/Controllers/KaChar.cs` | 1 | 1 | yes |
| `return true;` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | 1 (retained) | yes |
| `Activated = false` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | 1 (retained) | yes |
| `Key.Substring(other.Length - 1, 1)` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | 1 (retained) | yes |
| `Key.Contains(other)` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | 1 (retained) | yes |
| `latch` | `QuickFiler/Controllers/KaStringAsync.cs` | 0 | 0 | yes |
| `///` | `QuickFiler/Controllers/KaStringAsync.cs` | 0 | 0 | yes |
| `[TestMethod]` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 8 | 8 | yes |
| `KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 1 | 1 | yes |
| `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 | 0 | yes |
| `KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 | 0 | yes |
| `KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 | 0 | yes |
| `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 | 0 | yes |
| `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 | 0 | yes |
| `Be("b"` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 1 | 1 (retained, AC19) | yes |
| `ThrowExactly` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 0 | 0 | yes |
| `Update` | `QuickFiler/Interfaces/IKbdAction.cs` | 1 | (comment line, to become 0) | recorded |

Every measured value equals the register's "now" value. The ten values the task calls out explicitly all agree: `DelegateType` 3, `KaChar.cs` `_update` 6, `KaKey.cs` `_update` 6, `KaStringAsync.cs` `_update` 3, `if (Activated && Update is not null)` 1, `return true;` 1, `latch` 0, `///` 0, `[TestMethod]` 8, `ThrowExactly` 0.

## Physical line counts of the five in-scope files

| File | Baseline lines | Plan's expected baseline | Match |
|---|---|---|---|
| `QuickFiler/Controllers/KaStringAsync.cs` | 95 | 95 | yes |
| `QuickFiler/Controllers/KaChar.cs` | 99 | 99 | yes |
| `QuickFiler/Controllers/KaKey.cs` | 99 | 99 | yes |
| `QuickFiler/Interfaces/IKbdAction.cs` | 18 | 18 | yes |
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 168 | 168 | yes |

### Counting-method note (recorded so P5-T3 reproduces this method exactly)

`(Get-Content -LiteralPath $f | Measure-Object -Line).Lines` is NOT the correct idiom here: `Measure-Object -Line` omits empty lines, and it returned 86 / 84 / 84 / 17 / 143 for these files — every figure below the true count. The correct idiom is `(Get-Content -LiteralPath $f).Count`, which counts physical lines including blanks. Both methods were run and the physical-line figures were cross-checked against `wc -l`, which agreed exactly on all five files (95, 99, 99, 18, 168; 479 total). P5-T3 must use the physical-line method so its comparison against these baselines is commensurable.

## Line-number verification against the plan and spec

Every `file:line` citation the plan and spec rely on was independently re-derived by reading the current files, not assumed from the dated citations. All were confirmed unchanged:

- `KaStringAsync.cs:59` `if (Key.Contains(other))`, `:61` branch-1 gate, `:62` the out-of-scope `Substring`, `:63` `return true;`, `:70` branch-3 test, `:72` `if (Update is not null)` (the defect), `:77` `Activated = false`, `:81-86` the retained `Update` property, `:25` its constructor assignment, `:50` the `_activated = false` initializer.
- `KaChar.cs:6` the `using`, `:43-46` `DelegateType`, `:50-55` `KaChar.Update`, `:92-97` `KaCharAsync.Update`, `:11`/`:58` the two class declarations.
- `KaKey.cs:6` the `using` (retained), `:43-46` `DelegateType`, `:50-55` `KaKey.Update`, `:92-97` `KaKeyAsync.Update`.
- `IKbdAction.cs:11-14` the four live members, `:15-16` the two commented-out lines.
- `KaStringAsyncTests.cs:20-25` the `NewKa` helper, `:76-96` the AC3 witness test, `:89-91` the `.Be("b")` assertion, `:134` the method to rename, `:141` its `ka.Activated = true`, `:154-166` the null-delegates test.

## Repository-wide implementer enumeration (independent of the plan's list)

A repository-wide search over `*.cs` for `IKbdAction<` was run rather than relying on the plan's enumeration. It returns exactly five implementing classes plus the interface and one generic constraint:

```
QuickFiler/Controllers/KaChar.cs:11:    public class KaChar : IKbdAction<char, Action<char>>
QuickFiler/Controllers/KaChar.cs:58:    public class KaCharAsync : IKbdAction<char, Func<char, Task>>
QuickFiler/Controllers/KaKey.cs:11:    public class KaKey : IKbdAction<Keys, Action<Keys>>
QuickFiler/Controllers/KaKey.cs:58:    public class KaKeyAsync : IKbdAction<Keys, Func<Keys, Task>>
QuickFiler/Controllers/KaStringAsync.cs:10:    public class KaStringAsync : IKbdAction<string, Func<string, Task>>
QuickFiler/Controllers/KbdActions.cs:15:        where UClass : IKbdAction<TKey, VDelegate>, new()
```

There is no sixth implementer. `DelegateType` is declared on only `KaChar` and `KaKey` (`:43` in each) with one commented-out mention at `IKbdAction.cs:16`; `KaCharAsync`, `KaKeyAsync`, and `KaStringAsync` do not declare it, which is why restoring it to the interface would not compile. `Update` is declared on all five but read only inside `KaStringAsync`.

Output Summary: All 28 Literal Register token counts were measured with the Uniform Count Idiom and every one equals the register's "now" value, including the ten the task names explicitly (`DelegateType` 3, `KaChar.cs` `_update` 6, `KaKey.cs` `_update` 6, `KaStringAsync.cs` `_update` 3, `if (Activated && Update is not null)` 1, `return true;` 1, `latch` 0, `///` 0, `[TestMethod]` 8, `ThrowExactly` 0). Physical line counts are 95, 99, 99, 18, and 168, matching the plan's expected baselines exactly and cross-checked against `wc -l`. A counting-method hazard is recorded: `Measure-Object -Line` omits blank lines and understates every file, so `(Get-Content).Count` is the idiom P5-T3 must reuse. Every `file:line` citation the plan and spec depend on was independently re-derived from the current files and confirmed unchanged. A repository-wide `IKbdAction<` search confirms exactly five implementers with no sixth.
