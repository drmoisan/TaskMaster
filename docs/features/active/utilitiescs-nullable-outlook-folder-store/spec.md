# utilitiescs-nullable-outlook-folder-store — Spec

- **Issue:** #365
- **Parent (optional):** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18
- **Status:** Draft
- **Version:** 0.1

## Overview

What need or gap does this idea address?

The CI nullable gate, repaired by PR #361 to use `msbuild /t:Rebuild` so it performs a
genuine recompile rather than a silently-skipped incremental build, cannot be enforced
against new code until the pre-existing nullable-reference-type debt (CS86xx diagnostics)
is remediated under a per-file `#nullable enable` opt-in architecture. This feature is the
Wave-1 child that remediates the `UtilitiesCS/OutlookObjects/Folder/` (including
`MsgToMime/`) and `UtilitiesCS/OutlookObjects/Store/` directory trees only.

Scope is the Folder+Store cluster: recursively, 83 `.cs` files, of which 18 already carry
`#nullable enable` and are verify-only, 2 are Designer-generated code-behind files
recommended to remain non-opted-in (`DisabledStoresViewer.Designer.cs`,
`StoreWrapperViewer.Designer.cs`), leaving 63 files as opt-in targets. This refined count
(from `docs/features/active/utilitiescs-nullable-outlook-folder-store/research/2026-07-18T22-30-outlook-folder-store-nullable-research.md`)
supersedes the epic manifest's stale `~29` estimate, which predates the breadcrumb
(#327/#349/#350/#351) and folder-tree-percentage (#324/#325) work. This work is
null-annotation and null-safety remediation only; it introduces no behavior changes, no
refactors, no API redesign, no new features, and no new runtime guard code paths where an
annotation plus a justified `!` suffices.

## Behavior

What should the feature do at a high level?

Each remediated file receives a per-file `#nullable enable` pragma and is brought to zero
CS86xx diagnostics under that pragma with `TreatWarningsAsErrors`. Remediation applies
nullable annotations (`?`), null-flow corrections, and null-forgiving operators (`!`) only
where justified. Existing null guards already present in the files remain as-is. The two
already-enabled Designer-adjacent code-behind files stay non-opted-in by convention
(consistent with the `#364` `DvgForm.Designer.cs` precedent); if the maintainer later
requires full opt-in, the documented fallback is annotating only the generated
`IContainer? components` field without touching `InitializeComponent`.

The work is annotation and null-safety only. There are no behavior changes, no refactors, no
API redesign, and no feature work. Public method signatures remain behavior-compatible: an
existing caller that compiles today continues to compile and behaves identically. The
annotation choices reflect the true null behavior of each method so that the resulting
signatures are safe contracts for downstream consumers, including the epic's other Wave-1
and Wave-2 children.

COM/VSTO/WinForms coverage-exempt classes (Outlook Interop wrapper/event-handler classes
without an injectable seam, and WinForms `Form`-derived shells) are annotated for
null-safety like any other file, but no new tests are added around them and no new runtime
guard statements are introduced solely to satisfy an annotation, since that would create new
uncovered executable lines inconsistent with the coverage exemption in CLAUDE.md.

## Inputs / Outputs

- Inputs (CLI flags, files, env vars): none. This is a library-internal source change with no
  runtime inputs.
- Outputs (artifacts, logs, telemetry): none added. No logging or telemetry is introduced.
- Config keys and defaults: none.
- Versioning or backward-compatibility constraints: public method signatures of the remediated
  Folder and Store types remain behavior-compatible. The observable change is limited to
  nullability annotations, which are additive contract metadata rather than a source- or
  binary-breaking behavior change.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.

There is no CLI surface and no new API. This is a library-internal change. The relevant
"API surface" is the set of nullability annotations applied to the public and internal
members of `UtilitiesCS/OutlookObjects/Folder/` and `UtilitiesCS/OutlookObjects/Store/`.

- Example invocations with expected outputs (concise): not applicable; no command or CLI flag
  is added. No `/p:Nullable=enable` global flag is introduced into any verification command
  (see Toolchain note).
- Contracts and validation rules:
  - Public and internal signatures remain behavior-compatible; only nullability annotations
    change (for example, `FolderNavigator.GetOutlookFolder` becomes `Folder?`-returning;
    `StoreDisableService`'s optional `IStoreRehookService rehook = null` parameter becomes
    `IStoreRehookService? rehook = null`).
  - Partial-class groups are annotated together in the same commit/batch so shared members
    present a single, consistent nullable shape across both files:
    `FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs`, and
    `StoresWrapper.cs` + `StoresWrapper.Filtering.cs`. `IFolderSearchHandler.cs`'s nullable
    parameter shape must be decided in lockstep with `FolderPredictor.cs`, since the
    interface defines the contract the partial class implements.
  - Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`,
    `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) are not
    available or polyfilled on this target (net481) and must not be used or added (see
    Constraints & Risks).

