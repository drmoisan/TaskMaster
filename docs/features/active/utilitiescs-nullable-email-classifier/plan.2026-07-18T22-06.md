# utilitiescs-nullable-email-classifier — Plan

- **Issue:** #372
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-18T22-06
- **Status:** Draft
- **Version:** 0.1
- **Integration branch:** `epic/utilitiescs-nullable-remediation-integration`
- **Upstream contract dependency:** #363 (`utilitiescs-nullable-extensions`, Wave 0)

## Required References

- CLAUDE.md (standing instructions, C# toolchain section).
- `.claude/rules/general-code-change.md` (cross-language code change policy).
- `.claude/rules/general-unit-test.md` (cross-language unit test policy).
- `.claude/rules/csharp.md` (C#-specific toolchain and standards).
- Requirements sources: `docs/features/active/utilitiescs-nullable-email-classifier/issue.md`,
  `docs/features/active/utilitiescs-nullable-email-classifier/spec.md`,
  `docs/features/active/utilitiescs-nullable-email-classifier/user-story.md`.
- Research: `docs/features/active/utilitiescs-nullable-email-classifier/research/research-findings.2026-07-18T21-30.md`.
- Upstream sibling plan (house style mirrored): `docs/features/active/utilitiescs-nullable-extensions/plan.2026-07-18T21-20.md`.

**All work must comply with these policies; do not duplicate their content here.**

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY. Do NOT add a `<Nullable>` element to
  `UtilitiesCS/UtilitiesCS.csproj` (AC2). Enforcement is per-file pragma only; non-opted files
  remain null-oblivious and must not be cross-blocked.
- Verification uses the per-file pragma gate (per batch, project-scoped form):
  `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`,
  and the solution-wide form
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  at final QC. Do NOT pass `/p:Nullable=enable` globally; the global flag surfaces the whole-repo
  ~2131-diagnostic pre-existing debt and drowns this child's signal. `/t:Rebuild` is mandatory
  (per PR #361) so the compiler performs a genuine recompile rather than a silently-skipped
  incremental build.
- Target is net481 / C# 12. Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are NOT available/polyfilled and MUST NOT be used or added. Use plain `?`,
  `where T : notnull`, unconstrained `T?` returns/`out`, guard clauses, and justified `!`.
- No `init` accessor, positional `record`, or `record struct` may be INTRODUCED (they fail CS0518
  on net481). `FolderHierarchyNode` is an existing `sealed record` with get-only auto-properties set
  in a constructor; keep that shape and do not add an `init` accessor.
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign. NO change to
  classifier scoring, model logic, or corpus/probability math (AC3, AC5). Respect the DO-NOT-ALTER
  scoring guard list in `spec.md` (Implementation Strategy) and research §4.
- Files over 500 lines (`BayesianClassifierShared.cs` ~1008, `BayesianClassifierGroup.cs` ~515,
  `CategoryClassifierGroup.cs` ~523, `FlagParser.cs` ~633, `BayesianPerformanceMeasurement.cs`
  ~1537) are annotation-only; do NOT split them here.
- The #363 `NullExtensions.ThrowIfNull<T>` is `where T : notnull` with NO `[NotNull]` attribute, so a
  bare `x.ThrowIfNull();` statement does NOT narrow null-state under `#nullable enable`. Reach zero
  CS86xx by capturing the return value, adding a justified `!` with a `// why` comment, or annotating
  an invariant-guaranteed member as non-null. Do NOT rewrite these into new `if (x is null) throw`
  guards and do NOT add a `[NotNull]` polyfill.
- AC4 pressure: prefer nullable annotations and justified `!` (with a `// why` comment) over new
  runtime guard statements, to avoid introducing new uncovered executable lines. Existing guards
  stay as-is.
- Partial-class co-remediation: the SpamBayes 4-file partial set and the Triage 2-file partial set
  are each remediated together within a single batch (Batch E).
- `.claude/rules/*` must not be edited. The global-flag-versus-per-file-pragma conflict is deferred
  to the Wave-2 CI capstone child.

## Remediation-Set Note (measured, not enumerated)

The remediation set is defined behaviorally as "every in-scope `.cs` file under
`UtilitiesCS/EmailIntelligence/Bayesian`, `.../ClassifierGroups`, and `.../Flags` that emits CS86xx
under the per-file pragma." The definitive set is MEASURED at Phase 0 (task P0-T6). The batch file
lists below are the research static-estimate candidates; each batch task applies the pragma to the
files in its list that are confirmed CS86xx-emitting at Phase 0, and any candidate that proves
already null-clean receives the pragma with zero or near-zero code change. `Flags/` (Batch F) and
`Performance/` (Batch G) scope boundaries are confirmed at Phase 0 (P0-T6) before those batches run.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [x] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read receipt at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [x] [P0-T2] Enumerate the candidate in-scope `.cs` files under `UtilitiesCS/EmailIntelligence/Bayesian`, `UtilitiesCS/EmailIntelligence/ClassifierGroups`, and `UtilitiesCS/EmailIntelligence/Flags` (path, line count, whether the file already carries `#nullable enable`, and research classification REMEDIATE/EXCLUDE) and record the inventory at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact contains `Timestamp:` and lists each candidate file with its classification; confirms the `Obsolete/` files (6), the `Performance/` Designer+viewer files (4), the interface-only files (`IFolderPredictor.cs`, `IFlagTranslator.cs`), and the empty `Bayesian/SpamBayes.cs` stub are EXCLUDE, matching research §1.
- [x] [P0-T3] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [x] [P0-T4] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T5] Capture the clean baseline of the per-file nullable pragma gate by running `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) with no in-scope pragmas yet applied, and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx solution-wide in the pre-state (no in-scope file has opted in), establishing that non-opted files stay null-oblivious; `/p:Nullable=enable` is not passed.
- [x] [P0-T6] Measure the authoritative CS86xx remediation set: temporarily add `#nullable enable` to each REMEDIATE-candidate in-scope file from P0-T2, run `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`, record which files emit CS86xx (the authoritative remediation set) and which are already null-clean, then revert the probe pragmas; record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-remediation-set.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` listing each in-scope file that emits CS86xx (authoritative set) versus already-clean; reconciles the measured count against the epic ~18 target and confirms the `Flags/` (Batch F) and `Performance/` (Batch G) scope boundaries; confirms the probe pragmas were reverted (working tree unchanged except the artifact).
- [x] [P0-T7] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent); Cobertura XML written to the named evidence path.
- [x] [P0-T8] Confirm the AC2 baseline: verify `UtilitiesCS/UtilitiesCS.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in the csproj (AC2 baseline).

### Phase 1 — Batch A: Pure Data and Contract Leaves
- [x] [P1-T1] Add a `#nullable enable` pragma to each Batch A file confirmed CS86xx-emitting at P0-T6: `UtilitiesCS/EmailIntelligence/Bayesian/Prediction.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyNode.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/DoNotSerializeContractResolver.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierExtensions.cs`
  - Acceptance: each named Batch A file that emitted CS86xx at P0-T6 contains a `#nullable enable` pragma; `FolderHierarchyNode.cs` retains its get-only `sealed record` + constructor shape (no `init` accessor added); no `<Nullable>` element added to the csproj.
- [x] [P1-T2] Apply nullable annotations, guards, and justified `!` to the Batch A files so each reaches zero CS86xx under the pragma; annotate `Prediction<T>.CompareTo(Prediction<T>? other)` to match the existing `other is null → return 1` contract (do not alter `_probability.CompareTo` ordering), express unconstrained-generic null-state as `T?`, and annotate `DoNotSerializeContractResolver.CreateProperty` and config DTO members to their true null behavior; use only plain `?`, `where T : notnull`, unconstrained `T?`, and justified `!`
  - Acceptance: no `System.Diagnostics.CodeAnalysis` post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotations reflect actual null behavior; changes are annotation/null-safety only (AC3).
- [x] [P1-T3] Verify the Batch A DO-NOT-ALTER constraint: confirm no scoring/corpus/probability math changed, no operation reordered, and no new `if (x is null) throw` guard was added on any path in the Batch A files; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-a-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that only nullability annotations and justified `!` (with `// why` comments) changed; the `Prediction<T>.CompareTo` ordering and null contract are unchanged (AC3).
- [x] [P1-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-a-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass (any files it reformatted are recorded).
- [x] [P1-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-a-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P1-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the Batch A pragma-enabled files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P1-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-a-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression and no changed-line coverage regression versus the P0-T7 baseline (AC3, AC4).

### Phase 2 — Batch B: Corpus and Count Core
- [x] [P2-T1] Add a `#nullable enable` pragma to each Batch B file confirmed CS86xx-emitting at P0-T6: `UtilitiesCS/EmailIntelligence/Bayesian/Corpus.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/CorpusInherit.cs`
  - Acceptance: each named Batch B file that emitted CS86xx at P0-T6 contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [x] [P2-T2] Apply nullable annotations, guards, and justified `!` to the Batch B files so each reaches zero CS86xx under the pragma; annotate `Corpus.SubtractAsync(..., SegmentStopWatch? sw = null)` (the existing `sw ??= new(...)` already handles it), annotate `Clone()`'s `as Corpus` result with a justified `!` rather than adding a throw, and annotate `CorpusInherit` nullable locals/fields and the `DeserializeJson` nullable return; do NOT alter operator `+`/`-`, `SubtractAsync`, or `SubtractFilter` token-frequency arithmetic (`negTokenWt`/`minCt` thresholds, `TryUpdate`/`TryRemove` flow)
  - Acceptance: no post-condition attribute is added; `Corpus` operator/arithmetic paths are unchanged; public signatures remain behavior-compatible (AC5); changes are annotation/null-safety only (AC3).
- [x] [P2-T3] Verify the Batch B DO-NOT-ALTER constraint: confirm no `Corpus`/`CorpusInherit` set-arithmetic, threshold, or control-flow change and no new `if (x is null) throw` guard on any scoring/corpus path; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-b-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that only nullability annotations and justified `!` changed; the operator arithmetic and threshold constants are unchanged (AC3).
- [x] [P2-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-b-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass.
- [x] [P2-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-b-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P2-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the Batch A+B pragma-enabled files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P2-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-b-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the corpus classes and no changed-line coverage regression versus baseline (AC3, AC4).

### Phase 3 — Batch C: Scoring Engine Core
- [x] [P3-T1] Add a `#nullable enable` pragma to each Batch C file confirmed CS86xx-emitting at P0-T6: `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierShared.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/PerParentClassifier.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyTree.cs`
  - Acceptance: each named Batch C file that emitted CS86xx at P0-T6 contains a `#nullable enable` pragma; the >500-line files (`BayesianClassifierShared.cs`, `BayesianClassifierGroup.cs`) are NOT split; no `<Nullable>` element added to the csproj.
- [x] [P3-T2] Apply nullable annotations, guards, and justified `!` to the Batch C files so each reaches zero CS86xx under the pragma; annotate `BayesianClassifierShared.GetWordInfo` → `WordInfo?` (keep the existing `if (record is null)` branch in `GetWordDistance`), annotate the `Chi2SpamProb` non-evidence `(prob, null)` return element as nullable, annotate `_parent`/`Parent` null-state without adding a hot-path guard, annotate `PerParentClassifier`'s `group = null` default as `BayesianClassifierGroup? group = null`, and annotate `BayesianClassifierGroup`/`FolderHierarchyTree` dictionary `TryGetValue` flow; keep all DO-NOT-ALTER regions (Paul Graham/Robinson `UpdateProbability*`, `CombineProbabilities`, `Chi2SpamProb`, `chi2Q`, `GetClues`, `GetWordDistance`, `KnobList` constants, `Train`/`UnTrain` count paths, `ScoreChildren`/`ChildLogScore`/`LaplaceProbability`/`Normalize`) unchanged
  - Acceptance: no post-condition attribute is added; no arithmetic, comparison, constant, clamp, ordering, or control flow in the DO-NOT-ALTER regions changed; existing `probabilities is null` / `tokens is null` guards remain as-is; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T3] Verify the Batch C DO-NOT-ALTER constraint against the spec/research guard list: confirm no scoring/corpus/probability math change, no reordered `Math.Max`/`Math.Min`/division/log/exp expression, no altered `KnobList`/`LaplaceAlpha` constant, and no new `if (x is null) throw` guard on any scoring path; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-c-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-region confirmation covering the `BayesianClassifierShared.cs` and `PerParentClassifier.cs` guard-list regions; base/override virtual signatures (`UpdateProbability*`) kept consistent to avoid CS8765/CS8767 and to preserve the `SubBayesianClassifier`/`SubClassifierGroup`/`SubCorpus` test-double contracts (AC3, AC5).
- [x] [P3-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-c-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass.
- [x] [P3-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-c-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P3-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the Batch A+B+C pragma-enabled files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P3-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-c-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming the golden/property/characterization scoring suites pass unchanged and no changed-line coverage regression versus baseline (AC3, AC4).

### Phase 4 — Batch D: Engine Base and Generic Engines
- [x] [P4-T1] Add a `#nullable enable` pragma to each Batch D file confirmed CS86xx-emitting at P0-T6: `UtilitiesCS/EmailIntelligence/ClassifierGroups/TristateEngine.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/ConditionalItemEngine.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/MulticlassEngine.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/ClassifierGroupUtilities.cs`
  - Acceptance: each named Batch D file that emitted CS86xx at P0-T6 contains a `#nullable enable` pragma; the abstract bases (`TristateEngine`, `MulticlassEngine`) are annotated in this batch before their derived types in Batch E; no `<Nullable>` element added to the csproj.
- [x] [P4-T2] Apply nullable annotations, guards, and justified `!` to the Batch D files so each reaches zero CS86xx under the pragma; annotate `TristateEngine`'s null-by-default delegate fields (`_tokenize`, `_calculateProbability`, `_getTristateAsync`, `_callback`, `_threshhold`, ...) as `Func<...>?`/`Action<...>?`/`TristateThreshhold?` (keep `GetTristate(double)` thresholds unaltered), annotate `MulticlassEngine.InitAsync` → `Task<T?>` and `LoadStagingData` → `MinedMailInfo[]?` rather than adding a throw (keep `Condition`/`GetOlItemString` filtering and `ProbabilityThreshold = 0.8` unchanged), and honor the #363 `ThrowIfNull`/`ThrowIfNullOrEmpty` no-narrowing contract at bare-statement call sites by capturing the return or adding a justified `!` (do NOT convert to `if (x is null) throw`); annotate `ConditionalItemEngine<T>`, `ManagerAsyncLazy`, and `ClassifierGroupUtilities` null-flow
  - Acceptance: no post-condition attribute is added; no `if (x is null) throw` added at `ThrowIfNull` bare-statement sites; `GetTristate` thresholds and `MulticlassEngine` filtering/threshold defaults unchanged; base delegate/return annotations set so derived overrides in Batch E stay consistent (AC5); annotation/null-safety only (AC3).
- [x] [P4-T3] Verify the Batch D DO-NOT-ALTER constraint: confirm no `GetTristate` threshold, `MulticlassEngine` `Condition`/filtering, or `ProbabilityThreshold` change and no new `if (x is null) throw` guard at any `ThrowIfNull` bare-statement site or scoring path; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-d-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that only nullability annotations and justified `!` changed and that `ThrowIfNull` sites were remediated by return-capture/`!` rather than new throwing guards (AC3, AC4, AC5).
- [x] [P4-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-d-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass.
- [x] [P4-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-d-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P4-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-d-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the Batch A–D pragma-enabled files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P4-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-d-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-d-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the engine classes and no changed-line coverage regression versus baseline (AC3, AC4).

### Phase 5 — Batch E: Derived Engines and Predictors
- [x] [P5-T1] Add a `#nullable enable` pragma to each Batch E file confirmed CS86xx-emitting at P0-T6, remediating the partial sets together: the SpamBayes partial set (`UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs`, `SpamBayes.Actions.cs`, `SpamBayes.Classify.cs`, `SpamBayes.Conditions.cs`) and the Triage partial set (`UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs`, `Triage_OlLogic.cs`), plus `UtilitiesCS/EmailIntelligence/ClassifierGroups/Actionable/ActionableClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamInitTimingProbe.cs`
  - Acceptance: all four SpamBayes partial files and both Triage partial files carry the pragma together; each other named Batch E file that emitted CS86xx at P0-T6 carries the pragma; `CategoryClassifierGroup.cs` (>500 lines) is NOT split; no `<Nullable>` element added to the csproj.
- [x] [P5-T2] Apply nullable annotations, guards, and justified `!` to the Batch E files so each reaches zero CS86xx under the pragma; keep derived-override signatures consistent with the Batch C/D bases (avoid CS8765/CS8767), annotate `SpamBayes.Classify`'s `as MailItem is null` flow and `[]` returns and `SpamBayes.Conditions`'s `UserProperties.Find(...) is not null` flow, and for `OlFolder/OlFolderClassifierGroup.cs` use a justified `!` where the COM contract guarantees a non-null reference the SDK surfaces as nullable; co-annotate `UtilitiesCS/EmailIntelligence/Bayesian/IFolderPredictor.cs` in THIS batch only if a remediated implementer forces CS8767/CS8766
  - Acceptance: no post-condition attribute is added; base/override and interface/implementer nullability kept consistent; no scoring/model math changed; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); if `IFolderPredictor.cs` is co-annotated, that change is annotation-only.
- [x] [P5-T3] Verify the Batch E DO-NOT-ALTER constraint: confirm no derived-engine scoring/classification logic change, no `SpamBayes`/`Triage` behavioral filtering change, and no new `if (x is null) throw` guard on any scoring or COM path; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-e-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that only nullability annotations and justified `!` changed, that the partial sets were co-remediated, and that any `IFolderPredictor.cs` co-annotation is annotation-only (AC3, AC5).
- [x] [P5-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-e-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass.
- [x] [P5-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-e-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P5-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-e-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the Batch A–E pragma-enabled files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P5-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-e-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-e-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming the SpamBayes/Triage/predictor suites pass unchanged and no changed-line coverage regression versus baseline (AC3, AC4).

### Phase 6 — Batch F: Flags Subfolder
- [x] [P6-T1] Add a `#nullable enable` pragma to each Batch F file confirmed in-scope and CS86xx-emitting at P0-T6: `UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs`, `UtilitiesCS/EmailIntelligence/Flags/FlagClassNoItem.cs`, `UtilitiesCS/EmailIntelligence/Flags/FlagConsolidator.cs`, `UtilitiesCS/EmailIntelligence/Flags/FlagTranslator.cs`, `UtilitiesCS/EmailIntelligence/Flags/FlagParser.cs`
  - Acceptance: each named Batch F file that emitted CS86xx at P0-T6 (with `Flags/` confirmed in scope) contains a `#nullable enable` pragma; `FlagParser.cs` (>500 lines) is NOT split; no `<Nullable>` element added to the csproj. If P0-T6 confirmed `Flags/` out of scope, this phase records that determination and is marked not-applicable in its constraint artifact.
- [x] [P6-T2] Apply nullable annotations, guards, and justified `!` to the Batch F files so each reaches zero CS86xx under the pragma; co-annotate `UtilitiesCS/EmailIntelligence/Flags/IFlagTranslator.cs` in THIS batch only if a remediated implementer (`FlagTranslator`) forces CS8767/CS8766; use only plain `?`, `where T : notnull`, unconstrained `T?`, and justified `!`
  - Acceptance: no post-condition attribute is added; public signatures behavior-compatible (AC5); any `IFlagTranslator.cs` co-annotation is annotation-only; changes are annotation/null-safety only (AC3).
- [x] [P6-T3] Verify the Batch F DO-NOT-ALTER constraint: confirm no flag-parsing behavior change and no new `if (x is null) throw` guard beyond justified annotation/`!`; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-f-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that only nullability annotations and justified `!` changed (AC3, AC5), or, if `Flags/` was confirmed out of scope at P0-T6, records that determination.
- [x] [P6-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-f-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass.
- [x] [P6-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-f-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P6-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-f-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the Batch A–F pragma-enabled files (AC1); `/p:Nullable=enable` is not passed.
- [x] [P6-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-f-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-f-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming the Flags suites pass unchanged and no changed-line coverage regression versus baseline (AC3, AC4).

### Phase 7 — Batch G: Performance Tooling
- [x] [P7-T1] Add a `#nullable enable` pragma to each Batch G file confirmed in-scope and CS86xx-emitting at P0-T6: `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianMetricTypes.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianSerializationHelper.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianPerformanceMeasurement.cs`
  - Acceptance: each named Batch G file that emitted CS86xx at P0-T6 (with `Performance/` confirmed in scope) contains a `#nullable enable` pragma; `BayesianPerformanceMeasurement.cs` (>500 lines) is NOT split; the `Performance/` Designer and `Form`-derived viewer files remain EXCLUDE; no `<Nullable>` element added to the csproj. If P0-T6 confirmed `Performance/` out of scope, this phase records that determination and is marked not-applicable in its constraint artifact.
- [x] [P7-T2] Apply nullable annotations, guards, and justified `!` to the Batch G files so each reaches zero CS86xx under the pragma; treat these as measurement/serialization tooling (not scoring), annotate serialization I/O nullable returns and metric-type members to their true null behavior, and if the per-file build surfaces a struct with a `= default` reference-type field, apply `= default!` or a constructor-initialized non-nullable field (mirroring #363's `DfDeedle.EmailRecord` treatment) rather than introducing `record`/`init`
  - Acceptance: no post-condition attribute is added; no `record`/`record struct`/`init` introduced; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P7-T3] Verify the Batch G DO-NOT-ALTER constraint: confirm no measurement/serialization behavior change and no new `if (x is null) throw` guard beyond justified annotation/`!`; record the confirmation at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-g-constraint.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that only nullability annotations and justified `!` changed (AC3, AC5), or, if `Performance/` was confirmed out of scope at P0-T6, records that determination.
- [x] [P7-T4] Run `dotnet tool run csharpier .` and record the format result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-g-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports a clean pass.
- [x] [P7-T5] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-g-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P7-T6] Run the per-file nullable pragma gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/batch-g-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the full Batch A–G pragma-enabled set (AC1); `/p:Nullable=enable` is not passed.
- [x] [P7-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-g-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/regression-testing/batch-g-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming the Performance suites pass unchanged and no changed-line coverage regression versus baseline (AC3, AC4).

### Phase 8 — Final QC Full Toolchain and Acceptance Verification
- [ ] [P8-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass.
- [ ] [P8-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [ ] [P8-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across every pragma-enabled in-scope file (the full measured remediation set from P0-T6) under the per-file pragma (AC1); `/p:Nullable=enable` is not passed.
- [ ] [P8-T4] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts confirming the golden/property/characterization suites pass unchanged (AC3).
- [ ] [P8-T5] Compute and record the changed-line coverage delta at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`), post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), and changed-line coverage for the remediated classifier files
  - Acceptance: artifact reports baseline coverage, post-change coverage, and changed-line coverage numerically; confirms no coverage regression on changed lines (AC4); `Timestamp:` present. If changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [ ] [P8-T6] Verify AC2 end state by running `git diff UtilitiesCS/UtilitiesCS.csproj` and confirming no `<Nullable>` element was added, and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the exact `git diff UtilitiesCS/UtilitiesCS.csproj` command and its output, and confirmation that the diff adds no `<Nullable>` element (AC2).
- [ ] [P8-T7] Verify no prohibited nullable post-condition attribute and no polyfill were added, by grepping the remediated files and the repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage or a `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-no-postcondition-attrs.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage or polyfill was introduced by this feature.
- [ ] [P8-T8] Verify scope guards: confirm no in-scope file over 500 lines was split (`BayesianClassifierShared.cs`, `BayesianClassifierGroup.cs`, `CategoryClassifierGroup.cs`, `FlagParser.cs`, `BayesianPerformanceMeasurement.cs`), that `FolderHierarchyNode` remains a get-only `sealed record` with no `init` accessor, and that no `record`/`record struct`/`init` was introduced anywhere in the remediated set; record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and per-item confirmation of each scope guard (no file split; no `init`/`record`/`record struct` introduced) (AC3/AC5 scope compliance).
- [ ] [P8-T9] Verify AC5 signature compatibility by reviewing the git diff of the remediated files and confirming only nullability annotations (and justified `!` with `// why` comments) changed, with base/override and interface/implementer nullability kept consistent and no public-signature behavior change; record the result at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-signature-compat.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public/protected signature change is limited to additive nullability annotations that reflect actual null behavior and honor the #363 extension contracts (AC5); the `SubBayesianClassifier`/`SubClassifierGroup`/`SubCorpus` override contracts remain intact.
- [ ] [P8-T10] Record the acceptance-criteria status summary mapping AC1–AC5 to their supporting evidence artifacts at `docs/features/active/utilitiescs-nullable-email-classifier/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1–AC5 citing the exact evidence artifact path that demonstrates satisfaction (AC1→`final-nullable-pragma-gate.md`; AC2→`final-ac2-csproj-check.md`; AC3→`final-tests-coverage.md`+constraint artifacts; AC4→`final-coverage-delta.md`; AC5→`final-signature-compat.md`); any unmet AC is marked remediation-required rather than PASS.

## Test Plan

- Unit: existing `UtilitiesCS.Test` EmailIntelligence MSTest suite (MSTest + Moq + FluentAssertions),
  including the golden/property/characterization suites and the subclass test doubles
  (`SubBayesianClassifier`, `SubClassifierGroup`, `SubCorpus`), is the regression harness. No new
  temp files. No new tests are required because this is annotation-only; any incidental test touch
  must use MSTest + Moq + FluentAssertions and remain deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{a..g}-coverage.cobertura.xml`.
  - Post-change: `evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/qa-gates/final-tests-coverage.md`.
  - Changed-line comparison: `evidence/qa-gates/final-coverage-delta.md` (baseline vs post-change vs changed-line; AC4 no-regression gate).

## Open Questions / Notes

- Remediation-set count (measured at P0-T6): research statically estimates ~30–33 candidate files;
  the epic planning estimate is ~18 files requiring code edits. The measured CS86xx set from P0-T6 is
  authoritative and is reported against the ~18 target. `Flags/` (Batch F) and `Performance/` (Batch G)
  scope boundaries are confirmed at P0-T6 before those batches run; if either is confirmed out of
  scope, its phase records that determination and is marked not-applicable.
- Coverage-threshold conflict (flagged, not resolved here): CLAUDE.md states repository line coverage
  `>= 80%` and new-code `>= 90%`; `.claude/rules/general-unit-test.md` states uniform `>= 85%` line and
  `>= 75%` branch. This conflict is unresolved and is flagged for the maintainer. For this
  annotation-only feature the operative gate is AC4 (no coverage regression on changed lines), which is
  threshold-independent; the absolute-threshold conflict does not need to be resolved to complete this
  feature.
- Rules-vs-convention conflict (flagged, not resolved here): `.claude/rules/csharp.md` documents the
  type-check step as forcing `/p:Nullable=enable` globally, which conflicts with the epic's per-file
  opt-in convention. Per the epic Shared Design, the global flag is NOT used for this feature's
  verification; the conflict is deferred to the Wave-2 CI capstone child. Policy prohibits editing
  `.claude/rules/*`.
- `ThrowIfNull` no-narrowing friction (#363 §0 / research §0): the most repetitive remediation pattern
  in this cluster and the most likely place a well-meaning "add a guard" edit would violate AC3/AC4.
  Bare `ThrowIfNull()` statements do not narrow null-state; the executor annotates or adds a justified
  `!` at each dereference rather than converting the call into a throwing guard.
