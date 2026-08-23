# Phase 3 — Dead API Removal Boundary Verification (Issue #445, AC7 through AC11)

Timestamp: 2026-08-22T09-47

Command:
```powershell
(git grep -n -F 'DelegateType' -- '*.cs' | Measure-Object -Line).Lines
(git grep -n -F '_update' -- 'QuickFiler/Controllers/KaStringAsync.cs' | Measure-Object -Line).Lines
```
plus the companion counts tabulated below, all using the Uniform Count Idiom. Run from `WS`.

EXIT_CODE: 0

## The two gates this task names

| Gate | Baseline (P0-T19) | Now | Required | Pass |
|---|---|---|---|---|
| `DelegateType` repository-wide over `*.cs` | 3 | **0** | 0 | yes |
| `_update` in `QuickFiler/Controllers/KaStringAsync.cs` | 3 | **3** | exactly 3 | yes |

The `DelegateType` gate is a CHANGE gate: it was non-zero and is now zero, so it could have failed. The `_update` gate on `KaStringAsync.cs` is a RETENTION gate on a file this plan edits: it holds at 3, proving the retained member was not over-deleted while the four dead copies were removed.

## Full removal-boundary table

| Measurement | Baseline | Now | Expected | Pass |
|---|---|---|---|---|
| `DelegateType` repo-wide, `*.cs` | 3 | 0 | 0 | yes |
| `DelegateType` in `KaChar.cs` | 1 | 0 | 0 | yes |
| `DelegateType` in `KaKey.cs` | 1 | 0 | 0 | yes |
| `DelegateType` in `IKbdAction.cs` | 1 (comment) | 0 | 0 | yes |
| `_update` in `KaChar.cs` | 6 | 0 | 0 | yes |
| `_update` in `KaKey.cs` | 6 | 0 | 0 | yes |
| `_update` in `KaStringAsync.cs` | 3 | 3 | 3 (retained) | yes |
| `Update` in `IKbdAction.cs` | 1 (comment) | 0 | 0 | yes |
| `bool KeyEquals(T other);` in `IKbdAction.cs` | 1 | 1 | 1 (retained) | yes |
| `using System.Windows.Forms;` in `KaChar.cs` | 1 | 0 | 0 | yes |
| `using System.Windows.Forms;` in `KaKey.cs` | 1 | 1 | 1 (retained) | yes |
| `Keys` in `KaChar.cs` | 1 | 0 | 0 | yes |
| `public Action<string> Update` repo-wide, `*.cs` | 5 | 1 | 1 | yes |
| `Update = update;` in `KaStringAsync.cs` | 1 | 1 | 1 (retained) | yes |

The repository-wide `public Action<string> Update` count falling from 5 declarations to 1 is the direct measurement of AC8 plus AC9: four dead copies removed, one live copy retained. The surviving declaration is `KaStringAsync`'s, and its five-argument constructor still assigns it (`Update = update;`, count 1), which is the AC9 requirement.

## AC9 read-site confirmation — why `KaStringAsync.Update` is genuinely live

`Update` on `KaStringAsync` is read in the `KeyEquals` body at branch 1's guard and its argument, and at branch 3's guard and its argument, and it is written by the five-argument constructor. Those are the read/write sites the spec cites, all still present. The four deleted copies had zero read sites and zero write sites, which is what made them dead API rather than an unused-but-live seam.

## AC11 — `IKbdAction.cs` live members are byte-identical

The complete diff for the interface file is two deleted comment lines and nothing else:

```
--- a/QuickFiler/Interfaces/IKbdAction.cs
+++ b/QuickFiler/Interfaces/IKbdAction.cs
@@ -12,7 +12,5 @@ namespace QuickFiler.Interfaces
         T Key { get; set; }
         U Delegate { get; set; }
         bool KeyEquals(T other);
-        //Action<string> Update { get; set; }
-        //Type DelegateType { get; }
     }
 }
```

There is no `+` line. The four live members are therefore byte-identical to their pre-change text:

```csharp
        string SourceId { get; set; }
        T Key { get; set; }
        U Delegate { get; set; }
        bool KeyEquals(T other);
```