## Data & State

Data flow, storage, or state changes introduced by this feature.

- Data transformations and invariants: none changed. This is annotation-only; no runtime data
  flow, transform, or invariant is altered.
- Caching or persistence details: none. `Lazy<T>`/`AsyncLazy<T>`-backed fields in
  `FolderMinimalWrapper.cs` and `FolderWrapper .cs` are annotated to reflect their existing
  assign-on-`ResetLazy()` pattern; the pattern itself is not changed.
- Migration or backfill requirements (if any): none. No project-level `<Nullable>` element is
  introduced into `UtilitiesCS.csproj`; the project has no `<Nullable>` element today and must
  keep none. Enforcement is per-file pragma only.

## Constraints & Risks

List notable constraints (performance, compatibility, scope) or risks.

- Target framework net481, C# 12 (`LangVersion` 12.0). All nullable syntax is available: `?`,
  `!`, `is null` / `is not null` flow analysis.
- Nullable post-condition attributes from `System.Diagnostics.CodeAnalysis` (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`) are NOT available on this target and are NOT polyfilled
  in the repository. They must not be used or added. No `record`, `record struct`, or `init`
  accessor conversions are introduced anywhere in this cluster: `init`/`record`/`record struct`
  fail CS0518 on net481, which lacks `IsExternalInit`. `FolderRow.cs`, `FolderScore.cs`, and
  `StoreIdentity.cs` are already plain `readonly struct`; `StoreRehookResult.cs` is already a
  hand-written `sealed record` with constructor-set get-only properties (not positional/`init`
  syntax) and is net481-safe as-is. `FolderScorer.FolderScoring`'s reference-type fields are set
  by every current object-initializer call site; if any construction path is found to leave a
  reference field unset, it becomes `= default!` or is typed non-nullable and initialized
  explicitly, not converted to `record`/`init`.
- Pre-existing >500-line files (do NOT split — annotation-only scope, consistent with the
  precedent set by `#363`'s `ArrayExtensions.cs` (544 lines) and `#364`'s `PrettyPrint.cs` (677
  lines)): `FolderPredictor.cs` (974 lines), `FolderScorer.cs` (663 lines), and
  `FolderWrapper .cs` (531 lines, and see the filename hazard below). Flag all three as
  pre-existing policy exceptions; do not fix here.
- Near-limit file: `OutlookFolderNotificationSink.cs` (498 lines). Annotation edits could push
  it over 500 lines; if so, flag rather than split, consistent with `#364`'s
  `FilePathHelper.cs` (494 lines) precedent.
- Filename hazard: `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` contains a literal
  trailing space before `.cs`. Do not rename it (a rename is a workspace change beyond
  annotation-only scope). Quote the path in any tooling invocation that references it
  explicitly (`csharpier` invoked recursively as `csharpier .` and msbuild's `Compile Include`
  glob already handle it correctly without quoting); flag the space as a pre-existing naming
  defect for a future issue.
- COM/VSTO/WinForms coverage exemption (CLAUDE.md): Outlook Interop wrapper/event-handler
  classes without an injectable seam, and WinForms `Form`-derived shells
  (`DisabledStoresViewer.cs`, `StoreWrapperViewer.cs`, and their `.Designer.cs` files), are
  coverage-exempt. They are annotated for null-safety but no new tests are required around
  them, and no new runtime guard statements are added that would create new uncovered
  executable lines. Several files in this cluster mix exempt and non-exempt members in the
  same class (`OutlookFolderHierarchyReader.cs`, `OutlookFolderNotificationSink.cs`); the
  non-exempt members (already covered by existing tests, per the research inventory) are held
  to the same no-regression standard as any other testable-domain file.
