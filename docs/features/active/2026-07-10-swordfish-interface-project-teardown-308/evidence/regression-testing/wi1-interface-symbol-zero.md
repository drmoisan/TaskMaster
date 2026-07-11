# WI-1 — Dangling Interface Symbol Verification (P1-T5)

- **Timestamp:** 2026-07-11T13-05
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Removals performed (WI-1)

- `git rm UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs` (P1-T1, AC-3)
- `git rm UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection2.cs` (P1-T2, AC-4)
- `git rm UtilitiesCS/Interfaces/IToDo/ISubjectMapSco.cs` (P1-T3, AC-5)
- Removed dead `private static void UpdateForMove(..., ISubjectMapSco subMap)` from `QuickFiler/Controllers/QfcExplorerController.cs` (P1-T4, AC-6)
- Removed the three corresponding `<Compile Include>` entries from `UtilitiesCS/UtilitiesCS.csproj` (required for legacy non-SDK project; otherwise the build references missing files)

## Verification

- **Command:** `git grep -n "IScoCollection2\?\b|ISubjectMapSco" -- "*.cs"`
- **EXIT_CODE:** 1
- **Output Summary:** zero matches — no dangling `IScoCollection`, `IScoCollection2`, or `ISubjectMapSco` symbol remains in any `*.cs`.

- **Command:** `git grep -n "UpdateForMove" -- "QuickFiler/Controllers/QfcExplorerController.cs"`
- **EXIT_CODE:** 1
- **Output Summary:** zero matches — the dead method is gone; no call site existed.

- **Command:** `grep -rn "IScoCollection|ISubjectMapSco" --include="*.csproj" .`
- **EXIT_CODE:** 1
- **Output Summary:** zero matches — no residual `<Compile Include>` for the deleted interfaces.

## Verdict

WI-1 complete. Delivers AC-3, AC-4, AC-5, AC-6. No dangling symbol repo-wide.
