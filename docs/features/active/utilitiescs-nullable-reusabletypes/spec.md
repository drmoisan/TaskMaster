# utilitiescs-nullable-reusabletypes — Spec

- **Issue:** #366
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-10
- **Status:** Draft
- **Version:** 0.1

## Overview

What need or gap does this idea address?

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile rather than a silently-skipped incremental build, cannot be enforced
against new code until the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under a per-file `#nullable enable` opt-in architecture. This feature is the
Wave-0 child that remediates the `UtilitiesCS/ReusableTypeClasses/` directory tree
(recursively, including `TimedActions/` and `NewSmartSerializable/`).

Scope is the ReusableTypeClasses cluster: 54 `.cs` files recursively, of which 3
WinForms-host-derived and Designer-generated files are exempt (see Scope Decision), leaving
51 files in scope. None of the 54 files currently carry `#nullable enable`; this is a
greenfield remediation for the whole cluster. These are reusable base and value types
(collections, serialization bases, matrices, timed-action helpers, locking structures)
consumed across module boundaries. Their nullability annotations become cross-module
contracts that downstream epic children (OutlookObjects, EmailIntelligence, Dialogs
clusters) consume, so the annotations must reflect actual null behavior. This is the
`cross_module_contract_change` complexity-floor signal that places the child at complexity
band C3. This work is null-annotation and null-safety remediation only; it introduces no
behavior changes.

## Behavior

What should the feature do at a high level?

Each remediated file receives a per-file `#nullable enable` pragma and is brought to zero
CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation applies
nullable annotations (`?`), generic constraints (`where TKey : notnull` where required by an
annotated BCL base — see the CS8714 subsection), unconstrained `T?` / `TValue?` returns and
`out` parameters, null-flow corrections, and null-forgiving operators (`!`) only where
justified. Existing null guards already present in the files remain as-is.

The work is annotation and null-safety only. There are no behavior changes, no refactors, no
API redesign, no feature work, and no file splitting. Public method signatures and public
type surfaces remain behavior-compatible: an existing caller that compiles today continues to
compile and behaves identically. The annotation choices reflect the true null behavior of
each member so that the resulting signatures are safe contracts for downstream consumers.
Files that emit no CS86xx diagnostics and are not required for a clean opted-in build remain
non-opted-in and must not be cross-blocked.

## Scope Decision (in-scope vs. exempt)

Recommendation from research (`research/research-findings.2026-07-18T22-10.md`, section 3):
opt in every production file under `ReusableTypeClasses/` that emits CS86xx, except the
WinForms-host-derived and Designer-generated files. Net: 51 files in scope, 3 exempt.

Exempt (do NOT add `#nullable enable`), per the CLAUDE.md General Unit Test Policy WinForms
exemption (b) covering "WinForms form-derived classes and Designer-generated code":

- `Config/ConfigViewer.Designer.cs` (3734 lines) — Designer-generated. A pragma here would be
  overwritten on the next designer round-trip and delivers no downstream contract value.
- `Config/ConfigViewer.cs` (`public partial class ConfigViewer : Form`) — Form-derived; its
  members are UI event handlers that cannot be unit-tested without a live message pump and are
  not consumed as reusable cross-module contracts.
- `Config/ConfigGroupBox.cs` (`internal class ConfigGroupBox : GroupBox`) — WinForms
  control-derived; same posture.

In scope (opt in), explicitly including two Config-directory files that are NOT WinForms-host
types:

- `Config/ConfigController.cs` — a plain controller class (not `Form`/control-derived) with an
  injectable test seam and dedicated tests. This matches the policy carve-back that testable
  seams within otherwise-WinForms-bound assemblies are NOT exempt. It dereferences `Viewer` and
  constructs a `ConfigViewer`, so it emits CS86xx and needs the pragma.
- `Config/NewSmartSerializableConfig.cs` — a data/config type implementing
  `ISmartSerializableConfig` with `PropertyChanged` and Newtonsoft round-tripping; it has
  dedicated tests.