- Global-flag-vs-per-file-pragma conflict: `.claude/rules/csharp.md` documents forcing
  `/p:Nullable=enable` globally for the type-check stage, which conflicts with this feature's
  (and the whole epic's) per-file opt-in convention. This is the same conflict `#363` and `#364`
  flag at the epic level. Out of scope to resolve here; do not edit `.claude/rules/*`. This
  conflict defers to the Wave-2 CI capstone child (`utilitiescs-nullable-ci-capstone`,
  epic placeholder 9012).
- Designer-file opt-in conflict: the epic manifest lists `Store/DisabledStoresViewer.Designer.cs`
  and `Store/StoreWrapperViewer.Designer.cs` as in-scope files, but the repo convention (and
  `#364`'s `DvgForm.Designer.cs` precedent) is to leave Designer files non-opted-in by default,
  since `#nullable` is lexical/per-file and generated code produces no CS8618/CS8625 either way.
  This feature follows that convention and flags the epic-scope-vs-convention conflict for the
  maintainer rather than pragma-annotating generated code.
- Upstream contract consumption (#363 `utilitiescs-nullable-extensions` and #364
  `utilitiescs-nullable-helperclasses`): this cluster calls `string.IsNullOrEmpty()` (from
  `StringExtensions.cs`, #363 Batch B) in `FolderConverter.cs`/`FolderPredictor.cs`;
  `.ToLazy()`/`.ToLazyValue()` (from `LazyExtension.cs`, #363 Batch B) in
  `FolderMinimalWrapper.cs`/`FolderWrapper .cs`; `.ForEach(...)`/`.SentenceJoin()` (from
  `IEnumerableExtensions.cs`, #363 Batch C) in `FolderTree.cs`/`FolderScorer.cs`/
  `FolderConverter.cs`/`FolderPredictor.cs`; `AsTokenPattern()` (from `Tokenizer.cs`, #364) and
  `VerboseLogger<T>` (from `VerboseLogger.cs`, #364) in `FolderScorer.cs`; and `FilePathHelper`
  (from `FilePathHelper.cs`, #364's highest-contract-sensitivity file) in `StoreWrapper.cs`'s
  `ArchiveFsRoot` property and `StoreWrapperController.GetRelativeFsPath()`. As of this
  research pass, neither #363 nor #364 has landed its pragma in this worktree; both are
  `Status: Draft`. This feature's execution should not start until #363 and #364 land, per the
  epic's Wave-0-before-Wave-1 sequencing, so that annotation decisions at these call sites are
  made against real (not oblivious) upstream signatures.
- Additional cross-cluster dependencies not covered by #363/#364 (informational, non-blocking
  per the per-file architecture; flagged for the maintainer, not added to the epic manifest's
  `depends_on`, which this feature does not edit): `TreeNode<T>`, `ScoDictionaryNew<TKey,TValue>`,
  `SmartSerializable<T>`, and `AsyncLazy<T>` (all `UtilitiesCS/ReusableTypeClasses/...`, epic
  placeholder 9003); `ProgressTracker` and `CurrentStoreContext` (`UtilitiesCS/Threading/`, epic
  placeholder 9005); `FilePathHelperConverter` (`UtilitiesCS/NewtonsoftHelpers/`, epic
  placeholder 9004). None of these are currently `#nullable enable` in this worktree. Because
  enforcement is per-file pragma (not project-level), these oblivious upstream types do not
  cross-block this cluster's own opted-in files, but annotation choices at these call sites
  cannot be verified against a real upstream contract until those Wave-0 siblings land; re-run
  the pragma gate after they do to confirm no new CS86xx appears at those call sites.
- Cross-directory ordering within this cluster: `OutlookFolderHierarchyReader.cs` (Folder,
  Batch F5) takes a `StoresWrapper storesWrapper` constructor parameter and calls
  `store.ShouldInclude(_storesWrapper)` / `storesWrapper.ShouldIncludeStore(_store)`, so Batch S2
  (`StoresWrapper.cs` + `StoresWrapper.Filtering.cs`) must land before Batch F5.

## Implementation Strategy

- Implementation scope (what changes, not sequencing): add a `#nullable enable` pragma to each
  of the 63 opt-in-target files and bring each to zero CS86xx under the pragma; verify the 18
  already-enabled files still compile clean with no edits expected; leave the 2
  Designer-generated files non-opted-in per the documented convention. No project or solution
  file changes.
- New classes/functions/commands to add or update: none. No new types, methods, commands, or
  files are added; only nullability annotations on existing members change.
- Batch grouping (from research; leaf-first, interfaces/enums/DTOs before host-neutral domain
  types before COM-adjacent/WinForms-shell types):
  - Batch F0 — Folder interfaces, enums, and trivial DTOs (unblocks F1-F3): `IDeadlineClock.cs`,
    `IDispatcherYield.cs`, `IFolderHandleResolver.cs`, `IFolderHierarchyProvider.cs`,
    `IFolderSearchHandler.cs`, `IOutlookFolderHierarchyReader.cs`,
    `IOutlookFolderNotificationSink.cs`, `IOutlookFolderTreeService.cs`,
    `FolderTreeRefreshReason.cs`, `FolderRow.cs`, `FolderScore.cs`, `FolderBreadcrumbSegment.cs`,
    `FolderTreeSnapshotChangedEventArgs.cs`, `OutlookFolderHierarchyRecord.cs`. Ordering
    constraint: `IFolderSearchHandler.cs`'s nullable shape is decided in lockstep with
    `FolderPredictor.cs` (Batch F3).
  - Batch F1 — Folder value/key types and comparers: `FolderTreeNodeKey.cs`,
    `FolderTreeRequest.cs`, `FolderTreeSelectionOverlay.cs`, `FolderNodeViewModel.cs`,
    `DeadlineClock.cs`, `FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`,
    `FolderWrapperNameAndParentNameComparer.cs`, `FolderWrapperNodeComparer.cs` (depends on the
    prior two comparers), `FolderWrapperNodeContentsComparer.cs`.
  - Batch F2 — Folder tree snapshot family: `FolderTreeSnapshotNode.cs`, `FolderTreeSnapshot.cs`,
    `FolderTreeSnapshotQueries.cs`, `FolderTreeSnapshotBuilder.cs`,
    `FolderTreeCompatibilityView.cs`, `FolderTreeStateModel.cs`, `FolderHierarchyBuilder.cs`
    (the last three touch the cross-cluster `TreeNode<T>`; annotate them last within F2).
  - Batch F3 — Folder domain/COM-adjacent testable classes (moderate-high pressure, largest
    batch): `FolderConverter.cs`, `FolderNavigator.cs`, `FolderMinimalWrapper.cs`,
    `FolderWrapper .cs`, `FolderTree.cs`, `FolderScorer.cs`, `FolderPredictor.cs` +
    `FolderPredictor.IFolderSearchHandler.cs` (single commit). Ordering constraint:
    `FolderScorer.cs` precedes `FolderPredictor.cs` (the latter holds a `FolderScorer
    Suggestions` field). Already-enabled verify-only files in this area
    (`FolderProbabilityAdapter.cs`, `FolderSuggestionTree.cs`, `FolderSuggestionNode.cs`,
    `IFolderProbabilitySource.cs`, `FolderBreadcrumbBridgeRouter.cs`, the breadcrumb files, and
    `PercentageFormatter.cs`) are rebuilt after F0-F2 land to confirm no regression.
  - Batch F4 — Folder host-neutral facade/service layer: `OutlookFolderHierarchyProvider.cs`,
    `OutlookFolderTreeService.cs`.
  - Batch F5 — Folder COM-boundary adapters (last, thin/mostly-exempt):
    `OutlookFolderHandleResolver.cs`, `OutlookFolderHierarchyReader.cs`,
    `OutlookFolderNotificationSink.cs`, `MsgToMime/MAPIMethods.cs`, `WpfDispatcherYield.cs`.
  - Batch S0 — Store interfaces and trivial DTOs: `IDisabledStoresViewer.cs`,
    `IStoreWrapperViewer.cs`, `DisabledStoreRow.cs`.
  - Batch S1 — Store value types and pure attribution helpers: `StoreIdentity.cs`,
    `StoreLaunchReadinessEvaluator.cs`, `StoreFilterAttribution.cs`, `StoreLockupAttribution.cs`,
    `StoreWrapperInitClock.cs`, `StoreWrapperInitProbe.cs`. `StoreRehookResult.cs` is
    already enabled (verify-only; re-verify after S1/S2).
  - Batch S2 — Store domain classes (largest Store batch): `StoreWrapper.cs`,
    `StoresWrapper.cs` + `StoresWrapper.Filtering.cs` (single commit), `StoreDisableService.cs`.
  - Batch S3 — Store controllers: `StoreWrapperController.cs`, `DisabledStoresController.cs`.
  - Batch S4 — Store WinForms shells (last, thin/mostly-exempt): `StoreWrapperViewer.cs`,
    `DisabledStoresViewer.cs`. The two `.Designer.cs` siblings remain non-opted-in.
  - Recommended overall sequence: F0 -> F1 -> F2 -> F3 -> F4 -> S0 -> S1 -> S2 -> F5 -> S3 -> S4
    (F5 is deferred until after S2 specifically because `OutlookFolderHierarchyReader.cs`
    consumes `StoresWrapper`; F4 does not depend on Store and may run interleaved with S0/S1).
  - The full task-by-task sequencing belongs to the atomic plan, not this spec.
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

Acceptance criteria (from `issue.md`, mapped here for traceability):

- [x] AC1: Every `.cs` file under `UtilitiesCS/OutlookObjects/Folder/` and
  `UtilitiesCS/OutlookObjects/Store/` that emits CS86xx carries `#nullable enable` and compiles
  with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`.
- [x] AC2: No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`; no
  `/p:Nullable=enable` global flag is used in verification.
- [x] AC3: No behavior change; the existing `UtilitiesCS.Test` suite covering this cluster still
  passes.
- [x] AC4: No coverage regression on changed lines; COM-bound coverage-exempt files are
  annotated without new tests, per the CLAUDE.md coverage exemption.
- [x] AC5: Public signatures of the remediated Folder and Store types remain
  behavior-compatible; nullability annotations reflect actual null behavior so they are safe
  contracts for downstream epic consumers.
- [x] AC6: No `System.Diagnostics.CodeAnalysis` nullable post-condition attribute
  (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`,
  `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`) is added, and no `record`,
  `record struct`, or `init` accessor is introduced anywhere in this cluster.
- [x] AC7: Each partial-class group (`FolderPredictor.cs` +
  `FolderPredictor.IFolderSearchHandler.cs`; `StoresWrapper.cs` +
  `StoresWrapper.Filtering.cs`) is remediated in the same commit/batch with a consistent
  nullable shape for shared members.

## Seeded Test Conditions (from potential)
- [ ] Existing `UtilitiesCS.Test/OutlookObjects/Folder/` and
  `UtilitiesCS.Test/OutlookObjects/Store/` suites (approximately 80 test files, including the
  `Fakes/` helpers `FakeDeadlineClock.cs`, `FakeDispatcherYield.cs`, `FakeFolderHandleResolver.cs`,
  `FakeFolderHierarchyRecord.cs`, `FakeOutlookFolderHierarchyReader.cs`,
  `FakeOutlookFolderNotificationSink.cs`) continue to pass with no behavior change.
- [ ] Changed-line coverage does not regress relative to baseline (prefer annotation and
  justified `!` over new runtime guards to avoid introducing uncovered executable lines).
- [ ] The pragma-driven nullable gate (`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug
  /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`) produces zero CS86xx diagnostics for the
  remediated files, without passing `/p:Nullable=enable` globally.
- [ ] The pre-existing `#pragma warning disable CS8625` / `restore` pair inside
  `StoreWrapperController.StoreLaunchReadiness.NotReady` is re-evaluated once the file opts in
  to confirm it is still needed (or is redundant) after the file-level pragma is added; do not
  remove it without confirming a rebuild stays clean.

## Toolchain Note

Run the repo C# toolchain in CLAUDE.md order:

1. `csharpier .` (adding a pragma line and `?` annotations reformats; run before each build).
   Note the space-in-filename hazard for `FolderWrapper .cs`: the recursive `csharpier .`
   invocation picks it up correctly; only an explicit-path invocation would need quoting.
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
   /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers / code style).
3. Nullable verification via the per-file pragma gate:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
   /p:TreatWarningsAsErrors=true`. Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled
   file becomes an error while non-opted files stay silent. Use `/t:Rebuild` (not `/t:Build`),
   per PR #361's fix, to avoid a silently-skipped incremental build.
4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`.

Do NOT pass `/p:Nullable=enable` globally for this feature's verification. The global flag forces
nullable project-wide and surfaces the full pre-existing debt across the solution, drowning this
child's signal. That global-flag-versus-per-file-pragma mismatch is the rules-versus-convention
conflict the epic flags for the maintainer and defers to the Wave-2 CI capstone child
(`utilitiescs-nullable-ci-capstone`); resolving it is out of scope here.
