# `utilitiescs-nullable-email-classifier` — User Story

- Issue: #372
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T22-06
- Epic: `utilitiescs-nullable-remediation` (Wave 1)
- Integration branch: `epic/utilitiescs-nullable-remediation-integration`
- Upstream contract dependency: #363 (`utilitiescs-nullable-extensions`, Wave 0)

## Story Statement

- As the epic maintainer who owns the CI nullable gate, I want the pre-existing CS86xx
  nullable debt in the `UtilitiesCS/EmailIntelligence/` classifier cluster (`Bayesian`,
  `ClassifierGroups`, `Flags`) remediated under per-file `#nullable enable` and brought to
  zero diagnostics under `TreatWarningsAsErrors`, so that the gate repaired by PR #361 can be
  genuinely enforced against these T1 classifier files without permanently blocking future
  PRs and without altering any scoring behavior.
- As a downstream Wave-1+ classifier consumer who calls into these engines, I want their
  member signatures annotated to reflect their actual null behavior and to honor the upstream
  #363 extension contracts, so that I can rely on their nullability annotations as reliable
  contracts instead of guessing null-state or re-touching the classifier files.

## Problem / Why

What need or gap does this idea address?

The CI nullable gate was silently failing to catch nullable-reference-type debt until PR #361
changed the CI step to `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` so it performs a
genuine recompile. The repaired gate cannot be enforced against new code while roughly 2131
pre-existing CS86xx diagnostics remain across the repository. The chosen architecture is a
per-file `#nullable enable` opt-in: each remediated file is brought to zero CS86xx under its
own pragma, so files can be remediated and merged independently without a global force-enable
that would block every PR until all files were fixed at once.

