# utilitiescs-nullable-svgcontrol — Plan

- **Issue:** #368
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
- Requirements sources: `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/issue.md`,
  `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/spec.md`,
  `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/user-story.md`.
- Research: `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/research/research-findings.2026-07-18T22-10.md`.
- Epic Shared Design: `docs/features/epics/utilitiescs-nullable-remediation/epic.md`.

**All work must comply with these policies; do not duplicate their content here.**

## Scope Invariants (encode into every batch task)

- Per-file `#nullable enable` opt-in ONLY. Do NOT add a `<Nullable>` element to
  `SVGControl/SVGControl.csproj`, and do NOT add one at the solution level (AC2).
- Verification uses the per-file pragma gate:
  `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  for per-batch checks, and `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  for the final solution-wide gate. Do NOT pass `/p:Nullable=enable` at any point; the global flag
  would surface the pre-existing debt across every not-yet-remediated file in `SVGControl/` (and,
  solution-wide, across `UtilitiesCS/`) instead of isolating this cluster's per-file signal.
- Target is net481 / `LangVersion latest`. Nullable post-condition attributes (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`) are NOT available/polyfilled in `SVGControl/` (no
  `ProjectReference` to `UtilitiesCS`) and MUST NOT be used or added. Use plain `?`, flow-narrowed
  locals, guard clauses already present, and justified `!` only.
- Annotation and null-safety ONLY. No behavior changes, no refactors, no API redesign (AC3, AC5).
  Do NOT rename or delete the dead `SvgOptionsConverter1` class (in `SvgOptionsConverter.cs`) or the
  unreferenced `SVGParser.cs`. Do NOT convert `ISvgResource.cs`'s `SvgResource` class to a `record`.
  Do NOT split `RelativePath.cs` (pre-existing 1678-line file, verify-only).
- Prefer annotation plus justified `!` over new runtime guard statements; existing guards stay as-is
  (AC4 pressure: new `if (x is null) throw` lines would be new uncovered executable lines).
- `SvgImageSelector.ImagePath` judgment call (Batch C): the chosen, behavior-preserving resolution is
  a null-forgiving `_relativeImagePath!` with an in-code comment noting the setter is currently a
  no-op — NOT a `?? "(none)"` fallback, which would change the returned value on the dead-setter
  path. This decision must be recorded explicitly at the point it is applied (Phase 3), not treated
  as a routine annotation.
- Coverage posture: the `SVGControl.Test/` suite exercises only `RelativePath.cs` (verify-only); the
  automated changed-line coverage baseline for all 12 hand-authored remediation-target files is 0%,
  making AC4 numerically vacuous for those files specifically. Baseline and final-QC coverage
  capture tasks are still required (below) to record the numeric `SVGControl.Test` run figures and
  confirm no regression on the one file (`RelativePath.cs`) with a real baseline. No new automated
  tests are required by this feature; if a characterization test is added to protect the
  `ImagePath` judgment call, it must be MSTest + FluentAssertions and must not use temp files.
- WinForms `*.Designer.cs` and `Properties/Resources.Designer.cs`/`Properties/AssemblyInfo.cs` are
  NOT opted into the pragma; per research, none require a change to keep the pragma build clean
  (AC6). Confirm, do not edit unless a specific diagnostic requires a mechanical, behavior-preserving
  change.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance
- [x] [P0-T1] Read policy documents in the required order (CLAUDE.md, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read receipt at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read (all four policy files above).
- [x] [P0-T2] Enumerate the 20 `.cs` files under `SVGControl/` and record the baseline inventory (path, line count, and whether the file already carries `#nullable enable`) at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-file-inventory.md`
  - Acceptance: artifact lists all 20 files; confirms 12 hand-authored remediation targets, 3 already-`#nullable enable` verify-only files (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`), and 5 Designer/generated files not opted in; contains `Timestamp:`.
- [x] [P0-T3] Capture baseline CSharpier formatting state by running `dotnet tool run csharpier check .` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting).
- [x] [P0-T4] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T5] Capture baseline per-file nullable pragma-gate build by running `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and CS86xx count for the 3 currently-enabled verify-only files, expected zero; no CS86xx surfaced yet for the 12 not-yet-opted-in hand-authored files).
- [x] [P0-T6] Capture baseline test run with coverage by running `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric headline values (total tests passed/failed, baseline line-coverage percent and branch-coverage percent for the discovered `SVGControl.Test.dll` run); Cobertura XML written to the named evidence path.
- [x] [P0-T7] Confirm the AC2 baseline: verify `SVGControl/SVGControl.csproj` currently contains no `<Nullable>` element and record the finding at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/baseline/baseline-csproj-nullable-absent.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command used, and confirmation that zero `<Nullable>` occurrences exist in `SVGControl/SVGControl.csproj` (AC2 baseline).

### Phase 1 — Batch A Trivial Independent Leaves plus Verify-Only Confirmation
- [x] [P1-T1] Verify the 3 already-`#nullable enable` verify-only files `SVGControl/PathInternal.cs`, `SVGControl/RelativePath.cs`, `SVGControl/ValueStringBuilder.cs` still emit zero CS86xx under the pragma gate; make NO edits unless a diagnostic appears, and record the outcome at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/verify-only-preenabled.md`
  - Acceptance: pragma-gate rebuild (`msbuild SVGControl/SVGControl.csproj /t:Rebuild ... /p:TreatWarningsAsErrors=true`) reports zero CS86xx for all 3 files; artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all 3 files remain unmodified (or, if a diagnostic appeared, the minimal annotation fix is recorded).
- [x] [P1-T2] Add a `#nullable enable` pragma to each of the 4 Batch A files: `SVGControl/ISvgResource.cs`, `SVGControl/ToggleSwitch.cs`, `SVGControl/SVGParser.cs`, `SVGControl/SvgRenderer.cs`
  - Acceptance: each of the 4 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to `SVGControl.csproj`.
- [x] [P1-T3] Apply nullable annotations, guards, and justified `!` to `SVGControl/ISvgResource.cs` and `SVGControl/ToggleSwitch.cs` so each reaches zero CS86xx under the pragma; no annotation changes are expected on either file's members beyond the pragma itself
  - Acceptance: no post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P1-T4] Apply nullable annotations to `SVGControl/SVGParser.cs` so it reaches zero CS86xx under the pragma; do not fix or remove the pre-existing `if (TargetSize != null)` value-type comparison (a pre-existing defect unrelated to nullable reference types, out of scope) and do not rename/delete the file despite it having zero in-project consumers
  - Acceptance: `SVGParser.cs` compiles with zero CS86xx under the pragma; the pre-existing value-type null-comparison line is unchanged; file is not renamed or deleted (AC3, AC5).
