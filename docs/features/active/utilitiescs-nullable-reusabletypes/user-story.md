# `utilitiescs-nullable-reusabletypes` — User Story

- Issue: #366
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T22-10

## Story Statement

- As the repository maintainer who owns the CI nullable gate, I want the pre-existing CS86xx
  nullable debt in `UtilitiesCS/ReusableTypeClasses/` remediated under per-file `#nullable enable`
  and brought to zero diagnostics under `TreatWarningsAsErrors`, so that the gate repaired by PR
  #361 can be genuinely enforced against these reusable base and value types without permanently
  blocking future PRs.
- As a downstream epic-feature developer working on a Wave-1 cluster (OutlookObjects,
  EmailIntelligence, or Dialogs), I want the shared reusable types (collections, serialization
  bases, matrices, timed-action helpers, locking structures) annotated to reflect their actual
  null behavior, so that I can consume their nullability annotations as reliable cross-module
  contracts instead of guessing null-state or re-touching the ReusableTypeClasses files.

## Problem / Why

What need or gap does this idea address?

The CI nullable gate was silently failing to catch nullable-reference-type debt until PR #361
changed the CI step to `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` so it performs a
genuine recompile. The repaired gate cannot be enforced against new code while the pre-existing
CS86xx diagnostics remain across the repository. The chosen architecture is a per-file
`#nullable enable` opt-in: each remediated file is brought to zero CS86xx under its own pragma,
so files can be remediated and merged independently without a global force-enable that would
block every PR until all files were fixed at once.

This feature is the Wave-0 child covering `UtilitiesCS/ReusableTypeClasses/` (54 `.cs` files
recursively; 3 WinForms-host/Designer files exempt; 51 to remediate). These reusable base and
value types are consumed across module boundaries, so their annotations are remediated early:
their nullability annotations become the cross-module contracts that downstream Wave-1 children
depend on. This is the `cross_module_contract_change` complexity-floor signal placing the child
at complexity band C3. The work is null-annotation and null-safety only, with no behavior
change.

## Personas & Scenarios

- Persona: Repository maintainer (CI/quality owner)
  - who the user is: owns the CI nullable gate and the epic's per-file opt-in architecture.
  - what they care about: a gate that catches real null defects without blocking unrelated PRs;
    no behavior regressions; no reduction in coverage on changed lines; a correct, ratified
    resolution of the one public generic-parameter-list contract change (the `where TKey :
    notnull` constraint on the dictionary bases).
  - their constraints: the gate must rely on per-file pragmas, not a solution-level
    `<Nullable>enable`; `UtilitiesCS.csproj` must keep no `<Nullable>` element; `.claude/rules/*`
    must not be edited; net481 / C# 12 limits apply (no nullable post-condition attributes, no
    `record` / `init` conversions).
  - their goals and frustrations: wants the ReusableTypeClasses cluster provably clean under the
    pragma gate; frustrated by the pre-existing debt that made the gate a no-op.
  - their context and motivations: this Wave-0 child is one of several in the epic; the CI-gate
    finalization is the separate Wave-2 capstone child.
