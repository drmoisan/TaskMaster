# utilitiescs-nullable-reusabletypes — Plan

- **Issue:** #366
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 0)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-18T22-04
- **Status:** Draft
- **Version:** 0.2

## Required References

- CLAUDE.md (standing instructions, C# toolchain section).
- `.claude/rules/general-code-change.md` (cross-language code change policy).
- `.claude/rules/general-unit-test.md` (cross-language unit test policy).
- `.claude/rules/csharp.md` (C#-specific toolchain and standards).
- Requirements sources: `docs/features/active/utilitiescs-nullable-reusabletypes/issue.md`,
  `docs/features/active/utilitiescs-nullable-reusabletypes/spec.md`,
  `docs/features/active/utilitiescs-nullable-reusabletypes/user-story.md`.
- Research: `docs/features/active/utilitiescs-nullable-reusabletypes/research/research-findings.2026-07-18T22-10.md`.

**All work must comply with these policies; do not duplicate their content here.**

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY. Do NOT add a `<Nullable>` element to
  `UtilitiesCS/UtilitiesCS.csproj` (AC2).
- Verification uses the per-file pragma gate:
  `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
  Do NOT pass `/p:Nullable=enable` globally; the global flag forces nullable project-wide and
  surfaces the full pre-existing repo debt, drowning this child's signal. Enforcement is per-file
  pragma only.
- Target is net481 / C# 12. Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are NOT available/polyfilled and MUST NOT be used or added. Reach zero CS86xx
  with plain `?`, `where T : notnull`, unconstrained `TValue?` / `out TValue?` / `T?`, guard clauses,
  and justified `!` (with a `// why` comment).
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign (AC3, AC5).
- Do NOT convert any struct or serialization type to `record` / `init` / `record struct` (those fail
  CS0518 on net481, which lacks `IsExternalInit`). Reference-type fields that Newtonsoft populates by
  reflection become `= null!` (with a `// set by deserialization` comment); `= default` reference-type
  field initializers become `= default!`.
- Six files exceed the 500-line general limit (`Observable/ObservableDictionary.cs` 834,
  `NewSmartSerializable/SmartSerializable.cs` 596, `Serializable/SerializableList.cs` 575,
  `NewSmartSerializable/SmartSerializableBase.cs` 534,
  `Locking/Observable/LinkedList/LockingObservableLinkedList.cs` 522, plus the exempt Designer file).
  All are pre-existing. This child is annotation-only and MUST NOT split any file (a split is a
  refactor, out of scope). Flag for a separate future issue; do not fix here.
- AC4 pressure: prefer nullable annotations and justified `!` (with a `// why` comment) over new
  runtime guard statements, to avoid introducing new uncovered executable lines. Existing guards stay
  as-is.
- Exempt (do NOT add `#nullable enable`, WinForms exemption (b)):
  `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/Config/ConfigViewer.Designer.cs`,
  `.../ConfigViewer.cs`, `.../ConfigGroupBox.cs`. `ConfigController.cs` and
  `NewSmartSerializableConfig.cs` remain IN scope.
- `NewtonsoftHelpers` (#9004) is a SEPARATE sibling child and is OUT OF SCOPE. Do not annotate or
  touch any `NewtonsoftHelpers` file; only annotate the local usage sites in this cluster.
- CS8714 `where TKey : notnull` on the four generic dictionary bases
  (`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, `ScDictionary`) is a
  public generic-parameter-list contract change and MUST be ratified by the maintainer (Phase 6)
  before it is applied or committed.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [x] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read receipt at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [x] [P0-T2] Enumerate the 54 `.cs` files under `UtilitiesCS/ReusableTypeClasses/` (recursive) and record the baseline inventory (path, line count, whether the file already carries `#nullable enable`, and in-scope vs. exempt) at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact lists all 54 files; confirms exactly 3 exempt (`NewSmartSerializable/Config/ConfigViewer.Designer.cs`, `.../ConfigViewer.cs`, `.../ConfigGroupBox.cs`) and 51 in scope; confirms zero files currently carry `#nullable enable` (greenfield); contains `Timestamp:`.
- [x] [P0-T3] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [x] [P0-T4] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T5] Capture baseline per-file nullable pragma-gate build by running `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail; because no ReusableTypeClasses file currently carries the pragma, the expected CS86xx count attributable to this cluster is zero at baseline).
- [x] [P0-T6] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent); Cobertura XML written to the named evidence path.
- [x] [P0-T7] Confirm the AC2 baseline: verify `UtilitiesCS/UtilitiesCS.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in the csproj (AC2 baseline).

### Phase 1 — Batch 1 Trivial Leaves EventArgs Observers and Interfaces
- [x] [P1-T1] Add a `#nullable enable` pragma to each of the 13 Batch 1 files: `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Bag/BagChangedEventArgs.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Bag/ISimpleActionBagObserver.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Bag/SimpleActionBagObserver.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/DictionaryChangedEventArgs.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/SimpleActionDictionaryObserver.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/ILockingLinkedList.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/Observable/LinkedList/ILockingLinkedListObserver.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/Observable/LinkedList/LockingObservableLinkedListChangedEventArgs.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/Observable/LinkedList/SimpleActionLockingLinkedListObserver.cs`, `UtilitiesCS/ReusableTypeClasses/Observable/ObservableCollectionBatchUpdate.cs`, `UtilitiesCS/ReusableTypeClasses/Observable/ObserverHelper.cs`, `UtilitiesCS/ReusableTypeClasses/Other/AbstractCloneable.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/IConcurrentObservableCollectionSeams.cs`
  - Acceptance: each of the 13 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj. Interface-only files may carry the pragma for cluster consistency even though they emit no CS86xx.
- [x] [P1-T2] Apply nullable annotations, guards, and justified `!` to the 13 Batch 1 files so each reaches zero CS86xx under the pragma; annotate `object? sender` handler parameters and `EventHandler<...>?` / delegate-observer fields to reflect actual null behavior; use only plain `?`, `where T : notnull`, unconstrained `T?`, and justified `!` (no post-condition attributes; no new runtime guards where annotation suffices)
  - Acceptance: no `System.Diagnostics.CodeAnalysis` post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotations reflect actual null behavior; changes are annotation/null-safety only (AC3). Interface/EventArgs leaves with no executable behavior require no remediation beyond the pragma.
- [x] [P1-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-1-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 13 Batch 1 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P1-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-1-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-1-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 2 — Batch 2 Standalone Value and Utility Types
- [x] [P2-T1] Add a `#nullable enable` pragma to each of the 7 Batch 2 files: `UtilitiesCS/ReusableTypeClasses/Other/AsyncQueue.cs`, `UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs`, `UtilitiesCS/ReusableTypeClasses/LazyTry/LazyTry.cs`, `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs`, `UtilitiesCS/ReusableTypeClasses/Other/StackObjectCS.cs`, `UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs`, `UtilitiesCS/ReusableTypeClasses/Matrices/DataConverter2d.cs`
  - Acceptance: each of the 7 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P2-T2] Apply nullable annotations, guards, and justified `!` to the 7 Batch 2 files so each reaches zero CS86xx under the pragma; annotate `TreeNode<T>.Parent` as `TreeNode<T>?` (nullable root; `Depth` checks `Parent is null`), express `LazyTry` `Try`/`out` unconstrained-generic null-state as `out TValue?` / `T?`, and annotate `DataConverter2d` `object[,]` cast results as nullable; keep value-type/`struct`-constrained generics free of reference-nullable annotations
  - Acceptance: no post-condition attribute is added; unconstrained-generic null-state expressed via `out TValue?`/`T?` (not `[MaybeNullWhen]`); public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P2-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-2-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 7 Batch 2 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P2-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-2-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-2-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 3 — Batch 3 Matrices
- [x] [P3-T1] Add a `#nullable enable` pragma to each of the 3 Batch 3 files: `UtilitiesCS/ReusableTypeClasses/Matrices/DenMatrix.cs`, `UtilitiesCS/ReusableTypeClasses/Matrices/JaggedMatrix.cs`, `UtilitiesCS/ReusableTypeClasses/Matrices/Matrix.cs`
  - Acceptance: each of the 3 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P3-T2] Apply nullable annotations, guards, and justified `!` to the 3 Batch 3 files so each reaches zero CS86xx under the pragma; annotate `object[,]` / boxed element access and cast chains as nullable where the element may be null; keep value-type generic element storage free of reference-nullable annotations
  - Acceptance: no post-condition attribute is added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-3-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 3 Batch 3 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P3-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-3-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-3-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 4 — Batch 4 Timed Actions
- [x] [P4-T1] Add a `#nullable enable` pragma to each of the 5 Batch 4 files: `UtilitiesCS/ReusableTypeClasses/TimedActions/TimerWrapper.cs`, `UtilitiesCS/ReusableTypeClasses/TimedActions/TimedAsyncTask.cs`, `UtilitiesCS/ReusableTypeClasses/TimedActions/TimedBatchAction.cs`, `UtilitiesCS/ReusableTypeClasses/TimedActions/TimedQueueOfActions.cs`, `UtilitiesCS/ReusableTypeClasses/TimedActions/TimedDiskWriter.cs`
  - Acceptance: each of the 5 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P4-T2] Apply nullable annotations, guards, and justified `!` to the 5 Batch 4 files so each reaches zero CS86xx under the pragma; annotate timer/callback delegate fields and disk-IO seam members to reflect actual null behavior; isolate IO seams via existing injection points without adding new runtime guards where annotation suffices
  - Acceptance: no post-condition attribute is added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); no new IO in tests, no temp files.