- [x] [P1-T5] Apply nullable annotations, guards, and justified `!` to `SVGControl/SvgRenderer.cs` so it reaches zero CS86xx under the pragma: type `Render()`'s return as `Bitmap?`, the `Document` property and backing `_doc` field as `SvgDocument?`, the static `GetSvgDocument(byte[] file)` return as `SvgDocument?`, and the `AssemblyResolve` shim's `PublicKeyTokensEqual(byte[] a, byte[] b)` parameters as `byte[]?` (existing `== null` guards stay as-is)
  - Acceptance: no post-condition attribute is added; the four named members carry the nullable annotations listed; existing null guards in `PublicKeyTokensEqual` are unchanged; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P1-T6] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/batch-a-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 4 Batch A files (AC1).
- [x] [P1-T7] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-a-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-a-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 2 — Batch B ISvgResource Consumers (Pre-Hub)
- [x] [P2-T1] Add a `#nullable enable` pragma to each of the 2 Batch B files: `SVGControl/SvgResourceConverter.cs`, `SVGControl/DropDownEditor.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to `SVGControl.csproj`.
- [x] [P2-T2] Apply nullable annotations and justified `!` to `SVGControl/SvgResourceConverter.cs` so it reaches zero CS86xx under the pragma; the existing `value is null` guard before the `(ISvgResource)value` cast stays as-is
  - Acceptance: no post-condition attribute is added; no new guard clause is introduced; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P2-T3] Apply nullable annotations and justified `!` to `SVGControl/DropDownEditor.cs` so it reaches zero CS86xx under the pragma: declare the reassigned `Assembly asm = null;` local as `Assembly? asm = null;` (relying on existing flow narrowing, no new guard), annotate `(provider.GetService(typeof(IDesignerHost)) as IDesignerHost)!` with the null-forgiving operator to preserve the current NRE-on-null behavior at `host.RootComponentClassName`, and annotate the field `private IWindowsFormsEditorService _editorService;` as `IWindowsFormsEditorService?`
  - Acceptance: no post-condition attribute is added; no new `if (x is null) throw` guard is introduced; the three named null-flow points are resolved exactly as specified; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P2-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/batch-b-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch B files (AC1).
- [x] [P2-T5] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-b-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-b-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 3 — Batch C SvgImageSelector Hub (Isolated, Careful Review)
- [x] [P3-T1] Add a `#nullable enable` pragma to `SVGControl/SvgImageSelector.cs` and annotate `private string _relativeImagePath;`, `private string _absoluteImagePath;`, and `private ISvgResource _svgResource = null;` as their nullable equivalents (`string?`, `string?`, `ISvgResource?`)
  - Acceptance: the file contains a `#nullable enable` pragma; all 3 named fields are annotated nullable; no `<Nullable>` element added to `SVGControl.csproj`.