This feature is the Wave-1 child covering the `UtilitiesCS/EmailIntelligence/` classifier
cluster: `Bayesian`, `ClassifierGroups`, and `Flags`. These modules are classified T1
(Critical) per `.claude/rules/quality-tiers.md`: they are classifier engines (SpamBayes,
Triage) whose behavior bugs can cause silent model drift or misclassification. The work is
null-annotation and null-safety only, with no change to classifier scoring, model logic, or
corpus/probability math, and no observable behavior change. It consumes the annotation
contracts published by the Wave-0 Extensions child (issue #363), whose shared extension
methods (for example `ThrowIfNull`, `IEnumerableExtensions`, `DictionaryExtensions`,
`NullExtensions`, `StringExtensions`) are called throughout these classifier files. Existing
golden, property, and characterization tests remain unchanged.

## Personas & Scenarios

- Persona: Epic maintainer (CI/quality owner)
  - who the user is: owns the CI nullable gate and the epic's per-file opt-in architecture.
  - what they care about: a gate that catches real null defects without blocking unrelated
    PRs; no behavior or scoring regressions on T1 classifier engines; no reduction in coverage
    on changed lines.
  - their constraints: the gate must rely on per-file pragmas, not a solution-level
    `<Nullable>enable`; `.claude/rules/*` must not be edited; net481 / C# 12 limits apply; no
    `init`/`record`/`record struct` may be introduced; nullable post-condition attributes are
    unavailable and must not be added.
  - their goals and frustrations: wants the classifier cluster provably clean under the pragma
    gate; frustrated by the pre-existing debt that made the gate a no-op, and wary of any edit
    that touches a scoring or corpus math path.
  - their context and motivations: this Wave-1 child depends on the Wave-0 Extensions
    contracts (#363); the CI-gate finalization is the separate Wave-2 capstone child.
  - Scenario: Enforcing the gate for the classifier cluster
    - who is acting: the maintainer, reviewing the remediation branch.
    - what triggered the action: PR #361 repaired the gate; the epic sequences EmailIntelligence
      in Wave 1 after the Extensions contracts land.
    - what steps they take: at Phase 0, measure the exact CS86xx-emitting set with
      `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` (without
      `/p:Nullable=enable`) and reconcile it against the ~18 target; run the toolchain per
      batch (csharpier → analyzers/code style → the pragma-driven `/t:Rebuild
      /p:TreatWarningsAsErrors=true` nullable gate → vstest coverage); confirm every
      CS86xx-emitting file carries `#nullable enable` and builds clean; confirm
      `UtilitiesCS.csproj` still has no `<Nullable>` element; confirm existing tests pass and
      changed-line coverage does not regress.
    - what obstacles or decisions occur: the `Flags/` and `Performance/` subfolder scope
      boundaries are confirmed at Phase 0; nullable post-condition attributes must not be
      added; files over 500 lines must not be split; the DO-NOT-ALTER scoring guard list must
      be respected; the global `/p:Nullable=enable` flag must not be used for verification
      because it drowns this child's signal in the full repo-wide debt.
    - what outcome they expect: the classifier cluster passes the per-file pragma gate with
      zero CS86xx and no behavior or scoring change.

- Persona: Downstream Wave-1+ classifier consumer
  - who the user is: a developer or a later epic child whose code calls into the classifier
    engines (for example `MulticlassEngine`, `TristateEngine`, `BayesianClassifierShared`,
    `Corpus`, `PerParentClassifier`) or depends on their annotated member signatures.
  - what they care about: accurate nullability annotations on the members they call, so their
    own null-flow analysis under `#nullable enable` is correct.
  - their constraints: they cannot re-open or re-annotate the classifier files without creating
    cross-child churn; they depend on this child's annotations being correct and on the #363
    contracts being honored consistently.
  - their goals and frustrations: wants trustworthy contracts; frustrated if a member annotated
    non-nullable actually returns null (a false null-state assumption propagated downstream), or
    if a base/override or interface/implementer pair is annotated inconsistently.
  - their context and motivations: their cluster lists the EmailIntelligence classifier child
    and `utilitiescs-nullable-extensions` (#363) as dependencies and consumes their annotated
    contracts.
  - Scenario: Consuming an annotated classifier contract
    - who is acting: the downstream consumer.
    - what triggered the action: their code calls a remediated classifier member under
      `#nullable enable` (for example an `InitAsync` returning `Task<T?>`, a `GetWordInfo`
      returning `WordInfo?`, or a `Corpus.SubtractAsync(..., SegmentStopWatch? sw = null)`).
    - what steps they take: rely on the member's annotated signature to drive their own null
      handling.
    - what obstacles or decisions occur: none, provided the annotations reflect actual null
      behavior. They also rely on the #363 `ThrowIfNull<T> where T : notnull` contract, which
      carries no `[NotNull]` narrowing, so they know a bare `x.ThrowIfNull();` does not narrow
      null-state.
    - what outcome they expect: the annotations behave as documented contracts and require no
      changes to the classifier files.

## Acceptance-Oriented Scenarios (Given / When / Then)

- Scenario 1 — A remediated file compiles to zero CS86xx under the per-file pragma (AC1)
  - Given an in-scope `.cs` file under `Bayesian`, `ClassifierGroups`, or `Flags` that emitted
    CS86xx at Phase 0,
  - When it receives a `#nullable enable` pragma and is remediated with `?`,
    `where T : notnull`, unconstrained `T?`, and justified `!` only where needed,
  - Then `msbuild ... /t:Rebuild /p:TreatWarningsAsErrors=true` (without `/p:Nullable=enable`)
    reports zero CS86xx for that file.

- Scenario 2 — A non-opted file stays null-oblivious and is not cross-blocked (AC1, AC2)
  - Given a file that is excluded or not yet remediated (for example dead `Obsolete/` code, a
    Designer-generated file, an interface-only file with no forced co-annotation, or a
    not-yet-batched file),
  - When the pragma gate runs,
  - Then that file carries no `#nullable enable`, emits no CS86xx (it is null-oblivious), and
    does not block the build; and `UtilitiesCS.csproj` still contains no `<Nullable>` element.

- Scenario 3 — Scoring/corpus math is unchanged and golden/property tests stay green (AC3, AC4)
  - Given the DO-NOT-ALTER scoring guard list (Paul Graham / Robinson probability updates,
    chi-squared combine, hierarchical-shrinkage `ScoreChildren`/`ChildLogScore`, Laplace
    smoothing, `Normalize` softmax, `KnobList` constants, `Corpus` operator arithmetic,
    `GetTristate` thresholds),
  - When remediation annotates around those regions using annotation plus a justified `!`
    rather than a new `if (x is null) throw`,
  - Then no arithmetic, comparison, constant, clamp, ordering, or control flow changes; the
    existing `UtilitiesCS.Test` EmailIntelligence golden/property/characterization suites (and
    the subclass test doubles `SubBayesianClassifier`, `SubClassifierGroup`, `SubCorpus`) pass
    unchanged; and changed-line coverage does not regress relative to the Phase 0 baseline.

- Scenario 4 — Honoring an upstream #363 extension contract (AC5)
  - Given a classifier call site that invokes a #363-annotated extension method as a bare
    statement, for example `Globals.ThrowIfNull();` or `EngineName.ThrowIfNullOrEmpty();`,
  - When the file is remediated under `#nullable enable`,
  - Then the executor honors the published contract (`ThrowIfNull<T> where T : notnull`, no
    `[NotNull]` narrowing) by capturing the return value, adding a justified `!` with a
    `// why` comment, or annotating an invariant-guaranteed member as non-null — and does not
    add a `[NotNull]` polyfill or rewrite the call into a new `if (x is null) throw` guard; the
    remediated member's signature remains behavior-compatible.

## Acceptance Criteria

- [x] AC1: Every in-scope `.cs` file under `UtilitiesCS/EmailIntelligence/Bayesian`,
  `.../ClassifierGroups`, and `.../Flags` that emits CS86xx carries `#nullable enable` and
  compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [x] AC3: No behavior change; no change to any classifier scoring or model path; existing
  tests (including golden/property tests) still pass unchanged.
- [x] AC4: No coverage regression on changed lines.
- [x] AC5: Public signatures of remediated members remain behavior-compatible; nullability
  annotations reflect actual null behavior and honor the upstream #363 extension contracts.

## Non-Goals

Call out what is explicitly excluded from this feature.

- No project-level or solution-level nullable enable. No `<Nullable>` element is added to
  `UtilitiesCS.csproj`; enforcement is per-file pragma only, and non-opted files remain
  null-oblivious and must not be cross-blocked.
- No behavior changes, refactors, or API redesign. This is null-annotation and null-safety
  remediation only. No classifier scoring path, model logic, or corpus/probability math is
  altered. Files over 500 lines (for example `BayesianClassifierShared.cs`,
  `BayesianClassifierGroup.cs`, `CategoryClassifierGroup.cs`, `FlagParser.cs`) are not split,
  and `FolderHierarchyNode` is not converted away from its existing get-only `record` shape.
- No introduction of `init` accessors, positional `record`, or `record struct` (they fail
  CS0518 on net481, which lacks `IsExternalInit`).
- No use of nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) and no addition of a polyfill for them; they are unavailable on net481.
- No editing of `.claude/rules/*`. The rules-versus-convention conflict about the global
  `/p:Nullable=enable` flag is flagged for the maintainer, not resolved here.
- No re-annotation of the Wave-0 Extensions files (#363). This child consumes their published
  contracts and does not modify them.
- Finalizing the CI nullable-gate enforcement mechanism is the separate Wave-2 capstone child,
  not this feature.