- Scenario: Ratifying the CS8714 constraint and enforcing the gate for the cluster
  - who is acting: the maintainer, reviewing the remediation branch.
  - what triggered the action: PR #361 repaired the gate; the epic sequences ReusableTypeClasses
    in Wave 0.
  - what steps they take: ratify the `where TKey : notnull` constraint on
    `ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, and
    `ScDictionary` (a public generic-parameter-list contract change) before it is committed; run
    the toolchain (csharpier → analyzers/code style → the pragma-driven `/t:Rebuild
    /p:TreatWarningsAsErrors=true` nullable gate → vstest coverage); confirm every CS86xx-emitting
    file carries `#nullable enable` and builds clean; confirm `UtilitiesCS.csproj` still has no
    `<Nullable>` element; confirm existing tests pass and changed-line coverage does not regress.
  - what obstacles or decisions occur: CS8714 (an 87xx diagnostic) is not in the literal CS86xx
    set but still blocks the `/t:Rebuild /p:TreatWarningsAsErrors=true` build, so it must be
    resolved via the ratified constraint rather than suppressed; nullable post-condition
    attributes are unavailable on net481 and must not be added; six over-limit files must not be
    split; `NewtonsoftHelpers` (#9004) is a separate sibling child and must not be touched; the
    global `/p:Nullable=enable` flag must not be used for verification because it drowns this
    child's signal in the full repo-wide debt.
  - what outcome they expect: the ReusableTypeClasses cluster passes the per-file pragma gate with
    zero CS86xx (and zero CS8714 on the dictionary bases) and no behavior change.

- Persona: Downstream epic-feature developer (Wave-1 consumer)
  - who the user is: a developer remediating a Wave-1 cluster that consumes reusable types such as
    `ScoDictionaryNew<TKey,TValue>`, the `SmartSerializable<T>` family,
    `ConcurrentObservableDictionary<TKey,TValue>`, `SloLinkedList<T>`, `ScDictionary`, `ScBag<T>`,
    `SerializableList<T>`, or `TreeNode<T>`.
  - what they care about: accurate nullability annotations on the shared types they build on, so
    their own null-flow analysis is correct; a stable, non-null key contract on the dictionary
    bases.
  - their constraints: they cannot re-open or re-annotate the ReusableTypeClasses files without
    creating cross-child churn; they depend on this child's annotations being correct.
  - their goals and frustrations: wants trustworthy contracts; frustrated if a reusable type
    annotated non-nullable actually returns or holds null (a false null-state assumption
    propagated downstream).
  - their context and motivations: their Wave-1 child lists `utilitiescs-nullable-reusabletypes`
    as a dependency and consumes its annotated contracts. Production consumers include
    EmailIntelligence (`People`, `SubjectMap`), OutlookObjects (`StoresWrapper`), and cross-project
    consumers in QuickFiler and TaskVisualization.
  - Scenario: Consuming an annotated contract
    - who is acting: the Wave-1 developer.
    - what triggered the action: their cluster references a reusable type or its members under
      `#nullable enable`.
    - what steps they take: rely on the type's annotated surface (for example a `TValue?` return,
      a nullable `TreeNode<T>?` `Parent`, a nullable event, or the `where TKey : notnull` key
      contract) to drive their own null handling.
    - what obstacles or decisions occur: none, provided the annotations reflect actual null
      behavior; an incorrect annotation would surface as a false-positive or missed CS86xx in
      their own code.
    - what outcome they expect: the annotations behave as documented contracts and require no
      changes to the ReusableTypeClasses files.

- Persona: The CI nullable gate (automated enforcement)
  - who the user is: the pragma-driven `/t:Rebuild /p:TreatWarningsAsErrors=true` build step
    repaired by PR #361.
  - what it cares about: a set of files that pass clean under the per-file pragma so the gate can
    be genuinely enforced instead of silently no-opping.
  - Scenario: The gate is enforceable for the cluster
    - what triggered the action: the ReusableTypeClasses files are opted in and remediated to zero
      diagnostics.
    - what outcome it expects: the gate compiles the opted-in files clean while leaving
      non-opted-in files elsewhere untouched and uncross-blocked, so future regressions in this
      cluster are caught at build time.

## Acceptance Criteria

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

## Non-Goals

Call out what is explicitly excluded from this feature.

- No project-level or solution-level nullable enable. No `<Nullable>` element is added to
  `UtilitiesCS.csproj`; enforcement is per-file pragma only.
- No behavior changes, refactors, or API redesign beyond the annotation-only `where TKey :
  notnull` constraint. This is null-annotation and null-safety remediation only. In particular,
  the six over-limit files (`ObservableDictionary`, `SmartSerializable`, `SerializableList`,
  `SmartSerializableBase`, `LockingObservableLinkedList`, and the exempt Designer file) are not
  split, and no serialization type is converted to a `record` / `init` / `record struct`.
- No opting the WinForms-host-derived and Designer-generated files (`ConfigViewer.cs`,
  `ConfigViewer.Designer.cs`, `ConfigGroupBox.cs`) into the pragma; they follow the COM/VSTO/
  WinForms exemption. `ConfigController.cs` and `NewSmartSerializableConfig.cs` remain in scope.
- No touching of `NewtonsoftHelpers` (#9004), which is a separate sibling child; only the local
  usage sites in this cluster are annotated.
- No use of nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) and no addition of a polyfill for them; they are unavailable on net481.
- No editing of `.claude/rules/*`. The rules-versus-convention conflict about the global
  `/p:Nullable=enable` flag is flagged for the maintainer, not resolved here.
- Finalizing the CI nullable-gate enforcement mechanism is the separate Wave-2 capstone child, not
  this feature.
