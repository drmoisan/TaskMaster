# WI-0 Preflight Precondition — CLEAR Record (P0-T2)

- **Timestamp:** 2026-07-11T12-52
- **Feature:** swordfish-interface-project-teardown (#308), epic swordfish-removal child F5
- **Branch under evaluation:** feature/swordfish-interface-project-teardown-308 @ 1b65f7a7 (integration tip; all wave-0 features F1-F4 merged, plus remediation child #315/PR #316)
- **Result:** CLEAR — WI-0 assertions PASS. F5 proceeds to Phases 1-5.

## Prior HALT resolved

A previous attempt (2026-07-11T05-35 @ db6dc0e9) HALTed at this gate: the first-party
production type file
`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs` bound its base
to `Swordfish.NET.Collections.ConcurrentObservableDictionary` via `using Swordfish.NET.Collections;`
— a category-A binding outside F5's deletion scope (dictionary-lineage work owned by F1).

That blocker was resolved by remediation child #315 (PR #316, merged), which retired the legacy
Swordfish-bound `ScoDictionary<>` class. The file is now absent:

- **Command:** `ls UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`
- **EXIT_CODE:** 2 (No such file or directory)
- **Result:** confirmed deleted.

## Assertions evaluated

### Assertion (a): zero category-A `Swordfish` bindings in production `*.cs` outside F5 scope

- **Command:** `git grep -n "using Swordfish" -- "*.cs"`
- **EXIT_CODE:** 0 (matches found)
- **Command (broad type binding):** `git grep -n "Swordfish\.NET" -- "*.cs"`
- **EXIT_CODE:** 0 (matches found)
- **Result:** PASS. Every match is inside F5's own deletion scope or is documentary:

| File | Line | Match | F5 disposition |
|---|---|---|---|
| `UtilitiesCS.Test/ReusableTypeClasses/ObservableDictionary_Tests.cs` | 6 | `using Swordfish.NET.Collections;` | In scope — WI-4 (P2-T1) |
| `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs` | 8 | `using Swordfish.NET.Collections;` | In scope — WI-1 (P1-T1) |
| `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection2.cs` | 6 | `using Swordfish.NET.General.Collections;` | In scope — WI-1 (P1-T2) |
| `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs` | 14 | `/// vendored <c>Swordfish.NET.Collections...</c>` | DOCUMENTARY XML-doc comment, not a binding — not a HALT |
| `UtilitiesSwordfish.Test/**` | various | namespace/using/assembly literals | In scope — WI-3 (P4-T4) folder deletion |
| `UtilitiesSwordfish/**` | various | namespace/using literals | In scope — WI-3 (P4-T3) folder deletion |

No category-A first-party production **binding** exists outside F5's deletion scope. The single
surviving first-party documentary comment at `ConcurrentObservableCollection.cs:14` references the
vendored type F5 deletes; it is not a HALT condition and is reconciled at P5-T6 (AC-13).

### Assertion (b): `TraceUtility.cs` holds no `UtilitiesSwordfish.NET.*` literal

- **Command:** `git grep -n "UtilitiesSwordfish.NET" -- "UtilitiesCS/HelperClasses/Logging/TraceUtility.cs"`
- **EXIT_CODE:** 1 (zero matches)
- **Result:** PASS. TraceUtility.cs is clean (F4 landed for this item).

## Search scope / patterns (auditable)

- **SearchScope:** entire repository at `1b65f7a7`, tracked `*.cs` files (git grep).
- **SearchPatterns:** `using Swordfish`, `Swordfish\.NET`, `UtilitiesSwordfish.NET` (scoped to TraceUtility.cs).
- **SearchResult:** all `Swordfish.NET` `*.cs` matches are inside F5 deletion targets
  (three interfaces/test + `UtilitiesSwordfish/` + `UtilitiesSwordfish.Test/`) except one documentary
  comment in the surviving first-party `ConcurrentObservableCollection.cs`. Zero category-A bindings
  outside scope; zero `TraceUtility.cs` literal matches.

## Verdict

WI-0 CLEARS. Delivers AC-1, AC-2. Proceeding to Phase 1.
