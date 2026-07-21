# P6-T1 — RecentsList<T> Dead-Code Check

Timestamp: 2026-07-11T00-12
Command: `rg -n "RecentsList<" --glob '**/*.cs'` (executed via the Grep tool; `/bin/` and `/obj/` excluded)
EXIT_CODE: 0

## Output Summary

The TYPE `RecentsList<T>` has no live production consumer. All 16 hits fall into three
expected categories:

- The type definition: `UtilitiesCS/EmailIntelligence/Recents/RecentsList.cs:11`
  (`public class RecentsList<T> : ScoCollection<T>`).
- Its direct tests: `UtilitiesCS.Test/EmailIntelligence/RecentsList_Tests.cs` (11 hits).
- Commented-out (dead) blocks:
  - `TaskMaster/AppGlobals/AppAutoFileObjects.cs:245,246,254,279` (all `//`-commented).
  - `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs:21` (`//`-commented).

The live `AppAutoFileObjects.RecentsList` PROPERTY is typed `SloLinkedList<string>` (not
`RecentsList<T>`) and is unaffected by deleting the `RecentsList<T>` type. The type is therefore
safe to delete in P6-T2/P6-T3.
