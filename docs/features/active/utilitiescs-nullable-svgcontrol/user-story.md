# `utilitiescs-nullable-svgcontrol` — User Story

- Issue: #368
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T22-10

## Story Statement

- As the repository maintainer who owns the CI nullable gate, I want the pre-existing CS86xx
  nullable debt in `SVGControl/` remediated under per-file `#nullable enable` and brought to
  zero diagnostics under `TreatWarningsAsErrors`, so that the gate repaired by PR #361 can be
  genuinely enforced against this project without permanently blocking future PRs.
- As the maintainer sequencing the epic's Wave-0 children, I want `SVGControl/` to be
  remediated as an independently mergeable unit with no `ProjectReference` to `UtilitiesCS`, so
  that this child's merge does not wait on, or block, any other cluster in the epic.

## Problem / Why

What need or gap does this idea address?

The CI nullable gate was silently failing to catch nullable-reference-type debt until PR #361
changed the CI step to `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` so it performs a
genuine recompile. The repaired gate cannot be enforced against new code while pre-existing
CS86xx diagnostics remain across the solution. The chosen architecture is a per-file
`#nullable enable` opt-in: each remediated file is brought to zero CS86xx under its own
pragma, so files can be remediated and merged independently without a global force-enable that
would block every PR until all files were fixed at once.

`SVGControl/` is included in the epic's scope even though it is a separate, independent
`net481` WinForms control project with no `ProjectReference` to `UtilitiesCS`, because the
current solution-level nullable gate covers `SVGControl.csproj` too — the repaired gate cannot
be genuinely enforced solution-wide until every project covered by it, including this one, is
opted in on the same per-file basis. This feature is the Wave-0 child covering `SVGControl/`
(20 `.cs` files: 12 hand-authored files to remediate, 3 already-opted-in verify-only files, 5
Designer/generated files not opted in). Because this project has no cross-module consumer
within the epic, its annotations are file-local and not consumed as cross-module contracts by
any other epic child. The work is null-annotation and null-safety only, with no behavior
change.

## Personas & Scenarios

- Persona: Repository maintainer (CI/quality owner)
  - who the user is: owns the CI nullable gate and the epic's per-file opt-in architecture.
  - what they care about: a gate that catches real null defects in `SVGControl/` without
    blocking unrelated PRs; no behavior regressions; no reduction in coverage on changed lines;
    a genuinely solution-wide enforceable gate once every covered project, including
    `SVGControl.csproj`, is opted in.
  - their constraints: the gate must rely on per-file pragmas, not a solution-level or
    project-level `<Nullable>enable`; `.claude/rules/*` must not be edited; net481 / `LangVersion
    latest` limits apply, and `SVGControl.csproj` has no path to inherit a nullable-attribute
    polyfill from `UtilitiesCS` because it carries no `ProjectReference` to it.
  - their goals and frustrations: wants `SVGControl/` provably clean under the pragma gate so
    the solution-wide gate has no remaining unaddressed project; frustrated by the pre-existing
    debt that made the gate a no-op.
  - their context and motivations: this Wave-0 child is one of twelve in the epic and has no
    `depends_on` edges and no dependents, so it can be scheduled and merged on its own timeline
    relative to the other clusters; the CI-gate finalization is the separate Wave-2 capstone
    child.
  - Scenario: Enforcing the gate for the `SVGControl/` cluster
    - who is acting: the maintainer, reviewing the remediation branch.
    - what triggered the action: PR #361 repaired the gate; the epic sequences `SVGControl/` in
      Wave 0 because the solution-level gate already covers it.
    - what steps they take: run the toolchain (csharpier -> analyzers/code style -> the
      pragma-driven `/t:Rebuild /p:TreatWarningsAsErrors=true` nullable gate -> vstest coverage);
      confirm every CS86xx-emitting hand-authored `SVGControl/` file carries `#nullable enable`
      and builds clean; confirm `SVGControl.csproj` still has no `<Nullable>` element and none is
      introduced at the solution level; confirm existing tests pass and changed-line coverage
      does not regress; confirm the five Designer/generated files remain consistent with the
      pragma build.
    - what obstacles or decisions occur: nullable post-condition attributes are unavailable on
      net481 for this project (no `ProjectReference` to `UtilitiesCS`, hence no indirect
      polyfill) and must not be added; the `SvgImageSelector.ImagePath` dead-setter nuance
      requires an explicit, documented judgment call rather than a mechanical fix; the global
      `/p:Nullable=enable` flag must not be used for verification because it would surface the
      full pre-existing debt across every not-yet-remediated file instead of isolating this
      child's own signal.
    - what outcome they expect: the `SVGControl/` cluster passes the per-file pragma gate with
      zero CS86xx, no behavior change, and no project- or solution-level `<Nullable>` element.