- [x] [P4-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-4-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 5 Batch 4 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P4-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-4-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-4-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the timed-action classes (AC3).

### Phase 5 — Batch 5 Locking Core
- [x] [P5-T1] Add a `#nullable enable` pragma to each of the 4 Batch 5 files: `UtilitiesCS/ReusableTypeClasses/Locking/LockingLinkedListNode.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/LockingLinkedList.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/Observable/LinkedList/LockingObservableLinkedListNode.cs`, `UtilitiesCS/ReusableTypeClasses/Locking/Observable/LinkedList/LockingObservableLinkedList.cs`
  - Acceptance: each of the 4 named files contains a `#nullable enable` pragma; `LockingObservableLinkedList.cs` (522 lines, pre-existing >500) is NOT split; no `<Nullable>` element added to the csproj.
- [x] [P5-T2] Apply nullable annotations, guards, and justified `!` to the 4 Batch 5 files so each reaches zero CS86xx under the pragma; annotate the nullable node graph (`Next`/`Prev`/head/tail as `...Node?`) to reflect the linked-list null-terminus contract; keep locking behavior unchanged
  - Acceptance: no post-condition attribute is added; `LockingObservableLinkedList.cs` not split; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P5-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-5-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 4 Batch 5 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P5-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-5-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-5-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the locking classes (AC3).

### Phase 6 — Batch 6 Concurrent Observable Bases and CS8714 Ratification
- [x] [P6-T1] Add a `#nullable enable` pragma to each of the 5 Batch 6 files: `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Bag/ConcurrentObservableBag.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.Serialization.cs`, `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/ConcurrentObservableDictionary.cs`, `UtilitiesCS/ReusableTypeClasses/Observable/ObservableDictionary.cs`
  - Acceptance: each of the 5 named files contains a `#nullable enable` pragma; `ObservableDictionary.cs` (834 lines, pre-existing >500) is NOT split; the `ConcurrentObservableCollection` partial pair (`.cs` and `.Serialization.cs`) both carry the pragma and are remediated together in this phase; no `<Nullable>` element added to the csproj.
- [x] [P6-T2] STOP for maintainer ratification of the `where TKey : notnull` generic-parameter-list contract change on the four dictionary bases (`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, `ScDictionary`) required to clear CS8714 under the pragma gate; do NOT apply the constraint or commit until ratified, and record the decision at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/other/cs8714-notnull-ratification.md`
  - Acceptance: artifact contains `Timestamp:`, the exact constraint text (`where TKey : notnull`), the four affected type names, the rationale (CS8714 is an 87xx diagnostic that still errors under `/t:Rebuild /p:TreatWarningsAsErrors=true`; `ConcurrentDictionary` already rejects null keys at runtime so the constraint is IL-metadata-only with no runtime behavior change per AC3/AC5), the rejected alternative (`#pragma warning disable CS8714`), and an explicit `RATIFIED:` or `BLOCKED:` maintainer decision line. If `BLOCKED:`, execution halts and the phase outcome is remediation-required, not PASS.
  - RATIFIED: 2026-07-19T22:14:30Z — the project maintainer ratified the `where TKey : notnull` constraint on the four generic dictionary bases in-session. The STOP is cleared. The empirical finding stands (net481 BCL reference assemblies are not nullable-annotated, so zero CS8714 is actually emitted; the constraint is forward-looking public-contract hygiene with no runtime behavior change since `ConcurrentDictionary` already rejects null keys). Apply the constraint in [P6-T3] (`ConcurrentObservableDictionary`) and [P8-T2] (`ScoDictionaryNew`, `ScoDictionaryStatic`, `ScDictionary`); do NOT constrain the `ConcurrentBag<T>`-based `ConcurrentObservableBag`/`ScBag`. Dossier: `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/other/cs8714-notnull-ratification.md`.
- [x] [P6-T3] Apply nullable annotations, guards, justified `!`, and — only if [P6-T2] recorded `RATIFIED:` — the `where TKey : notnull` constraint on `ConcurrentObservableDictionary<TKey, TValue>` and — under the epic-authorized Option-A-extended-to-two-files scope waiver — add `where TKey : notnull` to `WrapperScoDictionary<TDerived, TKey, TValue>` in `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` (a #367-owned consumer that otherwise emits CS8714 at lines 24, 33, 195, 207) AND add `where TKey : notnull` to `ScoDictionaryConverter<TDerived, TKey, TValue>` in `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` (the second #367-owned consumer that otherwise emits CS8714 at lines 27, 28, 40); do NOT modify any other NewtonsoftHelpers file under this waiver to the 5 Batch 6 files so each reaches zero CS86xx and zero CS8714 under the pragma; annotate uninitialized events as `EventHandler<...>?`, `default(TValue)` locals as `TValue?`, and `Find`-style returns as `T?` / `TValue?`; `ConcurrentBag<T>`-based `ConcurrentObservableBag` takes `T` with no `notnull` requirement and MUST NOT receive the constraint
  - Acceptance: no post-condition attribute is added; the `where TKey : notnull` constraint is applied to `ConcurrentObservableDictionary` only when ratified; `ConcurrentObservableBag` is not constrained; public signatures behavior-compatible except the additive ratified constraint (AC5); annotation/null-safety only (AC3); the one-line `where TKey : notnull` additions to `WrapperScoDictionary<TDerived, TKey, TValue>` and `ScoDictionaryConverter<TDerived, TKey, TValue>` (in `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`) are applied under the epic Option-A-extended-to-two-files waiver and no other NewtonsoftHelpers file is touched.
- [x] [P6-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-6-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx and zero CS8714 for the 5 Batch 6 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P6-T5] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-6-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-6-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the concurrent-observable bases (AC3).

### Phase 7 — Batch 7 SmartSerializable Family and Config Controller
- [x] [P7-T1] Add a `#nullable enable` pragma to each of the 7 Batch 7 files: `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/Config/NewSmartSerializableConfig.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableStatic.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableNonTyped.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableLoader.cs`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/Config/ConfigController.cs`
  - Acceptance: each of the 7 named files contains a `#nullable enable` pragma; `SmartSerializable.cs` (596) and `SmartSerializableBase.cs` (534), both pre-existing >500, are NOT split; the three exempt WinForms files are NOT opted in; no `<Nullable>` element added to the csproj.
- [x] [P7-T2] Apply nullable annotations, guards, and justified `!` to the 7 Batch 7 files so each reaches zero CS86xx under the pragma; annotate uninitialized events as `PropertyChangedEventHandler?`, fields Newtonsoft populates by reflection (e.g. the `Lazy<JsonSerializerSettings>` trio in `NewSmartSerializableConfig`, `Config`) as `= null!` with a `// set by deserialization` comment, `null`-literal `altLoader` params as `Func<T>? altLoader`, and the `MethodBase.GetCurrentMethod()!.DeclaringType` logger initializer with a justifying `// why` comment; annotate `ConfigController.Viewer` construction/dereference to reflect actual null behavior; do NOT convert any type to `record`/`init`/`record struct`; do NOT touch any `NewtonsoftHelpers` file
  - Acceptance: no post-condition attribute is added; no `record`/`init`/`record struct` conversion; no `NewtonsoftHelpers` file edited; the three exempt WinForms files unchanged; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P7-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-7-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 7 Batch 7 files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P7-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-7-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-7-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the SmartSerializable family and config controller (AC3).

### Phase 8 — Batch 8 Serializable Wrappers
- [ ] [P8-T1] Add a `#nullable enable` pragma to each of the 7 Batch 8 files: `UtilitiesCS/ReusableTypeClasses/Serializable/SerializableList.cs`, `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/ScBag.cs`, `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryStatic.cs`, `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs`, `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloLinkedList.cs`, `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs`, `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/ScDictionary.cs`
  - Acceptance: each of the 7 named files contains a `#nullable enable` pragma; `SerializableList.cs` (575, pre-existing >500) is NOT split; no `<Nullable>` element added to the csproj.
- [ ] [P8-T2] Apply nullable annotations, guards, justified `!`, and the ratified `where TKey : notnull` constraint (from [P6-T2]) consistently to `ScoDictionaryNew<TKey, TValue>`, `ScoDictionaryStatic`, and `ScDictionary` so each of the 7 Batch 8 files reaches zero CS86xx and zero CS8714 under the pragma; annotate serialization round-trip fields (`Name`, `ism`) as `= null!` with a `// set by deserialization` comment, uninitialized events as `?`, `null`-literal `altLoader` params as `Func<...>? altLoader`, and `SloLinkedList` `NotImplementedException` interface stubs' parameters to match the interface nullable contract without implementing bodies; `ScBag` (`ConcurrentBag<T>`-based) takes `T` and MUST NOT receive the `notnull` constraint; do NOT touch any `NewtonsoftHelpers` file
  - Acceptance: no post-condition attribute is added; the `where TKey : notnull` constraint applied consistently to `ScoDictionaryNew`, `ScoDictionaryStatic`, `ScDictionary` (matching the [P6-T2] ratification) and NOT to `ScBag`; no `record`/`init` conversion; no `NewtonsoftHelpers` file edited; `SerializableList.cs` not split; public signatures behavior-compatible except the additive ratified constraint (AC5); annotation/null-safety only (AC3).
- [ ] [P8-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/batch-8-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx and zero CS8714 for the 7 Batch 8 files (AC1); `/p:Nullable=enable` is not passed.
- [ ] [P8-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-8-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/regression-testing/batch-8-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the serializable wrappers (AC3).

### Phase 9 — Final QC Full Toolchain and Acceptance Verification
- [ ] [P9-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass.
- [ ] [P9-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [ ] [P9-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx and zero CS8714 across all 51 in-scope ReusableTypeClasses files under the per-file pragma (AC1); `/p:Nullable=enable` is not passed; non-opted-in files elsewhere are not cross-blocked (AC6).
- [ ] [P9-T4] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts (AC3).
- [ ] [P9-T5] Compute and record the changed-line coverage delta at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`), post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), and changed-line coverage for the 51 remediated ReusableTypeClasses files
  - Acceptance: artifact reports baseline coverage, post-change coverage, and changed-line coverage numerically; confirms no coverage regression on changed lines (AC4); `Timestamp:` present. If changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [ ] [P9-T6] Verify AC2 end state: confirm `UtilitiesCS/UtilitiesCS.csproj` still contains no `<Nullable>` element and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation of zero `<Nullable>` occurrences in the csproj (AC2).
- [ ] [P9-T7] Verify no prohibited nullable post-condition attribute and no polyfill were added, by grepping the 51 remediated files and the repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage or a `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-no-postcondition-attrs.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage or polyfill was introduced by this feature.
- [ ] [P9-T8] Verify scope guards: confirm the five in-scope over-limit files (`Observable/ObservableDictionary.cs`, `NewSmartSerializable/SmartSerializable.cs`, `Serializable/SerializableList.cs`, `NewSmartSerializable/SmartSerializableBase.cs`, `Locking/Observable/LinkedList/LockingObservableLinkedList.cs`) were NOT split and no type was converted to `record`/`record struct`/`init`, and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and confirmation that each over-limit file remains a single file and no `record`/`init` conversion was introduced (AC3/AC5 scope compliance).
- [ ] [P9-T9] Verify the CS8714 constraint consistency and AC6 exemption: confirm the ratified `where TKey : notnull` constraint is present on all four dictionary bases (`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, `ScDictionary`) and on `WrapperScoDictionary<TDerived, TKey, TValue>` and `ScoDictionaryConverter<TDerived, TKey, TValue>` (the two epic-authorized Option-A-extended-to-two-files waiver consumers in `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` and `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`), and NOT on the `ConcurrentBag<T>`-based types (`ConcurrentObservableBag`, `ScBag`), and confirm the three exempt WinForms files (`ConfigViewer.Designer.cs`, `ConfigViewer.cs`, `ConfigGroupBox.cs`) carry no `#nullable enable`; record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-constraint-and-exemption-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep commands used, per-type confirmation of the constraint placement, confirmation the three exempt files remain null-oblivious (AC5/AC6), and confirmation that the `where TKey : notnull` waiver line is present on `WrapperScoDictionary<TDerived, TKey, TValue>` and on `ScoDictionaryConverter<TDerived, TKey, TValue>` (in `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`) and that no NewtonsoftHelpers file other than those two was modified.
- [ ] [P9-T10] Verify AC5 signature compatibility by reviewing the git diff of the 51 remediated files and confirming only nullability annotations, the ratified `where TKey : notnull` constraint, and justified `!` changed with no public-signature behavior change, and record the result at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/qa-gates/final-signature-compat.md`. The `WrapperScoDictionary<TDerived, TKey, TValue>` and `ScoDictionaryConverter<TDerived, TKey, TValue>` (in `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`) one-line additive `where TKey : notnull` constraints are epic-authorized Option-A-extended-to-two-files waiver changes (additive constraint, no behavior change) and are expected in the diff.
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public signature change is limited to additive nullability annotations (and the ratified constraint) that reflect actual null behavior (AC5).
- [ ] [P9-T11] Record the acceptance-criteria status summary mapping AC1–AC6 to their supporting evidence artifacts at `docs/features/active/utilitiescs-nullable-reusabletypes/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1–AC6 citing the exact evidence artifact path that demonstrates satisfaction; any unmet AC is marked remediation-required rather than PASS.

## Test Plan

- Unit: existing `UtilitiesCS.Test/` MSTest suite (MSTest + Moq + FluentAssertions) is the regression
  harness; no new temp files. No new tests are required because this is annotation-only, but any
  incidental test touch must use MSTest + Moq + FluentAssertions and remain deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{1..8}-coverage.cobertura.xml`.
  - Post-change: `evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/qa-gates/final-tests-coverage.md`.
  - Changed-line comparison: `evidence/qa-gates/final-coverage-delta.md` (baseline vs post-change vs changed-line; AC4 no-regression gate).

## Open Questions / Notes

- CS8714 `where TKey : notnull` (highest-risk decision): this is the one public generic-parameter-list
  contract change in the child. It is isolated to Phase 6 ([P6-T2] STOP for maintainer ratification)
  and applied only after `RATIFIED:` — to `ConcurrentObservableDictionary` in Phase 6 and consistently
  to `ScoDictionaryNew`, `ScoDictionaryStatic`, `ScDictionary` in Phase 8. The `ConcurrentBag<T>`-based
  types are not affected. `#pragma warning disable CS8714` is rejected (suppresses rather than fixes).
- Coverage-threshold conflict (flagged, not resolved here): CLAUDE.md states repository line coverage
  `>= 80%` and new-code `>= 90%`; `.claude/rules/general-unit-test.md` states uniform `>= 85%` line and
  `>= 75%` branch. This conflict is unresolved and is flagged for the maintainer. For this
  annotation-only feature the operative gate is AC4 (no coverage regression on changed lines), which is
  threshold-independent; the absolute-threshold conflict does not need to be resolved to complete this
  feature.
- Rules-vs-convention conflict (flagged, not resolved here): `.claude/rules/csharp.md` documents the
  type-check step as forcing `/p:Nullable=enable` globally, which conflicts with the epic's per-file
  opt-in convention. Per epic Shared Design, the global flag is NOT used for this feature's
  verification; the conflict is deferred to the Wave-2 CI capstone child. Policy prohibits editing
  `.claude/rules/*`.
- `NewtonsoftHelpers` (#9004) is a separate sibling child and is out of scope; only local usage sites
  in this cluster are annotated.