- All remaining 49 collection, serialization, matrix, timed-action, and locking types.

Under AC6, leaving the three exempt files null-oblivious does not cross-block any opted-in
file: null-oblivious types are treated as "unknown null-state" by consumers, never as errors.

Interface-only files (`ISimpleActionBagObserver`, `ILockingLinkedList`,
`ILockingLinkedListObserver`, and likely `IConcurrentObservableCollectionSeams`) contain no
method bodies and emit no CS86xx. Per AC1 the pragma is only required on files that emit
CS86xx; the pragma may be added to these for cluster consistency but they are effectively
verify-only.

## CS8714 `notnull`-Constraint Decision (maintainer ratification required)

This is the single highest-risk decision in the child and is called out prominently because it
is the one place where "accurate annotation" and "no API redesign" are in tension.

`ConcurrentObservableDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>` declares
`TKey` unconstrained. Under `#nullable enable`, the annotated BCL `ConcurrentDictionary<TKey,
TValue>` requires `where TKey : notnull`. An unconstrained `TKey` is "not known to be
non-null," so the base type argument emits **CS8714** ("Nullability of type argument 'TKey'
doesn't match 'notnull' constraint"). This propagates to the derived generic dictionary bases:
`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, and `ScDictionary`.

Important nuance: **CS8714 is an 87xx diagnostic, not CS86xx.** AC1 targets CS86xx, but the
verification gate is `/t:Rebuild /p:TreatWarningsAsErrors=true`, under which CS8714 in a
nullable-enabled file becomes an error and blocks the build. The plan must therefore resolve it
even though it is outside the literal CS86xx set.

Decision (recommended, annotation-only): add `where TKey : notnull` to
`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, and `ScDictionary`.

- This is the accurate contract: `ConcurrentDictionary` rejects null keys at runtime with
  `ArgumentNullException` regardless, so the constraint records existing runtime behavior.
- It is IL-metadata-only with no runtime behavior change (satisfies AC3) and gives downstream
  consumers the honest key contract (satisfies AC5).
- Existing callers instantiate with non-null reference or value keys and are unaffected; a
  caller would warn only if it explicitly used a nullable-reference key, of which there are none
  today.

Because adding a generic constraint touches the public generic-parameter list of shared
reusable types, this is a public generic-parameter-list contract change that **must be ratified
by the maintainer during execution before the planner or executor commits it.** The rejected
alternative — `#pragma warning disable CS8714` — suppresses rather than fixes, leaves an
inaccurate contract, and is discouraged by policy.

The `ConcurrentBag<T>`-based types (`ConcurrentObservableBag`, `ScBag`) take `T` with no
`notnull` requirement and are NOT affected by this decision.

