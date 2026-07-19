# `utilitiescs-nullable-outlook-folder-store` — User Story

- Issue: #365
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-18

## Story Statement

- As the repository maintainer who owns the CI nullable gate, I want the pre-existing CS86xx
  nullable debt in `UtilitiesCS/OutlookObjects/Folder/` and `UtilitiesCS/OutlookObjects/Store/`
  remediated under per-file `#nullable enable` and brought to zero diagnostics under
  `TreatWarningsAsErrors`, so that the gate repaired by PR #361 can be genuinely enforced
  against this cluster without permanently blocking future PRs.
- As a developer maintaining the Folder-navigation and Store-management surfaces (folder
  prediction/scoring, folder tree snapshots, breadcrumbs, store disable/rehook flows), I want
  the cluster's nullability annotations to accurately reflect actual null behavior, so that I
  can trust the compiler's flow analysis instead of manually re-deriving which properties and
  return values can be null.

## Problem / Why

What need or gap does this idea address?

The CI nullable gate was silently failing to catch nullable-reference-type debt until PR #361
changed the CI step to `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` so it performs a
genuine recompile. The repaired gate cannot be enforced against new code while the
pre-existing CS86xx diagnostics remain unaddressed. The chosen architecture is a per-file
`#nullable enable` opt-in: each remediated file is brought to zero CS86xx under its own
pragma, so files can be remediated and merged independently without a global force-enable
that would block every PR until all files were fixed at once.

This feature is the Wave-1 child covering `UtilitiesCS/OutlookObjects/Folder/` (including
`MsgToMime/`) and `UtilitiesCS/OutlookObjects/Store/`: 83 `.cs` files, of which 18 are
already opted in (verify-only), 2 are Designer-generated files recommended to stay
non-opted-in, and 63 are this feature's remediation targets. This refined count supersedes
the epic manifest's stale `~29` estimate, which predates the breadcrumb and
folder-tree-percentage additions. The cluster covers Outlook folder navigation, folder-name
prediction/scoring, folder-tree snapshotting, breadcrumb rendering, and Outlook store
identity, disable, and rehook management — code that mixes host-neutral, unit-tested domain
logic with COM-bound Interop adapters and WinForms shells that are coverage-exempt per
CLAUDE.md. The work is null-annotation and null-safety only, with no behavior change; it
depends on the annotated contracts already remediated by the Wave-0 children `#363`
(`utilitiescs-nullable-extensions`) and `#364` (`utilitiescs-nullable-helperclasses`).

## Personas & Scenarios

- Persona: Repository maintainer (CI/quality owner)
  - who the user is: owns the CI nullable gate and the epic's per-file opt-in architecture.
  - what they care about: a gate that catches real null defects in the Folder/Store cluster
    without blocking unrelated PRs; no behavior regressions; no reduction in coverage on
    changed lines; COM-bound files annotated without new tests forced around them.
  - their constraints: the gate must rely on per-file pragmas, not a solution-level
    `<Nullable>enable`; `.claude/rules/*` must not be edited; net481 / C# 12 limits apply
    (no post-condition attributes, no `record`/`init`); Designer-generated files stay
    non-opted-in by convention.
  - their goals and frustrations: wants the Folder/Store cluster provably clean under the
    pragma gate; frustrated by the pre-existing debt that made the gate a no-op, and by the
    epic manifest's stale file-count estimate.
  - their context and motivations: this Wave-1 child is one of five in its wave; the CI-gate
    finalization is the separate Wave-2 capstone child.
- Scenario: Enforcing the gate for the Folder/Store cluster
  - who is acting: the maintainer, reviewing the remediation branch after `#363`/`#364` have
    landed.
  - what triggered the action: PR #361 repaired the gate; the epic sequences this cluster in
    Wave 1, depending on the Wave-0 Extensions/HelperClasses contracts.
  - what steps they take: run the toolchain (csharpier -> analyzers/code style -> the
    pragma-driven `/t:Rebuild /p:TreatWarningsAsErrors=true` nullable gate -> vstest coverage);
    confirm every CS86xx-emitting Folder/Store file carries `#nullable enable` and builds
    clean; confirm `UtilitiesCS.csproj` still has no `<Nullable>` element; confirm the
    partial-class groups (`FolderPredictor`, `StoresWrapper`) were annotated together; confirm
    existing tests pass and changed-line coverage does not regress.
  - what obstacles or decisions occur: nullable post-condition attributes are unavailable on
    net481 and must not be added; `FolderPredictor.cs` (974 lines), `FolderScorer.cs` (663
    lines), and `FolderWrapper .cs` (531 lines, and its filename space) must not be split or
    renamed; the global `/p:Nullable=enable` flag must not be used for verification because it
    drowns this child's signal in the full repo-wide debt.
  - what outcome they expect: the Folder/Store cluster passes the per-file pragma gate with
    zero CS86xx and no behavior change.

