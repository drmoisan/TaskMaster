# `utilitiescs-nullable-newtonsofthelpers` — User Story

- Issue: #367
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T22-05
- Epic: utilitiescs-nullable-remediation (child, Wave 0)
- Work Mode: full-feature

## Story Statement

- As the repository maintainer, I want the `UtilitiesCS/NewtonsoftHelpers/` nullable-reference-type
  debt remediated under a per-file `#nullable enable` opt-in, so that the repaired CI nullable gate
  can be genuinely enforced on these serialization helpers without cross-blocking non-opted-in
  files elsewhere in the epic.
- As a developer of serialization/persistence code (ReusableTypes, OutlookObjects/Store,
  EmailIntelligence, and the app-globals consumers), I want the NewtonsoftHelpers converters,
  binders, trace writers, and wrappers annotated to reflect their actual null behavior and to match
  the Newtonsoft.Json framework signatures, so that I consume accurate nullability contracts and do
  not inherit incorrect null assumptions.

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/NewtonsoftHelpers/` directory tree (19 `.cs` files: custom Newtonsoft.Json
`JsonConverter`/`SerializationBinder`/`ITraceWriter` implementations, the dictionary wrapper
converters, and the SDIL Reader IL-parsing helpers) carries such pre-existing nullable debt. These
are serialization helpers consumed across module boundaries; their nullability annotations become
contracts that downstream persistence and settings-store code consume.

A global force-enable of nullable would make no epic child independently mergeable until all ~234
files (~2131 diagnostics) were fixed at once. The per-file opt-in lets this child be remediated and
merged on its own while non-opted-in files stay oblivious and non-cross-blocking. This directly
enables genuine CI nullable-gate enforcement on the opted-in files without cross-blocking files
that have not yet opted in.

## Personas & Scenarios

- Persona: Repository maintainer (drmoisan)
  - Who: owner of the nullable-remediation epic and the CI nullable gate.
  - Cares about: a genuinely enforceable nullable gate that does not permanently block future PRs;
    a per-file opt-in architecture that keeps each epic child independently mergeable.
  - Constraints: annotation and null-safety only — no behavior changes, no refactors, no API
    redesign; no project- or solution-level `<Nullable>` element; no editing of `.claude/rules/*`.
  - Goals and frustrations: wants the NewtonsoftHelpers debt cleared under the confirmed
    architecture, and wants scope conflicts (three >500-line wrappers, the duplicate
    `PeopleScoConverter`, the GLOBAL-namespace `NLogTraceWriter`, the rules-vs-convention conflict)
    surfaced as flags rather than silently resolved.
  - Context: NewtonsoftHelpers is a Wave-0 foundational cluster with `depends_on: []`; its
    annotations become the serialization contracts consumed by ReusableTypes, OutlookObjects/Store,
    and EmailIntelligence.

- Persona: Serialization/persistence consumer developer
  - Who: an agent or developer working on code that registers or invokes these converters
    (`ScoDictionaryNew`, the store (de)serialization path, the app-globals load path).
  - Cares about: consuming the converter/wrapper public members with nullability annotations that
    match the actual runtime behavior and the Newtonsoft framework signatures, so their own
    null-flow analysis is correct.
  - Constraints: must not have to re-derive or work around inaccurate NewtonsoftHelpers contracts.
  - Goals and frustrations: an incorrect annotation on a registered converter (for example, a
    `ReadJson` return marked non-null when the body returns `wrapper?.ToDerived()`) would propagate
    an incorrect assumption into every dependent (de)serialization call site.

- Scenario: Remediating and verifying a NewtonsoftHelpers batch
  - Who is acting: the executor delivering issue #367, batch by batch.
  - Trigger: the repaired nullable gate now surfaces pre-existing CS86xx in NewtonsoftHelpers.
  - Steps: opt each batch's files in with `#nullable enable`; apply annotation/null-safety edits
    (nullable `?`, guards, justified `!`, null-flow corrections), matching the Newtonsoft.Json
    framework signatures on all overrides; build with the pragma-only command
    (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`) to capture a
    per-batch baseline and drive the opted-in files to zero CS86xx; run the batch's MSTest tests
    and require them green and behavior-identical.
  - Obstacles/decisions: match the framework nullability on `existingValue`/`value`/`ReadJson`
    returns/`BindToType`/`BindToName`/`Trace` while keeping `serializer`/`reader`/`writer`/
    `objectType`/`typeName`/`message` non-null; move `NonRecursiveConverter.cs`'s mid-file pragma
    to the top and confirm; confirm which duplicate `PeopleScoConverter` is live before annotating
    the in-scope copy; annotate `NLogTraceWriter.cs` in place without touching its GLOBAL namespace;
    flag the three >500-line wrappers without splitting them; do not add `/p:Nullable=enable` to the
    verification command.
  - Expected outcome: every in-scope NewtonsoftHelpers file that emitted CS86xx is opted-in and
    clean under the pragma-only gate, with no behavior change and no coverage regression on changed
    lines, and all flagged conflicts documented for the maintainer.

## Acceptance Criteria

- [ ] Every `.cs` file under `UtilitiesCS/NewtonsoftHelpers/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] No project-level `<Nullable>` element is introduced in `UtilitiesCS.csproj`.
- [ ] No behavior change; existing tests still pass; no coverage regression on changed lines.

## Non-Goals

- No behavior changes, refactors, API redesign, or feature work of any kind. Nullable annotation
  and null-safety only.
- No project-level or solution-level `<Nullable>` element as an enforcement mechanism.
- No editing of `.claude/rules/*` to resolve the rules-vs-convention conflict (it is flagged at the
  epic level, capstone child).
- No splitting of the three >500-line wrappers (`WrapperScoDictionary.cs`,
  `WrapperPeopleScoDictionaryNew.cs`, `WrapperScDictionary.cs`) to meet the 500-line limit
  (pre-existing condition, flagged not fixed).
- No moving of `NLogTraceWriter.cs` out of the GLOBAL namespace (pre-existing structural oddity,
  annotated in place).
- No changes to the out-of-scope duplicate `ToDoModel/Data Model/People/PeopleScoConverter.cs`; only
  the in-scope `NewtonsoftHelpers/` copy is annotated.
- No changing of the framework-defined nullability on the Newtonsoft.Json overrides; implementations
  are annotated to MATCH the framework signatures, not to alter them.