- [x] [P3-T2] Resolve the `ImagePath` property's dead-setter CS8603 by applying the null-forgiving `_relativeImagePath!` in the `get` accessor's `else return _relativeImagePath;` branch, together with an in-code comment noting the `set` accessor's body is currently entirely commented out (a functional no-op), and do NOT introduce a `?? "(none)"` or any other fallback value
  - Acceptance: the `get` accessor returns `_relativeImagePath!` with the required in-code comment present; no fallback expression is introduced; the returned value on this path is unchanged (still `null` when `_relativeImagePath` is `null`), preserving current behavior (AC3).
- [x] [P3-T3] Record the `SvgImageSelector.ImagePath` judgment-call decision at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/other/imagepath-judgment-call-decision.md`, documenting the dead-setter nuance, the rejected `?? "(none)"` alternative and why it would change observable behavior, and the chosen `_relativeImagePath!` resolution
  - Acceptance: artifact contains `Timestamp:`, a description of the dead-setter nuance, the rejected alternative with its rationale, and the applied resolution, cross-referencing the exact file/line where it was applied (AC3, AC5).
- [x] [P3-T4] Apply nullable annotations to `SvgImageSelector.cs`'s public `ResourceName` property (`ISvgResource?`) and internal `AboluteImagePath` property (`string?`) so their exposed types reflect actual null behavior, without renaming the pre-existing `AboluteImagePath` typo
  - Acceptance: `ResourceName` is typed `ISvgResource?`; `AboluteImagePath` is typed `string?` and its name is unchanged; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P3-T5] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/batch-c-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for `SvgImageSelector.cs` (AC1).
- [x] [P3-T6] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-c-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-c-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 4 — Batch D SvgImageSelector Consumers
- [x] [P4-T1] Add a `#nullable enable` pragma to each of the 3 Batch D files: `SVGControl/SvgOptionsConverter.cs`, `SVGControl/SvgOptionsConverter2.cs`, `SVGControl/SVGFileNameEditor.cs`
  - Acceptance: each of the 3 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to `SVGControl.csproj`.
- [x] [P4-T2] Apply nullable annotations and justified `!` to `SVGControl/SvgOptionsConverter.cs` (class `SvgOptionsConverter1`, dead but still in scope) so it reaches zero CS86xx under the pragma, consuming the now-nullable `AboluteImagePath` from Batch C without re-editing `SvgImageSelector.cs`; do not rename or delete `SvgOptionsConverter1`
  - Acceptance: no post-condition attribute is added; `SvgImageSelector.cs` is not re-edited; class name is unchanged; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P4-T3] Apply nullable annotations and justified `!` to `SVGControl/SvgOptionsConverter2.cs` (class `SvgOptionsConverter`, live) so it reaches zero CS86xx under the pragma, consuming the now-nullable `ResourceName` and `AutoSize` members from Batch C without re-editing `SvgImageSelector.cs`
  - Acceptance: no post-condition attribute is added; `SvgImageSelector.cs` is not re-edited; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P4-T4] Apply nullable annotations to `SVGControl/SVGFileNameEditor.cs` so it reaches zero CS86xx under the pragma: give `private string _appPath;` the same `= string.Empty;` inline-initializer idiom already used for `_currentValue`, `_absoluteFilepath`, and `_fileName` three lines above it in the same file
  - Acceptance: `_appPath` carries `= string.Empty;`; the other three fields' existing initializers are unchanged; no post-condition attribute is added; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P4-T5] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/batch-d-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 3 Batch D files (AC1).