- Persona: Developer maintaining the Folder-navigation and Store-management surfaces
  - who the user is: a developer working on folder prediction/scoring, folder-tree snapshots,
    breadcrumbs, or store disable/rehook flows.
  - what they care about: accurate nullability annotations on the properties and methods they
    call day to day, so their own null-flow analysis is correct and COM-bound edge cases (for
    example, `FolderWrapper.Globals` being null-by-design, or `StoreWrapper`'s
    `GetSmtpAddressFromStore()` returning null on `COMException`) are visible in the type
    system instead of only in comments.
  - their constraints: they consume upstream contracts from `#363`/`#364` (for example
    `StringExtensions.IsNullOrEmpty()`, `LazyExtension.ToLazy()`, `FilePathHelper`); they
    cannot re-annotate those upstream files from within this feature; they must respect the
    COM/VSTO/WinForms coverage exemption when touching Interop-bound classes.
  - their goals and frustrations: wants trustworthy contracts across the cluster; frustrated
    if an annotated-non-null property or return value actually goes null at runtime (a false
    null-state assumption).
  - their context and motivations: their day-to-day work touches files across both `Folder/`
    and `Store/`, several of which share partial-class members or COM-boundary call patterns.
  - Scenario: Consuming an annotated contract while fixing a folder-tree bug
    - who is acting: the developer, working in `FolderTreeSnapshotBuilder.cs` or
      `OutlookFolderTreeService.cs`.
    - what triggered the action: a bug report or feature request touching folder-tree state.
    - what steps they take: rely on the annotated signatures (for example
      `FolderTreeSnapshot.TryGetNode(out FolderTreeSnapshotNode? node)`,
      `IOutlookFolderHierarchyReader`'s nullable-shaped return) to drive their own null
      handling, without re-deriving null behavior from source inspection.
    - what obstacles or decisions occur: none, provided the annotations reflect actual null
      behavior; an incorrect annotation would surface as a missed or false-positive CS86xx.
    - what outcome they expect: the annotations behave as documented contracts and require no
      changes to the already-remediated Folder/Store files.

## Acceptance Criteria

- [x] AC1: Every `.cs` file under `UtilitiesCS/OutlookObjects/Folder/` and
  `UtilitiesCS/OutlookObjects/Store/` that emits CS86xx carries `#nullable enable` and
  compiles with zero nullable diagnostics under the per-file pragma with
  `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`; no
  `/p:Nullable=enable` global flag is used in verification.
- [x] AC3: No behavior change; the existing `UtilitiesCS.Test` suite covering this cluster
  still passes.
- [x] AC4: No coverage regression on changed lines; COM-bound coverage-exempt files are
  annotated without new tests, per the CLAUDE.md coverage exemption.
- [x] AC5: Public signatures of the remediated Folder and Store types remain
  behavior-compatible; nullability annotations reflect actual null behavior so they are safe
  contracts for downstream epic consumers.
- [x] AC6: No `System.Diagnostics.CodeAnalysis` nullable post-condition attribute is added,
  and no `record`, `record struct`, or `init` accessor is introduced anywhere in this
  cluster.
- [x] AC7: Each partial-class group (`FolderPredictor.cs` +
  `FolderPredictor.IFolderSearchHandler.cs`; `StoresWrapper.cs` +
  `StoresWrapper.Filtering.cs`) is remediated in the same commit/batch with a consistent
  nullable shape for shared members.

## Non-Goals

Call out what is explicitly excluded from this feature.

- No project-level or solution-level nullable enable. No `<Nullable>` element is added to
  `UtilitiesCS.csproj`; enforcement is per-file pragma only.
- No behavior changes, refactors, or API redesign. This is null-annotation and null-safety
  remediation only. In particular, `FolderPredictor.cs` (974 lines), `FolderScorer.cs` (663
  lines), and `FolderWrapper .cs` (531 lines) are not split, and `FolderWrapper .cs` is not
  renamed.
- No editing of `.claude/rules/*` and no editing of the epic manifest
  (`docs/features/epics/utilitiescs-nullable-remediation/epic.md`). The rules-versus-convention
  conflict about the global `/p:Nullable=enable` flag, and the stale `~29`-file estimate, are
  flagged for the maintainer, not resolved here.
- No use of nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) and no addition of a polyfill for them; they are unavailable on net481.
- No pragma-annotation of `DisabledStoresViewer.Designer.cs` or
  `StoreWrapperViewer.Designer.cs` by default; these Designer-generated files remain
  non-opted-in per repo convention, flagged for the maintainer rather than annotated.
- No re-annotation of upstream `#363`/`#364` files, or of the Wave-0 siblings
  (`utilitiescs-nullable-reusabletypes`, `utilitiescs-nullable-newtonsofthelpers`,
  `utilitiescs-nullable-threading`) that this cluster consumes but does not depend on per the
  epic manifest; those are separate features.
- Finalizing the CI nullable-gate enforcement mechanism is the separate Wave-2 capstone child
  (`utilitiescs-nullable-ci-capstone`), not this feature.
