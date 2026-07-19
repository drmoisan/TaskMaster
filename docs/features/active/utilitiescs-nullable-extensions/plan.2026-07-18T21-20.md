# utilitiescs-nullable-extensions — Plan

- **Issue:** #363
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 0)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-18T21-20
- **Status:** Draft
- **Version:** 0.2

## Required References

- CLAUDE.md (standing instructions, C# toolchain section).
- `.claude/rules/general-code-change.md` (cross-language code change policy).
- `.claude/rules/general-unit-test.md` (cross-language unit test policy).
- `.claude/rules/csharp.md` (C#-specific toolchain and standards).
- Requirements sources: `docs/features/active/utilitiescs-nullable-extensions/issue.md`,
  `docs/features/active/utilitiescs-nullable-extensions/spec.md`,
  `docs/features/active/utilitiescs-nullable-extensions/user-story.md`.
- Research: `docs/features/active/utilitiescs-nullable-extensions/research/research-findings.2026-07-18T21-45.md`.

**All work must comply with these policies; do not duplicate their content here.**

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY. Do NOT add a `<Nullable>` element to
  `UtilitiesCS/UtilitiesCS.csproj` (AC2).
- Verification uses the per-file pragma gate:
  `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
  Do NOT pass `/p:Nullable=enable` globally; the global flag surfaces the whole-repo ~2131-diagnostic
  pre-existing debt and drowns this child's signal. Enforcement is per-file pragma only.
- Target is net481 / C# 12. Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are NOT available/polyfilled and MUST NOT be used or added. Use plain `?`,
  `where T : notnull`, unconstrained `out TValue?` / `TValue?`, guard clauses, and justified `!`.
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign (AC3, AC5).
- `ArrayExtensions.cs` (544 lines, pre-existing >500) is annotation-only; do NOT split it.
- `DfDeedle.cs` and `DfDeedle.FrameUtilities.cs` are one `partial class` and MUST be remediated in
  the same phase. Keep `DfDeedle.EmailRecord` a plain `private struct`; do not convert to `record` /
  `record struct` (`init` / positional records fail CS0518 on net481). `= default` reference-type
  field initializers become `= default!`.
- AC4 pressure: prefer nullable annotations and justified `!` (with a `// why` comment) over new
  runtime guard statements, to avoid introducing new uncovered executable lines. Existing guards
  stay as-is.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [ ] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read receipt at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [ ] [P0-T2] Enumerate the 25 `.cs` files under `UtilitiesCS/Extensions/` and record the baseline inventory (path, line count, and whether the file already carries `#nullable enable`) at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact lists all 25 files; confirms exactly 2 (`IAsyncEnumerableExtensions.cs`, `NullExtensions.cs`) are already `#nullable enable` (verify-only) and 23 are remediation targets; contains `Timestamp:`.
- [ ] [P0-T3] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [ ] [P0-T4] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [ ] [P0-T5] Capture baseline per-file nullable pragma-gate build by running `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and any CS86xx count from currently-enabled files, expected zero).
- [ ] [P0-T6] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent); Cobertura XML written to the named evidence path.
- [ ] [P0-T7] Confirm the AC2 baseline: verify `UtilitiesCS/UtilitiesCS.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/utilitiescs-nullable-extensions/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in the csproj (AC2 baseline).

### Phase 1 — Batch A Trivial Confirm-Clean Leaves plus Verify-Only Files
- [ ] [P1-T1] Verify the 2 pre-enabled files `UtilitiesCS/Extensions/IAsyncEnumerableExtensions.cs` and `UtilitiesCS/Extensions/NullExtensions.cs` still emit zero CS86xx under the pragma gate; make NO edits unless a diagnostic appears, and record the outcome at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/verify-only-preenabled.md`
  - Acceptance: pragma-gate rebuild (`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ... /p:TreatWarningsAsErrors=true`) reports zero CS86xx for both files; artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; both files remain unmodified (or, if a diagnostic appeared, the minimal annotation fix is recorded).
- [ ] [P1-T2] Add a `#nullable enable` pragma to each of the 6 Batch A files: `UtilitiesCS/Extensions/ExtToChar.cs`, `UtilitiesCS/Extensions/CompilerServicesExtensions.cs`, `UtilitiesCS/Extensions/DrawingExtensions.cs`, `UtilitiesCS/Extensions/QueueExtensions.cs`, `UtilitiesCS/Extensions/IControlExtensions.cs`, `UtilitiesCS/Extensions/ExceptionExtensions.cs`
  - Acceptance: each of the 6 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [ ] [P1-T3] Apply nullable annotations, guards, and justified `!` to the 6 Batch A files so each reaches zero CS86xx under the pragma; use only plain `?`, `where T : notnull`, unconstrained `T?`, and justified `!` (no post-condition attributes; no new runtime guards where annotation suffices)
  - Acceptance: no `System.Diagnostics.CodeAnalysis` post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotations reflect actual null behavior; changes are annotation/null-safety only (AC3).
- [ ] [P1-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/batch-a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 6 Batch A files (AC1).
- [ ] [P1-T5] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-a-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 2 — Batch B String Serialization and Image-Stream Utilities
- [ ] [P2-T1] Add a `#nullable enable` pragma to each of the 6 Batch B files: `UtilitiesCS/Extensions/StringExtensions.cs`, `UtilitiesCS/Extensions/JsonExtensions.cs`, `UtilitiesCS/Extensions/JsonSerializerExtensions.cs`, `UtilitiesCS/Extensions/ImageExtensions.cs`, `UtilitiesCS/Extensions/StreamExtensions.cs`, `UtilitiesCS/Extensions/LazyExtension.cs`
  - Acceptance: each of the 6 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [ ] [P2-T2] Apply nullable annotations, guards, and justified `!` to the 6 Batch B files so each reaches zero CS86xx under the pragma; in `LazyExtension.cs` keep the `where T : struct` (`ToLazyValue`) overloads free of `T?` reference annotations while annotating the `where T : class` (`ToLazy`) overloads; `ImageExtensions.ConvertTo` and similar object-returning helpers become `object?`
  - Acceptance: no post-condition attribute is added; struct-constrained overloads are not given reference-nullable annotations; public signatures remain behavior-compatible (AC5); changes are annotation/null-safety only (AC3).
- [ ] [P2-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/batch-b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 6 Batch B files (AC1).
- [ ] [P2-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-b-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 3 — Batch C Core Generic Collection Contracts
- [ ] [P3-T1] Add a `#nullable enable` pragma to each of the 4 Batch C files: `UtilitiesCS/Extensions/IEnumerableExtensions.cs`, `UtilitiesCS/Extensions/ArrayExtensions.cs`, `UtilitiesCS/Extensions/IListExtensions.cs`, `UtilitiesCS/Extensions/DictionaryExtensions.cs`
  - Acceptance: each of the 4 named files contains a `#nullable enable` pragma; `ArrayExtensions.cs` is NOT split; no `<Nullable>` element added to the csproj.
- [ ] [P3-T2] Apply nullable annotations, guards, and justified `!` to the 4 Batch C files so each reaches zero CS86xx under the pragma; express unconstrained-generic `out`/return null-state as `out TValue?` / `T?` (e.g. `Find<T>` → `T?`, `TryFindMax` `out T? max`, `UpdateOrRemove` `out TValue?`), annotate optional delegate params (e.g. `Action<int>? onItemCompleted = null`), and annotate `TryFlattenArrayTree`/`FlattenArrayTree` returns as `T[]?`; these annotations are the cross-module contracts consumed by Batch E and Wave-1 children, so they must reflect actual null behavior (AC5)
  - Acceptance: no post-condition attribute is added; unconstrained-generic null-state expressed via `out TValue?`/`T?` (not `[MaybeNullWhen]`); `ArrayExtensions.cs` not split; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [ ] [P3-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/batch-c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 4 Batch C files (AC1).
- [ ] [P3-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-c-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the core-contract classes (AC3).

### Phase 4 — Batch D Reflection Metadata and WinForms
- [ ] [P4-T1] Add a `#nullable enable` pragma to each of the 3 Batch D files: `UtilitiesCS/Extensions/EnumExtensions.cs`, `UtilitiesCS/Extensions/TraceExtensions.cs`, `UtilitiesCS/Extensions/WinFormsExtensions.cs`
  - Acceptance: each of the 3 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj.
- [ ] [P4-T2] Apply nullable annotations, guards, and justified `!` to the 3 Batch D files so each reaches zero CS86xx under the pragma; annotate reflection returns as nullable (`PropertyInfo`/`FieldInfo.GetValue` → `object?`, `Type.GetField`/`GetProperty` → nullable, `Type.FullName` → `string?`, `GetAncestor<T>` returning null, `Activator.CreateInstance` → `object?`, `EventHandler?`), and keep `Enum`-constrained value-type generics free of reference-nullable annotations
  - Acceptance: no post-condition attribute is added; `Enum`/value-type generics not given reference-nullable annotations; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [ ] [P4-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/batch-d-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 3 Batch D files (AC1).
- [ ] [P4-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-d-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-d-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 5 — Batch E Dataframe and Async Serialization
- [ ] [P5-T1] Add a `#nullable enable` pragma to each of the 4 Batch E files: `UtilitiesCS/Extensions/AsyncSerialization.cs`, `UtilitiesCS/Extensions/DfMLNet.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs`; `DfDeedle.cs` and `DfDeedle.FrameUtilities.cs` are one partial class and are remediated together in this phase
  - Acceptance: each of the 4 named files contains a `#nullable enable` pragma; both `DfDeedle` partial files carry the pragma; no `<Nullable>` element added to the csproj.
- [ ] [P5-T2] Apply nullable annotations, guards, and justified `!` to the 4 Batch E files so each reaches zero CS86xx under the pragma; annotate `GetFirstNonNull` → `object?`, `DataFrameColumn.Name` → `string?`, `Frame<>`-returning members that may return null as nullable, and object-cast chains; keep `DfDeedle.EmailRecord` a plain `private struct` and convert `= default` reference-type field initializers to `= default!` (do NOT convert to `record`/`record struct`); consume the already-annotated Batch C contracts (`CastNullSafe`, `ToStringArray`, `SliceColumn`, `To2D`) without re-touching Batch C files
  - Acceptance: no post-condition attribute is added; `DfDeedle.EmailRecord` remains a plain `private struct` (no `record`/`init`); no Batch C file is re-edited; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [ ] [P5-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/batch-e-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 4 Batch E files (AC1).
- [ ] [P5-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-e-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/regression-testing/batch-e-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the dataframe/async classes (AC3).

### Phase 6 — Final QC Full Toolchain and Acceptance Verification
- [ ] [P6-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass.
- [ ] [P6-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [ ] [P6-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 25 Extensions files under the per-file pragma (AC1); `/p:Nullable=enable` is not passed.
- [ ] [P6-T4] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts (AC3).
- [ ] [P6-T5] Compute and record the changed-line coverage delta at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`), post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), and changed-line coverage for the remediated Extensions files
  - Acceptance: artifact reports baseline coverage, post-change coverage, and changed-line coverage numerically; confirms no coverage regression on changed lines (AC4); `Timestamp:` present. If changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [ ] [P6-T6] Verify AC2 end state: confirm `UtilitiesCS/UtilitiesCS.csproj` still contains no `<Nullable>` element and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation of zero `<Nullable>` occurrences in the csproj (AC2).
- [ ] [P6-T7] Verify no prohibited nullable post-condition attribute and no polyfill were added, by grepping the 23 remediated files and the repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage or a `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-no-postcondition-attrs.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage or polyfill was introduced by this feature.
- [ ] [P6-T8] Verify scope guards: confirm `UtilitiesCS/Extensions/ArrayExtensions.cs` was not split and `DfDeedle.EmailRecord` remains a plain `private struct` (no `record`/`record struct`/`init`), and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and confirmation that `ArrayExtensions.cs` remains a single file and `EmailRecord` remains a plain struct (AC3/AC5 scope compliance).
- [ ] [P6-T9] Verify AC5 signature compatibility by reviewing the git diff of the 23 remediated files and confirming only nullability annotations (and justified `!`) changed with no public-signature behavior change, and record the result at `docs/features/active/utilitiescs-nullable-extensions/evidence/qa-gates/final-signature-compat.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public signature change is limited to additive nullability annotations that reflect actual null behavior (AC5).
- [ ] [P6-T10] Record the acceptance-criteria status summary mapping AC1–AC5 to their supporting evidence artifacts at `docs/features/active/utilitiescs-nullable-extensions/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1–AC5 citing the exact evidence artifact path that demonstrates satisfaction; any unmet AC is marked remediation-required rather than PASS.

## Test Plan

- Unit: existing `UtilitiesCS.Test/Extensions/` MSTest suite (MSTest + Moq + FluentAssertions) is the regression harness; no new temp files. No new tests are required because this is annotation-only, but any incidental test touch must use MSTest + Moq + FluentAssertions and remain deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{a..e}-coverage.cobertura.xml`.
  - Post-change: `evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/qa-gates/final-tests-coverage.md`.
  - Changed-line comparison: `evidence/qa-gates/final-coverage-delta.md` (baseline vs post-change vs changed-line; AC4 no-regression gate).

## Open Questions / Notes

- Coverage-threshold conflict (flagged, not resolved here): CLAUDE.md states repository line coverage
  `>= 80%` and new-code `>= 90%`; `.claude/rules/general-unit-test.md` states uniform `>= 85%` line and
  `>= 75%` branch. This conflict is unresolved and is flagged for the maintainer. For this annotation-only
  feature the operative gate is AC4 (no coverage regression on changed lines), which is threshold-independent;
  the absolute-threshold conflict does not need to be resolved to complete this feature.
- Rules-vs-convention conflict (flagged, not resolved here): `.claude/rules/csharp.md` documents the
  type-check step as forcing `/p:Nullable=enable` globally, which conflicts with the epic's per-file opt-in
  convention. Per epic Shared Design, the global flag is NOT used for this feature's verification; the conflict
  is deferred to the Wave-2 CI capstone child. Policy prohibits editing `.claude/rules/*`.
