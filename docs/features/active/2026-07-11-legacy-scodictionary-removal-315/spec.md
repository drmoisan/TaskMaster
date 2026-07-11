# legacy-scodictionary-removal - Refactor Spec

- **Issue:** #315
- **Parent (optional):** epic/swordfish-removal-integration
- **Owner:** drmoisan
- **Last Updated:** 2026-07-11T11-20
- **Status:** Draft
- **Version:** 0.2

## Intent & Outcomes

The legacy `ScoDictionary<TKey,TValue>` class in
`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs` derives from the vendored
`Swordfish.NET.Collections.ConcurrentObservableDictionary<,>` (resolved via `using Swordfish.NET.Collections;`).
Epic child F1 (issue #306) re-pointed every production consumer to the first-party `ScoDictionaryNew<>` class
and left the old class in place as an explicitly-declined optional cleanup. The class now has zero production
consumers but remains compiled and is exercised only by test code. Its continued presence blocks epic child F5
(issue #308, swordfish-interface-project-teardown), which cannot remove the `UtilitiesSwordfish` project
reference or delete the vendored project folders while a compiled first-party type still binds to
`Swordfish.NET.Collections`.

Outcome: the legacy class and its obsolete tests are removed, generic-infrastructure test coverage that used
the class only as a stand-in is preserved by retargeting to `ScoDictionaryNew<>`, and no source file binds
`Swordfish.NET.Collections` on behalf of `ScoDictionary`. This unblocks F5 (#308).

## Invariants (must not change)

- Generic serialization/wrapper infrastructure behavior exercised by the retargeted tests
  (`SmartSerializableBase.DeserializeObject<T>`, `SmartSerializableStatic.IsSmartSerializable`,
  `SmartSerializableNonTyped.IsSmartSerializable`) must remain identical. `ScoDictionaryNew<>` is a verified
  drop-in for the stand-in usages (parameterless constructor and `typeof(...)`), and returns `false` from
  `IsSmartSerializable` exactly as the old class did.
- On-disk JSON compatibility: any persisted payload touched by the retargeted tests must round-trip in the
  same flat `{"key":value}` dictionary shape. The retargeted tests use bare/default settings only and never
  exercise the globals-converter (`ScoDictionaryConverter`/`WrapperScoDictionary`) path.
- Public surfaces retained (owned by F5, not this change): the `IScoDictionary<TKey,TValue>` and
  `IPeopleScoDictionary` interfaces, and the `ScoDictionaryConverter`/`WrapperScoDictionary` family (live code
  for `ScoDictionaryNew`).
- Performance characteristics to preserve (latency/throughput/memory): none affected; this is a dead-code
  removal with no runtime path change.
- Compatibility guarantees (CLI flags, config schemas, versions): unchanged; no config or schema is touched.

## Scope (structural changes)

Retire the legacy `ScoDictionary<TKey,TValue>` class, limited to exactly the following:

- Delete `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs` and remove its single
  `<Compile Include>` entry from `UtilitiesCS/UtilitiesCS.csproj` (line 1048).
- DELETE the obsolete `ScoDictionary`-specific tests and remove their two `<Compile Include>` entries from
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (lines 380-381):
  - `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs`
  - `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs`
- RETARGET the generic-serialization-infrastructure tests that use `ScoDictionary` only as a concrete
  stand-in, swapping the type to `ScoDictionaryNew<>` (pure type swap):
  - `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs`
  - `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableNonTyped_Tests.cs`
  - `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs`
- Update stale comments that still name the old Swordfish-based `ScoDictionary`:
  - `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`
  - `UtilitiesCS.Test/EmailIntelligence/FolderRemapController_Tests.cs`
  - `UtilitiesCS.Test/EmailIntelligence/SubjectMapEncoder_Tests.cs`

Leave no remaining production or test binding to `Swordfish.NET.Collections` via `ScoDictionary`.

## Non-Goals

Explicitly out of scope (do not touch):

- The `IScoDictionary<TKey,TValue>` interface (`ISCODictionary.cs`) and `IPeopleScoDictionary`.
- `ScoDictionaryConverter`, `WrapperScoDictionary`, and their tests
  (`ScoDictionaryConverterTests.cs`, `WrapperScoDictionaryTest.cs`).
- `IntelligenceConfig_Tests.cs` (references only `ScoDictionaryNew`/`PeopleScoDictionaryNew`).
- F5 (#308) interface/project/solution teardown: removing the `UtilitiesSwordfish` project reference or
  deleting vendored folders.
- The unrelated `ObservableDictionary_Tests.cs` Swordfish usage (tests a different Swordfish type).
- Any new behavior, performance change, or UX change.

## Dependencies / Touchpoints

- Downstream: epic child F5 (#308, swordfish-interface-project-teardown) depends on this change to remove the
  last first-party binding to `Swordfish.NET.Collections` via `ScoDictionary`.
- Upstream: epic child F1 (#306) already migrated all production consumers to `ScoDictionaryNew<>`.
- Legacy non-SDK VSTO projects: `.csproj` `<Compile Include>` entries must be edited by hand; there are no
  implicit/global usings.
- Required coordination (other teams, CI/CD, release tooling): none beyond the epic merge gate.

## Risks & Mitigations

- Cross-cutting epic NFR — preserve on-disk JSON compatibility for persisted payloads. Mitigation: the
  retargeted tests use bare/default serializer settings and never touch the globals-converter path, so no JSON
  shape changes; existing `ScoDictionaryNew_OnDiskCompatibility_Tests.cs` remains the authoritative
  persisted-dictionary compatibility coverage and must stay green.
- Scope creep into F5 (#308). Mitigation: interface, converter, wrapper, and project/solution teardown are
  explicitly listed as Non-Goals.
- Manual `.csproj` editing risk in legacy non-SDK VSTO projects. Mitigation: remove only the three named
  `<Compile Include>` lines; verify a clean analyzer/warnings-as-errors build afterward.
- Retarget substitution divergence. Mitigation: research verified `ScoDictionaryNew<>` is a drop-in for the
  stand-in usages (parameterless constructor / `typeof`), and that `IsSmartSerializable` returns `false`
  identically; no retargeted test asserts on the `Config` property that `ScoDictionaryNew` adds.

## Technical Specifications

- Files/modules expected to change:
  - Production: delete `SCODictionary.cs`; edit `UtilitiesCS.csproj` (remove line 1048); update comment in
    `FolderScorer.cs`.
  - Tests: delete `SCODictionary_Tests.cs` and `SCODictionary_Additional_Tests.cs`; edit
    `UtilitiesCS.Test.csproj` (remove lines 380-381); retarget `SmartSerializableBase_Tests.cs`,
    `SmartSerializableNonTyped_Tests.cs`, `SmartSerializableStatic_Tests.cs`; update comments in
    `FolderRemapController_Tests.cs` and `SubjectMapEncoder_Tests.cs`.
- Public interfaces/contracts affected: none. The removed class had zero production consumers; interfaces and
  converter/wrapper types are unaffected.
- Data flow or validation adjustments: none. Retargeted tests are pure type swaps over generic infrastructure.
- Logging/telemetry updates: none.
- Migration or backfill needs: none. No persisted payload format changes.

## Test Strategy

- Regression tests to add or update: retarget three `SmartSerializable*_Tests.cs` files to `ScoDictionaryNew<>`
  (type swap on stand-in usages: `SmartSerializableBase_Tests.cs` lines 52/58/73;
  `SmartSerializableNonTyped_Tests.cs` lines 24/50/76/82/96 plus comment updates;
  `SmartSerializableStatic_Tests.cs` line 29 plus comment). No new tests are required.
- Invariant validation tests: retargeted generic-serialization tests must pass unchanged in intent against
  `ScoDictionaryNew<>`; the three `IsSmartSerializable...ReturnsFalse` cases must continue to return `false`.
- Edge cases and negative scenarios: invalid-JSON deserialization paths
  (`DeserializeObject_InvalidJson_ReturnsNull`/`ReturnsDefault`) preserved by the retarget.
- Error handling and logging verification: no error-handling behavior changes; not applicable beyond existing
  coverage.
- Coverage impact and targets for changed lines/modules: no coverage regression on changed lines. Deleting the
  obsolete class and its tests removes both from the denominator; retargeted files retain their existing
  coverage of the generic infrastructure.
- On-disk JSON compatibility: `ScoDictionaryNew_OnDiskCompatibility_Tests.cs` (existing, out of scope to edit)
  must remain green; it is the sufficient authoritative coverage for persisted-dictionary shape.
- Toolchain commands to run (format → lint → type-check → test):
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps: confirm a full-repo grep shows no remaining `using Swordfish.NET.Collections;` hit
  attributable to `ScoDictionary` (the `SCODictionary.cs` hit is gone; unrelated residual hits are out of
  scope).

## Definition of Done

- [x] Structure matches this spec; legacy paths retired or redirected
- [x] Invariants validated with tests or comparisons
- [x] Imports/tooling/entry points updated
- [x] Edge cases and error handling verified
- [x] Tests, linting, and type checks clean
- [x] Docs updated (initiative/README/tasks as needed)
- [x] Toolchain pass completed (format → lint → type-check → test)

## Acceptance Criteria

- [x] `SCODictionary.cs` no longer exists and its `<Compile Include>` entry is removed from `UtilitiesCS.csproj`.
- [x] No production or test code references the legacy `ScoDictionary<>` class or its `Swordfish.NET.Collections` binding.
- [x] Generic serialization/wrapper test coverage that used `ScoDictionary` as a stand-in is preserved by retargeting to a first-party type.
- [x] On-disk JSON compatibility is preserved for any persisted payload touched by retargeted tests.
- [x] Full C# toolchain passes (CSharpier, analyzers, nullable+TreatWarningsAsErrors, MSTest) with zero test regressions and no coverage regression on changed lines.

## Seeded Test Conditions (from potential)
- [x] Retargeted generic-serialization tests still pass against `ScoDictionaryNew<>`.
- [x] JSON on-disk compatibility verified for retargeted persistence tests.
- [x] No regression in overall MSTest suite; changed-line coverage preserved.
