# utilitiescs-nullable-email-parsing — Plan

- **Issue:** #370
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-18T22-05
- **Status:** Draft
- **Version:** 0.2

## Required References

- CLAUDE.md (standing instructions, C# toolchain section).
- `.claude/rules/general-code-change.md` (cross-language code change policy).
- `.claude/rules/general-unit-test.md` (cross-language unit test policy).
- `.claude/rules/csharp.md` (C#-specific toolchain and standards).
- Requirements sources: `docs/features/active/utilitiescs-nullable-email-parsing/issue.md`,
  `docs/features/active/utilitiescs-nullable-email-parsing/spec.md`,
  `docs/features/active/utilitiescs-nullable-email-parsing/user-story.md`.
- Research: `docs/features/active/utilitiescs-nullable-email-parsing/research/research.2026-07-18T22-05.md`.
- Upstream contract (Wave 0, must have merged before this plan begins): `docs/features/active/utilitiescs-nullable-extensions/plan.2026-07-18T21-20.md`
  and `docs/features/active/utilitiescs-nullable-extensions/spec.md` (issue #363) — this
  cluster consumes `NullExtensions.ThrowIfNull<T>` (verify-only), `StringExtensions.IsNullOrEmpty`
  (Batch B), and `IEnumerableExtensions.Transpose<T>` (Batch C) as post-remediation contracts.

**All work must comply with these policies; do not duplicate their content here.**

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY, applied to the 24 remediation-target `.cs` files in
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/` (14 files),
  `UtilitiesCS/EmailIntelligence/SubjectMap/` (7 files, minus the excluded
  `SubjectMapMetrics.Designer.cs`), and `UtilitiesCS/EmailIntelligence/Ctf/` (4 files). Do NOT
  add a `<Nullable>` element to `UtilitiesCS/UtilitiesCS.csproj` (AC2).
- Verification uses the per-file pragma gate:
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
  Do NOT pass `/p:Nullable=enable` globally in any command; the global flag surfaces the whole-solution
  pre-existing debt and drowns this child's signal. Enforcement is per-file pragma only.
- Target is net481 / C# 12. Nullable post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`,
  `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`, `[DoesNotReturn]`,
  `[MemberNotNull]`) are NOT available/polyfilled and MUST NOT be used or added. Use plain `?`,
  `where T : notnull`, unconstrained `T?`, guard clauses, and justified `!`.
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign (AC3, AC5).
  Existing null guards (e.g. `EmailFilerConfig.IsDeleteRelevant`'s `currentFolder.ThrowIfNull()`,
  `SubjectMapEncoder.RebuildEncoding()`'s `NullReferenceException` guard) remain unchanged. Prefer
  nullable annotation and justified `!` over new runtime guard statements, to avoid introducing new
  uncovered executable lines (AC4 pressure).
- `SortEmail.cs` (~1407 lines), `EmailTokenizer.cs` (~729 lines), and `SubjectMapEntry.cs`
  (~657 lines) exceed the 500-line general file-size limit as a pre-existing condition; this
  annotation-only remediation MUST NOT split them. Flag for a future refactor issue.
  `EmailDataMiner.FolderExtraction.cs` (~483 lines) is under the limit but is the largest of the
  four `EmailDataMiner` partial files; note only, no action required.
- `FolderStruct` (`EmailDataMiner.Transform.cs`, lines 17-28) is a plain `internal struct` using a
  C# 12 primary constructor; it MUST remain a plain struct — do NOT convert to `record`/
  `record struct` (fails CS0518 on net481, no `IsExternalInit`). `SpamBayesOptions`
  (`EmailTokenizer.cs`) is a plain `struct` with only `const` fields; no nullable-annotation action
  needed there beyond leaving it unchanged.
- `SubjectMapMetrics.Designer.cs` is Designer-generated code and is explicitly excluded from
  remediation; only its partial-class sibling `SubjectMapMetrics.cs` is remediated.
- Mandatory single-batch partial-class groups (must remediate together, one phase each):
  1. `EmailDataMiner` (4 files, namespace `UtilitiesCS.EmailIntelligence.Bayesian` — note this
     differs from the `EmailParsingSorting` folder name, a pre-existing folder/namespace mismatch
     with no bearing on annotation work): `EmailDataMiner.cs`,
     `EmailDataMiner.FolderExtraction.cs`, `EmailDataMiner.Serialization.cs`,
     `EmailDataMiner.Transform.cs` — shared private fields `_globals`/`_sw` are consumed across
     all four files. See Phase 6.
  2. `SubjectMapSco` (2 files, namespace `UtilitiesCS`): `SubjectMapSco.cs` and
     `SubjectMapSco.Orchestration.cs` — the class's public/internal surface (`Add`, `Find`,
     `Serialize`) is exercised across both files. See Phase 3.
- Before starting Phase 1, confirm the Wave-0 `utilitiescs-nullable-extensions` (issue #363)
  child has merged its verify-only file, Batch B, and Batch C, since this cluster's
  `EmailTokenizer.cs` and other files compile against those post-remediation signatures
  (`NullExtensions.ThrowIfNull<T>`, `StringExtensions.IsNullOrEmpty`,
  `IEnumerableExtensions.Transpose<T>`).
- Two duplicate-named test-file pairs exist (e.g. `EmailFiler_Tests.cs` in two directories;
  `EmailTokenizer(Tests|_Tests).cs`, `CommonWords_Test(s).cs`, `CtfMap(Tests|_Tests).cs`,
  `CtfIncidenceList(Tests|_Tests).cs`, `MinedMailInfo*Tests.cs`). This is not necessarily a build
  problem (MSTest requires unique fully-qualified class names, not unique file names), but the
  baseline test run (Phase 0) must be captured before any edit so any regression during
  remediation is attributable to an annotation change, not a pre-existing ambiguity.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [x] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read receipt at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [x] [P0-T2] Enumerate the 24 remediation-target `.cs` files under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`, `UtilitiesCS/EmailIntelligence/SubjectMap/`, and `UtilitiesCS/EmailIntelligence/Ctf/`, and record the baseline inventory (path, line count, whether `#nullable enable` is already present) at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact lists all 25 `.cs` files in the three directories; confirms `SubjectMapMetrics.Designer.cs` is excluded (generated); confirms the remaining 24 are remediation targets and none currently carries `#nullable enable`; contains `Timestamp:`.
- [x] [P0-T3] Confirm the Wave-0 upstream dependency: verify `UtilitiesCS/Extensions/NullExtensions.cs` already opens with `#nullable enable` and record the finding at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-upstream-dependency-check.md`
  - Acceptance: artifact contains `Timestamp:`, confirms `NullExtensions.cs`, `StringExtensions.cs`, and `IEnumerableExtensions.cs` are present and (for `NullExtensions.cs`) already `#nullable enable`, satisfying the Wave-0 merge precondition documented in Scope Invariants.
- [x] [P0-T4] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [x] [P0-T5] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T6] Capture baseline per-file nullable pragma-gate rebuild by running `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and CS86xx count, expected zero, since the 24 cluster files are not yet opted in and thus emit no pragma-gated diagnostics).
- [x] [P0-T7] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent); Cobertura XML written to the named evidence path.
- [x] [P0-T8] Confirm the AC2 baseline: verify `UtilitiesCS/UtilitiesCS.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in the csproj (AC2 baseline).

### Phase 1 — Batch A Trivial Leaves (DTOs / Obsolete / Small Interfaces)
- [x] [P1-T1] Add a `#nullable enable` pragma to each of the 6 Batch A files: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/IEmailTokenizer.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs`, `UtilitiesCS/EmailIntelligence/Ctf/CtfMapEntry.cs`, `UtilitiesCS/EmailIntelligence/Ctf/CtfIncidence.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/MinedMailInfo.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/MovedMailInfo.cs`
  - Acceptance: each of the 6 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P1-T2] Apply nullable annotations, guards, and justified `!` to the 6 Batch A files so each reaches zero CS86xx under the pragma; annotate `MinedMailInfo`/`MovedMailInfo` COM-backed lazy getters (`FolderOld`, `MailItem`) as `Folder?`/`MailItem?` since they already null-check before use and return `null` explicitly; keep `[Obsolete]` attributes on `CtfIncidence.cs` unchanged
  - Acceptance: no `System.Diagnostics.CodeAnalysis` post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotations reflect actual null behavior; changes are annotation/null-safety only (AC3).
- [x] [P1-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 6 Batch A files (AC1).
- [x] [P1-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-a-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 2 — Batch B CTF Map and Subject-Map Leaf Collections
- [x] [P2-T1] Add a `#nullable enable` pragma to each of the 3 Batch B files: `UtilitiesCS/EmailIntelligence/Ctf/CtfMap.cs`, `UtilitiesCS/EmailIntelligence/Ctf/CtfIncidenceList.cs`, `UtilitiesCS/EmailIntelligence/SubjectMap/CommonWords.cs`
  - Acceptance: each of the 3 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P2-T2] Apply nullable annotations, guards, and justified `!` to the 3 Batch B files so each reaches zero CS86xx under the pragma; keep the `[Obsolete]` attribute on `CtfIncidenceList.cs` unchanged; annotate `CtfMap.cs`'s `MAPIFolder`-typed members consistently with `CtfMapEntry`'s Batch A annotations
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); changes are annotation/null-safety only (AC3).
- [x] [P2-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 3 Batch B files (AC1).
- [x] [P2-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-b-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 3 — Batch C SubjectMap Encoding Chain (Includes Mandatory SubjectMapSco Partial-Class Group)
- [x] [P3-T1] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapEncoder.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; annotate the lazily-populated `_encoder`/`_decoder` `IScoDictionaryNew<,>` fields as `?`, keeping the existing `RebuildEncoding()` `NullReferenceException` guard on `_subjectMap` unchanged
  - Acceptance: `SubjectMapEncoder.cs` contains `#nullable enable`; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T2] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapEntry.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; treat fields assigned only inside `Init` overloads (`_folderTokens`, `_subjectTokens`, `_folderPath`) with justified `!` or `?` consistent with the existing `ArgumentNullException` guards in `Init`; do NOT split the file (pre-existing >500-line condition, see Scope Invariants)
  - Acceptance: `SubjectMapEntry.cs` contains `#nullable enable`; file is not split; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T3] Add a `#nullable enable` pragma to both `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.cs` and `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` in the same task, since this is the mandatory single-batch `SubjectMapSco` partial-class group
  - Acceptance: both files contain `#nullable enable`; no `<Nullable>` element added to the csproj (AC1, AC2).
