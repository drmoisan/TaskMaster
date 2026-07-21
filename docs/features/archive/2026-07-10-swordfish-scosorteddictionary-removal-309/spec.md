# swordfish-scosorteddictionary-removal — Spec

- **Issue:** #309
- **Parent (optional):** epic `swordfish-removal` (child F3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10T21-10
- **Status:** Draft
- **Version:** 0.2

## Overview

The vendored Swordfish-based `ScoSortedDictionary<TKey,TValue>`
(`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`) derives
from the Swordfish `ConcurrentObservableSortedDictionary` and is believed to have no production
consumer — only its own definition and its test
(`UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`). It is one of the
first-party dependencies on `Swordfish.NET.*` that the swordfish-removal epic must eliminate so
the vendored `UtilitiesSwordfish` project can be torn down (epic child F5). Removing the unused
class shrinks the analyzer-exempt vendored surface and unblocks the No-COM/testability direction.

Research (`research/research.2026-07-10T21-10.md`) confirmed a GO recommendation: no genuine
production consumer of `ScoSortedDictionary` or `ConcurrentObservableSortedDictionary` exists
anywhere in first-party source, csproj files, or persisted JSON. The deliverable for this feature
is deletion only; no replacement type is introduced.

## Behavior

Confirm, with an auditable repo-wide search, that no production code references
`ScoSortedDictionary` or `ConcurrentObservableSortedDictionary`. This confirmation has already
been performed and recorded in `research/research.2026-07-10T21-10.md` (Q1); the atomic plan and
executor re-verify it is still true immediately before deleting. If confirmed, delete the class
and its test. This feature's deliverable is deletion only.

Scope boundary: delete ONLY `ScoSortedDictionary` and its test (and the two matching
`<Compile Include>` entries in the classic-format `UtilitiesCS.csproj` and
`UtilitiesCS.Test.csproj`, required so the build does not reference a deleted file). Do NOT delete
the `UtilitiesSwordfish` project, remove any `ProjectReference`, touch `TaskMaster.sln`, migrate
interfaces, or touch the F1 (dictionary), F2 (collection/stack), or F4 (raw-usage) types.

## Inputs / Outputs

This is a pure deletion with no new CLI flags, environment variables, or configuration keys. No
new artifacts, logs, or telemetry are produced by the feature itself.

- Inputs: none (no new inputs are introduced; the change consumes only the existing repo state —
  the four files/entries identified in the deletion surface below).
- Outputs: none (no new outputs, artifacts, or telemetry are introduced). The only observable
  output is the reduced source tree and a smaller compiled assembly.
- Config keys and defaults: not applicable — no configuration surface exists for this type today,
  and none is added or removed as configuration (its removal is a source-file deletion, not a
  config change).
- Versioning or backward-compatibility constraints: not applicable — `ScoSortedDictionary` is not
  a public API surface consumed outside `UtilitiesCS`/`UtilitiesCS.Test`, and no persisted data
  references it (see Data & State below).

## API / CLI Surface

Not applicable. This feature deletes an internal class and its dedicated test; it does not add,
change, or remove any command, CLI flag, HTTP endpoint, or public API contract. No new API/CLI
surface is introduced by this change.

## Data & State

Not applicable in the sense of new state: this feature introduces no data flow, storage, or state
changes. It removes dead code.

- Data transformations and invariants: none introduced. `ScoSortedDictionary` implements
  `ISmartSerializable`-style serialize/deserialize members, but research confirmed zero JSON
  fixtures, config, or persisted payloads reference `ScoSortedDictionary` or
  `ConcurrentObservableSortedDictionary` (research Q1, command 4 — zero `*.json` matches for
  either token repo-wide). There is no on-disk serialization format to preserve or migrate.
- Caching or persistence details: none. No persisted production consumer exists.
- Migration or backfill requirements: none. Because no production caller ever constructed,
  serialized, or deserialized a `ScoSortedDictionary` instance, there is no on-disk data to
  migrate and no backward-compatibility shim to add.

## Constraints & Risks

- If any production consumer exists, STOP and report it as a blocking finding rather than
  deleting. (Research already ran this check and found none; the executor re-verifies
  immediately before deleting as a final guard against drift between research and execution.)
- Classic (non-SDK) csproj format: a deleted `.cs` still referenced by `<Compile Include>` breaks
  the build; the Compile entries must be removed in the same change.
- On-disk serialization compatibility is not a concern for this type because it has no persisted
  production consumer (confirmed by research: zero `*.json` matches for either type name
  repo-wide).
- The Swordfish base type `ConcurrentObservableSortedDictionary` itself
  (`UtilitiesSwordfish/Collections/ConcurrentObservableSortedDictionary.cs`) and its own test
  (`UtilitiesSwordfish.Test/ObservableSortedDictionaryTest.xaml.cs`) are explicitly out of scope
  for this feature; they remain until epic child F5 tears down the `UtilitiesSwordfish` project.

## Implementation Strategy

Implementation scope is exactly four edits, all executed together in a single change so the build
never passes through a broken intermediate state:

1. Delete `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`.
2. Delete `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`.
3. Remove the `<Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs" />`
   entry from `UtilitiesCS/UtilitiesCS.csproj` (currently line 1047).
4. Remove the `<Compile Include="ReusableTypeClasses\ScoSortedDictionary_Tests.cs" />` entry from
   `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (currently line 414).

Both `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` use the classic (non-SDK-style) MSBuild
project format with explicit, per-file `<Compile Include>` item lists rather than implicit
globbing. A `<Compile Include>` entry pointing at a file that no longer exists on disk causes an
MSBuild error, so edits 3 and 4 are mandatory in the same change as edits 1 and 2, not optional
follow-up cleanup.

- New classes/functions/commands to add or update: none. No new type is introduced; no existing
  type other than `ScoSortedDictionary` and `ScoSortedDictionary_Tests` is touched.
- Dependency changes: none. The `Swordfish.NET.*` package reference and the `UtilitiesSwordfish`
  `ProjectReference` are left untouched; they still back other first-party types until F1, F2, F4,
  and F5 complete.
- Logging/telemetry additions: none.
- Rollout plan: not applicable. This is a source-tree deletion in a single commit/PR; there is no
  staged rollout, feature flag, or fallback path. The full C# toolchain gate below is the sole
  release gate.

Post-deletion verification: run the full C# toolchain in the required order — `csharpier .` →
.NET analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild` build) → nullable/type-check build
(`Nullable=enable`/`TreatWarningsAsErrors=true`) → MSTest (`vstest.console.exe ... /EnableCodeCoverage`)
— and confirm the solution builds and all tests pass with the four edits applied. Confirm no
coverage regression on files outside the deleted pair (the deleted class and its 100%-dedicated
test are expected to leave overall coverage materially unchanged).

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable) — in this feature, "updated" means the
      dedicated test file is deleted alongside the class it tests, with no other test file touched
- [ ] Edge cases and error handling covered by tests — not applicable; no new logic is introduced
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable) — not applicable; no telemetry surface exists
- [ ] Toolchain pass completed (format -> lint -> type-check -> test)

## Seeded Test Conditions (from potential)

- [ ] Repo-wide reference search (production `.cs`, `.csproj`, and non-code references) confirms zero remaining references to `ScoSortedDictionary` or `ConcurrentObservableSortedDictionary` after deletion, other than the unrelated Swordfish base type file (`UtilitiesSwordfish/Collections/ConcurrentObservableSortedDictionary.cs`, out of F3 scope) and this feature's own planning docs
- [ ] Full solution build + MSTest run green after deletion
- [ ] Coverage: deletion removes a class and its dedicated test together; verify no unrelated coverage regression on changed/remaining lines