- [x] [P4-T6] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-d-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-d-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 5 — Batch E WinForms Controls (Top of the Dependency Graph)
- [x] [P5-T1] Add a `#nullable enable` pragma to each of the 2 Batch E files: `SVGControl/ButtonSVG.cs`, `SVGControl/PictureBoxSVG.cs`
  - Acceptance: each of the 2 named files contains a `#nullable enable` pragma; no `<Nullable>` element added to `SVGControl.csproj`.
- [x] [P5-T2] Apply nullable annotations to `SVGControl/ButtonSVG.cs` so it reaches zero CS86xx under the pragma: annotate `public static byte[] ObjectToByteArray(Object obj)` as `ObjectToByteArray(object? obj)` (existing `if (obj != null)` guard stays as-is) and the private `GetStringForValue(object value)` helper as `GetStringForValue(object? value)` (existing `if (value == null) return "null";` guard stays as-is); leave event handler parameters (`ButtonSVG_Resize`, `ImageSVG_PropertyChanged`, `Control_SizeChanged`) unannotated as oblivious framework delegate types
  - Acceptance: `ObjectToByteArray` and `GetStringForValue` signatures are updated exactly as specified; no new guard clause is introduced; event handler signatures are unchanged; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P5-T3] Apply nullable annotations to `SVGControl/PictureBoxSVG.cs` so it reaches zero CS86xx under the pragma, mirroring the same annotation treatment applied to `ButtonSVG.cs`'s equivalent members (its own independent copy of `GetStringForValue`, and any nullable-typed field/property exposure from its hosted `SvgImageSelector`); leave event handler parameters unannotated
  - Acceptance: no post-condition attribute is added; no new guard clause is introduced; event handler signatures are unchanged; public signatures remain behavior-compatible (AC5); annotation/null-safety only (AC3).
- [x] [P5-T4] Run `dotnet tool run csharpier .` then the pragma-gate rebuild `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/batch-e-nullable-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` showing zero CS86xx for the 2 Batch E files (AC1).
- [x] [P5-T5] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-e-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/regression-testing/batch-e-tests.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with pass/fail counts confirming no test regression (AC3).

### Phase 6 — Final QC Full Toolchain and Acceptance Verification
- [x] [P6-T1] Run `dotnet tool run csharpier .` across the repository and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-csharpier.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; CSharpier reports no residual formatting changes on a clean second pass.
- [x] [P6-T2] Run the analyzer/code-style build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-analyzers.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build succeeds with no new analyzer errors.
- [x] [P6-T3] Run the solution-wide per-file nullable pragma gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`) and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-nullable-pragma-gate.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 12 remediated files and the 3 verify-only files in `SVGControl/` under the per-file pragma (AC1); `/p:Nullable=enable` is not passed.
- [x] [P6-T4] Run the full test suite with coverage via `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-coverage.cobertura.xml` and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-tests-coverage.md`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric post-change line-coverage and branch-coverage percentages and pass/fail counts (AC3).
- [x] [P6-T5] Compute and record the changed-line coverage delta at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-coverage-delta.md`, comparing baseline coverage (`evidence/baseline/baseline-coverage.cobertura.xml`) against post-change coverage (`evidence/qa-gates/final-coverage.cobertura.xml`), reporting the `RelativePath.cs` changed-line coverage explicitly (the one file in scope with a real automated baseline), and explicitly noting that changed-line coverage for the 12 hand-authored remediation-target files is numerically vacuous (baseline 0%, per research)
  - Acceptance: artifact reports baseline coverage, post-change coverage, and `RelativePath.cs` changed-line coverage numerically; confirms no coverage regression on changed lines for `RelativePath.cs` (AC4); explicitly states the 0%-baseline/vacuous-gate posture for the 12 remediation-target files rather than omitting it; `Timestamp:` present. If `RelativePath.cs` changed-line coverage regresses, the outcome is remediation-required, not PASS.
- [x] [P6-T6] Verify AC2 end state: confirm `SVGControl/SVGControl.csproj` still contains no `<Nullable>` element and no `<Nullable>` element exists at the solution level, and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-ac2-csproj-check.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation of zero `<Nullable>` occurrences in `SVGControl/SVGControl.csproj` and in `TaskMaster.sln` (AC2).
- [x] [P6-T7] Verify no prohibited nullable post-condition attribute and no polyfill were added, by grepping the 12 remediated files and `SVGControl/` for `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull` attribute usage or a `namespace System.Diagnostics.CodeAnalysis` polyfill declaration, and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-no-postcondition-attrs.md`
  - Acceptance: artifact contains `Timestamp:`, the grep command(s) used, and confirmation that no post-condition attribute usage or polyfill was introduced by this feature.