- [x] [P3-T4] Apply nullable annotations, guards, and justified `!` to both `SubjectMapSco.cs` and `SubjectMapSco.Orchestration.cs` together so the combined partial type reaches zero CS86xx under the pragma; annotate `SubjectMapSco.Orchestration.cs`'s `ResolveFolder` ternary return as `MAPIFolder?`, consistent with the already-null-safe `.Where(tuple => tuple.Folder != null)` filter in `QueryOlFolders`
  - Acceptance: no post-condition attribute is added; the partial type's shared public/internal surface (`Add`, `Find`, `Serialize`) has a coherent nullable contract across both files; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T5] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapMetrics.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; do NOT modify `SubjectMapMetrics.Designer.cs` (excluded, Designer-generated)
  - Acceptance: `SubjectMapMetrics.cs` contains `#nullable enable`; `SubjectMapMetrics.Designer.cs` is unmodified; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T6] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 5 Batch C files (AC1).
- [x] [P3-T7] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-c-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression on the SubjectMap encoding chain (AC3).

### Phase 4 — Batch D Email Filing/Config Core
- [ ] [P4-T1] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; annotate `TryResolveDestinationFolder()` as `Folder?` (already returns `null` in both the not-found and catch branches); keep `IsDeleteRelevant`'s `currentFolder.ThrowIfNull()` guard unchanged
  - Acceptance: `EmailFilerConfig.cs` contains `#nullable enable`; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); annotations consistent with the upstream `NullExtensions.ThrowIfNull<T>` contract (AC5).
