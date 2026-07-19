# utilitiescs-nullable-outlook-folder-store — Plan

- **Issue:** #365
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-18T22-03
- **Status:** Draft
- **Version:** 0.1

## Required References

- CLAUDE.md (standing instructions, C# toolchain section).
- `.claude/rules/general-code-change.md` (cross-language code change policy).
- `.claude/rules/general-unit-test.md` (cross-language unit test policy).
- `.claude/rules/csharp.md` (C#-specific toolchain and standards).
- Requirements sources: `docs/features/active/utilitiescs-nullable-outlook-folder-store/issue.md`,
  `docs/features/active/utilitiescs-nullable-outlook-folder-store/spec.md`,
  `docs/features/active/utilitiescs-nullable-outlook-folder-store/user-story.md`.
- Research: `docs/features/active/utilitiescs-nullable-outlook-folder-store/research/2026-07-18T22-30-outlook-folder-store-nullable-research.md`.

**All work must comply with these policies; do not duplicate their content here.** All policy
files above MUST be read from the current worktree root
(`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a2627015d44378748`), not any
planning-worktree absolute path.

## Epic Dependency Note (Wave 1 / Wave 0 Fan-In)

This feature is a Wave-1 child that `depends_on` the Wave-0 children `#363`
(`utilitiescs-nullable-extensions`) and `#364` (`utilitiescs-nullable-helperclasses`). At
**execution** time (later, run by the epic-orchestrator), the upstream annotated contracts
(`StringExtensions.cs`, `LazyExtension.cs`, `IEnumerableExtensions.cs`, `Tokenizer.cs`,
`VerboseLogger.cs`, `FilePathHelper.cs`) are expected to already be present on the integration
branch this feature executes against. As of this plan's research pass, neither `#363` nor `#364`
has landed its pragma in this worktree (both remain `Status: Draft`); this plan's Phase 0 baseline
CS86xx counts may therefore partly reflect not-yet-merged upstream annotations rather than the
final upstream contract shape. This preparation/preflight planning pass is textual only: it does
not assert that upstream fan-in has occurred. Execution of Phases 1-11 below must occur only after
the Wave-0 upstream siblings land, per the epic's Wave-0-before-Wave-1 sequencing; Phase 0's
baseline capture and Phase 12's final QC re-verify the pragma gate at execution time regardless.

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY. Do NOT add a `<Nullable>` element to
  `UtilitiesCS/UtilitiesCS.csproj` (AC2).
- Verification uses the per-file pragma gate:
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
  Do NOT pass `/p:Nullable=enable` globally; the global flag surfaces the whole-repo pre-existing
  debt and drowns this child's signal. Enforcement is per-file pragma only. Use `/t:Rebuild` (not
  `/t:Build`) per PR #361's fix, to avoid a silently-skipped incremental build.
- Target is net481 / C# 12. Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are NOT available/polyfilled and MUST NOT be used or added. Use plain `?`,
  guard clauses, `is null`/`is not null` flow analysis, and justified `!` (with a `// why` comment)
  instead (AC6).
- No `record`, `record struct`, or `init` accessor conversions anywhere in this cluster (`init` /
  positional records fail CS0518 on net481, which lacks `IsExternalInit`) (AC6). `FolderRow.cs`,
  `FolderScore.cs`, and `StoreIdentity.cs` are already plain `readonly struct`; `StoreRehookResult.cs`
  is already a hand-written `sealed record` with constructor-set get-only properties and is
  net481-safe as-is — do not convert either pattern.
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign (AC3, AC5).
- Partial-class groups MUST be remediated together in the same phase/task (AC7):
  `FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs` (Phase 4); `StoresWrapper.cs` +
  `StoresWrapper.Filtering.cs` (Phase 8).
- Pre-existing >500-line files are annotation-only; do NOT split: `FolderPredictor.cs` (974 lines),
  `FolderScorer.cs` (663 lines), `FolderWrapper .cs` (531 lines).
- Near-limit file `OutlookFolderNotificationSink.cs` (498 lines): if annotation edits push it over
  500 lines, flag it as a pre-existing exception rather than splitting it.
- Filename hazard: `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` contains a literal
  trailing space before `.cs`. Do not rename it. Quote the full path
  (`"UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs"`) in any tooling invocation that
  references it explicitly (an explicit-path `csharpier` call, `git add`/`git status` pathspecs).
  The recursive `csharpier .` invocation and msbuild's `Compile Include` glob already handle it
  correctly without quoting.
- COM/VSTO/WinForms coverage-exempt classes (Outlook Interop wrapper/event-handler classes without
  an injectable seam, and WinForms `Form`-derived shells) are annotated for null-safety like any
  other file, but no new tests are added around them and no new runtime guard statements are
  introduced solely to satisfy an annotation (AC4).
- The two Designer-generated files (`Store/DisabledStoresViewer.Designer.cs`,
  `Store/StoreWrapperViewer.Designer.cs`) remain non-opted-in per repository convention; no batch
  task pragma-annotates them.
- Cross-directory hard ordering: Batch S2 (`StoresWrapper.cs` + `StoresWrapper.Filtering.cs`,
  Phase 8) MUST land before Batch F5 (`OutlookFolderHierarchyReader.cs`, Phase 9), because
  `OutlookFolderHierarchyReader.cs`'s constructor takes a `StoresWrapper storesWrapper` parameter
  and calls `store.ShouldInclude(_storesWrapper)` / `storesWrapper.ShouldIncludeStore(_store)`.
- No re-annotation of upstream `#363`/`#364` files or of the Wave-0 siblings that own `TreeNode<T>`,
  `ScoDictionaryNew<TKey,TValue>`, `SmartSerializable<T>`, `AsyncLazy<T>` (epic placeholder 9003),
  `ProgressTracker`/`CurrentStoreContext` (epic placeholder 9005), or `FilePathHelperConverter`
  (epic placeholder 9004). Where this cluster's files consume those types, annotate the consuming
  call site only and treat the external type as oblivious; re-verify after those siblings land.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [x] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) from the current worktree root and record the read receipt at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [x] [P0-T2] Enumerate the 83 `.cs` files under `UtilitiesCS/OutlookObjects/Folder/` (incl. `MsgToMime/`) and `UtilitiesCS/OutlookObjects/Store/`, recording path, line count, and whether each file already carries `#nullable enable`, at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact lists all 83 files; confirms exactly 18 are already `#nullable enable` (verify-only), 2 are Designer-generated and recommended non-opted-in, and 63 are remediation targets; contains `Timestamp:`.