- [x] [P6-T8] Verify scope guards: confirm `SVGControl/ISvgResource.cs`'s `SvgResource` class remains a plain class (no `record`/`record struct`/`init`), `SVGControl/RelativePath.cs` was not split, `SVGControl/SvgOptionsConverter.cs`'s `SvgOptionsConverter1` and `SVGControl/SVGParser.cs` were not renamed or deleted, and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-scope-guards.md`
  - Acceptance: artifact contains `Timestamp:` and confirmation of all 4 named scope guards (AC3/AC5 scope compliance).
- [x] [P6-T9] Verify AC5 signature compatibility by reviewing the git diff of the 12 remediated files and confirming only nullability annotations, flow-narrowed local retyping, and justified `!` changed with no public-signature behavior change, and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-signature-compat.md`
  - Acceptance: artifact contains `Timestamp:` and a per-file confirmation that each public signature change is limited to additive nullability annotations that reflect actual null behavior (AC5).
- [x] [P6-T10] Verify AC6 by diffing the 5 Designer/generated files (`SVGControl/ButtonSVG.Designer.cs`, `SVGControl/PictureBoxSVG.Designer.cs`, `SVGControl/ToggleSwitch.Designer.cs`, `SVGControl/Properties/Resources.Designer.cs`, `SVGControl/Properties/AssemblyInfo.cs`) against their pre-feature state, confirming each is either unchanged or carries only a mechanical, behavior-preserving edit, and record the result at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/qa-gates/final-ac6-designer-check.md`
  - Acceptance: artifact contains `Timestamp:`, the diff command used, and a per-file confirmation (unchanged, or mechanical/behavior-preserving edit only) for all 5 named files (AC6).
- [x] [P6-T11] Record the acceptance-criteria status summary mapping AC1–AC6 to their supporting evidence artifacts at `docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/evidence/other/ac-status-summary.md`
  - Acceptance: artifact contains `Timestamp:` and a row per AC1–AC6 citing the exact evidence artifact path that demonstrates satisfaction; any unmet AC is marked remediation-required rather than PASS.

## Test Plan

- Unit: existing `SVGControl.Test/` MSTest suite (`GetRelativePath_Test.cs`, `RelativePathCoverageTests.cs`; MSTest + FluentAssertions) is the regression harness; no new temp files. No new tests are required because this is annotation-only, but any incidental test touch must use MSTest + Moq + FluentAssertions and remain deterministic.
- Integration: none added.
- Coverage evidence:
  - Baseline: `evidence/baseline/baseline-coverage.cobertura.xml` and `evidence/baseline/baseline-tests-coverage.md`.
  - Per-batch: `evidence/regression-testing/batch-{a..e}-coverage.cobertura.xml`.
  - Post-change: `evidence/qa-gates/final-coverage.cobertura.xml` and `evidence/qa-gates/final-tests-coverage.md`.
  - Changed-line comparison: `evidence/qa-gates/final-coverage-delta.md` (baseline vs post-change vs `RelativePath.cs` changed-line; AC4 no-regression gate; the 12 remediation-target files are noted as numerically vacuous per the coverage posture documented above).

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
  verification; the conflict is deferred to the Wave-2 CI capstone child. Policy prohibits editing
  `.claude/rules/*`.
- `SvgImageSelector.ImagePath` judgment call: this is the single most consequential decision in the
  cluster. It is applied in Phase 3 (P3-T2) and recorded explicitly in Phase 3 (P3-T3), per the
  spec's requirement that it not be treated as a routine annotation.
- `SVGParser.cs` and `SvgOptionsConverter.cs`'s `SvgOptionsConverter1` class are dead/unreferenced
  code, confirmed by research; both remain in scope for AC1 and are not renamed, deleted, or
  otherwise refactored (Phase 1 P1-T4, Phase 4 P4-T2).
