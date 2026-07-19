# 2026-07-16-folder-hierarchy-live-provider — Plan

- **Issue:** #350
- **Parent:** epic `folder-tree-breadcrumb-redesign` (manifest issue 9101, wave 0, complexity band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16T21-52
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Authoritative Inputs

- Spec (authoritative contract + AC): `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/spec.md`
- User story (AC): `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/user-story.md`
- Issue: `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/issue.md`
- Research: `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/research/2026-07-16T21-40-folder-hierarchy-live-provider-research.md`
- Epic manifest: `docs/features/epics/folder-tree-breadcrumb-redesign/epic.md`

**All work must comply with the repository policies (CLAUDE.md, `.claude/rules/*`, and the C# code-change / unit-test policies). Do not duplicate their content here.**

## Evidence Location Invariant

All evidence artifacts produced by this plan MUST be written under:
`docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/<kind>/`
where `<kind>` is one of `baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`.
Writing evidence to any `artifacts/…` path is a policy violation. The tooling-input coverage path `artifacts/csharp/coverage.xml` is NOT evidence output; it is the feature-review coverage hook's input and is explicitly allowed by `enforce-evidence-locations.ps1`.

## Scope Lock (exhaustive; execution MUST NOT touch files outside this list)

ADD (production):
- `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs`
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`

MODIFY (production):
- `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs` — add `GetAncestorChain` only
- `UtilitiesCS/UtilitiesCS.csproj` — add exactly three `<Compile Include>` items (non-SDK-style project; explicit include list, no glob)

ADD (test):
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbSegmentTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesAncestorChainTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs`

MODIFY (test):
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — add exactly three `<Compile Include>` items (explicit include list, no glob)

MUST NOT TOUCH (wave-0 mergeability boundary):
- `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (`BuildFromRows` not deleted)
- `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` (`Build` not deleted)
- `QuickFiler/Controllers/EfcFormController.cs` (`BindFolderRows` not rewired)
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` (`SetFolderSuggestions` not rewired)
- Any scoring/ranking or feature-324 probability plumbing (`FolderScore.Probability` -> `FolderRow.Score` -> `PercentageFormatter.FormatPercent`)

Note: XML-doc `superseded` annotations on the two legacy methods are OUT OF SCOPE for this plan because they would edit files in the MUST NOT TOUCH list; they are deferred to the consuming UI features. Every production and test file must remain under 500 lines.

## C# Toolchain Order (exact, per repository policy)

1. `dotnet tool run csharpier .` (or `csharpier .`)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

If any step fails or changes files, restart from step 1.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Compliance and Baseline Capture

- [x] [P0-T1] Read the repository policy documents in the mandated policy-compliance order (CLAUDE.md; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/csharp.md`; `.claude/rules/quality-tiers.md`) and record the read in `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists with `Timestamp:`, `Policy Order:`, and an explicit list of every file read (all five paths above), created before any Phase 1 change
- [x] [P0-T2] Capture the baseline formatting state by running `dotnet tool run csharpier . --check` and writing `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (formatted/unformatted file count or clean status)
- [x] [P0-T3] Capture the baseline analyzer build by running the analyzer msbuild command (step 2 of the toolchain) and writing `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build succeeded/failed and analyzer warning count)
- [x] [P0-T4] Capture the baseline nullable/type-check build by running the nullable msbuild command (step 3 of the toolchain) and writing `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/baseline/baseline-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build succeeded/failed and warning-as-error count)
- [x] [P0-T5] Capture the baseline test-and-coverage run for `UtilitiesCS.Test.dll` by running `vstest.console.exe` with `/EnableCodeCoverage` and writing `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric baseline coverage headline (first-party line % and, when available, branch %) plus passed/failed test counts

### Phase 1 — Segment DTO and Provider Contract

- [x] [P1-T1] Create `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs` as an immutable, net48-safe `public sealed class` (plain readonly properties, no `init`/`record`) exposing `FolderTreeNodeKey Key`, `string DisplayName`, `string FolderPath`, `bool HasChildren` via a single constructor, with XML docs, and register it in `UtilitiesCS/UtilitiesCS.csproj` with `<Compile Include="OutlookObjects\Folder\FolderBreadcrumbSegment.cs" />`
  - Acceptance: file exists in the namespace `UtilitiesCS.OutlookObjects.Folder`, is deliberately probability-free (no `Probability`/`Score` member), is under 500 lines, and the csproj compile item is present so it builds into `UtilitiesCS.dll`
- [x] [P1-T2] Create `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs` declaring `public interface IFolderHierarchyProvider` with `GetAncestorChainAsync(FolderTreeNodeKey leafKey, CancellationToken cancellationToken)`, `GetImmediateSubfoldersAsync(FolderTreeNodeKey segmentKey, CancellationToken cancellationToken)`, and `ResolveLeafKeyAsync(string folderPath, CancellationToken cancellationToken)` (return types per spec §Public Contract), with XML docs for inputs/outputs/invariants, and register it in `UtilitiesCS/UtilitiesCS.csproj` with `<Compile Include="OutlookObjects\Folder\IFolderHierarchyProvider.cs" />`
  - Acceptance: interface signatures match spec §2 exactly (`Task<IReadOnlyList<FolderBreadcrumbSegment>>` for the two query members, `Task<FolderTreeNodeKey>` for resolve); type-only (no executable lines); csproj compile item present
- [x] [P1-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbSegmentTests.cs` (MSTest `[TestClass]`, Moq, FluentAssertions) covering construction with all four properties set and `HasChildren` true/false derivation, and register it in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` with `<Compile Include="OutlookObjects\Folder\FolderBreadcrumbSegmentTests.cs" />`
  - Acceptance: tests build and pass; each test follows Arrange-Act-Assert; no test touches a live Outlook process, COM, or a temporary file; csproj compile item present

### Phase 2 — Pure Ancestor-Chain Helper

- [x] [P2-T1] Add `public static IReadOnlyList<FolderTreeSnapshotNode> GetAncestorChain(FolderTreeSnapshot snapshot, FolderTreeNodeKey leafKey)` to `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs` that throws `ArgumentNullException` when `snapshot` is null, resolves `leafKey` via `TryGetNode`, walks `ParentKey` to the store root collecting nodes with a defensive visited-set cycle guard, and reverses to root-first order
  - Acceptance: method returns an empty list (never null) when `leafKey` is null or absent from the snapshot; a detected cyclic `ParentKey` returns the partial chain rather than looping; the file remains under 500 lines and no other method in the file is modified
- [x] [P2-T2] Create `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesAncestorChainTests.cs` (MSTest, FluentAssertions) building `FolderTreeSnapshot` fixtures via its public constructor and asserting the documented invariants, and register it in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` with `<Compile Include="OutlookObjects\Folder\FolderTreeSnapshotQueriesAncestorChainTests.cs" />`
  - Acceptance: tests cover single-level, multi-level, and root-only (leaf == root, single-element chain) chains; assert root-first/leaf-last ordering, last element equals the requested leaf, and every adjacent `(parent, child)` satisfies `child.ParentKey.Equals(parent.Key)`; csproj compile item present; no live Outlook/COM/temp-file usage
- [x] [P2-T3] Add negative-flow and edge-case test methods to `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesAncestorChainTests.cs` for null snapshot (`ArgumentNullException`), null leaf key (empty list), unknown/stale leaf key (empty list), the defensive cycle guard (partial chain, no hang), and duplicate segment display names at different depths distinguished by `FolderTreeNodeKey`
  - Acceptance: all five scenarios pass; the cycle-guard test constructs a snapshot with a malformed cyclic `ParentKey` and asserts termination with the expected partial chain; identity assertions compare `FolderTreeNodeKey`, not `DisplayName`

### Phase 3 — Provider Facade Implementation

- [x] [P3-T1] Create `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` implementing `IFolderHierarchyProvider` with a constructor taking `IOutlookFolderTreeService` (guarded against null), acquiring the snapshot via `GetSnapshotAsync(FolderTreeRequest.AllStores(allowStaleSnapshot: true), cancellationToken)`, and register it in `UtilitiesCS/UtilitiesCS.csproj` with `<Compile Include="OutlookObjects\Folder\OutlookFolderHierarchyProvider.cs" />`
  - Acceptance: class is host-neutral (depends only on the interface `IOutlookFolderTreeService`), adds no COM code and no `[ExcludeFromCodeCoverage]` attribute, is under 500 lines, and the csproj compile item is present so it builds into `UtilitiesCS.dll`
- [x] [P3-T2] Implement `GetAncestorChainAsync` (calls `FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey)` then maps each `FolderTreeSnapshotNode` to a `FolderBreadcrumbSegment` with `HasChildren = node.ChildKeys.Count > 0`), `GetImmediateSubfoldersAsync` (calls `snapshot.GetChildren(segmentKey)` then maps), and `ResolveLeafKeyAsync` (case-insensitive `FolderPath` match across snapshot nodes returning the node `Key`, or `null` when absent; first-match on duplicate paths) in `OutlookFolderHierarchyProvider.cs`
  - Acceptance: the two query members return `IReadOnlyList<FolderBreadcrumbSegment>` (empty, never null, for unknown keys); `ResolveLeafKeyAsync` returns `null` for an unresolved path; `cancellationToken` is passed to `GetSnapshotAsync`
- [x] [P3-T3] Create `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` (MSTest, Moq, FluentAssertions) with a `Mock<IOutlookFolderTreeService>` returning a prebuilt `FolderTreeSnapshot`, covering the ancestor-chain happy path, immediate subfolders (populated set), path resolution found and not-found, and register it in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` with `<Compile Include="OutlookObjects\Folder\OutlookFolderHierarchyProviderTests.cs" />`
  - Acceptance: tests build and pass; the mocked service is verified to receive `FolderTreeRequest.AllStores(allowStaleSnapshot: true)`; no test touches a live Outlook process, COM, or a temporary file; csproj compile item present
- [x] [P3-T4] Add negative-flow, edge-case, and cancellation test methods to `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` for empty subfolder set, unknown segment key (empty list), duplicate-path first-match resolution, null `IOutlookFolderTreeService` constructor argument (`ArgumentNullException`), and cancellation propagation (`OperationCanceledException` when the mocked snapshot acquisition observes a canceled token)
  - Acceptance: all listed scenarios pass; cancellation is asserted deterministically via a pre-canceled `CancellationToken` (no `Task.Delay`, `Thread.Sleep`, or wall-clock wait); results are `IReadOnlyList`, never null

### Phase 4 — Final QA Loop, Coverage, and Acceptance Criteria

- [x] [P4-T1] Run `dotnet tool run csharpier .` and write `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if formatting changed any file, restart the toolchain from this task
- [x] [P4-T2] Run the analyzer msbuild command (toolchain step 2) and write `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; build succeeded with zero new analyzer warnings on touched code; on failure, fix and restart from P4-T1
- [x] [P4-T3] Run the nullable/TreatWarningsAsErrors msbuild command (toolchain step 3) and write `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/final-nullable.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; build succeeded with zero nullable warnings-as-errors on touched code; on failure, fix and restart from P4-T1
- [x] [P4-T4] Run `vstest.console.exe UtilitiesCS.Test.dll /EnableCodeCoverage` and write `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric post-change coverage headline (first-party line % and branch %) and passed/failed counts; all new tests pass; on failure, fix and restart from P4-T1
- [x] [P4-T5] Compute the coverage delta and new-code coverage and write `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/coverage-delta.md`, reporting baseline coverage (from P0-T5), post-change coverage (from P4-T4), and new/changed-code coverage for `FolderBreadcrumbSegment.cs`, `OutlookFolderHierarchyProvider.cs`, and the `GetAncestorChain` method in `FolderTreeSnapshotQueries.cs`
  - Acceptance: artifact records the three numeric figures; new/changed first-party code coverage is `>= 90%`; repository coverage floor is not reduced versus baseline; `IFolderHierarchyProvider.cs` is noted as type-only (no executable lines) and legitimately excluded from the executable denominator
- [x] [P4-T6] Verify the wave-0 mergeability boundary by confirming that `FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build` still exist and that `EfcFormController.cs` and `ItemViewer.FolderSearch.cs` are unchanged in this branch, and record the check in `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/scope-boundary-check.md`
  - Acceptance: artifact confirms neither legacy method was deleted, neither UI caller was rewired, and the feature-324 probability plumbing files are untouched (git diff of the branch shows only Scope-Lock files)
- [x] [P4-T7] Check off the `## Acceptance Criteria` items in `spec.md` and `user-story.md` against the produced evidence and record the mapping in `docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/evidence/qa-gates/acceptance-criteria-checkoff.md`
  - Acceptance: every AC in both documents maps to a concrete evidence artifact (task ID + artifact path); the `FolderBreadcrumbSegment`-is-probability-free AC and the scoring/probability-unchanged AC are verified against P4-T6; the toolchain-green AC is verified against P4-T1 through P4-T4

## Test Plan

- Unit (MSTest + Moq + FluentAssertions), mirrored under `UtilitiesCS.Test/OutlookObjects/Folder/`:
  - `FolderBreadcrumbSegmentTests.cs` — DTO construction and `HasChildren` derivation.
  - `FolderTreeSnapshotQueriesAncestorChainTests.cs` — single-level, multi-level, root-only, null snapshot, null/unknown key, cycle guard, duplicate names by key.
  - `OutlookFolderHierarchyProviderTests.cs` — ancestor chain, immediate subfolders (populated/empty/unknown), path resolution (found/not-found/duplicate first-match), null-service constructor, cancellation propagation.
- Isolation: no test touches a live Outlook process, COM interop, or a temporary file; all tests deterministic (no wall-clock, no `Task.Delay`/`Thread.Sleep`).
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-tests-coverage.md`
  - Post-change: `evidence/qa-gates/final-tests-coverage.md`
  - Delta / new-code: `evidence/qa-gates/coverage-delta.md`

## Open Questions / Notes

- Scope split confirmed by research §7: 9101 ADDS the provider and defers deletion/rewiring of the legacy methods and their UI callers to 9102/9103. This plan does not include `superseded` XML-doc annotations because they would edit MUST-NOT-TOUCH files.
- Coverage-gate note: the feature-review hook reads `artifacts/csharp/coverage.xml` as JaCoCo; vstest emits Cobertura. If the epic/feature-review coverage hook is invoked, a Cobertura-to-JaCoCo conversion scoped to first-party production assemblies is required. That conversion is a review-tooling step, not an evidence-output path, and is out of this plan's implementation scope.