- [x] [P0-T3] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [x] [P0-T4] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T5] Capture the baseline per-file nullable pragma-gate build by running `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the CURRENT CS86xx count for the cluster: zero expected from the 18 already-enabled files, and an explicit note that the 63 opt-in-target files are still null-oblivious at this baseline and therefore emit no pragma-driven CS86xx yet under this gate.
- [x] [P0-T6] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent); Cobertura XML written to the named evidence path.
- [x] [P0-T7] Confirm the AC2 baseline: verify `UtilitiesCS/UtilitiesCS.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in the csproj (AC2 baseline).
- [x] [P0-T8] Record the upstream Wave-0 dependency status by grepping `#nullable enable` in `UtilitiesCS/Extensions/StringExtensions.cs`, `UtilitiesCS/Extensions/LazyExtension.cs`, `UtilitiesCS/Extensions/IEnumerableExtensions.cs`, `UtilitiesCS/HelperClasses/Tokenizer.cs`, `UtilitiesCS/HelperClasses/Logging/VerboseLogger.cs`, and `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`, and record the finding at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/baseline/baseline-upstream-dependency-note.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, whether each of the 6 upstream files currently carries `#nullable enable` in this worktree, and a statement that this plan's baseline CS86xx figures may not reflect the final upstream contract shape until `#363`/`#364` land (see Epic Dependency Note above).

### Phase 1 — Batch F0 Folder Interfaces, Enums, and Trivial DTOs
- [x] [P1-T1] Add a `#nullable enable` pragma to each of the 14 Batch F0 files: `UtilitiesCS/OutlookObjects/Folder/IDeadlineClock.cs`, `UtilitiesCS/OutlookObjects/Folder/IDispatcherYield.cs`, `UtilitiesCS/OutlookObjects/Folder/IFolderHandleResolver.cs`, `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs`, `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs`, `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderHierarchyReader.cs`, `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderNotificationSink.cs`, `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderTreeService.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeRefreshReason.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotChangedEventArgs.cs`, `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyRecord.cs`
  - Acceptance: each of the 14 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P1-T2] Apply nullable annotations, guards, and justified `!` to the 14 Batch F0 files so each reaches zero CS86xx under the pragma; decide `IFolderSearchHandler.cs`'s nullable parameter shape (`FolderArray`, `Suggestions`, `FolderRowArray`, `FindFolder(...)`) now, since `FolderPredictor.cs` (Phase 4) must implement this exact shape; annotate `IFolderHierarchyProvider.ResolveLeafKeyAsync` as `Task<FolderTreeNodeKey?>` and `IFolderHandleResolver.TryResolve`'s `out object folder` as `out object?`
  - Acceptance: no `System.Diagnostics.CodeAnalysis` post-condition attribute is added; annotations reflect actual null behavior (AC5); changes are annotation/null-safety only (AC3).
