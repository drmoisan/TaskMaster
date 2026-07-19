# `utilitiescs-nullable-extensions` — User Story

- Issue: #363
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T21-20

## Story Statement

- As the repository maintainer who owns the CI nullable gate, I want the pre-existing CS86xx
  nullable debt in `UtilitiesCS/Extensions/` remediated under per-file `#nullable enable` and
  brought to zero diagnostics under `TreatWarningsAsErrors`, so that the gate repaired by PR #361
  can be genuinely enforced against these files without permanently blocking future PRs.
- As a downstream epic-feature developer working on a Wave-1 cluster (OutlookObjects,
  EmailIntelligence, or Dialogs), I want the shared Extensions methods annotated to reflect their
  actual null behavior, so that I can consume their nullability annotations as reliable
  cross-module contracts instead of guessing null-state or re-touching the Extensions files.

## Problem / Why

What need or gap does this idea address?

The CI nullable gate was silently failing to catch nullable-reference-type debt until PR #361
changed the CI step to `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` so it performs a
genuine recompile. The repaired gate cannot be enforced against new code while roughly 2131
pre-existing CS86xx diagnostics remain across the repository. The chosen architecture is a
per-file `#nullable enable` opt-in: each remediated file is brought to zero CS86xx under its own
pragma, so files can be remediated and merged independently without a global force-enable that
would block every PR until all files were fixed at once.

This feature is the Wave-0 child covering `UtilitiesCS/Extensions/` (25 `.cs` files; 2 already
opted in; 23 to remediate). The Extensions methods are shared and consumed across module
boundaries, so their annotations are remediated early: five Wave-1 children depend on this
cluster's annotated contracts. The work is null-annotation and null-safety only, with no behavior
change.

## Personas & Scenarios

- Persona: Repository maintainer (CI/quality owner)
  - who the user is: owns the CI nullable gate and the epic's per-file opt-in architecture.
  - what they care about: a gate that catches real null defects without blocking unrelated PRs;
    no behavior regressions; no reduction in coverage on changed lines.
  - their constraints: the gate must rely on per-file pragmas, not a solution-level
    `<Nullable>enable`; `.claude/rules/*` must not be edited; net481 / C# 12 limits apply.
  - their goals and frustrations: wants the Extensions cluster provably clean under the pragma
    gate; frustrated by the pre-existing debt that made the gate a no-op.
  - their context and motivations: this Wave-0 child is one of twelve in the epic; the CI-gate
    finalization is the separate Wave-2 capstone child.
- Scenario: Enforcing the gate for the Extensions cluster
  - who is acting: the maintainer, reviewing the remediation branch.
  - what triggered the action: PR #361 repaired the gate; the epic sequences Extensions in Wave 0.
  - what steps they take: run the toolchain (csharpier → analyzers/code style → the pragma-driven
    `/t:Rebuild /p:TreatWarningsAsErrors=true` nullable gate → vstest coverage); confirm every
    CS86xx-emitting Extensions file carries `#nullable enable` and builds clean; confirm
    `UtilitiesCS.csproj` still has no `<Nullable>` element; confirm existing tests pass and
    changed-line coverage does not regress.
  - what obstacles or decisions occur: nullable post-condition attributes are unavailable on
    net481 and must not be added; `ArrayExtensions.cs` (544 lines) must not be split; the global
    `/p:Nullable=enable` flag must not be used for verification because it drowns this child's
    signal in the full repo-wide debt.
  - what outcome they expect: the Extensions cluster passes the per-file pragma gate with zero
    CS86xx and no behavior change.

- Persona: Downstream epic-feature developer (Wave-1 consumer)
  - who the user is: a developer remediating a Wave-1 cluster that calls Extensions methods such
    as `CastNullSafe`, `ToStringArray`, `SliceColumn`, `To2D`, `Find`, `TryFindMax`, or
    `UpdateOrRemove`.
  - what they care about: accurate nullability annotations on the shared methods they call, so
    their own null-flow analysis is correct.
  - their constraints: they cannot re-open or re-annotate the Extensions files without creating
    cross-child churn; they depend on this child's annotations being correct.
  - their goals and frustrations: wants trustworthy contracts; frustrated if an Extensions method
    annotated non-nullable actually returns null (a false null-state assumption propagated
    downstream).
  - their context and motivations: their Wave-1 child lists `utilitiescs-nullable-extensions` as a
    dependency and consumes its annotated contracts.
  - Scenario: Consuming an annotated contract
    - who is acting: the Wave-1 developer.
    - what triggered the action: their cluster calls a shared Extensions method under
      `#nullable enable`.
    - what steps they take: rely on the method's annotated signature (for example an
      `out TValue?` result or a `?`-returning method) to drive their own null handling.
    - what obstacles or decisions occur: none, provided the Extensions annotations reflect actual
      null behavior; an incorrect annotation would surface as a false-positive or missed CS86xx in
      their own code.
    - what outcome they expect: the annotations behave as documented contracts and require no
      changes to the Extensions files.

## Acceptance Criteria

- [ ] AC1: Every `.cs` file under `UtilitiesCS/Extensions/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`.
- [ ] AC3: No behavior change; existing tests still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of the remediated extension methods remain behavior-compatible;
  nullability annotations reflect actual null behavior so they are safe contracts for downstream
  epic consumers.

## Non-Goals

Call out what is explicitly excluded from this feature.

- No project-level or solution-level nullable enable. No `<Nullable>` element is added to
  `UtilitiesCS.csproj`; enforcement is per-file pragma only.
- No behavior changes, refactors, or API redesign. This is null-annotation and null-safety
  remediation only. In particular, `ArrayExtensions.cs` (544 lines) is not split, and
  `DfDeedle.EmailRecord` is not converted to a record.
- No editing of `.claude/rules/*`. The rules-versus-convention conflict about the global
  `/p:Nullable=enable` flag is flagged for the maintainer, not resolved here.
- No use of nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) and no addition of a polyfill for them; they are unavailable on net481.
- Finalizing the CI nullable-gate enforcement mechanism is the separate Wave-2 capstone child
  (`utilitiescs-nullable-ci-capstone`), not this feature.