- Persona: Wave-0 epic-feature developer implementing this child
  - who the user is: a developer executing the atomic plan for this feature.
  - what they care about: remediating all 12 hand-authored files to zero CS86xx without
    introducing a behavior change, given that automated test coverage for these files is
    effectively absent.
  - their constraints: cannot add nullable post-condition attributes or a polyfill; cannot
    convert the legacy non-SDK `.csproj` to SDK-style; cannot introduce `record`/`init`/`record
    struct` on net481; cannot split `RelativePath.cs` (pre-existing 1678-line file, verify-only,
    out of scope); must resolve the `SvgImageSelector.ImagePath` judgment call
    behavior-preservingly and document the decision rather than silently picking a fallback that
    changes observable output.
  - their goals and frustrations: wants a clean, leaf-first batch order (research batches A-E)
    so no file is re-touched after being brought to zero CS86xx; frustrated by the absence of an
    automated safety net for 12 of 12 remediation-target files, which raises the bar for manual
    behavior-preservation care.
  - their context and motivations: this is one Wave-0 cluster among six; independence from
    `UtilitiesCS` means this child's own review scope is self-contained.
  - Scenario: Resolving the `SvgImageSelector.ImagePath` judgment call
    - who is acting: the developer executing the Batch C task for `SvgImageSelector.cs`.
    - what triggered the action: the `ImagePath` property's `get` accessor has a real CS8603
      under the pragma because its `set` accessor is currently a commented-out no-op.
    - what steps they take: choose the null-forgiving `_relativeImagePath!` resolution with an
      in-code comment noting the setter is currently a no-op, rather than a `?? "(none)"`
      fallback that would change the returned value on this path.
    - what obstacles or decisions occur: a `?? "(none)"` fallback would be a subtle, real
      behavior change not covered by any automated test, so it is rejected in favor of the
      behavior-preserving null-forgiving resolution.
    - what outcome they expect: the property compiles clean under the pragma with its exact
      current runtime behavior preserved, and the decision is recorded rather than left implicit.

## Acceptance Criteria

- [ ] AC1: Every hand-authored `.cs` file in `SVGControl/` that emits CS86xx carries
  `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [ ] AC2: No project-level `<Nullable>` element is introduced into `SVGControl.csproj`, and no
  `<Nullable>` element is introduced at the solution level.
- [ ] AC3: No behavior change; existing tests still pass.
- [ ] AC4: No coverage regression on changed lines.
- [ ] AC5: Public signatures of the remediated control, parser, and converter types remain
  behavior-compatible; nullability annotations reflect actual null behavior.
- [ ] AC6: WinForms `*.Designer.cs` and generated `Properties/Resources.Designer.cs` files
  remain consistent with the pragma build; any edit to them is mechanical and
  behavior-preserving.

## Non-Goals

Call out what is explicitly excluded from this feature.

- No project-level or solution-level nullable enable. No `<Nullable>` element is added to
  `SVGControl.csproj` or at the solution level; enforcement is per-file pragma only.
- No behavior changes, refactors, or API redesign. This is null-annotation and null-safety
  remediation only. In particular, `SvgOptionsConverter1` (in `SvgOptionsConverter.cs`) and
  `SVGParser.cs` are not renamed or deleted despite being unreferenced/dead code;
  `ISvgResource.cs`'s `SvgResource` class is not converted to a `record`; `RelativePath.cs`
  (1678 lines, pre-existing over-limit, verify-only) is not split.
- No editing of `.claude/rules/*`. The rules-versus-convention conflict about the global
  `/p:Nullable=enable` flag is flagged for the maintainer, not resolved here.
- No use of nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) and no addition of a polyfill for them; they are unavailable on this
  project's net481 target, which has no `ProjectReference` to `UtilitiesCS`.
- No conversion of the non-SDK legacy `SVGControl.csproj` to SDK-style project format.
- No new automated tests are required by this feature; it is annotation-only. If the
  implementing plan chooses to add a characterization test to protect the
  `SvgImageSelector.ImagePath` judgment call, it must use MSTest + Moq + FluentAssertions and
  must not create or use temp files.
- Finalizing the CI nullable-gate enforcement mechanism is the separate Wave-2 capstone child
  (`utilitiescs-nullable-ci-capstone`), not this feature.