- [x] [P1-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f0-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 14 Batch F0 files (AC1).
- [x] [P1-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f0-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f0-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 2 — Batch F1 Folder Value/Key Types and Comparers
- [x] [P2-T1] Add a `#nullable enable` pragma to each of the 10 Batch F1 files: `UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeRequest.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeSelectionOverlay.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderNodeViewModel.cs`, `UtilitiesCS/OutlookObjects/Folder/DeadlineClock.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs`
  - Acceptance: each of the 10 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P2-T2] Apply nullable annotations, guards, and justified `!` to the 10 Batch F1 files so each reaches zero CS86xx under the pragma; annotate `FolderTreeNodeKey.Equals(object obj)` as `Equals(object? obj)` and `Equals(FolderTreeNodeKey other)` as `Equals(FolderTreeNodeKey? other)`; annotate `FolderTreeRequest`'s ctor `IEnumerable<string> storeIds` as `IEnumerable<string>? storeIds`; `FolderWrapperNodeComparer.cs`/`FolderWrapperNodeContentsComparer.cs` internally compose `FolderWrapperNameComparer.cs`/`FolderWrapperNameCountSizeComparer.cs`, so annotate the composed comparers' public surface first within this task
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); changes are annotation/null-safety only (AC3).
- [x] [P2-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f1-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 10 Batch F1 files (AC1).
- [x] [P2-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f1-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f1-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 3 — Batch F2 Folder Tree Snapshot Family
- [x] [P3-T1] Add a `#nullable enable` pragma to each of the 7 Batch F2 files: `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotNode.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeCompatibilityView.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderTreeStateModel.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs`
  - Acceptance: each of the 7 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P3-T2] Apply nullable annotations, guards, and justified `!` to the 7 Batch F2 files so each reaches zero CS86xx under the pragma; annotate `FolderTreeSnapshotNode.parentKey` as `FolderTreeNodeKey?` (root node has no parent); annotate `FolderTreeSnapshot.TryGetNode(out FolderTreeSnapshotNode? node)` and its `FindByPath` nullable return; annotate `FolderTreeCompatibilityView.CreateNode` as returning `TreeNode<FolderWrapper>?`; annotate `FolderTreeStateModel._highlighted` as `TreeNode<FolderNodeViewModel>?`; annotate `FolderHierarchyBuilder.cs`'s `currentNode`/`cumulative` locals as nullable; `TreeNode<T>` itself is an external oblivious type (epic placeholder 9003) and is not edited by this task
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); no `TreeNode<T>` source file is edited; changes are annotation/null-safety only (AC3).
- [x] [P3-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f2-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 7 Batch F2 files (AC1).
- [x] [P3-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f2-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f2-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 4 — Batch F3 Folder Domain and COM-Adjacent Testable Classes
- [x] [P4-T1] Add a `#nullable enable` pragma to each of the following 8 Batch F3 files: `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`, `"UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs"` (quote this path; see the filename hazard in Scope Invariants), `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs`; add the pragma to `FolderPredictor.cs` and `FolderPredictor.IFolderSearchHandler.cs` in this same task since they are one partial type (AC7)
  - Acceptance: all 8 files contain a `#nullable enable` pragma; `FolderWrapper .cs` is not renamed; no `<Nullable>` element added to the csproj.
- [x] [P4-T2] Apply nullable annotations, guards, and justified `!` to `FolderConverter.cs`, `FolderNavigator.cs`, `FolderMinimalWrapper.cs`, and `"FolderWrapper .cs"` so each reaches zero CS86xx under the pragma; annotate `FolderConverter.cs`'s two `ToFsFolderpath` overloads as returning `string?`; annotate `FolderNavigator.GetOutlookFolder` as returning `Folder?`; annotate `FolderMinimalWrapper.cs`'s `Lazy<string> _lazyName`/`_lazyRelativePath` fields (assigned only in `ResetLazy()`) and its `ToRelativePath()`/logger-path methods as nullable-returning; annotate `"FolderWrapper .cs"`'s `Lazy<T>` fields (assigned only in `ResetLazy()`, consuming `#363`'s `LazyExtension.ToLazy()`/`.ToLazyValue()`) and its `AsyncLazy<IItemInfo[]> ItemHelpers` property (consuming the oblivious epic-placeholder-9003 `AsyncLazy<T>`) to reflect the existing assign-on-`ResetLazy()` pattern without changing it; keep `IApplicationGlobals Globals { get; set; }` nullable-by-design, guarded by existing `is null` checks
  - Acceptance: no post-condition attribute is added; `FolderWrapper .cs` remains 531 lines or is flagged as a pre-existing exception if edits push it further, but is NOT split; annotation/null-safety only (AC3); annotations reflect actual null behavior (AC5).
- [x] [P4-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f3a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `FolderConverter.cs`, `FolderNavigator.cs`, `FolderMinimalWrapper.cs`, and `"FolderWrapper .cs"` (AC1).
- [x] [P4-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3a-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `FolderConverterTests.cs`, `FolderConverter_Tests.cs`, `FolderNavigatorTests.cs`, `FolderMinimalWrapperTests.cs`, `FolderWrapperCoverageExpansionTests.cs`, `FolderWrapperStateTests.cs`, `FolderWrapperTraversalTests.cs` (AC3).
- [x] [P4-T5] Apply nullable annotations, guards, and justified `!` to `FolderTree.cs` so it reaches zero CS86xx under the pragma; annotate `public event PropertyChangedEventHandler PropertyChanged` as `PropertyChangedEventHandler?`; annotate the tuple-returning `Compare`/`CompareMembers` methods' nullable-shaped members; confirm `_roots` is assigned on every constructor path; `FolderTree.cs` consumes the external oblivious `ProgressTracker` (epic placeholder 9005) at the consuming call site only, without editing `ProgressTracker`'s own source
  - Acceptance: no post-condition attribute is added; no `ProgressTracker` source file is edited; annotation/null-safety only (AC3); annotations reflect actual null behavior (AC5).
- [x] [P4-T6] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f3b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `FolderTree.cs` (AC1).
- [x] [P4-T7] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3b-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `FolderTreeTests.cs` (AC3).
- [x] [P4-T8] Apply nullable annotations, guards, and justified `!` to `FolderScorer.cs` so it reaches zero CS86xx under the pragma, without splitting the file (663 lines); annotate `object folderObject`/`object foldersObject` cast-with-`as` locals as nullable; annotate `MailItem olMail` parameters consistent with existing guard placement; verify every construction site of the internal `struct FolderScoring` sets every reference-type field explicitly (use `= default!` only if a gap is found); `FolderScorer.cs` consumes the external oblivious `ScoDictionaryNew<string,long>` (epic placeholder 9003) and `#364`'s `Tokenizer.AsTokenPattern()`/`VerboseLogger<T>` (not yet landed in this worktree per the Epic Dependency Note) at the consuming call sites only
  - Acceptance: no post-condition attribute is added; `FolderScorer.cs` is not split; no `ScoDictionaryNew<>`/`Tokenizer`/`VerboseLogger<T>` source file is edited; annotation/null-safety only (AC3); annotations reflect actual null behavior (AC5).
- [x] [P4-T9] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f3c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `FolderScorer.cs` (AC1); this task runs before P4-T11 because `FolderScorer.cs` must precede `FolderPredictor.cs` (which holds a `FolderScorer Suggestions` field).
- [x] [P4-T10] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3c-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `FolderScorerTests.cs`, `FolderScorerCoverageExpansionTests.cs`, `FolderScorerRegressionTests.cs` (AC3).
- [x] [P4-T11] Apply nullable annotations, guards, and justified `!` to `FolderPredictor.cs` and `FolderPredictor.IFolderSearchHandler.cs` together in this task, without splitting `FolderPredictor.cs` (974 lines), so shared members carry a single consistent nullable shape (AC7); annotate ctor-path fields (`_globals`, `_folderList`, `Suggestions`) and the `GetFolder` (×3 overloads)/`CreateFolder`/`CreateFolderAsync` nullable-returning methods; annotate optional parameters (`List<string> emailSearchRoots = null`, `IEnumerable<(...)> exclusions = null`, `string defaultValue = null`) as nullable; match `IFolderSearchHandler.cs`'s interface shape decided in P1-T2 exactly in `FolderPredictor`'s implementation
  - Acceptance: no post-condition attribute is added; `FolderPredictor.cs` is not split; both partial-class files are edited in this single task (AC7); annotation/null-safety only (AC3); annotations reflect actual null behavior (AC5).
- [x] [P4-T12] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f3d-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for both `FolderPredictor.cs` and `FolderPredictor.IFolderSearchHandler.cs` (AC1, AC7).
- [x] [P4-T13] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3d-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f3d-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `FolderPredictorTests.cs`, `FolderPredictorCoverageExpansionTests.cs`, `FolderPredictorSeam_Tests.cs`, `FolderPredictorSeam_DefaultOn_Tests.cs` (AC3).
- [x] [P4-T14] Verify the 17 already-enabled Folder verify-only files (`BreadcrumbBridgeMessages.cs`, `BreadcrumbDocumentAssets.cs`, `BreadcrumbHtmlRenderer.cs`, `BreadcrumbMessageCodec.cs`, `BreadcrumbMessages.cs`, `BreadcrumbRenderProjection.cs`, `BreadcrumbRow.cs`, `BreadcrumbRowBuilder.cs`, `BreadcrumbSegment.cs`, `BreadcrumbSelectionMap.cs`, `BreadcrumbStateModel.cs`, `FolderBreadcrumbBridgeRouter.cs`, `FolderProbabilityAdapter.cs`, `FolderSuggestionTree.cs`, `FolderSuggestionNode.cs`, `IFolderProbabilitySource.cs`, `PercentageFormatter.cs`) still emit zero CS86xx under the pragma gate now that F0-F3 have landed, make NO edits unless a diagnostic appears, and record the outcome at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f3-verify-only-recheck.md`
  - Acceptance: pragma-gate rebuild reports zero CS86xx for all 17 files; artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all 17 files remain unmodified (or, if a diagnostic appeared, the minimal annotation fix is recorded).

### Phase 5 — Batch F4 Folder Host-Neutral Facade/Service Layer
- [x] [P5-T1] Add a `#nullable enable` pragma to each of the 2 Batch F4 files: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`, `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P5-T2] Apply nullable annotations, guards, and justified `!` to the 2 Batch F4 files so each reaches zero CS86xx under the pragma; annotate `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync`'s `match?.Key` nullable-shaped return; annotate `OutlookFolderTreeService.cs`'s `_snapshot`, `_inFlightSnapshot`, `_scheduledRefresh`, `_pendingRefreshRequest` fields (which start `null`) as nullable-typed
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P5-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f4-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch F4 files (AC1).
- [x] [P5-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f4-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f4-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `OutlookFolderHierarchyProviderTests.cs`, `OutlookFolderTreeServiceConcurrencyTests.cs`, `OutlookFolderTreeServiceDisposalTests.cs`, `OutlookFolderTreeServiceInvalidationTests.cs`, `OutlookFolderTreeServiceScopeTests.cs`, `OutlookFolderTreeServiceStateTests.cs` (AC3).

### Phase 6 — Batch S0 Store Interfaces and Trivial DTOs
- [x] [P6-T1] Add a `#nullable enable` pragma to each of the 3 Batch S0 files: `UtilitiesCS/OutlookObjects/Store/IDisabledStoresViewer.cs`, `UtilitiesCS/OutlookObjects/Store/IStoreWrapperViewer.cs`, `UtilitiesCS/OutlookObjects/Store/DisabledStoreRow.cs`
  - Acceptance: each of the 3 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P6-T2] Apply nullable annotations, guards, and justified `!` to the 3 Batch S0 files so each reaches zero CS86xx under the pragma; keep `IDisabledStoresViewer.BindRows(IList<DisabledStoreRow> rows)` non-nullable by contract; keep `IStoreWrapperViewer`'s WinForms control-typed properties non-null by post-construction contract; annotate `DisabledStoreRow.cs`'s mutable DTO properties (`StoreIdentity Identity`, `string DisplayName`, `string ScopeLabel`, `bool IsFutureSession`) with explicit non-null discipline
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P6-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s0-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 3 Batch S0 files (AC1).
- [x] [P6-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s0-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s0-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 7 — Batch S1 Store Value Types and Pure Attribution Helpers
- [x] [P7-T1] Add a `#nullable enable` pragma to each of the 6 Batch S1 files: `UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs`, `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs`, `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs`, `UtilitiesCS/OutlookObjects/Store/StoreLockupAttribution.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperInitClock.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperInitProbe.cs`
  - Acceptance: each of the 6 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P7-T2] Apply nullable annotations, guards, and justified `!` to the 6 Batch S1 files so each reaches zero CS86xx under the pragma; annotate `StoreIdentity.Resolve(string displayName, string filePathFallback = null)` as `string? filePathFallback = null`; annotate `StoreFilterAttribution.Decide`'s documented-nullable parameters (`storeId`, `displayName`, `filePath`, `excludedStoreIds`, etc.) with explicit `?`; annotate `StoreLockupAttribution.FormatLine`'s `identity` parameter as nullable; annotate `StoreWrapperInitProbe.cs`'s `storeDisplayName` parameter as `string?`; confirm `StoreLaunchReadinessEvaluator.cs` and `StoreWrapperInitClock.cs` need no changes beyond the pragma given their existing null-conditional guards
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P7-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s1-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 6 Batch S1 files (AC1).
- [x] [P7-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s1-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s1-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `StoreIdentityTests.cs`, `StoreFilterAttributionTests.cs`, `StoreLockupAttributionTests.cs` (AC3).
- [x] [P7-T5] Verify the already-enabled `StoreRehookResult.cs` still emits zero CS86xx under the pragma gate now that S1 has landed, make NO edits unless a diagnostic appears, and record the outcome at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s1-verify-only-recheck.md`
  - Acceptance: pragma-gate rebuild reports zero CS86xx for `StoreRehookResult.cs`; artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the file remains unmodified (or, if a diagnostic appeared, the minimal annotation fix is recorded).

### Phase 8 — Batch S2 Store Domain Classes
- [x] [P8-T1] Add a `#nullable enable` pragma and apply nullable annotations, guards, and justified `!` to `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` so it reaches zero CS86xx under the pragma; annotate properties populated only inside `Init()`/`Restore()` (`DisplayName`, `StoreId`, `InnerStore`, `Inbox`, `RootFolder`, `UserEmailAddress`, `GlobalAddressBook`) as nullable-typed; annotate `GetSmtpAddressFromStore()` as returning `string?` (explicit `catch (COMException)` path returns `null`); `StoreWrapper.cs` consumes the external oblivious `UtilitiesCS.Threading.CurrentStoreContext` (epic placeholder 9005) at the consuming call site only
  - Acceptance: the file contains a `#nullable enable` pragma; no post-condition attribute is added; no `CurrentStoreContext` source file is edited; annotation/null-safety only (AC3); no `<Nullable>` element added to the csproj.
- [x] [P8-T2] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s2a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `StoreWrapper.cs` (AC1).
- [x] [P8-T3] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s2a-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s2a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `StoreWrapperTests.cs` (AC3).
- [x] [P8-T4] Add a `#nullable enable` pragma and apply nullable annotations, guards, and justified `!` to `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` and `UtilitiesCS/OutlookObjects/Store/StoresWrapper.Filtering.cs` together in this single task, since both are one partial type, so shared members carry a consistent nullable shape (AC7); annotate `Globals` (`IApplicationGlobals`, not default-initialized) as nullable-typed; annotate the static `StoreIsIncluded` overload's `storeId`/`excludedStoreIds`/`displayName`/`filePath` parameters consistently across both the instance and static overloads in both files; `StoresWrapper.cs` consumes the external oblivious `SmartSerializable<T>` (epic placeholder 9003) and `CurrentStoreContext` (epic placeholder 9005) at the consuming call sites only
  - Acceptance: both files contain a `#nullable enable` pragma added in this same task (AC7); no post-condition attribute is added; no `SmartSerializable<T>`/`CurrentStoreContext` source file is edited; annotation/null-safety only (AC3); no `<Nullable>` element added to the csproj.
- [x] [P8-T5] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s2b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for both `StoresWrapper.cs` and `StoresWrapper.Filtering.cs` (AC1, AC7).
- [x] [P8-T6] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s2b-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s2b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `StoresWrapperTests.cs`, `StoresWrapperDisableTests.cs`, `StoresWrapperRehookTests.cs`, `StoresWrapperTests.StoreIdExclusion.cs` (AC3).
- [x] [P8-T7] Add a `#nullable enable` pragma and apply nullable annotations, guards, and justified `!` to `UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs` so it reaches zero CS86xx under the pragma; annotate the constructor's `IStoreRehookService rehook = null` parameter as `IStoreRehookService? rehook = null`; confirm `GetModelOrNull()` returns `StoresWrapper?`
  - Acceptance: the file contains a `#nullable enable` pragma; no post-condition attribute is added; annotation/null-safety only (AC3); no `<Nullable>` element added to the csproj.
- [x] [P8-T8] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s2c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `StoreDisableService.cs` (AC1).
- [x] [P8-T9] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s2c-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s2c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `StoreDisableServiceTests.cs` (AC3).

### Phase 9 — Batch F5 Folder COM-Boundary Adapters (after Batch S2)
- [x] [P9-T1] Add a `#nullable enable` pragma to each of the 5 Batch F5 files: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHandleResolver.cs`, `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs`, `UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs`, `UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs`, `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`; this task executes only after Phase 8 (Batch S2) has completed, because `OutlookFolderHierarchyReader.cs`'s constructor takes a `StoresWrapper storesWrapper` parameter and calls `store.ShouldInclude(_storesWrapper)`/`storesWrapper.ShouldIncludeStore(_store)`
  - Acceptance: all 5 files contain a `#nullable enable` pragma; no `<Nullable>` element added to the csproj; Phase 8 evidence artifacts predate this task's timestamp.
- [x] [P9-T2] Apply nullable annotations, guards, and justified `!` to the 5 Batch F5 files so each reaches zero CS86xx under the pragma; annotate `OutlookFolderHandleResolver.TryResolve(FolderTreeSnapshotNode node, out object folder)` as `TryResolve(FolderTreeSnapshotNode? node, out object? folder)`; annotate `OutlookFolderHierarchyReader.GetRootFolder` as returning `IOutlookFolderAdapter?`; confirm `OutlookFolderNotificationSink.cs`'s existing `store?.StoreID` null-conditional patterns need no new guards, and record its current line count (watch the 500-line limit; flag rather than split if exceeded); confirm `MsgToMime/MAPIMethods.cs` (COM interop declarations only) and `WpfDispatcherYield.cs` (already `[ExcludeFromCodeCoverage]`) need only the pragma with no further edits
  - Acceptance: no post-condition attribute is added; no new runtime guard statements are added to COM-exempt classes (AC4); `OutlookFolderNotificationSink.cs`'s line count is recorded and flagged if it now exceeds 500 lines (not split); annotation/null-safety only (AC3).
- [x] [P9-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-f5-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 5 Batch F5 files (AC1).
- [x] [P9-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f5-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-f5-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `OutlookFolderHierarchyReaderTests.cs`, `OutlookFolderNotificationSinkTests.cs`, `FolderTreeNotificationFakeTests.cs` (AC3).

### Phase 10 — Batch S3 Store Controllers
- [ ] [P10-T1] Add a `#nullable enable` pragma to each of the 2 Batch S3 files: `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [ ] [P10-T2] Apply nullable annotations, guards, and justified `!` to the 2 Batch S3 files so each reaches zero CS86xx under the pragma; re-evaluate the existing `#pragma warning disable CS8625`/`restore` pair inside `StoreWrapperController.StoreLaunchReadiness.NotReady` once the file-level pragma is added, confirming via a clean rebuild whether it is still needed before deciding to keep or remove it; annotate internal viewer-bound fields (`ArchiveOutlook`, `ArchiveFS`, `JunkEmail`, `JunkPotential`) as nullable-typed; `StoreWrapperController.cs` consumes the external oblivious `FilePathHelperConverter` (epic placeholder 9004) and `#364`'s not-yet-landed `FilePathHelper` at `ArchiveFsRoot`/`GetRelativeFsPath()` call sites only; annotate `DisabledStoresController.cs`'s `Viewer` (`IDisabledStoresViewer`) as nullable-by-design (unset until `Launch()`)
  - Acceptance: no post-condition attribute is added; the CS8625 disable/restore pair is either confirmed-still-needed or confirmed-redundant-and-removed only after a clean rebuild; no `FilePathHelperConverter`/`FilePathHelper` source file is edited; annotation/null-safety only (AC3).
- [ ] [P10-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s3-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch S3 files (AC1).
- [ ] [P10-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s3-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s3-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression in `StoreWrapperControllerTests.cs`, `StoreWrapperController_Tests.cs`, `StoreWrapperControllerButtonAndPopulateTests.cs`, `StoreWrapperControllerExcludeStoreTests.cs`, `StoreWrapperControllerLaunchTests.cs`, `DisabledStoresControllerTests.cs` (AC3).

### Phase 11 — Batch S4 Store WinForms Shells
- [ ] [P11-T1] Add a `#nullable enable` pragma to each of the 2 Batch S4 files: `UtilitiesCS/OutlookObjects/Store/StoreWrapperViewer.cs`, `UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.cs`; do NOT add a pragma to the Designer-generated siblings `StoreWrapperViewer.Designer.cs`/`DisabledStoresViewer.Designer.cs` in this task
  - Acceptance: both named files contain a `#nullable enable` pragma; neither `.Designer.cs` sibling is edited; no `<Nullable>` element added to the csproj.
- [ ] [P11-T2] Apply nullable annotations, guards, and justified `!` to the 2 Batch S4 files so each reaches zero CS86xx under the pragma; keep `StoreWrapperViewer.Controller` (`StoreWrapperController`) nullable-by-design, matching the existing `Controller?.` guard usage; annotate `DisabledStoresViewer.cs`'s `DataGridView Dgv` backing field consistent with existing null-handling; introduce no new runtime guard statements for either COM/WinForms-exempt class (AC4)
  - Acceptance: no post-condition attribute is added; no new runtime guard statements are introduced solely to satisfy an annotation (AC4); annotation/null-safety only (AC3).
- [ ] [P11-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/batch-s4-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch S4 files (AC1).
- [ ] [P11-T4] Run the UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s4-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/regression-testing/batch-s4-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no regression across the full UtilitiesCS.Test suite (AC3).

### Phase 12 — Final QC Full Toolchain and Acceptance Verification
- [ ] [P12-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass.
- [ ] [P12-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [ ] [P12-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 63 remediated files and the 18 verify-only Folder/Store files under the per-file pragma (AC1); `/p:Nullable=enable` is not passed.
- [ ] [P12-T4] Run the full UtilitiesCS.Test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts (AC3).
- [ ] [P12-T5] Compute and record the changed-line coverage delta at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`), post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), and changed-line coverage for the 63 remediated Folder/Store files
  - Acceptance: artifact reports baseline coverage, post-change coverage, and changed-line coverage numerically; confirms no coverage regression on changed lines (AC4); `Timestamp:` present. If changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [ ] [P12-T6] Verify AC2 end state: confirm `UtilitiesCS/UtilitiesCS.csproj` still contains no `<Nullable>` element and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation of zero `<Nullable>` occurrences in the csproj (AC2).
- [ ] [P12-T7] Verify AC6: grep the 63 remediated files and the repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage, any `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and any new `record`/`record struct`/`init` accessor introduced in the Folder/Store cluster, and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-no-postcondition-attrs-and-records.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage, no polyfill, and no new `record`/`record struct`/`init` was introduced by this feature (AC6).
- [ ] [P12-T8] Verify scope guards: confirm `FolderPredictor.cs`, `FolderScorer.cs`, and `"FolderWrapper .cs"` were not split, `FolderWrapper .cs` was not renamed, and record `OutlookFolderNotificationSink.cs`'s final line count, at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and confirmation that all three pre-existing >500-line files remain single files, `FolderWrapper .cs`'s filename is unchanged, and `OutlookFolderNotificationSink.cs`'s final line count is recorded (flagged if over 500, not split).
- [ ] [P12-T9] Verify AC7: confirm both partial-class groups (`FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs`; `StoresWrapper.cs` + `StoresWrapper.Filtering.cs`) were remediated in their respective single tasks (P4-T11, P8-T4) with a consistent nullable shape for shared members, and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-ac7-partial-group-check.md`
  - Acceptance: artifact contains `Timestamp:` and cites the evidence task/artifact for each partial-class group confirming same-batch remediation and consistent shared-member nullability (AC7).
- [ ] [P12-T10] Verify AC5 signature compatibility by reviewing the git diff of the 63 remediated files and confirming only nullability annotations (and justified `!`) changed with no public-signature behavior change, and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-signature-compat.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public signature change is limited to additive nullability annotations that reflect actual null behavior (AC5).
- [ ] [P12-T11] Re-check the upstream Wave-0 dependency status recorded in P0-T8 (grep `#nullable enable` in the same 6 upstream files) to confirm whether `#363`/`#364` landed before this feature's execution phases ran, and record the result at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/qa-gates/final-upstream-dependency-recheck.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, a comparison against the P0-T8 baseline finding, and a statement of whether any annotation decisions at the upstream-consuming call sites (§3.2/§3.3 of the research) need re-verification because the upstream contract changed shape after this feature's batches ran.
- [ ] [P12-T12] Record the acceptance-criteria status summary mapping AC1-AC7 to their supporting evidence artifacts at `docs/features/active/utilitiescs-nullable-outlook-folder-store/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1-AC7 citing the exact evidence artifact path that demonstrates satisfaction; any unmet AC is marked remediation-required rather than PASS.

## Test Plan

- Unit: existing `UtilitiesCS.Test/OutlookObjects/Folder/` and `UtilitiesCS.Test/OutlookObjects/Store/`
  MSTest suites (MSTest + Moq + FluentAssertions, ~80 test files including the `Fakes/` helpers) are
  the regression harness; no new temp files. No new tests are required because this is
  annotation-only, but any incidental test touch must use MSTest + Moq + FluentAssertions and remain
  deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{f0,f1,f2,f3a,f3b,f3c,f3d,f4,s0,s1,s2a,s2b,s2c,f5,s3,s4}-coverage.cobertura.xml`.
  - Post-change: `evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/qa-gates/final-tests-coverage.md`.
  - Changed-line comparison: `evidence/qa-gates/final-coverage-delta.md` (baseline vs post-change vs
    changed-line; AC4 no-regression gate).

## Open Questions / Notes

- Coverage-threshold conflict (flagged, not resolved here): CLAUDE.md states repository line coverage
  `>= 80%` and new-code `>= 90%`; `.claude/rules/general-unit-test.md` states uniform `>= 85%` line and
  `>= 75%` branch. This conflict is unresolved and is flagged for the maintainer. For this
  annotation-only feature the operative gate is AC4 (no coverage regression on changed lines), which
  is threshold-independent; the absolute-threshold conflict does not need to be resolved to complete
  this feature.
- Rules-vs-convention conflict (flagged, not resolved here): `.claude/rules/csharp.md` documents the
  type-check step as forcing `/p:Nullable=enable` globally, which conflicts with the epic's per-file
  opt-in convention. Per epic Shared Design, the global flag is NOT used for this feature's
  verification; the conflict is deferred to the Wave-2 CI capstone child
  (`utilitiescs-nullable-ci-capstone`). Policy prohibits editing `.claude/rules/*`.
- Designer-file opt-in conflict (flagged, not resolved here): the epic manifest lists
  `Store/DisabledStoresViewer.Designer.cs` and `Store/StoreWrapperViewer.Designer.cs` as in-scope
  files, but repo convention (and the `#364` `DvgForm.Designer.cs` precedent) is to leave Designer
  files non-opted-in by default. This plan follows that convention; neither Designer file is
  pragma-annotated by any phase above.
- Stale epic-manifest estimate (informational): the epic manifest's `~29`-file estimate for this
  child is superseded by the research's verified 63-file opt-in count; this plan does not edit the
  epic manifest.
- Cross-cluster dependency gap (informational, flagged for the maintainer, not added to the epic
  manifest's `depends_on`): this cluster also consumes `TreeNode<T>`, `ScoDictionaryNew<TKey,TValue>`,
  `SmartSerializable<T>`, `AsyncLazy<T>` (epic placeholder 9003), `ProgressTracker`/
  `CurrentStoreContext` (epic placeholder 9005), and `FilePathHelperConverter` (epic placeholder 9004),
  none of which are declared in the epic manifest's `depends_on` list for this feature. Per-file
  architecture means this is not a hard execution blocker, but P12-T11 re-runs the pragma gate check
  against the upstream files this plan depends on and records whether re-verification is needed once
  those siblings land.
