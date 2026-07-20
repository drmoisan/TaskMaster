# `utilitiescs-nullable-helperclasses` — User Story

- Issue: #364
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18T21-45
- Epic: utilitiescs-nullable-remediation (child, Wave 0)

## Story Statement

- As the repository maintainer, I want the `UtilitiesCS/HelperClasses/` nullable-reference-type
  debt remediated under a per-file `#nullable enable` opt-in, so that the repaired CI nullable
  gate can be enforced on these files without cross-blocking non-opted-in files elsewhere in the
  epic.
- As a downstream Wave-1 feature developer (outlook-folder-store, outlook-mailitem-item,
  dialogs-misc), I want the shared HelperClasses public members annotated to reflect their actual
  null behavior, so that I consume accurate nullability contracts and do not inherit incorrect
  null assumptions when I remediate my own cluster.

## Problem / Why

The CI nullable gate (repaired by PR #361 to use `msbuild /t:Rebuild ... /p:Nullable=enable
/p:TreatWarningsAsErrors=true`) now performs a genuine recompile and surfaces pre-existing
CS86xx nullable-reference-type diagnostics that were previously masked. The
`UtilitiesCS/HelperClasses/` directory tree (43 `.cs` files, including the FileSystem,
ThemeHelpers, Logging, ToolTips, Windows Forms, CloningFunctions, BinaryFlags, and root helper
classes) carries such pre-existing nullable debt. These are shared helpers consumed across module
boundaries; their nullability annotations become contracts that downstream epic features
(OutlookObjects, EmailIntelligence, Dialogs clusters) consume.

A global force-enable of nullable would make no epic child independently mergeable until all ~234
files (~2131 diagnostics) were fixed at once. The per-file opt-in lets this child be remediated
and merged on its own while non-opted-in files stay oblivious and non-cross-blocking.

## Personas & Scenarios

- Persona: Repository maintainer (drmoisan)
  - Who: owner of the nullable-remediation epic and the CI nullable gate.
  - Cares about: a genuinely enforceable nullable gate that does not permanently block future PRs;
    a per-file opt-in architecture that keeps each epic child independently mergeable.
  - Constraints: annotation and null-safety only — no behavior changes, no refactors, no API
    redesign; no project- or solution-level `<Nullable>` element; no editing of `.claude/rules/*`.
  - Goals and frustrations: wants the HelperClasses debt cleared under the confirmed architecture,
    and wants scope conflicts (Designer file, `PrettyPrint.cs` line limit, rules-vs-convention)
    surfaced as flags rather than silently resolved.
  - Context: HelperClasses is a Wave-0 foundational cluster; its annotations gate the quality of
    the Wave-1 contracts that depend on it.

- Persona: Downstream Wave-1 feature developer
  - Who: an agent or developer remediating outlook-folder-store, outlook-mailitem-item, or
    dialogs-misc, which depend on HelperClasses.
  - Cares about: consuming HelperClasses public members with nullability annotations that match
    the actual runtime behavior, so their own null-flow analysis is correct.
  - Constraints: must not have to re-derive or work around inaccurate HelperClasses contracts.
  - Goals and frustrations: an incorrect annotation on a shared helper (for example, a member
    marked nullable that in fact throws, or vice versa) would propagate an incorrect assumption
    into every dependent cluster.

- Scenario: Remediating and verifying a HelperClasses batch
  - Who is acting: the executor delivering issue #364, batch by batch.
  - Trigger: the repaired nullable gate now surfaces pre-existing CS86xx in HelperClasses.
  - Steps: opt each batch's files in with `#nullable enable`; apply annotation/null-safety edits
    (nullable `?`, guards, justified `!`, null-flow corrections); build with the pragma-only
    command (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without `/p:Nullable=enable`) to capture
    a per-batch baseline and drive the opted-in files to zero CS86xx; run the batch's MSTest tests
    and require them green and behavior-identical.
  - Obstacles/decisions: at FileSystem adapter root boundaries, use behavior-preserving `!` and
    flag the latent root-throws behavior; leave `DvgForm.Designer.cs` non-opted-in and flag the
    epic-scope conflict; flag the `PrettyPrint.cs` 500-line pre-existing violation without
    splitting it; do not add `/p:Nullable=enable` to the verification command.
  - Expected outcome: every in-scope HelperClasses file that emitted CS86xx is opted-in and clean
    under the pragma-only gate, with no behavior change and no coverage regression on changed
    lines, and all flagged conflicts documented for the maintainer.

## Acceptance Criteria

- [x] Every `.cs` file under `UtilitiesCS/HelperClasses/` that emits CS86xx carries a
  `#nullable enable` pragma and compiles with zero nullable (CS86xx) diagnostics under the
  per-file pragma with `/p:TreatWarningsAsErrors=true`.
- [x] No project-level or solution-level `<Nullable>` element is introduced; `UtilitiesCS.csproj`
  retains none.
- [x] Changes are annotation/null-safety only: no behavior change, no API/signature semantics
  change, no refactor beyond nullable annotation.
- [x] All existing MSTest tests for UtilitiesCS still pass; no coverage regression on changed
  lines.
- [x] The full C# toolchain (csharpier -> analyzer/codestyle build -> nullable/
  TreatWarningsAsErrors build -> vstest with coverage) passes on the final pass, using the
  pragma-only type-check command (`/t:Rebuild /p:TreatWarningsAsErrors=true`, without
  `/p:Nullable=enable`) for this child.
- [x] `DvgForm.Designer.cs` handling and the epic-scope conflict are documented; the Designer
  file is not hand-edited.
- [x] The `PrettyPrint.cs` 500-line pre-existing violation is flagged (not fixed) in the feature
  docs.

## Non-Goals

- No behavior changes, refactors, API redesign, or feature work of any kind. Nullable annotation
  and null-safety only.
- No project-level or solution-level `<Nullable>` element as an enforcement mechanism.
- No editing of `.claude/rules/*` to resolve the rules-vs-convention conflict (it is flagged at
  the epic level, capstone child).
- No splitting of `PrettyPrint.cs` to meet the 500-line limit (pre-existing condition, flagged
  not fixed).
- No changes to files outside `UtilitiesCS/HelperClasses/`, including the out-of-scope FileSystem
  interfaces under `UtilitiesCS/Interfaces/IHelperClasses/`.
- No fix of the latent FileSystem adapter root-boundary throw behavior (flagged for a possible
  future issue).