- [ ] [P4-T2] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; annotate `TryMoveMailItemHelperAsync`'s `(MailItem Original, MailItem Moved)` tuple's `Moved` element as `MailItem?` without changing the tuple shape or the deconstruction call sites in `ProcessMailHelperAsync`/`TryMoveMailItemForProcessingAsync`
  - Acceptance: `EmailFiler.cs` contains `#nullable enable`; the tuple shape and deconstruction call sites are unchanged aside from the `Moved` element's nullability; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); annotations consistent with the upstream `NullExtensions.ThrowIfNull<T>`/`ThrowIfNullOrEmpty` and `StringExtensions.IsNullOrEmpty` contracts (AC5).
- [ ] [P4-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-d-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch D files (AC1).
- [ ] [P4-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-d-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-d-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression, including the two `EmailFiler_Tests.cs` duplicate-named test files (AC3).

### Phase 5 — Batch E Image/OCR/Tokenization Chain
- [ ] [P5-T1] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; annotate `PIL_decode_parts`'s conditionally-assigned locals (`byte[]? bytes`, `Image? image`, `Bitmap? bitmap`) and `GetFrameWithText`'s `imageWithText` return path with `?` or justified `!` consistent with existing null-checks (`image is not null`) before use
  - Acceptance: `ImageStripper.cs` contains `#nullable enable`; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); consumes upstream `StringExtensions.IsNullOrEmpty` contract correctly (AC5).
- [ ] [P5-T2] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailTokenizer.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; annotate the `crack_images` delegate field as nullable (`Func<...>?`), the `Func<string, int> _len` optional-parameter default, `IEnumerable<string> all_addrs`, and `MatchCollection matches` conditionally-assigned locals; preserve the existing `msg.Subject is not null` guards and the `?.Charset ?? string.Empty` null-safe pattern in `crack_content_xyz`; annotate `SpamBayesOptions` and `CharsetCodebase` fields as needed without converting either type to a `record`; do NOT split the file (pre-existing >500-line condition, see Scope Invariants)
  - Acceptance: `EmailTokenizer.cs` contains `#nullable enable`; file is not split; `SpamBayesOptions`/`CharsetCodebase` remain plain types (no `record`/`record struct`); no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); consumes upstream `IEnumerableExtensions.Transpose<T>` and `StringExtensions.IsNullOrEmpty` contracts correctly (AC5).
