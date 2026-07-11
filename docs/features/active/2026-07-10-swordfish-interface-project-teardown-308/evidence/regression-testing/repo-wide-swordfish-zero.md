# Repo-Wide Swordfish Zero (P5-T6) — AC-13

- **Timestamp:** 2026-07-11T13-30
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Verification

- **Command:** `git grep "Swordfish" -- "*.cs"`  — **EXIT_CODE:** 1 (zero matches)
- **Command:** `git grep "Swordfish" -- "*.csproj"` — **EXIT_CODE:** 1 (zero matches)
- **Command:** `git grep "Swordfish" -- "*.sln"` — **EXIT_CODE:** 1 (zero matches)
- **Output Summary:** ZERO matches over `*.cs`, `*.csproj`, and `*.sln`. Any remaining `Swordfish`
  mentions are limited to Markdown docs and `.claude/agent-memory/**`, which are outside the code
  globs (archived docs/memory, permitted by AC-13).

## Documentary-comment reconciliation (mechanically-necessary for AC-13)

After the structural teardown, ten documentary `Swordfish` comment mentions survived in six first-party
files authored by F1/F2 (prose such as "Swordfish-free" and dangling `<c>`-references to the deleted
vendored type). These are not code bindings, but the literal AC-13 code-glob search requires zero. To
satisfy F5's explicit epic-completion goal (spec Overview: "a repo-wide search for Swordfish over
`*.cs`/`*.csproj`/`*.sln` returns only archived docs/memory"), the ten mentions were reworded minimally
(comment-only, no behavior change), preserving the explanatory intent:

- `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs` (4 mentions: "Swordfish-free" -> "vendored-dependency-free"; dropped the dangling `<c>Swordfish.NET.Collections...</c>` clause; "vendored Swordfish base" -> "former vendored base"; "Swordfish observable semantics" -> "observable semantics")
- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs` (1)
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (1)
- `UtilitiesCS.Test/.../ConcurrentObservableCollection_Tests.cs` (2)
- `UtilitiesCS.Test/.../SloStack_Tests.cs` (1)
- `UtilitiesCS.Test/.../ScoDictionaryNew_OnDiskCompatibility_Tests.cs` (1)

The historical "replaced a vendored Swordfish library" context is preserved in the feature/epic
Markdown docs (the AC-13-permitted archived-docs location).

## Verdict

AC-13 delivered: the repo-wide `Swordfish` code-glob search returns zero first-party matches.