No member was added to the interface. This matters because `KaStringAsync`, `KaCharAsync`, and `KaKeyAsync` do not declare `DelegateType`, so restoring it to the interface would not compile; and restoring `Update` would force four implementers to keep a member that is dead on all four.

## Implementer enumeration re-run after the deletions (not carried forward)

A fresh repository-wide search over `*.cs` for `IKbdAction<` was run after the edits, rather than relying on the P0-T19 enumeration:

```
QuickFiler/Controllers/KaChar.cs:10:    public class KaChar : IKbdAction<char, Action<char>>
QuickFiler/Controllers/KaChar.cs:45:    public class KaCharAsync : IKbdAction<char, Func<char, Task>>
QuickFiler/Controllers/KaKey.cs:11:    public class KaKey : IKbdAction<Keys, Action<Keys>>
QuickFiler/Controllers/KaKey.cs:46:    public class KaKeyAsync : IKbdAction<Keys, Func<Keys, Task>>
QuickFiler/Controllers/KaStringAsync.cs:10:    public class KaStringAsync : IKbdAction<string, Func<string, Task>>
QuickFiler/Controllers/KbdActions.cs:15:        where UClass : IKbdAction<TKey, VDelegate>, new()
QuickFiler/Interfaces/IKbdAction.cs:9:    public interface IKbdAction<T, U>
```

Exactly five implementing classes, the same five as at baseline, with no sixth implementer anywhere in the repository. The generic constraint in `KbdActions.cs` is unchanged and that file was not edited. Every implementer therefore satisfies the interface's unchanged four-member contract, and no implementer reports a delegate type it does not store: `DelegateType` no longer exists on any type, which was the substance of defect 3 (`KaChar` stored an `Action<char>` but reported `typeof(Action<Keys>)`).

## AC10 — the `using` asymmetry is correct and deliberate

`using System.Windows.Forms;` was removed from `KaChar.cs` and retained in `KaKey.cs`. The discriminator is the `Keys` count: it fell from 1 to 0 in `KaChar.cs`, because `Keys` appeared there only inside the deleted `DelegateType` body, so the `using` became unused and would raise an unused-directive diagnostic. In `KaKey.cs`, `Keys` is the type's key type throughout (`IKbdAction<Keys, Action<Keys>>`, `private Keys _key;`, `public Keys Key`, `KeyEquals(Keys other)`), so the directive remains load-bearing and is retained.

## Post-deletion file sizes

| File | Baseline | Now | Change |
|---|---|---|---|
| `QuickFiler/Controllers/KaChar.cs` | 99 | **79** | shrank 20 |
| `QuickFiler/Controllers/KaKey.cs` | 99 | **80** | shrank 19 |
| `QuickFiler/Interfaces/IKbdAction.cs` | 18 | **16** | shrank 2 |
| `QuickFiler/Controllers/KaStringAsync.cs` | 95 | 161 | grew 66 (guard clause plus XML doc) |
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 168 | 279 | grew 111 (four new tests) |

All five are well under the 500-line cap, and the three files AC20 requires to shrink all shrank. `KaKey.cs` shrank by one line fewer than `KaChar.cs` because `KaChar.cs` also lost its `using` directive.

Output Summary: Both gates this task names pass. The repository-wide `DelegateType` count over `*.cs` fell from 3 at baseline to **0**, and `_update` in `KaStringAsync.cs` holds at exactly **3**. The full boundary table confirms `_update` is 0 in both `KaChar.cs` and `KaKey.cs`, `DelegateType` and `Update` are both 0 in `IKbdAction.cs`, `bool KeyEquals(T other);` is retained at 1, `using System.Windows.Forms;` is 0 in `KaChar.cs` and 1 in `KaKey.cs`, and `Keys` is 0 in `KaChar.cs`. Repository-wide, `public Action<string> Update` declarations fell from 5 to 1, that survivor being `KaStringAsync`'s, whose constructor still assigns it. The `IKbdAction.cs` diff contains two deleted comment lines and no added line, so the four live members are byte-identical and no member was added. A fresh repository-wide `IKbdAction<` search re-confirms exactly five implementers with no sixth, and no type now reports a delegate type it does not store. The three files AC20 requires to shrink did shrink: `KaChar.cs` 99 to 79, `KaKey.cs` 99 to 80, `IKbdAction.cs` 18 to 16.
