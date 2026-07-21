# legacy-scodictionary-removal (Issue #315)

- Date captured: 2026-07-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/legacy-scodictionary-removal/ (Issue #315)

- Issue: #315
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/315
- Last Updated: 2026-07-11
- Work Mode: full-feature

## Problem / Why

The legacy `ScoDictionary<TKey,TValue>` class in
`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs` derives from the vendored
`Swordfish.NET.Collections.ConcurrentObservableDictionary<,>` (resolved via `using Swordfish.NET.Collections;`).
Epic child F1 (issue #306) re-pointed every production consumer to the first-party `ScoDictionaryNew<>` class
and left the old class in place as an explicitly-declined optional cleanup. The class now has zero production
consumers but remains compiled and is exercised only by test code. Its continued presence blocks epic child F5
(issue #308, swordfish-interface-project-teardown), which cannot remove the `UtilitiesSwordfish` project
reference or delete the vendored project folders while a compiled first-party type still binds to
`Swordfish.NET.Collections`.

## Proposed Behavior

Retire the legacy `ScoDictionary<TKey,TValue>` class:
- Delete `SCODictionary.cs` and its `<Compile Include>` entry in `UtilitiesCS.csproj`.
- Reconcile every test that references the old class: delete tests that exercise `ScoDictionary`-specific
  behavior (now obsolete), and retarget tests that use `ScoDictionary` only as a concrete stand-in for generic
  serialization/wrapper infrastructure to `ScoDictionaryNew<>` so that infrastructure coverage is preserved.
- Remove `<Compile Include>` entries in `UtilitiesCS.Test.csproj` for any deleted test file.
- Update any stale comments that still name the old Swordfish-based `ScoDictionary`.
- Leave no remaining production or test binding to `Swordfish.NET.Collections` via `ScoDictionary`.

## Acceptance Criteria (early draft)

- [ ] `SCODictionary.cs` no longer exists and its `<Compile Include>` entry is removed from `UtilitiesCS.csproj`.
- [ ] No production or test code references the legacy `ScoDictionary<>` class or its `Swordfish.NET.Collections` binding.
- [ ] Generic serialization/wrapper test coverage that used `ScoDictionary` as a stand-in is preserved by retargeting to a first-party type.
- [ ] On-disk JSON compatibility is preserved for any persisted payload touched by retargeted tests.
- [ ] Full C# toolchain passes (CSharpier, analyzers, nullable+TreatWarningsAsErrors, MSTest) with zero test regressions and no coverage regression on changed lines.

## Constraints & Risks

- Cross-cutting epic NFR: preserve on-disk JSON compatibility for persisted payloads.
- Do not expand into F5 (#308) scope (interface/project/solution teardown).
- Legacy non-SDK VSTO project: `.csproj` `<Compile Include>` entries must be edited by hand; there are no implicit/global usings.

## Test Conditions to Consider

- [ ] Retargeted generic-serialization tests still pass against `ScoDictionaryNew<>`.
- [ ] JSON on-disk compatibility verified for retargeted persistence tests.
- [ ] No regression in overall MSTest suite; changed-line coverage preserved.

## Next Step

- [ ] Promote to GitHub issue (refactor)
- [ ] Create `docs/features/active/legacy-scodictionary-removal/` folder from the template
