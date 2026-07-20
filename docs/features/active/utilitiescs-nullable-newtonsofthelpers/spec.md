# utilitiescs-nullable-newtonsofthelpers — Spec

- **Issue:** #367
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (child, Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-05
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/NewtonsoftHelpers/` directory tree (19 `.cs` files across the root and the
`MonoExtension/` and `SDIL Reader/` subfolders: custom Newtonsoft.Json
`JsonConverter`/`SerializationBinder`/`ITraceWriter` implementations, the dictionary wrapper
converters, and the SDIL Reader IL-parsing helpers) carries such pre-existing nullable debt.
These are serialization helpers consumed across module boundaries; their nullability annotations
become contracts that downstream persistence, settings-store, and app-globals code consume.

This feature remediates that debt for the `NewtonsoftHelpers/` tree only, using a per-file
`#nullable enable` opt-in. It is annotation and null-safety work exclusively. It introduces no
behavior change and no refactor.

## Behavior

Remediate the pre-existing nullable-reference-type debt across `UtilitiesCS/NewtonsoftHelpers/`
using a per-file `#nullable enable` opt-in. The following are maintainer-mandated hard
constraints, not options; no alternative architecture is to be proposed or adopted:

- Add a `#nullable enable` pragma to each remediated file and bring that file to zero CS86xx
  diagnostics under the pragma.
- Do NOT enable nullable at the project or solution level. `UtilitiesCS.csproj` has no
  `<Nullable>` element and must keep none. No project-level or solution-level `<Nullable>`
  element may be introduced by this feature.
- Annotation and null-safety ONLY: nullable annotations (`?`), null guards, null-forgiving
  operators (`!`) only where justified, and null-flow corrections. No behavior changes, no
  refactors, no API redesign, no feature work.
- Keep public signatures behavior-compatible; annotate to reflect the actual runtime null
  behavior so the annotations serve as accurate downstream contracts. `JsonConverter<T>` /
  `JsonConverter`, `ISerializationBinder`, and `ITraceWriter` overrides carry framework-defined
  nullability that must be MATCHED, not changed (see API / CLI Surface).

Files that are not opted-in remain in an oblivious nullable context and are not cross-blocking.
This is the mechanism that lets each epic child merge independently without requiring the entire
epic (~2131 diagnostics across ~234 files) to be fixed first.

## Inputs / Outputs

- Inputs (files): the 19 `.cs` files under `UtilitiesCS/NewtonsoftHelpers/` (recursive, including
  the `MonoExtension/` and `SDIL Reader/` subfolders). The Newtonsoft.Json 13.0.4 assembly
  (referenced from `packages/Newtonsoft.Json.13.0.4/lib/net45/`) supplies the framework-defined
  nullability metadata against which the opted-in overrides are checked; it is a read-only input,
  not modified.
- Outputs (source changes): a `#nullable enable` pragma plus annotation/null-safety edits on each
  in-scope file that emits CS86xx; no new files, no removed files, no project-file edits.
- Config keys and defaults: none introduced. `UtilitiesCS.csproj` remains without a `<Nullable>`
  element.
- Versioning or backward-compatibility constraints: public member signatures must remain
  behavior-compatible. Nullability annotations added to public members become cross-module
  contracts consumed by serialization/persistence callers (ReusableTypes, OutlookObjects/Store,
  EmailIntelligence, and the app-level `AppGlobalsConverter` consumers); they must reflect actual
  null behavior rather than change it.

## API / CLI Surface

This feature exposes no new commands or CLI. The "surface" is the set of nullability annotations
applied to the converters, binders, trace writers, and wrappers. These annotations ARE the
cross-module contract consumed outside `NewtonsoftHelpers/`.

Framework-defined nullability that must be MATCHED (from Newtonsoft.Json 13.0.4, which embeds
`[Nullable]`/`[NullableContext]` metadata in `lib/net45`). Annotate implementations to the
framework signatures; do not restate them differently:

- `JsonConverter<T>` (generic): `ReadJson(JsonReader reader, Type objectType, T? existingValue,
  bool hasExistingValue, JsonSerializer serializer)` and `WriteJson(JsonWriter writer, T? value,
  JsonSerializer serializer)`. Legitimately-nullable positions: `existingValue` (`T?`), `value`
  (`T?`), and the `ReadJson` return (`T?`). Non-null positions that must stay non-null: `reader`,
  `objectType`, `serializer`, `writer`.
- non-generic `JsonConverter`: `ReadJson(..., object? existingValue, ...)`,
  `WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)`,
  `CanConvert(Type objectType)`. Nullable: `existingValue`/`value`/`ReadJson` return (`object?`).
  Non-null: `objectType`, `reader`, `writer`, `serializer`.
- `ISerializationBinder`: `Type BindToType(string? assemblyName, string typeName)` and
  `void BindToName(Type serializedType, out string? assemblyName, out string? typeName)`.
  Nullable: `assemblyName` in-parameter, and both `BindToName` `out string?` parameters. Non-null:
  the `BindToType` return `Type`, `typeName`, and `serializedType`.
- `ITraceWriter`: `void Trace(TraceLevel level, string message, Exception? ex)`. Nullable: `ex`
  (`Exception?`). Non-null: `message`.

Top cross-module-contract files (annotate deliberately; preserve current runtime behavior). The
override signatures are pinned by the framework, so the degrees of freedom that ripple outward are
the `ReadJson` return nullability and the public constructor parameter nullability:

- **ScoDictionaryConverter** (`UtilitiesCS`, generic `JsonConverter<TDerived>` plus an inner
  non-generic `JsonConverter`) — the primary `[JsonConverter]`-registered surface, consumed via
  `ScoDictionaryNew` across ReusableTypes / OutlookObjects/Store / EmailIntelligence. Generic
  `ReadJson` returns `TDerived?` (`wrapper?.ToDerived()`); the inner non-generic returns `object?`.
- **ScDictionaryConverter** (generic `JsonConverter<TDerived>`) — `ReadJson` returns `TDerived?`;
  `existingValue`/`value` become `TDerived?`.
- **PeopleScoConverter** (`JsonConverter<PeopleScoDictionaryNew>`) — `ReadJson` returns
  `PeopleScoDictionaryNew?`; `existingValue`/`value` become `PeopleScoDictionaryNew?`. A duplicate
  type exists (see Constraints & Risks item 3); annotate only the in-scope `NewtonsoftHelpers/`
  copy.
- **FilePathHelperConverter** (`JsonConverter<FilePathHelper>`) — largest converter body; `value`
  becomes `FilePathHelper?`; `ExtractFolderPath` returns `string?`; cross-module (FilePathHelper
  serialization, owned by sibling child #364). Keep the `FilePathHelperConverter(IFileSystemFolderPaths)`
  constructor parameter non-null (required dependency).
- **WrapperScoDictionary**, **WrapperScDictionary**, **WrapperPeopleScoDictionaryNew** — the
  reflection-based wrappers that feed the dictionary converters. Their public members
  (`CoDictionary`/`ConcurrentDictionary`, `RemainingObject`, `ToDerived()`/`ToComposition()`) are
  consumed by the converters and by reflection callers. `RemainingObject` is a `[JsonProperty]`
  public `object` populated only during (de)serialization; its annotation (`object?` vs `= null!`)
  is a per-file contract decision. The per-file `ModifyGetMethod`/`ModifySetMethod` return
  nullability differs across the three files and must each match existing per-file behavior without
  unifying.

Additional framework-override files (smaller bodies): `AppGlobalsConverter`
(`JsonConverter<IApplicationGlobals>`; keep its constructor parameter non-null),
`PeopleScoRemainingObjectConverter` (non-generic), `NonRecursiveConverter` (already conformed),
`KnownTypesBinder` (`ISerializationBinder`), `NConsoleTraceWriter` / `NLogTraceWriter`
(`ITraceWriter`). Non-override helpers (no framework contract; internal to the cluster):
`AllInclusiveBinder`, `MonoExtension`, `DerivedCompositionConverter_ConcurrentDictionary`, and the
`SDIL Reader/` trio (`ILGlobals`, `ILInstruction`, `MethodBodyReader`).

Contracts and validation rules: annotations must express the null behavior that already occurs at
runtime. Where a member currently returns null but the implemented interface declares a non-null
return (`KnownTypesBinder.BindToType`), the behavior-preserving annotation keeps the non-null
signature and applies `!` with a `// why` comment, rather than a nullable contract change that
would not match the implemented member.

## Data & State

This feature introduces no data flow, storage, persistence, caching, migration, or backfill
changes. Edits are confined to compile-time nullability annotations and null-flow corrections in
source. The (de)serialization round-trip behavior of every converter, binder, trace writer, and
wrapper is unchanged by design; the "no behavior change" constraint means observable state
transitions before and after remediation are identical.

## Constraints & Risks

The following mechanics flags are carried verbatim in substance from the research findings and
govern execution:

1. **Pragma-only verification command (do NOT use `/p:Nullable=enable`).** Local and CI
   verification of the opted-in files must use the pragma-only build
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`, relying on each file's own `#nullable enable` pragma. It must
   NOT add `/p:Nullable=enable`, which would enable nullable project-wide and surface the whole
   epic's ~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #367.
   This is a deliberate, documented deviation from the stock CLAUDE.md / `.claude/rules/csharp.md`
   type-check command for this child only; it must NOT be resolved by editing `.claude/rules/*`.
2. **Three wrapper files exceed the repo 500-line limit — PRE-EXISTING.** `WrapperScoDictionary.cs`
   (~645 lines), `WrapperPeopleScoDictionaryNew.cs` (~607 lines), and `WrapperScDictionary.cs`
   (~520 lines) already exceed 500 lines before any pragma is added. Annotation-only work adds a
   `#nullable enable` line plus per-line annotations and cannot bring these under 500 without a
   refactor, which is outside annotation-only scope. Do NOT split them; FLAG them as a known
   pre-existing policy exception (same handling as `PrettyPrint.cs` in the sibling #364 spec).
   Every other in-scope file is well under 500.
3. **Duplicate `PeopleScoConverter` — confirm the live type before annotating.** An in-scope
   `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs` and an out-of-scope
   `ToDoModel/Data Model/People/PeopleScoConverter.cs` both exist and both declare namespace
   `ToDoModel.Data_Model.People`. The plan/executor must confirm which is registered/live before
   finalizing the in-scope file's `ReadJson` return contract. In-scope is the `NewtonsoftHelpers/`
   copy only; annotate only that file.
4. **`NLogTraceWriter.cs` declares its class in the GLOBAL namespace — leave unchanged.** This is a
   pre-existing structural oddity (no `namespace` block), not a nullable issue. Annotate the file
   in place; do not move it into a namespace (that would be a reference/behavior change out of
   scope). Note it so the executor does not "fix" it.
5. **`NonRecursiveConverter.cs` already carries a partial mid-file `#nullable enable`.** The pragma
   sits at line 27 and the `ReadJson`/`WriteJson`/`OnReadJson`/`OnWriteJson` overrides already use
   `object?`. Action is to normalize the pragma to the top of the file so the whole file is opted
   in, then run a confirmation pass to zero CS86xx. No new annotations are expected beyond the
   pragma move.
6. **Rules-vs-convention conflict (flagged at epic level, not resolved here).**
   `.claude/rules/csharp.md` documents forcing `/p:Nullable=enable` globally, which conflicts with
   the per-file opt-in convention. This is flagged at the epic level (capstone child); it is not
   resolved in this feature and no `.claude/rules/*` file is edited.

Additional constraints and risks:

- Follow the repo C# toolchain order (csharpier -> msbuild analyzers/codestyle -> msbuild
  type-check -> vstest with coverage). For this child the type-check stage uses the pragma-only
  form in item (1), not the stock `/p:Nullable=enable` form. Any test work uses MSTest + Moq +
  FluentAssertions.
- Annotations become cross-module contracts; incorrect annotations could propagate incorrect null
  assumptions to serialization/persistence callers (ReusableTypes, OutlookObjects/Store,
  EmailIntelligence, and the app-level `AppGlobalsConverter` consumers).
- Cross-cluster independence: `FilePathHelperConverter` dereferences `FilePathHelper` (owned by
  sibling child #364) and the dictionary converters/wrappers dereference types owned by the
  ReusableTypes (#9003) and Extensions (#363) children. Because members of a not-yet-opted-in
  (oblivious) type impose no nullable obligation on a nullable-context caller, this cluster can be
  remediated independently regardless of the sibling clusters' state
  (`NewtonsoftHelpers` has `depends_on: []` in the epic manifest). `UtilitiesCS/Extensions/NullExtensions.cs`
  is already `#nullable enable`, so `x.ThrowIfNull()` call sites yield correct non-null flow.
- Contract decisions that are deliberate (not mechanical): `AllInclusiveBinder.GetAssemblies()`
  return `Assembly[]?`; `KnownTypesBinder.BindToType` non-null return with `!`; `KnownTypesBinder`
  `KnownTypes` property nullability; `NConsoleTraceWriter.Log` as `Action<string, Exception?>?`;
  the wrappers' `RemainingObject` (`object?` vs `= null!`) and per-file `ModifyGet/SetMethod`
  return nullability.

## Implementation Strategy

- Implementation scope: add `#nullable enable` to each in-scope `NewtonsoftHelpers/` file that
  emits CS86xx and apply annotation/null-safety edits to reach zero CS86xx per file under the
  pragma-only build. No new classes, functions, or commands; no dependency changes; no
  logging/telemetry additions; no project-file edits.
- Phasing: the research identifies an 8-batch sequence, foundational/low-risk clusters first and
  cross-module/high-contract files last. Wrappers precede the dictionary converters because the
  converters consume `wrapper.ToDerived()`/`ToComposition()` return types; settling the wrappers'
  nullability first prevents re-touching the converters. Batches are subdirectory-cohesive and
  independently reviewable; each opts in its files and reaches zero CS86xx for those files under
  the pragma-only verification. The batches (scope, not fine-grained sequencing) are:
  1. Leaf / isolated helpers (no framework override, no cross-module contract): `AllInclusiveBinder.cs`,
     `MonoExtension/MonoExtension.cs`.
  2. SDIL Reader subfolder (cohesive, isolated IL parsing): `SDIL Reader/ILGlobals.cs`,
     `SDIL Reader/ILInstruction.cs`, `SDIL Reader/MethodBodyReader.cs`.
  3. Trace writers (`ITraceWriter`): `NConsoleTraceWriter.cs`, `NLogTraceWriter.cs` (annotate the
     GLOBAL-namespace file in place per Constraints item 4).
  4. Binder + simple converters (framework overrides, small bodies): `KnownTypesBinder.cs`,
     `AppGlobalsConverter.cs`, `PeopleScoRemainingObjectConverter.cs`, `NonRecursiveConverter.cs`
     (move the mid-file pragma to the top and confirm per Constraints item 5).
  5. Reflection composition helper: `DerivedCompositionConverter_ConcurrentDictionary.cs`.
  6. Wrappers (foundational to the dictionary converters; heavy reflection, >500-line flags):
     `WrapperScDictionary.cs`, `WrapperScoDictionary.cs`, `WrapperPeopleScoDictionaryNew.cs`.
  7. Dictionary converters (consume the Batch-6 wrappers; the `[JsonConverter]`-registered
     cross-module contracts): `ScDictionaryConverter.cs`, `ScoDictionaryConverter.cs`,
     `PeopleScoConverter.cs` (confirm the live duplicate per Constraints item 3).
  8. High-contract finish: `FilePathHelperConverter.cs` (largest converter body, cross-module
     FilePathHelper serialization).
- Verification per batch: build with the pragma-only command to capture a per-batch CS86xx
  baseline, then drive the opted-in files to zero; run that batch's corresponding
  `UtilitiesCS.Test/NewtonsoftHelpers/` tests (and the related `Threading/AppGlobalsConverterTests.cs`,
  `HelperClasses/NLogTraceWriter_Test.cs`, and `ToDoModel.Test` People tests) and require them
  green and behavior-identical.
- Rollout: no feature flags or staged deploys. Each batch is additive; non-opted-in files remain
  oblivious until remediated.

## Definition of Done

- [x] Every `.cs` file under `UtilitiesCS/NewtonsoftHelpers/` that emits CS86xx carries a
  `#nullable enable` pragma and compiles with zero nullable (CS86xx) diagnostics under the
  per-file pragma with `/p:TreatWarningsAsErrors=true`.
- [x] No project-level or solution-level `<Nullable>` element is introduced; `UtilitiesCS.csproj`
  retains none.
- [x] Changes are annotation/null-safety only: no behavior change, no API/signature semantics
  change, no refactor beyond nullable annotation.
- [x] Framework-override signatures (`JsonConverter<T>`/`JsonConverter`, `ISerializationBinder`,
  `ITraceWriter`) are MATCHED to the Newtonsoft.Json 13.0.4 nullability: nullable positions
  (`existingValue`, `value`, converter `ReadJson` returns, `BindToType` `assemblyName`, `BindToName`
  `out string?` params, `Trace` `Exception? ex`) are annotated nullable; non-null positions
  (`serializer`, `reader`, `writer`, `objectType`, `serializedType`, `typeName`, `message`) stay
  non-null.
- [x] All existing MSTest tests for UtilitiesCS still pass; no coverage regression on changed
  lines.
- [x] The full C# toolchain (csharpier -> analyzer/codestyle build -> nullable/
  TreatWarningsAsErrors build -> vstest with coverage) passes on the final pass, using the
  pragma-only type-check command (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without
  `/p:Nullable=enable`) for this child.
- [x] The three wrapper 500-line pre-existing violations (`WrapperScoDictionary.cs` ~645,
  `WrapperPeopleScoDictionaryNew.cs` ~607, `WrapperScDictionary.cs` ~520) are flagged (not fixed)
  in the feature docs; the files are not split.
- [x] The duplicate `PeopleScoConverter` is confirmed (which copy is live) before the in-scope
  file's `ReadJson` return contract is finalized; only the `NewtonsoftHelpers/` copy is annotated.
- [x] `NLogTraceWriter.cs` is annotated in place with its GLOBAL namespace unchanged.
- [x] `NonRecursiveConverter.cs` has its pragma normalized to the top of the file and is confirmed
  at zero CS86xx.

## Seeded Test Conditions (from potential)

- [x] Existing MSTest suite for UtilitiesCS still passes post-annotation.
- [x] No coverage regression on changed lines.
- [x] Nullable gate passes for the opted-in files using the pragma-only build
  (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`).