- [ ] [P5-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-e-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch E files (AC1).
- [ ] [P5-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-e-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-e-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression, including the two `EmailTokenizer(Tests|_Tests).cs` duplicate-named test files (AC3).

### Phase 6 — Batch F EmailDataMiner Partial-Class Group (Mandatory Single Batch)
- [ ] [P6-T1] Add a `#nullable enable` pragma to all 4 Batch F files in the same task, since this is the mandatory single-batch `EmailDataMiner` partial-class group: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Serialization.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs`
  - Acceptance: all 4 named files contain `#nullable enable`; no `<Nullable>` element added to the csproj (AC1, AC2).
- [ ] [P6-T2] Annotate the shared private fields `_globals` and `_sw` declared in `EmailDataMiner.cs` consistently with their usage across all four partial files (`_globals.FS.SpecialFolders`, `_globals.Ol...` calls, `_sw.LogDuration(...)`)
  - Acceptance: `_globals` and `_sw` carry a single coherent nullable annotation applied consistently across all four files' usages; no post-condition attribute added; annotation/null-safety only (AC3).
- [ ] [P6-T3] Apply nullable annotations, guards, and justified `!` to `EmailDataMiner.Serialization.cs` so `Deserialize<T>`, `DeserializeFromFolder<T>`, and both `DeserializeAsync<T>` overloads become unconstrained `T?` returns (replacing the current `default(T)` sentinel on a missing lookup or file), and `ToMinedMail(IItemInfo[])`'s `?? null` LINQ projection is annotated consistently
  - Acceptance: the four listed members return unconstrained `T?`; no post-condition attribute added; public signatures behavior-compatible aside from the additive `?` (AC5); annotations consistent with the upstream `NullExtensions.ThrowIfNull<T>` contract used via `loader.ThrowIfNull()` (AC5).
- [ ] [P6-T4] Apply nullable annotations, guards, and justified `!` to `EmailDataMiner.Transform.cs` so `TryLoadObjectAndGetMemorySize<T>`'s `(default, 0)` catch-branch tuple has its `T Object` element annotated `T?`, and the `FolderStruct` primary-constructor struct (lines 17-28) remains a plain `internal struct` with no `record`/`record struct` conversion
  - Acceptance: `TryLoadObjectAndGetMemorySize<T>`'s tuple `Object` element is `T?`; `FolderStruct` remains a plain struct (no `record`/`init`); no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [ ] [P6-T5] Apply nullable annotations, guards, and justified `!` to `EmailDataMiner.FolderExtraction.cs` so `QueryOlFolders(FolderTreeSnapshot)`'s `resolver.TryResolve(node, out var folder) ? folder : null` ternary and `.OfType<MAPIFolder>()` filter, `CreateFolderWrapper`'s `resolver.TryResolve(...) && folder is MAPIFolder mapiFolder` branch, and `TryResolveMapiHandles(FolderTree, FolderWrapper[])`'s `FolderWrapper handle = null;` local are annotated consistently (`FolderWrapper? handle = null;` plus the existing guard)
  - Acceptance: the three named constructs are annotated with nullable-consistent types; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [ ] [P6-T6] Confirm `EmailDataMiner.cs` itself (the primary partial file declaring `_globals`/`_sw`) reaches zero CS86xx under the pragma after the annotations applied in the prior four tasks, making any remaining field-level or method-level fix needed for consistency across the partial type
  - Acceptance: `EmailDataMiner.cs` contains `#nullable enable` and, combined with the other three files, the whole partial type is internally consistent; no post-condition attribute added; annotation/null-safety only (AC3).
- [ ] [P6-T7] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-f-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for all 4 Batch F files (AC1).
- [ ] [P6-T8] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-f-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-f-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression across `EmailDataMiner_Tests.cs`, `EmailDataMiner_Additional_Tests.cs`, `EmailDataMiner_FolderExtractionCoverage_Tests.cs`, and `EmailDataMiner_TestSupport.cs` (AC3).

### Phase 7 — Batch G Static Sorting Orchestrators
- [ ] [P7-T1] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/EmailParsingSorting/AutoFile.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; leave `Category_IsAlreadySelected(dynamic objItem, string strCat)`'s `dynamic` parameter unannotated (exempt from nullable analysis)
  - Acceptance: `AutoFile.cs` contains `#nullable enable`; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3).
- [ ] [P7-T2] Add a `#nullable enable` pragma to `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` and apply nullable annotations so it reaches zero CS86xx under the pragma; annotate `Folder olDestination = null;`, `MailItem mailItemTemp = null;`, `string[] strOutput = null;`, and the uninitialized `string[,] strAryOutput;` local consistently with `?`; do NOT split the file (pre-existing >500-line condition, see Scope Invariants); no coverage-regression risk from this file's own lines since it is almost entirely `[ExcludeFromCodeCoverage]`
  - Acceptance: `SortEmail.cs` contains `#nullable enable`; file is not split; no post-condition attribute added; public signatures behavior-compatible (AC5); annotation/null-safety only (AC3); consumes upstream `StringExtensions.IsNullOrEmpty` contract correctly (AC5).
- [ ] [P7-T3] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/batch-g-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch G files (AC1).
- [ ] [P7-T4] Run the UtilitiesCS test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-g-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/regression-testing/batch-g-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression, including `AutoFile_Tests.cs` and `SortEmail_Tests.cs` (AC3).

### Phase 8 — Final QC Full Toolchain and Acceptance Verification
- [ ] [P8-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass. If any file changed, restart this Final QC phase from P8-T1.
- [ ] [P8-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors. If this step fails or changes files, restart this Final QC phase from P8-T1.
- [ ] [P8-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 24 remediated cluster files under the per-file pragma (AC1); `/p:Nullable=enable` is not passed. If this step fails or changes files, restart this Final QC phase from P8-T1.
- [ ] [P8-T4] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts (AC3). If any step in P8-T1 through P8-T4 fails or changes files, restart this Final QC phase from P8-T1.
- [ ] [P8-T5] Compute and record the changed-line coverage delta at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`), post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), and changed-line coverage for the 24 remediated cluster files
  - Acceptance: artifact reports baseline coverage percentage, post-change coverage percentage, and changed-line coverage percentage numerically; confirms no coverage regression on changed lines (AC4); `Timestamp:` present. If changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [ ] [P8-T6] Verify AC2 end state: confirm `UtilitiesCS/UtilitiesCS.csproj` still contains no `<Nullable>` element and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation of zero `<Nullable>` occurrences in the csproj (AC2).
- [ ] [P8-T7] Verify no prohibited nullable post-condition attribute and no polyfill were added, by grepping the 24 remediated files and the repository for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage or a `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-no-postcondition-attrs.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage or polyfill was introduced by this feature.
- [ ] [P8-T8] Verify scope guards: confirm `SortEmail.cs`, `EmailTokenizer.cs`, and `SubjectMapEntry.cs` were not split, `FolderStruct` and `SpamBayesOptions` remain plain structs (no `record`/`record struct`/`init`), and `SubjectMapMetrics.Designer.cs` was not modified, and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and confirmation of all five scope guards (file-size non-split, struct non-conversion x2, Designer-file non-modification) (AC3/AC5/AC6 scope compliance).
- [ ] [P8-T9] Verify AC5 signature compatibility by reviewing the git diff of the 24 remediated files and confirming only nullability annotations (and justified `!`) changed with no public-signature behavior change, and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-signature-compat.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public signature change is limited to additive nullability annotations that reflect actual null behavior, consistent with the upstream `utilitiescs-nullable-extensions` contracts (AC5).
- [ ] [P8-T10] Verify AC6: confirm no file outside the 24-file cluster (`EmailParsingSorting/`, `SubjectMap/`, `Ctf/`) was given a `#nullable enable` pragma or any nullable-related edit by reviewing the full git diff file list, and record the result at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-ac6-no-cross-block.md`
  - Acceptance: artifact contains `Timestamp:`, the diff file list reviewed, and confirmation that only the 24 cluster files (plus documentation/evidence artifacts) were modified, demonstrating non-remediated files elsewhere remain non-opted-in and are not cross-blocked (AC6).
- [ ] [P8-T11] Record the acceptance-criteria status summary mapping AC1–AC6 to their supporting evidence artifacts at `docs/features/active/utilitiescs-nullable-email-parsing/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1–AC6 citing the exact evidence artifact path that demonstrates satisfaction; any unmet AC is marked remediation-required rather than PASS.

## Test Plan

- Unit: existing `UtilitiesCS.Test/EmailIntelligence/` MSTest suite (MSTest + Moq + FluentAssertions) is the regression harness; no new temp files. No new tests are required because this is annotation-only, but any incidental test touch must use MSTest + Moq + FluentAssertions and remain deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{a..g}-coverage.cobertura.xml`.
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
  convention. Per the epic's shared design and this feature's Scope Invariants, the global flag is NOT used
  for this feature's verification; the conflict is deferred to the Wave-2 CI capstone child
  (`utilitiescs-nullable-ci-capstone`).
- Folder/namespace mismatch (flagged, no action needed): the four `EmailDataMiner.*` files live under the
  `EmailParsingSorting/` folder but declare namespace `UtilitiesCS.EmailIntelligence.Bayesian`. This is a
  pre-existing condition with no bearing on annotation work.
- Duplicate test-file-name pairs (flagged, no action needed): several classes have two test files with the
  same class name in different directories (see Scope Invariants). The Phase 0 baseline test run establishes
  the deterministic pre-existing pass/fail state so any regression during remediation is attributable to an
  annotation change, not this pre-existing ambiguity.