`NewtonsoftHelpers` (#9004) is a SEPARATE sibling child and is OUT OF SCOPE here. While it
remains null-oblivious, converter references from this cluster see it as unknown-null-state (no
CS86xx forced across the boundary). Do not annotate or touch any `NewtonsoftHelpers` file; only
annotate the local usage sites in this cluster.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public signatures and public type surfaces
  of the remediated reusable types remain behavior-compatible. The observable change is limited
  to nullability annotations and the ratified `where TKey : notnull` generic constraint on the
  four dictionary bases, which are additive contract metadata rather than a source- or
  binary-breaking behavior change.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

There is no CLI surface and no new API. This is a library-internal change. The relevant "API
surface" is the set of nullability annotations applied to the public members of the reusable
types in `UtilitiesCS/ReusableTypeClasses/`, plus the `where TKey : notnull` constraint added
to the four generic dictionary bases per the CS8714 decision.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag is
  added. No `/p:Nullable=enable` global flag is introduced into any verification command (see
  Toolchain Note).
- Contracts and validation rules:
  - Public signatures remain behavior-compatible; only nullability annotations change (for
    example, an uninitialized non-nullable event `PropertyChangedEventHandler PropertyChanged`
    becomes `PropertyChangedEventHandler?`; an `object sender` handler parameter becomes
    `object? sender`; a `default(TValue)` local or `Find` return becomes `TValue?` / `T?`).
  - The four generic dictionary bases gain `where TKey : notnull`, recording the existing runtime
    non-null-key contract.
  - Annotation choices reflect each member's actual null behavior. Because these are shared
    reusable types, the annotations become cross-module contracts consumed by downstream epic
    children; an incorrect annotation could propagate a false null-state assumption downstream.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
    available or polyfilled on this target and must not be used or added (see Constraints & Risks).

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered.
- Caching or persistence details: none. Serialization round-trips are unchanged; fields that
  Newtonsoft populates by reflection after construction are annotated (for example `= null!` with
  a "set by deserialization" comment) to reflect that they are non-null after a successful
  round-trip, without altering the serialization behavior.
- Migration or backfill requirements (if any): none. In particular, no project-level `<Nullable>`
  element is introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>` element today
  and must keep none. Enforcement is per-file pragma only.

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

- Per-file `#nullable enable` opt-in only. `UtilitiesCS.csproj` has no `<Nullable>` element and
  must keep none; no project-level or solution-level `<Nullable>` flip is performed. Enforcement
  is per-file pragma only.
- Target framework net481, C# 12 (`LangVersion` 12.0). All nullable syntax is available: `?`,
  `!`, unconstrained `T?`, `where T : notnull`, and `is null` / `is not null` flow analysis.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` are NOT available on
  this target and are NOT polyfilled in the repository. They must not be used or added. Zero
  CS86xx is reachable without them using plain `?`, `where TKey : notnull`, unconstrained `T?`,
  guard clauses, and justified `!`. Adding such a polyfill would be new production surface and is
  out of scope.
- Do NOT convert any serialization type to `record` / `init` / `record struct`. These fail
  CS0518 on net481, which lacks `IsExternalInit`. Reference-type fields that Newtonsoft populates
  become `= null!` (with a comment) or a nullable annotation where the property is genuinely
  optional.
- Six files exceed the general 500-line file limit (`ObservableDictionary` 834,
  `SmartSerializable` 596, `SerializableList` 575, `SmartSerializableBase` 534,
  `LockingObservableLinkedList` 522, plus the exempt Designer file). All are pre-existing. This
  child is annotation-only and MUST NOT split any file (that would be a refactor and out of
  scope). Flag for a separate future issue; do not fix here.
- Prefer annotation plus justified `!` over new runtime guard statements. New `if (x is null)
  throw` statements are executable lines that would require new test coverage (AC4 pressure) and
  could constitute a behavior change (AC3). Existing guards stay as-is.
- Annotations on shared reusable types are cross-module contracts; incorrect annotations could
  propagate false null-state assumptions to downstream Wave-1 children.
- The generic dictionary bases require care so that the `where TKey : notnull` decision (above)
  is applied consistently across all four affected types and ratified by the maintainer before it
  is committed.
- No editing of `.claude/rules/*`. The global-`/p:Nullable=enable`-versus-per-file-pragma
  tension is flagged for the maintainer and deferred to the Wave-2 CI capstone child; it is not
  resolved here.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each
  in-scope file that emits CS86xx and bring each to zero CS86xx under the pragma; add
  `where TKey : notnull` to the four generic dictionary bases (pending maintainer ratification);
  verify interface-only leaves compile clean with no remediation expected. No project or solution
  file changes.
- New classes/functions/commands to add or update: none. No new types, methods, commands, or
  files are added; only nullability annotations, the four `where TKey : notnull` constraints, and
  minimal null-flow corrections on existing members change.
- Batch grouping (from research, section 7; base/leaf-first so shared bases are annotated before
  dependents, minimizing re-touch):
  - Phase 1 — trivial leaves (EventArgs, observers, interfaces, batch/helper): 13 files.
  - Phase 2 — standalone value/util types: `AsyncQueue`, `AsyncLazy`, `LazyTry`, `StackGeek`,
    `StackObjectCS`, `TreeNodeOfT`, `DataConverter2d`.
  - Phase 3 — matrices: `DenMatrix`, `JaggedMatrix`, `Matrix`.
  - Phase 4 — timed actions: `TimerWrapper`, `TimedAsyncTask`, `TimedBatchAction`,
    `TimedQueueOfActions`, `TimedDiskWriter`.
  - Phase 5 — locking core: `LockingLinkedListNode`, `LockingLinkedList`,
    `LockingObservableLinkedListNode`, `LockingObservableLinkedList`.
  - Phase 6 — concurrent-observable bases + bag (the CS8714 `where TKey : notnull` decision is
    ratified here before any dependent consumes it): `ConcurrentObservableBag`,
    `ConcurrentObservableCollection`, `ConcurrentObservableCollection.Serialization`,
    `ConcurrentObservableDictionary`, `ObservableDictionary`.
  - Phase 7 — SmartSerializable family + config controller (base-first, highest cross-module
    contract scrutiny): `NewSmartSerializableConfig`, `SmartSerializableBase`,
    `SmartSerializable`, `SmartSerializableStatic`, `SmartSerializableNonTyped`,
    `SmartSerializableLoader`, `ConfigController`.
  - Phase 8 — serializable wrappers (depend on Phases 6-7): `SerializableList`, `ScBag`,
    `ScoDictionaryStatic`, `ScoDictionaryNew`, `SloLinkedList`, `SloStack`, `ScDictionary`.
  - Exempt (not a phase): `ConfigViewer.cs`, `ConfigViewer.Designer.cs`, `ConfigGroupBox.cs`.
  - Total in scope: 51 files across 8 phases. The full task-by-task sequencing belongs to the
    atomic plan, not this spec.
- Dependency changes (new/removed packages) and rationale: none.
- Logging/telemetry additions and locations: none.
- Rollout plan (feature flags, staged deploys, fallback path): not applicable. Each remediated
  file is independently mergeable because non-opted-in files remain null-oblivious and are not
  cross-blocking under the per-file pragma architecture.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

Acceptance criteria (authoritative for full-feature mode; consistent with `issue.md` AC1-AC6):

- [ ] AC1: Every `.cs` file under `UtilitiesCS/ReusableTypeClasses/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [ ] AC3: No behavior change; existing tests still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of the remediated reusable types remain behavior-compatible;
  nullability annotations reflect actual null behavior so they are safe contracts for downstream
  epic consumers. This includes the ratified `where TKey : notnull` constraint on the four
  generic dictionary bases, which records the existing non-null-key runtime contract.
- [ ] AC6: Non-opted-in files elsewhere in the repository are not cross-blocked by this change.

## Seeded Test Conditions (from potential)

- [ ] Existing `UtilitiesCS.Test/` suite covering these reusable types continues to pass with no
  behavior change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and justified
  `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) produces zero CS86xx (and zero CS8714 on
  the four dictionary bases) for the remediated files, without passing `/p:Nullable=enable`
  globally.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order:

1. `csharpier .` (adding a pragma line, `?` annotations, and generic constraints reformats; run
   before each build).
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style). New
   analyzer severities remain `suggestion` (root `.editorconfig`) so they cannot break the
   nullable gate.
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`. This is the pragma-driven gate (matching PR #361's
   `/t:Rebuild`). Under `TreatWarningsAsErrors`, any CS86xx (and any CS8714 on the four dictionary
   bases) in a pragma-enabled file becomes an error while non-opted files stay silent.
4. `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag forces
nullable project-wide and surfaces the full pre-existing debt across the solution, drowning this
child's signal. That global-flag-versus-per-file-pragma mismatch is the rules-versus-convention
conflict the epic flags for the maintainer and defers to the Wave-2 CI capstone child; resolving
it is out of scope here.
