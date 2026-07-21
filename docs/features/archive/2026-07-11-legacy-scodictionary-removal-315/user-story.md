# legacy-scodictionary-removal - User Story

- **Issue:** #315
- **Parent:** epic/swordfish-removal-integration
- **Owner:** drmoisan
- **Last Updated:** 2026-07-11T11-20
- **Status:** Draft

## Audience

- Epic child F5 (#308, swordfish-interface-project-teardown), which cannot remove the `UtilitiesSwordfish`
  project reference or delete the vendored Swordfish folders until no compiled first-party type binds to
  `Swordfish.NET.Collections`.
- Repository maintainers responsible for the swordfish-removal integration epic.

## Story

As the maintainer driving the swordfish-removal epic, I want the legacy `ScoDictionary<TKey,TValue>` class and
its obsolete tests removed, and its stand-in test usages retargeted to the first-party `ScoDictionaryNew<>`, so
that the last first-party binding to `Swordfish.NET.Collections` via `ScoDictionary` is eliminated and F5 (#308)
can proceed with the interface/project teardown.

## Context

Epic child F1 (#306) migrated every production consumer to `ScoDictionaryNew<>` and left the old class in place
as a declined optional cleanup. The class now has zero production consumers but is still compiled and is
exercised only by test code. While it remains compiled, it keeps a first-party `Swordfish.NET.Collections`
binding alive, which blocks F5.

## What "done" looks like (from the reader's perspective)

- A maintainer can confirm `SCODictionary.cs` is gone and its `<Compile Include>` entry is removed from
  `UtilitiesCS.csproj`.
- A full-repo search shows no production or test code references the legacy `ScoDictionary<>` class or its
  `Swordfish.NET.Collections` binding.
- Generic serialization/wrapper test coverage that used `ScoDictionary` as a stand-in still exists, now
  targeting `ScoDictionaryNew<>`, so infrastructure coverage is not lost.
- Persisted-payload JSON round-trips are unchanged; `ScoDictionaryNew_OnDiskCompatibility_Tests.cs` stays green.
- The full C# toolchain (CSharpier, analyzers, nullable+TreatWarningsAsErrors, MSTest) is clean, with zero test
  regressions and no coverage regression on changed lines.
- The F5 owner can start the interface/project teardown without encountering a `ScoDictionary`-bound blocker.

## Boundaries (what this story does not deliver)

- It does not remove the `IScoDictionary`/`IPeopleScoDictionary` interfaces or the
  `ScoDictionaryConverter`/`WrapperScoDictionary` types (all owned by F5 or retained for `ScoDictionaryNew`).
- It does not remove the `UtilitiesSwordfish` project reference or delete vendored folders — that is F5 (#308).
- It does not touch the unrelated `ObservableDictionary_Tests.cs` Swordfish usage.

## Acceptance Criteria

- [x] `SCODictionary.cs` no longer exists and its `<Compile Include>` entry is removed from `UtilitiesCS.csproj`.
- [x] No production or test code references the legacy `ScoDictionary<>` class or its `Swordfish.NET.Collections` binding.
- [x] Generic serialization/wrapper test coverage that used `ScoDictionary` as a stand-in is preserved by retargeting to a first-party type.
- [x] On-disk JSON compatibility is preserved for any persisted payload touched by retargeted tests.
- [x] Full C# toolchain passes (CSharpier, analyzers, nullable+TreatWarningsAsErrors, MSTest) with zero test regressions and no coverage regression on changed lines.
